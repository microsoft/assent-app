// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace Microsoft.CFS.Approvals.SyntheticTransaction.Common.Models;

using Microsoft.CFS.Approvals.Model;

/// <summary>
/// Synthetic Transaction Entity class
/// </summary>
public class SyntheticTransactionEntity : BaseTableEntity
{
    /// <summary>
    /// Constructor of SyntheticTransactionEntity
    /// </summary>
    public SyntheticTransactionEntity()
    {
    }

    public string JsonData { get; set; }
    public string Approver { get; set; }
    public string AppName { get; set; }
}