// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace Microsoft.CFS.Approvals.Common.DL;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.CFS.Approvals.Common.DL.Interface;
using Microsoft.CFS.Approvals.Contracts;
using Microsoft.CFS.Approvals.Contracts.DataContracts;
using Microsoft.CFS.Approvals.Data.Azure.CosmosDb.Interface;
using Microsoft.CFS.Approvals.Extensions;
using Microsoft.CFS.Approvals.LogManager.Model;
using Microsoft.CFS.Approvals.LogManager.Provider.Interface;
using Microsoft.CFS.Approvals.Model;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json.Linq;

/// <summary>
/// The Approval History Provider class
/// </summary>
public class ApprovalHistoryProvider : IApprovalHistoryProvider
{
    /// <summary>
    /// The configuration
    /// </summary>
    protected readonly IConfiguration _config;

    /// <summary>
    /// The approval tenantInfo
    /// </summary>
    protected readonly IApprovalTenantInfoProvider _approvalTenantInfoProvider;

    /// <summary>
    /// The log provider
    /// </summary>
    protected readonly ILogProvider _logProvider;

    /// <summary>
    /// The performance logger
    /// </summary>
    protected readonly IPerformanceLogger _performanceLogger;

    /// <summary>
    /// The Cosmos Db helper
    /// </summary>
    protected readonly ICosmosDbHelper _cosmosDbHelper;

    /// <summary>
    /// The history storage factory
    /// </summary>
    protected readonly IHistoryStorageFactory _historyStorageFactory;

    /// <summary>
    /// Constructor of ApprovalHistoryProvider
    /// </summary>
    /// <param name="config"></param>
    /// <param name="approvalTenantInfoProvider"></param>
    /// <param name="logProvider"></param>
    /// <param name="performanceLogger"></param>
    /// <param name="cosmosDbHelper"></param>
    /// <param name="historyStorageFactory"></param>
    public ApprovalHistoryProvider(
        IConfiguration config,
        IApprovalTenantInfoProvider approvalTenantInfoProvider,
        ILogProvider logProvider,
        IPerformanceLogger performanceLogger,
        ICosmosDbHelper cosmosDbHelper,
        IHistoryStorageFactory historyStorageFactory)
    {
        _config = config;
        _approvalTenantInfoProvider = approvalTenantInfoProvider;
        _logProvider = logProvider;
        _performanceLogger = performanceLogger;
        _cosmosDbHelper = cosmosDbHelper;
        _historyStorageFactory = historyStorageFactory;
    }

    /// <summary>
    /// Save approval transaction history
    /// </summary>
    /// <param name="tenantInfo"></param>
    /// <param name="historyData"></param>
    /// <returns></returns>
    public async Task AddApprovalHistoryAsync(ApprovalTenantInfo tenantInfo, TransactionHistory historyData)
    {
        var logData = new Dictionary<LogDataKey, object>
        {
            { LogDataKey.IsCriticalEvent, CriticalityLevel.No.ToString() },
            { LogDataKey.TenantId, tenantInfo.TenantId }
        };

        List<Task> tasks = new List<Task>();
        var historyStorageProvider = _historyStorageFactory.GetStorageProvider(tenantInfo);
        tasks.Add(historyStorageProvider.AddApprovalHistoryAsync(historyData));
        if (tenantInfo.HistoryLogging)
        {
            var historyTableStorageProvider = _historyStorageFactory.GetTableStorageProvider();
            tasks.Add(historyTableStorageProvider.AddApprovalHistoryAsync(historyData));
        }

        Task allTasks = Task.WhenAll(tasks.ToArray());

        try
        {
            await allTasks;
        }
        catch (Exception ex)
        {
            string message = string.Empty;
            foreach (var inEx in allTasks.Exception?.InnerExceptions)
            {
                message += inEx.Message;
            }

            logData.Add(LogDataKey.ErrorMessage, message);
            _logProvider.LogError(TrackingEvent.HistoryInsertFailure, ex, logData);
        }
    }

    /// <summary>
    /// Save list of approval transaction history
    /// </summary>
    /// <param name="tenantInfo"></param>
    /// <param name="historyDataList"></param>
    /// <returns></returns>
    public async Task AddApprovalHistoryAsync(ApprovalTenantInfo tenantInfo, List<TransactionHistory> historyDataList)
    {
        var logData = new Dictionary<LogDataKey, object>
        {
            // Add common data items to LogData
            { LogDataKey.IsCriticalEvent, CriticalityLevel.No.ToString() },
            { LogDataKey.TenantId, tenantInfo.TenantId }
        };

        List<Task> tasks = new List<Task>();
        var historyStorageProvider = _historyStorageFactory.GetStorageProvider(tenantInfo);
        tasks.Add(historyStorageProvider.AddApprovalHistoryAsync(historyDataList));
        if (tenantInfo.HistoryLogging)
        {
            var historyTableStorageProvider = _historyStorageFactory.GetTableStorageProvider();
            tasks.Add(historyTableStorageProvider.AddApprovalHistoryAsync(historyDataList));
        }

        Task allTasks = Task.WhenAll(tasks.ToArray());

        try
        {
            await allTasks;
        }
        catch (Exception ex)
        {
            string message = string.Empty;
            foreach (var inEx in allTasks.Exception?.InnerExceptions)
            {
                message += inEx.Message;
            }

            logData.Add(LogDataKey.ErrorMessage, message);
            _logProvider.LogError(TrackingEvent.HistoryInsertFailure, ex, logData);
        }
    }

    /// <summary>
    /// Check if history is inserted
    /// </summary>
    /// <param name="tenantInfo"></param>
    /// <param name="alias"></param>
    /// <param name="actionDate"></param>
    /// <param name="documentNumber"></param>
    /// <param name="actionTaken"></param>
    /// <param name="domain">Approver Domain</param>
    /// <param name="approverId">Approver Object Id</param>
    /// <returns></returns>
    public async Task<bool> CheckIfHistoryInsertedAsync(ApprovalTenantInfo tenantInfo, string alias, string actionDate, string documentNumber, string actionTaken, string domain, string approverId)
    {
        var historyStorageProvider = _historyStorageFactory.GetStorageProvider(tenantInfo);
        var historyList = await historyStorageProvider.GetHistoryDataAsync(alias, actionDate, documentNumber, actionTaken, domain, approverId);
        var list = historyList?.Where(h => (h.ActionDate.Value.ToUniversalTime()).ToString("yyyy-MM-dd HH:mm:ss") == actionDate).ToList();
        return (list == null || list.Count <= 0);
    }

    /// <summary>
    /// Get approver chain history data
    /// </summary>
    /// <param name="tenantInfo"></param>
    /// <param name="documentNumber"></param>
    /// <param name="fiscalYear"></param>
    /// <param name="alias"></param>
    /// <param name="Xcv"></param>
    /// <param name="Tcv"></param>
    /// <param name="sessionId"></param>
    /// <returns></returns>
    public async Task<List<TransactionHistoryExt>> GetApproverChainHistoryDataAsync(ApprovalTenantInfo tenantInfo, string documentNumber, string fiscalYear, string alias, string Xcv, string Tcv, string sessionId = null)
    {
        if (sessionId == null)
            sessionId = Tcv;

        var logData = new Dictionary<LogDataKey, object>
        {
            { LogDataKey.Tcv, Tcv},
            { LogDataKey.ReceivedTcv, Tcv },
            { LogDataKey.SessionId, sessionId},
            { LogDataKey.Xcv, Xcv},
            { LogDataKey.DXcv, documentNumber },
            { LogDataKey.UserRoleName, alias},
            { LogDataKey.EventType, Constants.FeatureUsageEvent },
            { LogDataKey.TenantId, tenantInfo.TenantId },
            { LogDataKey.DocumentNumber, documentNumber},
            { LogDataKey.FiscalYear, fiscalYear},
            { LogDataKey.IsCriticalEvent, CriticalityLevel.No.ToString()}
        };

        var historyData = new List<TransactionHistoryExt>();
        try
        {
            using (var perfTracer = _performanceLogger.StartPerformanceLogger("PerfLog", Constants.WebClient, string.Format(Constants.PerfLogActionWithInfo, "ApprovalHistoryProvider - GetApproverChainHistoryDataAsync", tenantInfo.TenantId, "Gets all the Historical Details by document number from the Azure Storage")
            , new Dictionary<LogDataKey, object> { { LogDataKey.DocumentNumber, documentNumber } }))
            {
                var historyStorageProvider = _historyStorageFactory.GetStorageProvider(tenantInfo);
                var dtTransactionHistory = await historyStorageProvider.GetHistoryDataAsync(tenantInfo.TenantId.ToString(), documentNumber);

                Func<TransactionHistory, TransactionHistoryExt> extendTransactionHistoryRecord
                    = (h) =>
                    {
                        var the = (h.ToJson()).FromJson<TransactionHistoryExt>();
                        return the;
                    };
                var tranHistories = dtTransactionHistory.ToList();
                var resultsExtended = from record in tranHistories
                                      let recordExtended = extendTransactionHistoryRecord(record)
                                      select recordExtended;
                historyData = resultsExtended.ToList();
            }
        }
        catch (Exception ex)
        {
            _logProvider.LogError(TrackingEvent.HistoryDataLoadFail, ex, logData);
        }
        return historyData;
    }

    /// <summary>
    /// Get approver chain history data
    /// </summary>
    /// <param name="tenantInfo"></param>
    /// <param name="documentNumber"></param>
    /// <param name="xcv"></param>
    /// <param name="tcv"></param>
    /// <param name="clientDevice"></param>
    /// <param name="sessionId"></param>
    /// <returns></returns>
    public async Task<List<TransactionHistoryExt>> GetApproverChainHistoryDataAsync(
        ApprovalTenantInfo tenantInfo,
        string documentNumber,
        string xcv,
        string tcv,
        string clientDevice,
        string sessionId = null)
    {
        if (string.IsNullOrWhiteSpace(sessionId))
            sessionId = tcv;

        var logData = new Dictionary<LogDataKey, object>
        {
            { LogDataKey.Tcv, tcv},
            { LogDataKey.ReceivedTcv, tcv },
            { LogDataKey.SessionId, sessionId },
            { LogDataKey.Xcv, xcv },
            { LogDataKey.DXcv, documentNumber },
            { LogDataKey.EventType, Constants.FeatureUsageEvent },
            { LogDataKey.DocumentNumber, documentNumber },
            { LogDataKey.IsCriticalEvent, CriticalityLevel.No.ToString() },
            { LogDataKey.ClientDevice, clientDevice }
        };

        var historyData = new List<TransactionHistoryExt>();
        try
        {
            using (_performanceLogger.StartPerformanceLogger("PerfLog", clientDevice, string.Format(Constants.PerfLogActionWithInfo, "ApprovalHistoryProvider - GetApproverChainHistoryDataAsync", string.Empty, "Gets all the Historical Details by document number from the Azure Storage")
            , new Dictionary<LogDataKey, object> { { LogDataKey.DocumentNumber, documentNumber } }))
            {
                var historyStorageProvider = _historyStorageFactory.GetStorageProvider(tenantInfo);
                var transactionHistory = await historyStorageProvider.GetHistoryDataAsync(tenantInfo?.TenantId.ToString(), documentNumber);

                Func<TransactionHistory, TransactionHistoryExt> ExtendTransactionHistoryRecord
                    = (history) =>
                    {
                        return (history.ToJson()).FromJson<TransactionHistoryExt>();
                    };

                var transactionHistories = transactionHistory.ToList();
                var resultsExtended = from record in transactionHistories
                                      let recordExtended = ExtendTransactionHistoryRecord(record)
                                      select recordExtended;
                historyData = resultsExtended.ToList();
            }
        }
        catch (Exception ex)
        {
            _logProvider.LogError(TrackingEvent.HistoryDataLoadFail, ex, logData);
        }

        return historyData;
    }

    /// <summary>
    /// Get transaction history count for a alias
    /// </summary>
    /// <param name="alias"></param>
    /// <param name="timePeriod"></param>
    /// <param name="searchCriteria"></param>
    /// <param name="signedInUser"></param>
    /// <param name="Xcv"></param>
    /// <returns></returns>
    public virtual async Task<JArray> GetHistoryCountforAliasAsync(string alias, int timePeriod, string searchCriteria, User signedInUser, string Xcv)
    {
        var logData = new Dictionary<LogDataKey, object>
        {
            // Add common data items to LogData
            { LogDataKey.Xcv, Xcv },
            { LogDataKey.Tcv, Xcv },
            { LogDataKey.ReceivedTcv, Xcv },
            { LogDataKey.SessionId, Xcv },
            { LogDataKey.UserRoleName, signedInUser.MailNickname },
            { LogDataKey.EventType, Constants.FeatureUsageEvent },
            { LogDataKey.UserAlias, alias },
            { LogDataKey.SearchText, searchCriteria },
            { LogDataKey.IsCriticalEvent, CriticalityLevel.No.ToString() }
        };

        if (string.IsNullOrEmpty(searchCriteria)) { searchCriteria = string.Empty; }
        if (timePeriod == 0) { timePeriod = int.Parse(_config[ConfigurationKey.MonthsOfHistoryDataValue.ToString()]); }
        searchCriteria = searchCriteria.ReplaceSqlSpecialCharacters();

        var tenantsCount = new JArray();
        try
        {
            var historyStorageProvider = _historyStorageFactory.GetStorageProvider(null);
            var history = await historyStorageProvider.GetHistoryDataAsync(signedInUser.MailNickname, signedInUser.UserPrincipalName.GetDomainFromUPN(), signedInUser.Id, timePeriod);
            history = history ?? new List<TransactionHistory>();
            List<TransactionHistory> dtTransactionHistory;
            if (string.IsNullOrEmpty(searchCriteria))
            {
                dtTransactionHistory = history.ToList();
            }
            else
            {
                dtTransactionHistory = history.Where(t => (t.PartitionKey.Contains(searchCriteria, StringComparison.OrdinalIgnoreCase) || t.AppName.Contains(searchCriteria, StringComparison.OrdinalIgnoreCase) ||
                                                           t.Title.Contains(searchCriteria, StringComparison.OrdinalIgnoreCase) || t.SubmitterName.Contains(searchCriteria, StringComparison.OrdinalIgnoreCase) ||
                                                           t.CompanyCode.Contains(searchCriteria, StringComparison.OrdinalIgnoreCase) || t.CompanyCode.Contains(searchCriteria, StringComparison.OrdinalIgnoreCase) ||
                                                           t.ActionTaken.Contains(searchCriteria, StringComparison.OrdinalIgnoreCase) || t.CustomAttribute.Contains(searchCriteria, StringComparison.OrdinalIgnoreCase) ||
                                                           t.UnitValue.Contains(searchCriteria, StringComparison.OrdinalIgnoreCase) ||
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

    /// <summary>
    /// Get history counts for each month in a specified time period
    /// </summary>
    /// <param name="alias"></param>
    /// <param name="timePeriod"></param>
    /// <param name="tcv"></param>
    /// <param name="approverDomain"></param>
    /// <param name="approverId"></param>
    /// <returns></returns>
    public virtual async Task<JArray> GetHistoryIntervalCountsforAliasAsync(string alias, int timePeriod, string tcv, string approverDomain, string approverId)
    {
        var logData = new Dictionary<LogDataKey, object>
        {
            // Add common data items to LogData
            { LogDataKey.EventType, Constants.FeatureUsageEvent },
            { LogDataKey.UserAlias, alias },
            { LogDataKey.Tcv, tcv },
            { LogDataKey.IsCriticalEvent, CriticalityLevel.No.ToString() }
        };

        if (timePeriod == 0) { timePeriod = int.Parse(_config[ConfigurationKey.MonthsOfHistoryDataValue.ToString()]); }

        var tasks = new List<Task>();
        var intervalsCount = new int[timePeriod];
        try
        {
            var historyStorageProvider = _historyStorageFactory.GetStorageProvider(null);
            List<TransactionHistory> history = null;
            foreach (var index in Enumerable.Range(0, timePeriod))
            {
                tasks.Add(Task.Run(async () => {
                    history = await historyStorageProvider.GetHistoryDataAsync(alias, approverDomain, approverId, index + 1, index);
                    history = history ?? new List<TransactionHistory>();
                    List<TransactionHistory> dtTransactionHistory;
                    dtTransactionHistory = history.ToList();
                    intervalsCount.SetValue(dtTransactionHistory.Count, (timePeriod - 1) - index);
                }));
            }
            await Task.WhenAll(tasks);
        }
        catch (Exception ex)
        {
            _logProvider.LogError(TrackingEvent.HistoryDataCountFail, ex, logData);
        }
        return JArray.FromObject(intervalsCount);
    }

    /// <summary>
    /// Get transaction history data
    /// </summary>
    /// <param name="alias"></param>
    /// <param name="timePeriod"></param>
    /// <param name="searchCriteria"></param>
    /// <param name="approverDomain">Approver Domain</param>
    /// <param name="approverId">Approver Object Id</param>
    /// <param name="page"></param>
    /// <param name="sortColumn"></param>
    /// <param name="sortDirection"></param>
    /// <param name="tenantId">TenantId. Unique for each Tenant</param>
    /// <returns></returns>
    public virtual async Task<PagedData<TransactionHistoryExtended>> GetHistoryDataAsync(string alias, int timePeriod, string searchCriteria,
       string approverDomain, string approverId, int? page = null, string sortColumn = null, string sortDirection = "DESC", string tenantId = "")
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
            skipSize = (pageSize * (page != 0 ? (int)page - 1 : 0));
        }

        searchCriteria = searchCriteria.ReplaceSqlSpecialCharacters();

        var historyStorageProvider = _historyStorageFactory.GetStorageProvider(null);
        var history = await historyStorageProvider.GetHistoryDataAsync(alias, approverDomain, approverId, timePeriod);
        history = history == null ? new List<TransactionHistory>() : history;
        List<TransactionHistory> dtTransactionHistory;
        if (string.IsNullOrEmpty(searchCriteria))
        {
            dtTransactionHistory = history.ToList();
        }
        else
        {
            dtTransactionHistory = history.Where(t => (t.PartitionKey.Contains(searchCriteria, StringComparison.OrdinalIgnoreCase) || t.AppName.Contains(searchCriteria, StringComparison.OrdinalIgnoreCase) ||
                                                       t.Title.Contains(searchCriteria, StringComparison.OrdinalIgnoreCase) || t.SubmitterName.Contains(searchCriteria, StringComparison.OrdinalIgnoreCase) ||
                                                       (t.CompanyCode != null && t.CompanyCode.Contains(searchCriteria, StringComparison.OrdinalIgnoreCase)) || t.UnitValue.Contains(searchCriteria, StringComparison.OrdinalIgnoreCase) ||
                                                       t.ActionTaken.Contains(searchCriteria, StringComparison.OrdinalIgnoreCase) || (t.CustomAttribute != null && t.CustomAttribute.Contains(searchCriteria, StringComparison.OrdinalIgnoreCase)) ||
                                                       t.ActionDate.Value.ToLocalTime().ToString().Contains(searchCriteria, StringComparison.OrdinalIgnoreCase)))
                                                       .ToList();
        }

        historyPagedData.TenantList = (from th in dtTransactionHistory
                                       select new TransactionHistoryExtended
                                       {
                                           TenantId = th.TenantId,
                                           AppName = th.AppName
                                       }).GroupBy(x => x.TenantId).Select(grp => grp.FirstOrDefault()).ToList();

        if (dtTransactionHistory != null && !string.IsNullOrWhiteSpace(tenantId) && int.TryParse(tenantId, out int value))
        {
            dtTransactionHistory = dtTransactionHistory.Where(h => h?.TenantId == tenantId)?.ToList();
        }

        historyPagedData.TotalCount = dtTransactionHistory.Count;

        if (sortColumn != null)
            dtTransactionHistory = sortDirection == "DESC" ? dtTransactionHistory.OrderByDescending(t => t.GetType().GetProperty(sortColumn).GetValue(t)).ToList() : dtTransactionHistory.OrderBy(t => t.GetType().GetProperty(sortColumn).GetValue(t)).ToList();
        else
            dtTransactionHistory = sortDirection == "DESC" ? dtTransactionHistory = dtTransactionHistory.OrderByDescending(t => t.ActionDate).ToList() : dtTransactionHistory = dtTransactionHistory.OrderBy(t => t.ActionDate).ToList();

        if (page != null)
            dtTransactionHistory = dtTransactionHistory.Skip(skipSize).Take(pageSize).ToList();

        TransactionHistoryExtended extendTransactionHistoryRecord(TransactionHistory h, ApprovalTenantInfo t)
        {
            var the = (h.ToJson()).FromJson<TransactionHistoryExtended>(); ;
            the.CondensedAppName = t.AppName.Replace(" ", "");
            the.TemplateName = t.TemplateName;
            the.IsHistoryClickable = t.IsHistoryClickable;
            the.BusinessProcessName = t.BusinessProcessName;
            return the;
        }

            var tranHistories = dtTransactionHistory.ToList();
            var tenants = _approvalTenantInfoProvider.GetTenantInfo();
            var resultsExtended = from record in tranHistories
                                  join tenant in tenants on record.TenantId equals tenant.RowKey
                                  let recordExtended = extendTransactionHistoryRecord(record, tenant)
                                  select recordExtended;
            historyPagedData.Result = resultsExtended.ToList();
            return historyPagedData;
        }

    /// <summary>
    /// Get transaction history data
    /// </summary>
    /// <param name="tenantInfo"></param>
    /// <param name="documentNumber"></param>
    /// <param name="approver"></param>
    /// <param name="Xcv"></param>
    /// <param name="Tcv"></param>
    /// <returns></returns>
    public async Task<List<TransactionHistory>> GetHistoryDataAsync(ApprovalTenantInfo tenantInfo, string documentNumber, string approver, string Xcv, string Tcv)
    {
        var logData = new Dictionary<LogDataKey, object>
        {
            // Add common data items to LogData
            { LogDataKey.Xcv, Xcv },
            { LogDataKey.Tcv, Tcv },
            { LogDataKey.ReceivedTcv, Tcv },
            { LogDataKey.UserAlias, approver },
            { LogDataKey.IsCriticalEvent, CriticalityLevel.No.ToString() },
            { LogDataKey.TenantId, tenantInfo.TenantId },
            { LogDataKey.DocumentNumber, documentNumber },
            { LogDataKey.DXcv, documentNumber }
        };

        var historyData = new List<TransactionHistory>();
        try
        {
            var historyStorageProvider = _historyStorageFactory.GetStorageProvider(tenantInfo);
            historyData = await historyStorageProvider.GetHistoryDataAsync(tenantInfo.TenantId.ToString(), documentNumber, approver);
        }
        catch (Exception ex)
        {
            _logProvider.LogError(TrackingEvent.WebApiHistoryDataLoadFail, ex, logData);
        }
        return historyData;
    }
}