// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace Microsoft.CFS.Approvals.Model;

using Newtonsoft.Json;

/// <summary>
/// Request payload sent to Approvals plugin.
/// </summary>
public class AdaptiveCardSubmissionData
{
    [JsonProperty("url", Order = 0)]
    public string Url { get; set; }

    [JsonProperty("scope", Order = 1)]
    public string Scope { get; set; }

    [JsonProperty("AskRequest", Order = 2)]
    public AskRequest AskRequest { get; set; }
}