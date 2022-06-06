// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.


namespace Microsoft.CFS.Approvals.SyntheticTransaction.API.Controllers
{
    using System;
    using Microsoft.AspNetCore.Mvc;
    using Microsoft.CFS.Approvals.SyntheticTransaction.API.Services;
    using Microsoft.CFS.Approvals.SyntheticTransaction.Common.Models;
    using Newtonsoft.Json;
    using Newtonsoft.Json.Linq;

    /// <summary>
    /// The Pull Tenant Details Controller
    /// </summary>
    [Route("api/TestPullTenantDetails")]
    [ApiController]
    public class TestPullTenantDetailsController : ControllerBase
    {
        /// <summary>
        /// The synthetic transaction helper
        /// </summary>
        private readonly ISyntheticTransactionHelper _syntheticTransactionHelper;
        public TestPullTenantDetailsController(ISyntheticTransactionHelper syntheticTransactionHelper)
        {
            _syntheticTransactionHelper = syntheticTransactionHelper;
        }

        /// <summary>
        /// Get request document details
        /// </summary>
        /// <param name="documentNumber"></param>
        /// <param name="alias"></param>
        /// <param name="tenantId"></param>
        /// <returns></returns>
        [HttpGet]
        [Route("{documentNumber}/{alias}/{tenantId}/{env}")]
        public IActionResult Get(string documentNumber, string alias, string tenantId)
        {
            var details = _syntheticTransactionHelper.GetSchemaFile(string.Format("{0}-{1}", tenantId, "Details.json")).Result;
            var Jdetails = JsonConvert.DeserializeObject<JObject>(details);
            switch ((TestPullTenant)Enum.Parse(typeof(TestPullTenant), tenantId, true))
            {
                case TestPullTenant.TestMSTime:                    
                    if (Jdetails?.SelectToken("laborId") != null)
                    {
                        Jdetails["laborId"] = documentNumber;
                    }
                    if (Jdetails?.SelectToken("userAlias") != null)
                    {
                        Jdetails["userAlias"] = alias;
                    }
                    break;
                default:
                    break;
            }
            return Ok(Jdetails.ToString());
        }
    }
}
