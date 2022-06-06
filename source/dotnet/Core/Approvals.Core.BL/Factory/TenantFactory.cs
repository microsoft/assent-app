// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace Microsoft.CFS.Approvals.Core.BL.Factory
{
    using System;
    using Microsoft.CFS.Approvals.Common.DL.Interface;
    using Microsoft.CFS.Approvals.Core.BL.Interface;
    using Microsoft.CFS.Approvals.Data.Azure.Storage.Interface;
    using Microsoft.CFS.Approvals.Domain.BL.Interface;
    using Microsoft.CFS.Approvals.LogManager.Provider.Interface;
    using Microsoft.CFS.Approvals.Model;
    using Microsoft.CFS.Approvals.Utilities.Interface;
    using Microsoft.Extensions.Configuration;

    public class TenantFactory : ITenantFactory
    {
        private readonly ILogProvider _logProvider;
        private readonly IPerformanceLogger _performanceLogger;
        private readonly IApprovalSummaryProvider _approvalSummaryProvider;
        private readonly IConfiguration _config;
        private readonly INameResolutionHelper _nameResolutionHelper;
        private readonly IApprovalDetailProvider _approvalDetailProvider;
        private readonly IFlightingDataProvider _flightingDataProvider;
        private readonly IApprovalHistoryProvider _approvalHistoryProvider;
        private readonly IBlobStorageHelper _blobStorageHelper;
        private readonly IAuthenticationHelper _authenticationHelper;
        private readonly IHttpHelper _httpHelper;

        public TenantFactory(
            ILogProvider logProvider,
            IPerformanceLogger performanceLogger,
            IApprovalSummaryProvider approvalSummaryProvider,
            IConfiguration config,
            INameResolutionHelper nameResolutionHelper,
            IApprovalDetailProvider approvalDetailProvider,
            IFlightingDataProvider flightingDataProvider,
            IApprovalHistoryProvider approvalHistoryProvider,
            IBlobStorageHelper blobHelper,
            IAuthenticationHelper authenticationHelper,
            IHttpHelper httpHelper)
        {
            _logProvider = logProvider;
            _performanceLogger = performanceLogger;
            _approvalSummaryProvider = approvalSummaryProvider;
            _config = config;
            _nameResolutionHelper = nameResolutionHelper;
            _approvalDetailProvider = approvalDetailProvider;
            _flightingDataProvider = flightingDataProvider;
            _approvalHistoryProvider = approvalHistoryProvider;
            _blobStorageHelper = blobHelper;
            _authenticationHelper = authenticationHelper;
            _httpHelper = httpHelper;
        }

        /// <summary>
        /// Getting Tenant object by matching its DocumentTypeId since DocumentTypeId will remain constant for a particular Tenant
        /// If this check is based on TenantID and if it is changed in ApprovalTenantInfo, this code won't work; hence using DocumentTypeId
        /// </summary>
        /// <param name="tenantInfo"></param>
        /// <returns></returns>
        public ITenant GetTenant(ApprovalTenantInfo tenantInfo)
        {
            return (ITenant)Activator.CreateInstance(Type.GetType(tenantInfo.ClassName), tenantInfo, _logProvider, _performanceLogger, _approvalSummaryProvider, _config, _nameResolutionHelper, _approvalDetailProvider, _flightingDataProvider, _approvalHistoryProvider, _blobStorageHelper, _authenticationHelper, _httpHelper);
        }

        /// <summary>
        /// Getting Tenant object by matching its DocumentTypeId since DocumentTypeId will remain constant for a particular Tenant
        /// If this check is based on TenantID and if it is changed in ApprovalTenantInfo, this code won't work; hence using DocumentTypeId
        /// </summary>
        /// <param name="tenantInfo"></param>
        /// <param name="alias"></param>
        /// <param name="clientDevice"></param>
        /// <param name="aadToken"></param>
        /// <returns></returns>
        public ITenant GetTenant(ApprovalTenantInfo tenantInfo, string alias, string clientDevice, string aadToken)
        {
            try
            {
                return (ITenant)Activator.CreateInstance(Type.GetType(tenantInfo.ClassName), tenantInfo, alias, clientDevice, aadToken, _logProvider, _performanceLogger, _approvalSummaryProvider, _config, _nameResolutionHelper, _approvalDetailProvider, _flightingDataProvider, _approvalHistoryProvider, _blobStorageHelper, _authenticationHelper, _httpHelper);
            }
            catch
            {
                throw new Exception("Tenant could not be identified. The operation cannot be completed");
            }
        }
    }
}