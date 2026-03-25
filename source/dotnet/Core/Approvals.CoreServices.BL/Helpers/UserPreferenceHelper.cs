// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace Microsoft.CFS.Approvals.CoreServices.BL.Helpers;

using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.CFS.Approvals.Contracts;
using Microsoft.CFS.Approvals.CoreServices.BL.Interface;
using Microsoft.CFS.Approvals.Data.Azure.Storage.Interface;
using Microsoft.CFS.Approvals.Extensions;
using Microsoft.CFS.Approvals.LogManager.Model;
using Microsoft.CFS.Approvals.LogManager.Provider.Interface;
using Microsoft.CFS.Approvals.Model;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json.Linq;

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
    /// Log Provider
    /// </summary>
    private readonly ILogProvider _logProvider;

    /// <summary>
    /// Constructor for UserPreferenceHelper
    /// </summary>
    /// <param name="tableHelper"></param>
    /// <param name="config"></param>
    /// <param name="logProvider"></param>
    public UserPreferenceHelper(
        ITableHelper tableHelper,
        IConfiguration config,
        ILogProvider logProvider)
    {
        _tableHelper = tableHelper;
        _config = config;
        _logProvider = logProvider;
    }

    /// <summary>
    /// Gets the user preferences of the logged-in user
    /// </summary>
    /// <param name="loggedInUpn">logged-in alias</param>
    /// <param name="clientDevice">Client Device</param>
    /// <returns>UserPreference object</returns>
    public UserPreference GetUserPreferences(string loggedInUpn, string clientDevice)
    {
        var tableName = _config[ConfigurationKey.UserPreferenceAzureTableName.ToString()];
        List<UserPreference> userPreferences = _tableHelper.GetTableEntityListByPartitionKey<UserPreference>(tableName, loggedInUpn).ToList();
        if(_config[Constants.OldWhitelistedDomains].Contains(loggedInUpn.GetDomainFromUPN(), StringComparison.InvariantCultureIgnoreCase))
        {
            if (userPreferences == null)
                userPreferences = _tableHelper.GetTableEntityListByPartitionKey<UserPreference>(tableName, loggedInUpn.GetAliasFromUPN()).ToList();
            else
                userPreferences.AddRange(_tableHelper.GetTableEntityListByPartitionKey<UserPreference>(tableName, loggedInUpn.GetAliasFromUPN()).ToList());
        }
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
    /// <param name="loggedInUpn">logged-in alias</param>
    /// <param name="clientDevice">Client Device</param>
    /// <param name="sessionId">session Id</param>
    /// <returns>status for table update</returns>
    public bool AddUpdateUserPreference(UserPreference userPreference, string loggedInUpn, string clientDevice, string sessionId)
    {
        #region Logging

        var logData = new Dictionary<LogDataKey, object>
        {
            { LogDataKey.Tcv, sessionId },
            { LogDataKey.StartDateTime, DateTime.UtcNow },
            { LogDataKey.SessionId, sessionId },
            { LogDataKey.UserAlias, loggedInUpn},
            { LogDataKey.ClientDevice, clientDevice},
            { LogDataKey.IsCriticalEvent, CriticalityLevel.No.ToString() }
        };

        #endregion Logging

        var tableName = _config[ConfigurationKey.UserPreferenceAzureTableName.ToString()];
        try
        {
            string readNotificationList = userPreference.ReadNotificationsList;
            string quickTourFeatureList = userPreference.QuickTourFeatureList;
            string featurePreferenceJson = userPreference.FeaturePreferenceJson;
            string priorityPreferenceJson = userPreference.PriorityPreferenceJson;
            UserPreference userPreferenceObj = GetUserPreferences(loggedInUpn, clientDevice);
            if (userPreferenceObj != null)
            {
                _tableHelper.DeleteRow(tableName, userPreferenceObj).Wait();
            }
            else
            {
                userPreferenceObj = new UserPreference
                {
                    PartitionKey = loggedInUpn,
                    RowKey = Guid.NewGuid().ToString(),
                    ClientDevice = clientDevice
                };
            }
            if (!string.IsNullOrEmpty(readNotificationList))
            {
                userPreferenceObj.ReadNotificationsList = readNotificationList;
            }
            if (!string.IsNullOrEmpty(quickTourFeatureList))
            {
                userPreferenceObj.QuickTourFeatureList = quickTourFeatureList;
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

            logData.Modify(LogDataKey.EndDateTime, DateTime.UtcNow);
            _logProvider.LogInformation(TrackingEvent.AddUpdateUserPreferenceSuccess, logData);
            return true;
        }
        catch (Exception ex)
        {
            logData.Modify(LogDataKey.EndDateTime, DateTime.UtcNow);
            _logProvider.LogError(TrackingEvent.AddUpdateUserPreferenceFailure, ex, logData);
            return false;
        }
    }

    /// <summary>
    /// Adds/ Updates the specific feature in the user preference list
    /// </summary>
    /// <param name="userPreferenceData">Column Data to update</param>
    /// <param name="userPreferenceColumn">The UserPreferenceSetting table ColumnName</param>
    /// <param name="loggedInUpn">logged-in alias</param>
    /// <param name="clientDevice">client Device</param>
    /// <param name="sessionId">session Id</param>
    /// <returns></returns>
    public bool AddUpdateSpecificUserPreference(string userPreferenceData, string userPreferenceColumn, string loggedInUpn, string clientDevice, string sessionId)
    {
        #region Logging

        var logData = new Dictionary<LogDataKey, object>
        {
            { LogDataKey.Tcv, sessionId },
            { LogDataKey.StartDateTime, DateTime.UtcNow },
            { LogDataKey.SessionId, sessionId },
            { LogDataKey.UserAlias, loggedInUpn},
            { LogDataKey.ClientDevice, clientDevice},
            { LogDataKey.IsCriticalEvent, CriticalityLevel.No.ToString() },
            { LogDataKey.UserPreferenceType, userPreferenceColumn}
        };

        #endregion Logging

        var tableName = _config[ConfigurationKey.UserPreferenceAzureTableName.ToString()];
        try
        {
            UserPreference userPreferenceObj = GetUserPreferences(loggedInUpn, clientDevice);
            if (userPreferenceObj != null)
            {
                _tableHelper.DeleteRow(tableName, userPreferenceObj).Wait();
            }
            userPreferenceObj = new UserPreference
            {
                PartitionKey = loggedInUpn,
                RowKey = Guid.NewGuid().ToString(),
                ClientDevice = clientDevice
            };
            List<string> preferenceList = new List<string>();
            object obj;
            UserPreferenceType preferenceType = Enum.TryParse(typeof(UserPreferenceType), userPreferenceColumn, true, out obj) 
                                                ? (UserPreferenceType)obj 
                                                : UserPreferenceType.None;
            switch (preferenceType)
            {
                case UserPreferenceType.FeaturePreferenceJson:
                    JObject columnDataJObj = userPreferenceData.FromJson<JObject>();
                    List<JObject> featurePreferenceList = new List<JObject>();
                    if (string.IsNullOrWhiteSpace(userPreferenceObj.FeaturePreferenceJson))
                        featurePreferenceList = new List<JObject>() { columnDataJObj };
                    else
                    {
                        featurePreferenceList = userPreferenceObj.FeaturePreferenceJson.FromJson<List<JObject>>();
                        featurePreferenceList.Add(columnDataJObj);
                    }
                    userPreferenceObj.FeaturePreferenceJson = featurePreferenceList.ToJson();
                    break;
                case UserPreferenceType.QuickTourFeatureList:
                    if (string.IsNullOrWhiteSpace(userPreferenceObj.QuickTourFeatureList))
                        preferenceList = new List<string>() { userPreferenceData };
                    else
                    {
                        preferenceList = userPreferenceObj.QuickTourFeatureList.FromJson<List<string>>();
                        preferenceList.Add(userPreferenceData);
                    }
                    userPreferenceObj.QuickTourFeatureList = preferenceList.ToJson();
                    break;
                case UserPreferenceType.ReadNotificationsList:
                    if (string.IsNullOrWhiteSpace(userPreferenceObj.ReadNotificationsList))
                        preferenceList = new List<string>() { userPreferenceData };
                    else
                    {
                        preferenceList = userPreferenceObj.ReadNotificationsList.FromJson<List<string>>();
                        preferenceList.Add(userPreferenceData);
                    }
                    userPreferenceObj.ReadNotificationsList = preferenceList.ToJson();
                    break;
                default: 
                    break;
            }

            userPreferenceObj.ClientDevice = clientDevice;

            _tableHelper.InsertOrReplace<UserPreference>(tableName, userPreferenceObj);

            logData.Modify(LogDataKey.EndDateTime, DateTime.UtcNow);
            _logProvider.LogInformation(TrackingEvent.UpdateSpecificUserPreferenceSuccess, logData);

            return true;
        }
        catch(Exception ex)
        {
            _logProvider.LogError(TrackingEvent.UpdateSpecificUserPreferenceFailure, ex, logData);
            return false;
        }
    }
}