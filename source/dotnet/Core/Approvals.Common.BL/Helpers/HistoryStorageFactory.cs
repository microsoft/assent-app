// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace Microsoft.CFS.Approvals.Common.DL;

using Microsoft.CFS.Approvals.Common.DL.Interface;
using Microsoft.CFS.Approvals.Data.Azure.CosmosDb.Interface;
using Microsoft.CFS.Approvals.Data.Azure.Storage.Interface;
using Microsoft.CFS.Approvals.Model;
using Microsoft.Extensions.Configuration;

public class HistoryStorageFactory : IHistoryStorageFactory
{
    /// <summary>
    /// The table helper
    /// </summary>
    private readonly ITableHelper _tableHelper;

    /// <summary>
    /// The CosmosDb helper
    /// </summary>
    private readonly ICosmosDbHelper _cosmosDbHelper;

    /// <summary>
    /// Configuration Helper
    /// </summary>
    private readonly IConfiguration _config;

    /// <summary>
    /// The Approval blob data provider
    /// </summary>
    private readonly IBlobStorageHelper _blobStorageHelper;

    /// <summary>
    /// Constructor of HistoryStorageFactory
    /// </summary>
    /// <param name="tableHelper"></param>
    /// <param name="cosmosDbHelper"></param>
    /// <param name="config"></param>
    /// <param name="_approvalBlobDataProvider"></param>
    public HistoryStorageFactory(ITableHelper tableHelper, ICosmosDbHelper cosmosDbHelper, IConfiguration config, IBlobStorageHelper blobStorageHelper)
    {
        _tableHelper = tableHelper;
        _cosmosDbHelper = cosmosDbHelper;
        _config = config;
        _blobStorageHelper = blobStorageHelper;
    }

    /// <summary>
    /// Get storage provider
    /// </summary>
    /// <param name="tenantInfo"></param>
    /// <returns></returns>
    public IHistoryStorageProvider GetStorageProvider(ApprovalTenantInfo tenantInfo)
    {
        IHistoryStorageProvider historyStorageProvider;
        if (tenantInfo != null && !tenantInfo.HistoryLogging)
        {
            historyStorageProvider = new HistoryTableStorageProvider(_tableHelper, _config, _blobStorageHelper);
        }
        else
        {
            historyStorageProvider = new HistoryDocumentDbProvider(_cosmosDbHelper, _config);
        }

        return historyStorageProvider;
    }

    /// <summary>
    /// Get table storage provider
    /// </summary>
    /// <returns></returns>
    public IHistoryStorageProvider GetTableStorageProvider()
    {
        IHistoryStorageProvider historyStorageProvider;
        historyStorageProvider = new HistoryTableStorageProvider(_tableHelper, _config, _blobStorageHelper);
        return historyStorageProvider;
    }
}