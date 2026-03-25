// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace Microsoft.CFS.Approvals.Contracts.AttributeValidators
{
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using Microsoft.CFS.Approvals.Contracts.DataContracts;

    /// <summary>
    /// The Name Alias Entity validator class.
    /// </summary>
    public class UserValidator : ValidationAttribute
    {
        /// <summary>
        /// The Validation of Name Alias Entity
        /// </summary>
        /// <param name="userEntity"></param>
        /// <param name="baseProperty"></param>
        /// <returns>List of validation results.</returns>
        public List<ValidationResult> Validator(User userEntity, string baseProperty)
        {
            var results = new List<ValidationResult>();
            System.ComponentModel.DataAnnotations.Validator.TryValidateObject(userEntity, new ValidationContext(userEntity), results);

            return results;
        }
    }
}