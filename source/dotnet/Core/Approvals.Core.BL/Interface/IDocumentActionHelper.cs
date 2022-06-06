// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace Microsoft.CFS.Approvals.Core.BL.Interface
{
    using System.Threading.Tasks;
    using Newtonsoft.Json.Linq;

    public interface IDocumentActionHelper
    {
        /// <summary>
        /// Take action
        /// </summary>
        /// <param name="tenantId"></param>
        /// <param name="userActionsString"></param>
        /// <param name="clientDevice"></param>
        /// <param name="userAlias"></param>
        /// <param name="loggedInUser"></param>
        /// <param name="aadUserToken"></param>
        /// <param name="xcv"></param>
        /// <param name="tcv"></param>
        /// <param name="sessionId"></param>
        /// <returns></returns>
        Task<JObject> TakeAction
            (
                int tenantId,
                string userActionsString,
                string clientDevice,
                string userAlias,
                string loggedInUser,
                string aadUserToken,
                string xcv,
                string tcv,
                string sessionId
            );
    }
}