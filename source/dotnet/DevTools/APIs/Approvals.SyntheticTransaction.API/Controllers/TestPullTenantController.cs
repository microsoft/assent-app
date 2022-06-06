// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace Microsoft.CFS.Approvals.SyntheticTransaction.API.Controllers
{
    using System;
    using System.Linq;
    using System.Threading.Tasks;
    using Microsoft.AspNetCore.Mvc;
    using Microsoft.AspNetCore.Mvc.Infrastructure;
    using Microsoft.CFS.Approvals.Data.Azure.Storage.Interface;
    using Microsoft.CFS.Approvals.SyntheticTransaction.Common.Models;
    using Microsoft.CFS.Approvals.SyntheticTransaction.Helpers.ExtensionMethods;
    using Microsoft.CFS.Approvals.SyntheticTransaction.Helpers.Helpers;

    /// <summary>
    /// The Pull Tenant Controller
    /// </summary>
    [Route("{env}/api/TestPullTenant")]
    [ApiController]
    public class TestPullTenantController : ControllerBase
    {
        /// <summary>
        /// The azure storage helper
        /// </summary>
        private readonly ITableHelper _azureStorageHelper;

        private readonly string _environment;

        /// <summary>
        /// Constructor of TestPullTenantController
        /// </summary>
        /// <param name="azureStorageHelper"></param>
        /// <param name="actionContextAccessor"></param>
        /// <param name="configurationSetting"></param>
        public TestPullTenantController(Func<string, string, ITableHelper> azureStorageHelper,
            IActionContextAccessor actionContextAccessor,
            ConfigurationSetting configurationSetting)
        {
            _environment = actionContextAccessor?.ActionContext?.RouteData?.Values["env"]?.ToString();
            _azureStorageHelper = azureStorageHelper(
                configurationSetting.appSettings[_environment].StorageAccountName,
                configurationSetting.appSettings[_environment].StorageAccountKey);
        }

        /// <summary>
        /// Get tenant summary data
        /// </summary>
        /// <param name="alias"></param>
        /// <param name="tenantId"></param>
        /// <returns></returns>
        [Route("{alias}/{tenantId}")]
        public async Task<IActionResult> Get(string alias, string tenantId)
        {
            try
            {
                var summaryData = (_azureStorageHelper.GetTableEntityListByPartitionKey<TestTenantSummaryEntity>("TenantSummaryData", alias)).Where(x => x.TenantID == tenantId);

                if (summaryData == null)
                {
                    return BadRequest("No Data Found");
                }

                var ApprovalSummaryData = (from record in summaryData
                                           select record.JsonData.ToJObject());
                var responseContent = new { response = new { ApprovalSummaryData } }.ToJToken();

                return Ok(responseContent);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }
    }
}