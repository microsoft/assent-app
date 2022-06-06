// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace Microsoft.CFS.Approvals.SyntheticTransaction.Helpers.Helpers
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;
    using Microsoft.AspNetCore.Mvc.Infrastructure;
    using Microsoft.Azure.Cosmos.Table;
    using Microsoft.CFS.Approvals.Contracts;
    using Microsoft.CFS.Approvals.Data.Azure.Storage.Interface;
    using Microsoft.CFS.Approvals.Model;
    using Microsoft.CFS.Approvals.SyntheticTransaction.API.Services;
    using Microsoft.CFS.Approvals.SyntheticTransaction.Common.Models;
    using Microsoft.CFS.Approvals.SyntheticTransaction.Helpers.Interface;
    using Newtonsoft.Json;
    using Newtonsoft.Json.Linq;

    /// <summary>
    /// Load Generator Helper class
    /// </summary>
    public class LoadGeneratorHelper : ILoadGeneratorHelper
    {
        /// <summary>
        /// The synthetic transaction helper
        /// </summary>
        private readonly ISyntheticTransactionHelper _syntheticTransactionHelper;

        /// <summary>
        /// The paylod receiver helper
        /// </summary>
        private readonly IPayloadReceiverHelper _payloadReceiverHelper;

        /// <summary>
        /// The azure storage helper
        /// </summary>
        private readonly ITableHelper _azureStorageHelper;

        private readonly string _environment;

        /// <summary>
        /// Constructor of LoadGeneratorHelper
        /// </summary>
        /// <param name="syntheticTransactionHelper"></param>
        /// <param name="payloadReceiverHelper"></param>
        /// <param name="azureStorageHelper"></param>
        /// <param name="actionContextAccessor"></param>
        /// <param name="configurationSetting"></param>
        public LoadGeneratorHelper(
            ISyntheticTransactionHelper syntheticTransactionHelper,
            IPayloadReceiverHelper payloadReceiverHelper,
            Func<string, string, ITableHelper> azureStorageHelper,
            IActionContextAccessor actionContextAccessor,
            ConfigurationSetting configurationSetting)
        {
            _environment = actionContextAccessor?.ActionContext?.RouteData?.Values["env"]?.ToString();
            _syntheticTransactionHelper = syntheticTransactionHelper;
            _payloadReceiverHelper = payloadReceiverHelper;
            _azureStorageHelper = azureStorageHelper(
                configurationSetting.appSettings[_environment].StorageAccountName,
                configurationSetting.appSettings[_environment].StorageAccountKey);
        }

        /// <summary>
        /// Generate load
        /// </summary>
        /// <param name="tenant"></param>
        /// <param name="approver"></param>
        /// <param name="load"></param>
        /// <param name="batchsize"></param>
        /// <param name="samplePayload"></param>
        /// <returns></returns>
        public async Task<object> GenerateLoad(string tenant, string approver, int load, int batchsize, string samplePayload)
        {
            string successDocuments = string.Empty;
            int numberofTaskToProccessInBatch = 1;
            var tenantEntity = (_azureStorageHelper.GetTableEntity<ApprovalTenantInfo>("ApprovalTenantInfo")).Where(x => x.RowKey == tenant).FirstOrDefault();
            List<Task<string>> tasks = new List<Task<string>>();
            for (int i = 1; i <= load; i++)
            {
                if (numberofTaskToProccessInBatch < batchsize)
                {
                    Task<string> task = Task.Run(() => CreateRequest(samplePayload, tenantEntity, approver));
                    tasks.Add(task);
                    numberofTaskToProccessInBatch++;
                }
                else
                {
                    numberofTaskToProccessInBatch = 1;
                    Task<string> task = Task.Run(() => CreateRequest(samplePayload, tenantEntity, approver));
                    tasks.Add(task);

                    // If TestHarnessLoadBatchDelay is 0, delay is not in effect. Otherwise, the value given is in seconds.
                    // For ex: if TestHarnessLoadBatchDelay has a value of 60, it sleeps for 60 seconds

                    var testHarnessLoadBatchDelay = _azureStorageHelper.GetTableEntityListByPartitionKey<ConfigurationKeys>("ConfigurationKeys", ConfigurationKey.SyntheticTransactionsLoadBatchDelay.ToString()).FirstOrDefault()?.KeyValue;
                    if (int.Parse(testHarnessLoadBatchDelay) > 0)
                    {
                        Thread.Sleep((int)(1000 * int.Parse(testHarnessLoadBatchDelay)));
                        successDocuments = "Successfully submitted " + Convert.ToString(i) + "requests";
                    }
                    else
                    {
                        Task.WaitAll(tasks.ToArray());

                        foreach (var result in tasks)
                        {
                            successDocuments = successDocuments + result.Result + ", ";
                            result.Dispose();
                        }
                    }

                    tasks = new List<Task<string>>();
                }
            }
            return new { SuccessDocuments = successDocuments };
        }

        /// <summary>
        /// Create request
        /// </summary>
        /// <param name="payload"></param>
        /// <param name="tenantEntity"></param>
        /// <param name="approver"></param>
        /// <returns></returns>
        private string CreateRequest(string payload, ApprovalTenantInfo tenantEntity, string approver)
        {
            string documentNumber = string.Empty;
            var generatePayload = _syntheticTransactionHelper.UpdatePayloadValue(payload, tenantEntity, approver);
            if ((bool)tenantEntity.IsPullModelEnabled)
            {
                var jpayload = JsonConvert.DeserializeObject<JObject>(generatePayload);
                documentNumber = jpayload?.SelectToken("laborId")?.ToString();
                _syntheticTransactionHelper.InsertSyntheticDetail(generatePayload, tenantEntity, approver);
                DynamicTableEntity testTenantSummaryEntity = new DynamicTableEntity
                {
                    PartitionKey = approver,
                    RowKey = string.Format("{0}-{1}", Guid.NewGuid(), documentNumber)
                };
                testTenantSummaryEntity.Properties["JsonData"].StringValue = generatePayload;
                testTenantSummaryEntity.Properties["Approver"].StringValue = approver;
                testTenantSummaryEntity.Properties["TenantID"].StringValue = tenantEntity.RowKey;
                _azureStorageHelper.Insert("TenantSummaryData", testTenantSummaryEntity);
            }
            else
            {
                generatePayload = UpdatePayloadApprover(generatePayload, approver);
                generatePayload = JsonConvert.SerializeObject(_syntheticTransactionHelper.GetPlaceholderDetails(generatePayload));
                var result = _payloadReceiverHelper.SendPayload(generatePayload).Result;
                var PayloadValidationResults = JsonConvert.DeserializeObject<JObject>(result.Content.ReadAsStringAsync().Result)?.SelectToken("PayloadProcessingResult")?.SelectToken("PayloadValidationResults")?.Value<JArray>();
                JObject jObject = JsonConvert.DeserializeObject<JObject>(generatePayload);
                documentNumber = jObject?.SelectToken("ApprovalIdentifier")?.SelectToken("DisplayDocumentNumber")?.Value<string>();
                if (result.IsSuccessStatusCode && PayloadValidationResults == null)
                {
                    DynamicTableEntity testHarnessPayload = new DynamicTableEntity
                    {
                        PartitionKey = approver,
                        RowKey = string.Format("{0}|{1}", documentNumber, Guid.NewGuid())
                    };
                    testHarnessPayload.Properties["Payload"].StringValue = generatePayload;
                    testHarnessPayload.Properties["Status"].StringValue = DocumentStatus.Pending.ToString();
                    testHarnessPayload.Properties["TenantID"].StringValue = tenantEntity.RowKey;
                    _azureStorageHelper.Insert("TestHarnessPayload", testHarnessPayload);

                    _syntheticTransactionHelper.InsertSyntheticDetail(generatePayload, tenantEntity, approver);
                }
            }
            return documentNumber;
        }

        /// <summary>
        /// Update payload approver
        /// </summary>
        /// <param name="payload"></param>
        /// <param name="Approver"></param>
        /// <returns></returns>
        private string UpdatePayloadApprover(string payload, string Approver)
        {
            JObject jobject = JsonConvert.DeserializeObject<JObject>(payload);
            //Add Approves
            var jarrayApprovers = new JArray();
            var jobjectApprover = new JObject
            {
                { "DetailTemplate", null },
                { "OriginalApprovers", new JArray { } },
                { "CanEdit", false },
                { "Alias", Approver },
                { "Name", string.Empty }
            };
            jarrayApprovers.Add(jobjectApprover);
            jobject["Approvers"] = jarrayApprovers;

            //Add Approval Hierarchy
            var jSummaryData = jobject?.SelectToken("SummaryData")?.Value<JObject>();
            if (jSummaryData != null)
            {
                var jApproversObject = new JObject();
                var jApproverHierarchy = new JArray();
                jarrayApprovers = new JArray();
                jobjectApprover = new JObject
                {
                    { "Alias", Approver },
                    { "Name", string.Empty }
                };
                jarrayApprovers.Add(jobjectApprover);
                jApproversObject.Add("Approvers", jarrayApprovers);
                jApproversObject.Add("ApproverType", "Final");
                jApproverHierarchy.Add(jApproversObject);
                jSummaryData["ApprovalHierarchy"] = jApproverHierarchy;
            }
            return jobject.ToString();
        }
    }
}