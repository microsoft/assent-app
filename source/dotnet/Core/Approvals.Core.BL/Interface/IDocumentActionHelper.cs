// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace Microsoft.CFS.Approvals.Core.BL.Interface;

using System.Threading.Tasks;
using Microsoft.CFS.Approvals.Contracts.DataContracts;
using Newtonsoft.Json.Linq;

public interface IDocumentActionHelper
{
    /// <summary>
    /// Take action
    /// </summary>
    /// <param name="tenantId"></param>
    /// <param name="userActionsString"></param>
    /// <param name="clientDevice"></param>
    /// <param name="onBehalfUser"></param>
    /// <param name="signedInUser"></param>
    /// <param name="oauth2UserToken"></param>
    /// <param name="xcv"></param>
    /// <param name="tcv"></param>
    /// <param name="sessionId"></param>
    /// <returns></returns>
    Task<JObject> TakeAction
        (
            int tenantId,
            string userActionsString,
            string clientDevice,
            User onBehalfUser,
            User signedInUser,
            string oauth2UserToken,
            string xcv,
            string tcv,
            string sessionId
        );
}