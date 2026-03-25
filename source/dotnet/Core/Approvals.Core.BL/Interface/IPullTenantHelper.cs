// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace Microsoft.CFS.Approvals.Core.BL.Interface;

using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.CFS.Approvals.Contracts.DataContracts;
using Newtonsoft.Json.Linq;

/// <summary>
/// Interface IPullTenantHelper.
/// </summary>
public interface IPullTenantHelper
{
    /// <summary>
    /// Get summary information for pending approval requests from tenant system.
    /// </summary>
    /// <param name="signedInUser">signed-in user</param>
    /// <param name="onBehalfUser">on-behalf user</param>
    /// <param name="oauth2UserToken">OAuth2 user token.</param></param>
    /// <param name="parameters">Input filter parameters.</param>
    /// <param name="tenantId">Tenant Id.</param>
    /// <param name="clientDevice">Client Device.</param>
    /// <param name="sessionId">Session Id.</param>
    /// <param name="xcv">XCV.</param>
    /// <param name="tcv">TCV.</param>
    /// <returns>Summary records.</returns>
    Task<JObject> GetSummaryAsync(User signedInUser, User onBehalfUser, string oauth2UserToken, Dictionary<string, object> parameters, int tenantId, string clientDevice, string sessionId, string xcv, string tcv);

    /// <summary>
    /// Get details information for an approval request from tenant system.
    /// </summary>
    /// <param name="signedInUser">signed-in user</param>
    /// <param name="onBehalfUser">on-behalf user</param>
    /// <param name="oauth2UserToken">OAuth2 user token.</param>
    /// <param name="operationType">Type of operation which needs to be executed from tenant info configuration.</param>
    /// <param name="parameters">Input filter parameters.</param>
    /// <param name="tenantId">Tenant Id.</param>
    /// <param name="clientDevice">Client Device.</param>
    /// <param name="sessionId">Session Id.</param>
    /// <param name="xcv">XCV.</param>
    /// <param name="tcv">TCV.</param>
    /// <returns>Details of a request.</returns>
    Task<JObject> GetDetailsAsync(User signedInUser, User onBehalfUser, string oauth2UserToken, string operationType, Dictionary<string, object> parameters, int tenantId, string clientDevice, string sessionId, string xcv, string tcv);

    /// <summary>
    /// Get summary count for pull tenants.
    /// </summary>
    /// <param name="signedInUser">Logged-in user</param>
    /// <param name="onBehalfUser">On-behalf user</param>
    /// <param name="oauth2UserToken">OAuth2 user token.</param>
    /// <param name="operationType">Type of operation which needs to be executed from tenant info configuration.</param>
    /// <param name="sessionId">Session Id.</param>
    /// <param name="xcv">XCV.</param>
    /// <param name="tcv">TCV.</param>
    /// <param name="clientDevice">Client Device.</param>
    /// <returns>Array of summary count.</returns>
    Task<JArray> GetSummaryCountAsync(User signedInUser, User onBehalfUser, string oauth2UserToken, string operationType, string sessionId, string xcv, string tcv, string clientDevice);
}