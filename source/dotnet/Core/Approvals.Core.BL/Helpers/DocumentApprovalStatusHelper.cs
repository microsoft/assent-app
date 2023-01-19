// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace Microsoft.CFS.Approvals.Core.BL.Helpers;

using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.CFS.Approvals.Common.DL.Interface;
using Microsoft.CFS.Approvals.Contracts;
using Microsoft.CFS.Approvals.Contracts.DataContracts;
using Microsoft.CFS.Approvals.Core.BL.Interface;
using Microsoft.CFS.Approvals.Extensions;
using Microsoft.CFS.Approvals.LogManager.Model;
using Microsoft.CFS.Approvals.LogManager.Provider.Interface;
using Microsoft.CFS.Approvals.Model;
using Microsoft.CFS.Approvals.Utilities.Interface;
using Microsoft.Extensions.Configuration;

/// <summary>
/// Class DocumentApprovalStatusHelper.
/// </summary>
/// <seealso cref="IDocumentApprovalStatusHelper" />
public class DocumentApprovalStatusHelper : IDocumentApprovalStatusHelper
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
    /// Approval TenantInfo Helper
    /// </summary>
    protected readonly IApprovalTenantInfoHelper _approvalTenantInfoHelper = null;

    /// <summary>
    /// Approval History Provider
    /// </summary>
    protected readonly IApprovalHistoryProvider _approvalHistoryProvider = null;

    #endregion Variables

    #region Constructor

    /// <summary>
    /// Initializes a new instance of the <see cref="DocumentApprovalStatusHelper"/> class.
    /// </summary>
    /// <param name="approvalSummaryProvider">The approval summary provider.</param>
    /// <param name="config">The configuration.</param>
    /// <param name="logger">The logger.</param>
    /// <param name="nameResolutionHelper">The name resolution helper.</param>
    /// <param name="performanceLogger">The performance logger.</param>
    public DocumentApprovalStatusHelper(
        IApprovalSummaryProvider approvalSummaryProvider,
        IConfiguration config,
        ILogProvider logger,
        INameResolutionHelper nameResolutionHelper,
        IPerformanceLogger performanceLogger,
        IApprovalTenantInfoHelper approvalTenantInfoHelper,
        IApprovalHistoryProvider approvalHistoryProvider)
    {
        _approvalSummaryProvider = approvalSummaryProvider;
        _config = config;
        _logger = logger;
        _nameResolutionHelper = nameResolutionHelper;
        _performanceLogger = performanceLogger;
        _approvalTenantInfoHelper = approvalTenantInfoHelper;
        _approvalHistoryProvider = approvalHistoryProvider;
    }

    #endregion Constructor

    /// <summary>
    /// Get the document status.
    /// </summary>
    /// <param name="tenantId"></param>
    /// <param name="requestData"></param>
    /// <param name="clientDevice"></param>
    /// <param name="userAlias"></param>
    /// <param name="loggedInUser"></param>
    /// <param name="tcv"></param>
    /// <param name="sessionId"></param>
    /// <param name="xcv"></param>
    /// <returns>Document status response object.</returns>
    public Task<DocumentStatusResponse> DocumentStatus(int tenantId, string requestData, string clientDevice, string userAlias, string loggedInUser, string tcv, string sessionId, string xcv)
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
            { LogDataKey.UserRoleName, loggedInUser },
            { LogDataKey.TenantId, tenantId },
            { LogDataKey.UserAlias, userAlias },
            { LogDataKey.ClientDevice, clientDevice },
            { LogDataKey.Xcv, xcv },
            { LogDataKey.StartDateTime, DateTime.UtcNow }
        };

        #endregion Logging Prep

        DocumentStatusResponse documentStatusResponse;
        ApprovalSummaryRow summaryData = null;
        ApprovalTenantInfo tenantInfo = null;

        try
        {
            using (_performanceLogger.StartPerformanceLogger("PerfLog", "DocumentStatus", "DocumentStatusAPI", logData))
            {
                #region Process Request Data

                DocumentStatusRequest documentStatusRequest = requestData.FromJson<DocumentStatusRequest>();

                #endregion Process Request Data

                logData.Add(LogDataKey.Approver, documentStatusRequest.ApproverAlias);
                logData.Add(LogDataKey.DocumentNumber, documentStatusRequest.DocumentNumber);

                #region Get Tenant Info

                using (_performanceLogger.StartPerformanceLogger("PerfLog", "_approvalTenantInfoHelper", string.Format(Constants.PerfLogAction, "GetTenantInfo", "DocumentStatus"), logData))
                {
                    tenantInfo = _approvalTenantInfoHelper.GetTenantInfo(tenantId);
                }

                #endregion Get Tenant Info

                logData.Add(LogDataKey.DocumentTypeId, tenantInfo.DocTypeId);

                using (var summaryRowInsertTracer = _performanceLogger.StartPerformanceLogger("PerfLog", "_approvalSummaryProvider", string.Format(Constants.PerfLogAction, "Get Summary by DocumentNumber and Approver", "DocumentStatus"), logData))
                {
                    summaryData = _approvalSummaryProvider.GetApprovalSummaryByDocumentNumberAndApprover(tenantInfo.DocTypeId, documentStatusRequest.DocumentNumber, userAlias);
                }

                //TODO:: Need to revisit the code for outlook actionable api
                if (summaryData != null)
                {
                    if (summaryData.RequestVersion != documentStatusRequest.RequestVersion)
                    {
                        //This is old request, we can not allow action
                        documentStatusResponse = GetSummaryData(summaryData, "OldRequest");
                    }
                    else if (summaryData.LobPending)
                    {
                        //Action taken but response is pending from tenant
                        documentStatusResponse = GetSummaryData(summaryData, "LobPending");
                    }
                    else if (summaryData.IsOfflineApproval)
                    {
                        //Request is submitted for background approval
                        documentStatusResponse = GetSummaryData(summaryData, "SubmittedForBackgroundApproval");
                    }
                    else if (summaryData.IsOutOfSyncChallenged)
                    {
                        //Request is out of synchronization from tenant system
                        documentStatusResponse = GetSummaryData(summaryData, "OutOfSyncRecord");
                    }
                    else
                    {
                        //Action not taken yet
                        documentStatusResponse = GetSummaryData(summaryData, "Pending");
                    }
                }
                else
                {
                    //Action submitted and completed from tenant system
                    documentStatusResponse = new DocumentStatusResponse()
                    {
                        ActionDate = DateTime.MaxValue,
                        CurrentStatus = "ActionTaken",
                        ActionTakenOnClient = string.Empty,
                        UnitOfMeasure = string.Empty,
                        UnitValue = string.Empty,
                        SubmitterAlias = string.Empty,
                        SubmittedDate = DateTime.MaxValue,
                        SubmitterName = string.Empty,
                        TenantName = tenantInfo.AppName,
                        ApproverAlias = documentStatusRequest.ApproverAlias,
                        ApproverName = documentStatusRequest.ApproverAlias
                    };
                }

                logData.Add(LogDataKey.BusinessProcessName, string.Format(tenantInfo.BusinessProcessName, Constants.BusinessProcessNameDocumentApprovalStatus, documentStatusResponse?.CurrentStatus?.ToString()));

                _logger.LogInformation(TrackingEvent.WebApiDocumentStatusSuccess, logData);
                logData.Modify(LogDataKey.EndDateTime, DateTime.UtcNow);
                return Task.FromResult(documentStatusResponse);
            }
        }
        catch (Exception exception)
        {
            logData.Modify(LogDataKey.EndDateTime, DateTime.UtcNow);
            _logger.LogError(TrackingEvent.WebApiDocumentStatusFail, exception, logData);
            throw;
        }
    }

    /// <summary>
    /// Extract data from approval summary row
    /// </summary>
    /// <param name="summaryRow">The summary row.</param>
    /// <param name="action">The action.</param>
    /// <returns>Return DocumentStatusResponse</returns>
    private DocumentStatusResponse GetSummaryData(ApprovalSummaryRow summaryRow, string action)
    {
        SummaryJson summaryJson = summaryRow.SummaryJson.ToJObject().ToObject<SummaryJson>();

        DocumentStatusResponse documentStatusResponse = new DocumentStatusResponse()
        {
            ActionDate = summaryRow.OperationDateTime,
            CurrentStatus = action,
            ActionTakenOnClient = summaryRow.ActionTakenOnClient,
            UnitOfMeasure = summaryJson.UnitOfMeasure,
            UnitValue = summaryJson.UnitValue,
            SubmitterAlias = summaryJson.Submitter.Alias,
            SubmittedDate = summaryJson.SubmittedDate,
            SubmitterName = summaryJson.Submitter.Name,
            TenantName = summaryRow.Application,
            ApproverAlias = summaryRow.Approver,
            ApproverName = summaryRow.Approver
        };

        return documentStatusResponse;
    }
}