// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace Microsoft.CFS.Approvals.SyntheticTransaction.API.Controllers
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;
    using Microsoft.AspNetCore.Mvc;
    using Microsoft.AspNetCore.Mvc.Infrastructure;
    using Microsoft.Azure.Cosmos.Table;
    using Microsoft.CFS.Approvals.Data.Azure.Storage.Interface;
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

        private readonly string _environment;

        public SyntheticTransactionController(ISyntheticTransactionHelper syntheticTransactionHelper,
            IPayloadReceiverHelper payloadReceiverHelper,
            Func<string, string, ITableHelper> azureStorageHelper,
            IConfiguration configuration,
            IActionContextAccessor actionContextAccessor,
            ConfigurationSetting configurationSetting)
        {
            _environment = actionContextAccessor?.ActionContext?.RouteData?.Values["env"]?.ToString();
            _syntheticTransactionHelper = syntheticTransactionHelper;
            _payloadReceiverHelper = payloadReceiverHelper;
            _azureStorageHelper = azureStorageHelper(
                configurationSetting.appSettings[_environment].StorageAccountName,
                configurationSetting.appSettings[_environment].StorageAccountKey);
            _configuration = configuration;
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
            var schema = _syntheticTransactionHelper.GetSchemaFile(blobName);
            return schema;
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
                _syntheticTransactionHelper.UploadDataToBlob(dataToUpload);
            }
            catch (Exception)
            {
                return StatusCode(500);
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
            string uiSchemaFile = _syntheticTransactionHelper.GetUISchemaFile("UISchema.json").Result;
            string sampleData = _syntheticTransactionHelper.GetSchemaFile(string.Format("{0}.json", tenant)).Result;
            if (string.IsNullOrWhiteSpace(sampleData))
            {
                sampleData = _syntheticTransactionHelper.GetSchemaFile(_configuration["MasterPayload"]).Result;
            }
            if (string.IsNullOrWhiteSpace(sampleData))
            {
                return NotFound(new { message = "Tenant configuration yet to be done. Please Configure selected tenant." });
            }

            ApprovalTenantInfo tenantEntity = (_azureStorageHelper.GetTableEntity<ApprovalTenantInfo>("ApprovalTenantInfo")).Where(x => x.RowKey == tenant).FirstOrDefault();
            string payload = _syntheticTransactionHelper.UpdatePayloadValue(sampleData, tenantEntity, string.Empty);
            string schema = await _syntheticTransactionHelper.GenerateSchemaFromSamplePayload(payload);
            Dictionary<string, object> defaultData = _syntheticTransactionHelper.GetPlaceholderDetails(payload);
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
            return Ok(new { formSchema = JObject.Parse(schema), formData = defaultData, uiSchema = JObject.Parse(uiSchemaFile) });
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
            try
            {
                var tenantEntity = (_azureStorageHelper.GetTableEntity<ApprovalTenantInfo>("ApprovalTenantInfo")).Where(x => x.RowKey == tenant).FirstOrDefault();
                JObject jpayload = JsonConvert.DeserializeObject<JObject>(payload);
                jpayload["DocumentTypeId"] = tenantEntity?.DocTypeId;
                jpayload["Operation"] = 1;
                var summaryData = jpayload?.SelectToken("SummaryData");
                if (summaryData != null)
                {
                    summaryData["DocumentTypeId"] = tenantEntity?.DocTypeId;
                    summaryData["RequestVersion"] = Guid.NewGuid();
                    if (summaryData?.SelectToken("Submitter") != null)
                    {
                        var submitter = summaryData?.SelectToken("Submitter")?.Value<JObject>();
                        submitter["Name"] = string.Empty;
                    }
                }
                if (jpayload?.SelectToken("Approvers") != null)
                {
                    var approvers = jpayload?.SelectToken("Approvers")?.Value<JArray>();
                    for (int i = 0; i < approvers.Children().Count(); i++)
                    {
                        approvers[i]["Name"] = string.Empty;
                    }
                }
                if (jpayload?.SelectToken("Approvers") != null)
                {
                    var approvers = jpayload?.SelectToken("Approvers")?.Value<JArray>();
                    for (int i = 0; i < approvers.Children().Count(); i++)
                    {
                        approvers[i]["Name"] = string.Empty;
                    }
                }
                var actionBy = jpayload?.SelectToken("ActionDetail")?.SelectToken("ActionBy")?.Value<JObject>();
                if (actionBy != null)
                {
                    actionBy["Name"] = string.Empty;
                }
                var result = _payloadReceiverHelper.SendPayload(jpayload?.ToString()).Result;
                var PayloadValidationResults = JsonConvert.DeserializeObject<JObject>(result.Content.ReadAsStringAsync().Result)?.SelectToken("PayloadProcessingResult")?.SelectToken("PayloadValidationResults")?.Value<JArray>();
                if (result.IsSuccessStatusCode && PayloadValidationResults == null)
                {
                    var approvers = jpayload?.SelectToken("Approvers");
                    foreach (var approver in approvers.Children())
                    {
                        DynamicTableEntity testHarnessPayload = new DynamicTableEntity
                        {
                            PartitionKey = approver?["Alias"].ToString(),
                            RowKey = string.Format("{0}|{1}", jpayload?.SelectToken("ApprovalIdentifier")?.SelectToken("DisplayDocumentNumber").Value<string>(), Guid.NewGuid())
                        };
                        testHarnessPayload.Properties["Payload"].StringValue = jpayload?.ToString();
                        testHarnessPayload.Properties["Status"].StringValue = DocumentStatus.Pending.ToString();
                        testHarnessPayload.Properties["TenantID"].StringValue = tenant;
                        _azureStorageHelper.Insert("TestHarnessPayload", testHarnessPayload);
                    }
                    _syntheticTransactionHelper.InsertSyntheticDetail(jpayload?.ToString(), tenantEntity, null);
                }
                return Ok(result);
            }
            catch (Exception)
            {
                throw;
            }
        }
    }
}