// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace Microsoft.CFS.Approvals.Core.BL.Interface;

using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.CFS.Approvals.Domain.BL.Interface;
using Microsoft.CFS.Approvals.Model;
using Newtonsoft.Json.Linq;

public interface ISummaryHelper
{
    /// <summary>
    /// Create summary data list
    /// </summary>
    /// <param name="approvalsData"></param>
    /// <param name="tenants"></param>
    /// <param name="checkTenantUserDelegationEnable"></param>
    /// <returns></returns>
    List<ApprovalSummaryData> CreateSummaryDataList
                            (
                                List<ApprovalSummaryData> approvalsData,
                                List<ApprovalTenantInfo> tenants,
                                bool checkTenantUserDelegationEnable = false
                            );

    /// <summary>
    /// Get summary data
    /// </summary>
    /// <param name="documentNumber"></param>
    /// <param name="fiscalYear"></param>
    /// <param name="tenantInfo"></param>
    /// <param name="alias"></param>
    /// <param name="loggedInAlias"></param>
    /// <param name="xcv"></param>
    /// <param name="tcv"></param>
    /// <returns></returns>
    ApprovalSummaryRow GetSummaryData
                            (
                                string documentNumber,
                                string fiscalYear,
                                ApprovalTenantInfo tenantInfo,
                                string alias,
                                string loggedInAlias,
                                string xcv,
                                string tcv,
                                ITenant tenantAdaptor
                            );

    /// <summary>
    /// Get summary
    /// </summary>
    /// <param name="loggedInAlias"></param>
    /// <param name="alias"></param>
    /// <param name="host"></param>
    /// <param name="sessionId"></param>
    /// <param name="tenantDocTypeId"></param>
    /// <returns></returns>
    Task<JArray> GetSummary
                            (
                                string loggedInAlias,
                                string alias,
                                string host,
                                string sessionId,
                                string tenantDocTypeId = ""
                            );

    /// <summary>
    /// Get other summary requests
    /// </summary>
    /// <param name="loggedInAlias"></param>
    /// <param name="alias"></param>
    /// <param name="host"></param>
    /// <param name="viewType"></param>
    /// <param name="sessionId"></param>
    /// <param name="tenantDocTypeId"></param>
    /// <returns></returns>
    Task<JArray> GetOtherSummaryRequests
                            (
                                string loggedInAlias,
                                string alias,
                                string host,
                                string viewType,
                                string sessionId,
                                string tenantDocTypeId = ""
                            );

    /// <summary>
    /// Get summary count data
    /// </summary>
    /// <param name="tenantDocTypeId"></param>
    /// <param name="loggedInAlias"></param>
    /// <param name="userAlias"></param>
    /// <param name="sessionId"></param>
    /// <param name="clientDevice"></param>
    /// <returns></returns>
    Task<JArray> GetSummaryCountData
                            (
                                string tenantDocTypeId,
                                string loggedInAlias,
                                string userAlias,
                                string sessionId,
                                string clientDevice);

    /// <summary>
    /// Get other summary requests count data
    /// </summary>
    /// <param name="tenantDocTypeId"></param>
    /// <param name="loggedInAlias"></param>
    /// <param name="userAlias"></param>
    /// <param name="viewType"></param>
    /// <param name="sessionId"></param>
    /// <param name="clientDevice"></param>
    /// <returns></returns>
    Task<JArray> GetOtherSummaryRequestsCountData
                            (
                                string tenantDocTypeId,
                                string loggedInAlias,
                                string userAlias,
                                string viewType,
                                string sessionId,
                                string clientDevice);
}