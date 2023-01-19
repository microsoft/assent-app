// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace Microsoft.CFS.Approvals.DevTools.Model.Models;

using Microsoft.CFS.Approvals.Model;

/// <summary>
/// Schema Entity class
/// </summary>
public class SchemaEntity : BaseTableEntity
{
    /// <summary>
    /// Constructor of SchemaEntity
    /// </summary>
    public SchemaEntity()
    {

    }
    public string Schema { get; set; }
}
