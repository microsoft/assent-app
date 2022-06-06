// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace Microsoft.CFS.Approvals.Contracts.AttributeValidators
{
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using Microsoft.CFS.Approvals.Contracts.DataContracts;

    /// <summary>
    /// The Custom Attribute validator class
    /// </summary>
    public class CustomAttributeValidator : ValidationAttribute
    {
        /// <summary>
        /// The Validation of custom attribute.
        /// </summary>
        /// <param name="customAttribute"></param>
        /// <returns>List of validation results.</returns>
        public List<ValidationResult> Validator(CustomAttribute customAttribute)
        {
            var results = new List<ValidationResult>();
            System.ComponentModel.DataAnnotations.Validator.TryValidateObject(customAttribute, new ValidationContext(customAttribute), results);
            return results;
        }
    }
}
