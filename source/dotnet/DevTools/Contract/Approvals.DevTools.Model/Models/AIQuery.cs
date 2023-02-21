// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace Microsoft.CFS.Approvals.DevTools.Model.Models;
using Microsoft.CFS.Approvals.Model;

/// <summary>
/// AI Query class
/// </summary>
public class AIQuery : BaseTableEntity
{
    /// <summary>
    /// Constructor of AIQuery
    /// </summary>
    public AIQuery()
    {

    }
    public string Query { get; set; }
    public string Title { get; set; }
}
