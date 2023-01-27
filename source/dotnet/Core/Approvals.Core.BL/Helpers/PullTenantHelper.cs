// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace Microsoft.CFS.Approvals.Core.BL.Helpers;

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
using Microsoft.CFS.Approvals.Utilities.Helpers;
using Microsoft.CFS.Approvals.Utilities.Interface;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json.Linq;

/// <summary>
/// the pulltenanthelper class.
/// </summary>
public class PullTenantHelper : IPullTenantHelper
{
    #region VARIABLES

    /// <summary>
    /// The performance logger.
    /// </summary>
    private readonly IPerformanceLogger _performanceLogger = null;

    /// <summary>
    /// The Log Provider
    /// </summary>
    private readonly ILogProvider _logProvider = null;

    /// <summary>
    /// The name Resolution Helper.
    /// </summary>
    private readonly INameResolutionHelper _nameResolutionHelper = null;

    ///  <summary>
    /// The Approval Summary Provider.
    /// </summary>
    private readonly IApprovalSummaryProvider _approvalSummaryProvider = null;

    /// <summary>
    /// The configuration.
    /// </summary>
    private readonly IConfiguration _config = null;

    /// <summary>
    /// The approval Detail Provider.
    /// </summary>
    private readonly IApprovalDetailProvider _approvalDetailProvider = null;

    /// <summary>
    /// The flighting DataProvider.
    /// </summary>
    private readonly IFlightingDataProvider _flightingDataProvider = null;

    /// <summary>
    /// The ApprovalTenantInfo helper
    /// </summary>
    private readonly IApprovalTenantInfoHelper _approvalTenantInfoHelper = null;

    /// <summary>
    /// The Tenant factory
    /// </summary>
    private readonly ITenantFactory _tenantFactory;

    #endregion VARIABLES

    #region CONSTRUCTOR

    /// <summary>
    /// Initializes a new instance of the <see cref="PullTenantHelper"/> class.
    /// </summary>
    /// <param name="performanceLogger">The performance logger.</param>
    /// <param name="logProvider">The log provider.</param>
    /// <param name="nameResolutionHelper">The name resolution helper.</param>
    /// <param name="approvalSummaryProvider">The summary helper for Pull Tenant.</param>
    /// <param name="config">The configuration helper.</param>
    /// <param name="approvalDetailProvider">The Approval Detail Provider.</param>
    /// <param name="flightingDataProvider">The flighting Data Provider. </param>
    /// <param name="approvalTenantInfoHelper">The Approval Tenantinfo Helper.</param>
    /// <param name="tenantFactory">The tenant factory.</param>
    public PullTenantHelper(IApprovalTenantInfoHelper approvalTenantInfoHelper,
        IPerformanceLogger performanceLogger,
        ILogProvider logProvider,
        INameResolutionHelper nameResolutionHelper,
        IApprovalSummaryProvider approvalSummaryProvider,
        IConfiguration config,
        IApprovalDetailProvider approvalDetailProvider,
        IFlightingDataProvider flightingDataProvider,
        ITenantFactory tenantFactory)
    {
        _approvalTenantInfoHelper = approvalTenantInfoHelper;
        _performanceLogger = performanceLogger;
        _logProvider = logProvider;
        _nameResolutionHelper = nameResolutionHelper;
        _approvalSummaryProvider = approvalSummaryProvider;
        _config = config;
        _approvalDetailProvider = approvalDetailProvider;
        _flightingDataProvider = flightingDataProvider;
        _tenantFactory = tenantFactory;
    }

    #endregion CONSTRUCTOR

    #region Implemented Methods

    /// <summary>
    /// Get summary information for pending approval requests from tenant system.
    /// </summary>
    /// <param name="approverAlias">Approver alias (Contains delegated user alias if operation is performed on behalf of delegated user).</param>
    /// <param name="loggedInAlias">Logged-in user alias.</param>
    /// <param name="parameters">Input filter parameters.</param>
    /// <param name="tenantId">Tenant Id.</param>
    /// <param name="clientDevice">Client Device.</param>
    /// <param name="sessionId">Session Id.</param>
    /// <param name="xcv">XCV.</param>
    /// <param name="tcv">TCV.</param>
    /// <returns>Summary records.</returns>
    public async Task<JObject> GetSummaryAsync(string approverAlias,
                                      string loggedInAlias,
                                      Dictionary<string, object> parameters,
                                      int tenantId,
                                      string clientDevice,
                                      string sessionId,
                                      string xcv,
                                      string tcv
                                     )
    {
        #region Logging

        xcv = !string.IsNullOrWhiteSpace(xcv) ? xcv : Guid.NewGuid().ToString();
        tcv = !string.IsNullOrWhiteSpace(tcv) ? tcv : Guid.NewGuid().ToString();

        var logData = new Dictionary<LogDataKey, object>()
        {
            { LogDataKey.StartDateTime, DateTime.UtcNow },
            { LogDataKey.Xcv, xcv },
            { LogDataKey.Tcv, tcv },
            { LogDataKey.SessionId, sessionId },
            { LogDataKey.TenantId, tenantId },
            { LogDataKey.UserAlias, approverAlias },
            { LogDataKey.UserRoleName, loggedInAlias },
            { LogDataKey.EventType, Constants.BusinessProcessEvent },
            { LogDataKey.IsCriticalEvent, CriticalityLevel.Yes.ToString() },
            { LogDataKey.ClientDevice, clientDevice }
        };

        #endregion Logging

        try
        {
            using (_performanceLogger.StartPerformanceLogger("PerfLog", Constants.WebClient, string.Format(Constants.PerfLogAction, "PullTenantHelper", "GetSummaryAsync"), logData))
            {
                // Fetch tenant information.
                var tenant = _approvalTenantInfoHelper.GetTenantInfo(tenantId);

                // If tenant info cannot be read, summary cannot be fetched and hence throw an exception
                if (tenant == null)
                {
                    throw new ArgumentNullException(nameof(tenant), "Tenant Info is null");
                }

                // Only tenants enabled for pull model are allowed for this operation.
                if (!tenant.IsPullModelEnabled)
                {
                    throw new InvalidOperationException($"Tenant {tenant.AppName} is not configured for pull model.");
                }

                logData.Add(LogDataKey.TenantName, !string.IsNullOrWhiteSpace(tenant.AppName) ? tenant.AppName : string.Empty);

                var tenantAdapter = _tenantFactory.GetTenant(tenant);

                // Get all the pending approval requests for the user from tenant system.
                var httpResponseMessage = await tenantAdapter.GetTenantSummaryAsync(parameters, approverAlias, loggedInAlias, xcv, tcv, sessionId);
                logData.Add(LogDataKey.EndDateTime, DateTime.UtcNow);

                if (!tenantAdapter.TreatNotFoundAsError() && httpResponseMessage.StatusCode == System.Net.HttpStatusCode.NotFound)
                {
                    // Log event.
                    logData.Add(LogDataKey.ResponseContent, await httpResponseMessage.Content.ReadAsStringAsync());
                    _logProvider.LogInformation(TrackingEvent.WebApiExternalSummaryWithNotFoundStatus, logData);

                    return await JSONHelper.HandleNotFound();
                }

                // Return validated JSON resposne.
                var responseMessage = await JSONHelper.ValidateJsonResponse(_config, httpResponseMessage, tenant.AppName);

                // Log Success event.
                logData.Modify(LogDataKey.EndDateTime, DateTime.UtcNow);
                _logProvider.LogInformation(TrackingEvent.WebApiExternalSummarySuccess, logData);
                return responseMessage;
            }
        }
        catch (Exception ex)
        {
            // Log failure event.
            logData.Modify(LogDataKey.EndDateTime, DateTime.UtcNow);
            _logProvider.LogError(TrackingEvent.WebApiExternalSummaryFail, ex, logData);
            throw;
        }
    }

    /// <summary>
    /// Get details information for an approval request from tenant system.
    /// </summary>
    /// <param name="approverAlias">Approver alias (Contains delegated user alias if operation is performed on behalf of delegated user).</param>
    /// <param name="loggedInAlias">Logged-in user alias.</param>
    /// <param name="operationType">Type of operation which needs to be executed from tenant info configuration.</param>
    /// <param name="parameters">Input filter parameters.</param>
    /// <param name="tenantId">Tenant Id.</param>
    /// <param name="clientDevice">Client Device.</param>
    /// <param name="sessionId">Session Id.</param>
    /// <param name="xcv">XCV.</param>
    /// <param name="tcv">TCV.</param>
    /// <returns>Details of a request.</returns>
    public async Task<JObject> GetDetailsAsync(
        string approverAlias,
        string loggedInAlias,
        string operationType,
        Dictionary<string, object> parameters,
        int tenantId,
        string clientDevice,
        string sessionId,
        string xcv,
        string tcv)
    {
        #region Logging

        tcv = !string.IsNullOrWhiteSpace(tcv) ? tcv : Guid.NewGuid().ToString();

        var logData = new Dictionary<LogDataKey, object>
        {
            { LogDataKey.StartDateTime, DateTime.UtcNow },
            { LogDataKey.Xcv, xcv },
            { LogDataKey.Tcv, tcv },
            { LogDataKey.SessionId, sessionId },
            { LogDataKey.TenantId, tenantId },
            { LogDataKey.UserAlias, approverAlias },
            { LogDataKey.UserRoleName , loggedInAlias },
            { LogDataKey.OperationType, operationType },
            { LogDataKey.EventType, Constants.BusinessProcessEvent },
            { LogDataKey.IsCriticalEvent, CriticalityLevel.Yes.ToString() },
            { LogDataKey.ClientDevice, clientDevice }
        };

        if (parameters.ContainsKey(Constants.DocumentNumber))
        {
            logData.Add(LogDataKey.DocumentNumber, parameters[Constants.DocumentNumber]?.ToString());
        }

        #endregion Logging

        try
        {
            using (_performanceLogger.StartPerformanceLogger("PerfLog", Constants.WebClient, string.Format(Constants.PerfLogAction, "PullTenantHelper", "GetDetailsAsync"), logData))
            {
                // Throw exception if Document Number is not present to fetch details.
                if (string.IsNullOrWhiteSpace(parameters[Constants.DocumentNumber]?.ToString()))
                {
                    throw new ArgumentNullException(Constants.DocumentNumber, "Document Number is not available to fetch details");
                }

                // Fetch tenant information.
                var tenant = _approvalTenantInfoHelper.GetTenantInfo(tenantId);

                // If tenant info cannot be read, request details cannot be fetched and hence throw an exception.
                if (tenant == null)
                {
                    throw new ArgumentNullException(nameof(tenant), "Tenant Info is null");
                }

                // Only tenants enabled for pull model are allowed for this operation.
                if (!tenant.IsPullModelEnabled)
                {
                    throw new InvalidOperationException($"Tenant {tenant.AppName} is not configured for pull model.");
                }

                logData.Add(LogDataKey.TenantName, !string.IsNullOrWhiteSpace(tenant.AppName) ? tenant.AppName : string.Empty);

                var tenantAdapter = _tenantFactory.GetTenant(tenant);

                // Get the details of an approval request from tenant system.
                var httpResponseMessage = await tenantAdapter.GetTenantDetailsAsync(operationType, parameters, approverAlias, loggedInAlias, clientDevice, xcv, tcv, sessionId);
                logData.Add(LogDataKey.EndDateTime, DateTime.UtcNow);

                if (!tenantAdapter.TreatNotFoundAsError() && httpResponseMessage.StatusCode == System.Net.HttpStatusCode.NotFound)
                {
                    // Log event.
                    logData.Add(LogDataKey.ResponseContent, await httpResponseMessage.Content.ReadAsStringAsync());
                    _logProvider.LogInformation(TrackingEvent.WebApiExternalDetailWithNotFoundStatus, logData);

                    return await JSONHelper.HandleNotFound();
                }

                var responseMessage = await JSONHelper.ValidateJsonResponse(_config, httpResponseMessage, tenant.AppName);

                // Log Success event.
                logData.Modify(LogDataKey.EndDateTime, DateTime.UtcNow);
                _logProvider.LogInformation(TrackingEvent.WebApiExternalDetailsSuccess, logData);
                return responseMessage;
            }
        }
        catch (Exception ex)
        {
            // Log failure event.
            logData.Modify(LogDataKey.EndDateTime, DateTime.UtcNow);
            _logProvider.LogError(TrackingEvent.WebApiExternalDetailsFail, ex, logData);
            throw;
        }
    }

    /// <summary>
    /// Get summary count for pull tenants.
    /// </summary>
    /// <param name="operationType">Type of operation which needs to be executed from tenant info configuration.</param>
    /// <param name="approverAlias">Approver alias (Contains delegated user alias if operation is performed on behalf of delegated user).</param>
    /// <param name="loggedInAlias">Logged-in user alias.</param>
    /// <param name="sessionId">Session Id.</param>
    /// <param name="xcv">XCV.</param>
    /// <param name="tcv">TCV.</param>
    /// <param name="clientDevice">Client Device.</param>
    /// <returns>Array of summary count.</returns>
    public async Task<JArray> GetSummaryCountAsync(
        string operationType,
        string approverAlias,
        string loggedInAlias,
        string sessionId,
        string xcv,
        string tcv,
        string clientDevice)
    {
        tcv = !string.IsNullOrWhiteSpace(tcv) ? tcv : Guid.NewGuid().ToString();

        var logData = new Dictionary<LogDataKey, object>
        {
            { LogDataKey.StartDateTime, DateTime.UtcNow },
            { LogDataKey.Xcv, xcv },
            { LogDataKey.Tcv, tcv },
            { LogDataKey.SessionId, sessionId },
            { LogDataKey.UserAlias, approverAlias },
            { LogDataKey.UserRoleName , loggedInAlias },
            { LogDataKey.OperationType, operationType },
            { LogDataKey.EventType, Constants.BusinessProcessEvent },
            { LogDataKey.IsCriticalEvent, CriticalityLevel.Yes.ToString() },
            { LogDataKey.ClientDevice, clientDevice }
        };

        try
        {
            using (_performanceLogger.StartPerformanceLogger("PerfLog", Constants.WebClient, string.Format(Constants.PerfLogAction, "PullTenantHelper", "GetSummaryCountAsync"), logData))
            {
                // Fetch tenant information.
                var allTenants = await _approvalTenantInfoHelper.GetTenants();
                var pullTenants = allTenants.Where(t => t.IsPullModelEnabled).ToList();

                // If tenant info cannot be read, request details cannot be fetched and hence throw an exception.
                if (pullTenants == null || pullTenants.Count == 0)
                {
                    throw new ArgumentNullException(nameof(pullTenants), "There is no Pull Tenant available");
                }

                JArray summaryCountArray = new JArray();
                foreach (var tenant in pullTenants)
                {
                    try
                    {
                        var tenantAdapter = _tenantFactory.GetTenant(tenant);

                        var parameters = new Dictionary<string, object>
                    {
                        { "alias", loggedInAlias }
                    };

                        // Get the details of an approval request from tenant system.
                        var httpResponseMessage = await tenantAdapter.GetTenantDetailsAsync(operationType, parameters,
                            approverAlias, loggedInAlias, clientDevice, xcv, tcv, sessionId);
                        if (httpResponseMessage == null)
                        {
                            continue;
                        }

                        var responseMessage = await JSONHelper.ValidateJsonResponse(_config, httpResponseMessage, tenant.AppName);
                        var response = responseMessage["response"] ?? responseMessage;
                        if (response != null)
                        {
                            var dataModelMapping = tenant.DataModelMapping.ToJObject();
                            dynamic summaryCount = new JObject();
                            summaryCount.TenantId = tenant.TenantId;
                            summaryCount.AppName = tenant.AppName;
                            summaryCount.CondensedAppName = tenant.AppName.Replace(" ", "");
                            summaryCount.Count = Convert.ToInt32(response[dataModelMapping["summaryCount"].ToString()]);
                            summaryCountArray.Add(summaryCount);
                        }
                    }
                    catch (Exception ex)
                    {
                        // Log failure event.
                        logData.Modify(LogDataKey.EndDateTime, DateTime.UtcNow);
                        _logProvider.LogError(TrackingEvent.WebApiExternalSummaryCountFail, ex, logData);
                    }
                }

                logData.Modify(LogDataKey.EndDateTime, DateTime.UtcNow);
                // Log Success event.
                _logProvider.LogInformation(TrackingEvent.WebApiExternalSummaryCountSuccess, logData);

                return summaryCountArray;
            }
        }
        catch (Exception ex)
        {
            // Log failure event.
            logData.Modify(LogDataKey.EndDateTime, DateTime.UtcNow);
            _logProvider.LogError(TrackingEvent.WebApiExternalSummaryCountFail, ex, logData);
            throw;
        }
    }

    #endregion Implemented Methods
}