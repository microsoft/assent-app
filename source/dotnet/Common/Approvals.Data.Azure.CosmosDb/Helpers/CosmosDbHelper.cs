// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace Microsoft.CFS.Approvals.Data.Azure.CosmosDb.Helpers;

using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using global::Azure.Identity;
using Microsoft.Azure.Cosmos;
using Microsoft.CFS.Approvals.Data.Azure.CosmosDb.Interface;

public class CosmosDbHelper : ICosmosDbHelper
{

    #region Varibales

/// <summary>
/// The endpoint
/// </summary>
private readonly string _endpoint;

    /// <summary>
    /// The client
    /// </summary>
    private readonly CosmosClient _client;

/// <summary>
/// The database
/// </summary>
private Database _database;

    /// <summary>
    /// The collection
    /// </summary>
    private Container _collection;

#endregion Varibales

#region Public Methods

    public CosmosDbHelper(CosmosClient client)
    {
        _client = client;
    }

    public async Task<CosmosClient> GetClient(string endpoint)
    {
        await Task.Delay(100);
        if (string.IsNullOrWhiteSpace(endpoint))
        {
            endpoint = _endpoint;
        }
        return new CosmosClient(endpoint, new DefaultAzureCredential());
    }

    public async Task SetTarget(string databaseName, string collectionName, string partitionKeyPath, CosmosClient client = null)
    {
        if (client == null)
        {
            client = _client;
        }

        _database = await GetDataBase(client, databaseName);
        _collection = await GetCollection(client, databaseName, collectionName, partitionKeyPath);
    }

    #endregion

    #region Private Methods

    /// <summary>
    /// Gets the data base.
    /// </summary>
    /// <param name="client">Document client</param>
    /// <param name="databaseName">Name of the database.</param>
    /// <returns>Database.</returns>
    private async Task<Database> GetDataBase(CosmosClient client, string databaseName)
    {
        return (await client.CreateDatabaseIfNotExistsAsync(databaseName)).Database;
    }

    /// <summary>
    /// Gets the collection.
    /// </summary>
    /// <param name="client">Document client</param>
    /// <param name="databaseName">The database name.</param>
    /// <param name="collectionName">Name of the collection.</param>
    /// <returns>DocumentCollection.</returns>
    private async Task<Container> GetCollection(CosmosClient client, string databaseName, string collectionName, string partitionKeyPath)
    {
        var database = (await client.CreateDatabaseIfNotExistsAsync(databaseName)).Database;
        ContainerProperties containerProperties = new ContainerProperties();
        containerProperties.Id = collectionName;
        containerProperties.PartitionKeyPath = partitionKeyPath;

        var containerResposne = await database.CreateContainerIfNotExistsAsync(containerProperties);
        //return database.GetContainer(collectionName);
        return containerResposne.Container;
    }

    #endregion

    #region Public Methods

    public async Task<ItemResponse<T>[]> InsertDocumentsAsync<T>(List<T> documents,
        string databaseName = "", string collectionName = "", string partitionKeyPath = "") where T : class
    {
        var collection = _collection;
        if (!string.IsNullOrWhiteSpace(databaseName) && !string.IsNullOrWhiteSpace(collectionName))
        {
            collection = await GetCollection(_client, databaseName, collectionName, partitionKeyPath);
        }

        List<Task<ItemResponse<T>>> concurrentTasks = new List<Task<ItemResponse<T>>>();
        foreach (T document in documents)
        {
            concurrentTasks.Add(collection.CreateItemAsync(document));
        }
        return await Task.WhenAll(concurrentTasks);
    }

    /// <summary>
    /// Saves the document.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="data"></param>
    /// <param name="databaseName"></param>
    /// <param name="collectionName"></param>
    /// <returns></returns>
    public async Task<ItemResponse<T>> InsertDocumentAsync<T>(T data, string databaseName = "", string collectionName = "", string partitionKeyPath = "")
    {
        var collection = _collection;
        if (!string.IsNullOrWhiteSpace(databaseName) && !string.IsNullOrWhiteSpace(collectionName))
        {
            collection = await GetCollection(_client, databaseName, collectionName, partitionKeyPath);
        }
        return await collection.CreateItemAsync(data);
    }

    /// <summary>
    /// Insert Document Async
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="data"></param>
    /// <param name="databaseName"></param>
    /// <param name="collectionName"></param>
    /// <returns></returns>
    public async Task<ItemResponse<T>> InsertDocumentAsync<T>(T data, PartitionKey partitionKey, string databaseName = "", string collectionName = "", string partitionKeyPath = "")
    {
        var collection = _collection;
        if (!string.IsNullOrWhiteSpace(databaseName) && !string.IsNullOrWhiteSpace(collectionName))
        {
            collection = await GetCollection(_client, databaseName, collectionName, partitionKeyPath);
        }
        return await collection.CreateItemAsync(data, partitionKey);
    }

    /// <summary>
    /// Get All Documents Async by query
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="sqlQuery"></param>
    /// <param name="partitionKey"></param>
    /// <param name="client"></param>
    /// <param name="databaseName"></param>
    /// <param name="collectionName"></param>
    /// <returns></returns>
    public async Task<List<T>> GetAllDocumentsAsync<T>(string sqlQuery, string partitionKey = "",
        CosmosClient client = null, string databaseName = "", string collectionName = "", string partitionKeyPath = "") where T : class
    {
        if (client == null)
        {
            client = _client;
        }

        var collection = _collection;
        if (!string.IsNullOrWhiteSpace(databaseName) && !string.IsNullOrWhiteSpace(collectionName))
        {
            collection = await GetCollection(client, databaseName, collectionName, partitionKeyPath);
        }

        List<T> results = new List<T>();

        QueryRequestOptions requestOptions = new QueryRequestOptions();
        if (!string.IsNullOrWhiteSpace(partitionKey))
        {
            requestOptions.PartitionKey = new PartitionKey(partitionKey);
        }

        string continuationToken = null;


        var query = collection.GetItemQueryIterator<T>(sqlQuery, continuationToken, requestOptions);
        while (query.HasMoreResults)
        {
            var result = await query.ReadNextAsync();
            results.AddRange(result.Resource.ToList());
        }
        return results;
    }

    /// <summary>
    /// Get All Documents Async by  sql query definition
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="sqlQuery"></param>
    /// <param name="partitionKey"></param>
    /// <param name="client"></param>
    /// <param name="databaseName"></param>
    /// <param name="collectionName"></param>
    /// <returns></returns>
    public async Task<List<T>> GetAllDocumentsAsync<T>(QueryDefinition queryDefinition, string partitionKey = "",
        CosmosClient client = null, string databaseName = "", string collectionName = "", string partitionKeyPath = "") where T : class
    {
        if (client == null)
        {
            client = _client;
        }

        var collection = _collection;
        if (!string.IsNullOrWhiteSpace(databaseName) && !string.IsNullOrWhiteSpace(collectionName))
        {
            collection = await GetCollection(client, databaseName, collectionName, partitionKeyPath);
        }

        List<T> results = new List<T>();

        QueryRequestOptions requestOptions = new QueryRequestOptions();
        if (!string.IsNullOrWhiteSpace(partitionKey))
        {
            requestOptions.PartitionKey = new PartitionKey(partitionKey);
        }

        string continuationToken = null;


        var query = collection.GetItemQueryIterator<T>(queryDefinition, continuationToken, requestOptions);
        while (query.HasMoreResults)
        {
            var result = await query.ReadNextAsync();
            results.AddRange(result.Resource.ToList());
        }
        return results;
    }

    #endregion
}
