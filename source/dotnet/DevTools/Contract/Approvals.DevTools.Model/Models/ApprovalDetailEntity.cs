// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace Microsoft.CFS.Approvals.DevTools.Model.Models
{
    using Microsoft.Azure.Cosmos.Table;

    /// <summary>
    /// Approval Detail Entity class
    /// </summary>
    public class ApprovalDetailEntity : TableEntity
    {
        public string JSONData { get; set; }
        public int TenantID { get; set; }
    }
}
