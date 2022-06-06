// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace Microsoft.CFS.Approvals.CoreServices.Controllers.api.v1
{
    using System;
    using Microsoft.AspNetCore.Mvc;
    using Microsoft.CFS.Approvals.CoreServices.BL.Interface;
    using Swashbuckle.AspNetCore.Annotations;

    /// <summary>
    /// Class HelpController.
    /// </summary>
    /// <seealso cref="BaseApiController" />
    public class HelpController : BaseApiController
    {
        /// <summary>
        /// The about helper
        /// </summary>
        private readonly IAboutHelper _aboutHelper;

        /// <summary>
        /// Initializes a new instance of the <see cref="HelpController"/> class.
        /// </summary>
        /// <param name="aboutHelper">The about helper.</param>
        public HelpController(IAboutHelper aboutHelper)
        {
            _aboutHelper = aboutHelper;
        }

        // GET api/help
        /// <summary>
        /// Get information for Help page
        /// </summary>
        /// <param name="sessionId">GUID SessionId. Unique for each user session</param>
        /// <returns>
        /// URLs respective to the Help page
        /// </returns>
        [SwaggerOperation(Tags = new[] { "Others" })]
        [ApiExplorerSettings(IgnoreApi = true)]
        [HttpGet]
        public IActionResult Get(string sessionId = "")
        {
            try
            {
                var responseObject = _aboutHelper.GetHelpData(Host, sessionId, LoggedInAlias, Host, Alias);
                return Ok(responseObject);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }
    }
}