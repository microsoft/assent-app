// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace Microsoft.CFS.Approvals.Contracts.AttributeValidators
{
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using Microsoft.CFS.Approvals.Contracts.DataContracts;

    /// <summary>
    /// The Approval Identifier Validator class.
    /// </summary>
    public class ApprovalIdentifierValidator : ValidationAttribute
    {
        /// <summary>
        /// The Validation of approval identifier.
        /// </summary>
        /// <param name="approvalIdentifier"></param>
        /// <returns>List of validation result</returns>
        public List<ValidationResult> Validator(ApprovalIdentifier approvalIdentifier)
        {
            var results = new List<ValidationResult>();

            System.ComponentModel.DataAnnotations.Validator.TryValidateObject(approvalIdentifier, new ValidationContext(approvalIdentifier), results);
            return results;            
        }
    }
}
