// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace Microsoft.CFS.Approvals.Common.DL
{
    using System;
    using System.Collections.Generic;
    using System.Threading.Tasks;
    using Microsoft.Azure.Cosmos.Table;
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
            _cosmosDbHelper.SetTarget(config[ConfigurationKey.CosmosDbNameActionAuditLog.ToString()], config[ConfigurationKey.CosmosDbCollectionActionAuditLog.ToString()]);
        }

        /// <summary>
        /// Gets the action audit logs by document number and approver.
        /// </summary>
        /// <param name="documentNumber">The document number.</param>
        /// <param name="actualApprover">The actual approver.</param>
        /// <returns>List of ActionAuditLogTableRow as per the query results.</returns>
        public List<ActionAuditLogTableRow> GetActionAuditLogsByDocumentNumberAndApprover(string documentNumber, string actualApprover)
        {
            TableQuery<ActionAuditLogTableRow> query = (new TableQuery<ActionAuditLogTableRow>()
                .Where(TableQuery.CombineFilters(TableQuery.GenerateFilterCondition("PartitionKey", QueryComparisons.Equal, documentNumber), TableOperators.And, TableQuery.GenerateFilterCondition("ActualUser", QueryComparisons.Equal, actualApprover))));
            return _tableHelper.GetDataCollectionByTableQuery<ActionAuditLogTableRow>(_config[ConfigurationKey.ActionAuditLogAzureTableName.ToString()], query);
        }

        /// <summary>
        /// log action details as an asynchronous operation.
        /// </summary>
        /// <param name="documentNumber">The document number.</param>
        /// <param name="impersonatedUser">The impersonated user.</param>
        /// <param name="actualApprover">The actual approver.</param>
        /// <param name="actionType">Type of the action.</param>
        /// <param name="clientDevice">The client device.</param>
        /// <param name="tenantId">The tenant identifier.</param>
        /// <param name="actionTime">The action time.</param>
        /// <param name="summaryJson">SummaryJson object for Cosmos DB logging ActionAuditLog</param>
        /// <returns>Task.</returns>
        public async Task LogActionDetailsAsync(string documentNumber, string impersonatedUser, string actualApprover, string actionType, string clientDevice, string tenantId, DateTime actionTime, SummaryJson summaryJson)
        {
            try
            {
                var actionAuditLogTableRow = new ActionAuditLogTableRow
                {
                    PartitionKey = documentNumber,
                    RowKey = Guid.NewGuid().ToString(),
                    ActionTime = actionTime,
                    ActualUser = actualApprover,
                    ImpersonatedUser = impersonatedUser,
                    ClientType = clientDevice,
                    ActionType = actionType,
                    TenantId = tenantId
                };

                await _tableHelper.Insert<ActionAuditLogTableRow>(_config[ConfigurationKey.ActionAuditLogAzureTableName.ToString()], actionAuditLogTableRow);

                // Insert into Cosmos DB
                ActionAuditLogInfo auditDataObject = null;

                auditDataObject = new ActionAuditLogInfo()
                {
                    DisplayDocumentNumber = documentNumber,
                    ActionDateTime = actionTime.ToUniversalTime().ToString("o"),
                    TenantId = tenantId,
                    ActionTaken = actionType,
                    UnitValue = summaryJson?.UnitValue ?? string.Empty,
                    UnitOfMeasure = summaryJson?.UnitOfMeasure ?? string.Empty,
                    ActionStatus = "Success",
                    ErrorMessage = string.Empty,
                    ClientType = clientDevice,
                    Approver = actualApprover
                };
                await Task.Run(() => _cosmosDbHelper.InsertDocumentAsync(auditDataObject));
            }
            catch (Exception ex)
            {
                _logProvider.LogError(TrackingEvent.LogActionAuditFailure, ex);
            }
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
                _logProvider.LogError(TrackingEvent.LogActionAuditFailure, ex);
            }
        }
    }
}