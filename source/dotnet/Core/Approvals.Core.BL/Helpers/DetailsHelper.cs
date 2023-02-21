// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace Microsoft.CFS.Approvals.Core.BL.Helpers;

using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Net.Http;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
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
using Microsoft.CFS.Approvals.Utilities.Extension;
using Microsoft.CFS.Approvals.Utilities.Helpers;
using Microsoft.CFS.Approvals.Utilities.Interface;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

/// <summary>
/// The Details Helper class
/// </summary>
public class DetailsHelper : IDetailsHelper
{
    /// <summary>
    /// The delegation helper
    /// </summary>
    private readonly IDelegationHelper _delegationHelper;

    /// <summary>
    /// The action audit log helper
    /// </summary>
    private readonly IActionAuditLogHelper _actionAuditLogHelper;

    /// <summary>
    /// The log provider
    /// </summary>
    private readonly ILogProvider _logProvider;

    /// <summary>
    /// The performance logger
    /// </summary>
    private readonly IPerformanceLogger _performanceLogger;

    /// <summary>
    /// The approval tenantInfo helper
    /// </summary>
    private readonly IApprovalTenantInfoHelper _approvalTenantInfoHelper;

    /// <summary>
    /// The configuration
    /// </summary>
    private readonly IConfiguration _config;

    /// <summary>
    /// The name resolution helper
    /// </summary>
    private readonly INameResolutionHelper _nameResolutionHelper;

    /// <summary>
    /// The approval detail provider
    /// </summary>
    private readonly IApprovalDetailProvider _approvalDetailProvider;

    /// <summary>
    /// The flighting data provider
    /// </summary>
    private readonly IFlightingDataProvider _flightingDataProvider;

    /// <summary>
    /// The editable configuration helper
    /// </summary>
    private readonly IEditableConfigurationHelper _editableConfigurationHelper;

    /// <summary>
    /// The summary helper
    /// </summary>
    private readonly ISummaryHelper _summaryHelper;

    /// <summary>
    /// The approval history provider
    /// </summary>
    private readonly IApprovalHistoryProvider _approvalHistoryProvider;

    /// <summary>
    /// The tenant factory
    /// </summary>
    private readonly ITenantFactory _tenantFactory;

    /// <summary>
    /// The image retriever
    /// </summary>
    private readonly IImageRetriever _imageRetriever;

    /// <summary>
    /// Blob storage helper.
    /// </summary>
    private readonly IBlobStorageHelper _blobStorageHelper;

    /// <summary>
    /// Constructor of DetailsHelper
    /// </summary>
    /// <param name="delegationHelper"></param>
    /// <param name="actionAuditLogHelper"></param>
    /// <param name="logProvider"></param>
    /// <param name="performanceLogger"></param>
    /// <param name="approvalTenantInfoHelper"></param>
    /// <param name="config"></param>
    /// <param name="nameResolutionHelper"></param>
    /// <param name="approvalDetailProvider"></param>
    /// <param name="flightingDataProvider"></param>
    /// <param name="editableConfigurationHelper"></param>
    /// <param name="summaryHelper"></param>
    /// <param name="approvalHistoryProvider"></param>
    /// <param name="tenantFactory"></param>
    /// <param name="imageRetriever"></param>
    /// <param name="blobStorageHelper"></param>
    public DetailsHelper(
        IDelegationHelper delegationHelper,
        IActionAuditLogHelper actionAuditLogHelper,
        ILogProvider logProvider,
        IPerformanceLogger performanceLogger,
        IApprovalTenantInfoHelper approvalTenantInfoHelper,
        IConfiguration config,
        INameResolutionHelper nameResolutionHelper,
        IApprovalDetailProvider approvalDetailProvider,
        IFlightingDataProvider flightingDataProvider,
        IEditableConfigurationHelper editableConfigurationHelper,
        ISummaryHelper summaryHelper,
        IApprovalHistoryProvider approvalHistoryProvider,
        ITenantFactory tenantFactory,
        IImageRetriever imageRetriever,
        IBlobStorageHelper blobStorageHelper)
    {
        _delegationHelper = delegationHelper;
        _actionAuditLogHelper = actionAuditLogHelper;
        _logProvider = logProvider;
        _performanceLogger = performanceLogger;
        _approvalTenantInfoHelper = approvalTenantInfoHelper;
        _config = config;
        _nameResolutionHelper = nameResolutionHelper;
        _approvalDetailProvider = approvalDetailProvider;
        _flightingDataProvider = flightingDataProvider;
        _editableConfigurationHelper = editableConfigurationHelper;
        _summaryHelper = summaryHelper;
        _approvalHistoryProvider = approvalHistoryProvider;
        _tenantFactory = tenantFactory;
        _imageRetriever = imageRetriever;
        _blobStorageHelper = blobStorageHelper;
    }

    #region Implemented Methods

    /// <summary>
    /// Auth Sum.
    /// </summary>
    /// <param name="tenantAdaptor">The tenant adaptor</param>
    /// <param name="approvalDetails">The approval details</param>
    /// <param name="tenantId">Tenant Id of the tenant (1/2/3..)</param>
    /// <param name="tenantInfo">ApprovalTenantInfo object for the current tenant</param>
    /// <param name="documentSummary">ApprovalSummaryRow object</param>
    /// <param name="transactionHistoryExts">Historical data</param>
    /// <param name="currentApproverInDbJson">Current Approver JSON from Approval Details table</param>
    /// <param name="documentNumber">Document Number of the request</param>
    /// <param name="operation">Operation type (DT1/LINE etc.)</param>
    /// <param name="alias">Alias of the Approver of this request</param>
    /// <param name="loggedInAlias">Logged in User Alias</param>
    /// <param name="sessionId">GUID session id</param>
    /// <param name="fiscalYear">Fiscal year of the request</param>
    /// <param name="page">Page number</param>
    /// <param name="xcv">Cross system correlation vector for telemetry and logging</param>
    /// <param name="tcv">GUID transaction correlation vector for telemetry and logging</param>
    /// <param name="isWorkerTriggered">is Worker role triggered</param>
    /// <param name="sectionType">Section type e.g. Summary, Details</param>
    /// <param name="clientDevice">Client Device</param>
    /// <param name="aadUserToken">AAD User Token</param>
    /// <returns>Details data as JObject</returns>
    public async Task<JObject> AuthSum(
        ITenant tenantAdaptor,
        List<ApprovalDetailsEntity> approvalDetails,
        int tenantId,
        ApprovalTenantInfo tenantInfo,
        ApprovalSummaryRow documentSummary,
        List<TransactionHistoryExt> transactionHistoryExts,
        ApprovalDetailsEntity currentApproverInDbJson,
        string documentNumber,
        string operation,
        string alias,
        string loggedInAlias,
        string sessionId,
        string fiscalYear,
        int page,
        string xcv,
        string tcv,
        bool isWorkerTriggered,
        int sectionType,
        string clientDevice,
        string aadUserToken)
    {
        #region Logging Prep

        var logData = new Dictionary<LogDataKey, object>
        {
            { LogDataKey.Tcv, tcv },
            { LogDataKey.SessionId, sessionId },
            { LogDataKey.Xcv, xcv },
            { LogDataKey.UserRoleName, loggedInAlias },
            { LogDataKey.BusinessProcessName, string.Format(tenantInfo.BusinessProcessName, Constants.BusinessProcessNameGetDetails, Constants.BusinessProcessNameDetailsFromStorage) },
            { LogDataKey.TenantId, tenantId },
            { LogDataKey.DocumentNumber, documentNumber },
            { LogDataKey.DXcv, documentNumber },
            { LogDataKey.FiscalYear, fiscalYear },
            { LogDataKey.Operation, operation },
            { LogDataKey.UserAlias, alias },
            { LogDataKey.IsCriticalEvent, CriticalityLevel.No.ToString() }
        };

        #endregion Logging Prep

        JObject responseObject = null;

        try
        {
            #region AuthSum

            using (_performanceLogger.StartPerformanceLogger("PerfLog", Constants.WebClient, string.Format(Constants.PerfLogActionWithInfo, tenantInfo.AppName, operation, "Authsum Details from storage"), logData))
            {
                #region Extract AdditionalDetails from ApprovalDetails data

                string additionalData = approvalDetails.FirstOrDefault(details => details.RowKey.Equals(Constants.AdditionalDetails))?.JSONData;

                #endregion Extract AdditionalDetails from ApprovalDetails data

                #region Check if Summary or History Exist

                try
                {
                    if (documentSummary != null)
                    {
                        responseObject = documentSummary.SummaryJson.ToJObject();
                        if (responseObject[Constants.AdditionalData] != null && responseObject[Constants.AdditionalData].Any() && additionalData == null)
                        {
                            additionalData = JObject.FromObject(new { AdditionalData = responseObject[Constants.AdditionalData] }).ToJson();
                            ApprovalDetailsEntity detailsRow = new ApprovalDetailsEntity()
                            {
                                JSONData = additionalData,
                                TenantID = tenantInfo.TenantId,
                                PartitionKey = documentNumber,
                                RowKey = Constants.AdditionalDetails
                            };
                            approvalDetails.Add(detailsRow);
                        }

                        logData.Add(LogDataKey.ReceivedTcv, documentSummary.Tcv);
                    }
                    else if (transactionHistoryExts != null)
                    {
                        // Check if a history record exists for the given Action Date
                        var history = transactionHistoryExts.OrderByDescending(h => h.ActionDate).FirstOrDefault();

                        // Get response object only if a History element exists
                        if (history != null)
                        {
                            responseObject = history.JsonData.ToJObject();
                        }
                    }
                }
                catch
                {
                    throw new InvalidOperationException(_config[ConfigurationKey.Message_ServiceNotRelayed.ToString()]);
                }

                #endregion Check if Summary or History Exist

                #region Add additional data if Summary or History exist

                // TODO::MD1:: Check if this code can be changed to handle cases when Summary exists vs. does not
                // TODO::MD1:: Because when Summary does not exist, it means the approval has been fully completed and Approvers, Actions, Failed Actions, Editable Fields need not be added
                if (responseObject != null)
                {
                    #region Get Available Details

                    Dictionary<string, ApprovalDetailsEntity> availableDetails = GetAvailableDetailsAndOperatioNames(
                            approvalDetails,
                            tenantInfo,
                            alias,
                            logData);

                    #endregion Get Available Details

                    if (sectionType != (int)DataCallType.Details)
                    {
                        #region Add Approvers

                        string approverChainString = await CreateCurrentApproverChain(
                                documentSummary,
                                transactionHistoryExts,
                                approvalDetails,
                                alias,
                                currentApproverInDbJson,
                                tenantInfo,
                                xcv,
                                tcv,
                                isWorkerTriggered);

                        responseObject["Approvers"] = approverChainString.ToJArray();

                        #endregion Add Approvers
                    }

                    #region Add primary and secondary actions if this is pending approval

                    if (responseObject.Property("Actions") == null)
                    {
                        var actions = await AddAllowedActions(alias, loggedInAlias, tenantInfo, documentSummary, additionalData, clientDevice, sessionId, xcv, tcv, aadUserToken);
                        if (actions != null)
                        {
                            responseObject.Add("Actions", JToken.FromObject(actions));
                        }
                    }

                    #endregion Add primary and secondary actions if this is pending approval

                    #region Add App Name and related details

                    if (responseObject.Property("AppName") == null)
                    {
                        responseObject.Add("AppName", tenantInfo.AppName);
                    }

                    if (responseObject.Property("CondensedAppName") == null)
                    {
                        responseObject.Add("CondensedAppName", tenantInfo.AppName.Replace(" ", string.Empty));
                    }

                    if (responseObject.Property("templateName") == null)
                    {
                        responseObject.Add("templateName", tenantInfo.TemplateName);
                    }

                    if (responseObject.Property("BusinessProcessName") == null)
                    {
                        responseObject.Add("BusinessProcessName", tenantInfo.BusinessProcessName);
                    }

                    // Send document number prefix based on LOB
                    responseObject["DocumentNumberPrefix"] = tenantInfo.DocumentNumberPrefix;
                    responseObject["ToolName"] = tenantInfo.ToolName;

                    #endregion Add App Name and related details

                    #region Add Editable Fields Information

                    Dictionary<string, string> editableConfiguration = _editableConfigurationHelper.GetEditableConfigurationByTenant(tenantInfo.TenantId).ToDictionary(k => k.RowKey, v => v.RegularExpression);
                    responseObject.Add("EditableFields", editableConfiguration.ToJson().ToJToken());

                    #endregion Add Editable Fields Information

                    #region Tenantwise Modify Response to remove certain fields

                    responseObject = tenantAdaptor.RemoveFieldsFromResponse(responseObject);

                    #endregion Tenantwise Modify Response to remove certain fields

                    #region Get Available Details and Call Back URLs for missing details

                    // Find out all Operations which do NOT have data in local storage
                    // For these operations URLs need to be provided so that Front End Controller can make additional calls
                    var approvalIdentifier = responseObject["ApprovalIdentifier"].ToString(Formatting.None).FromJson<ApprovalIdentifier>();
                    List<string> operations = GetOperationListForMissingDetails(availableDetails, tenantInfo, approvalIdentifier);

                    #endregion Get Available Details and Call Back URLs for missing details

                    #region Add Available Details and Call Back URLs for missing details

                    JObject partDetail = null;

                    // Add these details to the response object
                    foreach (var detail in availableDetails.ToList())
                    {
                        partDetail = null;
                        if (detail.Value != null && detail.Value.JSONData != null)
                        {
                            partDetail = detail.Value.JSONData.ToJObject();
                        }

                        foreach (var part in partDetail)
                        {
                            // TODO:: This code assumes Attachments property can be a part of both SummaryJson (as part of ARX) or in any of the tenant calls or sub-property
                            // This might undergo changes.

                            if (part.Key.Equals("Attachments", StringComparison.InvariantCultureIgnoreCase) && !string.IsNullOrEmpty(responseObject.Property(part.Key).Value.ToString()))
                            {
                                var attachments = responseObject.Property(part.Key).Value.ToObject<List<Attachment>>();

                                var tenantAttachments = part.Value.ToObject<List<Attachment>>();

                                foreach (var attachment in tenantAttachments)
                                {
                                    attachments.Add(attachment);
                                }

                                responseObject.Property(part.Key).Value = attachments.ToJToken();
                            }

                            if (responseObject.Property(part.Key) != null && string.IsNullOrEmpty(responseObject.Property(part.Key).Value.ToString()))
                            {
                                responseObject.Property(part.Key).Value = part.Value;
                            }
                            else if (responseObject.Property(part.Key) == null)
                            {
                                responseObject.Add(part.Key, part.Value);
                            }
                        }
                    }

                    #region Check if Request Activities are sent by tenant and set the token

                    responseObject.Add("NextLevelApprovalList", new JArray());
                    responseObject.Add("defaultnextlevel", string.Empty);

                    // Adding Business Approvers
                    if (responseObject["AdditionalData"] != null && responseObject["AdditionalData"].HasValues)
                    {
                        var additionalDataValue = responseObject["AdditionalData"];
                        if (additionalDataValue["RequestActivities"] != null)
                        {
                            var requestActivities = additionalDataValue["RequestActivities"];
                            if (requestActivities != null && ((JValue)requestActivities).Value != null)
                            {
                                var value = ((JValue)requestActivities).Value;
                                var requestActivitiesObject = JsonConvert.DeserializeObject(value.ToString());
                                responseObject["RequestActivities"] = requestActivitiesObject.ToJson().ToJArray();
                            }
                        }
                        if (additionalDataValue["Approvers"] != null)
                        {
                            var approvers = additionalDataValue["Approvers"];
                            if (approvers != null && ((JValue)approvers).Value != null)
                            {
                                var value = ((JValue)approvers).Value;
                                var NextLevelApprovalList = JsonConvert.DeserializeObject(value.ToString());
                                responseObject["NextLevelApprovalList"] = NextLevelApprovalList.ToJson().ToJArray();

                                var defaultNextLevel = string.Empty;
                                foreach (JToken approver in JArray.Parse(approvers?.ToString())?.Children())
                                {
                                    if ((bool)approver?.SelectToken("Default") == true)
                                    {
                                        defaultNextLevel = approver?.ToString();
                                    }
                                }
                                responseObject["defaultnextlevel"] = Regex.Replace(defaultNextLevel.Replace(Environment.NewLine, string.Empty), Constants.RegexRemoveExtraSpace, "$1");
                            }
                        }
                    }

                    #endregion Check if Request Activities are sent by tenant and set the token

                    if (sectionType != (int)DataCallType.Summary)
                    {
                        // Adding pending operations
                        string urlPropertyName = "CallBackURLCollection";
                        if (responseObject.Property(urlPropertyName) == null)
                        {
                            responseObject.Add(urlPropertyName, operations.ToJson().ToJToken());
                        }
                    }

                    #endregion Add Available Details and Call Back URLs for missing details

                    #region Modify Response as per tenant configuration by processing Post AuthSum actions

                    responseObject = tenantAdaptor.ExecutePostAuthSum(responseObject);

                    #endregion Modify Response as per tenant configuration by processing Post AuthSum actions

                    #region Add last failed action for this document based on previous action by this user

                    // Add transactional details to the response object from ApprovalDetails table
                    // Get TransactionalDetails for given user/ approver
                    var transDetails = approvalDetails.FirstOrDefault(t => t.RowKey == Constants.TransactionDetailsOperationType + "|" + alias);
                    if (transDetails != null)
                    {
                        if (transDetails.JSONData.ToJObject()["LastFailedExceptionMessage"] != null)
                        {
                            responseObject.Add("LastFailedExceptionMessage", transDetails.JSONData.ToJObject()["LastFailedExceptionMessage"]);
                        }

                        if (transDetails.JSONData.ToJObject()["LastFailedOutOfSyncMessage"] != null)
                        {
                            responseObject.Add("LastFailedOutOfSyncMessage", transDetails.JSONData.ToJObject()["LastFailedOutOfSyncMessage"]);
                        }
                    }

                    // Inject Failed Exception Message
                    if (documentSummary != null)
                    {
                        if (responseObject.Property("LastFailed") == null)
                        {
                            responseObject.Add("LastFailed", documentSummary.LastFailed.ToJson().ToJToken());
                        }

                        if (responseObject.Property("LastFailedExceptionMessage") == null)
                        {
                            responseObject.Add("LastFailedExceptionMessage", ((documentSummary.LastFailedExceptionMessage == null) ? new JArray() : documentSummary.LastFailedExceptionMessage.ToJson().ToJToken()));
                        }

                        if (responseObject.Property("LastFailedOutOfSyncMessage") == null)
                        {
                            responseObject.Add("LastFailedOutOfSyncMessage", documentSummary.LastFailedOutOfSyncMessage.ToJson().ToJToken());
                        }
                    }

                    #endregion Add last failed action for this document based on previous action by this user

                    if (documentSummary == null)
                    {
                        var actionAuditLog = (await _actionAuditLogHelper.GetActionAuditLogsByDocumentNumberAndApprover(documentNumber, alias)).OrderByDescending(a => a.ActionDateTime).FirstOrDefault();
                        if (actionAuditLog != null)
                        {
                            responseObject["ActionTakeOnMessage"] = string.Format(_config[ConfigurationKey.ActionAlreadyTakenFromApprovalsMessage.ToString()], actionAuditLog.ActionTaken, actionAuditLog.ActionDateTime, actionAuditLog.ClientType);
                        }
                        else
                        {
                            responseObject["ActionTakeOnMessage"] = Constants.ActionTakenMessage;
                        }
                    }
                    else if (documentSummary.LobPending) // Show notification that action is already taken.
                    {
                        responseObject["ActionTakeOnMessage"] = string.Format(_config[ConfigurationKey.ActionAlreadyTakenMessage.ToString()], documentNumber);
                    }

                    #region Check if Offline Approval is supported for the given request upfront

                    responseObject.Add("IsBackgroundApprovalSupportedUpfront", tenantInfo.IsBackgroundProcessingEnabledUpfront);

                    #endregion Check if Offline Approval is supported for the given request upfront

                    #region Check if details data can be viewed from history

                    responseObject.Add("IsHistoryClickable", tenantInfo.IsHistoryClickable);

                    #endregion Check if details data can be viewed from history

                    #region Check if pictorial line items are enabled

                    responseObject.Add("IsPictorialLineItemsEnabled", tenantInfo.IsPictorialLineItemsEnabled);
                    responseObject.Add("LineItemFilterCategories", tenantInfo.LineItemFilterCategories);

                    #endregion Check if pictorial line items are enabled

                    #region Add base64 UserImage string

                    if (sectionType != (int)DataCallType.Details)
                    {
                        string submitterAlias = responseObject["Submitter"] != null && responseObject["Submitter"]["Alias"] != null ? responseObject["Submitter"]["Alias"].ToString() : string.Empty;
                        responseObject.Add("UserImage", await GetUserImage(submitterAlias, sessionId, clientDevice, logData));
                    }

                    #endregion Add base64 UserImage string

                    #region Check if Upload Attachment feature enabled
                    var isUploadAttachmentEnabled = (tenantInfo.IsUploadAttachmentsEnabled ?
                                                     _flightingDataProvider.IsFeatureEnabledForUser(alias, (int)FlightingFeatureName.UploadAttachment)
                                                    : false);
                    responseObject.Add("isUploadAttachmentEnabled", isUploadAttachmentEnabled);
                    #endregion Check if Upload Attachment feature enabled
                }

                #endregion Add additional data if Summary or History exist
            }

            #endregion AuthSum
        }
        catch (Exception ex)
        {
            _logProvider.LogError(TrackingEvent.DetailFetchFailure, ex, logData);
            throw;
        }

        return responseObject;
    }

    /// <summary>
    /// Download Document Async.
    /// </summary>
    /// <param name="tenantAdaptor">The tenant adaptor</param>
    /// <param name="tenantId">Tenant Id of the tenant (1/2/3..)</param>
    /// <param name="tenantInfo">ApprovalTenantInfo object for the current tenant</param>
    /// <param name="approvalIdentifier">The Approval Identifier object which has the DisplayDocumentNumber, DocumentNumber and FiscalYear</param>
    /// <param name="alias">Alias of the Approver of this request</param>
    /// <param name="attachmentId">Attachment ID of the Document to be downloaded</param>
    /// <param name="sessionId">GUID session id</param>
    /// <param name="loggedInAlias">Logged in User Alias</param>
    /// <param name="xcv">Cross system correlation vector for telemetry and logging</param>
    /// <param name="tcv">GUID transaction correlation vector for telemetry and logging</param>
    /// <returns>HttpResponseMessage with Stream data of the attachment</returns>
    public async Task<byte[]> DownloadDocumentAsync(ITenant tenantAdaptor, int tenantId, ApprovalTenantInfo tenantInfo, ApprovalIdentifier approvalIdentifier, string alias, string attachmentId, string sessionId, string loggedInAlias, string xcv, string tcv)
    {
        #region Logging Prep

        var logData = new Dictionary<LogDataKey, object>
        {
            { LogDataKey.Tcv, tcv },
            { LogDataKey.ReceivedTcv, tcv },
            { LogDataKey.SessionId, sessionId },
            { LogDataKey.Xcv, xcv },
            { LogDataKey.UserRoleName, loggedInAlias },
            { LogDataKey.BusinessProcessName, string.Format(tenantInfo.BusinessProcessName, Constants.BusinessProcessNameGetDocuments, Constants.BusinessProcessNameUserTriggered) },
            { LogDataKey.TenantId, tenantId },
            { LogDataKey.DocumentNumber, approvalIdentifier.DisplayDocumentNumber },
            { LogDataKey.DXcv, approvalIdentifier.DisplayDocumentNumber },
            { LogDataKey.FiscalYear, approvalIdentifier.FiscalYear },
            { LogDataKey.UserAlias, alias },
            { LogDataKey.IsCriticalEvent, CriticalityLevel.No.ToString() }
        };

        #endregion Logging Prep

        byte[] httpResponseMessage = null;
        try
        {
            using (_performanceLogger.StartPerformanceLogger("PerfLog", Constants.WebClient, string.Format(Constants.PerfLogAction, tenantInfo.AppName, Constants.DocumentDownloadAction), logData))
            {
                ApprovalsTelemetry telemetry = new ApprovalsTelemetry()
                {
                    Xcv = xcv,
                    Tcv = tcv,
                    BusinessProcessName = string.Format(tenantInfo.BusinessProcessName, Constants.BusinessProcessNameGetDocuments, Constants.BusinessProcessNameUserTriggered),
                    TenantTelemetry = new Dictionary<string, string>()
                };

                // Check if a specific attachment id has been provided, in which case use the method which uses attachment id
                if (!string.IsNullOrWhiteSpace(attachmentId))
                {
                    httpResponseMessage = await tenantAdaptor.DownloadDocumentUsingAttachmentIdAsync(approvalIdentifier, attachmentId, telemetry);
                }
            }
        }
        catch (Exception tenantDownloadException)
        {
            _logProvider.LogError(TrackingEvent.DocumentDownloadFailure, tenantDownloadException, logData);
            throw;
        }

        return httpResponseMessage;
    }

    /// <summary>
    /// Bulk Attachment Download : Gets the attachments from the LOB application for the selected requests
    /// </summary>
    /// <param name="tenantId">Tenant Id of the tenant (1/2/3..)</param>
    /// <param name="sessionId">GUID session id</param>
    /// <param name="tcv">GUID transaction correlation vector for telemetry and logging</param>
    /// <param name="requestContent">Request body which is sent to the LoB application as part of the content in the Http call</param>
    /// <param name="userAlias">Alias of the Approver of this request</param>
    /// <param name="loggedInAlias">Logged in User Alias</param>
    /// <param name="clientDevice">Client Device (Web/WP8..)</param>
    /// <param name="authorizationToken">Authorization Token</param>
    /// <returns>HttpResponseMessage with Stream data of all the attachments</returns>
    public async Task<byte[]> GetAllAttachmentsInBulk(int tenantId, string sessionId, string tcv, string requestContent, string userAlias, string loggedInAlias, string clientDevice, string authorizationToken)
    {
        #region Logging Prep

        if (string.IsNullOrEmpty(sessionId))
        {
            sessionId = Guid.NewGuid().ToString();
        }

        if (string.IsNullOrEmpty(tcv))
        {
            tcv = Guid.NewGuid().ToString();
        }

        var logData = new Dictionary<LogDataKey, object>
        {
            { LogDataKey.Tcv, tcv },
            { LogDataKey.SessionId, sessionId },
            { LogDataKey.UserRoleName, loggedInAlias },
            { LogDataKey.TenantId, tenantId },
            { LogDataKey.UserAlias, userAlias },
            { LogDataKey.StartDateTime, DateTime.UtcNow },
            { LogDataKey.IsCriticalEvent, CriticalityLevel.No.ToString() },
            { LogDataKey.ClientDevice, clientDevice }
        };

        #endregion Logging Prep

        try
        {
            #region Getting the Tenant ID

            ApprovalTenantInfo tenantInfo = _approvalTenantInfoHelper.GetTenantInfo(tenantId);
            logData.Add(LogDataKey.BusinessProcessName, string.Format(tenantInfo.BusinessProcessName, Constants.BusinessProcessNameGetDocuments, Constants.BusinessProcessNameUserTriggered));

            #endregion Getting the Tenant ID

            #region Forh the list of Approval Identifiers

            List<ApprovalRequest> approvalRequests = requestContent.FromJson<List<ApprovalRequest>>();

            //Adding Telemetry data to the request
            foreach (var approvalRequest in approvalRequests)
            {
                if (approvalRequest.Telemetry == null)
                {
                    approvalRequest.Telemetry = new ApprovalsTelemetry()
                    {
                        Xcv = approvalRequest.ApprovalIdentifier.DisplayDocumentNumber,
                        Tcv = tcv,
                        BusinessProcessName = string.Format(tenantInfo.BusinessProcessName, Constants.BusinessProcessNameGetDocuments, Constants.BusinessProcessNameUserTriggered)
                    };
                }
            }

            #endregion Forh the list of Approval Identifiers

            #region Get Tenant Type

            ITenant tenantAdaptor = null;

            tenantAdaptor = _tenantFactory.GetTenant(
                                                        tenantInfo,
                                                        userAlias,
                                                        clientDevice,
                                                        authorizationToken);

            #endregion Get Tenant Type

            using (var docDownloadTracer = _performanceLogger.StartPerformanceLogger("PerfLog", string.IsNullOrWhiteSpace(clientDevice) ? Constants.WebClient : clientDevice, string.Format(Constants.PerfLogAction, tenantInfo.AppName, "Bulk Document Download"), logData))
            {
                var response = await tenantAdaptor.BulkDownloadDocumentAsync(approvalRequests, loggedInAlias, sessionId, clientDevice);

                if (response != null)
                {
                    _logProvider.LogInformation(TrackingEvent.WebApiBulkDocumentDownloadSuccess, logData);
                }
                else
                {
                    _logProvider.LogError(TrackingEvent.WebApiBulkDocumentDownloadFail, new Exception(Constants.AttachmentDownloadGenericFailedMessage), logData);
                }
                logData.Modify(LogDataKey.EndDateTime, DateTime.UtcNow);
                _logProvider.LogInformation(TrackingEvent.WebApiBulkDocumentDownloadSuccess, logData);
                return response;
            }
        }
        catch (Exception ex)
        {
            logData.Modify(LogDataKey.EndDateTime, DateTime.UtcNow);
            _logProvider.LogError(TrackingEvent.WebApiBulkDocumentDownloadFail, ex, logData);
            throw new InvalidOperationException("An internal error occurred", ex);
        }
    }

    /// <summary>
    /// Get all documents zipped.
    /// </summary>
    /// <param name="tenantId">Tenant Id of the tenant (1/2/3..)</param>
    /// <param name="documentNumber">Document Number of the request</param>
    /// <param name="displayDocumentNumber">Display Document Number of the request</param>
    /// <param name="fiscalYear">Fiscal year of the request</param>
    /// <param name="attachments">Attachment IDs of the Documents to be downloaded</param>
    /// <param name="sessionId">GUID session id</param>
    /// <param name="tcv">GUID transaction correlation vector for telemetry and logging</param>
    /// <param name="xcv">Cross system correlation vector for telemetry and logging</param>
    /// <param name="userAlias">Alias of the Approver of this request</param>
    /// <param name="loggedInAlias">Logged in User Alias</param>
    /// <param name="clientDevice">Client Device (Web/WP8..)</param>
    /// <param name="aadUserToken">The Azure AD user token</param>
    /// <returns>HttpResponseMessage with Stream data of the attachment</returns>
    public async Task<byte[]> GetAllDocumentsZipped(int tenantId, string documentNumber, string displayDocumentNumber, string fiscalYear, IRequestAttachment[] attachments, string sessionId, string tcv, string xcv, string userAlias, string loggedInAlias, string clientDevice, string aadUserToken)
    {
        #region Logging Prep

        if (string.IsNullOrEmpty(sessionId))
        {
            sessionId = Guid.NewGuid().ToString();
        }

        if (string.IsNullOrEmpty(tcv))
        {
            tcv = Guid.NewGuid().ToString();
        }

        if (string.IsNullOrEmpty(xcv))
        {
            xcv = displayDocumentNumber;
        }

        var logData = new Dictionary<LogDataKey, object>
        {
            { LogDataKey.Tcv, tcv },
            { LogDataKey.SessionId, sessionId },
            { LogDataKey.Xcv, xcv },
            { LogDataKey.DXcv, displayDocumentNumber },
            { LogDataKey.UserRoleName, loggedInAlias },
            { LogDataKey.TenantId, tenantId },
            { LogDataKey.DocumentNumber, displayDocumentNumber },
            { LogDataKey.FiscalYear, fiscalYear },
            { LogDataKey.UserAlias, userAlias },
            { LogDataKey.StartDateTime, DateTime.UtcNow },
            { LogDataKey.IsCriticalEvent, CriticalityLevel.No.ToString() },
            { LogDataKey.ClientDevice, clientDevice }
        };

        #endregion Logging Prep

        try
        {
            #region Getting the Tenant ID

            ApprovalTenantInfo tenantInfo = _approvalTenantInfoHelper.GetTenantInfo(tenantId);
            logData.Add(LogDataKey.BusinessProcessName, string.Format(tenantInfo.BusinessProcessName, Constants.BusinessProcessNameGetDocuments, Constants.BusinessProcessNameUserTriggered));

            #endregion Getting the Tenant ID

            #region Get Tenant Type

            ITenant tenantAdaptor = null;

            tenantAdaptor = _tenantFactory.GetTenant(tenantInfo, userAlias, clientDevice, aadUserToken);

            #endregion Get Tenant Type

            #region Get All available details from ApprovalDetails table

            List<ApprovalDetailsEntity> requestDetails = await _approvalDetailProvider.GetAllApprovalDetailsByTenantAndDocumentNumber(tenantId, displayDocumentNumber);

            // Get the Current Approver row
            var currentApproverInDbJson = requestDetails.FirstOrDefault(r => r.RowKey.Equals(Constants.CurrentApprover, StringComparison.OrdinalIgnoreCase));

            // Get the Complete Approver Chain which has all the previous approvers
            var previousApprovers = requestDetails.FirstOrDefault(r => r.RowKey.Equals(Constants.ApprovalChainOperation, StringComparison.OrdinalIgnoreCase));

            #endregion Get All available details from ApprovalDetails table

            #region Check Permissions

            JObject authResponseObject = new JObject();
            CheckUserPermissions(tenantId, displayDocumentNumber, fiscalYear, sessionId, tcv, xcv, userAlias, loggedInAlias, currentApproverInDbJson, previousApprovers, null, out authResponseObject);
            if (authResponseObject != null && authResponseObject.Count > 0)
            {
                throw new UnauthorizedAccessException("User doesn't have permission to see the report.");
            }

            #endregion Check Permissions

            var approvalIdentifier = new ApprovalIdentifier() { DocumentNumber = documentNumber, DisplayDocumentNumber = displayDocumentNumber, FiscalYear = fiscalYear };
            using (var docDownloadTracer = _performanceLogger.StartPerformanceLogger("PerfLog", Constants.WebClient, string.Format(Constants.PerfLogAction, tenantInfo.AppName, "Document Download All Zipped"), logData))
            {
                using (var memoryStream = new MemoryStream())
                {
                    using (var zip = new ZipArchive(memoryStream, ZipArchiveMode.Create, true))
                    {
                        foreach (IRequestAttachment attachment in attachments)
                        {
                            byte[] response = null;

                            if (attachment.IsPreAttached)
                            {
                                response = await DownloadDocumentAsync(tenantAdaptor,
                                                                              tenantId,
                                                                              tenantInfo,
                                                                              approvalIdentifier,
                                                                              userAlias,
                                                                              attachment.ID,
                                                                              sessionId,
                                                                              loggedInAlias,
                                                                              xcv,
                                                                              tcv);
                            }
                            else
                            {
                                response = await DownloadUserAttachedDocuments(approvalIdentifier.DocumentNumber, attachment.ID, tenantInfo, requestDetails);
                            }


                            if (response != null)
                            {
                                _logProvider.LogInformation(TrackingEvent.WebApiDetailAllDocumentDownloadSuccess, logData);
                                // add the item name to the zip
                                ZipArchiveEntry zipItem = zip.CreateEntry(attachment.Name);

                                using (var entryStream = zipItem.Open())
                                {
                                    entryStream.Write(response, 0, response.Length);
                                }

                                logData.Modify(LogDataKey.EndDateTime, DateTime.UtcNow);
                            }
                            else
                            {
                                _logProvider.LogError(TrackingEvent.WebApiDetailAllDocumentDownloadFail, new Exception(Constants.AttachmentDownloadGenericFailedMessage), logData);
                            }
                        }
                    }
                    return memoryStream.ToArray();
                }
            }
        }
        catch (UnauthorizedAccessException authEx)
        {
            LogMessageProgress(TrackingEvent.WebApiDetailAllDocumentDownloadFail, new FailureData() { Message = authEx.Message }, CriticalityLevel.Yes, logData);
            _logProvider.LogError(TrackingEvent.WebApiDetailDocumentDownloadFail, authEx, logData);
            throw;
        }
        catch (Exception ex)
        {
            _logProvider.LogError(TrackingEvent.WebApiDetailAllDocumentDownloadFail, ex, logData);
            throw new InvalidOperationException("An internal error occurred", ex);
        }
    }

    /// <summary>
    /// Get Detail Async.
    /// </summary>
    /// <param name="tenantAdaptor">The tenant adaptor</param>
    /// <param name="tenantId">Tenant Id of the tenant (1/2/3..)</param>
    /// <param name="tenantInfo">ApprovalTenantInfo object for the current tenant</param>
    /// <param name="documentNumber">Document Number of the request</param>
    /// <param name="operation">Operation type (DT1/LINE etc.)</param>
    /// <param name="alias">Alias of the Approver of this request</param>
    /// <param name="loggedInAlias">Logged in User Alias</param>
    /// <param name="sessionId">GUID session id</param>
    /// <param name="fiscalYear">Fiscal year of the request</param>
    /// <param name="page">Page number</param>
    /// <param name="xcv">Cross system correlation vector for telemetry and logging</param>
    /// <param name="tcv">GUID transaction correlation vector for telemetry and logging</param>
    /// <param name="clientDevice">Client Device (Web/WP8..)</param>
    /// <returns>HttpResponseMessage with details data of the request</returns>
    public async Task<HttpResponseMessage> GetDetailAsync(ITenant tenantAdaptor, int tenantId, ApprovalTenantInfo tenantInfo, string documentNumber, string operation, string alias, string loggedInAlias, string sessionId, string fiscalYear, int page, string xcv, string tcv, string clientDevice)
    {
        #region Logging Prep

        var logData = new Dictionary<LogDataKey, object>
        {
            { LogDataKey.Tcv, tcv },
            { LogDataKey.ReceivedTcv, tcv },
            { LogDataKey.SessionId, sessionId },
            { LogDataKey.Xcv, xcv },
            { LogDataKey.UserRoleName, loggedInAlias },
            { LogDataKey.BusinessProcessName, string.Format(tenantInfo.BusinessProcessName, Constants.BusinessProcessNameGetDetailsFromTenant, Constants.BusinessProcessNameUserTriggered) },
            { LogDataKey.TenantId, tenantId },
            { LogDataKey.DocumentNumber, documentNumber },
            { LogDataKey.FiscalYear, fiscalYear },
            { LogDataKey.Operation, operation },
            { LogDataKey.UserAlias, alias },
            { LogDataKey.IsCriticalEvent, CriticalityLevel.No.ToString() }
        };

        #endregion Logging Prep

        HttpResponseMessage httpResponseMessage = null;

        try
        {
            using (var detailRowsInsertTracer = _performanceLogger.StartPerformanceLogger("PerfLog", string.IsNullOrWhiteSpace(clientDevice) ? Constants.WebClient : clientDevice, string.Format(Constants.PerfLogActionWithInfo, tenantInfo.AppName, operation, "Time to complete"), logData))
            {
                httpResponseMessage = await tenantAdaptor.LoadDetailAsync(tenantInfo, new ApprovalIdentifier() { DocumentNumber = documentNumber, DisplayDocumentNumber = documentNumber, FiscalYear = fiscalYear }, operation, page, loggedInAlias, xcv, tcv, clientDevice);
            }
        }
        catch (Exception ex)
        {
            _logProvider.LogError(TrackingEvent.DetailFetchFailure, ex, logData);
            throw;
        }

        return httpResponseMessage;
    }

    /// <summary>
    /// This method gets the details of the request
    /// </summary>
    /// <param name="tenantId">Tenant Id of the tenant (1/2/3..)</param>
    /// <param name="documentNumber">Document Number of the request</param>
    /// <param name="operation">Operation type (DT1/LINE etc.)</param>
    /// <param name="fiscalYear">Fiscal year of the request</param>
    /// <param name="page">Page number</param>
    /// <param name="sessionId">GUID session id</param>
    /// <param name="tcv">GUID transaction correlation vector for telemetry and logging</param>
    /// <param name="xcv">Cross system correlation vector for telemetry and logging</param>
    /// <param name="userAlias">Alias of the Approver of this request</param>
    /// <param name="loggedInAlias">Logged in User Alias</param>
    /// <param name="clientDevice">Client Device (Web/WP8..)</param>
    /// <param name="aadUserToken">The Azure AD user token</param>
    /// <param name="isWorkerTriggered">To understand if Worker role has triggered the details fetch</param>
    /// <param name="sectionType">section type. eg. Summary Details</param>
    /// <param name="pageType">This the page calling the Details API e.g. Detail, History</param>
    /// <param name="source">Source for Details call eg. Summary, Notification</param>
    /// <returns>Details of the request as a Task of JObject</returns>
    public async Task<JObject> GetDetails(int tenantId, string documentNumber, string operation, string fiscalYear, int page, string sessionId, string tcv, string xcv, string userAlias, string loggedInAlias, string clientDevice, string aadUserToken, bool isWorkerTriggered, int sectionType, string pageType, string source)
    {
        #region Logging Prep

        if (string.IsNullOrEmpty(sessionId))
        {
            sessionId = Guid.NewGuid().ToString();
        }

        if (string.IsNullOrEmpty(tcv))
        {
            tcv = Guid.NewGuid().ToString();
        }

        if (string.IsNullOrEmpty(xcv))
        {
            xcv = documentNumber;
        }

        var logData = new Dictionary<LogDataKey, object>
        {
            { LogDataKey.Tcv, tcv },
            { LogDataKey.SessionId, sessionId },
            { LogDataKey.Xcv, xcv },
            { LogDataKey.DXcv, documentNumber },
            { LogDataKey.UserRoleName, loggedInAlias },
            { LogDataKey.TenantId, tenantId },
            { LogDataKey.DocumentNumber, documentNumber },
            { LogDataKey.FiscalYear, fiscalYear },
            { LogDataKey.Operation, operation },
            { LogDataKey.UserAlias, userAlias },
            { LogDataKey.IsCriticalEvent, CriticalityLevel.No.ToString() },
            { LogDataKey.ClientDevice, clientDevice },
            { LogDataKey.ReceivedTcv, tcv },
            { LogDataKey.Approver, userAlias },
            { LogDataKey.StartDateTime, DateTime.UtcNow },
            { LogDataKey.DisplayDocumentNumber, documentNumber }
        };

        #endregion Logging Prep

        try
        {
            #region Getting the Tenant ID

            ApprovalTenantInfo tenantInfo = _approvalTenantInfoHelper.GetTenantInfo(tenantId);
            if (isWorkerTriggered)
            {
                logData.Add(LogDataKey.BusinessProcessName, string.Format(tenantInfo.BusinessProcessName, Constants.BusinessProcessNameGetDetails, Constants.BusinessProcessNameDetailsWorkerTriggered));
            }
            else
            {
                logData.Add(LogDataKey.BusinessProcessName, string.Format(tenantInfo.BusinessProcessName, Constants.BusinessProcessNameGetDetails, Constants.BusinessProcessNameUserTriggered));
            }
            logData[LogDataKey.TenantName] = tenantInfo.AppName;
            logData[LogDataKey.DocumentTypeId] = tenantInfo.DocTypeId;

            #endregion Getting the Tenant ID

            #region Get Tenant Type

            ITenant tenantAdaptor = null;
            tenantAdaptor = _tenantFactory.GetTenant(
                    tenantInfo,
                    userAlias,
                    clientDevice,
                    aadUserToken);

            #endregion Get Tenant Type

            JObject responseJObject = null;
            LogMessageProgress(TrackingEvent.UserTriggeredDetailInitiated, null, CriticalityLevel.Yes, logData);

            using (var detailsTracer = _performanceLogger.StartPerformanceLogger("PerfLog", string.IsNullOrWhiteSpace(clientDevice) ? Constants.WebClient : clientDevice, string.Format(Constants.PerfLogActionWithInfo, tenantInfo.AppName, operation, "Detail"), logData))
            {
                if (operation.ToUpper() == Constants.AuthSumOperationType.ToUpper())
                {
                    #region AuthSum

                    using (var authsumTracer = _performanceLogger.StartPerformanceLogger("PerfLog", string.IsNullOrWhiteSpace(clientDevice) ? Constants.WebClient : clientDevice, string.Format(Constants.PerfLogAction, tenantInfo.AppName, "Authsum operation"), logData))
                    {
                        List<ApprovalDetailsEntity> requestDetails = new List<ApprovalDetailsEntity>();
                        if (sectionType == (int)DataCallType.Summary)
                        {
                            List<string> operationList = tenantAdaptor.GetSummaryOperationTypes();
                            foreach (var op in operationList)
                            {
                                var requestDetail = await _approvalDetailProvider.GetApprovalDetailsByOperation(tenantId, documentNumber, op);
                                if (requestDetail != null)
                                    requestDetails.Add(requestDetail);
                            }
                        }
                        if (sectionType == (int)DataCallType.Details || sectionType == (int)DataCallType.All)
                        {
                            requestDetails = await _approvalDetailProvider.GetAllApprovalDetailsByTenantAndDocumentNumber(tenantId, documentNumber);
                        }

                        // Get the Current Approver row
                        var currentApproverInDbJson = requestDetails.FirstOrDefault(r => r.RowKey.Equals(Constants.CurrentApprover, StringComparison.OrdinalIgnoreCase));

                        // Get the Complete Approver Chain which has all the previous approvers
                        var previousApprovers = requestDetails.FirstOrDefault(r => r.RowKey.Equals(Constants.ApprovalChainOperation, StringComparison.OrdinalIgnoreCase));

                        #region Get Prerequisites - Summary

                        // Get the List of ApprovalSumamryRow
                        ApprovalSummaryRow documentSummary = null;

                        var attachmentProperties = tenantInfo.IsUploadAttachmentsEnabled ? tenantInfo?.AttachmentProperties?.FromJson<AttachmentProperties>() : null;

                        var documentSummaries = requestDetails.FirstOrDefault(r => r.RowKey.Equals(Constants.SummaryOperationType, StringComparison.OrdinalIgnoreCase));
                        if (documentSummaries != null)
                        {
                            // Getting document summary for the pending request
                            var documentSummariesJson = documentSummaries.JSONData.FromJson<List<ApprovalSummaryRow>>();
                            documentSummary = documentSummariesJson.FirstOrDefault(x => x.PartitionKey.Equals(userAlias, StringComparison.OrdinalIgnoreCase));

                            var userAttachmentsParameter = requestDetails.FirstOrDefault(r => r.RowKey.Equals(Constants.AttachmentsOperationType, StringComparison.OrdinalIgnoreCase));

                            if (userAttachmentsParameter != null)
                            {
                                var userAttachments = JsonConvert.DeserializeObject<Attachment[]>(userAttachmentsParameter?.JSONData.ToString());
                                ConsolidateAttachments(userAttachments, documentSummary);
                            }
                        }

                        List<ApprovalSummaryRow> documentSummariesObj = new List<ApprovalSummaryRow>();
                        // Get the Summary data from ApprovalSummary table if the ApprovalDetails table do not have the required details
                        if (documentSummary == null && currentApproverInDbJson != null)
                        {
                            var currentApproversInDb = currentApproverInDbJson.JSONData.FromJson<List<Approver>>();
                            var approver = currentApproversInDb.Where(t => t.Alias == userAlias).ToList().FirstOrDefault();

                            // Condition to check that user is current approver but SUM record not present in DB
                            if (approver != null)
                            {
                                documentSummary = _summaryHelper.GetSummaryData(documentNumber, fiscalYear, tenantInfo, approver.Alias, loggedInAlias, xcv, tcv, tenantAdaptor);
                                if (documentSummaries == null)
                                {
                                    documentSummariesObj = new List<ApprovalSummaryRow>() { documentSummary };
                                }
                                else
                                {
                                    documentSummariesObj = documentSummaries.JSONData.FromJson<List<ApprovalSummaryRow>>();
                                    documentSummariesObj.Add(documentSummary);
                                }
                            }
                            else
                            {
                                // Logic for fetching summary for historical data
                                documentSummariesObj = new List<ApprovalSummaryRow>();
                                if (documentSummaries == null)
                                {
                                    foreach (var currentAppr in currentApproversInDb)
                                    {
                                        documentSummary = _summaryHelper.GetSummaryData(documentNumber, fiscalYear, tenantInfo, currentApproversInDb.FirstOrDefault().Alias, loggedInAlias, xcv, tcv, tenantAdaptor);
                                        documentSummariesObj.Add(documentSummary);
                                    }
                                }
                                else
                                {
                                    documentSummariesObj = documentSummaries.JSONData.FromJson<List<ApprovalSummaryRow>>();
                                    documentSummary = documentSummariesObj.FirstOrDefault();
                                }
                            }

                            // Add SUM details into ApprovalDetails Table
                            ApprovalDetailsEntity approvalDetailsEntity = new ApprovalDetailsEntity()
                            {
                                PartitionKey = documentSummary.DocumentNumber,
                                RowKey = Constants.SummaryOperationType,
                                ETag = global::Azure.ETag.All,
                                JSONData = documentSummariesObj.ToJson(),
                                TenantID = int.Parse(tenantInfo.RowKey)
                            };
                            ApprovalsTelemetry telemetry = new ApprovalsTelemetry()
                            {
                                Xcv = xcv,
                                Tcv = tcv,
                                BusinessProcessName = isWorkerTriggered ? string.Format(tenantInfo.BusinessProcessName, Constants.BusinessProcessNameAddSummaryCopy, Constants.BusinessProcessNameDetailsWorkerTriggered)
                                                        : string.Format(tenantInfo.BusinessProcessName, Constants.BusinessProcessNameAddSummaryCopy, Constants.BusinessProcessNameUserTriggered)
                            };
                            _approvalDetailProvider.AddTransactionalAndHistoricalDataInApprovalsDetails(approvalDetailsEntity, tenantInfo, telemetry);
                        }

                        var summaryDataList = documentSummaries != null ? documentSummaries.JSONData.FromJson<List<ApprovalSummaryRow>>() : documentSummariesObj;

                        #endregion Get Prerequisites - Summary

                        #region Check Permissions

                        List<TransactionHistoryExt> documentHistoryExts = CheckUserPermissions(tenantId, documentNumber, fiscalYear, sessionId, tcv, xcv, userAlias, loggedInAlias, currentApproverInDbJson, previousApprovers, summaryDataList, out JObject authResponseObject);

                        if (authResponseObject != null && authResponseObject.Count > 0)
                        {
                            return authResponseObject;
                        }

                        #endregion Check Permissions

                        responseJObject = await AuthSum(
                                tenantAdaptor,
                                requestDetails,
                                tenantId,
                                tenantInfo,
                                documentSummary,
                                documentHistoryExts,
                                currentApproverInDbJson,
                                documentNumber,
                                operation,
                                userAlias,
                                loggedInAlias,
                                sessionId,
                                fiscalYear,
                                page,
                                xcv,
                                tcv,
                                isWorkerTriggered,
                                sectionType,
                                clientDevice,
                                aadUserToken);

                        if (responseJObject == null)
                        {
                            throw new InvalidOperationException("An exception occurred while getting AuthSum");
                        }

                        if (responseJObject.ContainsKey("TenantId"))
                        {
                            responseJObject["TenantId"] = tenantInfo.RowKey;
                        }
                        else
                        {
                            responseJObject.Add("TenantId", tenantInfo.RowKey);
                        }

                        if ((tenantInfo?.ActionAdditionalPropertiesList != null) && (tenantInfo.ActionAdditionalPropertiesList.Any()))
                        {
                            responseJObject.Add("ActionAdditionalProperties", tenantInfo?.ActionAdditionalPropertiesList?.ToJToken());
                        }
                        if (currentApproverInDbJson != null)
                        {
                            responseJObject.Add("CurrentApprovers", string.Join(",", currentApproverInDbJson?.JSONData?.FromJson<List<Approver>>()?.Select(s => s.Name)));
                        }


                        if (responseJObject.ContainsKey("FileAttachmentOptions"))
                        {
                            responseJObject["FileAttachmentOptions"] = attachmentProperties?.FileAttachmentOptions.ToJToken();
                        }
                        else
                        {
                            responseJObject.Add("FileAttachmentOptions", attachmentProperties?.FileAttachmentOptions.ToJToken());
                        }

                        responseJObject["SessionId"] = Guid.NewGuid().ToString();

                        logData.Modify(LogDataKey.EndDateTime, DateTime.UtcNow);
                        LogMessageProgress(TrackingEvent.UserTriggeredDetailSuccessful, null, CriticalityLevel.Yes, logData);

                        if (clientDevice.Equals(Constants.TeamsClient, StringComparison.InvariantCultureIgnoreCase))
                        {
                            FetchMissingDetailsDataFromLOB(responseJObject, tenantId, documentNumber, fiscalYear, sessionId, tcv, xcv, userAlias, loggedInAlias, clientDevice, aadUserToken, sectionType, pageType, source);
                        }

                        logData.Modify(LogDataKey.EndDateTime, DateTime.UtcNow);
                        _logProvider.LogInformation(TrackingEvent.WebApiDetailSuccess, logData);
                        return responseJObject;
                    }

                    #endregion AuthSum
                }
                else
                {
                    //// For all other operations

                    #region Get Details

                    HttpResponseMessage httpResponseMessage = null;

                    using (var authsumTracer = _performanceLogger.StartPerformanceLogger("PerfLog", string.IsNullOrWhiteSpace(clientDevice) ? Constants.WebClient : clientDevice, string.Format(Constants.PerfLogAction, tenantInfo.AppName, operation), logData))
                    {
                        httpResponseMessage = await GetDetailAsync(
                                tenantAdaptor,
                                tenantId,
                                tenantInfo,
                                documentNumber,
                                operation,
                                userAlias,
                                loggedInAlias,
                                sessionId,
                                fiscalYear,
                                page,
                                xcv,
                                tcv,
                                clientDevice);

                        string detail = await httpResponseMessage.Content.ReadAsStringAsync();
                        responseJObject = detail.ToJObject();

                        logData[LogDataKey.IsCriticalEvent] = CriticalityLevel.Yes.ToString();

                        LogMessageProgress(TrackingEvent.UserTriggeredDetailSuccessful, null, CriticalityLevel.Yes, logData);
                        _logProvider.LogInformation(TrackingEvent.WebApiDetailSuccess, logData);

                        if (responseJObject.ContainsKey("TenantId"))
                        {
                            responseJObject["TenantId"] = tenantInfo.RowKey;
                        }
                        else
                        {
                            responseJObject.Add("TenantId", tenantInfo.RowKey);
                        }

                        if ((tenantInfo?.ActionAdditionalPropertiesList != null) && (tenantInfo.ActionAdditionalPropertiesList.Any()))
                        {
                            responseJObject.Add("ActionAdditionalProperties", tenantInfo?.ActionAdditionalPropertiesList?.ToJToken());
                        }

                        logData.Modify(LogDataKey.EndDateTime, DateTime.UtcNow);
                        _logProvider.LogInformation(TrackingEvent.WebApiDetailSuccess, logData);
                        return responseJObject;
                    }

                    #endregion Get Details
                }
            }
        }
        catch (UnauthorizedAccessException authEx)
        {
            LogMessageProgress(TrackingEvent.UserTriggeredDetailFail, new FailureData() { Message = authEx.Message }, CriticalityLevel.Yes, logData);
            _logProvider.LogError(TrackingEvent.WebApiDetailFail, authEx, logData);
            throw;
        }
        catch (Exception ex)
        {
            LogMessageProgress(TrackingEvent.UserTriggeredDetailFail, new FailureData() { Message = ex.Message }, CriticalityLevel.Yes, logData);
            _logProvider.LogError(TrackingEvent.WebApiDetailFail, ex, logData);
            throw new InvalidOperationException(_config[ConfigurationKey.DetailControllerExceptionMessage.ToString()]);
        }
    }

    /// <summary>
    /// Consolidate the pre attached files with the user attached documents.
    /// </summary>
    /// <param name="userAttachments">User attached documents.</param>
    /// <param name="documentSummary">Document summary that have the list of attachments that is pre attached from tenant.</param>
    private void ConsolidateAttachments(Attachment[] userAttachments, ApprovalSummaryRow documentSummary)
    {
        var summaryJson = documentSummary?.SummaryJson?.FromJson<SummaryJson>();
        if (summaryJson != null && userAttachments != null)
        {
            foreach (var userAttachment in userAttachments)
            {
                if (summaryJson.Attachments == null)
                {
                    summaryJson.Attachments = new List<Attachment>();

                }

                summaryJson.Attachments.Add(new Attachment() { ID = userAttachment.ID, Name = userAttachment.Name, IsPreAttached = userAttachment.IsPreAttached });
            }

            documentSummary.SummaryJson = summaryJson.ToJson();
        }

    }

    /// <summary>
    /// This method gets the Attachment content for preview
    /// </summary>
    /// <param name="tenantId">Tenant Id of the tenant (1/2/3..)</param>
    /// <param name="documentNumber">Document Number of the request</param>
    /// <param name="displayDocumentNumber">Display Document Number of the request</param>
    /// <param name="fiscalYear">Fiscal year of the request</param>
    /// <param name="attachmentId">Attachment ID of the Document to be downloaded</param>
    /// <param name="isPreAttached">Identify if the attachment is provided by the user or the tenant.</param>
    /// <param name="sessionId">GUID session id</param>
    /// <param name="tcv">GUID transaction correlation vector for telemetry and logging</param>
    /// <param name="xcv">Cross system correlation vector for telemetry and logging</param>
    /// <param name="userAlias">Alias of the Approver of this request</param>
    /// <param name="loggedInAlias">Logged in User Alias</param>
    /// <param name="clientDevice">Client Device (Web/WP8..)</param>
    /// <param name="aadUserToken">The Azure AD user token</param>
    /// <returns>HttpResponseMessage with Stream data of the attachment</returns>
    public async Task<byte[]> GetDocumentPreview(int tenantId, string documentNumber, string displayDocumentNumber, string fiscalYear, string attachmentId, bool isPreAttached, string sessionId, string tcv, string xcv, string userAlias, string loggedInAlias, string clientDevice, string aadUserToken)
    {
        #region Logging Prep

        if (string.IsNullOrEmpty(sessionId))
        {
            sessionId = Guid.NewGuid().ToString();
        }

        if (string.IsNullOrEmpty(tcv))
        {
            tcv = Guid.NewGuid().ToString();
        }

        if (string.IsNullOrEmpty(xcv))
        {
            xcv = displayDocumentNumber;
        }

        var logData = new Dictionary<LogDataKey, object>
        {
            { LogDataKey.Tcv, tcv },
            { LogDataKey.SessionId, sessionId },
            { LogDataKey.Xcv, xcv },
            { LogDataKey.DXcv, displayDocumentNumber },
            { LogDataKey.UserRoleName, loggedInAlias },
            { LogDataKey.TenantId, tenantId },
            { LogDataKey.DocumentNumber, displayDocumentNumber },
            { LogDataKey.FiscalYear, fiscalYear },
            { LogDataKey.UserAlias, userAlias },
            { LogDataKey.StartDateTime, DateTime.UtcNow },
            { LogDataKey.IsCriticalEvent, CriticalityLevel.No.ToString() },
            { LogDataKey.ClientDevice, clientDevice }
        };

        #endregion Logging Prep

        try
        {
            #region Getting the Tenant ID

            ApprovalTenantInfo tenantInfo = _approvalTenantInfoHelper.GetTenantInfo(tenantId);
            logData.Add(LogDataKey.BusinessProcessName, string.Format(tenantInfo.BusinessProcessName, Constants.BusinessProcessNameGetDocuments, Constants.BusinessProcessNameUserTriggered));

            #endregion Getting the Tenant ID

            #region Get Tenant Type

            ITenant tenantAdaptor = null;

            tenantAdaptor = _tenantFactory.GetTenant(
                                                        tenantInfo,
                                                        userAlias,
                                                        clientDevice,
                                                        aadUserToken);

            #endregion Get Tenant Type

            #region Get All available details from ApprovalDetails table

            List<ApprovalDetailsEntity> requestDetails = await _approvalDetailProvider.GetAllApprovalDetailsByTenantAndDocumentNumber(tenantId, displayDocumentNumber);

            // Get the Current Approver row
            var currentApproverInDbJson = requestDetails.FirstOrDefault(r => r.RowKey.Equals(Constants.CurrentApprover, StringComparison.OrdinalIgnoreCase));

            // Get the Complete Approver Chain which has all the previous approvers
            var previousApprovers = requestDetails.FirstOrDefault(r => r.RowKey.Equals(Constants.ApprovalChainOperation, StringComparison.OrdinalIgnoreCase));

            #endregion Get All available details from ApprovalDetails table

            #region Check Permissions

            JObject authResponseObject = new JObject();
            CheckUserPermissions(tenantId, displayDocumentNumber, fiscalYear, sessionId, tcv, xcv, userAlias, loggedInAlias, currentApproverInDbJson, previousApprovers, null, out authResponseObject);
            if (authResponseObject != null && authResponseObject.Count > 0)
            {
                throw new UnauthorizedAccessException("User doesn't have permission to see the report.");
            }

            #endregion Check Permissions


            byte[] response = null;

            if (isPreAttached == false)
            {
                response = await DownloadUserAttachedDocuments(documentNumber, attachmentId, tenantInfo, requestDetails);
            }
            else
            {
                var approvalIdentifier = new ApprovalIdentifier() { DocumentNumber = documentNumber, DisplayDocumentNumber = displayDocumentNumber, FiscalYear = fiscalYear };
                using (var docDownloadTracer = _performanceLogger.StartPerformanceLogger("PerfLog", Constants.WebClient, string.Format(Constants.PerfLogAction, tenantInfo.AppName, "Document Download"), logData))
                {
                    response = await PreviewDocumentAsync(tenantAdaptor,
                                                             tenantId,
                                                             tenantInfo,
                                                             approvalIdentifier,
                                                             userAlias,
                                                             attachmentId,
                                                             sessionId,
                                                             loggedInAlias,
                                                             xcv,
                                                             tcv);

                    logData.Modify(LogDataKey.EndDateTime, DateTime.UtcNow);
                    if (response != null)
                    {
                        _logProvider.LogInformation(TrackingEvent.WebApiDetailPreviewDocumentSuccess, logData);
                    }
                    else
                    {
                        _logProvider.LogError(TrackingEvent.WebApiDetailPreviewDocumentFail, new Exception(Constants.AttachmentDownloadGenericFailedMessage), logData);
                    }
                }
            }

            return response;
        }
        catch (UnauthorizedAccessException authEx)
        {
            LogMessageProgress(TrackingEvent.WebApiDetailPreviewDocumentFail, new FailureData() { Message = authEx.Message }, CriticalityLevel.Yes, logData);
            _logProvider.LogError(TrackingEvent.WebApiDetailPreviewDocumentFail, authEx, logData);
            throw;
        }
        catch (Exception ex)
        {
            _logProvider.LogError(TrackingEvent.WebApiDetailPreviewDocumentFail, ex, logData);
            throw new InvalidOperationException("An internal error occurred", ex);
        }
    }

    /// <summary>
    /// Download the user attached douments from the blob storage per tenant.
    /// </summary>
    /// <param name="documentNumber">Document number of the approval request.</param>
    /// <param name="attachmentId">Attachment Id for the document.</param>
    /// <param name="tenantInfo">Tenant information.</param>
    /// <param name="requestDetails">request details for the approval request.</param>
    /// <returns>File in byte array.</returns>
    private async Task<byte[]> DownloadUserAttachedDocuments(string documentNumber, string attachmentId, ApprovalTenantInfo tenantInfo, List<ApprovalDetailsEntity> requestDetails)
    {
        List<Attachment> attachmentsSummary = new List<Attachment>();
        if (requestDetails != null && requestDetails.Any())
        {
            // Filter to get only the row which has TransactionalDetails 
            var existingAttachmentsRecord = requestDetails.FirstOrDefault(x => x.RowKey.Equals(Constants.AttachmentsOperationType, StringComparison.InvariantCultureIgnoreCase));

            if (existingAttachmentsRecord != null)
            {
                attachmentsSummary = JsonConvert.DeserializeObject<List<Attachment>>(existingAttachmentsRecord?.JSONData);
            }

            var attachmentFileName = attachmentsSummary.Where(x => x.ID.Equals(attachmentId, StringComparison.InvariantCultureIgnoreCase))?.FirstOrDefault()?.Name;
            var attachmentProperties = tenantInfo.IsUploadAttachmentsEnabled ? tenantInfo?.AttachmentProperties?.FromJson<AttachmentProperties>() : null;

            if (!string.IsNullOrEmpty(documentNumber) && !string.IsNullOrEmpty(attachmentFileName))
            {
                var response = await _blobStorageHelper.DownloadStreamData(attachmentProperties?.AttachmentContainerName, $"{documentNumber}/{attachmentFileName}");
                return response?.ToArray();
            }
        }

        return null;
    }

    /// <summary>
    /// This method gets the Attachment content
    /// </summary>
    /// <param name="tenantId">Tenant Id of the tenant (1/2/3..)</param>
    /// <param name="documentNumber">Document Number of the request</param>
    /// <param name="displayDocumentNumber">Display Document Number of the request</param>
    /// <param name="fiscalYear">Fiscal year of the request</param>
    /// <param name="attachmentId">Attachment ID of the Document to be downloaded</param>
    /// <param name="sessionId">GUID session id</param>
    /// <param name="tcv">GUID transaction correlation vector for telemetry and logging</param>
    /// <param name="xcv">Cross system correlation vector for telemetry and logging</param>
    /// <param name="userAlias">Alias of the Approver of this request</param>
    /// <param name="loggedInAlias">Logged in User Alias</param>
    /// <param name="clientDevice">Client Device (Web/WP8..)</param>
    /// <param name="aadUserToken">The Azure AD user token</param>
    /// <returns>HttpResponseMessage with Stream data of the attachment</returns>
    public async Task<byte[]> GetDocuments(int tenantId, string documentNumber, string displayDocumentNumber, string fiscalYear, string attachmentId, bool IsPreAttached, string sessionId, string tcv, string xcv, string userAlias, string loggedInAlias, string clientDevice, string aadUserToken)
    {
        #region Logging Prep

        if (string.IsNullOrEmpty(sessionId))
        {
            sessionId = Guid.NewGuid().ToString();
        }

        if (string.IsNullOrEmpty(tcv))
        {
            tcv = Guid.NewGuid().ToString();
        }

        if (string.IsNullOrEmpty(xcv))
        {
            xcv = displayDocumentNumber;
        }

        var logData = new Dictionary<LogDataKey, object>
        {
            { LogDataKey.Tcv, tcv },
            { LogDataKey.SessionId, sessionId },
            { LogDataKey.Xcv, xcv },
            { LogDataKey.DXcv, displayDocumentNumber },
            { LogDataKey.UserRoleName, loggedInAlias },
            { LogDataKey.TenantId, tenantId },
            { LogDataKey.DocumentNumber, displayDocumentNumber },
            { LogDataKey.FiscalYear, fiscalYear },
            { LogDataKey.UserAlias, userAlias },
            { LogDataKey.StartDateTime, DateTime.UtcNow },
            { LogDataKey.IsCriticalEvent, CriticalityLevel.No.ToString() },
            { LogDataKey.ClientDevice, clientDevice }
        };

        #endregion Logging Prep

        try
        {
            #region Getting the Tenant ID

            ApprovalTenantInfo tenantInfo = _approvalTenantInfoHelper.GetTenantInfo(tenantId);
            logData.Add(LogDataKey.BusinessProcessName, string.Format(tenantInfo.BusinessProcessName, Constants.BusinessProcessNameGetDocuments, Constants.BusinessProcessNameUserTriggered));

            #endregion Getting the Tenant ID

            #region Get Tenant Type

            ITenant tenantAdaptor = null;

            tenantAdaptor = _tenantFactory.GetTenant(
                                                        tenantInfo,
                                                        userAlias,
                                                        clientDevice,
                                                        aadUserToken);

            #endregion Get Tenant Type

            #region Get All available details from ApprovalDetails table

            List<ApprovalDetailsEntity> requestDetails = await _approvalDetailProvider.GetAllApprovalDetailsByTenantAndDocumentNumber(tenantId, displayDocumentNumber);

            // Get the Current Approver row
            var currentApproverInDbJson = requestDetails.FirstOrDefault(r => r.RowKey.Equals(Constants.CurrentApprover, StringComparison.OrdinalIgnoreCase));

            // Get the Complete Approver Chain which has all the previous approvers
            var previousApprovers = requestDetails.FirstOrDefault(r => r.RowKey.Equals(Constants.ApprovalChainOperation, StringComparison.OrdinalIgnoreCase));

            #endregion Get All available details from ApprovalDetails table

            #region Check Permissions

            JObject authResponseObject = new JObject();
            CheckUserPermissions(tenantId, displayDocumentNumber, fiscalYear, sessionId, tcv, xcv, userAlias, loggedInAlias, currentApproverInDbJson, previousApprovers, null, out authResponseObject);
            if (authResponseObject != null && authResponseObject.Count > 0)
            {
                throw new UnauthorizedAccessException("User doesn't have permission to see the report.");
            }

            #endregion Check Permissions

            byte[] response = null;

            if (IsPreAttached == false)
            {
                response = await DownloadUserAttachedDocuments(documentNumber, attachmentId, tenantInfo, requestDetails); ;
            }
            else
            {
                var approvalIdentifier = new ApprovalIdentifier() { DocumentNumber = documentNumber, DisplayDocumentNumber = displayDocumentNumber, FiscalYear = fiscalYear };
                using (var docDownloadTracer = _performanceLogger.StartPerformanceLogger("PerfLog", string.IsNullOrWhiteSpace(clientDevice) ? Constants.WebClient : clientDevice, string.Format(Constants.PerfLogAction, tenantInfo.AppName, "Document Download"), logData))
                {
                    response = await DownloadDocumentAsync(tenantAdaptor,
                                                              tenantId,
                                                              tenantInfo,
                                                              approvalIdentifier,
                                                              userAlias,
                                                              attachmentId,
                                                              sessionId,
                                                              loggedInAlias,
                                                              xcv,
                                                              tcv);

                    logData.Modify(LogDataKey.EndDateTime, DateTime.UtcNow);
                    if (response != null)
                    {
                        _logProvider.LogInformation(TrackingEvent.WebApiDetailDocumentDownloadSuccess, logData);
                    }
                    else
                    {
                        _logProvider.LogError(TrackingEvent.WebApiDetailDocumentDownloadFail, new Exception(Constants.AttachmentDownloadGenericFailedMessage), logData);
                    }
                }
            }

            return response;
        }
        catch (UnauthorizedAccessException authEx)
        {
            LogMessageProgress(TrackingEvent.WebApiDetailDocumentDownloadFail, new FailureData() { Message = authEx.Message }, CriticalityLevel.Yes, logData);
            _logProvider.LogError(TrackingEvent.WebApiDetailDocumentDownloadFail, authEx, logData);
            throw;
        }
        catch (Exception ex)
        {
            _logProvider.LogError(TrackingEvent.WebApiDetailDocumentDownloadFail, ex, logData);
            throw new InvalidOperationException("An internal error occurred", ex);
        }
    }

    /// <summary>
    /// Get User Image.
    /// </summary>
    /// <param name="alias"></param>
    /// <param name="SessionId"></param>
    /// <param name="clientDevice"></param>
    /// <param name="logData"></param>
    /// <returns></returns>
    public async Task<string> GetUserImage(string alias, string SessionId, string clientDevice, Dictionary<LogDataKey, object> logData)
    {
        string base64String = clientDevice switch
        {
            Constants.TeamsClient => string.Format("{0},{1}", "data:image/svg+xml;base64", Constants.DefaultTeamsUserImageBase64String),
            _ => string.Format("{0},{1}", "data:image/jpeg;base64", Constants.DefaultUserImageBase64String),
        };
        byte[] photo = null;

        try
        {
            if (!string.IsNullOrEmpty(alias))
            {
                // Here added telemetry in catch block with more details along with documentnumber.
                // We will deploy this code into production and keep watching this exception.
                // So we will get more AI information with documentNumber.
                // So we can check details data in azure table for that documentNumber to identify rootcause. Why submitter alias coming null for that documentNumber.
                photo = await _imageRetriever.GetUserImageAsync(alias, SessionId, clientDevice);
                if (photo != null)
                {
                    // Convert the Byte Array to Base64 Encoded string.
                    base64String = Convert.ToBase64String(photo, 0, photo.Length);
                }
            }
            else
            {
                _logProvider.LogError(TrackingEvent.WebApiUserImageFail, new InvalidDataException("Alias can not be an empty"), logData);
            }
        }
        catch (Exception ex)
        {
            _logProvider.LogError(TrackingEvent.WebApiUserImageFail, ex, logData);
        }
        finally
        {
            if (photo == null)
            {
                base64String = clientDevice switch
                {
                    Constants.TeamsClient => string.Format("{0},{1}", "data:image/svg+xml;base64", Constants.DefaultTeamsUserImageBase64String),
                    _ => Constants.DefaultUserImageBase64String,
                };
            }
        }

        if (base64String.Contains("svg+xml"))
            return base64String;
        else
            return string.Format("{0},{1}", "data:image/jpeg;base64", base64String);
    }

    #endregion Implemented Methods

    #region Helper Methods

    /// <summary>
    /// This method get all the available details for the request from the Approvals database
    /// </summary>
    /// <param name="approvalDetails">The approval details</param>
    /// <param name="tenantInfo">ApprovalTenantInfo object for the current tenant</param>
    /// <param name="userAlias">Alias of the Approver of this request</param>
    /// <param name="logData">log Data</param>
    /// <returns>Dictionary of available details</returns>
    private Dictionary<string, ApprovalDetailsEntity> GetAvailableDetailsAndOperatioNames(
                                                        List<ApprovalDetailsEntity> approvalDetails,
                                                        ApprovalTenantInfo tenantInfo,
                                                        string userAlias,
                                                        Dictionary<LogDataKey, object> logData)
    {
        Dictionary<string, ApprovalDetailsEntity> detailsAvailable = new Dictionary<string, ApprovalDetailsEntity>();
        using (_performanceLogger.StartPerformanceLogger("PerfLog", Constants.WebClient, string.Format(Constants.PerfLogAction, tenantInfo.AppName, "ApprovalDetails from storage"), logData))
        {
            // Get EditedDetails for given user/ approver
            var editedApprovalDetails = approvalDetails.FirstOrDefault(t => t.RowKey == Constants.EditedDetailsOperationType + "|" + userAlias);

            // Getting list of all operations for this document number for the given tenant
            var detailOperations = from op in tenantInfo.DetailOperations.DetailOpsList
                                   where op._client == true
                                   select new { Name = op.operationtype, pagable = op.SupportsPagination };

            if (approvalDetails.Count > 0)
            {
                // Get details based on each operation that is needed for this tenant
                foreach (var tenantOperationDetails in detailOperations)
                {
                    try
                    {
                        // Checking if details fetched from Storage has data for the required Operation
                        ApprovalDetailsEntity detail = approvalDetails.Where(t => t.RowKey == tenantOperationDetails.Name).ToList().FirstOrDefault();

                        if (detail != null)
                        {
                            // Check if details are edited. If so display saved edited details
                            if (editedApprovalDetails != null)
                            {
                                var detailObject = detail.JSONData.ToJObject();
                                var editedDetailsObject = editedApprovalDetails.JSONData.ToJObject();
                                foreach (var part in detailObject)
                                {
                                    // TODO:: This code assumes Attachments property can be a part of both SummaryJson (as part of ARX) or in any of the tenant calls or sub-property
                                    // This might undergo changes.
                                    if (detailObject.Property(part.Key) != null)
                                    {
                                        detailObject.Property(part.Key).Value = editedDetailsObject[part.Key];
                                    }
                                    else
                                    {
                                        detailObject.Add(part.Key, editedDetailsObject[part.Key]);
                                    }
                                }

                                detail.JSONData = detailObject.ToJson();
                            }

                            // Adding the detail to the Dictionary
                            detailsAvailable.Add(tenantOperationDetails.Name, detail);
                        }
                    }
                    catch (Exception ex)
                    {
                        logData.Modify(LogDataKey.EventId, TrackingEvent.GetAvailableDetailsAndOperatioNamesFailure + tenantInfo.TenantId);
                        logData.Modify(LogDataKey.EventName, TrackingEvent.GetAvailableDetailsAndOperatioNamesFailure.ToString());
                        _logProvider.LogError(TrackingEvent.GetAvailableDetailsAndOperatioNamesFailure, ex, logData);
                    }
                }
            }

            return detailsAvailable;
        }
    }

    /// <summary>
    /// Creates the Approver Chain data
    /// </summary>
    /// <param name="documentSummary">ApprovalSummaryRow object</param>
    /// <param name="historyDataExts">List of TransactionHistory data</param>
    /// <param name="approvalDetails">The approval details</param>
    /// <param name="alias">Alias of the Approver of this request</param>
    /// <param name="currentApproverInDbJson">Current Approver details from Approval Details table</param>
    /// <param name="tenantInfo">ApprovalTenantInfo object for the current tenant</param>
    /// <param name="xcv">Cross system correlation vector for telemetry and logging</param>
    /// <param name="tcv">GUID transaction correlation vector for telemetry and logging</param>
    /// <param name="isWorkerTriggered"></param>
    /// <returns>String of the approver chain data</returns>
    private async Task<string> CreateCurrentApproverChain(ApprovalSummaryRow documentSummary, List<TransactionHistoryExt> historyDataExts, List<ApprovalDetailsEntity> approvalDetails, string alias, ApprovalDetailsEntity currentApproverInDbJson, ApprovalTenantInfo tenantInfo, string xcv, string tcv, bool isWorkerTriggered)
    {
        /*TODO:: IsOldHierarchyEnbled flag is introduced to control Request Activities display with default value as false. When multiple approvers occur at one level, all the current approvers at same level are shown when the flag is false.
                     If any tenant has issue with RequestActivities, Add this flag in TenantInfo table and mark it as 'true' so that Old Hierarchy code will be executed for that tenant.
                     Whenever all the tenants' display appropriate Request Activities, remove this flag.*/
        if (tenantInfo.IsOldHierarchyEnabled == false)
        {
            #region TODO:: Remove OldHierarchyEnabled flag specific Code and enable the code for all tenants

            // Get the Complete Approver Chain which has all the previous approvers
            var previousApprovers = approvalDetails.FirstOrDefault(x => x.RowKey.Equals(Constants.ApprovalChainOperation, StringComparison.OrdinalIgnoreCase));

            if (previousApprovers != null)
            {
                var previousApproverInDb = previousApprovers.JSONData.FromJson<List<ApproverChainEntity>>();

                previousApproverInDb.Add(new ApproverChainEntity()
                {
                    Alias = documentSummary.Approver,
                    Name = await _nameResolutionHelper.GetUserName(documentSummary.Approver),
                    Action = string.Empty,
                    _future = false
                });

                SummaryJson summary = documentSummary.SummaryJson.FromJson<SummaryJson>();
                if (summary.ApprovalHierarchy != null && summary.ApprovalHierarchy.Any())
                {
                    int approverCount = 0;
                    foreach (var approver in summary.ApprovalHierarchy)
                    {
                        // Skipping the current Approver from the Previous Approver chain
                        if (summary.ApprovalHierarchy.Count >= previousApproverInDb.Count && previousApproverInDb.Count > ++approverCount)
                        {
                            continue;
                        }
                        var isFutureApprover = true;
                        if (approver.Approvers != null && approver.Approvers.FirstOrDefault(x => x.Alias == documentSummary.Approver) != null)
                        {
                            // Setting up ApproverType for current approver
                            foreach (var previousApprover in previousApproverInDb.Where(x => x.Alias.Equals(documentSummary.Approver, StringComparison.OrdinalIgnoreCase)))
                            {
                                if (tenantInfo.IsSameApproverMultipleLevelSupported)
                                {
                                    if (previousApprover.ActionDate == null && string.IsNullOrWhiteSpace(previousApprover.Type))
                                    {
                                        previousApprover.Type = approver.ApproverType;
                                        isFutureApprover = false;
                                    }
                                    else if (previousApprover.Type == approver.ApproverType)
                                    {
                                        isFutureApprover = false;
                                    }
                                }
                                else
                                {
                                    previousApprover.Type = approver.ApproverType;
                                    isFutureApprover = false;
                                }
                            }

                            // Parallel approvers at same level as current approver gets added to historyDataExts to display under Request Activities
                            if (tenantInfo.IsAllCurrentApproversDisplayInHierarchy && !isFutureApprover && approver.Approvers != null)
                            {
                                foreach (var currentApprover in approver.Approvers.Where(a => a.Alias != documentSummary.Approver))
                                {
                                    previousApproverInDb.Add(new ApproverChainEntity()
                                    {
                                        Alias = currentApprover.Alias,
                                        Name = string.IsNullOrEmpty(currentApprover.Name) ? await _nameResolutionHelper.GetUserName(currentApprover.Alias) : currentApprover.Name,
                                        Action = string.Empty,
                                        Type = approver.ApproverType,
                                        _future = false
                                    });
                                }
                            }
                        }
                        AddApproverChainEntityRange(isFutureApprover, approver, true, previousApproverInDb);
                    }
                }

                string approverChainString = previousApproverInDb.ToJson(new JsonSerializerSettings { NullValueHandling = NullValueHandling.Ignore });
                return approverChainString;
            }
            else
            {
                if (documentSummary != null)
                {
                    historyDataExts.Add(new TransactionHistoryExt()
                    {
                        Approver = documentSummary.Approver,
                        JsonData = documentSummary.SummaryJson,
                        ActionTaken = string.Empty
                    });

                    // Add Future Approver Chain into TransactionHistoryExts object
                    if (!string.IsNullOrEmpty(documentSummary.SummaryJson))
                    {
                        SummaryJson summary = documentSummary.SummaryJson.FromJson<SummaryJson>();
                        if (summary.ApprovalHierarchy != null && summary.ApprovalHierarchy.Any())
                        {
                            foreach (var approver in summary.ApprovalHierarchy)
                            {
                                var isFutureApprover = true;
                                if (approver.Approvers != null && approver.Approvers.FirstOrDefault(x => x.Alias == documentSummary.Approver) != null)
                                {
                                    // Add approver type into current Approver
                                    foreach (var transactionHistoryExt in historyDataExts.Where(transactionHistoryExt => transactionHistoryExt.Approver ==
                                                                                                                         documentSummary.Approver))
                                    {
                                        if (tenantInfo.IsSameApproverMultipleLevelSupported)
                                        {
                                            if (transactionHistoryExt.ActionDate == null && string.IsNullOrWhiteSpace(transactionHistoryExt.ApproverType))
                                            {
                                                transactionHistoryExt.ApproverType = approver.ApproverType;
                                                isFutureApprover = false;
                                            }
                                            else if (transactionHistoryExt.ApproverType == approver.ApproverType)
                                            {
                                                isFutureApprover = false;
                                            }
                                        }
                                        else
                                        {
                                            transactionHistoryExt.ApproverType = approver.ApproverType;
                                            isFutureApprover = false;
                                        }
                                    }
                                    // Parallel approvers at same level as current approvers gets added to historyDataExts to display under Request Activities
                                    if (tenantInfo.IsAllCurrentApproversDisplayInHierarchy && !isFutureApprover && approver.Approvers != null)
                                    {
                                        foreach (var currentApprover in approver.Approvers.Where(a => a.Alias != documentSummary.Approver))
                                        {
                                            historyDataExts.Add(new TransactionHistoryExt()
                                            {
                                                Approver = currentApprover.Alias,
                                                JsonData = documentSummary.SummaryJson,
                                                ActionTaken = string.Empty,
                                                ApproverType = approver.ApproverType,
                                                _future = false,
                                                ApproverName = currentApprover.Name
                                            });
                                        }
                                    }
                                }
                                AddHistoryDataExtsRange(isFutureApprover, approver, true, historyDataExts, documentSummary);
                            }
                        }
                    }
                }

                int historyCount = 0;
                var approverChains = (from history in historyDataExts
                                      let histCount = historyCount++
                                      let isBasicActions = history.ActionTaken.Equals("System Cancel", StringComparison.InvariantCultureIgnoreCase)
                                             || history.ActionTaken.Equals("System Send Back", StringComparison.InvariantCultureIgnoreCase)
                                             || history.ActionTaken.Equals("Cancel", StringComparison.InvariantCultureIgnoreCase)
                                             || history.ActionTaken.Equals("Resubmitted", StringComparison.InvariantCultureIgnoreCase)
                                      let isBasicActionsWithTakeback = isBasicActions || history.ActionTaken.Equals("Takeback", StringComparison.InvariantCultureIgnoreCase)

                                      let historyJson = string.IsNullOrEmpty(history.JsonData)
                                         ? "{}".ToJObject()
                                         : history.JsonData.ToJObject()
                                      let historyNotes = string.IsNullOrEmpty(history.ApproversNote)
                                         ? "{}".ToJObject()
                                         : history.ApproversNote.ToJObject()
                                      let approverAlias = history.Approver
                                      let approverName = string.IsNullOrEmpty(history.ApproverName) ? _nameResolutionHelper.GetUserName(history.Approver).Result : history.ApproverName
                                      let approverType = string.IsNullOrEmpty(history.ApproverType) ? GetApprovalType(historyJson, approverAlias, histCount) : history.ApproverType // Use ApproverType directly if exists
                                      select new ApproverChainEntity()
                                      {
                                          Alias = (isBasicActionsWithTakeback)
                                                 ? string.Empty
                                                 : history.Approver,
                                          Name = (isBasicActionsWithTakeback)
                                                 ? string.Empty
                                                 : (history.Approver.Equals(approverAlias, StringComparison.InvariantCultureIgnoreCase)
                                                     ? approverName
                                                     : _nameResolutionHelper.GetUserName(history.Approver).Result),
                                          Action = history.ActionTaken,
                                          ActionDate = history.ActionDate,
                                          Type = (isBasicActionsWithTakeback)
                                                 ? null
                                                 : approverType,
                                          Justification = (isBasicActions)
                                                 ? null
                                                 : MSAHelper.ExtractValueFromJSON(historyNotes, "JustificationText"),
                                          Notes = (isBasicActions)
                                                 ? null
                                                 : MSAHelper.ExtractValueFromJSON(historyNotes, "Comment"),
                                          _future = history._future,
                                          DelegateUser = history.DelegateUser ?? null
                                      }).ToList();

                CheckUnauthorizedAccess(approverChains, alias);

                string approverChainString = approverChains.ToJson(new JsonSerializerSettings { NullValueHandling = NullValueHandling.Ignore });

                // Add APPRCHAIN details if not exists
                if (currentApproverInDbJson != null)
                {
                    var apprChainObj = approverChains;
                    List<ApproverChainEntity> approverChainList = new List<ApproverChainEntity>();
                    foreach (var apprChain in apprChainObj)
                    {
                        AddApproverChain(approverChainList, apprChain);
                    }

                    AddTransactionalAndHistoricalData(approverChainList, documentSummary, tenantInfo, isWorkerTriggered, xcv, tcv);
                }
                return approverChainString;
            }

            #endregion TODO:: Remove OldHierarchyEnabled flag specific Code and enable the code for all tenants
        }
        else
        {
            #region Hierarchy for remaining all tenants

            // Get the Complete Approver Chain which has all the previous approvers
            var previousApprovers = approvalDetails.FirstOrDefault(x => x.RowKey.Equals(Constants.ApprovalChainOperation, StringComparison.OrdinalIgnoreCase));

            if (previousApprovers != null)
            {
                var previousApproverInDb = previousApprovers.JSONData.FromJson<List<ApproverChainEntity>>();

                previousApproverInDb.Add(new ApproverChainEntity()
                {
                    Alias = documentSummary.Approver,
                    Name = await _nameResolutionHelper.GetUserName(documentSummary.Approver),
                    Action = string.Empty,
                    _future = false
                });

                SummaryJson summary = documentSummary.SummaryJson.FromJson<SummaryJson>();
                if (summary.ApprovalHierarchy != null && summary.ApprovalHierarchy.Any())
                {
                    var isFutureApprover = false;
                    foreach (var approver in summary.ApprovalHierarchy)
                    {
                        if (approver.Approvers != null && approver.Approvers.FirstOrDefault(x => x.Alias == documentSummary.Approver) != null)
                        {
                            // Add approver type into current Approver
                            foreach (var previousApprover in previousApproverInDb.Where(x => x.Alias.Equals(documentSummary.Approver, StringComparison.OrdinalIgnoreCase)))
                            {
                                if (tenantInfo.IsSameApproverMultipleLevelSupported)
                                {
                                    if (previousApprover.ActionDate == null && string.IsNullOrWhiteSpace(previousApprover.Type))
                                    {
                                        previousApprover.Type = approver.ApproverType;
                                    }
                                }
                                else
                                {
                                    previousApprover.Type = approver.ApproverType;
                                }
                            }
                            isFutureApprover = true;
                        }
                        AddApproverChainEntityRange(isFutureApprover, approver, approver.Approvers.FirstOrDefault(x => x.Alias == documentSummary.Approver) == null, previousApproverInDb);
                    }
                }

                string approverChainString = previousApproverInDb.ToJson(new JsonSerializerSettings { NullValueHandling = NullValueHandling.Ignore });
                return approverChainString;
            }
            else
            {
                if (documentSummary != null)
                {
                    historyDataExts.Add(new TransactionHistoryExt()
                    {
                        Approver = documentSummary.Approver,
                        JsonData = documentSummary.SummaryJson,
                        ActionTaken = string.Empty
                    });

                    // Add Future Approver Chain into TransactionHistoryExts object
                    if (!string.IsNullOrEmpty(documentSummary.SummaryJson))
                    {
                        SummaryJson summary = documentSummary.SummaryJson.FromJson<SummaryJson>();
                        if (summary.ApprovalHierarchy != null && summary.ApprovalHierarchy.Any())
                        {
                            var isFutureApprover = false;
                            foreach (var approver in summary.ApprovalHierarchy)
                            {
                                if (approver.Approvers != null && approver.Approvers.FirstOrDefault(x => x.Alias == documentSummary.Approver) != null)
                                {
                                    // Add approver type into current Approver
                                    foreach (var transactionHistoryExt in historyDataExts.Where(transactionHistoryExt => transactionHistoryExt.Approver ==
                                                                                                                         documentSummary.Approver))
                                    {
                                        transactionHistoryExt.ApproverType = approver.ApproverType;
                                    }

                                    isFutureApprover = true;
                                }
                                AddHistoryDataExtsRange(isFutureApprover, approver,
                                    approver.Approvers.FirstOrDefault(x => x.Alias == documentSummary.Approver) == null, historyDataExts, documentSummary);
                            }
                        }
                    }
                }

                var approverChains = (from history in historyDataExts
                                      let historyJson = string.IsNullOrEmpty(history.JsonData)
                                         ? "{}".ToJObject()
                                         : history.JsonData.ToJObject()
                                      let historyNotes = string.IsNullOrEmpty(history.ApproversNote)
                                         ? "{}".ToJObject()
                                         : history.ApproversNote.ToJObject()
                                      let isBasicActions = history.ActionTaken.Equals("System Cancel", StringComparison.InvariantCultureIgnoreCase)
                                           || history.ActionTaken.Equals("System Send Back", StringComparison.InvariantCultureIgnoreCase)
                                           || history.ActionTaken.Equals("Cancel", StringComparison.InvariantCultureIgnoreCase)
                                           || history.ActionTaken.Equals("Resubmitted", StringComparison.InvariantCultureIgnoreCase)
                                      let isBasicActionsWithTakeback = isBasicActions || history.ActionTaken.Equals("Takeback", StringComparison.InvariantCultureIgnoreCase)

                                      let approverAlias = history.Approver
                                      let approverName = string.IsNullOrEmpty(history.ApproverName) ? _nameResolutionHelper.GetUserName(history.Approver).Result : history.ApproverName
                                      let approverType = string.IsNullOrEmpty(history.ApproverType) ? GetApprovalType(historyJson, approverAlias) : history.ApproverType // Use ApproverType directly if exists
                                      select new ApproverChainEntity()
                                      {
                                          Alias = (isBasicActionsWithTakeback)
                                                 ? string.Empty
                                                 : history.Approver,
                                          Name = (isBasicActionsWithTakeback)
                                                 ? string.Empty
                                                 : (history.Approver.Equals(approverAlias, StringComparison.InvariantCultureIgnoreCase)
                                                     ? approverName
                                                     : _nameResolutionHelper.GetUserName(history.Approver).Result),
                                          Action = history.ActionTaken,
                                          ActionDate = history.ActionDate,
                                          Type = (isBasicActionsWithTakeback)
                                                 ? null
                                                 : approverType,
                                          Justification = (isBasicActions)
                                                 ? null
                                                 : MSAHelper.ExtractValueFromJSON(historyNotes, "JustificationText"),
                                          Notes = (isBasicActions)
                                                 ? null
                                                 : MSAHelper.ExtractValueFromJSON(historyNotes, "Comment"),
                                          _future = history._future,
                                          DelegateUser = history.DelegateUser ?? null
                                      }).ToList();
                CheckUnauthorizedAccess(approverChains, alias);

                string approverChainString = approverChains.ToJson(new JsonSerializerSettings { NullValueHandling = NullValueHandling.Ignore });

                // Add APPRCHAIN details if not exists
                if (currentApproverInDbJson != null)
                {
                    var apprChainObj = approverChains.ToList();
                    List<ApproverChainEntity> approverChainList = new List<ApproverChainEntity>();
                    foreach (var apprChain in apprChainObj)
                    {
                        AddApproverChain(approverChainList, apprChain);
                    }

                    AddTransactionalAndHistoricalData(approverChainList, documentSummary, tenantInfo, isWorkerTriggered, xcv, tcv);
                }
                return approverChainString;
            }

            #endregion Hierarchy for remaining all tenants
        }
    }

    /// <summary>
    /// Add approver chain entity range
    /// </summary>
    /// <param name="isFutureApprover"></param>
    /// <param name="approver"></param>
    /// <param name="isAlreadyApproved"></param>
    /// <param name="previousApproverInDb"></param>
    private void AddApproverChainEntityRange(bool isFutureApprover, ApprovalHierarchy approver, bool isAlreadyApproved, List<ApproverChainEntity> previousApproverInDb)
    {
        if (isFutureApprover && approver.Approvers != null && isAlreadyApproved)
        {
            previousApproverInDb.AddRange(
                approver.Approvers.Select(approverAlias => new ApproverChainEntity()
                {
                    Alias = approverAlias.Alias,
                    Name = string.IsNullOrEmpty(approverAlias.Name) ? _nameResolutionHelper.GetUserName(approverAlias.Alias).Result : approverAlias.Name,
                    Action = string.Empty,
                    Type = approver.ApproverType,
                    _future = true
                }));
        }
    }

    /// <summary>
    /// Add TransactionHistory
    /// </summary>
    /// <param name="isFutureApprover"></param>
    /// <param name="approver"></param>
    /// <param name="isAlreadyApproved"></param>
    /// <param name="historyDataExts"></param>
    /// <param name="documentSummary"></param>
    private void AddHistoryDataExtsRange(bool isFutureApprover, ApprovalHierarchy approver, bool isAlreadyApproved,
        List<TransactionHistoryExt> historyDataExts, ApprovalSummaryRow documentSummary)
    {
        if (isFutureApprover && approver.Approvers != null && isAlreadyApproved)
        {
            historyDataExts.AddRange(
                approver.Approvers.Select(approverAlias => new TransactionHistoryExt()
                {
                    Approver = approverAlias.Alias,
                    JsonData = documentSummary.SummaryJson,
                    ActionTaken = string.Empty,
                    ApproverType = approver.ApproverType,
                    _future = true,
                    ApproverName = approverAlias.Name
                }));
        }
    }

    /// <summary>
    /// Check unauthorized access
    /// </summary>
    /// <param name="approverChains"></param>
    /// <param name="alias"></param>
    private void CheckUnauthorizedAccess(List<ApproverChainEntity> approverChains, string alias)
    {
        if (approverChains.FirstOrDefault(h => h.Alias.Trim().Equals(alias, StringComparison.InvariantCultureIgnoreCase)) == null)
        {
            throw new UnauthorizedAccessException("User doesn't have permission to see the report.");
        }
    }

    /// <summary>
    /// Add approver chain
    /// </summary>
    /// <param name="approverChainList"></param>
    /// <param name="apprChain"></param>
    private void AddApproverChain(List<ApproverChainEntity> approverChainList, ApproverChainEntity apprChain)
    {
        if (apprChain._future == false && !string.IsNullOrEmpty(apprChain.Action) && apprChain.ActionDate != null)
        {
            ApproverChainEntity approverChain = new ApproverChainEntity()
            {
                Action = apprChain.Action,
                ActionDate = apprChain.ActionDate,
                Alias = apprChain.Alias,
                DelegateUser = apprChain.DelegateUser,
                Justification = apprChain.Justification,
                Name = apprChain.Name,
                Notes = apprChain.Notes,
                Type = apprChain.Type,
                _future = apprChain._future
            };
            approverChainList.Add(approverChain);
        }
    }

    /// <summary>
    /// Add transactional and historical data
    /// </summary>
    /// <param name="approverChainList"></param>
    /// <param name="documentSummary"></param>
    /// <param name="tenantInfo"></param>
    /// <param name="isWorkerTriggered"></param>
    /// <param name="xcv"></param>
    /// <param name="tcv"></param>
    private void AddTransactionalAndHistoricalData(List<ApproverChainEntity> approverChainList,
        ApprovalSummaryRow documentSummary,
        ApprovalTenantInfo tenantInfo,
        bool isWorkerTriggered,
        string xcv, string tcv)
    {
        if (approverChainList != null && approverChainList.Count > 0)
        {
            ApprovalDetailsEntity approvalDetailsEntity = new ApprovalDetailsEntity()
            {
                PartitionKey = documentSummary.DocumentNumber,
                RowKey = Constants.ApprovalChainOperation,
                ETag = global::Azure.ETag.All,
                JSONData = approverChainList.ToJson(),
                TenantID = int.Parse(tenantInfo.RowKey)
            };
            ApprovalsTelemetry telemetry = new ApprovalsTelemetry()
            {
                Xcv = xcv,
                Tcv = tcv,
                BusinessProcessName = isWorkerTriggered ? string.Format(tenantInfo.BusinessProcessName, Constants.BusinessProcessNameAddSummaryCopy, Constants.BusinessProcessNameDetailsWorkerTriggered)
                : string.Format(tenantInfo.BusinessProcessName, Constants.BusinessProcessNameAddSummaryCopy, Constants.BusinessProcessNameUserTriggered)
            };
            _approvalDetailProvider.AddTransactionalAndHistoricalDataInApprovalsDetails(approvalDetailsEntity, tenantInfo, telemetry);
        }
    }

    /// <summary>
    /// Add actions in response object based on filters
    /// </summary>
    /// <param name="alias">Alias of the Approver of this request</param>
    /// <param name="loggedInAlias">Logged in User Alias</param>
    /// <param name="tenantInfo">ApprovalTenantInfo object for the current tenant</param>
    /// <param name="documentSummary">ApprovalSummaryRow object</param>
    /// <param name="additionalDataJson">The Additional Data json from ApprovalDetails table</param>
    /// <param name="clientDevice">Client Device</param>
    /// <param name="sessionId">Session ID</param>
    /// <param name="xcv">X-correlation ID</param>
    /// <param name="tcv">T-correlation ID</param>
    /// <param name="aadUserToken">AAD User Token</param>
    /// <returns>Tenant Action Details object</returns>
    private async Task<TenantActionDetails> AddAllowedActions(
                            string alias,
                            string loggedInAlias,
                            ApprovalTenantInfo tenantInfo,
                            ApprovalSummaryRow documentSummary,
                            string additionalDataJson,
                            string clientDevice,
                            string sessionId,
                            string xcv,
                            string tcv,
                            string aadUserToken)
    {
        var actions = new TenantActionDetails();

        // No Actions to show when a Request with LOBPending= true.
        if (documentSummary != null && documentSummary.LobPending)
        {
            return actions;
        }

        var delegationAccessLevel = _delegationHelper.GetDelegationAccessLevel(alias, loggedInAlias);

        if (delegationAccessLevel != DelegationAccessLevel.Admin)
        {
            return actions;
        }

        if (!tenantInfo.IsExternalTenantActionDetails && (string.IsNullOrEmpty(tenantInfo.TenantActionDetails) || !tenantInfo.TenantEnabled))
        {
            return actions;
        }

        if (documentSummary != null && documentSummary.Approver.Equals(alias, StringComparison.InvariantCultureIgnoreCase))
        {
            var summary = documentSummary.SummaryJson.FromJson<SummaryModel>(
                                                            new JsonSerializerSettings
                                                            {
                                                                NullValueHandling = NullValueHandling.Ignore
                                                            });
            summary.AdditionalData = (summary.AdditionalData == null || summary.AdditionalData.Count == 0) ?
                additionalDataJson?.ToJObject()[Constants.AdditionalData]?.ToJson()?.FromJson<Dictionary<string, string>>() : summary.AdditionalData;

            var summaries = new List<SummaryModel> { summary };

            var documentSummaries = new List<ApprovalSummaryRow> { documentSummary };

            AddApproverType(alias, summaries);

            //TODO :: Call Tenant Info method from here
            TenantActionDetails actionDetailsObject;
            if (tenantInfo.IsExternalTenantActionDetails && !tenantInfo.IsPullModelEnabled)
            {
                var tenantInfoNew = await _approvalTenantInfoHelper.GetTenantActionDetails(tenantInfo.TenantId, loggedInAlias, alias, clientDevice, sessionId, xcv, tcv, aadUserToken);
                actionDetailsObject = tenantInfoNew.TenantActionDetails.FromJson<TenantActionDetails>();
            }
            else
                actionDetailsObject = tenantInfo.TenantActionDetails.FromJson<TenantActionDetails>();

            actions.Primary = GetPrimaryActionsFilter(actionDetailsObject, summaries);
            actions.Secondary = GetSecondaryActionsFilter(actionDetailsObject, summaries, documentSummaries);
            TenantAction outOfSyncActionObjOld = actions.Secondary.Where(a => a.Code == Constants.OutOfSyncAction).ToList().FirstOrDefault();

            // Second filter based on list of ApprovalActions property.
            var allowedActions = documentSummary.SummaryJson?.FromJson<SummaryJson>().ApprovalActionsApplicable;
            actions.Primary = allowedActions != null && allowedActions.Count > 0 ? actions.Primary.Where(a => allowedActions.Contains(a.Code)).ToList() : actions.Primary;
            actions.Secondary = allowedActions != null && allowedActions.Count > 0 ? actions.Secondary.Where(a => allowedActions.Contains(a.Code)).ToList() : actions.Secondary;

            TenantAction outOfSyncActionObj = actions.Secondary.Where(a => a.Code == Constants.OutOfSyncAction).ToList().FirstOrDefault();
            if (outOfSyncActionObjOld != null && outOfSyncActionObj == null)
            {
                outOfSyncActionObj = outOfSyncActionObjOld;
                actions.Secondary.Add(outOfSyncActionObj);
            }

            bool isOutOfSyncEnabled = _flightingDataProvider.IsFeatureEnabledForUser(loggedInAlias, (int)FlightingFeatureName.ManageOutOfSync);
            if (!isOutOfSyncEnabled && outOfSyncActionObj != null)
            {
                actions.Secondary.Remove(outOfSyncActionObj);
            }
        }

        return actions;
    }

    /// <summary>
    /// Gets all the operation names for which details is missing in Approvals database
    /// </summary>
    /// <param name="availableDetails">Available details fetched from Approvals database</param>
    /// <param name="tenantInfo">ApprovalTenantInfo object for the current tenant</param>
    /// <param name="approvalIdentifier">The Approval Identifier object which has the DisplayDocumentNumber, DocumentNumber and FiscalYear</param>
    /// <returns>List of string for all the operations for which details is missing in Approvals database</returns>
    private List<string> GetOperationListForMissingDetails(
                                                        Dictionary<string, ApprovalDetailsEntity> availableDetails,
                                                        ApprovalTenantInfo tenantInfo,
                                                        ApprovalIdentifier approvalIdentifier)
    {
        List<string> operations = new List<string>();
        string baseUrl = string.Empty;

        // Getting list of all operations for this document number for the given tenant
        var detailOperations = from op in tenantInfo.DetailOperations.DetailOpsList
                               where op._client == true
                               select new { Name = op.operationtype, pagable = op.SupportsPagination };

        baseUrl = _config[ConfigurationKey.APIUrlRoot.ToString()];
        baseUrl += "detail/" + tenantInfo.TenantId + "/" + approvalIdentifier.GetDocNumber(tenantInfo) + "/{0}";

        if (approvalIdentifier.FiscalYear != null)
        {
            baseUrl += "?fiscalYear=" + approvalIdentifier.FiscalYear;
        }

        string url = string.Empty;
        int availableCount = availableDetails.Count;

        foreach (var operation in detailOperations)
        {
            url = string.Empty;
            ApprovalDetailsEntity detail = null;

            // Checking if details fetched from Storage has data for the required Operation
            if (availableCount > 0)
            {
                availableDetails.TryGetValue(operation.Name, out detail);
            }

            if (detail == null)
            {
                url = string.Format(baseUrl, operation.Name);
            }

            if (!string.IsNullOrEmpty(url))
            {
                operations.Add(url);
            }
        }

        return operations;
    }

    /// <summary>
    /// This method basically checks the Authorization for the user to view or work on this request.
    /// This includes viewing the details or downloading the attachments.
    /// First checks whether the user is the current approver if not check if the user is one of the previous approvers from ApproverChain data.
    /// If the approver chain data is missing, fall-back to get the complete history from TransactionHistory table.
    /// </summary>
    /// <param name="tenantId">Tenant Id of the tenant (1/2/3..)</param>
    /// <param name="documentNumber">Document Number of the request</param>
    /// <param name="fiscalYear">Fiscal year of the request</param>
    /// <param name="sessionId">GUID session id</param>
    /// <param name="tcv">GUID transaction correlation vector for telemetry and logging</param>
    /// <param name="xcv">Cross system correlation vector for telemetry and logging</param>
    /// <param name="userAlias">Alias of the Approver of this request</param>
    /// <param name="loggedInAlias">Logged in User Alias</param>
    /// <param name="currentApproverInDbJson">CurrentApprover JSON from ApprovalDetails table</param>
    /// <param name="previousApprovers">Previous Approver JSON from ApprovalDetails table (History) </param>
    /// <returns>List of TransactionHistoryExt object which contains the historical transactional details. </returns>
    private List<TransactionHistoryExt> CheckUserPermissions(int tenantId, string documentNumber, string fiscalYear, string sessionId, string tcv, string xcv, string userAlias, string loggedInAlias, ApprovalDetailsEntity currentApproverInDbJson, ApprovalDetailsEntity previousApprovers, List<ApprovalSummaryRow> summaryDataList, out JObject authResponseObject)
    {
        authResponseObject = null;
        List<TransactionHistoryExt> documentHistoryExts = new List<TransactionHistoryExt>();

        // get the currentAprpover row
        bool isUserAuthorized = false;
        bool isCurrentApprover = false, isPreviousApprover = false;

        if (currentApproverInDbJson != null)
        {
            var currentApproversInDb = currentApproverInDbJson.JSONData.FromJson<List<Approver>>();
            foreach (var approver in currentApproversInDb)
            {
                if (approver.Alias.Equals(userAlias, StringComparison.OrdinalIgnoreCase))
                {
                    isCurrentApprover = true;
                    break;
                }
            }
        }

        if (previousApprovers != null)
        {
            var previousApproverInDb = previousApprovers.JSONData.FromJson<List<ApproverChainEntity>>();
            foreach (var approver in previousApproverInDb)
            {
                if (approver.Alias.Equals(userAlias, StringComparison.OrdinalIgnoreCase))
                {
                    isPreviousApprover = true;
                    break;
                }
            }
        }
        else
        {
            //// Get the Approver Chain information from history table as the fallback mechanism

            #region Get Prerequisites - History

            var tenantInfo = _approvalTenantInfoHelper.GetTenantInfo(tenantId);

            // Given GetApproverChainHistoryData makes an http call over Azure Mobile Service
            // If executed as Task.Run, the thread is mostly going to wait for the response to come back
            // Hence a separate thread which will wait most of the time will not help
            // Hence choosing to implement this call on the current thread and await later
            // Doing this, additional threads won't be needed
            List<TransactionHistoryExt> getApproverChainHistoryData = _approvalHistoryProvider.GetApproverChainHistoryDataAsync(tenantInfo, documentNumber, fiscalYear, loggedInAlias, xcv, tcv, sessionId).Result;

            #endregion Get Prerequisites - History

            #region Get History - Await for results

            // Used Composition instead of Inheritance
            documentHistoryExts = getApproverChainHistoryData.ToList();

            #endregion Get History - Await for results

            if (documentHistoryExts.FirstOrDefault(h => h.Approver.Equals(userAlias, StringComparison.InvariantCultureIgnoreCase)) != null)
            {
                isPreviousApprover = true;
            }
        }

        isUserAuthorized = isCurrentApprover || isPreviousApprover;
        if (!isUserAuthorized)
        {
            authResponseObject = JObject.FromObject(new { Message = "User doesn't have permission to see the report." });
        }

        ApprovalSummaryRow summaryData = null;
        if (summaryDataList != null && summaryDataList.Count > 0)
            summaryData = summaryDataList.FirstOrDefault(d => d.Approver.Equals(userAlias));
        if (summaryData != null)
        {
            if (summaryData.LobPending)
            {
                //Action taken but response is pending from tenant
                authResponseObject = JObject.FromObject(new { Message = Constants.LobPendingMessage });
            }
            else if (summaryData.IsOfflineApproval)
            {
                //Request is submitted for background approval
                authResponseObject = JObject.FromObject(new { Message = Constants.SubmittedForBackgroundMessage });
            }
            else if (summaryData.IsOutOfSyncChallenged)
            {
                //Request is out of synchronization from tenant system
                authResponseObject = JObject.FromObject(new { Message = Constants.OutOfSyncMessage });
            }
        }

        return documentHistoryExts;
    }

    /// <summary>
    /// Log the message progress
    /// </summary>
    /// <param name="trackingEvent">Tracking Event for the logging</param>
    /// <param name="failureData">Failure data object which contains information about the reason of failure</param>
    /// <param name="criticalityLevel">Criticality level (Yes- if core functionality else No)</param>
    /// <param name="tenantLogData">Log data</param>
    private void LogMessageProgress(TrackingEvent trackingEvent, FailureData failureData, CriticalityLevel criticalityLevel, Dictionary<LogDataKey, object> tenantLogData = null)
    {
        if (tenantLogData == null)
        {
            tenantLogData = new Dictionary<LogDataKey, object>();
        }

        tenantLogData[LogDataKey.IsCriticalEvent] = criticalityLevel.ToString();
        tenantLogData[LogDataKey.LocalTime] = DateTime.UtcNow;
        tenantLogData[LogDataKey.FailureData] = failureData;

        tenantLogData[LogDataKey.OperationType] = "GetDetail";

        // overwrite TenantId as a work around to store DocumentTypeId
        tenantLogData[LogDataKey.TenantId] = tenantLogData[LogDataKey.DocumentTypeId];

        tenantLogData[LogDataKey.EventId] = (int)trackingEvent;
        tenantLogData[LogDataKey.EventName] = trackingEvent.ToString();

        if (tenantLogData.ContainsKey(LogDataKey.Operation))
        {
            tenantLogData[LogDataKey.EventName] = tenantLogData[LogDataKey.EventName] + "-" + tenantLogData[LogDataKey.Operation].ToString();
        }
        _logProvider.LogInformation(trackingEvent, tenantLogData);
    }

    /// <summary>
    /// Preview documents
    /// </summary>
    /// <param name="tenantAdaptor">The tenant adaptor</param>
    /// <param name="tenantId">Tenant Id of the tenant (1/2/3..)</param>
    /// <param name="tenantInfo">ApprovalTenantInfo object for the current tenant</param>
    /// <param name="approvalIdentifier">The Approval Identifier object which has the DisplayDocumentNumber, DocumentNumber and FiscalYear</param>
    /// <param name="alias">Alias of the Approver of this request</param>
    /// <param name="attachmentId">Attachment ID of the Document to be previewed</param>
    /// <param name="sessionId">GUID session id</param>
    /// <param name="loggedInAlias">Logged in User Alias</param>
    /// <param name="xcv">Cross system correlation vector for telemetry and logging</param>
    /// <param name="tcv">GUID transaction correlation vector for telemetry and logging</param>
    /// <returns>HttpResponseMessage with Stream data of the attachment</returns>
    private async Task<byte[]> PreviewDocumentAsync(
        ITenant tenantAdaptor,
        int tenantId,
        ApprovalTenantInfo tenantInfo,
        ApprovalIdentifier approvalIdentifier,
        string alias,
        string attachmentId,
        string sessionId,
        string loggedInAlias,
        string xcv,
        string tcv)
    {
        #region Logging Prep

        var logData = new Dictionary<LogDataKey, object>
        {
            { LogDataKey.Tcv, tcv },
            { LogDataKey.ReceivedTcv, tcv },
            { LogDataKey.SessionId, sessionId },
            { LogDataKey.Xcv, xcv },
            { LogDataKey.UserRoleName, loggedInAlias },
            { LogDataKey.BusinessProcessName, string.Format(tenantInfo.BusinessProcessName, Constants.BusinessProcessNameGetDocuments, Constants.BusinessProcessNameUserTriggered) },
            { LogDataKey.TenantId, tenantId },
            { LogDataKey.DocumentNumber, approvalIdentifier.DisplayDocumentNumber },
            { LogDataKey.DXcv, approvalIdentifier.DisplayDocumentNumber },
            { LogDataKey.FiscalYear, approvalIdentifier.FiscalYear },
            { LogDataKey.UserAlias, alias },
            { LogDataKey.IsCriticalEvent, CriticalityLevel.No.ToString() }
        };

        #endregion Logging Prep

        byte[] response = null;
        try
        {
            using (_performanceLogger.StartPerformanceLogger("PerfLog", Constants.WebClient, string.Format(Constants.PerfLogAction, tenantInfo.AppName, Constants.DocumentPreviewAction), logData))
            {
                ApprovalsTelemetry telemetry = new ApprovalsTelemetry()
                {
                    Xcv = xcv,
                    Tcv = tcv,
                    BusinessProcessName = string.Format(tenantInfo.BusinessProcessName, Constants.BusinessProcessNameGetDocuments, Constants.BusinessProcessNameUserTriggered),
                    TenantTelemetry = new Dictionary<string, string>()
                };

                // Check if a specific attachment id has been provided, in which case use the method which uses attachment id
                if (!string.IsNullOrWhiteSpace(attachmentId))
                {
                    response = await tenantAdaptor.PreviewDocumentUsingAttachmentIdAsync(approvalIdentifier, attachmentId, telemetry);
                }
            }
        }
        catch (Exception tenantDownloadException)
        {
            _logProvider.LogError(TrackingEvent.DocumentDownloadFailure, tenantDownloadException, logData);
            throw;
        }

        return response;
    }

    /// <summary>
    /// Add ApproverType from ApprovalHierarchy to AdditionalData of Summary (if not present)
    /// </summary>
    /// <param name="alias">Alias of the Approver of this request</param>
    /// <param name="summaries">List of SummaryModel object</param>
    private void AddApproverType(string alias, List<SummaryModel> summaries)
    {
        foreach (var summary in summaries)
        {
            var approvalHierarchy = summary.ApprovalHierarchy;
            if (approvalHierarchy != null)
            {
                var approver = approvalHierarchy.FirstOrDefault(x => x.Approvers != null && x.Approvers.FirstOrDefault(y => y.Alias == alias) != null);
                if (approver != null && summary.AdditionalData != null && !summary.AdditionalData.ContainsKey("ApproverType"))
                {
                    summary.AdditionalData.Add("ApproverType", approver.ApproverType);
                }
            }
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
        var approvalHierarchy = MSAHelper.ExtractValueFromJSON(historyJson, "ApprovalHierarchy").FromJson<List<ApprovalHierarchy>>();
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
            approverType = MSAHelper.ExtractValueFromJSON(historyJson, "AdditionalData.ApproverType");
        }
        return approverType;
    }

    /// <summary>
    /// Gets the Approval Type value for the approver
    /// </summary>
    /// <param name="historyJson">Historical data</param>
    /// <param name="approverAlias">Alias of the Approver of this request</param>
    /// <returns>Approval Type value for the approver as string</returns>
    private string GetApprovalType(JObject historyJson, string approverAlias, int historyCount)
    {
        string approverType = string.Empty;
        var approvalHierarchy = MSAHelper.ExtractValueFromJSON(historyJson, "ApprovalHierarchy").FromJson<List<ApprovalHierarchy>>();
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
            approverType = MSAHelper.ExtractValueFromJSON(historyJson, "AdditionalData.ApproverType");
        }
        return approverType ?? string.Empty;
    }

    /// <summary>
    /// Helper function which gets Primary Actions
    /// </summary>
    /// <param name="actionDetailsObject">Tenant Action details object</param>
    /// <param name="summaries">List of SummaryModel object</param>
    /// <returns>List of Primary Actions supported</returns>
    private List<TenantAction> GetPrimaryActionsFilter(TenantActionDetails actionDetailsObject, List<SummaryModel> summaries)
    {
        var actions = new List<TenantAction>();

        if (actionDetailsObject != null && actionDetailsObject.Primary != null)
        {
            // we send all primary buttons info to client even "IsEnabled:false". We will disable the button from client size javascript
            actions.AddRange(actionDetailsObject.Primary.Where(action => ExecuteConditions(action.Condition, summaries)).Select(action => ProcessActionToken(action, summaries)));
        }

        return actions;
    }

    /// <summary>
    /// Helper function which gets Secondary Actions
    /// </summary>
    /// <param name="actionDetailsObject">Tenant Action details object</param>
    /// <param name="summaries">List of SummaryModel object</param>
    /// <param name="documentSummaries">List of ApprovalSummaryRows</param>
    /// <returns>List of Secondary Actions supported</returns>
    private List<TenantAction> GetSecondaryActionsFilter(TenantActionDetails actionDetailsObject, List<SummaryModel> summaries, List<ApprovalSummaryRow> documentSummaries = null)
    {
        var actions = new List<TenantAction>();

        if (actionDetailsObject != null && actionDetailsObject.Secondary != null)
        {
            // we send all secondary buttons info to client even "IsEnabled:false". We will disable the button from client size javascript
            actions.AddRange(actionDetailsObject.Secondary.Where(action => ExecuteConditions(action.Condition, summaries, documentSummaries)).Select(action => ProcessActionToken(action, summaries)));
        }

        return actions;
    }

    /// <summary>
    /// Evaluates the expressions (conditions) on the input data and returns true or false based on whether the conditions are passed
    /// </summary>
    /// <param name="condition">Expression which needs to be evaluated</param>
    /// <param name="listData">Data (List of Summary model) on which conditions needs to be evaluated</param>
    /// <param name="documentSummaries">List of ApprovalSummaryRows</param>
    /// <returns>true or false </returns>
    private bool ExecuteConditions(string condition, List<SummaryModel> listData, List<ApprovalSummaryRow> documentSummaries = null)
    {
        var isConditionPassed = false;
        try
        {
            if (!string.IsNullOrEmpty(condition))
            {
                var conditions = condition.Split('&');
                foreach (var cnd in conditions)
                {
                    var conditionValues = cnd.Split('^');
                    if (conditionValues.Count() == 2)
                    {
                        var condtn = conditionValues[1];
                        var key = conditionValues[0];
                        if (key == "_client")
                        {
                            isConditionPassed = true;
                        }
                        else if (key == "IsOutOfSyncChallenged" && documentSummaries != null)
                        {
                            var tempList = documentSummaries.AsQueryable();
                            isConditionPassed = (!tempList.Any()) || tempList.AsQueryable().Where(condtn).Any();
                        }
                        else
                        {
                            var tempList = listData.AsQueryable().Where(x => x.AdditionalData.ContainsKey(key));
                            isConditionPassed = (!tempList.Any()) || tempList.AsQueryable().Where(condtn).Any();
                        }

                        if (!isConditionPassed)
                        {
                            break;
                        }
                    }
                    else
                    {
                        isConditionPassed = listData.AsQueryable().Where(cnd).Any();
                    }
                }
            }
            else
            {
                isConditionPassed = true;
            }
        }
        catch
        {
            // Do Nothing
        }

        return isConditionPassed;
    }

    /// <summary>
    /// Filters Justifications and Placements based on conditions
    /// </summary>
    /// <param name="action">Configured Action object from ApprovalTenantInfo</param>
    /// <param name="summaries">List of Summary model objects</param>
    /// <returns>Tenant Action object</returns>
    private TenantAction ProcessActionToken(TenantAction action, List<SummaryModel> summaries)
    {
        if (action != null && action.Justifications != null && action.Justifications.Count > 0)
        {
            action.Justifications = action.Justifications.Where(j => ExecuteConditions(j.Condition, summaries)).ToList();
        }

        if (action != null && action.Placements != null && action.Placements.Count > 0)
        {
            action.Placements = action.Placements.Where(p => ExecuteConditions(p.Condition, summaries)).ToList();
        }

        if (action.AdditionalInformation != null)
        {
            foreach (var addnInfo in action.AdditionalInformation)
            {
                if (addnInfo != null && addnInfo.Values != null && addnInfo.Values.Count > 0)
                {
                    addnInfo.Values = addnInfo.Values.Where(p => ExecuteConditions(p.Condition, summaries)).ToList();
                }
            }
        }

        return action;
    }

    /// <summary>
    /// This method will fetch missing details from LOB
    /// </summary>
    /// <param name="responseJObject">The responseJObject</param>
    /// <param name="tenantId">The tenantId</param>
    /// <param name="documentNumber">The documentNumber</param>
    /// <param name="fiscalYear">The fiscalYear</param>
    /// <param name="sessionId">The sessionId</param>
    /// <param name="tcv">The tcv</param>
    /// <param name="xcv">The xcv</param>
    /// <param name="userAlias">The userAlias</param>
    /// <param name="loggedInAlias">The loggedInAlias</param>
    /// <param name="clientDevice">The clientDevice</param>
    /// <param name="aadUserToken">The aadUserToken</param>
    /// <param name="callType">The callType</param>
    /// <param name="pageType">This the page calling the Details API e.g. Detail, History</param>
    /// <param name="source">Source for Details call eg. Summary, Notification</param>
    public void FetchMissingDetailsDataFromLOB(JObject responseJObject,
        int tenantId,
        string documentNumber,
        string fiscalYear,
        string sessionId,
        string tcv,
        string xcv,
        string userAlias,
        string loggedInAlias,
        string clientDevice,
        string aadUserToken,
        int callType,
        string pageType,
        string source)
    {
        JObject missingDataResponseJObject = null;
        var urls = responseJObject.Property("CallBackURLCollection") != null ? responseJObject.Property("CallBackURLCollection").Value.ToList() : new List<JToken>();

        if (urls.Any())
        {
            Parallel.ForEach(urls, (url) =>
            {
                // operationName is like HDR,DTL,LINE
                string operationName = url.Value<string>().Split('/').Last();

                // Fetch missing details from LOB system and store it in azuretable
                missingDataResponseJObject = Task.Run(() => GetDetails(tenantId, documentNumber,
                    operationName, fiscalYear, 1, sessionId, tcv, xcv, userAlias, loggedInAlias, clientDevice,
                    aadUserToken, false, callType, pageType, source)).Result;

                if (missingDataResponseJObject != null && missingDataResponseJObject.Count > 0)
                {
                    // Merge missing details call into mail responseJObject to build complete adaptive card with all required details
                    responseJObject.Merge(missingDataResponseJObject);
                }
            });
        }
    }

    #endregion Helper Methods
}