// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace Microsoft.CFS.Approvals.Model;

using Newtonsoft.Json;

/// <summary>
/// Chat request payload received from copilot platform.
/// </summary>
public class PluginChatRequest
{
    [JsonProperty("data", Order = 1)]
    public AdaptiveCardSubmissionData AdaptiveCardSubmissionData { get; set; }

    [JsonProperty("AskRequest", Order = 2)]
    public AskRequest AskRequest { get; set; }
}