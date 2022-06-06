// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace Microsoft.CFS.Approvals.Core.BL.Interface
{
    using System.Collections.Generic;
    using System.Threading.Tasks;
    using Newtonsoft.Json.Linq;

    /// <summary>
    /// Interface to fetch AdaptiveTemplates based on clientDevice
    /// </summary>
    public interface IAdaptiveDetailsHelper
    {
        /// <summary>
        /// Get Adaptive tempalates based on the Call Type
        /// </summary>
        /// <param name="tenantId">Tenant ID</param>
        /// <param name="userAlias">User Alias</param>
        /// <param name="loggedInAlias">Logged-in Alias</param>
        /// <param name="clientDevice">Client Device</param>
        /// <param name="aadUserToken">AAD User Token</param>
        /// <param name="sessionId">Session ID</param>
        /// <param name="xcv">X-correlation ID</param>
        /// <param name="tcv">T-Correlation ID</param>
        /// <param name="templateType">Template Type</param>
        /// <returns>Adaptive Card Payload JObject</returns>
        Task<Dictionary<string, JObject>> GetAdaptiveTemplate(
            int tenantId,
            string userAlias,
            string loggedInAlias,
            string clientDevice,
            string aadUserToken,
            string sessionId,
            string xcv,
            string tcv,
            int templateType);
    }
}