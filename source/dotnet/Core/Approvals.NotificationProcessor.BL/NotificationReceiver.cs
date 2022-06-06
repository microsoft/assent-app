// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace Microsoft.CFS.Approvals.NotificationProcessor.BL
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Threading.Tasks;
    using Microsoft.Azure.ServiceBus;
    using Microsoft.CFS.Approvals.Common.BL.Interface;
    using Microsoft.CFS.Approvals.Common.DL.Interface;
    using Microsoft.CFS.Approvals.Contracts;
    using Microsoft.CFS.Approvals.Core.BL.Interface;
    using Microsoft.CFS.Approvals.Data.Azure.Storage.Interface;
    using Microsoft.CFS.Approvals.Domain.BL.Interface;
    using Microsoft.CFS.Approvals.Extensions;
    using Microsoft.CFS.Approvals.LogManager.Model;
    using Microsoft.CFS.Approvals.LogManager.Provider.Interface;
    using Microsoft.CFS.Approvals.Model;
    using Microsoft.CFS.Approvals.NotificationProcessor.BL.Interface;
    using Microsoft.Extensions.Configuration;
    using Newtonsoft.Json;

    /// <summary>
    /// The Notification Receiver class
    /// </summary>
    public class NotificationReceiver : IApprovalsTopicReceiver
    {
        /// <summary>
        /// The performance logger
        /// </summary>
        private readonly IPerformanceLogger _performanceLogger;

        /// <summary>
        /// The configuration
        /// </summary>
        private readonly IConfiguration _config;

        /// <summary>
        /// The log provider
        /// </summary>
        private readonly ILogProvider _logProvider;

        /// <summary>
        /// The tenant factory
        /// </summary>
        private readonly ITenantFactory _tenantFactory;

        /// <summary>
        /// The notification processor
        /// </summary>
        private readonly INotificationProcessor _notificationProcessor;

        /// <summary>
        /// The blob storage helper
        /// </summary>
        private readonly IBlobStorageHelper _blobStorageHelper;

        public ApprovalTenantInfo TenantInfo { get; set; }
        public int FailedCount { get; set; }
        public DateTime LastMessageProcessingTime { get; set; }

        /// <summary>
        /// Constructor of NotificationReceiver
        /// </summary>
        /// <param name="performanceLogger"></param>
        /// <param name="config"></param>
        /// <param name="logProvider"></param>
        /// <param name="tenantFactory"></param>
        /// <param name="notificationProcessor"></param>
        /// <param name="blobStorageHelper"></param>
        public NotificationReceiver(
            IPerformanceLogger performanceLogger,
            IConfiguration config,
            ILogProvider logProvider,
            ITenantFactory tenantFactory,
            INotificationProcessor notificationProcessor,
            IBlobStorageHelper blobStorageHelper)
        {
            _performanceLogger = performanceLogger;
            _config = config;
            _logProvider = logProvider;
            _tenantFactory = tenantFactory;
            _notificationProcessor = notificationProcessor;
            _blobStorageHelper = blobStorageHelper;
        }

        /// <summary>
        /// Business logic to process a message from the main topic.
        /// </summary>
        /// <param name="blobId"></param>
        /// <param name="message"></param>
        public virtual async Task OnMainMessageReceived(string blobId, Message message)
        {
            if (!string.IsNullOrWhiteSpace(blobId))
            {
                using (var notificationMsgReceivedTracer = _performanceLogger.StartPerformanceLogger("PerfLog", "NotificationWorker", string.Format(Constants.PerfLogAction, "Notification", "Notification processing time"), new Dictionary<LogDataKey, object>()))
                {
                    await Task.Run(() => ProcessMessageOnTaskAndAwait(blobId, message));
                }
            }
        }

        /// <summary>
        /// Business logic to retry process a message from the retry topic.
        /// </summary>
        /// <param name="blobId"></param>
        /// <param name="message"></param>
        /// <returns></returns>
        public Task OnRetryMessageRecieved(string blobId, Message message)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Processes the message on a separate thread and calls remaining workflow
        /// </summary>
        /// <param name="blobId"></param>
        /// <param name="serviceBusMessage"></param>
        /// <returns></returns>
        private async Task ProcessMessageOnTaskAndAwait(string blobId, Message serviceBusMessage)
        {
            #region Logging

            var logData = new Dictionary<LogDataKey, object>
            {
                { LogDataKey.MessageId, blobId },
                { LogDataKey.LocalTime, DateTime.UtcNow }
            };

            #endregion Logging

            using (var perfTracer = _performanceLogger.StartPerformanceLogger("PerfLog", "NotificationWorker", string.Format(Constants.PerfLogAction, "Notification", "Processes the message on a separate thread and calls remaining workflow"), logData))
            {
                try
                {
                    logData.Add(LogDataKey.SubscriptionName, _config[ConfigurationKey.SubscriptionNameNotification.ToString()]);

                    byte[] message = await _blobStorageHelper.DownloadByteArray(Constants.NotificationMessageContainer, blobId);

                    var numberOfRetries = int.Parse(_config[ConfigurationKey.MainTopicFailCountThreshold.ToString()]);
                    Stream stream = new MemoryStream(message);
                    int tenantId = 0;

                    var streamReader = new StreamReader(stream);
                    var messageBody = streamReader.ReadToEnd();
                    messageBody = messageBody.Replace("MS.IT.CFE.FIN.Approvals.Model", "Microsoft.CFS.Approvals.Model");
                    stream.Position = 0;
                    if (serviceBusMessage.UserProperties.ContainsKey("ApprovalNotificationDetails") &&
                        serviceBusMessage.UserProperties["ApprovalNotificationDetails"].ToString().ToLower() == "true")
                    {
                        var approvalNotificationDetails = messageBody.FromJson<ApprovalNotificationDetails>();
                        var jobject = JsonConvert.SerializeObject(approvalNotificationDetails);
                        tenantId = (approvalNotificationDetails != null && approvalNotificationDetails.ApprovalTenantInfo != null) ? approvalNotificationDetails.ApprovalTenantInfo.TenantId : 0;

                        #region Logging

                        logData.Add(LogDataKey.DocumentNumber, approvalNotificationDetails?.ApprovalIdentifier?.DocumentNumber);
                        logData.Add(LogDataKey.DisplayDocumentNumber, approvalNotificationDetails?.ApprovalIdentifier?.DisplayDocumentNumber);
                        logData.Add(LogDataKey.DetailStatus, approvalNotificationDetails?.DetailsLoadSuccess);
                        logData.Add(LogDataKey.DXcv, approvalNotificationDetails?.ApprovalIdentifier?.DisplayDocumentNumber);
                        logData.Add(LogDataKey.EventType, Constants.FeatureUsageEvent);
                        logData.Add(LogDataKey.Xcv, approvalNotificationDetails?.Xcv);
                        logData.Add(LogDataKey.Tcv, approvalNotificationDetails?.Tcv);
                        logData.Add(LogDataKey.ReceivedTcv, approvalNotificationDetails?.Tcv);
                        logData.Add(LogDataKey.TenantTelemetryData, approvalNotificationDetails?.TenantTelemetry);
                        logData.Add(LogDataKey.TenantId, approvalNotificationDetails?.ApprovalTenantInfo?.DocTypeId);
                        logData.Add(LogDataKey.TenantName, approvalNotificationDetails?.ApprovalTenantInfo?.AppName);
                        logData.Add(LogDataKey.Approver, approvalNotificationDetails?.DeviceNotificationInfo?.Approver);
                        logData.Add(LogDataKey.NotificationTemplateKey, approvalNotificationDetails?.DeviceNotificationInfo?.NotificationTemplateKey);
                        logData[LogDataKey.EventId] = TrackingEvent.ARXReceivedByNotificationWorker;
                        logData[LogDataKey.EventName] = TrackingEvent.ARXReceivedByNotificationWorker.ToString();
                        _logProvider.LogInformation(TrackingEvent.ARXReceivedByNotificationWorker, logData);

                        #endregion Logging

                        // Process the message if valid
                        await ProcessMainApproval(approvalNotificationDetails, blobId, numberOfRetries);
                    }
                }
                catch (MessageLockLostException lockLostException)
                {
                    _logProvider.LogError(TrackingEvent.ARXFailedByNotificationWorker, lockLostException, logData);
                }
                catch (Exception ex)
                {
                    logData[LogDataKey.EventId] = TrackingEvent.ARXFailedByNotificationWorker;
                    logData[LogDataKey.EventName] = TrackingEvent.ARXFailedByNotificationWorker.ToString();
                    _logProvider.LogError(TrackingEvent.ARXFailedByNotificationWorker, ex, logData);
                }
            }
        }

        /// <summary>
        /// Main method which hosts the logic for processing a Blob Message
        /// </summary>
        /// <param name="requestExpressions"></param>
        /// <param name="blobId"></param>
        /// <param name="numberOfRetries"></param>
        private async Task ProcessMainApproval(ApprovalNotificationDetails requestExpressions, string blobId, int numberOfRetries)
        {
            using (var perfTracer = _performanceLogger.StartPerformanceLogger("PerfLog", "NotificationWorker", string.Format(Constants.PerfLogAction, requestExpressions.ApprovalTenantInfo.AppName, "Processes received Brokered Message")
                   , new Dictionary<LogDataKey, object>())) // TODO: log message ID here.
            {
                if (numberOfRetries >= 0)
                {
                    // Process the Brokered Message and check if any errors occurred
                    var failedRequest = await ProcessNotificationDetails(requestExpressions, blobId);
                    if (failedRequest != null)
                    {
                        await ProcessMainApproval(requestExpressions, blobId, numberOfRetries - 1);
                    }
                    else
                    {
                        if (await _blobStorageHelper.DoesExist(Constants.NotificationMessageContainer, blobId))
                        {
                            await _blobStorageHelper.DeleteBlob(Constants.NotificationMessageContainer, blobId);
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Processes notification details and send email/device notifications
        /// </summary>
        /// <param name="requestExpressions"></param>
        /// <param name="blobId"></param>
        /// <returns></returns>
        private async Task<ApprovalNotificationDetails> ProcessNotificationDetails(
            ApprovalNotificationDetails requestExpressions,
            string blobId)
        {
            #region Logging

            var logData = new Dictionary<LogDataKey, object>
            {
                { LogDataKey.Xcv, requestExpressions.Xcv },
                { LogDataKey.ReceivedTcv, requestExpressions.Tcv },
                { LogDataKey.TenantTelemetryData, requestExpressions.TenantTelemetry },
                { LogDataKey.Tcv, requestExpressions.Tcv },
                { LogDataKey.EventType, Constants.FeatureUsageEvent },
                { LogDataKey.MessageId, blobId },
                { LogDataKey.DisplayDocumentNumber, requestExpressions?.ApprovalIdentifier?.DisplayDocumentNumber },
                { LogDataKey.DocumentNumber, requestExpressions?.ApprovalIdentifier?.DocumentNumber },
                { LogDataKey.TenantId, requestExpressions?.ApprovalTenantInfo?.DocTypeId },
                { LogDataKey.TenantName, requestExpressions?.ApprovalTenantInfo?.AppName },
                { LogDataKey.DetailStatus, requestExpressions?.DetailsLoadSuccess },
                { LogDataKey.Approver, requestExpressions?.DeviceNotificationInfo?.Approver },
                { LogDataKey.NotificationTemplateKey, requestExpressions?.DeviceNotificationInfo?.NotificationTemplateKey }
            };

            #endregion Logging

            using (var perfTracer = _performanceLogger.StartPerformanceLogger("PerfLog", "NotificationWorker", string.Format(Constants.PerfLogAction, requestExpressions.ApprovalTenantInfo.AppName, "Send Notification"), logData))
            {
                ApprovalNotificationDetails approvalRequest = null;
                try
                {
                    ITenant tenant = _tenantFactory.GetTenant(requestExpressions.ApprovalTenantInfo);

                    await _notificationProcessor.SendNotifications(requestExpressions, tenant);

                    await _notificationProcessor.SendTeamsNotifications(requestExpressions);
                }
                catch (Exception ex)
                {
                    logData.Add(LogDataKey.DXcv, requestExpressions?.ApprovalIdentifier?.DisplayDocumentNumber);
                    _logProvider.LogWarning(TrackingEvent.NotificationProcessingFail, logData, ex);
                    approvalRequest = requestExpressions;
                }
                return approvalRequest;
            }
        }
    }
}