// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace Microsoft.CFS.Approvals.SupportServices.Helper.ServiceHelper;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Azure;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using Microsoft.CFS.Approvals.Data.Azure.Storage.Interface;
using Microsoft.CFS.Approvals.DevTools.Model.Models;
using Microsoft.CFS.Approvals.SupportServices.Helper.ExtensionMethods;
using Microsoft.CFS.Approvals.SupportServices.Helper.Interface;

/// <summary>
/// User Delegation Helper
/// </summary>
public class UserDelegationHelper : IUserDelegationHelper
{
    /// <summary>
    /// The table storage helper
    /// </summary>
    private readonly ITableHelper _azureTableStorageHelper;

    private readonly string _environment;

    /// <summary>
    /// Constructor of UserDelegationHelper
    /// </summary>
    /// <param name="azureTableStorageHelper"></param>
    /// <param name="configurationHelper"></param>
    /// <param name="actionContextAccessor"></param>
    public UserDelegationHelper(
        Func<string, string, ITableHelper> azureTableStorageHelper,
        ConfigurationHelper configurationHelper,
        IActionContextAccessor actionContextAccessor)
    {
        _environment = actionContextAccessor?.ActionContext?.RouteData?.Values["env"]?.ToString();
        _azureTableStorageHelper = azureTableStorageHelper(
            configurationHelper.appSettings[_environment]["StorageAccountName"],
            configurationHelper.appSettings[_environment]["StorageAccountKey"]);
    }

    /// <summary>
    /// Get delegation
    /// </summary>
    /// <param name="id"></param>
    /// <returns></returns>
    public UserDelegationEntity GetDelegation(string id)
    {
        return _azureTableStorageHelper.GetTableEntityByRowKey<UserDelegationEntity>("UserDelegationSetting", id);
    }

    /// <summary>
    /// Get user delegations
    /// </summary>
    /// <returns></returns>
    public async Task<List<dynamic>> GetUserDelegations()
    {
        var results = new List<dynamic>();
        var userDelegation = _azureTableStorageHelper.GetTableEntity<UserDelegationEntity>("UserDelegationSetting");
        var activeDelegation = userDelegation.Where(d => d.DateTo >= DateTime.UtcNow.Date).ToList();
        var expiredDelegation = userDelegation.Where(d => d.DateTo < DateTime.UtcNow.Date).ToList();
        foreach (var delegation in activeDelegation)
        {
            results.Add(new
            {
                Id = delegation.Id,
                TenantId = delegation.TenantId,
                Source = delegation.IsHidden ? "Support Portal" : "Approvals",
                ManagerAlias = delegation.ManagerAlias,
                DelegationAccess = 1 == delegation.AccessType ? "ReadOnly" : "FullAccess",
                DelegatedTo = delegation.DelegatedToAlias,
                PartitionKey = delegation.PartitionKey,
                RowKey = delegation.RowKey,
                StartDate = delegation.DateFrom.ToString("yyyy/MM/dd"),
                EndDate = delegation.DateTo.ToString("yyyy/MM/dd")
            });
        }
        if (expiredDelegation.Count > 0)
        {
            foreach (var delegation in expiredDelegation)
            {
                await DeleteUserDelegations(delegation);
            }
        }

        return results;
    }

    /// <summary>
    /// Delete user delegations
    /// </summary>
    /// <param name="delegation"></param>
    public async Task DeleteUserDelegations(UserDelegationEntity delegation)
    {
        delegation.ETag = global::Azure.ETag.All;
        await _azureTableStorageHelper.DeleteRow<UserDelegationEntity>("UserDelegationSetting", delegation);
        await InsertUserDelegationHistory(PrepareDelegationHistory(delegation));
    }

    /// <summary>
    /// Prepare delegation history
    /// </summary>
    /// <param name="delegationRow"></param>
    /// <returns></returns>
    private UserDelegationSettingsHistory PrepareDelegationHistory(UserDelegationEntity delegationRow)
    {
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
        return history;
    }

    /// <summary>
    /// Insert user delegation history
    /// </summary>
    /// <param name="userDelegationSettingsHistory"></param>
    /// <returns></returns>
    public async Task<bool> InsertUserDelegationHistory(UserDelegationSettingsHistory userDelegationSettingsHistory)
    {
        return await _azureTableStorageHelper.InsertOrReplace<UserDelegationSettingsHistory>("UserDelegationSettingsHistory", userDelegationSettingsHistory);
    }

    /// <summary>
    /// Check is delegation exist
    /// </summary>
    /// <param name="DelegationFor"></param>
    /// <param name="DelegationTo"></param>
    /// <param name="tenantID"></param>
    /// <returns></returns>
    public bool IsDelegationExist(string DelegationFor, string DelegationTo, int tenantID)
    {
        var userDelegation = _azureTableStorageHelper.GetTableEntity<UserDelegationEntity>("UserDelegationSetting").Where(s => s.ManagerAlias == DelegationFor && s.DelegatedToAlias == DelegationTo && s.TenantId == tenantID);
        return userDelegation.Count() > 0;
    }

    /// <summary>
    /// Insert user delegation
    /// </summary>
    /// <param name="userDelegationEntity"></param>
    /// <returns></returns>
    public async Task<bool> InsertUserDelegation(UserDelegationEntity userDelegationEntity)
    {
        return await _azureTableStorageHelper.InsertOrReplace<UserDelegationEntity>("UserDelegationSetting", userDelegationEntity);
    }
}