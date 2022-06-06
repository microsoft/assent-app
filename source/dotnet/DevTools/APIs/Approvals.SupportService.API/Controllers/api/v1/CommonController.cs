// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace Microsoft.CFS.Approvals.SupportService.API.Controllers.api.v1
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using Microsoft.AspNetCore.Mvc;
    using Microsoft.AspNetCore.Mvc.Infrastructure;
    using Microsoft.CFS.Approvals.Data.Azure.Storage.Interface;
    using Microsoft.CFS.Approvals.DevTools.Model.Models;
    using Microsoft.CFS.Approvals.SupportServices.Helper.ServiceHelper;
    using Microsoft.Extensions.Configuration;

    /// <summary>
    /// The Common Controller
    /// </summary>
    [Route("api/v1/Common")]
    [ApiController]
    public class CommonController : ControllerBase
    {
        /// <summary>
        /// The table helper
        /// </summary>
        private readonly ITableHelper _azureTableStorageHelper;

        /// <summary>
        /// The configuration
        /// </summary>
        private readonly IConfiguration _configuration;

        private readonly string _environment;

        /// <summary>
        /// Constructor of CommonController
        /// </summary>
        /// <param name="azureTableStorageHelper"></param>
        /// <param name="configurationHelper"></param>
        /// <param name="actionContextAccessor"></param>
        /// <param name="configuration"></param>
        public CommonController(Func<string, string, ITableHelper> azureTableStorageHelper,
            ConfigurationHelper configurationHelper,
            IActionContextAccessor actionContextAccessor,
            IConfiguration configuration)
        {
            _environment = actionContextAccessor?.ActionContext?.RouteData?.Values["env"]?.ToString() ?? string.Empty;
            _azureTableStorageHelper = azureTableStorageHelper(
                 (configurationHelper.appSettings.ContainsKey(_environment) ? configurationHelper.appSettings[_environment].StorageAccountName : string.Empty),
                 (configurationHelper.appSettings.ContainsKey(_environment) ? configurationHelper.appSettings[_environment].StorageAccountKey : string.Empty));
            _configuration = configuration;
        }

        /// <summary>
        /// Get list of tenant
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        [Route("GetTenant/{env}")]
        public IActionResult GetTenant()
        {
            try
            {
                List<TenantEntity> result = _azureTableStorageHelper.GetTableEntity<TenantEntity>("ApprovalTenantInfo").ToList();
                return Ok(result.OrderBy(x => x.AppName));
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        /// <summary>
        /// Get list of environment
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        [Route("GetEnvironment")]
        public IActionResult GetEnvironments()
        {
            List<string> envNames = new List<string>();
            var environmentNames = _configuration["Environmentlist"];
            envNames = environmentNames.Split(',').ToList();

            return Ok(envNames);
        }
    }
}