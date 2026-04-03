// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace Microsoft.CFS.Approvals.Core.BL.Interface;

using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.CFS.Approvals.Contracts;
using Microsoft.CFS.Approvals.Contracts.DataContracts;
using Microsoft.CFS.Approvals.Model;
using Newtonsoft.Json.Linq;

public interface IDelegationHelper
{
    /// <summary>
    /// Get delegation access level
    /// </summary>
    /// <param name="manager"></param>
    /// <param name="delegateToUser"></param>
    /// <returns></returns>
    DelegationAccessLevel GetDelegationAccessLevel(User manager, User delegateToUser);

    /// <summary>
    /// Get user delegations for current user
    /// </summary>
    /// <param name="signedInUser"></param>
    /// <returns></returns>
    List<UserDelegationSetting> GetUserDelegationsForCurrentUser(User signedInUser);

    /// <summary>
    /// Get people delegated to logged in user
    /// </summary>
    /// <param name="signedInUser"></param>
    /// <returns></returns>
    List<UserDelegationSetting> GetPeopleDelegatedToMe(User signedInUser);

    /// <summary>
    /// Get info of people delegated to logged in user
    /// </summary>
    /// <param name="signedInUser"></param>
    /// <param name="onBehalfUser"></param>
    /// <param name="clientDevice"></param>
    /// <param name="sessionId"></param>
    /// <param name="xcv"></param>
    /// <param name="tcv"></param>
    /// <returns></returns>
    Task<List<dynamic>> GetInfoOfPeopleDelegatedToMe(User signedInUser, User onBehalfUser, string clientDevice, string sessionId, string xcv, string tcv);

    /// <summary>
    /// Get user delegated to async
    /// </summary>
    /// <param name="signedInUser"></param>
    /// <param name="onBehalfUser"></param>
    /// <param name="tenantId"></param>
    /// <param name="clientDevice"></param>
    /// <param name="sessionId"></param>
    /// <param name="xcv"></param>
    /// <param name="tcv"></param>
    /// <returns></returns>
    Task<JArray> GetUsersDelegatedToAsync(User signedInUser, User onBehalfUser, int tenantId, string clientDevice, string sessionId, string xcv, string tcv);

    /// <summary>
    /// Gets delegation data from Delegation Platform API and UserDelegationSettings table
    /// </summary>
    /// <param name="signedInUser"></param>
    /// <param name="onBehalfUser"></param>
    /// <param name="oauth2UserToken"></param>
    /// <param name="clientDevice"></param>
    /// <param name="sessionId"></param>
    /// <param name="xcv"></param>
    /// <param name="tcv"></param>
    /// <returns></returns>
    public Task<JArray> GetMergedDelegationData(User signedInUser, User onBehalfUser, string oauth2UserToken, string clientDevice, string sessionId, string xcv, string tcv);

    /// <summary>
    /// Gets user delegation from Delegation platform API
    /// </summary>
    /// <param name="signedInUser"></param>
    /// <param name="onBehalfUser"></param>
    /// <param name="clientDevice"></param>
    /// <param name="sessionId"></param>
    /// <param name="xcv"></param>
    /// <param name="tcv"></param>
    /// <returns></returns>
    public Task<List<DelegationPlatformResponse>> GetUserDelegation(User signedInUser, User onBehalfUser, string oauth2UserToken, string clientDevice, string sessionId, string xcv, string tcv);

    /// <summary>
    /// Check if the user is authorized to view the report based on delegation settings
    /// </summary>
    /// <param name="signedInUser"></param>
    /// <param name="onBehalfUser"></param>
    /// <param name="oauth2UserToken"></param>
    /// <param name="clientDevice"></param>
    /// <param name="sessionId"></param>
    /// <param name="xcv"></param>
    /// <param name="tcv"></param>
    /// <returns></returns>
    /// <exception cref="UnauthorizedAccessException"></exception>
    public Task<bool> CheckUserAuthorization(User signedInUser, User onBehalfUser, string oauth2UserToken, string clientDevice, string sessionId, string xcv, string tcv);
}