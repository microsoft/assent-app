// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace Microsoft.CFS.Approvals.Model
{
    using Microsoft.Azure.Cosmos.Table;

    /// <summary>
    /// User Preference model class.
    /// </summary>
    public class UserPreference : TableEntity
    {
        public string FeaturePreferenceJson { get; set; }
        public string ReadNotificationsList { get; set; }
        public string PriorityPreferenceJson { get; set; }
        public string ClientDevice { get; set; }
    }
}