// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace Microsoft.CFS.Approvals.Data.Azure.Storage.Helpers
{
    using System.Collections.Generic;
    using System.Globalization;
    using System.Linq;
    using System.Threading.Tasks;
    using Microsoft.Azure.Cosmos.Table;
    using Microsoft.CFS.Approvals.Data.Azure.Storage.Interface;

    /// <summary>
    /// The TableHelper class
    /// </summary>
    public class TableHelper : ITableHelper
    {
        private readonly string _azureStorageAccountName;
        private readonly string _azureStorageAccountKey;
        private CloudTableClient _cloudTableClient;

        /// <summary>
        /// Constructor of TableHelper
        /// </summary>
        /// <param name="azureStorageAccountName"></param>
        /// <param name="azureStorageAccountKey"></param>
        public TableHelper(string azureStorageAccountName, string azureStorageAccountKey)
        {
            _azureStorageAccountName = azureStorageAccountName;
            _azureStorageAccountKey = azureStorageAccountKey;
        }

        private CloudTableClient CloudTableClient
        {
            get
            {
                if (_cloudTableClient == null)
                {
                    _cloudTableClient = CreateTableClient();
                }
                return _cloudTableClient;
            }
        }

        /// <summary>
        /// Get the table reference
        /// </summary>
        /// <returns></returns>
        private CloudTableClient CreateTableClient()
        {
            StorageCredentials storageCredentials = new StorageCredentials(_azureStorageAccountName, _azureStorageAccountKey);
            CloudStorageAccount storageAccount = new CloudStorageAccount(storageCredentials, true);
            CloudTableClient cloudTableClient = storageAccount.CreateCloudTableClient();
            return cloudTableClient;
        }

        /// <summary>
        /// Get Table entity
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="TableName"></param>
        /// <returns></returns>
        public IEnumerable<T> GetTableEntity<T>(string TableName) where T : ITableEntity, new()
        {
            CloudTable table = CloudTableClient.GetTableReference(TableName);
            TableQuery<T> query = new TableQuery<T>();
            var result = table.ExecuteQuery(query);
            return result;
        }

        /// <summary>
        /// Get Table Entity by RowKey
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="TableName"></param>
        /// <param name="rowKey"></param>
        /// <returns></returns>
        public T GetTableEntityByRowKey<T>(string TableName, string rowKey) where T : ITableEntity, new()
        {
            CloudTable table = CloudTableClient.GetTableReference(TableName);
            TableQuery<T> query = new TableQuery<T>().Where(TableQuery.GenerateFilterCondition("RowKey",
                                                                  QueryComparisons.Equal,
                                                                  rowKey));
            return table.ExecuteQuery(query).FirstOrDefault();
        }

        /// <summary>
        /// Get table entity by PartitionKey
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="TableName"></param>
        /// <param name="PartitionKey"></param>
        /// <returns></returns>
        public T GetTableEntityByPartitionKey<T>(string TableName, string PartitionKey) where T : ITableEntity, new()
        {
            CloudTable table = CloudTableClient.GetTableReference(TableName);
            TableQuery<T> query = new TableQuery<T>().Where(TableQuery.GenerateFilterCondition("PartitionKey",
                                                                  QueryComparisons.Equal,
                                                                  PartitionKey));
            return table.ExecuteQuery(query).FirstOrDefault();
        }

        /// <summary>
        /// Get table entity list by RowKey
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="TableName"></param>
        /// <param name="RowKey"></param>
        /// <returns></returns>
        public List<T> GetTableEntityListByRowKey<T>(string TableName, string RowKey) where T : ITableEntity, new()
        {
            CloudTable table = CloudTableClient.GetTableReference(TableName);
            TableQuery<T> query = new TableQuery<T>().Where(TableQuery.GenerateFilterCondition("RowKey",
                                                                  QueryComparisons.Equal,
                                                                  RowKey));
            return table.ExecuteQuery(query).ToList();
        }

        /// <summary>
        /// Get table entity list by partitionKey
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="TableName"></param>
        /// <param name="PartitionKey"></param>
        /// <returns></returns>
        public List<T> GetTableEntityListByPartitionKey<T>(string TableName, string PartitionKey) where T : ITableEntity, new()
        {
            CloudTable table = CloudTableClient.GetTableReference(TableName);
            TableQuery<T> query = new TableQuery<T>().Where(TableQuery.GenerateFilterCondition("PartitionKey",
                                                                  QueryComparisons.Equal,
                                                                  PartitionKey));
            return table.ExecuteQuery(query).ToList();
        }

        /// <summary>
        /// Get table entity by field
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="TableName"></param>
        /// <param name="fieldName"></param>
        /// <param name="fieldValue"></param>
        /// <returns></returns>
        public T GetTableEntityByfield<T>(string TableName, string fieldName, string fieldValue) where T : ITableEntity, new()
        {
            CloudTable table = CloudTableClient.GetTableReference(TableName);
            TableQuery<T> query = new TableQuery<T>().Where(TableQuery.GenerateFilterCondition(fieldName,
                                                                  QueryComparisons.Equal,
                                                                  fieldValue.ToLower()));
            var result = table.ExecuteQuery(query).FirstOrDefault();
            return result;
        }

        /// <summary>
        /// Get table entity list by field
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="TableName"></param>
        /// <param name="fieldName"></param>
        /// <param name="fieldValue"></param>
        /// <returns></returns>
        public List<T> GetTableEntityListByfield<T>(string TableName, string fieldName, string fieldValue) where T : ITableEntity, new()
        {
            CloudTable table = CloudTableClient.GetTableReference(TableName);
            TableQuery<T> query = new TableQuery<T>().Where(TableQuery.GenerateFilterCondition(fieldName,
                                                                  QueryComparisons.Equal,
                                                                  fieldValue.ToLower()));
            var result = table.ExecuteQuery(query);
            return result?.ToList();
        }

        /// <summary>
        /// Get table entity by partitionKey and RowKey
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="TableName"></param>
        /// <param name="PartitionKey"></param>
        /// <param name="RowKey"></param>
        /// <returns></returns>
        public T GetTableEntityByPartitionKeyAndRowKey<T>(string TableName, string PartitionKey, string RowKey) where T : ITableEntity, new()
        {
            CloudTable table = CloudTableClient.GetTableReference(TableName);
            string pkFilter = TableQuery.GenerateFilterCondition("PartitionKey", QueryComparisons.Equal, PartitionKey);
            string rkFilter = TableQuery.GenerateFilterCondition("RowKey", QueryComparisons.Equal, RowKey);
            TableQuery<T> query = new TableQuery<T>().Where(TableQuery.CombineFilters(pkFilter, TableOperators.And, rkFilter));
            var result = table.ExecuteQuery(query).FirstOrDefault();
            return result;
        }

        /// <summary>
        /// Get table entity list by partitionKey and RowKey
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="TableName"></param>
        /// <param name="PartitionKey"></param>
        /// <param name="RowKey"></param>
        /// <returns></returns>
        public List<T> GetTableEntityListByPartitionKeyAndRowKey<T>(string TableName, string PartitionKey, string RowKey) where T : ITableEntity, new()
        {
            CloudTable table = CloudTableClient.GetTableReference(TableName);
            string pkFilter = TableQuery.GenerateFilterCondition("PartitionKey", QueryComparisons.Equal, PartitionKey);
            string rkFilter = TableQuery.GenerateFilterCondition("RowKey", QueryComparisons.Equal, RowKey);
            TableQuery<T> query = new TableQuery<T>().Where(TableQuery.CombineFilters(pkFilter, TableOperators.And, rkFilter));
            var result = table.ExecuteQuery(query).ToList();
            return result;
        }

        /// <summary>
        /// Insert or Replace table entity
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="TableName"></param>
        /// <param name="entity"></param>
        /// <param name="caseConstraint"></param>
        /// <returns></returns>
        public async Task<bool> InsertOrReplace<T>(string TableName, T entity, bool caseConstraint = false) where T : ITableEntity, new()
        {
            CloudTable table = CloudTableClient.GetTableReference(TableName);
            TableOperation tableOperation = TableOperation.InsertOrReplace(entity);
            await table.ExecuteAsync(tableOperation);
            return true;
        }

        /// <summary>
        /// Insert or Replace table entities
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="TableName"></param>
        /// <param name="entities"></param>
        /// <param name="caseConstraint"></param>
        /// <returns></returns>
        public async Task InsertOrReplaceRows<T>(string TableName, List<T> entities, bool caseConstraint = false) where T : ITableEntity, new()
        {
            CloudTable table = CloudTableClient.GetTableReference(TableName);
            var tasks = new List<Task>();
            if (entities != null)
            {
                foreach (var rowsByPartitionKeyGroup in entities.GroupBy(p => p.PartitionKey))
                {
                    var task = Task.Run(async () =>
                    {
                        var batch = new TableBatchOperation();
                        foreach (var row in rowsByPartitionKeyGroup)
                        {
                            if (caseConstraint)
                            {
                                row.PartitionKey = row.PartitionKey.ToLowerInvariant();
                            }

                            batch.Add(TableOperation.InsertOrReplace(row));
                        }

                        // Submit for execution
                        await table.ExecuteBatchAsync(batch);
                    });
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
        public async Task<bool> ReplaceRow<T>(string TableName, T entity, bool caseConstraint = false) where T : ITableEntity, new()
        {
            CloudTable table = CloudTableClient.GetTableReference(TableName);
            TableOperation tableOperation = TableOperation.Replace(entity);
            if (caseConstraint)
                entity.PartitionKey = entity.PartitionKey.ToLowerInvariant();
            await table.ExecuteAsync(tableOperation);
            return true;
        }

        /// <summary>
        /// Insert a single entity in the Azure Table Storage
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="TableName"></param>
        /// <param name="entity"></param>
        /// <returns></returns>
        public async Task<bool> Insert<T>(string TableName, T entity) where T : ITableEntity, new()
        {
            CloudTable table = CloudTableClient.GetTableReference(TableName);
            TableOperation tableOperation = TableOperation.Insert(entity);
            await table.ExecuteAsync(tableOperation);
            return true;
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
        public List<T> GetTableEntityByPartitionKeyAndField<T>(string TableName, string PartitionKey, string fieldName, string fieldValue) where T : ITableEntity, new()
        {
            CloudTable table = CloudTableClient.GetTableReference(TableName);
            string pkFilter = TableQuery.GenerateFilterCondition("PartitionKey", QueryComparisons.Equal, PartitionKey);
            string Filter = TableQuery.GenerateFilterCondition(fieldName, QueryComparisons.Equal, fieldValue);
            TableQuery<T> query = new TableQuery<T>().Where(TableQuery.CombineFilters(pkFilter, TableOperators.And, Filter));
            var result = table.ExecuteQuery(query)?.ToList();
            return result;
        }

        /// <summary>
        /// Get collection of entities by table query
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="TableName"></param>
        /// <param name="query"></param>
        /// <returns></returns>
        public List<T> GetDataCollectionByTableQuery<T>(string TableName, TableQuery<T> query) where T : ITableEntity, new()
        {
            List<T> result = new List<T>();
            CloudTable table = CloudTableClient.GetTableReference(TableName);
            result = (List<T>)table.ExecuteQuery(query).ToList();
            return result;
        }

        /// <summary>
        /// Get collection of entities by table query segmented
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="TableName"></param>
        /// <param name="query"></param>
        /// <returns></returns>
        public List<T> GetDataCollectionByTableQuerySegmented<T>(string TableName, TableQuery<T> query) where T : ITableEntity, new()
        {
            List<T> result = new List<T>();
            CloudTable table = CloudTableClient.GetTableReference(TableName);
            TableContinuationToken continuationToken = null;
            do
            {
                var response = table.ExecuteQuerySegmented(query, continuationToken);
                continuationToken = response.ContinuationToken;
                result.AddRange(response);
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
        public List<T> GetDataCollectionByColumns<T>(string TableName, KeyValuePair<string, string> columnOne, string columnOneQComparison, string tableOperator, KeyValuePair<string, string> columnTwo, string columnTwoQComparison) where T : ITableEntity, new()
        {
            CloudTable table = CloudTableClient.GetTableReference(TableName);
            TableQuery<T> query = (new TableQuery<T>()
           .Where(
                   TableQuery.CombineFilters(
                                               TableQuery.GenerateFilterCondition(columnOne.Key.ToString(CultureInfo.InvariantCulture), columnOneQComparison, columnOne.Value.ToString(CultureInfo.InvariantCulture)),
                                               tableOperator,
                                               TableQuery.GenerateFilterCondition(columnTwo.Key.ToString(CultureInfo.InvariantCulture), columnTwoQComparison, columnTwo.Value.ToString(CultureInfo.InvariantCulture))
                                            )
                   ));

            List<T> data = table.ExecuteQuery<T>(query).ToList();
            return data;
        }

        /// <summary>
        /// Delete azure table storage entity
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="TableName"></param>
        /// <param name="Entity"></param>
        /// <returns></returns>
        public async Task<bool> DeleteRow<T>(string TableName, T Entity) where T : ITableEntity, new()
        {
            CloudTable table = CloudTableClient.GetTableReference(TableName);
            Entity.ETag = "*";
            var operation = TableOperation.Delete(Entity);
            await table.ExecuteAsync(operation);
            return true;
        }

        /// <summary>
        /// Delete azure table storage entities
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="TableName"></param>
        /// <param name="entities"></param>
        /// <returns></returns>
        public async Task DeleteRowsAsync<T>(string TableName, List<T> entities) where T : ITableEntity, new()
        {
            CloudTable table = CloudTableClient.GetTableReference(TableName);
            var tasks = new List<Task>();
            if (entities != null)
            {
                foreach (var rowsByPartitionKeyGroup in entities.GroupBy(p => p.PartitionKey))
                {
                    var task = Task.Run(async () =>
                    {
                        var batch = new TableBatchOperation();
                        foreach (var row in rowsByPartitionKeyGroup)
                        {
                            batch.Add(TableOperation.Delete(row));
                        }

                        // Submit for execution
                        await table.ExecuteBatchAsync(batch);
                    });
                    tasks.Add(task);
                }
                await Task.WhenAll(tasks);
            }
        }

        /// <summary>
        /// Deletes entities in the Table
        /// </summary>
        /// <param name="tableName">Name of storage table</param>
        /// <param name="rows">List of TElement rows</param>
        /// <param name="caseConstraint">bool true if need to set row.PartitionKey to LowerInvariant. default false</param>
        public async Task DeleteRows<TElement>(string tableName, List<TElement> rows, bool caseConstraint = false) where TElement : ITableEntity
        {
            // TODO:: modified code to perform batch operation instead of single operation inside iteration
            CloudTable table = CloudTableClient.GetTableReference(tableName);
            foreach (TElement row in rows)
            {
                if (caseConstraint)
                {
                    row.PartitionKey = row.PartitionKey.ToLowerInvariant();
                }
                // forcefully remove the entity from the table
                row.ETag = "*";
                TableOperation op = TableOperation.Delete(row);
                await table.ExecuteAsync(op);
            }
        }
    }
}