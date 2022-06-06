// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace Microsoft.CFS.Approvals.Model
{
    using System;

    /// <summary>
    /// Document status response class
    /// </summary>
    public class DocumentStatusResponse
    {
        /// <summary>
        /// optional datetime in UTC timezone
        /// </summary>
        public DateTime? ActionDate { get; set; }
        /// <summary>
        /// Current Action Status for Request
        /// </summary>
        public string CurrentStatus { get; set; }
        /// <summary>
        /// Action taken on which client like Outlook/Web
        /// </summary>
        public string ActionTakenOnClient { get; set; }
        /// <summary>
        /// Amount value
        /// </summary>
        public string UnitValue { get; set; }
        /// <summary>
        /// Amount Unit
        /// </summary>
        public string UnitOfMeasure { get; set; }
        /// <summary>
        /// Submitter Alias
        /// </summary>
        public string SubmitterAlias { get; set; }
        /// <summary>
        /// Submitted date in UTC timezone
        /// </summary>
        public DateTime SubmittedDate { get; set; }
        /// <summary>
        /// Submitter name
        /// </summary>
        public string SubmitterName { get; set; }
        /// <summary>
        /// Tenant name
        /// </summary>
        public string TenantName { get; set; }
        /// <summary>
        /// Approver name
        /// </summary>
        public string ApproverName { get; set; }
        /// <summary>
        /// Approver alias
        /// </summary>
        public string ApproverAlias { get; set; }
    }
}
