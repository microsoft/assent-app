// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace Microsoft.CFS.Approvals.DevTools.Model.Models
{
    using Microsoft.Azure.Cosmos.Table;

    /// <summary>
    /// Tenant Entity class
    /// </summary>
    public class TenantEntity : TableEntity
    {
        public string TenantActionDetails { get; set; }
        public string DocTypeId { get; set; }
        public string AppName { get; set; }
        public string ActionableEmailFolderName { get; set; }
        public string TenantImage { get; set; }

    }
}
