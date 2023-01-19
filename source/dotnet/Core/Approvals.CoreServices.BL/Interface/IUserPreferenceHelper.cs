// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace Microsoft.CFS.Approvals.CoreServices.BL.Interface;

using Microsoft.CFS.Approvals.Model;

public interface IUserPreferenceHelper
{
    /// <summary>
    /// Gets the user preferences of the logged-in user
    /// </summary>
    /// <param name="loggedInAlias">logged-in alias</param>
    /// <param name="clientDevice">Client Device</param>
    /// <returns>UserPreference object</returns>
    UserPreference GetUserPreferences(string loggedInAlias, string clientDevice);

    /// <summary>
    /// Adds or updates the user preference data
    /// </summary>
    /// <param name="loggedInAlias">logged-in alias</param>
    /// <param name="clientDevice">Client Device</param>
    /// <returns>status for table update</returns>
    bool AddUpdateUserPreference(UserPreference userPreference, string loggedInAlias, string clientDevice);
}