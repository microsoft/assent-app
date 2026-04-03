// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.
namespace Approvals.SecondaryProcessor.BL
{
    using System;
    using System.Collections.Generic;
    using System.Globalization;
    using System.Text;
    using System.Threading.Tasks;
    using Approvals.SecondaryProcessor.BL.Interface;
    using Azure.Messaging.ServiceBus;
    using Microsoft.CFS.Approvals.Common.DL.Interface;
    using Microsoft.CFS.Approvals.Contracts;
    using Microsoft.CFS.Approvals.Contracts.DataContracts;
    using Microsoft.CFS.Approvals.Core.BL.Interface;
    using Microsoft.CFS.Approvals.Data.Azure.Storage.Interface;
    using Microsoft.CFS.Approvals.Domain.BL.Interface;
    using Microsoft.CFS.Approvals.Extensions;
    using Microsoft.CFS.Approvals.LogManager.Model;
    using Microsoft.CFS.Approvals.LogManager.Provider.Interface;
    using Microsoft.CFS.Approvals.Messaging.Azure.ServiceBus.Interface;
    using Microsoft.CFS.Approvals.Model;
    using Microsoft.CFS.Approvals.Utilities.Extension;
    using Microsoft.Extensions.Configuration;
    using Newtonsoft.Json;
    using Attachment = Microsoft.CFS.Approvals.Contracts.DataContracts.Attachment;
    using Constants = Microsoft.CFS.Approvals.Contracts.Constants;
    using User = Microsoft.CFS.Approvals.Contracts.DataContracts.User;

    public class SecondaryProcessor : ISecondaryProcessor
    {
        #region Properties

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
        private readonly ITenantFactory _tenantFactory;

        /// <summary>
        /// The service bus helper
        /// </summary>
        private readonly IServiceBusHelper _serviceBusHelper;

        /// <summary>
        /// The approval summary provider
        /// </summary>
        private readonly IApprovalSummaryProvider _approvalSummaryProvider;

        /// <summary>
        /// The details helper
        /// </summary>
        private readonly IDetailsHelper _detailsHelper;

        /// <summary>
        /// The approval detail provider
        /// </summary>
        private readonly IApprovalDetailProvider _approvalDetailProvider;

        /// <summary>
        /// The openAI helper
        /// </summary>
        private readonly IIntelligenceHelper _openAIHelper;

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
        /// <param name="configuration"></param>
        /// <param name="logProvider"></param>
        /// <param name="tenantFactory"></param>
        /// <param name="serviceBusHelper"></param>
        /// <param name="approvalSummaryProvider"></param>
        /// <param name="detailsHelper"></param>
        /// <param name="approvalDetailProvider"></param>
        /// <param name="openAIHelper"></param>
        /// <param name="blobStorageHelper"></param>
        public SecondaryProcessor
            (IPerformanceLogger performanceLogger,
            IConfiguration configuration,
            ILogProvider logProvider,
            ITenantFactory tenantFactory,
            IServiceBusHelper serviceBusHelper,
            IApprovalSummaryProvider approvalSummaryProvider,
            IDetailsHelper detailsHelper,
            IApprovalDetailProvider approvalDetailProvider,
            IIntelligenceHelper openAIHelper,
            IBlobStorageHelper blobStorageHelper)
        {
            _performanceLogger = performanceLogger;
            _config = configuration;
            _logProvider = logProvider;
            _tenantFactory = tenantFactory;
            _serviceBusHelper = serviceBusHelper;
            _approvalSummaryProvider = approvalSummaryProvider;
            _detailsHelper = detailsHelper;
            _approvalDetailProvider = approvalDetailProvider;
            _openAIHelper = openAIHelper;
            _blobStorageHelper = blobStorageHelper;
        }

        #endregion Constructor

        /// <summary>
        /// Process secondary details
        /// </summary>
        /// <param name="approvalRequest"></param>
        /// <param name="blobId"></param>
        /// <param name="tenantInfo"></param>        
        /// <param name="message"></param>
        /// <returns></returns>
        public async Task<ApprovalRequestExpressionExt> ProcessSecondaryDetailsAsync(ApprovalRequestExpressionExt approvalRequest, string blobId, ApprovalTenantInfo tenantInfo,
            ServiceBusReceivedMessage message, bool isReassignmentFlow = false)
        {
            var logData = new Dictionary<LogDataKey, object>();
            logData.Add(LogDataKey.Xcv, approvalRequest?.Telemetry?.Xcv);
            logData.Add(LogDataKey.Tcv, approvalRequest?.Telemetry?.Tcv);
            logData.Add(LogDataKey.TenantTelemetryData, approvalRequest?.Telemetry?.TenantTelemetry);
            logData.Add(LogDataKey.EventType, Constants.FeatureUsageEvent);
            logData.Add(LogDataKey.MessageId, blobId);
            logData.Add(LogDataKey.DisplayDocumentNumber, approvalRequest?.ApprovalIdentifier?.DisplayDocumentNumber);
            logData.Add(LogDataKey.DocumentNumber, approvalRequest?.ApprovalIdentifier?.DocumentNumber);
            logData.Add(LogDataKey.TenantId, tenantInfo?.DocTypeId);
            logData.Add(LogDataKey.TenantName, tenantInfo?.AppName);

            using (var perfTracer = _performanceLogger.StartPerformanceLogger("PerfLog", "SecondaryWorker", string.Format(Constants.PerfLogAction, "Secondary", "Fetch and process OCR attachments"), logData))
            {
                try
                {
                    // Get the source of the message
                    string source = message.ApplicationProperties.ContainsKey("Source") ? message.ApplicationProperties["Source"].ToString() : Constants.ServiceBusSourcePrimary;

                    //lookup the destination for the message
                    ServiceBusDestination serviceBusParameters = JsonConvert.DeserializeObject<ServiceBusDestination>(_config[source]);

                    // Secondary processing is only for auto-reassignment as it is not part of primary processing. Secondary Processor finds out the backup approvers (default as an immediate manager
                    // or as per the payload receiving from the client), prepares the update payload
                    // and sends the updated payload to the auto-reassignment queue with a scheduled enqueue time based on the threshold defined in the tenant

                    // Check if the tenant has auto-reassignment enabled
                    isReassignmentFlow = tenantInfo?.AutoReassignment?.IsAutoReassignmentEnabled == true;

                    // Check for the autoreassignment, Prepare the update payload, and send it to the Auto-reassignment Service bus queue
                    if (isReassignmentFlow)
                    {
                        _logProvider.LogInformation(TrackingEvent.ReassignmentProcessingStart, logData);

                        List<ApprovalSummaryRow> summaryRows = new List<ApprovalSummaryRow>();

                        // Validate if the request is already actioned by the original approver
                        // No action needed if it is delete payload
                        // No action needed if there are few approvers as backup - this means it is already a reassigned request
                        if (approvalRequest.Operation != ApprovalRequestOperation.Delete &&
                             approvalRequest?.Approvers != null && approvalRequest.Approvers.Any() &&
                            (approvalRequest.Approvers.Where(a => a.IsBackupApprover) == null || approvalRequest.Approvers.Where(a => a.IsBackupApprover).Count() == 0))
                        {
                            foreach (var approver in approvalRequest.Approvers)
                            {
                                string approverAlias = approver.Alias;
                                string approverId = approver.Id.ToString();
                                string domain = approver.UserPrincipalName.GetDomainFromUPN();

                                ApprovalSummaryRow filteredSummaryRow = _approvalSummaryProvider.GetApprovalSummaryByDocumentNumberAndApprover
                                    (approvalRequest?.DocumentTypeId.ToString(),
                                     approvalRequest?.ApprovalIdentifier?.DisplayDocumentNumber,
                                     approverAlias,
                                     approverId,
                                     domain);

                                if (filteredSummaryRow != null)
                                    summaryRows.Add(filteredSummaryRow);
                            }

                            List<ApprovalSummaryRow> actionPendingSummaryRows = null;
                            int count = 0;
                            if (summaryRows != null && summaryRows.Count > 0)
                            {
                                count = summaryRows.Count;
                                actionPendingSummaryRows = summaryRows.Where(s => s.LobPending == false && s.IsOutOfSyncChallenged == false && (s.AutoReassignmentSequenceId == null || s.AutoReassignmentSequenceId == 0)).ToList();
                            }

                            // If none of the approvers has actioned the request, proceed with reassignment
                            if (actionPendingSummaryRows != null && actionPendingSummaryRows.Count > 0 && actionPendingSummaryRows.Count.Equals(count))
                            {
                                await BuildAndSendReassignmentPayloadAsync(approvalRequest, actionPendingSummaryRows, tenantInfo, _config[ConfigurationKey.QueueNameReassignment.ToString()], blobId, logData);
                            }

                            // Request is already actioned, no need to process further
                            else
                            {
                                logData[LogDataKey.EventType] = TrackingEvent.ReassignmentProcessingSkip;
                                _logProvider.LogInformation(TrackingEvent.ReassignmentProcessingSkip, logData);
                            }
                        }
                    }

                    if (source.Equals(Constants.ServiceBusSourcePrimary))
                    {
                        #region Logging
                        logData.Add(LogDataKey.DetailStatus, approvalRequest?.IsDetailsLoadSuccess);
                        logData.Add(LogDataKey.AttachmentDownloadStatus, approvalRequest?.IsDownloadAttachmentSuccess);
                        #endregion Logging

                        ITenant tenant = _tenantFactory.GetTenant(tenantInfo);
                        var approvalSummaryRows = _approvalSummaryProvider.GetDocumentSummaryByRowKey(approvalRequest.ApprovalIdentifier.ToAzureTableRowKey(tenantInfo));
                        bool ocrTriggered = false;

                        if (approvalRequest.IsDownloadAttachmentSuccess && approvalSummaryRows != null && approvalSummaryRows.Count > 0)
                        {
                            // Get the attachment details from SummaryJson.Attachments or Tenant details (DT1 or HDR or REC)
                            List<Attachment> attachments = await tenant.GetAttachmentDetails(approvalSummaryRows, approvalRequest.ApprovalIdentifier, approvalRequest.Telemetry);
                           logData.Add(LogDataKey.Attachments, attachments);
                            if (attachments != null && attachments.Count > 0)
                            {
                                List<OCRAttachmentData> ocrAttachmentDataList = new List<OCRAttachmentData>();
                                foreach (var attachment in attachments)
                                {
                                    if (string.IsNullOrEmpty(attachment.Url) && !string.IsNullOrEmpty(attachment.ID))
                                    {
                                        var attachmentOCRInfo = new OCRAttachmentData()
                                        {
                                            Name = attachment.Name,
                                            Id = attachment.ID,
                                            DocumentNumber = approvalRequest.ApprovalIdentifier.GetDocNumber(tenantInfo),
                                            TenantId = tenantInfo.RowKey,
                                            Url = String.Format(Microsoft.CFS.Approvals.Contracts.Constants.BlobUrl, _config[ConfigurationKey.StorageAccountName.ToString()], Constants.NotificationAttachmentsBlobName, tenantInfo.RowKey, approvalRequest.ApprovalIdentifier.DisplayDocumentNumber, attachment.ID.ToString())
                                        };
                                        ocrAttachmentDataList.Add(attachmentOCRInfo);
                                    }
                                }
                                if (ocrAttachmentDataList != null && ocrAttachmentDataList.Any())
                                {
                                    // Serialize and encode the list of OCR attachment data
                                    byte[] byteArray = Encoding.UTF8.GetBytes(System.Text.Json.JsonSerializer.Serialize(ocrAttachmentDataList));

                                    // Upload JSON data to blob storage
                                    await _blobStorageHelper.UploadByteArray(byteArray, _config[ConfigurationKey.OCRAttachmentsContainer.ToString()], blobId);

                                    // Send the blob ID to the service bus
                                    await _serviceBusHelper.SendMessageToSerivceBus(new ServiceBusMessage(blobId), serviceBusParameters.Destination);
                                    ocrTriggered = true;
                                }
                            }
                        }

                        if (!ocrTriggered)
                        {
                            string documentNumber = approvalRequest?.ApprovalIdentifier?.DocumentNumber ?? string.Empty;
                            string tenantId = tenantInfo?.TenantId.ToString(CultureInfo.InvariantCulture) ?? string.Empty;
                            var payload = new
                            {
                                documentNumber = documentNumber, 
                                tenantId = tenantId
                            };

                            ServiceBusDestination serviceBusParametersAuxiliary = JsonConvert.DeserializeObject<ServiceBusDestination>(_config[ConfigurationKey.SourceOCR.ToString()]);
                            await _serviceBusHelper.AddFilterAndSendMessage(payload, string.Empty, string.Empty, string.Empty, serviceBusParametersAuxiliary.Destination, serviceBusParametersAuxiliary.Filter);
                        }
                    }
                    else
                    {
                        dynamic messageBody = JsonConvert.DeserializeObject(System.Text.Encoding.UTF8.GetString(message.Body));
                        await _serviceBusHelper.AddFilterAndSendMessage(messageBody, string.Empty, string.Empty, string.Empty, serviceBusParameters?.Destination, serviceBusParameters?.Filter);
                    }
                }
                catch (Exception ex)
                {
                    logData.Add(LogDataKey.DXcv, approvalRequest?.ApprovalIdentifier?.DisplayDocumentNumber);
                    _logProvider.LogWarning(TrackingEvent.SecondaryProcessingFail, logData, ex);
                    return approvalRequest;
                }
                return null;
            }
        }
        /// <summary>
        /// Build and send reassignment payload
        /// </summary>
        /// <param name="approvalRequest"></param>
        /// <param name="tenantInfo"></param>
        /// <param name="reassignmentQueueName"></param>
        /// <param name="blobId"></param>
        /// <returns></returns>
        private async Task BuildAndSendReassignmentPayloadAsync(ApprovalRequestExpressionExt approvalRequest, List<ApprovalSummaryRow> summaryRows,
            ApprovalTenantInfo tenantInfo, string reassignmentQueueName, string blobId, Dictionary<LogDataKey, object> logData)
        {
            var uploadPayload = approvalRequest;
            uploadPayload.Operation = ApprovalRequestOperation.Update;
            uploadPayload.DeleteFor = approvalRequest.Approvers.Select(approver => approver.Alias).ToList();
            uploadPayload.RefreshDetails = true;

            // Start with the existing approvers
            var ExistingApprovers = new List<Approver>(approvalRequest.Approvers ?? new List<Approver>());
            var backupApprovers = await AddBackupApproversInApprovalRequest(approvalRequest, tenantInfo);

            if (backupApprovers != null && backupApprovers.Any())
            {
                // Merge backup into the existing approvers
                ExistingApprovers.AddRange(backupApprovers);
                ExistingApprovers = ExistingApprovers.DistinctBy(a => a.Alias).ToList();

                // Assign merged list back to payload 
                uploadPayload.Approvers = ExistingApprovers;

                // Action Details
                uploadPayload.ActionDetail = new ActionDetail
                {
                    Name = "System Reassign",
                    Comment = "Auto-reassigned to backup approver",
                    Date = DateTime.UtcNow,
                    ActionBy = new User
                    {
                        Alias = "System Reassign"
                    },
                    NewApprover = backupApprovers.FirstOrDefault() != null ? new User
                    {
                        Alias = backupApprovers.FirstOrDefault().Alias,
                        Name = backupApprovers.FirstOrDefault().Name
                    } : null
                };

                // TODO: RequestVersion is readonly property. Find a way to set it to unique guid 
                if (uploadPayload.SummaryData != null)
                {
                    var summaryData = new SummaryJson()
                    {
                        AdditionalData = uploadPayload.SummaryData.AdditionalData,
                        ApprovalActionsApplicable = uploadPayload.SummaryData.ApprovalActionsApplicable,
                        ApprovalHierarchy = uploadPayload.SummaryData.ApprovalHierarchy,
                        ApprovalIdentifier = uploadPayload.SummaryData.ApprovalIdentifier,
                        Attachments = uploadPayload.SummaryData.Attachments,
                        ApproverNotes = uploadPayload.SummaryData.ApproverNotes,
                        CompanyCode = uploadPayload.SummaryData.CompanyCode,
                        CustomAttribute = uploadPayload.SummaryData.CustomAttribute,
                        DetailPageURL = uploadPayload.SummaryData.DetailPageURL,
                        DocumentTypeId = uploadPayload.SummaryData.DocumentTypeId,
                        SubmittedDate = uploadPayload.SummaryData.SubmittedDate,
                        Submitter = uploadPayload.SummaryData.Submitter,
                        Title = uploadPayload.SummaryData.Title,
                        UnitOfMeasure = uploadPayload.SummaryData.UnitOfMeasure,
                        UnitValue = uploadPayload.SummaryData.UnitValue
                    };
                    uploadPayload.SummaryData = summaryData;
                }

                // Upload the updated payload to blob storage
                byte[] payloadBytes = Encoding.UTF8.GetBytes(System.Text.Json.JsonSerializer.Serialize(uploadPayload));
                await _blobStorageHelper.UploadByteArray(payloadBytes, _config[ConfigurationKey.ReassignmentContainer.ToString()], blobId);
                var sequenceNumber = await _serviceBusHelper.SendScheduledMessageToServiceBus(new ServiceBusMessage(blobId), DateTimeOffset.Now + TimeSpan.FromDays(tenantInfo.AutoReassignment.ThresholdInDays), reassignmentQueueName);

                foreach (var actionPendingRow in summaryRows)
                {
                    actionPendingRow.AutoReassignmentSequenceId = sequenceNumber;
                    actionPendingRow.AutoReassignmentBlobId = blobId;
                    actionPendingRow.BackupApprovers = backupApprovers.ToJson(new JsonSerializerSettings { NullValueHandling = NullValueHandling.Ignore });

                }
                await _approvalSummaryProvider.UpdateSummaryInBatchAsync(summaryRows, summaryRows.FirstOrDefault().DocumentNumber, summaryRows.FirstOrDefault().DocumentNumber, Guid.NewGuid().ToString(), tenantInfo, "System Reassign");
            }
            // Skip is no Backup approver found (in case of CEO or single manager)
            else
            {
                logData[LogDataKey.EventType] = TrackingEvent.ReassignmentProcessingSkip;
                _logProvider.LogInformation(TrackingEvent.ReassignmentProcessingSkip, logData);
            }
        }

        /// <summary>
        /// Fetching Backup approver 
        /// </summary>
        /// <param name="approvalRequest"></param>
        /// <param name="tenantInfo"></param>
        /// <returns></returns>
        private async Task<List<Approver>> AddBackupApproversInApprovalRequest(ApprovalRequestExpressionExt approvalRequest,
            ApprovalTenantInfo tenantInfo)
        {
            if (approvalRequest.BackupApprovers != null && approvalRequest.BackupApprovers.Any())
            {
                return approvalRequest.BackupApprovers;
            }
            else
            {
                List<Approver> approvers = new List<Approver>();
                foreach (var approver in approvalRequest.Approvers)
                {
                    if (approver != null && !approver.IsBackupApprover)
                    {
                        var activeManagers = await _detailsHelper.GetActiveManagersofUser(approver.Id);
                        int managerCount = activeManagers?.Count ?? 0;

                        //TODO :: Can we move this method in NameResolutionHelper?
                        if (activeManagers != null && activeManagers.Count > 0 && managerCount > tenantInfo.AutoReassignment?.NonEscalationManagerLevel)
                        {
                            // If there are multiple active managers, and the count is greater than NonEscalationManagerLevel,
                            // set BackupApprover to immediate manager (first in the list)
                            Approver tempApprover = new Approver()
                            {
                                Alias = activeManagers.FirstOrDefault().UserPrincipalName.GetAliasFromUPN(),
                                UserPrincipalName = activeManagers.FirstOrDefault().UserPrincipalName,
                                Id = activeManagers.FirstOrDefault().Id,
                                Name = activeManagers.FirstOrDefault().DisplayName,
                                IsBackupApprover = true
                            };
                            approvers.Add(tempApprover);
                        }
                    }
                }
                return approvers;
            }
        }
    }
}
