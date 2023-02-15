// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace Microsoft.CFS.Approvals.Data.Azure.Storage.Helpers;

using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using global::Azure;
using global::Azure.Core;
using global::Azure.Data.Tables;
using Microsoft.CFS.Approvals.Data.Azure.Storage.Interface;

/// <summary>
/// The TableHelper class
/// </summary>
public class TableHelper : ITableHelper
{
    private readonly string _azureStorageAccountName;
    private readonly TokenCredential _tokenCredential;

    public TableHelper(string accountName, TokenCredential tokenCredential)
    {
        _azureStorageAccountName = accountName;
        _tokenCredential = tokenCredential;
    }

    /// <summary>
    /// Get the table reference
    /// </summary>
    /// <returns></returns>
    private TableClient CreateTableClient(string tableName)
    {
        var tableClient = new TableClient(new Uri($"https://" + _azureStorageAccountName + ".table.core.windows.net/"),
                                            tableName,
                                            _tokenCredential);
        return tableClient;
    }

    /// <summary>
    /// Get Table entity
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="TableName"></param>
    /// <returns></returns>
    public IEnumerable<T> GetTableEntity<T>(string TableName) where T : class, ITableEntity, new()
    {
        TableClient tableClient = CreateTableClient(TableName);
        Pageable<T> queryResults = tableClient.Query<T>();
        return queryResults.AsEnumerable();
    }

    /// <summary>
    /// Get Table Entity by RowKey
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="TableName"></param>
    /// <param name="rowKey"></param>
    /// <returns></returns>
    public T GetTableEntityByRowKey<T>(string TableName, string rowKey) where T : class, ITableEntity, new()
    {
        TableClient tableClient = CreateTableClient(TableName);
        return tableClient.Query<T>(filter: $"RowKey eq '{rowKey}'").FirstOrDefault();
    }

    /// <summary>
    /// Get table entity by PartitionKey
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="TableName"></param>
    /// <param name="PartitionKey"></param>
    /// <returns></returns>
    public T GetTableEntityByPartitionKey<T>(string TableName, string PartitionKey) where T : class, ITableEntity, new()
    {
        TableClient tableClient = CreateTableClient(TableName);
        return tableClient.Query<T>(filter: $"PartitionKey eq '{PartitionKey}'").FirstOrDefault();

    }

    /// <summary>
    /// Get table entity list by RowKey
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="TableName"></param>
    /// <param name="RowKey"></param>
    /// <returns></returns>
    public List<T> GetTableEntityListByRowKey<T>(string TableName, string RowKey) where T : class, ITableEntity, new()
    {
        TableClient tableClient = CreateTableClient(TableName);
        return tableClient.Query<T>(filter: $"RowKey eq '{RowKey}'").ToList();
    }

    /// <summary>
    /// Get table entity list by partitionKey
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="TableName"></param>
    /// <param name="PartitionKey"></param>
    /// <returns></returns>
    public List<T> GetTableEntityListByPartitionKey<T>(string TableName, string PartitionKey) where T : class, ITableEntity, new()
    {
        TableClient tableClient = CreateTableClient(TableName);
        return tableClient.Query<T>(filter: $"PartitionKey eq '{PartitionKey}'").ToList();
    }

    /// <summary>
    /// Get table entity by field
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="TableName"></param>
    /// <param name="fieldName"></param>
    /// <param name="fieldValue"></param>
    /// <returns></returns>
    public T GetTableEntityByfield<T>(string TableName, string fieldName, string fieldValue) where T : class, ITableEntity, new()
    {
        TableClient tableClient = CreateTableClient(TableName);
        Pageable<T> queryResultsFilter = tableClient.Query<T>(filter: $"{fieldName} eq '{fieldValue}'");
        return queryResultsFilter.FirstOrDefault();
    }

    /// <summary>
    /// Get table entity list by field
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="TableName"></param>
    /// <param name="fieldName"></param>
    /// <param name="fieldValue"></param>
    /// <returns></returns>
    public List<T> GetTableEntityListByfield<T>(string TableName, string fieldName, string fieldValue) where T : class, ITableEntity, new()
    {
        TableClient tableClient = CreateTableClient(TableName);
        Pageable<T> queryResultsFilter = tableClient.Query<T>(filter: $"{fieldName} eq '{fieldValue}'");
        return queryResultsFilter?.ToList();
    }

    /// <summary>
    /// Get table entity by partitionKey and RowKey
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="TableName"></param>
    /// <param name="PartitionKey"></param>
    /// <param name="RowKey"></param>
    /// <returns></returns>
    public T GetTableEntityByPartitionKeyAndRowKey<T>(string TableName, string PartitionKey, string RowKey) where T : class, ITableEntity, new()
    {
        TableClient tableClient = CreateTableClient(TableName);
        return tableClient.GetEntity<T>(PartitionKey, RowKey).Value;
    }

    /// <summary>
    /// Get table entity list by partitionKey and RowKey
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="TableName"></param>
    /// <param name="PartitionKey"></param>
    /// <param name="RowKey"></param>
    /// <returns></returns>
    public List<T> GetTableEntityListByPartitionKeyAndRowKey<T>(string TableName, string PartitionKey, string RowKey) where T : class, ITableEntity, new()
    {
        TableClient tableClient = CreateTableClient(TableName);
        return tableClient.Query<T>(filter: $"PartitionKey eq '{PartitionKey}' and RowKey eq '{RowKey}'")?.ToList();
    }

    /// <summary>
    /// Insert or Replace table entity
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="TableName"></param>
    /// <param name="entity"></param>
    /// <param name="caseConstraint"></param>
    /// <returns></returns>
    public async Task<bool> InsertOrReplace<T>(string TableName, T entity, bool caseConstraint = false) where T : class, ITableEntity, new()
    {
        TableClient tableClient = CreateTableClient(TableName);
        entity.Timestamp = entity.Timestamp == null ? new DateTimeOffset(DateTime.UtcNow) : entity.Timestamp;
        var response = await tableClient.UpsertEntityAsync(entity);
        return !response.IsError;
    }

    /// <summary>
    /// Insert or Replace table entities
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="TableName"></param>
    /// <param name="entities"></param>
    /// <param name="caseConstraint"></param>
    /// <returns></returns>
    public async Task InsertOrReplaceRows<T>(string TableName, List<T> entities, bool caseConstraint = false) where T : class, ITableEntity, new()
    {
        TableClient tableClient = CreateTableClient(TableName);
        var tasks = new List<Task>();
        if (entities != null)
        {
            foreach (var rowsByPartitionKeyGroup in entities.GroupBy(p => p.PartitionKey))
            {
                List<TableTransactionAction> upsertEntitiesBatch = new List<TableTransactionAction>();

                foreach (var row in rowsByPartitionKeyGroup)
                {
                    row.Timestamp = row.Timestamp == null ? new DateTimeOffset(DateTime.UtcNow) : row.Timestamp;
                    if (caseConstraint)
                    {
                        row.PartitionKey = row.PartitionKey.ToLowerInvariant();
                    }
                    upsertEntitiesBatch.Add(new TableTransactionAction(TableTransactionActionType.UpsertReplace, row));
                }

                // Submit for execution
                var task = tableClient.SubmitTransactionAsync(upsertEntitiesBatch);
                tasks.Add(task);
            }
            await Task.WhenAll(tasks);
        }
    }

    /// <summary>
    /// Replaces a single entity in the Azure Table Storage.
    /// </summary>
    /// <param name="tableName">Name of storage table</param>
    /// <param name="row">TElement row</param>
    /// <param name="caseConstraint">bool true if need to set row.PartitionKEy to LowerInvariant. default false</param>
    /// <param name="isEncryptionEnabled">bool true if Encryption to be enabled for the operation</param>
    /// <returns>TableResult</returns>
    public async Task<bool> ReplaceRow<T>(string TableName, T entity, bool caseConstraint = false) where T : class, ITableEntity, new()
    {
        TableClient tableClient = CreateTableClient(TableName);
        entity.Timestamp = entity.Timestamp == null ? new DateTimeOffset(DateTime.UtcNow) : entity.Timestamp;
        if (caseConstraint)
            entity.PartitionKey = entity.PartitionKey.ToLowerInvariant();
        var response = await tableClient.UpdateEntityAsync(entity, ETag.All);
        return !response.IsError;
    }

    /// <summary>
    /// Insert a single entity in the Azure Table Storage
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="TableName"></param>
    /// <param name="entity"></param>
    /// <returns></returns>
    public async Task<bool> Insert<T>(string TableName, T entity) where T : class, ITableEntity, new()
    {
        TableClient tableClient = CreateTableClient(TableName);
        entity.Timestamp = entity.Timestamp == null ? new DateTimeOffset(DateTime.UtcNow) : entity.Timestamp;
        var response = await tableClient.AddEntityAsync(entity);
        return !response.IsError;
    }

    /// <summary>
    /// Get table entities by partitionKey and field
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="TableName"></param>
    /// <param name="PartitionKey"></param>
    /// <param name="fieldName"></param>
    /// <param name="fieldValue"></param>
    /// <returns></returns>
    public List<T> GetTableEntityByPartitionKeyAndField<T>(string TableName, string PartitionKey, string fieldName, string fieldValue) where T : class, ITableEntity, new()
    {
        TableClient tableClient = CreateTableClient(TableName);
        Pageable<T> queryResultsFilter = tableClient.Query<T>(filter: $"PartitionKey eq '{PartitionKey}' and {fieldName} eq '{fieldValue}'");
        return queryResultsFilter?.ToList();
    }

    /// <summary>
    /// Get collection of entities by table query
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="TableName"></param>
    /// <param name="query"></param>
    /// <returns></returns>
    public List<T> GetDataCollectionByTableQuery<T>(string TableName, string query) where T : class, ITableEntity, new()
    {
        TableClient tableClient = CreateTableClient(TableName);
        return tableClient.Query<T>(filter: query)?.ToList();
    }

    /// <summary>
    /// Get collection of entities by table query segmented
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="TableName"></param>
    /// <param name="query"></param>
    /// <returns></returns>
    public List<T> GetDataCollectionByTableQuerySegmented<T>(string TableName, string query) where T : class, ITableEntity, new()
    {
        List<T> result = new List<T>();
        TableClient tableClient = CreateTableClient(TableName);
        string continuationToken = null;
        do
        {
            var responseList = tableClient.Query<T>(filter: query);
            foreach (var response in responseList.AsPages())
            {
                continuationToken = response.ContinuationToken;
                result.AddRange(response.Values);
            }

        } while (continuationToken != null);
        return result;
    }

    /// <summary>
    /// Get collection of entities by columns
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="TableName"></param>
    /// <param name="columnOne"></param>
    /// <param name="columnOneQComparison"></param>
    /// <param name="tableOperator"></param>
    /// <param name="columnTwo"></param>
    /// <param name="columnTwoQComparison"></param>
    /// <returns></returns>
    public List<T> GetDataCollectionByColumns<T>(string TableName, KeyValuePair<string, string> columnOne, string columnOneQComparison, string tableOperator, KeyValuePair<string, string> columnTwo, string columnTwoQComparison) where T : class, ITableEntity, new()
    {
        TableClient tableClient = CreateTableClient(TableName);
        var query = tableClient.Query<T>(filter: $"{columnOne.Key.ToString(CultureInfo.InvariantCulture)} {columnOneQComparison} '{columnOne.Value.ToString(CultureInfo.InvariantCulture)}' {tableOperator} {columnTwo.Key.ToString(CultureInfo.InvariantCulture)} {columnTwoQComparison} '{columnTwo.Value.ToString(CultureInfo.InvariantCulture)}'");
        return query.ToList();
    }

    /// <summary>
    /// Delete azure table storage entity
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="TableName"></param>
    /// <param name="Entity"></param>
    /// <returns></returns>
    public async Task<bool> DeleteRow<T>(string TableName, T Entity) where T : class, ITableEntity, new()
    {
        TableClient tableClient = CreateTableClient(TableName);
        Entity.ETag = ETag.All;
        var response = await tableClient.DeleteEntityAsync(Entity.PartitionKey, Entity.RowKey, Entity.ETag);
        return !response.IsError;
    }

    /// <summary>
    /// Delete azure table storage entities
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="tableName"></param>
    /// <param name="entities"></param>
    /// <returns></returns>
    public async Task DeleteRowsAsync<T>(string tableName, List<T> entities) where T : class, ITableEntity, new()
    {
        TableClient tableClient = CreateTableClient(tableName);
        var tasks = new List<Task>();
        if (entities != null)
        {

            foreach (var rowsByPartitionKeyGroup in entities.GroupBy(p => p.PartitionKey))
            {
                List<TableTransactionAction> deleteEntitiesBatch = new List<TableTransactionAction>();

                foreach (var row in rowsByPartitionKeyGroup)
                {
                    deleteEntitiesBatch.Add(new TableTransactionAction(TableTransactionActionType.Delete, row));
                }

                // Submit for execution
                var task = tableClient.SubmitTransactionAsync(deleteEntitiesBatch);
                tasks.Add(task);
            }
            await Task.WhenAll(tasks);
        }
    }
    
    /// <summary>
    /// Merge entity 
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="TableName">Name of Storage table</param>
    /// <param name="Entity">TElement Row</param>
    /// <returns></returns>
    public async Task<bool> Merge<T>(string TableName, T Entity) where T :class, ITableEntity, new()
    {
        TableClient tableClient = CreateTableClient(TableName);
        Entity.ETag = ETag.All;
        var response = await tableClient.UpdateEntityAsync(Entity, ETag.All, TableUpdateMode.Merge);
        return !response.IsError;

}
}