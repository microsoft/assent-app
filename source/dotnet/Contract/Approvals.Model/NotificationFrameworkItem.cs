// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace Microsoft.CFS.Approvals.Model;

using System.Collections.Generic;

/// <summary>
/// The NotificationFrameworkItem class for notification provider
/// </summary>
public class NotificationFrameworkItem : NotificationData
{
    /// <summary>
    /// Gets or sets NotificationTypes
    /// </summary>
    public List<NotificationFrameworkType> NotificationTypes { get; set; }

    /// <summary>
    /// Gets or sets ApplicationName
    /// </summary>
    public string ApplicationName { get; set; }

    /// <summary>
    /// Gets or sets WebPushNotifcationTag
    /// </summary>
    public string WebPushNotificationTag { get; set; }

    /// <summary>
    /// Gets or sets DeeplinkUrl
    /// </summary>
    public string DeeplinkUrl { get; set; }

    /// <summary>
    /// Gets or sets SendOnUtcData
    /// </summary>
    public string SendOnUtcDate { get; set; }

    /// <summary>
    /// Gets or sets TenantIdentifier
    /// </summary>
    public string TenantIdentifier { get; set; }

    /// <summary>
    /// Gets or sets TemplateId
    /// </summary>
    public string TemplateId { get; set; }

    /// <summary>
    /// Gets or sets Telemetry
    /// </summary>
    public Telemetry Telemetry { get; set; }

    /// <summary>
    /// Gets or sets EmailAccountNumberToUse
    /// </summary>
    public string EmailAccountNumberToUse { get; set; }

    /// <summary>
    /// Gets or sets Priority
    /// </summary>
    public NotificationPriority Priority { get; set; }
}

/// <summary>
/// Enum which will be used for Notification Framework Type
/// </summary>
public enum NotificationFrameworkType
{
    Mail,
    ActionableEmail,
    Tile,
    Toast,
    Badge,
    Raw,
    WebPush,
    Text
}

/// <summary>
/// Enum which will be used for Notification Priority
/// </summary>
public enum NotificationPriority
{
    High = 0,
    Low = 1,
    Normal = 2
}

/// <summary>
/// Class for Telemetry
/// </summary>
public class Telemetry
{
    /// <summary>
    /// Gets or sets Telemetry Xcv
    /// </summary>
    public string Xcv { get; set; }

    /// <summary>
    /// Get or sets MessageId
    /// </summary>
    public string MessageId { get; set; }
}
