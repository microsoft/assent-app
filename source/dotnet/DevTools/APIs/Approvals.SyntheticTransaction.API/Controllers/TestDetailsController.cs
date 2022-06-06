// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace Microsoft.CFS.Approvals.SyntheticTransaction.API.Controllers
{
    using System;
    using System.Threading.Tasks;
    using Microsoft.AspNetCore.Mvc;
    using Microsoft.AspNetCore.Mvc.Infrastructure;
    using Microsoft.CFS.Approvals.Data.Azure.Storage.Interface;
    using Microsoft.CFS.Approvals.SyntheticTransaction.Common.Models;
    using Microsoft.CFS.Approvals.SyntheticTransaction.Helpers.ExtensionMethods;
    using Microsoft.CFS.Approvals.SyntheticTransaction.Helpers.Helpers;
    using Newtonsoft.Json.Linq;

    /// <summary>
    /// The Details Controller
    /// </summary>
    [Route("api/TestDetails/{env}")]
    [ApiController]
    public class TestDetailsController : ControllerBase
    {
        private readonly ITableHelper _azureStorageHelper;
        private readonly string _environment;

        /// <summary>
        /// Constructor of TestDetailsController
        /// </summary>
        /// <param name="azureStorageHelper"></param>
        /// <param name="configurationSetting"></param>
        /// <param name="actionContextAccessor"></param>
        public TestDetailsController(
            Func<string, string, ITableHelper> azureStorageHelper,
            ConfigurationSetting configurationSetting,
            IActionContextAccessor actionContextAccessor)
        {
            _environment = actionContextAccessor?.ActionContext?.RouteData?.Values["env"]?.ToString();
            _azureStorageHelper = azureStorageHelper(
               configurationSetting.appSettings[_environment].StorageAccountName,
               configurationSetting.appSettings[_environment].StorageAccountKey);
        }

        /// <summary>
        /// Get synthetic transaction details
        /// </summary>
        /// <param name="operation"></param>
        /// <param name="documentNumber"></param>
        /// <returns></returns>
        [HttpGet]
        [Route("{operation}/{documentNumber}")]
        public async Task<IActionResult> Get(string operation, string documentNumber)
        {
            var detail = _azureStorageHelper.GetTableEntityByPartitionKeyAndRowKey<SyntheticTransactionEntity>("SyntheticTransactionDetails", documentNumber, operation);
            var response = !string.IsNullOrWhiteSpace(detail?.JsonData.ToString()) ? detail?.JsonData.ToString().FromJson<JObject>() : new JObject();
            return Ok(response);
        }
    }
}