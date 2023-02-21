// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace Microsoft.CFS.Approvals.Model;

/// <summary>
/// User Preference model class.
/// </summary>
public class UserPreference : BaseTableEntity
{
    public string FeaturePreferenceJson { get; set; }
    public string ReadNotificationsList { get; set; }
    public string PriorityPreferenceJson { get; set; }
    public string ClientDevice { get; set; }
}