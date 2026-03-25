// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace Microsoft.CFS.Approvals.Model;

using Newtonsoft.Json;

/// <summary>
/// Attachment details with additional data
/// </summary>
public class DiscrepancyData
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
    /// Riskscore gotten from Azure OpenAI call
    /// </summary>
    [JsonProperty("riskScore")]
    public string RiskScore { get; set; }


    /// <summary>
    /// Riskscore explanation gotten from Azure OpenAI call
    /// </summary>
    [JsonProperty("RiskScoreSummary")]
    public string RiskScoreSummary { get; set; }
}