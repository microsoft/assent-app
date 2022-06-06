// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace Microsoft.CFS.Approvals.DevTools.Model.Models
{
    using Microsoft.Azure.Cosmos.Table;

    /// <summary>
    /// Transaction History Entity class
    /// </summary>
    public class TransactionHistoryEntity:TableEntity
    {
        /// <summary>
        /// Constructor of TransactionHistoryEntity
        /// </summary>
        public TransactionHistoryEntity()
        {
                
        }
        public string AppName { get; set; }
        public string Approver { get; set; }
        public string SubmitterName { get; set; }
        public string DocumentNumber { get; set; }
        public string ActionTaken { get; set; }
    }
}
