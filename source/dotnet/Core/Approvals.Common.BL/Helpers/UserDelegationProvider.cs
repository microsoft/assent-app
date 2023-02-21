// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace Microsoft.CFS.Approvals.Common.DL;

using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.CFS.Approvals.Common.DL.Interface;
using Microsoft.CFS.Approvals.Contracts;
using Microsoft.CFS.Approvals.Data.Azure.Storage.Interface;
using Microsoft.CFS.Approvals.Extensions;
using Microsoft.CFS.Approvals.Model;
using Microsoft.CFS.Approvals.Model.Flighting;

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
    /// Constructor of UserDelegationProvider
    /// </summary>
    /// <param name="tableHelper"></param>
    public UserDelegationProvider(ITableHelper tableHelper)
    {
        _tableHelper = tableHelper;
    }

    /// <summary>
    /// Delete delegation settings
    /// </summary>
    /// <param name="delegationRow"></param>
    public void DeleteDelegationSettings(UserDelegationSetting delegationRow)
    {
        var userDelegationSetting = new UserDelegationSetting
        {
            PartitionKey = delegationRow.ManagerAlias,
            RowKey = delegationRow.RowKey,
            Id = delegationRow.Id,
            ManagerAlias = delegationRow.ManagerAlias,
            DelegatedToAlias = delegationRow.DelegatedToAlias,
            TenantId = delegationRow.TenantId,
            DateFrom = delegationRow.DateFrom,
            DateTo = delegationRow.DateTo,
            AccessType = delegationRow.AccessType,
            IsHidden = delegationRow.IsHidden,
            ETag = global::Azure.ETag.All
        };
        _tableHelper.DeleteRow(Constants.UserDelegationSettingsAzureTableName, userDelegationSetting);

        //insert to history
        var history = (delegationRow.ToJson()).FromJson<UserDelegationSettingsHistory>();
        history.RowKey = delegationRow.RowKey;
        history.Id = delegationRow.Id;
        history.Action = "Delete";
        history.ModifiedBy = delegationRow.ManagerAlias; //the current loggedin user
        history.ModifiedDate = DateTime.UtcNow;
        history.PartitionKey = delegationRow.ManagerAlias;
        history.ManagerAlias = delegationRow.ManagerAlias;
        history.DelegatedToAlias = delegationRow.DelegatedToAlias;
        history.TenantId = delegationRow.TenantId;
        history.DateFrom = delegationRow.DateFrom;
        history.DateTo = delegationRow.DateTo;
        history.AccessType = delegationRow.AccessType;
        history.IsHidden = delegationRow.IsHidden;
        _tableHelper.InsertOrReplace(Constants.UserDelegationSettingsHistoryAzureTableName, history);
    }

    /// <summary>
    /// Get all user delegation settings
    /// </summary>
    /// <returns></returns>
    public List<UserDelegationSetting> GetAllUserDelegationSettings()
    {
        return _tableHelper.GetTableEntity<UserDelegationSetting>(Constants.UserDelegationSettingsAzureTableName).ToList();
    }

    /// <summary>
    /// Get delegation access level
    /// </summary>
    /// <param name="managerAlias"></param>
    /// <param name="delegateToAlias"></param>
    /// <returns></returns>
    public DelegationAccessLevel GetDelegationAccessLevel(string managerAlias, string delegateToAlias)
    {
        var accesLevel = DelegationAccessLevel.Admin;
        string query = "PartitionKey eq '" + managerAlias + "' and DelegatedToAlias eq '" + delegateToAlias + "'";
        var userDelegationSetting = _tableHelper.GetDataCollectionByTableQuery<UserDelegationSetting>(Constants.UserDelegationSettingsAzureTableName, query);
        if (userDelegationSetting != null && userDelegationSetting.FirstOrDefault() != null)
            if (userDelegationSetting.FirstOrDefault().AccessType == 1)
                accesLevel = DelegationAccessLevel.ReadOnly;
        return accesLevel;
    }

    /// <summary>
    /// Get delegation from a user
    /// </summary>
    /// <param name="DelegationTo"></param>
    /// <returns></returns>
    public UserDelegationSetting GetDelegationFrom(string DelegationTo)
    {
        return _tableHelper.GetTableEntityByfield<UserDelegationSetting>(Constants.UserDelegationSettingsAzureTableName, "ManagerAlias", DelegationTo);
    }

    /// <summary>
    /// Get a user delegated to current logged in user
    /// </summary>
    /// <param name="loggedInAlias"></param>
    /// <returns></returns>
    public List<UserDelegationSetting> GetPeopleDelegatedToMe(string loggedInAlias)
    {
        string query = "DelegatedToAlias eq '" + loggedInAlias + "' and DateFrom le datetime'" + DateTime.UtcNow.ToString("yyyy-MM-ddTHH:mm:ssZ") + "' and DateTo ge datetime'" + DateTime.UtcNow.ToString("yyyy-MM-ddTHH:mm:ssZ") + "'";
        return _tableHelper.GetDataCollectionByTableQuery<UserDelegationSetting>(Constants.UserDelegationSettingsAzureTableName, query);
    }

    /// <summary>
    /// Get user delegation settings by Id
    /// </summary>
    /// <param name="rowKey"></param>
    /// <returns></returns>
    public UserDelegationSetting GetUserDelegationSettingsById(string rowKey)
    {
        return _tableHelper.GetTableEntityByRowKey<UserDelegationSetting>(Constants.UserDelegationSettingsAzureTableName, rowKey);
    }

    /// <summary>
    /// Get user delegation settings from one user to another
    /// </summary>
    /// <param name="managerAlias"></param>
    /// <param name="tenantId"></param>
    /// <param name="delegatedTo"></param>
    /// <returns></returns>
    public List<UserDelegationSetting> GetUserDelegationSettingsFrom(string managerAlias, int tenantId, string delegatedTo)
    {
        string query = "PartitionKey eq '" + managerAlias + "' and TenantId eq " + tenantId + " and DelegatedToAlias eq '" + delegatedTo + "'";
        return _tableHelper.GetDataCollectionByTableQuery<UserDelegationSetting>(Constants.UserDelegationSettingsAzureTableName, query);
    }

    /// <summary>
    /// Get user delegations for current user
    /// </summary>
    /// <param name="loggedInAlias"></param>
    /// <returns></returns>
    public List<UserDelegationSetting> GetUserDelegationsForCurrentUser(string loggedInAlias)
    {
        return _tableHelper.GetTableEntityListByPartitionKey<UserDelegationSetting>(Constants.UserDelegationSettingsAzureTableName, loggedInAlias.ToLowerInvariant());
    }

/// <summary>
/// Insert delegation settings
/// </summary>
/// <param name="insertData"></param>
public void InsertDelegationSettings(UserDelegationSetting insertData)
{
    var userDelegationSetting =
        new UserDelegationSetting
        {
            PartitionKey = insertData.ManagerAlias,
            RowKey = Guid.NewGuid().ToString(),
            Id = insertData.Id,
            ManagerAlias = insertData.ManagerAlias,
            DelegatedToAlias = insertData.DelegatedToAlias,
            TenantId = insertData.TenantId,
            DateFrom = insertData.DateFrom,
            DateTo = insertData.DateTo,
            AccessType = insertData.AccessType,
            IsHidden = insertData.IsHidden
            };
        _tableHelper.Insert(Constants.UserDelegationSettingsAzureTableName, userDelegationSetting);
        //insert to history
        var history = (insertData.ToJson()).FromJson<UserDelegationSettingsHistory>(); ;
        history.PartitionKey = insertData.ManagerAlias;
        history.RowKey = Guid.NewGuid().ToString(); //insertData.Id.ToString();
        history.Action = "Add";
        history.ModifiedBy = insertData.ManagerAlias; //the current loggedin user
        history.ModifiedDate = DateTime.UtcNow;
        history.Id = insertData.Id;
        history.ManagerAlias = insertData.ManagerAlias;
        history.DelegatedToAlias = insertData.DelegatedToAlias;
        history.TenantId = insertData.TenantId;
        history.DateFrom = insertData.DateFrom;
        history.DateTo = insertData.DateTo;
        history.AccessType = insertData.AccessType;
        history.IsHidden = insertData.IsHidden;
        _tableHelper.InsertOrReplace(Constants.UserDelegationSettingsHistoryAzureTableName, history);
    }

    /// <summary>
    /// Update delegation settings
    /// </summary>
    /// <param name="delegationRow"></param>
    /// <param name="oldTenantId"></param>
    /// <param name="oldDelegatedUser"></param>
    public void UpdateDelegationSettings(UserDelegationSetting delegationRow, int oldTenantId, string oldDelegatedUser)
    {
        var userDelegationSetting =
            new UserDelegationSetting
            {
                PartitionKey = delegationRow.ManagerAlias,
                RowKey = delegationRow.RowKey,
                Id = delegationRow.Id,
                ManagerAlias = delegationRow.ManagerAlias,
                DelegatedToAlias = delegationRow.DelegatedToAlias,
                TenantId = delegationRow.TenantId,
                DateFrom = delegationRow.DateFrom,
                DateTo = delegationRow.DateTo,
                AccessType = delegationRow.AccessType,
                IsHidden = delegationRow.IsHidden
            };
        _tableHelper.InsertOrReplace(Constants.UserDelegationSettingsAzureTableName, userDelegationSetting);
        //insert to history
        var history = (delegationRow.ToJson()).FromJson<UserDelegationSettingsHistory>();
        history.PartitionKey = delegationRow.ManagerAlias;
        history.RowKey = delegationRow.Id.ToString();
        history.Action = "Update";
        history.ModifiedBy = delegationRow.ManagerAlias; //the current loggedin user
        history.ModifiedDate = DateTime.UtcNow;
        history.Id = delegationRow.Id;
        history.ManagerAlias = delegationRow.ManagerAlias;
        history.DelegatedToAlias = delegationRow.DelegatedToAlias;
        history.TenantId = delegationRow.TenantId;
        history.DateFrom = delegationRow.DateFrom;
        history.DateTo = delegationRow.DateTo;
        history.AccessType = delegationRow.AccessType;
        history.IsHidden = delegationRow.IsHidden;
        _tableHelper.InsertOrReplace(Constants.UserDelegationSettingsHistoryAzureTableName, history);
    }
}