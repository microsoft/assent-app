// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace Microsoft.CFS.Approvals.Contracts.AttributeValidators
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using Microsoft.CFS.Approvals.Contracts.DataContracts;

    /// <summary>
    /// The Summary Json Validator class.
    /// </summary>
    public class SummaryJsonValidator : ValidationAttribute
    {
        /// <summary>
        /// The Validation of Summary Json.
        /// </summary>
        /// <param name="summaryJson"></param>
        /// <param name="documentTypeId"></param>
        /// <returns>List of validation result</returns>
        public List<ValidationResult> Validator(SummaryJson summaryJson, Guid documentTypeId)
        {
            var results = new List<ValidationResult>();

            System.ComponentModel.DataAnnotations.Validator.TryValidateObject(summaryJson, new ValidationContext(summaryJson), results);
            
            if (!String.IsNullOrEmpty(summaryJson.DocumentTypeId))
            {
                if (!summaryJson.DocumentTypeId.Equals(documentTypeId.ToString(), StringComparison.InvariantCultureIgnoreCase))
                {
                    results.Add(new ValidationResult(Constants.ARXandSummaryTenantIdMatchMessage, new List<string> { "ApprovalRequestExpression.SummaryJson.DocumentTypeId", "ApprovalRequestExpression.DocumentTypeId" }));
                }
            }

            if (summaryJson.ApprovalIdentifier != null)
                results.AddRange(new ApprovalIdentifierValidator().Validator(summaryJson.ApprovalIdentifier));
            if (summaryJson.Submitter != null)
                results.AddRange(new NameAliasEntityValidator().Validator(summaryJson.Submitter, "ApprovalRequestExpression.SummaryJson.Submitter"));
            if (summaryJson.CustomAttribute != null)
                results.AddRange(new CustomAttributeValidator().Validator(summaryJson.CustomAttribute));
            if (summaryJson.ApprovalHierarchy != null)
                results.AddRange(new ApprovalHierarchyValidator().Validator(summaryJson.ApprovalHierarchy));

            return results;
        }
    }
}
