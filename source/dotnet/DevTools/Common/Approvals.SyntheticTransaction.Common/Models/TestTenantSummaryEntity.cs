// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace Microsoft.CFS.Approvals.SyntheticTransaction.Common.Models
{
    using Microsoft.Azure.Cosmos.Table;

    /// <summary>
    /// Test Tenant Summary Entity class
    /// </summary>
    public class TestTenantSummaryEntity : TableEntity
    {
        /// <summary>
        /// Constructor of TestTenantSummaryEntity
        /// </summary>
        public TestTenantSummaryEntity()
        {
        }

        public string JsonData { get; set; }
        public string Approver { get; set; }
        public string TenantID { get; set; }
        public bool IsActionTaken { get; set; }
    }
}