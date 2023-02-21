// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace Microsoft.CFS.Approvals.DevTools.Model.Models;

using System;
using Microsoft.CFS.Approvals.Model;

/// <summary>
/// Flighting Feature Entity class
/// </summary>
public class FlightingFeatureEntity : BaseTableEntity
{
    public int Id { get; set; }
    public string FeatureName { get; set; }
    public int FeatureStatusID { get; set; }
    public Nullable<System.DateTime> FeatureStartDate { get; set; }
    public string FeatureDescription { get; set; }
    public Nullable<int> FlightingRing { get; set; }
    public Nullable<System.DateTime> FeatureLastUpdate { get; set; }
}
