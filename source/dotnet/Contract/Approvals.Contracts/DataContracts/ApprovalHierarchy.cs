// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace Microsoft.CFS.Approvals.Contracts.DataContracts
{
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;

    /// <summary>
    /// Approver Hierarchy Object - part of Approval Hierarchy List.
    /// </summary>
    public class ApprovalHierarchy
    {
        /// <summary>
        /// List of the approver(s) at the same/current level for which the approver is pending.
        /// Count of Approvers should be greater than 0. It should contain at least one approver information.
        /// </summary>
        [Required(ErrorMessage = Constants.ApproverHeirarchyNullMessage)]
        [MinLength(1, ErrorMessage = Constants.ApproverHeirarchyCountMessage)]
        public List<NameAliasEntity> Approvers { get; set; }

        /// <summary>
        /// Type of approver - Example: Interim, Safe, VP.
        /// This is an optional field
        /// </summary>
        public string ApproverType { get; set; }
    }
}