// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace Microsoft.CFS.Approvals.Data.Azure.CosmosDb.Helpers
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Linq.Expressions;
    using System.Threading.Tasks;
    using Data.Azure.CosmosDb.Interface;
    using Data.Azure.CosmosDb.Model;
    using Microsoft.Azure.Documents;
    using Microsoft.Azure.Documents.Client;
    using Microsoft.Azure.Documents.Linq;
    using Newtonsoft.Json.Linq;

    public class CosmosDbHelper : ICosmosDbHelper
    {
        #region Varibales

        /// <summary>
        /// The endpoint
        /// </summary>
        private readonly string _endpoint;

        /// <summary>
        /// The authentication key
        /// </summary>
        private readonly string _authKey;

        /// <summary>
        /// The client
        /// </summary>
        private readonly DocumentClient _client;

        /// <summary>
        /// The database
        /// </summary>
        private Database _database;

        /// <summary>
        /// The collection
        /// </summary>
        private DocumentCollection _collection;

        #endregion Varibales

        #region Public Methods

        /// <summary>
        /// Initializes a new instance of the <see cref="CosmosDbHelper"/> class.
        /// </summary>
        /// <param name="endpoint">The endpoint.</param>
        /// <param name="authKey">The authentication key.</param>
        /// <param name="databaseName">Name of the database.</param>
        /// <param name="collectionName">Name of the collection.</param>
        /// <exception cref="Exception">Error connecting to docDB</exception>
        public CosmosDbHelper(string endpoint, string authKey, string databaseName = "", string collectionName = "")
        {
            _endpoint = endpoint;
            _authKey = authKey;
            _client = GetClient(_endpoint, _authKey).Result;
            if (!string.IsNullOrWhiteSpace(databaseName))
            {
                _database = GetDataBase(_client, databaseName).Result;
            }
            if (!string.IsNullOrWhiteSpace(collectionName))
            {
                _collection = GetCollection(_client, _database.SelfLink, collectionName).Result;
            }
        }

        /// <summary>
        /// Gets the client.
        /// </summary>
        /// <param name="endpoint">The endpoint</param>
        /// <param name="authKey">The authentication key</param>
        /// <returns>Task.</returns>
        public async Task<DocumentClient> GetClient(string endpoint, string authKey)
        {
            await Task.Delay(100);
            if (string.IsNullOrWhiteSpace(endpoint) && string.IsNullOrWhiteSpace(authKey))
            {
                endpoint = _endpoint;
                authKey = _authKey;
            }
            return new DocumentClient(new Uri(endpoint), authKey, ConnectionPolicy.Default);
        }

        /// <summary>
        /// Sets the target.
        /// </summary>
        /// <param name="databaseName">Name of the database.</param>
        /// <param name="collectionName">Name of the collection.</param>
        /// <param name="client">Document client</param>
        public async Task SetTarget(string databaseName, string collectionName, DocumentClient client = null)
        {
            if (client == null)
            {
                client = _client;
            }

            _database = await GetDataBase(client, databaseName);
            _collection = await GetCollection(client, _database.SelfLink, collectionName);
        }

        /// <summary>
        /// Gets the document
        /// </summary>
        /// <param name="id"></param>
        /// <param name="partitionKey"></param>
        /// <param name="client">Document client</param>
        /// <param name="databaseName">Name of the database.</param>
        /// <param name="collectionName">Name of the collection.</param>
        /// <returns></returns>
        public async Task<Document> GetDocument(string id, string partitionKey = "", DocumentClient client = null, string databaseName = "", string collectionName = "")
        {
            if (client == null)
            {
                client = _client;
            }

            var database = _database;
            var collection = _collection;

            if (!string.IsNullOrWhiteSpace(databaseName))
            {
                database = await GetDataBase(client, databaseName);
            }
            if (!string.IsNullOrWhiteSpace(collectionName))
            {
                collection = await GetCollection(client, database.SelfLink, collectionName);
            }

            if (string.IsNullOrWhiteSpace(partitionKey))
            {
                return client.CreateDocumentQuery<Document>(collection.SelfLink, new FeedOptions { MaxItemCount = -1, EnableCrossPartitionQuery = true })
                    .Where(r => r.Id == id)
                    .AsEnumerable().SingleOrDefault();
            }
            return client.CreateDocumentQuery<Document>(collection.SelfLink, new FeedOptions { MaxItemCount = -1, PartitionKey = new PartitionKey(partitionKey) })
                .Where(r => r.Id == id)
                .AsEnumerable().SingleOrDefault();
        }

        /// <summary>
        /// Get Documents in batch
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="predicate"></param>
        /// <param name="batchSize"></param>
        /// <param name="databaseName">Name of the database.</param>
        /// <param name="collectionName">Name of the collection.</param>
        /// <returns></returns>
        public async Task<IEnumerable<T>> GetItemsAsync<T>(Expression<Func<T, bool>> predicate, int batchSize, string databaseName = "", string collectionName = "")
        {
            var database = _database;
            var collection = _collection;
            if (!string.IsNullOrWhiteSpace(databaseName))
            {
                database = await GetDataBase(_client, databaseName);
            }
            if (!string.IsNullOrWhiteSpace(collectionName))
            {
                collection = await GetCollection(_client, database.SelfLink, collectionName);
            }

            var queryOptions = new FeedOptions { MaxItemCount = -1, EnableCrossPartitionQuery = true };
            return _client.CreateDocumentQuery<T>(collection.SelfLink, queryOptions).Where(predicate).Take(batchSize);
        }

        /// <summary>
        /// Get one expense contract by query
        /// </summary>
        /// <param name="query"></param>
        /// <param name="databaseName">Name of the database.</param>
        /// <param name="collectionName">Name of the collection.</param>
        /// <returns></returns>
        public async Task<T> GetDocumentByQuery<T>(string query, string databaseName = "", string collectionName = "")
        {
            var database = _database;
            var collection = _collection;
            if (!string.IsNullOrWhiteSpace(databaseName))
            {
                database = await GetDataBase(_client, databaseName);
            }
            if (!string.IsNullOrWhiteSpace(collectionName))
            {
                collection = await GetCollection(_client, database.SelfLink, collectionName);
            }

            var queryOptions = new FeedOptions { MaxItemCount = -1, EnableCrossPartitionQuery = true };
            return _client.CreateDocumentQuery<T>(collection.SelfLink, query, queryOptions).AsEnumerable().FirstOrDefault();
        }

        /// <summary>
        /// Fetch documents matched based on sql query and page size.
        /// </summary>
        /// <typeparam name="T">Type to return.</typeparam>
        /// <param name="sqlQuery">Sql query</param>
        /// <param name="pageSize">The pagesize.</param>
        /// <param name="continuationToken">The continuation token.</param>
        /// <param name="client">Document client</param>
        /// <param name="databaseName">Name of the database.</param>
        /// <param name="collectionName">Name of the collection.</param>
        /// <returns>Returns paged data</returns>
        public async Task<PagedData<T>> GetPagedDocumentsAsync<T>(string sqlQuery,
            int pageSize,
            string continuationToken,
            DocumentClient client = null,
            string databaseName = "",
            string collectionName = "") where T : class
        {
            if (client == null)
            {
                client = _client;
            }

            var database = _database;
            var collection = _collection;
            if (!string.IsNullOrWhiteSpace(databaseName))
            {
                database = await GetDataBase(client, databaseName);
            }
            if (!string.IsNullOrWhiteSpace(collectionName))
            {
                collection = await GetCollection(client, database.SelfLink, collectionName);
            }

            var options = new FeedOptions
            {
                PopulateQueryMetrics = true,
                MaxItemCount = pageSize,
                EnableCrossPartitionQuery = true
            };

            if (!string.IsNullOrWhiteSpace(continuationToken))
            {
                options.RequestContinuation = continuationToken;
            }

            var query = client.CreateDocumentQuery<T>(collection.SelfLink, sqlQuery, options).AsDocumentQuery();

            var result = await query.ExecuteNextAsync<T>();
            return new PagedData<T> { Result = result.ToList<T>(), ContinuationToken = result.ResponseContinuation };
        }

        /// <summary>
        /// Fetch all documents matched based on sql query
        /// </summary>
        /// <typeparam name="T">Type to return</typeparam>
        /// <param name="sqlQuery">Sql query</param>
        /// <param name="partitionKey"></param>
        /// <param name="client">Document client</param>
        /// <param name="databaseName">Name of the database.</param>
        /// <param name="collectionName">Name of the collection.</param>
        /// <returns>Returns List of type</returns>
        public async Task<List<T>> GetAllDocumentsAsync<T>(string sqlQuery,
            string partitionKey = "",
            DocumentClient client = null,
            string databaseName = "",
            string collectionName = "") where T : class
        {
            if (client == null)
            {
                client = _client;
            }

            var database = _database;
            var collection = _collection;
            if (!string.IsNullOrWhiteSpace(databaseName))
            {
                database = await GetDataBase(client, databaseName);
            }
            if (!string.IsNullOrWhiteSpace(collectionName))
            {
                collection = await GetCollection(client, database.SelfLink, collectionName);
            }

            List<T> results = new List<T>();
            var options = new FeedOptions
            {
                PopulateQueryMetrics = true,
                MaxItemCount = -1,
                MaxDegreeOfParallelism = 10
            };

            if (string.IsNullOrWhiteSpace(partitionKey))
            {
                options.EnableCrossPartitionQuery = true;
            }
            else
            {
                options.EnableCrossPartitionQuery = false;
                options.PartitionKey = new PartitionKey(partitionKey);
            }

            var query = client.CreateDocumentQuery<T>(collection.SelfLink, sqlQuery, options).AsDocumentQuery();

            while (query.HasMoreResults)
            {
                var result = await query.ExecuteNextAsync<T>();
                results.AddRange(result.ToList());
            }

            return results;
        }

        /// <summary>
        /// Saves the document.
        /// </summary>
        /// <param name="data">The data.</param>
        /// <param name="databaseName">Name of the database.</param>
        /// <param name="collectionName">Name of the collection.</param>
        /// <returns></returns>
        public async Task InsertDocumentAsync(object data, string databaseName = "", string collectionName = "")
        {
            var database = _database;
            var collection = _collection;
            if (!string.IsNullOrWhiteSpace(databaseName))
            {
                database = await GetDataBase(_client, databaseName);
            }
            if (!string.IsNullOrWhiteSpace(collectionName))
            {
                collection = await GetCollection(_client, database.SelfLink, collectionName);
            }
            await _client.CreateDocumentAsync(collection.SelfLink, data);
        }

        /// <summary>
        /// Inserts documents in batch
        /// </summary>
        /// <param name="documents">List of documents</param>
        /// <param name="disableAutomaticIdGeneration">The disableAutomaticIdGeneration</param>
        /// <param name="enableUpsert">The enableUpsert</param>
        /// <param name="maxConcurrencyPerPartitionKeyRange">The maxConcurrencyPerPartitionKeyRange</param>
        /// <param name="maxInMemorySortingBatchSize">The maxInMemorySortingBatchSize</param>
        /// <param name="databaseName">Name of the database.</param>
        /// <param name="collectionName">Name of the collection.</param>
        /// <returns>Returns JObject which contains output of BulkImport</returns>
        public async Task<JObject> InsertDocumentsAsync<T>(List<T> documents,
            bool disableAutomaticIdGeneration = false,
            bool enableUpsert = true,
            int? maxConcurrencyPerPartitionKeyRange = null,
            int? maxInMemorySortingBatchSize = null,
            string databaseName = "",
            string collectionName = "") where T : class
        {
            var database = _database;
            var collection = _collection;
            if (!string.IsNullOrWhiteSpace(databaseName))
            {
                database = await GetDataBase(_client, databaseName);
            }
            if (!string.IsNullOrWhiteSpace(collectionName))
            {
                collection = await GetCollection(_client, database.SelfLink, collectionName);
            }

            // Initialize BulkExecutor
            var bulkExecutor = await CosmosDbBulkExecutor.Instance(_endpoint, _authKey, collection);
            return await bulkExecutor.BulkImportAsync(documents, enableUpsert, disableAutomaticIdGeneration, maxConcurrencyPerPartitionKeyRange, maxInMemorySortingBatchSize);
        }

        /// <summary>
        /// Inserts or Replaces the document.
        /// </summary>
        /// <param name="databaseName">Name of the database.</param>
        /// <param name="collectionName">Name of the collection.</param>
        /// <param name="data">The data.</param>
        /// <returns></returns>
        public async Task<Document> UpsertDocumentAsync(string databaseName, string collectionName, object data)
        {
            var database = _database;
            var collection = _collection;
            if (!string.IsNullOrWhiteSpace(databaseName))
            {
                database = await GetDataBase(_client, databaseName);
            }
            if (!string.IsNullOrWhiteSpace(collectionName))
            {
                collection = await GetCollection(_client, database.SelfLink, collectionName);
            }
            return await _client.UpsertDocumentAsync(collection.SelfLink, data);
        }

        /// <summary>
        /// Replaces the document.
        /// </summary>
        /// <param name="doc"></param>
        /// <param name="data">The data.</param>
        /// <returns></returns>
        public async Task ReplaceDocumentAsync(Document doc, object data)
        {
            await _client.ReplaceDocumentAsync(doc.SelfLink, data);
        }

        /// <summary>
        /// Deletes document by document id
        /// </summary>
        /// <param name="documentId"></param>
        /// <param name="collectionName"></param>
        /// <param name="partitionKey"></param>
        /// <param name="databaseName">Name of the database.</param>
        /// <param name="collectionName">Name of the collection.</param>
        /// <returns></returns>
        public async Task DeleteDocumentAsync(string documentId, string partitionKey, string databaseName = "", string collectionName = "")
        {
            var requestOptions = new RequestOptions { PartitionKey = new PartitionKey(partitionKey) };
            var database = _database;
            var collection = _collection;
            if (!string.IsNullOrWhiteSpace(databaseName))
            {
                database = await GetDataBase(_client, databaseName);
            }
            if (!string.IsNullOrWhiteSpace(collectionName))
            {
                collection = await GetCollection(_client, database.SelfLink, collectionName);
            }

            var result = await _client.ReadDocumentAsync(UriFactory.CreateDocumentUri(database.Id, collection.Id, documentId), requestOptions);

            await _client.DeleteDocumentAsync(result.Resource.SelfLink, requestOptions);
        }

        /// <summary>
        /// Executes stored procedure
        /// </summary>
        /// <param name="sprocName"></param>
        /// <param name="partitionKey"></param>
        /// <param name="jsonContent"></param>
        /// <param name="databaseName">Name of the database.</param>
        /// <param name="collectionName">Name of the collection.</param>
        /// <returns></returns>
        public async Task<bool> ExecSprocAsync(string sprocName, string partitionKey, string jsonContent, string databaseName = "", string collectionName = "")
        {
            var requestOptions = new RequestOptions() { PartitionKey = new PartitionKey(partitionKey) };
            var database = _database;
            var collection = _collection;
            if (!string.IsNullOrWhiteSpace(databaseName))
            {
                database = await GetDataBase(_client, databaseName);
            }
            if (!string.IsNullOrWhiteSpace(collectionName))
            {
                collection = await GetCollection(_client, database.SelfLink, collectionName);
            }
            return await _client.ExecuteStoredProcedureAsync<bool>(UriFactory.CreateStoredProcedureUri(database.Id, collection.Id, sprocName), requestOptions, jsonContent, partitionKey);
        }

        #endregion Public Methods

        #region Private Methods

        /// <summary>
        /// Gets the data base.
        /// </summary>
        /// <param name="client">Document client</param>
        /// <param name="databaseName">Name of the database.</param>
        /// <returns>Database.</returns>
        private async Task<Database> GetDataBase(DocumentClient client, string databaseName)
        {
            var db = client.CreateDatabaseQuery()
                         .Where(d => d.Id == databaseName)
                         .AsEnumerable()
                         .FirstOrDefault() ?? await client.CreateDatabaseAsync(new Database { Id = databaseName });
            return db;
        }

        /// <summary>
        /// Gets the collection.
        /// </summary>
        /// <param name="client">Document client</param>
        /// <param name="dbLink">The database link.</param>
        /// <param name="collectionName">Name of the collection.</param>
        /// <returns>DocumentCollection.</returns>
        private async Task<DocumentCollection> GetCollection(DocumentClient client, string dbLink, string collectionName)
        {
            var col = client.CreateDocumentCollectionQuery(dbLink)
                          .Where(c => c.Id == collectionName)
                          .AsEnumerable()
                          .FirstOrDefault();

            if (col != null)
            {
                return col;
            }

            var collectionSpec = new DocumentCollection { Id = collectionName };
            var requestOptions = new RequestOptions { OfferType = "S1" };

            col = await client.CreateDocumentCollectionAsync(dbLink, collectionSpec, requestOptions);

            return col;
        }

        #endregion Private Methods
    }
}