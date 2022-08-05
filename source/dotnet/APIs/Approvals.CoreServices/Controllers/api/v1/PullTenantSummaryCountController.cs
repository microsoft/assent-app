// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace Microsoft.CFS.Approvals.CoreServices.Controllers.api.v1
{
    using System;
    using System.Threading.Tasks;
    using Microsoft.AspNetCore.Mvc;
    using Microsoft.CFS.Approvals.Core.BL.Interface;
    using Swashbuckle.AspNetCore.Annotations;

    /// <summary>
    /// the PullTenantSummaryCountController class.
    /// </summary>
    /// <seealso cref="BaseApiController" />
    public class PullTenantSummaryCountController : BaseApiController
    {
        /// <summary>
        /// The pull tenant helper.
        /// </summary>
        private readonly IPullTenantHelper _pullTenantHelper = null;

        /// <summary>
        /// Initializes a new instance of the <see cref="PullTenantSummaryCountController"/> class.
        /// </summary>
        /// <param name="pullTenantHelper">The helper for pull model tenant.</param>
        public PullTenantSummaryCountController(IPullTenantHelper pullTenantHelper)
        {
            _pullTenantHelper = pullTenantHelper;
        }

        /// <summary>
        /// Get summary count for the current user and specified tenant list.
        /// </summary>
        /// <param name="operationType">The name of operation</param>
        /// <param name="sessionId">The SessionId</param>
        /// <returns> This method returns a JSON specifying the total number of request pending approval for the given user and tenant combination.</returns>
        [SwaggerOperation(Tags = new[] { "Summary" })]
        [HttpGet]
        public async Task<IActionResult> Get(string operationType = "REQCOUNT", string sessionId = "")
        {
            try
            {
                var result = await _pullTenantHelper.GetSummaryCountAsync(operationType, LoggedInAlias, Alias, sessionId, Xcv, Tcv, Host);
                return Ok(result);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }
    }
}