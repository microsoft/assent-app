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
    using Microsoft.CFS.Approvals.DevTools.Model.Constant;
    using Microsoft.CFS.Approvals.DevTools.Model.Models;
    using Microsoft.CFS.Approvals.SupportServices.Helper.ServiceHelper;

    /// <summary>
    /// The Down Time Alert Notification Controller
    /// </summary>
    [Route("api/v1/DownTimeAlertNotification/{env}")]
    [ApiController]
    public class DownTimeAlertNotificationController : ControllerBase
    {
        /// <summary>
        /// The table helper
        /// </summary>
        private readonly ITableHelper _azureTableStorageHelper;

        private readonly string _environment;

        /// <summary>
        /// Constructor of DownTimeAlertNotificationController
        /// </summary>
        /// <param name="azureTableStorageHelper"></param>
        /// <param name="configurationHelper"></param>
        /// <param name="actionContextAccessor"></param>
        public DownTimeAlertNotificationController(
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
        /// Get tenant down time messages list
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        public IActionResult Get()
        {
            try
            {
                List<TenantDownTimeMessageEntity> tenantDownTimeMessage = _azureTableStorageHelper.GetTableEntity<TenantDownTimeMessageEntity>("TenantDownTimeMessages").OrderBy(m => m.EventStartTime).ToList();
                return Ok(tenantDownTimeMessage);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        /// <summary>
        /// Delete tenant down time message by ID
        /// </summary>
        /// <param name="rowID"></param>
        /// <returns></returns>
        [HttpDelete]
        public IActionResult Delete(string rowID)
        {
            try
            {
                var tenantDownTimeMessage = _azureTableStorageHelper.GetTableEntityByRowKey<TenantDownTimeMessageEntity>("TenantDownTimeMessages", rowID);
                _azureTableStorageHelper.DeleteRow<TenantDownTimeMessageEntity>("TenantDownTimeMessages", tenantDownTimeMessage);
                return Ok("Notification deleted successfully");
            }
            catch (Exception ex)
            {
                return BadRequest(string.Format("Deletion unsuccessful : {0}", ex.Message));
            }
        }

        /// <summary>
        /// Insert or Update Tenant down time message
        /// </summary>
        /// <param name="downTimeMessage"></param>
        /// <returns></returns>
        [HttpPost]
        public IActionResult Post([FromBody] TenantDownTimeMessageEntity downTimeMessage)
        {
            try
            {
                if (downTimeMessage.EventStartTime < DateTime.UtcNow ||
                     downTimeMessage.EventEndTime < DateTime.UtcNow)
                {
                    return BadRequest(Constants.CurrentDateValidationMessage);
                }
                if (downTimeMessage.EventStartTime > downTimeMessage.EventEndTime)
                {
                    return BadRequest(Constants.DateTimeRangeValidationMessage);
                }
                if (!string.IsNullOrWhiteSpace(downTimeMessage.RowKey))
                {
                    var tenantDownTimeMessage = _azureTableStorageHelper.GetTableEntityByRowKey<TenantDownTimeMessageEntity>("TenantDownTimeMessages", downTimeMessage.RowKey);
                    if (tenantDownTimeMessage != null)
                        _azureTableStorageHelper.DeleteRow<TenantDownTimeMessageEntity>("TenantDownTimeMessages", tenantDownTimeMessage);
                }

                downTimeMessage.RowKey = Guid.NewGuid().ToString();
                downTimeMessage.CreatedDate = DateTime.UtcNow;

                _azureTableStorageHelper.InsertOrReplace<TenantDownTimeMessageEntity>("TenantDownTimeMessages", downTimeMessage);
                return Ok();
            }
            catch (Exception ex)
            {
                return BadRequest(string.Format("Insert or Update failed : {0}", ex.Message));
            }
        }
    }
}