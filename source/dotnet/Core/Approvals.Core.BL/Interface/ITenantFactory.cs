// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace Microsoft.CFS.Approvals.Core.BL.Interface;

using Microsoft.CFS.Approvals.Domain.BL.Interface;
using Microsoft.CFS.Approvals.Model;

public interface ITenantFactory
{
    /// <summary>
    /// Get tenant
    /// </summary>
    /// <param name="tenantInfo"></param>
    /// <returns></returns>
    ITenant GetTenant(ApprovalTenantInfo tenantInfo);

    /// <summary>
    /// Get tenant
    /// </summary>
    /// <param name="tenantInfo"></param>
    /// <param name="alias"></param>
    /// <param name="clientDevice"></param>
    /// <param name="aadToken"></param>
    /// <returns></returns>
    ITenant GetTenant(ApprovalTenantInfo tenantInfo, string alias, string clientDevice, string aadToken);
}