// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace Microsoft.CFS.Approvals.AuditProcessor.DL
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;
    using Microsoft.Azure.Documents;
    using Microsoft.Azure.Documents.Client;
    using Microsoft.Azure.ServiceBus;
    using Microsoft.CFS.Approvals.AuditProcessor.DL.Interface;
    using Microsoft.CFS.Approvals.Contracts;
    using Microsoft.CFS.Approvals.Contracts.DataContracts;
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
        /// The endpoint URL
        /// </summary>
        private readonly string _endpointUrl;

        /// <summary>
        /// The authorization key
        /// </summary>
        private readonly string _authorizationKey;

        /// <summary>
        /// The log provider
        /// </summary>
        private readonly ILogProvider _logProvider;

        /// <summary>
        /// The primary
        /// </summary>
        private const string PRIMARY = "primary";

        /// <summary>
        /// The client
        /// </summary>
        private const string CLIENT = "client";

        /// <summary>
        /// The document name
        /// </summary>
        private static string docDbName = string.Empty;

        /// <summary>
        /// The document collection name
        /// </summary>
        private static string docCollectionName = string.Empty;

        /// <summary>
        /// The dictionary document client
        /// </summary>
        private static Dictionary<string, DocumentClient> dictDocumentClient = null;

        /// <summary>
        /// The dictionary document collection
        /// </summary>
        private static Dictionary<string, DocumentCollection> dictDocumentCollection = null;

        /// <summary>
        /// Initializes a new instance of the <see cref="AuditAgentDataProvider"/> class.
        /// </summary>
        /// <param name="config">The configuration.</param>
        /// <param name="logProvider">The log provider.</param>
        public AuditAgentDataProvider(IConfiguration config, ILogProvider logProvider)
        {
            if (dictDocumentCollection == null)
            {
                /// IConfigurationHelper configHelper, ILogProvider logProvider
                _config = config;
                _logProvider = logProvider;
                _endpointUrl = _config[ConfigurationKey.CosmosDbEndPoint.ToString()];
                _authorizationKey = _config[ConfigurationKey.CosmosDbAuthKey.ToString()];

                // Create a new instance of the DocumentClient.
                var client = new DocumentClient(new Uri(_endpointUrl), _authorizationKey);

                docCollectionName = _config[ConfigurationKey.CosmosDbCollectionAuditAgent.ToString()];
                docDbName = _config[ConfigurationKey.CosmosDbNameAuditAgent.ToString()];

                dictDocumentClient = new Dictionary<string, DocumentClient>();
                dictDocumentCollection = new Dictionary<string, DocumentCollection>();

                if (!dictDocumentClient.ContainsKey(CLIENT))
                    dictDocumentClient.Add(CLIENT, client);

                var database = client.CreateDatabaseQuery().Where(x => x.Id == docDbName).AsEnumerable().FirstOrDefault();

                if (database == null)
                {
                    throw new Exception($"Document DB {docDbName} doesn't exist.");
                }

                // Try to retrieve the collection (MCLIENTicrosoft.Azure.Documents.DocumentCollection) whose Id is equal to collectionId
                var documentCollection = client.CreateDocumentCollectionQuery(database.SelfLink).Where(c => c.Id == docCollectionName).ToArray().FirstOrDefault();
                if (documentCollection == null)
                {
                    throw new Exception($"Document DB collection {docCollectionName} doesn't exist.");
                }

                if (!dictDocumentCollection.ContainsKey(PRIMARY))
                    dictDocumentCollection.Add(PRIMARY, documentCollection);
            }
        }

        /// <summary>
        /// Inserts the in to document database.
        /// </summary>
        /// <param name="approvalRequestExpressionExt">The approval request expression ext.</param>
        /// <param name="rawArJson">The raw ar json.</param>
        /// <param name="brokeredMessage">The brokered message.</param>
        /// <param name="auditAgentLoggingHelper">Audit agent logging helper object</param>
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
                    TenantId = applicationId
                };

                var docResponse = dictDocumentClient[CLIENT].CreateDocumentAsync(dictDocumentCollection[PRIMARY].SelfLink, auditDataObject);
            }
            catch (Exception ex)
            {
                _logProvider.LogError(TrackingEvent.AuditAgentDocDbOperationFailed, ex, new Dictionary<LogDataKey, object>() { { LogDataKey.DocDbName, docDbName }, { LogDataKey.DocDbCollectionName, docCollectionName } });
            }
        }

        /// <summary>
        /// Deletes document from the document database.
        /// </summary>
        /// <param name="document">Document</param>
        /// <returns></returns>
        public async Task DeleteDocument(string document)
        {
            await dictDocumentClient[CLIENT].DeleteDocumentAsync(document);
        }

        /// <summary>
        /// Gets list of documents from the document database.
        /// </summary>
        /// <param name="documentNumber">Document Number</param>
        /// <param name="partitionKey">Partition Key</param>
        /// <returns></returns>
        public List<dynamic> GetDocuments(string documentNumber, string partitionKey)
        {
            try
            {
                SqlParameterCollection sqlparameters = new SqlParameterCollection();
                string queryText =
                    "SELECT * " +
                    "FROM audit c " +
                    "WHERE c.ApprovalRequest.ApprovalIdentifier.DisplayDocumentNumber = @DocNumber";
                sqlparameters.Add(new SqlParameter("@DocNumber", documentNumber));

                return dictDocumentClient[CLIENT].CreateDocumentQuery<dynamic>(dictDocumentCollection[PRIMARY].SelfLink,
                                                                              new SqlQuerySpec { QueryText = queryText, Parameters = sqlparameters },
                                                                              new FeedOptions() { PartitionKey = new PartitionKey(partitionKey) }).ToList();
            }
            catch (Exception ex)
            {
                return null;
            }
        }
    }
}