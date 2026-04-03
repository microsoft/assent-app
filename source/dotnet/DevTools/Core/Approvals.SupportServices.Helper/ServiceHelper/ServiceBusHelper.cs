// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace Microsoft.CFS.Approvals.SupportServices.Helper.ServiceHelper;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using global::Azure.Identity;
using global::Azure.Messaging.ServiceBus;
using global::Azure.Messaging.ServiceBus.Administration;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using Microsoft.CFS.Approvals.Contracts.DataContracts;
using Microsoft.CFS.Approvals.Core.BL.Interface;
using Microsoft.CFS.Approvals.Data.Azure.Storage.Interface;
using Microsoft.CFS.Approvals.DevTools.AppConfiguration;
using Microsoft.CFS.Approvals.DevTools.Model.Constant;
using Microsoft.CFS.Approvals.Model;
using Microsoft.CFS.Approvals.SupportServices.Helper.Interface;

/// <summary>
/// The Service Bus Helper class
/// </summary>
public class ServiceBusHelper : IServiceBusHelper
{
    private readonly ServiceBusAdministrationClient _managementClient;
    private readonly ConfigurationHelper _configurationHelper;
    private readonly string _environment;
    private string _messageType;
    private readonly IARConverterFactory _aRConverterFactory;
    private readonly IBlobStorageHelper _blobStorageHelper;
    public ApprovalTenantInfo TenantInfo { get; set; }

    /// <summary>
    /// Constructor of ServiceBusHelper
    /// </summary>
    /// <param name="configurationHelper"></param>
    /// <param name="actionContextAccessor"></param>
    /// <param name="managementClient"></param>
    public ServiceBusHelper(ConfigurationHelper configurationHelper,
        IActionContextAccessor actionContextAccessor,
        Func<string, ServiceBusAdministrationClient> managementClient,
        Func<string, IARConverterFactory> arConverterFactory,
        Func<string, IBlobStorageHelper> blobStorageHelper)
    {
        _environment = actionContextAccessor?.ActionContext?.RouteData?.Values["env"]?.ToString();
        _configurationHelper = configurationHelper;
        _managementClient = managementClient(_configurationHelper.appSettings[_environment]["ServiceBusNamespace:fullyQualifiedNamespace"]);
        _aRConverterFactory = arConverterFactory(_environment);
        _blobStorageHelper = blobStorageHelper(configurationHelper.appSettings[_environment]["StorageAccountName"]);
    }

    /// <summary>
    /// Peek service bus message
    /// </summary>
    /// <param name="topicName"></param>
    /// <param name="subscriptionName"></param>
    /// <param name="messageType"></param>
    /// <returns></returns>
    public Dictionary<string, ArrayList> PeekServiceBusMessage(string topicName, string subscriptionName, string messageType)
    {
        Dictionary<string, ArrayList> result = new Dictionary<string, ArrayList>();
        _messageType = messageType;
        var messages = GetMessagesFromSubscriptionAsync(topicName, subscriptionName, messageType);
        result = GetMessage(messages.GetAwaiter().GetResult());
        return result;
    }

    /// <summary>
    /// Get subscriptions
    /// </summary>
    /// <param name="topicName"></param>
    /// <returns></returns>
    public async Task<List<string>> GetSubscriptionsAsync(string topicName)
    {
        var subscriptionDescription = new List<string>();
        var subscriptions = _managementClient.GetSubscriptionsAsync(topicName);
        await foreach (SubscriptionProperties prop in subscriptions)
        {
            subscriptionDescription.Add(prop.SubscriptionName);
        }            
        return subscriptionDescription;
    }

    /// <summary>
    /// Get topics
    /// </summary>
    /// <returns></returns>
    public async Task<List<string>> GetTopicsAsync()
    {
        var topicDescriptions = new List<string>();
        var topics = _managementClient.GetTopicsAsync();

        await foreach (TopicProperties prop in topics)
        {
            topicDescriptions.Add(prop.Name);
        }
        return topicDescriptions;
    }

    /// <summary>
    /// Get all brokered messages for selectd topic and subscription which are either deadletter or active
    /// </summary>
    /// <param name="topicName"></param>
    /// <param name="subscriptionName"></param>
    /// <param name="messageType"></param>
    /// <returns></returns>
    public async Task<IList<ServiceBusReceivedMessage>> GetMessagesFromSubscriptionAsync(string topicName, string subscriptionName, string messageType)
    {
        ServiceBusMessageType msgType;
        Enum.TryParse(messageType, out msgType);
        var messagesOfQueue = new List<ServiceBusReceivedMessage>();
        var previousSequenceNumber = -1L;
        var sequenceNumber = 0L;

        // Use DefaultAzureCredential only in DEBUG (local development), ManagedIdentityCredential in production.
        global::Azure.Core.TokenCredential credential;
#if DEBUG
        credential = new DefaultAzureCredential();  // CodeQL [SM05137] Suppress CodeQL issue since we only use DefaultAzureCredential in development environments.
#else
        credential = new ManagedIdentityCredential();
#endif

        await using var client = new ServiceBusClient(_configurationHelper.appSettings[_environment]["ServiceBusNamespace:fullyQualifiedNamespace"], credential);
        ServiceBusReceiver messageReceiver = client.CreateReceiver(topicName, subscriptionName + (msgType == ServiceBusMessageType.DeadLetter ? "/$DeadLetterQueue" : ""), new ServiceBusReceiverOptions() { ReceiveMode = ServiceBusReceiveMode.PeekLock, PrefetchCount = 100 });
        do
        {
            var messageBatch = await messageReceiver.PeekMessagesAsync(int.MaxValue, sequenceNumber);

            if (messageBatch.Count > 0)
            {
                sequenceNumber = messageBatch[^1].SequenceNumber;

                if (sequenceNumber == previousSequenceNumber)
                    break;

                messagesOfQueue.AddRange(messageBatch);

                previousSequenceNumber = sequenceNumber;
            }
            else
            {
                break;
            }
        } while (true);

        return messagesOfQueue;
    }

    /// <summary>
    /// Get message
    /// </summary>
    /// <param name="messages"></param>
    /// <returns></returns>
    public Dictionary<string, ArrayList> GetMessage(IList<ServiceBusReceivedMessage> messages)
    {
        Dictionary<string, ArrayList> result = new Dictionary<string, ArrayList>();
        var arxs = new List<ApprovalRequestExpression>();
        foreach (var message in messages)
        {
            try
            {
                byte[] messageContent;
                if (message.ApplicationProperties.ContainsKey("ApprovalRequestVersion") && message.ApplicationProperties["ApprovalRequestVersion"].ToString() == _configurationHelper.appSettings[_environment][Microsoft.CFS.Approvals.Contracts.ConfigurationKey.ApprovalRequestVersion.ToString()])
                    messageContent = _blobStorageHelper.DownloadByteArray(Microsoft.CFS.Approvals.Contracts.Constants.PrimaryMessageContainer, System.Text.Encoding.UTF8.GetString(message.Body)).Result;
                else
                    messageContent = message.Body.ToArray();

                var arConverterAdaptor = _aRConverterFactory.GetARConverter();
                var requestExpressions = arConverterAdaptor.GetAR(messageContent, message, new ApprovalTenantInfo());
                requestExpressions.ForEach(s => { GenerateResponse(s, message, _messageType, ref result); });
            }
            catch
            { }

        }

        return result;
    }

    /// <summary>
    /// Generate response
    /// </summary>
    /// <param name="approvalRequestExpressions"></param>
    /// <param name="message"></param>
    /// <param name="messageType"></param>
    /// <param name="result"></param>
    public void GenerateResponse(ApprovalRequestExpression approvalRequestExpressions, ServiceBusReceivedMessage message, string messageType, ref Dictionary<string, ArrayList> result)
    {
        Enum.TryParse(messageType, out ServiceBusMessageType msgType);
        if (msgType == ServiceBusMessageType.DeadLetter)
        {
            if (!result.Keys.Contains("columns"))
            {
                result["columns"] = new ArrayList { "Request Number", "Submitter", "Approver", "Error", "Description" };
            }
            if (!result.Keys.Contains("rows"))
            {
                result["rows"] = new ArrayList();
            }

            result["rows"].Add(new string[] {
                     approvalRequestExpressions?.ApprovalIdentifier?.DisplayDocumentNumber,
                    approvalRequestExpressions?.SummaryData?.Submitter?.Alias,
                    (approvalRequestExpressions?.Approvers != null ? string.Join(",", approvalRequestExpressions?.Approvers?.Select(s => s.Alias)) : ""),
                    message?.ApplicationProperties["DeadLetterReason"]?.ToString(),
                    message?.ApplicationProperties["DeadLetterErrorDescription"]?.ToString()
                });

        }
        else
        {
            if (!result.Keys.Contains("columns"))
            {
                result["columns"] = new ArrayList { "Request Number", "Submitter", "Approver" };
            }
            if (!result.Keys.Contains("rows"))
            {
                result["rows"] = new ArrayList();
            }
            result["rows"].Add(new string[]{approvalRequestExpressions?.ApprovalIdentifier?.DisplayDocumentNumber,
                    approvalRequestExpressions?.SummaryData?.Submitter?.Alias,
                    (approvalRequestExpressions?.Approvers !=null ?string.Join(",", approvalRequestExpressions?.Approvers?.Select(s => s.Alias)) : "") });
        }

    }

}