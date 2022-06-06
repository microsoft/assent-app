// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace Microsoft.CFS.Approvals.DevTools.Model.Models
{
    using System;
    using Microsoft.Azure.Cosmos.Table;

    /// <summary>
    /// Tenant Down Time Message Entity class
    /// </summary>
    public class TenantDownTimeMessageEntity : TableEntity
    {
        public string BannerType { get; set; }
        public string CreatedBy { get; set; }
        public DateTime CreatedDate { get; set; }
        public DateTime EventEndTime { get; set; }
        public DateTime EventStartTime { get; set; }
        public bool IsScheduled { get; set; }
        public string NotificationBody { get; set; }
        public string NotificationTitle { get; set; }
        public int TenantId { get; set; }
    }
}
