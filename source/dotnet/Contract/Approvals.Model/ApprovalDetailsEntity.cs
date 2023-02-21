// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace Microsoft.CFS.Approvals.Model;

public class ApprovalDetailsEntity : BaseTableEntity
{
    public int TenantID { get; set; }

//[EncryptProperty]
public string JSONData { get; set; }

public string BlobPointer { get; set; }

public string Version { get; set; }
}