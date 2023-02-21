// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace Microsoft.CFS.Approvals.Common.DL;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
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
        string query = "PartitionKey eq '" + documentNumber + "' and Approver eq '" + alias + "' and ActionTaken eq '" + actionTaken + "'";
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
            string query = "PartitionKey eq '" + documentNumber + "' and TenantId eq '" + tenantId + "'";
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
        string query = "PartitionKey eq '" + documentNumber + "' and TenantId eq '" + tenantId + "' and Approver eq '" + approver.ToLowerInvariant() + "'";
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
        string query = "Approver eq '" + alias.ToLowerInvariant() + "' and ActionDate ge datetime'" + DateTime.Now.AddMonths(timePeriod * -1).ToString("yyyy-MM-ddTHH:mm:ssZ") + "'";
        return Task.FromResult(_tableHelper.GetDataCollectionByTableQuery<TransactionHistory>(Constants.TransactionHistoryTableName, query));
    }
}