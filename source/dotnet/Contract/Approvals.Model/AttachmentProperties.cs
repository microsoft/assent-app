// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace Microsoft.CFS.Approvals.Model;

/// <summary>
/// Class for the tenant configuration for attachment uploads.
/// </summary>
public class AttachmentProperties
{
    /// <summary>
    /// Gets or sets the container for the tenant.
    /// </summary>
    public string AttachmentContainerName { get; set; }

    /// <summary>
    /// Gets or sets the file attachment options for UI to validate.
    /// </summary>
    public FileAttachmentOptions FileAttachmentOptions { get; set; }
}

/// <summary>
/// File attachment configuration per tenant.
/// </summary>
public class FileAttachmentOptions
{
    /// <summary>
    /// Gets or sets whether the file upload feature is enabled.
    /// </summary>
    public bool AllowFileUpload { get; set; }

    /// <summary>
    /// Gets or sets the maximum number of files that can be attached for the request. 
    /// </summary>
    public int? MaxAttachments { get; set; }

    /// <summary>
    /// Gets or sets the file types that allowed to be uploaded for the request.
    /// </summary>
    public string AllowedFileTypes { get; set; }

    /// <summary>
    /// Gets or sets the max file size that is allowed to be uploaded.
    /// </summary>
    public long MaxFileSizeInBytes { get; set; }
}
