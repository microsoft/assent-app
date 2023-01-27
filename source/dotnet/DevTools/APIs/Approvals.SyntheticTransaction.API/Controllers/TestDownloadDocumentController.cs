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
using Microsoft.CFS.Approvals.SyntheticTransaction.Common.Constant;
using Microsoft.CFS.Approvals.SyntheticTransaction.Helpers.ExtensionMethods;
using Microsoft.CFS.Approvals.SyntheticTransaction.Helpers.Helpers;
using Newtonsoft.Json.Linq;
using Constants = Common.Constant.Constants;

/// <summary>
/// The Download Document Controller
/// </summary>
[Route("api/TestDownloadDocument/{env}")]
[ApiController]
public class TestDownloadDocumentController : ControllerBase
{
    private readonly IBlobStorageHelper _blobStorageHelper;
    private readonly ILogProvider _logProvider;
    private readonly string _environment;

    /// <summary>
    /// Constructor of TestDownloadDocumentController
    /// </summary>
    /// <param name="blobStorageHelper"></param>
    /// <param name="configurationSetting"></param>
    /// <param name="actionContextAccessor"></param>
    /// <param name="logProvider"></param>
    public TestDownloadDocumentController(
        Func<string, string, IBlobStorageHelper> blobStorageHelper,
        ConfigurationSetting configurationSetting,
        IActionContextAccessor actionContextAccessor,
        ILogProvider logProvider)
    {
        _environment = actionContextAccessor?.ActionContext?.RouteData?.Values["env"]?.ToString();
        _blobStorageHelper = blobStorageHelper(
          configurationSetting.appSettings[_environment].StorageAccountName,
          configurationSetting.appSettings[_environment].StorageAccountKey);
        _logProvider = logProvider;
    }

    /// <summary>
    /// Download document
    /// </summary>
    /// <param name="documentName"></param>
    /// <returns></returns>
    [HttpGet]
    [Route("{documentName}")]
    public async Task<IActionResult> Get(string documentName)
    {
        List<JObject> attachments;

        var tcv = Request.Headers["Tcv"];
        Dictionary<LogDataKey, object> logData = new Dictionary<LogDataKey, object>();
        logData.Add(LogDataKey.Xcv, tcv);
        logData.Add(LogDataKey.Tcv, tcv);
        logData.Add(LogDataKey.Environment, _environment);
        logData.Add(LogDataKey.DetailOperation, "GetAttachment");
        logData.Add(LogDataKey.Operation, "Test Download Document - Controller");

        attachments = (Constants.AttachmentsJson).FromJson<List<JObject>>();
        var attachment = attachments.Where(s => s.SelectToken("ID")?.ToString() == documentName?.ToString()).FirstOrDefault();
        //TODO: Add back conditions (&& attachments == null && attachments.Count == 0) once the AttachmentJson moved to blob. Removed now to cover BadRequest code with unit test, which is otherwise not reachable code.
        if (documentName == null && attachment == null)
        {
            _logProvider.LogInformation(TrackingEvent.GetAttachmentFailure, logData);
            return BadRequest("AttachmentID cannot be null");
        }

        try
        {
            string blobNameFormat = "{0}|{1}";
            string blobName = string.Format(blobNameFormat, documentName, attachment.SelectToken("Name")?.ToString());
            logData.Add(LogDataKey.Attachment, blobName);
            byte[] contentArray = await _blobStorageHelper.DownloadByteArray("testharnessattachments", blobName);
            return File(contentArray, "application/octet-stream");
        }
        catch (Exception ex)
        {
            logData.Add(LogDataKey.EventName, "GetAttachmentFailure");
            _logProvider.LogError(TrackingEvent.GetAttachmentFailure , ex, logData);
            return BadRequest("AttachmentID cannot be null");
        }
    }
}