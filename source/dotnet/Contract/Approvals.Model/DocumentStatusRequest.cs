// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace Microsoft.CFS.Approvals.Model
{
    using System;

    /// <summary>
    /// DocumentStatusRequest class
    /// </summary>
    public class DocumentStatusRequest
    {
        /// <summary>
        /// Approver alias
        /// </summary>
        public string ApproverAlias { get; set; }
        /// <summary>
        /// Document number
        /// </summary>
        public string DocumentNumber { get; set; }
        /// <summary>
        /// Approval request version
        /// </summary>
        public Guid RequestVersion { get; set; }
    }
}
