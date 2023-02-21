// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace Microsoft.CFS.Approvals.Common.DL;

using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.CFS.Approvals.Common.DL.Interface;
using Microsoft.CFS.Approvals.Contracts;
using Microsoft.CFS.Approvals.Contracts.DataContracts;
using Microsoft.CFS.Approvals.Data.Azure.CosmosDb.Interface;
using Microsoft.CFS.Approvals.Data.Azure.Storage.Interface;
using Microsoft.CFS.Approvals.LogManager.Model;
using Microsoft.CFS.Approvals.LogManager.Provider.Interface;
using Microsoft.CFS.Approvals.Model;
using Microsoft.Extensions.Configuration;

/// <summary>
/// The Action Audit Logger class
/// </summary>
public class ActionAuditLogger : IActionAuditLogger
{
    /// <summary>
    /// The table helper
    /// </summary>
    private readonly ITableHelper _tableHelper;

    /// <summary>
    /// The configuration
    /// </summary>
    private readonly IConfiguration _config;

    /// <summary>
    /// The DocumentDb helper
    /// </summary>
    private readonly ICosmosDbHelper _cosmosDbHelper;

    /// <summary>
    /// The Log provider
    /// </summary>
    private readonly ILogProvider _logProvider;

    /// <summary>
    /// Constructor of ActionAuditLogger
    /// </summary>
    /// <param name="tableHelper"></param>
    /// <param name="config"></param>
    /// <param name="cosmosDbHelper"></param>
    /// <param name="logProvider"></param>
    public ActionAuditLogger(ITableHelper tableHelper, IConfiguration config, ICosmosDbHelper cosmosDbHelper, ILogProvider logProvider)
    {
        _cosmosDbHelper = cosmosDbHelper;
        _tableHelper = tableHelper;
        _config = config;
        _logProvider = logProvider;
        _cosmosDbHelper.SetTarget(config[ConfigurationKey.CosmosDbNameActionAuditLog.ToString()], config[ConfigurationKey.CosmosDbCollectionActionAuditLog.ToString()], config[ConfigurationKey.CosmosDbPartitionKeyPath.ToString()]);
    }

    /// <summary>
    /// Gets the action audit logs by document number and approver.
    /// </summary>
    /// <param name="documentNumber">The document number.</param>
    /// <param name="actualApprover">The actual approver.</param>
    /// <returns>List of ActionAuditLogTableRow as per the query results.</returns>
    public async Task<List<ActionAuditLogInfo>> GetActionAuditLogsByDocumentNumberAndApprover(string documentNumber, string actualApprover)
    {
        var sqlQuery = "select * from c where c.DisplayDocumentNumber = '" + documentNumber + "' and c.Approver = '" +
                       actualApprover.ToLowerInvariant() + "'";

        return await _cosmosDbHelper.GetAllDocumentsAsync<ActionAuditLogInfo>(sqlQuery);
    }

    /// <summary>
    /// This method will log ActionAuditLog into storage
    /// </summary>
    /// <param name="actionAuditLogs">List of ActionAuditLogInfo</param>
    public async Task LogActionDetailsAsync(List<ActionAuditLogInfo> actionAuditLogs)
    {
        try
        {
            await _cosmosDbHelper.InsertDocumentsAsync(actionAuditLogs);
        }
        catch (Exception ex)
        {
            _logProvider.LogError<TrackingEvent, LogDataKey>(TrackingEvent.LogActionAuditFailure, ex);
        }
    }
}