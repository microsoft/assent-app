// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace Microsoft.CFS.Approvals.Core.BL.Interface;

using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.CFS.Approvals.Model;
using Newtonsoft.Json.Linq;

public interface IApprovalSummaryHelper
{
    /// <summary>
    /// Get Approval SummaryJson Approver for multiple Tenants for given view type
    /// </summary>
    /// <param name="approver">approver alias</param>
    /// <param name="tenants">List<ApprovalTenantInfo> - list of tenants</param>
    /// <param name="viewType">Default value 'Summary'</param>
    /// <returns>List of approval summary data</returns>
    List<ApprovalSummaryData> GetApprovalSummaryJsonByApproverAndTenants(string approver, List<ApprovalTenantInfo> tenants, string viewType = "Summary");

    /// <summary>
    /// Get ApprovalSummaryCountJson By Approver for multiple tenants
    /// </summary>
    /// <param name="approver">approver alias</param>
    /// <param name="tenants">List<ApprovalTenantInfo> - list of tenants</param>
    /// <param name="viewType">Default value 'Summary'</param>
    /// <returns>JArray- with fields Id, tenantName and count</returns>
    JArray GetApprovalSummaryCountJsonByApproverAndTenants(string approver, List<ApprovalTenantInfo> tenants, string viewType = "Summary");

    /// <summary>
    /// Get Approval Counts by approver
    /// </summary>
    /// <param name="approver"></param>
    /// <returns></returns>
    Task<ApprovalCount[]> GetApprovalCounts(string approver);

    /// <summary>
    /// Get Approval Summary By Document Number and Approver
    /// </summary>
    /// <param name="documentTypeID"></param>
    /// <param name="documentNumber"></param>
    /// <param name="approver"></param>
    /// <returns></returns>
    ApprovalSummaryRow GetApprovalSummaryByDocumentNumberAndApprover(string documentTypeID, string documentNumber, string approver);
}