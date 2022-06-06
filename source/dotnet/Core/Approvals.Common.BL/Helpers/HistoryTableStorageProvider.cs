// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace Microsoft.CFS.Approvals.Common.DL
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;
    using Microsoft.Azure.Cosmos.Table;
    using Microsoft.CFS.Approvals.Common.DL.Interface;
    using Microsoft.CFS.Approvals.Contracts;
    using Microsoft.CFS.Approvals.Data.Azure.Storage.Interface;
    using Microsoft.CFS.Approvals.Model;

    /// <summary>
    /// The History Table Storage Provider
    /// </summary>
    public class HistoryTableStorageProvider : IHistoryStorageProvider
    {
        /// <summary>
        /// The Table helper
        /// </summary>
        private readonly ITableHelper _tableHelper;

        /// <summary>
        /// Constructor of HistoryTableStorageProvider
        /// </summary>
        /// <param name="tableHelper"></param>
        public HistoryTableStorageProvider(ITableHelper tableHelper)
        {
            _tableHelper = tableHelper;
        }

        /// <summary>
        /// Save TransactionHistory entity
        /// </summary>
        /// <param name="historyData"></param>
        /// <returns></returns>
        public async Task AddApprovalHistoryAsync(TransactionHistory historyData)
        {
            await Task.Run(() => _tableHelper.InsertOrReplace<TransactionHistory>(Constants.TransactionHistoryTableName, historyData));
        }

        /// <summary>
        /// Save TransactionHistory entities
        /// </summary>
        /// <param name="historyDataList"></param>
        /// <returns></returns>
        public async Task AddApprovalHistoryAsync(List<TransactionHistory> historyDataList)
        {
            await Task.Run(() => _tableHelper.InsertOrReplaceRows<TransactionHistory>(Constants.TransactionHistoryTableName, historyDataList));
        }

        /// <summary>
        /// Get list of TransactionHistory data
        /// </summary>
        /// <param name="alias"></param>
        /// <param name="actionDate"></param>
        /// <param name="documentNumber"></param>
        /// <param name="actionTaken"></param>
        /// <returns></returns>
        public Task<List<TransactionHistory>> GetHistoryDataAsync(string alias, string actionDate, string documentNumber, string actionTaken)
        {
            TableQuery<TransactionHistory> query = new TableQuery<TransactionHistory>().Where(TableQuery.CombineFilters(
                TableQuery.CombineFilters(
                    TableQuery.GenerateFilterCondition("PartitionKey", QueryComparisons.Equal, documentNumber),
                    TableOperators.And,
                    TableQuery.GenerateFilterCondition("Approver", QueryComparisons.Equal, alias)),
                TableOperators.And,
                TableQuery.GenerateFilterCondition("ActionTaken", QueryComparisons.Equal, actionTaken)));
            var historyList = _tableHelper.GetDataCollectionByTableQuery<TransactionHistory>(Constants.TransactionHistoryTableName, query);
            return Task.FromResult(historyList);
        }

        /// <summary>
        /// Get list of TransactionHistory data
        /// </summary>
        /// <param name="tenantId"></param>
        /// <param name="documentNumber"></param>
        /// <returns></returns>
        public Task<List<TransactionHistory>> GetHistoryDataAsync(string tenantId, string documentNumber)
        {
            List<TransactionHistory> dtTransactionHistory = new List<TransactionHistory>();
            if (string.IsNullOrWhiteSpace(tenantId))
            {
                dtTransactionHistory = _tableHelper.GetTableEntityListByPartitionKey<TransactionHistory>(Constants.TransactionHistoryTableName,
                    documentNumber).OrderBy(t => t.ActionDate).ToList();
            }
            else
            {
                TableQuery<TransactionHistory> query = new TableQuery<TransactionHistory>().Where(TableQuery.CombineFilters(
                    TableQuery.GenerateFilterCondition("PartitionKey", QueryComparisons.Equal, documentNumber),
                    TableOperators.And,
                    TableQuery.GenerateFilterCondition("TenantId", QueryComparisons.Equal, tenantId)));
                dtTransactionHistory = _tableHelper.GetDataCollectionByTableQuery<TransactionHistory>(Constants.TransactionHistoryTableName, query).OrderBy(t => t.ActionDate).ToList(); ;
            }

            return Task.FromResult(dtTransactionHistory);
        }

        /// <summary>
        /// Get list of TransactionHistory data
        /// </summary>
        /// <param name="tenantId"></param>
        /// <param name="documentNumber"></param>
        /// <param name="approver"></param>
        /// <returns></returns>
        public Task<List<TransactionHistory>> GetHistoryDataAsync(string tenantId, string documentNumber, string approver)
        {
            TableQuery<TransactionHistory> query = new TableQuery<TransactionHistory>().Where(TableQuery.CombineFilters(
                TableQuery.CombineFilters(
                    TableQuery.GenerateFilterCondition("PartitionKey", QueryComparisons.Equal, documentNumber),
                    TableOperators.And,
                    TableQuery.GenerateFilterCondition("TenantId", QueryComparisons.Equal, tenantId)),
                TableOperators.And,
                TableQuery.GenerateFilterCondition("Approver", QueryComparisons.Equal, approver.ToLowerInvariant())));
            var historyList = _tableHelper.GetDataCollectionByTableQuery<TransactionHistory>(Constants.TransactionHistoryTableName, query);
            return Task.FromResult(historyList);
        }

        /// <summary>
        /// Get list of TransactionHistory data
        /// </summary>
        /// <param name="alias"></param>
        /// <param name="timePeriod"></param>
        /// <returns></returns>
        public Task<List<TransactionHistory>> GetHistoryDataAsync(string alias, int timePeriod)
        {
            TableQuery<TransactionHistory> query = new TableQuery<TransactionHistory>().Where(TableQuery.CombineFilters(
                    TableQuery.GenerateFilterCondition("Approver", QueryComparisons.Equal, alias.ToLowerInvariant()),
                    TableOperators.And,
                    TableQuery.GenerateFilterConditionForDate("ActionDate", QueryComparisons.GreaterThanOrEqual, DateTime.Now.AddMonths(timePeriod * -1))));
            return Task.FromResult(_tableHelper.GetDataCollectionByTableQuery<TransactionHistory>(Constants.TransactionHistoryTableName, query));
        }
    }
}