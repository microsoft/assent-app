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
using Microsoft.CFS.Approvals.Core.BL.Interface;
using Microsoft.CFS.Approvals.Extensions;
using Microsoft.CFS.Approvals.LogManager.Model;
using Microsoft.CFS.Approvals.LogManager.Provider.Interface;
using Microsoft.CFS.Approvals.Utilities.Helpers;
using Swashbuckle.AspNetCore.Annotations;

/// <summary>
/// Save Editable Details controller
/// </summary>
/// <seealso cref="BaseApiController" />
public class SaveEditableDetailsController : BaseApiController
{
    /// <summary>
    /// The save details helper
    /// </summary>
    public ISaveEditableDetailsHelper _saveEditableDetailsHelper = null;

    /// <summary>
    /// The performance logger
    /// </summary>
    private readonly IPerformanceLogger _performanceLogger = null;

    /// <summary>
    /// The log provider
    /// </summary>
    private readonly ILogProvider _logProvider = null;

    /// <summary>
    /// Initializes a new instance of the <see cref="SaveEditableDetailsController"/> class.
    /// </summary>
    /// <param name="saveEditableDetailsHelper">The save details helper.</param>
    /// <param name="performanceLogger">The performance logger</param>
    /// <param name="logProvider">The log provider</param>
    public SaveEditableDetailsController(ISaveEditableDetailsHelper saveEditableDetailsHelper, IPerformanceLogger performanceLogger, ILogProvider logProvider)
    {
        _saveEditableDetailsHelper = saveEditableDetailsHelper;
        _performanceLogger = performanceLogger;
        _logProvider = logProvider;
    }

    /// <summary>
    /// This method returns the flag so as to decide whether
    /// the edit details functionality should be enabled for given
    /// combination of Approver and Tenant
    /// </summary>
    /// <param name="tenantId"></param>
    /// <param name="documentNumber"></param>
    /// <returns></returns>
    [SwaggerOperation(Tags = new[] { "Details" })]
    [HttpGet]
    public IActionResult Get(int tenantId, string documentNumber)
    {
        #region Logging

        var logData = new Dictionary<LogDataKey, object>
        {
            { LogDataKey.Xcv, Xcv },
            { LogDataKey.DXcv, Tcv },
            { LogDataKey.StartDateTime, DateTime.UtcNow },
            { LogDataKey.TenantId, tenantId },
            { LogDataKey.DocumentNumber, documentNumber },
            { LogDataKey.IsCriticalEvent, CriticalityLevel.No.ToString() }
        };

        #endregion Logging

        try
        {
            using (_performanceLogger.StartPerformanceLogger("PerfLog", Constants.WebClient, string.Format(Constants.PerfLogAction, "SaveEditableDetailsController", "CheckUserAuthorizationForEdit"), logData))
            {
                ArgumentGuard.NotNull(tenantId, nameof(tenantId));
                ArgumentGuard.NotNull(documentNumber, nameof(documentNumber));

                var result = _saveEditableDetailsHelper.CheckUserAuthorizationForEdit(tenantId, documentNumber, Alias);
                logData.Modify(LogDataKey.EndDateTime, DateTime.UtcNow);
                _logProvider.LogInformation(TrackingEvent.WebApiSaveEditableDetailsSuccess, logData);
                return Ok(result);
            }
        }
        catch (Exception ex)
        {
            logData.Modify(LogDataKey.EndDateTime, DateTime.UtcNow);
            _logProvider.LogError(TrackingEvent.WebApiSaveEditableDetailsFail, ex, logData);
            return BadRequest(ex.Message);
        }
    }

    /// <summary>
    /// This method saves the details edited by the user into ApprovalDetails table
    /// The details are inserted if not present or replaced if already present
    /// </summary>
    /// <param name="tenantId"></param>
    /// <param name="sessionId"></param>
    /// <param name="xcv"></param>
    /// <param name="tcv"></param>
    /// <returns></returns>
    [SwaggerOperation(Tags = new[] { "Details" })]
    [HttpPost]
    [Route("api/v1/[controller]/{tenantId}")]
    public async Task<IActionResult> Post(int tenantId, string sessionId = "", string xcv = "", string tcv = "")
    {
        #region Logging

        var logData = new Dictionary<LogDataKey, object>
        {
            { LogDataKey.Xcv, xcv },
            { LogDataKey.DXcv, tcv },
            { LogDataKey.StartDateTime, DateTime.UtcNow },
            { LogDataKey.TenantId, tenantId },
            { LogDataKey.SessionId, sessionId },
            { LogDataKey.IsCriticalEvent, CriticalityLevel.No.ToString() }
        };

        #endregion Logging

        try
        {
            using (_performanceLogger.StartPerformanceLogger("PerfLog", Constants.WebClient, string.Format(Constants.PerfLogAction, "SaveEditableDetailsController", "CheckUserAuthorizationForEdit"), logData))
            {
                string requestContent;
                using (var reader = new StreamReader(Request.Body))
                {
                    requestContent = await reader.ReadToEndAsync();
                }
                var detailsString = requestContent;
                List<string> validationResults = _saveEditableDetailsHelper.SaveEditedDetails(detailsString, tenantId, Alias, xcv, tcv, LoggedInAlias);

                logData.Modify(LogDataKey.EndDateTime, DateTime.UtcNow);
                _logProvider.LogInformation(TrackingEvent.WebApiSaveEditableDetailsSuccess, logData);

                if (validationResults != null && validationResults.Count != 0)
                {
                    return Ok(validationResults);
                }
                else
                {
                    return Ok(new List<string>());
                }
            }
        }
        catch (Exception ex)
        {
            logData.Modify(LogDataKey.EndDateTime, DateTime.UtcNow);
            _logProvider.LogError(TrackingEvent.WebApiSaveEditableDetailsFail, ex, logData);
            return BadRequest("Unable to save details due to: " + ex.Message);
        }
    }
}