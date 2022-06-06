// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace Microsoft.CFS.Approvals.Core.BL.Helpers
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.Threading.Tasks;
    using Microsoft.CFS.Approvals.Common.DL.Interface;
    using Microsoft.CFS.Approvals.Contracts;
    using Microsoft.CFS.Approvals.Contracts.DataContracts;
    using Microsoft.CFS.Approvals.Core.BL.Interface;
    using Microsoft.CFS.Approvals.LogManager.Model;
    using Microsoft.CFS.Approvals.Model;
    using Microsoft.Extensions.Configuration;

    /// <summary>
    /// The Validation Helper class
    /// </summary>
    public class ValidationHelper : IValidationHelper
    {
        private readonly IConfiguration _config = null;
        private readonly IPerformanceLogger _performanceLogger = null;
        private readonly IPayloadValidator _payloadValidator = null;

        public ValidationHelper(IConfiguration config, IPerformanceLogger performanceLogger, IPayloadValidator payloadValidator)
        {
            _config = config;
            _performanceLogger = performanceLogger;
            _payloadValidator = payloadValidator;
        }

        #region Implemented Methods

        /// <summary>
        /// Validator method which checks if the input payload (ApprovalRequestExpression) confirms with the contract definition or not
        /// </summary>
        /// <param name="approvalRequest">Input payload in ApprovalRequestExpression format</param>
        /// <param name="correlationId">The correlation id </param>
        /// <param name="tenant">The Approval Tenant Info</param>
        /// <returns>List of ValidationResults containing validation error (if any)</returns>
        public async Task<List<ValidationResult>> ValidateApprovalRequestExpression(ApprovalRequestExpression approvalRequest, string correlationId, ApprovalTenantInfo tenant)
        {
            // TODO:: Remove this validation and start using the Payload Validator
            //Modified the existing code to make sure the code is backward compatible and the older tenants do not break
            using (_performanceLogger.StartPerformanceLogger("PerfLog", Constants.WorkerRole, string.Format(Constants.PerfLogAction, tenant.AppName, "AR Validation Time"), new Dictionary<LogDataKey, object>()))
            {
                var vrs = new List<ValidationResult>();

                if (Convert.ToBoolean(_config[ConfigurationKey.EnableValidation.ToString()]))
                {
                    ValidateForNull<ApprovalRequestExpression>(approvalRequest, ValidationEntityName.ApprovalRequestExpression, vrs);

                    if (vrs.Count == 0)
                    {
                        if (approvalRequest.DocumentTypeId == Guid.Empty)
                        {
                            vrs.Add(new ValidationResult(_config[ConfigurationKey.Message_ValueEmptyGUID.ToString()], new List<string> { "ApprovalRequestExpression.DocumentTypeId" }));
                        }
                        else
                        {
                            if (!tenant.DocTypeId.Equals(approvalRequest.DocumentTypeId.ToString(), StringComparison.InvariantCultureIgnoreCase))
                            {
                                vrs.Add(new ValidationResult(_config[ConfigurationKey.Message_ValueNotExist.ToString()], new List<string> { "ApprovalRequestExpression.DocumentTypeId" }));
                            }
                        }

                        ValidateForNull<ApprovalIdentifier>(approvalRequest.ApprovalIdentifier, ValidationEntityName.ApprovalIdentifier, vrs);

                        if (approvalRequest.Operation == ApprovalRequestOperation.Update && (approvalRequest.ActionDetail?.ActionBy == null || string.IsNullOrWhiteSpace(approvalRequest.ActionDetail.ActionBy.Alias)))
                        {
                            vrs.Add(new ValidationResult(_config[ConfigurationKey.Message_ValueNullOrEmpty.ToString()], new List<string> { "ApprovalRequestExpression.ActionDetail.ActionBy" }));
                        }

                        vrs.AddRange(await _payloadValidator.ServerSideValidation(approvalRequest));
                    }
                }

                return vrs;
            }
        }

        #endregion Implemented Methods

        #region Helper methods

        /// <summary>
        /// Checks if the input element is null or not
        /// If null, this method adds the entity name in the ValidationResults
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="element">Input Element</param>
        /// <param name="validationEntityName">Name of the Element</param>
        /// <param name="vrs">Validation Results</param>
        private void ValidateForNull<T>(T element, ValidationEntityName validationEntityName, List<ValidationResult> vrs)
        {
            if (element == null)
            {
                vrs.Add(new ValidationResult(_config[ConfigurationKey.Message_ValueNullOrEmpty.ToString()], new List<string> { validationEntityName.ToString() }));
            }
        }

        #endregion Helper methods
    }
}