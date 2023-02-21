// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace Microsoft.CFS.Approvals.SyntheticTransaction.Common.Models;

using System;
using Microsoft.CFS.Approvals.Model;

/// <summary>
/// Summary Entity class
/// </summary>
public class SummaryEntity : BaseTableEntity
{
    /// <summary>
    /// Constructor of SummaryEntity
    /// </summary>
    public SummaryEntity()
    {
    }

    public string DocumentNumber { get; set; }
    public string SummaryJson { get; set; }
    public DateTime Timestamp { get; set; }
}