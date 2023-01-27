// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace Microsoft.CFS.Approvals.CoreServices.Controllers.api.v1;

using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.CFS.Approvals.Common.DL.Interface;
using Microsoft.CFS.Approvals.Contracts;
using Microsoft.CFS.Approvals.CoreServices.BL.Interface;
using Microsoft.CFS.Approvals.Extensions;
using Microsoft.CFS.Approvals.LogManager.Model;
using Microsoft.CFS.Approvals.LogManager.Provider.Interface;
using Microsoft.CFS.Approvals.Model;
using Swashbuckle.AspNetCore.Annotations;

/// <summary>
/// The UserPreferenceController class
/// </summary>
/// <seealso cref="BaseApiController" />
public class UserPreferenceController : BaseApiController
{
    /// <summary>
    /// The user preference helper
    /// </summary>
    private readonly IUserPreferenceHelper _userPreferenceHelper = null;

    /// <summary>
    /// The performance logger
    /// </summary>
    private readonly IPerformanceLogger _performanceLogger = null;

    /// <summary>
    /// The log provider
    /// </summary>
    private readonly ILogProvider _logProvider = null;

    /// <summary>
    /// Initializes a new instance of the <see cref="UserPreferenceController"/> class.
    /// </summary>
    /// <param name="userPreferenceHelper"></param>
    /// <param name="performanceLogger"></param>
    /// <param name="logProvider"></param>
    public UserPreferenceController(IUserPreferenceHelper userPreferenceHelper, IPerformanceLogger performanceLogger, ILogProvider logProvider)
    {
        _userPreferenceHelper = userPreferenceHelper;
        _performanceLogger = performanceLogger;
        _logProvider = logProvider;
    }

    /// <summary>
    /// HTTP GET api/UserPreference
    /// </summary>
    /// <param name="sessionId">Session Id </param>
    /// <returns>Http action result</returns>
    [SwaggerOperation(Tags = new[] { "User" })]
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
            using (_performanceLogger.StartPerformanceLogger("PerfLog", Constants.WebClient, string.Format(Constants.PerfLogAction, "UserPreferenceController", "Get User Preference"), logData))
            {
                var responseObject = _userPreferenceHelper.GetUserPreferences(LoggedInAlias.ToLowerInvariant(), Host);
                logData.Modify(LogDataKey.EndDateTime, DateTime.UtcNow);
                _logProvider.LogInformation(TrackingEvent.WebApiUserPreferenceSuccess, logData);
                return Ok(responseObject);
            }
        }
        catch (Exception ex)
        {
            logData.Modify(LogDataKey.EndDateTime, DateTime.UtcNow);
            _logProvider.LogError(TrackingEvent.WebApiUserPreferenceFail, ex, logData);
            return BadRequest(Constants.UserPreferenceGetError);
        }
    }

    /// <summary>
    /// HTTP POST api/UserPreference
    /// </summary>
    /// <param name="sessionId">Session Id</param>
    /// <returns>Http action result</returns>
    [SwaggerOperation(Tags = new[] { "User" })]
    [HttpPost]
    public async Task<IActionResult> Post(string sessionId = "")
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
            using (_performanceLogger.StartPerformanceLogger("PerfLog", Constants.WebClient, string.Format(Constants.PerfLogAction, "UserPreferenceController", "Post User Preference"), logData))
            {
                string jsonData;
                using (var reader = new StreamReader(Request.Body))
                {
                    jsonData = await reader.ReadToEndAsync();
                }
                var userPreference = jsonData.FromJson<UserPreference>();
                bool isPreferenceUpdateSuccess = _userPreferenceHelper.AddUpdateUserPreference(userPreference, LoggedInAlias.ToLowerInvariant(), Host);

                if (isPreferenceUpdateSuccess)
                {
                    logData.Modify(LogDataKey.EndDateTime, DateTime.UtcNow);
                    _logProvider.LogInformation(TrackingEvent.WebApiAboutSuccess, logData);
                    return Ok();
                }
                else
                {
                    throw new InvalidOperationException(Constants.UserPreferencePostError);
                }
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