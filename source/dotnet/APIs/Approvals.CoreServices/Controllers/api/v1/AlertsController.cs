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
    using Microsoft.CFS.Approvals.LogManager.Model;
    using Swashbuckle.AspNetCore.Annotations;

    /// <summary>
    /// Class AlertsController.
    /// </summary>
    /// <seealso cref="BaseApiController" />
    public class AlertsController : BaseApiController
    {
        /// <summary>
        /// The tenant down time messages helper
        /// </summary>
        private readonly ITenantDownTimeMessagesHelper _tenantDownTimeMessagesHelper = null;

        /// <summary>
        /// The performance logger
        /// </summary>
        private readonly IPerformanceLogger _performanceLogger = null;

        /// <summary>
        /// Initializes a new instance of the <see cref="AlertsController"/> class.
        /// </summary>
        /// <param name="tenantDownTimeMessagesHelper">The tenant down time messages helper.</param>
        /// <param name="performanceLogger">The performance logger</param>
        public AlertsController(ITenantDownTimeMessagesHelper tenantDownTimeMessagesHelper, IPerformanceLogger performanceLogger)
        {
            _tenantDownTimeMessagesHelper = tenantDownTimeMessagesHelper;
            _performanceLogger = performanceLogger;
        }

        /// <summary>
        /// Gets the specified session identifier.
        /// </summary>
        /// <param name="sessionId">The session identifier.</param>
        /// <returns>IHttpActionResult.</returns>
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
                using (_performanceLogger.StartPerformanceLogger("PerfLog", Constants.WebClient, string.Format(Constants.PerfLogAction, "AlertsController", "Get Alert Notification"), logData))
                {
                    return Ok(new { Alerts = _tenantDownTimeMessagesHelper.GetAllAlerts(sessionId, LoggedInAlias, Alias, Host) });
                }
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }
    }
}