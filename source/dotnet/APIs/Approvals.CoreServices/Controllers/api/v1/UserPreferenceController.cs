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
[Route("api/v1/user/preferences")]
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
    /// HTTP GET api/v1/user/preferences
    /// </summary>
    /// <param name="sessionId">session Id </param>
    /// <returns>Http action result</returns>
    [SwaggerOperation(Tags = new[] { "User" })]
    [HttpGet]
    public IActionResult Get(string sessionId = "")
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
            using (_performanceLogger.StartPerformanceLogger("PerfLog", Constants.WebClient, string.Format(Constants.PerfLogAction, "UserPreferenceController", "Get User Preference"), logData))
            {
                var responseObject = _userPreferenceHelper.GetUserPreferences(SignedInUser.UserPrincipalName.ToLowerInvariant(), ClientDevice);
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
    /// HTTP POST api/v1/user/preferences
    /// </summary>
    /// <param name="sessionId">session Id</param>
    /// <returns>Http action result</returns>
    [SwaggerOperation(Tags = new[] { "User" })]
    [HttpPost]
    public async Task<IActionResult> Post(string sessionId = "")
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
            using (_performanceLogger.StartPerformanceLogger("PerfLog", Constants.WebClient, string.Format(Constants.PerfLogAction, "UserPreferenceController", "Post User Preference"), logData))
            {
                string jsonData;
                using (var reader = new StreamReader(Request.Body))
                {
                    jsonData = await reader.ReadToEndAsync();
                }
                var userPreference = jsonData.FromJson<UserPreference>();
                bool isPreferenceUpdateSuccess = _userPreferenceHelper.AddUpdateUserPreference(userPreference, SignedInUser.UserPrincipalName.ToLowerInvariant(), ClientDevice, sessionId);

                if (isPreferenceUpdateSuccess)
                    return Ok();
                else
                    throw new InvalidOperationException(Constants.UserPreferencePostError);
            }
        }
        catch (Exception ex)
        {
            logData.Modify(LogDataKey.EndDateTime, DateTime.UtcNow);
            _logProvider.LogError(TrackingEvent.WebApiUserPreferenceFail, ex, logData);
            return BadRequest(Constants.UserPreferencePostError);
        }
    }

    /// <summary>
    /// HTTP PUT api/v1/user/preferences?userPreferenceColumn= for updating specific user preference data
    /// </summary>
    /// <param name="userPreferenceColumn"></param>
    /// <param name="sessionId"></param>
    /// <returns></returns>
    [SwaggerOperation(Tags = new[] { "User" })]
    [HttpPut]
    public async Task<IActionResult> Put(string userPreferenceColumn, string sessionId = "")
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
            string userPreferenceData;
            using (var reader = new StreamReader(Request.Body))
            {
                userPreferenceData = await reader.ReadToEndAsync();
            }
            using (_performanceLogger.StartPerformanceLogger("PerfLog", Constants.WebClient, string.Format(Constants.PerfLogAction, "UserPreferenceController", "Post User Preference"), logData))
            {
                bool isPreferenceUpdateSuccess = _userPreferenceHelper.AddUpdateSpecificUserPreference(userPreferenceData, userPreferenceColumn, SignedInUser.UserPrincipalName.ToLowerInvariant(), ClientDevice, sessionId);
                if (isPreferenceUpdateSuccess)
                    return Ok();
                else
                    throw new InvalidOperationException(Constants.UserPreferencePostError);
            }
        }
        catch
        {
            return BadRequest(Constants.UserPreferencePostError);
        }
    }
}