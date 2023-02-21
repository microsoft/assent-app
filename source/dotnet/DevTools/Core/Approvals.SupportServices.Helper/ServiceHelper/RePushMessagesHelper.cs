// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace Microsoft.CFS.Approvals.SupportServices.Helper.ServiceHelper;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.Azure.ServiceBus;
using Microsoft.CFS.Approvals.Common.DL.Interface;
using Microsoft.CFS.Approvals.Contracts;
using Microsoft.CFS.Approvals.Contracts.DataContracts;
using Microsoft.CFS.Approvals.Core.BL.Interface;
using Microsoft.CFS.Approvals.Data.Azure.Storage.Interface;
using Microsoft.CFS.Approvals.LogManager.Model;
using Microsoft.CFS.Approvals.LogManager.Provider.Interface;
using Microsoft.CFS.Approvals.SupportServices.Helper.ExtensionMethods;
using Microsoft.CFS.Approvals.SupportServices.Helper.Interface;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json.Linq;

public class RePushMessagesHelper : IRePushMessagesHelper 
{
    private readonly IConfiguration _config = null;
    private readonly IPerformanceLogger _logger;
    private readonly IDocumentStatusAuditHelper _documentStatusAuditHelper;
    private readonly IApprovalTenantInfoHelper _approvalTenantInfoHelper;
    private readonly IBlobStorageHelper _blobStorageHelper = null;
    private readonly ILogProvider _logProvider;

    public bool SendNotification { get; set; }

    public RePushMessagesHelper(IConfiguration config, IPerformanceLogger logger, IDocumentStatusAuditHelper documentStatusAuditHelper, 
        IApprovalTenantInfoHelper approvalTenantInfoHelper, ILogProvider logProvider, IBlobStorageHelper blobStorageHelper)
    {
        this._config = config;
        this._logger = logger;
        this._approvalTenantInfoHelper = approvalTenantInfoHelper;
        this._documentStatusAuditHelper = documentStatusAuditHelper;
        this._blobStorageHelper = blobStorageHelper;
        this._logProvider = logProvider;
    }

    /// <summary>
    /// Check and repush if payload exists
    /// </summary>
    /// <param name="jsonData"></param>
    /// <returns></returns>
    public async Task<HttpResponseMessage> CheckAndRePushIfPayloadExists(string jsonData)
    {
        List<string> failedRecords = new List<string>();
        var documentNumber = string.Empty;
        var brokeredMsgId = string.Empty;
        var tenantName = string.Empty;
        var logData = new Dictionary<LogDataKey, object>
        {
            { LogDataKey.AppAction, "CheckAndRePushIfPayloadExists" },
            {LogDataKey.ComponentName, "PayloadReprocessingFunction" }
        };
        try
        {
            var jtokenData = jsonData.FromJson<JToken>();
            var receivedDataJtoken = jtokenData["receivedData"];
            var collectionName = jtokenData["colName"].ToString();

            JContainer container = receivedDataJtoken as JContainer;

            foreach (JToken el in container.Children())
            {
                JProperty p = el as JProperty;
                if (p != null && p.Name == "DisplayDocumentNumber")
                {
                    documentNumber = p.Value.ToString();
                }
                if (p != null && p.Name == "BrokeredMessageID")
                {
                    brokeredMsgId = Guid.Parse(p.Value.ToString()).ToString();
                }
                if (p != null && p.Name == "TenantName")
                {
                    tenantName = p.Value.ToString();
                }
            }

            var tenantId = _approvalTenantInfoHelper.GetTenants().Result.Where(t => t.AppName == tenantName).ToList().FirstOrDefault().DocTypeId;
            var resultsAudit = _documentStatusAuditHelper.GetDocumentHistory(tenantId, string.Empty, documentNumber, collectionName, brokeredMsgId, string.Empty);

            if (resultsAudit != null && resultsAudit.Count > 0)
            {
                SendNotification = true;
                if (jtokenData?["SendNotification"] != null)
                {
                    SendNotification = Convert.ToBoolean(jtokenData?["SendNotification"]);
                }
                failedRecords = await RepushDocumentAsync(resultsAudit);
                if (failedRecords != null && failedRecords.Any())
                {
                    return new HttpResponseMessage(HttpStatusCode.BadRequest) { Content = new StringContent("List of Failed Messages are : " + string.Join<string>(",", failedRecords)) };
                }
                return new HttpResponseMessage(HttpStatusCode.OK) { Content = new StringContent("Records re-pushed successfully") };
            }
            else
            {
                return new HttpResponseMessage(HttpStatusCode.NotFound);
            }
        }
        catch(Exception ex)
        {
            this._logProvider.LogError(TrackingEvent.PayloadReprocessingFailed, ex, logData);
            return new HttpResponseMessage(HttpStatusCode.BadRequest) { Content = new StringContent("List of Failed Messages are : " + string.Join<string>(",", failedRecords)) };
        }
    }

    /// <summary>
    /// Repush document
    /// </summary>
    /// <param name="resultsAudit"></param>
    /// <param name="failedRecords"></param>
    public async Task<List<string>> RepushDocumentAsync(List<dynamic> resultsAudit)
    {
        var logData = new Dictionary<LogDataKey, object>
        {
            { LogDataKey.AppAction, "RepushDocumentAsync" },
            {LogDataKey.ComponentName, "PayloadReprocessingFunction" }
        };
        using (var documentHistorytracer = _logger.StartPerformanceLogger("PerfLog", "Support Portal", string.Format(Constants.PerfLogCommon, "Document History Post")
                 , new Dictionary<string, string>()))
        {
            var failedRecords = new List<string>();
            foreach (var result in resultsAudit)
            {
                bool isSuccess = false;
                var brokeredMsgId = ((JObject)result)["BrokeredMsgId"];
                try
                {
                    isSuccess = await BuildAndSendBrokeredMessageAsync(result);
                    if (!isSuccess)
                    {
                        failedRecords.Add(brokeredMsgId.ToString());
                    }
                }
                catch (Exception ex)
                {
                    this._logProvider.LogError(TrackingEvent.PayloadReprocessingFailed, ex, logData);
                    failedRecords.Add(brokeredMsgId.ToString());
                }
            }
            return failedRecords;
        }
    }

    /// <summary>
    /// Builds the ARX (De-serializes the JSON fetched from Audit agent), creates a Brokered message and send to MSApprovals topic
    /// </summary>
    /// <param name="resultAudit"></param>
    /// <returns></returns>
    public async Task<bool> BuildAndSendBrokeredMessageAsync(dynamic resultAudit)
    {
        var logData = new Dictionary<LogDataKey, object>
        {
            { LogDataKey.AppAction, "BuildAndSendBrokeredMessageAsync" },
            {LogDataKey.ComponentName, "PayloadReprocessingFunction" }
        };
        try
        {
            JObject brokeredMsgProperty = (JObject)resultAudit.BrokeredMessageProperty;
            JObject data = (JObject)resultAudit.ApprovalRequest;
            if (data != null & data?.SelectToken("NotificationDetail")?.Value<JObject>() != null)
                data["NotificationDetail"]["SendNotification"] = SendNotification;

            int noOfRetries = 1;
            ApprovalRequestExpression arxToSend = BuildRetryARX(data, noOfRetries);

            var brokeredMessage = await BuildBrokerMessageAsync(arxToSend, brokeredMsgProperty);
            var client = new TopicClient(_config[ConfigurationKey.ServiceBusConnectionString.ToString()],
                _config[ConfigurationKey.TopicNameMain.ToString()]);
            await client.SendAsync(brokeredMessage);
            return true;
        }
        catch (Exception ex)
        {
            this._logProvider.LogError(TrackingEvent.PayloadReprocessingFailed, ex, logData);
            return false;
        }
    }

    /// <summary>
    /// Builds the Brokered Message from the ARX object
    /// </summary>
    /// <param name="approvalRequestExpression"></param>
    /// <param name="messageProperties"></param>
    /// <returns></returns>
    private async Task<Message> BuildBrokerMessageAsync(ApprovalRequestExpression approvalRequestExpression, JObject messageProperties)
    {
        string messageBody = string.Format("{0}|{1}|{2}", approvalRequestExpression.DocumentTypeId, approvalRequestExpression.ApprovalIdentifier.DisplayDocumentNumber, approvalRequestExpression.Operation.ToString());

        byte[] messageToUpload = ConvertToByteArray(approvalRequestExpression);
        await _blobStorageHelper.UploadByteArray(messageToUpload, Constants.PrimaryMessageContainer, messageBody);
        await _blobStorageHelper.UploadByteArray(messageToUpload, Constants.AuditAgentMessageContainer, messageBody);
        // Create a BrokeredMessage of the customized class,
        Message message = new Message();

        if (approvalRequestExpression != null)
        {
            foreach (var properties in messageProperties)
            {
                message.UserProperties[properties.Key] = properties.Value.ToString();
            }
        }

        message.MessageId = Guid.NewGuid().ToString();
        message.CorrelationId = Guid.NewGuid().ToString();

        // Adding properties to the Message
        message.UserProperties["ApplicationId"] = approvalRequestExpression.DocumentTypeId.ToString();
        message.UserProperties["ApprovalRequestVersion"] = _config[ConfigurationKey.ApprovalRequestVersion.ToString()].ToString();
        message.UserProperties["CreatedDate"] = DateTime.UtcNow;
        message.UserProperties["ContentType"] = "ApprovalRequestExpression";
        message.ContentType = "application/json";
        message.Body = System.Text.Encoding.UTF8.GetBytes(messageBody);

        //Add Additional Properties to the BrokeredMessage
        if (!message.UserProperties.ContainsKey("ApprovalRequestVersion"))
        {
            message.UserProperties["ApprovalRequestVersion"] = "1";
        }
        if (!message.UserProperties.ContainsKey("ApprovalMessageType"))
        {
            message.UserProperties["ApprovalMessageType"] = "Summary";
        }
        return message;
    }

    /// <summary>
    /// Retries building of ARX object
    /// Takes into account the Older format of Additional Dictionary as well which would be present in older Audit Agent data
    /// </summary>
    /// <param name="data"></param>
    /// <param name="noOfRetries"></param>
    /// <returns></returns>
    private ApprovalRequestExpression BuildRetryARX(JObject data, int noOfRetries)
    {
        ApprovalRequestExpression arxToSend = new ApprovalRequestExpressionV1();
        if (noOfRetries > 0)
        {
            try
            {
                arxToSend = data?.ToString().FromJson<ApprovalRequestExpressionV1>();
            }
            catch
            {
                arxToSend = BuildRetryARX(data, noOfRetries - 1);
            }
        }
        else
        {
            // Create new JArray and add item to the Deserialized ARX object
            JToken actionAdditionalDataDictionary = null, additionalDataDictionary = null, summaryAdditionalDataDictionary = null;

            var additionalData = data["AdditionalData"];
            if (data["AdditionalData"] != null)
            {
                // Create new JArray and add item to the Deserialized ARX object
                additionalDataDictionary = additionalData?.ToString().FromJson<JToken>();

                var actionDetail = data["ActionDetail"];
                if (actionDetail != null && actionDetail["AdditionalData"] != null)
                {
                    actionAdditionalDataDictionary = actionDetail["AdditionalData"]?.ToString().FromJson<JToken>();
                }

                var summaryDetail = data["SummaryData"];
                if (summaryDetail != null && summaryDetail["AdditionalData"] != null)
                {
                    summaryAdditionalDataDictionary = summaryDetail["AdditionalData"]?.ToString().FromJson<JToken>();
                }

                RemoveFields(data, new string[] { "AdditionalData" });
            }

            arxToSend = data?.ToString().FromJson<ApprovalRequestExpressionV1>();

            var newActionAdditionalData = new Dictionary<string, string>();
            var newAdditionalData = new Dictionary<string, string>();
            var newSummaryAdditionalData = new Dictionary<string, string>();

            if (additionalDataDictionary != null)
            {
                foreach (JToken item in additionalDataDictionary)
                {
                    newAdditionalData.Add(item["Key"].ToString(), item["Value"].ToString());
                }
            }
            if (actionAdditionalDataDictionary != null)
            {
                foreach (JToken item in actionAdditionalDataDictionary)
                {
                    newActionAdditionalData.Add(item["Key"].ToString(), item["Value"].ToString());
                }
            }
            if (summaryAdditionalDataDictionary != null)
            {
                foreach (JToken item in summaryAdditionalDataDictionary)
                {
                    newSummaryAdditionalData.Add(item["Key"].ToString(), item["Value"].ToString());
                }
            }

            if (arxToSend.ActionDetail != null)
            {
                arxToSend.ActionDetail.AdditionalData = newActionAdditionalData;
            }
            arxToSend.AdditionalData = newAdditionalData;
            if (arxToSend.SummaryData != null)
            {
                arxToSend.SummaryData.AdditionalData = newSummaryAdditionalData;
            }
        }

        arxToSend.OperationDateTime = DateTime.UtcNow;
        return arxToSend;
    }

    /// <summary>
    /// Remove Fields
    /// </summary>
    /// <param name="token"></param>
    /// <param name="fields"></param>
    private void RemoveFields(JToken token, string[] fields)
    {
        JContainer container = token as JContainer;
        if (container == null) return;

        List<JToken> removeList = new List<JToken>();
        foreach (JToken el in container.Children())
        {
            JProperty p = el as JProperty;
            if (p != null && fields.Contains(p.Name))
            {
                removeList.Add(el);
            }
            RemoveFields(el, fields);
        }

        foreach (JToken el in removeList)
        {
            el.Remove();
        }
    }

    /// <summary>
    /// Convert to byte array
    /// </summary>
    /// <param name="ard"></param>
    /// <returns></returns>
    private byte[] ConvertToByteArray(object ard)
    {
        return System.Text.Json.JsonSerializer.SerializeToUtf8Bytes(ard);
    }
}
