// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace Microsoft.CFS.Approvals.SupportServices.Helper.ServiceHelper;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using Microsoft.Azure.ServiceBus;
using Microsoft.Azure.ServiceBus.Management;
using Microsoft.CFS.Approvals.Contracts.DataContracts;
using Microsoft.CFS.Approvals.Core.BL.Interface;
using Microsoft.CFS.Approvals.Data.Azure.Storage.Interface;
using Microsoft.CFS.Approvals.DevTools.Model.Constant;
using Microsoft.CFS.Approvals.Model;
using Microsoft.CFS.Approvals.SupportServices.Helper.Interface;

/// <summary>
/// The Service Bus Helper class
/// </summary>
public class ServiceBusHelper : IServiceBusHelper
{
    private readonly ManagementClient _managementClient;
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
        Func<string, ManagementClient> managementClient,
        Func<string, IARConverterFactory> arConverterFactory,
        Func<string, IBlobStorageHelper> blobStorageHelper)
    {
        _environment = actionContextAccessor?.ActionContext?.RouteData?.Values["env"]?.ToString();
        _configurationHelper = configurationHelper;
        _managementClient = managementClient(_configurationHelper.appSettings[_environment]["ServiceBusConnection"]);
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
        var messages = GetMessagesFromSubscription(topicName, subscriptionName, messageType);
        result = GetMessage(messages);
        return result;
    }

    /// <summary>
    /// Get subscriptions
    /// </summary>
    /// <param name="topicName"></param>
    /// <returns></returns>
    public List<string> GetSubscriptions(string topicName)
    {
        var subscriptionDescription = new List<string>();
        var subscription = _managementClient.GetSubscriptionsAsync(topicName).Result;
        subscriptionDescription.AddRange(subscription.Select(s => s.SubscriptionName));
        return subscriptionDescription;
    }

    /// <summary>
    /// Get topics
    /// </summary>
    /// <returns></returns>
    public List<string> GetTopics()
    {
        var topicDescriptions = new List<string>();

        for (int skip = 0; skip < 1000; skip += 100)
        {
            var topics = _managementClient.GetTopicsAsync(100, skip).Result;
            if (!topics.Any()) break;

            topicDescriptions.AddRange(topics.Select(s => s.Path));
        }
        return topicDescriptions;
    }

    /// <summary>
    /// Get all brokered messages for selectd topic and subscription which are either deadletter or active
    /// </summary>
    /// <param name="connectionString"></param>
    /// <param name="topicName"></param>
    /// <param name="subscriptionName"></param>
    /// <param name="messageType"></param>
    /// <returns></returns>
    public IList<Message> GetMessagesFromSubscription(string topicName, string subscriptionName, string messageType)
    {
        ServiceBusMessageType msgType;
        Enum.TryParse(messageType, out msgType);
        Microsoft.Azure.ServiceBus.Core.MessageReceiver messageReceiver = new Microsoft.Azure.ServiceBus.Core.MessageReceiver(_configurationHelper.appSettings[_environment]["ServiceBusConnection"], EntityNameHelper.FormatSubscriptionPath(topicName, subscriptionName) + (msgType == ServiceBusMessageType.DeadLetter ? "/$DeadLetterQueue" : ""), Microsoft.Azure.ServiceBus.ReceiveMode.PeekLock);
        messageReceiver.PrefetchCount = 100;
        List<Message> brokeredMessages = new List<Message>();

        IList<Message> messages = new List<Message>();
        messages = (IList<Message>)messageReceiver.PeekAsync(100).Result;

        brokeredMessages.AddRange(messages);
        while (messages.Count > 0)
        {
            messages = (IList<Message>)messageReceiver.PeekAsync(100).Result;
            brokeredMessages.AddRange(messages);
        }
        return brokeredMessages;


    }

    /// <summary>
    /// Get message
    /// </summary>
    /// <param name="messages"></param>
    /// <returns></returns>
    public Dictionary<string, ArrayList> GetMessage(IList<Message> messages)
    {
        Dictionary<string, ArrayList> result = new Dictionary<string, ArrayList>();
        var arxs = new List<ApprovalRequestExpression>();
        foreach (var message in messages)
        {
            try
            {
                byte[] messageContent;
                if (message.UserProperties.ContainsKey("ApprovalRequestVersion") && message.UserProperties["ApprovalRequestVersion"].ToString() == _configurationHelper.appSettings[_environment][Microsoft.CFS.Approvals.Contracts.ConfigurationKey.ApprovalRequestVersion.ToString()])
                    messageContent = _blobStorageHelper.DownloadByteArray(Microsoft.CFS.Approvals.Contracts.Constants.PrimaryMessageContainer, System.Text.Encoding.UTF8.GetString(message.Body)).Result;
                else
                    messageContent = message.Body;

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
    public void GenerateResponse(ApprovalRequestExpression approvalRequestExpressions, Message message, string messageType, ref Dictionary<string, ArrayList> result)
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
                    message?.UserProperties["DeadLetterReason"]?.ToString(),
                    message?.UserProperties["DeadLetterErrorDescription"]?.ToString()
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