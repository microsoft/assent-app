// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace Microsoft.CFS.Approvals.Core.BL.Interface;

using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.CFS.Approvals.Contracts;
using Microsoft.CFS.Approvals.Contracts.DataContracts;

public interface IClientActionHelper
{
    /// <summary>
    /// Processes the User String and formulates a proper Response Card OR Error response after taking action on request from Outlook
    /// </summary>
    /// <param name="tenantId">Tenant ID</param>
    /// <param name="request">Http request</param>
    /// <param name="clientDevice">Client Device</param>
    /// <param name="onBehalfUser">On-behalf user entity</param>
    /// <param name="signedInUser">signed-in user entity</param>
    /// <param name="oauth2UserToken">OAuth 2.0 Token</param>
    /// <param name="submissionType">Action submission type</param>
    /// <param name="xcv">X-Correlation ID</param>
    /// <param name="tcv">T-Correlation ID</param>
    /// <param name="sessionId">Session ID</param>
    /// <returns>Http Response</returns>
    Task<IActionResult> TakeActionFromNonWebClient(
            int tenantId,
            HttpRequest request,
            string clientDevice,
            User onBehalfUser,
            User signedInUser,
            string oauth2UserToken,
            ActionSubmissionType submissionType,
            string xcv = "",
            string tcv = "",
            string sessionId = "");

    /// <summary>
    /// Formulates a proper response card OR error response after checking the Status of a particular request
    /// </summary>
    /// <param name="onBehalfUser">on-Behalf User</param>
    /// <param name="signedInUser">Logged-in user </param>
    /// <param name="oauth2UserToken"></param>
    /// <param name="tenantId">Tenant ID</param>
    /// <param name="request">Http request</param>
    /// <param name="clientDevice">Client Device</param>
    /// <param name="tcv">T-correlation ID</param>
    /// <param name="sessionId">Session ID</param>
    /// <param name="xcv">X-Correlation ID</param>
    /// <param name="domainName"></param>
    /// <returns>Https Response</returns>
    Task<IActionResult> ClientAutoRefresh(User signedInUser, User onBehalfUser, string oauth2UserToken, int tenantId, HttpRequest request, string clientDevice, string tcv, string sessionId, string xcv, string domainName);
}