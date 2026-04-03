// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace Approvals.ReassignmentProcessor.BL
{
    using System.Threading.Tasks;
    using Approvals.ReassignmentProcessor.BL.Interface;
    using Azure.Messaging.ServiceBus;
    using Microsoft.CFS.Approvals.Common.BL.Interface;
    using Microsoft.CFS.Approvals.Common.DL.Interface;
    using Microsoft.CFS.Approvals.Contracts;
    using Microsoft.CFS.Approvals.Contracts.DataContracts;
    using Microsoft.CFS.Approvals.Data.Azure.Storage.Interface;
    using Microsoft.CFS.Approvals.Extensions;
    using Microsoft.CFS.Approvals.LogManager.Model;
    using Microsoft.CFS.Approvals.LogManager.Provider.Interface;
    using Microsoft.CFS.Approvals.Model;
    using Microsoft.Extensions.Configuration;

    public class ReassignmentReceiver : IApprovalsQueueReceiver
    {
        #region Properties

        public ApprovalTenantInfo TenantInfo { get; set; }

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
        /// The secondary processor
        /// </summary>
        private readonly IReassignmentProcessor _reassignmentProcessor;

        /// <summary>
        /// The blob storage helper
        /// </summary>
        private readonly IBlobStorageHelper _blobStorageHelper;

        #endregion Properties

        #region Constructor

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="performanceLogger"></param>
        /// <param name="logProvider"></param>
        /// <param name="configuration"></param>
        /// <param name="blobStorageHelper"></param>
        /// <param name="reassignmentProcessor"></param>
        public ReassignmentReceiver
            (IPerformanceLogger performanceLogger,
            ILogProvider logProvider,
            IConfiguration configuration,
            IBlobStorageHelper blobStorageHelper,
            IReassignmentProcessor reassignmentProcessor)
        {
            _performanceLogger = performanceLogger;
            _logProvider = logProvider;
            _config = configuration;
            _blobStorageHelper = blobStorageHelper;
            _reassignmentProcessor = reassignmentProcessor;
        }

        #endregion Constructor

        /// <summary>
        ///
        /// </summary>
        /// <param name="message"></param>
        /// <param name="blobId"></param>
        /// <returns></returns>
        public async Task OnMainMessageReceived(ServiceBusReceivedMessage message, string blobId)
        {
            if (!string.IsNullOrWhiteSpace(blobId))
            {
                using (var secondaryMsgReceivedTracer = _performanceLogger.StartPerformanceLogger("PerfLog", "SecondaryWorker", string.Format(Constants.PerfLogAction, "Secondary", "Secondary processing time"), new Dictionary<LogDataKey, object>()))
                {
                    await Task.Run(() => ProcessMessageOnTaskAndAwait(blobId, message));
                }
            }
        }

        /// <summary>
        /// Processes the message on a separate thread and calls remaining workflow
        /// </summary>
        /// <param name="blobId"></param>
        /// <param name="serviceBusMessage"></param>
        /// <returns></returns>
        private async Task ProcessMessageOnTaskAndAwait(string blobId, ServiceBusReceivedMessage serviceBusMessage)
        {
            #region Logging

            var logData = new Dictionary<LogDataKey, object>
            {
                { LogDataKey.MessageId, blobId },
                { LogDataKey.LocalTime, DateTime.UtcNow }
            };

            #endregion Logging

            using (var perfTracer = _performanceLogger.StartPerformanceLogger("PerfLog", "ReassignmentWorker", string.Format(Constants.PerfLogAction, "Reassignment", "Processes the message on a separate thread and calls remaining workflow"), logData))
            {
                try
                {
                    logData.Add(LogDataKey.QueueName, _config[ConfigurationKey.QueueNameReassignment.ToString()]);

                    byte[] message;
                    message = await _blobStorageHelper.DownloadByteArray(Constants.reassingmentMessageContainer, blobId);

                    var numberOfRetries = int.Parse(_config[ConfigurationKey.MainTopicFailCountThreshold.ToString()]);
                    Stream stream = new MemoryStream(message);
                    var streamReader = new StreamReader(stream);
                    var messageBody = streamReader.ReadToEnd();

                    var approvalRequest = messageBody.FromJson<ApprovalRequestExpressionExt>();

                    #region Logging

                    logData.Add(LogDataKey.DocumentNumber, approvalRequest?.ApprovalIdentifier?.DocumentNumber);
                    logData.Add(LogDataKey.DisplayDocumentNumber, approvalRequest?.ApprovalIdentifier?.DisplayDocumentNumber);
                    logData.Add(LogDataKey.DetailStatus, approvalRequest.IsDetailsLoadSuccess);
                    logData.Add(LogDataKey.DXcv, approvalRequest?.ApprovalIdentifier?.DisplayDocumentNumber);
                    logData.Add(LogDataKey.EventType, Constants.FeatureUsageEvent);
                    logData.Add(LogDataKey.Xcv, approvalRequest?.Telemetry?.Xcv);
                    logData.Add(LogDataKey.Tcv, approvalRequest?.Telemetry?.Tcv);
                    logData.Add(LogDataKey.TenantTelemetryData, approvalRequest?.Telemetry?.TenantTelemetry);
                    logData.Add(LogDataKey.TenantId, approvalRequest?.DocumentTypeId);
                    logData.Add(LogDataKey.TenantName, TenantInfo?.AppName);
                    logData.Add(LogDataKey.Approver, approvalRequest?.Approvers?.ToJson());
                    _logProvider.LogInformation(TrackingEvent.ARXReceivedByReassignmentWorker, logData);

                    #endregion Logging

                    // Process the message if valid
                    await ProcessMainApproval(approvalRequest, blobId, numberOfRetries, serviceBusMessage);
                }
                catch (ServiceBusException ex)
                {
                    _logProvider.LogError(TrackingEvent.ARXFailedByReassignmentWorker, ex, logData);
                }
                catch (Exception ex)
                {
                    _logProvider.LogError(TrackingEvent.ARXFailedByReassignmentWorker, ex, logData);
                }
            }
        }

        /// <summary>
        /// Main method which hosts the logic for processing a Blob Message
        /// </summary>
        /// <param name="requestExpressions"></param>
        /// <param name="blobId"></param>
        /// <param name="numberOfRetries"></param>
        /// <param name="sbMessage">service bus message</param>
        private async Task ProcessMainApproval(ApprovalRequestExpressionExt requestExpressions, string blobId, int numberOfRetries, ServiceBusReceivedMessage sbMessage)
        {
            using (var perfTracer = _performanceLogger.StartPerformanceLogger("PerfLog", "ReassignmentWorker", string.Format(Constants.PerfLogAction, TenantInfo.AppName, "Processes received Brokered Message")
                   , new Dictionary<LogDataKey, object>())) // TODO: log message ID here.
            {
                if (numberOfRetries >= 0)
                {
                    // Process the Brokered Message and check if any errors occurred
                    var failedRequest = await _reassignmentProcessor.ProcessReassignmentDetails(requestExpressions, blobId, TenantInfo, sbMessage);
                    if (failedRequest != null)
                    {
                        await ProcessMainApproval(requestExpressions, blobId, numberOfRetries - 1, sbMessage);
                    }
                    else
                    {
                        if (await _blobStorageHelper.DoesExist(_config[ConfigurationKey.ReassignmentContainer.ToString()], blobId))
                        {
                            await _blobStorageHelper.DeleteBlob(_config[ConfigurationKey.ReassignmentContainer.ToString()], blobId);
                        }
                    }
                }
            }
        }
    }
}
