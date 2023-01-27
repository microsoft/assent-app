// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.


namespace Microsoft.CFS.Approvals.SyntheticTransaction.API.Controllers;

using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using Microsoft.CFS.Approvals.DevTools.Model.Constant;
using Microsoft.CFS.Approvals.LogManager.Provider.Interface;
using Microsoft.CFS.Approvals.SyntheticTransaction.API.Services;
using Microsoft.CFS.Approvals.SyntheticTransaction.Common.Models; 
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

/// <summary>
/// The Pull Tenant Details Controller
/// </summary>
[Route("api/TestPullTenantDetails")]
[ApiController]
public class TestPullTenantDetailsController : ControllerBase
{
    /// <summary>
    /// The synthetic transaction helper
    /// </summary>
    private readonly ISyntheticTransactionHelper _syntheticTransactionHelper;

    /// <summary>
    /// The log provider
    /// </summary>
    private readonly ILogProvider _logProvider;
    private readonly string _environment;

    /// <summary>
    /// Constructor of TestPullTenantDetailsController
    /// </summary>
    /// <param name="syntheticTransactionHelper"></param>
    /// <param name="actionContextAccessor"></param>
    /// <param name="logProvider"></param>
    public TestPullTenantDetailsController(ISyntheticTransactionHelper syntheticTransactionHelper,
        IActionContextAccessor actionContextAccessor,
        ILogProvider logProvider)
    {
        _environment = actionContextAccessor?.ActionContext?.RouteData?.Values["env"]?.ToString();
        _syntheticTransactionHelper = syntheticTransactionHelper;
        _logProvider = logProvider;
    }

    /// <summary>
    /// Get request document details
    /// </summary>
    /// <param name="documentNumber"></param>
    /// <param name="alias"></param>
    /// <param name="tenantId"></param>
    /// <returns></returns>
    [HttpGet]
    [Route("{documentNumber}/{alias}/{tenantId}/{env}")]
    public async Task<IActionResult> GetAsync(string documentNumber, string alias, string tenantId)
    {
        var tcv = Request.Headers["Tcv"];
        Dictionary<LogDataKey, object> logData = new Dictionary<LogDataKey, object>();
        logData.Add(LogDataKey.Xcv, documentNumber);
        logData.Add(LogDataKey.Tcv, tcv);
        logData.Add(LogDataKey.ComponentName, "API");
        logData.Add(LogDataKey.MSAComponentName, "TestHarness");
        logData.Add(LogDataKey.Environment, _environment);
        logData.Add(LogDataKey.UserAlias, alias);
        logData.Add(LogDataKey.TenantId, tenantId);
        logData.Add(LogDataKey.Operation, "Test Pull Tenant Details - Controller");

        try
        {
            var details = await _syntheticTransactionHelper.GetSchemaFile(string.Format("{0}-{1}", tenantId, "Details.json"), tcv);
            var Jdetails = JsonConvert.DeserializeObject<JObject>(details);
            switch ((TestPullTenant)Enum.Parse(typeof(TestPullTenant), tenantId, true))
            {
                case TestPullTenant.TestMSTime:
                    if (Jdetails?.SelectToken("laborId") != null)
                    {
                        Jdetails["laborId"] = documentNumber;
                    }
                    if (Jdetails?.SelectToken("userAlias") != null)
                    {
                        Jdetails["userAlias"] = alias;
                    }
                    break;
                default:
                    break;
            }
            _logProvider.LogInformation(TrackingEvent.PullTenantDetailsSuccess, logData);
            return Ok(Jdetails.ToString());
        }
        catch (Exception ex)
        {
            logData.Add(LogDataKey.EventName, "PullTenantDetailsFailure");
            _logProvider.LogError(TrackingEvent.PullTenantDetailsFailure, ex, logData);
            return BadRequest(ex.Message);
        }
    }
}