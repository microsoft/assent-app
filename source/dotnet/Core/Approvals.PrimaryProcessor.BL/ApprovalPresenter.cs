// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace Microsoft.CFS.Approvals.PrimaryProcessor.BL;

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using global::Azure.Data.Tables;
using global::Azure.Identity;
using global::Azure.Messaging.ServiceBus;
using Microsoft.Azure.ServiceBus;
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
using Microsoft.CFS.Approvals.Utilities.Interface;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json.Linq;

/// <summary>
/// The Approval Presenter class
/// </summary>
public class ApprovalPresenter : IApprovalPresenter
{
    #region Private Variables

    /// <summary>
    /// The configuration
    /// </summary>
    private readonly IConfiguration _config;

    /// <summary>
    /// The approval detail provider
    /// </summary>
    private readonly IApprovalDetailProvider _approvalDetailProvider = null;

    /// <summary>
    /// The approval history provider
    /// </summary>
    private readonly IApprovalHistoryProvider _historyProvider = null;

    /// <summary>
    /// The log provider
    /// </summary>
    private readonly ILogProvider _logger = null;

    /// <summary>
    /// The performance _logger
    /// </summary>
    private readonly IPerformanceLogger _performanceLogger = null;

    /// <summary>
    /// The approval summary provider
    /// </summary>
    private readonly IApprovalSummaryProvider _approvalSummaryProvider = null;

    /// <summary>
    /// The name resolution helper
    /// </summary>
    private readonly INameResolutionHelper _nameResolutionHelper = null;

    /// <summary>
    /// The table helper
    /// </summary>
    private readonly ITableHelper _tableHelper;

    /// <summary>
    /// The tenant factory
    /// </summary>
    private readonly ITenantFactory _tenantFactory = null;

    /// <summary>
    /// The blob storage helper
    /// </summary>
    private readonly IBlobStorageHelper _blobStorageHelper = null;

    /// <summary>
    /// The authentication helper
    /// </summary>
    private readonly IAuthenticationHelper _authenticationHelper = null;

    private readonly string _serviceBusNamespace;
    private readonly string _notificationTopic;

    #endregion Private Variables

    #region Public Properties

    public ApprovalTenantInfo TenantInfo { get; set; }

    #endregion Public Properties

    /// <summary>
    /// Constructor of ApprovalPresenter
    /// </summary>
    /// <param name="config"></param>
    /// <param name="approvalDetailProvider"></param>
    /// <param name="logger"></param>
    /// <param name="performanceLogger"></param>
    /// <param name="approvalSummaryProvider"></param>
    /// <param name="nameResolutionHelper"></param>
    /// <param name="tableHelper"></param>
    /// <param name="historyProvider"></param>
    /// <param name="tenantFactory"></param>
    /// <param name="blobStorageHelper"></param>
    /// <param name="authenticationHelper"></param>
    public ApprovalPresenter(
        IConfiguration config,
        IApprovalDetailProvider approvalDetailProvider,
        ILogProvider logger,
        IPerformanceLogger performanceLogger,
        IApprovalSummaryProvider approvalSummaryProvider,
        INameResolutionHelper nameResolutionHelper,
        ITableHelper tableHelper,
        IApprovalHistoryProvider historyProvider,
        ITenantFactory tenantFactory,
        IBlobStorageHelper blobStorageHelper,
        IAuthenticationHelper authenticationHelper)
    {
        _config = config;
        _approvalDetailProvider = approvalDetailProvider;
        _logger = logger;
        _performanceLogger = performanceLogger;
        _approvalSummaryProvider = approvalSummaryProvider;
        _nameResolutionHelper = nameResolutionHelper;
        _tableHelper = tableHelper;
        _historyProvider = historyProvider;
        _tenantFactory = tenantFactory;
        _blobStorageHelper = blobStorageHelper;
        _authenticationHelper = authenticationHelper;
        _serviceBusNamespace = _config[ConfigurationKey.ServiceBusNamespace.ToString()];
        _notificationTopic = _config[ConfigurationKey.TopicNameNotification.ToString()];
    }

    #region Implemented methods

    /// <summary>
    /// Process approval request expressions
    /// </summary>
    /// <param name="approvalRequests"></param>
    /// <param name="message"></param>
    /// <returns>List of approval request expressions</returns>
    public async Task<List<ApprovalRequestExpressionExt>> ProcessApprovalRequestExpressions(List<ApprovalRequestExpressionExt> approvalRequests, Message message)
    {
        using (var perfTracer = _performanceLogger.StartPerformanceLogger("PerfLog", Constants.WorkerRole, string.Format(Constants.PerfLogAction, TenantInfo.AppName, "Process list of ApprovalRequestExpressions ")
            , new Dictionary<LogDataKey, object> { { LogDataKey.MessageId, message.MessageId } }))
        {
            List<ApprovalRequestExpressionExt> failedRequests = new List<ApprovalRequestExpressionExt>();
            foreach (var approvalRequest in approvalRequests)
            {
                Dictionary<LogDataKey, object> logData = new Dictionary<LogDataKey, object>()
                {
                    { LogDataKey._CorrelationId, message.GetCorrelationId() }
                };
                try
                {
                    logData.Add(LogDataKey.BusinessProcessName, string.Format(TenantInfo.BusinessProcessName, Constants.BusinessProcessNameSendPayload, approvalRequest.Operation));
                    logData.Add(LogDataKey.Tcv, approvalRequest.Telemetry.Tcv);
                    logData.Add(LogDataKey.ReceivedTcv, approvalRequest.Telemetry.Tcv);
                    logData.Add(LogDataKey.TenantTelemetryData, approvalRequest.Telemetry.TenantTelemetry);
                    logData.Add(LogDataKey.Xcv, approvalRequest.Telemetry.Xcv);
                    logData.Add(LogDataKey.DXcv, approvalRequest.ApprovalIdentifier.DisplayDocumentNumber);
                    ApprovalRequestResult result = await ProcessApprovalRequest(approvalRequest, message);

                    // If there was an error in processing (creating, updating or Deleting) the approval request then throw
                    // an exception so that the message is not marked as complete
                    if (result != null && result.Result == ApprovalRequestResultType.Error)
                    {
                        if (result.Exception != null)
                        {
                            throw result.Exception;
                        }
                        else
                        {
                            throw new MicrosoftApprovalsException("No Summary Data Received");
                        }
                    }
                }
                catch (TenantDataNotFoundException noDataFoundException)
                {
                    _logger.LogError(TrackingEvent.SummaryNotFoundInTenantSystem, noDataFoundException, logData);
                }
                catch (Exception ex)
                {
                    _logger.LogError(TrackingEvent.ARXProcessingFail, ex, logData);
                    failedRequests.Add(approvalRequest);
                }
            }
            return failedRequests;
        }
    }

    /// <summary>
    /// Add approval details to azure table
    /// </summary>
    /// <param name="approvalRequest"></param>
    /// <param name="message"></param>
    /// <returns></returns>
    public async Task<bool> AddApprovalDetailsToAzureTable(ApprovalRequestDetails approvalRequest, Message message)
    {
        var logData = new Dictionary<LogDataKey, object>
        {
            { LogDataKey.TenantId, TenantInfo.TenantId},
            { LogDataKey.DocumentNumber, approvalRequest.ApprovalIdentifier.DisplayDocumentNumber},
            { LogDataKey.BusinessProcessName, string.Format(TenantInfo.BusinessProcessName, Constants.BusinessProcessNameAddDetails, Constants.BusinessProcessNameDetailsPrefetched)},
            { LogDataKey.Tcv, approvalRequest.Tcv},
            { LogDataKey.Xcv, approvalRequest.Xcv},
            { LogDataKey.ReceivedTcv, approvalRequest.Tcv},
            { LogDataKey.TenantTelemetryData, approvalRequest.TenantTelemetry},
            { LogDataKey.DXcv, approvalRequest.ApprovalIdentifier.DisplayDocumentNumber}
        };

        using (var detailRowsInsertTracer = _performanceLogger.StartPerformanceLogger("PerfLog", "SecondaryWorker", string.Format(Constants.PerfLogAction, TenantInfo.AppName, "Add approval details into azure storage"), logData))
        {
            List<ApprovalDetailsEntity> detailsExistsOrNot = new List<ApprovalDetailsEntity>();
            bool detailsLoadSuccess = false;

            try
            {
                detailsExistsOrNot = _approvalDetailProvider.GetAllApprovalsDetails(TenantInfo.TenantId, approvalRequest.ApprovalIdentifier.DisplayDocumentNumber);
                List<string> tenantOperationNames = new List<string>();
                if (TenantInfo.DetailOperations != null && TenantInfo.DetailOperations.DetailOpsList != null)
                {
                    tenantOperationNames = TenantInfo.DetailOperations.DetailOpsList.Select(o => o.operationtype).ToList();
                }

                detailsExistsOrNot = detailsExistsOrNot.Where(d => tenantOperationNames.Contains(d.RowKey)).ToList();
                logData.Add(LogDataKey.EntryUtcDateTime, approvalRequest.CreateDateTime);

                // Insert only if brokered message UTC time is later than storage timestamp.
                if (detailsExistsOrNot.Count == 0 || approvalRequest.CreateDateTime > detailsExistsOrNot.FirstOrDefault().Timestamp)
                {
                    ApprovalRequestExpressionExt approvalReqestExt = new ApprovalRequestExpressionExt
                    {
                        Operation = approvalRequest.Operation,
                        ApprovalIdentifier = approvalRequest.ApprovalIdentifier,
                        DetailsData = approvalRequest.DetailsData,
                        Telemetry = new ApprovalsTelemetry
                        {
                            BusinessProcessName = approvalRequest.BusinessProcessName,
                            Xcv = approvalRequest.Xcv,
                            Tcv = approvalRequest.Tcv,
                            TenantTelemetry = approvalRequest.TenantTelemetry
                        },
                        Approvers = new List<Approver>
                    {
                        new Approver
                        {
                            Alias = approvalRequest.DeviceNotificationInfo.Approver
                        }
                    }
                    };

                    detailsLoadSuccess = await AddDetailsData(message, approvalReqestExt, approvalRequest.ApprovalIdentifier, logData, approvalRequest.DetailsData, approvalRequest.Xcv, approvalRequest.Tcv, approvalRequest.BusinessProcessName, TenantInfo, approvalRequest.DeviceNotificationInfo.Approver);
                }
                return detailsLoadSuccess;
            }
            catch (Exception failedToGetDetailsFromAzureTable)
            {
                _logger.LogError(TrackingEvent.AzureStorageGetRequestDetailsFail, failedToGetDetailsFromAzureTable, logData);
                return false;
            }
        }
    }

    /// <summary>
    /// Download and store attachments
    /// </summary>
    /// <param name="approvalRequest"></param>
    /// <param name="activityId"></param>
    /// <param name="tenant"></param>
    /// <returns></returns>
    public async Task<bool> DownloadAndStoreAttachments(ApprovalRequestDetails approvalRequest, string activityId, ITenant tenant)
    {
        var logData = new Dictionary<LogDataKey, object>()
        {
            { LogDataKey.BusinessProcessName, string.Format(TenantInfo.BusinessProcessName, Constants.BusinessProcessNameGetDocuments, Constants.BusinessProcessNameDetailsPrefetched) },
            { LogDataKey.Tcv, approvalRequest.Tcv },
            { LogDataKey.ReceivedTcv, approvalRequest.Tcv },
            { LogDataKey.TenantTelemetryData, approvalRequest.TenantTelemetry },
            { LogDataKey.Xcv, approvalRequest.Xcv },
            { LogDataKey.DocumentNumber, approvalRequest.ApprovalIdentifier.DisplayDocumentNumber },
            { LogDataKey.DXcv, approvalRequest.ApprovalIdentifier.DisplayDocumentNumber }
        };
        try
        {
            ApprovalsTelemetry telemetry = new ApprovalsTelemetry()
            {
                Xcv = approvalRequest.Xcv,
                Tcv = approvalRequest.Tcv,
                BusinessProcessName = approvalRequest.BusinessProcessName,
                TenantTelemetry = approvalRequest.TenantTelemetry
            };

            // Get the attachment details from SummaryJson.Attachments or Tenant details (DT1 or HDR or REC)
            List<Attachment> attachments = await tenant.GetAttachmentDetails(approvalRequest.SummaryRows, approvalRequest.ApprovalIdentifier, telemetry);
            logData.Add(LogDataKey.Attachments, attachments);

            if (attachments != null && attachments.Count > 0)
            {
                foreach (var attachment in attachments)
                {
                    await tenant.DownloadDocumentUsingAttachmentIdAsync(approvalRequest.ApprovalIdentifier, attachment.ID.ToString(), telemetry);
                }
                // return true when the 'Attachments' is null or empty JArray
                _logger.LogInformation(TrackingEvent.DownloadAndStoreAttachmentsSuccessOrSkipped, logData);
            }
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(TrackingEvent.DownloadAndStoreAttachmentsFail, ex, logData);
            return false;
        }
    }

    /// <summary>
    /// Creates a new Brokered Message and pushes it to Notification Topic
    /// The messages which will reach this topic will be processed separately
    /// </summary>
    /// <param name="approvalRequest"></param>
    /// <param name="message"
    /// <param name="summaryRows"></param>
    /// <param name="notification"></param>
    /// <param name="detailsLoadSuccess"></param>
    /// <returns></returns>
    public async Task MoveMessageToNotificationTopic(ApprovalRequestExpressionExt approvalRequest, Message message, List<ApprovalSummaryRow> summaryRows, DeviceNotificationInfo notification = null, bool detailsLoadSuccess = false)
    {
        // TODO:: Handle cases to send multiple Device notifications in case of multiple approvers
        var logData = new Dictionary<LogDataKey, object>()
        {
            { LogDataKey._CorrelationId, message.GetCorrelationId() },
            { LogDataKey.ReceivedTcv, approvalRequest.Telemetry.Tcv },
            { LogDataKey.TenantTelemetryData, approvalRequest.Telemetry.TenantTelemetry },
            { LogDataKey.BusinessProcessName, string.Format(TenantInfo.BusinessProcessName, Constants.BusinessProcessNameSendPayload, approvalRequest.Operation.ToString()) },
            { LogDataKey.Tcv, approvalRequest.Telemetry.Tcv },
            { LogDataKey.Xcv, approvalRequest.Telemetry.Xcv },
            { LogDataKey.DocumentNumber, approvalRequest.ApprovalIdentifier.DisplayDocumentNumber },
            { LogDataKey.DXcv, approvalRequest.ApprovalIdentifier.DisplayDocumentNumber },
            { LogDataKey.DetailStatus, detailsLoadSuccess },
            { LogDataKey.TenantName, TenantInfo.AppName},
            { LogDataKey.Approver, notification?.Approver },
            { LogDataKey.NotificationTemplateKey, notification?.NotificationTemplateKey}
        };
        try
        {
            if (summaryRows == null || !summaryRows.Any())
            {
                throw new InvalidDataException("Invalid or No Summary Rows");
            }
            ApprovalSummaryRow summaryRow;
            if (approvalRequest.ActionDetail != null && approvalRequest.ActionDetail.ActionBy != null && !string.IsNullOrWhiteSpace(approvalRequest.ActionDetail.ActionBy.Alias))
            {
                summaryRow = summaryRows?.Where(a => a.Approver?.ToLower() == approvalRequest?.ActionDetail?.ActionBy?.Alias?.ToLower())?.FirstOrDefault();
                if (summaryRow == null)
                    summaryRow = summaryRows.FirstOrDefault();
            }
            else
                summaryRow = summaryRows.FirstOrDefault();

            // Get the notification object that needs to be sent to the user
            if (notification == null)
            {
                notification = approvalRequest.ToDeviceNotificationInfo(summaryRow, message.GetCorrelationId());
            }

            logData[LogDataKey.Approver] = notification?.Approver;
            logData[LogDataKey.NotificationTemplateKey] = notification?.NotificationTemplateKey;
            logData.Add(LogDataKey.UserAlias, summaryRow.Approver);

            int noOfRetries = 3;
            for (int i = 1; i <= noOfRetries; i++)
            {
                try
                {
                    logData[LogDataKey.Counter] = i;

                    Dictionary<string, string> additionalData = null;
                    if (approvalRequest.DetailsData != null && approvalRequest.DetailsData.ContainsKey(Constants.AdditionalDetails))
                    {
                        additionalData = approvalRequest.DetailsData[Constants.AdditionalDetails]?.ToJObject()[Constants.AdditionalData]?.ToString().FromJson<Dictionary<string, string>>();
                    }
                    // Create customized class having ApprovalIdentifier, ApprovalSummaryRow, DeviceNotificationInfo
                    ApprovalNotificationDetails notificationDetails = new ApprovalNotificationDetails()
                    {
                        ApprovalTenantInfo = TenantInfo,
                        ApprovalIdentifier = approvalRequest.ApprovalIdentifier,
                        CreateDateTime = DateTime.UtcNow,
                        DeviceNotificationInfo = notification,
                        SummaryRows = summaryRows,
                        AdditionalData = additionalData,
                        DetailsLoadSuccess = detailsLoadSuccess,
                        Xcv = approvalRequest.Telemetry.Xcv,
                        Tcv = approvalRequest.Telemetry.Tcv
                    };

                    if (string.IsNullOrEmpty(approvalRequest.Telemetry.BusinessProcessName))
                    {
                        approvalRequest.Telemetry.BusinessProcessName = TenantInfo.BusinessProcessName;
                    }

                    notificationDetails.BusinessProcessName = approvalRequest.Telemetry.BusinessProcessName;
                    notificationDetails.TenantTelemetry = approvalRequest.Telemetry.TenantTelemetry;

                    byte[] messageToUpload = ConvertToByteArray(notificationDetails);
                    string blobId = Encoding.UTF8.GetString(message.Body);

                    if (message.UserProperties.ContainsKey("ApprovalRequestVersion") && message.UserProperties["ApprovalRequestVersion"].ToString() == _config[ConfigurationKey.ApprovalRequestVersion.ToString()])
                    {
                        await _blobStorageHelper.DeleteBlob(Constants.NotificationMessageContainer, blobId);
                    }
                    else
                    {
                        blobId = string.Format("{0}|{1}|{2}", approvalRequest.DocumentTypeId, approvalRequest.ApprovalIdentifier.DisplayDocumentNumber, approvalRequest.Operation.ToString());
                    }

                    await _blobStorageHelper.UploadByteArray(messageToUpload, Constants.NotificationMessageContainer, blobId);

                    // Create a BrokeredMessage of the customized class,
                    // with ApplicationId property set to DocumentTypeId, and the same CorrelationID as the orginal BrokeredMessage
                    ServiceBusMessage newMessage = new ServiceBusMessage(blobId);
                    newMessage.ApplicationProperties["ApplicationId"] = message.UserProperties["ApplicationId"];
                    newMessage.ApplicationProperties["ApprovalNotificationDetails"] = true;
                    newMessage.ApplicationProperties["ApprovalNotificationRequestVersion"] = _config[ConfigurationKey.ApprovalRequestVersion.ToString()];
                    newMessage.CorrelationId = message.GetCorrelationId();

                    logData[LogDataKey.BrokerMessage] = new { MessageBody = blobId, newMessage.ApplicationProperties, newMessage.CorrelationId }.ToJson();

                    ServiceBusClient client = new ServiceBusClient(_serviceBusNamespace + ".servicebus.windows.net", new DefaultAzureCredential());
                    var messageSender = client.CreateSender(_notificationTopic);
                    await messageSender.SendMessageAsync(newMessage);

                    // If no error occurs and all are processed, set the flag to true
                    approvalRequest.IsNotificationSent = true;
                    break;
                }
                catch (Exception ex)
                {
                    _logger.LogError(TrackingEvent.MoveMessageToNotificationTopicFail, ex, logData);

                    // If no error occurs and all are processed, set the flag to true
                    approvalRequest.IsNotificationSent = false;
                }
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(TrackingEvent.MoveMessageToNotificationTopicFail, ex, logData);

            // If no error occurs and all are processed, set the flag to true
            approvalRequest.IsNotificationSent = false;
        }
    }

    #endregion Implemented methods

    #region Private methods

    /// <summary>
    /// Process Approval Request
    /// </summary>
    /// <param name="approvalRequest"></param>
    /// <param name="message"></param>
    /// <returns></returns>
    private async Task<ApprovalRequestResult> ProcessApprovalRequest(ApprovalRequestExpressionExt approvalRequest, Message message)
    {
        using (var perfTracer = _performanceLogger.StartPerformanceLogger("PerfLog", "Worker", string.Format(Constants.PerfLogAction, TenantInfo.AppName, "Process ApprovalRequestExpression based on operation asynchrounously.")
            , new Dictionary<LogDataKey, object> { { LogDataKey.MessageId, message.MessageId }, { LogDataKey.DocumentNumber, approvalRequest?.ApprovalIdentifier?.DocumentNumber } }))
        {
            return approvalRequest.Operation switch
            {
                ApprovalRequestOperation.Create => await ProcessCreateApprovalRequest(approvalRequest, message),
                ApprovalRequestOperation.Update => await ProcessUpdateApprovalRequest(approvalRequest, message),
                ApprovalRequestOperation.Delete => await ProcessDeleteApprovalRequest(message, approvalRequest, true, 2),
                _ => throw new InvalidOperationException("Operation type is not supported."),
            };
        }
    }

    /// <summary>
    /// Process create operation for approval request
    /// </summary>
    /// <param name="approvalRequest"></param>
    /// <param name="message"></param>
    /// <returns></returns>
    private async Task<ApprovalRequestResult> ProcessCreateApprovalRequest(ApprovalRequestExpressionExt approvalRequest, Message message)
    {
        using (var perfTracer = _performanceLogger.StartPerformanceLogger("PerfLog", Constants.WorkerRole, string.Format(Constants.PerfLogAction, TenantInfo.AppName, "Process request creation based on summary exists or not")
            , new Dictionary<LogDataKey, object> { { LogDataKey.MessageId, message.MessageId }, { LogDataKey.DisplayDocumentNumber, approvalRequest?.ApprovalIdentifier?.DocumentNumber } }))
        {
            ApprovalRequestResult result = null;
            var logData = new Dictionary<LogDataKey, object>()
            {
                { LogDataKey.BusinessProcessName, string.Format(TenantInfo.BusinessProcessName, Constants.BusinessProcessNameSendPayload, approvalRequest.Operation) },
                { LogDataKey.Tcv, approvalRequest.Telemetry.Tcv },
                { LogDataKey.ReceivedTcv, approvalRequest.Telemetry.Tcv },
                { LogDataKey.TenantTelemetryData, approvalRequest.Telemetry.TenantTelemetry },
                { LogDataKey.Xcv, approvalRequest.Telemetry.Xcv },
                { LogDataKey.DXcv, approvalRequest.ApprovalIdentifier.DisplayDocumentNumber },
                { LogDataKey._CorrelationId, message.GetCorrelationId() },
                { LogDataKey.MessageId, message.MessageId }
            };
            try
            {
                LogMessageProgress(new List<ApprovalRequestExpressionExt> { approvalRequest }, TrackingEvent.CreateProcessStarts, message, null, CriticalityLevel.Yes);
                result = await ProcessCreateApprovalRequestForComputeTypes(approvalRequest, message, logData);
                LogMessageProgress(new List<ApprovalRequestExpressionExt> { approvalRequest }, TrackingEvent.CreateProcessCompleted, message, null, CriticalityLevel.Yes, logData);
            }
            catch (TenantDataNotFoundException)
            {
                throw;
            }
            catch (Exception exception)
            {
                _logger.LogError(TrackingEvent.TopicMessageProcessCreate, exception, logData);

                result = new ApprovalRequestResult()
                {
                    ApprovalIdentifier = approvalRequest.ApprovalIdentifier,
                    Result = ApprovalRequestResultType.Error,
                    TimeStamp = DateTime.UtcNow,
                    Exception = exception,
                };
            }

            return result;
        }
    }

    /// <summary>
    /// Processes ARX to push part of the operation to Background Processing Topic
    /// </summary>
    /// <param name="message"></param>
    /// <param name="approvalRequest"></param>
    /// <param name="logData"></param>
    /// <returns></returns>
    private async Task<ApprovalRequestResult> ProcessCreateApprovalRequestForComputeTypes(ApprovalRequestExpressionExt approvalRequest, Message message, Dictionary<LogDataKey, object> logData)
    {
        using (var perfTracer = _performanceLogger.StartPerformanceLogger("PerfLog", Constants.WorkerRole, string.Format(Constants.PerfLogAction, TenantInfo.AppName, "Process request creation which contains summary"), logData))
        {
            ApprovalRequestResult result = null;

            ITenant tenant = _tenantFactory.GetTenant(TenantInfo);

            // Get summary from with in the ARX (/ Brokered Message)
            if (approvalRequest == null || approvalRequest.ApprovalIdentifier == null || approvalRequest.ApprovalIdentifier.DisplayDocumentNumber == null)
            {
                throw new InvalidDataException("Invalid ARX Data: " + approvalRequest.ToJson());
            }

            List<ApprovalSummaryRow> approvalSummaryRows = null;

            if (approvalRequest.SummaryData != null && TenantInfo.ProcessAttachedSummary)
            {
                approvalSummaryRows = await tenant.GetSummaryFromARX(approvalRequest, approvalRequest.SummaryData);
            }

            // Check if summary cannot be fetched, process the message on Main Topic, though this is very unlikely
            if (approvalSummaryRows == null || approvalSummaryRows.Count == 0)
            {
                approvalSummaryRows = await GetSummaryFromTenant(approvalRequest);
            }

            if (approvalSummaryRows == null || approvalSummaryRows.Count == 0)
            {
                throw new MicrosoftApprovalsException(_config[ConfigurationKey.Message_SummaryDataNotAvailable.ToString()]);
            }

            bool isSummaryInserted = true;

            #region Insert summary rows into storage

            List<Task<bool>> allTasks = new List<Task<bool>>();

            // Insert summary row(s) into storage
            if (approvalRequest.IsCreateOperationComplete == false)
            {
                using (var summaryRowInsertTracer = _performanceLogger.StartPerformanceLogger("PerfLog", "Worker", string.Format(Constants.PerfLogAction, TenantInfo.AppName, "Compute Channel: Total time to insert Summary Row into Azure Storage"), logData))
                {
                    isSummaryInserted = await _approvalSummaryProvider.AddApprovalSummary(TenantInfo, approvalRequest, approvalSummaryRows);

                    #region Save a copy of Summary Data in Details Table

                    ApprovalDetailsEntity approvalDetails = new ApprovalDetailsEntity()
                    {
                        PartitionKey = approvalRequest?.ApprovalIdentifier?.GetDocNumber(TenantInfo),
                        RowKey = Constants.SummaryOperationType,
                        JSONData = approvalSummaryRows.ToJson(),
                        TenantID = Int32.Parse(TenantInfo.RowKey)
                    };

                    ApprovalsTelemetry telemetry = new ApprovalsTelemetry()
                    {
                        Xcv = approvalSummaryRows.FirstOrDefault().Xcv,
                        Tcv = approvalSummaryRows.FirstOrDefault().Tcv,
                        BusinessProcessName = string.Format(TenantInfo.BusinessProcessName, Constants.BusinessProcessNameAddSummaryCopy, Constants.BusinessProcessNameDetailsPrefetched)
                    };

                    Task<bool> isSummaryDetailsInserted = Task.Run(() => _approvalDetailProvider.AddTransactionalAndHistoricalDataInApprovalsDetails(approvalDetails, TenantInfo, telemetry));
                    allTasks.Add(isSummaryDetailsInserted);

                    #endregion Save a copy of Summary Data in Details Table

                    approvalRequest.IsCreateOperationComplete = true;

                    LogMessageProgress(new List<ApprovalRequestExpressionExt> { approvalRequest }, TrackingEvent.SummaryInsertedInBackground, message, null, CriticalityLevel.Yes);
                }
            }

            #endregion Insert summary rows into storage

            // If summary was successfully processed, process next steps of sending notifications and pre-fetching details
            if (isSummaryInserted)
            {
                switch (TenantInfo.ProcessSecondaryAction)
                {
                    case (int)(ProcessSecondaryActions.ProcessSecondaryActionsOnMainChannel):

                        #region Pre-fetch details and add to Azure Table

                        Task<bool> isDetailsInserted = Task.Run(() => AddDetailsData(message, approvalRequest, approvalRequest.ApprovalIdentifier, logData, approvalRequest.DetailsData, approvalRequest.Telemetry.Xcv, approvalRequest.Telemetry.Tcv, approvalRequest.Telemetry.BusinessProcessName, TenantInfo, approvalRequest.Approvers[0].Alias));
                        allTasks.Add(isDetailsInserted);

                        #endregion Pre-fetch details and add to Azure Table

                        Task.WaitAll(allTasks.ToArray());

                        approvalRequest.IsDetailsLoadSuccess = allTasks[1].Result;

                        #region Prefetch and store attachments

                        if (approvalRequest.RefreshDetails)
                        {
                            // Check if Documents attached has already been downloaded successfully in the previous run for the same message, if yes skip this call.
                            if (approvalRequest.IsDetailsLoadSuccess && !approvalRequest.IsDownloadAttachmentSuccess)
                            {
                                var notification = approvalRequest.ToDeviceNotificationInfo(approvalSummaryRows.FirstOrDefault(), message.MessageId);
                                ApprovalRequestDetails ard = approvalRequest.ToApprovalRequestDetails(approvalSummaryRows, notification, TenantInfo);
                                approvalRequest.IsDownloadAttachmentSuccess = await DownloadAndStoreAttachments(ard, message.MessageId, tenant);
                            }
                        }
                        else
                        {
                            approvalRequest.IsDownloadAttachmentSuccess = true;
                        }

                        #endregion Prefetch and store attachments

                        #region Send notifications

                        if (!TenantInfo.IsDayZeroActivityRunning) //if day zero activity is not running
                        {
                            await MoveMessageToNotificationTopic(approvalRequest, message, approvalSummaryRows, null, approvalRequest.IsDetailsLoadSuccess);
                            LogMessageProgress(new List<ApprovalRequestExpressionExt> { approvalRequest }, TrackingEvent.BrokeredMessageMovedToNotificationTopic, message, null, CriticalityLevel.Yes);
                        }

                        #endregion Send notifications

                        break;

                    case (int)(ProcessSecondaryActions.ProcessSecondaryActionsNone):

                        #region Prefetch and store attachments

                        if (approvalRequest.RefreshDetails)
                        {
                            // Check if Documents attached has already been downloaded successfully in the previous run for the same message, if yes skip this call.
                            if (!approvalRequest.IsDownloadAttachmentSuccess)
                            {
                                ApprovalSummaryRow summaryRow = approvalSummaryRows.FirstOrDefault();
                                var notification = approvalRequest.ToDeviceNotificationInfo(summaryRow, message.MessageId);
                                ApprovalRequestDetails ard = approvalRequest.ToApprovalRequestDetails(approvalSummaryRows, notification, TenantInfo);

                                approvalRequest.IsDownloadAttachmentSuccess = await DownloadAndStoreAttachments(ard, message.MessageId, tenant);
                            }
                        }
                        else
                        {
                            approvalRequest.IsDownloadAttachmentSuccess = true;
                        }

                        #endregion Prefetch and store attachments

                        #region Send notifications

                        if (!TenantInfo.IsDayZeroActivityRunning) //if day zero activity is not running
                        {
                            await MoveMessageToNotificationTopic(approvalRequest, message, approvalSummaryRows);
                            LogMessageProgress(new List<ApprovalRequestExpressionExt> { approvalRequest }, TrackingEvent.BrokeredMessageMovedToNotificationTopic, message, null, CriticalityLevel.Yes);
                        }

                        #endregion Send notifications

                        break;
                }
            }
            result = new ApprovalRequestResult()
            {
                ApprovalIdentifier = approvalRequest.ApprovalIdentifier,
                Result = ApprovalRequestResultType.Success,
                TimeStamp = DateTime.UtcNow,
            };

            return result;
        }
    }

    /// <summary>
    /// Process update operation for approval request
    /// </summary>
    /// <param name="approvalRequest"></param>
    /// <param name="message"></param>
    /// <returns></returns>
    private async Task<ApprovalRequestResult> ProcessUpdateApprovalRequest(ApprovalRequestExpressionExt approvalRequest, Message message)
    {
        try
        {
            LogMessageProgress(new List<ApprovalRequestExpressionExt> { approvalRequest }, TrackingEvent.ARXUpdateProcessStarts, message, null, CriticalityLevel.Yes);
            var result = await ProcessDeleteApprovalRequest(message, approvalRequest, false, 1);

            if (result.Result == ApprovalRequestResultType.Success)
            {
                result = await ProcessCreateApprovalRequest(approvalRequest, message);

                if (result.Result == ApprovalRequestResultType.Success)
                {
                    LogMessageProgress(new List<ApprovalRequestExpressionExt> { approvalRequest }, TrackingEvent.ARXUpdateProcessCompletes, message, null, CriticalityLevel.Yes);

                    result = new ApprovalRequestResult()
                    {
                        ApprovalIdentifier = approvalRequest.ApprovalIdentifier,
                        Result = ApprovalRequestResultType.Success,
                        TimeStamp = DateTime.UtcNow,
                    };

                    return result;
                }
                if (result.Result == ApprovalRequestResultType.Error)
                {
                    // We do not want to modify the result in case there was an error returned by the
                    // ProcessCreateApprovalRequest function. We make decidion based on the exception type

                    return result;
                }
            }
            else if (result.Result == ApprovalRequestResultType.Error)
            {
                return result;
            }
        }
        catch (Exception exception)
        {
            var result = new ApprovalRequestResult()
            {
                ApprovalIdentifier = approvalRequest.ApprovalIdentifier,
                Result = ApprovalRequestResultType.Error,
                TimeStamp = DateTime.UtcNow,
                Exception = exception,
            };

            return result;
        }

        var result2 = new ApprovalRequestResult()
        {
            ApprovalIdentifier = approvalRequest.ApprovalIdentifier,
            Result = ApprovalRequestResultType.Unknown,
            TimeStamp = DateTime.UtcNow,
        };

        return result2;
    }

    /// <summary>
    /// Process delete operation of approval request
    /// </summary>
    /// <param name="message"></param>
    /// <param name="approvalRequest"></param>
    /// <param name="isNotificationRequired"></param>
    /// <param name="numberOfRetry"></param>
    /// <returns></returns>
    private async Task<ApprovalRequestResult> ProcessDeleteApprovalRequest(Message message, ApprovalRequestExpressionExt approvalRequest, Boolean isNotificationRequired, int numberOfRetry = 0)
    {
        using (var perfTracer = _performanceLogger.StartPerformanceLogger("PerfLog", Constants.WorkerRole, string.Format(Constants.PerfLogAction, TenantInfo.AppName, "DeleteNotificationProcessing")
            , new Dictionary<LogDataKey, object> { { LogDataKey.MessageId, message.MessageId }, { LogDataKey.DocumentNumber, (approvalRequest == null || approvalRequest.ApprovalIdentifier == null || approvalRequest.ApprovalIdentifier.DocumentNumber == null) ? string.Empty : approvalRequest.ApprovalIdentifier.DocumentNumber } }))
        {
            var logData = new Dictionary<LogDataKey, object>()
            {
                { LogDataKey._CorrelationId, message.GetCorrelationId() },
                { LogDataKey.BusinessProcessName, string.Format(TenantInfo.BusinessProcessName, Constants.BusinessProcessNameSendPayload, approvalRequest.Operation) },
                { LogDataKey.DXcv, approvalRequest.ApprovalIdentifier.DisplayDocumentNumber },
                { LogDataKey.Tcv, approvalRequest.Telemetry.Tcv },
                { LogDataKey.Xcv, approvalRequest.Telemetry.Xcv },
                { LogDataKey.ReceivedTcv, approvalRequest.Telemetry.Tcv },
                { LogDataKey.TenantTelemetryData, approvalRequest.Telemetry.TenantTelemetry }
            };
            try
            {
                LogMessageProgress(new List<ApprovalRequestExpressionExt> { approvalRequest }, TrackingEvent.ARXDeleteProcessStarts, message, null, CriticalityLevel.Yes);

                ITenant tenant = _tenantFactory.GetTenant(TenantInfo);

                if (approvalRequest.IsDeleteOperationComplete
                    && (approvalRequest.IsHistoryLogged)
                    && ((isNotificationRequired && approvalRequest.IsNotificationSent) || (false == isNotificationRequired)))
                {
                    return new ApprovalRequestResult()
                    {
                        ApprovalIdentifier = approvalRequest.ApprovalIdentifier,
                        Result = ApprovalRequestResultType.Success,
                        TimeStamp = DateTime.UtcNow,
                    };
                }

                AzureTableRowDeletionResult azuretablerowdeletionresult = AzureTableRowDeletionResult.SkippedDueToNonExistence;

                string rowKeyValue = approvalRequest.ApprovalIdentifier.ToAzureTableRowKey(TenantInfo);

                IEnumerable<ApprovalSummaryRow> summaryRows = await GetSummaryRowsForDeletion(approvalRequest, rowKeyValue);

                List<DeviceNotificationInfo> notifications = new List<DeviceNotificationInfo>();
                TransactionHistory historyData = null;

                if (summaryRows != null && summaryRows.Any() && approvalRequest.DeleteFor != null && approvalRequest.DeleteFor.Any())
                {
                    summaryRows = summaryRows.Where(x => approvalRequest.DeleteFor.Contains(x.PartitionKey)).ToList();
                }
                if (summaryRows == null || !summaryRows.Any() && numberOfRetry > 0 && TenantInfo.IsRaceConditionHandled)
                {
                    var logDataRaceCondition = new Dictionary<LogDataKey, object>()
                    {
                        {LogDataKey._CorrelationId, message.GetCorrelationId() },
                        {LogDataKey.RaceConditionSleepTime, TenantInfo.RaceConditionSleepTimeInSecond },
                        { LogDataKey.Counter, numberOfRetry }
                    };
                    _logger.LogInformation(TrackingEvent.DeleteProcessForRaceCondition, logDataRaceCondition);
                    Thread.Sleep(TenantInfo.RaceConditionSleepTimeInSecond * 1000);
                    numberOfRetry--;
                    return await ProcessDeleteApprovalRequest(message, approvalRequest, isNotificationRequired, numberOfRetry);
                }

                List<ApprovalSummaryRow> summaryRowsFromTenant = new List<ApprovalSummaryRow>();

                if (summaryRows == null || !summaryRows.Any())
                {
                    #region Get Summary from Tenant in case of Tacit Approval

                    if (approvalRequest.IsHistoryLogged == false)
                    {
                        if (approvalRequest.SummaryData != null && TenantInfo.ProcessAttachedSummary)
                        {
                            summaryRowsFromTenant = await tenant.GetSummaryFromARX(approvalRequest, approvalRequest.SummaryData);
                        }
                        else
                        {
                            summaryRowsFromTenant = await GetSummaryFromTenant(approvalRequest);
                        }
                    }

                    #endregion Get Summary from Tenant in case of Tacit Approval
                }
                else
                {
                    summaryRowsFromTenant = summaryRows.ToList();
                    var additionalDetailsEntity = _approvalDetailProvider.GetApprovalsDetails(TenantInfo.TenantId,
                                                    approvalRequest.ApprovalIdentifier.GetDocNumber(TenantInfo),
                                                    Constants.AdditionalDetails);

                    JObject additionalDataFromSummary = null;
                    if (summaryRowsFromTenant.FirstOrDefault().SummaryJson.FromJson<SummaryJson>()?.AdditionalData != null)
                    {
                        var additionalData = summaryRowsFromTenant.FirstOrDefault().SummaryJson.FromJson<SummaryJson>()?.AdditionalData;
                        additionalDataFromSummary = JObject.FromObject(new { AdditionalData = additionalData.ToJToken() });
                    }

                    var additionalDetailsFromDetails = additionalDetailsEntity != null ? additionalDetailsEntity.JSONData
                        : additionalDataFromSummary?.ToJson();

                    tenant.AddAdditionalDataToDetailsData(approvalRequest, null, additionalDetailsFromDetails);
                }

                if (summaryRowsFromTenant != null && summaryRowsFromTenant.Any())
                {
                    foreach (var summaryRow in summaryRowsFromTenant)
                    {
                        notifications.Add(approvalRequest.ToDeviceNotificationInfo(summaryRow, message.GetCorrelationId()));
                    }

                    historyData = TransactionHistoryExtended.CreateHistoryData(summaryRowsFromTenant.FirstOrDefault(), approvalRequest, TenantInfo, message.MessageId);
                }

                bool? isHistoryInserted = null;
                if (!approvalRequest.IsDeleteOperationComplete)
                {
                    azuretablerowdeletionresult = await _approvalSummaryProvider.RemoveApprovalSummary(approvalRequest, summaryRows.ToList(), message, TenantInfo);
                    LogMessageProgress(new List<ApprovalRequestExpressionExt> { approvalRequest }, TrackingEvent.SummaryDeletedInBackground, message, null, CriticalityLevel.Yes);

                    #region Delete Details from Azure Table

                    if (approvalRequest.RefreshDetails || approvalRequest.Operation == ApprovalRequestOperation.Delete)
                    {
                        if (azuretablerowdeletionresult != AzureTableRowDeletionResult.SkippedDueToRaceCondition)
                        {
                            // remove detail row from here
                            await RemoveApprovalDetailsFromAzureTable(approvalRequest, azuretablerowdeletionresult, summaryRowsFromTenant);
                            LogMessageProgress(new List<ApprovalRequestExpressionExt> { approvalRequest }, TrackingEvent.DetailsDeletedInBackground, message, null, CriticalityLevel.Yes);
                        }
                        else
                            LogMessageProgress(new List<ApprovalRequestExpressionExt> { approvalRequest }, TrackingEvent.ApprovalDetailRemovalSkipped, message, null, CriticalityLevel.Yes);
                    }

                    #endregion Delete Details from Azure Table

                    #region Create ApprovalHierarchy, encapsulate in ApprovalDetailsEntity object and save in ApprovalsDetails table

                    //Added this to prevent issues while creating Approval Chain in a scenario wherein duplicate update payloads are sent

                    string actionDate = (historyData.ActionDate.Value.ToUniversalTime()).ToString("yyyy-MM-dd HH:mm:ss");
                    isHistoryInserted = await _historyProvider.CheckIfHistoryInsertedAsync(TenantInfo, historyData.Approver, actionDate, historyData.DocumentNumber, historyData.ActionTaken);
                    if (isHistoryInserted.HasValue && isHistoryInserted.Value)
                    {
                        await GenerateAndSaveApprovalChain(approvalRequest, summaryRows, historyData);
                    }

                    #endregion Create ApprovalHierarchy, encapsulate in ApprovalDetailsEntity object and save in ApprovalsDetails table
                }

                switch (azuretablerowdeletionresult)
                {
                    case AzureTableRowDeletionResult.DeletionSuccessful:
                    case AzureTableRowDeletionResult.SkippedDueToNonExistence:
                    case AzureTableRowDeletionResult.SkippedDueToRaceCondition:
                        if (historyData != null && approvalRequest.IsHistoryLogged == false)
                        {
                            string actionDate = (historyData.ActionDate.Value.ToUniversalTime()).ToString("yyyy-MM-dd HH:mm:ss");
                            if (isHistoryInserted ?? await _historyProvider.CheckIfHistoryInsertedAsync(TenantInfo, historyData.Approver, actionDate, historyData.DocumentNumber, historyData.ActionTaken))
                            {
                                await _historyProvider.AddApprovalHistoryAsync(TenantInfo, historyData);
                                approvalRequest.IsHistoryLogged = true;
                            }
                        }
                        if (!TenantInfo.IsDayZeroActivityRunning && isNotificationRequired && !approvalRequest.IsNotificationSent)
                        {
                            if (TenantInfo.NotifyEmail)
                            {
                                DeviceNotificationInfo notification;
                                if (approvalRequest.ActionDetail != null && approvalRequest.ActionDetail.ActionBy != null && !string.IsNullOrWhiteSpace(approvalRequest.ActionDetail.ActionBy.Alias))
                                {
                                    notification = notifications.Where(a => a.Approver.Equals(approvalRequest.ActionDetail.ActionBy.Alias)).FirstOrDefault();
                                    if (notification == null)
                                        notification = notifications.FirstOrDefault();
                                }
                                else
                                {
                                    notification = notifications.FirstOrDefault();
                                }
                                await MoveMessageToNotificationTopic(approvalRequest, message, summaryRowsFromTenant, notification);
                                LogMessageProgress(new List<ApprovalRequestExpressionExt> { approvalRequest }, TrackingEvent.BrokeredMessageMovedToNotificationTopic, message, null, CriticalityLevel.Yes);
                            }
                            approvalRequest.IsNotificationSent = true;
                        }
                        LogMessageProgress(new List<ApprovalRequestExpressionExt> { approvalRequest }, TrackingEvent.ARXDeleteProcessCompletes, message, null, CriticalityLevel.Yes);
                        return new ApprovalRequestResult()
                        {
                            ApprovalIdentifier = approvalRequest.ApprovalIdentifier,
                            Result = ApprovalRequestResultType.Success,
                            TimeStamp = DateTime.UtcNow,
                        };

                    case AzureTableRowDeletionResult.DeletionFailed:
                        _logger.LogInformation(TrackingEvent.DeleteProcessFailed, logData);
                        return new ApprovalRequestResult()
                        {
                            ApprovalIdentifier = approvalRequest.ApprovalIdentifier,
                            Result = ApprovalRequestResultType.Error,
                            TimeStamp = DateTime.UtcNow,
                            Exception = null,
                        };
                }
            }
            catch (Exception exception)
            {
                return ProcessAzureTableRowDeletionFailure(approvalRequest, exception);
            }
        }
        return ProcessAzureTableRowDeletionFailure(approvalRequest, null);
    }

    /// <summary>
    /// This method Creates the Approver Chain and stores that as an entry in the ApprovalDetails table
    /// </summary>
    /// <param name="approvalRequest"></param>
    /// <param name="summaryRows"></param>
    /// <param name="historyData"></param>
    private async Task GenerateAndSaveApprovalChain(ApprovalRequestExpressionExt approvalRequest, IEnumerable<ApprovalSummaryRow> summaryRows, TransactionHistory historyData)
    {
        // This needs to be done only in case of UPDATE operations.
        // For DELETE the workflow ends and thus, we needn't store any further details in Approval Details table
        if (approvalRequest.Operation.Equals(ApprovalRequestOperation.Update))
        {
            // Get Existing ApproverChain if any
            List<ApproverChainEntity> existingApproverChainEntity;
            var approverChainRow = _approvalDetailProvider.GetApprovalsDetails(Int32.Parse(TenantInfo.RowKey), approvalRequest.ApprovalIdentifier.DisplayDocumentNumber, Constants.ApprovalChainOperation);
            if (approverChainRow != null)
            {
                existingApproverChainEntity = approverChainRow.JSONData.FromJson<List<ApproverChainEntity>>();
            }
            else
            {
                approverChainRow = new ApprovalDetailsEntity()
                {
                    PartitionKey = approvalRequest?.ApprovalIdentifier?.GetDocNumber(TenantInfo),
                    RowKey = Constants.ApprovalChainOperation,
                    TenantID = Int32.Parse(TenantInfo.RowKey)
                };
                existingApproverChainEntity = new List<ApproverChainEntity>();
            }

            // Create the current ApproverChain
            ApproverChainEntity currentApproverChain = new ApproverChainEntity
            {
                Action = historyData.ActionTaken,
                ActionDate = historyData.ActionDate,
                Alias = historyData.Approver,
                Name = await _nameResolutionHelper.GetUserName(historyData.Approver),
                Type = TenantInfo.IsOldHierarchyEnabled ? //TODO remove this old hierachy Specific code
                       GetApprovalType((historyData.JsonData).ToJObject(), historyData.Approver) :
                       GetApprovalType((historyData.JsonData).ToJObject(), historyData.Approver, existingApproverChainEntity.Count),
                DelegateUser = historyData.DelegateUser,
                _future = false,
                Justification = Extension.ExtractValueFromJSON((historyData.ApproversNote).ToJObject(), "JustificationText"),
                Notes = Extension.ExtractValueFromJSON((historyData.ApproversNote).ToJObject(), "Comment")
            };

            // Add to the exisitng ApproverChain list
            existingApproverChainEntity.Add(currentApproverChain);

            // Encapsulate ApprovalDetailsEntity object
            approverChainRow.JSONData = existingApproverChainEntity.ToJson();

            // Save in ApprovalDetails table
            ApprovalsTelemetry telemetry = new ApprovalsTelemetry()
            {
                Xcv = summaryRows.FirstOrDefault().Xcv,
                Tcv = summaryRows.FirstOrDefault().Tcv,
                BusinessProcessName = string.Format(TenantInfo.BusinessProcessName, Constants.BusinessProcessNameAddSummaryCopy, Constants.BusinessProcessNameDetailsPrefetched)
            };

            _approvalDetailProvider.AddTransactionalAndHistoricalDataInApprovalsDetails(approverChainRow, TenantInfo, telemetry);
        }
    }

    /// <summary>
    /// Finds out the Approval Type from the ApprovalHierarchy or the AdditionalData property in History
    /// </summary>
    /// <param name="historyJson"></param>
    /// <param name="approverAlias"></param>
    /// <returns></returns>
    private string GetApprovalType(JObject historyJson, string approverAlias)
    {
        string approverType = string.Empty;
        var approvalHierarchy = Extension.ExtractValueFromJSON(historyJson, "ApprovalHierarchy").FromJson<List<ApprovalHierarchy>>();
        if (approvalHierarchy != null)
        {
            foreach (var approver in approvalHierarchy.Where(approver => approver.Approvers != null && approver.Approvers.FirstOrDefault(x => x.Alias == approverAlias) != null))
            {
                approverType = approver.ApproverType;
                break;
            }
        }
        else
        {
            approverType = Extension.ExtractValueFromJSON(historyJson, "AdditionalData.ApproverType");
        }
        return approverType;
    }

    /// <summary>
    /// Finds out the Approval Type from the ApprovalHierarchy or the AdditionalData property in History
    /// </summary>
    /// <param name="historyJson"></param>
    /// <param name="approverAlias"></param>
    /// <param name="historyCount"></param>
    /// <returns></returns>
    private string GetApprovalType(JObject historyJson, string approverAlias, int historyCount)
    {
        string approverType = string.Empty;
        var approvalHierarchy = Extension.ExtractValueFromJSON(historyJson, "ApprovalHierarchy").FromJson<List<ApprovalHierarchy>>();
        if (approvalHierarchy != null)
        {
            var approverHierarchies = approvalHierarchy?.Where(approver => approver?.Approvers != null && approver?.Approvers?.FirstOrDefault(x => x?.Alias == approverAlias) != null);
            if (approverHierarchies?.Count() > 1)
            {
                var hierarchy = approvalHierarchy?.Count > historyCount ? approvalHierarchy[historyCount] : null;
                var approver = hierarchy?.Approvers?.FirstOrDefault(x => x.Alias == approverAlias);
                if (approver != null)
                    approverType = hierarchy?.ApproverType;
            }
            else
            {
                approverType = approverHierarchies?.FirstOrDefault()?.ApproverType;
            }
        }
        else
        {
            approverType = Extension.ExtractValueFromJSON(historyJson, "AdditionalData.ApproverType");
        }
        return approverType ?? string.Empty;
    }

    /// <summary>
    /// Process azure table row deletion failure
    /// </summary>
    /// <param name="approvalRequest"></param>
    /// <param name="exception"></param>
    /// <returns></returns>
    private ApprovalRequestResult ProcessAzureTableRowDeletionFailure(ApprovalRequestExpression approvalRequest, Exception exception)
    {
        var result = new ApprovalRequestResult()
        {
            ApprovalIdentifier = approvalRequest.ApprovalIdentifier,
            Result = ApprovalRequestResultType.Error,
            TimeStamp = DateTime.UtcNow,
            Exception = exception,
        };

        return result;
    }

    /// <summary>
    /// Convert to byte array
    /// </summary>
    /// <param name="ard"></param>
    /// <returns></returns>
    private byte[] ConvertToByteArray(object ard)
    {
        return JsonSerializer.SerializeToUtf8Bytes(ard);
    }

    /// <summary>
    /// Get summary from Tenant
    /// </summary>
    /// <param name="approvalRequest"></param>
    /// <returns></returns>
    private async Task<List<ApprovalSummaryRow>> GetSummaryFromTenant(ApprovalRequestExpression approvalRequest)
    {
        if (approvalRequest == null || approvalRequest.ApprovalIdentifier == null || approvalRequest.ApprovalIdentifier.DisplayDocumentNumber == null)
        {
            throw new InvalidDataException("Invalid ARX Data: " + approvalRequest.ToJson());
        }
        using (var perfTracer = _performanceLogger.StartPerformanceLogger("PerfLog", "Worker", string.Format(Constants.PerfLogAction, TenantInfo.AppName, "Gets summary from tenant")
            , new Dictionary<LogDataKey, object> { { LogDataKey.DocumentNumber, approvalRequest?.ApprovalIdentifier?.DocumentNumber } }))
        {
            return await GetApprovalSummaryJsonFromLOB(TenantInfo, approvalRequest);
        }
    }

    /// <summary>
    /// Gets the summary data from the data service of the LOB's
    /// </summary>
    /// <param name="_tenantInfo"></param>
    /// <param name="approvalRequest"></param>
    /// <returns></returns>
    private async Task<List<ApprovalSummaryRow>> GetApprovalSummaryJsonFromLOB(ApprovalTenantInfo _tenantInfo, ApprovalRequestExpression approvalRequest)
    {
        using (var perfTracer = _performanceLogger.StartPerformanceLogger("PerfLog", "Worker", string.Format(Constants.PerfLogAction, _tenantInfo.AppName, "Get approval summary Json from tenant")
                  , new Dictionary<LogDataKey, object> { { LogDataKey.DocumentNumber, approvalRequest?.ApprovalIdentifier?.DocumentNumber } }))
        {
            ITenant tenant = _tenantFactory.GetTenant(_tenantInfo);
            return await tenant.GetSummaryFromTenantAsync(approvalRequest, Environment.UserName, approvalRequest.Telemetry);
        }
    }

    /// <summary>
    /// Add details data
    /// </summary>
    /// <param name="message"></param>
    /// <param name="approvalRequest"></param>
    /// <param name="approvalIdentifier"></param>
    /// <param name="logData"></param>
    /// <param name="detailsData"></param>
    /// <param name="Xcv"></param>
    /// <param name="Tcv"></param>
    /// <param name="BusinessProcessName"></param>
    /// <param name="approvalTenantInfo"></param>
    /// <param name="approver"></param>
    /// <returns></returns>
    private async Task<bool> AddDetailsData(Message message, ApprovalRequestExpressionExt approvalRequest, ApprovalIdentifier approvalIdentifier, Dictionary<LogDataKey, object> logData, Dictionary<string, string> detailsData, string Xcv, string Tcv, string BusinessProcessName, ApprovalTenantInfo approvalTenantInfo, string approver)
    {
        // Fix issue if any tenant send us RefreshDetail = false for create. For create, we need to ignore this flag or RefreshDetail has to set to true. Otherwise, worker role will not populate detail correctly.
        if (approvalRequest.Operation != ApprovalRequestOperation.Create && !approvalRequest.RefreshDetails)
        {
            return true;
        }

        if (!logData.ContainsKey(LogDataKey.TenantId))
        {
            logData.Add(LogDataKey.TenantId, TenantInfo.TenantId);
        }

        if (!logData.ContainsKey(LogDataKey.DocumentNumber))
        {
            logData.Add(LogDataKey.DocumentNumber, approvalIdentifier.DocumentNumber);
        }

        if (!logData.ContainsKey(LogDataKey.DXcv))
        {
            logData.Add(LogDataKey.DXcv, approvalIdentifier.DocumentNumber);
        }

        int page = 1;
        HttpContent responseContent = null;
        List<ApprovalDetailsEntity> detailsRows = new List<ApprovalDetailsEntity>();

        JObject serviceParameterObject = (approvalTenantInfo.ServiceParameter).ToJObject();

        SetServiceParameter(serviceParameterObject);

        var oauth2AppToken = await _authenticationHelper.AcquireOAuth2TokenByScopeAsync(
            serviceParameterObject[Constants.ClientID].ToString(),
            serviceParameterObject[Constants.AuthKey].ToString(),
            serviceParameterObject[Constants.Authority].ToString(),
            serviceParameterObject[Constants.ResourceURL].ToString(),
            "/.default");

        var tenantAdaptor = _tenantFactory.GetTenant(TenantInfo, String.Empty, Constants.WebClient, oauth2AppToken.AccessToken);
        try
        {
            var detailOperationList = TenantInfo.DetailOperations.DetailOpsList.Where(o => o.IsCached == true).ToList();
            bool detailsLoadSuccess = false;
            if (_approvalSummaryProvider.GetApprovalSummaryByDocumentNumberAndApprover(approvalTenantInfo.DocTypeId, approvalIdentifier.DisplayDocumentNumber, approver) != null)
            {
                for (int retryCount = 0; retryCount < TenantInfo.TenantDetailRetryCount && detailOperationList.Count > 0; retryCount++)
                {
                    Dictionary<string, bool> operationStatus = new Dictionary<string, bool>();
                    foreach (var operation in detailOperationList)
                    {
                        try
                        {
                            if (detailsData != null && detailsData.ContainsKey(operation.operationtype))
                            {
                                var jsonDetail = tenantAdaptor.PostProcessDetails(detailsData[operation.operationtype], operation.operationtype);

                                // Adding dynamic editable properties if any
                                jsonDetail = tenantAdaptor.AddEditableFieldsProperties(jsonDetail, operation.operationtype);
                                var detailObject = new ApprovalDetailsEntity()
                                {
                                    JSONData = jsonDetail,
                                    RowKey = operation.operationtype,
                                    TenantID = TenantInfo.TenantId,
                                    PartitionKey = approvalIdentifier?.GetDocNumber(TenantInfo)
                                };

                                detailsRows.Add(detailObject);
                                LogMessageProgress(new List<ApprovalRequestExpressionExt> { approvalRequest }, TrackingEvent.DetailsDataPreattachedInARX, message, null, CriticalityLevel.Yes, null, operation.operationtype);
                            }
                            else
                            {
                                using (var detailRowsInsertTracer = _performanceLogger.StartPerformanceLogger("PerfLog", Constants.WorkerRole, string.Format(Constants.PerfLogAction, TenantInfo.AppName, operation.operationtype), logData))
                                {
                                    LogMessageProgress(new List<ApprovalRequestExpressionExt> { approvalRequest }, TrackingEvent.DetailsDataPrefetchIntitiated, message, null, CriticalityLevel.Yes, null, operation.operationtype);
                                    var responseAdaptor = await tenantAdaptor.GetDetailAsync(approvalIdentifier, operation.operationtype, page, Environment.UserName, Xcv, Tcv, BusinessProcessName);
                                    responseContent = responseAdaptor.Content;
                                    if (responseAdaptor.StatusCode.Equals(HttpStatusCode.OK))
                                    {
                                        ApprovalDetailsEntity detailsRow = new ApprovalDetailsEntity()
                                        {
                                            JSONData = await responseContent.ReadAsStringAsync(),
                                            TenantID = TenantInfo.TenantId,
                                            PartitionKey = approvalIdentifier?.GetDocNumber(TenantInfo),
                                            RowKey = operation.operationtype
                                        };
                                        detailsRows.Add(detailsRow);
                                        LogMessageProgress(new List<ApprovalRequestExpressionExt> { approvalRequest }, TrackingEvent.DetailsDataPrefetchSuccess, message, null, CriticalityLevel.Yes, null, operation.operationtype);
                                    }
                                    else
                                    {
                                        throw new WebException(await responseAdaptor.Content.ReadAsStringAsync());
                                    }
                                }
                            }
                            operationStatus.Add(operation.operationtype, true);
                        }
                        catch (Exception)
                        {
                            operationStatus.Add(operation.operationtype, false);
                            LogMessageProgress(new List<ApprovalRequestExpressionExt> { approvalRequest }, TrackingEvent.DetailsDataPrefetchFailed, message, null, CriticalityLevel.Yes, null, operation.operationtype);
                        }
                    }
                    detailOperationList.RemoveAll(o => operationStatus.Where(s => s.Value == true).Select(s => s.Key).Contains(o.operationtype));
                }
            }
            if (detailOperationList.Count == 0)
            {
                detailsLoadSuccess = true;
            }

            // Needn't call the method when there are no details to store
            if (detailsRows != null && detailsRows.Count > 0)
            {
                await _approvalDetailProvider.AddApprovalsDetails(detailsRows, TenantInfo, Environment.UserName, Xcv, Tcv);
                LogMessageProgress(new List<ApprovalRequestExpressionExt> { approvalRequest }, TrackingEvent.DetailsInsertedInBackground, message, null, CriticalityLevel.Yes);
            }
            return detailsLoadSuccess;
        }
        catch (Exception failedToAddDetailsInAzureTable)
        {
            _logger.LogError(TrackingEvent.AzureStorageAddRequestDetailsFail, failedToAddDetailsInAzureTable, logData);
            return false;
        }
    }

    /// <summary>
    /// Set Service Parameter
    /// </summary>
    /// <param name="serviceParameterObject"></param>
    private void SetServiceParameter(JObject serviceParameterObject)
    {
        if (serviceParameterObject != null)
        {
            if (!serviceParameterObject.ContainsKey(Constants.Authority))
            {
                serviceParameterObject[Constants.Authority] = _config[ConfigurationKey.Authority.ToString()].ToString();
            }
            if (serviceParameterObject.ContainsKey(Constants.KeyVaultUri))
            {
                serviceParameterObject[Constants.AuthKey] = _config[ConfigurationKey.ServiceParameterAuthKey.ToString() + "-" + serviceParameterObject[Constants.KeyVaultUri].ToString()].ToString();
                serviceParameterObject[Constants.ClientID] = _config[ConfigurationKey.ServiceParameterClientID.ToString() + "-" + serviceParameterObject[Constants.KeyVaultUri].ToString()].ToString();
            }
            else
            {
                serviceParameterObject[Constants.AuthKey] = _config[ConfigurationKey.ServiceParameterAuthKey.ToString()].ToString();
                serviceParameterObject[Constants.ClientID] = _config[ConfigurationKey.ServiceParameterClientID.ToString()].ToString();
            }
        }
    }

    /// <summary>
    /// Gets the list of SummaryRows which needs to be deleted
    /// </summary>
    /// <param name="approvalRequest"></param>
    /// <param name="rowKeyValue"></param>
    /// <returns></returns>
    private async Task<IEnumerable<ApprovalSummaryRow>> GetSummaryRowsForDeletion(ApprovalRequestExpressionExt approvalRequest, string rowKeyValue)
    {
        #region Logging

        var logData = new Dictionary<LogDataKey, object>()
    {
        { LogDataKey.BusinessProcessName, string.Format(TenantInfo.BusinessProcessName, Constants.BusinessProcessNameSendPayload, approvalRequest.Operation) },
        { LogDataKey.DXcv, approvalRequest.ApprovalIdentifier.DisplayDocumentNumber },
        { LogDataKey.Tcv, approvalRequest.Telemetry.Tcv },
        { LogDataKey.ReceivedTcv, approvalRequest.Telemetry.Tcv },
        { LogDataKey.TenantTelemetryData, approvalRequest.Telemetry.TenantTelemetry },
        { LogDataKey.Xcv, approvalRequest.Telemetry.Xcv }
    };

        #endregion Logging

        List<ApprovalSummaryRow> summaryRows = null;

        // Uses the DeleteFor property of ARX to find out the alias for which the summary rows needs to be deleted
        if (approvalRequest.DeleteFor != null && approvalRequest.DeleteFor.Count > 0)
        {
            using (var perfTracer = _performanceLogger.StartPerformanceLogger("PerfLog", Constants.WorkerRole, string.Format(Constants.PerfLogActionWithInfo, TenantInfo.AppName, "DeleteNotificationProcessing", "DeleteForDataReceivedApprover"), logData))
            {
                summaryRows = await GetSummaryRowsFromTableAsync(approvalRequest.DeleteFor, rowKeyValue);
            }
        }

        // Uses the CurrentApprover data from ApproverDetails table to find out the alias for which the summary rows needs to be deleted
        // This is executed in scenarios where DeleteFor property is unavailable
        else
        {
            using (var perfTracer = _performanceLogger.StartPerformanceLogger("PerfLog", Constants.WorkerRole, string.Format(Constants.PerfLogActionWithInfo, TenantInfo.AppName, "DeleteNotificationProcessing", "QueryingDetailsTableForApprover"), logData))
            {
                ApprovalDetailsEntity approvalDetails = _tableHelper.GetDataCollectionByColumns<ApprovalDetailsEntity>(
                        Constants.ApprovalDetailsAzureTableName,
                        new KeyValuePair<string, string>("PartitionKey", approvalRequest.ApprovalIdentifier.DisplayDocumentNumber),
                        QueryComparisons.Equal,
                        TableOperators.And,
                        new KeyValuePair<string, string>("RowKey", "CurrentApprover"),
                        QueryComparisons.Equal
                        ).FirstOrDefault();
                if (approvalDetails != null)
                {
                    List<string> currentApproverList = approvalDetails.JSONData.FromJson<List<Approver>>().Select(a => a.Alias).ToList();
                    if (currentApproverList != null && currentApproverList.Count > 0)
                    {
                        summaryRows = await GetSummaryRowsFromTableAsync(currentApproverList, rowKeyValue);
                    }
                }
            }
        }

        // if DeleteFor is available then we already tried to fetch summary using those alias.
        if (approvalRequest.DeleteFor == null || approvalRequest.DeleteFor.Count == 0)
        {
            // Default fall back method when the above two approaches doesn't return any data (SummaryRows)
            if (summaryRows == null || summaryRows.Count == 0)
            {
                using (var perfTracer = _performanceLogger.StartPerformanceLogger("PerfLog", Constants.WorkerRole, string.Format(Constants.PerfLogActionWithInfo, TenantInfo.AppName, "DeleteNotificationProcessing", "QueryingSummaryTableWithoutApprover"), logData))
                {
                    summaryRows = _tableHelper.GetTableEntityListByRowKey<ApprovalSummaryRow>(_config[ConfigurationKey.ApprovalSummaryAzureTableName.ToString()], rowKeyValue).ToList();
                }
            }
        }

        return summaryRows;
    }

    /// <summary>
    /// Gets the summary rows from table asynchronous.
    /// </summary>
    /// <param name="approvers">The approvers.</param>
    /// <param name="rowKeyValue">The row key value.</param>
    /// <returns>returns list of approvalSummaryrow</returns>
    private async Task<List<ApprovalSummaryRow>> GetSummaryRowsFromTableAsync(List<string> approvers, string rowKeyValue)
    {
        var tasks = new List<Task<List<ApprovalSummaryRow>>>();

        var summaryRows = new List<ApprovalSummaryRow>();

        foreach (var keyItem in approvers)
        {
            tasks.Add(GetDataCollectionAsync(keyItem, rowKeyValue));
        }

        // to ensure all the task are executed and getting the result.
        var results = await Task.WhenAll(tasks);
        foreach (var item in results.ToList())
        {
            summaryRows.AddRange(item);
        }

        return summaryRows;
    }

    /// <summary>
    /// Gets the data collection asynchronous.
    /// </summary>
    /// <param name="filterString">The filter string.</param>
    /// <returns>returns api result</returns>
    private Task<List<ApprovalSummaryRow>> GetDataCollectionAsync(string partitionKey, string rowKey)
    {
        return Task.Run(() =>
        {
            var summaryRows = _tableHelper.GetTableEntityListByPartitionKeyAndRowKey<ApprovalSummaryRow>(_config[ConfigurationKey.ApprovalSummaryAzureTableName.ToString()], partitionKey, rowKey);
            return summaryRows;
        });
    }

    /// <summary>
    /// Removes the Approval Details (along with attachments) from Table storage and BLOB respectively
    /// </summary>
    /// <param name="approvalRequest"></param>
    /// <param name="azuretablerowdeletionresult"></param>
    /// <param name="summaryRows"></param>
    private async Task RemoveApprovalDetailsFromAzureTable(ApprovalRequestExpressionExt approvalRequest, AzureTableRowDeletionResult azuretablerowdeletionresult, List<ApprovalSummaryRow> summaryRows)
    {
        try
        {
            if (azuretablerowdeletionresult == AzureTableRowDeletionResult.DeletionSuccessful || azuretablerowdeletionresult == AzureTableRowDeletionResult.SkippedDueToNonExistence)
            {
                List<ApprovalDetailsEntity> detailsRows = new List<ApprovalDetailsEntity>();
                AzureTableRowDeletionResult azuretabledetailrowdeletionresult = AzureTableRowDeletionResult.SkippedDueToNonExistence;

                string partitionKey = approvalRequest.ApprovalIdentifier.DisplayDocumentNumber;
                if (TenantInfo.UseDocumentNumberForRowKey)
                {
                    partitionKey = approvalRequest.ApprovalIdentifier.DocumentNumber;
                }

                detailsRows = _approvalDetailProvider.GetAllApprovalsDetails(TenantInfo.TenantId, partitionKey).Where(x => (x.RowKey != Constants.CurrentApprover && x.RowKey != Constants.SummaryOperationType)).ToList();
                if (detailsRows != null && detailsRows.Count > 0)
                {
                    // TODO:: Find better way to do this
                    // filter the detailsRows and remove the 'APPRCHAIN' row so that it doesn't get deleted during an UPDATE operation
                    if (approvalRequest.Operation.Equals(ApprovalRequestOperation.Update))
                    {
                        detailsRows = detailsRows.Where(x => (x.RowKey != Constants.ApprovalChainOperation && x.RowKey != Constants.TransactionDetailsOperationType)).ToList();
                    }
                    azuretabledetailrowdeletionresult = await _approvalDetailProvider.RemoveApprovalsDetails(detailsRows);
                }

                #region Get Attachment Details and initiate a Delete call

                ITenant tenant = _tenantFactory.GetTenant(TenantInfo);

                List<Attachment> attachments = await tenant.GetAttachmentDetails(summaryRows, approvalRequest.ApprovalIdentifier, approvalRequest.Telemetry);

                // Fire and forget code to delete the blob related to this DocumentNumber and tenantId combination
                await _approvalDetailProvider.RemoveAttachmentFromBlob(attachments, approvalRequest.ApprovalIdentifier, tenant.AttachmentOperationName(), Constants.WorkerRole, TenantInfo, approvalRequest.Telemetry);

                #endregion Get Attachment Details and initiate a Delete call
            }
        }
        catch (Exception failedToRemoveDetailsFromAzureTable)
        {
            var logData = new Dictionary<LogDataKey, object>()
        {
            { LogDataKey.Tcv, approvalRequest.Telemetry.Tcv },
            { LogDataKey.Xcv, approvalRequest.Telemetry.Xcv },
            { LogDataKey.ReceivedTcv, approvalRequest.Telemetry.Tcv },
            { LogDataKey.TenantTelemetryData, approvalRequest.Telemetry.TenantTelemetry },
            { LogDataKey.BusinessProcessName, string.Format(TenantInfo.BusinessProcessName, Constants.BusinessProcessNameSendPayload, approvalRequest.Operation) },
            { LogDataKey.DXcv, approvalRequest.ApprovalIdentifier.DisplayDocumentNumber },
            { LogDataKey.TenantId, TenantInfo.TenantId },
            { LogDataKey.DocumentNumber, approvalRequest.ApprovalIdentifier.DisplayDocumentNumber }
        };
            _logger.LogError(TrackingEvent.AzureStorageRemoveRequestDetailsFail, failedToRemoveDetailsFromAzureTable, logData);
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
    /// <param name="tenantLogData"></param>
    /// <param name="DetailsOperation"></param>
    private void LogMessageProgress(List<ApprovalRequestExpressionExt> expressions, TrackingEvent trackingEvent, Message message, FailureData failureData, CriticalityLevel criticalityLevel, Dictionary<LogDataKey, object> tenantLogData = null, string DetailsOperation = "")
    {
        foreach (var expression in expressions)
        {
            StringBuilder approverList = new StringBuilder();
            if (expression.Approvers != null && expression.Approvers.Count > 0)
            {
                for (int i = 0; i < expression.Approvers.Count; i++)
                {
                    if (i > 0) { approverList.Append(", "); }
                    approverList.Append(expression.Approvers[i].Alias);
                }
            }
            if (tenantLogData == null)
            {
                tenantLogData = new Dictionary<LogDataKey, object>();
            }

            tenantLogData[LogDataKey.IsCriticalEvent] = criticalityLevel.ToString();
            tenantLogData[LogDataKey.Tcv] = expression.Telemetry.Tcv;
            tenantLogData[LogDataKey.ReceivedTcv] = expression.Telemetry.Tcv;
            tenantLogData[LogDataKey.TenantTelemetryData] = expression.Telemetry.TenantTelemetry;
            tenantLogData[LogDataKey.BusinessProcessName] = string.Format(TenantInfo.BusinessProcessName, Constants.BusinessProcessNameSendPayload, expression.Operation);
            tenantLogData[LogDataKey.Xcv] = expression.Telemetry.Xcv;
            tenantLogData[LogDataKey.DXcv] = expression.ApprovalIdentifier.DisplayDocumentNumber;
            tenantLogData[LogDataKey.DocumentNumber] = expression.ApprovalIdentifier.DocumentNumber;
            tenantLogData[LogDataKey.DisplayDocumentNumber] = expression.ApprovalIdentifier.DisplayDocumentNumber;
            tenantLogData[LogDataKey.FiscalYear] = expression.ApprovalIdentifier.FiscalYear;
            tenantLogData[LogDataKey.Approver] = approverList.ToString();
            tenantLogData[LogDataKey.OperationType] = expression.Operation.ToString();
            tenantLogData[LogDataKey.TenantId] = TenantInfo.DocTypeId;
            tenantLogData[LogDataKey.TenantName] = TenantInfo.AppName;
            tenantLogData[LogDataKey.LocalTime] = DateTime.UtcNow;
            tenantLogData[LogDataKey.DeleteForAliases] = expression.Operation != ApprovalRequestOperation.Create ? expression.DeleteFor.ToJson() : string.Empty;
            tenantLogData[LogDataKey.OperationDateTime] = expression.OperationDateTime;
            tenantLogData[LogDataKey._CorrelationId] = message.GetCorrelationId();
            tenantLogData[LogDataKey.BrokerMessageProperty] = message.UserProperties.ToJson();
            tenantLogData[LogDataKey.FailureData] = failureData;
            tenantLogData[LogDataKey.EventId] = (int)trackingEvent;
            tenantLogData[LogDataKey.EventName] = trackingEvent.ToString();

            if (!string.IsNullOrEmpty(DetailsOperation) && !tenantLogData.ContainsKey(LogDataKey.Operation))
            {
                tenantLogData[LogDataKey.Operation] = DetailsOperation;
                tenantLogData[LogDataKey.EventName] = tenantLogData[LogDataKey.EventName] + "-" + DetailsOperation;
            }
            _logger.LogInformation((int)trackingEvent, tenantLogData);
        }
    }

    #endregion Private methods
}