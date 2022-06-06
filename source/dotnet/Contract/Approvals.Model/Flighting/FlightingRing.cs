// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace Microsoft.CFS.Approvals.Model.Flighting
{
    using Microsoft.Azure.Cosmos.Table;
    public class FlightingRing: TableEntity
    {
        public int Id { get; set; }
        public int RingLevel { get; set; }
        public string RingDescription { get; set; }
    }
}
