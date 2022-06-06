// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace Microsoft.CFS.Approvals.SyntheticTransaction.API.Controllers
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;
    using Microsoft.AspNetCore.Mvc;
    using Microsoft.AspNetCore.Mvc.Infrastructure;
    using Microsoft.CFS.Approvals.Contracts;
    using Microsoft.CFS.Approvals.Data.Azure.Storage.Interface;
    using Microsoft.CFS.Approvals.Model;
    using Microsoft.CFS.Approvals.SyntheticTransaction.Common.Models;
    using Microsoft.CFS.Approvals.SyntheticTransaction.Helpers.Helpers;

    using Microsoft.Extensions.Configuration;

    /// <summary>
    /// The Common controller
    /// </summary>
    [Route("api/v1/Common")]
    [ApiController]
    public class CommonController : ControllerBase
    {
        /// <summary>
        /// The table storage helper
        /// </summary>
        private readonly ITableHelper _azureStorageHelper;

        /// <summary>
        /// The configuration helper
        /// </summary>
        private readonly IConfiguration _configuration;

        /// <summary>
        /// Environment
        /// </summary>
        private readonly string _environment;

        /// <summary>
        /// Construction of CommonController
        /// </summary>
        /// <param name="configuration"></param>
        /// <param name="azureStorageHelper"></param>
        /// <param name="actionContextAccessor"></param>
        /// <param name="configurationSetting"></param>
        public CommonController(
            IConfiguration configuration,
            Func<string, string, ITableHelper> azureStorageHelper,
            IActionContextAccessor actionContextAccessor,
            ConfigurationSetting configurationSetting)
        {
            _environment = actionContextAccessor?.ActionContext?.RouteData?.Values["env"]?.ToString() ?? string.Empty;
            _configuration = configuration;
            _azureStorageHelper = azureStorageHelper(
                (configurationSetting.appSettings.ContainsKey(_environment) ? configurationSetting?.appSettings[_environment]?.StorageAccountName : string.Empty),
                (configurationSetting.appSettings.ContainsKey(_environment) ? configurationSetting?.appSettings[_environment]?.StorageAccountKey : string.Empty));
        }

        /// <summary>
        /// Get environments
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        [Route("GetEnvironment")]
        public IActionResult GetEnvironments()
        {
            var environmentNames = _configuration["Environmentlist"];
            List<string> envNames = environmentNames.Split(',').ToList();

            return Ok(envNames);
        }

        /// <summary>
        /// Get select options
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        [Route("GetSelectOptions/{env}")]
        public async Task<IActionResult> GetSelectOptions()
        {
            try
            {
                var tenantEntities = new List<ApprovalTenantInfo>();
                tenantEntities.AddRange((_azureStorageHelper.GetTableEntity<ApprovalTenantInfo>("ApprovalTenantInfo")).Where(t => t.AppName.ToString().StartsWith("Test")).OrderBy(x => x.AppName.ToString()));

                var approverList = _azureStorageHelper.GetTableEntityByPartitionKey<ConfigurationKeys>("ConfigurationKeys", ConfigurationKey.SyntheticTransactionsApproverAliasList.ToString())?.KeyValue;
                return Ok(new { tenantEntities, approverList });
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }
    }
}