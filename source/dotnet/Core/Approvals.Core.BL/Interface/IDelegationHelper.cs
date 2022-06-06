// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace Microsoft.CFS.Approvals.Core.BL.Interface
{
    using System;
    using System.Collections.Generic;
    using System.Threading.Tasks;
    using Microsoft.CFS.Approvals.Contracts;
    using Microsoft.CFS.Approvals.Model.Flighting;
    using Newtonsoft.Json.Linq;

    public interface IDelegationHelper
    {
        /// <summary>
        /// Get delegation from user alias
        /// </summary>
        /// <param name="delegateToAlias"></param>
        /// <returns></returns>
        UserDelegationSetting GetDelegationFrom(string delegateToAlias);

        /// <summary>
        /// Get delegation access level
        /// </summary>
        /// <param name="managerAlias"></param>
        /// <param name="delegateToAlias"></param>
        /// <returns></returns>
        DelegationAccessLevel GetDelegationAccessLevel(string managerAlias, string delegateToAlias);

        /// <summary>
        /// Get all user delegation settings
        /// </summary>
        /// <returns></returns>
        List<UserDelegationSetting> GetAllUserDelegationSettings();

        /// <summary>
        /// Get user delegations for current user
        /// </summary>
        /// <param name="loggedInUserAlias"></param>
        /// <returns></returns>
        List<UserDelegationSetting> GetUserDelegationsForCurrentUser(string loggedInUserAlias);

        /// <summary>
        /// Get people delegated to logged in user
        /// </summary>
        /// <param name="loggedInUserAlias"></param>
        /// <returns></returns>
        List<UserDelegationSetting> GetPeopleDelegatedToMe(string loggedInUserAlias);

        /// <summary>
        /// Get user delegation settings
        /// </summary>
        /// <param name="loggedInUserAlias"></param>
        /// <param name="tenantId"></param>
        /// <param name="delegatedTo"></param>
        /// <returns></returns>
        List<UserDelegationSetting> GetUserDelegationSettingsFrom(string loggedInUserAlias, int tenantId, string delegatedTo);

        /// <summary>
        /// Get user delegation settings by RowKey
        /// </summary>
        /// <param name="rowKey"></param>
        /// <returns></returns>
        UserDelegationSetting GetUserDelegationSettingsById(string rowKey);

        /// <summary>
        /// Add delegation settings
        /// </summary>
        /// <param name="insertData"></param>
        /// <param name="loggedInUserAlias"></param>
        /// <param name="clientDevice"></param>
        void InsertDelegationSettings(UserDelegationSetting insertData, string loggedInUserAlias, string clientDevice);

        /// <summary>
        /// Get info of people delegated to logged in user
        /// </summary>
        /// <param name="loggedInUserAlias"></param>
        /// <param name="alias"></param>
        /// <param name="clientDevice"></param>
        /// <param name="sessionId"></param>
        /// <param name="xcv"></param>
        /// <param name="tcv"></param>
        /// <returns></returns>
        Task<List<dynamic>> GetInfoOfPeopleDelegatedToMe(string loggedInUserAlias, string alias, string clientDevice, string sessionId, string xcv, string tcv);

        /// <summary>
        /// Get user delegated to async
        /// </summary>
        /// <param name="loggedInUserAlias"></param>
        /// <param name="alias"></param>
        /// <param name="tenantId"></param>
        /// <param name="clientDevice"></param>
        /// <param name="sessionId"></param>
        /// <param name="xcv"></param>
        /// <param name="tcv"></param>
        /// <returns></returns>
        Task<JArray> GetUsersDelegatedToAsync(string loggedInUserAlias, string alias, int tenantId, string clientDevice, string sessionId, string xcv, string tcv);

        /// <summary>
        /// Process  and insert delegation
        /// </summary>
        /// <param name="loggedInUserAlias"></param>
        /// <param name="alias"></param>
        /// <param name="clientDevice"></param>
        /// <param name="sessionId"></param>
        /// <param name="jsonData"></param>
        Task ProcessAndInsertDelegation(string loggedInUserAlias, string alias, string clientDevice, string sessionId, string jsonData);

        /// <summary>
        /// Update delegation settings
        /// </summary>
        /// <param name="delegationRow"></param>
        /// <param name="oldTenantId"></param>
        /// <param name="oldDelegatedUser"></param>
        /// <param name="loggedInUserAlias"></param>
        /// <param name="clientDevice"></param>
        /// <param name="sessionId"></param>
        void UpdateDelegationSettings(UserDelegationSetting delegationRow, int oldTenantId, string oldDelegatedUser, string loggedInUserAlias, string clientDevice, string sessionId);

        /// <summary>
        /// Process and update delegation
        /// </summary>
        /// <param name="loggedInUserAlias"></param>
        /// <param name="alias"></param>
        /// <param name="clientDevice"></param>
        /// <param name="sessionId"></param>
        /// <param name="jsonData"></param>
        Task ProcessAndUpdateDelegation(string loggedInUserAlias, string alias, string clientDevice, string sessionId, string jsonData);

        /// <summary>
        /// Delete delegation settings
        /// </summary>
        /// <param name="delegationRow"></param>
        /// <param name="sessionId"></param>
        /// <param name="clientDevice"></param>
        void DeleteDelegationSettings(UserDelegationSetting delegationRow, string sessionId = "", string clientDevice = "");
    }
}