// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace Microsoft.CFS.Approvals.AuditProcessor.BL;

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Serialization;
using System.Threading.Tasks;
using System.Xml;
using global::Azure.Messaging.ServiceBus;
using Microsoft.CFS.Approvals.AuditProcessor.BL.Interface;
using Microsoft.CFS.Approvals.AuditProcessor.DL.Interface;
using Microsoft.CFS.Approvals.Contracts;
using Microsoft.CFS.Approvals.Contracts.DataContracts;
using Microsoft.CFS.Approvals.Core.BL.Interface;
using Microsoft.CFS.Approvals.Data.Azure.Storage.Interface;
using Microsoft.CFS.Approvals.Extensions;
using Microsoft.CFS.Approvals.LogManager.Model;
using Microsoft.CFS.Approvals.LogManager.Provider.Interface;
using Microsoft.CFS.Approvals.Model;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;

public class AuditAgentHelper : IAuditAgentHelper
{
    /// <summary>
    /// The log Provider
    /// </summary>
    private readonly ILogProvider _logProvider = null;

    /// <summary>
    /// The AR Converter
    /// </summary>
    private readonly IARConverterFactory _arConverterFactory = null;

    /// <summary>
    /// The Approval Tenant Helper
    /// </summary>
    private readonly IApprovalTenantInfoHelper _approvalTenantInfoHelper = null;

    /// <summary>
    /// The Blob Storage Helper
    /// </summary>
    private readonly IBlobStorageHelper _blobStorageHelper = null;

    /// <summary>
    /// The Audit Agent Logging Helper
    /// </summary>
    private readonly IAuditAgentLoggingHelper _auditAgentLoggingHelper = null;

    /// <summary>
    /// The audit agent data provider instance/
    /// </summary>
    private readonly IAuditAgentDataProvider _auditAgentDataProvider = null;

    /// <summary>
    /// Configuration
    /// </summary>
    private readonly IConfiguration _config = null;

    public AuditAgentHelper(
        IAuditAgentDataProvider auditAgentDataProvider,
        IAuditAgentLoggingHelper auditAgentLoggingHelper,
        IARConverterFactory arConverterFactory,
        IBlobStorageHelper blobStorageHelper,
        ILogProvider logProvider,
        IApprovalTenantInfoHelper approvalTenantInfoHelper,
        IConfiguration config)
    {
        _auditAgentDataProvider = auditAgentDataProvider;
        _auditAgentLoggingHelper = auditAgentLoggingHelper;
        _logProvider = logProvider;
        _arConverterFactory = arConverterFactory;
        _blobStorageHelper = blobStorageHelper;
        _approvalTenantInfoHelper = approvalTenantInfoHelper;
        _config = config;
    }

    /// <summary>
    /// Processes and save ARX into cosmos db
    /// </summary>
    /// <param name="blobId">Blod Id</param>
    /// <param name="message">Service Bus Message</param>
    /// <returns></returns>
    public async Task ProcessMessage(string blobId, ServiceBusReceivedMessage message)
    {
        // Log processing of message has started
        // TODO:: Logging into critical logs still throwing exception. Need to work on this getting the issue fixed.
        await _auditAgentLoggingHelper.Log(TrackingEvent.ARXProcessingStartedByAuditAgent, message, DateTime.UtcNow);

        try
        {
            List<ApprovalRequestExpressionExt> approvalNotificationARXObj = new List<ApprovalRequestExpressionExt>();
            string rawArJson = await Task.Run(() => GetRawAr(blobId, message, out approvalNotificationARXObj));

            ApprovalTenantInfo tenant = (await _approvalTenantInfoHelper.GetTenants(false)).Where(t => t.DocTypeId.Equals(approvalNotificationARXObj.FirstOrDefault().DocumentTypeId.ToString(), StringComparison.InvariantCultureIgnoreCase)).FirstOrDefault();
            if (tenant == null)
            {
                throw new InvalidDataException("Tenant Entity is null and hence the ARN cannot be processed further");
            }

            // Save each ARN within the brokered message into storage
            // Also get back the list of ARNs which did not get processed so that corresponding information can be logged
            List<ApprovalRequestExpressionExt> failedMessages = ProcessARXCollection(approvalNotificationARXObj, rawArJson, tenant, message);

            // Logging information for each failed message
            // and letting the process continue
            // without being impacted due to exceptions raised during ARN processing
            if (failedMessages != null && failedMessages.Count > 0)
            {
                foreach (ApprovalRequestExpressionExt approvalRequestNotificationExt in failedMessages)
                {
                    #region Logging Failures

                    var logData = new Dictionary<LogDataKey, object>()
                    {
                        { LogDataKey.DisplayDocumentNumber, approvalRequestNotificationExt.ApprovalIdentifier.DisplayDocumentNumber },
                        { LogDataKey.DocumentNumber, approvalRequestNotificationExt.ApprovalIdentifier.DocumentNumber },
                        { LogDataKey.OperationType, approvalRequestNotificationExt.Operation }
                    };
                    _logProvider.LogError(TrackingEvent.WorkerStartup, new Exception("Brokered message with DocumentTypeId " + approvalRequestNotificationExt.DocumentTypeId + " could not be processed i.e. written into storage table"), logData);

                    #endregion Logging Failures
                }
            }

            if (message.ApplicationProperties.ContainsKey("ApprovalRequestVersion") && message.ApplicationProperties["ApprovalRequestVersion"].ToString() == _config[Microsoft.CFS.Approvals.Contracts.ConfigurationKey.ApprovalRequestVersion.ToString()])
            {
                await _blobStorageHelper.DeleteBlob(Constants.AuditAgentMessageContainer, blobId);
            }
            // Log processing of message has completed
            // TODO:: Logging into critical logs still throwing exception. Need to work on this getting the issue fixed.
            await _auditAgentLoggingHelper.Log(TrackingEvent.ARXProcessingCompleteByAuditAgent, message, DateTime.UtcNow, approvalNotificationARXObj);
        }
        catch (Exception ex)
        {
            _logProvider.LogError<TrackingEvent, LogDataKey>(TrackingEvent.AuditMessageProcessingFailed, ex);
            throw;
        }
    }

    /// <summary>
    /// Get Json of original Approval request type.
    /// </summary>
    /// <param name="newBrokeredMessage"></param>
    /// <returns></returns>
    private string GetRawAr(string blobId, ServiceBusReceivedMessage newBrokeredMessage, out List<ApprovalRequestExpressionExt> approvalNotificationARXObj)
    {
        byte[] message;
        if (newBrokeredMessage.ApplicationProperties.ContainsKey("ApprovalRequestVersion") && newBrokeredMessage.ApplicationProperties["ApprovalRequestVersion"].ToString() == _config[ConfigurationKey.ApprovalRequestVersion.ToString()])
            message = _blobStorageHelper.DownloadByteArray(Constants.AuditAgentMessageContainer, blobId).Result;
        else
            message = newBrokeredMessage.Body.ToArray();

        var arConverterAdaptor = _arConverterFactory.GetARConverter();
        approvalNotificationARXObj = arConverterAdaptor.GetAR(message, newBrokeredMessage, new ApprovalTenantInfo() { AppName = "AuditAgent", BusinessProcessName = "AuditAgentTenant", RowKey = "0" });

        //To avoid logging PII data
        approvalNotificationARXObj.ForEach(arx => arx.SummaryData = null);
        approvalNotificationARXObj.ForEach(arx => arx.DetailsData = null);

        string rawArJSON = string.Empty;

        //Converting ARX brokered message to json
        bool isARX = newBrokeredMessage.ApplicationProperties.ContainsKey("ContentType") && newBrokeredMessage.ApplicationProperties["ContentType"].ToString() == "ApprovalRequestExpression";
        if (newBrokeredMessage.ApplicationProperties.Keys.Contains("ApprovalRequestVersion") || isARX)
        {
            rawArJSON = approvalNotificationARXObj.ToJson();
        }
        else
        {
            //converting brokered message's body to stream.
            var stream = newBrokeredMessage.Body.ToStream();
            var streamReader = new StreamReader(stream);
            var messageBody = streamReader.ReadToEnd();
            stream.Position = 0;

            //Converting ApprovalRequestNotificationExt brokered message to json string
            if (messageBody.Contains("ApprovalRequestNotificationExt"))
            {
                #region Processing ApprovalRequestNotificationExt

                var arns = new List<MS.IT.CFE.FIN.MSApprovals.Model.ApprovalRequestNotificationExt>();
                if (messageBody.Contains("ArrayOfApprovalRequestNotificationExt"))
                {
                    var serializer = new DataContractSerializer((new List<MS.IT.CFE.FIN.MSApprovals.Model.ApprovalRequestNotificationExt>()).GetType());
                    var reader = XmlDictionaryReader.CreateBinaryReader(stream, XmlDictionaryReaderQuotas.Max);
                    arns = (List<MS.IT.CFE.FIN.MSApprovals.Model.ApprovalRequestNotificationExt>)serializer.ReadObject(reader);
                }
                else
                {
                    var serializer = new DataContractSerializer((new MS.IT.CFE.FIN.MSApprovals.Model.ApprovalRequestNotificationExt()).GetType());
                    var reader = XmlDictionaryReader.CreateBinaryReader(stream, XmlDictionaryReaderQuotas.Max);
                    var approvalInfo = (MS.IT.CFE.FIN.MSApprovals.Model.ApprovalRequestNotificationExt)serializer.ReadObject(reader);
                    arns.Add(approvalInfo);
                }
                rawArJSON = arns.ToJson();

                #endregion Processing ApprovalRequestNotificationExt
            }

            //Converting ApprovalRequestNotification brokered message to json string
            else if (messageBody.Contains("ApprovalRequestNotification"))
            {
                #region Processing ARN

                var arns = new List<Microsoft.Approvals.Framework.Services.Interfaces.DataContracts.ApprovalRequestNotification>();
                if (messageBody.Contains("ArrayOfApprovalRequestNotification"))
                {
                    var serializer = new DataContractSerializer((new List<Microsoft.Approvals.Framework.Services.Interfaces.DataContracts.ApprovalRequestNotification>()).GetType());
                    var reader = XmlDictionaryReader.CreateBinaryReader(stream, XmlDictionaryReaderQuotas.Max);
                    arns = (List<Microsoft.Approvals.Framework.Services.Interfaces.DataContracts.ApprovalRequestNotification>)serializer.ReadObject(reader);
                }
                else
                {
                    var serializer = new DataContractSerializer((new Microsoft.Approvals.Framework.Services.Interfaces.DataContracts.ApprovalRequestNotification()).GetType());
                    var reader = XmlDictionaryReader.CreateBinaryReader(stream, XmlDictionaryReaderQuotas.Max);
                    var approvalInfo = (Microsoft.Approvals.Framework.Services.Interfaces.DataContracts.ApprovalRequestNotification)serializer.ReadObject(reader);
                    arns.Add(approvalInfo);
                }

                rawArJSON = arns.ToJson();

                #endregion Processing ARN
            }
        }

        return rawArJSON;
    }

    /// <summary>
    /// Inserts the ARNExtns provided as inputs
    /// </summary>
    /// <param name="approvalRequestNotificationExts"></param>
    /// <returns></returns>
    private List<ApprovalRequestExpressionExt> ProcessARXCollection(List<ApprovalRequestExpressionExt> approvalRequestNotificationExts, string rawArJson, ApprovalTenantInfo tenant, ServiceBusReceivedMessage brokeredMessageFull)
    {
        // Defining a failed message list to contain all failed ARNExtn objects, to be logged later
        List<ApprovalRequestExpressionExt> failedMessages = new List<ApprovalRequestExpressionExt>();
        foreach (ApprovalRequestExpressionExt approvalRequestNotificationExt in approvalRequestNotificationExts)
        {
            try
            {
                ProcessARX(approvalRequestNotificationExt, rawArJson, tenant, brokeredMessageFull);
            }
            catch (Exception ex)
            {
                var logData = new Dictionary<LogDataKey, object>()
                    {
                        { LogDataKey.DisplayDocumentNumber, approvalRequestNotificationExt.ApprovalIdentifier.DisplayDocumentNumber },
                        { LogDataKey.DocumentNumber, approvalRequestNotificationExt.ApprovalIdentifier.DocumentNumber },
                        { LogDataKey.OperationType, approvalRequestNotificationExt.Operation }
                    };
                _logProvider.LogError(TrackingEvent.AuditMessageProcessingFailed, ex, logData);
                failedMessages.Add(approvalRequestNotificationExt);
            }
        }
        return failedMessages;
    }

    /// <summary>
    /// Processes one Brokered Message and inserts a record into storage
    /// </summary>
    /// <param name="approvalRequestExpressionExt"></param>
    /// <returns></returns>
    private void ProcessARX(ApprovalRequestExpressionExt approvalRequestExpressionExt, string rawArJson, ApprovalTenantInfo tenantInfo, ServiceBusReceivedMessage brokeredMessageFull)
    {
        // Converting the ARN into JSON and writing it
        string arxJSON = approvalRequestExpressionExt.ToJson(new JsonSerializerSettings { NullValueHandling = NullValueHandling.Ignore });

        // Writing to the main audit agent table
        _auditAgentDataProvider.InsertInToDocumentDB(approvalRequestExpressionExt, rawArJson, brokeredMessageFull, "", "");

        // Log that the audit agent has logged the message
        // TODO:: Logging into critical logs still throwing exception. Need to work on this getting the issue fixed.
        _auditAgentLoggingHelper.Log(TrackingEvent.ARXLoggedByAuditAgent, brokeredMessageFull.MessageId, arxJSON, tenantInfo, DateTime.UtcNow, "");
    }
}