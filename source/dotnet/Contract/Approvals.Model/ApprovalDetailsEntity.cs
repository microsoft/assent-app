// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace Microsoft.CFS.Approvals.Model
{
    using Microsoft.Azure.Cosmos.Table;

    public class ApprovalDetailsEntity : TableEntity
    {
        public int TenantID { get; set; }

        //[EncryptProperty]
        public string JSONData { get; set; }

        public string BlobPointer { get; set; }

        public string Version { get; set; }
    }
}
