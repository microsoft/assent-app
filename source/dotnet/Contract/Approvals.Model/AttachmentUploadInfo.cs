// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Microsoft.CFS.Approvals.Contracts.DataContracts;

namespace Microsoft.CFS.Approvals.Model;

/// <summary>
/// File Upload Information.
/// </summary>
public class AttachmentUploadInfo : Attachment
{
    /// <summary>
    /// Gets or sets the file size.
    /// </summary>
    public long FileSize { get; set; }

    /// <summary>
    /// Gets or sets the file as a base64 string.
    /// </summary>
    public string Base64Content { get; set; }
}
