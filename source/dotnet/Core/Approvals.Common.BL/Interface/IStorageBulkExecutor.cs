// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace Microsoft.CFS.Approvals.Common.DL.Interface;

using System.Collections.Generic;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;
public interface IStorageBulkExecutor
{
    /// <summary>
    /// This method will import documents in bulk.
    /// </summary>
    /// <typeparam name="T">Type of document</typeparam>
    /// <param name="documents">List of documents to be imported</param>
    /// <param name="enableUpsert"></param>
    /// <param name="disableAutomaticIdGeneration"></param>
    /// <param name="maxConcurrencyPerPartitionKeyRange">Maximum concurrency per partition key range</param>
    /// <param name="maxInMemorySortingBatchSize">Maximum InMemory Sorting batch size</param>
    /// <returns>Returns response of BulkImport</returns>
    Task<JObject> BulkImportAsync<T>(
        List<T> documents,
        bool enableUpsert = true,
        bool disableAutomaticIdGeneration = false,
        int? maxConcurrencyPerPartitionKeyRange = null,
        int? maxInMemorySortingBatchSize = null) where T : class;
}
