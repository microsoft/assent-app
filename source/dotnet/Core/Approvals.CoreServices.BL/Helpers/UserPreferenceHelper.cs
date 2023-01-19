// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace Microsoft.CFS.Approvals.CoreServices.BL.Helpers;

using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.CFS.Approvals.Contracts;
using Microsoft.CFS.Approvals.CoreServices.BL.Interface;
using Microsoft.CFS.Approvals.Data.Azure.Storage.Interface;
using Microsoft.CFS.Approvals.Model;
using Microsoft.Extensions.Configuration;

/// <summary>
/// User Preference Helper class.
/// </summary>
public class UserPreferenceHelper : IUserPreferenceHelper
{
    /// <summary>
    /// Configuration helper
    /// </summary>
    private readonly IConfiguration _config;

    /// <summary>
    /// Table storage helper.
    /// </summary>
    private readonly ITableHelper _tableHelper;

    /// <summary>
    /// Constructor for UserPreferenceHelper
    /// </summary>
    /// <param name="tableHelper"></param>
    /// <param name="config"></param>
    public UserPreferenceHelper(
        ITableHelper tableHelper,
        IConfiguration config)
    {
        _tableHelper = tableHelper;
        _config = config;
    }

    /// <summary>
    /// Gets the user preferences of the logged-in user
    /// </summary>
    /// <param name="loggedInAlias">logged-in alias</param>
    /// <param name="clientDevice">Client Device</param>
    /// <returns>UserPreference object</returns>
    public UserPreference GetUserPreferences(string loggedInAlias, string clientDevice)
    {
        var tableName = _config[ConfigurationKey.UserPreferenceAzureTableName.ToString()];
        List<UserPreference> userPreferences = _tableHelper.GetTableEntityListByPartitionKey<UserPreference>(tableName, loggedInAlias).ToList();
        UserPreference userPreferenceForClient = null;
        if (userPreferences != null && userPreferences.Count > 0)
        {
            if (clientDevice.Equals(Constants.WebClient))
                userPreferenceForClient = userPreferences.Where(u => string.IsNullOrEmpty(u.ClientDevice) || u.ClientDevice.Equals(clientDevice, StringComparison.InvariantCultureIgnoreCase)).FirstOrDefault();
            else
                userPreferenceForClient = userPreferences.Where(u => !string.IsNullOrEmpty(u.ClientDevice) && u.ClientDevice.Equals(clientDevice, StringComparison.InvariantCultureIgnoreCase)).FirstOrDefault();
        }
        return userPreferenceForClient;
    }

    /// <summary>
    /// Adds or updates the user preference data
    /// </summary>
    /// <param name="userPreference">user preference object for data passing like readnotifiations, featurepreferencejson</param>
    /// <param name="loggedInAlias">logged-in alias</param>
    /// <param name="clientDevice">Client Device</param>
    /// <returns>status for table update</returns>
    public bool AddUpdateUserPreference(UserPreference userPreference, string loggedInAlias, string clientDevice)
    {
        var tableName = _config[ConfigurationKey.UserPreferenceAzureTableName.ToString()];
        try
        {
            string readNotificationList = userPreference.ReadNotificationsList;
            string featurePreferenceJson = userPreference.FeaturePreferenceJson;
            string priorityPreferenceJson = userPreference.PriorityPreferenceJson;
            UserPreference userPreferenceObj = GetUserPreferences(loggedInAlias, clientDevice);
            if (userPreferenceObj == null)
            {
                userPreferenceObj = new UserPreference
                {
                    PartitionKey = loggedInAlias,
                    RowKey = Guid.NewGuid().ToString(),
                    ClientDevice = clientDevice
                };
            }
            if (!string.IsNullOrEmpty(readNotificationList))
            {
                userPreferenceObj.ReadNotificationsList = readNotificationList;
            }
            if (!string.IsNullOrEmpty(featurePreferenceJson))
            {
                userPreferenceObj.FeaturePreferenceJson = featurePreferenceJson;
            }
            if (!string.IsNullOrWhiteSpace(priorityPreferenceJson))
            {
                userPreferenceObj.PriorityPreferenceJson = priorityPreferenceJson;
            }
            userPreferenceObj.ClientDevice = clientDevice;
            _tableHelper.InsertOrReplace<UserPreference>(tableName, userPreferenceObj);
            return true;
        }
        catch
        {
            return false;
        }
    }
}