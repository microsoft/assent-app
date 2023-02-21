// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace Microsoft.CFS.Approvals.Core.BL.Helpers;

using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.CFS.Approvals.Common.DL.Interface;
using Microsoft.CFS.Approvals.Contracts;
using Microsoft.CFS.Approvals.Core.BL.Interface;
using Microsoft.CFS.Approvals.Model;
using Newtonsoft.Json.Linq;

/// <summary>
/// The Approval Summary Helper class
/// </summary>
public class ApprovalSummaryHelper : IApprovalSummaryHelper
{
    /// <summary>
    /// The approval summary provider
    /// </summary>
    private readonly IApprovalSummaryProvider _approvalSummaryProvider;

    /// <summary>
    /// Constructor of ApprovalSummaryHelper
    /// </summary>
    /// <param name="approvalSummaryProvider"></param>
    public ApprovalSummaryHelper(IApprovalSummaryProvider approvalSummaryProvider)
    {
        _approvalSummaryProvider = approvalSummaryProvider;
    }

    /// <summary>
    /// Get ApprovalSummaryCountJson By Approver for multiple tenants
    /// </summary>
    /// <param name="approver">approver alias</param>
    /// <param name="tenants">List<ApprovalTenantInfo> - list of tenants</param>
    /// <param name="viewType">Default value 'Summary'</param>
    /// <returns>JArray- with fields Id, tenantName and count</returns>
    public JArray GetApprovalSummaryCountJsonByApproverAndTenants(string approver, List<ApprovalTenantInfo> tenants, string viewType = "Summary")
    {
        IEnumerable<ApprovalSummaryRow> jsonSummary = _approvalSummaryProvider.GetApprovalSummaryCountJsonByApproverAndTenants(approver, tenants);
        List<ApprovalSummaryRow> filteredRowKeys = new List<ApprovalSummaryRow>();
        JArray summaryCount = new JArray();
        switch (viewType)
        {
            case Constants.OutOfSyncAction:
                foreach (var tenant in tenants)
                {
                    JObject jSummaryTenantCountData = new JObject();
                    filteredRowKeys = jsonSummary.Where(l => l.LobPending == false && l.IsOutOfSyncChallenged == true && tenant.DocTypeId == l.RowKey.Split('|').FirstOrDefault()).ToList();
                    jSummaryTenantCountData.Add("ID", tenant.DocTypeId);
                    jSummaryTenantCountData.Add("TenantName", tenant.AppName);
                    jSummaryTenantCountData.Add("Count", filteredRowKeys.Count);
                    if (filteredRowKeys.Count > 0)
                        summaryCount.Add(jSummaryTenantCountData);
                }
                break;

            case Constants.OfflineApproval:
                foreach (var tenant in tenants)
                {
                    JObject jSummaryTenantCountData = new JObject();
                    filteredRowKeys = jsonSummary.Where(l => l.LobPending == false && l.IsOfflineApproval == true && tenant.DocTypeId == l.RowKey.Split('|').FirstOrDefault()).ToList();
                    jSummaryTenantCountData.Add("ID", tenant.DocTypeId);
                    jSummaryTenantCountData.Add("TenantName", tenant.AppName);
                    jSummaryTenantCountData.Add("Count", filteredRowKeys.Count);
                    if (filteredRowKeys.Count > 0)
                        summaryCount.Add(jSummaryTenantCountData);
                }
                break;

            default:
                foreach (var tenant in tenants)
                {
                    JObject jSummaryTenantCountData = new JObject();
                    filteredRowKeys = jsonSummary.Where(l => l.LobPending == false && l.IsOfflineApproval == false && l.IsOutOfSyncChallenged == false && tenant.DocTypeId == l.RowKey.Split('|').FirstOrDefault()).ToList();
                    jSummaryTenantCountData.Add("ID", tenant.DocTypeId);
                    jSummaryTenantCountData.Add("TenantName", tenant.AppName);
                    jSummaryTenantCountData.Add("Count", filteredRowKeys.Count);
                    if (filteredRowKeys.Count > 0)
                        summaryCount.Add(jSummaryTenantCountData);
                }
                break;
        }

        return summaryCount;
    }

    /// <summary>
    /// Get Approval SummaryJson Approver for multiple Tenants for given view type
    /// </summary>
    /// <param name="approver">approver alias</param>
    /// <param name="tenants">List<ApprovalTenantInfo></ApprovalTenantInfo></param>
    /// <param name="viewType">Default value 'Summary'</param>
    /// <returns>List of approval summary data</returns>
    public List<ApprovalSummaryData> GetApprovalSummaryJsonByApproverAndTenants(string approver, List<ApprovalTenantInfo> tenants, string viewType = "Summary")
    {
        List<ApprovalSummaryData> filteredSummaryData;
        var approvalSummaryData = _approvalSummaryProvider.GetApprovalSummaryJsonByApproverAndTenants(approver, tenants);
        switch (viewType)
        {
            case Constants.OutOfSyncAction:
                filteredSummaryData = approvalSummaryData.Where(l => l.LobPending == false && l.IsOutOfSyncChallenged == true && tenants.Any(tenant => tenant.DocTypeId == l.DocumentTypeId)).Select(j => j).ToList();
                break;

            case Constants.OfflineApproval:
                filteredSummaryData = approvalSummaryData.Where(l => l.LobPending == false && l.IsOfflineApproval == true && tenants.Any(tenant => tenant.DocTypeId == l.DocumentTypeId)).Select(j => j).ToList();
                break;

            default:
                filteredSummaryData = approvalSummaryData.Where(l => l.LobPending == false && l.IsOfflineApproval == false && l.IsOutOfSyncChallenged == false && tenants.Any(tenant => tenant.DocTypeId == l.DocumentTypeId)).Select(j => j).ToList();
                break;
        }
        return filteredSummaryData;
    }

    /// <summary>
    /// Get approval counts
    /// </summary>
    /// <param name="approver"></param>
    /// <returns></returns>
    public async Task<ApprovalCount[]> GetApprovalCounts(string approver)
    {
        return await _approvalSummaryProvider.GetApprovalCounts(approver);
    }

    /// <summary>
    /// Get Approval Summary By Document Number and Approver
    /// </summary>
    /// <param name="documentTypeID"></param>
    /// <param name="documentNumber"></param>
    /// <param name="approver"></param>
    /// <returns></returns>
    public ApprovalSummaryRow GetApprovalSummaryByDocumentNumberAndApprover(string documentTypeID, string documentNumber, string approver)
    {
        return _approvalSummaryProvider.GetApprovalSummaryByDocumentNumberAndApprover(documentTypeID, documentNumber, approver);
    }
}