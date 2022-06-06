// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace Microsoft.CFS.Approvals.SyntheticTransaction.Common.Models
{
    using Microsoft.Azure.Cosmos.Table;

    /// <summary>
    /// Test Harness Document class
    /// </summary>
    public class TestHarnessDocument : TableEntity
    {
        /// <summary>
        /// Constructor of TestHarnessDocument
        /// </summary>
        public TestHarnessDocument()
        {
        }

        public string Payload { get; set; }
        public string Status { get; set; }

        public string TenantID { get; set; }
        public string NextApprover { get; set; }
    }
}