// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Microsoft.CFS.Approvals.Contracts.DataContracts;
using System.Threading.Tasks;

namespace Microsoft.CFS.Approvals.Core.BL.Interface;

public interface IReadDetailsHelper
{
    /// <summary>
    /// Update detail table for IsRead flag
    /// </summary>
    /// <param name="signedInUser"></param>
    /// <param name="onBehalfUser"></param>
    /// <param name="oauth2UserToken"></param>
    /// <param name="postData"></param>
    /// <param name="tenantId"></param>

    /// <param name="clientDevice"></param>
    /// <param name="sessionId"></param>
    /// <param name="Tcv"></param>
    /// <param name="Xcv"></param>
    /// <param name="domain">Alias's domain</param>
    /// <returns></returns>
    Task<bool> UpdateIsReadDetails(User signedInUser, User onBehalfUser, string oauth2UserToken, string postData, int tenantId, string clientDevice, string sessionId, string Tcv, string Xcv, string domain);
}