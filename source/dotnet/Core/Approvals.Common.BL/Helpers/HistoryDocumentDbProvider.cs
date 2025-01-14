// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace Microsoft.CFS.Approvals.Common.DL;

using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Azure.Cosmos;
using Microsoft.CFS.Approvals.Common.DL.Interface;
using Microsoft.CFS.Approvals.Contracts.DataContracts;
using Microsoft.CFS.Approvals.Data.Azure.CosmosDb.Interface;
using Microsoft.CFS.Approvals.Model;
using Microsoft.Graph;

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
        _cosmosDbHelper.SetTarget("history", "transactionhistory", "/DocumentNumber");
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
        var query = "select * from c where c.DocumentNumber = @documentNumber and LOWER(c.Approver) = LOWER(@approver) and c.ActionTaken = @actionTaken";

        var queryDefinition = new QueryDefinition(query)
            .WithParameter("@documentNumber", documentNumber)
            .WithParameter("@approver", alias)
        .WithParameter("@actionTaken", actionTaken);

        var historyList = await _cosmosDbHelper.GetAllDocumentsAsync<TransactionHistory>(queryDefinition);
        return historyList;
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
        var query = "select * from c where c.DocumentNumber = @documentNumber";
        var parameters = new Dictionary<string, string>
        {
            { "@documentNumber", documentNumber }
        };

        if (!string.IsNullOrWhiteSpace(tenantId))
        {
            query += " and c.TenantId = @tenantId";
            parameters.Add("@tenantId", tenantId);
        }

        if (!string.IsNullOrWhiteSpace(approver))
        {
            query += " and LOWER(c.Approver) = LOWER(@approver)";
            parameters.Add("@approver", approver);
        }

        var queryDefinition = new QueryDefinition(query);
        foreach (var param in parameters)
        {
            queryDefinition.WithParameter(param.Key, param.Value);
        }

        return await _cosmosDbHelper.GetAllDocumentsAsync<TransactionHistory>(queryDefinition);
    }

    /// <summary>
    /// Get list of TransactionHistory data
    /// </summary>
    /// <param name="alias"></param>
    /// <param name="timePeriod"></param>
    /// <returns></returns>
    public async Task<List<TransactionHistory>> GetHistoryDataAsync(string alias, int timePeriod)
    {
        var query = "select * from c where LOWER(c.Approver) = LOWER(@approver) and c.ActionDate >= @actionDate";

        var queryDefinition = new QueryDefinition(query)
            .WithParameter("@approver", alias)
            .WithParameter("@actionDate", DateTime.Now.AddMonths(timePeriod * -1).ToString("o"));

        var historyList = await _cosmosDbHelper.GetAllDocumentsAsync<TransactionHistory>(queryDefinition);
        return historyList;
    }
}