// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace Microsoft.CFS.Approvals.Core.BL.Helpers;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.CFS.Approvals.Common.DL.Interface;
using Microsoft.CFS.Approvals.Contracts;
using Microsoft.CFS.Approvals.Contracts.DataContracts;
using Microsoft.CFS.Approvals.Core.BL.Interface;
using Microsoft.CFS.Approvals.Extensions;
using Microsoft.CFS.Approvals.LogManager.Model;
using Microsoft.CFS.Approvals.LogManager.Provider.Interface;
using Microsoft.CFS.Approvals.Utilities.Interface;
using Newtonsoft.Json.Linq;

/// <summary>
/// The Read Details Helper class
/// </summary>
public class ReadDetailsHelper : IReadDetailsHelper
{
    /// <summary>
    /// The log provider
    /// </summary>
    private readonly ILogProvider _logProvider;

    /// <summary>
    /// The tenantInfo helper
    /// </summary>
    private readonly IApprovalTenantInfoHelper _tenantInfoHelper;

    /// <summary>
    /// The summary helper
    /// </summary>
    private readonly ISummaryHelper _summaryHelper;

    /// <summary>
    /// The performance logger
    /// </summary>
    private readonly IPerformanceLogger _performanceLogger;

    /// <summary>
    /// The name resolution helper
    /// </summary>
    private readonly INameResolutionHelper _nameResolutionHelper;

    private readonly IDelegationHelper _delegationHelper;

    /// <summary>
    /// Constructor of ReadDetailsHelper
    /// </summary>
    /// <param name="summaryHelper"></param>
    /// <param name="tenantInfoHelper"></param>
    /// <param name="logProvider"></param>
    /// <param name="performanceLogger"></param>
    public ReadDetailsHelper(ISummaryHelper summaryHelper, IApprovalTenantInfoHelper tenantInfoHelper, ILogProvider logProvider, IPerformanceLogger performanceLogger, INameResolutionHelper nameResolutionHelper, IDelegationHelper delegationHelper)
    {
        _summaryHelper = summaryHelper;
        _tenantInfoHelper = tenantInfoHelper;
        _logProvider = logProvider;
        _performanceLogger = performanceLogger;
        _nameResolutionHelper = nameResolutionHelper;
        _delegationHelper = delegationHelper;
    }

    /// <summary>
    /// Update detail table for IsRead flag
    /// </summary>
    /// <param name="signedInUser"></param>
    /// <param name="onBehalfUser"></param>
    /// <param name="oauth2UserToken"></param>
    /// <param name="postData"></param>
    /// <param name="tenantId"></param>

    /// <param name="clientDevice"></param>
    /// <param name="sessionId"></param>
    /// <param name="Tcv"></param>
    /// <param name="Xcv"></param>
    /// <param name="domain">Alias's domain</param>
    /// <returns></returns>
    public async Task<bool> UpdateIsReadDetails(User signedInUser, User onBehalfUser, string oauth2UserToken, string postData, int tenantId, string clientDevice, string sessionId, string Tcv, string Xcv, string domain)
    {
        // Create a unique GUID per user transaction
        var activityId = Guid.NewGuid().ToString();

        #region Logging Prep

        if (string.IsNullOrEmpty(sessionId))
        {
            sessionId = Guid.NewGuid().ToString();
        }
        if (string.IsNullOrEmpty(Tcv))
        {
            Tcv = Guid.NewGuid().ToString();
        }
        if (string.IsNullOrEmpty(Xcv))
        {
            Xcv = Tcv;
        }

        // TODO:: Add DXcv, DocumentNumber, FiscalYear to logData
        var logData = new Dictionary<LogDataKey, object>
        {
            {LogDataKey.Tcv, Tcv},
            {LogDataKey.EventType, Constants.FeatureUsageEvent},
            {LogDataKey.SessionId, sessionId},
            {LogDataKey.Xcv, Xcv},
            {LogDataKey.UserRoleName, signedInUser.MailNickname},
            {LogDataKey.TenantId, tenantId},
            {LogDataKey.Approver, onBehalfUser.MailNickname},
            {LogDataKey._ActivityId, activityId},
            {LogDataKey.UserAlias, onBehalfUser.MailNickname},
            {LogDataKey.IsCriticalEvent, CriticalityLevel.No.ToString()},
            {LogDataKey.StartDateTime, DateTime.UtcNow },
            {LogDataKey.ClientDevice, clientDevice}
        };

        #endregion Logging Prep

        try
        {
            using (_performanceLogger.StartPerformanceLogger("PerfLog", Constants.WebClient, string.Format(Constants.PerfLogAction, "ReadDetailsHelper", "UpdatesIsReadDetails"), logData))
            {
                await _delegationHelper.CheckUserAuthorization(signedInUser, onBehalfUser, oauth2UserToken, clientDevice, sessionId, Xcv, Tcv);

                #region Fetch Object Id and Domain for alias

                string approverDomain = string.IsNullOrWhiteSpace(domain) ? _nameResolutionHelper.GetUserPrincipalName(onBehalfUser.MailNickname).Result.GetDomainFromUPN() : domain;
                string approverId = onBehalfUser.Id;
                if (string.IsNullOrWhiteSpace(onBehalfUser.Id) && !string.IsNullOrWhiteSpace(approverDomain))
                    approverId = _nameResolutionHelper.GetUser(onBehalfUser.MailNickname + approverDomain).Result?.Id;

                #endregion Fetch Object Id and Domain for alias

                #region Get the user action string from request content and validate it

                // Getting the user action string from request content
                string userActionsString = postData;
                // Unknown error
                if (string.IsNullOrEmpty(userActionsString))
                {
                    throw new ArgumentNullException("Error no details to be read");
                }
                JObject actionObject = (userActionsString).ToJObject();

                // Set clientdevice = OUTLOOK if action is initiated from outlook
                if (userActionsString.IsJson())
                {
                    if (actionObject["ClientType"] != null
                        && !string.IsNullOrEmpty(actionObject["ClientType"].ToString()))
                    {
                        clientDevice = actionObject["ClientType"].ToString();
                    }
                    if (actionObject["ClientDevice"] == null)
                    {
                        actionObject["ClientDevice"] = clientDevice;
                    }
                }

                #endregion Get the user action string from request content and validate it

                #region Get Tenant Info

                var tenantInfo = _tenantInfoHelper.GetTenantInfo(tenantId);

                #endregion Get Tenant Info

                await _summaryHelper.UpdateSummary(tenantInfo, actionObject["DocumentKeys"].ToString(), onBehalfUser.MailNickname, approverId, approverDomain, DateTime.UtcNow, Constants.IsRead);
                logData.Modify(LogDataKey.EndDateTime, DateTime.UtcNow);
                _logProvider.LogInformation(TrackingEvent.WebApiReadDetailsSuccess, logData);
            }
        }
        catch (Exception exception)
        {
            logData.Modify(LogDataKey.EndDateTime, DateTime.UtcNow);
            _logProvider.LogError(TrackingEvent.WebApiReadDetailsFail, exception, logData);
            throw;
        }
        return true;
    }
}