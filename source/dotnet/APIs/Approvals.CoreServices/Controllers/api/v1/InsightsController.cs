// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace Microsoft.CFS.Approvals.CoreServices.Controllers.api.v1;

using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.CFS.Approvals.Common.DL.Interface;
using Microsoft.CFS.Approvals.Contracts;
using Microsoft.CFS.Approvals.Core.BL.Helpers;
using Microsoft.CFS.Approvals.Core.BL.Interface;
using Microsoft.CFS.Approvals.CoreServices.BL.Interface;
using Microsoft.CFS.Approvals.Extensions;
using Microsoft.CFS.Approvals.LogManager.Model;
using Microsoft.CFS.Approvals.LogManager.Provider.Interface;
using Swashbuckle.AspNetCore.Annotations;

/// <summary>
/// Class InsightsController.
/// </summary>
/// <seealso cref="BaseApiController" />
public class InsightsController : BaseApiController
{
    private readonly IInsightsHelper _insightsHelper;
    /// <summary>
    /// The performance logger
    /// </summary>
    private readonly IPerformanceLogger _performanceLogger = null;

    /// <summary>
    /// The log provider
    /// </summary>
    private readonly ILogProvider _logProvider = null;

    /// <summary>
    /// Initializes a new instance of the <see cref="InsightsController"/> class.
    /// </summary>
    /// <param name="performanceLogger">The performance logger</param>
    /// <param name="logProvider"></param>
    /// <param name="insightsHelper"></param>
    public InsightsController(IPerformanceLogger performanceLogger, ILogProvider logProvider, IInsightsHelper insightsHelper)
    {
        _performanceLogger = performanceLogger;
        _logProvider = logProvider;
        _insightsHelper = insightsHelper;
    }

    /// <summary>
    /// HTTP GET api/v1/me/Insights
    /// Gets the specified session identifier.
    /// </summary>
    /// <param name="sessionId">The session identifier.</param>
    /// <param name="page">The page where the API is called from</param>
    /// <param name="timeperiod">The timeperiod in months.</param>
    /// <returns>IHttpActionResult.</returns>
    [Route("/api/v1/me/[controller]")]
    [HttpGet]
    public async Task<IActionResult> Get(string sessionId = "", string page = nameof(PageType.Summary), int timeperiod = 0)
    {
        #region Logging

        var logData = new Dictionary<LogDataKey, object>
        {
            { LogDataKey.Xcv, Xcv },
            { LogDataKey.DXcv, MessageId },
            { LogDataKey.StartDateTime, DateTime.UtcNow },
            { LogDataKey.SessionId, sessionId },
            { LogDataKey.IsCriticalEvent, CriticalityLevel.No.ToString() }
        };

        #endregion Logging

        try
        {
            using (_performanceLogger.StartPerformanceLogger("PerfLog", Constants.WebClient, string.Format(Constants.PerfLogAction, "InsightsController", "Get Insights"), logData))
            {
                if (page.Equals(nameof(PageType.History), StringComparison.OrdinalIgnoreCase))
                {
                    return Ok(await _insightsHelper.GetHistoryInsights(OnBehalfUser, ClientDevice, sessionId, timeperiod));
                }
                else
                {
                    return Ok(await _insightsHelper.GetSummaryInsights(SignedInUser, OnBehalfUser, GetTokenOrCookie(), ClientDevice, sessionId, Xcv, OnBehalfUser.UserPrincipalName.GetDomainFromUPN()));
                }
            }
        }
        catch (Exception ex)
        {
            return BadRequest(ex.Message);
        }
    }
}