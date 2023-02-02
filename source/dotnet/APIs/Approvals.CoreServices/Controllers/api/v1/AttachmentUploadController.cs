// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace Microsoft.CFS.Approvals.CoreServices.Controllers.api.v1;

using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.CFS.Approvals.Core.BL.Interface;
using Microsoft.CFS.Approvals.Model;
using Microsoft.CFS.Approvals.Utilities.Helpers;
using Swashbuckle.AspNetCore.Annotations;

/// <summary>
/// Class AttachmentUploadController,
/// </summary>
public class AttachmentUploadController : BaseApiController
{
    /// <summary>
    /// Attachment helper.
    /// </summary>
    private readonly IAttachmentHelper attachmentHelper;

    /// <summary>
    /// AttachmentUploadController.
    /// </summary>
    /// <param name="attachmentHelper"></param>       
    public AttachmentUploadController(IAttachmentHelper attachmentHelper)
    {
        this.attachmentHelper = attachmentHelper;
    }

    /// <summary>
    /// Post method for uploading the attachments.
    /// </summary>
    /// <param name="files"> List of files to be uploaded into the blob storage.</param>
    /// <param name="tenantId">Tenant id.</param>
    /// <param name="documentNumber">Document number.</param>
    /// <param name="sessionId">GUID SessionId. Unique for each user session</param>
    /// <param name="tcv">GUID Transaction Id. Unique for each transaction</param>
    /// <param name="xcv">GUID Cross Correlation Vector. Unique for each request across system boundaries</param>
    /// <returns></returns>
    [SwaggerOperation(Tags = new[] { "AttachmentUpload" })]
    [HttpPost("{tenantId}/{documentNumber}")]
    public async Task<IActionResult> Post(
        List<AttachmentUploadInfo> files,
        int tenantId,
        string documentNumber = "",
        string sessionId = "",
        string tcv = "",
        string xcv = "")
    {
        try
        {
            ArgumentGuard.NotNull(tenantId, nameof(tenantId));
            ArgumentGuard.NotNull(documentNumber, nameof(documentNumber));

            List<AttachmentUploadStatus> uploadStatuses = await attachmentHelper.UploadAttachments(files, tenantId, documentNumber, Alias, sessionId, tcv, xcv);

            return Ok(uploadStatuses);
        }
        catch (Exception exception)
        {
            return BadRequest(exception.InnerException != null ? exception.InnerException.Message : exception.Message);
        }
    }

}
