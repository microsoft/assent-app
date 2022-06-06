// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace Microsoft.CFS.Approvals.Model
{
    using System;
    using Microsoft.Azure.Cosmos.Table;
    public class ActionAuditLogTableRow: TableEntity
    {
        public DateTime ActionTime { get; set; }
        public string ActualUser { get; set; }
        public string ImpersonatedUser { get; set; }
        public string ClientType { get; set; }
        public string ActionType { get; set; }
        public string TenantId { get; set; }
    }
}
