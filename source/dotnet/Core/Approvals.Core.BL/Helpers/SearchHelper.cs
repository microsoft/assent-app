// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.CFS.Approvals.Common.DL.Interface;
using Microsoft.CFS.Approvals.Core.BL.Interface;
using Microsoft.CFS.Approvals.Extensions;
using Microsoft.CFS.Approvals.LogManager.Model;
using Microsoft.CFS.Approvals.LogManager.Provider.Interface;
using Microsoft.CFS.Approvals.Model;
using Microsoft.CFS.Approvals.Utilities.Helpers;
using Microsoft.CFS.Approvals.Contracts.DataContracts;
using Microsoft.Extensions.Configuration;

namespace Microsoft.CFS.Approvals.Core.BL.Helpers;

/// <summary>
/// Helper class for searching approval summaries with flexible filtering, sorting, and limiting.
/// </summary>
public class SearchHelper : ISearchHelper
{
    #region Variables

    /// <summary>
    /// The log provider
    /// </summary>
    protected readonly ILogProvider _logProvider;

    /// <summary>
    /// The performance logger
    /// </summary>
    protected readonly IPerformanceLogger _performanceLogger;

    /// <summary>
    /// The Configuration
    /// </summary>
    protected readonly IConfiguration _config;

    /// <summary>
    /// The summary helper
    /// </summary>
    protected readonly ISummaryHelper _summaryHelper;

    /// <summary>
    /// The Tenant Info helper
    /// </summary>
    protected readonly IApprovalTenantInfoHelper _tenantInfoHelper;

    /// <summary>
    /// The approval summary provider for user-scoped document lookups.
    /// </summary>
    protected readonly IApprovalSummaryProvider _approvalSummaryProvider;

    #endregion Variables

    #region Constructor

    /// <summary>
    /// Constructor
    /// </summary>
    /// <param name="logProvider"></param>
    /// <param name="performanceLogger"></param>
    /// <param name="configuration"></param>
    /// <param name="summaryHelper"></param>
    /// <param name="tenantInfoHelper"></param>
    /// <param name="approvalSummaryProvider"></param>
    public SearchHelper(
        ILogProvider logProvider,
        IPerformanceLogger performanceLogger,
        IConfiguration configuration,
        ISummaryHelper summaryHelper,
        IApprovalTenantInfoHelper tenantInfoHelper,
        IApprovalSummaryProvider approvalSummaryProvider)
    {
        _logProvider = logProvider;
        _performanceLogger = performanceLogger;
        _config = configuration;
        _summaryHelper = summaryHelper;
        _tenantInfoHelper = tenantInfoHelper;
        _approvalSummaryProvider = approvalSummaryProvider;
    }

    #endregion Constructor

    /// <summary>
    /// Returns a list of document numbers matching the search filters.
    /// <param name="searchFilters"></param>
    /// <param name="domain"></param>
    /// <param name="signedInUser"></param>
    /// <param name="onBehalfUser"></param>
    /// <param name="host"></param>
    /// <param name="oauth2UserToken"></param>
    /// </summary>
    public async Task<List<string>> GetSearchResultsDocumentNumbersAsync(
        Filters searchFilters,
        string domain,
        User signedInUser,
        User onBehalfUser,
        string host,
        string oauth2UserToken)
    {
        #region Logging

        var logData = new Dictionary<LogDataKey, object>
        {
            { LogDataKey.StartDateTime, DateTime.UtcNow },
            { LogDataKey.UserAlias, signedInUser.UserPrincipalName },
            { LogDataKey.UserRoleName, onBehalfUser.UserPrincipalName },
            { LogDataKey.SearchFilters, searchFilters }
        };

        _logProvider.LogInformation(TrackingEvent.DeepSearchInitiated, logData);

        #endregion Logging

        try
        {
            // Use the generic method to select document numbers
            var documentNumbers = await GetResultsAsync(
                searchFilters, domain, signedInUser, onBehalfUser, host, oauth2UserToken,
                summary => summary.ApprovalIdentifier.DisplayDocumentNumber
            );

            _logProvider.LogInformation(TrackingEvent.DeepSearchSuccess, logData);

            return documentNumbers;
        }
        catch (Exception ex)
        {
            _logProvider.LogError(TrackingEvent.DeepSearchFailed, ex, logData);
            throw;
        }
    }

    /// <summary>
    /// Returns a list of ApprovalSummaryData objects matching the search filters.
    /// <param name="searchFilters"></param>
    /// <param name="domain"></param>
    /// <param name="signedInUser"></param>
    /// <param name="onBehalfUser"></param>
    /// <param name="host"></param>
    /// <param name="oauth2UserToken"></param>
    /// </summary>
    public async Task<List<ApprovalSummaryData>> GetSearchResultSummaryObjectsAsync(
        Filters searchFilters,
        string domain,
        User signedInUser,
        User onBehalfUser,
        string host,
        string oauth2UserToken)
    {
        // Use the generic method to select full summary objects

        #region Logging

        var logData = new Dictionary<LogDataKey, object>
        {
            { LogDataKey.StartDateTime, DateTime.UtcNow },
            { LogDataKey.UserAlias, signedInUser.UserPrincipalName },
            { LogDataKey.UserRoleName, onBehalfUser.UserPrincipalName },
            { LogDataKey.SearchFilters, searchFilters }
        };

        _logProvider.LogInformation(TrackingEvent.DeepSearchInitiated, logData);

        #endregion Logging

        try
        {
            return await GetResultsAsync(
            searchFilters, domain, signedInUser, onBehalfUser, host, oauth2UserToken,
            summary => summary
            );
        }
        catch (Exception ex)
        {
            _logProvider.LogError(TrackingEvent.DeepSearchFailed, ex, logData);
            throw;
        }
    }

    /// <summary>
    /// Resolves tenantId for a provided document number (matches DocumentNumber or ApprovalIdentifier.DisplayDocumentNumber).
    /// Uses a fast path via ApprovalDetails table lookup before falling back to full summary search.
    /// Returns null if not found.
    /// </summary>
    public async Task<int?> FindTenantIdByDocumentNumberAsync(string documentNumber, User signedInUser, User onBehalfUser, string host, string domain, string oauth2UserToken)
    {
        if (string.IsNullOrWhiteSpace(documentNumber)) return null;

        #region Logging

        var logData = new Dictionary<LogDataKey, object>
        {
            { LogDataKey.StartDateTime, DateTime.UtcNow },
            { LogDataKey.UserAlias, signedInUser.UserPrincipalName },
            { LogDataKey.UserRoleName, onBehalfUser.UserPrincipalName },
            { LogDataKey.DocumentNumber, documentNumber }
        };

        _logProvider.LogInformation(TrackingEvent.FindTenantIdByDocumentNumberInitiated, logData);

        #endregion Logging

        try
        {
            // Fast path: Query ApprovalSummary table scoped to the signed-in user's approval queue.
            // approverAlias/approverId are derived from the auth context (not user input) to prevent spoofing.
            var approverAlias = onBehalfUser.MailNickname;
            var approverId = onBehalfUser.Id;
            var summaryRow = await _approvalSummaryProvider.FindSummaryByApproverAndDocumentNumberAsync(documentNumber, approverAlias, approverId, domain);
            if (summaryRow != null)
            {
                // Extract docTypeId from the RowKey (format: "docTypeId|...")
                var docTypeId = summaryRow.RowKey.Split('|')[0];
                var tenants = _tenantInfoHelper.GetTenantInfoByHost(host);
                var tenantInfo = tenants?.FirstOrDefault(t => string.Equals(t.DocTypeId, docTypeId, StringComparison.OrdinalIgnoreCase));
                if (tenantInfo != null && tenantInfo.TenantId > 0)
                {
                    logData[LogDataKey.EndDateTime] = DateTime.UtcNow;
                    _logProvider.LogInformation(TrackingEvent.FindTenantIdByDocumentNumberSuccess, logData);
                    return tenantInfo.TenantId;
                }
            }

            // Fallback: Search through user summaries if not found via direct lookup.
            // This handles edge cases where the summary row might not be returned by the fast path.
            var summaryArray = await _summaryHelper.GetSummary(signedInUser, onBehalfUser, host, domain, oauth2UserToken);
            var summaries = summaryArray.ToObject<List<ApprovalSummaryData>>();
            if (summaries == null || summaries.Count == 0) return null;
            var match = summaries.FirstOrDefault(s =>
                string.Equals(s.DocumentNumber, documentNumber, StringComparison.OrdinalIgnoreCase) ||
                (s.ApprovalIdentifier?.DisplayDocumentNumber != null && string.Equals(s.ApprovalIdentifier.DisplayDocumentNumber, documentNumber, StringComparison.OrdinalIgnoreCase)));

            logData[LogDataKey.EndDateTime] = DateTime.UtcNow;
            _logProvider.LogInformation(TrackingEvent.FindTenantIdByDocumentNumberSuccess, logData);
            return match?.TenantId;
        }
        catch (Exception ex)
        {
            logData[LogDataKey.EndDateTime] = DateTime.UtcNow;
            _logProvider.LogError(TrackingEvent.FindTenantIdByDocumentNumberFailed, ex, logData);
            return null;
        }
    }

    #region Private Methods

    /// <summary>
    /// Centralized generic method for filtering, sorting, and limiting summary results.
    /// </summary>
    /// <typeparam name="TResult">The type to project each summary to.</typeparam>
    /// <param name="searchFilters">Search filters to apply.</param>
    /// <param name="domain">Tenant domain.</param>
    /// <param name="signedInUser">Signed-in user.</param>
    /// <param name="onBehalfUser">On-behalf user.</param>
    /// <param name="host">Tenant host.</param>
    /// <param name="oauth2UserToken">OAuth2 token.</param>
    /// <param name="selector">Projection function for each summary.</param>
    /// <returns>List of projected results.</returns>
    private async Task<List<TResult>> GetResultsAsync<TResult>(
       Filters searchFilters,
       string domain,
       User signedInUser,
       User onBehalfUser,
       string host,
       string oauth2UserToken,
       Func<ApprovalSummaryData, TResult> selector)
    {
        var summaryArray = await _summaryHelper.GetSummary(signedInUser, onBehalfUser, host, domain, oauth2UserToken);
        List<ApprovalSummaryData> approverSummary = summaryArray.ToObject<List<ApprovalSummaryData>>();

        if (searchFilters != null)
        {
            var filtered = FilterSummaries(approverSummary, searchFilters.SearchFilters);
            var sorted = SortSummaries(filtered, searchFilters.Sort);
            var limited = LimitSummaries(sorted, searchFilters.Limit);

            return limited.Select(x => selector(x.summary)).ToList();
        }

        return new List<TResult>();
    }

    /// <summary>
    /// Filters the summaries based on the provided search filters.
    /// <param name="userSummaryJson"></param>
    /// <param name="searchFilters"></param>
    /// </summary>
    private static IEnumerable<(ApprovalSummaryData summary, Dictionary<string, string> dict)> FilterSummaries(
        List<ApprovalSummaryData> userSummaryJson, SearchFilters searchFilters)
    {
        var isAndOperator = string.Equals(searchFilters.Operator, "AND", StringComparison.OrdinalIgnoreCase);
        return userSummaryJson
            .Select(summary => (summary, dict: ToDictionary(summary)))
            .Where(x =>
            {
                var filterResults = searchFilters.Conditions.Select(filter =>
                    MatchesFilter(x.dict, filter));
                return isAndOperator ? filterResults.All(r => r) : filterResults.Any(r => r);
            });
    }

    /// <summary>
    /// Checks if the summary matches the filter condition.
    /// <param name="objectDictionary"></param>
    /// <param name="condition"></param>
    /// </summary>
    private static bool MatchesFilter(Dictionary<string, string> objectDictionary, Condition condition)
    {
        if (!objectDictionary.TryGetValue(condition.Field, out var actualValue)) return false;
        return EvaluateCondition(actualValue, condition);
    }

    /// <summary>
    /// Evaluates a single filter condition against an actual value.
    /// <param name="actualValue"></param>
    /// <param name="condition"></param>
    /// </summary>
    private static bool EvaluateCondition(string actualValue, Condition condition)
    {
        var targetValue = condition.Value;
        var op = condition.Operator;

        // Special cases
        if (op == "isNotNumeric")
        {
            return string.IsNullOrWhiteSpace(actualValue) || !decimal.TryParse(actualValue, out _);
        }

        // Date comparison
        if (condition.Field == "SubmittedDate")
        {
            if (DateTime.TryParse(actualValue, out var actualDate) && DateTime.TryParse(targetValue, out var targetDate))
            {
                return op switch
                {
                    "=" => actualDate == targetDate,
                    "!=" => actualDate != targetDate,
                    ">" => actualDate > targetDate,
                    "<" => actualDate < targetDate,
                    ">=" => actualDate >= targetDate,
                    "<=" => actualDate <= targetDate,
                    _ => false
                };
            }
        }

        // Numeric comparison
        if (op is ">" or "<" or ">=" or "<=")
        {
            if (decimal.TryParse(actualValue, out var actualNum) && decimal.TryParse(targetValue, out var targetNum))
            {
                return op switch
                {
                    ">" => actualNum > targetNum,
                    "<" => actualNum < targetNum,
                    ">=" => actualNum >= targetNum,
                    "<=" => actualNum <= targetNum,
                    _ => false
                };
            }
            return false;
        }

        // Equality comparison
        if (op is "=" or "!=")
        {
            if (decimal.TryParse(actualValue, out var actualNum) && decimal.TryParse(targetValue, out var targetNum))
            {
                return op == "=" ? actualNum == targetNum : actualNum != targetNum;
            }
            return op == "=" ? actualValue == targetValue : actualValue != targetValue;
        }

        // String comparison
        return op switch
        {
            "~" => actualValue.Contains(targetValue, StringComparison.OrdinalIgnoreCase),
            "!~" => !actualValue.Contains(targetValue, StringComparison.OrdinalIgnoreCase),
            _ => false
        };
    }

    /// <summary>
    /// Sorts the filtered summaries based on the sort options provided.
    /// <param name="summaries"></param>
    /// <param name="sort"></param>
    /// </summary>
    private static IEnumerable<(ApprovalSummaryData summary, Dictionary<string, string> dict)> SortSummaries(
        IEnumerable<(ApprovalSummaryData summary, Dictionary<string, string> dict)> summaries, SortOptions sort)
    {
        if (sort == null || string.IsNullOrEmpty(sort.Field))
            return summaries;

        var keySelector = GetSortKeySelector(sort);
        return sort.Direction == "desc"
            ? summaries.OrderByDescending(x => keySelector(x.dict))
            : summaries.OrderBy(x => keySelector(x.dict));
    }

    /// <summary>
    /// Gets the key selector function for sorting based on the sort options.
    /// <param name="sort"></param>
    /// </summary>
    private static Func<Dictionary<string, string>, IComparable> GetSortKeySelector(SortOptions sort)
    {
        var sortType = sort.Type?.ToLowerInvariant();
        if (sortType == "number")
        {
            return dict =>
                dict.TryGetValue(sort.Field, out var value) && decimal.TryParse(value, out var num) ? num : decimal.MinValue;
        }
        else if (sortType == "date")
        {
            return dict =>
                dict.TryGetValue(sort.Field, out var value) && DateTime.TryParse(value, out var dt) ? dt : DateTime.MinValue;
        }
        else
        {
            return dict =>
                dict.TryGetValue(sort.Field, out var value) ? value : string.Empty;
        }
    }

    /// <summary>
    /// Limits the number of summaries returned based on the limit specified.
    /// <param name="summaries"></param>
    /// <param name="limit"></param>
    /// </summary>
    private static IEnumerable<(ApprovalSummaryData summary, Dictionary<string, string> dict)> LimitSummaries(
        IEnumerable<(ApprovalSummaryData summary, Dictionary<string, string> dict)> summaries, int? limit)
    {
        if (limit.HasValue && limit.Value > 0)
            return summaries.Take(limit.Value);
        return summaries;
    }

    /// <summary>
    /// Converts a summary to a dictionary for filtering and sorting.
    /// <param name="summary"></param>
    /// </summary>
    private static Dictionary<string, string> ToDictionary(ApprovalSummaryData summary)
    {
        var dict = new Dictionary<string, string>();
        JSONHelper.ConvertJsonToDictionary(dict, summary.ToJson());
        return dict;
    }

    #endregion Private Methods
}