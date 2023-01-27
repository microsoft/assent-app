// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace Microsoft.CFS.Approvals.Model.Flighting;

using System;

public partial class Flighting : BaseTableEntity
{
    public int Id { get; set; }
    public string Alias { get; set; }
    public int FeatureID { get; set; }
    public Nullable<System.DateTime> FlightingStartDate { get; set; }

    public virtual FlightingFeature FlightingFeature { get; set; }
}