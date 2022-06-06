// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace Microsoft.CFS.Approvals.CoreServices.Controllers.api.v1
{
    using System;
    using System.Collections.Generic;
    using Microsoft.AspNetCore.Mvc;
    using Microsoft.CFS.Approvals.Common.DL.Interface;
    using Microsoft.CFS.Approvals.Contracts;
    using Microsoft.CFS.Approvals.CoreServices.BL.Interface;
    using Microsoft.CFS.Approvals.Extensions;
    using Microsoft.CFS.Approvals.LogManager.Model;
    using Microsoft.CFS.Approvals.LogManager.Provider.Interface;
    using Swashbuckle.AspNetCore.Annotations;

    /// <summary>
    /// Class AboutController.
    /// </summary>
    /// <seealso cref="BaseApiController" />
    public class AboutController : BaseApiController
    {
        /// <summary>
        /// The about helper
        /// </summary>
        private readonly IAboutHelper _aboutHelper = null;

        /// <summary>
        /// The performance logger
        /// </summary>
        private readonly IPerformanceLogger _performanceLogger = null;

        /// <summary>
        /// The log provider
        /// </summary>
        private readonly ILogProvider _logProvider = null;

        /// <summary>
        /// Initializes a new instance of the <see cref="AboutController"/> class.
        /// </summary>
        /// <param name="aboutHelper">The about helper.</param>
        /// <param name="performanceLogger">The performance logger</param>
        /// <param name="logProvider">The log provider</param>
        public AboutController(IAboutHelper aboutHelper, IPerformanceLogger performanceLogger, ILogProvider logProvider)
        {
            _aboutHelper = aboutHelper;
            _performanceLogger = performanceLogger;
            _logProvider = logProvider;
        }

        // GET api/about
        /// <summary>
        /// Get information for About page
        /// </summary>
        /// <param name="sessionId">GUID SessionId. Unique for each user session</param>
        /// <returns>
        /// URLs from Configuration respective to the About page
        /// </returns>
        [SwaggerOperation(Tags = new[] { "Others" })]
        [ApiExplorerSettings(IgnoreApi = true)]
        [HttpGet]
        public IActionResult Get(string sessionId = "")
        {
            #region Logging

            var logData = new Dictionary<LogDataKey, object>
            {
                { LogDataKey.Xcv, Xcv },
                { LogDataKey.DXcv, Tcv },
                { LogDataKey.StartDateTime, DateTime.UtcNow },
                { LogDataKey.SessionId, sessionId },
                { LogDataKey.IsCriticalEvent, CriticalityLevel.No.ToString() }
            };

            #endregion Logging

            try
            {
                using (_performanceLogger.StartPerformanceLogger("PerfLog", Constants.WebClient, string.Format(Constants.PerfLogAction, "AboutController", "Get information for About page"), logData))
                {
                    var result = _aboutHelper.GetAbout(Host, sessionId, LoggedInAlias, Host, Alias);
                    logData.Modify(LogDataKey.EndDateTime, DateTime.UtcNow);
                    _logProvider.LogInformation(TrackingEvent.WebApiAboutSuccess, logData);
                    return Ok(result);
                }
            }
            catch (Exception ex)
            {
                logData.Modify(LogDataKey.EndDateTime, DateTime.UtcNow);
                _logProvider.LogError(TrackingEvent.WebApiAboutFail, ex, logData);
                return BadRequest(ex.Message);
            }
        }
    }
}