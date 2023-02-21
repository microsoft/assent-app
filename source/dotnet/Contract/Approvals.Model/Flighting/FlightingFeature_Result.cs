// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace Microsoft.CFS.Approvals.Model.Flighting;

using System;

public class FlightingFeature_Result
{
    public int id { get; set; }
    public string FeatureName { get; set; }
    public int FeatureStatusID { get; set; }
    public Nullable<System.DateTime> FeatureStartDate { get; set; }
    public string FeatureDescription { get; set; }
    public Nullable<int> FlightingRing { get; set; }
    public Nullable<System.DateTime> FeatureLastUpdate { get; set; }
    public Nullable<decimal> FeatureVersion { get; set; }
    public int IsSubscribed { get; set; }
    public int EnableButton { get; set; }
}
