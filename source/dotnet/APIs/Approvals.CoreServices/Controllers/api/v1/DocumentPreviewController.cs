// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace Microsoft.CFS.Approvals.CoreServices.Controllers.api.v1
{
    using System;
    using System.Net.Http;
    using System.Threading.Tasks;
    using Microsoft.AspNetCore.Mvc;
    using Microsoft.CFS.Approvals.Contracts;
    using Microsoft.CFS.Approvals.Core.BL.Interface;
    using Microsoft.CFS.Approvals.CoreServices.BL.Interface;
    using Microsoft.CFS.Approvals.Utilities.Helpers;
    using Swashbuckle.AspNetCore.Annotations;

    /// <summary>
    /// The document preview controller.
    /// </summary>
    /// <seealso cref="BaseApiController" />
    [Route("api/v1/[controller]/{tenantId}/{documentNumber}")]
    public class DocumentPreviewController : BaseApiController
    {
        /// <summary>
        /// The Details helper.
        /// </summary>
        private readonly IDetailsHelper _detailsHelper;

        /// <summary>
        /// The Office document creator.
        /// </summary>
        private readonly IOfficeDocumentCreator _officeDocumentCreator;

        /// <summary>
        /// Initializes a new instance of the <see cref="DocumentPreviewController"/> class.
        /// </summary>
        /// <param name="detailsHelper"></param>
        /// <param name="officeDocumentCreator"></param>
        public DocumentPreviewController(IDetailsHelper detailsHelper, IOfficeDocumentCreator officeDocumentCreator)
        {
            _detailsHelper = detailsHelper;
            _officeDocumentCreator = officeDocumentCreator;
        }

        /// <summary>
        /// Retrieves attachment file for preview on the details page from the tenant LOB system that the approval belongs to.
        /// </summary>
        /// <param name="tenantId">Indicates the Tenant for which the request's (documentNumber) data needs to be retrieved</param>
        /// <param name="documentNumber">The request number for which data needs to be fetched</param>
        /// <param name="displayDocumentNumber">The request number for which data needs to be fetched</param>
        /// <param name="fiscalYear">Fiscal Year of Request number (if any)</param>
        /// <param name="page"></param>
        /// <param name="attachmentId">The unique attachmentId of the attachment to be previewed from LOB system</param>
        /// <param name="attachmentName">The attachmentName of the attachment to be previewed from LOB system</param>
        /// <param name="sessionId">GUID SessionId. Unique for each user session</param>
        /// <param name="tcv">GUID Transaction Id. Unique for each transaction</param>
        /// <param name="xcv">GUID Cross Correlation Vector. Unique for each request across system boundaries</param>
        /// <returns>
        /// This method connects to the LOB system and based on the input parameters (documentNumber/attachmentId),
        /// preview the Attachment for the request and return that to the client device as a HttpResponse
        /// </returns>
        [SwaggerOperation(Tags = new[] { "Document" })]
        [HttpGet]
        public async Task<IActionResult> Get(
            int tenantId,
            string documentNumber,
            string displayDocumentNumber = "",
            string fiscalYear = "",
            int page = 1,
            string attachmentId = "",
            string attachmentName = "",
            string sessionId = "",
            string tcv = "",
            string xcv = "")
        {
            try
            {
                ArgumentGuard.NotNull(tenantId, nameof(tenantId));
                ArgumentGuard.NotNullAndEmpty(documentNumber, nameof(documentNumber));
                ArgumentGuard.NotNullAndEmpty(displayDocumentNumber, nameof(displayDocumentNumber));

                var officeDocType = GetOfficeDocType(attachmentName);

                var httpResponseMessage = await _detailsHelper.GetDocumentPreview
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
                string response = "";
                if (officeDocType != OfficeDocumentType.NONE)
                {
                    string fileName = _officeDocumentCreator.GetDocumentURL(httpResponseMessage, displayDocumentNumber, attachmentName, LoggedInAlias, sessionId);
                    response = (@"{""filename"": """ + fileName + @"""}");
                    return File(response, "application/octet-stream");
                }
                else
                {
                    return File(httpResponseMessage, "application/octet-stream");
                }
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        /// <summary>
        /// Get office document type.
        /// </summary>
        /// <param name="attachmentName"></param>
        /// <returns>document type</returns>
        private string GetOfficeDocType(string attachmentName)
        {
            string officeDocType = OfficeDocumentType.NONE;
            if (attachmentName.ToLower().EndsWith(".xls") ||
                attachmentName.ToLower().EndsWith(".xlsx")
                )
            {
                officeDocType = OfficeDocumentType.EXCEL;
            }
            else if (attachmentName.ToLower().EndsWith(".doc") ||
                attachmentName.ToLower().EndsWith(".docx")
                )
            {
                officeDocType = OfficeDocumentType.WORD;
            }
            return officeDocType;
        }
    }
}