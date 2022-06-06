// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace Microsoft.CFS.Approvals.CoreServices.Controllers.api.v1
{
    using System;
    using System.Threading.Tasks;
    using Microsoft.AspNetCore.Mvc;
    using Microsoft.CFS.Approvals.Contracts;
    using Microsoft.CFS.Approvals.Core.BL.Interface;
    using Microsoft.CFS.Approvals.Extensions;
    using Microsoft.CFS.Approvals.Utilities.Helpers;
    using Swashbuckle.AspNetCore.Annotations;

    /// <summary>
    /// the PullTenantController class.
    /// </summary>
    /// <seealso cref="BaseApiController" />
    public class PullTenantController : BaseApiController
    {
        /// <summary>
        /// The pull tenant helper.
        /// </summary>
        private readonly IPullTenantHelper _pullTenantHelper = null;

        /// <summary>
        /// Initializes a new instance of the <see cref="PullTenantController"/> class.
        /// </summary>
        /// <param name="pullTenantHelper">The helper for pull model tenant.</param>
        public PullTenantController(IPullTenantHelper pullTenantHelper)
        {
            _pullTenantHelper = pullTenantHelper;
        }

        /// <summary>
        /// Get summary for the current user from tenant system.
        /// </summary>
        /// <param name="tenantId">TenantId which is unique to each Tenant.</param>
        /// <param name="sessionId">SessionId which is unique to each user session.</param>
        /// <returns>
        /// Pending approvals for the current user from the tenant system.
        /// This contains only summary data (i.e. no details) to be displayed on the tabular bulk approval view page.
        /// </returns>
        /// <remarks>
        /// <para>
        /// e.g.
        /// HTTP GET api/PullTenant/[tenantId]?SessionId=[userSessionId]
        /// </para>
        /// </remarks>
        [SwaggerOperation(Tags = new[] { "Summary" })]
        [HttpGet("{tenantId}")]
        public async Task<IActionResult> Get(int tenantId, string sessionId = "")
        {
            try
            {
                ArgumentGuard.NotNull(tenantId, nameof(tenantId));

                var parameters = GetFilterParameters();
                if (!parameters.ContainsKey("alias"))
                {
                    parameters.Add("alias", Alias);
                }

                var summaryData = await _pullTenantHelper.GetSummaryAsync(Alias,
                    LoggedInAlias,
                    parameters,
                    tenantId,
                    Host,
                    sessionId,
                    Xcv,
                    Tcv
                    );
                return Ok(summaryData);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        /// <summary>
        /// Get summary for the current user from tenant system.
        /// </summary>
        /// <param name="tenantId">TenantId which is unique to each Tenant.</param>
        /// <param name="documentNumber">DocumentNumber which is unique to each record.</param>
        /// <param name="operationType">Type of operation which needs to be executed from tenant info configuration.</param>
        /// <param name="sessionId">SessionId which is unique to each user session.</param>
        /// <returns>
        /// The Details of an request from Details API of Tenant system.
        /// </returns>
        /// <remarks>
        /// <para>
        /// e.g.
        /// HTTP GET api/PullTenant/[tenantId]/[documentNumber]?SessionId=[userSessionId]
        /// UI should call with Header (FilterParameters) containing all the values to be replaced in the details URL.
        /// </para>
        /// </remarks>
        [SwaggerOperation(Tags = new[] { "Details" })]
        [HttpGet("{tenantId}/{documentNumber}")]
        public async Task<IActionResult> Get(int tenantId, string documentNumber, string operationType = "DTL", string sessionId = "")
        {
            try
            {
                var parameters = GetFilterParameters();
                if (!parameters.ContainsKey("alias"))
                {
                    parameters.Add("alias", Alias);
                }

                if (!parameters.ContainsKey(Constants.DocumentNumber))
                {
                    parameters.Add(Constants.DocumentNumber, documentNumber);
                }

                return Ok(await _pullTenantHelper.GetDetailsAsync(
                    Alias,
                    LoggedInAlias,
                    operationType,
                    parameters,
                    tenantId,
                    Host,
                    sessionId,
                    Xcv,
                    Tcv
                    ));
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }
    }
}