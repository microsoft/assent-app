// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace Microsoft.CFS.Approvals.SyntheticTransaction.Common.Models;

using Microsoft.CFS.Approvals.Model;

/// <summary>
/// Tenant Entity class
/// </summary>
public class TenantEntity : BaseTableEntity
{
    /// <summary>
    /// Constructor of TenantEntity
    /// </summary>
    /// <param name="applicationID"></param>
    /// <param name="tenantNumber"></param>
    public TenantEntity(string applicationID, string tenantNumber)
    {
        PartitionKey = applicationID;
        RowKey = tenantNumber;
    }

    /// <summary>
    /// Constructor of TenantEntity
    /// </summary>
    public TenantEntity()
    { }

    public string AppName { get; set; }
    public string DocTypeId { get; set; }
    public string DocumentNumberPrefix { get; set; }
    public string TenantActionDetails { get; set; }
    public string TemplateName { get; set; }
    public string BusinessProcessName { get; set; }
    public bool IsPullModelEnabled { get; set; }
    public bool IsExternalTenantActionDetails { get; set; }
}