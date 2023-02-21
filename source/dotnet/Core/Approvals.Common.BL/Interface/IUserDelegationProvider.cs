// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace Microsoft.CFS.Approvals.Common.DL.Interface;

using System.Collections.Generic;
using Microsoft.CFS.Approvals.Contracts;
using Microsoft.CFS.Approvals.Model.Flighting;

public interface IUserDelegationProvider
{
    /// <summary>
    /// Get delegation from a user
    /// </summary>
    /// <param name="DelegationTo"></param>
    /// <returns></returns>
    UserDelegationSetting GetDelegationFrom(string DelegationTo);

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
    /// Get user delegation for current user
    /// </summary>
    /// <param name="loggedInAlias"></param>
    /// <returns></returns>
    List<UserDelegationSetting> GetUserDelegationsForCurrentUser(string loggedInAlias);

    /// <summary>
    /// Get people delegated to logged in alias
    /// </summary>
    /// <param name="loggedInAlias"></param>
    /// <returns></returns>
    List<UserDelegationSetting> GetPeopleDelegatedToMe(string loggedInAlias);

    /// <summary>
    /// Get user delegation settings from a user
    /// </summary>
    /// <param name="managerAlias"></param>
    /// <param name="tenantId"></param>
    /// <param name="delegatedTo"></param>
    /// <returns></returns>
    List<UserDelegationSetting> GetUserDelegationSettingsFrom(string managerAlias, int tenantId, string delegatedTo);

    /// <summary>
    /// Get user delegation settings by Id
    /// </summary>
    /// <param name="rowKey"></param>
    /// <returns></returns>
    UserDelegationSetting GetUserDelegationSettingsById(string rowKey);

    /// <summary>
    /// Add delegation settings
    /// </summary>
    /// <param name="insertData"></param>
    void InsertDelegationSettings(UserDelegationSetting insertData);

    /// <summary>
    /// Update delegation settings
    /// </summary>
    /// <param name="delegationRow"></param>
    /// <param name="oldTenantId"></param>
    /// <param name="oldDelegatedUser"></param>
    void UpdateDelegationSettings(UserDelegationSetting delegationRow, int oldTenantId, string oldDelegatedUser);

    /// <summary>
    /// Delete delegation settings
    /// </summary>
    /// <param name="delegationRow"></param>
    void DeleteDelegationSettings(UserDelegationSetting delegationRow);
}
