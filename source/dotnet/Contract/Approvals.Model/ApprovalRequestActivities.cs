// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace Microsoft.CFS.Approvals.Model
{
    using System.Collections.Generic;

    /// <summary>
    /// RequestActivities Object to populate approval Activities done in tenant systems.
    /// </summary>
    public class ApprovalRequestActivities
    {
        /// <summary>
        /// Gets or sets the RequestActivities values provided by tenant systems.  Tenant can consume the properties of ApproverChainEntity.
        /// </summary>
        public List<ApproverChainEntity> Value { get; set; }

        /// <summary>
        /// Gets or sets the Name for RequestActivity row.
        /// </summary>
        public string Name { get; set; }
    }
}
