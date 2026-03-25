// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.CFS.Approvals.Contracts;
using Microsoft.CFS.Approvals.Model;
using Microsoft.CFS.Approvals.Contracts.DataContracts;

namespace Microsoft.CFS.Approvals.Core.BL.Interface;

/// <summary>
/// Abstraction for performing AI-assisted search over approval data.
/// </summary>
public interface IAIAssistedSearchHelper
{
    /// <summary>
    /// Executes an AI-assisted search for the given <paramref name="query"/> and returns results of the specified <paramref name="returnType"/>.
    /// </summary>
    /// <typeparam name="T">The result type to materialize (e.g., <c>ApprovalSummaryData</c>).</typeparam>
    /// <param name="signedInUser">The signed-in user.</param>
    /// <param name="onBehalfUser">The user for whom the search is being performed.</param>
    /// <param name="query">The natural language query.</param>
    /// <param name="returnType">The result detail level.</param>
    /// <param name="domain">The domain derived from UPN.</param>
    /// <param name="host">The client host or device identifier.</param>
    /// <param name="oauth2UserToken">OAuth2 token for downstream calls.</param>
    /// <returns>A list of results of type <typeparamref name="T"/>.</returns>
    Task<List<T>> GetAIAssistedSearchResults<T>(User signedInUser, User onBehalfUser, string query, SearchResultReturnType returnType, string domain, string host, string oauth2UserToken);
}
