// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace Microsoft.CFS.Approvals.Common.DL
{
    using System.Collections.Generic;
    using System.Globalization;
    using System.Linq;
    using System.Threading.Tasks;
    using Microsoft.CFS.Approvals.Common.DL.Interface;
    using Microsoft.CFS.Approvals.Contracts;
    using Microsoft.CFS.Approvals.Data.Azure.Storage.Interface;
    using Microsoft.CFS.Approvals.Extensions;
    using Microsoft.CFS.Approvals.Model;
    using Microsoft.Extensions.Configuration;

    /// <summary>
    /// The Approval TenantInfo Provider class
    /// </summary>
    public class ApprovalTenantInfoProvider : IApprovalTenantInfoProvider
    {
        /// <summary>
        /// The configuration
        /// </summary>
        private readonly IConfiguration _config;

        /// <summary>
        /// The table helper
        /// </summary>
        private readonly ITableHelper _tableHelper;

        /// <summary>
        /// The blob data provider
        /// </summary>
        private readonly IApprovalBlobDataProvider _blobDataProvider;

        /// <summary>
        /// Constructor of ApprovalTenantInfoProvider
        /// </summary>
        /// <param name="config"></param>
        /// <param name="tableHelper"></param>
        /// <param name="blobDataProvider"></param>
        public ApprovalTenantInfoProvider(IConfiguration config, ITableHelper tableHelper, IApprovalBlobDataProvider blobDataProvider)
        {
            _config = config;
            _tableHelper = tableHelper;
            _blobDataProvider = blobDataProvider;
        }

        /// <summary>
        /// Get all tenantInfo data
        /// </summary>
        /// <returns></returns>
        public async Task<IEnumerable<ApprovalTenantInfo>> GetAllTenantInfo(bool fetchImageDetails = true)
        {
            IEnumerable<ApprovalTenantInfo> jsonATIInfo = _tableHelper.GetTableEntity<ApprovalTenantInfo>(_config[ConfigurationKey.ApprovalTenantInfo.ToString()]);

            List<ApprovalTenantInfo> tenantList = jsonATIInfo.Where(x => x.TenantEnabled.Equals(true)).ToList();
            if (fetchImageDetails)
            {
                foreach (ApprovalTenantInfo tenant in tenantList)
                {
                    tenant.TenantImageDetails = tenant.TenantImage?.FromJson<TenantImageInfo>();
                    if (!string.IsNullOrWhiteSpace(tenant.TenantImageDetails?.FileName) && !string.IsNullOrWhiteSpace(tenant.TenantImageDetails?.FileType))
                    {
                        tenant.TenantImageDetails.FileBase64 = await _blobDataProvider.GetTenantImageBase64(tenant.TenantImageDetails.FileName + '.' + tenant.TenantImageDetails.FileType);
                    }
                }
            }
            return tenantList;
        }

        /// <summary>
        /// Get all the information related to tenant from azure config table(ApprovalTenantInfo)
        /// Note: DetailOperations is a list of TenantOperationDetails class
        /// </summary>
        /// <param name="tenantId">TenantId whose details information needs to be retrived</param>
        /// <returns>Object of ApprovalTenantInfo class containing data or NULL if no tenant info exists</returns>
        public ApprovalTenantInfo GetTenantInfo(int tenantId)
        {
            return _tableHelper.GetTableEntityByRowKey<ApprovalTenantInfo>(_config[ConfigurationKey.ApprovalTenantInfo.ToString()], tenantId.ToString(CultureInfo.InvariantCulture));
        }

        /// <summary>
        /// Get TenantInfo By Host
        /// </summary>
        /// <returns></returns>
        public List<ApprovalTenantInfo> GetTenantInfo()
        {
            IEnumerable<ApprovalTenantInfo> jsonATIInfo = _tableHelper.GetTableEntity<ApprovalTenantInfo>(_config[ConfigurationKey.ApprovalTenantInfo.ToString()]);

            return jsonATIInfo.ToList();
        }
    }
}