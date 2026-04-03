// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace Microsoft.CFS.Approvals.Common.DL;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.CFS.Approvals.Common.DL.Interface;
using Microsoft.CFS.Approvals.Contracts;
using Microsoft.CFS.Approvals.Contracts.DataContracts;
using Microsoft.CFS.Approvals.Data.Azure.Storage.Helpers;
using Microsoft.CFS.Approvals.Data.Azure.Storage.Interface;
using Microsoft.CFS.Approvals.Model;
using Microsoft.CFS.Approvals.Utilities.Extension;
using Microsoft.Extensions.Configuration;

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
    /// Configuration Helper
    /// </summary>
    private readonly IConfiguration _config;

    /// <summary>
    /// The Approval blob data provider
    /// </summary>
    private readonly IBlobStorageHelper _blobStorageHelper;

    /// <summary>
    /// Constructor of HistoryTableStorageProvider
    /// </summary>
    /// <param name="tableHelper"></param>
    /// <param name="config"></param>
    public HistoryTableStorageProvider(ITableHelper tableHelper, IConfiguration config, IBlobStorageHelper blobStorageHelper)
    {
        _tableHelper = tableHelper;
        _config = config;
        _blobStorageHelper = blobStorageHelper;
    }

    /// <summary>
    /// Save TransactionHistory entity
    /// </summary>
    /// <param name="historyData"></param>
    /// <returns></returns>
    public async Task AddApprovalHistoryAsync(TransactionHistory historyData)
    {
        bool blobCheck = false;
        try
        {
            await Task.Run(() => _tableHelper.InsertOrReplace<TransactionHistory>(Constants.TransactionHistoryTableName, historyData));
        }
        catch (global::Azure.RequestFailedException ex)
        {
            // Checks whether exception caused was due to large data.
            if (ex.ErrorCode == "PropertyValueTooLarge" || ex.ErrorCode == "EntityTooLarge" ||
                ex.ErrorCode == "RequestBodyTooLarge")
            {
                blobCheck = true;
            }
            else
            {
                throw;
            }
        }

        #region Insertion in approvalsummaryblobdata

        if (blobCheck == true)
        {
            var blobPointer = historyData.Approver.ToString() + "|" + historyData.DocumentTypeID + "|" + historyData.DocumentNumber;
            await _blobStorageHelper.UploadText(historyData.JsonData, Constants.ApprovalSummaryBlobContainerName, blobPointer);
            historyData.BlobPointer = blobPointer;
            historyData.JsonData = string.Empty;

            await Task.Run(() => _tableHelper.InsertOrReplace<TransactionHistory>(Constants.TransactionHistoryTableName, historyData));
        }

        #endregion Insertion in approvalsummaryblobdata
    }

    /// <summary>
    /// Save TransactionHistory entities
    /// </summary>
    /// <param name="historyDataList"></param>
    /// <returns></returns>
    public async Task AddApprovalHistoryAsync(List<TransactionHistory> historyDataList)
    {
        foreach (var historyData in historyDataList)
        {
            bool blobCheck = false;
            try
            {
                await _tableHelper.InsertOrReplace<TransactionHistory>(Constants.TransactionHistoryTableName, historyData);
            }
            catch (global::Azure.RequestFailedException ex)
            {
                // Checks whether exception caused was due to large data.
                if (ex.ErrorCode == "PropertyValueTooLarge" || ex.ErrorCode == "EntityTooLarge" ||
                    ex.ErrorCode == "RequestBodyTooLarge")
                {
                    blobCheck = true;
                }
                else
                {
                    throw;
                }
            }

            #region Insertion in approvalsummaryblobdata

            if (blobCheck == true)
            {
                var blobPointer = historyData.Approver.ToString() + "|" + historyData.DocumentTypeID + "|" + historyData.DocumentNumber;
                await _blobStorageHelper.UploadText(historyData.JsonData, Constants.ApprovalSummaryBlobContainerName, blobPointer);
                historyData.BlobPointer = blobPointer;
                historyData.JsonData = string.Empty;

                await Task.Run(() => _tableHelper.InsertOrReplace<TransactionHistory>(Constants.TransactionHistoryTableName, historyData));
            }

            #endregion Insertion in approvalsummaryblobdata

        }
        await Task.Run(() => _tableHelper.InsertOrReplaceRows<TransactionHistory>(Constants.TransactionHistoryTableName, historyDataList));
    }

    /// <summary>
    /// Get list of TransactionHistory data
    /// </summary>
    /// <param name="alias"></param>
    /// <param name="actionDate"></param>
    /// <param name="documentNumber"></param>
    /// <param name="actionTaken"></param>
    /// <param name="domain">Approver domain</param>
    /// <param name="approverId">Approver Object Id</param>
    /// <returns></returns>
    public async Task<List<TransactionHistory>> GetHistoryDataAsync(string alias, string actionDate, string documentNumber, string actionTaken, string domain, string approverId)
    {
        string query = "PartitionKey eq '" + documentNumber + "' and Approver eq '" + alias + "' and ActionTaken eq '" + actionTaken + "'";
        var historyList = _tableHelper.GetDataCollectionByTableQuery<TransactionHistory>(Constants.TransactionHistoryTableName, query);
        historyList = (List<TransactionHistory>)historyList.GetUpdatedObject(_config[Constants.OldWhitelistedDomains], domain, approverId);
        return await GetSummaryFromBlobIfAny(historyList);
    }

    /// <summary>
    /// Get list of TransactionHistory data
    /// </summary>
    /// <param name="tenantId"></param>
    /// <param name="documentNumber"></param>
    /// <returns></returns>
    public async Task<List<TransactionHistory>> GetHistoryDataAsync(string tenantId, string documentNumber)
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

        return await GetSummaryFromBlobIfAny(dtTransactionHistory);
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
        string query = "PartitionKey eq '" + documentNumber + "' and TenantId eq '" + tenantId + "' and Approver eq '" + approver.ToLowerInvariant() + "'";
        var historyList = _tableHelper.GetDataCollectionByTableQuery<TransactionHistory>(Constants.TransactionHistoryTableName, query);

        return await GetSummaryFromBlobIfAny(historyList);
    }

    /// <summary>
    /// Get list of TransactionHistory data
    /// </summary>
    /// <param name="alias"></param>
    /// <param name="approverDomain">Approver Domain</param>
    /// <param name="approverId">Approver Object Id</param>
    /// <param name="timePeriod"></param>
    /// <returns></returns>
    public async Task<List<TransactionHistory>> GetHistoryDataAsync(string alias, string approverDomain, string approverId, int timePeriod, int endTimePeriod = 0)
    {
        string query = "Approver eq '" + alias.ToLowerInvariant() + "' and ActionDate ge datetime'" + DateTime.Now.AddMonths(timePeriod * -1).ToString("yyyy-MM-ddTHH:mm:ssZ") + "'";
        if (endTimePeriod > 0)
        {
            query += " and ActionDate lt datetime'" + DateTime.Now.AddMonths(endTimePeriod * -1).ToString("yyyy-MM-ddTHH:mm:ssZ") + "'";
        }
        var historyList = _tableHelper.GetDataCollectionByTableQuery<TransactionHistory>(Constants.TransactionHistoryTableName, query);
        historyList = (List<TransactionHistory>)historyList.GetUpdatedObject(_config[Constants.OldWhitelistedDomains], approverDomain, approverId);
        return await GetSummaryFromBlobIfAny(historyList);
    }

    /// <summary>
    /// Get json summary data from Blob in case BlobPointer is set
    /// </summary>
    /// <param name="historyList">list of History data</param>
    /// <returns></returns>
    private Task<List<TransactionHistory>> GetSummaryFromBlobIfAny(List<TransactionHistory> historyList)
    {
        Parallel.ForEach(historyList, async history =>
        {
            history.JsonData = string.IsNullOrWhiteSpace(history.JsonData) && !string.IsNullOrWhiteSpace(history.BlobPointer) ?
                                await _blobStorageHelper.DownloadText(Constants.ApprovalSummaryBlobContainerName, history.BlobPointer) :
                                history.JsonData;
        });
        return Task.FromResult(historyList);
    }
}