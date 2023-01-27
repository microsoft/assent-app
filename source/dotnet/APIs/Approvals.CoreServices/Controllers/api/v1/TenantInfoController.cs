// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace Microsoft.CFS.Approvals.CoreServices.Controllers.api.v1;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.CFS.Approvals.Common.DL.Interface;
using Microsoft.CFS.Approvals.Contracts;
using Microsoft.CFS.Approvals.Core.BL.Interface;
using Microsoft.CFS.Approvals.Extensions;
using Microsoft.CFS.Approvals.LogManager.Model;
using Microsoft.CFS.Approvals.LogManager.Provider.Interface;
using Newtonsoft.Json.Linq;
using Swashbuckle.AspNetCore.Annotations;

/// <summary>
/// Class - TenantInfoController handles the CRUD operation for ApprovalTenantInfo table.
/// </summary>
/// <seealso cref="BaseApiController" />
public class TenantInfoController : BaseApiController
{
    /// <summary>
    /// The approval tenant info helper
    /// </summary>
    private readonly IApprovalTenantInfoHelper _approvalTenantInfoHelper = null;

    /// <summary>
    /// The performance logger
    /// </summary>
    private readonly IPerformanceLogger _performanceLogger = null;

    /// <summary>
    /// The log provider
    /// </summary>
    private readonly ILogProvider _logProvider = null;

    /// <summary>
    /// Initializes a new instance of the <see cref="TenantInfoController"/> class.
    /// </summary>
    /// <param name="approvalTenantInfoHelper"></param>
    /// <param name="performanceLogger"></param>
    /// <param name="logProvider"></param>
    public TenantInfoController(IApprovalTenantInfoHelper approvalTenantInfoHelper, IPerformanceLogger performanceLogger, ILogProvider logProvider)
    {
        _approvalTenantInfoHelper = approvalTenantInfoHelper;
        _performanceLogger = performanceLogger;
        _logProvider = logProvider;
    }

    /// <summary>
    /// HTTP GET api/TenantInfo
    /// </summary>
    /// <returns>Http action result</returns>
    [SwaggerOperation(Tags = new[] { "Metadata" })]
    [HttpGet]
    public async Task<IActionResult> Get()
    {
        #region Logging

        var logData = new Dictionary<LogDataKey, object>
        {
            { LogDataKey.Xcv, Xcv },
            { LogDataKey.DXcv, Tcv },
            { LogDataKey.StartDateTime, DateTime.UtcNow },
            { LogDataKey.IsCriticalEvent, CriticalityLevel.No.ToString() }
        };

        #endregion Logging

        try
        {
            using (_performanceLogger.StartPerformanceLogger("PerfLog", Constants.WebClient, string.Format(Constants.PerfLogAction, "TenantInfoController", "Get Tenants"), logData))
            {
                var responseObject = await _approvalTenantInfoHelper.GetTenants();

                #region Remove ServiceParameter from response

                responseObject = responseObject?.Select(x =>
                {
                    x.ServiceParameter = string.Empty;
                    if (x.TenantOperationDetails.Contains(Constants.ServiceParameter))
                    {
                        var tenantOperationDetails = JObject.Parse(x?.TenantOperationDetails);
                        foreach (var tenantOperationDetail in from tenantOperationDetail in tenantOperationDetails[Constants.DetailOpsList]
                                                              where tenantOperationDetail[Constants.ServiceParameter] != null
                                                              select tenantOperationDetail)
                        {
                            tenantOperationDetail[Constants.ServiceParameter] = string.Empty;
                        }
                        x.TenantOperationDetails = tenantOperationDetails.ToString();
                    }
                    return x;
                }).ToList();
                logData.Modify(LogDataKey.EndDateTime, DateTime.UtcNow);
                _logProvider.LogInformation(TrackingEvent.TenantApiComplete, logData);

                #endregion Remove ServiceParameter from response

                return Ok(responseObject);
            }
        }
        catch (Exception ex)
        {
            logData.Modify(LogDataKey.EndDateTime, DateTime.UtcNow);
            _logProvider.LogError(TrackingEvent.TenantApiFail, ex, logData);
            return BadRequest(ex.Message);
        }
    }

    /// <summary>
    /// HTTP GET api/TenantInfo/{tenantId}?SessionId={SessionId}
    /// </summary>
    /// <param name="tenantId"></param>
    /// <param name="sessionId"></param>
    /// <returns>Returns Tenant Action Details</returns>
    [SwaggerOperation(Tags = new[] { "Metadata" })]
    [HttpGet("{tenantId}")]
    public async Task<IActionResult> Get(int tenantId, string sessionId = "")
    {
        try
        {
            var approvalTenantInfo = await _approvalTenantInfoHelper.GetTenantActionDetails(tenantId, LoggedInAlias, Alias, Host, sessionId, Xcv, Tcv, GetTokenOrCookie());
            approvalTenantInfo.ServiceParameter = string.Empty;
            return Ok(approvalTenantInfo);
        }
        catch (Exception ex)
        {
            return BadRequest(ex.Message);
        }
    }
}