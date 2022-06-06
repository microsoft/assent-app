// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace Microsoft.CFS.Approvals.SupportService.API.Controllers.api.v1
{
    using System;
    using Microsoft.AspNetCore.Mvc;
    using Microsoft.AspNetCore.Mvc.Infrastructure;
    using Microsoft.CFS.Approvals.Data.Azure.Storage.Interface;
    using Microsoft.CFS.Approvals.DevTools.Model.Models;
    using Microsoft.CFS.Approvals.SupportServices.Helper.ServiceHelper;
    using Newtonsoft.Json;
    using Newtonsoft.Json.Linq;

    /// <summary>
    /// The TenantInfo Controller
    /// </summary>
    [Route("api/v1/TenantInfo")]
    [ApiController]
    public class TenantInfoController : ControllerBase
    {
        /// <summary>
        /// The table helper
        /// </summary>
        private readonly ITableHelper _azureTableStorageHelper;

        private readonly string _environment;

        /// <summary>
        /// Constructor of TenantInfoController
        /// </summary>
        /// <param name="azureTableStorageHelper"></param>
        /// <param name="configurationHelper"></param>
        /// <param name="actionContextAccessor"></param>
        public TenantInfoController(
            Func<string, string, ITableHelper> azureTableStorageHelper,
            ConfigurationHelper configurationHelper,
            IActionContextAccessor actionContextAccessor)
        {
            _environment = actionContextAccessor?.ActionContext?.RouteData?.Values["env"]?.ToString();
            _azureTableStorageHelper = azureTableStorageHelper(
                configurationHelper.appSettings[_environment].StorageAccountName,
                configurationHelper.appSettings[_environment].StorageAccountKey);
        }

        /// <summary>
        /// Update action detail
        /// </summary>
        /// <param name="body"></param>
        /// <param name="tenantID"></param>
        /// <returns></returns>
        [HttpPost]
        [Route("UpdateActionDetail/{env}")]
        public IActionResult UpdateActionDetail([FromBody] JObject body, string tenantID)
        {
            try
            {
                var tenant = _azureTableStorageHelper.GetTableEntityByRowKey<TenantEntity>("ApprovalTenantInfo", tenantID);
                var tenantDetails = body.SelectToken("tenantDetails");
                tenant.TenantActionDetails = JsonConvert.SerializeObject(tenantDetails);
                var result = _azureTableStorageHelper.InsertOrReplace<TenantEntity>("ApprovalTenantInfo", tenant);
                return Ok();
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }
    }
}