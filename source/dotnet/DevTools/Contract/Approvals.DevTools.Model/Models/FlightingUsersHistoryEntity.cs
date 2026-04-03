// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.


namespace Microsoft.CFS.Approvals.DevTools.Model.Models
{
    using System;
    using Microsoft.CFS.Approvals.Model;

    /// <summary>
    /// Flighting Users History Entity Class
    /// </summary>
    public class FlightingUsersHistoryEntity : BaseTableEntity
    {
        public int FeatureID { get; set; }
        public bool IsFeatureEnabled { get; set; }
    }
}
