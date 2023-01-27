// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace Microsoft.CFS.Approvals.SyntheticTransaction.API.Controllers;

using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using Microsoft.CFS.Approvals.Data.Azure.Storage.Interface;
using Microsoft.CFS.Approvals.DevTools.Model.Constant;
using Microsoft.CFS.Approvals.LogManager.Provider.Interface;
using Microsoft.CFS.Approvals.SyntheticTransaction.Common.Models;
using Microsoft.CFS.Approvals.SyntheticTransaction.Helpers.ExtensionMethods;
using Microsoft.CFS.Approvals.SyntheticTransaction.Helpers.Helpers;
using Newtonsoft.Json.Linq;

/// <summary>
/// The Details Controller
/// </summary>
[Route("api/TestDetails/{env}")]
[ApiController]
public class TestDetailsController : ControllerBase
{
    private readonly ITableHelper _azureStorageHelper;
    private readonly ILogProvider _logProvider;
    private readonly string _environment;

    /// <summary>
    /// Constructor of TestDetailsController
    /// </summary>
    /// <param name="azureStorageHelper"></param>
    /// <param name="configurationSetting"></param>
    /// <param name="actionContextAccessor"></param>
    /// <param name="logProvider"></param>
    public TestDetailsController(
        Func<string, string, ITableHelper> azureStorageHelper,
        ConfigurationSetting configurationSetting,
        IActionContextAccessor actionContextAccessor,
        ILogProvider logProvider)
    {
        _environment = actionContextAccessor?.ActionContext?.RouteData?.Values["env"]?.ToString();
        _azureStorageHelper = azureStorageHelper(
           configurationSetting.appSettings[_environment].StorageAccountName,
           configurationSetting.appSettings[_environment].StorageAccountKey);
        _logProvider = logProvider;
    }

    /// <summary>
    /// Get synthetic transaction details
    /// </summary>
    /// <param name="operation"></param>
    /// <param name="documentNumber"></param>
    /// <returns></returns>
    [HttpGet]
    [Route("{operation}/{documentNumber}")]
    public async Task<IActionResult> Get(string operation, string documentNumber)
    {
        var tcv = Request.Headers["Tcv"];
        Dictionary<LogDataKey, object> logData = new Dictionary<LogDataKey, object>();
        logData.Add(LogDataKey.Xcv, documentNumber);
        logData.Add(LogDataKey.Tcv, tcv);
        logData.Add(LogDataKey.Environment, _environment);
        logData.Add(LogDataKey.DetailOperation, operation);
        logData.Add(LogDataKey.Operation, "Test Details - Controller");

        try
        {
            var detail = _azureStorageHelper.GetTableEntityByPartitionKeyAndRowKey<SyntheticTransactionEntity>("SyntheticTransactionDetails", documentNumber, operation);
            var response = !string.IsNullOrWhiteSpace(detail.JsonData) ? detail.JsonData.FromJson<JObject>() : new JObject();
            _logProvider.LogInformation(TrackingEvent.DetailFetchSuccessful, logData);
            return Ok(response);
        }
        catch (Exception ex)
        {
            logData.Add(LogDataKey.EventName, "DetailFetchFailure");
            _logProvider.LogError(TrackingEvent.DetailFetchFailure, ex, logData);
            return BadRequest(ex.Message);
        }
    }
}