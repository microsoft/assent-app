// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace Microsoft.CFS.Approvals.SyntheticTransaction.Common.Models
{
    using System;
    using Microsoft.Azure.Cosmos.Table;

    /// <summary>
    /// Summary Entity class
    /// </summary>
    public class SummaryEntity : TableEntity
    {
        /// <summary>
        /// Constructor of SummaryEntity
        /// </summary>
        public SummaryEntity()
        {
        }

        public string DocumentNumber { get; set; }
        public string SummaryJson { get; set; }
        public DateTime Timestamp { get; set; }
    }
}