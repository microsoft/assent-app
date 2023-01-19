// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace Microsoft.CFS.Approvals.DevTools.Model.Models;

using Microsoft.CFS.Approvals.Model;

/// <summary>
/// Summary Entity class
/// </summary>
public class SummaryEntity: BaseTableEntity
{
    /// <summary>
    /// Constructor of SummaryEntity
    /// </summary>
    public SummaryEntity()
    {

    }
    public string Application { get; set; }
    public string Approver { get; set; }
    public string Requestor { get; set; }
    public string DocumentNumber { get; set; }
    public bool LastFailed { get; set; }
    public bool IsOutOfSyncChallenged { get; set; }
}
