// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace Microsoft.CFS.Approvals.Common.DL;

using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.CFS.Approvals.Common.DL.Interface;
using Microsoft.CFS.Approvals.Contracts;
using Microsoft.CFS.Approvals.Contracts.DataContracts;
using Microsoft.CFS.Approvals.Data.Azure.Storage.Interface;
using Microsoft.CFS.Approvals.Extensions;
using Microsoft.CFS.Approvals.Model;
using Microsoft.Extensions.Configuration;

/// <summary>
/// The User Delegation Provider class
/// </summary>
public class UserDelegationProvider : IUserDelegationProvider
{
    /// <summary>
    /// The table helper
    /// </summary>
    private readonly ITableHelper _tableHelper;

    /// <summary>
    /// The configuration object
    /// </summary>
    private readonly IConfiguration _config;

    /// <summary>
    /// Constructor of UserDelegationProvider
    /// </summary>
    /// <param name="tableHelper"></param>
    /// <param name="config"></param>
    public UserDelegationProvider(ITableHelper tableHelper, IConfiguration config)
    {
        _tableHelper = tableHelper;
        _config = config;
    }

    /// <summary>
    /// Get delegation access level
    /// </summary>
    /// <param name="manager"></param>
    /// <param name="delegateToUser"></param>
    /// <returns></returns>
    public DelegationAccessLevel GetDelegationAccessLevel(User manager, User delegateToUser)
    {
        var accesLevel = DelegationAccessLevel.Admin;
        string query = "PartitionKey eq '" + manager.UserPrincipalName + "' and DelegateUpn eq '" + delegateToUser.UserPrincipalName + "'";
        var userDelegationSetting = _tableHelper.GetDataCollectionByTableQuery<UserDelegationSetting>(_config[ConfigurationKey.UserDelegationSettingsAzureTableName.ToString()], query);
        if (userDelegationSetting != null && userDelegationSetting.FirstOrDefault() != null)
            if (userDelegationSetting.FirstOrDefault().AccessType == 1)
                accesLevel = DelegationAccessLevel.ReadOnly;
        return accesLevel;
    }

    /// <summary>
    /// Get a user delegated to current logged in user
    /// </summary>
    /// <param name="loggedInUser"></param>
    /// <returns></returns>
    public List<UserDelegationSetting> GetPeopleDelegatedToMe(User loggedInUser)
    {
        string query = "DelegateId eq '" + loggedInUser.Id + "' and DateFrom le datetime'" + DateTime.UtcNow.ToString("yyyy-MM-ddTHH:mm:ssZ") + "' and DateTo ge datetime'" + DateTime.UtcNow.ToString("yyyy-MM-ddTHH:mm:ssZ") + "'";
        return _tableHelper.GetDataCollectionByTableQuery<UserDelegationSetting>(_config[ConfigurationKey.UserDelegationSettingsAzureTableName.ToString()], query);
    }

    /// <summary>
    /// Get user delegations for current user
    /// </summary>
    /// <param name="loggedInUser"></param>
    /// <returns></returns>
    public List<UserDelegationSetting> GetUserDelegationsForCurrentUser(User loggedInUser)
    {
        return _tableHelper.GetTableEntityListByPartitionKey<UserDelegationSetting>(_config[ConfigurationKey.UserDelegationSettingsAzureTableName.ToString()], loggedInUser.UserPrincipalName.ToLowerInvariant());
    }
}