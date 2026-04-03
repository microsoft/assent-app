using Microsoft.CFS.Approvals.Contracts.DataContracts;
using Microsoft.CFS.Approvals.Model;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Microsoft.CFS.Approvals.Core.BL.Interface;

public interface ISearchHelper
{
    /// <summary>
    /// The function that returns the search results (document numbers) based on the query
    /// </summary>
    /// <param name="searchFilters"></param>
    /// <param name="domain"></param>
    /// <param name="signedInUser"></param>
    /// <param name="onBehalfUser"></param>
    /// <param name="host"></param>
    /// <param name="oauth2UserToken"></param>
    /// <returns></returns>
    Task<List<string>> GetSearchResultsDocumentNumbersAsync(
        Filters searchFilters,
        string domain,
        User signedInUser,
        User onBehalfUser,
        string host,
        string oauth2UserToken
        );

    /// <summary>
    /// The function that returns the search results (summary objects) based on the query
    /// </summary>
    /// <param name="searchFilters"></param>
    /// <param name="domain"></param>
    /// <param name="signedInUser"></param>
    /// <param name="onBehalfUser"></param>
    /// <param name="host"></param>
    /// <param name="oauth2UserToken"></param>
    /// <returns></returns>
    Task<List<ApprovalSummaryData>> GetSearchResultSummaryObjectsAsync(
        Filters searchFilters, 
        string domain, 
        User signedInUser, 
        User onBehalfUser, 
        string host, 
        string oauth2UserToken);

    /// <summary>
    /// Attempts to resolve the tenantId for a given document (display) document number that exists in the user's current queue.
    /// Returns null if not found.
    /// </summary>
    /// <param name="documentNumber">Document or display document number.</param>
    /// <param name="signedInUser">Signed in user.</param>
    /// <param name="onBehalfUser">On behalf user.</param>
    /// <param name="host">Host (used to scope tenants).</param>
    /// <param name="domain">User domain.</param>
    /// <param name="oauth2UserToken">Auth token.</param>
    /// <returns>tenantId if found else null.</returns>
    Task<int?> FindTenantIdByDocumentNumberAsync(string documentNumber, User signedInUser, User onBehalfUser, string host, string domain, string oauth2UserToken);
}

