// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace Microsoft.CFS.Approvals.CoreServices.Controllers.api.v1;

using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.CFS.Approvals.Common.DL.Interface;
using Microsoft.CFS.Approvals.Contracts;
using Microsoft.CFS.Approvals.CoreServices.BL.Interface;
using Microsoft.CFS.Approvals.LogManager.Model;
using Swashbuckle.AspNetCore.Annotations;

/// <summary>
/// Class QuickTourController.
/// </summary>
/// <seealso cref="BaseApiController" />
public class QuickTourController : BaseApiController
{
    /// <summary>
    /// The tenant down time messages helper
    /// </summary>
    private readonly IQuickTourHelper _quickTourHelper = null;

    /// <summary>
    /// The performance logger
    /// </summary>
    private readonly IPerformanceLogger _performanceLogger = null;

    /// <summary>
    /// Initializes a new instance of the <see cref="QuickTourController"/> class.
    /// </summary>
    /// <param name="quickTourHelper">Quick tour helper</param>
    /// <param name="performanceLogger">The performance logger</param>
    public QuickTourController(IQuickTourHelper quickTourHelper, IPerformanceLogger performanceLogger)
    {
        _quickTourHelper = quickTourHelper;
        _performanceLogger = performanceLogger;
    }

    /// <summary>
    /// HTTP GET api/v1/metadata/QuickTour
    /// Gets the specified session identifier.
    /// </summary>
    /// <param name="sessionId">The session identifier.</param>
    /// <returns>IHttpActionResult.</returns>
    [Route("/api/v1/metadata/[controller]")]
    [SwaggerOperation(Tags = new[] { "Metadata" })]
    [HttpGet]
    public async Task<IActionResult> Get(string sessionId = "")
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
            using (_performanceLogger.StartPerformanceLogger("PerfLog", Constants.WebClient, string.Format(Constants.PerfLogAction, "QuickTourController", "Get Quick Tour Features"), logData))
            {
                return Ok(new { QuickTour = await _quickTourHelper.GetAllQuickTourFeatures(sessionId, SignedInUser.UserPrincipalName, OnBehalfUser.MailNickname, ClientDevice, DomainName) });
            }
        }
        catch
        {
            return BadRequest(Constants.QuickTourAPIErrorMessage);
        }
    }
}