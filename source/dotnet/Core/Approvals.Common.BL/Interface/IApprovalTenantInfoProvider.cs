// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace Microsoft.CFS.Approvals.Common.DL.Interface
{
    using System.Collections.Generic;
    using System.Threading.Tasks;
    using Microsoft.CFS.Approvals.Model;

    public interface IApprovalTenantInfoProvider
    {
        /// <summary>
        /// Get all tenant info
        /// </summary>
        /// <returns></returns>
        Task<IEnumerable<ApprovalTenantInfo>> GetAllTenantInfo(bool fetchImageDetails = true);

        /// <summary>
        /// Get list of tenant info
        /// </summary>
        /// <returns></returns>
        List<ApprovalTenantInfo> GetTenantInfo();

        /// <summary>
        /// Get tenant info by tenantId
        /// </summary>
        /// <param name="tenantId"></param>
        /// <returns></returns>
        ApprovalTenantInfo GetTenantInfo(int tenantId);
    }
}