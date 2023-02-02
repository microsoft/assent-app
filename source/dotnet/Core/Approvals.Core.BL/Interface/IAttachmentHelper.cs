// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace Microsoft.CFS.Approvals.Core.BL.Interface;

using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.CFS.Approvals.Model;

/// <summary>
/// Interface for Upload attachments.
/// </summary>
public interface IAttachmentHelper
{
    /// <summary>
    /// Upload the attachments into the tenant based blob storage.
    /// </summary>
    /// <param name="files">List of file in base64 and metadata.</param>
    /// <param name="tenantId">Tenant Id.</param>
    /// <param name="documentId">Document Id.</param>
    /// <param name="sessionId">Session id for the upload attachment.</param>
    /// <param name="tcv">Transaction Id for the attachment.</param>
    /// <param name="xcv">Document id for the approval request.</param>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
    Task<List<AttachmentUploadStatus>> UploadAttachments(List<AttachmentUploadInfo> files, int tenantId, string documentId, string userAlias, string sessionId, string tcv, string xcv);

    /// <summary>
    /// Get the blob urls with sas token for the tenant to read the files.
    /// </summary>
    /// <param name="tenantId">Tenant id.</param>
    /// <param name="documentNumber">Document number.</param>
    /// <returns></returns>
    object GetAttachmentDetailsForTenantNotification(int tenantId, string documentNumber);
}
