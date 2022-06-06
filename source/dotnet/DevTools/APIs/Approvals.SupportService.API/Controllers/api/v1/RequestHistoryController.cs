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
    using Microsoft.CFS.Approvals.SupportServices.Helper.Interface;
    using Microsoft.CFS.Approvals.SupportServices.Helper.ServiceHelper;
    using Newtonsoft.Json;
    using Newtonsoft.Json.Linq;

    /// <summary>
    /// The Request History Contorller
    /// </summary>
    [Route("api/v1/RequestHistory")]
    [ApiController]
    public class RequestHistoryController : ControllerBase
    {
        /// <summary>
        /// The table helper
        /// </summary>
        private readonly ITableHelper _azureTableStorageHelper;

        /// <summary>
        /// The application insights helper
        /// </summary>
        private readonly IAIHelper _aIHelper;

        private readonly ConfigurationHelper _configurationHelper;
        private readonly string _environment;

        /// <summary>
        /// Constructor of RequestHistoryController
        /// </summary>
        /// <param name="azureTableStorageHelper"></param>
        /// <param name="aIHelper"></param>
        /// <param name="configurationHelper"></param>
        /// <param name="actionContextAccessor"></param>
        public RequestHistoryController(Func<string, string, ITableHelper> azureTableStorageHelper,
            IAIHelper aIHelper,
            ConfigurationHelper configurationHelper,
            IActionContextAccessor actionContextAccessor)
        {
            _environment = actionContextAccessor?.ActionContext?.RouteData?.Values["env"]?.ToString();

            _configurationHelper = configurationHelper;
            _azureTableStorageHelper = azureTableStorageHelper(
                configurationHelper.appSettings[_environment].StorageAccountName,
                configurationHelper.appSettings[_environment].StorageAccountKey);
            _aIHelper = aIHelper;
        }

        /// <summary>
        /// Get request history data
        /// </summary>
        /// <param name="form"></param>
        /// <param name="requestType"></param>
        /// <param name="scope"></param>
        /// <returns></returns>
        [HttpPost]
        [Route("SearchRequestHistory/{env}")]
        public IActionResult Post([FromBody] string form, string requestType, string scope)
        {
            if (string.IsNullOrEmpty(scope))
            {
                return BadRequest("please select valid scope from the list.");
            }
            JArray historyData = new JArray();
            List<JToken> aiScope = JsonConvert.DeserializeObject<JArray>(_configurationHelper.appSettings[_environment].AIScope).Where(s => scope.Contains(s?.SelectToken("scopeName").ToString())).ToList();
            var query = _azureTableStorageHelper.GetTableEntityListByPartitionKey<AIQuery>("AIQueryConfiguration", requestType);
            var Inputs = JsonConvert.DeserializeObject<JObject>(form);
            if (Inputs != null)
            {
                foreach (JToken token in Inputs.Children())
                {
                    var key = ((JProperty)token.AsJEnumerable()).Name;
                    var jdata = ((JProperty)token).Value;
                    var placehodler = string.Format("#{0}#", key.Trim());
                    query.ForEach(s => s.Query = s.Query.Replace(placehodler, jdata?.ToString(), StringComparison.OrdinalIgnoreCase));
                }
                foreach (AIQuery aIQuery in query)
                {
                    var aiResponse = _aIHelper.GetAIData(aIQuery.Query, aiScope);
                    var appInsightsResponse = aiResponse?.SelectToken("tables");
                    if (appInsightsResponse != null)
                    {
                        foreach (JToken item in appInsightsResponse.Children())
                        {
                            item["title"] = aIQuery.Title;
                        }

                        historyData.Add(aiResponse);
                    }
                }
            }
            return Ok(historyData);
        }
    }
}