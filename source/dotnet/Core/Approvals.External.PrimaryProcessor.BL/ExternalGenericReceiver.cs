// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace Microsoft.Approvals.PrimaryProcessor.BL;

using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using global::Azure.Messaging.ServiceBus;
using Microsoft.CFS.Approvals.Common.BL.Interface;
using Microsoft.CFS.Approvals.Common.DL.Interface;
using Microsoft.CFS.Approvals.Contracts;
using Microsoft.CFS.Approvals.Contracts.DataContracts;
using Microsoft.CFS.Approvals.Core.BL.Interface;
using Microsoft.CFS.Approvals.Data.Azure.Storage.Interface;
using Microsoft.CFS.Approvals.Extensions;
using Microsoft.CFS.Approvals.LogManager.Model;
using Microsoft.CFS.Approvals.LogManager.Provider.Interface;
using Microsoft.CFS.Approvals.Model;
using Microsoft.CFS.Approvals.PayloadReceiver.BL.Interface;
using Microsoft.Extensions.Configuration;

/// <summary>
/// The Generic Receiver class
/// </summary>
public class ExternalGenericReceiver : IApprovalsTopicReceiver
{
    #region Private Variables

    private readonly IPerformanceLogger _performanceLogger;
    private readonly IARConverterFactory _arConverterFactory = null;
    private readonly IConfiguration _config;
    private readonly IBlobStorageHelper _blobHelper;
    private readonly ILogProvider _logProvider = null;

    /// <summary>
    /// Dependency for sending payloads to Service Bus 
    /// </summary>
    private readonly IPayloadDelivery<ApprovalRequestExpression> _payloadDelivery;


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
    public ExternalGenericReceiver(IPerformanceLogger performanceLogger,
        IARConverterFactory arConverterFactory,
        IConfiguration config,
        IBlobStorageHelper blobHelper,
        ILogProvider logProvider,
        IPayloadDelivery<ApprovalRequestExpression> payloadDelivery)
    {
        _performanceLogger = performanceLogger;
        _arConverterFactory = arConverterFactory;
        _blobHelper = blobHelper;
        _config = config;
        _logProvider = logProvider;
        _payloadDelivery = payloadDelivery;
    }

    #endregion Constructor - to set up state of this class for each tenant

    /// <summary>
    /// Business logic to process a message from the main topic.
    /// </summary>
    /// <param name="blobId"></param>
    /// <param name="message"></param>
    public virtual async Task OnMainMessageReceived(string blobId, ServiceBusReceivedMessage message)
    {
        if (blobId != null)
        {
            using (_performanceLogger.StartPerformanceLogger("PerfLog", Constants.WorkerRole, string.Format(Constants.PerfLogAction, TenantInfo.AppName, "Message processing time in ExternalGenericReceiver"), new Dictionary<LogDataKey, object> { { LogDataKey.BlobId, message.Body } }))
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
    public virtual async Task OnRetryMessageRecieved(string blobId, ServiceBusReceivedMessage message)
    {
        throw new NotImplementedException();
    }

    #region Helper Methods

    /// <summary>
    /// Processes the message on a separate thread and calls remaining workflow
    /// </summary>
    /// <param name="blobId"></param>
    /// <param name="brokeredMessage"></param>
    /// <returns></returns>
    private async Task ProcessMessageOnTaskAndAwait(string blobId, ServiceBusReceivedMessage brokeredMessage)
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
            ApprovalRequestExpressionExt requestExpression = null;

            try
            {
                var numberOfRetries = int.Parse(_config[ConfigurationKey.MainTopicFailCountThreshold.ToString()]);
                byte[] message = await _blobHelper.DownloadByteArray(Constants.PrimaryMessageContainer, blobId);

                var converter = _arConverterFactory.GetARConverter(ConfigurationKey.ARConverterExternalClass);
                var dataContractType = Type.GetType(TenantInfo.DataContractClassName);
                if (dataContractType == null)
                {
                    // Throw an exception if the type could not be loaded
                    throw new InvalidOperationException($"Could not load type '{TenantInfo.DataContractClassName}'.");
                }
                // Use reflection to get the generic MapEventToARX<T> method and make it for the specific data contract type
                var mapEventToARXMethod = converter.GetType().GetMethod("MapEventToARX").MakeGenericMethod(dataContractType);

                // Invoke the method to convert the event payload to ApprovalRequestExpressionExt
                requestExpression = (ApprovalRequestExpressionExt)mapEventToARXMethod.Invoke(converter, [message, brokeredMessage, TenantInfo]);

                if (requestExpression == null)
                {
                    throw new InvalidOperationException("Failed to deserialize payload.");
                }
                // Process the message
                if (requestExpression.Operation.Equals(ApprovalRequestOperation.Skip))
                {
                    await _blobHelper.DeleteBlob(Constants.PrimaryMessageContainer, blobId);
                    LogMessageProgress(requestExpression, TrackingEvent.ARXSkippedToProcessInExternalMainTopic, brokeredMessage, null, CriticalityLevel.Yes);
                }
                else
                    await ProcessMainApproval(blobId, requestExpression, brokeredMessage);
            }
            catch (ServiceBusException ex)
            {
                LogMessageProgress(requestExpression, TrackingEvent.ARXFailedToProcessInExternalMainTopic, brokeredMessage, new FailureData() { ID = (TrackingEvent.MoveMessageToDeadletterFromMainTopic + TenantInfo.AppName).ToString(), Message = ex.Message }, CriticalityLevel.Yes);
            }
            catch (Exception ex)
            {
                LogMessageProgress(requestExpression, TrackingEvent.ARXFailedToProcessInExternalMainTopic, brokeredMessage, new FailureData() { ID = ((int)TrackingEvent.MoveMessageToDeadletterFromMainTopic + TenantInfo.AppName).ToString(), Message = ex.Message }, CriticalityLevel.Yes);
            }
        }
    }

    /// <summary>
    /// Main method which hosts the logic for processing a Brokered Message
    /// </summary>
    /// <param name="blobId"></param>
    /// <param name="expression"></param>
    /// <param name="message"></param>
    /// <returns></returns>
    private async Task ProcessMainApproval(string blobId, ApprovalRequestExpressionExt expression, ServiceBusReceivedMessage message)
    {
        var logData = new Dictionary<LogDataKey, object>()
        {
            { LogDataKey._CorrelationId, message.GetCorrelationId() },
            { LogDataKey.IsCriticalEvent, CriticalityLevel.Yes.ToString() },
            { LogDataKey.MessageId, message.MessageId }
        };

        using (_performanceLogger.StartPerformanceLogger("PerfLog", Constants.WorkerRole, string.Format(Constants.PerfLogAction, TenantInfo.AppName, "Processes received Brokered Message"), logData))
        {

            logData.Add(LogDataKey.Xcv, expression.Telemetry.Xcv);
            logData.Add(LogDataKey.Tcv, expression.Telemetry.Tcv);
            logData.Add(LogDataKey.DXcv, expression.ApprovalIdentifier.DisplayDocumentNumber);
            logData.Add(LogDataKey.ReceivedTcv, expression.Telemetry.Tcv);
            logData.Add(LogDataKey.TenantTelemetryData, expression.Telemetry.TenantTelemetry);
            logData.Add(LogDataKey.BusinessProcessName, string.Format(TenantInfo.BusinessProcessName, Constants.BusinessProcessNameSendPayload, expression.Operation));

            // Send payload to Service Bus using IPayloadDelivery
            if (await _payloadDelivery.SendPayload(expression, expression.DocumentTypeId.ToString(), expression.Telemetry.Tcv, expression.Telemetry.Xcv))
            {
                await _blobHelper.DeleteBlob(Constants.PrimaryMessageContainer, blobId);
            }

            LogMessageProgress(expression, TrackingEvent.ARXSuccessfulToProcessInExternalMainTopic, message, null, CriticalityLevel.Yes);
        }
    }

    /// <summary>
    /// Log message progress
    /// </summary>
    /// <param name="expression"></param>
    /// <param name="trackingEvent"></param>
    /// <param name="message"></param>
    /// <param name="failureData"></param>
    /// <param name="criticalityLevel"></param>
    private void LogMessageProgress(ApprovalRequestExpressionExt expression, TrackingEvent trackingEvent, ServiceBusReceivedMessage message, FailureData failureData, CriticalityLevel criticalityLevel)
    {
        if (expression != null)
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
        else
        {
            Dictionary<LogDataKey, object> tenantLogData = new Dictionary<LogDataKey, object>()
            {
                { LogDataKey.IsCriticalEvent, criticalityLevel.ToString() },
                { LogDataKey.BusinessProcessName, string.Format(TenantInfo.BusinessProcessName, Constants.BusinessProcessNameSendPayload, string.Empty) },
                { LogDataKey.TenantId, TenantInfo.DocTypeId },
                { LogDataKey.TenantName, TenantInfo.AppName },
                { LogDataKey.LocalTime, DateTime.UtcNow },
                { LogDataKey._CorrelationId, message.GetCorrelationId() },
                { LogDataKey.MessageId, message.MessageId },
                { LogDataKey.FailureData, failureData }
            };
            _logProvider.LogInformation(trackingEvent, tenantLogData);
        }
    }

    #endregion Helper Methods
}