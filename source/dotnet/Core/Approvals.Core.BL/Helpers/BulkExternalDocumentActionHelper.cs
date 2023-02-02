// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace Microsoft.CFS.Approvals.Core.BL.Helpers;

using System;
using System.Collections.Generic;
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
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

/// <summary>
/// Class BulkExternalDocumentActionHelper.
/// </summary>
/// <seealso cref="DocumentActionHelper" />
public class BulkExternalDocumentActionHelper : DocumentActionHelper
{
    /// <summary>
    /// The _historyProvider.
    /// </summary>
    private readonly IApprovalHistoryProvider _historyProvider = null;

    /// <summary>
    /// Initializes a new instance of the <see cref="BulkExternalDocumentActionHelper"/> class.
    /// </summary>
    /// <param name="approvalSummaryProvider">The approval summary provider.</param>
    /// <param name="config">The configuration.</param>
    /// <param name="logProvider">The logger.</param>
    /// <param name="nameResolutionHelper">The name resolution helper.</param>
    /// <param name="performanceLogger">The performance logger.</param>
    /// <param name="approvalDetailProvider">The approval detail provider.</param>
    /// <param name="flightingDataProvider">The flighting data provider.</param>
    /// <param name="actionAuditLogHelper">The action audit log helper.</param>
    /// <param name="tableHelper">The storage helper.</param>
    /// <param name="approvalTenantInfoHelper">The approval tenant helper.</param>
    /// <param name="tenantFactory">The tenant factory</param>
    /// <param name="historyProvider">The history provider.</param>
    /// <param name="attachmentHelper">The attachment helper.</param>
    public BulkExternalDocumentActionHelper(
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
        IApprovalHistoryProvider historyProvider,
        IAttachmentHelper attachmentHelper)
        : base(
              approvalSummaryProvider,
              config,
              logProvider,
              nameResolutionHelper,
              performanceLogger,
              approvalDetailProvider,
              flightingDataProvider,
              actionAuditLogHelper,
              tableHelper,
              approvalTenantInfoHelper,
              tenantFactory,
              attachmentHelper)
    {
        _historyProvider = historyProvider;
    }

    /// <summary>
    /// Process user actions as an asynchronous operation.
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
    protected override async Task<JObject> ProcessUserActionsAsync(
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
        var userActionsResponseArray = new JArray();
        var userActionsResponseObject = new JObject();
        var approvalReq = approvalRequests.FirstOrDefault();

        #region Prepare Log Data

        var logData = new Dictionary<LogDataKey, object>
        {
            { LogDataKey.ClientDevice, clientDevice },
            { LogDataKey.IsCriticalEvent, CriticalityLevel.Yes.ToString() },
            { LogDataKey.EventId, TrackingEvent.DocumentActionFailure },
            { LogDataKey.EventName, TrackingEvent.DocumentActionFailure.ToString() },
            { LogDataKey.Tcv, tcv },
            { LogDataKey.Xcv, xcv },
            { LogDataKey.ReceivedTcv, tcv },
            { LogDataKey.SessionId, sessionId },
            { LogDataKey.UserRoleName, loggedInAlias },
            { LogDataKey.Approver, loggedInAlias },
            { LogDataKey.BusinessProcessName, string.Format(tenantInfo?.BusinessProcessName, Constants.BusinessProcessNameApprovalAction, approvalReq?.Action) },
            { LogDataKey.TenantId, tenantId },
            { LogDataKey.TenantName, tenantInfo?.AppName },
            { LogDataKey.UserAlias, alias },
            { LogDataKey.AppAction, approvalReq?.Action },
            { LogDataKey.Operation, approvalReq?.Action },
            { LogDataKey.DocumentTypeId, tenantInfo?.DocTypeId }
        };

        #endregion Prepare Log Data

        HttpResponseMessage actionResponse = null;
        var approvalResponses = new List<ApprovalResponse>();
        bool bTenantCallSuccess = false; // Boolean value for checking tenant call status

        try
        {
            var tenantOperations = tenantInfo.DetailOperations.DetailOpsList;
            var operation = tenantOperations.FirstOrDefault(item => item.operationtype == Constants.OperationTypeOutOfSync);

            if (operation == null &&
                (approvalReq.Action.Equals(Constants.OutOfSyncAction, StringComparison.InvariantCultureIgnoreCase) ||
                approvalReq.Action.Equals(Constants.UndoOutOfSyncAction, StringComparison.InvariantCultureIgnoreCase)))
            {
                // Logic for Out of Sync action if the endpoint is not present
                return new JObject();
            }
            else
            {
                #region Executing Bulk External Action Call

                var approvalRequestsToTenant = JsonConvert.DeserializeObject<List<ApprovalRequest>>(approvalRequests.ToJson());
                foreach (var approvalRequest in approvalRequestsToTenant)
                {
                    approvalRequest.AdditionalData.Remove("summaryJSON");
                }
                actionResponse = await tenantAdapter.ExecuteActionAsync(approvalRequestsToTenant, loggedInAlias, sessionId, clientDevice, xcv, tcv, null);

                #endregion Executing Bulk External Action Call

                bTenantCallSuccess = true;

                if (actionResponse != null)
                {
                    LogMessageProgress(
                        actionResponse.IsSuccessStatusCode
                            ? TrackingEvent.ApprovalActionSuccessfullyCompleted
                            : TrackingEvent.ApprovalActionFailed, null, CriticalityLevel.Yes, logData);
                }
            }
        }
        catch (UnauthorizedAccessException unauthorizedException)
        {
            bTenantCallSuccess = false;
            _logger.LogError(TrackingEvent.ApprovalActionFailed, unauthorizedException, logData);
            foreach (var approvalRequest in approvalRequests)
            {
                // Setup Generic Error message based on device type
                tenantAdapter.SetupGenericErrorMessage(approvalRequest);

                approvalResponses.Add(new ApprovalResponse
                {
                    ActionResult = false,
                    DisplayMessage = unauthorizedException.Message,
                    ApprovalIdentifier = approvalRequest.ApprovalIdentifier,
                    DocumentTypeID = approvalRequest.DocumentTypeID,
                    E2EErrorInformation = new ApprovalResponseErrorInfo
                    {
                        ErrorMessages = new List<string>()
                        {
                            unauthorizedException.InnerException != null ?
                            unauthorizedException.InnerException.Message :
                            unauthorizedException.Message
                        }
                    }
                });
            }

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
            _logger.LogError(TrackingEvent.DocumentActionFailure, invalidDataException, logData);
            LogMessageProgress(TrackingEvent.ApprovalActionFailed, new FailureData() { Message = invalidDataException.Message }, CriticalityLevel.Yes, logData);

            foreach (var approvalRequest in approvalRequests)
            {
                // Setup Generic Error message based on device type
                tenantAdapter.SetupGenericErrorMessage(approvalRequest);

                approvalResponses.Add(new ApprovalResponse
                {
                    ActionResult = false,
                    DisplayMessage = invalidDataException.Message,
                    ApprovalIdentifier = approvalRequest.ApprovalIdentifier,
                    DocumentTypeID = approvalRequest.DocumentTypeID,
                    E2EErrorInformation = new ApprovalResponseErrorInfo { ErrorMessages = new List<string>() { invalidDataException.InnerException != null ? invalidDataException.InnerException.Message : invalidDataException.Message } }
                });
            }

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
            _logger.LogError(TrackingEvent.ApprovalActionFailed, ex, logData);
            foreach (var approvalRequest in approvalRequests)
            {
                // Setup Generic Error message based on device type
                tenantAdapter.SetupGenericErrorMessage(approvalRequest);

                approvalResponses.Add(new ApprovalResponse
                {
                    ActionResult = false,
                    DisplayMessage = tenantAdapter.GenericErrorMessage,
                    ApprovalIdentifier = approvalRequest.ApprovalIdentifier,
                    DocumentTypeID = approvalRequest.DocumentTypeID,
                    E2EErrorInformation = new ApprovalResponseErrorInfo
                    {
                        ErrorMessages = new List<string>()
                        {
                            ex.InnerException != null ? ex.InnerException.Message : ex.Message
                        }
                    }
                });
            }

            actionResponse = new HttpResponseMessage()
            {
                Content = new StringContent(approvalResponses.ToJson(), new UTF8Encoding(), Constants.ContentTypeJson),
                StatusCode = HttpStatusCode.BadRequest
            };
        }
        finally
        {
            var actionResponseContentObject = new JObject();
            await Task.Run(async () =>
            {
                await PostProcessActionResponses(tenantInfo, tenantId, alias, loggedInAlias, clientDevice, approvalRequests, tcv,
                   actionResponse, bTenantCallSuccess, tenantAdapter, actionResponseContentObject, userActionsResponseArray, null, sessionId);
            });
        }

        #region For each processed user action, add appropriate response

        var failedDocuments = new Dictionary<string, string>();
        var allTasks = new List<Task<JToken>>();
        FormulateActionResponse(tenantInfo,
            clientDevice,
            approvalRequests,
            userActionsResponseArray,
            allTasks,
            out failedDocuments);

        #endregion For each processed user action, add appropriate response

        userActionsResponseObject.Add("ActionResponseContent", JToken.FromObject(userActionsResponseArray));
        return userActionsResponseObject;
    }

    /// <summary>
    /// This method formulates the response array by doing post processing on the action response received from the tenant
    /// </summary>
    /// <param name="actionAuditLogHelper">The action audit log helper</param>
    /// <param name="approvalDetailProvider">The approval detail provider</param>
    /// <param name="logger">The logger</param>
    /// <param name="tenantInfo">The approval tenant information object</param>
    /// <param name="tenantId">The Tenant Id</param>
    /// <param name="alias">Delegated user alias</param>
    /// <param name="loggedInAlias">Logged in alias</param>
    /// <param name="clientDevice">The client device</param>
    /// <param name="approvalRequests">List of Approval requests</param>
    /// <param name="tcv">TCV value</param>
    /// <param name="actionResponse">Action response for bulk action</param>
    /// <param name="bTenantCallSuccess">Success status flag</param>
    /// <param name="tenantAdapter">Tenant adapter</param>
    /// <param name="actionResponseContentObject">Action response object returned to client side code</param>
    /// <param name="userActionsResponseArray">Action response array returned to client side code</param>
    /// <param name="summaryJsonList">List of summaryJson for logging into Cosmos DB</param>
    /// <param name="sessionId">The sessionId</param>
    /// <param name="bIsHandledException">The bIsHandledException</param>
    protected override async Task PostProcessActionResponses(
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
        #region Extract required information from documentKey and add to action response content object

        var approvalResponses = tenantAdapter.ParseResponseString<List<ApprovalResponse>>(await actionResponse.Content.ReadAsStringAsync());

        foreach (var approvalResp in approvalResponses)
        {
            var jToken = JToken.FromObject(new { approvalResp.ApprovalIdentifier.DocumentNumber, approvalResp.ApprovalIdentifier.DisplayDocumentNumber });
            var actionObj = new JObject { { Constants.DocumentKeys, jToken } };
            userActionsResponseArray.Add(actionObj);
        }

        #endregion Extract required information from documentKey and add to action response content object

        #region Process the userActionResponseArray

        var logData = new Dictionary<LogDataKey, object>
        {
            { LogDataKey.ClientDevice, clientDevice },
            { LogDataKey.IsCriticalEvent, CriticalityLevel.Yes.ToString() },
            { LogDataKey.GlobalTcv, tcv },
            { LogDataKey.SessionId, sessionId },
            { LogDataKey.UserRoleName, loggedInAlias },
            { LogDataKey.Approver, alias },
            { LogDataKey.TenantId, tenantId },
            { LogDataKey.TenantName, tenantInfo?.AppName },
            { LogDataKey.UserAlias, alias },
            { LogDataKey.DocumentTypeId, tenantInfo?.DocTypeId }
        };

        if (actionResponse.IsSuccessStatusCode)
        {
            var approvalReq = approvalRequests.FirstOrDefault();
            var actionDateTime = Convert.ToDateTime(approvalReq.ActionDetails[approvalReq.ActionDetails.Keys.FirstOrDefault(x => x.ToLowerInvariant() == Constants.ActionDateKey.ToLowerInvariant())]);
            List<ActionAuditLogInfo> actionAuditLogs = new List<ActionAuditLogInfo>();
            foreach (var approvalResponse in approvalResponses)
            {
                var approvalrequest = approvalRequests.FirstOrDefault(x => x.ApprovalIdentifier.DisplayDocumentNumber == approvalResponse?.ApprovalIdentifier?.GetDocNumber(tenantInfo));
                var summaryJSON = approvalrequest.AdditionalData["summaryJSON"].ToJObject();

                    var actionAuditLogInfo = new ActionAuditLogInfo
                    {
                        DisplayDocumentNumber = approvalResponse?.ApprovalIdentifier?.GetDocNumber(tenantInfo),
                        ActionDateTime = actionDateTime.ToUniversalTime().ToString("o"),
                        ActionStatus = Constants.SuccessStatus,
                        ActionTaken = approvalReq?.Action,
                        UnitValue = summaryJSON?["unitValue"]?.ToString() ?? string.Empty,
                        UnitOfMeasure = summaryJSON?["unitOfMeasure"]?.ToString() ?? string.Empty,
                        Approver = alias,
                        ImpersonatedUser = loggedInAlias,
                        ClientType = clientDevice,
                        TenantId = tenantInfo?.TenantId.ToString(),
                        Id = Guid.NewGuid()
                    };

                actionAuditLogs.Add(actionAuditLogInfo);
                logData.Modify(LogDataKey.Tcv, approvalResponse?.Telemetry?.Tcv);
                logData.Modify(LogDataKey.Xcv, approvalResponse?.Telemetry?.Xcv);
                logData.Modify(LogDataKey.BusinessProcessName, string.Format(tenantInfo?.BusinessProcessName, Constants.BusinessProcessNameApprovalAction, approvalReq?.Action));
                logData.Modify(LogDataKey.DocumentNumber, approvalResponse?.ApprovalIdentifier?.DisplayDocumentNumber);
                logData.Modify(LogDataKey.DXcv, approvalResponse?.ApprovalIdentifier?.DisplayDocumentNumber);
                logData.Modify(LogDataKey.DisplayDocumentNumber, approvalResponse?.ApprovalIdentifier?.DisplayDocumentNumber);
                logData.Modify(LogDataKey.AppAction, approvalReq?.Action);
                logData.Modify(LogDataKey.Operation, approvalReq?.Action);

                _logger.LogInformation(TrackingEvent.DocumentActionSuccess, logData);
            }

            List<Task> tasks = new List<Task>
            {
                LogActionDetailsAsync(actionAuditLogs, tenantInfo, clientDevice, tcv, sessionId, loggedInAlias, alias),
                InsertTransactionHistoryAsync(approvalRequests, tenantInfo, tenantAdapter, clientDevice, tcv, sessionId, loggedInAlias, alias)
            };
            Task.WaitAll(tasks.ToArray());

            // Adds the Target page information to the action response object which is passed to the client side
            // Based on the TargetPage property and delay time, the UI either navigates to the Summary / calling page or stays on the details page
            AddTargetPageInformation(tenantInfo, actionResponseContentObject, approvalReq);
        }
        else if (!actionResponse.IsSuccessStatusCode || !bTenantCallSuccess)
        {
            List<ActionAuditLogInfo> actionAuditLogs = new List<ActionAuditLogInfo>();
            List<ApprovalRequest> successApprovalRequests = new List<ApprovalRequest>();
            for (var i = 0; i < approvalResponses.Count; i++)
            {
                var actionObj = userActionsResponseArray[i].ToString().ToJObject();
                foreach (var approvalRequest in approvalRequests)
                {
                    if (!approvalRequest.ApprovalIdentifier.DisplayDocumentNumber.Equals(approvalResponses[i].ApprovalIdentifier.DisplayDocumentNumber))
                    {
                        continue;
                    }

                    logData.Modify(LogDataKey.Tcv, approvalResponses[i]?.Telemetry?.Tcv);
                    logData.Modify(LogDataKey.Xcv, approvalResponses[i]?.Telemetry?.Xcv);
                    logData.Modify(LogDataKey.BusinessProcessName, string.Format(tenantInfo?.BusinessProcessName, Constants.BusinessProcessNameApprovalAction, approvalRequest.Action));
                    logData.Modify(LogDataKey.DocumentNumber, approvalResponses[i]?.ApprovalIdentifier?.DisplayDocumentNumber);
                    logData.Modify(LogDataKey.DXcv, approvalResponses[i]?.ApprovalIdentifier?.DisplayDocumentNumber);
                    logData.Modify(LogDataKey.DisplayDocumentNumber, approvalResponses[i]?.ApprovalIdentifier?.DisplayDocumentNumber);
                    logData.Modify(LogDataKey.AppAction, approvalRequest.Action);
                    logData.Modify(LogDataKey.Operation, approvalRequest.Action);

                        var actionDateTime = Convert.ToDateTime(approvalRequest.ActionDetails[approvalRequest.ActionDetails.Keys.FirstOrDefault(x => x.ToLowerInvariant() == Constants.ActionDateKey.ToLowerInvariant())]);
                        var summaryJSON = approvalRequest.AdditionalData["summaryJSON"].ToJObject();
                        if (!approvalResponses[i].ActionResult)
                        {
                            if (actionObj.Property("ErrorMessage") == null)
                            {
                                actionObj.Add("ErrorMessage", "Tracking ID:" + tcv + " :: " + approvalResponses[i].DisplayMessage + ".");
                            }
                            var actionAuditLogInfo = new ActionAuditLogInfo
                            {
                                DisplayDocumentNumber = approvalResponses[i]?.ApprovalIdentifier?.GetDocNumber(tenantInfo),
                                ActionDateTime = actionDateTime.ToUniversalTime().ToString("o"),
                                ActionStatus = Constants.FailedStatus,
                                ActionTaken = approvalRequest?.Action,
                                UnitValue = summaryJSON?["unitValue"]?.ToString() ?? string.Empty,
                                UnitOfMeasure = summaryJSON?["unitOfMeasure"]?.ToString() ?? string.Empty,
                                Approver = alias,
                                ImpersonatedUser = loggedInAlias,
                                ClientType = clientDevice,
                                TenantId = tenantInfo?.TenantId.ToString(),
                                ErrorMessage = approvalResponses[i]?.E2EErrorInformation?.ErrorMessages?.ToJson(),
                                Id = Guid.NewGuid()
                            };

                        actionAuditLogs.Add(actionAuditLogInfo);

                            _logger.LogError(TrackingEvent.DocumentActionFailure, new Exception(approvalResponses[i]?.E2EErrorInformation?.ErrorMessages?.ToJson()), logData);
                        }
                        else
                        {
                            var actionAuditLogInfo = new ActionAuditLogInfo
                            {
                                DisplayDocumentNumber = approvalResponses[i]?.ApprovalIdentifier?.GetDocNumber(tenantInfo),
                                ActionDateTime = actionDateTime.ToUniversalTime().ToString("o"),
                                ActionStatus = Constants.SuccessStatus,
                                ActionTaken = approvalRequest.Action,
                                UnitValue = summaryJSON?["unitValue"]?.ToString() ?? string.Empty,
                                UnitOfMeasure = summaryJSON?["unitOfMeasure"]?.ToString() ?? string.Empty,
                                Approver = alias,
                                ImpersonatedUser = loggedInAlias,
                                ClientType = clientDevice,
                                TenantId = tenantInfo?.TenantId.ToString(),
                                Id = Guid.NewGuid()
                            };
                            actionAuditLogs.Add(actionAuditLogInfo);
                            _logger.LogInformation(TrackingEvent.DocumentActionSuccess, logData);
                            successApprovalRequests.Add(approvalRequest);
                        }
                    }

                userActionsResponseArray[i] = actionObj;
            }

            List<Task> tasks = new List<Task>
            {
                LogActionDetailsAsync(actionAuditLogs, tenantInfo, clientDevice, tcv, sessionId, loggedInAlias, alias),
                InsertTransactionHistoryAsync(successApprovalRequests, tenantInfo, tenantAdapter, clientDevice, tcv, sessionId, loggedInAlias, alias)
            };

            Task.WaitAll(tasks.ToArray());
        }

        #endregion Process the userActionResponseArray
    }

    /// <summary>
    /// Extracts the failed requests data from Action Response for Bulk actions
    /// </summary>
    /// <param name="userActionsResponseArray">the user actions response array.</param>
    /// <param name="allTasks">all task threads which are running each individual actions.</param>
    /// <returns>Dictionary of string, string</returns>
    protected override async Task<Dictionary<string, string>> ExtractDataFromActionResponse(
        JArray userActionsResponseArray,
        List<Task<JToken>> allTasks)
    {
        var failedDocuments = new Dictionary<string, string>();
        foreach (var userActionObject in userActionsResponseArray)
        {
            if (userActionObject["ErrorMessage"] != null &&
                userActionObject[Constants.DocumentKeys] != null &&
                userActionObject[Constants.DocumentKeys].Type != JTokenType.Null &&
                userActionObject[Constants.DocumentKeys][Constants.DisplayDocumentNumber] != null &&
                userActionObject[Constants.DocumentKeys][Constants.DisplayDocumentNumber].Type != JTokenType.Null)
            {
                failedDocuments.Add(userActionObject[Constants.DocumentKeys][Constants.DisplayDocumentNumber].ToString(), " Error : " + userActionObject["ErrorMessage"].ToString());
            }
        }

        return failedDocuments;
    }

    /// <summary>
    /// Pre-processes, validates and sanitizes the User action input string
    /// </summary>
    /// <param name="userActionsString">Request body which is pre-processed to form List of ApprovalRequest objects</param>
    /// <param name="tcv">GUID transaction correlation vector for telemetry and logging</param>
    /// <param name="tenantInfo">ApprovalTenantInfo object for the current tenant</param>
    /// <returns>List of ApprovalRequest which is sent to the LoB application as part of the content in the Http call</returns>
    protected override List<ApprovalRequest> PreProcessUserActionString(
        string userActionsString,
        string tcv,
        ApprovalTenantInfo tenantInfo)
    {
        #region Validate the input string

        if (string.IsNullOrWhiteSpace(userActionsString) || !userActionsString.IsJson())
        {
            throw new InvalidDataException("Action string is not valid: " + userActionsString);
        }

        #endregion Validate the input string

        #region Standardize the input string to List<ApprovalRequest>

        var approvalRequests = userActionsString.FromJson<List<ApprovalRequest>>();

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
    /// This method will be used to insert history data into TransactionHistory table
    /// </summary>
    /// <param name="approvalRequests">List of approval request.</param>
    /// <param name="approvalTenantInfo">The approval tenant info.</param>
    /// <param name="tenantAdapter">The tenant adapter object</param>
    /// <param name="logger">The logger</param>
    /// <param name="clientDevice">The clientDevice</param>
    /// <param name="tcv">The tcv</param>
    /// <param name="sessionId">The sessionId</param>
    /// <param name="loggedInAlias">The loggedInAlias</param>
    /// <param name="alias">The alias</param>
    private async Task InsertTransactionHistoryAsync(List<ApprovalRequest> approvalRequests, ApprovalTenantInfo approvalTenantInfo, ITenant tenantAdapter, string clientDevice, string tcv, string sessionId, string loggedInAlias, string alias)
    {
        var logData = new Dictionary<LogDataKey, object>
        {
            { LogDataKey.StartDateTime, DateTime.UtcNow },
            { LogDataKey.ClientDevice, clientDevice },
            { LogDataKey.IsCriticalEvent, CriticalityLevel.No.ToString() },
            { LogDataKey.GlobalTcv, tcv },
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
            List<TransactionHistory> historyDataList = new List<TransactionHistory>();
            foreach (var approvalRequest in approvalRequests)
            {
                logData.Modify(LogDataKey.DocumentNumber,
                    approvalRequest?.ApprovalIdentifier?.DisplayDocumentNumber);
                logData.Modify(LogDataKey.Tcv, approvalRequest?.Telemetry?.Tcv);
                logData.Modify(LogDataKey.Xcv, approvalRequest?.Telemetry?.Xcv);
                logData.Modify(LogDataKey.DXcv, approvalRequest?.ApprovalIdentifier?.DisplayDocumentNumber);
                logData.Modify(LogDataKey.DisplayDocumentNumber,
                    approvalRequest?.ApprovalIdentifier?.DisplayDocumentNumber);

                var summaryJSON = approvalRequest.AdditionalData["summaryJSON"].ToJObject();
                if (summaryJSON != null)
                {
                    var approvalsNote = string.Empty;
                    if (approvalRequest.ActionDetails != null)
                    {
                        approvalsNote = approvalRequest.ActionDetails.ToJson();
                    }

                        var historyData = new TransactionHistory()
                        {
                            PartitionKey = approvalRequest.ApprovalIdentifier.GetDocNumber(approvalTenantInfo),
                            RowKey = Guid.NewGuid().ToString(),
                            Title = tenantAdapter.GetRequestTitleDescription(summaryJSON),
                            Approver = summaryJSON["approverAlias"]?.ToString(),
                            UnitValue = summaryJSON["unitValue"]?.ToString(),
                            AmountUnits = summaryJSON["unitOfMeasure"]?.ToString(),
                            SubmittedDate = Convert.ToDateTime(summaryJSON["submittedDate"]),
                            SubmitterName = summaryJSON["submitterName"]?.ToString(),
                            SubmittedAlias = summaryJSON["submitterAlias"]?.ToString(),
                            ActionDate = (!Convert.ToDateTime(approvalRequest.ActionDetails["actionDate"]).Equals(DateTime.MinValue))
                                ? Convert.ToDateTime(approvalRequest.ActionDetails["actionDate"])
                                : DateTime.UtcNow,
                            ActionTaken = !String.IsNullOrWhiteSpace(approvalRequest.Action)
                                ? approvalRequest.Action
                                : "None",
                            DocumentNumber = approvalRequest.ApprovalIdentifier?.GetDocNumber(approvalTenantInfo),
                            JsonData = summaryJSON.ToString(),
                            TenantId = approvalTenantInfo.TenantId.ToString(),
                            DocumentTypeID = approvalRequest.DocumentTypeID,
                            AppName = approvalTenantInfo.AppName,
                            DelegateUser = approvalRequest.ActionByDelegateInMSApprovals,
                            Xcv = approvalRequest.Telemetry.Xcv,
                            ActionTakenOnClient = clientDevice,
                            ApproversNote = approvalsNote,
                            CompanyCode = summaryJSON["companyCode"]?.ToString(),
                            Timestamp = DateTime.UtcNow
                        };

                    historyDataList.Add(historyData);
                }
            }

            if (historyDataList.Count > 0)
            {
                await _historyProvider.AddApprovalHistoryAsync(approvalTenantInfo, historyDataList);

                logData.Modify(LogDataKey.EndDateTime, DateTime.UtcNow);
                _logger.LogInformation(TrackingEvent.HistoryInsertSuccess, logData);
            }
        }
        catch (Exception ex)
        {
            logData.Modify(LogDataKey.EndDateTime, DateTime.UtcNow);
            _logger.LogError(TrackingEvent.HistoryInsertFailure, ex, logData);
        }
    }
}