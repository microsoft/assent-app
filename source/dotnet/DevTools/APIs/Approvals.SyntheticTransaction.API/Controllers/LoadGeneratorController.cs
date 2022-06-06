// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.


namespace Microsoft.CFS.Approvals.SyntheticTransaction.API.Controllers
{
    using Microsoft.AspNetCore.Mvc;
    using Microsoft.CFS.Approvals.SyntheticTransaction.API.Services;
    using Microsoft.CFS.Approvals.SyntheticTransaction.Helpers.Interface;
    using Microsoft.Extensions.Configuration;

    /// <summary>
    /// The Load Generator Controller
    /// </summary>
    [Route("api/v1/LoadGenerator")]
    [ApiController]
    public class LoadGeneratorController : ControllerBase
    {
        /// <summary>
        /// The synthetic transaction helper
        /// </summary>
        private readonly ISyntheticTransactionHelper _syntheticTransactionHelper;

        /// <summary>
        /// The configuration helper
        /// </summary>
        private readonly IConfiguration _configuration;

        /// <summary>
        /// The load generator helper
        /// </summary>
        private readonly ILoadGeneratorHelper _loadGeneratorHelper;

        /// <summary>
        /// Constructor of LoadGeneratorController
        /// </summary>
        /// <param name="syntheticTransactionHelper"></param>
        /// <param name="configuration"></param>
        /// <param name="loadGeneratorHelper"></param>
        public LoadGeneratorController(ISyntheticTransactionHelper syntheticTransactionHelper,
            IConfiguration configuration,
            ILoadGeneratorHelper loadGeneratorHelper)
        {
            _syntheticTransactionHelper = syntheticTransactionHelper;
            _configuration = configuration;
            _loadGeneratorHelper = loadGeneratorHelper;
        }

        /// <summary>
        /// Generate load
        /// </summary>
        /// <param name="Tenant"></param>
        /// <param name="Approver"></param>
        /// <param name="Load"></param>
        /// <param name="Batchsize"></param>
        /// <returns></returns>
        [HttpPost]
        [Route("GenerateLoad/{env}")]
        public IActionResult GenerateLoad(string Tenant, string Approver, int Load, int Batchsize)
        {
            var sampleData = _syntheticTransactionHelper.GetSchemaFile(string.Format("{0}.json", Tenant)).Result;
            if (string.IsNullOrWhiteSpace(sampleData))
            {
                sampleData = _syntheticTransactionHelper.GetSchemaFile(_configuration["MasterPayload"]).Result;
            }
            if (string.IsNullOrWhiteSpace(sampleData))
                return NotFound(new { message = "Tenant configuration yet to be done. Please Configure selected tenant." });

            var result = _loadGeneratorHelper.GenerateLoad(Tenant, Approver, Load, Batchsize, sampleData);
            return Ok(result);
        }
    }
}
