// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace Microsoft.CFS.Approvals.Core.BL.Helpers;

using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
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
using Microsoft.CFS.Approvals.Utilities.Interface;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json.Linq;

/// <summary>
/// Class DocumentActionHelper.
/// </summary>
/// <seealso cref="IDocumentActionHelper" />
public class DocumentActionHelper : IDocumentActionHelper
{
    #region Variables

    /// <summary>
    /// The approval summary provider
    /// </summary>
    protected readonly IApprovalSummaryProvider _approvalSummaryProvider = null;

    /// <summary>
    /// The configuration
    /// </summary>
    protected readonly IConfiguration _config;

    /// <summary>
    /// The logger
    /// </summary>
    protected readonly ILogProvider _logger = null;

    /// <summary>
    /// The name resolution helper
    /// </summary>
    protected readonly INameResolutionHelper _nameResolutionHelper = null;

    /// <summary>
    /// The performance logger
    /// </summary>
    protected readonly IPerformanceLogger _performanceLogger = null;

    /// <summary>
    /// The approval detail provider
    /// </summary>
    protected readonly IApprovalDetailProvider _approvalDetailProvider = null;

    /// <summary>
    /// The flighting data provider
    /// </summary>
    protected readonly IFlightingDataProvider _flightingDataProvider = null;

    /// <summary>
    /// The action audit log helper
    /// </summary>
    protected readonly IActionAuditLogHelper _actionAuditLogHelper = null;

    /// <summary>
    /// The table storage helper
    /// </summary>
    protected readonly ITableHelper _tableHelper;

    /// <summary>
    /// The approval tenantinfo helper.
    /// </summary>
    protected readonly IApprovalTenantInfoHelper _approvalTenantInfoHelper;

    /// <summary>
    /// The Tenant Factory.
    /// </summary>
    protected readonly ITenantFactory _tenantFactory;

    /// <summary>
    /// Attachment helper.
    /// </summary>
    protected readonly IAttachmentHelper _attachmentHelper;

    #endregion Variables

    #region Constructor

    /// <summary>
    /// Initializes a new instance of the <see cref="DocumentActionHelper"/> class.
    /// </summary>
    /// <param name="approvalSummaryProvider">The approval summary provider.</param>
    /// <param name="config">The configuration.</param>
    /// <param name="logProvider">The logger.</param>
    /// <param name="nameResolutionHelper">The name resolution helper.</param>
    /// <param name="performanceLogger">The performance logger.</param>
    /// <param name="approvalDetailProvider">The approval detail provider.</param>
    /// <param name="flightingDataProvider">The flighting data provider.</param>
    /// <param name="actionAuditLogHelper">The action audit log helper.</param>
    /// <param name="tableHelper">The table helper.</param>
    /// <param name="approvalTenantInfoHelper"> The approval tenantinfo helper.</param>
    /// <param name="tenantFactory">The tenant factory.</param>
    /// <param name="attachmentHelper">Attachment helper.</param>
    public DocumentActionHelper(
        IApprovalSummaryProvider approvalSummaryProvider,
        IConfiguration config,
        ILogProvider logProvider,
        INameResolutionHelper nameResolutionHelper,
        IPerformanceLogger performanceLogger,
        IApprovalDetailProvider approvalDetailProvider,
        IFlightingDataProvider flightingDataProvider,
        IActionAuditLogHelper actionAuditLogHelper,
        ITableHelper tableHelper,
        IApprovalTenantInfoHelper approvalTenantInfoHelper,
        ITenantFactory tenantFactory,
        IAttachmentHelper attachmentHelper)
    {
        _approvalSummaryProvider = approvalSummaryProvider;
        _config = config;
        _logger = logProvider;
        _nameResolutionHelper = nameResolutionHelper;
        _performanceLogger = performanceLogger;
        _approvalDetailProvider = approvalDetailProvider;
        _flightingDataProvider = flightingDataProvider;
        _actionAuditLogHelper = actionAuditLogHelper;
        _tableHelper = tableHelper;
        _approvalTenantInfoHelper = approvalTenantInfoHelper;
        _tenantFactory = tenantFactory;
        _attachmentHelper = attachmentHelper;
    }

    #endregion Constructor

    #region Implemented Methods

    /// <summary>
    /// Takes the action.
    /// </summary>
    /// <param name="tenantId">The tenant map identifier.</param>
    /// <param name="userActionsString">The user actions string.</param>
    /// <param name="clientDevice">The client device.</param>
    /// <param name="userAlias">The user alias.</param>
    /// <param name="loggedInUser">The logged in user.</param>
    /// <param name="aadUserToken">The Azure AD user token.</param>
    /// <param name="xcv">The xcv.</param>
    /// <param name="tcv">The TCV.</param>
    /// <param name="sessionId">The session identifier.</param>
    /// <returns>Task of JObject.</returns>
    public async Task<JObject> TakeAction(
            int tenantId,
            string userActionsString,
            string clientDevice,
            string userAlias,
            string loggedInUser,
            string aadUserToken,
            string xcv,
            string tcv,
            string sessionId)
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
            { LogDataKey.StartDateTime, DateTime.UtcNow },
            { LogDataKey.Xcv, xcv },
            { LogDataKey.Tcv, tcv },
            { LogDataKey.ReceivedTcv, tcv },
            { LogDataKey.SessionId, sessionId },
            { LogDataKey.UserRoleName, loggedInUser },
            { LogDataKey.TenantId, tenantId },
            { LogDataKey.UserAlias, userAlias },
            { LogDataKey.ClientDevice, clientDevice },
            { LogDataKey.UserActionsString, userActionsString }
        };

        #endregion Logging Prep

        try
        {
            #region Get Tenant Info

            ApprovalTenantInfo tenantInfo = _approvalTenantInfoHelper.GetTenantInfo(tenantId);

            #endregion Get Tenant Info

            #region Check if Tenant Actions are enabled

            List<TenantAction> tenantActions = (tenantInfo.ActionDetails.Primary == null ? new List<TenantAction>() : tenantInfo.ActionDetails.Primary.Where(a => a.IsEnabled))
                                                .Union(tenantInfo.ActionDetails.Secondary == null ? new List<TenantAction>() : tenantInfo.ActionDetails.Secondary.Where(a => a.IsEnabled))
                                                .ToList();
            if (tenantActions == null || tenantActions.Count == 0)
            {
                var supportEmailId = _config[ConfigurationKey.SupportEmailId.ToString()];
                if (clientDevice.Equals(Constants.OutlookClient))
                {
                    // extracts only Email ID, without "mailto"
                    var supportEmail = supportEmailId.Contains("mailto:") ? supportEmailId.Split(':')[1] : supportEmailId;
                    throw new DataException(string.Format(Constants.TenantDowntimeMessage, tenantInfo.AppName, supportEmail));
                }
                else
                {
                    throw new DataException(string.Format(Constants.TenantDowntimeMessage, tenantInfo.AppName, supportEmailId));
                }
            }

            #endregion Check if Tenant Actions are enabled

            #region Process user action string

            List<ApprovalRequest> approvalRequests = PreProcessUserActionString(userActionsString, tcv, tenantInfo);
            JObject userActionsResponseObject = null;

            #endregion Process user action string

            #region Get type of Tenant

            ITenant tenantAdaptor = _tenantFactory.GetTenant(tenantInfo, userAlias, clientDevice, aadUserToken);

            #endregion Get type of Tenant

            using (var documentActionTracer = _performanceLogger.StartPerformanceLogger("PerfLog", string.IsNullOrWhiteSpace(clientDevice) ? Constants.WebClient : clientDevice, string.Format(Constants.PerfLogAction, tenantInfo.AppName, "Document Action"), logData))
            {
                #region Process User Actions and Await Tenant and Approvals Internal Response

                userActionsResponseObject = await ProcessUserActionsAsync(
                                                            tenantAdaptor,
                                                            tenantInfo,
                                                            tenantId,
                                                            userAlias,
                                                            loggedInUser,
                                                            clientDevice,
                                                            approvalRequests,
                                                            xcv,
                                                            tcv,
                                                            sessionId);

                #endregion Process User Actions and Await Tenant and Approvals Internal Response
            }

            logData.Add(LogDataKey.BusinessProcessName, string.Format(tenantInfo.BusinessProcessName, Constants.BusinessProcessNameApprovalAction, approvalRequests.FirstOrDefault()?.Action));
            logData.Add(LogDataKey.EndDateTime, DateTime.UtcNow);
            _logger.LogInformation(TrackingEvent.WebApiDocumentActionSuccess, logData);
            return userActionsResponseObject;
        }
        catch (Exception exception)
        {
            logData.Add(LogDataKey.EndDateTime, DateTime.UtcNow);
            _logger.LogError(TrackingEvent.WebApiDocumentActionFail, exception, logData);
            throw;
        }
    }

    #endregion Implemented Methods

    #region Helper Methods

    #region Protected Methods

    /// <summary>
    /// process user actions as an asynchronous operation.
    /// </summary>
    /// <param name="tenantAdapter">The tenant adapter.</param>
    /// <param name="tenantInfo">The tenant information.</param>
    /// <param name="tenantId">The tenant map identifier.</param>
    /// <param name="alias">The alias.</param>
    /// <param name="loggedInAlias">The logged in alias.</param>
    /// <param name="clientDevice">The client device.</param>
    /// <param name="approvalRequests">The approval requests.</param>
    /// <param name="xcv">The Xcv.</param>
    /// <param name="tcv">The TCV.</param>
    /// <param name="sessionId">The session identifier.</param>
    /// <returns>Task of JObject.</returns>
    protected virtual async Task<JObject> ProcessUserActionsAsync(
            ITenant tenantAdapter,
            ApprovalTenantInfo tenantInfo,
            int tenantId,
            string alias,
            string loggedInAlias,
            string clientDevice,
            List<ApprovalRequest> approvalRequests,
            string xcv,
            string tcv,
            string sessionId)
    {
        JArray userActionsResponseArray = new JArray();
        JObject userActionsResponseObject = new JObject();

        // Resolve dependencies
        List<Task<JToken>> allTasks = new List<Task<JToken>>();

        #region Process each user action separately

        ApprovalSummaryRow approvalSummaryRow = null;
        foreach (var approvalRequest in approvalRequests)
        {
            var logData = new Dictionary<LogDataKey, object>
                    {
                        { LogDataKey.GlobalTcv, tcv },
                        { LogDataKey.Tcv, approvalRequest?.Telemetry?.Tcv },
                        { LogDataKey.ReceivedTcv, approvalSummaryRow != null ? approvalSummaryRow.Tcv : tcv },
                        { LogDataKey.SessionId, sessionId },
                        { LogDataKey.Xcv, approvalRequest?.Telemetry?.Xcv },
                        { LogDataKey.DXcv, approvalRequest.ApprovalIdentifier.DisplayDocumentNumber },
                        { LogDataKey.UserRoleName, loggedInAlias },
                        { LogDataKey.Approver, loggedInAlias },
                        { LogDataKey.BusinessProcessName, string.Format(tenantInfo.BusinessProcessName, Constants.BusinessProcessNameApprovalAction, approvalRequest.Action) },
                        { LogDataKey.TenantId, tenantId },
                        { LogDataKey.TenantName, tenantInfo.AppName },
                        { LogDataKey.UserAlias, alias },
                        { LogDataKey.DocumentNumber, approvalRequest.ApprovalIdentifier.DisplayDocumentNumber },
                        { LogDataKey.DisplayDocumentNumber, approvalRequest.ApprovalIdentifier.DisplayDocumentNumber },
                        { LogDataKey.FiscalYear, approvalRequest.ApprovalIdentifier.FiscalYear },
                        { LogDataKey.AppAction, approvalRequest.Action },
                        { LogDataKey.Operation, approvalRequest.Action }
                    };

            try
            {
                // This is done to fetch the ReceivedTcv value from SummaryRow
                approvalSummaryRow = _approvalSummaryProvider.GetApprovalSummaryByDocumentNumberAndApprover(tenantInfo.DocTypeId, approvalRequest.ApprovalIdentifier.DisplayDocumentNumber, alias);

                List<TenOpsDetails> tenantOperations = tenantInfo.DetailOperations.DetailOpsList;
                var operation = tenantOperations.Where(item => item.operationtype == Constants.OperationTypeOutOfSync).ToList().FirstOrDefault();

                // If Out of sync action taken and no endpoint is available
                if (operation == null && (approvalRequest.Action.Equals(Constants.OutOfSyncAction, StringComparison.InvariantCultureIgnoreCase) || approvalRequest.Action.Equals(Constants.UndoOutOfSyncAction, StringComparison.InvariantCultureIgnoreCase)))
                {
                    #region Update Summary record to mark the request a Out Of Sync

                    _approvalSummaryProvider.UpdateSummaryIsOutOfSyncChallenged(tenantInfo, approvalSummaryRow, DateTime.Parse(approvalRequest.ActionDetails[Constants.ActionDateKey]), approvalRequest.Action);

                    TenantAction action = tenantInfo.ActionDetails.Secondary.FirstOrDefault(a => a.Code.Equals(approvalRequest.Action.ToString(), StringComparison.InvariantCultureIgnoreCase));

                    if (action != null && action.TargetPage.FirstOrDefault() != null)
                    {
                        JObject actionJObject = new JObject
                        {
                            { Constants.TargetPageKey, action.TargetPage.FirstOrDefault().PageType },
                            { Constants.DelayTimeKey, action.TargetPage.FirstOrDefault().DelayTime.ToString() }
                        };
                        JObject content = new JObject
                        {
                            { "Content", actionJObject }
                        };
                        userActionsResponseArray.Add(content);
                    }

                    #endregion Update Summary record to mark the request a Out Of Sync
                }
                else
                {
                    Task<JToken> processUserAction = Task.Run(()
                        => ProcessUserActionAsync(
                                    tenantAdapter,
                                    tenantInfo,
                                    approvalSummaryRow,
                                    tenantId,
                                    alias,
                                    loggedInAlias,
                                    clientDevice,
                                    approvalRequest,
                                    sessionId,
                                    xcv,
                                    tcv));
                    allTasks.Add(processUserAction);
                }

                _logger.LogInformation(TrackingEvent.ProcessUserActionsSuccess, logData);
            }
            catch (Exception exception)
            {
                // Handle this exception and allow the process to continue
                // Log all information and then proceed with next documentKey in the collection
                _logger.LogError(TrackingEvent.ProcessUserActionsFailed, exception, logData);
            }
        }

        // Await on all tasks
        await Task.WhenAll(allTasks.ToArray());

        #endregion Process each user action separately

        #region For each processed user action, add appropriate response

        FormulateActionResponse(tenantInfo, clientDevice, approvalRequests, userActionsResponseArray, allTasks, out Dictionary<string, string> failedDocuments);

        #endregion For each processed user action, add appropriate response

        userActionsResponseObject.Add("ActionResponseContent", JToken.FromObject(userActionsResponseArray));
        return userActionsResponseObject;
    }

    /// <summary>
    /// Checks whether the user is authorized to take action on the request or not.
    /// If the record genuinely belongs to the user, the user will receive an appropriate message
    /// If the record is being hacked, then the checks are safe but limited to Approvals knowledge of the record because the tenants do not support custom security checks
    /// </summary>
    /// <param name="alias">Approver alias</param>
    /// <param name="approvalRequest">Approval request object</param>
    /// <param name="approvalSummaryRow">Approval summary row data</param>
    /// <param name="tenantInfo"></param>
    protected async Task CheckAuthorization(string alias, ApprovalRequest approvalRequest, ApprovalSummaryRow approvalSummaryRow, ApprovalTenantInfo tenantInfo)
    {
        if (approvalSummaryRow == null)
        {
            var actionAuditLog = (await _actionAuditLogHelper.GetActionAuditLogsByDocumentNumberAndApprover(approvalRequest.ApprovalIdentifier.DisplayDocumentNumber, alias)).OrderByDescending(a => a.ActionDateTime).FirstOrDefault();
            if (actionAuditLog != null)
            {
                // You have already taken action "{0}" on this request on {1} from {2}. No further action is required.
                throw new UnauthorizedAccessException(string.Format(_config[ConfigurationKey.ActionAlreadyTakenFromApprovalsMessage.ToString()], actionAuditLog.ActionTaken, actionAuditLog.ActionDateTime, actionAuditLog.ClientType));
            }
            else
            {
                // Action cannot be performed at this time. Unauthorized action OR action previously taken from {0} application.
                throw new UnauthorizedAccessException(string.Format(_config[ConfigurationKey.ActionAlreadyTakenMessage.ToString()], tenantInfo.AppName));
            }
        }

        if (!string.IsNullOrEmpty(approvalRequest.ActionByAlias) && !approvalRequest.ActionByAlias.Equals(alias, StringComparison.InvariantCultureIgnoreCase))
        {
            // You do not have permission to act on this approval request.
            throw new UnauthorizedAccessException(_config[ConfigurationKey.UnAuthorizedException.ToString()]);
        }
    }

    /// <summary>
    /// Checks whether the version of the request on which the user is trying to take action on matches with the one present in Approvals backend
    /// If this doesn't match, the request is a stale record and thus an invalid data exception is thrown to the caller.
    /// </summary>
    /// <param name="clientDevice">Client device (Web/ WP8/ Outlook etc.)</param>
    /// <param name="approvalRequest">Approval request object</param>
    /// <param name="approvalSummaryRow">Approval summary row data</param>
    /// <param name="tenantInfo"></param>
    protected void RequestVersionCheck(string clientDevice, ApprovalRequest approvalRequest, ApprovalSummaryRow approvalSummaryRow, ApprovalTenantInfo tenantInfo)
    {
        // ClientDevice should be passed in the header from Outlook Quick Action Service as OUTLOOK
        if (clientDevice.Equals(Constants.OutlookClient.ToUpperInvariant(), StringComparison.InvariantCultureIgnoreCase)
            && approvalRequest.ActionDetails != null
                && approvalRequest.ActionDetails.ContainsKey(Constants.RequestVersion))
        {
            if (Guid.TryParse(approvalRequest.ActionDetails[Constants.RequestVersion], out Guid requestVersion)
                && !requestVersion.Equals(approvalSummaryRow.RequestVersion))
            {
                // Details of this request has been changed in {0} application. Please use the latest request to take an action.
                throw new InvalidDataException(string.Format(_config[ConfigurationKey.InvalidRequestException.ToString()], tenantInfo.AppName));
            }
            if (!approvalRequest.ActionDetails.ContainsKey(Constants.ActionDateKey))
            {
                approvalRequest.ActionDetails[Constants.ActionDateKey] = DateTime.UtcNow.ToString("o");
            }
        }
    }

    /// <summary>
    /// Adds metadata information to the Approval Request object
    /// </summary>
    /// <param name="tenantAdapter">tenant Adapter</param>
    /// <param name="tenantInfo">approval tenant info for selected request</param>
    /// <param name="tenantId">tenant id for selected request</param>
    /// <param name="alias">Approver alias</param>
    /// <param name="loggedInAlias">logged-in alias</param>
    /// <param name="sessionId">Session Id</param>
    /// <param name="tcv">Transaction vector</param>
    /// <param name="logData">log Data</param>
    /// <param name="approvalRequest">Approval request object</param>
    /// <param name="approvalSummaryRow">Approval summary row data</param>
    protected void AddMetadataToApprovalRequest(ITenant tenantAdapter, ApprovalTenantInfo tenantInfo, int tenantId, string alias, string loggedInAlias, string sessionId, string tcv, Dictionary<LogDataKey, object> logData, ApprovalRequest approvalRequest, ApprovalSummaryRow approvalSummaryRow)
    {
        #region Get Edited details data for given document

        ApprovalDetailsEntity editedDetailsRow = _approvalDetailProvider.GetApprovalsDetails(tenantId, approvalRequest.ApprovalIdentifier.DisplayDocumentNumber, Constants.EditedDetailsOperationType + "|" + alias);
        JObject detailsData = null;
        if (editedDetailsRow != null)
        {
            detailsData = JObject.Parse(editedDetailsRow.JSONData);
        }

        #endregion Get Edited details data for given document

        var editableFieldAuditLog = _tableHelper.GetTableEntityListByPartitionKey<EditableFieldAuditEntity>(Constants.EditableFieldAuditLogs, approvalRequest.ApprovalIdentifier.GetDocNumber(tenantInfo));

        lock (approvalRequest)
        {
            #region Add delegation information

            approvalRequest.ActionByDelegateInMSApprovals = loggedInAlias;

            // This is done just to handle older requests where the OriginalApprovers is stored as 'null' as a string type
            if (approvalSummaryRow.OriginalApprovers != null && approvalSummaryRow.OriginalApprovers.Equals("null", StringComparison.InvariantCultureIgnoreCase))
            {
                approvalSummaryRow.OriginalApprovers = string.Empty;
            }

            if (!string.IsNullOrEmpty(approvalSummaryRow.OriginalApprovers))
            {
                approvalRequest.OriginalApproverInTenantSystem = approvalSummaryRow.OriginalApprovers.FromJson<List<string>>().FirstOrDefault();
            }

            #endregion Add delegation information

            #region Adding the final form of editable fields

            // Adding editable fields in ApprovalRequest -> AdditionalData
            if (detailsData != null)
            {
                var property = Constants.EditableField;
                var editableFields = new JArray();
                var editableFieldsAuditTrail = new JArray();
                AddEditableFieldsToRequest(detailsData.ToString().ToJToken(), property, ref editableFields);
                AddEditableFieldsToRequest(detailsData[Constants.LineItems]?.ToString()?.ToJToken(), property, ref editableFields);
                AddEditableFieldsTrailToRequest(editableFieldAuditLog, editableFieldsAuditTrail);

                // Add the editable fields data to AdditionalData as a key value pair
                JObject additionalData = new JObject
                {
                    { Constants.ApprovalIdentifier, approvalRequest.ApprovalIdentifier.ToJson().ToJToken() },
                    { Constants.EditableFields, editableFields.ToJson().ToJToken() }
                };

                approvalRequest.AdditionalData = new Dictionary<string, string>
                {
                    { Constants.EditableFields, additionalData.ToJson() },
                    { Constants.EditableFieldsAuditTrail, editableFieldsAuditTrail.ToJson() }
                };
            }

            #endregion Adding the final form of editable fields

            approvalRequest.AdditionalData = tenantAdapter.GetAdditionalData(approvalRequest.AdditionalData, approvalRequest.ApprovalIdentifier.GetDocNumber(tenantInfo), tenantId);

            logData[LogDataKey.UserActionsString] = approvalRequest.ToJson();
            logData[LogDataKey.DXcv] = approvalRequest.ApprovalIdentifier.DisplayDocumentNumber;
            logData[LogDataKey.Xcv] = approvalRequest?.Telemetry?.Xcv;
            logData[LogDataKey.Tcv] = approvalRequest?.Telemetry?.Tcv;
            logData[LogDataKey.GlobalTcv] = tcv;
            logData[LogDataKey.ReceivedTcv] = approvalSummaryRow.Tcv;
        }
    }

    /// <summary>
    /// Creating Additional information data ,audit trail for editable fields we need to send to tenant
    /// </summary>
    /// <param name="editableFieldAuditLogList">List of audit log entities for editable fields</param>
    /// <param name="editableFieldsTrail">JArray we need to send to tenant</param>
    protected void AddEditableFieldsTrailToRequest(IEnumerable<EditableFieldAuditEntity> editableFieldAuditLogList, JArray editableFieldsTrail)
    {
        if (editableFieldsTrail == null)
        {
            editableFieldsTrail = new JArray();
        }
        if (editableFieldAuditLogList != null && editableFieldAuditLogList.Any())
        {
            foreach (var auditLog in editableFieldAuditLogList)
            {
                editableFieldsTrail.Add((auditLog.ToJson().FromJson<EditableFieldAuditLogEntity>()).ToJToken());
            }
        }
    }

    /// <summary>
    /// Adding "property" (EditableField) recursively into ApprovalRequest object in AdditionalData
    /// </summary>
    /// <param name="source">Source from which the property is to be added</param>
    /// <param name="property">Name of the property which is to be added</param>
    /// <param name="target">Target where the property is to be added</param>
    protected void AddEditableFieldsToRequest(JToken source, string property, ref JArray target)
    {
        if (target == null)
        {
            target = new JArray();
        }

        if (source is JObject)
        {
            var requestDetails = JObject.Parse(source.ToString());
            if (requestDetails[property] != null)
            {
                target.Add(requestDetails[property]);
            }
        }
        else if (source is JArray)
        {
            var requestDetails = JArray.Parse(source.ToString());
            foreach (var detail in requestDetails)
            {
                if (detail[property] != null)
                {
                    target.Add(detail[property]);
                }

                if (detail["Children"] != null && !string.IsNullOrEmpty(detail["Children"].ToString()))
                {
                    AddEditableFieldsToRequest(detail["Children"], property, ref target);
                }
            }
        }
    }

    /// <summary>
    /// Processes the ActionResponse response received from the LOB application and forms the final content which needs to be sent to the Approvals client side code
    /// </summary>
    /// <param name="tenantInfo">The approval tenant information object</param>
    /// <param name="tenantId">The Tenant Id</param>
    /// <param name="alias">Delegated user alias</param>
    /// <param name="loggedInAlias">Logged in alias</param>
    /// <param name="clientDevice">The client device</param>
    /// <param name="approvalRequests">List of Approval requests</param>
    /// <param name="tcv">TCV value</param>
    /// <param name="actionResponse">Action response for bulk action</param>
    /// <param name="bTenantCallSuccess">Success status flag</param>
    /// <param name="tenantAdapter">The tenant adapter object</param>
    /// <param name="actionResponseContentObject">Action response object returned to client side code</param>
    /// <param name="userActionsResponseArray">Action response array returned to client side code</param>
    /// <param name="summaryJsonList">List of summaryJson for logging into Cosmos DB</param>
    /// <param name="sessionId">The sessionId</param>
    /// <param name="bIsHandledException">The bIsHandledException</param>
    protected virtual async Task PostProcessActionResponses(
        ApprovalTenantInfo tenantInfo,
        int tenantId,
        string alias,
        string loggedInAlias,
        string clientDevice,
        List<ApprovalRequest> approvalRequests,
        string tcv,
        HttpResponseMessage actionResponse,
        bool bTenantCallSuccess,
        ITenant tenantAdapter,
        JObject actionResponseContentObject,
        JArray userActionsResponseArray,
        List<SummaryJson> summaryJsonList,
        string sessionId,
        bool bIsHandledException = false)

    {
        var approvalRequest = approvalRequests.FirstOrDefault();
        var summaryJson = summaryJsonList.FirstOrDefault();
        if (approvalRequest == null)
        {
            throw new InvalidDataException("Invalid ApprovalRequest object");
        }

        var logData = new Dictionary<LogDataKey, object>
            {
                { LogDataKey.ClientDevice, clientDevice },
                { LogDataKey.IsCriticalEvent, CriticalityLevel.Yes.ToString() },
                { LogDataKey.ReceivedTcv, tcv },
                { LogDataKey.SessionId, sessionId },
                { LogDataKey.UserRoleName, loggedInAlias },
                { LogDataKey.Approver, alias },
                { LogDataKey.BusinessProcessName, string.Format(tenantInfo?.BusinessProcessName, Constants.BusinessProcessNameApprovalAction, approvalRequest?.Action) },
                { LogDataKey.TenantId, tenantId },
                { LogDataKey.TenantName, tenantInfo?.AppName },
                { LogDataKey.UserAlias, alias },
                { LogDataKey.DocumentNumber, approvalRequest?.ApprovalIdentifier?.DisplayDocumentNumber },
                { LogDataKey.DisplayDocumentNumber, approvalRequest?.ApprovalIdentifier?.DisplayDocumentNumber },
                { LogDataKey.DXcv, approvalRequest?.ApprovalIdentifier?.DisplayDocumentNumber },
                { LogDataKey.FiscalYear, approvalRequest?.ApprovalIdentifier?.FiscalYear },
                { LogDataKey.AppAction, approvalRequest?.Action },
                { LogDataKey.Operation, approvalRequest?.Action },
                { LogDataKey.DocumentTypeId, tenantInfo?.DocTypeId }
            };

        #region Extract required information from documentKey and add to action response content object

        JToken documentKeys = JToken.FromObject(new { approvalRequest.ApprovalIdentifier.DocumentNumber, approvalRequest.ApprovalIdentifier.DisplayDocumentNumber });
        actionResponseContentObject.Add(Constants.DocumentKeys, documentKeys);

        #endregion Extract required information from documentKey and add to action response content object

        #region Process Tenant Response for success and failure cases

        var actionDateTime = Convert.ToDateTime(approvalRequest.ActionDetails[approvalRequest.ActionDetails.Keys.FirstOrDefault(x => x.ToLowerInvariant() == Constants.ActionDateKey.ToLowerInvariant())]);

            // Checking the responses received from tenantAdapter and creating
            if (actionResponse.IsSuccessStatusCode)
            {
                var actionAuditLogInfo = new ActionAuditLogInfo
                {
                    DisplayDocumentNumber = approvalRequest?.ApprovalIdentifier?.GetDocNumber(tenantInfo),
                    ActionDateTime = actionDateTime.ToUniversalTime().ToString("o"),
                    ActionStatus = Constants.SuccessStatus,
                    ActionTaken = approvalRequest?.Action,
                    Approver = alias,
                    ImpersonatedUser = loggedInAlias,
                    ClientType = clientDevice,
                    TenantId = tenantInfo?.TenantId.ToString(),
                    UnitValue = summaryJson?.UnitValue ?? string.Empty,
                    UnitOfMeasure = summaryJson?.UnitOfMeasure ?? string.Empty,
                    Id = Guid.NewGuid()
                };

            await LogActionDetailsAsync(actionAuditLogInfo, tenantInfo, clientDevice, tcv, sessionId, loggedInAlias, alias);

            var approvalResponse = tenantAdapter.ParseResponseString<List<ApprovalResponse>>(await actionResponse.Content.ReadAsStringAsync()).FirstOrDefault();
            logData.Add(LogDataKey.Tcv, approvalResponse?.Telemetry?.Tcv);
            logData.Add(LogDataKey.Xcv, approvalResponse?.Telemetry?.Xcv);

            _logger.LogInformation(TrackingEvent.DocumentActionSuccess, logData);

            // Adds the Target page information to the action response object which is passed to the client side
            // Based on the TargetPage property and delay time, the UI either navigates to the Summary or calling page or stays on the details page
            AddTargetPageInformation(tenantInfo, actionResponseContentObject, approvalRequest);
        }
        else if (!actionResponse.IsSuccessStatusCode || !bTenantCallSuccess)
        {
            List<ApprovalSummaryRow> summaryRowsToUpdateInBatch = new List<ApprovalSummaryRow>();
            List<ApprovalDetailsEntity> detailsRowsToUpdateInBatch = new List<ApprovalDetailsEntity>();

            // The actionResponse will always contain List<ApprovalResponse> as it is handled in TenantBase.cs
            var approvalResponse = tenantAdapter.ParseResponseString<List<ApprovalResponse>>(await actionResponse.Content.ReadAsStringAsync()).FirstOrDefault();

            logData.Add(LogDataKey.Tcv, approvalResponse?.Telemetry?.Tcv);
            logData.Add(LogDataKey.Xcv, approvalResponse?.Telemetry?.Xcv);

            // Add Tracking ID to the action response string.
            if (actionResponseContentObject.Property("ErrorMessage") == null)
            {
                // DisplayMessage will always contain value
                actionResponseContentObject.Add("ErrorMessage", "Tracking ID:" + tcv + " :: " + approvalResponse?.DisplayMessage + ".");
            }

                // Failed Telemtry should receive the response received from Tenant
                var actionAuditLogInfo = new ActionAuditLogInfo
                {
                    DisplayDocumentNumber = approvalRequest?.ApprovalIdentifier?.GetDocNumber(tenantInfo),
                    ActionDateTime = actionDateTime.ToUniversalTime().ToString("o"),
                    ActionStatus = Constants.FailedStatus,
                    ActionTaken = approvalRequest?.Action,
                    Approver = alias,
                    ImpersonatedUser = loggedInAlias,
                    ClientType = clientDevice,
                    TenantId = tenantInfo?.TenantId.ToString(),
                    UnitValue = summaryJson?.UnitValue ?? string.Empty,
                    UnitOfMeasure = summaryJson?.UnitOfMeasure ?? string.Empty,
                    ErrorMessage = approvalResponse?.E2EErrorInformation?.ErrorMessages?.ToJson(),
                    Id = Guid.NewGuid()
                };
                await LogActionDetailsAsync(actionAuditLogInfo, tenantInfo, clientDevice, tcv, sessionId, loggedInAlias, alias);

            // bTenantCallSuccess is false whenever there is an exception either from the tenant (legacy wcf) or Approvals code
            // The below code updates the all the transactional details (which includes error messages) in the Approval Summary/ApprovalDetails table
            if (!actionResponse.IsSuccessStatusCode || !bTenantCallSuccess)
            {
                Tuple<ApprovalSummaryRow, List<ApprovalDetailsEntity>> returnTuple = tenantAdapter.UpdateTransactionalDetails(approvalRequest, false, actionResponseContentObject["ErrorMessage"]?.ToString(), loggedInAlias, clientDevice, null);

                if (returnTuple != null)
                {
                    if (returnTuple.Item1 != null)
                    {
                        summaryRowsToUpdateInBatch.Add(returnTuple.Item1);
                    }

                    if (returnTuple.Item2 != null)
                    {
                        detailsRowsToUpdateInBatch.AddRange(returnTuple.Item2);
                    }
                }
            }

            if (summaryRowsToUpdateInBatch.Count > 0)
            {
                await _approvalSummaryProvider.UpdateSummaryInBatchAsync(summaryRowsToUpdateInBatch, tcv, sessionId, tcv,
                    tenantInfo, approvalRequest.Action);
            }

            if (detailsRowsToUpdateInBatch.Count > 0)
            {
                await _approvalDetailProvider.UpdateDetailsInBatchAsync(detailsRowsToUpdateInBatch, tcv, sessionId, tcv,
                    tenantInfo,
                    approvalRequest.Action);
            }

            _logger.LogError(TrackingEvent.DocumentActionFailure, new Exception(approvalResponse?.E2EErrorInformation?.ErrorMessages?.ToJson()), logData);
        }

        #endregion Process Tenant Response for success and failure cases
    }

    /// <summary>
    /// Adds the Target page information to the action response object which is passed to the client side
    /// Based on the TargetPage property and delay time, the UI either navigates to the Summary / calling page or stays on the details page
    /// </summary>
    /// <param name="tenantInfo">The Approval Tenant Info object</param>
    /// <param name="actionResponseContentObject">Action Response content JObject</param>
    /// <param name="approvalRequest">The Approval Request</param>
    /// <returns>the modified ActionResponseContent JObject</returns>
    protected JObject AddTargetPageInformation(ApprovalTenantInfo tenantInfo, JObject actionResponseContentObject, ApprovalRequest approvalRequest)
    {
        TenantAction action = tenantInfo.ActionDetails.Primary.FirstOrDefault(a => a.Code.Equals(approvalRequest.Action.ToString(), StringComparison.InvariantCultureIgnoreCase));
        if (action == null)
        {
            action = tenantInfo.ActionDetails.Secondary.FirstOrDefault(a => a.Code.Equals(approvalRequest.Action.ToString(), StringComparison.InvariantCultureIgnoreCase));
        }

        if (action?.TargetPage != null)
        {
            foreach (TenantActionTargetPage targetPage in action.TargetPage)
            {
                if (ValidateCondition(targetPage.Condition, approvalRequest))
                {
                    JObject targetPageInformation = new JObject
                    {
                        { Constants.TargetPageKey, targetPage.PageType },
                        { Constants.DelayTimeKey, targetPage.DelayTime.ToString() }
                    };
                    actionResponseContentObject.Add("Content", targetPageInformation);
                    break;
                }
            }
        }

        return actionResponseContentObject;
    }

    /// <summary>
    /// This method formulates the response after the action is complete (success/fail) which is sent to the client side code.
    /// This is a generalized implementation for single as well as bulk approval across all device/platforms
    /// </summary>
    /// <param name="tenantInfo">The tenant information.</param>
    /// <param name="clientDevice">The client device.</param>
    /// <param name="approvalRequests">The approval requests.</param>
    /// <param name="userActionsResponseArray">The user actions response array.</param>
    /// <param name="allTasks">All tasks.</param>
    /// <param name="failedDocuments">The failed documents.</param>
    protected void FormulateActionResponse(ApprovalTenantInfo tenantInfo, string clientDevice, List<ApprovalRequest> approvalRequests, JArray userActionsResponseArray, List<Task<JToken>> allTasks, out Dictionary<string, string> failedDocuments)
    {
        var action = approvalRequests.FirstOrDefault().Action;
        Dictionary<bool, object> cumulativeResponse = new Dictionary<bool, object>();
        Dictionary<string, string> successfulDocuments = new Dictionary<string, string>();
        failedDocuments = ExtractDataFromActionResponse(userActionsResponseArray, allTasks).Result;

        cumulativeResponse.Add(false, JArray.FromObject(failedDocuments.ToList()));

        // Get the list of successful documentNumber
        foreach (var failedItem in failedDocuments)
        {
            approvalRequests.RemoveAll((x) => x.ApprovalIdentifier.DisplayDocumentNumber == failedItem.Key);
        }

        // Add the successful document details into a dictionary similar to that of failedDocuments
        foreach (var successfulItem in approvalRequests)
        {
            successfulDocuments.Add(successfulItem.ApprovalIdentifier.DisplayDocumentNumber, Constants.ActionSuccessfulMessage);
        }

        cumulativeResponse.Add(true, JArray.FromObject(successfulDocuments.ToList()));

        // The response needs to be sent for all platforms except Webjob
        if (failedDocuments.Count > 0)
        {
            var errorMessage = string.Join(",",
                failedDocuments.Select(f => tenantInfo.DocumentNumberPrefix + ": " + f.Key + " - " + f.Value));
            if (clientDevice.Equals(Constants.WebClient) || clientDevice.Equals(Constants.OutlookClient) || clientDevice.Equals(Constants.TeamsClient) || clientDevice.Equals(Constants.ReactClient))
            {
                var completeExceptionInfo = new JObject
                {
                    { "ErrorMessage", errorMessage },
                    { "ApprovalResponseDetails", JToken.FromObject(cumulativeResponse) }
                };

                if (clientDevice.Equals(Constants.OutlookClient))
                {
                    var tenantFailureResponseObj = completeExceptionInfo["ApprovalResponseDetails"]["False"]?.ToJson()?.ToJArray();
                    throw new InvalidOperationException(tenantFailureResponseObj[0]["Value"].ToString());
                }
                else
                {
                    throw new InvalidOperationException(completeExceptionInfo.ToJson());
                }
            }
            else
            {
                // response for phone devices which do not support background approval at this moment. Currently includes Windows Phone 8 app and Xamarin App
                throw new InvalidOperationException(errorMessage);
            }
        }
    }

    /// <summary>
    /// Extracts the failed requests data from Action Response for Single/Pseudo actions
    /// </summary>
    /// <param name="userActionsResponseArray">the user actions response array.</param>
    /// <param name="allTasks">all task threads which are running each individual actions.</param>
    /// <returns>Returns a dictionary of string, string</returns>
    protected virtual async Task<Dictionary<string, string>> ExtractDataFromActionResponse(JArray userActionsResponseArray, List<Task<JToken>> allTasks)
    {
        var failedDocuments = new Dictionary<string, string>();
        foreach (Task<JToken> t in allTasks)
        {
            JToken result = await t;
            userActionsResponseArray.Add(result);
            if (result["ErrorMessage"] != null
                && result[Constants.DocumentKeys] != null && result[Constants.DocumentKeys].Type != JTokenType.Null && result[Constants.DocumentKeys][Constants.DisplayDocumentNumber] != null && result[Constants.DocumentKeys][Constants.DisplayDocumentNumber].Type != JTokenType.Null)
            {
                failedDocuments.Add(result[Constants.DocumentKeys][Constants.DisplayDocumentNumber].ToString(), " Error : " + result["ErrorMessage"].ToString());
            }
        }

        return failedDocuments;
    }

    /// <summary>
    /// Log the message progress
    /// </summary>
    /// <param name="trackingEvent">Tracking Event for the logging</param>
    /// <param name="failureData">Failure data object which contains information about the reason of failure</param>
    /// <param name="criticalityLevel">Criticality level (Yes- if core functionality else No)</param>
    /// <param name="tenantLogData">Log data</param>
    protected void LogMessageProgress(TrackingEvent trackingEvent, FailureData failureData, CriticalityLevel criticalityLevel, Dictionary<LogDataKey, object> tenantLogData = null)
    {
        if (tenantLogData == null)
        {
            tenantLogData = new Dictionary<LogDataKey, object>();
        }

        tenantLogData[LogDataKey.IsCriticalEvent] = criticalityLevel.ToString();
        tenantLogData[LogDataKey.LocalTime] = DateTime.UtcNow;
        if (failureData != null)
        {
            tenantLogData[LogDataKey.ResponseContent] = failureData.Message;
        }
        tenantLogData[LogDataKey.FailureData] = failureData;
        tenantLogData[LogDataKey.OperationType] = "ApprovalAction";

        // overwrite TenantId as a work around to store DocumentTypeId
        tenantLogData[LogDataKey.TenantId] = tenantLogData[LogDataKey.DocumentTypeId];

        _logger.LogInformation(trackingEvent, tenantLogData);
    }

    /// <summary>
    /// This method will accepts list of actionAuditLog and logs into action audit log.
    /// </summary>
    /// <param name="actionAuditLogs">List of actionAuditLogs</param>
    /// <param name="approvalTenantInfo">The approvalTenantInfo</param>
    /// <param name="clientDevice">The clientDevice</param>
    /// <param name="tcv">The tcv</param>
    /// <param name="sessionId">The sessionId</param>
    /// <param name="loggedInAlias">The loggedInAlias</param>
    /// <param name="alias">The alias</param>
    protected async Task LogActionDetailsAsync(List<ActionAuditLogInfo> actionAuditLogs,
                                                ApprovalTenantInfo approvalTenantInfo,
                                                string clientDevice,
                                                string tcv,
                                                string sessionId,
                                                string loggedInAlias,
                                                string alias)
    {
        var logData = new Dictionary<LogDataKey, object>
        {
            { LogDataKey.StartDateTime, DateTime.UtcNow },
            { LogDataKey.ClientDevice, clientDevice },
            { LogDataKey.IsCriticalEvent, CriticalityLevel.No.ToString() },
            { LogDataKey.Tcv, tcv },
            { LogDataKey.Xcv, tcv },
            { LogDataKey.SessionId, sessionId },
            { LogDataKey.UserRoleName, loggedInAlias },
            { LogDataKey.Approver, alias },
            { LogDataKey.TenantId, approvalTenantInfo?.TenantId },
            { LogDataKey.TenantName, approvalTenantInfo?.AppName },
            { LogDataKey.UserAlias, alias },
            { LogDataKey.DocumentTypeId, approvalTenantInfo?.DocTypeId }
        };

        try
        {
            await _actionAuditLogHelper.LogActionDetailsAsync(actionAuditLogs);
            logData.Add(LogDataKey.EndDateTime, DateTime.UtcNow);
            _logger.LogInformation(TrackingEvent.LogActionAuditSuccess, logData);
        }
        catch (Exception ex)
        {
            logData.Modify(LogDataKey.EndDateTime, DateTime.UtcNow);
            _logger.LogError(TrackingEvent.LogActionAuditFailure, ex, logData);
        }
    }

    /// <summary>
    /// This method will accepts list of actionAuditLog and logs into action audit log.
    /// </summary>
    /// <param name="approvalTenantInfo">The approvalTenantInfo</param>
    /// <param name="clientDevice">The clientDevice</param>
    /// <param name="tcv">The tcv</param>
    /// <param name="sessionId">The sessionId</param>
    /// <param name="loggedInAlias">The loggedInAlias</param>
    /// <param name="alias">The alias</param>
    protected async Task LogActionDetailsAsync(ActionAuditLogInfo actionAuditLog,
                                                ApprovalTenantInfo approvalTenantInfo,
                                                string clientDevice,
                                                string tcv,
                                                string sessionId,
                                                string loggedInAlias,
                                                string alias)
    {
        await LogActionDetailsAsync(new List<ActionAuditLogInfo>() { actionAuditLog },
                                    approvalTenantInfo,
                                    clientDevice,
                                    tcv,
                                    sessionId,
                                    loggedInAlias,
                                    alias);
    }

    #endregion Protected Methods

    #region Private Methods

    /// <summary>
    /// Pre-processes, validates and sanitizes the User action input string
    /// </summary>
    /// <param name="userActionsString">Request body which is pre-processed to form List of ApprovalRequest objects</param>
    /// <param name="tcv">GUID transaction correlation vector for telemetry and logging</param>
    /// <param name="tenantInfo">ApprovalTenantInfo object for the current tenant</param>
    /// <returns>List of ApprovalRequest which is sent to the LoB application as part of the content in the Http call</returns>
    protected virtual List<ApprovalRequest> PreProcessUserActionString(
        string userActionsString,
        string tcv,
        ApprovalTenantInfo tenantInfo)
    {
        #region Validate the input string

        List<ApprovalRequest> approvalRequests = new List<ApprovalRequest>();
        if (string.IsNullOrWhiteSpace(userActionsString) || !userActionsString.IsJson())
        {
            throw new InvalidDataException("Action string is not valid: " + userActionsString);
        }

        #endregion Validate the input string

        #region Standardize the input string to List<ApprovalRequest>

        var actionObject = userActionsString.FromJson<JObject>();
        var listOfApprovalIdentifier = actionObject[Constants.DocumentKeys]?.ToString().FromJson<JArray>(); // once changed from client side, this can change to jActionObject["ApprovalIdentifier"]
        actionObject.Remove(Constants.DocumentKeys);
        foreach (var approvalIdentifier in listOfApprovalIdentifier)
        {
            actionObject.Add(Constants.ApprovalIdentifier, approvalIdentifier);
            ApprovalRequest approvalRequest = actionObject?.ToString().FromJson<ApprovalRequest>();

            #region Need to Remove this code once outlook team fix desktop outlook bug to apply default selection in dropdown when default value is serialize json string

            // TODO : Need to Remove this code once outllok team fix the bug. Applying  temparory fix to apply dropdown default selection while default value contain serialize json string.
            // Due to outlook desktop bug dropdown default selection could not get apply while default value contain json serialize string with traillin comma(,) after each key and value.
            // to fix default selection issue replacing (,) to (;) while prepating adaptive card paylod for actionable email.
            // here applying logic to do reverse replacement from (;) to (,) to avoid error while deserializing jsomg string to any object.
            if (approvalRequest != null)
            {
                if (approvalRequest.ActionDetails != null && approvalRequest.ActionDetails.ContainsKey("NextApprover"))
                {
                    approvalRequest.ActionDetails["NextApprover"] = approvalRequest.ActionDetails["NextApprover"]?.Replace(";", ",");
                }
            }

            #endregion Need to Remove this code once outlook team fix desktop outlook bug to apply default selection in dropdown when default value is serialize json string

            string xcv = approvalRequest.ApprovalIdentifier.DisplayDocumentNumber;
            if (approvalIdentifier["Xcv"] != null)
            {
                xcv = approvalIdentifier["Xcv"].ToString();
            }

            approvalRequest.Telemetry = new ApprovalsTelemetry()
            {
                Xcv = xcv,
                Tcv = tcv,
                BusinessProcessName = string.Format(tenantInfo.BusinessProcessName, Constants.BusinessProcessNameApprovalAction, approvalRequest.Action)
            };
            approvalRequests.Add(approvalRequest);
            actionObject.Remove(Constants.ApprovalIdentifier);
        }

        #endregion Standardize the input string to List<ApprovalRequest>

        #region Validate the input (List<ApprovalRequest>) after standardization

        if (!approvalRequests.Any())
        {
            throw new InvalidDataException("Action string is not valid: " + userActionsString);
        }

        #endregion Validate the input (List<ApprovalRequest>) after standardization

        return approvalRequests;
    }

    /// <summary>
    /// Process user action as an asynchronous operation.
    /// </summary>
    /// <param name="tenantAdapter">The tenant adapter.</param>
    /// <param name="tenantInfo">The tenant information.</param>
    /// <param name="approvalSummaryRow">The approval summary row</param>
    /// <param name="tenantId">The tenant map identifier.</param>
    /// <param name="alias">The alias.</param>
    /// <param name="loggedInAlias">The logged in alias.</param>
    /// <param name="clientDevice">The client device.</param>
    /// <param name="approvalRequest">The approval request.</param>
    /// <param name="sessionId">The session identifier.</param>
    /// <param name="xcv">The Xcv.</param>
    /// <param name="tcv">The Tcv.</param>
    /// <returns>Task of JToken.</returns>
    private async Task<JToken> ProcessUserActionAsync(
                                ITenant tenantAdapter,
                                ApprovalTenantInfo tenantInfo,
                                ApprovalSummaryRow approvalSummaryRow,
                                int tenantId,
                                string alias,
                                string loggedInAlias,
                                string clientDevice,
                                ApprovalRequest approvalRequest,
                                string sessionId,
                                string xcv,
                                string tcv
                                )
    {
        #region Logging Prep

        var logData = new Dictionary<LogDataKey, object>
            {
                { LogDataKey.ClientDevice, clientDevice },
                { LogDataKey.IsCriticalEvent, CriticalityLevel.Yes.ToString() },
                { LogDataKey.GlobalTcv, tcv },
                { LogDataKey.Tcv, approvalRequest?.Telemetry?.Tcv },
                { LogDataKey.ReceivedTcv, tcv },
                { LogDataKey.SessionId, sessionId },
                { LogDataKey.Xcv, approvalRequest?.Telemetry?.Xcv },
                { LogDataKey.DXcv, approvalRequest?.ApprovalIdentifier?.DisplayDocumentNumber },
                { LogDataKey.UserRoleName, loggedInAlias },
                { LogDataKey.Approver, alias },
                { LogDataKey.BusinessProcessName, string.Format(tenantInfo?.BusinessProcessName, Constants.BusinessProcessNameApprovalAction, approvalRequest?.Action) },
                { LogDataKey.TenantId, tenantId },
                { LogDataKey.TenantName, tenantInfo?.AppName },
                { LogDataKey.UserAlias, alias },
                { LogDataKey.DocumentNumber, approvalRequest?.ApprovalIdentifier?.DisplayDocumentNumber },
                { LogDataKey.DisplayDocumentNumber, approvalRequest?.ApprovalIdentifier?.DisplayDocumentNumber },
                { LogDataKey.FiscalYear, approvalRequest?.ApprovalIdentifier?.FiscalYear },
                { LogDataKey.AppAction, approvalRequest?.Action },
                { LogDataKey.Operation, approvalRequest?.Action },
                { LogDataKey.DocumentTypeId, tenantInfo?.DocTypeId }
            };

        #endregion Logging Prep

        #region Setting up default ApprovalResponse

        // Setup Generic Error message based on device type
        tenantAdapter.SetupGenericErrorMessage(approvalRequest);
        var approvalResponses = new List<ApprovalResponse>()
        {
            new ApprovalResponse()
            {
                ActionResult = false,
                DisplayMessage = tenantAdapter.GenericErrorMessage,
                ApprovalIdentifier = approvalRequest.ApprovalIdentifier,
                DocumentTypeID = approvalRequest.DocumentTypeID
            }
        };

        #endregion Setting up default ApprovalResponse

        // SummaryJson list to be passed for ActionAuditLogs in CosmosDB
        List<SummaryJson> summaryJsonList = new List<SummaryJson>();
        List<ApprovalSummaryRow> summaryRowsToUpdateInBatch = new List<ApprovalSummaryRow>();
        List<ApprovalDetailsEntity> detailsRowsToUpdateInBatch = new List<ApprovalDetailsEntity>();
        HttpResponseMessage actionResponse = null;
        bool bTenantCallSuccess = false; // Boolean value for checking tenant call status
        bool bIsHandledException = false; // Boolean value for checking Handled Exception for RequestVersion and UnAuthorization
        JObject actionResponseContentObject = new JObject();

        try
        {
            // Authorization check
            // If this fails, user gets a 401 status code (Unauthorized)
            await CheckAuthorization(alias, approvalRequest, approvalSummaryRow, tenantInfo);

            // Version check
            // If this fails, user gets a 400 status code (Invalid data - Stale request)
            RequestVersionCheck(clientDevice, approvalRequest, approvalSummaryRow, tenantInfo);

            LogMessageProgress(TrackingEvent.ApprovalActionInitiated, null, CriticalityLevel.Yes, logData);

            // Add metadata like Edited information and delegation information to the approval request object
            AddMetadataToApprovalRequest(tenantAdapter, tenantInfo, tenantId, alias, loggedInAlias, sessionId, tcv, logData, approvalRequest, approvalSummaryRow);

            // Mark the record as soft-deleted
            Tuple<ApprovalSummaryRow, List<ApprovalDetailsEntity>> returnTuple = tenantAdapter.UpdateTransactionalDetails(approvalRequest, true, string.Empty, loggedInAlias, sessionId, approvalSummaryRow);

            if (returnTuple != null)
            {
                if (returnTuple.Item1 != null)
                {
                    summaryRowsToUpdateInBatch.Add(returnTuple.Item1);
                }

                if (returnTuple.Item2 != null)
                {
                    detailsRowsToUpdateInBatch.AddRange(returnTuple.Item2);
                }
            }

            if (summaryRowsToUpdateInBatch.Count > 0)
            {
                await _approvalSummaryProvider.UpdateSummaryInBatchAsync(summaryRowsToUpdateInBatch, approvalRequest.Telemetry.Xcv, sessionId,
                   approvalRequest.Telemetry.Tcv, tenantInfo, approvalRequest.Action);
            }

            if (detailsRowsToUpdateInBatch.Count > 0)
            {
                await _approvalDetailProvider.UpdateDetailsInBatchAsync(detailsRowsToUpdateInBatch, approvalRequest.Telemetry.Xcv, sessionId,
                    approvalRequest.Telemetry.Tcv, tenantInfo, approvalRequest.Action);
            }

            // Adding this for logging in ActionAuditLogs in CosmosDB
            summaryJsonList.Add(approvalSummaryRow.SummaryJson.FromJson<SummaryJson>());

            #region Call the tenant service to submit document action and receive response.

            if (tenantInfo.AppendAdditionaDataToUserActionString)
            {
                var summary = approvalSummaryRow.SummaryJson.FromJson<SummaryJson>();
                if (approvalRequest?.AdditionalData == null)
                {
                    approvalRequest.AdditionalData = new Dictionary<string, string>();
                    approvalRequest.AdditionalData = summary?.AdditionalData;
                }
                else if (summary?.AdditionalData != null)
                {
                    foreach (var additionaData in summary?.AdditionalData)
                    {
                        if (!approvalRequest.AdditionalData.ContainsKey(additionaData.Key))
                            approvalRequest.AdditionalData.Add(additionaData.Key, additionaData.Value);
                    }
                }
            }

            // Add the blob urls for the user document as an additional data for tenants to consume.
            if (tenantInfo.IsUploadAttachmentsEnabled)
                AddUserDocumentsInfo(approvalSummaryRow, tenantId, approvalRequest);

            // Making a call into the async method which in turn will call tenant service to post user action and wait for result
            var approvalRequests = new List<ApprovalRequest>() { approvalRequest };
            actionResponse = await tenantAdapter.ExecuteActionAsync(approvalRequests, loggedInAlias, sessionId, clientDevice, xcv, tcv, approvalSummaryRow);

            #endregion Call the tenant service to submit document action and receive response.

            bTenantCallSuccess = true;

            if (actionResponse != null)
            {
                if (actionResponse.IsSuccessStatusCode)
                {
                    LogMessageProgress(TrackingEvent.ApprovalActionSuccessfullyCompleted, null, CriticalityLevel.Yes, logData);
                }
                else
                {
                    LogMessageProgress(TrackingEvent.ApprovalActionFailed, new FailureData() { Message = await actionResponse.Content.ReadAsStringAsync() }, CriticalityLevel.Yes, logData);
                }
            }
        }
        catch (UnauthorizedAccessException unauthorizedException)
        {
            bTenantCallSuccess = false;
            bIsHandledException = true;

            logData.Modify(LogDataKey.EventId, (int)TrackingEvent.DocumentActionFailure);
            logData.Modify(LogDataKey.EventName, TrackingEvent.DocumentActionFailure.ToString());
            _logger.LogError(TrackingEvent.DocumentActionFailure, unauthorizedException, logData);
            LogMessageProgress(TrackingEvent.ApprovalActionFailed, new FailureData() { Message = unauthorizedException.Message }, CriticalityLevel.Yes, logData);
            approvalResponses.FirstOrDefault().E2EErrorInformation = new ApprovalResponseErrorInfo { ErrorMessages = new List<string>() { unauthorizedException.InnerException != null ? unauthorizedException.InnerException.Message : unauthorizedException.Message } };
            approvalResponses.FirstOrDefault().DisplayMessage = unauthorizedException.Message;
            actionResponse = new HttpResponseMessage()
            {
                Content = new StringContent(approvalResponses.ToJson(), new UTF8Encoding(), Constants.ContentTypeJson),
                StatusCode = HttpStatusCode.BadRequest
            };
        }
        catch (InvalidDataException invalidDataException)
        {
            // This is to handle the tenant service call related exception
            bTenantCallSuccess = false;
            bIsHandledException = true;

            logData.Modify(LogDataKey.EventId, (int)TrackingEvent.DocumentActionFailure);
            logData.Modify(LogDataKey.EventName, TrackingEvent.DocumentActionFailure.ToString());
            _logger.LogError(TrackingEvent.DocumentActionFailure, invalidDataException, logData);
            LogMessageProgress(TrackingEvent.ApprovalActionFailed, new FailureData() { Message = invalidDataException.Message }, CriticalityLevel.Yes, logData);

            approvalResponses.FirstOrDefault().E2EErrorInformation = new ApprovalResponseErrorInfo { ErrorMessages = new List<string>() { invalidDataException.InnerException != null ? invalidDataException.InnerException.Message : invalidDataException.Message } };
            approvalResponses.FirstOrDefault().DisplayMessage = invalidDataException.Message;
            actionResponse = new HttpResponseMessage()
            {
                Content = new StringContent(approvalResponses.ToJson(), new UTF8Encoding(), Constants.ContentTypeJson),
                StatusCode = HttpStatusCode.BadRequest
            };
        }
        catch (Exception ex)
        {
            // This is to handle the tenant service call related exception
            bTenantCallSuccess = false;
            bIsHandledException = false;

            logData.Modify(LogDataKey.EventId, (int)TrackingEvent.DocumentActionFailure);
            logData.Modify(LogDataKey.EventName, TrackingEvent.DocumentActionFailure.ToString());
            _logger.LogError(TrackingEvent.DocumentActionFailure, ex, logData);
            LogMessageProgress(TrackingEvent.ApprovalActionFailed, new FailureData() { Message = ex.Message }, CriticalityLevel.Yes, logData);

            approvalResponses.FirstOrDefault().E2EErrorInformation = new ApprovalResponseErrorInfo { ErrorMessages = new List<string>() { ex.InnerException != null ? ex.InnerException.Message : ex.Message } };
            actionResponse = new HttpResponseMessage()
            {
                Content = new StringContent(approvalResponses.ToJson(), new UTF8Encoding(), Constants.ContentTypeJson),
                StatusCode = HttpStatusCode.BadRequest
            };
        }
        finally
        {
            JArray userActionsResponseArray = new JArray();
            var approvalRequests = new List<ApprovalRequest>() { approvalRequest };
            await Task.Run(async () =>
            {
                await PostProcessActionResponses(
                    tenantInfo,
                    tenantId,
                    alias,
                    loggedInAlias,
                    clientDevice,
                    approvalRequests,
                    tcv,
                    actionResponse,
                    bTenantCallSuccess,
                    tenantAdapter,
                    actionResponseContentObject,
                    userActionsResponseArray,
                    summaryJsonList,
                    sessionId,
                    bIsHandledException);
            });
        }
        return actionResponseContentObject;
    }

    /// <summary>
    /// Add user document details based on the type of the document.
    /// </summary>
    /// <param name="approvalSummaryRow">Approval summary.</param>
    /// <param name="tenantId">Tenant Id.</param>
    /// <param name="approvalRequest">Approval Request.</param>
    private void AddUserDocumentsInfo(ApprovalSummaryRow approvalSummaryRow, int tenantId, ApprovalRequest approvalRequest)
    {
        if (approvalSummaryRow != null)
        {
            var userDocuments = _attachmentHelper.GetAttachmentDetailsForTenantNotification(tenantId, approvalSummaryRow.DocumentNumber);
            if (approvalRequest?.AdditionalData == null)
            {
                if (userDocuments != null)
                {
                    approvalRequest.AdditionalData = new Dictionary<string, string>();
                    approvalRequest.AdditionalData.Add(Constants.UserAttachments, userDocuments.ToJToken().ToString());
                }
            }
            else if (userDocuments != null)
            {
                approvalRequest.AdditionalData.Modify(Constants.UserAttachments, userDocuments.ToJToken().ToString());
            }
        }
    }

    /// <summary>
    /// Validates the condition.
    /// </summary>
    /// <param name="condition">The condition.</param>
    /// <param name="requestObject">The request object.</param>
    /// <returns>returns boolean</returns>
    private bool ValidateCondition(string condition, ApprovalRequest requestObject)
    {
        bool isConditionPassed = true;
        try
        {
            JObject requestJObject = requestObject.ToJson().ToJObject();
            if (!string.IsNullOrEmpty(condition))
            {
                string[] splitCondition = condition.Split('^');
                if (splitCondition.Count() == 2)
                {
                    var compareKey = splitCondition[0];
                    var compareValue = splitCondition[1];

                    isConditionPassed = Convert.ToString(requestJObject.SelectToken(compareKey)).Equals(compareValue, StringComparison.InvariantCultureIgnoreCase);
                }
            }
        }
        catch
        {
            // do nothing, default value would be returned
        }

        return isConditionPassed;
    }

    #endregion Private Methods

    #endregion Helper Methods
}