// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.


namespace Microsoft.CFS.Approvals.SyntheticTransaction.API.Controllers
{
    using Microsoft.AspNetCore.Mvc;
    using Microsoft.CFS.Approvals.SyntheticTransaction.Helpers.Interface;

    /// <summary>
    /// The Bulk Delete Controller
    /// </summary>
    [Route("api/v1/BulkDelete")]
    [ApiController]
    public class BulkDeleteController : ControllerBase
    {
        private readonly IBulkDeleteHelper _bulkDeleteHelper;

        /// <summary>
        /// Constructor of BulkDeleteController
        /// </summary>
        /// <param name="bulkDeleteHelper"></param>
        public BulkDeleteController(IBulkDeleteHelper bulkDeleteHelper)
        {
            _bulkDeleteHelper = bulkDeleteHelper;
        }

        /// <summary>
        /// Bulk delete document
        /// </summary>
        /// <param name="Tenant"></param>
        /// <param name="Approver"></param>
        /// <param name="Days"></param>
        /// <param name="DocNumber"></param>
        /// <returns></returns>
        [HttpPost]
        [Route("BulkDeleteDocument/{env}")]
        public IActionResult BulkDeleteDocument(string Tenant, string Approver, string Days, string DocNumber)
        {
            var result = _bulkDeleteHelper.BulkDelete(Tenant, Approver, Days, DocNumber);
            return Ok(result);
        }
    }
}
