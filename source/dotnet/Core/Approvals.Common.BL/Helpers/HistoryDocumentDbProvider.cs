// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace Microsoft.CFS.Approvals.Common.DL;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Azure.Cosmos;
using Microsoft.CFS.Approvals.Common.DL.Interface;
using Microsoft.CFS.Approvals.Contracts;
using Microsoft.CFS.Approvals.Data.Azure.CosmosDb.Interface;
using Microsoft.CFS.Approvals.Model;
using Microsoft.CFS.Approvals.Utilities.Extension;
using Microsoft.Extensions.Configuration;

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
    /// Configuration Helper
    /// </summary>
    private readonly IConfiguration _config;

    /// <summary>
    /// Constructor of HistoryCosmosDbProvider
    /// </summary>
    /// <param name="cosmosDbHelper"></param>
    /// <param name="config"></param>
    public HistoryDocumentDbProvider(ICosmosDbHelper cosmosDbHelper, IConfiguration config)
    {
        _cosmosDbHelper = cosmosDbHelper;
        _cosmosDbHelper.SetTarget("history", "transactionhistory", "/DocumentNumber");
        _config = config;
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
    /// <param name="domain">Approver domain</param>
    /// <param name="approverId">Approver Object Id</param>
    /// <returns></returns>
    public async Task<List<TransactionHistory>> GetHistoryDataAsync(string alias, string actionDate, string documentNumber, string actionTaken, string domain, string approverId)
    {
        var query = "select * from c where c.DocumentNumber = @documentNumber and LOWER(c.Approver) = LOWER(@approver) and c.ActionTaken = @actionTaken";

        var queryDefinition = new QueryDefinition(query)
            .WithParameter("@documentNumber", documentNumber)
            .WithParameter("@approver", alias)
            .WithParameter("@actionTaken", actionTaken);

        queryDefinition = (QueryDefinition)queryDefinition.GetUpdatedObject(_config[Constants.OldWhitelistedDomains], domain, approverId);

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
    /// Get list of TransactionHistory data within an interval
    /// </summary>
    /// <param name="alias"></param>
    /// <param name="approverDomain">Approver Domain</param>
    /// <param name="approverId">Approver Object Id</param>
    /// <param name="startTimePeriod"></param>
    /// <param name="endTimePeriod"></param>
    /// <returns></returns>
    public async Task<List<TransactionHistory>> GetHistoryDataAsync(string alias, string approverDomain, string approverId, int startTimePeriod, int endTimePeriod = 0)
    {
        var query = "select * from c where LOWER(c.Approver) = LOWER(@approver) and c.ActionDate >= @startActionDate";
        if (endTimePeriod > 0)
        {
            query += " and c.ActionDate < @endActionDate";
        }

        var queryDefinition = new QueryDefinition(query)
            .WithParameter("@approver", alias)
            .WithParameter("@startActionDate", DateTime.Now.AddMonths(startTimePeriod * -1).ToString("o"));
        if (endTimePeriod > 0)
        {
            queryDefinition.WithParameter("@endActionDate", DateTime.Now.AddMonths(endTimePeriod * -1).ToString("o"));
        }

        queryDefinition = (QueryDefinition)queryDefinition.GetUpdatedObject(_config[Constants.OldWhitelistedDomains], approverDomain, approverId);

        var historyList = await _cosmosDbHelper.GetAllDocumentsAsync<TransactionHistory>(queryDefinition);
        return historyList;
    }

}