// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace Microsoft.CFS.Approvals.Core.BL.Factory
{
    using System;
    using System.Linq;
    using System.Threading.Tasks;
    using Microsoft.CFS.Approvals.Common.BL.Interface;
    using Microsoft.CFS.Approvals.Contracts.DataContracts;
    using Microsoft.CFS.Approvals.Core.BL.Interface;
    using Microsoft.CFS.Approvals.Domain.BL.Interface;
    using Microsoft.CFS.Approvals.Domain.BL.Tenants.Validations;
    using Microsoft.CFS.Approvals.Utilities.Interface;
    using Microsoft.Extensions.Configuration;

    /// <summary>
    /// The Validation Factory class
    /// </summary>
    public class ValidationFactory : IValidationFactory
    {
        /// <summary>
        /// The configuration helper
        /// </summary>
        private readonly IConfiguration _config = null;

        /// <summary>
        /// The name resolution helper
        /// </summary>
        private readonly INameResolutionHelper _nameResolutionHelper = null;

        /// <summary>
        /// The approval tenantInfo helper
        /// </summary>
        private readonly IApprovalTenantInfoHelper _approvalTenantInfoHelper = null;

        /// <summary>
        /// Constructor of ValidationFactory
        /// </summary>
        /// <param name="config"></param>
        /// <param name="nameResolutionHelper"></param>
        /// <param name="approvalTenantInfoHelper"></param>
        public ValidationFactory(IConfiguration config, INameResolutionHelper nameResolutionHelper, IApprovalTenantInfoHelper approvalTenantInfoHelper)
        {
            _config = config;
            _nameResolutionHelper = nameResolutionHelper;
            _approvalTenantInfoHelper = approvalTenantInfoHelper;
        }

        /// <summary>
        /// Get tenant validation class
        /// </summary>
        /// <param name="docTypeId"></param>
        /// <returns></returns>
        public async Task<string> GetTenantValidationClass(Guid docTypeId)
        {
            return (await _approvalTenantInfoHelper.GetTenants(false)).FirstOrDefault(t => Guid.Parse(t.DocTypeId) == docTypeId).ValidationClassName;
        }

        /// <summary>
        /// Perform validations on ARX
        /// </summary>
        /// <param name="arx"></param>
        /// <returns></returns>
        public async Task<IValidation> Validations(ApprovalRequestExpression arx)
        {
            return (ValidationBase)Activator.CreateInstance(Type.GetType(await GetTenantValidationClass(arx.DocumentTypeId)), _config, _nameResolutionHelper);
        }
    }
}