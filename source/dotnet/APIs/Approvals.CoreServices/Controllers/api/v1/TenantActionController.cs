// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace Microsoft.CFS.Approvals.CoreServices.Controllers.api.v1
{
    using System;
    using System.Threading.Tasks;
    using Microsoft.AspNetCore.Mvc;
    using Microsoft.CFS.Approvals.Contracts;
    using Microsoft.CFS.Approvals.Core.BL.Interface;
    using Microsoft.Extensions.Configuration;
    using Swashbuckle.AspNetCore.Annotations;

    /// <summary>
    /// the TenantActionController class
    /// </summary>
    /// <seealso cref="BaseApiController" />
    public class TenantActionController : BaseApiController
    {
        private readonly IApprovalTenantInfoHelper _approvalTenantInfoHelper;

        /// <summary>
        /// Initializes a new instance of the <see cref="TenantActionController"/> class.
        /// </summary>
        /// <param name="config">The configuration</param>
        /// <param name="approvalTenantInfoHelper">The approval tenant information helper.</param>
        public TenantActionController(IConfiguration config, IApprovalTenantInfoHelper approvalTenantInfoHelper)
        {
            _config = config;
            _approvalTenantInfoHelper = approvalTenantInfoHelper;
        }

        /// <summary>
        /// Gets the specified session identifier.
        /// </summary>
        /// <param name="sessionId">The session identifier.</param>
        /// <returns>return httpaction result</returns>
        [SwaggerOperation(Tags = new[] { "Metadata" })]
        [HttpGet]
        public async Task<IActionResult> Get(string sessionId = "")
        {
            try
            {
                string bulkActionConcurrentMessageFormat = _config[ConfigurationKey.BulkActionConcurrentCallMessage.ToString()];
                var tenantActionString = await _approvalTenantInfoHelper.GetBulkViewTenantActions(bulkActionConcurrentMessageFormat, LoggedInAlias, Alias, Host, sessionId);
                return Ok(tenantActionString);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }
    }
}