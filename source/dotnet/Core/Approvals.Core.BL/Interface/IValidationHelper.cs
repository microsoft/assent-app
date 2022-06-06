// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace Microsoft.CFS.Approvals.Core.BL.Interface
{
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.Threading.Tasks;
    using Microsoft.CFS.Approvals.Contracts.DataContracts;
    using Microsoft.CFS.Approvals.Model;

    public interface IValidationHelper
    {
        /// <summary>
        /// Validator method which checks if the input payload (ApprovalRequestExpression) confirms with the contract definition or not
        /// </summary>
        /// <param name="approvalRequest">Input payload in ApprovalRequestExpression format</param>
        /// <param name="correlationId">The correlation id </param>
        /// <param name="tenant">The Approval Tenant Info</param>
        /// <returns>List of ValidationResults containing validation error (if any)</returns>
        Task<List<ValidationResult>> ValidateApprovalRequestExpression(ApprovalRequestExpression approvalRequest, string correlationId, ApprovalTenantInfo tenant);
    }
}