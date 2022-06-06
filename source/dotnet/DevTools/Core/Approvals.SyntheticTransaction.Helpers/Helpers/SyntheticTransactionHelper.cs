// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace Microsoft.CFS.Approvals.SyntheticTransaction.API.Services
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text.RegularExpressions;
    using System.Threading.Tasks;
    using Microsoft.AspNetCore.Mvc.Infrastructure;
    using Microsoft.Azure.Cosmos.Table;
    using Microsoft.CFS.Approvals.Contracts;
    using Microsoft.CFS.Approvals.Data.Azure.Storage.Interface;
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
              ConfigurationSetting configurationSetting)
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
        }

        /// <summary>
        /// Gets blob text from azure container storage
        /// </summary>
        /// <returns></returns>
        public async Task<string> GetSchemaFile(string blobName)
        {
            var container = _configurationHelper.GetConfigurationValue(ConfigurationKeyEnum.BlobContainerForSchema);
            var name = string.IsNullOrEmpty(blobName) ? _configurationHelper.GetConfigurationValue(ConfigurationKeyEnum.BlobForSchema) : blobName;
            return (await _azureBlobStorageHelper.DownloadText(container, name));
        }

        /// <summary>
        /// Gets blob text from azure container storage
        /// </summary>
        /// <returns></returns>
        public async Task<string> GetUISchemaFile(string blobName)
        {
            var container = _configurationHelper.GetConfigurationValue(ConfigurationKeyEnum.BlobContainerForSchema);
            return !string.IsNullOrEmpty(blobName) ? await _azureBlobStorageHelper.DownloadText(container, blobName) : "";
        }

        /// <summary>
        /// Gets random data for lob app
        /// </summary>
        /// <param name="strJson">Json parameter representing type and value to be generated</param>
        /// <returns>returns updated key value pair dictionary object having random data generated</returns>
        public Dictionary<string, object> GetPlaceholderDetails(string strJson)
        {
            return _randomFormDetails.CreateFormData(strJson);
        }

        /// <summary>
        /// Uploads data to blob
        /// </summary>
        /// <param name="data">data to be uploaded</param>
        public void UploadDataToBlob(string data)
        {
            var container = _configurationHelper.GetConfigurationValue(ConfigurationKeyEnum.BlobContainerForDataUpload);
            var blobName = GenerateFileName();
            _azureBlobStorageHelper.UploadText(data, container, blobName);
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
        /// <returns></returns>
        public async Task<string> GenerateSchemaFromSamplePayload(string payload)
        {
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
                    var approverList = _azureStorageHelper.GetTableEntityByPartitionKey<ConfigurationKeys>("ConfigurationKeys", ConfigurationKey.SyntheticTransactionsApproverAliasList.ToString())?.KeyValue;
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
                throw;
            }
        }

        /// <summary>
        /// Update payload value
        /// </summary>
        /// <param name="payload"></param>
        /// <param name="tenantEntity"></param>
        /// <param name="Approver"></param>
        /// <returns></returns>
        public string UpdatePayloadValue(string payload, ApprovalTenantInfo tenantEntity, string Approver)
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
                    approvalDetails["assignedApprover"] = Approver;
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
        /// <returns></returns>
        public bool InsertSyntheticDetail(string payload, ApprovalTenantInfo tenant, string approver)
        {
            DynamicTableEntity syntheticTransactionEntity;
            JObject jObject = payload.FromJson<JObject>();
            var documentNumber = jObject?.SelectToken("ApprovalIdentifier")?.SelectToken("DisplayDocumentNumber").Value<string>();
            if (jObject?.SelectToken("DetailsData")?.Children() != null)
            {
                foreach (var jtoken in jObject?.SelectToken("DetailsData")?.Children())
                {
                    var key = ((JProperty)jtoken.AsJEnumerable()).Name;
                    var jdata = ((JProperty)jtoken).Value;
                    syntheticTransactionEntity = new DynamicTableEntity
                    {
                        PartitionKey = documentNumber,
                        RowKey = key
                    };
                    syntheticTransactionEntity.Properties["JsonData"].StringValue = jdata.ToString();
                    syntheticTransactionEntity.Properties["AppName"].StringValue = tenant.RowKey;
                    syntheticTransactionEntity.Properties["Approver"].StringValue = approver;
                    _azureStorageHelper.Insert("SyntheticTransactionDetails", syntheticTransactionEntity);
                }
            }
            //Insert Summary
            syntheticTransactionEntity = new DynamicTableEntity
            {
                RowKey = "SUM"
            };
            var summaryData = jObject?.SelectToken("SummaryData")?.Value<JObject>();
            if (tenant.IsPullModelEnabled)
            {
                syntheticTransactionEntity.PartitionKey = jObject?.SelectToken("laborId")?.ToString();
                syntheticTransactionEntity.Properties["JsonData"].StringValue = payload;
            }
            else
            {
                syntheticTransactionEntity.PartitionKey = documentNumber;
                syntheticTransactionEntity.Properties["JsonData"].StringValue = summaryData?.ToString();
            }

            syntheticTransactionEntity.Properties["AppName"].StringValue = tenant.RowKey;
            _azureStorageHelper.Insert("SyntheticTransactionDetails", syntheticTransactionEntity);

            return true;
        }
    }
}