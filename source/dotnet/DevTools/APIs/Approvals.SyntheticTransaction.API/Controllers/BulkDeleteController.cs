// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace Microsoft.CFS.Approvals.SyntheticTransaction.API.Controllers;

using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.CFS.Approvals.DevTools.Model.Constant;
using Microsoft.CFS.Approvals.LogManager.Provider.Interface;
using Microsoft.CFS.Approvals.SyntheticTransaction.Helpers.Interface;

/// <summary>
/// The Bulk Delete Controller
/// </summary>
[Route("api/v1/BulkDelete")]
[ApiController]
public class BulkDeleteController : ControllerBase
{
    private readonly IBulkDeleteHelper _bulkDeleteHelper;
    private readonly ILogProvider _logProvider;
    private readonly string _environment;

    /// <summary>
    /// Constructor of BulkDeleteController
    /// </summary>
    /// <param name="bulkDeleteHelper"></param>
    /// <param name="logProvider"></param>
    public BulkDeleteController(IBulkDeleteHelper bulkDeleteHelper,
        ILogProvider logProvider)
    {
        _bulkDeleteHelper = bulkDeleteHelper;
        _logProvider = logProvider;
    }

    /// <summary>
    /// Bulk delete document
    /// </summary>
    /// <param name="Tenant"></param>
    /// <param name="Approver"></param>
    /// <param name="Days"></param>
    /// <param name="DocNumber"></param>
    /// <returns></returns>
    [HttpPost]
    [Route("BulkDeleteDocument/{env}")]
    public async Task<IActionResult> BulkDeleteDocument(string tenant, string approver, string days, string docNumber)
    {
        Dictionary<LogDataKey, object> logData = new Dictionary<LogDataKey, object>();
        var tcv = Guid.NewGuid().ToString();
        logData.Add(LogDataKey.Xcv, tcv);
        logData.Add(LogDataKey.Tcv, tcv);
        logData.Add(LogDataKey.Environment, _environment);

        try
        {
            _logProvider.LogInformation(TrackingEvent.BulkDeleteStarted, logData);
            var result = await _bulkDeleteHelper.BulkDelete(tenant, approver, days, docNumber, tcv);
            return Ok(result);
        }
        catch (Exception ex)
        {
            logData.Add(LogDataKey.EventName, "BulkDeleteFailure");
            logData.Add(LogDataKey.Operation, "Failed to Delete the Bulk Requests");
            _logProvider.LogError(TrackingEvent.BulkDeleteFailure, ex, logData);
            return BadRequest(ex.Message);
        }
    }
}