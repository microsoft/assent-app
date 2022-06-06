// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace Microsoft.CFS.Approvals.CoreServices.Controllers.api.v1
{
    using System;
    using System.IO;
    using System.Threading.Tasks;
    using Microsoft.AspNetCore.Mvc;
    using Microsoft.CFS.Approvals.Core.BL.Interface;
    using Microsoft.CFS.Approvals.Utilities.Helpers;
    using Swashbuckle.AspNetCore.Annotations;

    /// <summary>
    /// Class DocumentStatusController.
    /// </summary>
    /// <seealso cref="BaseApiController" />
    [Route("api/v1/[controller]/{tenantId}")]
    public class DocumentStatusController : BaseApiController
    {
        /// <summary>
        /// The Document Approval Satus Helper
        /// </summary>
        private readonly IDocumentApprovalStatusHelper _documentApprovalStatusHelper;

        /// <summary>
        /// Constructor of DocumentStatusController
        /// </summary>
        /// <param name="documentApprovalStatusHelper"></param>
        public DocumentStatusController(IDocumentApprovalStatusHelper documentApprovalStatusHelper)
        {
            _documentApprovalStatusHelper = documentApprovalStatusHelper;
        }

        /// <summary>
        /// Checks Document status based on tenantId and ApprovalIdentifier whether it is pending/approve/reject
        /// The ApprovalIdentifier is a part of the HttpRequest's Content, which contains details of documentnumber and displaydocumentnumber;
        /// </summary>
        /// <param name="tenantId">The Unique TenantId (Int32) for which the action needs to be performed for the given request or documentNumber</param>
        /// <param name="sessionId">GUID SessionId. Unique for each user session</param>
        /// <param name="tcv">GUID Transaction Id. Unique for each transaction</param>
        /// <param name="xcv">Document number.</param>
        /// <returns>
        /// This method returns an OK HttpResponse if the action is successful;
        /// else return a Bad Request along with the Error Message specifying the reason for failure
        /// A tracking GUID is also sent in the response content for failure scenarios which can help in failure log analysis
        /// </returns>
        /// <remarks>
        /// <para>
        /// e.g.
        /// HTTP POST api/DocumentStatus/[tenantId]
        /// </para>
        /// </remarks>
        [SwaggerOperation(Tags = new[] { "Details" })]
        [HttpPost]
        public async Task<IActionResult> Post(int tenantId, string sessionId = "", string tcv = "", string xcv = "")
        {
            try
            {
                ArgumentGuard.NotNull(tenantId, nameof(tenantId));
                string requestContent;
                using (var reader = new StreamReader(Request.Body))
                {
                    requestContent = await reader.ReadToEndAsync();
                }
                return Ok(await _documentApprovalStatusHelper.DocumentStatus(tenantId, requestContent, Host, Alias, LoggedInAlias, tcv, sessionId, xcv));
            }
            catch (Exception exception)
            {
                return BadRequest(exception.InnerException != null ? exception.InnerException.Message : exception.Message);
            }
        }
    }
}