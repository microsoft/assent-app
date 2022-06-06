// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace Microsoft.CFS.Approvals.Core.BL.Helpers
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.Threading.Tasks;
    using Microsoft.CFS.Approvals.Common.BL.Interface;
    using Microsoft.CFS.Approvals.Contracts.AttributeValidators;
    using Microsoft.CFS.Approvals.Contracts.DataContracts;
    using Microsoft.CFS.Approvals.Core.BL.Interface;

    /// <summary>
    /// The Payload validator class
    /// </summary>
    public class PayloadValidator : IPayloadValidator
    {
        /// <summary>
        /// The validation factory
        /// </summary>
        private readonly IValidationFactory _validationFactory = null;

        public PayloadValidator()
        {
        }

        public PayloadValidator(IValidationFactory validationFactory)
        {
            _validationFactory = validationFactory;
        }

        /// <summary>
        /// Validates the payload of type ApprovalRequestExpressionV1
        /// </summary>
        /// <param name="arx"></param>
        /// <returns></returns>
        public List<ValidationResult> Validate(ApprovalRequestExpressionV1 arx)
        {
            try
            {
                return new ApprovalRequestExpressionValidator().Validator(arx);
            }
            catch (Exception ex)
            {
                throw new ApplicationException("Payload validation encountered errors and cannot be completed. Please check details or contact Approvals Engineering Team", ex);
            }
        }

        /// <summary>
        /// Performs Additional Serverside validations
        /// </summary>
        /// <param name="arx"></param>
        /// <returns></returns>
        public async Task<List<ValidationResult>> ServerSideValidation(ApprovalRequestExpression arx)
        {
            try
            {
                var dynamicClass = await _validationFactory.Validations(arx);
                return await dynamicClass.ValidateAlias(arx);
            }
            catch (Exception ex)
            {
                throw new ApplicationException("Payload validation encountered errors and cannot be completed. Please check details or contact Approvals Engineering Team", ex);
            }
        }
    }
}