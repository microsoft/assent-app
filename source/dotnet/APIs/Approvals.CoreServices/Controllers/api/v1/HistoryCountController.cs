// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace Microsoft.CFS.Approvals.CoreServices.Controllers.api.v1
{
    using System;
    using System.Threading.Tasks;
    using Microsoft.AspNetCore.Mvc;
    using Microsoft.CFS.Approvals.CoreServices.BL.Interface;
    using Microsoft.CFS.Approvals.Extensions;
    using Swashbuckle.AspNetCore.Annotations;

    /// <summary>
    /// Class HistoryCountController.
    /// </summary>
    /// <seealso cref="BaseApiController" />
    public class HistoryCountController : BaseApiController
    {
        /// <summary>
        /// The approval history helper.
        /// </summary>
        private readonly IApprovalHistoryHelper _approvalHistoryHelper = null;

        /// <summary>
        /// Initializes a new instance of the <see cref="HistoryCountController"/> class.
        /// </summary>
        /// <param name="approvalHistoryHelper">The approval history helper.</param>
        public HistoryCountController(IApprovalHistoryHelper approvalHistoryHelper)
        {
            _approvalHistoryHelper = approvalHistoryHelper;
        }

        /// <summary>
        /// Retrieves count of historical approvals, based on search criteria.
        /// </summary>
        /// <param name="searchCriteria"> criteria on which search needs to be done</param>
        /// <param name="timePeriod">time period for which records needs to be fetched</param>
        /// <param name="sessionId">GUID SessionId. Unique for each user session</param>
        /// <returns>
        /// This method returns an JSON with the total number of records for the given user
        /// </returns>
        /// <remarks>
        /// <para>
        /// e.g.
        /// HTTP GET api/HistoryCount?[QueryString]
        /// </para>
        /// </remarks>
        [SwaggerOperation(Tags = new[] { "History" })]
        [HttpGet]
        public async Task<IActionResult> Get(string searchCriteria = "", int timePeriod = 0, string sessionId = "")
        {
            try
            {
                var historyCountData = await _approvalHistoryHelper.GetHistoryCountforAlias(Alias, timePeriod, searchCriteria, LoggedInAlias, sessionId, Host, Xcv, Tcv);
                return Ok(historyCountData);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }
    }
}