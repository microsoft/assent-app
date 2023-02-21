// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace Microsoft.CFS.Approvals.CoreServices.Controllers.api.v1;

using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.CFS.Approvals.Common.BL.Interface;
using Microsoft.CFS.Approvals.Core.BL.Interface;
using Microsoft.CFS.Approvals.Utilities.Helpers;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Swashbuckle.AspNetCore.Annotations;

/// <summary>
/// The alldocumentdownloadcontroller class.
/// </summary>
/// <seealso cref="BaseApiController" />
public class AllDocumentDownloadController : BaseApiController
{
    /// <summary>
    /// The Details Helper
    /// </summary>
    private readonly IDetailsHelper _detailsHelper;

    /// <summary>
    /// Constructor of AllDocumentDownloadController
    /// </summary>
    /// <param name="detailsHelper"></param>
    public AllDocumentDownloadController(IDetailsHelper detailsHelper)
    {
        _detailsHelper = detailsHelper;
    }

    /// <summary>
    /// Retrieves attachment file for download on the details page from the tenant LOB system that the approval belongs to.
    /// </summary>
    /// <param name="tenantid">Indicates the Tenant for which the request's (documentNumber) data needs to be retrieved</param>
    /// <param name="documentNumber">The request number for which data needs to be fetched</param>
    /// <param name="displayDocumentNumber">The request number for which data needs to be fetched</param>
    /// <param name="sessionId">GUID SessionId. Unique for each user session</param>
    /// <returns>
    /// This method connects to the LOB system and based on the input parameters (documentNumber),
    /// downloads all the Attachment for the request and return that to the client device as a HttpResponse in a zip
    /// </returns>
    [SwaggerOperation(Tags = new[] { "Document" })]
    [HttpPost]
    public async Task<IActionResult> Post(int tenantid, string documentNumber, string displayDocumentNumber = "", string sessionId = "")
    {
        try
        {
            ArgumentGuard.NotNull(tenantid, nameof(tenantid));
            ArgumentGuard.NotNullAndEmpty(documentNumber, nameof(documentNumber));
            ArgumentGuard.NotNullAndEmpty(displayDocumentNumber, nameof(displayDocumentNumber));

            string attachmentString;
            using (var reader = new StreamReader(Request.Body))
            {
                attachmentString = await reader.ReadToEndAsync();
            }

            var parameters = JObject.Parse(attachmentString);
            var attachments = JsonConvert.DeserializeObject<RequestAttachment[]>(parameters["Attachments"].ToString());

            var httpResponseMessage = await _detailsHelper.GetAllDocumentsZipped
               (
                   tenantid,
                   documentNumber,
                   displayDocumentNumber,
                   null,
                   attachments,
                   sessionId,
                   Tcv,
                   Xcv,
                   Alias,
                   LoggedInAlias,
                   Host,
                   GetTokenOrCookie());

            return File(httpResponseMessage, "application/zip");
        }
        catch (Exception ex)
        {
            return BadRequest(ex.Message);
        }
    }
}