// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace Microsoft.CFS.Approvals.Common.DL.Interface;

using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.CFS.Approvals.Contracts.DataContracts;
using Microsoft.CFS.Approvals.Model;
using Newtonsoft.Json.Linq;

/// <summary>
/// IApprovalHistoryProvider interface
/// </summary>
public interface IApprovalHistoryProvider
{
    /// <summary>
    /// This method will fetch History Data
    /// </summary>
    /// <param name="alias">The alias.</param>
    /// <param name="timePeriod">The timePeriod.</param>
    /// <param name="searchCriteria">The search criteria.</param>
    /// <param name="approverDomain">Approver Domain</param>
    /// <param name="approverId">Approver Object Id</param>
    /// <param name="page">The page.</param>
    /// <param name="sortColumn">The sort column.</param>
    /// <param name="sortDirection">The sort direction.</param>
    /// <param name="tenantId">TenantId. Unique for each Tenant</param>
    /// <returns>Returns TransactionHistoryExtended.</returns>
    Task<PagedData<TransactionHistoryExtended>> GetHistoryDataAsync(string alias, int timePeriod, string searchCriteria,
        string approverDomain, string approverId, int? page = null, string sortColumn = null, string sortDirection = "DESC", string tenantId = "");

    /// <summary>
    /// This method will check whether history is inserted or not
    /// </summary>
    /// <param name="tenantInfo">The tenantInfo.</param>
    /// <param name="alias">The alias.</param>
    /// <param name="actionDate">The actionDate.</param>
    /// <param name="documentNumber">The documentNumber.</param>
    /// <param name="actionTaken">The actionTaken.</param>
    /// <param name="domain">Approver Domain</param>
    /// <param name="approverId">Approver Object Id</param>
    /// <returns>Returns boolean to indicate whether history is inserted or not</returns>
    Task<bool> CheckIfHistoryInsertedAsync(ApprovalTenantInfo tenantInfo, string alias, string actionDate, string documentNumber, string actionTaken, string domain, string approverId);

    /// <summary>
    /// This method will insert Approval History
    /// </summary>
    /// <param name="tenantInfo">The tenantInfo.</param>
    /// <param name="historyData">The historyData.</param>
    Task AddApprovalHistoryAsync(ApprovalTenantInfo tenantInfo, TransactionHistory historyData);

    /// <summary>
    /// This method will insert list of TransactionHistory into azure table storage
    /// </summary>
    /// <param name="historyDataList">List of TransactionHistory</param>
    Task AddApprovalHistoryAsync(ApprovalTenantInfo tenantInfo, List<TransactionHistory> historyDataList);

    /// <summary>
    /// This method will get history count for the user alias.
    /// </summary>
    /// <param name="alias">The alias.</param>
    /// <param name="timePeriod">The timePeriod.</param>
    /// <param name="searchCriteria">The searchCriteria.</param>
    /// <param name="signedInUser">The signed-in user entity.</param>
    /// <param name="Xcv">The Xcv.</param>
    /// <returns>Returns JArray which includes count information.</returns>
    Task<JArray> GetHistoryCountforAliasAsync(string alias, int timePeriod, string searchCriteria, User signedInUser, string Xcv);

    /// <summary>
    /// Get history counts for each month in a specified time period
    /// </summary>
    /// <param name="alias"></param>
    /// <param name="timePeriod"></param>
    /// <param name="tcv"></param>
    /// <param name="approverDomain"></param>
    /// <param name="approverId"></param>
    /// <returns></returns>
    Task<JArray> GetHistoryIntervalCountsforAliasAsync(string alias, int timePeriod, string tcv, string approverDomain, string approverId);

    /// <summary>
    /// This method will Get Approver chain history data.
    /// </summary>
    /// <param name="tenantInfo">The tenantInfo.</param>
    /// <param name="documentNumber">The documentNumber.</param>
    /// <param name="fiscalYear">The fiscalYear.</param>
    /// <param name="alias">The alias.</param>
    /// <param name="Xcv">The Xcv.</param>
    /// <param name="Tcv">The Tcv.</param>
    /// <param name="sessionId">The sessionId.</param>
    /// <returns>Returns list of TransactionHistory extension.</returns>
    Task<List<TransactionHistoryExt>> GetApproverChainHistoryDataAsync(ApprovalTenantInfo tenantInfo, string documentNumber,
        string fiscalYear, string alias, string Xcv, string Tcv, string sessionId = null);

    /// <summary>
    /// This method will get History data.
    /// </summary>
    /// <param name="tenantInfo">The tenantInfo.</param>
    /// <param name="documentNumber">The documentNumber.</param>
    /// <param name="approver">The approver.</param>
    /// <param name="Xcv">The Xcv.</param>
    /// <param name="Tcv">The Tcv.</param>
    /// <returns>Returns List of TransactionHistory</returns>
    Task<List<TransactionHistory>> GetHistoryDataAsync(ApprovalTenantInfo tenantInfo, string documentNumber, string approver, string Xcv, string Tcv);

    /// <summary>
    /// This method will get ApproverChainHistoryData
    /// </summary>
    /// <param name="tenantInfo">The tenantInfo.</param>
    /// <param name="documentNumber">The documentNumber.</param>
    /// <param name="xcv">The xcv.</param>
    /// <param name="tcv">The tcv.</param>
    /// <param name="clientDevice">The clientDevice.</param>
    /// <param name="sessionId">The sessionId.</param>
    /// <returns>Returns List of TransactionHistoryExtension</returns>
    Task<List<TransactionHistoryExt>> GetApproverChainHistoryDataAsync(ApprovalTenantInfo tenantInfo, string documentNumber, string xcv, string tcv, string clientDevice, string sessionId = null);
}
