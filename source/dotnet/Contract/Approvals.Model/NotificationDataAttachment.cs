// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace Microsoft.CFS.Approvals.Model;

public class NotificationDataAttachment
{
    /// <summary>
    /// Gets or sets the file base64.
    /// </summary>
    public string FileBase64 { get; set; }

    /// <summary>
    /// Gets or sets the name of the file.
    /// </summary>
    public string FileName { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether this instance is inline.
    /// </summary>
    public bool IsInline { get; set; }

    /// <summary>
    /// Gets or sets File size in bytes
    /// </summary>
    public long FileSize { get; set; }

    /// <summary>
    /// Gets or sets File Url
    /// </summary>
    public string FileUrl { get; set; }
}
