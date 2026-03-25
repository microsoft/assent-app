// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace Microsoft.CFS.Approvals.Core.BL.Helpers;

using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using Microsoft.CFS.Approvals.Common.DL.Interface;
using Microsoft.CFS.Approvals.Contracts;
using Microsoft.CFS.Approvals.Contracts.DataContracts;
using Microsoft.CFS.Approvals.Core.BL.Interface;
using Microsoft.CFS.Approvals.Extensions;
using Microsoft.CFS.Approvals.LogManager.Model;
using Microsoft.CFS.Approvals.LogManager.Provider.Interface;
using Microsoft.CFS.Approvals.Model;
using Microsoft.CFS.Approvals.Utilities.Helpers;
using Microsoft.CFS.Approvals.Utilities.Interface;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

/// <summary>
/// The Delegation Helper class
/// </summary>
public class DelegationHelper : IDelegationHelper
{
    /// <summary>
    /// The user delegation provider
    /// </summary>
    private readonly IUserDelegationProvider _userDelegationProvider;

    /// <summary>
    /// The name resolution helper
    /// </summary>
    private readonly INameResolutionHelper _nameResolutionHelper;

    /// <summary>
    /// The approval tenantInfo helper
    /// </summary>
    private readonly IApprovalTenantInfoHelper _approvalTenantInfoHelper;

    /// <summary>
    /// The Http Helper
    /// </summary>
    private readonly IHttpHelper _httpHelper;

    /// <summary>
    /// The log provider
    /// </summary>
    private readonly ILogProvider _logProvider;

    /// <summary>
    /// The performance _logger
    /// </summary>
    private readonly IPerformanceLogger _logger;

    /// <summary>
    /// The configuration
    /// </summary>
    private readonly IConfiguration _config;

    /// <summary>
    /// The tenant factory
    /// </summary>
    private readonly ITenantFactory _tenantFactory;

    /// <summary>
    /// The authentication helper
    /// </summary>
    private readonly IAuthenticationHelper _authenticationHelper;

    /// <summary>
    /// Constructor of DelegationHelper
    /// </summary>
    /// <param name="userDelegationProvider"></param>
    /// <param name="httpHelper"></param>
    /// <param name="logProvider"></param>
    /// <param name="nameResolutionHelper"></param>
    /// <param name="logger"></param>
    /// <param name="config"></param>
    /// <param name="approvalTenantInfoHelper"></param>
    /// <param name="tenantFactory"></param>
    /// <param name="authenticationHelper"></param>
    public DelegationHelper(IUserDelegationProvider userDelegationProvider,
        IHttpHelper httpHelper,
        ILogProvider logProvider,
        INameResolutionHelper nameResolutionHelper,
        IPerformanceLogger logger,
        IConfiguration config,
        IApprovalTenantInfoHelper approvalTenantInfoHelper,
        ITenantFactory tenantFactory,
        IAuthenticationHelper authenticationHelper)
    {
        _userDelegationProvider = userDelegationProvider;
        _httpHelper = httpHelper;
        _logProvider = logProvider;
        _nameResolutionHelper = nameResolutionHelper;
        _logger = logger;
        _config = config;
        _approvalTenantInfoHelper = approvalTenantInfoHelper;
        _tenantFactory = tenantFactory;
        _authenticationHelper = authenticationHelper;
    }

    /// <summary>
    /// Gets the delegation access level.
    /// </summary>
    /// <param name="manager">The manager user entity.</param>
    /// <param name="delegateToUser">The delegate to user entity.</param>
    /// <returns></returns>
    public DelegationAccessLevel GetDelegationAccessLevel(User manager, User delegateToUser)
    {
        return _userDelegationProvider.GetDelegationAccessLevel(manager, delegateToUser);
    }

    /// <summary>
    /// Gets the information of people delegated to me.
    /// </summary>
    /// <param name="signedInUser">The logged in user entity.</param>
    /// <param name="onBehalfUser">The on behalf user entity.</param>
    /// <param name="clientDevice">The client device.</param>
    /// <param name="sessionId">The session identifier.</param>
    /// <param name="xcv">The xcv</param>
    /// <param name="tcv">The tcv</param>
    /// <returns>Returns delegated users in forms of dynamic list</returns>
    /// <exception cref="System.UnauthorizedAccessException">You are not allowed to query for a different user.</exception>
    public async Task<List<dynamic>> GetInfoOfPeopleDelegatedToMe(User signedInUser, User onBehalfUser, string clientDevice, string sessionId, string xcv, string tcv)
    {
        #region Logging

        xcv = !string.IsNullOrWhiteSpace(xcv) ? xcv : Guid.NewGuid().ToString();
        tcv = !string.IsNullOrWhiteSpace(tcv) ? tcv : xcv;
        sessionId = !string.IsNullOrWhiteSpace(sessionId) ? sessionId : xcv;

        // Add common data items to LogData
        var logData = new Dictionary<LogDataKey, object>
        {
            { LogDataKey.StartDateTime, DateTime.UtcNow },
            { LogDataKey.Xcv, xcv },
            { LogDataKey.Tcv, tcv },
            { LogDataKey.SessionId, sessionId },
            { LogDataKey.UserRoleName, signedInUser.UserPrincipalName },
            { LogDataKey.ClientDevice, clientDevice },
            { LogDataKey.EventType, Constants.FeatureUsageEvent },
            { LogDataKey.UserAlias, onBehalfUser.UserPrincipalName },
            { LogDataKey.IsCriticalEvent, CriticalityLevel.No.ToString() }
        };

        #endregion Logging

        try
        {
            using (_logger.StartPerformanceLogger("PerfLog", "User Delegation Settings", string.Format(Constants.PerfLogCommon, "User Delegation Settings GetPeopleDelegatedToMe"), logData))
            {
                var results = new List<dynamic>();
                var delegations = new List<dynamic>();

                List<UserDelegationSetting> userDelegationRows = GetPeopleDelegatedToMe(signedInUser);
                if (userDelegationRows == null || !userDelegationRows.Any())
                {
                    return null;
                }

                foreach (UserDelegationSetting delegation in userDelegationRows)
                {
                    delegations.Add(new
                    {
                        Name = await GetUserFullName(delegation.ManagerAlias),
                        Alias = delegation.ManagerAlias,
                        DelegatorUpn = delegation.DelegatorUpn,
                        DelegatorId = delegation.DelegatorId,
                        AccessPermission = new { Level = delegation.AccessType }
                    });
                }
                results.Add(new
                {
                    AppId = Constants.ServiceTreeAppId,
                    AppName = Constants.ApplicationName,
                    Delegations = delegations
                });

                // Log Success
                _logProvider.LogInformation(TrackingEvent.WebApiImpersonationReadSuccess, logData);
                return results;
            }
        }
        catch (Exception ex)
        {
            _logProvider.LogError(TrackingEvent.WebApiImpersonationReadFail, ex, logData);
            throw;
        }
    }

    /// <summary>
    /// Gets the people delegated to me.
    /// </summary>
    /// <param name="signedInUser">The logged in user entity.</param>
    /// <returns></returns>
    public List<UserDelegationSetting> GetPeopleDelegatedToMe(User signedInUser)
    {
        return _userDelegationProvider.GetPeopleDelegatedToMe(signedInUser);
    }

    /// <summary>
    /// Gets the user delegations for current user.
    /// </summary>
    /// <param name="signedInUser">The logged in user entity.</param>
    /// <returns></returns>
    public List<UserDelegationSetting> GetUserDelegationsForCurrentUser(User signedInUser)
    {
        return _userDelegationProvider.GetUserDelegationsForCurrentUser(signedInUser);
    }

    /// <summary>
    /// This method will fetch deleged users for loggedIn user
    /// </summary>
    /// <param name="signedInUser">The logged in user entity</param>
    /// <param name="onBehalfUser">The on behalf user entity</param>
    /// <param name="tenantId">The TenanId</param>
    /// <param name="clientDevice">The ClientDevice</param>
    /// <param name="sessionId">The SessionId</param>
    /// <param name="xcv">The xcv</param>
    /// <param name="tcv">The tcv</param>
    /// <returns>Returns delegated users in forms of JsonObject</returns>
    public async Task<JArray> GetUsersDelegatedToAsync(User signedInUser, User onBehalfUser, int tenantId, string clientDevice, string sessionId, string xcv, string tcv)
    {
        #region Logging

        xcv = !string.IsNullOrWhiteSpace(xcv) ? xcv : Guid.NewGuid().ToString();
        tcv = !string.IsNullOrWhiteSpace(tcv) ? tcv : xcv;
        sessionId = !string.IsNullOrWhiteSpace(sessionId) ? sessionId : xcv;

        var logData = new Dictionary<LogDataKey, object>
        {
            { LogDataKey.StartDateTime, DateTime.UtcNow },
            { LogDataKey.Xcv, xcv },
            { LogDataKey.Tcv, tcv },
            { LogDataKey.SessionId, sessionId },
            { LogDataKey.TenantId, tenantId },
            { LogDataKey.UserRoleName, signedInUser.UserPrincipalName },
            { LogDataKey.UserAlias, onBehalfUser.UserPrincipalName },
            { LogDataKey.EventType, Constants.FeatureUsageEvent },
            { LogDataKey.IsCriticalEvent, CriticalityLevel.No.ToString() },
            { LogDataKey.ClientDevice, clientDevice }
        };

        #endregion Logging

        try
        {
            using (_logger.StartPerformanceLogger("PerfLog", "User Delegation Settings", string.Format(Constants.PerfLogCommon, "User Delegation Settings Get"), logData))
            {
                // Fetch tenant information.
                var tenant = _approvalTenantInfoHelper.GetTenantInfo(tenantId);

                // If tenant info cannot be read, summary cannot be fetched and hence throw an exception
                if (tenant == null)
                {
                    throw new ArgumentNullException(nameof(tenant), "Tenant Info is null");
                }

                logData.Add(LogDataKey.TenantName, !string.IsNullOrWhiteSpace(tenant.AppName) ? tenant.AppName : string.Empty);

                var tenantAdapter = _tenantFactory.GetTenant(tenant);

                Dictionary<string, object> parameters = new Dictionary<string, object> { { "alias", onBehalfUser.MailNickname } };

                // Get the details of an approval request from tenant system.
                var httpResponseMessage = await tenantAdapter.GetUsersDelegatedToAsync(onBehalfUser, parameters, clientDevice, xcv, tcv, sessionId);
                logData.Add(LogDataKey.EndDateTime, DateTime.UtcNow);
                var responseString = await httpResponseMessage.Content.ReadAsStringAsync();

                if (!httpResponseMessage.IsSuccessStatusCode)
                {
                    logData.Add(LogDataKey.ResponseContent, responseString);
                    throw new WebException("Status Code: " + httpResponseMessage.StatusCode.ToString() + " " + responseString, WebExceptionStatus.ReceiveFailure);
                }

                // Log Success event.
                logData.Modify(LogDataKey.EndDateTime, DateTime.UtcNow);
                _logProvider.LogInformation(TrackingEvent.WebApiImpersonationReadSuccess, logData);

                return (responseString).ToJArray();
            }
        }
        catch (Exception ex)
        {
            // Log failure event.
            logData.Modify(LogDataKey.EndDateTime, DateTime.UtcNow);
            _logProvider.LogError(TrackingEvent.WebApiImpersonationSettingsReadFail, ex, logData);
            throw;
        }
    }

    /// <summary>
    /// Get Delegation Platform API response
    /// </summary>
    /// <param name="signedInUser"></param>
    /// <param name="onBehalfUser"></param>
    /// <param name="oauth2UserToken"></param>
    /// <param name="clientDevice"></param>
    /// <param name="sessionId"></param>
    /// <param name="xcv"></param>
    /// <param name="tcv"></param>
    /// <returns></returns>
    public async Task<List<DelegationPlatformResponse>> GetUserDelegation(User signedInUser, User onBehalfUser, string oauth2UserToken, string clientDevice, string sessionId, string xcv, string tcv)
    {
        #region Logging

        xcv = !string.IsNullOrWhiteSpace(xcv) ? xcv : Guid.NewGuid().ToString();
        tcv = !string.IsNullOrWhiteSpace(tcv) ? tcv : xcv;
        sessionId = !string.IsNullOrWhiteSpace(sessionId) ? sessionId : xcv;

        // Add common data items to LogData
        var logData = new Dictionary<LogDataKey, object>
        {
            { LogDataKey.StartDateTime, DateTime.UtcNow },
            { LogDataKey.Xcv, xcv },
            { LogDataKey.Tcv, tcv },
            { LogDataKey.SessionId, sessionId },
            { LogDataKey.UserRoleName, signedInUser.UserPrincipalName },
            { LogDataKey.ClientDevice, clientDevice },
            { LogDataKey.EventType, Constants.FeatureUsageEvent },
            { LogDataKey.UserAlias, onBehalfUser.UserPrincipalName },
            { LogDataKey.IsCriticalEvent, CriticalityLevel.No.ToString() }
        };

        #endregion Logging

        try
        {
            var delegationUri = string.Format(_config[ConfigurationKey.DelegationPlatformApi.ToString()],
                                _config[ConfigurationKey.DelegationPlatformAppId.ToString()], signedInUser.Id);
            logData.Add(LogDataKey.Uri, delegationUri);

            HttpRequestMessage requestMessage = new HttpRequestMessage(HttpMethod.Get, delegationUri);

            var accessToken = await _authenticationHelper.GetOnBehalfUserToken(oauth2UserToken.Replace("Bearer ", string.Empty).Replace("bearer ", string.Empty),
                                JObject.FromObject(new
                                {
                                    ClientID = _config[ConfigurationKey.MSAInternalClientId.ToString()],
                                    ResourceURL = _config[ConfigurationKey.DelegationPlatformResourceUrl.ToString()],
                                    Authority = _config[ConfigurationKey.Authority.ToString()]
                                }),
                                _config[ConfigurationKey.ManagedIdentityClientId.ToString()],
                                _config[ConfigurationKey.ManagedIdentityFederatedAudience.ToString()]);
            logData.Add(LogDataKey.IdentityProviderTokenType, "OnBehalfUserToken");

            requestMessage.Headers.Authorization = new AuthenticationHeaderValue(Constants.AuthorizationHeaderScheme, accessToken);

            var response = await _httpHelper.SendRequestAsync(requestMessage);
            var responseData = await response.Content.ReadAsStringAsync();
            
            if (!response.IsSuccessStatusCode)
            {
                logData.Add(LogDataKey.ResponseContent, responseData);
                throw new WebException("Status Code: " + response.StatusCode.ToString() + " " + responseData, WebExceptionStatus.ReceiveFailure);
            }

            var delegationDataList = JsonConvert.DeserializeObject<List<DelegationPlatformResponse>>(responseData);
            Parallel.ForEach(delegationDataList, delegationData =>
            {
                delegationData.IsDelegationPlatform = true;
            });

            _logProvider.LogInformation(TrackingEvent.WebApiDelegationPlatformSuccess, logData);
            return delegationDataList;
        }
        catch (Exception ex)
        {
            // Log failure event.
            logData.Modify(LogDataKey.EndDateTime, DateTime.UtcNow);
            _logProvider.LogError(TrackingEvent.WebApiDelegationPlatformFail, ex, logData);
            return null;
        }
    }

    /// <summary>
    /// Gets delegation data from Delegation Platform API and UserDelegationSettings table
    /// </summary>
    /// <param name="signedInUser"></param>
    /// <param name="onBehalfUser"></param>
    /// <param name="oauth2UserToken"></param>
    /// <param name="clientDevice"></param>
    /// <param name="sessionId"></param>
    /// <param name="xcv"></param>
    /// <param name="tcv"></param>
    /// <returns></returns>
    public async Task<JArray> GetMergedDelegationData(User signedInUser, User onBehalfUser, string oauth2UserToken, string clientDevice, string sessionId, string xcv, string tcv)
    {
        #region Logging

        xcv = !string.IsNullOrWhiteSpace(xcv) ? xcv : Guid.NewGuid().ToString();
        tcv = !string.IsNullOrWhiteSpace(tcv) ? tcv : xcv;
        sessionId = !string.IsNullOrWhiteSpace(sessionId) ? sessionId : xcv;

        // Add common data items to LogData
        var logData = new Dictionary<LogDataKey, object>
        {
            { LogDataKey.StartDateTime, DateTime.UtcNow },
            { LogDataKey.Xcv, xcv },
            { LogDataKey.Tcv, tcv },
            { LogDataKey.SessionId, sessionId },
            { LogDataKey.UserRoleName, signedInUser.UserPrincipalName },
            { LogDataKey.ClientDevice, clientDevice },
            { LogDataKey.EventType, Constants.FeatureUsageEvent },
            { LogDataKey.UserAlias, onBehalfUser.UserPrincipalName },
            { LogDataKey.IsCriticalEvent, CriticalityLevel.No.ToString() }
        };

        #endregion Logging
        var delegationApiResponse = await GetUserDelegation(signedInUser, onBehalfUser, oauth2UserToken, clientDevice, sessionId, xcv, tcv);
        var userDelegationSettingsResponse = await GetInfoOfPeopleDelegatedToMe(signedInUser, onBehalfUser, clientDevice, sessionId, xcv, tcv);

        if (delegationApiResponse == null && userDelegationSettingsResponse == null)
        {
            return new JArray(); // Return empty if data is unavailable
        }
        var mergedResponse = new JArray();

        //Processing and merging delegation API response
        var groupedDelegationData = delegationApiResponse != null ? delegationApiResponse.GroupBy(delegation => delegation.Delegator.UserPrincipalName).ToList() : new List<IGrouping<string, DelegationPlatformResponse>>();
        foreach (var delegationGroup in groupedDelegationData)
        {
            var appList = new JArray();
            foreach (var delegationEntry in delegationGroup)
            {
                var app = JObject.FromObject(new
                {
                    appId = delegationEntry.AppId,
                    appName = delegationEntry.AppName,
                    isDelegationPlatform = true
                });
                appList.Add(app);
            }

            var delegatorObject = JObject.FromObject(new
            {
                upn = delegationGroup.Key,
                apps = appList,
                delegator = delegationGroup.First().Delegator
            });

            mergedResponse.Add(delegatorObject);
        }

        // Processing and merging delegation table response
        var delegationTableResponse = userDelegationSettingsResponse != null ? userDelegationSettingsResponse.ToJson().FromJson<JArray>() : new JArray();
        foreach (var delegationItem in delegationTableResponse)
        {
            foreach (var delegationDetail in delegationItem["Delegations"])
            {
                var matchedDelegator = mergedResponse
                    .Where(response => response["upn"].ToString().Equals(delegationDetail["DelegatorUpn"].ToString()))
                    .FirstOrDefault();

                if (matchedDelegator != null)
                {
                    var existingApps = JArray.Parse(matchedDelegator["apps"].ToString());
                    existingApps.Add(JObject.FromObject(new
                    {
                        appId = delegationItem["AppId"],
                        appName = delegationItem["AppName"],
                        isDelegationPlatform = false
                    }));
                    matchedDelegator["apps"] = existingApps;
                }
                else
                {
                    var newApp = JObject.FromObject(new
                    {
                        appId = delegationItem["AppId"],
                        appName = delegationItem["AppName"],
                        isDelegationPlatform = false
                    });

                    var newAppList = new JArray { newApp };

                    var newDelegationObject = JObject.FromObject(new
                    {
                        upn = delegationDetail["DelegatorUpn"],
                        apps = newAppList,
                        delegator = JObject.FromObject(new
                        {
                            Id = delegationDetail["DelegatorId"],
                            DisplayName = delegationDetail["Name"],
                            UserPrincipalName = delegationDetail["DelegatorUpn"]
                        })
                    });

                    mergedResponse.Add(newDelegationObject);
                }
            }
        }

        return mergedResponse;
    }

    /// <summary>
    /// Check if the user is authorized to view the report based on delegation settings
    /// </summary>
    /// <param name="signedInUser"></param>
    /// <param name="onBehalfUser"></param>
    /// <param name="oauth2UserToken"></param>
    /// <param name="clientDevice"></param>
    /// <param name="sessionId"></param>
    /// <param name="xcv"></param>
    /// <param name="tcv"></param>
    /// <returns></returns>
    /// <exception cref="UnauthorizedAccessException"></exception>
    public async Task<bool> CheckUserAuthorization(User signedInUser, User onBehalfUser, string oauth2UserToken, string clientDevice, string sessionId, string xcv, string tcv)
    {
        //if loggedInAlias != current alias (Alias), that mean current user is in delegate mode. So enable the UserDelegation flag checking when creating the Summary list.
        if (!signedInUser.UserPrincipalName.Equals(onBehalfUser.UserPrincipalName, StringComparison.OrdinalIgnoreCase))
        {
            // check if entry made from support portal
            if (GetUserDelegationsForCurrentUser(onBehalfUser)
                    ?.FirstOrDefault(d => d.DelegateUpn == signedInUser.UserPrincipalName && d.IsHidden == true) == null &&
                (await GetUserDelegation(signedInUser, onBehalfUser, oauth2UserToken, clientDevice, sessionId, xcv, tcv))
                    ?.FirstOrDefault(t => t.Delegator.UserPrincipalName.Equals(onBehalfUser.UserPrincipalName)) == null)
            {
                throw new UnauthorizedAccessException("User doesn't have permission to see the report.");
            }
        }
        return true;
    }

    #region Helper Methods

    /// <summary>
    /// Get user full name
    /// </summary>
    /// <param name="alias"></param>
    /// <returns></returns>
    private async Task<string> GetUserFullName(string alias)
    {
        string userName = await _nameResolutionHelper.GetUserName(alias);
        return userName;
    }

    #endregion Helper Methods
}