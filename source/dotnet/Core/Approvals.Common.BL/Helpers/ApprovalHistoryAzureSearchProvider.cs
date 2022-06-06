// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace Microsoft.CFS.Approvals.Common.DL
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;
    using Extensions;
    using Microsoft.Azure.Search;
    using Microsoft.Azure.Search.Models;
    using Microsoft.CFS.Approvals.Common.DL.Interface;
    using Microsoft.CFS.Approvals.Contracts;
    using Microsoft.CFS.Approvals.Data.Azure.CosmosDb.Interface;
    using Microsoft.CFS.Approvals.Data.Azure.Storage.Interface;
    using Microsoft.CFS.Approvals.LogManager.Model;
    using Microsoft.CFS.Approvals.LogManager.Provider.Interface;
    using Microsoft.Extensions.Configuration;
    using Model;
    using Newtonsoft.Json.Linq;

    public class ApprovalHistoryAzureSearchProvider : ApprovalHistoryProvider
    {
        private readonly ISearchIndexClient _indexClientForQueries;

        public ApprovalHistoryAzureSearchProvider(
            IConfiguration config,
            ITableHelper tableHelper,
            ILogProvider logger,
            IPerformanceLogger performanceLogger,
            IApprovalTenantInfoProvider approvalTenantInfoProvider,
            IHistoryStorageFactory historyStorageFactory,
            ICosmosDbHelper cosmosDbHelper)
            : base(
                  config,
                  approvalTenantInfoProvider,
                  logger,
                  performanceLogger,
                  cosmosDbHelper,
                  historyStorageFactory
                  )
        {
            _indexClientForQueries = CreateSearchServiceClient();
        }

        private SearchIndexClient CreateSearchServiceClient()
        {
            string searchServiceName = _config[ConfigurationKey.AzureSearchServiceName.ToString()];
            string queryApiKey = _config[ConfigurationKey.AzureSearchServiceQueryApiKey.ToString()];
            string indexName = _config[ConfigurationKey.AzureSearchTransactionHistoryIndexName.ToString()];

            SearchIndexClient indexClient = new SearchIndexClient(searchServiceName, indexName, new SearchCredentials(queryApiKey));
            return indexClient;
        }

        /// <summary>
        /// Used to Get the History data to be shown on the History Page
        /// </summary>
        /// <param name="alias"></param>
        /// <param name="timePeriod"></param>
        /// <param name="searchCriteria"></param>
        /// <param name="page"></param>
        /// <param name="sortColumn"></param>
        /// <param name="sortDirection"></param>
        /// <returns></returns>
        public override async Task<PagedData<TransactionHistoryExtended>> GetHistoryDataAsync(string alias, int timePeriod, string searchCriteria
            , int? page = null, string sortColumn = null, string sortDirection = "DESC")
        {
            var historyPagedData = new PagedData<TransactionHistoryExtended>();
            if (timePeriod == 0)
            {
                timePeriod =
                    int.Parse(_config[ConfigurationKey.MonthsOfHistoryDataValue.ToString()]);
            }
            var pageSize = 0;
            var skipSize = 0;
            if (page != null)
            {
                pageSize = int.Parse(_config[ConfigurationKey.HistoryPageSize.ToString()]);
                skipSize = (pageSize * ((int)page != 0 ? (int)page - 1 : 0));
            }

            //searchCriteria = searchCriteria.ReplaceSqlSpecialCharacters();

            SearchParameters parameters;
            DocumentSearchResult<TransactionHistory> results;
            List<TransactionHistory> history = new List<TransactionHistory>();

            var actionDateRange = DateTime.UtcNow.AddMonths(timePeriod * -1).ToString("o");
            parameters =
                new SearchParameters()
                {
                    Filter = "(Approver eq '" + alias.ToLowerInvariant() + "' or Approver eq '" + alias.ToUpperInvariant() + "' or Approver eq '" + alias + "') and ActionDate ge " + actionDateRange,
                    OrderBy = new[] { "ActionDate desc" },
                };

            results = _indexClientForQueries.Documents.Search<TransactionHistory>("*", parameters);

            if (results == null)
                throw new Exception("Azure Search results returned null");

            foreach (SearchResult<TransactionHistory> result in results.Results)
            {
                history.Add(result.Document);
            }

            while (results.ContinuationToken != null)
            {
                results = _indexClientForQueries.Documents.ContinueSearch<TransactionHistory>(results.ContinuationToken);
                foreach (SearchResult<TransactionHistory> result in results.Results)
                {
                    history.Add(result.Document);
                }
            }

            List<TransactionHistory> dtTransactionHistory;

            if (string.IsNullOrEmpty(searchCriteria))
            {
                dtTransactionHistory = history.ToList();
            }
            else
            {
                dtTransactionHistory = history.Where(t => (t.PartitionKey.Contains(searchCriteria, StringComparison.OrdinalIgnoreCase) || t.AppName.Contains(searchCriteria, StringComparison.OrdinalIgnoreCase) ||
                                                        t.Title.Contains(searchCriteria, StringComparison.OrdinalIgnoreCase) || t.SubmitterName.Contains(searchCriteria, StringComparison.OrdinalIgnoreCase) ||
                                                        (t.CompanyCode != null && t.CompanyCode.Contains(searchCriteria, StringComparison.OrdinalIgnoreCase)) || (t.UnitValue != null && t.UnitValue.Contains(searchCriteria, StringComparison.OrdinalIgnoreCase)) ||
                                                        t.ActionTaken.Contains(searchCriteria, StringComparison.OrdinalIgnoreCase) || (t.CustomAttribute != null && t.CustomAttribute.Contains(searchCriteria, StringComparison.OrdinalIgnoreCase)) ||
                                                        t.ActionDate.Value.ToLocalTime().ToString().Contains(searchCriteria, StringComparison.OrdinalIgnoreCase)))
                                                        .ToList();
            }

            historyPagedData.TotalCount = dtTransactionHistory.Count;

            if (sortColumn != null)
                dtTransactionHistory = dtTransactionHistory.OrderByDescending(t => t.GetType().GetProperty(sortColumn).GetValue(t)).ToList();
            else
                dtTransactionHistory = dtTransactionHistory.OrderByDescending(t => t.ActionDate).ToList();

            if (page != null)
                dtTransactionHistory = dtTransactionHistory.Skip(skipSize).Take(pageSize).ToList();

            TransactionHistoryExtended extendTransactionHistoryRecord(TransactionHistory h, ApprovalTenantInfo t)
            {
                var the = (h.ToJson()).FromJson<TransactionHistoryExtended>();
                the.CondensedAppName = t.AppName.Replace(" ", "");
                the.TemplateName = t.TemplateName;
                the.IsHistoryClickable = t.IsHistoryClickable;
                the.BusinessProcessName = t.BusinessProcessName;
                return the;
            }

            var tranHistories = dtTransactionHistory.ToList();
            var tenants = _approvalTenantInfoProvider.GetAllTenantInfo();
            var resultsExtended = from record in tranHistories
                                  join tenant in tenants.Result on record.TenantId equals tenant.RowKey
                                  let recordExtended = extendTransactionHistoryRecord(record, tenant)
                                  select recordExtended;
            historyPagedData.Result = resultsExtended.ToList();
            return historyPagedData;
        }

        /// <summary>
        /// This method gets the total count of historical records for the given user
        /// </summary>
        /// <param name="alias"></param>
        /// <param name="timePeriod"></param>
        /// <param name="searchCriteria"></param>
        /// <param name="loggedInAlias"></param>
        /// <param name="Xcv"></param>
        /// <returns></returns>
        public override async Task<JArray> GetHistoryCountforAliasAsync(string alias, int timePeriod, string searchCriteria, string loggedInAlias, string Xcv)
        {
            var logData = new Dictionary<LogDataKey, object>
            {
                { LogDataKey.Xcv, Xcv },
                { LogDataKey.Tcv, Xcv },
                { LogDataKey.ReceivedTcv, Xcv },
                { LogDataKey.SessionId, Xcv },
                { LogDataKey.UserRoleName, loggedInAlias },
                { LogDataKey.EventType, Constants.FeatureUsageEvent },
                { LogDataKey.UserAlias, alias },
                { LogDataKey.SearchText, searchCriteria },
                { LogDataKey.IsCriticalEvent, CriticalityLevel.No.ToString() }
            };

            if (string.IsNullOrEmpty(searchCriteria)) { searchCriteria = string.Empty; }
            if (timePeriod == 0) { timePeriod = int.Parse(_config[ConfigurationKey.MonthsOfHistoryDataValue.ToString()]); }

            var tenantsCount = new JArray();
            try
            {
                SearchParameters parameters;
                DocumentSearchResult<TransactionHistory> results;
                List<TransactionHistory> history = new List<TransactionHistory>();
                List<TransactionHistory> dtTransactionHistory;
                var actionDateRange = DateTime.UtcNow.AddMonths(timePeriod * -1).ToString("o");
                parameters =
                    new SearchParameters()
                    {
                        Filter = "(Approver eq '" + alias.ToLowerInvariant() + "' or Approver eq '" + alias.ToUpperInvariant() + "' or Approver eq '" + alias + "') and ActionDate ge " + actionDateRange,
                        OrderBy = new[] { "ActionDate desc" },
                    };

                results = _indexClientForQueries.Documents.Search<TransactionHistory>("*", parameters);

                if (results == null)
                    throw new Exception("Azure Search results returned null");

                foreach (SearchResult<TransactionHistory> result in results.Results)
                {
                    history.Add(result.Document);
                }

                while (results.ContinuationToken != null)
                {
                    results = _indexClientForQueries.Documents.ContinueSearch<TransactionHistory>(results.ContinuationToken);
                    foreach (SearchResult<TransactionHistory> result in results.Results)
                    {
                        history.Add(result.Document);
                    }
                }

                if (string.IsNullOrEmpty(searchCriteria))
                {
                    dtTransactionHistory = history.ToList();
                }
                else
                {
                    dtTransactionHistory = history.Where(t => (t.PartitionKey.Contains(searchCriteria, StringComparison.OrdinalIgnoreCase) || t.AppName.Contains(searchCriteria, StringComparison.OrdinalIgnoreCase) ||
                                                               t.Title.Contains(searchCriteria, StringComparison.OrdinalIgnoreCase) || t.SubmitterName.Contains(searchCriteria, StringComparison.OrdinalIgnoreCase) ||
                                                               (t.CompanyCode != null && t.CompanyCode.Contains(searchCriteria, StringComparison.OrdinalIgnoreCase)) || (t.CompanyCode != null && t.CompanyCode.Contains(searchCriteria, StringComparison.OrdinalIgnoreCase)) ||
                                                               t.ActionTaken.Contains(searchCriteria, StringComparison.OrdinalIgnoreCase) || (t.CustomAttribute != null && t.CustomAttribute.Contains(searchCriteria, StringComparison.OrdinalIgnoreCase)) ||
                                                               (t.UnitValue != null && t.UnitValue.Contains(searchCriteria, StringComparison.OrdinalIgnoreCase)) ||
                                                               t.ActionDate.Value.ToLocalTime().ToString().Contains(searchCriteria, StringComparison.OrdinalIgnoreCase)))
                                                               .ToList();
                }
                tenantsCount.Add(dtTransactionHistory.Count);
            }
            catch (Exception ex)
            {
                _logProvider.LogError(TrackingEvent.HistoryDataCountFail, ex, logData);
            }
            return tenantsCount;
        }
    }
}