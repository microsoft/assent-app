// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace Microsoft.CFS.Approvals.Model
{
    public class TransactionHistoryExt : TransactionHistory
    {
        public string ApproverType { get; set; }
        public bool _future { get; set; }
        public bool _isPreApprover { get; set; }
        public string ApproverName { get; set; }
    }
}
