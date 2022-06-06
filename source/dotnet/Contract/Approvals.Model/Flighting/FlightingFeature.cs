// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace Microsoft.CFS.Approvals.Model.Flighting
{
    using System;
    using System.Collections.Generic;
    using Microsoft.Azure.Cosmos.Table;

    public partial class FlightingFeature : TableEntity
    {
        public FlightingFeature()
        {
            Flightings = new HashSet<Flighting>();
        }

        public int Id { get; set; }
        public string FeatureName { get; set; }
        public int FeatureStatusID { get; set; }
        public Nullable<DateTime> FeatureStartDate { get; set; }
        public string FeatureDescription { get; set; }
        public Nullable<int> FlightingRing { get; set; }
        public Nullable<DateTime> FeatureLastUpdate { get; set; }
        public Nullable<decimal> FeatureVersion { get; set; }
        public virtual ICollection<Flighting> Flightings { get; set; }
    }
}