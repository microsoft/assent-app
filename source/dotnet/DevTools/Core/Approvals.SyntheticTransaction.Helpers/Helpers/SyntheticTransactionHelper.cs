// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace Microsoft.CFS.Approvals.SyntheticTransaction.API.Services;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using Microsoft.CFS.Approvals.Data.Azure.Storage.Interface;
using Microsoft.CFS.Approvals.DevTools.Model.Constant;
using Microsoft.CFS.Approvals.LogManager.Provider.Interface;
using Microsoft.CFS.Approvals.Model;
using Microsoft.CFS.Approvals.SyntheticTransaction.Common.Helper;
using Microsoft.CFS.Approvals.SyntheticTransaction.Common.Interface;
using Microsoft.CFS.Approvals.SyntheticTransaction.Common.Models;
using Microsoft.CFS.Approvals.SyntheticTransaction.Helpers.ExtensionMethods;
using Microsoft.CFS.Approvals.SyntheticTransaction.Helpers.Helpers;
using Microsoft.CFS.Approvals.SyntheticTransaction.Helpers.Interface;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

/// <summary>
/// Synthetic Transaction Helper class
/// </summary>
public class SyntheticTransactionHelper : ISyntheticTransactionHelper
{
    /// <summary>
    /// The random form details
    /// </summary>
    private readonly IRandomFormDetails _randomFormDetails;

    /// <summary>
    /// The blob storage helper
    /// </summary>
    private readonly IBlobStorageHelper _azureBlobStorageHelper;

    /// <summary>
    /// The azure storage helper
    /// </summary>
    private readonly ITableHelper _azureStorageHelper;

    /// <summary>
    /// The schema generator
    /// </summary>
    private readonly ISchemaGenerator _schemaGenerator;

    /// <summary>
    /// The log provider
    /// </summary>
    private readonly ILogProvider _logProvider;

    private readonly ConfigurationHelper _configurationHelper;
    private readonly string _environment;

    /// <summary>
    /// Constructor of SyntheticTransactionHelper
    /// </summary>
    /// <param name="randomFormDetails"></param>
    /// <param name="blobStorageHelper"></param>
    /// <param name="configurationHelper"></param>
    /// <param name="azureStorageHelper"></param>
    /// <param name="actionContextAccessor"></param>
    /// <param name="schemaGenerator"></param>
    /// <param name="configurationSetting"></param>
    public SyntheticTransactionHelper(IRandomFormDetails randomFormDetails,
        Func<string, string, IBlobStorageHelper> blobStorageHelper,
        ConfigurationHelper configurationHelper,
        Func<string, string, ITableHelper> azureStorageHelper,
        IActionContextAccessor actionContextAccessor,
        ISchemaGenerator schemaGenerator,
        ConfigurationSetting configurationSetting,
        ILogProvider logProvider)
    {
        _environment = actionContextAccessor?.ActionContext?.RouteData?.Values["env"]?.ToString();
        _randomFormDetails = randomFormDetails;
        _azureBlobStorageHelper = blobStorageHelper(
            configurationSetting.appSettings[_environment].StorageAccountName,
            configurationSetting.appSettings[_environment].StorageAccountKey);
        _configurationHelper = configurationHelper;
        _azureStorageHelper = azureStorageHelper(
            configurationSetting.appSettings[_environment].StorageAccountName,
            configurationSetting.appSettings[_environment].StorageAccountKey);
        _schemaGenerator = schemaGenerator;
        _logProvider = logProvider;
    }

    /// <summary>
    /// Gets blob text from azure container storage
    /// </summary>
    /// <param name="blobName"></param>
    /// <param name="tcv"></param>
    /// <returns></returns>
    public async Task<string> GetSchemaFile(string blobName, string tcv)
    {
        Dictionary<LogDataKey, object> logData = new Dictionary<LogDataKey, object>();
        logData.Add(LogDataKey.Xcv, tcv);
        logData.Add(LogDataKey.Tcv, tcv);
        logData.Add(LogDataKey.Environment, _environment);
        logData.Add(LogDataKey.BlobName, blobName);
        logData.Add(LogDataKey.Operation, "Get Schema File - Helper");

        try
        {
            var container = _configurationHelper.GetConfigurationValue(ConfigurationKeyEnum.BlobContainerForSchema);
            var name = string.IsNullOrEmpty(blobName) ? _configurationHelper.GetConfigurationValue(ConfigurationKeyEnum.BlobForSchema) : blobName;
            return (await _azureBlobStorageHelper.DownloadText(container, name));
        }
        catch (Exception ex)
        {
            logData.Add(LogDataKey.EventName, "GetSchemaFileFailure");
            _logProvider.LogError(TrackingEvent.GetSchemaFileFailure, ex, logData);
            return null;
        }
    }

    /// <summary>
    /// Gets blob text from azure container storage
    /// </summary>
    /// <param name="blobName"></param>
    /// <param name="tcv"></param>
    /// <returns></returns>
    public async Task<string> GetUISchemaFile(string blobName, string tcv)
    {
        var container = _configurationHelper.GetConfigurationValue(ConfigurationKeyEnum.BlobContainerForSchema);
        return !string.IsNullOrEmpty(blobName) ? await _azureBlobStorageHelper.DownloadText(container, blobName) : "";
    }

    /// <summary>
    /// Gets random data for lob app
    /// </summary>
    /// <param name="strJson">Json parameter representing type and value to be generated</param>
    /// <param name="tcv"></param>
    /// <returns>returns updated key value pair dictionary object having random data generated</returns>
    public Dictionary<string, object> GetPlaceholderDetails(string strJson, string tcv)
    {
        Dictionary<LogDataKey, object> logData = new Dictionary<LogDataKey, object>();
        logData.Add(LogDataKey.Xcv, tcv);
        logData.Add(LogDataKey.Tcv, tcv);
        logData.Add(LogDataKey.Environment, _environment);
        logData.Add(LogDataKey.Operation, "Get Placeholder details - Helper");
        try
        {
            return _randomFormDetails.CreateFormData(strJson);
        }
        catch (Exception ex)
        {
            logData.Add(LogDataKey.EventName, "GetUISchemaFileFailure");
            _logProvider.LogError(TrackingEvent.GetUISchemaFileFailure, ex, logData);
            return null;
        }
    }

    /// <summary>
    /// Uploads data to blob
    /// </summary>
    /// <param name="data">data to be uploaded</param>
    /// <param name="tcv"></param>
    public void UploadDataToBlob(string data, string tcv)
    {
        Dictionary<LogDataKey, object> logData = new Dictionary<LogDataKey, object>();
        logData.Add(LogDataKey.Xcv, tcv);
        logData.Add(LogDataKey.Tcv, tcv);
        logData.Add(LogDataKey.Environment, _environment);
        logData.Add(LogDataKey.Operation, "Upload Data to Blob - Helper");
        try
        {

            var container = _configurationHelper.GetConfigurationValue(ConfigurationKeyEnum.BlobContainerForDataUpload);
            var blobName = GenerateFileName();
            _azureBlobStorageHelper.UploadText(data, container, blobName);
        }
        catch (Exception ex)
        {
            logData.Add(LogDataKey.EventName, "UploadDataToBlobFailure");
            _logProvider.LogError(TrackingEvent.UploadDataToBlobFailure, ex, logData);
            throw;
        }
    }

    /// <summary>
    /// Generates unique file name
    /// </summary>
    /// <returns>Returns string representing unique file name</returns>
    private string GenerateFileName()
    {
        var prefix = _configurationHelper.GetConfigurationValue(ConfigurationKeyEnum.PrefixForBlobName);
        return string.Format(@"{0}_{1}.json", prefix, DateTime.Now.Ticks);
    }

    /// <summary>
    /// Gets blob text from azure container storage
    /// </summary>
    /// <param name="payload"></param>
    /// <param name="tcv"></param>
    /// <returns></returns>
    public async Task<string> GenerateSchemaFromSamplePayload(string payload, string tcv)
    {
        Dictionary<LogDataKey, object> logData = new Dictionary<LogDataKey, object>();
        logData.Add(LogDataKey.Xcv, tcv);
        logData.Add(LogDataKey.Tcv, tcv);
        logData.Add(LogDataKey.Environment, _environment);
        logData.Add(LogDataKey.Operation, "Generate Schema From Sample Payload - Helper");

        try
        {
            var jobject = JsonConvert.DeserializeObject<JObject>(payload);
            foreach (var jtoken in jobject.SelectToken("DetailsData").Children())
            {
                var key = ((JProperty)jtoken.AsJEnumerable()).Name;
                var jdata = ((JProperty)jtoken).Value;
                jobject["DetailsData"][key] = (JToken)JsonConvert.DeserializeObject(jdata.ToString());
            }
            var schemaFromFile = _schemaGenerator.Generate(jobject.ToString());

            var result = JObject.Parse(schemaFromFile.ToJson(Formatting.None));
            var properties = result?.SelectToken("properties")?.Value<JObject>();
            if (properties != null)
            {
                var DocumentTypeId = properties?.SelectToken("DocumentTypeId")?.Value<JObject>();
                if (DocumentTypeId != null)
                {
                    DocumentTypeId["type"] = "null";
                }
                var Operation = properties?.SelectToken("Operation")?.Value<JObject>();
                if (Operation != null)
                {
                    Operation["type"] = "null";
                }

                var approverJArray = new JArray();
                var approverList = _azureStorageHelper.GetTableEntityByPartitionKey<ConfigurationKeys>("ConfigurationKeys", ConfigurationKey.TestHarnessApproverAlias.ToString())?.KeyValue;
                foreach (var approver in approverList.Split(";"))
                {
                    var approveJObject = new JObject
                        {
                            { "type", "string" },
                            { "title", approver },
                            { "enum", new JArray { approver } }
                        };
                    approverJArray.Add(approveJObject);
                }
                var approvers = properties?.SelectToken("Approvers")?.SelectToken("items")?.SelectToken("properties")?.Value<JObject>();
                if (approvers != null)
                {
                    var alias = approvers?.SelectToken("Alias")?.Value<JObject>();
                    if (alias != null)
                    {
                        alias.Add("anyOf", approverJArray);
                        alias.Add("title", "Alias");
                    }
                    if (approvers?.SelectToken("Name") != null)
                        approvers.Property("Name").Remove();
                }
                var summaryProperties = properties?.SelectToken("SummaryData")?.Value<JObject>();
                if (summaryProperties != null)
                {
                    var documentTypeId = summaryProperties?.SelectToken("properties")?.SelectToken("DocumentTypeId")?.Value<JObject>();
                    if (documentTypeId != null)
                    {
                        documentTypeId["type"] = "null";
                    }

                    var requestVersion = summaryProperties?.SelectToken("properties")?.SelectToken("RequestVersion")?.Value<JObject>();
                    if (requestVersion != null)
                    {
                        requestVersion["type"] = "null";
                    }

                    var submitter = summaryProperties?.SelectToken("properties")?.SelectToken("Submitter")?.SelectToken("properties")?.Value<JObject>();
                    if (submitter != null)
                    {
                        var alias = submitter?.SelectToken("Alias")?.Value<JObject>();
                        if (alias != null)
                        {
                            alias.Add("anyOf", approverJArray);
                            alias.Add("title", "Alias");
                        }
                        if (submitter?.SelectToken("Name") != null)
                            submitter.Property("Name").Remove();
                    }
                    var ApprovalHierarchy = summaryProperties?.SelectToken("properties")?
                    .SelectToken("ApprovalHierarchy")?
                    .SelectToken("items")?.SelectToken("properties")?
                    .SelectToken("Approvers")?
                    .SelectToken("items")?.SelectToken("properties")?.Value<JObject>();
                    if (ApprovalHierarchy != null)
                    {
                        var alias = ApprovalHierarchy?.SelectToken("Alias")?.Value<JObject>();
                        if (alias != null)
                        {
                            alias.Add("anyOf", approverJArray);
                            alias.Add("title", "Alias");
                        }
                        if (ApprovalHierarchy?.SelectToken("Name") != null)
                            ApprovalHierarchy.Property("Name").Remove();
                    }
                }
                var actionDetails = properties?.SelectToken("ActionDetail")?.Value<JObject>();
                if (actionDetails != null)
                {
                    var actionBy = actionDetails?.SelectToken("properties")?.SelectToken("ActionBy")?.SelectToken("properties")?.Value<JObject>();
                    if (actionBy != null)
                    {
                        var alias = actionBy?.SelectToken("Alias")?.Value<JObject>();
                        if (alias != null)
                        {
                            alias.Add("anyOf", approverJArray);
                            alias.Add("title", "Alias");
                        }
                        if (actionBy?.SelectToken("Name") != null)
                            actionBy.Property("Name").Remove();
                    }
                }
            }

            result.Property("$schema").Remove();
            return result.ToString();
        }
        catch (Exception ex)
        {
            logData.Add(LogDataKey.EventName, "GetSchemaFromSamplePayloadFailure");
            _logProvider.LogError(TrackingEvent.GetSchemaFromSamplePayloadFailure, ex, logData);
            return null;
        }
    }

    /// <summary>
    /// Update payload value
    /// </summary>
    /// <param name="payload"></param>
    /// <param name="tenantEntity"></param>
    /// <param name="approver"></param>
    /// <param name="tcv"></param>
    /// <returns></returns>
    public string UpdatePayloadValue(string payload, ApprovalTenantInfo tenantEntity, string approver, string tcv)
    {
        Dictionary<LogDataKey, object> logData = new Dictionary<LogDataKey, object>();
        logData.Add(LogDataKey.Xcv, tcv);
        logData.Add(LogDataKey.Tcv, tcv);
        logData.Add(LogDataKey.UserAlias, approver);
        logData.Add(LogDataKey.Environment, _environment);
        logData.Add(LogDataKey.Operation, "Upload Payload Value - Helper");

        try
        {
            var jobject = JsonConvert.DeserializeObject<JObject>(payload);
            if (tenantEntity.IsPullModelEnabled)
            {
                if (jobject?.SelectToken("laborId") != null)
                {
                    jobject["laborId"] = Guid.NewGuid();
                }
                if (jobject?.SelectToken("laborDate") != null)
                {
                    jobject["laborDate"] = DateTime.UtcNow;
                }
                if (jobject?.SelectToken("approvalDetails")?.SelectToken("assignedApprover") != null)
                {
                    var approvalDetails = jobject?.SelectToken("approvalDetails")?.Value<JObject>();
                    approvalDetails["assignedApprover"] = approver;
                }
            }
            else
            {
                jobject["DocumentTypeId"] = tenantEntity.DocTypeId;
                string preFix = String.IsNullOrEmpty(tenantEntity.DocumentNumberPrefix) ? tenantEntity.TemplateName : tenantEntity.DocumentNumberPrefix;
                string DocumentNumber = string.Format("{0}-{1}", Regex.Replace(preFix, @"\s+", ""), _randomFormDetails.RandomNumber(0));
                if (jobject?.SelectToken("ApprovalIdentifier") != null)
                {
                    var ApprovalIdentifier = jobject?.SelectToken("ApprovalIdentifier")?.Value<JObject>();
                    ApprovalIdentifier["DisplayDocumentNumber"] = DocumentNumber;
                    ApprovalIdentifier["DocumentNumber"] = DocumentNumber;
                }
                if (jobject?.SelectToken("SummaryData") != null)
                {
                    var summaryData = jobject?.SelectToken("SummaryData")?.Value<JObject>();
                    summaryData["DocumentTypeId"] = tenantEntity.DocTypeId;
                    if (summaryData?.SelectToken("ApprovalIdentifier") != null)
                    {
                        var approvalIdentifier = summaryData?.SelectToken("ApprovalIdentifier")?.Value<JObject>();
                        approvalIdentifier["DisplayDocumentNumber"] = DocumentNumber;
                        approvalIdentifier["DocumentNumber"] = DocumentNumber;
                    }
                }
                if (jobject?.SelectToken("Telemetry") != null)
                {
                    var telemetry = jobject?.SelectToken("Telemetry")?.Value<JObject>();
                    if (telemetry != null)
                    {
                        telemetry["Xcv"] = DocumentNumber;
                        telemetry["Tcv"] = Guid.NewGuid();
                        telemetry["BusinessProcessName"] = tenantEntity.BusinessProcessName;
                    }
                }
                if (jobject?.SelectToken("NotificationDetail")?.SelectToken("Reminder")?.SelectToken("Expiration") != null)
                {
                    var reminder = jobject?.SelectToken("NotificationDetail")?.SelectToken("Reminder")?.Value<JObject>();
                    if (reminder != null)
                    {
                        reminder["Expiration"] = DateTime.UtcNow;
                    }
                }
            }
            return jobject.ToString();
        }
        catch (Exception ex)
        {
            logData.Add(LogDataKey.EventName, "UploadPayloadValueFailure");
            _logProvider.LogError(TrackingEvent.UploadPayloadValueFailure, ex, logData);
            return null;
        }
    }

    /// <summary>
    /// Get environment name
    /// </summary>
    /// <param name="envNames"></param>
    public void GetEnvironmentName(ref List<string> envNames)
    {
        var environmentNames = _configurationHelper.GetConfigurationValue(ConfigurationKeyEnum.EnvironmentList);
        envNames = environmentNames.Split(',').ToList();
    }

    /// <summary>
    /// Insert synthetic detail
    /// </summary>
    /// <param name="payload"></param>
    /// <param name="tenant"></param>
    /// <param name="approver"></param>
    /// <param name="tcv"></param>
    /// <returns></returns>
    public async Task<bool> InsertSyntheticDetail(string payload, ApprovalTenantInfo tenant, string approver, string tcv)
    {
        Dictionary<LogDataKey, object> logData = new Dictionary<LogDataKey, object>();
        logData.Add(LogDataKey.Tcv, tcv);
        logData.Add(LogDataKey.UserAlias, approver);
        logData.Add(LogDataKey.Environment, _environment);
        logData.Add(LogDataKey.Operation, "Insert Synthetic Detail - Helper");

        SyntheticTransactionEntity syntheticTransactionEntity;
        JObject jObject = payload.FromJson<JObject>();
        var documentNumber = jObject?.SelectToken("ApprovalIdentifier")?.SelectToken("DisplayDocumentNumber").Value<string>();
        logData.Add(LogDataKey.DocumentNumber, documentNumber);
        logData.Add(LogDataKey.Xcv, documentNumber);

        try
        {
            if (jObject?.SelectToken("DetailsData")?.Children() != null)
            {
                foreach (var jtoken in jObject?.SelectToken("DetailsData")?.Children())
                {
                    var key = ((JProperty)jtoken.AsJEnumerable()).Name;
                    var jdata = ((JProperty)jtoken).Value;
                    syntheticTransactionEntity = new SyntheticTransactionEntity();
                    syntheticTransactionEntity.PartitionKey = documentNumber;
                    syntheticTransactionEntity.RowKey = key;
                    syntheticTransactionEntity.JsonData = jdata.ToString();
                    syntheticTransactionEntity.AppName = tenant.RowKey;
                    syntheticTransactionEntity.Approver = approver;
                    _azureStorageHelper.InsertOrReplace<SyntheticTransactionEntity>("SyntheticTransactionDetails", syntheticTransactionEntity);
                }
            }
            //Insert Summary
            syntheticTransactionEntity = new SyntheticTransactionEntity();
            var summaryData = jObject?.SelectToken("SummaryData")?.Value<JObject>();
            if (tenant.IsPullModelEnabled)
            {
                syntheticTransactionEntity.PartitionKey = jObject?.SelectToken("laborId")?.ToString();
                syntheticTransactionEntity.JsonData = payload;
            }
            else
            {
                syntheticTransactionEntity.PartitionKey = documentNumber;
                syntheticTransactionEntity.JsonData = summaryData?.ToString();
            }
            syntheticTransactionEntity.RowKey = "SUM";
            syntheticTransactionEntity.AppName = tenant.RowKey;
            await _azureStorageHelper.InsertOrReplace<SyntheticTransactionEntity>("SyntheticTransactionDetails", syntheticTransactionEntity);

            return true;
        }
        catch (Exception ex)
        {
            logData.Add(LogDataKey.EventName, "InsertSyntheticDetailFailure");
            _logProvider.LogError(TrackingEvent.InsertSyntheticDetailFailure, ex, logData);
            return false;
        }
    }
}