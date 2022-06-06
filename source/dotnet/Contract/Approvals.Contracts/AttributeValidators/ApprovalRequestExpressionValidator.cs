// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace Microsoft.CFS.Approvals.Contracts.AttributeValidators
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using Microsoft.CFS.Approvals.Contracts.DataContracts;

    /// <summary>
    /// The Approval Request Expression Validator class
    /// </summary>
    public class ApprovalRequestExpressionValidator : ValidationAttribute
    {
        /// <summary>
        /// The Validation of ARXV1
        /// </summary>
        /// <param name="arx"></param>
        /// <returns>List of validation result</returns>
        public List<ValidationResult> Validator(ApprovalRequestExpressionV1 arx)
        {
            var results = new List<ValidationResult>();

            if (arx == null)
            {
                ValidationResult validationResult = new ValidationResult(Constants.ARXisNullMessage);
                results.Add(validationResult);
                return results;
            }

            System.ComponentModel.DataAnnotations.Validator.TryValidateObject(arx, new ValidationContext(arx), results);

            //Check if the document type id is blank GUID "00000000-0000-00..."
            if (arx.DocumentTypeId == Guid.Empty)
            {
                results.Add(new ValidationResult(Constants.DocTypeIdEmpty, new List<string> { "ApprovalRequestExpression.DocumentTypeId" }));
            }

            // TODO: HARDIK: Add validation for verifying the Document Type id against ApprovalTenantInfo table
            //if (arx.Operation == null)
            //{
            //    ValidationResult validationResult = new ValidationResult(Constants.ActionDetailNullMessage);
            //    results.Add(validationResult);
            //    return results;
            //}

            if (arx.Operation == ApprovalRequestOperation.Delete || arx.Operation == ApprovalRequestOperation.Update)
            {
                if (arx.DeleteFor == null || arx.DeleteFor.Count == 0)
                {
                    results.Add(new ValidationResult(Constants.DeleteForNullMessage, new List<string> { "ApprovalRequestExpression.DeleteFor" }));
                }
            
                if (arx.ActionDetail == null)
                {
                    results.Add(new ValidationResult(Constants.ActionDetailNullMessage, new List<string> { "ApprovalRequestExpression.ActionDetail" }));
                }
            }

            if (arx.Operation == ApprovalRequestOperation.Create || arx.Operation == ApprovalRequestOperation.Update)
            {
                if (arx.Approvers == null || arx.Approvers.Count == 0)
                {
                    results.Add(new ValidationResult(Constants.ARXApproversNullMessage, new List<string> { "ApprovalRequestExpression.Approvers" }));
                }
            
                if (arx.SummaryData == null)
                {
                    results.Add(new ValidationResult(Constants.SummaryJsonNullMessage, new List<string> { "ApprovalRequestExpression.SummaryData" }));
                }
            }
            
            if (arx.ApprovalIdentifier != null)
                results.AddRange(new ApprovalIdentifierValidator().Validator(arx.ApprovalIdentifier));
            if (arx.Approvers != null)
                results.AddRange(new ApproversValidator().Validator(arx.Approvers));
            if (arx.ActionDetail != null)
                results.AddRange(new ActionDetailValidator().Validator(arx.ActionDetail));
            if (arx.NotificationDetail != null)
                results.AddRange(new NotificationDetailsValidator().Validator(arx.NotificationDetail));
            if (arx.SummaryData != null)
                results.AddRange(new SummaryJsonValidator().Validator(arx.SummaryData, arx.DocumentTypeId));

            return results;
        }
    }
}
