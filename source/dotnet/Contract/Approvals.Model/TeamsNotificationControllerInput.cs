// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace Microsoft.CFS.Approvals.Model;

using System;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

/// <summary>
/// Data object used to call the teams notification handler.
/// </summary>
public class TeamsNotificationControllerInput
{
    /// <summary>
    /// Gets or sets the user objectId for the notification receiver for the approval.
    /// </summary>
    [JsonProperty(Required = Required.Always)]
    public string[] NotificationReceiverObjectId { get; set; }

    /// <summary>
    /// Gets or sets the user objectId for the creator of the approval.
    /// </summary>
    [JsonProperty(Required = Required.Always)]
    public string NotificationSender { get; set; }

    /// <summary>
    /// Gets or sets the title of the approval request.
    /// </summary>
    [JsonProperty(Required = Required.Always)]
    public string Title { get; set; }

    /// <summary>
    /// Gets or sets the title of the approval request.
    /// </summary>
    [JsonProperty(Required = Required.Always)]
    [JsonConverter(typeof(StringEnumConverter))]
    public ApprovalUserRole UserRole { get; set; }

    /// <summary>
    /// Gets or sets the title of the approval request.
    /// </summary>
    [JsonProperty(Required = Required.Always)]
    public Uri TemplateUri { get; set; }

    /// <summary>
    /// Gets or sets the title of the approval request.
    /// </summary>
    [JsonProperty(Required = Required.Always)]
    public Uri DetailsUri { get; set; }

    /// <summary>
    /// Gets or sets the Response of the approval request.
    /// </summary>
    public string Response { get; set; }

    /// <summary>
    /// Gets or sets the Provider of the approval request.
    /// </summary>
    [JsonConverter(typeof(StringEnumConverter))]
    public ApprovalProviderType Provider { get; set; }

    /// <summary>
    /// Gets or sets the title of the approval request.
    /// </summary>
    [JsonProperty(Required = Required.Always)]
    [JsonConverter(typeof(StringEnumConverter))]
    public ApprovalEventType EventType { get; set; }
}

/// <summary>
/// Enum EventType
/// </summary>
public enum ApprovalEventType
{
    Creation,
    Response,
    Completion,
    Cancellation
}

/// <summary>
/// Enum UserRole
/// </summary>
public enum ApprovalUserRole
{
    Approver,
    Owner
}

/// <summary>
/// Enum ApprovalProviderType
/// </summary>
public enum ApprovalProviderType
{
    LineOfBusiness,
    Extensibility
}