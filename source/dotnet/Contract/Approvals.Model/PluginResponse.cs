// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace Microsoft.CFS.Approvals.Model;

using System.Collections.Generic;
using Newtonsoft.Json;

/// <summary>
/// Response payload sent from the Approvals plugin.
/// </summary>
public class PluginResponse
{
    [JsonProperty("message", Order = 1)]
    public string Message { get; set; }

    [JsonProperty("promptQuestions", Order = 3)]
    public string[] PromptQuestions { get; set; }

    [JsonProperty("messageType", Order = 4)]
    public string MessageType { get; set; }

}
