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
using Microsoft.CFS.Approvals.DevTools.Model.Constant;
using Microsoft.CFS.Approvals.LogManager.Provider.Interface;
using Microsoft.CFS.Approvals.SyntheticTransaction.Common.Models;
using Microsoft.CFS.Approvals.SyntheticTransaction.Helpers.ExtensionMethods;
using Microsoft.CFS.Approvals.SyntheticTransaction.Helpers.Helpers;

/// <summary>
/// The Bulk Action Controller
/// </summary>
[Route("{env}/api/TestBulkActions")]
[ApiController]
public class TestBulkActionsController : ControllerBase
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
    /// Constructor of TestBulkActionsController
    /// </summary>
    /// <param name="azureStorageHelper"></param>
    /// <param name="actionContextAccessor"></param>
    /// <param name="configurationSetting"></param>
    /// <param name="logProvider"></param>
    public TestBulkActionsController(Func<string, string, ITableHelper> azureStorageHelper,
        IActionContextAccessor actionContextAccessor,
        ConfigurationSetting configurationSetting,
        ILogProvider logProvider)
    {
        _environment = actionContextAccessor?.ActionContext?.RouteData?.Values["env"]?.ToString();
        _azureStorageHelper = azureStorageHelper(
            configurationSetting.appSettings[_environment].StorageAccountName,
            configurationSetting.appSettings[_environment].StorageAccountKey);
        _logProvider = logProvider;
    }

    /// <summary>
    /// Generate approval response
    /// </summary>
    /// <param name="requests"></param>
    /// <returns></returns>
    [HttpPost]
    public async Task<IActionResult> Post([FromBody] LOBRequest[] requests)
    {
        var tcv = Request.Headers["Tcv"];
        Dictionary<LogDataKey, object> logData = new Dictionary<LogDataKey, object>();
        logData.Add(LogDataKey.Xcv, String.Join(",", requests.Select(s => s.DocumentKeys.DisplayDocumentNumber)));
        logData.Add(LogDataKey.Tcv, tcv);
        logData.Add(LogDataKey.Environment, _environment);
        logData.Add(LogDataKey.Operation, "Test Bulk Actions - Controller");

        ApprovalResponse approvalResponse;
        var error = new ApprovalResponseErrorInfo();
        var tenantEntity = _azureStorageHelper.GetTableEntityByfield<TenantEntity>("ApprovalTenantInfo", "DocTypeId", requests[0].DocumentTypeID);

        try
        {
            approvalResponse = ApprovalResponseHelper.GenerateApprovalResponse(tenantEntity.AppName, requests[0].DocumentTypeID, error, TenantActionMessage.MsgActionSuccess.StringValue<TenantActionMessage>());

            logData.Add(LogDataKey.TenantName, tenantEntity.AppName);
            _logProvider.LogInformation(TrackingEvent.BulkActionSuccessful, logData);
            return Ok(approvalResponse);
        }
        catch (Exception ex)
        {
            logData.Add(LogDataKey.EventName, "ActionFailure");
            logData.Add(LogDataKey.TenantName, tenantEntity.AppName);
            _logProvider.LogError(TrackingEvent.ActionFailure, ex, logData);

            error.ErrorMessages = new List<string> { TenantActionMessage.MsgActionFailure.StringValue<TenantActionMessage>() };
            error.ErrorType = ApprovalResponseErrorType.UnintendedError;
            approvalResponse = ApprovalResponseHelper.GenerateApprovalResponse(tenantEntity.AppName, requests[0].DocumentTypeID, error, TenantActionMessage.MsgActionSuccess.StringValue<TenantActionMessage>());
            return Ok(approvalResponse);
        }
    }
}