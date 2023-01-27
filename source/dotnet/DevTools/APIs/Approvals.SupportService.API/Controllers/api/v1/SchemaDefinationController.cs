// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace Microsoft.CFS.Approvals.SupportService.API.Controllers.api.v1;
using System;
using System.Linq;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using Microsoft.CFS.Approvals.Data.Azure.Storage.Interface;
using Microsoft.CFS.Approvals.DevTools.Model.Models;
using Microsoft.CFS.Approvals.SupportServices.Helper.ServiceHelper;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

/// <summary>
/// The Schema Defination Controller
/// </summary>
[Route("api/v1/SchemaDefination")]
[ApiController]
public class SchemaDefinationController : ControllerBase
{
    /// <summary>
    /// The table helper
    /// </summary>
    private readonly ITableHelper _azureTableStorageHelper;

    private readonly ConfigurationHelper _configurationHelper;
    private readonly string _environment;

    /// <summary>
    /// Constructor of SchemaDefinationController
    /// </summary>
    /// <param name="azureTableStorageHelper"></param>
    /// <param name="configurationHelper"></param>
    /// <param name="actionContextAccessor"></param>
    public SchemaDefinationController(Func<string, string, ITableHelper> azureTableStorageHelper,
        ConfigurationHelper configurationHelper,
        IActionContextAccessor actionContextAccessor)
    {
        _environment = actionContextAccessor?.ActionContext?.RouteData?.Values["env"]?.ToString();

        _configurationHelper = configurationHelper;
        _azureTableStorageHelper = azureTableStorageHelper(
             configurationHelper.appSettings[_environment]["StorageAccountName"],
            configurationHelper.appSettings[_environment]["StorageAccountKey"]);
    }

    /// <summary>
    /// Get schema entity
    /// </summary>
    /// <param name="form"></param>
    /// <returns></returns>
    [Route("{env}")]
    public IActionResult Get(string form)
    {
        var schema = _azureTableStorageHelper.GetTableEntityByPartitionKey<SchemaEntity>("SupportPortalUISchema", form);
        return Ok(schema.Schema);
    }

    /// <summary>
    /// Get scope by environment
    /// </summary>
    /// <returns></returns>
    [Route("GetScope/{env}")]
    public IActionResult Get()
    {
        var scope = JsonConvert.DeserializeObject<JArray>(_configurationHelper.appSettings[_environment]["AIScope"]).Select(s => s?.SelectToken("scopeName").ToString());
        return Ok(scope);
    }
}