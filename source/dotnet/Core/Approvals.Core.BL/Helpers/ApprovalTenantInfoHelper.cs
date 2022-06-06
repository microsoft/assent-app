// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace Microsoft.CFS.Approvals.Core.BL.Helpers
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;
    using Microsoft.CFS.Approvals.Common.DL.Interface;
    using Microsoft.CFS.Approvals.Contracts;
    using Microsoft.CFS.Approvals.Core.BL.Interface;
    using Microsoft.CFS.Approvals.Extensions;
    using Microsoft.CFS.Approvals.LogManager.Model;
    using Microsoft.CFS.Approvals.LogManager.Provider.Interface;
    using Microsoft.CFS.Approvals.Model;
    using Microsoft.CFS.Approvals.Utilities.Helpers;
    using Microsoft.Extensions.Configuration;
    using Newtonsoft.Json;
    using Newtonsoft.Json.Linq;
    using Newtonsoft.Json.Serialization;

    /// <summary>
    /// The Approval TenantInfo Helper class
    /// </summary>
    public class ApprovalTenantInfoHelper : IApprovalTenantInfoHelper
    {
        /// <summary>
        /// The approval tenantInfo provider
        /// </summary>
        private readonly IApprovalTenantInfoProvider _approvalTenantInfoProvider;

        /// <summary>
        /// The performance logger
        /// </summary>
        private readonly IPerformanceLogger _performanceLogger;

        /// <summary>
        /// The log provider
        /// </summary>
        private readonly ILogProvider _logProvider;

        /// <summary>
        /// The configuration
        /// </summary>
        private readonly IConfiguration _config;

        /// <summary>
        /// The tenant factory
        /// </summary>
        private readonly ITenantFactory _tenantFactory;

        /// <summary>
        /// Initializes a new instance of the <see cref="ApprovalTenantInfoHelper"/> class.
        /// </summary>
        /// <param name="approvalTenantInfoProvider">The approval tenant info provider.</param>
        /// <param name="performanceLogger">The performance logger.</param>
        /// <param name="logProvider">The log provider.</param>
        /// <param name="config">The configuration.</param>
        /// <param name="tenantFactory">The tenant Factory.</param>
        public ApprovalTenantInfoHelper(
            IApprovalTenantInfoProvider approvalTenantInfoProvider,
            IPerformanceLogger performanceLogger,
            ILogProvider logProvider,
            IConfiguration config,
            ITenantFactory tenantFactory)
        {
            _approvalTenantInfoProvider = approvalTenantInfoProvider;
            _performanceLogger = performanceLogger;
            _logProvider = logProvider;
            _config = config;
            _tenantFactory = tenantFactory;
        }

        /// <summary>
        /// This method will get bulk view tenant actions.
        /// </summary>
        /// <param name="bulkActionConcurrentMessageFormat">The bulkActionConcurrentMessageFormat.</param>
        /// <param name="loggedInAlias">The loggedInAlias.</param>
        /// <param name="alias">The alias.</param>
        /// <param name="clientDevice">The clientDevice.</param>
        /// <param name="sessionId">The sessionId.</param>
        /// <returns>Returns BulkViewTenantActions</returns>
        public async Task<string> GetBulkViewTenantActions(string bulkActionConcurrentMessageFormat, string loggedInAlias, string alias, string clientDevice, string sessionId)
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
                { LogDataKey.ClientDevice, clientDevice },
                { LogDataKey.EventType, Constants.FeatureUsageEvent },
                { LogDataKey.UserAlias, alias },
                { LogDataKey.StartDateTime, DateTime.UtcNow },
                { LogDataKey.IsCriticalEvent, CriticalityLevel.No.ToString() }
            };

            #endregion Logging

            try
            {
                using (var summaryTracer = _performanceLogger.StartPerformanceLogger("PerfLog", string.IsNullOrWhiteSpace(clientDevice) ? Constants.WebClient : clientDevice, string.Format(Constants.PerfLogCommon, "TenantAction"), logData))
                {
                    var tenantActions = (await _approvalTenantInfoProvider.GetAllTenantInfo(false)).Where(t => t.TenantEnabled == true)
                        .Select(t =>
                            new
                            {
                                t.TenantId,
                                BulkActions = (t.ActionDetails.Primary == null ? new List<TenantAction>() : t.ActionDetails.Primary.Where(a => a.IsEnabled == true && a.IsBulkAction == true)).Union((t.ActionDetails.Secondary == null ? new List<TenantAction>() : t.ActionDetails.Secondary.Where(a => a.IsEnabled == true && a.IsBulkAction == true))),
                                t.BulkActionConcurrentCall,
                                BulkActionConcurrentCallMessage = string.Format(bulkActionConcurrentMessageFormat, t.BulkActionConcurrentCall)
                            })
                        .Where(t1 => t1.BulkActions.Count() > 0).ToList();
                    logData.Modify(LogDataKey.EndDateTime, DateTime.UtcNow);
                    _logProvider.LogInformation(TrackingEvent.TenantApiComplete, logData);
                    return tenantActions.ToJson();
                }
            }
            catch (Exception ex)
            {
                logData.Modify(LogDataKey.EndDateTime, DateTime.UtcNow);
                _logProvider.LogError(TrackingEvent.TenantApiFail, ex, logData);
                throw;
            }
        }

        /// <summary>
        /// This method will fetch all the application names.
        /// </summary>
        /// <returns>List of aplication names</returns>
        public async Task<List<string>> GetNames()
        {
            return (await _approvalTenantInfoProvider.GetAllTenantInfo(false)).ToList().Select(x => x.AppName).ToList();
        }

        /// <summary>
        /// This method will retrieve TenantActionDetails from Tenant.
        /// </summary>
        /// <param name="tenantId">The tenantId.</param>
        /// <param name="loggedInAlias">The loggedInAlias.</param>
        /// <param name="alias">The alias.</param>
        /// <param name="clientDevice">The clientDevice.</param>
        /// <param name="sessionId">The sessionId.</param>
        /// <param name="xcv">The xcv.</param>
        /// <param name="tcv">The tcv.</param>
        /// <param name="aadUserToken">AAD User Token</param>
        /// <returns>Returns Approval tenant info with action details.</returns>
        public async Task<ApprovalTenantInfo> GetTenantActionDetails(int tenantId, string loggedInAlias, string alias, string clientDevice, string sessionId, string xcv, string tcv, string aadUserToken)
        {
            #region Logging

            var logData = new Dictionary<LogDataKey, object>()
                {
                    { LogDataKey.StartDateTime, DateTime.UtcNow },
                    { LogDataKey.Xcv, xcv },
                    { LogDataKey.Tcv, tcv },
                    { LogDataKey.SessionId, sessionId },
                    { LogDataKey.TenantId, tenantId },
                    { LogDataKey.UserAlias, alias },
                    { LogDataKey.UserRoleName, loggedInAlias },
                    { LogDataKey.EventType, Constants.BusinessProcessEvent },
                    { LogDataKey.BusinessProcessName, Constants.BusinessProcessNameGetActionDetailsFromTenant },
                    { LogDataKey.IsCriticalEvent, CriticalityLevel.Yes.ToString() },
                    { LogDataKey.ClientDevice, clientDevice },
                    { LogDataKey.Operation, OperationType.TenantActionDetail },
                    { LogDataKey.AppAction, Constants.BusinessProcessNameGetActionDetailsFromTenant }
                };

            #endregion Logging

            try
            {
                // Fetch tenant information.
                var tenant = GetTenantInfo(tenantId);
                // If tenant info cannot be read, throw an exception
                if (tenant == null)
                {
                    throw new ArgumentNullException(nameof(tenant), "Tenant Info is null");
                }

                if (tenant.SummaryDataMapping != null)
                {
                    try
                    {
                        var dataMapping = JObject.Parse(tenant.SummaryDataMapping);
                        if (dataMapping != null)
                        {
                            dataMapping["appName"] = tenant?.AppName;
                            var dataMappingString = dataMapping.ToString(Formatting.None);
                            tenant.SummaryDataMapping = dataMappingString;
                        }
                    }
                    catch (Exception ex)
                    {
                        _logProvider.LogError(TrackingEvent.WebApiSummaryDataMappingFail, ex, logData);
                    }
                }

                if (tenant.IsExternalTenantActionDetails)
                {
                    JObject tenantActionDetails = null;

                    logData.Add(LogDataKey.TenantName,
                        !string.IsNullOrWhiteSpace(tenant.AppName) ? tenant.AppName : string.Empty);

                    var tenantAdapter = _tenantFactory.GetTenant(tenant,
                        alias,
                        clientDevice,
                        aadUserToken);

                    // Get the TenantActionDetails for the user from tenant system.
                    var httpResponseMessage = await tenantAdapter.GetTenantActionDetails(alias, loggedInAlias, clientDevice, sessionId, xcv, tcv);
                    logData.Add(LogDataKey.EndDateTime, DateTime.UtcNow);
                    if (!httpResponseMessage.IsSuccessStatusCode)
                    {
                        // Log response string.
                        logData.Add(LogDataKey.ResponseContent, await httpResponseMessage.Content.ReadAsStringAsync());
                    }

                    // Return validated JSON response.
                    tenantActionDetails = await JSONHelper.ValidateJsonResponse(_config, httpResponseMessage, tenant.AppName);
                    // Log Success event.
                    _logProvider.LogInformation(TrackingEvent.WebApiExternalActionDetailsSuccess, logData);

                    tenant.TenantActionDetails = tenantActionDetails.ToJson(new JsonSerializerSettings { ContractResolver = new CamelCasePropertyNamesContractResolver() });
                }

                return tenant;
            }
            catch (Exception ex)
            {
                // Log failure event.
                if (!logData.ContainsKey(LogDataKey.EndDateTime))
                {
                    logData.Add(LogDataKey.EndDateTime, DateTime.UtcNow);
                }

                _logProvider.LogError(TrackingEvent.WebApiExternalActionDetailsFail, ex, logData);
                throw;
            }
        }

        /// <summary>
        /// This method will get TenantDocTypeId.
        /// </summary>
        /// <param name="tenantName">The TenantName.</param>
        /// <returns>returns TenantDocTypeId.</returns>
        public async Task<string> GetTenantDocTypeId(string tenantName)
        {
            return (await _approvalTenantInfoProvider.GetAllTenantInfo(false)).ToList().Single(x => x.AppName == tenantName).DocTypeId;
        }

        /// <summary>
        /// This method will return TenantIds.
        /// </summary>
        /// <param name="tenantName">The tenant name.</param>
        /// <returns>The TenantIds.</returns>
        public async Task<string> GetTenantIds(string tenantName)
        {
            return (await _approvalTenantInfoProvider.GetAllTenantInfo(false)).ToList().Single(x => x.AppName == tenantName).RowKey;
        }

        /// <summary>
        /// This mehtod will fetch Approval Tenant Information by tenantId.
        /// </summary>
        /// <param name="tenantId">The TenantId.</param>
        /// <returns>The ApprovalTenantInformation.</returns>
        public ApprovalTenantInfo GetTenantInfo(int tenantId)
        {
            return _approvalTenantInfoProvider.GetTenantInfo(tenantId);
        }

        /// <summary>
        /// This method will fetch all the Teant Informations.
        /// </summary>
        /// <returns>List of ApprovalTenantInformation.</returns>
        public async Task<List<ApprovalTenantInfo>> GetTenants(bool fetchImageDetails = true)
        {
            return (await _approvalTenantInfoProvider.GetAllTenantInfo(fetchImageDetails)).ToList();
        }

        /// <summary>
        /// This method with fetch Approval Tenant Informations by host.
        /// </summary>
        /// <param name="host">The host/ClientDevice.</param>
        /// <returns>List of ApprovalTenantInformation.</returns>
        public List<ApprovalTenantInfo> GetTenantInfoByHost(string host)
        {
            var listTenantInfo = _approvalTenantInfoProvider.GetTenantInfo();
            return listTenantInfo.Where(x => x.TenantEnabled.Equals(true) && x.RegisteredClientsList.Contains(host.ToUpper())).ToList();
        }

        /// <summary>
        /// This method will fetch all the tenants who has disabled user delegations.
        /// </summary>
        /// <returns>List of application names</returns>
        public async Task<List<string>> GetUserDelegationDisabledTenants()
        {
            return (await _approvalTenantInfoProvider.GetAllTenantInfo(false)).ToList().Where(x => (x.TenantType ?? "").Equals(Constants.TenantTypeProduction, StringComparison.InvariantCultureIgnoreCase) && (x.EnableUserDelegation) == false)
                                                                         .OrderBy(a => a.AppName)
                                                                         .Select(a => a.AppName)
                                                                         .ToList();
        }

        /// <summary>
        /// This method will fetch UserDelegatiohnDisabledTenants message.
        /// </summary>
        /// <param name="sessionId">The sessionId.</param>
        /// <param name="alias">The alias.</param>
        /// <param name="clientDevice">the clientDevice.</param>
        /// <returns>returns message.</returns>
        public async Task<string> GetUserDelegationDisabledTenantsMessage(string sessionId, string alias, string clientDevice)
        {
            var logData = new Dictionary<LogDataKey, object>();

            try
            {
                #region Logging

                var Tcv = Guid.NewGuid().ToString();

                if (!string.IsNullOrEmpty(sessionId))
                {
                    Tcv = sessionId;
                }
                // Add common data items to LogData
                logData.Add(LogDataKey.Xcv, Tcv);
                logData.Add(LogDataKey.Tcv, Tcv);
                logData.Add(LogDataKey.SessionId, Tcv);
                logData.Add(LogDataKey.UserRoleName, alias);
                logData.Add(LogDataKey.ClientDevice, clientDevice);
                logData.Add(LogDataKey.EventType, Constants.FeatureUsageEvent);
                logData.Add(LogDataKey.UserAlias, alias);
                logData.Add(LogDataKey.IsCriticalEvent, CriticalityLevel.No.ToString());

                #endregion Logging

                using (var documentStatustracer = _performanceLogger.StartPerformanceLogger("PerfLog", string.IsNullOrWhiteSpace(clientDevice) ? Constants.WebClient : clientDevice, string.Format(Constants.PerfLogCommon, "User Delegation Disabled Tenants Get"), logData))
                {
                    List<string> disabledTenantList = await GetUserDelegationDisabledTenants();
                    string disabledTenants = string.Join(", ", disabledTenantList);
                    if (string.IsNullOrEmpty(disabledTenants))
                        disabledTenants = "Requests from all tenants can be delegated";
                    else
                        disabledTenants = "Requests from following tenants cannot be delegated: " + disabledTenants + ".";

                    // Log Success
                    _logProvider.LogInformation(TrackingEvent.WebApiImpersonationSettingsTenantInfoSuccess, logData);

                    return (disabledTenants);
                }
            }
            catch (Exception ex)
            {
                _logProvider.LogError(TrackingEvent.WebApiImpersonationSettingsTenantInfoFail, ex, logData);
                throw;
            }
        }
    }
}