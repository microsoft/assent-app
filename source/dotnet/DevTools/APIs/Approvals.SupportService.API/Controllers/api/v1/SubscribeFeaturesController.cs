// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace Microsoft.CFS.Approvals.SupportService.API.Controllers.api.v1
{
    using System;
    using System.Linq;
    using Microsoft.AspNetCore.Mvc;
    using Microsoft.AspNetCore.Mvc.Infrastructure;
    using Microsoft.CFS.Approvals.Data.Azure.Storage.Interface;
    using Microsoft.CFS.Approvals.DevTools.Model.Models;
    using Microsoft.CFS.Approvals.SupportServices.Helper.Interface;
    using Microsoft.CFS.Approvals.SupportServices.Helper.ServiceHelper;
    using Newtonsoft.Json.Linq;

    /// <summary>
    /// The Subscribe Features Controller
    /// </summary>
    [Route("api/v1/SubscribeFeatures")]
    [ApiController]
    public class SubscribeFeaturesController : ControllerBase
    {
        /// <summary>
        /// The table helper
        /// </summary>
        private readonly ITableHelper _azureTableStorageHelper;

        /// <summary>
        /// The subscribe features helper
        /// </summary>
        private readonly ISubscribeFeaturesHelper _subscribeFeaturesHelper;

        private readonly string _environment;

        /// <summary>
        /// Constructor of SubscribeFeaturesController
        /// </summary>
        /// <param name="azureTableStorageHelper"></param>
        /// <param name="configurationHelper"></param>
        /// <param name="actionContextAccessor"></param>
        /// <param name="subscribeFeaturesHelper"></param>
        public SubscribeFeaturesController(
            Func<string, string, ITableHelper> azureTableStorageHelper,
            ConfigurationHelper configurationHelper,
            IActionContextAccessor actionContextAccessor,
            ISubscribeFeaturesHelper subscribeFeaturesHelper)
        {
            _environment = actionContextAccessor?.ActionContext?.RouteData?.Values["env"]?.ToString();
            _azureTableStorageHelper = azureTableStorageHelper(
                configurationHelper.appSettings[_environment].StorageAccountName,
                configurationHelper.appSettings[_environment].StorageAccountKey);
            _subscribeFeaturesHelper = subscribeFeaturesHelper;
        }

        /// <summary>
        /// Get flighting feature entities
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        [Route("{env}")]
        public IActionResult Get()
        {
            try
            {
                var result = _azureTableStorageHelper.GetTableEntity<FlightingFeatureEntity>("FlightingFeature").Where(c => c.FeatureStatusID == 3).ToList();
                return Ok(result);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        /// <summary>
        /// Manage feature subscription
        /// </summary>
        /// <param name="body"></param>
        /// <returns></returns>
        [HttpPost]
        [Route("{env}")]
        public IActionResult Post([FromBody] JObject body)
        {
            try
            {
                var data = body.SelectToken("data");
                var result = _subscribeFeaturesHelper.ManageFeatureSubscription(data);
                return Ok(result);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }
    }
}