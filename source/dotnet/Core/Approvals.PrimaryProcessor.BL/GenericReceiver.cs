// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace Microsoft.CFS.Approvals.PrimaryProcessor.BL;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using global::Azure.Identity;
using global::Azure.Messaging.ServiceBus;
using Microsoft.Azure.ServiceBus;
using Microsoft.CFS.Approvals.Common.BL.Interface;
using Microsoft.CFS.Approvals.Common.DL.Interface;
using Microsoft.CFS.Approvals.Contracts;
using Microsoft.CFS.Approvals.Contracts.DataContracts;
using Microsoft.CFS.Approvals.Core.BL.Interface;
using Microsoft.CFS.Approvals.Data.Azure.Storage.Interface;
using Microsoft.CFS.Approvals.Domain.BL.Interface;
using Microsoft.CFS.Approvals.Extensions;
using Microsoft.CFS.Approvals.LogManager.Model;
using Microsoft.CFS.Approvals.LogManager.Provider.Interface;
using Microsoft.CFS.Approvals.Model;
using Microsoft.CFS.Approvals.PrimaryProcessor.BL.Interface;
using Microsoft.CFS.Approvals.Utilities.Extension;
using Microsoft.Extensions.Configuration;

/// <summary>
/// The Generic Receiver class
/// </summary>
public class GenericReceiver : IApprovalsTopicReceiver
{
    #region Private Variables

    private readonly string _serviceBusNamespace;
    private readonly string _approvalsRetryTopic;
    private readonly IPerformanceLogger _performanceLogger;
    private readonly IARConverterFactory _arConverterFactory = null;
    private readonly IConfiguration _config;
    private readonly IBlobStorageHelper _blobHelper;
    private readonly ILogProvider _logProvider = null;
    private readonly IValidationHelper _validationHelper = null;
    private readonly ITenantFactory _tenantFactory = null;
    private readonly ITableHelper _tableHelper = null;
    private readonly IApprovalPresenter _approvalPresenter = null;

    #endregion Private Variables

    #region Public Properties

    public ApprovalTenantInfo TenantInfo { get; set; }

    public DateTime LastMessageProcessingTime { get; set; }

    public int FailedCount { get; set; }

    #endregion Public Properties

    #region Constructor - to set up state of this class for each tenant

    /// <summary>
    /// Create Generic Receiver
    /// </summary>
    public GenericReceiver(IPerformanceLogger performanceLogger,
        IARConverterFactory arConverterFactory,
        IConfiguration config,
        IBlobStorageHelper blobHelper,
        ILogProvider logProvider,
        IValidationHelper validationHelper,
        ITenantFactory tenantFactory,
        IApprovalPresenter approvalPresenter,
        ITableHelper tableHelper)
    {
        _performanceLogger = performanceLogger;
        _arConverterFactory = arConverterFactory;
        _blobHelper = blobHelper;
        _config = config;
        _logProvider = logProvider;
        _validationHelper = validationHelper;
        _tenantFactory = tenantFactory;
        _tableHelper = tableHelper;
        _approvalPresenter = approvalPresenter;

        // Getting the Service Bus Connection Strings
        _serviceBusNamespace = _config[ConfigurationKey.ServiceBusNamespace.ToString()];

        // Getting the Main and Retry Topic Names
        _approvalsRetryTopic = _config[ConfigurationKey.TopicNameRetry.ToString()];
    }

    #endregion Constructor - to set up state of this class for each tenant

    /// <summary>
    /// Business logic to process a message from the main topic.
    /// </summary>
    /// <param name="blobId"></param>
    /// <param name="message"></param>
    public virtual async Task OnMainMessageReceived(string blobId, Message message)
    {
        if (blobId != null)
        {
            using (_performanceLogger.StartPerformanceLogger("PerfLog", Constants.WorkerRole, string.Format(Constants.PerfLogAction, TenantInfo.AppName, "Message processing time in GenericReceiver"), new Dictionary<LogDataKey, object> { { LogDataKey.BlobId, message.Body } }))
            {
                // Processing the message on a separate thread and freeing up the main thread to receive new messages
                // This will also ensure the number of new messages picked up will never exceed Throttling limit as the call is awaited
                // This is mainly to allow fast processing of messages especially for tenants who sent Summary along with message
                // This will avoid synchronous calling of messages for such tenants
                // For other tenants whose summary is fetch making a http call, things will work as before
                await ProcessMessageOnTaskAndAwait(blobId, message);
            }
        }
    }

    /// <summary>
    /// Business logic to retry process a message from the retry topic.
    /// </summary>
    /// <param name="blobId"></param>
    /// <param name="message"></param>
    /// <returns></returns>
    public virtual async Task OnRetryMessageRecieved(string blobId, Message message)
    {
        if (message != null)
        {
            var logData = new Dictionary<LogDataKey, object>
            {
                { LogDataKey.MessageId, Guid.Parse(message.MessageId).ToString() }
            };

            try
            {
                string correlationId = message.GetCorrelationId();
                logData.Add(LogDataKey._CorrelationId, correlationId);
                logData.Add(LogDataKey.BrokerMessage, message.ToString());

                if (TenantInfo != null)
                {
                    logData.Add(LogDataKey.SubscriptionName, TenantInfo.Subscription);
                    logData.Add(LogDataKey.TenantId, TenantInfo.TenantId);
                    logData.Add(LogDataKey.TenantName, TenantInfo.AppName);
                    logData.Add(LogDataKey.DocumentTypeId, TenantInfo.DocTypeId);
                }

                using (_performanceLogger.StartPerformanceLogger("PerfLog", Constants.WorkerRole, string.Format(Constants.PerfLogAction, TenantInfo.AppName, "Retry message processing time in GenericReceiver"), logData))
                {
                    byte[] messageContent;
                    if (message.UserProperties.ContainsKey("ApprovalRequestVersion") && message.UserProperties["ApprovalRequestVersion"].ToString() == _config[ConfigurationKey.ApprovalRequestVersion.ToString()])
                        messageContent = await _blobHelper.DownloadByteArray(Constants.PrimaryMessageContainer, blobId);
                    else
                        messageContent = message.Body;

                    var arConverterAdaptor = _arConverterFactory.GetARConverter();
                    List<ApprovalRequestExpressionExt> requestExpressions = arConverterAdaptor.GetAR(messageContent, message, TenantInfo);
                    logData.Add(LogDataKey.Xcv, requestExpressions.FirstOrDefault().Telemetry.Xcv);
                    logData.Add(LogDataKey.Tcv, requestExpressions.FirstOrDefault().Telemetry.Tcv);
                    logData.Add(LogDataKey.ReceivedTcv, requestExpressions.FirstOrDefault().Telemetry.Tcv);
                    logData.Add(LogDataKey.TenantTelemetryData, requestExpressions.FirstOrDefault().Telemetry.TenantTelemetry);
                    logData.Add(LogDataKey.BusinessProcessName, string.Format(TenantInfo.BusinessProcessName, Constants.BusinessProcessNameSendPayload, requestExpressions.FirstOrDefault().Operation));
                    if (logData.ContainsKey(LogDataKey.EventId))
                    {
                        logData.Remove(LogDataKey.EventId);
                    }

                    if (logData.ContainsKey(LogDataKey.EventName))
                    {
                        logData.Remove(LogDataKey.EventName);
                    }

                    logData.Add(LogDataKey.EventId, TrackingEvent.NewMessageRecievedInRetryTopic + TenantInfo.TenantId);
                    logData.Add(LogDataKey.EventName, TrackingEvent.NewMessageRecievedInRetryTopic.ToString());
                    LogMessageProgress(requestExpressions, TrackingEvent.ARXReceivedSuccessfullyByServiceBusInRetryTopic, message, null, CriticalityLevel.Yes);

                    _approvalPresenter.TenantInfo = TenantInfo;
                    var failedRequest = await _approvalPresenter.ProcessApprovalRequestExpressions(requestExpressions, message);
                    if (failedRequest != null && failedRequest.Count > 0)
                    {
                        throw new InvalidOperationException("Request failed while processing in retry topic. Identifier: " + failedRequest.FirstOrDefault().ApprovalIdentifier.ToJson());
                    }

                    if (logData.ContainsKey(LogDataKey.EventId))
                    {
                        logData.Remove(LogDataKey.EventId);
                    }

                    if (logData.ContainsKey(LogDataKey.EventName))
                    {
                        logData.Remove(LogDataKey.EventName);
                    }

                    logData.Add(LogDataKey.EventId, TrackingEvent.MessageCompleteSuccessFromRetryTopic + TenantInfo.TenantId);
                    logData.Add(LogDataKey.EventName, TrackingEvent.MessageCompleteSuccessFromRetryTopic.ToString());
                    LogMessageProgress(requestExpressions, TrackingEvent.ARXProcessedSuccessfullyInRetryTopic, message, null, CriticalityLevel.Yes);
                }
            }
            catch (Exception ex)
            {
                if (logData.ContainsKey(LogDataKey.EventId))
                {
                    logData.Remove(LogDataKey.EventId);
                }

                if (logData.ContainsKey(LogDataKey.EventName))
                {
                    logData.Remove(LogDataKey.EventName);
                }

                logData.Add(LogDataKey.EventId, TrackingEvent.MoveMessageToDeadletterFromRetryTopic + TenantInfo.TenantId);
                logData.Add(LogDataKey.EventName, TrackingEvent.MoveMessageToDeadletterFromRetryTopic.ToString());
                _logProvider.LogError((int)TrackingEvent.MoveMessageToDeadletterFromRetryTopic + TenantInfo.TenantId, ex, logData);
            }
        }
    }

    #region Helper Methods

    /// <summary>
    /// Processes the message on a separate thread and calls remaining workflow
    /// </summary>
    /// <param name="blobId"></param>
    /// <param name="brokeredMessage"></param>
    /// <returns></returns>
    private async Task ProcessMessageOnTaskAndAwait(string blobId, Message brokeredMessage)
    {
        var logData = new Dictionary<LogDataKey, object>()
        {
            { LogDataKey.MessageId, brokeredMessage.MessageId},
            { LogDataKey.TenantName, TenantInfo.AppName },
            { LogDataKey.TenantId, TenantInfo.TenantId },
            { LogDataKey.DocumentTypeId, TenantInfo.DocTypeId },
            { LogDataKey.SubscriptionName, TenantInfo.Subscription }
        };

        using (_performanceLogger.StartPerformanceLogger("PerfLog", Constants.WorkerRole, string.Format(Constants.PerfLogAction, TenantInfo.AppName, "Processes the message on a separate thread and calls remaining workflow"), new Dictionary<LogDataKey, object>()))
        {
            LastMessageProcessingTime = DateTime.Now;
            string corId = brokeredMessage.GetCorrelationId();
            logData.Add(LogDataKey._CorrelationId, corId);
            List<ApprovalRequestExpressionExt> requestExpressions = null;

            try
            {
                var numberOfRetries = int.Parse(_config[ConfigurationKey.MainTopicFailCountThreshold.ToString()]);
                byte[] message;
                if (brokeredMessage.UserProperties.ContainsKey("ApprovalRequestVersion") && brokeredMessage.UserProperties["ApprovalRequestVersion"].ToString() == _config[ConfigurationKey.ApprovalRequestVersion.ToString()])
                    message = await _blobHelper.DownloadByteArray(Constants.PrimaryMessageContainer, blobId);
                else
                    message = brokeredMessage.Body;

                var arConverterAdaptor = _arConverterFactory.GetARConverter();
                requestExpressions = arConverterAdaptor.GetAR(message, brokeredMessage, TenantInfo);

                foreach (var expression in requestExpressions)
                {
                    // Re-initialise the logData for each expression being processed
                    logData = new Dictionary<LogDataKey, object>
                    {
                        { LogDataKey.MessageId, brokeredMessage.MessageId },
                        { LogDataKey.TenantName, TenantInfo.AppName },
                        { LogDataKey.TenantId, TenantInfo.TenantId },
                        { LogDataKey.DocumentTypeId, TenantInfo.DocTypeId },
                        { LogDataKey.Xcv, expression.Telemetry?.Xcv },
                        { LogDataKey.DXcv, expression.ApprovalIdentifier?.DisplayDocumentNumber },
                        { LogDataKey.Tcv, expression.Telemetry?.Tcv },
                        { LogDataKey.ReceivedTcv, expression.Telemetry?.Tcv },
                        { LogDataKey.TenantTelemetryData, expression.Telemetry?.TenantTelemetry },
                        { LogDataKey.BusinessProcessName, string.Format(TenantInfo.BusinessProcessName, Constants.BusinessProcessNameSendPayload, expression.Operation) }
                    };

                    LogMessageProgress(new List<ApprovalRequestExpressionExt> { expression }, TrackingEvent.ARXReceivedByMainWorker, brokeredMessage, null, CriticalityLevel.Yes);

                    // Server Side validations
                    LogMessageProgress(new List<ApprovalRequestExpressionExt> { expression }, TrackingEvent.ARXServerSideValidationStarted, brokeredMessage, null, CriticalityLevel.No);

                    List<System.ComponentModel.DataAnnotations.ValidationResult> validationResults = await _validationHelper.ValidateApprovalRequestExpression(expression, blobId, TenantInfo);

                    LogMessageProgress(new List<ApprovalRequestExpressionExt> { expression }, TrackingEvent.ARXServerSideValidationCompleted, brokeredMessage, null, CriticalityLevel.No);

                    // If there was an error in validating the approval request then throw
                    // an exception so that the message is not marked as complete
                    if (validationResults != null && validationResults.Count > 0)
                    {
                        string errorMessage;
                        if (expression.ApprovalIdentifier != null)
                        {
                            errorMessage = "Approval Request Expression (DocumentNumber: " + expression.ApprovalIdentifier.DocumentNumber
                                                + " - " + expression.ApprovalIdentifier.DisplayDocumentNumber + ") validation failed: " + validationResults.ToJson();
                        }
                        else
                        {
                            errorMessage = "Approval Request Expression (Message ID: " + blobId + ") validation failed: " + validationResults.ToJson();
                        }

                        logData.Add(LogDataKey.ErrorMessage, errorMessage);
                        logData.Add(LogDataKey.ValidationResults, validationResults);
                        _logProvider.LogError(TrackingEvent.PayloadValidationFailure, new Exception(TrackingEvent.PayloadValidationFailure.ToString()), logData);

                        LogMessageProgress(new List<ApprovalRequestExpressionExt> { expression }, TrackingEvent.ARXValidationFailed, brokeredMessage, new FailureData() { Message = validationResults.Count + " business rules failed", ID = ((int)TrackingEvent.ARXValidationFailed).ToString() }, CriticalityLevel.Yes);
                        return;
                    }

                    LogMessageProgress(new List<ApprovalRequestExpressionExt> { expression }, TrackingEvent.ARXValidationSuccess, brokeredMessage, null, CriticalityLevel.No);
                }

                // ITenant object created to change ARX object. This could be modified for CREATE & UPDATE operations only
                ITenant tenant = _tenantFactory.GetTenant(TenantInfo);
                tenant.ModifyApprovalRequestExpression(requestExpressions);

                MaintainQueue(requestExpressions, "Insert");

                // Process the message if valid
                await ProcessMainApproval(blobId, requestExpressions, brokeredMessage, numberOfRetries);

                MaintainQueue(requestExpressions, "Delete");
            }
            catch (MessageLockLostException lockLostException)
            {
                logData[LogDataKey.EventId] = TrackingEvent.MoveMessageToDeadletterFromMainTopic;
                logData[LogDataKey.EventName] = TrackingEvent.MoveMessageToDeadletterFromMainTopic.ToString();
                LogMessageProgress(requestExpressions, TrackingEvent.ARXFailedToProcessInMainTopic, brokeredMessage, new FailureData() { ID = (TrackingEvent.MoveMessageToDeadletterFromMainTopic + TenantInfo.TenantId).ToString(), Message = lockLostException.Message }, CriticalityLevel.Yes);
            }
            catch (Exception ex)
            {
                logData[LogDataKey.EventId] = TrackingEvent.MoveMessageToDeadletterFromMainTopic;
                logData[LogDataKey.EventName] = TrackingEvent.MoveMessageToDeadletterFromMainTopic.ToString();
                LogMessageProgress(requestExpressions, TrackingEvent.ARXFailedToProcessInMainTopic, brokeredMessage, new FailureData() { ID = ((int)TrackingEvent.MoveMessageToDeadletterFromMainTopic + TenantInfo.TenantId).ToString(), Message = ex.Message }, CriticalityLevel.Yes);
            }
        }
    }

    /// <summary>
    /// Main method which hosts the logic for processing a Brokered Message
    /// </summary>
    /// <param name="blobId"></param>
    /// <param name="message"></param>
    /// <param name="requestExpressions"></param>
    /// <param name="numberOfRetries"></param>
    /// <returns></returns>
    private async Task ProcessMainApproval(string blobId, List<ApprovalRequestExpressionExt> requestExpressions, Message message, int numberOfRetries)
    {
        var logData = new Dictionary<LogDataKey, object>()
        {
            { LogDataKey._CorrelationId, message.GetCorrelationId() },
            { LogDataKey.IsCriticalEvent, CriticalityLevel.Yes.ToString() },
            { LogDataKey.MessageId, message.MessageId }
        };

        using (_performanceLogger.StartPerformanceLogger("PerfLog", Constants.WorkerRole, string.Format(Constants.PerfLogAction, TenantInfo.AppName, "Processes received Brokered Message"), logData))
        {
            if (numberOfRetries < 0)
            {
                var retryMessages = new List<ServiceBusMessage>();
                foreach (var expression in requestExpressions)
                {
                    logData.Add(LogDataKey.Xcv, expression.Telemetry.Xcv);
                    logData.Add(LogDataKey.Tcv, expression.Telemetry.Tcv);
                    logData.Add(LogDataKey.DXcv, expression.ApprovalIdentifier.DisplayDocumentNumber);
                    logData.Add(LogDataKey.ReceivedTcv, expression.Telemetry.Tcv);
                    logData.Add(LogDataKey.TenantTelemetryData, expression.Telemetry.TenantTelemetry);
                    logData.Add(LogDataKey.BusinessProcessName, string.Format(TenantInfo.BusinessProcessName, Constants.BusinessProcessNameSendPayload, expression.Operation));

                    if (message.UserProperties.ContainsKey("ApprovalRequestVersion") && message.UserProperties["ApprovalRequestVersion"].ToString() == _config[ConfigurationKey.ApprovalRequestVersion.ToString()])
                    {
                        byte[] messageToUpload = ConvertToByteArray(expression);
                        await _blobHelper.UploadByteArray(messageToUpload, Constants.PrimaryMessageContainer, blobId);
                    }
                    var brokreredMessage = BuildBrokeredMessage(expression, message);
                    retryMessages.Add(brokreredMessage.Map());
                }

                ServiceBusClient client = new ServiceBusClient(_serviceBusNamespace + ".servicebus.windows.net", new DefaultAzureCredential());
                var messageSender = client.CreateSender(_approvalsRetryTopic);
                await messageSender.SendMessagesAsync(retryMessages);
                LogMessageProgress(requestExpressions, TrackingEvent.BrokeredMessageMovedToRetryTopic, message, new FailureData() { ID = ((int)TrackingEvent.MoveMessageToRetry).ToString(), Message = TrackingEvent.MoveMessageToRetry.ToString() }, CriticalityLevel.Yes);
            }
            else
            {
                // Process the Brokered Message and check if any errors occurred
                _approvalPresenter.TenantInfo = TenantInfo;
                var failedApprovalRequests = await _approvalPresenter.ProcessApprovalRequestExpressions(requestExpressions, message);
                foreach (var requestExpression in requestExpressions)
                {
                    logData.Add(LogDataKey.Xcv, requestExpression.Telemetry.Xcv);
                    logData.Add(LogDataKey.Tcv, requestExpression.Telemetry.Tcv);
                    logData.Add(LogDataKey.ReceivedTcv, requestExpression.Telemetry.Tcv);
                    logData.Add(LogDataKey.TenantTelemetryData, requestExpression.Telemetry.TenantTelemetry);
                    logData.Add(LogDataKey.BusinessProcessName, string.Format(TenantInfo.BusinessProcessName, Constants.BusinessProcessNameSendPayload, requestExpression.Operation));
                    if (!failedApprovalRequests.Any(f => f.ApprovalIdentifier.DisplayDocumentNumber == requestExpression.ApprovalIdentifier.DisplayDocumentNumber))
                    {
                        if (message.UserProperties.ContainsKey("ApprovalRequestVersion") && message.UserProperties["ApprovalRequestVersion"].ToString() == _config[ConfigurationKey.ApprovalRequestVersion.ToString()])
                        {
                            await _blobHelper.DeleteBlob(Constants.PrimaryMessageContainer, blobId);
                        }
                        LogMessageProgress(requestExpressions, TrackingEvent.ARXProcessedSuccessfullyInMainTopic, message, null, CriticalityLevel.Yes);
                    }
                }

                // Check if there were errors while processing ARX
                if (failedApprovalRequests.Count > 0)
                {
                    Thread.Sleep(10000);    // Added 10 seconds delay in retry
                    await ProcessMainApproval(blobId, failedApprovalRequests, message, numberOfRetries - 1);
                }
            }
        }
    }

    /// <summary>
    /// This method validates if a payload related to the particular document number is already in queue for processing.
    /// </summary>
    /// <param name="requestExpressions"></param>
    /// <returns></returns>
    private bool PayloadInQueue(List<ApprovalRequestExpressionExt> requestExpressions)
    {
        List<BaseTableEntity> result = null;
        foreach (var requestExpression in requestExpressions)
        {
            result = _tableHelper.GetTableEntityListByPartitionKey<BaseTableEntity>("ApprovalRequestExpressionQueue", requestExpression.ApprovalIdentifier.GetDocNumber(TenantInfo));
        }
        if (result != null && result.Any())
        {
            return true;
        }
        else
            return false;
    }

    /// <summary>
    /// This method maintains a queue while processing the payload, deletes the request from queue after processing is complete.
    /// </summary>
    /// <param name="requestExpressions"></param>
    /// <param name="operation"></param>
    /// <returns></returns>
    private void MaintainQueue(List<ApprovalRequestExpressionExt> requestExpressions, string operation)
    {
        int timeLimit = 0;
        int loopSleepTime = 10;
        foreach (var requestExpression in requestExpressions)
        {
            var row = new BaseTableEntity
            {
                PartitionKey = requestExpression?.ApprovalIdentifier?.GetDocNumber(TenantInfo),
                RowKey = String.IsNullOrEmpty(requestExpression?.Approvers?.FirstOrDefault()?.Alias) ? (String.IsNullOrEmpty(requestExpression.DeleteFor?.FirstOrDefault()) ? requestExpression.ApprovalIdentifier.GetDocNumber(TenantInfo) : requestExpression.DeleteFor.FirstOrDefault()) : requestExpression.Approvers.FirstOrDefault().Alias
            };

            if (operation == "Insert")
            {
                //Check if the message is already in the queue for processing
                while (PayloadInQueue(requestExpressions) && timeLimit <= Convert.ToInt32(ConfigurationKey.ArxQueueWaitTime))
                {
                    Thread.Sleep(1000 * loopSleepTime);

                    //Maintaining time limit to avoid infinite loop
                    timeLimit += loopSleepTime;
                }

                _tableHelper.InsertOrReplace("ApprovalRequestExpressionQueue", row);
            }
            else if (operation == "Delete")
                _tableHelper.DeleteRow("ApprovalRequestExpressionQueue", row);
        }
    }

    /// <summary>
    /// Log message progress
    /// </summary>
    /// <param name="expressions"></param>
    /// <param name="trackingEvent"></param>
    /// <param name="message"></param>
    /// <param name="failureData"></param>
    /// <param name="criticalityLevel"></param>
    private void LogMessageProgress(List<ApprovalRequestExpressionExt> expressions, TrackingEvent trackingEvent, Message message, FailureData failureData, CriticalityLevel criticalityLevel)
    {
        foreach (var expression in expressions)
        {
            StringBuilder approverList = new StringBuilder();
            if (expression.Approvers != null && expression.Approvers.Count > 0)
            {
                for (int i = 0; i < expression.Approvers.Count; i++)
                {
                    if (i > 0)
                    {
                        approverList.Append(", ");
                    }

                    approverList.Append(expression.Approvers[i].Alias);
                }
            }

            Dictionary<LogDataKey, object> tenantLogData = new Dictionary<LogDataKey, object>()
            {
                { LogDataKey.IsCriticalEvent, criticalityLevel.ToString() },
                { LogDataKey.Tcv, expression.Telemetry.Tcv },
                { LogDataKey.ReceivedTcv, expression.Telemetry.Tcv },
                { LogDataKey.TenantTelemetryData, expression.Telemetry.TenantTelemetry },
                { LogDataKey.BusinessProcessName, string.Format(TenantInfo.BusinessProcessName, Constants.BusinessProcessNameSendPayload, expression.Operation) },
                { LogDataKey.Xcv, expression.Telemetry.Xcv },
                { LogDataKey.DXcv, expression.ApprovalIdentifier.DisplayDocumentNumber },
                { LogDataKey.DocumentNumber, expression.ApprovalIdentifier.DocumentNumber },
                { LogDataKey.DisplayDocumentNumber, expression.ApprovalIdentifier.DisplayDocumentNumber },
                { LogDataKey.FiscalYear, expression.ApprovalIdentifier.FiscalYear },
                { LogDataKey.Approver, approverList.ToString() },
                { LogDataKey.OperationType, expression.Operation.ToString() },
                { LogDataKey.TenantId, TenantInfo.DocTypeId },
                { LogDataKey.TenantName, TenantInfo.AppName },
                { LogDataKey.LocalTime, DateTime.UtcNow },
                { LogDataKey._CorrelationId, message.GetCorrelationId() },
                { LogDataKey.FailureData, failureData }
            };

            _logProvider.LogInformation(trackingEvent, tenantLogData);
        }
    }

    /// <summary>
    /// Builds a relevant Brokered brokeredMessage using ARX and adding additional useful information to brokeredMessage properties
    /// </summary>
    /// <param name="approvalRequestExpression"> Arx from which brokered brokeredMessage will be created.</param>
    /// <param name="message">Brokered message property object</param>
    /// <returns>Returns the brokered brokeredMessage.</returns>
    private Message BuildBrokeredMessage(ApprovalRequestExpression approvalRequestExpression, Message message)
    {
        if (approvalRequestExpression == null)
            throw new ArgumentNullException("approvalRequestExpression", "Approval Request Expression cannot be null");

        Message brokeredMessage = new Message(message.Body)
        {
            MessageId = message.MessageId
        };
        brokeredMessage.CorrelationId = message.CorrelationId;

        // Adding properties to the Message
        brokeredMessage.UserProperties["ApplicationId"] = message.UserProperties.ToJson().FromJson<Dictionary<string, string>>()["ApplicationId"];
        brokeredMessage.UserProperties["ApprovalRequestVersion"] = message.UserProperties["ApprovalRequestVersion"]?.ToString();
        brokeredMessage.UserProperties["CreatedDate"] = DateTime.UtcNow;
        brokeredMessage.UserProperties["ContentType"] = "ApprovalRequestExpression";
        brokeredMessage.ContentType = "application/json";

        return brokeredMessage;
    }

    /// <summary>
    /// Convert To Byte Array
    /// </summary>
    /// <param name="requestExpression"></param>
    private byte[] ConvertToByteArray(object requestExpression)
    {
        return System.Text.Json.JsonSerializer.SerializeToUtf8Bytes(requestExpression);
    }

    #endregion Helper Methods
}