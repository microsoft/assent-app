// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace Microsoft.CFS.Approvals.Utilities.Helpers;

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Sockets;
using System.Threading.Tasks;
using Microsoft.CFS.Approvals.Common.DL.Interface;
using Microsoft.CFS.Approvals.Contracts;
using Microsoft.CFS.Approvals.LogManager.Model;
using Microsoft.CFS.Approvals.LogManager.Provider.Interface;
using Microsoft.CFS.Approvals.Utilities.Interface;
using Microsoft.Extensions.Configuration;
using Microsoft.Graph.Models;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Constants = Contracts.Constants;

/// <summary>
/// The Name Resolution Helper class
/// </summary>
public class NameResolutionHelper : INameResolutionHelper
{
    /// <summary>
    /// The Http Helper
    /// </summary>
    private readonly IHttpHelper _httpHelper;

    /// <summary>
    /// The configuration
    /// </summary>
    private readonly IConfiguration _config;

    /// <summary>
    /// The log provider
    /// </summary>
    private readonly ILogProvider _logProvider;

    /// <summary>
    /// The performance logger
    /// </summary>
    private readonly IPerformanceLogger _performanceLogger;

    /// <summary>
    /// Constructor of NameResolutionHelper class
    /// </summary>
    /// <param name="config"></param>
    /// <param name="logProvider"></param>
    /// <param name="performanceLogger"></param>
    public NameResolutionHelper(
        IHttpHelper httpHelper,
        IConfiguration config,
        ILogProvider logProvider,
        IPerformanceLogger performanceLogger)
    {
        _httpHelper = httpHelper;
        _config = config;
        _logProvider = logProvider;
        _performanceLogger = performanceLogger;
    }

    #region Implemented Methods

    /// <summary>
    /// Get user by alias
    /// </summary>
    /// <param name="alias"></param>
    /// <returns></returns>
    public async Task<User> GetUser(string alias)
    {
        bool isIdOrUpn;
        if (string.IsNullOrWhiteSpace(alias))
            return null;
        else
        {
            var graphApiResponse = await GetADUser(GetGraphApiEndPoint(alias, string.Empty, out isIdOrUpn));
            if (graphApiResponse.IsSuccessStatusCode)
            {
                return JsonConvert.DeserializeObject<User>(await graphApiResponse.Content.ReadAsStringAsync());
            }
            else if (graphApiResponse.StatusCode is HttpStatusCode.NotFound && !isIdOrUpn)
            {
                var whitelistedDomains = _config[Constants.OldWhitelistedDomains]?.Split(";").ToList();

                foreach (var domain in whitelistedDomains)
                {
                    if (domain.Length > 0)
                    {
                        graphApiResponse = await GetADUser(GetGraphApiEndPoint(alias, domain, out isIdOrUpn));

                        var user = JsonConvert.DeserializeObject<JObject>(await graphApiResponse.Content.ReadAsStringAsync())?.SelectToken("value")?.Value<JArray>();
                        if (graphApiResponse.IsSuccessStatusCode && user?.Count > 0)
                        {
                            return JsonConvert.DeserializeObject<User>(user?.FirstOrDefault()?.ToString());
                        }
                    }
                }
            }
            return null;
        }
    }

    /// <summary>
    /// Get user by mail
    /// </summary>
    /// <param name="mail"></param>
    /// <returns></returns>
    public async Task<User> GetUserByMail(string mail)
    {
        string targetUri = string.Format(_config[ConfigurationKey.GraphAPIUri.ToString()] + "?$filter=startswith(mail,'{0}')", mail);
        var graphApiResponse = await GetADUser(targetUri);
        if (graphApiResponse.IsSuccessStatusCode)
        {
            var user = JsonConvert.DeserializeObject<JObject>(await graphApiResponse.Content.ReadAsStringAsync())?.SelectToken("value")?.Value<JArray>();
            if (user?.Count > 0)
            {
                return JsonConvert.DeserializeObject<User>(user.FirstOrDefault()?.ToString());
            }
        }
        return null;
    }

    /// <summary>
    /// Resolves the User alias into User FullName
    /// </summary>
    /// <param name="alias"></param>
    /// <returns></returns>
    public async Task<string> GetUserName(string alias)
    {
        #region Logging

        var logData = new Dictionary<LogDataKey, object>
        {
            { LogDataKey.EventType, Constants.FeatureUsageEvent },
            { LogDataKey.UserAlias, alias }
        };

        #endregion Logging

        try
        {
            //make sure alias is not null or empty string before call Graph API to resolve. Otherwise, GraphAPI will take long time to return and timeout.
            if (string.IsNullOrEmpty(alias))
            {
                throw new InvalidDataException("Empty alias passed in. Not able to resolve from Graph API.");
            }

            if (bool.Parse(_config[ConfigurationKey.ValidateAliasUsingPayloadValidator.ToString()]))
            {
                using (var getUserNameTracer = _performanceLogger.StartPerformanceLogger("PerfLog", "NameResolution", string.Format(Constants.PerfLogCommon, "Name Resolution By Alias"), logData))
                {
                    var employee = await GetUser(alias);
                    if (employee != null)
                    {
                        return (employee.GivenName + " " + employee.Surname).Trim();
                    }
                    return alias;
                }
            }
            else
            {
                return alias;
            }
        }
        catch (Exception ex)
        {
            _logProvider.LogError(TrackingEvent.NameResolutionError, ex, logData);
            if (alias == null)
                return "";
            else
                return alias;
        }
    }

    /// <summary>
    /// Get user image by alias
    /// </summary>
    /// <param name="alias"></param>
    /// <returns></returns>
    public async Task<byte[]> GetUserImage(string alias)
    {
        bool graphAPIMITokenEnabled = Convert.ToBoolean(_config[ConfigurationKey.GraphAPIMITokenEnabled.ToString()]);
        string clientId = graphAPIMITokenEnabled == false ? _config[ConfigurationKey.GraphAPIClientId.ToString()] : _config[ConfigurationKey.ManagedIdentityClientId.ToString()];
        string clientSecret = _config[ConfigurationKey.GraphAPIClientSecret.ToString()];
        string authority = _config[ConfigurationKey.GraphAPIAuthString.ToString()];

        var graphApiResponse = await _httpHelper.SendRequestAsync(
            HttpMethod.Get,
            clientId,
            clientSecret,
            authority,
            "https://graph.microsoft.com",
            string.Format("https://graph.microsoft.com/v1.0/users/{0}/photo/$value", alias + _config[ConfigurationKey.DomainName.ToString()]),
            null,
            "",
            graphAPIMITokenEnabled);

        if (graphApiResponse.IsSuccessStatusCode)
        {
            return await graphApiResponse.Content.ReadAsByteArrayAsync();
        }
        return null;
    }

    /// <summary>
    /// Check if user is valid.
    /// </summary>
    /// <param name="alias"></param>
    /// <returns></returns>
    public async Task<Tuple<bool, string>> IsValidUser(string alias)
    {
        #region Logging

        var logData = new Dictionary<LogDataKey, object>
        {
            { LogDataKey.EventType, Constants.FeatureUsageEvent },
            { LogDataKey.UserAlias, alias }
        };

        #endregion Logging

        try
        {
            //make sure alias is not null or empty string before call Graph API to resolve. Otherwise, GraphAPI will take long time to return and timeout.
            if (string.IsNullOrEmpty(alias))
            {
                throw new InvalidDataException("Empty alias passed in. Not able to resolve from Graph API.");
            }
            using (var isValidUserTracer = _performanceLogger.StartPerformanceLogger("PerfLog", "ValidateUser", string.Format(Constants.PerfLogCommon, "Get User By Alias"), logData))
            {
                var user = await GetUser(alias);
                return new Tuple<bool, string>(user != null, string.Empty);
            }
        }
        catch (SocketException ex)
        {
            _logProvider.LogError(TrackingEvent.NameResolutionError, ex, logData);
            return new Tuple<bool, string>(false, Constants.SocketExceptionMessage);
        }
        catch (Exception ex)
        {
            _logProvider.LogError(TrackingEvent.NameResolutionError, ex, logData);
            return new Tuple<bool, string>(false, ex.Message);
        }
        
    }

    /// <summary>
    /// Gets Id of the given alias
    /// </summary>
    /// <param name="alias"></param>
    /// <returns></returns>
    public async Task<string> GetObjectId(string alias)
    {
        #region Logging

        var logData = new Dictionary<LogDataKey, object>
        {
            { LogDataKey.EventType, Constants.FeatureUsageEvent },
            { LogDataKey.UserAlias, alias }
        };

        #endregion Logging

        try
        {
            //make sure alias is not null or empty string before call Graph API to resolve. Otherwise, GraphAPI will take long time to return and timeout.
            if (string.IsNullOrEmpty(alias))
            {
                throw new InvalidDataException("Empty alias passed in. Not able to resolve ObjectId from Graph API.");
            }
            using (var getObjectIdTracer = _performanceLogger.StartPerformanceLogger("PerfLog", "NameResolution", string.Format(Constants.PerfLogCommon, "Fetch ObjectId By Alias"), logData))
            {
                var user = await GetUser(alias);
                if (user != null)
                {
                    return user.Id;
                }
                return string.Empty;
            }
        }
        catch(Exception ex) 
        {
            _logProvider.LogError(TrackingEvent.NameResolutionError, ex, logData);
            return string.Empty;
        }
    }

    /// <summary>
    /// Gets UserPrincipalName of the given alias
    /// </summary>
    /// <param name="alias"></param>
    /// <returns></returns>
    public async Task<string> GetUserPrincipalName(string alias)
    {
        var logData = new Dictionary<LogDataKey, object>
        {
            { LogDataKey.EventType, Constants.FeatureUsageEvent },
            { LogDataKey.UserAlias, alias }
        };

        try
        {
            //make sure alias is not null or empty string before call Graph API to resolve. Otherwise, GraphAPI will take long time to return and timeout.
            if (string.IsNullOrEmpty(alias))
            {
                throw new InvalidDataException("Empty alias passed in. Not able to resolve UserPrincipalName from Graph API.");
            }
            using (var getUpnTracer = _performanceLogger.StartPerformanceLogger("PerfLog", "NameResolution", string.Format(Constants.PerfLogCommon, "Fetch UPN By Alias"), logData))
            {
                var user = await GetUser(alias);
                if (user != null)
                {
                    return (!string.IsNullOrWhiteSpace(user.UserPrincipalName) ? user.UserPrincipalName : string.Empty);
                }
                return string.Empty;
            }
        }
        catch (Exception ex)
        {
            _logProvider.LogError(TrackingEvent.NameResolutionError, ex, logData);
            return string.Empty;
        }
    }

    #endregion Implemented Methods
    /// <summary>
    /// Get user from Active Directory
    /// </summary>
    /// <param name="targetUri"></param>
    /// <returns></returns>
    private async Task<HttpResponseMessage> GetADUser(string targetUri)
    {
        bool graphAPIMITokenEnabled = Convert.ToBoolean(_config[ConfigurationKey.GraphAPIMITokenEnabled.ToString()]);
        string clientId = graphAPIMITokenEnabled == false ? _config[ConfigurationKey.GraphAPIClientId.ToString()] : _config[ConfigurationKey.ManagedIdentityClientId.ToString()];
        string clientSecret = _config[ConfigurationKey.GraphAPIClientSecret.ToString()];
        string authority = _config[ConfigurationKey.GraphAPIAuthString.ToString()];
        string resourceUri = _config[ConfigurationKey.GraphAPIResourceUri.ToString()];

        var graphApiResponse = await _httpHelper.SendRequestAsync(
            HttpMethod.Get,
            clientId,
            clientSecret,
            authority,
            resourceUri,
            targetUri,
            null,
            "",
            graphAPIMITokenEnabled);

        return graphApiResponse;
    }

    /// <summary>
    /// Gets the Graph API endpoint to be called to fetch AD Graph user
    /// </summary>
    /// <param name="aliasOrIdOrUpn">alias or Id or UserPrincipalName</param>
    /// <param name="domain">Domain</param>
    /// <param name="isIdOrUpn">whether the 1st parameter is Id or Upn</param>
    /// <returns>graph API endpoint</returns>
    private string GetGraphApiEndPoint(string aliasOrIdOrUpn, string domain, out bool isIdOrUpn)
    {
        string graphApiEndPoint = string.Empty;
        isIdOrUpn = false;
        Guid guid;
        if (!string.IsNullOrWhiteSpace(aliasOrIdOrUpn))
        {
            if (!aliasOrIdOrUpn.Contains("@") && !Guid.TryParse(aliasOrIdOrUpn, out guid))
            {
                graphApiEndPoint = string.IsNullOrWhiteSpace(domain) ?
                    string.Format(_config[ConfigurationKey.GraphAPIUri.ToString()] + "{0}", aliasOrIdOrUpn + _config[ConfigurationKey.DomainName.ToString()]) :
                    string.Format(_config[ConfigurationKey.GraphAPIUri.ToString()] + _config[ConfigurationKey.GraphAPIUriFilter.ToString()], aliasOrIdOrUpn + domain);
            }
            else
            {
                graphApiEndPoint = string.Format(_config[ConfigurationKey.GraphAPIUri.ToString()] + "{0}", aliasOrIdOrUpn);
                isIdOrUpn = true;
            }
            return graphApiEndPoint;
        }
        return graphApiEndPoint;
    }

    /// <summary>
    /// Get the manager of the Graph user by user object Id
    /// </summary>
    /// <param name="userObjectId"></param>
    /// <returns></returns>
    public async Task<Microsoft.Graph.Models.User> GetUserManagerId(string userObjectId)
    {
        if (!string.IsNullOrWhiteSpace(userObjectId))
        {
            bool graphAPIMITokenEnabled = Convert.ToBoolean(_config[ConfigurationKey.GraphAPIMITokenEnabled.ToString()]);
            string clientId = graphAPIMITokenEnabled == false ? _config[ConfigurationKey.GraphAPIClientId.ToString()] : _config[ConfigurationKey.ManagedIdentityClientId.ToString()];
            string authority = _config[ConfigurationKey.GraphAPIAuthString.ToString()];
            string resourceUri = _config[ConfigurationKey.GraphAPIResourceUri.ToString()];
            string targetUri = string.Format(_config[ConfigurationKey.GraphAPIUri.ToString()] + "{0}/?$expand=manager($levels=1;$select=id,displayName,accountEnabled)&$select=id,displayName,userPrincipalName,accountEnabled", userObjectId);

            var graphApiResponse = await _httpHelper.SendRequestAsync(
               HttpMethod.Get,
               clientId,
               null,
               authority,
               resourceUri,
               targetUri,
               null,
               "",
               graphAPIMITokenEnabled);

            if (graphApiResponse.IsSuccessStatusCode)
            {
                return JsonConvert.DeserializeObject<User>(await graphApiResponse.Content.ReadAsStringAsync());
            }
        }
        return null;
    }

    
}