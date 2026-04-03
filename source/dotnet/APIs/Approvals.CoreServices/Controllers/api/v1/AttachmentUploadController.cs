// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace Microsoft.CFS.Approvals.CoreServices.Controllers.api.v1;

using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.CFS.Approvals.Contracts.DataContracts;
using Microsoft.CFS.Approvals.Core.BL.Interface;
using Microsoft.CFS.Approvals.Model;
using Microsoft.CFS.Approvals.Utilities.Helpers;
using Newtonsoft.Json;
using Swashbuckle.AspNetCore.Annotations;

/// <summary>
/// Class AttachmentUploadController,
/// </summary>
public class AttachmentUploadController : BaseApiController
{
    /// <summary>
    /// The Tenant Factory.
    /// </summary>
    private readonly ITenantFactory _tenantFactory;

    /// <summary>
    /// Tenant informational helper.
    /// </summary>
    private readonly IApprovalTenantInfoHelper _approvalTenantInfoHelper;
    private readonly IDetailsHelper _detailsHelper;

    /// <summary>
    /// AttachmentUploadController
    /// </summary>
    /// <param name="approvalTenantInfoHelper">approvalTenantInfoHelper</param>
    /// <param name="tenantFactory">tenantFactory</param>
    /// <param name="detailsHelper">tenantFactory</param>
    public AttachmentUploadController(IApprovalTenantInfoHelper approvalTenantInfoHelper, ITenantFactory tenantFactory, IDetailsHelper detailsHelper)
    {
        _approvalTenantInfoHelper = approvalTenantInfoHelper;
        _tenantFactory = tenantFactory;
        _detailsHelper = detailsHelper;
    }

    /// <summary>
    /// Post method for uploading the attachments.
    /// </summary>
    /// <param name="attachments"> List of files to be uploaded into the blob storage.</param>
    /// <param name="tenantId">Tenant id.</param>
    /// <param name="documentNumber">Document number.</param>
    /// <param name="displayDocumentNumber">Display Document number.</param>
    /// <param name="fiscalYear">Fiscal year.</param>
    /// <param name="sessionId">GUID SessionId. Unique for each user session</param>
    /// <param name="tcv">GUID Transaction Id. Unique for each transaction</param>
    /// <param name="xcv">GUID Cross Correlation Vector. Unique for each request across system boundaries</param>
    /// <returns></returns>
    [SwaggerOperation(Tags = new[] { "AttachmentUpload" })]
    [HttpPost("{tenantId}/{documentNumber}")]
    public async Task<IActionResult> Post(
        IFormCollection attachments,
        int tenantId,
        string documentNumber = "",
        string displayDocumentNumber = "",
        string fiscalYear = "",
        string sessionId = "",
        string tcv = "",
        string xcv = "")
    {
        try
        {
            ArgumentGuard.NotNull(tenantId, nameof(tenantId));
            ArgumentGuard.NotNull(documentNumber, nameof(documentNumber));
            List<AttachmentUploadStatus> uploadStatuses = await _detailsHelper.UploadAttachments(
                SignedInUser,
                OnBehalfUser,
                ClientDevice,
                GetTokenOrCookie(),
                JsonConvert.DeserializeObject<List<AttachmentUploadInfo>>(attachments["attachmentUploadInfo"]),
                attachments.Files,
                tenantId,
                new ApprovalIdentifier { DocumentNumber = documentNumber, DisplayDocumentNumber = string.IsNullOrWhiteSpace(displayDocumentNumber) ? documentNumber : displayDocumentNumber, FiscalYear = fiscalYear },
                sessionId,
                tcv,
                xcv);
            return Ok(uploadStatuses);
        }
        catch (Exception exception)
        {
            return BadRequest(exception.InnerException != null ? exception.InnerException.Message : exception.Message);
        }
    }

}
