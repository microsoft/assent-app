// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace Microsoft.CFS.Approvals.Contracts.AttributeValidators
{
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using Microsoft.CFS.Approvals.Contracts.DataContracts;

    /// <summary>
    /// The Approvers Validator class.
    /// </summary>
    public class ApproversValidator : ValidationAttribute
    {
        /// <summary>
        /// The Validation of list of approvers.
        /// </summary>
        /// <param name="approvers"></param>
        /// <returns>List of validation results</returns>
        public List<ValidationResult> Validator(List<Approver> approvers)
        {
            var results = new List<ValidationResult>();
            var resultsTemp = new List<ValidationResult>();

            if (approvers.Count == 0)
                results.Add(new ValidationResult(Constants.ARXApproversCountZeroMessage, new List<string> { "ApprovalRequestExpression.Approvers" }));

            foreach (Approver approver in approvers)
            {
                System.ComponentModel.DataAnnotations.Validator.TryValidateObject(approver, new ValidationContext(approver), resultsTemp);
                if (approver.Alias != null && (approver.Alias.Contains("^") || approver.Alias.Contains("/") || approver.Alias.Contains("\\") || approver.Alias.Contains("#") || approver.Alias.Contains("?")))
                    results.Add(new ValidationResult(Constants.NameAliasEntityAliasInvalidCharsMessage, new List<string> { "ApprovalRequestExpression.Approvers.Alias" }));
            }
            results.AddRange(resultsTemp);
            return results;
        }
    }
}