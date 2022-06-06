// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.


namespace Microsoft.CFS.Approvals.SyntheticTransaction.API.Controllers
{
    using System;
    using Microsoft.AspNetCore.Mvc;
    using Microsoft.CFS.Approvals.SyntheticTransaction.Helpers.Interface;

    /// <summary>
    /// The Harness Environment Controller
    /// </summary>
    [ApiController]
    [Route("api/v1/TestHarnessEnvironment")]
    public class TestHarnessEnvironment : ControllerBase
    {
        private readonly IKeyVaultHelper _keyVaultHelper;
        public TestHarnessEnvironment(IKeyVaultHelper keyVaultHelper)
        {
            _keyVaultHelper = keyVaultHelper;
        }

        /// <summary>
        /// Get environments
        /// </summary>
        /// <param name="env"></param>
        /// <returns></returns>
        [HttpGet]
        [Route("GetEnvironment")]
        public IActionResult GetEnvironment(string env)
        {
            try
            {
                //_keyVaultHelper.GetKeyVault(env);
                return Ok(env);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

    }
}
