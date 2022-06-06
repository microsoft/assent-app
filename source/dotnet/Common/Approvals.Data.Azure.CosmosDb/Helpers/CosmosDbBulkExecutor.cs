// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace Microsoft.CFS.Approvals.Data.Azure.CosmosDb.Helpers
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;
    using Data.Azure.CosmosDb.Interface;
    using Microsoft.Azure.CosmosDB.BulkExecutor;
    using Microsoft.Azure.Documents;
    using Microsoft.Azure.Documents.Client;
    using Newtonsoft.Json.Linq;

    /// <summary>
    /// class CosmosDbBulkExecutor
    /// </summary>
    public class CosmosDbBulkExecutor : ICosmosDbBulkExecutor
    {
        #region Varibales

        /// <summary>
        /// The cosmosDbBulkExecutor
        /// </summary>
        private static ICosmosDbBulkExecutor _cosmosDbBulkExecutor;

        /// <summary>
        /// The bulkExecutor
        /// </summary>
        private static IBulkExecutor _bulkExecutor;

        /// <summary>
        /// The bulkExecutor dictionary
        /// </summary>
        private static readonly Dictionary<string, IBulkExecutor> _bulkExecutors = new Dictionary<string, IBulkExecutor>();

        /// <summary>
        /// The lockObject to handle single instance of bulkExecutor
        /// </summary>
        private static readonly SemaphoreSlim _semaphoreSlim = new SemaphoreSlim(1);

        /// <summary>
        /// The documentClient
        /// </summary>
        private static string _documentEndpoint;

        /// <summary>
        /// The documentCollection
        /// </summary>
        private static readonly List<DocumentCollection> _documentCollections = new List<DocumentCollection>();

        #endregion Varibales

        #region Constrctor

        /// <summary>
        /// Private constructor of the <see cref="CosmosDbBulkExecutor"/> to make it singleton.
        /// </summary>
        private CosmosDbBulkExecutor()
        {
        }

        #endregion Constrctor

        #region Public Methods

        /// <summary>
        /// Create single instance of CosmosDbBulkExecutor
        /// </summary>
        /// <param name="endpoint">The endpoint.</param>
        /// <param name="authKey">The authKey.</param>
        /// <param name="collection">The DocumentCollection</param>
        /// <returns>Returns instance of <see cref="CosmosDbBulkExecutor"/></returns>
        public static async Task<ICosmosDbBulkExecutor> Instance(string endpoint, string authKey, DocumentCollection collection)
        {
            await _semaphoreSlim.WaitAsync();
            try
            {
                var documentDbCollection = _documentCollections.FirstOrDefault(d => d.Id == collection.Id);
                if (_cosmosDbBulkExecutor == null || _documentEndpoint != endpoint || documentDbCollection == null || !_bulkExecutors.ContainsKey(collection.Id))
                {
                    _cosmosDbBulkExecutor = new CosmosDbBulkExecutor();
                    var client = new DocumentClient(new Uri(endpoint), authKey);

                    // Set retry options high during initialization (default values).
                    client.ConnectionPolicy.RetryOptions.MaxRetryWaitTimeInSeconds = 30;
                    client.ConnectionPolicy.RetryOptions.MaxRetryAttemptsOnThrottledRequests = 9;
                    client.ConnectionPolicy.ConnectionMode = ConnectionMode.Direct;
                    client.ConnectionPolicy.ConnectionProtocol = Protocol.Tcp;

                    // Initializing BulkExecutor
                    _bulkExecutor = new BulkExecutor(client, collection);
                    await _bulkExecutor.InitializeAsync();

                    // Set retries to 0 to pass complete control to bulk executor.
                    client.ConnectionPolicy.RetryOptions.MaxRetryWaitTimeInSeconds = 0;
                    client.ConnectionPolicy.RetryOptions.MaxRetryAttemptsOnThrottledRequests = 0;

                    _documentCollections.Add(collection);
                    _bulkExecutors.Add(collection.Id, _bulkExecutor);
                    _documentEndpoint = endpoint;
                }
                else
                {
                    _bulkExecutor = _bulkExecutors[collection.Id];
                }

                return _cosmosDbBulkExecutor;
            }
            finally
            {
                _semaphoreSlim.Release();
            }
        }

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
        public async Task<JObject> BulkImportAsync<T>(
            List<T> documents,
            bool enableUpsert = true,
            bool disableAutomaticIdGeneration = false,
            int? maxConcurrencyPerPartitionKeyRange = null,
            int? maxInMemorySortingBatchSize = null) where T : class
        {
            int retryCount = 0;
            do
            {
                try
                {
                    var response = await _bulkExecutor.BulkImportAsync(
                        documents,
                        enableUpsert,
                        disableAutomaticIdGeneration,
                        maxConcurrencyPerPartitionKeyRange,
                        maxInMemorySortingBatchSize);

                    return JObject.FromObject(response);
                }
                catch
                {
                    retryCount++;
                    Thread.Sleep(1000);
                    await _bulkExecutor.InitializeAsync();
                }
            } while (retryCount < 2);

            return null;
        }

        #endregion Public Methods
    }
}