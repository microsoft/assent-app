// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace Microsoft.CFS.Approvals.Data.Azure.CosmosDb.Interface;

using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Azure.Cosmos;

public interface ICosmosDbHelper
{
    /// <summary>
    /// Gets the client.
    /// </summary>
    /// <param name="endpoint">The endpoint</param>
    /// <param name="authKey">The authentication key</param>
    /// <returns>Task.</returns>
    Task<CosmosClient> GetClient(string endpoint);

    /// <summary>
    /// Sets the target.
    /// </summary>
    /// <param name="databaseName">Name of the database.</param>
    /// <param name="collectionName">Name of the collection.</param>
    /// <param name="client">Document client</param>
    void SetTarget(string databaseName, string collectionName, string partitionKeyPath, CosmosClient client = null);

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
    Task<ItemResponse<T>[]> InsertDocumentsAsync<T>(List<T> documents,
        string databaseName = "",
        string collectionName = "",
        string partitionKeyPath = "") where T : class;

    /// <summary>
    /// Saves the document.
    /// </summary>
    /// <param name="data">The data.</param>
    /// <param name="databaseName">Name of the database.</param>
    /// <param name="collectionName">Name of the collection.</param>
    /// <returns></returns>
    Task<ItemResponse<T>> InsertDocumentAsync<T>(T data, string databaseName = "", string collectionName = "", string partitionKeyPath = "");

    /// <summary>
    /// Saves the document.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="data"></param>
    /// <param name="partitionKey"></param>
    /// <param name="databaseName"></param>
    /// <param name="collectionName"></param>
    /// <param name="partitionKeyPath"></param>
    /// <returns></returns>
    Task<ItemResponse<T>> InsertDocumentAsync<T>(T data, PartitionKey partitionKey, string databaseName = "", string collectionName = "", string partitionKeyPath = "");

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
        CosmosClient client = null,
        string databaseName = "",
        string collectionName = "",
        string partitionKeyPath = "") where T : class;

    /// <summary>
    /// Fetch all documents matched based on sql query definition
    /// </summary>
    /// <typeparam name="T">Type to return</typeparam>
    /// <param name="sqlQuery">Sql query</param>
    /// <param name="partitionKey"></param>
    /// <param name="client">Document client</param>
    /// <param name="databaseName">Name of the database.</param>
    /// <param name="collectionName">Name of the collection.</param>
    /// <returns>Returns List of type</returns>
    Task<List<T>> GetAllDocumentsAsync<T>(QueryDefinition queryDefinition,
        string partitionKey = "",
        CosmosClient client = null,
        string databaseName = "",
        string collectionName = "",
        string partitionKeyPath = "") where T : class;
}