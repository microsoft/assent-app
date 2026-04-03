// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Newtonsoft.Json;

namespace Microsoft.CFS.Approvals.Model;

/// <summary>
/// Attachment details with additional data
/// </summary>
public class OCRAttachmentData
{
    /// <summary>
    /// Attachment Name
    /// </summary>
    [JsonProperty("name")]
    public string Name { get; set; }

    /// <summary>
    /// Attachement Id
    /// </summary>
    [JsonProperty("id")]
    public string Id { get; set; }

    /// <summary>
    /// Document number
    /// </summary>
    [JsonProperty("documentNumber")]
    public string DocumentNumber { get; set; }

    /// <summary>
    /// Tenant Id
    /// </summary>
    [JsonProperty("tenantId")]
    public string TenantId { get; set; }

    /// <summary>
    /// Attachment Blob Url
    /// </summary>
    [JsonProperty("url")]
    public string Url { get; set; }
}