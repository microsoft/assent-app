// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace Microsoft.CFS.Approvals.Core.BL.Helpers;

using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using global::Azure.Messaging.ServiceBus;
using Microsoft.CFS.Approvals.Common.DL.Interface;
using Microsoft.CFS.Approvals.Contracts;
using Microsoft.CFS.Approvals.Contracts.DataContracts;
using Microsoft.CFS.Approvals.Core.BL.Interface;
using Microsoft.CFS.Approvals.Data.Azure.Storage.Interface;
using Microsoft.CFS.Approvals.Domain.BL.Interface;
using Microsoft.CFS.Approvals.Extensions;
using Microsoft.CFS.Approvals.LogManager.Model;
using Microsoft.CFS.Approvals.LogManager.Provider.Interface;
using Microsoft.CFS.Approvals.Model;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Constants = Contracts.Constants;

/// <summary>
/// The Summary Helper class
/// </summary>
public class SummaryHelper : ISummaryHelper
{
    #region Private Variables

    /// <summary>
    /// The configuration
    /// </summary>
    protected readonly IConfiguration _config;

    /// <summary>
    /// The approval tenantInfo helper
    /// </summary>
    protected readonly IApprovalTenantInfoHelper _approvalTenantInfoHelper;

    /// <summary>
    /// The delegation helper
    /// </summary>
    protected readonly IDelegationHelper _delegationHelper;

    /// <summary>
    /// The log provider
    /// </summary>
    protected readonly ILogProvider _logProvider;

    /// <summary>
    /// The performance logger
    /// </summary>
    private readonly IPerformanceLogger _performanceLogger;

    /// <summary>
    /// The approval summary provider
    /// </summary>
    private readonly IApprovalSummaryProvider _approvalSummaryProvider;

    /// <summary>
    /// The flighting data provider
    /// </summary>
    private readonly IFlightingDataProvider _flightingDataProvider;

    #endregion Private Variables

    #region Constructor

    /// <summary>
    /// Constructor of SummaryHelper
    /// </summary>
    /// <param name="config"></param>
    /// <param name="approvalTenantInfoHelper"></param>
    /// <param name="delegationHelper"></param>
    /// <param name="logProvider"></param>
    /// <param name="performanceLogger"></param>
    /// <param name="approvalSummaryProvider"></param>
    public SummaryHelper(
        IConfiguration config,
        IApprovalTenantInfoHelper approvalTenantInfoHelper,
        IDelegationHelper delegationHelper,
        ILogProvider logProvider,
        IPerformanceLogger performanceLogger,
        IApprovalSummaryProvider approvalSummaryProvider,
        IFlightingDataProvider flightingDataProvider)
    {
        _config = config;
        _approvalTenantInfoHelper = approvalTenantInfoHelper;
        _delegationHelper = delegationHelper;
        _logProvider = logProvider;
        _performanceLogger = performanceLogger;
        _approvalSummaryProvider = approvalSummaryProvider;
        _flightingDataProvider = flightingDataProvider;
    }

    #endregion Constructor

    #region CREATE

    /// <summary>
    /// Add approval summary
    /// </summary>
    /// <param name="tenant"></param>
    /// <param name="approvalRequest"></param>
    /// <param name="summaryRows"></param>
    /// <returns></returns>
    public async Task<bool> AddApprovalSummary(ApprovalTenantInfo tenant, ApprovalRequestExpression approvalRequest, List<ApprovalSummaryRow> summaryRows)
    {
        return await _approvalSummaryProvider.AddApprovalSummary(tenant, approvalRequest, summaryRows);
    }

    /// <summary>
    /// Creates the summary data list.
    /// </summary>
    /// <param name="approvalsData">The approvals data.</param>
    /// <param name="tenants">The tenants.</param>
    /// <param name="checkTenantUserDelegationEnable">if set to <c>true</c> [check tenant user delegation enable].</param>
    /// <returns>returns list containing Approval Summary Data</returns>
    public List<ApprovalSummaryData> CreateSummaryDataList(List<ApprovalSummaryData> approvalsData, List<ApprovalTenantInfo> tenants, bool checkTenantUserDelegationEnable = false)
    {
        string AdaptiveTemplateUrl = _config[ConfigurationKey.ApprovalsCoreServicesURL.ToString()] + "/api/v1/AdaptiveDetail/{0}";
        string DataUrl = _config[ConfigurationKey.ApprovalsCoreServicesURL.ToString()] + "/api/v1/Detail/{0}/{1}";

        //if checkTenantUserDelegationEnable == true, then apply the "EnbaleUserDelegation" filter. Otherwise, ignore the filter by doing this: (t.EnableUserDelegation == t.EnableUserDelegation)
        var allTenantInfo = (from t in tenants
                             where checkTenantUserDelegationEnable ? t.EnableUserDelegation == true : t.EnableUserDelegation == t.EnableUserDelegation
                             select new
                             {
                                 TenantID = t.TenantId,
                                 t.AppName,
                                 t.DocumentNumberPrefix,
                                 DetailOperations = (t.DetailOperations == null ? new List<TenOpsDetails>() : t.DetailOperations.DetailOpsList),
                                 t.DocTypeId,
                                 t.TemplateName,
                                 t.BusinessProcessName,
                                 t.IsBackgroundProcessingEnabledUpfront,
                                 t.IsControlsAndComplianceRequired,
                                 t?.AllowBulkApprovalCondition
                             }).ToList();
        var summaryData = new List<ApprovalSummaryData>();
        object lockObject = new();
        Parallel.ForEach(approvalsData, approvalData =>
        {
            var tenantInfo =
                    allTenantInfo.FirstOrDefault(
                          t => t.DocTypeId.Equals(approvalData.DocumentTypeId, StringComparison.OrdinalIgnoreCase));

            if (tenantInfo != null)
            {
                var json = (approvalData.SummaryJson).ToJObject();
                json.Add("TenantId", tenantInfo.TenantID);
                json.Add("AppName", tenantInfo.AppName);
                json.Add("CondensedAppName", tenantInfo.AppName.Replace(" ", string.Empty));
                json.Add("TemplateName", tenantInfo.TemplateName);
                json.Add("BusinessProcessName", tenantInfo.BusinessProcessName);
                json.Add("IsBackgroundApprovalSupportedUpfront", tenantInfo.IsBackgroundProcessingEnabledUpfront);
                json.Add("IsControlsAndComplianceRequired", tenantInfo.IsControlsAndComplianceRequired);
                json.Add("AllowBulkApprovalCondition", tenantInfo?.AllowBulkApprovalCondition);

                var approvalDataJson = (approvalData.ToJson()).ToJObject();

                var propertyDiff = from prop in approvalDataJson.Properties()
                                   join prop1 in json.Properties() on prop.Name equals prop1.Name into propJoin
                                   from propJoinVal in propJoin.DefaultIfEmpty()
                                   where propJoinVal == null
                                   select prop;

                foreach (var prop in propertyDiff)
                {
                    json.Add(prop.Name, prop.Value);
                }

                if (json["ApprovalIdentifier"] != null)
                    json["ApprovalIdentifier"]["DocumentNumberPrefix"] = tenantInfo.DocumentNumberPrefix;
                json.Remove("Approver");
                json.Remove("SummaryJson");
                lock (lockObject)
                {
                    try
                    {
                        var approvalSummaryData = json.ToString().FromJson<ApprovalSummaryData>(new JsonSerializerSettings
                        {
                            NullValueHandling = NullValueHandling.Ignore
                        });

                        if (approvalSummaryData.AdditionalData != null)
                        {
                            approvalSummaryData.AdditionalData.Add("TemplateUri", string.Format(AdaptiveTemplateUrl, tenantInfo.TenantID));
                            approvalSummaryData.AdditionalData.Add("DetailsUri", string.Format(DataUrl, tenantInfo.TenantID, approvalData.DocumentNumber));
                        }
                        else
                        {
                            approvalSummaryData.AdditionalData = new Dictionary<string, string>() {
                                { "TemplateUri", string.Format(AdaptiveTemplateUrl, tenantInfo.TenantID) },
                                { "DetailsUri", string.Format(DataUrl, tenantInfo.TenantID, approvalData.DocumentNumber) }};
                        }

                        summaryData.Add(approvalSummaryData);
                    }
                    catch (Exception ex)
                    {
                        _logProvider.LogError<TrackingEvent, LogDataKey>(TrackingEvent.WebApiCreateSummaryDataListFail, ex);
                    }
                }
            }
        });

        return summaryData;
    }

    #endregion CREATE

    #region READ

    /// <summary>
    /// Get other summary requests
    /// </summary>
    /// <param name="signedInUser"></param>
    /// <param name="onBehalfUser"></param>
    /// <param name="host"></param>
    /// <param name="viewType"></param>
    /// <param name="sessionId"></param>
    /// <param name="approverId">Approver Alias's object Id</param>
    /// <param name="domain">Alias's Domain</param>
    /// <param name="oauth2UserToken"></param>
    /// <param name="tenantDocTypeId"></param>
    /// <returns></returns>
    public async Task<JArray> GetOtherSummaryRequests(User signedInUser, User onBehalfUser, string host, string viewType, string sessionId, string approverId, string domain, string oauth2UserToken, string tenantDocTypeId = "")
    {
        #region Logging

        var Tcv = Guid.NewGuid().ToString();
        if (!string.IsNullOrEmpty(sessionId))
        {
            Tcv = sessionId;
        }
        var logData = new Dictionary<LogDataKey, object>
        {
            { LogDataKey.Xcv, Tcv },
            { LogDataKey.Tcv, Tcv },
            { LogDataKey.SessionId, Tcv },
            { LogDataKey.UserRoleName, signedInUser.UserPrincipalName },
            { LogDataKey.ClientDevice, host },
            { LogDataKey.EventType, Constants.FeatureUsageEvent },
            { LogDataKey.UserAlias, onBehalfUser.UserPrincipalName },
            { LogDataKey.StartDateTime, DateTime.UtcNow },
            { LogDataKey.IsCriticalEvent, CriticalityLevel.No.ToString() }
        };

        #endregion Logging

        try
        {
            string actionName = string.Empty;
            actionName = string.IsNullOrEmpty(tenantDocTypeId) ? "Other Summary" : "Get Other Summary By Id";
            using (var summaryTracer = _performanceLogger.StartPerformanceLogger("PerfLog", string.IsNullOrWhiteSpace(host) ? Constants.WebClient : host, string.Format(Constants.PerfLogCommon, actionName), logData))
            {
                // Check if Host is null or empty and throw back an exception which will get returned as bad request to caller
                if (string.IsNullOrEmpty(host))
                {
                    throw new InvalidOperationException(_config[ConfigurationKey.Message_NoTenantForDevice.ToString()]);
                }

                List<ApprovalTenantInfo> tenants = new List<ApprovalTenantInfo>();
                if (string.IsNullOrEmpty(tenantDocTypeId))
                {
                    // Get Tenant Info by Host
                    tenants = _approvalTenantInfoHelper.GetTenantInfoByHost(host).ToList();
                }
                else
                {
                    // Get Tenant Info by Host
                    tenants = (await _approvalTenantInfoHelper.GetTenants(false)).ToList().Where(t => t.DocTypeId.Equals(tenantDocTypeId)).ToList();
                }

                // If tenant info cannot be read, summary cannot be fetched and hence throw an exception
                if (tenants.Count == 0)
                {
                    throw new InvalidOperationException(_config[ConfigurationKey.Message_NoTenantForDevice.ToString()]);
                }

                logData.Add(LogDataKey.Tenants, String.Join(", ", tenants.Select(t => t.TenantId.ToString(CultureInfo.InvariantCulture)).ToArray()));

                // Get the Summary Json by Approver and Tenants so that can then be modified into final form adding in Tenant Metadata
                var approvalsData = GetApprovalSummaryJsonByApproverAndTenants(onBehalfUser.MailNickname, tenants, approverId, domain, viewType);

                bool checkEnableUserDelegationFlag = false;

                await _delegationHelper.CheckUserAuthorization(signedInUser, onBehalfUser, oauth2UserToken, host, sessionId, Tcv, Tcv);

                // Get final summary data
                var summaryData = CreateSummaryDataList(approvalsData, tenants, checkEnableUserDelegationFlag);

                // Change the IsRead flag to true for all requests which have failed
                summaryData.ForEach(x => x.IsRead = (x.LastFailed == true || x.IsRead));

                // Serialize
                var serializedSummaryData = (summaryData.ToJson()).ToJArray();

                // Log Success
                logData.Modify(LogDataKey.EndDateTime, DateTime.UtcNow);
                _logProvider.LogInformation(TrackingEvent.WebApiSummarySuccess, logData);

                return serializedSummaryData;
            }
        }
        catch (Exception ex)
        {
            logData.Modify(LogDataKey.EndDateTime, DateTime.UtcNow);
            if (string.IsNullOrEmpty(tenantDocTypeId))
                _logProvider.LogError(TrackingEvent.WebApiSummaryFail, ex, logData);
            else
                _logProvider.LogError(TrackingEvent.WebApiSummaryByIdFail, ex, logData);
            throw;
        }
    }

    /// <summary>
    /// Get other summary requests count data
    /// </summary>
    /// <param name="tenantDocTypeId"></param>
    /// <param name="loggedInAlias"></param>
    /// <param name="userAlias"></param>
    /// <param name="viewType"></param>
    /// <param name="sessionId"></param>
    /// <param name="clientDevice"></param>
    /// <param name="approverId">Approver Alias's object Id</param>
    /// <param name="domain">Alias's Domain</param>
    /// <returns></returns>
    public async Task<JArray> GetOtherSummaryRequestsCountData(string tenantDocTypeId, string loggedInAlias, string userAlias, string viewType, string sessionId, string clientDevice, string approverId, string domain)
    {
        #region Logging

        var Tcv = Guid.NewGuid().ToString();
        if (!string.IsNullOrEmpty(sessionId))
        {
            Tcv = sessionId;
        }
        var logData = new Dictionary<LogDataKey, object>
        {
            // Add common data items to LogData
            { LogDataKey.Xcv, Tcv },
            { LogDataKey.Tcv, Tcv },
            { LogDataKey.SessionId, Tcv },
            { LogDataKey.UserRoleName, loggedInAlias },
            { LogDataKey.ClientDevice, clientDevice },
            { LogDataKey.EventType, Constants.FeatureUsageEvent },
            { LogDataKey.UserAlias, userAlias },
            { LogDataKey.DocumentTypeId, tenantDocTypeId },
            { LogDataKey.StartDateTime, DateTime.UtcNow },
            { LogDataKey.IsCriticalEvent, CriticalityLevel.No.ToString() }
        };

        #endregion Logging

        try
        {
            string actionName = string.Empty;
            actionName = string.IsNullOrEmpty(tenantDocTypeId) ? "Get Other Summary Count for Approver" : "Get Other Summary Count for Approver and Tenant";
            using (var summaryCountTracer = _performanceLogger.StartPerformanceLogger("PerfLog", string.IsNullOrWhiteSpace(clientDevice) ? Constants.WebClient : clientDevice, string.Format(Constants.PerfLogCommon, actionName), logData))
            {
                var tenants = await _approvalTenantInfoHelper.GetTenants(false);
                if (string.IsNullOrEmpty(tenantDocTypeId))
                {
                    tenants = tenants.ToList();
                    logData.Add(LogDataKey.Tenants, String.Join(", ", tenants.Select(t => t.TenantId.ToString(CultureInfo.InvariantCulture)).ToArray()));
                }
                else
                {
                    #region Validations

                    Guid documentTypeID;
                    //TODO:: Do we need to check if the input is a valid GUID ?
                    if (!Guid.TryParse(tenantDocTypeId, out documentTypeID))
                    {
                        throw new InvalidDataException("Invalid DocumentTypeID");
                    }

                    #endregion Validations

                    tenants = tenants.Where(x => x.DocTypeId.Equals(Guid.Parse(tenantDocTypeId).ToString())).ToList();
                }

                // If tenant info cannot be read, summary cannot be fetched and hence throw an exception
                if (tenants == null || tenants.Count == 0)
                {
                    if (!string.IsNullOrEmpty(tenantDocTypeId))
                        throw new InvalidDataException("Tenant DocumentTypeID not present in Approvals");
                    else
                        throw new InvalidOperationException(_config[ConfigurationKey.Message_NoTenantForDevice.ToString()]);
                }

                var approvalsData = GetApprovalSummaryCountJsonByApproverAndTenants(userAlias, tenants, approverId, domain, viewType);

                logData.Add(LogDataKey.ApprovalCountResults, approvalsData);
                logData.Modify(LogDataKey.EndDateTime, DateTime.UtcNow);
                if (string.IsNullOrEmpty(tenantDocTypeId))
                    _logProvider.LogInformation(TrackingEvent.WebApiSummaryCountSuccess, logData);
                else
                    _logProvider.LogInformation(TrackingEvent.WebApiTenantWiseSummaryCountSuccess, logData);

                return approvalsData;
            }
        }
        catch (Exception ex)
        {
            logData.Modify(LogDataKey.EndDateTime, DateTime.UtcNow);
            if (string.IsNullOrEmpty(tenantDocTypeId))
                _logProvider.LogError(TrackingEvent.WebApiSummaryCountFail, ex, logData);
            else
                _logProvider.LogError(TrackingEvent.WebApiTenantWiseSummaryCountFail, ex, logData);
            throw;
        }
    }

    /// <summary>
    /// Get summary
    /// </summary>
    /// <param name="signedInUser"></param>
    /// <param name="onBehalfUser"></param>
    /// <param name="host"></param>
    /// <param name="sessionId"></param>
    /// <param name="approverId">Approver Alias's object Id</param>
    /// <param name="domain">Alias's Domain</param>
    /// <param name="oauth2UserToken"></param>
    /// <param name="tenantDocTypeId"></param>
    /// <param name="isSubmittedRequest">flag to get submitted requests</param>
    /// <returns></returns>
    public async Task<JArray> GetSummary(User signedInUser, User onBehalfUser, string host, string sessionId, string oauth2UserToken, string tenantDocTypeId = "", bool isSubmittedRequest = false)
    {
        #region Logging

        var Tcv = Guid.NewGuid().ToString();
        if (!string.IsNullOrEmpty(sessionId))
        {
            Tcv = sessionId;
        }
        var logData = new Dictionary<LogDataKey, object>()
        {
            { LogDataKey.StartDateTime, DateTime.UtcNow },
            { LogDataKey.Xcv, Tcv },
            { LogDataKey.Tcv, Tcv },
            { LogDataKey.SessionId, Tcv },
            { LogDataKey.UserRoleName, signedInUser.UserPrincipalName },
            { LogDataKey.ClientDevice, host },
            { LogDataKey.EventType, Constants.FeatureUsageEvent },
            { LogDataKey.UserAlias, onBehalfUser.UserPrincipalName },
            { LogDataKey.IsCriticalEvent, CriticalityLevel.Yes.ToString() },
            { LogDataKey.ObjectId, onBehalfUser.Id },
            { LogDataKey.Domain, onBehalfUser.UserPrincipalName.GetDomainFromUPN()},
            { LogDataKey.IsSubmittedRequest, isSubmittedRequest}
        };

        #endregion Logging

        try
        {
            using (_performanceLogger.StartPerformanceLogger("PerfLog", Constants.WebClient, string.Format(Constants.PerfLogAction, "SummaryHelper", "GetSummary"), logData))
            {
                // Check if Host is null or empty and throw back an exception which will get returned as bad request to caller
                if (string.IsNullOrEmpty(host))
                {
                    throw new InvalidOperationException(_config[ConfigurationKey.Message_NoTenantForDevice.ToString()]);
                }

                List<ApprovalTenantInfo> tenants = new List<ApprovalTenantInfo>();
                if (string.IsNullOrEmpty(tenantDocTypeId))
                {
                    // Get Tenant Info by Host
                    tenants = _approvalTenantInfoHelper.GetTenantInfoByHost(host).ToList();
                }
                else
                {
                    // Get Tenant Info by Host
                    tenants = (await _approvalTenantInfoHelper.GetTenants(false)).ToList().Where(t => t.DocTypeId.Equals(tenantDocTypeId)).ToList();
                }

                // If tenant info cannot be read, summary cannot be fetched and hence throw an exception
                if (tenants.Count == 0)
                {
                    throw new InvalidOperationException(_config[ConfigurationKey.Message_NoTenantForDevice.ToString()]);
                }
                logData.Add(LogDataKey.Tenants, string.Join(", ", tenants.Select(t => t.TenantId.ToString(CultureInfo.InvariantCulture)).ToArray()));

                List<Guid> allowedTenantIds = new List<Guid>();
                var delegateUsers = (await _delegationHelper.GetUserDelegation(signedInUser, onBehalfUser, oauth2UserToken, host, sessionId, Tcv, Tcv))?.Where(t => t.Delegator.UserPrincipalName.Equals(onBehalfUser.UserPrincipalName));
                var onBehalfUserInfo = _delegationHelper.GetUserDelegationsForCurrentUser(onBehalfUser)?.FirstOrDefault(d => d.DelegateUpn == signedInUser.UserPrincipalName && d.IsHidden == true);
                if (delegateUsers != null && delegateUsers.Count() > 0)
                {
                    try
                    {
                        if (onBehalfUserInfo == null || !onBehalfUserInfo.DelegatorUpn.Equals(delegateUsers.FirstOrDefault().Delegator.UserPrincipalName, StringComparison.InvariantCultureIgnoreCase))
                        {
                            allowedTenantIds = delegateUsers
                                .Select(t => Guid.TryParse(t.AppId, out var appId) ? appId : Guid.Empty)
                                .Where(guid => guid != Guid.Empty)
                                .ToList();
                        }
                    }
                    catch (Exception ex)
                    {
                        throw new InvalidOperationException("Invalid serialized tenant data format.", ex);
                    }
                }

                if (allowedTenantIds.Any())
                {
                    // Filter data before fetching
                    tenants = tenants
                        .Where(t => Guid.TryParse(t.DocTypeId, out var docTypeId) && allowedTenantIds.Contains(docTypeId))
                        .ToList();
                }

                // Get the Summary Json by Approver and Tenants so that can then be modified into final form adding in Tenant Metadata
                List<ApprovalSummaryData> approvalsData;

                // Check if the Submitter View flighting feature is enabled for the signed-in user
                using (_performanceLogger.StartPerformanceLogger("PerfLog", Constants.WebClient, string.Format(Constants.PerfLogAction, "SummaryHelper", "SubmitterView flighting check and data fetch"), logData))
                {
                    if (isSubmittedRequest && !signedInUser.UserPrincipalName.Equals(onBehalfUser.UserPrincipalName, StringComparison.OrdinalIgnoreCase))
                    {
                        throw new UnauthorizedAccessException("Submitter view is not available in delegation mode.");
                    }

                    if (isSubmittedRequest && !_flightingDataProvider.IsFeatureEnabledForUser(signedInUser.UserPrincipalName, (int)FlightingFeatureName.SubmitterView))
                    {
                        logData.Modify(LogDataKey.IsSubmittedRequest, false);
                        _logProvider.LogWarning(TrackingEvent.SubmitterViewFlightingDisabled, logData);
                        isSubmittedRequest = false;
                    }
                }
                
                if (isSubmittedRequest)
                {
                    using (_performanceLogger.StartPerformanceLogger("PerfLog", Constants.WebClient, string.Format(Constants.PerfLogAction, "SummaryHelper", "GetSubmittedRequestsSummary"), logData))
                    {
                        approvalsData = GetApprovalSummaryJsonByApproverAndTenants(onBehalfUser.MailNickname, tenants, signedInUser.Id, signedInUser.UserPrincipalName.GetDomainFromUPN(), "Summary", true);
                    }
                }
                else
                {
                    approvalsData = GetApprovalSummaryJsonByApproverAndTenants(onBehalfUser.MailNickname, tenants, onBehalfUser.Id, onBehalfUser.UserPrincipalName.GetDomainFromUPN());
                }

                bool checkEnableUserDelegationFlag = false;

                //if loggedInAlias != current alias (Alias), that mean current user is in delegate mode. So enable the UserDelegation flag checking when creating the Summary list.
                if (!signedInUser.UserPrincipalName.Equals(onBehalfUser.UserPrincipalName, StringComparison.OrdinalIgnoreCase))
                {
                    // check if entry made from support portal
                    if (onBehalfUserInfo == null &&
                        (delegateUsers == null || delegateUsers.Count() == 0))
                    {
                        throw new UnauthorizedAccessException("User doesn't have permission to see the request.");
                    }
                }

                // Get final summary data
                var summaryData = CreateSummaryDataList(approvalsData, tenants, checkEnableUserDelegationFlag);

                // Change the IsRead flag to true for all requests which have failed
                summaryData.ForEach(x => x.IsRead = (x.LastFailed == true || x.IsRead));

                // Serialize
                var serializedSummaryData = (summaryData.ToJson()).ToJArray();

                // Log Success
                logData.Modify(LogDataKey.EndDateTime, DateTime.UtcNow);
                logData.Add(LogDataKey.SummaryCount, summaryData.Count);
                if (isSubmittedRequest)
                {
                    logData.Modify(LogDataKey.IsSubmittedRequest, true);
                    _logProvider.LogInformation(TrackingEvent.WebApiSubmitterViewSummarySuccess, logData);
                }
                _logProvider.LogInformation(TrackingEvent.WebApiSummarySuccess, logData);

                return serializedSummaryData;
            }
        }
        catch (Exception ex)
        {
            logData.Modify(LogDataKey.EndDateTime, DateTime.UtcNow);
            if (isSubmittedRequest)
            {
                logData.Modify(LogDataKey.IsSubmittedRequest, true);
                _logProvider.LogError(TrackingEvent.WebApiSubmitterViewSummaryFail, ex, logData);
            }
            if (string.IsNullOrEmpty(tenantDocTypeId))
                _logProvider.LogError(TrackingEvent.WebApiSummaryFail, ex, logData);
            else
                _logProvider.LogError(TrackingEvent.WebApiSummaryByIdFail, ex, logData);
            throw;
        }
    }

    /// <summary>
    /// Get summary count data
    /// </summary>
    /// <param name="signedInUser"></param>
    /// <param name="onBehalfUser"></param>
    /// <param name="tenantDocTypeId"></param>
    /// <param name="sessionId"></param>
    /// <param name="clientDevice"></param>
    /// <param name="domain">Alias's Domain</param>
    /// <param name="oauth2UserToken"></param>
    /// <returns></returns>
    public async Task<JArray> GetSummaryCountData(User signedInUser, User onBehalfUser, string tenantDocTypeId, string sessionId, string clientDevice, string domain, string oauth2UserToken)
    {
        #region Logging

        var Tcv = Guid.NewGuid().ToString();
        if (!string.IsNullOrEmpty(sessionId))
        {
            Tcv = sessionId;
        }
        var logData = new Dictionary<LogDataKey, object>
        {
            // Add common data items to LogData
            { LogDataKey.Xcv, Tcv },
            { LogDataKey.Tcv, Tcv },
            { LogDataKey.SessionId, Tcv },
            { LogDataKey.UserRoleName, signedInUser.MailNickname },
            { LogDataKey.ClientDevice, clientDevice },
            { LogDataKey.EventType, Constants.FeatureUsageEvent },
            { LogDataKey.UserAlias, onBehalfUser.MailNickname },
            { LogDataKey.IsCriticalEvent, CriticalityLevel.No.ToString() },
            { LogDataKey.StartDateTime, DateTime.UtcNow },
            { LogDataKey.DocumentTypeId, tenantDocTypeId }
        };

        #endregion Logging

        try
        {
            string actionName = string.Empty;
            actionName = string.IsNullOrEmpty(tenantDocTypeId) ? "Get Summary Count for Approver" : "Get Summary Count for Approver and Tenant";
            using (var summaryTracer = _performanceLogger.StartPerformanceLogger("PerfLog", string.IsNullOrWhiteSpace(clientDevice) ? Constants.WebClient : clientDevice, string.Format(Constants.PerfLogCommon, actionName), logData))
            {
                await _delegationHelper.CheckUserAuthorization(signedInUser, onBehalfUser, oauth2UserToken, clientDevice, sessionId, Tcv, Tcv);

                var tenants = await _approvalTenantInfoHelper.GetTenants(false);
                if (string.IsNullOrEmpty(tenantDocTypeId))
                {
                    tenants = tenants.ToList();
                    logData.Add(LogDataKey.Tenants, String.Join(", ", tenants.Select(t => t.TenantId.ToString(CultureInfo.InvariantCulture)).ToArray()));
                }
                else
                {
                    #region Validations

                    // TODO:: Do we need to check if the input is a valid GUID ?
                    if (!Guid.TryParse(tenantDocTypeId, out Guid documentTypeID))
                    {
                        throw new InvalidDataException("Invalid DocumentTypeID");
                    }

                    #endregion Validations

                    tenants = tenants.Where(x => x.DocTypeId.Equals(Guid.Parse(tenantDocTypeId).ToString())).ToList();
                }

                // If tenant info cannot be read, summary cannot be fetched and hence throw an exception
                if (tenants == null || tenants.Count == 0)
                {
                    if (!string.IsNullOrEmpty(tenantDocTypeId))
                        throw new InvalidDataException("Tenant DocumentTypeID not present in Approvals");
                    else
                        throw new InvalidOperationException(_config[ConfigurationKey.Message_NoTenantForDevice.ToString()]);
                }

                var approvalsData = GetApprovalSummaryCountJsonByApproverAndTenants(onBehalfUser.MailNickname, tenants, onBehalfUser.Id, domain);

                logData.Add(LogDataKey.ApprovalCountResults, approvalsData);
                logData.Modify(LogDataKey.EndDateTime, DateTime.UtcNow);
                if (string.IsNullOrEmpty(tenantDocTypeId))
                    _logProvider.LogInformation(TrackingEvent.WebApiSummaryCountSuccess, logData);
                else
                    _logProvider.LogInformation(TrackingEvent.WebApiTenantWiseSummaryCountSuccess, logData);

                return approvalsData;
            }
        }
        catch (Exception ex)
        {
            logData.Modify(LogDataKey.EndDateTime, DateTime.UtcNow);
            if (string.IsNullOrEmpty(tenantDocTypeId))
                _logProvider.LogError(TrackingEvent.WebApiSummaryCountFail, ex, logData);
            else
                _logProvider.LogError(TrackingEvent.WebApiTenantWiseSummaryCountFail, ex, logData);
            throw;
        }
    }

    /// <summary>
    /// Get summary data
    /// </summary>
    /// <param name="documentNumber"></param>
    /// <param name="fiscalYear"></param>
    /// <param name="tenantInfo"></param>
    /// <param name="alias"></param>
    /// <param name="approverId"></param>
    /// <param name="approverDomain"></param>
    /// <param name="loggedInAlias"></param>
    /// <param name="xcv"></param>
    /// <param name="tcv"></param>
    /// <param name="tenantAdaptor"></param>
    /// <returns></returns>
    public ApprovalSummaryRow GetSummaryData(string documentNumber, string fiscalYear, ApprovalTenantInfo tenantInfo, string alias, string approverId, string approverDomain, string loggedInAlias, string xcv, string tcv, ITenant tenantAdaptor)
    {
        #region Logging

        var logData = new Dictionary<LogDataKey, object>
        {
            { LogDataKey.Tcv, tcv },
            { LogDataKey.Xcv, xcv },
            { LogDataKey.DocumentNumber, documentNumber },
            { LogDataKey.DXcv, documentNumber },
            { LogDataKey.FiscalYear, fiscalYear },
            { LogDataKey.UserRoleName, loggedInAlias },
            { LogDataKey.UserAlias, alias },
            { LogDataKey.TenantId, tenantInfo.TenantId }
        };

        #endregion Logging

        ApprovalSummaryRow documentSummary = null;
        using (var trace1 = _performanceLogger.StartPerformanceLogger("PerfLog", Constants.WebClient, string.Format(Constants.PerfLogAction, tenantInfo.AppName, "Get summary data for current approver"), logData))
        {
            documentSummary = tenantAdaptor.GetApprovalSummaryByRowKeyAndApprover(documentNumber, alias, approverId, approverDomain, fiscalYear, tenantInfo);
        }
        if (documentSummary != null)
        {
            logData.Add(LogDataKey.ReceivedTcv, documentSummary.Tcv);
        }
        return documentSummary;
    }

    /// <summary>
    /// Get ApprovalSummaryCountJson By Approver for multiple tenants
    /// </summary>
    /// <param name="approver">approver alias</param>
    /// <param name="tenants">List<ApprovalTenantInfo> - list of tenants</param>
    /// <param name="approverId">Approver Alias's object Id</param>
    /// <param name="domain">Alias's Domain</param>
    /// <param name="viewType">Default value 'Summary'</param>
    /// <returns>JArray- with fields Id, tenantName and count</returns>
    public JArray GetApprovalSummaryCountJsonByApproverAndTenants(string approver, List<ApprovalTenantInfo> tenants, string approverId, string domain, string viewType = "Summary")
    {
        IEnumerable<ApprovalSummaryRow> jsonSummary = _approvalSummaryProvider.GetApprovalSummaryCountJsonByApproverAndTenants(approver, tenants, approverId, domain);

        List<ApprovalSummaryRow> filteredRowKeys = new List<ApprovalSummaryRow>();
        JArray summaryCount = new JArray();
        switch (viewType)
        {
            case Constants.OutOfSyncAction:
                foreach (var tenant in tenants)
                {
                    JObject jSummaryTenantCountData = new JObject();
                    filteredRowKeys = jsonSummary.Where(l => l.LobPending == false && l.IsOutOfSyncChallenged == true && tenant.DocTypeId == l.RowKey.Split('|').FirstOrDefault()).ToList();
                    jSummaryTenantCountData.Add("ID", tenant.DocTypeId);
                    jSummaryTenantCountData.Add("TenantName", tenant.AppName);
                    jSummaryTenantCountData.Add("Count", filteredRowKeys.Count);
                    if (filteredRowKeys.Count > 0)
                        summaryCount.Add(jSummaryTenantCountData);
                }
                break;

            case Constants.OfflineApproval:
                foreach (var tenant in tenants)
                {
                    JObject jSummaryTenantCountData = new JObject();
                    filteredRowKeys = jsonSummary.Where(l => l.LobPending == false && l.IsOfflineApproval == true && tenant.DocTypeId == l.RowKey.Split('|').FirstOrDefault()).ToList();
                    jSummaryTenantCountData.Add("ID", tenant.DocTypeId);
                    jSummaryTenantCountData.Add("TenantName", tenant.AppName);
                    jSummaryTenantCountData.Add("Count", filteredRowKeys.Count);
                    if (filteredRowKeys.Count > 0)
                        summaryCount.Add(jSummaryTenantCountData);
                }
                break;

            default:
                foreach (var tenant in tenants)
                {
                    JObject jSummaryTenantCountData = new JObject();
                    filteredRowKeys = jsonSummary.Where(l => l.LobPending == false && l.IsOfflineApproval == false && l.IsOutOfSyncChallenged == false && tenant.DocTypeId == l.RowKey.Split('|').FirstOrDefault()).ToList();
                    jSummaryTenantCountData.Add("ID", tenant.DocTypeId);
                    jSummaryTenantCountData.Add("TenantName", tenant.AppName);
                    jSummaryTenantCountData.Add("Count", filteredRowKeys.Count);
                    if (filteredRowKeys.Count > 0)
                        summaryCount.Add(jSummaryTenantCountData);
                }
                break;
        }

        return summaryCount;
    }

    /// <summary>
    /// Get Approval SummaryJson Approver for multiple Tenants for given view type
    /// </summary>
    /// <param name="approver">approver alias</param>
    /// <param name="tenants">List<ApprovalTenantInfo></ApprovalTenantInfo></param>
    /// <param name="approverId">Approver Alias's object Id</param>
    /// <param name="domain">Alias's Domain</param>
    /// <param name="viewType">Default value 'Summary'</param>
    /// <param name="isSubmittedRequest">flag to get submitted requests</param>
    /// <returns>List of approval summary data</returns>
    public List<ApprovalSummaryData> GetApprovalSummaryJsonByApproverAndTenants(string approver, List<ApprovalTenantInfo> tenants, string approverId, string domain, string viewType = "Summary", bool isSubmittedRequest = false)
    {
        List<ApprovalSummaryData> filteredSummaryData;
        var approvalSummaryData = _approvalSummaryProvider.GetApprovalSummaryJsonByApproverAndTenants(approver, tenants, approverId, domain, isSubmittedRequest);
        switch (viewType)
        {
            case Constants.OutOfSyncAction:
                filteredSummaryData = approvalSummaryData.Where(l => l.LobPending == false && l.IsOutOfSyncChallenged == true && tenants.Any(tenant => tenant.DocTypeId == l.DocumentTypeId)).Select(j => j).ToList();
                break;

            case Constants.OfflineApproval:
                filteredSummaryData = approvalSummaryData.Where(l => l.LobPending == false && l.IsOfflineApproval == true && tenants.Any(tenant => tenant.DocTypeId == l.DocumentTypeId)).Select(j => j).ToList();
                break;

            default:
                filteredSummaryData = approvalSummaryData.Where(l => l.LobPending == false && l.IsOfflineApproval == false && l.IsOutOfSyncChallenged == false && tenants.Any(tenant => tenant.DocTypeId == l.DocumentTypeId)).Select(j => j).ToList();
                break;
        }
        return filteredSummaryData;
    }

    /// <summary>
    /// Get approval counts
    /// </summary>
    /// <param name="approver"></param>
    /// <returns></returns>
    public async Task<ApprovalCount[]> GetApprovalCounts(string approver)
    {
        return await _approvalSummaryProvider.GetApprovalCounts(approver);
    }

    /// <summary>
    /// Get Approval Summary By Document Number and Approver
    /// </summary>
    /// <param name="documentTypeID"></param>
    /// <param name="documentNumber"></param>
    /// <param name="approverAlias"></param>
    /// <param name="approverId"></param>
    /// <param name="approverDomain"></param>
    /// <returns></returns>
    public ApprovalSummaryRow GetApprovalSummaryByDocumentNumberAndApprover(string documentTypeID, string documentNumber, string approverAlias, string approverId, string approverDomain)
    {
        return _approvalSummaryProvider.GetApprovalSummaryByDocumentNumberAndApprover(documentTypeID, documentNumber, approverAlias, approverId, approverDomain);
    }

    #endregion READ

    #region UPDATE

    /// <summary>
    /// Update summary
    /// </summary>
    /// <param name="tenant"></param>
    /// <param name="documentNumber"></param>
    /// <param name="approverAlias"></param>
    /// <param name="approverId"></param>
    /// <param name="approverDomain"></param>
    /// <returns></returns>
    public async Task UpdateSummary(ApprovalTenantInfo tenant, string documentNumber, string approverAlias, string approverId, string approverDomain, DateTime? actionDate, string actionName)
    {
        await _approvalSummaryProvider.UpdateSummary(tenant, documentNumber, approverAlias, approverId, approverDomain, actionDate, actionName);
    }

    /// <summary>
    /// Update summary
    /// </summary>
    /// <param name="tenant"></param>
    /// <param name="summaryRow"></param>
    /// <param name="actionDate"></param>
    /// <param name="actionName"></param>
    public async Task UpdateSummary(ApprovalTenantInfo tenant, ApprovalSummaryRow summaryRow, DateTime? actionDate, string actionName)
    {
        await _approvalSummaryProvider.UpdateSummary(tenant, summaryRow, actionDate, actionName);
    }

    /// <summary>
    /// Update summary in batch async
    /// </summary>
    /// <param name="summaryRows"></param>
    /// <param name="xcv"></param>
    /// <param name="sessionId"></param>
    /// <param name="tcv"></param>
    /// <param name="tenantInfo"></param>
    /// <param name="actionName"></param>
    /// <returns></returns>
    public async Task UpdateSummaryInBatchAsync(List<ApprovalSummaryRow> summaryRows, string xcv, string sessionId, string tcv, ApprovalTenantInfo tenantInfo, string actionName)
    {
        await _approvalSummaryProvider.UpdateSummaryInBatchAsync(summaryRows, xcv, sessionId, tcv, tenantInfo, actionName);
    }

    #endregion UPDATE

    #region DELETE

    /// <summary>
    /// Remove approval summary
    /// </summary>
    /// <param name="approvalRequest"></param>
    /// <param name="summaryRows"></param>
    /// <param name="message"></param>
    /// <param name="tenantInfo"></param>
    /// <returns></returns>
    public async Task<AzureTableRowDeletionResult> RemoveApprovalSummary(ApprovalRequestExpressionExt approvalRequest, List<ApprovalSummaryRow> summaryRows, ServiceBusReceivedMessage message, ApprovalTenantInfo tenantInfo)
    {
        return await _approvalSummaryProvider.RemoveApprovalSummary(approvalRequest, summaryRows, message, tenantInfo);
    }

    #endregion DELETE
}