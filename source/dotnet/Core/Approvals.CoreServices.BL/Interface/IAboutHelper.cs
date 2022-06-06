// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace Microsoft.CFS.Approvals.CoreServices.BL.Interface
{
    using Newtonsoft.Json.Linq;

    public interface IAboutHelper
    {
        /// <summary>
        /// Get About helper
        /// </summary>
        /// <param name="host"></param>
        /// <param name="sessionId"></param>
        /// <param name="loggedInAlias"></param>
        /// <param name="clientDevice"></param>
        /// <param name="alias"></param>
        /// <returns></returns>
        dynamic GetAbout(string host, string sessionId, string loggedInAlias, string clientDevice, string alias);

        /// <summary>
        /// Get help data
        /// </summary>
        /// <param name="host"></param>
        /// <param name="sessionId"></param>
        /// <param name="loggedInAlias"></param>
        /// <param name="clientDevice"></param>
        /// <param name="alias"></param>
        /// <returns></returns>
        JObject GetHelpData(string host, string sessionId, string loggedInAlias, string clientDevice, string alias);
    }
}