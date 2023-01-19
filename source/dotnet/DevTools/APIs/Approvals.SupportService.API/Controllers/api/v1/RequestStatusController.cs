// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace Microsoft.CFS.Approvals.SupportService.API.Controllers.api.v1;
using System;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using Microsoft.CFS.Approvals.Data.Azure.Storage.Interface;
using Microsoft.CFS.Approvals.DevTools.Model.Models;
using Microsoft.CFS.Approvals.SupportServices.Helper.ServiceHelper;
using Newtonsoft.Json.Linq;

/// <summary>
/// The Request Status Controller
/// </summary>
[Route("api/v1/RequestStatus")]
[ApiController]
public class RequestStatusController : ControllerBase
{
    /// <summary>
    /// The table helper
    /// </summary>
    private readonly ITableHelper _azureTableStorageHelper;

    private readonly ConfigurationHelper _configurationHelper;
    private readonly string _environment;

    /// <summary>
    /// Constructor of RequestStatusController
    /// </summary>
    /// <param name="azureTableStorageHelper"></param>
    /// <param name="configurationHelper"></param>
    /// <param name="actionContextAccessor"></param>
    public RequestStatusController(
        Func<string, string, ITableHelper> azureTableStorageHelper,
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
    /// Get request status by request number
    /// </summary>
    /// <param name="RequestNumber"></param>
    /// <returns></returns>
    [HttpGet]
    [Route("{env}")]
    public IActionResult Get(string RequestNumber)
    {
        JArray response = null;
        var summary = _azureTableStorageHelper.GetTableEntityListByfield<SummaryEntity>(_configurationHelper.appSettings[_environment]["ApprovalSummaryTable"], "DocumentNumber", RequestNumber);
        if (summary != null && summary.Count > 0)
        {
            response = new JArray();
            foreach (var row in summary)
            {
                JObject data = new JObject();
                data.Add("TenantName", row.Application);
                data.Add("DocumentNumber", row.DocumentNumber);
                data.Add("Approver", row.Approver);
                data.Add("Status", row.LastFailed == true ? "Failed" : "Pending for approval");
                data.Add("Timestamp", row.Timestamp);
                response.Add(data);
            }
        }
        else
        {
            var transactionHistory = _azureTableStorageHelper.GetTableEntityListByPartitionKey<TransactionHistoryEntity>("TransactionHistory", RequestNumber);
            if (transactionHistory != null && transactionHistory.Count > 0)
            {
                response = new JArray();
                foreach (var row in transactionHistory)
                {
                    JObject data = new JObject();
                    data.Add("TenantName", row.AppName);
                    data.Add("DocumentNumber", row.DocumentNumber);
                    data.Add("Approver", row.Approver);
                    data.Add("Status", row.ActionTaken);
                    data.Add("Timestamp", row.Timestamp);
                    response.Add(data);
                }
            }
        }
        return Ok(response);
    }
}