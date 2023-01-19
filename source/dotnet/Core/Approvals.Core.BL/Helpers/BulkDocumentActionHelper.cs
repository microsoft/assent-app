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
using Newtonsoft.Json.Linq;

// <summary>
/// Class BulkDocumentActionHelper.
/// </summary>
/// <seealso cref="DocumentActionHelper" />
public class BulkDocumentActionHelper : DocumentActionHelper
{
    /// <summary>
    /// Initializes a new instance of the <see cref="BulkDocumentActionHelper"/> class.
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
    /// <param name="tenantFactory">The tenant factory.</param>
    public BulkDocumentActionHelper(
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
        ITenantFactory tenantFactory)
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
              tenantFactory)
    {
    }

    #region Helper Methods

    /// <summary>
    /// Process user actions as an asynchronous operation.
    /// </summary>
    /// <param name="configurationHelper">The configuration helper.</param>
    /// <param name="actionAuditLogHelper">The action audit log helper.</param>
    /// <param name="logger">The logger.</param>
    /// <param name="tenantAdapter">The tenant adapter.</param>
    /// <param name="tenantInfo">The tenant information.</param>
    /// <param name="tenantId">The tenant map identifier.</param>
    /// <param name="alias">The alias.</param>
    /// <param name="loggedInAlias">The logged in alias.</param>
    /// <param name="clientDevice">The client device.</param>
    /// <param name="approvalRequests">The approval requests.</param>
    /// <param name="xcv">The XCV.</param>
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

        // SummaryJson list to be passed for ActionAuditLogs in CosmosDB
        var summaryJsonList = new List<SummaryJson>();
        HttpResponseMessage actionResponse = null;
        var approvalResponses = new List<ApprovalResponse>();
        bool bTenantCallSuccess = false; // Boolean value for checking tenant call status
        bool bIsHandledException = false; // Boolean value for checking Handled Exception for RequestVersion and UnAuthorization

        try
        {
            var tenantOperations = tenantInfo?.DetailOperations.DetailOpsList;
            var operation = tenantOperations.Where(item => item.operationtype == Constants.OperationTypeOutOfSync)
                .ToList().FirstOrDefault();

            if (operation == null && (approvalReq.Action.Equals(Constants.OutOfSyncAction,
                                          StringComparison.InvariantCultureIgnoreCase)
                                      || approvalReq.Action.Equals(Constants.UndoOutOfSyncAction,
                                          StringComparison.InvariantCultureIgnoreCase)))
            {
                // Logic for Out of Sync action if the endpoint is not present
                return new JObject();
            }
            else
            {
                LogMessageProgress(TrackingEvent.ApprovalActionInitiated, null, CriticalityLevel.Yes, logData);

                #region Marking soft delete the pending summary records and update Details entities in Batch

                List<ApprovalSummaryRow> summaryRowsToUpdateInBatch = new List<ApprovalSummaryRow>();
                List<ApprovalDetailsEntity> detailsRowsToUpdateInBatch = new List<ApprovalDetailsEntity>();
                foreach (var approvalRequest in approvalRequests)
                {
                    Tuple<ApprovalSummaryRow, List<ApprovalDetailsEntity>> returnTuple = tenantAdapter.UpdateTransactionalDetails(approvalRequest, true, string.Empty, loggedInAlias, sessionId, null);

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
                    await _approvalSummaryProvider.UpdateSummaryInBatchAsync(summaryRowsToUpdateInBatch, xcv, sessionId,
                        approvalReq.Telemetry.Tcv, tenantInfo, approvalReq.Action);
                }

                if (detailsRowsToUpdateInBatch.Count > 0)
                {
                    await _approvalDetailProvider.UpdateDetailsInBatchAsync(detailsRowsToUpdateInBatch,
                        xcv, sessionId, tcv, tenantInfo, approvalReq.Action);
                }

                #endregion Marking soft delete the pending summary records and update Details entities in Batch

                #region Pre-processing Approval Requests

                List<ApprovalRequest>
                    approvalRequestsToTenant =
                        new List<ApprovalRequest>(); // Non exception approval requests send to tenant
                foreach (var approvalRequest in approvalRequests)
                {
                    try
                    {
                        logData.Modify(LogDataKey.Tcv, approvalRequest?.Telemetry?.Tcv);
                        logData.Modify(LogDataKey.Xcv, approvalRequest?.ApprovalIdentifier?.DisplayDocumentNumber);
                        logData.Modify(LogDataKey.BusinessProcessName,
                            string.Format(tenantInfo?.BusinessProcessName,
                                Constants.BusinessProcessNameApprovalAction, approvalRequest?.Action));
                        logData.Modify(LogDataKey.DocumentNumber,
                            approvalRequest?.ApprovalIdentifier?.DisplayDocumentNumber);
                        logData.Modify(LogDataKey.DXcv, approvalRequest?.ApprovalIdentifier?.DisplayDocumentNumber);
                        logData.Modify(LogDataKey.AppAction, approvalRequest?.Action);
                        logData.Modify(LogDataKey.Operation, approvalRequest?.Action);

                        ApprovalSummaryRow approvalSummaryRow =
                            _approvalSummaryProvider.GetApprovalSummaryByDocumentNumberAndApprover(
                                tenantInfo.DocTypeId, approvalRequest.ApprovalIdentifier.DisplayDocumentNumber,
                                alias);

                        // Authorization check
                        // If this fails, user gets a 401 status code (Unauthorized)
                        await CheckAuthorization(alias, approvalRequest, approvalSummaryRow, tenantInfo);

                        // Version check
                        // If this fails, user gets a 400 status code (Invalid data - Stale request)
                        RequestVersionCheck(clientDevice, approvalRequest, approvalSummaryRow, tenantInfo);

                        // Add metadata like Edited information and delegation information to the approval request object
                        AddMetadataToApprovalRequest(tenantAdapter, tenantInfo, tenantId, alias, loggedInAlias, sessionId, tcv, logData, approvalRequest, approvalSummaryRow);

                        // Adding this for logging in ActionAuditLogs in CosmosDB
                        summaryJsonList.Add(approvalSummaryRow.SummaryJson.FromJson<SummaryJson>());

                        approvalRequestsToTenant.Add(approvalRequest);
                    }
                    catch (UnauthorizedAccessException unauthorizedException)
                    {
                        bIsHandledException = true;
                        bTenantCallSuccess = false;
                        _logger.LogError(TrackingEvent.DocumentActionFailure, unauthorizedException, logData);
                        LogMessageProgress(TrackingEvent.ApprovalActionFailed, new FailureData()
                        { Message = unauthorizedException.Message }, CriticalityLevel.Yes, logData);
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
                                    unauthorizedException.InnerException != null
                                        ? unauthorizedException.InnerException.Message
                                        : unauthorizedException.Message
                                }
                            }
                        });
                    }
                    catch (InvalidDataException invalidDataException)
                    {
                        bTenantCallSuccess = false;
                        bIsHandledException = true;
                        _logger.LogError(TrackingEvent.DocumentActionFailure, invalidDataException, logData);
                        LogMessageProgress(TrackingEvent.ApprovalActionFailed, new FailureData()
                        { Message = invalidDataException.Message }, CriticalityLevel.Yes, logData);
                        approvalResponses.Add(new ApprovalResponse
                        {
                            ActionResult = false,
                            DisplayMessage = invalidDataException.Message,
                            ApprovalIdentifier = approvalRequest.ApprovalIdentifier,
                            DocumentTypeID = approvalRequest.DocumentTypeID,
                            E2EErrorInformation = new ApprovalResponseErrorInfo
                            {
                                ErrorMessages = new List<string>()
                                {
                                    invalidDataException.InnerException != null
                                        ? invalidDataException.InnerException.Message
                                        : invalidDataException.Message
                                }
                            }
                        });
                    }
                    catch (Exception ex)
                    {
                        // This is to handle the tenant service call related exception
                        bTenantCallSuccess = false;
                        _logger.LogError(TrackingEvent.DocumentActionFailure, ex, logData);
                        LogMessageProgress(TrackingEvent.ApprovalActionFailed, new FailureData()
                        { Message = ex.Message }, CriticalityLevel.Yes, logData);

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
                                    {ex.InnerException != null ? ex.InnerException.Message : ex.Message}
                            }
                        });
                    }
                }

                #endregion Pre-processing Approval Requests

                if (approvalRequestsToTenant.Count > 0)
                {
                    #region Executing Bulk Action Call

                    actionResponse = await tenantAdapter.ExecuteActionAsync(approvalRequestsToTenant, loggedInAlias,
                        sessionId, clientDevice, xcv, tcv, null);

                    #endregion Executing Bulk Action Call

                    #region Forming Global httpResponse object merging locally failed requests and tenant responded requests

                    // Converting actionResponse from Tenant for non exception requests into list of ApprovalResponse object
                    List<ApprovalResponse> approvalResponsesFromTenant =
                        tenantAdapter.ParseResponseString<List<ApprovalResponse>>(
                            await actionResponse.Content.ReadAsStringAsync());

                    // Added above Approval Response list received from tenant into the global ApprovalResponses which might have a locally failed response
                    approvalResponses.AddRange(approvalResponsesFromTenant);
                }

                // Merged httpResponse for all approval requests
                if (approvalResponses.Any(res => res.E2EErrorInformation?.ErrorMessages != null) &&
                    approvalResponses.Any(res => res.E2EErrorInformation?.ErrorMessages.Count > 0))
                {
                    actionResponse = new HttpResponseMessage()
                    {
                        Content = new StringContent(approvalResponses.ToJson(), new UTF8Encoding(),
                            Constants.ContentTypeJson),
                        StatusCode = HttpStatusCode.BadRequest
                    };
                    bTenantCallSuccess = false;
                    if (!actionResponse.IsSuccessStatusCode)
                    {
                        LogMessageProgress(TrackingEvent.ApprovalActionFailed,
                            new FailureData()
                            {
                                Message = await actionResponse.Content.ReadAsStringAsync()
                            },
                            CriticalityLevel.Yes, logData);
                    }
                }
                else // no error messages in ApprovalResponses
                {
                    actionResponse = new HttpResponseMessage()
                    {
                        Content = new StringContent(approvalResponses.ToJson(), new UTF8Encoding(),
                            Constants.ContentTypeJson),
                        StatusCode = HttpStatusCode.OK
                    };
                    bTenantCallSuccess = true;
                    if (actionResponse.IsSuccessStatusCode)
                    {
                        LogMessageProgress(TrackingEvent.ApprovalActionSuccessfullyCompleted, null,
                            CriticalityLevel.Yes, logData);
                    }
                }

                #endregion Forming Global httpResponse object merging locally failed requests and tenant responded requests
            }
        }
        catch (Exception ex)
        {
            // This is to handle the tenant service call related exception
            bTenantCallSuccess = false;
            _logger.LogError(TrackingEvent.DocumentActionFailure, ex, logData);
            LogMessageProgress(TrackingEvent.ApprovalActionFailed, new FailureData() { Message = ex.Message }, CriticalityLevel.Yes, logData);
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
                    E2EErrorInformation = new ApprovalResponseErrorInfo { ErrorMessages = new List<string>() { ex.InnerException != null ? ex.InnerException.Message : ex.Message } }
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

        #region For each processed user action, add appropriate response

        var failedDocuments = new Dictionary<string, string>();
        var allTasks = new List<Task<JToken>>();
        FormulateActionResponse(tenantInfo, clientDevice, approvalRequests, userActionsResponseArray, allTasks, out failedDocuments);

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

        List<ApprovalResponse> approvalResponses = (await actionResponse.Content.ReadAsStringAsync()).FromJson<List<ApprovalResponse>>();

        foreach (var approvalResp in approvalResponses)
        {
            JToken jToken = JToken.FromObject(new { approvalResp.ApprovalIdentifier.DocumentNumber, approvalResp.ApprovalIdentifier.DisplayDocumentNumber });
            JObject actionObj = new JObject
            {
                { Constants.DocumentKeys, jToken }
            };
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
                var summaryData = summaryJsonList.FirstOrDefault(d => d.ApprovalIdentifier.DisplayDocumentNumber.Equals(approvalResponse.ApprovalIdentifier.DisplayDocumentNumber));

                    var actionAuditLogInfo = new ActionAuditLogInfo
                    {
                        DisplayDocumentNumber = approvalResponse?.ApprovalIdentifier?.GetDocNumber(tenantInfo),
                        ActionDateTime = actionDateTime.ToUniversalTime().ToString("o"),
                        ActionStatus = Constants.SuccessStatus,
                        ActionTaken = approvalReq?.Action,
                        Approver = alias,
                        ImpersonatedUser = loggedInAlias,
                        ClientType = clientDevice,
                        TenantId = tenantInfo?.TenantId.ToString(),
                        UnitValue = summaryData?.UnitValue ?? string.Empty,
                        UnitOfMeasure = summaryData?.UnitOfMeasure ?? string.Empty,
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

            await LogActionDetailsAsync(actionAuditLogs, tenantInfo, clientDevice, tcv, sessionId, loggedInAlias, alias);

            // Adds the Target page information to the action response object which is passed to the client side
            // Based on the TargetPage property and delay time, the UI either navigates to the Summary / calling page or stays on the details page
            AddTargetPageInformation(tenantInfo, actionResponseContentObject, approvalReq);
        }
        else if (!actionResponse.IsSuccessStatusCode || !bTenantCallSuccess)
        {
            List<ActionAuditLogInfo> actionAuditLogs = new List<ActionAuditLogInfo>();
            List<ApprovalSummaryRow> summaryRowsToUpdateInBatch = new List<ApprovalSummaryRow>();
            List<ApprovalDetailsEntity> detailsRowsToUpdateInBatch = new List<ApprovalDetailsEntity>();
            for (int i = 0; i < approvalResponses.Count; i++)
            {
                var actionObj = userActionsResponseArray[i].ToString().ToJObject();
                for (int j = 0; j < approvalRequests.Count; j++)
                {
                    if (!approvalRequests[j].ApprovalIdentifier.DisplayDocumentNumber.Equals(approvalResponses[i].ApprovalIdentifier.DisplayDocumentNumber))
                    {
                        continue;
                    }

                    logData.Modify(LogDataKey.Tcv, approvalResponses[i]?.Telemetry?.Tcv);
                    logData.Modify(LogDataKey.Xcv, approvalResponses[i]?.Telemetry?.Xcv);
                    logData.Modify(LogDataKey.BusinessProcessName, string.Format(tenantInfo?.BusinessProcessName, Constants.BusinessProcessNameApprovalAction, approvalRequests[j]?.Action));
                    logData.Modify(LogDataKey.DocumentNumber, approvalResponses[i]?.ApprovalIdentifier?.DisplayDocumentNumber);
                    logData.Modify(LogDataKey.DXcv, approvalResponses[i]?.ApprovalIdentifier?.DisplayDocumentNumber);
                    logData.Modify(LogDataKey.DisplayDocumentNumber, approvalResponses[i]?.ApprovalIdentifier?.DisplayDocumentNumber);
                    logData.Modify(LogDataKey.AppAction, approvalRequests[j]?.Action);
                    logData.Modify(LogDataKey.Operation, approvalRequests[j]?.Action);

                    var actionDateTime = Convert.ToDateTime(approvalRequests[j].ActionDetails[approvalRequests[j].ActionDetails.Keys.FirstOrDefault(x => x.ToLowerInvariant() == Constants.ActionDateKey.ToLowerInvariant())]);
                    var summaryData = summaryJsonList.FirstOrDefault(d => d.ApprovalIdentifier.DisplayDocumentNumber.Equals(approvalResponses[i].ApprovalIdentifier.DisplayDocumentNumber));

                        if (!approvalResponses[i].ActionResult)
                        {
                            if (actionObj.Property("ErrorMessage") == null)
                            {
                                // DisplayMessage will always contain value
                                actionObj.Add("ErrorMessage", "Tracking ID:" + tcv + " :: " + approvalResponses[i].DisplayMessage + ".");
                            }
                            var actionAuditLogInfo = new ActionAuditLogInfo
                            {
                                DisplayDocumentNumber = approvalResponses[i]?.ApprovalIdentifier?.GetDocNumber(tenantInfo),
                                ActionDateTime = actionDateTime.ToUniversalTime().ToString("o"),
                                ActionStatus = Constants.FailedStatus,
                                ActionTaken = approvalRequests[j].Action,
                                Approver = alias,
                                ImpersonatedUser = loggedInAlias,
                                ClientType = clientDevice,
                                TenantId = tenantInfo?.TenantId.ToString(),
                                UnitValue = summaryData?.UnitValue ?? string.Empty,
                                UnitOfMeasure = summaryData?.UnitOfMeasure ?? string.Empty,
                                ErrorMessage = approvalResponses[i]?.E2EErrorInformation?.ErrorMessages?.ToJson(),
                                Id = Guid.NewGuid()
                            };
                            actionAuditLogs.Add(actionAuditLogInfo);

                        _logger.LogError(TrackingEvent.DocumentActionFailure, new Exception(approvalResponses[i]?.E2EErrorInformation?.ErrorMessages?.ToJson()), logData);

                        // bTenantCallSuccess is false whenever there is an exception either from the tenant (legacy wcf) or Approvals code
                        // The below code updates the all the transactional details (which includes error messages) in the Approval Summary/ApprovalDetails table
                        if (!bTenantCallSuccess)
                        {
                            Tuple<ApprovalSummaryRow, List<ApprovalDetailsEntity>> returnTuple = tenantAdapter.UpdateTransactionalDetails(approvalRequests[j], false, actionObj["ErrorMessage"]?.ToString(), loggedInAlias, clientDevice, null);

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
                        }
                        else
                        {
                            var actionAuditLogInfo = new ActionAuditLogInfo
                            {
                                DisplayDocumentNumber = approvalResponses[i]?.ApprovalIdentifier?.GetDocNumber(tenantInfo),
                                ActionDateTime = actionDateTime.ToUniversalTime().ToString("o"),
                                ActionStatus = Constants.SuccessStatus,
                                ActionTaken = approvalRequests[j]?.Action,
                                Approver = alias,
                                ImpersonatedUser = loggedInAlias,
                                ClientType = clientDevice,
                                TenantId = tenantInfo?.TenantId.ToString(),
                                UnitValue = summaryData?.UnitValue ?? string.Empty,
                                UnitOfMeasure = summaryData?.UnitOfMeasure ?? string.Empty,
                                Id = Guid.NewGuid()
                            };
                            actionAuditLogs.Add(actionAuditLogInfo);
                            _logger.LogInformation(TrackingEvent.DocumentActionSuccess, logData);
                        }
                    }
                    userActionsResponseArray[i] = actionObj;
                }
                if (summaryRowsToUpdateInBatch.Count > 0)
                {
                    await _approvalSummaryProvider.UpdateSummaryInBatchAsync(summaryRowsToUpdateInBatch, tcv, sessionId,
                        tcv, tenantInfo, approvalRequests[0].Action);
                }

            if (detailsRowsToUpdateInBatch.Count > 0)
            {
                await _approvalDetailProvider.UpdateDetailsInBatchAsync(detailsRowsToUpdateInBatch, tcv, sessionId,
                    tcv, tenantInfo, approvalRequests[0].Action);
            }

            await LogActionDetailsAsync(actionAuditLogs, tenantInfo, clientDevice, tcv, sessionId, loggedInAlias, alias);
        }

        #endregion Process the userActionResponseArray
    }

    /// <summary>
    /// Extracts the failed requests data from Action Response for Bulk actions
    /// </summary>
    /// <param name="userActionsResponseArray">the user actions response array.</param>
    /// <param name="allTasks">all task threads which are running each individual actions.</param>
    /// <returns>Dictionary of string, string</returns>
    protected override async Task<Dictionary<string, string>> ExtractDataFromActionResponse(JArray userActionsResponseArray, List<Task<JToken>> allTasks)
    {
        var failedDocuments = new Dictionary<string, string>();
        foreach (var userActionObject in userActionsResponseArray)
        {
            if (userActionObject["ErrorMessage"] != null
                && userActionObject[Constants.DocumentKeys] != null && userActionObject[Constants.DocumentKeys].Type != JTokenType.Null
                && userActionObject[Constants.DocumentKeys][Constants.DisplayDocumentNumber] != null && userActionObject[Constants.DocumentKeys][Constants.DisplayDocumentNumber].Type != JTokenType.Null)
            {
                failedDocuments.Add(userActionObject[Constants.DocumentKeys][Constants.DisplayDocumentNumber].ToString(), " Error : " + userActionObject["ErrorMessage"].ToString());
            }
        }

        return failedDocuments;
    }

    #endregion Helper Methods
}