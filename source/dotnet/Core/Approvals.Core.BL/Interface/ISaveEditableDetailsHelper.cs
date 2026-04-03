// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace Microsoft.CFS.Approvals.Core.BL.Interface;

using Microsoft.CFS.Approvals.Contracts.DataContracts;
using System.Collections.Generic;
using System.Threading.Tasks;

public interface ISaveEditableDetailsHelper
{
    /// <summary>
    /// Method to check whether to enable Edit Details functionality for given user and tenant
    /// </summary>
    /// <param name="signedInUser"></param>
    /// <param name="onBehalfUser"></param>
    /// <param name="oauth2UserToken"></param>
    /// <param name="host"></param>
    /// <param name="Xcv"></param>
    /// <param name="Tcv"></param>
    /// <param name="tenantId"></param>
    /// <param name="documentNumber"></param>
    /// <returns></returns>
    Task<bool> CheckUserAuthorizationForEdit(User signedInUser, User onBehalfUser, string oauth2UserToken, string clientDevice , string Xcv, string Tcv, int tenantId, string documentNumber);

    /// <summary>
    /// Method to save the edited details into ApprovalDetails table
    /// </summary>
    /// <param name="signedInUser">logged in user</param>
    /// <param name="onBehalfUser">on-behalf user</param>
    /// <param name="oauth2UserToken">oauth2 user token</param>
    /// <param name="clientDevice">client device</param>
    /// <param name="detailsString">details string</param>
    /// <param name="tenantId">teanat</param>
    /// <param name="sessionId">session id</param>
    /// <param name="Xcv">cross corelational vector</param>
    /// <param name="Tcv">transactional vector</param>
    /// <returns></returns>
    Task<List<string>> SaveEditedDetails(User signedInUser, User onBehalfUser, string oauth2UserToken, string clientDevice, string detailsString, int tenantId, string sessionId, string Xcv, string Tcv);
}