// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace Microsoft.CFS.Approvals.Model.Flighting;

public class FlightingRing: BaseTableEntity
{
    public int Id { get; set; }
    public int RingLevel { get; set; }
    public string RingDescription { get; set; }
}
