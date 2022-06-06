// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace Microsoft.CFS.Approvals.Model
{
    using System.Collections.Generic;

    /// <summary>
    /// The AskQuestion Model
    /// </summary>
    public class AskQuestionsData
    {
        /// <summary>
        /// Gets or sets the total pending request.
        /// </summary>
        /// <value>
        /// The total pending request.
        /// </value>
        public int TotalPendingRequest { get; set; }

        /// <summary>
        /// Gets or sets the approval summary list.
        /// </summary>
        /// <value>
        /// The approval summary list.
        /// </value>
        public IEnumerable<ApprovalSummaryData> ApprovalSummaryList { get; set; }

        /// <summary>
        /// Gets or sets the ask questions item list.
        /// </summary>
        /// <value>
        /// The ask questions item list.
        /// </value>
        public IEnumerable<AskQuestionTenants> AskQuestionsItemList { get; set; }
    }
}
