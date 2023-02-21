namespace Microsoft.CFS.Approvals.Model;

using System;
using global::Azure.Data.Tables;
using Newtonsoft.Json;

public class BaseTableEntity : ITableEntity
{
    public BaseTableEntity()
    { }

    public BaseTableEntity(string partitionKey, string rowKey)
    {
        PartitionKey = partitionKey;
        RowKey = rowKey;
    }

    public string PartitionKey { get; set; }
    public string RowKey { get; set; }
    public DateTimeOffset? Timestamp { get; set; }

    [JsonIgnore]
    public global::Azure.ETag ETag { get; set; }
}