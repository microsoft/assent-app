// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace Microsoft.CFS.Approvals.Core.BL.Interface
{
    public interface IReadDetailsHelper
    {
        /// <summary>
        /// Update is read details
        /// </summary>
        /// <param name="postData"></param>
        /// <param name="tenantId"></param>
        /// <param name="loggedInAlias"></param>
        /// <param name="alias"></param>
        /// <param name="clientDevice"></param>
        /// <param name="sessionId"></param>
        /// <param name="Tcv"></param>
        /// <param name="Xcv"></param>
        /// <returns></returns>
        bool UpdateIsReadDetails(string postData, int tenantId, string loggedInAlias, string alias, string clientDevice, string sessionId, string Tcv, string Xcv);
    }
}