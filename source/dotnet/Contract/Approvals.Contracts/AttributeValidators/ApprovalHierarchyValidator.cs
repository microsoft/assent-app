// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace Microsoft.CFS.Approvals.Contracts.AttributeValidators
{
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using Microsoft.CFS.Approvals.Contracts.DataContracts;

    /// <summary>
    /// The Approval Hierarchy Validator class.
    /// </summary>
    public class ApprovalHierarchyValidator : ValidationAttribute
    {
        /// <summary>
        /// The Validation of approval hierarchy.
        /// </summary>
        /// <param name="approvalHierarchy"></param>
        /// <returns>List of validation results</returns>
        public List<ValidationResult> Validator(List<ApprovalHierarchy> approvalHierarchy)
        {
            var results = new List<ValidationResult>();
            var resultsTemp = new List<ValidationResult>();

            if (approvalHierarchy.Count == 0)
            {
                results.Add(new ValidationResult(Constants.ApprovalHierarchyZeroCount, new List<string> { "ApprovalRequestExpression.SummaryJson.ApprovalHierarchy" }));
            }
            foreach (ApprovalHierarchy apprHierarchy in approvalHierarchy)
            {
                System.ComponentModel.DataAnnotations.Validator.TryValidateObject(apprHierarchy, new ValidationContext(apprHierarchy), resultsTemp);
                if (apprHierarchy.Approvers != null && apprHierarchy.Approvers.Count == 0)
                {
                    results.Add(new ValidationResult(Constants.ApproverHeirarchyCountMessage, new List<string> { "ApprovalRequestExpression.SummaryJson.ApprovalHierarchy.Approvers" }));
                }
                if (apprHierarchy.Approvers != null && apprHierarchy.Approvers.Count > 0)
                {
                    foreach (NameAliasEntity approver in apprHierarchy.Approvers)
                    {
                        results.AddRange(new NameAliasEntityValidator().Validator(approver, "ApprovalRequestExpression.SummaryJson.ApprovalHierarchy.Approvers"));
                    }
                }
            }
            results.AddRange(resultsTemp);
            return results;
        }
    }
}
