// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace Microsoft.CFS.Approvals.SyntheticTransaction.API.Controllers;

using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.CFS.Approvals.DevTools.Model.Constant;
using Microsoft.CFS.Approvals.LogManager.Provider.Interface;
using Microsoft.CFS.Approvals.SyntheticTransaction.API.Services;
using Microsoft.CFS.Approvals.SyntheticTransaction.Helpers.Interface;
using Microsoft.Extensions.Configuration;

/// <summary>
/// The Load Generator Controller
/// </summary>
[Route("api/v1/LoadGenerator")]
[ApiController]
public class LoadGeneratorController : ControllerBase
{
    /// <summary>
    /// The synthetic transaction helper
    /// </summary>
    private readonly ISyntheticTransactionHelper _syntheticTransactionHelper;

    /// <summary>
    /// The configuration helper
    /// </summary>
    private readonly IConfiguration _configuration;

    /// <summary>
    /// The load generator helper
    /// </summary>
    private readonly ILoadGeneratorHelper _loadGeneratorHelper;

    /// <summary>
    /// The log provider
    /// </summary>
    private readonly ILogProvider _logProvider;

    /// <summary>
    /// Environment
    /// </summary>
    private readonly string _environment;

    /// <summary>
    /// Constructor of LoadGeneratorController
    /// </summary>
    /// <param name="syntheticTransactionHelper"></param>
    /// <param name="configuration"></param>
    /// <param name="loadGeneratorHelper"></param>
    /// <param name="logProvider"></param>
    public LoadGeneratorController(ISyntheticTransactionHelper syntheticTransactionHelper,
        IConfiguration configuration,
        ILoadGeneratorHelper loadGeneratorHelper,
        ILogProvider logProvider)
    {
        _syntheticTransactionHelper = syntheticTransactionHelper;
        _configuration = configuration;
        _loadGeneratorHelper = loadGeneratorHelper;
        _logProvider = logProvider;
    }

    /// <summary>
    /// Generate load
    /// </summary>
    /// <param name="tenant"></param>
    /// <param name="approver"></param>
    /// <param name="load"></param>
    /// <param name="batchsize"></param>
    /// <returns></returns>
    [HttpPost]
    [Route("GenerateLoad/{env}")]
    public async Task<IActionResult> GenerateLoad(string tenant, string approver, int load, int batchsize)
    {
        Dictionary<LogDataKey, object> logData = new Dictionary<LogDataKey, object>();
        var tcv = Guid.NewGuid().ToString();
        logData.Add(LogDataKey.Xcv, tcv);
        logData.Add(LogDataKey.Tcv, tcv);
        logData.Add(LogDataKey.Environment, _environment);
        logData.Add(LogDataKey.TenantName, tenant);
        logData.Add(LogDataKey.UserAlias, approver);
        logData.Add(LogDataKey.Load, load.ToString());
        logData.Add(LogDataKey.BatchSize, batchsize.ToString());
        logData.Add(LogDataKey.PayloadType, "Create");
        logData.Add(LogDataKey.Operation, "Generate Load - Controller");

        try
        {
            var sampleData = _syntheticTransactionHelper.GetSchemaFile(string.Format("{0}.json", tenant), tcv).Result;
            if (string.IsNullOrWhiteSpace(sampleData))
            {
                sampleData = _syntheticTransactionHelper.GetSchemaFile(_configuration["MasterPayload"], tcv).Result;
            }
            if (string.IsNullOrWhiteSpace(sampleData))
                return NotFound(new { message = "Tenant configuration yet to be done. Please Configure selected tenant." });

            var result = await _loadGeneratorHelper.GenerateLoad(tenant, approver, load, batchsize, sampleData, tcv);
            return Ok(result); // CodeQL [SM04901] justification: The tenant parameter is not user input but is selected from a predefined list of legitimate options by our system. Data is returned only if tenant is a valid and legitimate option and would contain the request numbers of load test created. It is unrelated to the "tid" claim.
        }
        catch (Exception ex)
        {
            logData.Add(LogDataKey.EventName, "LoadGenerationFailure");
            _logProvider.LogError(TrackingEvent.LoadGenerationFailure, ex, logData);
            return BadRequest(ex.Message);
        }
    }
}