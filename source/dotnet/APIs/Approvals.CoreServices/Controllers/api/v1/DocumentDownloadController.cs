// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace Microsoft.CFS.Approvals.CoreServices.Controllers.api.v1
{
    using System;
    using System.IO;
    using System.Net.Http;
    using System.Threading.Tasks;
    using Microsoft.AspNetCore.Mvc;
    using Microsoft.CFS.Approvals.Core.BL.Interface;
    using Microsoft.CFS.Approvals.Utilities.Helpers;
    using Swashbuckle.AspNetCore.Annotations;

    /// <summary>
    /// the DocumentDownloadController class
    /// </summary>
    /// <seealso cref="BaseApiController" />
    public class DocumentDownloadController : BaseApiController
    {
        /// <summary>
        /// The Details Helper.
        /// </summary>
        /// <seealso cref="BaseApiController" />
        private readonly IDetailsHelper _detailsHelper;

        /// <summary>
        /// Constructor of Details Helper.
        /// </summary>
        /// <param name="detailsHelper"></param>
        public DocumentDownloadController(IDetailsHelper detailsHelper)
        {
            _detailsHelper = detailsHelper;
        }

        /// <summary>
        /// Retrieves attachment file for download on the details page from the tenant LOB system that the approval belongs to.
        /// </summary>
        /// <param name="tenantId">Indicates the Tenant for which the request's (documentNumber) data needs to be retrieved</param>
        /// <param name="documentNumber">The request number for which data needs to be fetched</param>
        /// <param name="displayDocumentNumber">The request number for which data needs to be fetched</param>
        /// <param name="fiscalYear">Fiscal Year of Request number (if any)</param>
        /// <param name="page"></param>
        /// <param name="attachmentId">The unique attachmentId of the attachment to be downloaded from LOB system</param>
        /// <param name="sessionId">GUID SessionId. Unique for each user session</param>
        /// <param name="tcv">GUID Transaction Id. Unique for each transaction</param>
        /// <param name="xcv">GUID Cross Correlation Vector. Unique for each request across system boundaries</param>
        /// <returns>
        /// This method connects to the LOB system and based on the input parameters (documentNumber/attachmentId),
        /// downloads the Attachment for the request and return that to the client device as a HttpResponse
        /// </returns>
        /// <remarks>
        /// <para>
        /// e.g.
        /// This is called when there is no specific attachmentId
        /// HTTP GET api/DocumentDownload/[tenantId]/[documentNumber]
        /// </para>
        /// <para>
        /// e.g.
        /// This is called when there is fiscalyear and attachmentId specified
        /// HTTP GET api/DocumentDownload/[tenantId]/[documentNumber]/[fiscalYear]/[attachmentId]
        /// </para>
        /// </remarks>

        [SwaggerOperation(Tags = new[] { "Document" })]
        [HttpGet("{tenantId}/{documentNumber}")]
        public async Task<IActionResult> Get(
            int tenantId,
            string documentNumber,
            string displayDocumentNumber = "",
            string fiscalYear = "",
            int page = 1,
            string attachmentId = "",
            string sessionId = "",
            string tcv = "",
            string xcv = "")
        {
            try
            {
                ArgumentGuard.NotNull(tenantId, nameof(tenantId));
                ArgumentGuard.NotNullAndEmpty(documentNumber, nameof(documentNumber));
                ArgumentGuard.NotNullAndEmpty(displayDocumentNumber, nameof(displayDocumentNumber));

                var httpResponseMessage = await _detailsHelper.GetDocuments
                   (
                       tenantId,
                       documentNumber,
                       displayDocumentNumber,
                       fiscalYear,
                       attachmentId,
                       sessionId,
                       tcv,
                       xcv,
                       Alias,
                       LoggedInAlias,
                       Host,
                       GetTokenOrCookie());

                return File(httpResponseMessage, "application/octet-stream");
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        /// <summary>
        /// Retrieves attachment file for download on the details page from the tenant LOB system that the approval belongs to.
        /// </summary>
        /// <param name="tenantId">Indicates the Tenant for which the request's (documentNumber) data needs to be retrieved</param>
        /// <param name="sessionId">GUID SessionId. Unique for each user session</param>
        /// <param name="tcv">GUID Transaction Id. Unique for each transaction</param>
        /// <returns>
        /// This method connects to the LOB system and based on the request content (list of documentNumbers),
        /// downloads all the Attachment for the set of requests and return that to the client device as a compressed file in a HttpResponse
        /// </returns>
        [SwaggerOperation(Tags = new[] { "Document" })]
        [HttpPost("{tenantId}")]
        public async Task<IActionResult> Post(int tenantId, string tcv = "", string sessionId = "")
        {
            try
            {
                ArgumentGuard.NotNull(tenantId, nameof(tenantId));

                string requestBody;
                using (var reader = new StreamReader(Request.Body))
                {
                    requestBody = await reader.ReadToEndAsync();
                }
                var httpResponseMessage = await _detailsHelper.GetAllAttachmentsInBulk
                    (
                       tenantId,
                       sessionId,
                       tcv,
                       requestBody,
                       Alias,
                       LoggedInAlias,
                       Host,
                       GetTokenOrCookie());

                return File(httpResponseMessage, "application/octet-stream");
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }
    }
}