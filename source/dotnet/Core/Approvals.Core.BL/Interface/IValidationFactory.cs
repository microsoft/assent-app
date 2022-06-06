// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace Microsoft.CFS.Approvals.Common.BL.Interface
{
    using System;
    using System.Threading.Tasks;
    using Microsoft.CFS.Approvals.Contracts.DataContracts;
    using Microsoft.CFS.Approvals.Domain.BL.Interface;

    public interface IValidationFactory
    {
        /// <summary>
        /// Get tenant validation class
        /// </summary>
        /// <param name="docTypeId"></param>
        /// <returns></returns>
        Task<string> GetTenantValidationClass(Guid docTypeId);

        /// <summary>
        /// Get validations for arx
        /// </summary>
        /// <param name="arx"></param>
        /// <returns></returns>
        Task<IValidation> Validations(ApprovalRequestExpression arx);
    }
}