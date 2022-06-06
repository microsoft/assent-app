// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace Microsoft.CFS.Approvals.Common.DL
{
    using System;
    using System.Collections.Generic;
    using System.Threading.Tasks;
    using Microsoft.CFS.Approvals.Common.DL.Interface;
    using Microsoft.CFS.Approvals.Data.Azure.CosmosDb.Interface;
    using Microsoft.CFS.Approvals.Model;

    /// <summary>
    /// The History DocumentDb provider
    /// </summary>
    public class HistoryDocumentDbProvider : IHistoryStorageProvider
    {
        /// <summary>
        /// The DocDb helper
        /// </summary>
        private readonly ICosmosDbHelper _cosmosDbHelper;

        /// <summary>
        /// Constructor of HistoryCosmosDbProvider
        /// </summary>
        /// <param name="cosmosDbHelper"></param>
        public HistoryDocumentDbProvider(ICosmosDbHelper cosmosDbHelper)
        {
            _cosmosDbHelper = cosmosDbHelper;
            _cosmosDbHelper.SetTarget("history", "transactionhistory");
        }

        /// <summary>
        /// Saves TransactionHistory data
        /// </summary>
        /// <param name="historyData">The transaction data.</param>
        /// <returns>
        /// </returns>
        public async Task AddApprovalHistoryAsync(TransactionHistory historyData)
        {
            await Task.Run(() => _cosmosDbHelper.InsertDocumentAsync(historyData));
        }

        /// <summary>
        /// Saves list of TransactionHistory data
        /// </summary>
        /// <param name="historyDataList"></param>
        /// <returns></returns>
        public async Task AddApprovalHistoryAsync(List<TransactionHistory> historyDataList)
        {
            await _cosmosDbHelper.InsertDocumentsAsync(historyDataList);
        }

        /// <summary>
        /// Get list of TransactionHistory data
        /// </summary>
        /// <param name="alias"></param>
        /// <param name="actionDate"></param>
        /// <param name="documentNumber"></param>
        /// <param name="actionTaken"></param>
        /// <returns></returns>
        public async Task<List<TransactionHistory>> GetHistoryDataAsync(string alias, string actionDate, string documentNumber, string actionTaken)
        {
            var sqlQuery = "select * from c where c.DocumentNumber = '" + documentNumber + "' and (c.Approver = '" +
                           alias.ToLowerInvariant() + "' or c.Approver = '" +
                           alias.ToUpperInvariant() + "' or c.Approver = '" +
                           alias + "') and c.ActionTaken = '" + actionTaken + "'";

            return await _cosmosDbHelper.GetAllDocumentsAsync<TransactionHistory>(sqlQuery);
        }

        /// <summary>
        /// Get list of TransactionHistory data
        /// </summary>
        /// <param name="tenantId"></param>
        /// <param name="documentNumber"></param>
        /// <returns></returns>
        public async Task<List<TransactionHistory>> GetHistoryDataAsync(string tenantId, string documentNumber)
        {
            return await Task.Run(() => GetHistoryDataAsync(tenantId, documentNumber, null));
        }

        /// <summary>
        /// Get list of TransactionHistory data
        /// </summary>
        /// <param name="tenantId"></param>
        /// <param name="documentNumber"></param>
        /// <param name="approver"></param>
        /// <returns></returns>
        public async Task<List<TransactionHistory>> GetHistoryDataAsync(string tenantId, string documentNumber, string approver)
        {
            var sqlQuery = "select * from c where c.DocumentNumber = '" + documentNumber + "'";
            if (!string.IsNullOrWhiteSpace(tenantId))
            {
                sqlQuery += " and c.TenantId = '" + tenantId + "'";
            }
            if (!string.IsNullOrWhiteSpace(approver))
            {
                sqlQuery += " and (c.Approver = '" + approver.ToLowerInvariant() + "' or c.Approver = '" + approver.ToUpperInvariant() + "' or c.Approver = '" + approver + "')";
            }

            return await _cosmosDbHelper.GetAllDocumentsAsync<TransactionHistory>(sqlQuery);
        }

        /// <summary>
        /// Get list of TransactionHistory data
        /// </summary>
        /// <param name="alias"></param>
        /// <param name="timePeriod"></param>
        /// <returns></returns>
        public async Task<List<TransactionHistory>> GetHistoryDataAsync(string alias, int timePeriod)
        {
            var sqlQuery = "select * from c where (c.Approver = '" + alias.ToLowerInvariant() + "' or c.Approver = '" + alias.ToUpperInvariant() + "' or c.Approver = '" + alias + "') and c.ActionDate >= '" + DateTime.Now.AddMonths(timePeriod * -1).ToString("o") + "'";
            return await _cosmosDbHelper.GetAllDocumentsAsync<TransactionHistory>(sqlQuery);
        }
    }
}