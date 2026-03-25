// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace Microsoft.CFS.Approvals.Model;

using System;
using System.Collections.Generic;
using Microsoft.CFS.Approvals.Contracts;
using Newtonsoft.Json;

/// <summary>
/// Request payload sent to Approvals plugin.
/// </summary>
public class AskRequest
{
    [JsonProperty("userId", Order = 1)]
    public string UserId { get; set; }

    [JsonProperty("userName", Order = 2)]
    public string UserName { get; set; }

    [JsonProperty("userEmail", Order = 3)]
    public string UserEmail { get; set; }

    [JsonProperty("input", Order = 4)]
    public string Input { get; set; }

    [JsonProperty("ChatId", Order = 5)]
    public string ChatId { get; set; }

    [JsonProperty("history", Order = 7)]
    public List<History> History { get; set; }

    [JsonProperty("additionalDetails", Order = 8)]
    public Dictionary<string, string> AdditionalDetails { get; set; }
}

/// <summary>
/// Chat conversation history.
/// </summary>
public class History
{
    [JsonProperty("user", Order = 1)]
    public string User { get; set; }

    [JsonProperty("bot", Order = 2)]
    public string Bot { get; set; }
}

/// <summary>
/// Chat request context
/// </summary>
public class ChatRequestEventArgs : EventArgs
{
    [JsonProperty("userAlias", Order = 1)]
    public string UserAlias { get; set; }

    // 0 is none, 1 is out of sync
    [JsonProperty("copilotErrorType", Order = 2)]
    public CopilotErrorType CopilotErrorType { get; set; }

    [JsonProperty("documentNumber", Order = 3)]
    public string DocumentNumber { get; set; }

    [JsonProperty("tenantId", Order = 4)]
    public int TenantId { get; set; }

    [JsonProperty("detailsData", Order = 5)]
    public string DetailsData { get; set; }

    [JsonProperty("attachmentData", Order = 6)]
    public string AttachmentData { get; set; }

    [JsonProperty("userContext", Order = 7)]
    public string UserContext { get; set; }

    [JsonProperty("userConsent", Order = 8)]
    public ActionConsentType UserConsent { get; set; }

    [JsonProperty("summaryData", Order = 9)]
    public string SummaryData { get; set; }

    [JsonProperty("tenantNames", Order = 10)]
    public string TenantNames { get; set; }

    [JsonProperty("onBehalfUserAlias", Order = 11)]
    public string OnBehalfUserAlias { get; set; }

    [JsonProperty("onBehalfUserId", Order = 12)]
    public string OnBehalfUserId { get; set; }


}