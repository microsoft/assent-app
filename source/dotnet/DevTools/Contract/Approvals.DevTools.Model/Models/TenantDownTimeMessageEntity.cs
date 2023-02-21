// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace Microsoft.CFS.Approvals.DevTools.Model.Models;

using System;
using Microsoft.CFS.Approvals.DevTools.Model.Extension;
using Microsoft.CFS.Approvals.Model;

/// <summary>
/// Tenant Down Time Message Entity class
/// </summary>
public class TenantDownTimeMessageEntity : BaseTableEntity
{
    private DateTime eventStartTime { get; set; }
    private DateTime eventEndTime { get; set; }
    public string BannerType { get; set; }
    public string CreatedBy { get; set; }
    public DateTime CreatedDate { get; set; }
    public DateTime EventEndTime { get { return eventEndTime; } set { eventEndTime = value.GetDateTimeWithUtcKind(); } }
    public DateTime EventStartTime { get { return eventStartTime; } set { eventStartTime = value.GetDateTimeWithUtcKind(); } }
    public bool IsScheduled { get; set; }
    public string NotificationBody { get; set; }
    public string NotificationTitle { get; set; }
    public int TenantId { get; set; }
}
