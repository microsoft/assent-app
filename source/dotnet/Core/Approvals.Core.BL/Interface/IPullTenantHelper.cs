// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace Microsoft.CFS.Approvals.Core.BL.Interface
{
    using System.Collections.Generic;
    using System.Threading.Tasks;
    using Newtonsoft.Json.Linq;

    /// <summary>
    /// Interface IPullTenantHelper.
    /// </summary>
    public interface IPullTenantHelper
    {
        /// <summary>
        /// Get summary information for pending approval requests from tenant system.
        /// </summary>
        /// <param name="approverAlias">Approver alias (Contains delegated user alias if operation is performed on behalf of delegated user).</param>
        /// <param name="loggedInAlias">Logged-in user alias.</param>
        /// <param name="parameters">Input filter parameters.</param>
        /// <param name="tenantId">Tenant Id.</param>
        /// <param name="clientDevice">Client Device.</param>
        /// <param name="sessionId">Session Id.</param>
        /// <param name="xcv">XCV.</param>
        /// <param name="tcv">TCV.</param>
        /// <returns>Summary records.</returns>
        Task<JObject> GetSummaryAsync
        (
            string approverAlias,
            string loggedInAlias,
            Dictionary<string, object> parameters,
            int tenantId,
            string clientDevice,
            string sessionId,
            string xcv,
            string tcv
        );

        /// <summary>
        /// Get details information for an approval request from tenant system.
        /// </summary>
        /// <param name="approverAlias">Approver alias (Contains delegated user alias if operation is performed on behalf of delegated user).</param>
        /// <param name="loggedInAlias">Logged-in user alias.</param>
        /// <param name="operationType">Type of operation which needs to be executed from tenant info configuration.</param>
        /// <param name="parameters">Input filter parameters.</param>
        /// <param name="tenantId">Tenant Id.</param>
        /// <param name="clientDevice">Client Device.</param>
        /// <param name="sessionId">Session Id.</param>
        /// <param name="xcv">XCV.</param>
        /// <param name="tcv">TCV.</param>
        /// <returns>Details of a request.</returns>
        Task<JObject> GetDetailsAsync
        (
            string approverAlias,
            string loggedInAlias,
            string operationType,
            Dictionary<string, object> parameters,
            int tenantId,
            string clientDevice,
            string sessionId,
            string xcv,
            string tcv
        );

        /// <summary>
        /// Get summary count for pull tenants.
        /// </summary>
        /// <param name="operationType">Type of operation which needs to be executed from tenant info configuration.</param>
        /// <param name="approverAlias">Approver alias (Contains delegated user alias if operation is performed on behalf of delegated user).</param>
        /// <param name="loggedInAlias">Logged-in user alias.</param>
        /// <param name="sessionId">Session Id.</param>
        /// <param name="xcv">XCV.</param>
        /// <param name="tcv">TCV.</param>
        /// <param name="clientDevice">Client Device.</param>
        /// <returns>Array of SummaryCount</returns>
        Task<JArray> GetSummaryCountAsync(
            string operationType,
            string approverAlias,
            string loggedInAlias,
            string sessionId,
            string xcv,
            string tcv,
            string clientDevice);
    }
}