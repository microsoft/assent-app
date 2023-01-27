// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace Microsoft.CFS.Approvals.Core.BL.Helpers;

using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Net;
using System.Net.Mail;
using System.Threading.Tasks;
using Microsoft.CFS.Approvals.Common.DL.Interface;
using Microsoft.CFS.Approvals.Contracts;
using Microsoft.CFS.Approvals.Core.BL.Interface;
using Microsoft.CFS.Approvals.Extensions;
using Microsoft.CFS.Approvals.LogManager.Model;
using Microsoft.CFS.Approvals.LogManager.Provider.Interface;
using Microsoft.CFS.Approvals.Model.Flighting;
using Microsoft.CFS.Approvals.Utilities.Interface;
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
    /// The log provider
    /// </summary>
    private readonly ILogProvider _logProvider;

    /// <summary>
    /// The performance _logger
    /// </summary>
    private readonly IPerformanceLogger _logger;

    /// <summary>
    /// The tenant factory
    /// </summary>
    private readonly ITenantFactory _tenantFactory;

    /// <summary>
    /// Constructor of DelegationHelper
    /// </summary>
    /// <param name="_userDelegationProvider"></param>
    /// <param name="_logProvider"></param>
    /// <param name="_nameResolutionHelper"></param>
    /// <param name="_logger"></param>
    /// <param name="_approvalTenantInfoHelper"></param>
    /// <param name="_tenantFactory"></param>
    public DelegationHelper(IUserDelegationProvider userDelegationProvider,
        ILogProvider logProvider,
        INameResolutionHelper nameResolutionHelper,
        IPerformanceLogger logger,
        IApprovalTenantInfoHelper approvalTenantInfoHelper,
        ITenantFactory tenantFactory)
    {
        _userDelegationProvider = userDelegationProvider;
        _logProvider = logProvider;
        _nameResolutionHelper = nameResolutionHelper;
        _logger = logger;
        _approvalTenantInfoHelper = approvalTenantInfoHelper;
        _tenantFactory = tenantFactory;
    }

    /// <summary>
    /// Delete Delegation Settings.
    /// </summary>
    /// <param name="delegationRow"></param>
    /// <param name="sessionId"></param>
    /// <param name="clientDevice"></param>
    public void DeleteDelegationSettings(UserDelegationSetting delegationRow, string sessionId = "", string clientDevice = "")
    {
        #region Logging

        var Tcv = Guid.NewGuid().ToString();

        // Add common data items to LogData
        var logData = new Dictionary<LogDataKey, object>
        {
            { LogDataKey.Xcv, Tcv },
            { LogDataKey.Tcv, Tcv },
            { LogDataKey.SessionId, sessionId },
            { LogDataKey.UserRoleName, delegationRow.PartitionKey },
            { LogDataKey.ClientDevice, clientDevice },
            { LogDataKey.EventType, Constants.FeatureUsageEvent },
            { LogDataKey.UserAlias, delegationRow.DelegatedToAlias },
            { LogDataKey.IsCriticalEvent, CriticalityLevel.No.ToString() }
        };

        #endregion Logging

        _userDelegationProvider.DeleteDelegationSettings(delegationRow);
        _logProvider.LogInformation(TrackingEvent.WebApiImpersonationSettingsDeleteSuccess, logData);
    }

    /// <summary>
    /// Gets all user delegation settings.
    /// </summary>
    /// <returns></returns>
    public List<UserDelegationSetting> GetAllUserDelegationSettings()
    {
        return _userDelegationProvider.GetAllUserDelegationSettings();
    }

    /// <summary>
    /// Gets the delegation access level.
    /// </summary>
    /// <param name="managerAlias">The manager alias.</param>
    /// <param name="delegateToAlias">The delegate to alias.</param>
    /// <returns></returns>
    public DelegationAccessLevel GetDelegationAccessLevel(string managerAlias, string delegateToAlias)
    {
        return _userDelegationProvider.GetDelegationAccessLevel(managerAlias, delegateToAlias);
    }

    /// <summary>
    /// Gets the delegation from.
    /// </summary>
    /// <param name="delegatedToAlias">The delegated to alias.</param>
    /// <returns></returns>
    public UserDelegationSetting GetDelegationFrom(string delegatedToAlias)
    {
        return _userDelegationProvider.GetDelegationFrom(delegatedToAlias);
    }

    /// <summary>
    /// Gets the information of people delegated to me.
    /// </summary>
    /// <param name="loggedInUserAlias">The logged in user alias.</param>
    /// <param name="alias">The alias.</param>
    /// <param name="clientDevice">The client device.</param>
    /// <param name="sessionId">The session identifier.</param>
    /// <param name="xcv">The xcv</param>
    /// <param name="tcv">The tcv</param>
    /// <returns>Returns delegated users in forms of dynamic list</returns>
    /// <exception cref="System.UnauthorizedAccessException">You are not allowed to query for a different user.</exception>
    public async Task<List<dynamic>> GetInfoOfPeopleDelegatedToMe(string loggedInUserAlias, string alias, string clientDevice, string sessionId, string xcv, string tcv)
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
            { LogDataKey.UserRoleName, loggedInUserAlias },
            { LogDataKey.ClientDevice, clientDevice },
            { LogDataKey.EventType, Constants.FeatureUsageEvent },
            { LogDataKey.UserAlias, alias },
            { LogDataKey.IsCriticalEvent, CriticalityLevel.No.ToString() }
        };

        #endregion Logging

        try
        {
            using (_logger.StartPerformanceLogger("PerfLog", "User Delegation Settings", string.Format(Constants.PerfLogCommon, "User Delegation Settings GetPeopleDelegatedToMe"), logData))
            {
                var results = new List<dynamic>();
                var delegations = new List<dynamic>();

                List<UserDelegationSetting> userDelegationRows = GetPeopleDelegatedToMe(loggedInUserAlias);
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
    /// <param name="loggedInUserAlias">The logged in user alias.</param>
    /// <returns></returns>
    public List<UserDelegationSetting> GetPeopleDelegatedToMe(string loggedInUserAlias)
    {
        if (loggedInUserAlias.Contains("@"))
        {
            loggedInUserAlias = new MailAddress(loggedInUserAlias).User;
        }

        return _userDelegationProvider.GetPeopleDelegatedToMe(loggedInUserAlias);
    }

    /// <summary>
    /// Gets the user delegation settings by identifier.
    /// </summary>
    /// <param name="rowKey">The row key.</param>
    /// <returns></returns>
    public UserDelegationSetting GetUserDelegationSettingsById(string rowKey)
    {
        return _userDelegationProvider.GetUserDelegationSettingsById(rowKey);
    }

    /// <summary>
    /// Gets the user delegation settings from.
    /// </summary>
    /// <param name="loggedInUserAlias">The logged in user alias.</param>
    /// <param name="tenantId">The tenant identifier.</param>
    /// <param name="delegatedTo">The delegated to.</param>
    /// <returns></returns>
    public List<UserDelegationSetting> GetUserDelegationSettingsFrom(string loggedInUserAlias, int tenantId, string delegatedTo)
    {
        return _userDelegationProvider.GetUserDelegationSettingsFrom(loggedInUserAlias, tenantId, delegatedTo);
    }

    /// <summary>
    /// Gets the user delegations for current user.
    /// </summary>
    /// <param name="loggedInUserAlias">The logged in user alias.</param>
    /// <returns></returns>
    public List<UserDelegationSetting> GetUserDelegationsForCurrentUser(string loggedInUserAlias)
    {
        return _userDelegationProvider.GetUserDelegationsForCurrentUser(loggedInUserAlias);
    }

    /// <summary>
    /// This method will fetch deleged users for loggedIn user
    /// </summary>
    /// <param name="loggedInUserAlias">The logged in user alias.s</param>
    /// <param name="alias">The alias</param>
    /// <param name="tenantId">The TenanId</param>
    /// <param name="clientDevice">The ClientDevice</param>
    /// <param name="sessionId">The SessionId</param>
    /// <param name="xcv">The xcv</param>
    /// <param name="tcv">The tcv</param>
    /// <returns>Returns delegated users in forms of JsonObject</returns>
    public async Task<JArray> GetUsersDelegatedToAsync(string loggedInUserAlias, string alias, int tenantId, string clientDevice, string sessionId, string xcv, string tcv)
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
            { LogDataKey.UserRoleName, loggedInUserAlias },
            { LogDataKey.UserAlias, alias },
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

                Dictionary<string, object> parameters = new Dictionary<string, object> { { "alias", alias } };

                // Get the details of an approval request from tenant system.
                var httpResponseMessage = await tenantAdapter.GetUsersDelegatedToAsync(alias, parameters, clientDevice, xcv, tcv, sessionId);
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
    /// Inserts the delegation settings.
    /// </summary>
    /// <param name="insertData">The insert data.</param>
    /// <param name="loggedInUserAlias">The logged in user alias.</param>
    /// <param name="clientDevice"> ClientDevice i.e. device/component through which the action will be taken</param>
    public void InsertDelegationSettings(UserDelegationSetting insertData, string loggedInUserAlias, string clientDevice)
    {
        #region Logging

        // Add common data items to LogData
        var logData = new Dictionary<LogDataKey, object>
        {
            {LogDataKey.ManagerAlias, insertData.ManagerAlias},
            {LogDataKey.DelegatedToAlias, insertData.DelegatedToAlias},
            {LogDataKey.TenantId, insertData.TenantId.ToString()},
            {LogDataKey.StartDateTime, insertData.DateFrom.ToString(CultureInfo.InvariantCulture)},
            {LogDataKey.EndDateTime, insertData.DateTo.ToString(CultureInfo.InvariantCulture)},
            {LogDataKey.DelegationAccessType, insertData.AccessType.ToString()},
            {LogDataKey.DelegationIsHidden, insertData.IsHidden.ToString()},
            {LogDataKey.ClientDevice, clientDevice},
            {LogDataKey.UserRoleName, loggedInUserAlias}
        };

        #endregion Logging

        try
        {
            _userDelegationProvider.InsertDelegationSettings(insertData);
            _logProvider.LogInformation(TrackingEvent.DelegationInsert, logData);
        }
        catch (Exception ex)
        {
            _logProvider.LogError(TrackingEvent.DelegationInsertFailed, ex, logData);
            throw;
        }
    }

    /// <summary>
    /// Processes the and insert delegation.
    /// </summary>
    /// <param name="loggedInUserAlias">The logged in user alias.</param>
    /// <param name="alias">The alias.</param>
    /// <param name="clientDevice">The client device.</param>
    /// <param name="sessionId">The session identifier.</param>
    /// <param name="jsonData">The json data.</param>
    /// <exception cref="Model.CustomException">
    /// </exception>
    public async Task ProcessAndInsertDelegation(string loggedInUserAlias, string alias, string clientDevice, string sessionId, string jsonData)
    {
        #region Logging

        var Tcv = Guid.NewGuid().ToString();

        // Add common data items to LogData
        var logData = new Dictionary<LogDataKey, object>
        {
            { LogDataKey.Xcv, Tcv },
            { LogDataKey.Tcv, Tcv },
            { LogDataKey.SessionId, sessionId },
            { LogDataKey.UserRoleName, loggedInUserAlias },
            { LogDataKey.ClientDevice, clientDevice },
            { LogDataKey.EventType, Constants.FeatureUsageEvent },
            { LogDataKey.UserAlias, alias },
            { LogDataKey.IsCriticalEvent, CriticalityLevel.No.ToString() }
        };

        #endregion Logging

        try
        {
            using (_logger.StartPerformanceLogger("PerfLog", "User Delegation Settings", string.Format(Constants.PerfLogCommon, "User Delegation Settings Post"), logData))
            {
                var allUserDelegationsData = GetUserDelegationsForCurrentUser(loggedInUserAlias);
                int tenantId = 0, accessType = 0;

                var receivedData = jsonData.FromJson<JObject>();

                if (Convert.ToBoolean(receivedData["AccessType"]))
                {
                    accessType = 1;
                }

                if (DateTime.Parse(receivedData["DateFrom"].ToString()) > DateTime.Parse(receivedData["DateTo"].ToString()))
                {
                    throw new Model.CustomException(Constants.UserDelegationEndDateError);
                }

                if ((allUserDelegationsData.Where(u => !u.IsHidden).ToList().Count == 0 || allUserDelegationsData.Where(u => !u.IsHidden && u.AccessType == accessType).ToList().Count == 0)
                    && loggedInUserAlias != receivedData["DelegatedTo"].ToString())
                {
                    var delegation = new UserDelegationSetting { ManagerAlias = loggedInUserAlias, TenantId = tenantId, DateFrom = DateTime.Parse(receivedData["DateFrom"].ToString()), DateTo = DateTime.Parse(receivedData["DateTo"].ToString()), IsHidden = false, DelegatedToAlias = receivedData["DelegatedTo"].ToString(), AccessType = accessType };

                    if (await IsValidAlias(delegation.DelegatedToAlias))
                    {
                        try
                        {
                            InsertDelegationSettings(delegation, loggedInUserAlias, clientDevice);
                        }
                        catch
                        {
                            throw new Model.CustomException(Constants.UserDelegationPostError);
                        }
                    }
                    else
                    {
                        throw new Model.CustomException(Constants.InvalidDelegateAlias);
                    }
                }
                else
                {
                    throw new Model.CustomException(Constants.ExistingDelegateError);
                }

                // Log Success
                _logProvider.LogInformation(TrackingEvent.WebApiImpersonationSettingsCreateSuccess, logData);
            }
        }
        catch (Exception ex)
        {
            _logProvider.LogError(TrackingEvent.WebApiImpersonationSettingsCreateFail, ex, logData);
            throw;
        }
    }

    /// <summary>
    /// Processes the and update delegation.
    /// </summary>
    /// <param name="loggedInUserAlias">The logged in user alias.</param>
    /// <param name="alias">The alias.</param>
    /// <param name="clientDevice">The client device.</param>
    /// <param name="sessionId">The session identifier.</param>
    /// <param name="jsonData">The json data.</param>
    /// <exception cref="Model.CustomException">
    /// </exception>
    public async Task ProcessAndUpdateDelegation(string loggedInUserAlias, string alias, string clientDevice, string sessionId, string jsonData)
    {
        #region Logging

        var Tcv = Guid.NewGuid().ToString();

        // Add common data items to LogData
        var logData = new Dictionary<LogDataKey, object>
        {
            { LogDataKey.Xcv, Tcv },
            { LogDataKey.Tcv, Tcv },
            { LogDataKey.SessionId, sessionId },
            { LogDataKey.UserRoleName, loggedInUserAlias },
            { LogDataKey.ClientDevice, clientDevice },
            { LogDataKey.EventType, Constants.FeatureUsageEvent },
            { LogDataKey.UserAlias, alias },
            { LogDataKey.IsCriticalEvent, CriticalityLevel.No.ToString() }
        };

        #endregion Logging

        try
        {
            using (_logger.StartPerformanceLogger("PerfLog", "User Delegation Settings", string.Format(Constants.PerfLogCommon, "User Delegation Settings Delete"), new Dictionary<LogDataKey, object>()))
            {
                int tenantId = 0, accessType = 0;
                var receivedData = jsonData.FromJson<JObject>();
                if (Convert.ToBoolean(receivedData["AccessType"]))
                {
                    accessType = 1;
                }
                if (DateTime.Parse(receivedData["DateFrom"].ToString()) > DateTime.Parse(receivedData["DateTo"].ToString()))
                {
                    throw new Model.CustomException(Constants.UserDelegationEndDateError);
                }

                var delegation = new UserDelegationSetting { Id = Int32.Parse(receivedData["Id"].ToString()), ManagerAlias = loggedInUserAlias, TenantId = tenantId, DateFrom = DateTime.Parse(receivedData["DateFrom"].ToString()), DateTo = DateTime.Parse(receivedData["DateTo"].ToString()), IsHidden = false, DelegatedToAlias = receivedData["DelegatedTo"].ToString(), AccessType = accessType, PartitionKey = receivedData["PartitionKey"].ToString(), RowKey = receivedData["RowKey"].ToString() };
                var oldDelegatedTo = receivedData["OldDelegatedTo"].ToString();
                if (await IsValidAlias(delegation.DelegatedToAlias))
                {
                    try
                    {
                        UpdateDelegationSettings(delegation, tenantId, oldDelegatedTo, loggedInUserAlias, clientDevice, sessionId);
                    }
                    catch
                    {
                        throw new Model.CustomException(Constants.UserDelegationPostError);
                    }
                }
                else
                {
                    throw new Model.CustomException(Constants.InvalidDelegateAlias);
                }
                _logProvider.LogInformation(TrackingEvent.WebApiImpersonationSettingsUpdateSuccess, logData);
            }
        }
        catch (Exception ex)
        {
            _logProvider.LogError(TrackingEvent.WebApiImpersonationSettingsUpdateFail, ex, logData);
            throw;
        }
    }

    /// <summary>
    /// Updates the delegation settings.
    /// </summary>
    /// <param name="delegationRow">The delegation row.</param>
    /// <param name="oldTenantId">The old tenant identifier.</param>
    /// <param name="oldDelegatedUser">The old delegated user.</param>
    /// <param name="loggedInUserAlias">The logged in user alias.</param>
    /// <param name="clientDevice"> ClientDevice i.e. device/component through which the action will be taken</param>
    /// <param name="sessionId">The session identifier.</param>
    public void UpdateDelegationSettings(UserDelegationSetting delegationRow, int oldTenantId, string oldDelegatedUser, string loggedInUserAlias, string clientDevice, string sessionId)
    {
        #region Logging

        var Tcv = Guid.NewGuid().ToString();

        // Add common data items to LogData
        var logData = new Dictionary<LogDataKey, object>
        {
            { LogDataKey.Xcv, Tcv },
            { LogDataKey.Tcv, Tcv },
            { LogDataKey.SessionId, sessionId },
            { LogDataKey.ManagerAlias, delegationRow.ManagerAlias },
            { LogDataKey.DelegatedToAlias, delegationRow.DelegatedToAlias },
            { LogDataKey.TenantId, delegationRow.TenantId.ToString() },
            { LogDataKey.StartDateTime, delegationRow.DateFrom.ToString(CultureInfo.InvariantCulture) },
            { LogDataKey.EndDateTime, delegationRow.DateTo.ToString(CultureInfo.InvariantCulture) },
            { LogDataKey.DelegationAccessType, delegationRow.AccessType.ToString() },
            { LogDataKey.DelegationIsHidden, delegationRow.IsHidden.ToString() },
            { LogDataKey.ClientDevice, clientDevice },
            { LogDataKey.UserRoleName, loggedInUserAlias }
        };

        #endregion Logging

        try
        {
            _userDelegationProvider.UpdateDelegationSettings(delegationRow, oldTenantId, oldDelegatedUser);

            _logProvider.LogInformation(TrackingEvent.DelegationUpdate, logData);
        }
        catch (Exception ex)
        {
            _logProvider.LogError(TrackingEvent.DelegationUpdateFailed, ex, logData);
            throw;
        }
    }

    #region Helper Methods

    /// <summary>
    /// Gets the name of the tenant.
    /// </summary>
    /// <param name="tenantId">The tenant identifier.</param>
    /// <returns></returns>
    private string GetTenantName(int tenantId)
    {
        if (tenantId > 0)
        {
            Model.ApprovalTenantInfo tenantInfo = _approvalTenantInfoHelper.GetTenantInfo(tenantId);
            var tenant = tenantInfo.AppName;
            return tenant;
        }

        return " ";
    }

    /// <summary>
    /// Determines whether [is valid alias] [the specified alias].
    /// </summary>
    /// <param name="alias">The alias.</param>
    /// <returns>
    ///   <c>true</c> if [is valid alias] [the specified alias]; otherwise, <c>false</c>.
    /// </returns>
    private async Task<bool> IsValidAlias(string alias)
    {
        string userName = await GetUserFullName(alias);
        if (userName.Equals(alias))
        {
            return false;
        }
        return true;
    }

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