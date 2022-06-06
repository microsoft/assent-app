// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace Microsoft.CFS.Approvals.SupportServices.Helper.ServiceHelper
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;
    using Microsoft.AspNetCore.Mvc.Infrastructure;
    using Microsoft.CFS.Approvals.Data.Azure.Storage.Interface;
    using Microsoft.CFS.Approvals.DevTools.Model.Constant;
    using Microsoft.CFS.Approvals.DevTools.Model.Models;
    using Microsoft.CFS.Approvals.SupportServices.Helper.ExtensionMethods;
    using Microsoft.CFS.Approvals.SupportServices.Helper.Interface;
    using Newtonsoft.Json.Linq;

    /// <summary>
    /// The Mark Request Out of Sync Helper
    /// </summary>
    public class MarkRequestOutOfSyncHelper : IMarkRequestOutOfSyncHelper
    {
        /// <summary>
        /// The table storage helper
        /// </summary>
        private readonly ITableHelper _azureTableStorageHelper;

        private readonly ConfigurationHelper _configurationHelper;
        private readonly string _environment;

        /// <summary>
        /// Constructor of MarkRequestOutOfSyncHelper
        /// </summary>
        /// <param name="azureTableStorageHelper"></param>
        /// <param name="configurationHelper"></param>
        /// <param name="actionContextAccessor"></param>
        public MarkRequestOutOfSyncHelper(
            Func<string, string, ITableHelper> azureTableStorageHelper,
            ConfigurationHelper configurationHelper,
            IActionContextAccessor actionContextAccessor)
        {
            _environment = actionContextAccessor?.ActionContext?.RouteData?.Values["env"]?.ToString();
            _azureTableStorageHelper = azureTableStorageHelper(
                configurationHelper.appSettings[_environment].StorageAccountName,
                configurationHelper.appSettings[_environment].StorageAccountKey);
            _configurationHelper = configurationHelper;
        }

        /// <summary>
        /// Mark requests out of sync
        /// </summary>
        /// <param name="documentCollection"></param>
        /// <param name="approver"></param>
        /// <param name="tenantID"></param>
        /// <returns></returns>
        public Dictionary<string, string> MarkRequestsOutOfSync(List<string> documentCollection, string approver, int tenantID)
        {
            int batches = 1;
            int maxIterationCount = documentCollection.Count > 10 ? 10 : documentCollection.Count;
            List<Task<Tuple<string, string, string, string>>> tasks = new List<Task<Tuple<string, string, string, string>>>();
            Dictionary<string, string> results = new Dictionary<string, string>();
            TenantEntity tenant = _azureTableStorageHelper.GetTableEntityByRowKey<TenantEntity>("ApprovalTenantInfo", tenantID.ToString());
            foreach (string document in documentCollection)
            {
                if (batches < maxIterationCount)
                {
                    Task<Tuple<string, string, string, string>> task = Task.Run(() => MarkRequestsOutOfSync(document.Trim(), approver, tenant));
                    tasks.Add(task);
                    batches++;
                }
                else
                {
                    Task<Tuple<string, string, string, string>> task = Task.Run(() => MarkRequestsOutOfSync(document, approver, tenant));
                    tasks.Add(task);
                    Task.WaitAll(tasks.ToArray());
                    foreach (var result in tasks)
                    {
                        var taskResult = result.Result;
                        if (!string.IsNullOrWhiteSpace(taskResult.Item1))
                        {
                            if (results.ContainsKey(Constants.currentApproverMissingForDocument))
                            {
                                results[Constants.currentApproverMissingForDocument] = string.Join(",", results[Constants.currentApproverMissingForDocument], taskResult.Item1);
                            }
                            else
                                results.Add(Constants.currentApproverMissingForDocument, string.Format("{0}", taskResult.Item1));
                        }
                        if (!string.IsNullOrWhiteSpace(taskResult.Item2))
                        {
                            if (results.ContainsKey(Constants.summaryNotFoundForDocument))
                            {
                                results[Constants.summaryNotFoundForDocument] = string.Join(",", results[Constants.summaryNotFoundForDocument], taskResult.Item2);
                            }
                            else
                                results.Add(Constants.summaryNotFoundForDocument, string.Format("{0}", taskResult.Item2));
                        }
                        if (!string.IsNullOrWhiteSpace(taskResult.Item3))
                        {
                            if (results.ContainsKey(Constants.failedtoMarkOutOfSync))
                            {
                                results[Constants.failedtoMarkOutOfSync] = string.Join(",", results[Constants.failedtoMarkOutOfSync], taskResult.Item3);
                            }
                            else
                                results.Add(Constants.failedtoMarkOutOfSync, string.Format("{0}", taskResult.Item3));
                        }
                        if (!string.IsNullOrWhiteSpace(taskResult.Item4))
                        {
                            if (results.ContainsKey(Constants.invalidTenantSelection))
                            {
                                results[Constants.invalidTenantSelection] = string.Join(",", results[Constants.invalidTenantSelection], taskResult.Item4);
                            }
                            else
                                results.Add(Constants.invalidTenantSelection, string.Format("{0}", taskResult.Item4));
                        }
                    }
                    batches = 1;
                    tasks = new List<Task<Tuple<string, string, string, string>>>();
                }
            }
            return results;
        }

        /// <summary>
        /// Mark requsets out of sync
        /// </summary>
        /// <param name="document"></param>
        /// <param name="approver"></param>
        /// <param name="tenant"></param>
        /// <returns></returns>
        private async Task<Tuple<string, string, string, string>> MarkRequestsOutOfSync(string document, string approver, TenantEntity tenant)
        {
            string failedtoMarkOutOfSync = string.Empty;
            string currentApproverMissingForDocument = string.Empty;
            string summaryNotFoundForDocument = string.Empty;
            string invalidTenantSelection = string.Empty;
            if (string.IsNullOrWhiteSpace(approver))
            {
                List<string> currentApprover = GetCurrentApprover(Convert.ToInt32(tenant.RowKey), document);
                if (!currentApprover.Any())
                {
                    var summaryData = _azureTableStorageHelper.GetTableEntityListByfield<SummaryEntity>(_configurationHelper.appSettings[_environment].ApprovalSummaryTable, "DocumentNumber", document);
                    if (summaryData != null)
                    {
                        foreach (var summaryRow in summaryData)
                        {
                            if (summaryRow.RowKey.Split('|')[0].ToString().Trim().Equals(tenant.DocTypeId.Trim()))
                            {
                                var isSuccess = await UpdateSummaryRowToMarkRequestOutOfSync(summaryRow, document);
                                if (!isSuccess)
                                {
                                    failedtoMarkOutOfSync = document;
                                }
                            }
                            else
                            {
                                invalidTenantSelection = document;
                            }
                        }
                    }
                    else
                    {
                        // If current approver not found ,  get summary row using doc number
                        currentApproverMissingForDocument = document;
                    }
                }
                else
                {
                    foreach (string currentapprover in currentApprover)
                    {
                        var summaryRow = _azureTableStorageHelper.GetTableEntityByPartitionKeyAndField<SummaryEntity>(_configurationHelper.appSettings[_environment].ApprovalSummaryTable, currentapprover, "DocumentNumber", document).FirstOrDefault();
                        if (summaryRow != null)
                        {
                            if (summaryRow.RowKey.Split('|')[0].ToString().Trim().Equals(tenant.DocTypeId.Trim()))
                            {
                                var isSuccess = await UpdateSummaryRowToMarkRequestOutOfSync(summaryRow, document);
                                if (!isSuccess)
                                {
                                    failedtoMarkOutOfSync = document;
                                }
                            }
                            else
                            {
                                invalidTenantSelection = document;
                            }
                        }
                        else
                        {
                            summaryNotFoundForDocument = document;
                        }
                    }
                }
            }
            else
            {
                var summaryRow = _azureTableStorageHelper.GetTableEntityByPartitionKeyAndField<SummaryEntity>(_configurationHelper.appSettings[_environment].ApprovalSummaryTable, approver, "DocumentNumber", document).FirstOrDefault();
                if (summaryRow != null)
                {
                    if (summaryRow.RowKey.Split('|')[0].ToString().Trim().Equals(tenant.DocTypeId.Trim()))
                    {
                        var isSuccess = await UpdateSummaryRowToMarkRequestOutOfSync(summaryRow, document);
                        if (!isSuccess)
                        {
                            failedtoMarkOutOfSync = document;
                        }
                    }
                    else
                    {
                        invalidTenantSelection = document;
                    }
                }
                else
                {
                    summaryNotFoundForDocument = document;
                }
            }
            return new Tuple<string, string, string, string>(currentApproverMissingForDocument, summaryNotFoundForDocument, failedtoMarkOutOfSync, invalidTenantSelection);
        }

        /// <summary>
        /// Getting current approver
        /// </summary>
        /// <param name="tenantId">tenantId</param>
        /// <param name="documentNumber">document number</param>
        /// <returns>List<string> current approver alias list</returns>
        private List<string> GetCurrentApprover(int tenantId, string documentNumber)
        {
            List<string> currentApprovers = new List<string>();
            var detailsData = _azureTableStorageHelper.GetTableEntityListByPartitionKey<ApprovalDetailEntity>("ApprovalDetails", documentNumber).Where(d => d.RowKey == Constants.CurrentApprover && d.TenantID == tenantId).FirstOrDefault();
            if (detailsData != null)
            {
                var approverAlias = detailsData.JSONData.FromJson<JArray>();
                foreach (var alias in approverAlias)
                {
                    currentApprovers.Add(alias["Alias"].ToString().Trim());
                }
            }
            return currentApprovers;
        }

        /// <summary>
        /// Update summary row to mark request out of sync
        /// </summary>
        /// <param name="summaryRow"></param>
        /// <param name="document"></param>
        /// <returns></returns>
        private async Task<bool> UpdateSummaryRowToMarkRequestOutOfSync(SummaryEntity summaryRow, string document)
        {
            summaryRow.IsOutOfSyncChallenged = true;
            var isSuccess = await _azureTableStorageHelper.InsertOrReplace<SummaryEntity>(_configurationHelper.appSettings[_environment].ApprovalSummaryTable, summaryRow);
            return isSuccess;
        }
    }
}