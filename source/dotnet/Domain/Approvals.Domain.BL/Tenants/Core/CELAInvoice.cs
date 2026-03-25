// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace Microsoft.CFS.Approvals.Domain.BL.Tenants.Core;

using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.CFS.Approvals.Common.DL.Interface;
using Microsoft.CFS.Approvals.Contracts.DataContracts;
using Microsoft.CFS.Approvals.Data.Azure.Storage.Interface;
using Microsoft.CFS.Approvals.Extensions;
using Microsoft.CFS.Approvals.LogManager.Provider.Interface;
using Microsoft.CFS.Approvals.Model;
using Microsoft.CFS.Approvals.Utilities.Interface;
using Microsoft.Extensions.Configuration;

/// <summary>
/// Class CELAInvoice
/// </summary>
/// <seealso cref="GenericTenant" />
public class CELAInvoice : GenericTenant
{
    #region CONSTRUCTOR

    public CELAInvoice(
        ApprovalTenantInfo tenantInfo,
        ILogProvider logger,
        IPerformanceLogger performanceLogger,
        IApprovalSummaryProvider approvalSummaryProvider,
        IConfiguration config,
        INameResolutionHelper nameResolutionHelper,
        IApprovalDetailProvider approvalDetailProvider,
        IFlightingDataProvider flightingDataProvider,
        IApprovalHistoryProvider approvalHistoryProvider,
        IBlobStorageHelper blobStorageHelper,
        IAuthenticationHelper authenticationHelper,
        IHttpHelper httpHelper)
        : base(
              tenantInfo,
              logger,
              performanceLogger,
              approvalSummaryProvider,
              config,
              nameResolutionHelper,
              approvalDetailProvider,
              flightingDataProvider,
              approvalHistoryProvider,
              blobStorageHelper,
              authenticationHelper,
              httpHelper)
    {
    }

    public CELAInvoice(
        ApprovalTenantInfo tenantInfo,
        string alias,
        string clientDevice,
        string oauth2Token,
        ILogProvider logger,
        IPerformanceLogger performanceLogger,
        IApprovalSummaryProvider approvalSummaryProvider,
        IConfiguration config,
        INameResolutionHelper nameResolutionHelper,
        IApprovalDetailProvider approvalDetailProvider,
        IFlightingDataProvider flightingDataProvider,
        IApprovalHistoryProvider approvalHistoryProvider,
        IBlobStorageHelper blobStorageHelper,
        IAuthenticationHelper authenticationHelper,
        IHttpHelper httpHelper,
        string objectId,
        string domain)
        : base(
              tenantInfo,
              alias,
              clientDevice,
              oauth2Token,
              logger,
              performanceLogger,
              approvalSummaryProvider,
              config,
              nameResolutionHelper,
              approvalDetailProvider,
              flightingDataProvider,
              approvalHistoryProvider,
              blobStorageHelper,
              authenticationHelper,
              httpHelper,
              objectId,
              domain)
    {
    }

    #endregion CONSTRUCTOR

    /// <summary>
    /// Gets detail url
    /// </summary>
    /// <param name="urlFormat">url string format</param>
    /// <param name="approvalIdentifier">Approval Identifier</param>
    /// <param name="page">page number</param>
    /// <param name="xcv">X-Correlation ID</param>
    /// <param name="tcv">T-Correlation ID</param>
    /// <param name="businessProcessName">Business process name</param>
    /// <param name="docTypeId">Document Type ID</param>
    /// <returns>detail url string</returns>
    protected override async Task<string> GetDetailURL(string urlFormat, ApprovalIdentifier approvalIdentifier, int page, string xcv = "", string tcv = "", string businessProcessName = "", string docTypeId = "")
    {
        var actualApprovalIdentifier = await GetApprovalIdentifier(approvalIdentifier.DocumentNumber, xcv, tcv) ?? approvalIdentifier;
        return String.Format(urlFormat, actualApprovalIdentifier.DocumentNumber, actualApprovalIdentifier.FiscalYear);
    }

    /// <summary>
    /// Gets Approval Identifier object either from ApprovalSummary table or TransactionHistory table.
    /// </summary>
    /// <param name="documentNumber">Document Number.</param>
    /// <param name="xcv">Xcv.</param>
    /// <param name="tcv">Tcv.</param>
    /// <returns><see cref="ApprovalIdentifier"/> object.</returns>
    private async Task<ApprovalIdentifier> GetApprovalIdentifier(string documentNumber, string xcv = "", string tcv = "")
    {
        ApprovalIdentifier approvalIdentifier = null;

        // If ApprovalSummary record is available, get the ApprovalIdentifier from SummaryJson.
        // Else if TransactionHistory record is available, get the ApprovalIdentifier from JsonData.
        var summary = ApprovalSummaryProvider.GetApprovalSummaryByDocumentNumberAndApprover(approvalTenantInfo.DocTypeId, documentNumber, Alias, ObjectId, Domain);

        if (summary != null)
        {
            var approvalSummary = summary?.SummaryJson?.FromJson<SummaryJson>();
            approvalIdentifier = approvalSummary?.ApprovalIdentifier;
        }
        else
        {
            var historyData = await ApprovalHistoryProvider.GetHistoryDataAsync(approvalTenantInfo, documentNumber, Alias, xcv, tcv);

            if (historyData != null && historyData.Count > 0)
            {
                var historyRow = historyData.OrderByDescending(x => x.ActionDate).FirstOrDefault();
                var summaryFromHistory = historyRow?.JsonData?.FromJson<SummaryJson>();
                approvalIdentifier = summaryFromHistory?.ApprovalIdentifier;
            }
        }

        return approvalIdentifier;
    }
}