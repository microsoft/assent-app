// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace Microsoft.CFS.Approvals.CoreServices.Controllers.api.v1
{
    using System;
    using System.Threading.Tasks;
    using Microsoft.AspNetCore.Mvc;
    using Microsoft.CFS.Approvals.Contracts;
    using Microsoft.CFS.Approvals.Core.BL.Interface;
    using Swashbuckle.AspNetCore.Annotations;

    /// <summary>
    /// the OutOfSyncSummaryController
    /// </summary>
    /// <seealso cref="BaseApiController"/>
    public class OutOfSyncSummaryController : BaseApiController
    {
        /// <summary>
        /// The summary helper
        /// </summary>
        private readonly ISummaryHelper _summaryHelper = null;

        /// <summary>
        /// Initializes a new instance of the <see cref="OutOfSyncSummaryController"/> class.
        /// </summary>
        /// <param name="summaryHelper"></param>
        public OutOfSyncSummaryController(ISummaryHelper summaryHelper)
        {
            _summaryHelper = summaryHelper;
        }

        /// <summary>
        /// Get out of sync requests for the current user sorted/ filtered by tenant from azure table storage given Document Type ID (optional).
        /// </summary>
        /// <param name="id">GUID DocumentTypeId of the Tenant</param>
        /// <param name="sessionId">GUID SessionId. Unique for each user session</param>
        /// <returns>
        /// This method returns a JSON for all the out of sync approval requests for the given user and Tenant combination.
        /// This contains only summary data (i.e. no details) to be displayed on the home page.
        /// </returns>
        /// <remarks>
        /// <para>
        /// e.g.
        /// HTTP GET api/v1/OutOfSyncSummary?id=[tenantDocTypeID]
        /// </para>
        /// </remarks>
        [SwaggerOperation(Tags = new[] { "Summary" })]
        [HttpGet]
        public async Task<IActionResult> Get(string id, string sessionId = "")
        {
            try
            {
                return Ok(await _summaryHelper.GetOtherSummaryRequests(LoggedInAlias, Alias, Host, Constants.OutOfSyncAction, sessionId, id));
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }
    }
}