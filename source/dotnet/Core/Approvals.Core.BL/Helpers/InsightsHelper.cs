// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.CFS.Approvals.Common.DL.Interface;
using Microsoft.CFS.Approvals.Contracts;
using Microsoft.CFS.Approvals.Contracts.DataContracts;
using Microsoft.CFS.Approvals.Core.BL.Interface;
using Microsoft.CFS.Approvals.Extensions;
using Microsoft.CFS.Approvals.LogManager;
using Microsoft.CFS.Approvals.LogManager.Model;
using Microsoft.CFS.Approvals.LogManager.Provider.Interface;
using Microsoft.CFS.Approvals.Model;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json.Linq;

namespace Microsoft.CFS.Approvals.Core.BL.Helpers;

public class InsightsHelper : IInsightsHelper
{
    #region Variables

    /// <summary>
    /// The log provider
    /// </summary>
    protected readonly ILogProvider _logProvider;

    /// <summary>
    /// The performance logger
    /// </summary>
    private readonly IPerformanceLogger _performanceLogger;

    /// <summary>
    /// The configuration
    /// </summary>
    private readonly IConfiguration _config;

    /// <summary>
    /// The Approvals plugin helper
    /// </summary>
    private readonly IApprovalsPluginHelper _approvalsPluginHelper;

    /// <summary>
    /// The history Helper
    /// </summary>
    private readonly IApprovalHistoryProvider _approvalHistoryProvider;

    #endregion Variables

    #region Constructor

    /// <summary>
    /// Constructor
    /// </summary>
    /// <param name="logProvider"></param>
    /// <param name="performanceLogger"></param>
    /// <param name="configuration"></param>
    /// <param name="approvalHistoryHelper"></param>
    /// <param name="approvalsPluginHelper"></param>
    public InsightsHelper(
        ILogProvider logProvider,
        IPerformanceLogger performanceLogger,
        IConfiguration configuration,
        IApprovalsPluginHelper approvalsPluginHelper,
        IApprovalHistoryProvider approvalHistoryProvider)
    {
        _approvalsPluginHelper = approvalsPluginHelper;
        _approvalHistoryProvider = approvalHistoryProvider;
        _logProvider = logProvider;
        _performanceLogger = performanceLogger;
        _config = configuration;
    }

    #endregion Constructor

    /// <summary>
    /// Returns insights on the users current and past records.
    /// </summary>
    /// <param name="signedInUser"></param>
    /// <param name="onBehalfUser"></param>
    /// <param name="oauth2UserToken"></param>
    /// <param name="clientDevice"></param>
    /// <param name="sessionId"></param>
    /// <param name="xcv"></param>
    /// <param name="tenantDocTypeId"></param>
    /// <returns></returns>
    public async Task<JObject> GetSummaryInsights(User signedInUser, User onBehalfUser, string oauth2UserToken, string clientDevice, string sessionId, string xcv, string tenantDocTypeId = "")
    {
        #region Logging

        var tcv = Guid.NewGuid().ToString();
        if (!string.IsNullOrEmpty(sessionId))
        {
            tcv = sessionId;
        }
        var logData = new Dictionary<LogDataKey, object>
        {
            { LogDataKey.StartDateTime, DateTime.UtcNow },
            { LogDataKey.SessionId, sessionId },
            { LogDataKey.Tcv, tcv },
            { LogDataKey.ClientDevice, clientDevice },
            { LogDataKey.UserAlias, onBehalfUser.MailNickname },
            { LogDataKey.ObjectId, onBehalfUser.Id },
            { LogDataKey.Domain, onBehalfUser.UserPrincipalName.GetDomainFromUPN()}
        };

        #endregion Logging

        try {
            //get prioritization criteria
            var prioritizationCriteria = _config[ConfigurationKey.SummaryPrioritizationCriteria.ToString()];
            var userPrompt = $"Using this prioritization criteria: {prioritizationCriteria}, find top 3 requests with the highest priority, and provide the answer as a comma seperated list of only the DocumentNumber values of the top 3 requests";
            var additionalDetails = new Dictionary<string, string>
            {
                { "UserContext", "Summary" },
            };
            //pass data to intelligence helper

            AskRequest askRequest = new AskRequest
            {
                Input = userPrompt,
                AdditionalDetails = additionalDetails,
            };

            var chatCompletion = await _approvalsPluginHelper.GetApprovalsPluginCompletionAsync(signedInUser, onBehalfUser, oauth2UserToken, clientDevice, askRequest, tcv, xcv);
            var completionMessage = chatCompletion.Message;

            return new JObject
            {
                { "HighPriority", completionMessage },
            };
        }
        catch (Exception ex)
        {
            logData.Modify(LogDataKey.EndDateTime, DateTime.UtcNow);
            _logProvider.LogError(TrackingEvent.GetSummaryInsightsFailed, ex, logData);
            throw;
        }
    }

    /// <summary>
    /// Get insights on the user's history data
    /// </summary>
    /// <param name="alias"></param>
    /// <param name="host"></param>
    /// <param name="sessionId"></param>
    /// <param name="approverId">Approver Alias's object Id</param>
    /// <param name="domain">Alias's Domain</param>
    /// <param name="tenantDocTypeId"></param>
    /// <returns></returns>
    public async Task<JObject> GetHistoryInsights(Graph.Models.User onBehalfUser, string host, string sessionId, int timeperiod, string tenantDocTypeId = "")
    {
        #region Logging

        var tcv = Guid.NewGuid().ToString();
        if (!string.IsNullOrEmpty(sessionId))
        {
            tcv = sessionId;
        }
        var logData = new Dictionary<LogDataKey, object>
        {
            { LogDataKey.StartDateTime, DateTime.UtcNow },
            { LogDataKey.SessionId, sessionId },
            { LogDataKey.Tcv, tcv },
            { LogDataKey.ClientDevice, host },
            { LogDataKey.UserAlias, onBehalfUser.MailNickname },
            { LogDataKey.ObjectId, onBehalfUser.Id },
            { LogDataKey.Domain, onBehalfUser.UserPrincipalName.GetDomainFromUPN()}
        };

        #endregion Logging

        try
        {
            //history insights
            var historyTrends = await _approvalHistoryProvider.GetHistoryIntervalCountsforAliasAsync(onBehalfUser.MailNickname, timeperiod, tcv, onBehalfUser.UserPrincipalName.GetDomainFromUPN(), onBehalfUser.Id);
            return new JObject
            {
                { "TotalCounts", historyTrends },
            };
        }
        catch (Exception ex)
        {
            logData.Modify(LogDataKey.EndDateTime, DateTime.UtcNow);
            _logProvider.LogError(TrackingEvent.GetHistoryInsightsFailed, ex, logData);
            throw;
        }
    }

}