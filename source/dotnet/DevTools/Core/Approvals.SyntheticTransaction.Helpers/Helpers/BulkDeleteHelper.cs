// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace Microsoft.CFS.Approvals.SyntheticTransaction.Helpers.Helpers
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Net.Http;
    using System.Threading.Tasks;
    using Microsoft.AspNetCore.Mvc.Infrastructure;
    using Microsoft.CFS.Approvals.Contracts.DataContracts;
    using Microsoft.CFS.Approvals.Data.Azure.Storage.Interface;
    using Microsoft.CFS.Approvals.SyntheticTransaction.Common.Models;
    using Microsoft.CFS.Approvals.SyntheticTransaction.Helpers.Interface;
    using Newtonsoft.Json;
    using Newtonsoft.Json.Linq;

    /// <summary>
    /// Bulk Delete Helper class
    /// </summary>
    public class BulkDeleteHelper : IBulkDeleteHelper
    {
        /// <summary>
        /// The azure storage helepr
        /// </summary>
        private readonly ITableHelper _azureStorageHelper;

        /// <summary>
        /// The payload receiver helper
        /// </summary>
        private readonly IPayloadReceiverHelper _payloadReceiverHelper;

        private readonly string _environment;

        public BulkDeleteHelper(
            Func<string, string, ITableHelper> azureStorageHelper,
            Func<string, string, IBlobStorageHelper> blobStorageHelper,
            IPayloadReceiverHelper payloadReceiverHelper,
            IActionContextAccessor actionContextAccessor,
            ConfigurationSetting configurationSetting)
        {
            _environment = actionContextAccessor?.ActionContext?.RouteData?.Values["env"]?.ToString();
            _azureStorageHelper = azureStorageHelper(
               configurationSetting.appSettings[_environment].StorageAccountName,
               configurationSetting.appSettings[_environment].StorageAccountKey);
            _payloadReceiverHelper = payloadReceiverHelper;
        }

        /// <summary>
        /// Bulk delete
        /// </summary>
        /// <param name="tenant"></param>
        /// <param name="approver"></param>
        /// <param name="days"></param>
        /// <param name="docNumber"></param>
        /// <returns></returns>
        public async Task<object> BulkDelete(string tenant, string approver, string days, string docNumber)
        {
            DateTime dateFilter;
            string successDocuments = string.Empty;
            string failureDocuments = string.Empty;
            string invalidDouments = string.Empty;
            int noOfDays = 0, deleteSuccess = 0;

            List<TestHarnessDocument> documentList = new List<TestHarnessDocument>();
            documentList = _azureStorageHelper.GetTableEntityListByPartitionKey<TestHarnessDocument>("TestHarnessPayload", approver).Where(x => x.Status == DocumentStatus.Pending.ToString() && x.TenantID == tenant.Trim()).ToList();
            if (!string.IsNullOrEmpty(days) && Int32.TryParse(days, out noOfDays))
            {
                dateFilter = DateTime.UtcNow.AddDays(-noOfDays);
                documentList = documentList.Where(x => x.Timestamp < dateFilter).ToList();
            }
            else if (!string.IsNullOrWhiteSpace(docNumber))
            {
                var documnetnumber = docNumber.Split(",").Select(doc => doc.Trim()).ToArray();
                documentList = documentList.Where(x => documnetnumber.Contains<string>(x.RowKey.Split("|")[0])).ToList();
                invalidDouments = string.Join(',', documnetnumber.Except(documentList.Select(x => x.RowKey.Split("|")[0])));
            }
            else
            {
                dateFilter = DateTime.UtcNow;
                documentList = documentList.Where(x => x.Timestamp < dateFilter).ToList();
            }
            if (documentList.Count > 0)
            {
                foreach (var document in documentList)
                {
                    var result = SendDeletePayload(document, approver, "Bulk Delete", "Delete");
                    var PayloadValidationResults = JsonConvert.DeserializeObject<JObject>(result.Content.ReadAsStringAsync().Result)?.SelectToken("PayloadProcessingResult")?.SelectToken("PayloadValidationResults")?.Value<JArray>();
                    if (result.IsSuccessStatusCode && PayloadValidationResults == null)
                    {
                        deleteSuccess--;
                        successDocuments = successDocuments + document.RowKey.Split('|')[0] + ", ";
                        document.Status = DocumentStatus.Approved.ToString();
                        await UpdateDocumentStatus(document);
                    }
                    else
                        failureDocuments = failureDocuments + document.RowKey.Split('|')[0] + ", ";
                }
                return new { bulkSuccessDocuments = successDocuments, bulkFailureDocuments = failureDocuments };
            }
            else
            {
                return new { bulkSuccessDocuments = "No pending records found for Approver!!" };
            }
        }

        /// <summary>
        /// Send delete payload
        /// </summary>
        /// <param name="documnet"></param>
        /// <param name="approver"></param>
        /// <param name="comment"></param>
        /// <param name="action"></param>
        /// <returns></returns>
        public HttpResponseMessage SendDeletePayload(TestHarnessDocument documnet, string approver, string comment, string action)
        {
            JObject documentJObject = JsonConvert.DeserializeObject<JObject>(documnet.Payload);
            documentJObject["Operation"] = (int)PayLoadOperation.Delete;

            if (!documentJObject.ContainsKey("DeleteFor"))
            {
                documentJObject.Add("DeleteFor");
            }

            documentJObject["DeleteFor"] = new JArray { approver };

            var actionDetail = documentJObject?.SelectToken("ActionDetail")?.Value<JObject>();

            if (actionDetail != null)
            {
                actionDetail["Name"] = action;
                actionDetail["Date"] = DateTime.UtcNow.ToString();
                actionDetail["Comment"] = comment;
                var ActionBy = new JObject
                {
                    { "Alias", approver }
                };
                actionDetail["ActionBy"] = ActionBy;
            }
            else
            {
                ActionDetail actionDetail1 = new ActionDetail
                {
                    Name = action,
                    Date = DateTime.UtcNow,
                    Comment = comment,
                    ActionBy = new NameAliasEntity
                    {
                        Alias = approver
                    }
                };
                documentJObject["ActionDetail"] = JObject.FromObject(actionDetail1);
            }

            var result = _payloadReceiverHelper.SendPayload(documentJObject.ToString()).Result;
            return result;
        }

        /// <summary>
        /// Update document  status
        /// </summary>
        /// <param name="document"></param>
        /// <returns></returns>
        public async Task<bool> UpdateDocumentStatus(TestHarnessDocument document)
        {
            try
            {
                var result = _azureStorageHelper.Insert("TestHarnessPayload", document);
                return true;
            }
            catch
            {
                return false;
            }
        }
    }
}