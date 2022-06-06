// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace Microsoft.CFS.Approvals.Core.BL.Helpers
{
    using System;
    using System.Collections.Generic;
    using System.Globalization;
    using System.Linq;
    using System.Threading.Tasks;
    using Microsoft.CFS.Approvals.Common.DL.Interface;
    using Microsoft.CFS.Approvals.Contracts;
    using Microsoft.CFS.Approvals.Core.BL.Interface;
    using Microsoft.CFS.Approvals.Domain.BL.Interface;
    using Microsoft.CFS.Approvals.Extensions;
    using Microsoft.CFS.Approvals.LogManager.Model;
    using Microsoft.CFS.Approvals.LogManager.Provider.Interface;
    using Microsoft.CFS.Approvals.Model;
    using Microsoft.CFS.Approvals.Utilities.Extension;
    using Microsoft.Extensions.Configuration;
    using Newtonsoft.Json;
    using Newtonsoft.Json.Linq;

    /// <summary>
    /// The Summary Helper class
    /// </summary>
    public class SummaryHelper : ISummaryHelper
    {
        /// <summary>
        /// The configuration
        /// </summary>
        protected readonly IConfiguration _config;

        /// <summary>
        /// The approval tenantInfo helper
        /// </summary>
        protected readonly IApprovalTenantInfoHelper _approvalTenantInfoHelper;

        /// <summary>
        /// The approval summary helper
        /// </summary>
        protected readonly IApprovalSummaryHelper _approvalSummaryHelper;

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
        /// Constructor of SummaryHelper
        /// </summary>
        /// <param name="config"></param>
        /// <param name="approvalTenantInfoHelper"></param>
        /// <param name="approvalSummaryHelper"></param>
        /// <param name="delegationHelper"></param>
        /// <param name="logProvider"></param>
        /// <param name="performanceLogger"></param>
        /// <param name="approvalSummaryProvider"></param>
        public SummaryHelper(
            IConfiguration config,
            IApprovalTenantInfoHelper approvalTenantInfoHelper,
            IApprovalSummaryHelper approvalSummaryHelper,
            IDelegationHelper delegationHelper,
            ILogProvider logProvider,
            IPerformanceLogger performanceLogger,
            IApprovalSummaryProvider approvalSummaryProvider)
        {
            _config = config;
            _approvalTenantInfoHelper = approvalTenantInfoHelper;
            _approvalSummaryHelper = approvalSummaryHelper;
            _delegationHelper = delegationHelper;
            _logProvider = logProvider;
            _performanceLogger = performanceLogger;
            _approvalSummaryProvider = approvalSummaryProvider;
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
                                     t.IsControlsAndComplianceRequired
                                 }).ToList();
            var summaryData = new List<ApprovalSummaryData>();
            object lockObject = new object();
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
                            _logProvider.LogError(TrackingEvent.WebApiCreateSummaryDataListFail, ex);
                        }
                    }
                }
            }
);

            return summaryData;
        }

        /// <summary>
        /// Get other summary requests
        /// </summary>
        /// <param name="loggedInAlias"></param>
        /// <param name="alias"></param>
        /// <param name="host"></param>
        /// <param name="viewType"></param>
        /// <param name="sessionId"></param>
        /// <param name="tenantDocTypeId"></param>
        /// <returns></returns>
        public async Task<JArray> GetOtherSummaryRequests(string loggedInAlias, string alias, string host, string viewType, string sessionId, string tenantDocTypeId = "")
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
                { LogDataKey.UserRoleName, loggedInAlias },
                { LogDataKey.ClientDevice, host },
                { LogDataKey.EventType, Constants.FeatureUsageEvent },
                { LogDataKey.UserAlias, alias },
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
                        throw new Exception(_config[ConfigurationKey.Message_NoTenantForDevice.ToString()]);
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
                        throw new Exception(_config[ConfigurationKey.Message_NoTenantForDevice.ToString()]);
                    }

                    logData.Add(LogDataKey.Tenants, String.Join(", ", tenants.Select(t => t.TenantId.ToString(CultureInfo.InvariantCulture)).ToArray()));

                    // Get the Summary Json by Approver and Tenants so that can then be modified into final form adding in Tenant Metadata
                    var approvalsData = _approvalSummaryHelper.GetApprovalSummaryJsonByApproverAndTenants(alias, tenants, viewType);

                    bool checkEnableUserDelegationFlag = false;

                    //if loggedInAlias != current alias (Alias), that mean current user is in delegate mode. So enable the UserDelegation flag checking when creating the Summary list.
                    if (!loggedInAlias.Equals(alias, StringComparison.OrdinalIgnoreCase))
                    {
                        // check if entry made from support portal
                        if (_delegationHelper.GetUserDelegationsForCurrentUser(alias).Where(d => d.DelegatedToAlias == loggedInAlias && d.IsHidden == true) == null)
                            checkEnableUserDelegationFlag = true;
                    }

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
        /// <returns></returns>
        public async Task<JArray> GetOtherSummaryRequestsCountData(string tenantDocTypeId, string loggedInAlias, string userAlias, string viewType, string sessionId, string clientDevice)
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
                            throw new Exception("Invalid DocumentTypeID");
                        }

                        #endregion Validations

                        tenants = tenants.Where(x => x.DocTypeId.Equals(Guid.Parse(tenantDocTypeId).ToString())).ToList();
                    }

                    // If tenant info cannot be read, summary cannot be fetched and hence throw an exception
                    if (tenants == null || tenants.Count == 0)
                    {
                        if (!string.IsNullOrEmpty(tenantDocTypeId))
                            throw new Exception("Tenant DocumentTypeID not present in Approvals");
                        else
                            throw new Exception(_config[ConfigurationKey.Message_NoTenantForDevice.ToString()]);
                    }

                    var approvalsData = _approvalSummaryHelper.GetApprovalSummaryCountJsonByApproverAndTenants(userAlias, tenants, viewType);

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
        /// <param name="loggedInAlias"></param>
        /// <param name="alias"></param>
        /// <param name="host"></param>
        /// <param name="sessionId"></param>
        /// <param name="tenantDocTypeId"></param>
        /// <returns></returns>
        public async Task<JArray> GetSummary(string loggedInAlias, string alias, string host, string sessionId, string tenantDocTypeId = "")
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
                { LogDataKey.UserRoleName, loggedInAlias },
                { LogDataKey.ClientDevice, host },
                { LogDataKey.EventType, Constants.FeatureUsageEvent },
                { LogDataKey.UserAlias, alias },
                { LogDataKey.IsCriticalEvent, CriticalityLevel.Yes.ToString() }
            };

            #endregion Logging

            try
            {
                using (_performanceLogger.StartPerformanceLogger("PerfLog", Constants.WebClient, string.Format(Constants.PerfLogAction, "SummaryHelper", "GetSummary"), logData))
                {
                    // Check if Host is null or empty and throw back an exception which will get returned as bad request to caller
                    if (string.IsNullOrEmpty(host))
                    {
                        throw new Exception(_config[ConfigurationKey.Message_NoTenantForDevice.ToString()]);
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
                        throw new Exception(_config[ConfigurationKey.Message_NoTenantForDevice.ToString()]);
                    }
                    logData.Add(LogDataKey.Tenants, string.Join(", ", tenants.Select(t => t.TenantId.ToString(CultureInfo.InvariantCulture)).ToArray()));

                    // Get the Summary Json by Approver and Tenants so that can then be modified into final form adding in Tenant Metadata
                    var approvalsData = _approvalSummaryHelper.GetApprovalSummaryJsonByApproverAndTenants(alias, tenants);

                    bool checkEnableUserDelegationFlag = false;

                    //if loggedInAlias != current alias (Alias), that mean current user is in delegate mode. So enable the UserDelegation flag checking when creating the Summary list.
                    if (!loggedInAlias.Equals(alias, StringComparison.OrdinalIgnoreCase))
                    {
                        // check if entry made from support portal
                        if (_delegationHelper.GetUserDelegationsForCurrentUser(alias).Where(d => d.DelegatedToAlias == loggedInAlias && d.IsHidden == true) == null)
                            checkEnableUserDelegationFlag = true;
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
        /// Get summary count data
        /// </summary>
        /// <param name="tenantDocTypeId"></param>
        /// <param name="loggedInAlias"></param>
        /// <param name="userAlias"></param>
        /// <param name="sessionId"></param>
        /// <param name="clientDevice"></param>
        /// <returns></returns>
        public async Task<JArray> GetSummaryCountData(string tenantDocTypeId, string loggedInAlias, string userAlias, string sessionId, string clientDevice)
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
                            throw new Exception("Invalid DocumentTypeID");
                        }

                        #endregion Validations

                        tenants = tenants.Where(x => x.DocTypeId.Equals(Guid.Parse(tenantDocTypeId).ToString())).ToList();
                    }

                    // If tenant info cannot be read, summary cannot be fetched and hence throw an exception
                    if (tenants == null || tenants.Count == 0)
                    {
                        if (!string.IsNullOrEmpty(tenantDocTypeId))
                            throw new Exception("Tenant DocumentTypeID not present in Approvals");
                        else
                            throw new Exception(_config[ConfigurationKey.Message_NoTenantForDevice.ToString()]);
                    }

                    var approvalsData = _approvalSummaryHelper.GetApprovalSummaryCountJsonByApproverAndTenants(userAlias, tenants);

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
        /// <param name="loggedInAlias"></param>
        /// <param name="xcv"></param>
        /// <param name="tcv"></param>
        /// <returns></returns>
        public ApprovalSummaryRow GetSummaryData(string documentNumber, string fiscalYear, ApprovalTenantInfo tenantInfo, string alias, string loggedInAlias, string xcv, string tcv, ITenant tenantAdaptor)
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
                documentSummary = tenantAdaptor.GetApprovalSummaryByRowKeyAndApprover(documentNumber, alias, fiscalYear, tenantInfo);
            }
            if (documentSummary != null)
            {
                logData.Add(LogDataKey.ReceivedTcv, documentSummary.Tcv);
            }
            return documentSummary;
        }
    }
}