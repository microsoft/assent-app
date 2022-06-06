// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace Microsoft.CFS.Approvals.Contracts.AttributeValidators
{
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using Microsoft.CFS.Approvals.Contracts.DataContracts;

    /// <summary>
    /// The Action Detail Validator class
    /// </summary>
    public class ActionDetailValidator : ValidationAttribute
    {
        /// <summary>
        /// Validation  of arx.
        /// </summary>
        /// <param name="arxActionDetail"></param>
        /// <returns>List of validation results</returns>
        public List<ValidationResult> Validator(ActionDetail arxActionDetail)
        {
            var results = new List<ValidationResult>();
            System.ComponentModel.DataAnnotations.Validator.TryValidateObject(arxActionDetail, new ValidationContext(arxActionDetail), results);

            if (arxActionDetail.ActionBy != null)
                results.AddRange(new NameAliasEntityValidator().Validator(arxActionDetail.ActionBy, "ApprovalRequestExpression.ActionDetail.ActionBy"));
            return results;
        }
    }
}
