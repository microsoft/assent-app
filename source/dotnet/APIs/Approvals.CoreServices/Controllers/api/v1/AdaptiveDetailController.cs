// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace Microsoft.CFS.Approvals.CoreServices.Controllers.api.v1
{
    using System;
    using System.Collections.Generic;
    using System.Threading.Tasks;
    using Microsoft.AspNetCore.Mvc;
    using Microsoft.CFS.Approvals.Common.DL.Interface;
    using Microsoft.CFS.Approvals.Contracts;
    using Microsoft.CFS.Approvals.Core.BL.Interface;
    using Microsoft.CFS.Approvals.Extensions;
    using Microsoft.CFS.Approvals.LogManager.Model;
    using Microsoft.CFS.Approvals.LogManager.Provider.Interface;
    using Microsoft.CFS.Approvals.Utilities.Helpers;
    using Swashbuckle.AspNetCore.Annotations;

    /// <summary>
    /// the Adaptive Detail Controller
    /// </summary>
    /// <seealso cref="BaseApiController" />
    [Route("api/v1/[controller]/{tenantId?}")]
    public class AdaptiveDetailController : BaseApiController
    {
        /// <summary>
        /// The Adaptive Details Helper
        /// </summary>
        private readonly IAdaptiveDetailsHelper _adaptiveDetailsHelper = null;

        /// <summary>
        /// The performance logger
        /// </summary>
        private readonly IPerformanceLogger _performanceLogger = null;

        /// <summary>
        /// The log provider
        /// </summary>
        private readonly ILogProvider _logProvider = null;

        /// <summary>
        /// Constructor for AdaptiveDetailsController
        /// </summary>
        /// <param name="adaptiveDetailsHelper">Adaptive Details Helper class</param>
        /// <param name="logProvider">Log provider</param>
        /// <param name="performanceLogger">Performance logger</param>
        public AdaptiveDetailController(IAdaptiveDetailsHelper adaptiveDetailsHelper, IPerformanceLogger performanceLogger, ILogProvider logProvider)
        {
            _adaptiveDetailsHelper = adaptiveDetailsHelper;
            _performanceLogger = performanceLogger;
            _logProvider = logProvider;
        }

        /// <summary>
        /// Retrieves adaptive card template in Dictionary format as per the template type
        /// </summary>
        /// <param name="tenantId">Indicates the Tenant for which the request's (documentNumber) data needs to be retrieved</param>
        /// <param name="sessionId">GUID SessionId. Unique for each user session</param>
        /// <param name="tcv">GUID Transaction Id. Unique for each transaction</param>
        /// <param name="xcv">GUID Cross Correlation Vector. Unique for each request across system boundaries</param>
        /// <param name="templateType">Template Type which can be Summary, Details, Action, Footer, All, Full</param>
        /// <returns>
        /// This method returns the tenant specific adaptive templatefrom storage according to TemplateType passed
        /// </returns>
        /// <remarks>
        /// <para>
        /// e.g.
        /// Gets Adaptive Templates for the tenant
        /// HTTP GET api/AdaptiveDetail/[tenantMapIdId]
        /// </para>
        /// </remarks>
        [SwaggerOperation(Tags = new[] { "Details" })]
        [HttpGet]
        public async Task<IActionResult> Get(int tenantId, string sessionId = "", string tcv = "", string xcv = "", string templateType = "Full")
        {
            #region Logging

            var logData = new Dictionary<LogDataKey, object>
            {
                { LogDataKey.Xcv, xcv },
                { LogDataKey.DXcv, tcv },
                { LogDataKey.TenantId, tenantId },
                { LogDataKey.SessionId, sessionId },
                { LogDataKey.StartDateTime, DateTime.UtcNow },
                { LogDataKey.IsCriticalEvent, CriticalityLevel.No.ToString() }
            };

            #endregion Logging

            try
            {
                using (_performanceLogger.StartPerformanceLogger("PerfLog", Constants.WebClient, string.Format(Constants.PerfLogAction, "AdaptiveDetailController", "Get Adaptive Templates for the tenant"), logData))
                {
                    ArgumentGuard.NotNull(tenantId, nameof(tenantId));

                    TemplateType type = (TemplateType)Enum.Parse(typeof(TemplateType), templateType, true);
                    var result = await _adaptiveDetailsHelper.GetAdaptiveTemplate(tenantId, Alias, LoggedInAlias, Host, GetTokenOrCookie(), sessionId, xcv, tcv, (int)type);

                    logData.Modify(LogDataKey.EndDateTime, DateTime.UtcNow);
                    _logProvider.LogInformation(TrackingEvent.WebApiAdaptiveDetailFail, logData);
                    return Ok(result);
                }
            }
            catch (Exception ex)
            {
                logData.Modify(LogDataKey.EndDateTime, DateTime.UtcNow);
                _logProvider.LogError(TrackingEvent.WebApiAdaptiveDetailSuccess, ex, logData);
                return BadRequest(ex.Message);
            }
        }
    }
}