// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace Microsoft.CFS.Approvals.Model;

using Newtonsoft.Json;


/// <summary>
/// Attachment details with additional data
/// </summary>
public class OCRJsonData
{
    /// <summary>
    /// Attachement Id
    /// </summary>
    [JsonProperty("id")]
    public string Id { get; set; }

    /// <summary>
    /// Attachment Name
    /// </summary>
    [JsonProperty("fileName")]
    public string FileName { get; set; }

    /// <summary>
    /// Document number
    /// </summary>
    [JsonProperty("OCRoutput")]
    public string OCRoutput { get; set; }
}