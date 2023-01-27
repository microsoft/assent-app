// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace Microsoft.CFS.Approvals.CoreServices.BL.Interface;

using System.Threading.Tasks;
using Newtonsoft.Json.Linq;

/// <summary>
/// Interface IApprovalHistoryHelper.
/// </summary>
public interface IApprovalHistoryHelper
{
    /// <summary>
    /// Get history data for the user with the given search criteria.
    /// </summary>
    /// <param name="page">Page number.</param>
    /// <param name="sortColumn">Sort column.</param>
    /// <param name="sortDirection">Sort direction (ASC or DESC).</param>
    /// <param name="searchCriteria">Search criteria.</param>
    /// <param name="timePeriod">Time period.</param>
    /// <param name="sessionId">Session Id.</param>
    /// <param name="loggedInAlias">Logged-in user alias.</param>
    /// <param name="alias">Alias.</param>
    /// <param name="clientDevice">Client device.</param>
    /// <param name="xcv">Xcv.</param>
    /// <param name="tcv">Tcv.</param>
    /// <param name="tenantId">TenantId. Unique for each Tenant</param>
    /// <returns>History data for the user with the given search criteria.</returns>
    Task<JObject> GetHistory(int page, string sortColumn, string sortDirection, string searchCriteria, int timePeriod, string sessionId, string loggedInAlias, string alias, string clientDevice, string xcv, string tcv, string tenantId = "");

    /// <summary>
    /// Download history data for the user with the given search criteria in excel.
    /// </summary>
    /// <param name="sortColumn">Sort column.</param>
    /// <param name="sortDirection">Sort direction (ASC or DESC).</param>
    /// <param name="searchCriteria">Search criteria.</param>
    /// <param name="timePeriod">Time period.</param>
    /// <param name="sessionId">Session Id.</param>
    /// <param name="loggedInAlias">Logged-in user alias.</param>
    /// <param name="alias">Alias.</param>
    /// <param name="clientDevice">Client device.</param>
    /// <param name="xcv">Xcv.</param>
    /// <param name="tcv">Tcv.</param>
    /// <param name="tenantId">TenantId. Unique for each Tenant</param>
    /// <returns>Excel with hisotry data for the user with the given search criteria.</returns>
    Task<byte[]> DownloadHistoryDataInExcel(string sortColumn, string sortDirection, string searchCriteria, int timePeriod, string sessionId, string loggedInAlias, string alias, string clientDevice, string xcv, string tcv, string tenantId = "");

    /// <summary>
    /// Get history count for the user with the given search criteria.
    /// </summary>
    /// <param name="alias">Alias.</param>
    /// <param name="timePeriod">Time period.</param>
    /// <param name="searchCriteria">Search criteria.</param>
    /// <param name="loggedInAlias">Logged-in user alias.</param>
    /// <param name="sessionId">Session Id.</param>
    /// <param name="clientDevice">Client device.</param>
    /// <param name="xcv">Xcv.</param>
    /// <param name="tcv">Tcv.</param>
    /// <returns>History count for the user with the given search criteria.</returns>
    Task<JArray> GetHistoryCountforAlias(string alias, int timePeriod, string searchCriteria, string loggedInAlias, string sessionId, string clientDevice, string xcv, string tcv);

    /// <summary>
    /// Get history data for the user with the given search criteria.
    /// </summary>
    /// <param name="page">Page number.</param>
    /// <param name="sortColumn">Sort column.</param>
    /// <param name="sortDirection">Sort direction (ASC or DESC).</param>
    /// <param name="searchCriteria">Search criteria.</param>
    /// <param name="timePeriod">Time period.</param>
    /// <param name="sessionId">Session Id.</param>
    /// <param name="loggedInAlias">Logged-in user alias.</param>
    /// <param name="alias">Alias.</param>
    /// <param name="clientDevice">Client device.</param>
    /// <param name="xcv">Xcv.</param>
    /// <param name="tcv">Tcv.</param>
    /// <param name="tenantId">TenantId. Unique for each Tenant</param>
    /// <returns>History data for the user with the given search criteria.</returns>
    Task<JArray> GetHistoryMappedToSummary(int page, string sortColumn, string sortDirection, string searchCriteria, int timePeriod, string sessionId, string loggedInAlias, string alias, string clientDevice, string xcv, string tcv, string tenantId = "");
}