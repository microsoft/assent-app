// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace Microsoft.CFS.Approvals.SupportService.API.Controllers.api.v1
{
    using System;
    using System.Collections.Generic;
    using System.Net.Http;
    using System.Threading.Tasks;
    using Microsoft.AspNetCore.Mvc;
    using Microsoft.AspNetCore.Mvc.Infrastructure;
    using Microsoft.CFS.Approvals.SupportServices.Helper.ExtensionMethods;
    using Microsoft.CFS.Approvals.SupportServices.Helper.ServiceHelper;
    using Microsoft.CFS.Approvals.Utilities.Interface;
    using Newtonsoft.Json;
    using Newtonsoft.Json.Linq;

    /// <summary>
    /// The Payload Reprocessing Controller
    /// </summary>
    [Route("api/v1/PayloadReProcessing/{env}")]
    [ApiController]
    public class PayloadReProcessingController : ControllerBase
    {
        private readonly ConfigurationHelper _configurationHelper;
        private readonly string _environment;
        private readonly IHttpHelper _httpHelper;

        /// <summary>
        /// The Constructor of PayloadReProcessingController
        /// </summary>
        /// <param name="configurationHelper"></param>
        /// <param name="actionContextAccessor"></param>
        /// <param name="httpHelper"></param>
        public PayloadReProcessingController(
            ConfigurationHelper configurationHelper,
            IActionContextAccessor actionContextAccessor,
            IHttpHelper httpHelper)
        {
            _environment = actionContextAccessor?.ActionContext?.RouteData?.Values["env"]?.ToString();
            _configurationHelper = configurationHelper;
            _httpHelper = httpHelper;
        }

        /// <summary>
        /// Get request for payload reprocessing
        /// </summary>
        /// <param name="requestBody"></param>
        /// <returns></returns>
        [HttpGet]
        public async Task<IActionResult> Get(string requestBody)
        {
            try
            {
                JObject functionAppConfiguration = JsonConvert.DeserializeObject<JObject>(_configurationHelper.appSettings[_environment].FunctionAppConfiguration);
                var response = await _httpHelper.SendRequestAsync(
                    HttpMethod.Get,
                    functionAppConfiguration["clientID"].ToString(),
                    functionAppConfiguration["clientSecret"].ToString(),
                    functionAppConfiguration["audiance"].ToString(),
                    functionAppConfiguration["resource"].ToString(),
                    _configurationHelper.appSettings[_environment].PayloadProcessingFunctionURL,
                    null,
                    requestBody);

                var result = await response.Content.ReadAsStringAsync();
                if (response.IsSuccessStatusCode)
                {
                    return Ok(result.FromJson<List<dynamic>>());
                }
                else
                {
                    return BadRequest(result);
                }
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        /// <summary>
        /// Send request for Payload reprocessing
        /// </summary>
        /// <param name="requestBody"></param>
        /// <returns></returns>
        [HttpPost]
        public async Task<IActionResult> Post([FromBody] JObject requestBody)
        {
            try
            {
                JObject functionAppConfiguration = JsonConvert.DeserializeObject<JObject>(_configurationHelper.appSettings[_environment].FunctionAppConfiguration);

                var response = await _httpHelper.SendRequestAsync(
                    HttpMethod.Post,
                    functionAppConfiguration["clientID"].ToString(),
                    functionAppConfiguration["clientSecret"].ToString(),
                    functionAppConfiguration["audiance"].ToString(),
                    functionAppConfiguration["resource"].ToString(),
                    _configurationHelper.appSettings[_environment].PayloadProcessingFunctionURL,
                    null,
                    JsonConvert.SerializeObject(requestBody));

                if (response.IsSuccessStatusCode)
                {
                    return Ok(response);
                }
                else
                {
                    return BadRequest(await response.Content.ReadAsStringAsync());
                }
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }
    }
}