// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace Microsoft.CFS.Approvals.Model;

using global::Azure.Data.Tables;

/// <summary>
/// The Submission Summary Row class
/// </summary>
public class SubmissionSummaryRow : BaseTableEntity
{
    public string ApprovalSummaryPartitionKey { get; set; }
    public string ApprovalSummaryRowKey { get; set; }
}
