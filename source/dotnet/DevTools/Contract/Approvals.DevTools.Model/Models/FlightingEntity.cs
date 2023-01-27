// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.


namespace Microsoft.CFS.Approvals.DevTools.Model.Models;

using System;
using Microsoft.CFS.Approvals.Model;

/// <summary>
/// Flighting Entity class
/// </summary>
public class FlightingEntity : BaseTableEntity
{
    public int Id { get; set; }
    public string Alias { get; set; }
    public int FeatureID { get; set; }
    public Nullable<System.DateTime> FlightingStartDate { get; set; }
}
