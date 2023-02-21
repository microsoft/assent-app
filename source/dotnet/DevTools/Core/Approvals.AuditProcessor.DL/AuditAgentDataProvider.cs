// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace Microsoft.CFS.Approvals.AuditProcessor.DL;

using System;
using System.Collections.Generic;
using Microsoft.Azure.Cosmos;
using Microsoft.Azure.ServiceBus;
using Microsoft.CFS.Approvals.AuditProcessor.DL.Interface;
using Microsoft.CFS.Approvals.Contracts;
using Microsoft.CFS.Approvals.Contracts.DataContracts;
using Microsoft.CFS.Approvals.Data.Azure.CosmosDb.Interface;
using Microsoft.CFS.Approvals.LogManager.Model;
using Microsoft.CFS.Approvals.LogManager.Provider.Interface;
using Microsoft.Extensions.Configuration;

public class AuditAgentDataProvider : IAuditAgentDataProvider
{
    /// <summary>
    /// The configuration
    /// </summary>
    private readonly IConfiguration _config;

    /// <summary>
    /// The log provider
    /// </summary>
    private readonly ILogProvider _logProvider;

    /// <summary>
    /// The cosmosdb helper
    /// </summary>
    private readonly ICosmosDbHelper _cosmosDbHelper;


    /// <summary>
    /// The document name
    /// </summary>
    private static string docDbName = string.Empty;

    /// <summary>
    /// The document collection name
    /// </summary>
    private static string docCollectionName = string.Empty;

    /// <summary>
    /// Initializes a new instance of the <see cref="AuditAgentDataProvider"/> class.
    /// </summary>
    /// <param name="config">The configuration.</param>
    /// <param name="logProvider">The log provider.</param>
    public AuditAgentDataProvider(IConfiguration config, ILogProvider logProvider, ICosmosDbHelper cosmosDbHelper)
    {
        /// IConfigurationHelper configHelper, ILogProvider logProvider
        _config = config;
        _logProvider = logProvider;
        _cosmosDbHelper = cosmosDbHelper;
        docCollectionName = _config[ConfigurationKey.CosmosDbCollectionAuditAgent.ToString()];
        docDbName = _config[ConfigurationKey.CosmosDbNameAuditAgent.ToString()];
        _cosmosDbHelper.SetTarget(docDbName, docCollectionName, _config[ConfigurationKey.CosmosDbPartitionKeyPathAuditAgent.ToString()]);
    }

    /// <summary>
    /// Inserts the in to document database.
    /// </summary>
    /// <param name="approvalRequestExpressionExt">The approval request expression ext.</param>
    /// <param name="rawArJson">The raw ar json.</param>
    /// <param name="brokeredMessage">The brokered message.</param>
    /// <param name="exceptionMessage">The exception message.</param>
    /// <param name="stackTrace">The stack trace.</param>
    /// <returns></returns>
    public void InsertInToDocumentDB(ApprovalRequestExpressionExt approvalRequestExpressionExt, string rawArJson, Message brokeredMessage,
        string exceptionMessage = "", string stackTrace = "")
    {
        DateTime dateTime;
        // Added try catch if EnqueuedTimeUtc throws an error.
        try
        {
            dateTime = brokeredMessage.SystemProperties.EnqueuedTimeUtc;
        }
        catch
        {
            dateTime = DateTime.UtcNow;
        }

        try
        {
            var applicationId = "";
            if (brokeredMessage.UserProperties.ContainsKey("ApplicationId"))
                applicationId = brokeredMessage.UserProperties["ApplicationId"].ToString();

            dynamic auditDataObject = new
            {
                BrokeredMsgId = Guid.Parse(brokeredMessage.MessageId),
                ApprovalRequest = approvalRequestExpressionExt,
                Raw_AR = rawArJson,
                BrokeredMessageProperty = brokeredMessage.UserProperties,
                EnqueuedTimeUtc = dateTime,
                ExceptionMessage = exceptionMessage,
                ExceptionStackTrace = stackTrace,
                CorrelationID = brokeredMessage.MessageId,
                TenantId = applicationId,
                id = Guid.NewGuid()
            };
            _cosmosDbHelper.InsertDocumentAsync(auditDataObject, new PartitionKey(auditDataObject.TenantId)).Wait();
        }
        catch (Exception ex)
        {
            _logProvider.LogError(TrackingEvent.AuditAgentDocDbOperationFailed, ex, new Dictionary<LogDataKey, object>() { { LogDataKey.DocDbName, docDbName }, { LogDataKey.DocDbCollectionName, docCollectionName } });
        }
    }
}