// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace Microsoft.CFS.Approvals.Data.Azure.Storage.Interface
{
    using System.Collections.Generic;
    using System.Threading.Tasks;
    using Microsoft.Azure.Cosmos.Table;

    public interface ITableHelper
    {
        IEnumerable<T> GetTableEntity<T>(string TableName) where T : ITableEntity, new();

        T GetTableEntityByRowKey<T>(string TableName, string rowKey) where T : ITableEntity, new();

        T GetTableEntityByPartitionKey<T>(string TableName, string PartitionKey) where T : ITableEntity, new();

        List<T> GetTableEntityListByRowKey<T>(string TableName, string RowKey) where T : ITableEntity, new();

        List<T> GetTableEntityListByPartitionKey<T>(string TableName, string PartitionKey) where T : ITableEntity, new();

        Task<bool> InsertOrReplace<T>(string TableName, T entity, bool caseConstraint = false) where T : ITableEntity, new();

        Task<bool> Insert<T>(string TableName, T entity) where T : ITableEntity, new();

        Task<bool> ReplaceRow<T>(string TableName, T entity, bool caseConstraint = false) where T : ITableEntity, new();

        T GetTableEntityByfield<T>(string TableName, string fieldName, string fieldValue) where T : ITableEntity, new();

        List<T> GetTableEntityListByfield<T>(string TableName, string fieldName, string fieldValue) where T : ITableEntity, new();

        T GetTableEntityByPartitionKeyAndRowKey<T>(string TableName, string PartitionKey, string RowKey) where T : ITableEntity, new();

        List<T> GetTableEntityListByPartitionKeyAndRowKey<T>(string TableName, string PartitionKey, string RowKey) where T : ITableEntity, new();

        List<T> GetTableEntityByPartitionKeyAndField<T>(string TableName, string PartitionKey, string fieldName, string fieldValue) where T : ITableEntity, new();

        List<T> GetDataCollectionByTableQuery<T>(string TableName, TableQuery<T> query) where T : ITableEntity, new();

        Task InsertOrReplaceRows<T>(string TableName, List<T> entities, bool caseConstraint = false) where T : ITableEntity, new();

        List<T> GetDataCollectionByColumns<T>(string TableName, KeyValuePair<string, string> columnOne, string columnOneQComparison, string tableOperator, KeyValuePair<string, string> columnTwo, string columnTwoQComparison) where T : ITableEntity, new();

        Task<bool> DeleteRow<T>(string TableName, T Entity) where T : ITableEntity, new();

        Task DeleteRowsAsync<T>(string TableName, List<T> entities) where T : ITableEntity, new();
    }
}