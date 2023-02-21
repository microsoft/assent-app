// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace Microsoft.CFS.Approvals.DevTools.Model.Models;
using Microsoft.CFS.Approvals.Model;

/// <summary>
/// Approval Detail Entity class
/// </summary>
public class ApprovalDetailEntity : BaseTableEntity
{
    public string JSONData { get; set; }
    public int TenantID { get; set; }
}
