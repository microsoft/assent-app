// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace Microsoft.CFS.Approvals.SyntheticTransaction.Common.Models
{
    using Microsoft.Azure.Cosmos.Table;

    /// <summary>
    /// Synthetic Transaction Entity class
    /// </summary>
    public class SyntheticTransactionEntity : TableEntity
    {
        /// <summary>
        /// Constructor of SyntheticTransactionEntity
        /// </summary>
        public SyntheticTransactionEntity()
        {
        }

        public string JsonData { get; set; }
        public string Approver { get; set; }
        public string AppName { get; set; }
    }
}