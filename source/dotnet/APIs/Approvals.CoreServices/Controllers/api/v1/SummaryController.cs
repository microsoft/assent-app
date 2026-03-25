// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace Microsoft.CFS.Approvals.CoreServices.Controllers.api.v1;

using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.CFS.Approvals.Common.DL.Interface;
using Microsoft.CFS.Approvals.Contracts;
using Microsoft.CFS.Approvals.Core.BL.Interface;
using Microsoft.CFS.Approvals.LogManager.Model;
using Microsoft.CFS.Approvals.LogManager.Provider.Interface;
using Newtonsoft.Json.Linq;
using Swashbuckle.AspNetCore.Annotations;

/// <summary>
/// Class SummaryController.
/// </summary>
/// <seealso cref="BaseApiController" />
public class SummaryController : BaseApiController
{
    /// <summary>
    /// The summary helper/
    /// </summary>
    private readonly ISummaryHelper _summaryHelper = null;

    /// <summary>
    /// The performance logger
    /// </summary>
    private readonly IPerformanceLogger _performanceLogger = null;

    /// <summary>
    /// The log provider
    /// </summary>
    private readonly ILogProvider _logProvider = null;

    /// <summary>
    /// Initializes a new instance of the <see cref="SummaryController"/> class.
    /// </summary>
    /// <param name="summaryHelper">The summary helper.</param>
    /// <param name="performanceLogger">The performance logger.</param>
    /// <param name="logProvider">The log provider.</param>
    public SummaryController(ISummaryHelper summaryHelper, IPerformanceLogger performanceLogger, ILogProvider logProvider)
    {
        _summaryHelper = summaryHelper;
        _performanceLogger = performanceLogger;
        _logProvider = logProvider;
    }

    /// <summary>
    /// Get summary for the current user sorted/ filtered by tenant from azure table storage given Document Type ID (optional).
    /// </summary>
    /// <param name="Id">GUID DocumentTypeId of the Tenant</param>
    /// <param name="SessionId">GUID SessionId. Unique for each user session</param>
    /// <param name="isSubmittedRequest"></param>
    /// <returns>
    /// This method returns a JSON for all the Pending approval requests for the given user and Tenant combination.
    /// This contains only summary data (i.e. no details) to be displayed on the home page.
    /// </returns>
    /// <remarks>
    /// <para>
    /// e.g.
    /// HTTP GET api/v1/Summary?Id=[tenantDocTypeID]
    /// </para>
    /// </remarks>
    [SwaggerOperation(Tags = new[] { "Summary" })]
    [HttpGet]
    public async Task<IActionResult> Get(String Id, string SessionId = "", bool isSubmittedRequest = false)
    {
        var logData = new Dictionary<LogDataKey, object>
        {
            { LogDataKey.StartDateTime, DateTime.UtcNow },
            { LogDataKey.SessionId, SessionId },
            { LogDataKey.UserRoleName, SignedInUser.UserPrincipalName },
            { LogDataKey.ClientDevice, ClientDevice },
            { LogDataKey.EventType, Constants.FeatureUsageEvent },
            { LogDataKey.UserAlias, OnBehalfUser.UserPrincipalName },
            { LogDataKey.IsCriticalEvent, CriticalityLevel.Yes.ToString() },
            { LogDataKey.IsSubmittedRequest, isSubmittedRequest }
        };

        try
        {
            if (isSubmittedRequest)
            {
                _logProvider.LogInformation(TrackingEvent.SummaryViewInitiated, logData);
            }

            using (_performanceLogger.StartPerformanceLogger("PerfLog", Constants.WebClient, string.Format(Constants.PerfLogAction, "SummaryController", isSubmittedRequest ? "GetSubmittedRequestsSummary" : "GetSummary"), logData))
            {
                var result = await _summaryHelper.GetSummary(SignedInUser, OnBehalfUser, ClientDevice, SessionId, GetTokenOrCookie(), Id, isSubmittedRequest);
                logData[LogDataKey.EndDateTime] = DateTime.UtcNow;
                if (isSubmittedRequest)
                {
                    _logProvider.LogInformation(TrackingEvent.SummaryViewSuccess, logData);
                }
                return Ok(result);
            }
        }
        catch (UnauthorizedAccessException ex)
        {
            logData[LogDataKey.EndDateTime] = DateTime.UtcNow;
            if (isSubmittedRequest)
            {
                _logProvider.LogWarning(TrackingEvent.SummaryViewFailed, logData, ex);
            }
            _logProvider.LogWarning(TrackingEvent.WebApiSummaryFail, logData, ex);
            
            return Ok(new JArray() { JObject.FromObject(new { Message = ex.Message }) });
        }
        catch (Exception ex)
        {
            logData[LogDataKey.EndDateTime] = DateTime.UtcNow;
            if (isSubmittedRequest)
            {
                _logProvider.LogWarning(TrackingEvent.SummaryViewFailed, logData, ex);
            }
            return BadRequest(ex.Message);
        }
    }
}