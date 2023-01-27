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
using Microsoft.CFS.Approvals.Model;
using Microsoft.CFS.Approvals.SyntheticTransaction.API.Services;
using Microsoft.CFS.Approvals.SyntheticTransaction.Common.Models;
using Microsoft.CFS.Approvals.SyntheticTransaction.Helpers.Helpers;
using Microsoft.CFS.Approvals.SyntheticTransaction.Helpers.Interface;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

/// <summary>
/// Represents Synthetic Transaction controller class
/// </summary>
[ApiController]
[Route("api/v1/SyntheticTransaction")]
public class SyntheticTransactionController : ControllerBase
{
    /// <summary>
    /// The synthetic transaction helper
    /// </summary>
    private readonly ISyntheticTransactionHelper _syntheticTransactionHelper;

    /// <summary>
    /// The payload receiver helper
    /// </summary>
    private readonly IPayloadReceiverHelper _payloadReceiverHelper;

    /// <summary>
    /// The azure storage helper
    /// </summary>
    private readonly ITableHelper _azureStorageHelper;

    /// <summary>
    /// The Configuration helper
    /// </summary>
    private readonly IConfiguration _configuration;

    /// <summary>
    /// The log provider
    /// </summary>
    private readonly ILogProvider _logProvider;

    private readonly string _environment;

    public SyntheticTransactionController(ISyntheticTransactionHelper syntheticTransactionHelper,
        IPayloadReceiverHelper payloadReceiverHelper,
        Func<string, string, ITableHelper> azureStorageHelper,
        IConfiguration configuration,
        IActionContextAccessor actionContextAccessor,
        ConfigurationSetting configurationSetting,
        ILogProvider logProvider)
    {
        _environment = actionContextAccessor?.ActionContext?.RouteData?.Values["env"]?.ToString();
        _syntheticTransactionHelper = syntheticTransactionHelper;
        _payloadReceiverHelper = payloadReceiverHelper;
        _azureStorageHelper = azureStorageHelper(
            configurationSetting.appSettings[_environment].StorageAccountName,
            configurationSetting.appSettings[_environment].StorageAccountKey);
        _configuration = configuration;
        _logProvider = logProvider;
    }

    /// <summary>
    /// Gets blob content for specifed blobname
    /// </summary>
    /// <param name="blobName">It is an optional parameter representing blobname to be fetched</param>
    /// <returns>Blob content as a string</returns>
    [HttpGet]
    [Route("GetBlob/{env}")]
    public Task<string> GetBlob(string blobName = null)
    {
        var tcv = Guid.NewGuid().ToString();
        Dictionary<LogDataKey, object> logData = new Dictionary<LogDataKey, object>();
        logData.Add(LogDataKey.Xcv, tcv);
        logData.Add(LogDataKey.Tcv, tcv);
        logData.Add(LogDataKey.Environment, _environment);
        logData.Add(LogDataKey.BlobName, blobName);
        logData.Add(LogDataKey.Operation, "Get Blob - Controller");
        try
        {
            var schema = _syntheticTransactionHelper.GetSchemaFile(blobName, tcv);
            _logProvider.LogInformation(TrackingEvent.GetBlobSuccessful, logData);
            return schema;
        }
        catch (Exception ex)
        {
            logData.Add(LogDataKey.EventName, "GetBlobFailure");
            _logProvider.LogError(TrackingEvent.GetBlobFailure, ex, logData);
            return null;
        }
    }

    /// <summary>
    /// Uploads file to blob
    /// </summary>
    /// <param name="formData"> contains data to be uploaded to blob</param>
    /// <returns>IActionResult representing upload operation</returns>
    [HttpPost]
    [Route("UploadDataToBlob/{env}")]
    public IActionResult UploadDataToBlob([FromBody] string formData)
    {
        var tcv = Guid.NewGuid().ToString();
        Dictionary<LogDataKey, object> logData = new Dictionary<LogDataKey, object>();
        logData.Add(LogDataKey.Xcv, tcv);
        logData.Add(LogDataKey.Tcv, tcv);
        logData.Add(LogDataKey.ComponentName, "API");
        logData.Add(LogDataKey.MSAComponentName, "TestHarness");
        logData.Add(LogDataKey.Environment, _environment);
        logData.Add(LogDataKey.Operation, "Upload Data to Blob - Controller");

        try
        {
            JArray array = new JArray
                {
                    JObject.Parse(formData)
                };

            JObject jobj = new JObject
                {
                    { "creditCardTransactions", array }
                };
            var dataToUpload = JsonConvert.SerializeObject(jobj, Formatting.Indented);
            _syntheticTransactionHelper.UploadDataToBlob(dataToUpload, tcv);
            _logProvider.LogInformation(TrackingEvent.UploadToBlobSuccessful, logData);
        }
        catch (Exception ex)
        {
            logData.Add(LogDataKey.EventName, "UploadToBlobFailure");
            _logProvider.LogError(TrackingEvent.UploadToBlobFailure, ex, logData);
            return BadRequest(ex.Message);
        }

        return Ok();
    }

    /// <summary>
    /// Generate form
    /// </summary>
    /// <param name="tenant"></param>
    /// <param name="approver"></param>
    /// <param name="submitter"></param>
    /// <returns></returns>
    [HttpGet]
    [Route("GenerateForm/{env}")]
    public async Task<IActionResult> GenerateForm(string tenant = null, string approver = null, string submitter = null)
    {
        var tcv = Guid.NewGuid().ToString();
        Dictionary<LogDataKey, object> logData = new Dictionary<LogDataKey, object>();
        logData.Add(LogDataKey.Xcv, tcv);
        logData.Add(LogDataKey.Tcv, tcv);
        logData.Add(LogDataKey.ComponentName, "API");
        logData.Add(LogDataKey.MSAComponentName, "TestHarness");
        logData.Add(LogDataKey.Environment, _environment);
        logData.Add(LogDataKey.TenantName, tenant);
        logData.Add(LogDataKey.UserAlias, approver);
        logData.Add(LogDataKey.Submitter, submitter);
        logData.Add(LogDataKey.Operation, "Generate Form - Controller");

        try
        {
            string uiSchemaFile = _syntheticTransactionHelper.GetUISchemaFile("UISchema.json", tcv).Result;
            string sampleData = _syntheticTransactionHelper.GetSchemaFile(string.Format("{0}.json", tenant), tcv).Result;
            if (string.IsNullOrWhiteSpace(sampleData))
            {
                sampleData = _syntheticTransactionHelper.GetSchemaFile(_configuration["MasterPayload"], tcv).Result;
                _logProvider.LogInformation(TrackingEvent.SampleDataFetchForMasterPayload, logData);
            }
            else
                _logProvider.LogInformation(TrackingEvent.SampleDataFetchForTenantPayload, logData);

            if (string.IsNullOrWhiteSpace(sampleData))
            {
                _logProvider.LogInformation(TrackingEvent.MasterPayloadSampleDataFetchFailure, logData);
                return NotFound(new { message = "Tenant configuration yet to be done. Please Configure selected tenant." });
            }

            ApprovalTenantInfo tenantEntity = _azureStorageHelper.GetTableEntityByRowKey<ApprovalTenantInfo>("ApprovalTenantInfo", tenant);
            string payload = _syntheticTransactionHelper.UpdatePayloadValue(sampleData, tenantEntity, string.Empty, tcv);
            string schema = await _syntheticTransactionHelper.GenerateSchemaFromSamplePayload(payload, tcv);
            Dictionary<string, object> defaultData = _syntheticTransactionHelper.GetPlaceholderDetails(payload, tcv);
            defaultData["DocumentTypeId"] = null;
            defaultData["Operation"] = null;

            if (!string.IsNullOrEmpty(approver) && defaultData["Approvers"].GetType() == typeof(JArray))
            {
                JArray approvers = JArray.Parse(JsonConvert.SerializeObject(defaultData["Approvers"]));
                approvers[0]["Alias"] = approver;
                defaultData["Approvers"] = approvers;
            }

            object summaryData = defaultData["SummaryData"];
            if (summaryData != null)
            {
                string json = JsonConvert.SerializeObject(summaryData);
                Dictionary<string, object> dictionary = JsonConvert.DeserializeObject<Dictionary<string, object>>(json);
                if (!string.IsNullOrEmpty(submitter) && dictionary["Submitter"].GetType() == typeof(JObject))
                {
                    JObject sbmtr = JObject.Parse(JsonConvert.SerializeObject(dictionary["Submitter"]));
                    sbmtr["Alias"] = submitter;
                    dictionary["Submitter"] = sbmtr;
                }
                dictionary["DocumentTypeId"] = null;
                dictionary["RequestVersion"] = null;
                defaultData["SummaryData"] = dictionary;
            }
            _logProvider.LogInformation(TrackingEvent.GenerateFormSuccessful, logData);
            return Ok(new { formSchema = JObject.Parse(schema), formData = defaultData, uiSchema = JObject.Parse(uiSchemaFile) });
        }
        catch (Exception ex)
        {
            logData.Add(LogDataKey.EventName, "GenerateFormFailure");
            _logProvider.LogError(TrackingEvent.GenerateFormFailure, ex, logData);
            return BadRequest(ex.Message);
        }
    }

    /// <summary>
    /// Submit payload
    /// </summary>
    /// <param name="payload"></param>
    /// <param name="tenant"></param>
    /// <returns></returns>
    [HttpPost]
    [Route("SubmitPayload/{env}")]
    public async Task<IActionResult> SubmitPayload([FromBody] string payload, string tenant)
    {
        var tcv = Guid.NewGuid().ToString();
        Dictionary<LogDataKey, object> logData = new Dictionary<LogDataKey, object>();
        logData.Add(LogDataKey.Xcv, tcv);
        logData.Add(LogDataKey.Tcv, tcv);
        logData.Add(LogDataKey.ComponentName, "API");
        logData.Add(LogDataKey.MSAComponentName, "TestHarness");
        logData.Add(LogDataKey.Environment, _environment);
        logData.Add(LogDataKey.TenantName, tenant);
        logData.Add(LogDataKey.PayloadType, "Create");
        logData.Add(LogDataKey.Operation, "Submit Payload - Controller");

        try
        {
            var tenantEntity = _azureStorageHelper.GetTableEntityByRowKey<ApprovalTenantInfo>("ApprovalTenantInfo", tenant);
            JObject jpayload = JsonConvert.DeserializeObject<JObject>(payload);
            jpayload["DocumentTypeId"] = tenantEntity.DocTypeId;
            jpayload["Operation"] = 1;
            var summaryData = jpayload?.SelectToken("SummaryData");
            if (summaryData != null)
            {
                summaryData["DocumentTypeId"] = tenantEntity.DocTypeId;
                summaryData["RequestVersion"] = Guid.NewGuid();
                if (summaryData?.SelectToken("Submitter") != null)
                {
                    var submitter = summaryData?.SelectToken("Submitter")?.Value<JObject>();
                    submitter["Name"] = string.Empty;
                    logData.Add(LogDataKey.Submitter, submitter["Alias"].ToString());
                }
            }
            if (jpayload?.SelectToken("Approvers") != null)
            {
                var approvers = jpayload?.SelectToken("Approvers")?.Value<JArray>();
                for (int i = 0; i < approvers.Children().Count(); i++)
                {
                    approvers[i]["Name"] = string.Empty;
                }
                logData.Add(LogDataKey.UserAlias, approvers[0]["Alias"].ToString());
            }
            var actionBy = jpayload?.SelectToken("ActionDetail")?.SelectToken("ActionBy")?.Value<JObject>();
            if (actionBy != null)
            {
                actionBy["Name"] = string.Empty;
            }
            logData.Add(LogDataKey.DocumentNumber, jpayload?.SelectToken("ApprovalIdentifier")?.SelectToken("DisplayDocumentNumber").Value<string>());
            var result = _payloadReceiverHelper.SendPayload(jpayload?.ToString(), tcv).Result;

            var PayloadValidationResults = JsonConvert.DeserializeObject<JObject>(result.Content.ReadAsStringAsync().Result)?.SelectToken("PayloadProcessingResult")?.SelectToken("PayloadValidationResults")?.Value<JArray>();
            if (result.IsSuccessStatusCode && PayloadValidationResults == null)
            {
                _logProvider.LogInformation(TrackingEvent.SendPayloadSuccessful, logData);
                var approvers = jpayload?.SelectToken("Approvers");
                foreach (var approver in approvers.Children())
                {
                    TestHarnessDocument testHarnessPayload = new TestHarnessDocument
                    {
                        PartitionKey = approver?["Alias"].ToString(),
                        RowKey = string.Format("{0}|{1}", jpayload?.SelectToken("ApprovalIdentifier")?.SelectToken("DisplayDocumentNumber").Value<string>(), Guid.NewGuid()),
                        Payload = jpayload?.ToString(),
                        Status = DocumentStatus.Pending.ToString(),
                        TenantID = tenant
                    };
                    await _azureStorageHelper.InsertOrReplace("TestHarnessPayload", testHarnessPayload);
                    _logProvider.LogInformation(TrackingEvent.SavePayloadSuccessful, logData);
                }
                await _syntheticTransactionHelper.InsertSyntheticDetail(jpayload?.ToString(), tenantEntity, null, tcv);
            }
            else
            {
                logData.Add(LogDataKey.PayloadResult, result.ToString());
                _logProvider.LogInformation(TrackingEvent.SendPayloadFailure, logData);
            }
            return Ok(result);
        }
        catch (Exception ex)
        {
            logData.Add(LogDataKey.EventName, "SendPayloadFailure");
            _logProvider.LogError(TrackingEvent.SendPayloadFailure, ex, logData);
            return BadRequest(ex.Message);
        }
    }
}