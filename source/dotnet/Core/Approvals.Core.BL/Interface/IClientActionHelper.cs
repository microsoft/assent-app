// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace Microsoft.CFS.Approvals.Core.BL.Interface
{
    using System.Threading.Tasks;
    using Microsoft.AspNetCore.Http;
    using Microsoft.AspNetCore.Mvc;
    using Microsoft.CFS.Approvals.Contracts;

    public interface IClientActionHelper
    {
        /// <summary>
        /// Processes the User String and formulates a proper Response Card OR Error response after taking action on request from Outlook
        /// </summary>
        /// <param name="tenantId">Tenant ID</param>
        /// <param name="request">Http request</param>
        /// <param name="clientDevice">Client Device</param>
        /// <param name="userAlias">User Alias</param>
        /// <param name="loggedInUser">Logged-in user Alias</param>
        /// <param name="aadUserToken">AAD Token</param>
        /// <param name="submissionType">Action submission type</param>
        /// <param name="xcv">X-Correlation ID</param>
        /// <param name="tcv">T-Correlation ID</param>
        /// <param name="sessionId">Session ID</param>
        /// <returns>Http Response</returns>
        Task<IActionResult> TakeActionFromNonWebClient(
                int tenantId,
                HttpRequest request,
                string clientDevice,
                string userAlias,
                string loggedInUser,
                string aadUserToken,
                ActionSubmissionType submissionType,
                string xcv = "",
                string tcv = "",
                string sessionId = "");

        /// <summary>
        /// Formulates a proper response card OR error response after checking the Status of a particular request
        /// </summary>
        /// <param name="tenantId">Tenant ID</param>
        /// <param name="request">Http request</param>
        /// <param name="clientDevice">Client Device</param>
        /// <param name="userAlias">User Alias</param>
        /// <param name="loggedInUser">Logged-in user Alias</param>
        /// <param name="tcv">T-correlation ID</param>
        /// <param name="sessionId">Session ID</param>
        /// <param name="xcv">X-Correlation ID</param>
        /// <returns>Https Response</returns>
        Task<IActionResult> ClientAutoRefresh(int tenantId, HttpRequest request, string clientDevice, string userAlias, string loggedInUser, string tcv, string sessionId, string xcv);
    }
}