// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace Microsoft.CFS.Approvals.Data.Azure.CosmosDb.Interface
{
    using System.Collections.Generic;
    using System.Linq.Expressions;
    using System.Threading.Tasks;
    using Data.Azure.CosmosDb.Model;
    using Microsoft.Azure.Documents;
    using Microsoft.Azure.Documents.Client;
    using Newtonsoft.Json.Linq;

    public interface ICosmosDbHelper
    {
        /// <summary>
        /// Gets the client.
        /// </summary>
        /// <param name="endpoint">The endpoint</param>
        /// <param name="authKey">The authentication key</param>
        /// <returns>Task.</returns>
        Task<DocumentClient> GetClient(string endpoint, string authKey);

        /// <summary>
        /// Sets the target.
        /// </summary>
        /// <param name="databaseName">Name of the database.</param>
        /// <param name="collectionName">Name of the collection.</param>
        /// <param name="client">Document client</param>
        Task SetTarget(string databaseName, string collectionName, DocumentClient client = null);

        /// <summary>
        /// Gets the document
        /// </summary>
        /// <param name="id"></param>
        /// <param name="partitionKey"></param>
        /// <param name="client">Document client</param>
        /// <param name="databaseName">Name of the database.</param>
        /// <param name="collectionName">Name of the collection.</param>
        /// <returns></returns>
        Task<Document> GetDocument(string id, string partitionKey = "", DocumentClient client = null, string databaseName = "", string collectionName = "");

        /// <summary>
        /// Gets one expense contract by query
        /// </summary>
        /// <param name="query"></param>
        /// <param name="databaseName">Name of the database.</param>
        /// <param name="collectionName">Name of the collection.</param>
        /// <returns></returns>
        Task<T> GetDocumentByQuery<T>(string query, string databaseName = "", string collectionName = "");

        /// <summary>
        /// Get Documents in batch
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="predicate"></param>
        /// <param name="batchSize"></param>
        /// <param name="databaseName">Name of the database.</param>
        /// <param name="collectionName">Name of the collection.</param>
        /// <returns></returns>
        Task<IEnumerable<T>> GetItemsAsync<T>(Expression<System.Func<T, bool>> predicate, int batchSize, string databaseName = "", string collectionName = "");

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
        Task<PagedData<T>> GetPagedDocumentsAsync<T>(string sqlQuery,
            int pageSize,
            string continuationToken,
            DocumentClient client = null,
            string databaseName = "",
            string collectionName = "") where T : class;

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
        Task<List<T>> GetAllDocumentsAsync<T>(string sqlQuery,
            string partitionKey = "",
            DocumentClient client = null,
            string databaseName = "",
            string collectionName = "") where T : class;

        /// <summary>
        /// Saves the document.
        /// </summary>
        /// <param name="data">The data.</param>
        /// <param name="databaseName">Name of the database.</param>
        /// <param name="collectionName">Name of the collection.</param>
        /// <returns></returns>
        Task InsertDocumentAsync(object data, string databaseName = "", string collectionName = "");

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
        Task<JObject> InsertDocumentsAsync<T>(List<T> documents,
            bool disableAutomaticIdGeneration = false,
            bool enableUpsert = true,
            int? maxConcurrencyPerPartitionKeyRange = null,
            int? maxInMemorySortingBatchSize = null,
            string databaseName = "",
            string collectionName = "") where T : class;

        /// <summary>
        /// Inserts or Replaces the document.
        /// </summary>
        /// <param name="databaseName">Name of the database.</param>
        /// <param name="collectionName">Name of the collection.</param>
        /// <param name="data">The data.</param>
        Task<Document> UpsertDocumentAsync(string databaseName, string collectionName, object data);

        /// <summary>
        /// Replaces the document.
        /// </summary>
        /// <param name="doc"></param>
        /// <param name="data">The data.</param>
        Task ReplaceDocumentAsync(Document doc, object data);

        /// <summary>
        /// Deletes document by document id
        /// </summary>
        /// <param name="documentId"></param>
        /// <param name="partitionKey"></param>
        /// <param name="databaseName">Name of the database.</param>
        /// <param name="collectionName">Name of the collection.</param>
        /// <returns></returns>
        Task DeleteDocumentAsync(string documentId, string partitionKey, string databaseName = "", string collectionName = "");

        /// <summary>
        /// Executes stored procedure
        /// </summary>
        /// <param name="sprocName"></param>
        /// <param name="partitionKey"></param>
        /// <param name="jsonContent"></param>
        /// <param name="databaseName">Name of the database.</param>
        /// <param name="collectionName">Name of the collection.</param>
        /// <returns></returns>
        Task<bool> ExecSprocAsync(string sprocName, string partitionKey, string jsonContent, string databaseName = "", string collectionName = "");
    }
}