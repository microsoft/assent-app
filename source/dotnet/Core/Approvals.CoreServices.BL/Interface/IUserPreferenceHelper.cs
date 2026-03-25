// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace Microsoft.CFS.Approvals.CoreServices.BL.Interface;

using Microsoft.CFS.Approvals.Model;

public interface IUserPreferenceHelper
{
    /// <summary>
    /// Gets the user preferences of the logged-in user
    /// </summary>
    /// <param name="loggedInUpn">logged-in alias</param>
    /// <param name="clientDevice">Client Device</param>
    /// <returns>UserPreference object</returns>
    UserPreference GetUserPreferences(string loggedInUpn, string clientDevice);

    /// <summary>
    /// Adds or updates the user preference data
    /// </summary>
    /// <param name="userPreference">UserPreference Data</param>
    /// <param name="loggedInUpn">logged-in alias</param>
    /// <param name="clientDevice">client Device</param>
    /// <param name="sessionId">session Id</param>
    /// <returns>status for table update</returns>
    bool AddUpdateUserPreference(UserPreference userPreference, string loggedInUpn, string clientDevice, string sessionId);

    /// <summary>
    /// Adds/ Updates the specific feature in the user preference list
    /// </summary>
    /// <param name="userPreferenceData">Column Data to Update</param>
    /// <param name="userPreferenceColumn">The UserPreferenceSetting table ColumnName</param>
    /// <param name="loggedInUpn">logged-in alias</param>
    /// <param name="clientDevice">client Device</param>
    /// <param name="sessionId">session Id</param>
    /// <returns></returns>
    bool AddUpdateSpecificUserPreference(string userPreferenceData, string userPreferenceColumn, string loggedInUpn, string clientDevice, string sessionId);
}