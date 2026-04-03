// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace Microsoft.CFS.Approvals.SyntheticTransaction.API.Controllers;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using Microsoft.CFS.Approvals.Data.Azure.Storage.Interface;
using Microsoft.CFS.Approvals.DevTools.AppConfiguration;
using Microsoft.CFS.Approvals.DevTools.Model.Constant;
using Microsoft.CFS.Approvals.LogManager.Provider.Interface;
using Microsoft.CFS.Approvals.SyntheticTransaction.Common.Models;
using Microsoft.CFS.Approvals.SyntheticTransaction.Helpers.ExtensionMethods;

/// <summary>
/// The Pull Tenant Controller
/// </summary>
[Route("{env}/api/TestPullTenant")]
[ApiController]
public class TestPullTenantController : ControllerBase
{
    /// <summary>
    /// The azure storage helper
    /// </summary>
    private readonly ITableHelper _azureStorageHelper;

    /// <summary>
    /// The log provider
    /// </summary>
    private readonly ILogProvider _logProvider;

    private readonly string _environment;

    /// <summary>
    /// Constructor of TestPullTenantController
    /// </summary>
    /// <param name="azureStorageHelper"></param>
    /// <param name="actionContextAccessor"></param>
    /// <param name="configurationHelper"></param>
    /// <param name="logProvider"></param>
    public TestPullTenantController(Func<string, ITableHelper> azureStorageHelper,
        IActionContextAccessor actionContextAccessor,
        ConfigurationHelper configurationHelper,
        ILogProvider logProvider)
    {
        _environment = actionContextAccessor?.ActionContext?.RouteData?.Values["env"]?.ToString();
        _azureStorageHelper = azureStorageHelper(
            configurationHelper.appSettings[_environment]["StorageAccountName"]);
        _logProvider = logProvider;
    }

    /// <summary>
    /// Get tenant summary data
    /// </summary>
    /// <param name="alias"></param>
    /// <param name="tenantId"></param>
    /// <returns></returns>
    [Route("{alias}/{tenantId}")]
    public async Task<IActionResult> Get(string alias, string tenantId)
    {
        var tcv = Request.Headers["Tcv"];
        Dictionary<LogDataKey, object> logData = new Dictionary<LogDataKey, object>();
        logData.Add(LogDataKey.Xcv, tcv);
        logData.Add(LogDataKey.Tcv, tcv);
        logData.Add(LogDataKey.ComponentName, "API");
        logData.Add(LogDataKey.MSAComponentName, "TestHarness");
        logData.Add(LogDataKey.Environment, _environment);
        logData.Add(LogDataKey.UserAlias, alias);
        logData.Add(LogDataKey.TenantId, tenantId);
        logData.Add(LogDataKey.Operation, "Test Pull Tenant Summary - Controller");

        try
        {
            var summaryData = _azureStorageHelper.GetTableEntityByPartitionKeyAndField<TestTenantSummaryEntity>("TenantSummaryData", alias, "TenantID", tenantId);
            if (summaryData == null)
            {
                _logProvider.LogInformation(TrackingEvent.NoPendingSummaryData, logData);
                return Ok("No pending Summary Data for this user");
            }
            else
            {
                var ApprovalSummaryData = (from record in summaryData
                                           select record.JsonData.ToJObject());
                var responseContent = new
                {
                    response = new { ApprovalSummaryData }
                }.ToJToken();

                _logProvider.LogInformation(TrackingEvent.PullTenantSummaryDataSuccess, logData);
                return Ok(responseContent);
            }
        }
        catch (Exception ex)
        {
            _logProvider.LogError(TrackingEvent.PullTenantSummaryFailure, ex, logData);
            return BadRequest(ex.Message);
        }
    }
}