// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace Microsoft.CFS.Approvals.CoreServices.BL.Helpers;

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.CFS.Approvals.Common.DL.Interface;
using Microsoft.CFS.Approvals.Contracts;
using Microsoft.CFS.Approvals.Contracts.DataContracts;
using Microsoft.CFS.Approvals.CoreServices.BL.Interface;
using Microsoft.CFS.Approvals.Extensions;
using Microsoft.CFS.Approvals.LogManager.Model;
using Microsoft.CFS.Approvals.LogManager.Provider.Interface;
using Microsoft.CFS.Approvals.Model;
using Microsoft.CFS.Approvals.Utilities.Helpers;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json.Linq;

/// <summary>
/// the approvalhistoryhelper class.
/// </summary>
public class ApprovalHistoryHelper : IApprovalHistoryHelper
{
    #region VARIABLES

    /// <summary>
    /// The history provider.
    /// </summary>
    private readonly IApprovalHistoryProvider _historyProvider = null;

    /// <summary>
    /// The performance logger.
    /// </summary>
    private readonly IPerformanceLogger _performanceLogger = null;

    /// <summary>
    /// The Log Provider.
    /// </summary>
    private readonly ILogProvider _logProvider = null;

    /// <summary>
    /// The configuration.
    /// </summary>
    private readonly IConfiguration _config;

    #endregion VARIABLES

    #region CONSTRUCTOR

    /// <summary>
    /// Initializes a new instance of the <see cref="ApprovalHistoryHelper"/> class.
    /// </summary>
    /// <param name="historyProvider">THe history provider.</param>
    /// <param name="config">The configuration.</param>
    /// <param name="logProvider">The log provider.</param>
    /// <param name="performanceLogger">The performance logger.</param>
    public ApprovalHistoryHelper(IApprovalHistoryProvider historyProvider, IConfiguration config, ILogProvider logProvider, IPerformanceLogger performanceLogger)
    {
        _historyProvider = historyProvider;
        _logProvider = logProvider;
        _performanceLogger = performanceLogger;
        _config = config;
    }

    #endregion CONSTRUCTOR

    #region Implemented Methods

    /// <summary>
    /// Get history data for the user with the given search criteria.
    /// </summary>
    /// <param name="alias">Alias.</param>
    /// <param name="timePeriod">Time period.</param>
    /// <param name="searchCriteria">Search criteria.</param>
    /// <param name="page">Page number.</param>
    /// <param name="sortColumn">Sort column.</param>
    /// <param name="sortDirection">Sort direction (ASC or DESC).</param>
    /// <returns>History data for the user with the given search criteria.</returns>
    private async Task<PagedData<TransactionHistoryExtended>> GetHistoryData(string alias, int timePeriod, string searchCriteria, int? page = null, string sortColumn = null, string sortDirection = "DESC")
    {
        return await _historyProvider.GetHistoryDataAsync(alias, timePeriod, searchCriteria, page, sortColumn, sortDirection);
    }

    /// <summary>
    /// Get history data for the user with the given search criteria.
    /// </summary>
    /// <param name="page">Page number.</param>
    /// <param name="sortColumn">Sort column.</param>
    /// <param name="sortDirection">Sort direction (ASC or DESC).</param>
    /// <param name="searchCriteria">Search criteria.</param>
    /// <param name="timePeriod">Time period.</param>
    /// <param name="sessionId">Session Id.</param>
    /// <param name="loggedInAlias">Logged-in user alias.</param>
    /// <param name="alias">Alias.</param>
    /// <param name="clientDevice">Client device.</param>
    /// <param name="xcv">Xcv.</param>
    /// <param name="tcv">Tcv.</param>
    /// <param name="tenantId">TenantId. Unique for each Tenant</param>
    /// <returns>History data for the user with the given search criteria.</returns>
    public async Task<JObject> GetHistory(int page, string sortColumn, string sortDirection, string searchCriteria, int timePeriod, string sessionId, string loggedInAlias, string alias, string clientDevice, string xcv, string tcv, string tenantId = "")
    {
        #region Logging

        var logData = new Dictionary<LogDataKey, object>
        {
            { LogDataKey.StartDateTime, DateTime.UtcNow },
            { LogDataKey.Xcv, xcv },
            { LogDataKey.Tcv, tcv },
            { LogDataKey.SessionId, sessionId },
            { LogDataKey.UserRoleName, loggedInAlias },
            { LogDataKey.ClientDevice, clientDevice },
            { LogDataKey.EventType, Constants.FeatureUsageEvent },
            { LogDataKey.UserAlias, alias },
            { LogDataKey.SearchText, searchCriteria },
            { LogDataKey.PageNum, page },
            { LogDataKey.IsCriticalEvent, CriticalityLevel.No.ToString() }
        };

        #endregion Logging

        try
        {
            using (var historyTracer = _performanceLogger.StartPerformanceLogger("PerfLog", string.IsNullOrWhiteSpace(clientDevice) ? Constants.WebClient : clientDevice, String.Format(Constants.PerfLogCommon, "History"), logData))
            {
                if (string.IsNullOrWhiteSpace(searchCriteria))
                {
                    searchCriteria = string.Empty;
                }

                if (page < 1)
                {
                    throw new InvalidDataException(_config[ConfigurationKey.Message_PageLessThan1.ToString()]);
                }

                var historyData = await GetHistoryData(alias, timePeriod, searchCriteria, page, sortColumn, sortDirection);
                if (historyData.Result != null && !string.IsNullOrWhiteSpace(tenantId) && int.TryParse(tenantId, out int value))
                {
                    historyData.Result = historyData.Result.Where(h => h?.TenantId == tenantId)?.ToList();
                }

                var responseObject = new
                {
                    TotalRecords = historyData.TotalCount,
                    Records = historyData.Result
                };

                // Serialize.
                var serializedHistoryData = (responseObject.ToJson()).ToJObject();

                // Log Success.
                logData.Modify(LogDataKey.EndDateTime, DateTime.UtcNow);
                _logProvider.LogInformation(TrackingEvent.WebApiHistoryDataLoadSuccess, logData);

                // Send the history data.
                return serializedHistoryData;
            }
        }
        catch (Exception ex)
        {
            // Log Failure.
            logData.Modify(LogDataKey.EndDateTime, DateTime.UtcNow);
            _logProvider.LogError(TrackingEvent.WebApiHistoryDataLoadFail, ex, logData);
            throw;
        }
    }

    /// <summary>
    /// Get history data for the user with the given search criteria.
    /// </summary>
    /// <param name="page">Page number.</param>
    /// <param name="sortColumn">Sort column.</param>
    /// <param name="sortDirection">Sort direction (ASC or DESC).</param>
    /// <param name="searchCriteria">Search criteria.</param>
    /// <param name="timePeriod">Time period.</param>
    /// <param name="sessionId">Session Id.</param>
    /// <param name="loggedInAlias">Logged-in user alias.</param>
    /// <param name="alias">Alias.</param>
    /// <param name="clientDevice">Client device.</param>
    /// <param name="xcv">Xcv.</param>
    /// <param name="tcv">Tcv.</param>
    /// <param name="tenantId">TenantId. Unique for each Tenant</param>
    /// <returns>History data for the user with the given search criteria.</returns>
    public async Task<JArray> GetHistoryMappedToSummary(int page, string sortColumn, string sortDirection, string searchCriteria, int timePeriod, string sessionId, string loggedInAlias, string alias, string clientDevice, string xcv, string tcv, string tenantId = "")
    {
        var logData = new Dictionary<LogDataKey, object>
        {
            { LogDataKey.StartDateTime, DateTime.UtcNow },
            { LogDataKey.Xcv, xcv },
            { LogDataKey.Tcv, tcv },
            { LogDataKey.SessionId, sessionId },
            { LogDataKey.UserRoleName, loggedInAlias },
            { LogDataKey.ClientDevice, clientDevice },
            { LogDataKey.EventType, Constants.FeatureUsageEvent },
            { LogDataKey.UserAlias, alias },
            { LogDataKey.SearchText, searchCriteria },
            { LogDataKey.PageNum, page },
            { LogDataKey.IsCriticalEvent, CriticalityLevel.No.ToString() }
        };

        try
        {
            var responseObject = await GetHistory(page, sortColumn, sortDirection, searchCriteria, timePeriod, sessionId, loggedInAlias, alias, clientDevice, xcv, tcv, tenantId);
            List<TransactionHistoryExtended> historyData = responseObject["Records"].ToJson().FromJson<List<TransactionHistoryExtended>>();

            // Log Success.
            logData.Add(LogDataKey.EndDateTime, DateTime.UtcNow);
            _logProvider.LogInformation(TrackingEvent.WebApiHistoryDataLoadSuccess, logData);
            return GetSummaryFromHistory(historyData);
        }
        catch (Exception ex)
        {
            // Log Failure.
            logData.Add(LogDataKey.EndDateTime, DateTime.UtcNow);
            _logProvider.LogError(TrackingEvent.WebApiHistoryDataLoadFail, ex, logData);
            throw;
        }
    }

    /// <summary>
    /// Download history data for the user with the given search criteria in excel.
    /// </summary>
    /// <param name="sortColumn">Sort column.</param>
    /// <param name="sortDirection">Sort direction (ASC or DESC).</param>
    /// <param name="searchCriteria">Search criteria.</param>
    /// <param name="timePeriod">Time period.</param>
    /// <param name="sessionId">Session Id.</param>
    /// <param name="loggedInAlias">Logged-in user alias.</param>
    /// <param name="alias">Alias.</param>
    /// <param name="clientDevice">Client device.</param>
    /// <param name="xcv">Xcv.</param>
    /// <param name="tcv">Tcv.</param>
    /// <param name="tenantId">TenantId. Unique for each Tenant</param>
    /// <returns>Excel with hisotry data for the user with the given search criteria.</returns>
    public async Task<byte[]> DownloadHistoryDataInExcel(string sortColumn, string sortDirection, string searchCriteria, int timePeriod, string sessionId, string loggedInAlias, string alias, string clientDevice, string xcv, string tcv, string tenantId = "")
    {
        #region Logging

        var logData = new Dictionary<LogDataKey, object>
        {
            { LogDataKey.StartDateTime, DateTime.UtcNow },
            { LogDataKey.Xcv, xcv },
            { LogDataKey.Tcv, tcv },
            { LogDataKey.SessionId, sessionId },
            { LogDataKey.UserRoleName, loggedInAlias },
            { LogDataKey.ClientDevice, clientDevice },
            { LogDataKey.EventType, Constants.FeatureUsageEvent },
            { LogDataKey.UserAlias, alias },
            { LogDataKey.SearchText, searchCriteria },
            { LogDataKey.Months, timePeriod },
            { LogDataKey.SortColumn, sortColumn },
            { LogDataKey.SortDirection, sortDirection },
            { LogDataKey.IsCriticalEvent, CriticalityLevel.No.ToString() }
        };

        #endregion Logging

        try
        {
            var historyData = await GetHistoryData(alias, timePeriod, searchCriteria, null, sortColumn, sortDirection);
            if (historyData.Result != null && !string.IsNullOrWhiteSpace(tenantId))
            {
                historyData.Result = historyData.Result.Where(h => h?.TenantId == tenantId)?.ToList();
            }

            var commaDelimited = string.Join("\n", (from record in historyData.Result
                                                    select MSAHelper.FormatForCSV(record.SubmitterName) + ","
                                                    + MSAHelper.FormatForCSV(record.AppName) + ","
                                                    + MSAHelper.FormatForCSV(record.DocumentNumber) + ","
                                                    + MSAHelper.FormatForCSV(record.Title) + ","
                                                    + MSAHelper.FormatForCSV(record.ActionTaken) + ","
                                                    + MSAHelper.FormatForCSV(record.ActionDate) + ","
                                                    + MSAHelper.FormatForCSV(record.UnitValue) + ","
                                                    + MSAHelper.FormatForCSV(record.AmountUnits) + ","
                                                    + MSAHelper.FormatForCSV(record.CompanyCode) + ","
                                                    + MSAHelper.FormatForCSV(record.CustomAttribute)).ToArray());
            commaDelimited = "Submitter,App,Record#,Title,Action,Date,UnitValue,UnitMeasure,Company Code,Additional Information\n" + commaDelimited;

            var encoding = Encoding.UTF8;
            var fileContent = encoding.GetString(new byte[] { 0xEF, 0xBB, 0xBF }) + commaDelimited;
            var response = Encoding.UTF8.GetBytes(fileContent);
            // Log Success.
            logData.Add(LogDataKey.EndDateTime, DateTime.UtcNow);
            _logProvider.LogInformation(TrackingEvent.WebApiHistoryDownloadSuccess, logData);

            return response;
        }
        catch (Exception ex)
        {
            // Log Failure.
            logData.Add(LogDataKey.EndDateTime, DateTime.UtcNow);
            _logProvider.LogError(TrackingEvent.WebApiHistoryDownloadFail, ex, logData);
            throw;
        }
    }

    /// <summary>
    /// Get history count for the user with the given search criteria.
    /// </summary>
    /// <param name="alias">Alias.</param>
    /// <param name="timePeriod">Time period.</param>
    /// <param name="searchCriteria">Search criteria.</param>
    /// <param name="loggedInAlias">Logged-in user alias.</param>
    /// <param name="sessionId">Session Id.</param>
    /// <param name="clientDevice">Client device.</param>
    /// <param name="xcv">Xcv.</param>
    /// <param name="tcv">Tcv.</param>
    /// <returns>History count for the user with the given search criteria.</returns>
    public async Task<JArray> GetHistoryCountforAlias(string alias, int timePeriod, string searchCriteria, string loggedInAlias, string sessionId, string clientDevice, string xcv, string tcv)
    {
        #region Logging

        var logData = new Dictionary<LogDataKey, object>
        {
            { LogDataKey.StartDateTime, DateTime.UtcNow },
            { LogDataKey.Xcv, xcv },
            { LogDataKey.Tcv, tcv },
            { LogDataKey.SessionId, sessionId },
            { LogDataKey.UserRoleName, loggedInAlias },
            { LogDataKey.ClientDevice, clientDevice },
            { LogDataKey.EventType, Constants.FeatureUsageEvent },
            { LogDataKey.UserAlias, alias },
            { LogDataKey.SearchText, searchCriteria },
            { LogDataKey.IsCriticalEvent, CriticalityLevel.No.ToString() }
        };

        #endregion Logging

        try
        {
            using (_performanceLogger.StartPerformanceLogger("PerfLog", string.IsNullOrWhiteSpace(clientDevice) ? Constants.WebClient : clientDevice, string.Format(Constants.PerfLogCommon, "History Count"), logData))
            {
                var historyData = await _historyProvider.GetHistoryCountforAliasAsync(alias, timePeriod, searchCriteria, loggedInAlias, tcv);

                // Log Success.
                logData.Modify(LogDataKey.EndDateTime, DateTime.UtcNow);
                _logProvider.LogInformation(TrackingEvent.WebApiHistoryDataCountSuccess, logData);

                // Send the historyCount data.
                return historyData;
            }
        }
        catch (Exception ex)
        {
            // Log Failure.
            logData.Modify(LogDataKey.EndDateTime, DateTime.UtcNow);
            _logProvider.LogError(TrackingEvent.WebApiHistoryDataCountFail, ex, logData);
            throw;
        }
    }

    #endregion Implemented Methods

    #region Helper Methods

    /// <summary>
    /// This method will return history data in the format of Summary Data Contract
    /// </summary>
    /// <param name="historyDataList">History datalist</param>
    /// <returns>returns JArray of history data</returns>
    public JArray GetSummaryFromHistory(List<TransactionHistoryExtended> historyDataList)
    {
        JArray summaryRows = new JArray();
        string AdaptiveTemplateUrl = _config[ConfigurationKey.ApprovalsCoreServicesURL.ToString()] + "/api/v1/AdaptiveDetail/{0}";
        string DataUrl = _config[ConfigurationKey.ApprovalsCoreServicesURL.ToString()] + "/api/v1/Detail/{0}/{1}?PageType=History";

        foreach (var historyData in historyDataList)
        {
            int tenantId = Int32.Parse(historyData.TenantId);
            var customAttribute = new Dictionary<string, string>
            {
                { "CustomAttributeName", string.Empty },
                { "CustomeAttributeVale", historyData.CustomAttribute }
            };

            var approvalIdentifier = new JObject
            {
                { "DisplayDocumentNumber", historyData.PartitionKey },
                { "DocumentNumber", historyData.DocumentNumber },
                { "FiscalYear", historyData.FiscalYear },
                { "DocumentNumberPrefix", string.Empty }
            };

            var submitter = new NameAliasEntity() { Name = historyData.SubmitterName, Alias = historyData.SubmittedAlias };

            Dictionary<string, string> additionalData = new Dictionary<string, string>() {
                                    { "TemplateUri", string.Format(AdaptiveTemplateUrl, tenantId) },
                                    { "DetailsUri", string.Format(DataUrl, tenantId, historyData.PartitionKey) }};

            object summaryData = new
            {
                TenantId = tenantId,
                DocumentTypeId = historyData?.DocumentTypeID,
                Approver = string.Empty,
                SummaryJson = string.Empty,
                historyData?.AppName,
                historyData?.CondensedAppName,
                Submitter = submitter,
                historyData?.Title,
                Amount = string.Empty,
                CurrencyCode = string.Empty,
                SubmittedDate = historyData?.SubmittedDate != null ? ((DateTimeOffset)historyData?.SubmittedDate).UtcDateTime.ToString("s") + "Z" : null,
                DocumentNumber = string.Empty,
                CustomAttribute = customAttribute,
                DetailOperations = string.Empty,
                historyData?.TemplateName,
                historyData?.CompanyCode,
                IsOfflineApprovalSupported = false,
                ReadDetails = false,
                ApprovalIdentifier = approvalIdentifier,
                historyData?.UnitValue,
                UnitOfMeasure = historyData?.AmountUnits,
                AdditionalData = additionalData,
                LastFailed = false,
                LastFailedExceptionMessage = string.Empty,
                historyData?.Xcv,
                historyData?.BusinessProcessName,
                IsRead = false,
                IsControlsAndComplianceRequired = true,
                IsBackgroundApprovalSupportedUpfront = false
            };

            summaryRows.Add(summaryData?.ToJson()?.ToJObject());
        }

        return summaryRows;
    }

    #endregion Helper Methods
}