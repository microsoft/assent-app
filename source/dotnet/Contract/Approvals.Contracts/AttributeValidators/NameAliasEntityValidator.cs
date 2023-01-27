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
    public class NameAliasEntityValidator : ValidationAttribute
    {
        /// <summary>
        /// The Validation of Name Alias Entity
        /// </summary>
        /// <param name="nameAliasEntity"></param>
        /// <param name="baseProperty"></param>
        /// <returns>List of validation results.</returns>
        public List<ValidationResult> Validator(NameAliasEntity nameAliasEntity, string baseProperty)
        {
            var results = new List<ValidationResult>();
            System.ComponentModel.DataAnnotations.Validator.TryValidateObject(nameAliasEntity, new ValidationContext(nameAliasEntity), results);

            return results;
        }
    }
}