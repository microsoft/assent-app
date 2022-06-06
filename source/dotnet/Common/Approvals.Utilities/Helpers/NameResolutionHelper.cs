// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace Microsoft.CFS.Approvals.Utilities.Helpers
{
    using System;
    using System.Collections.Generic;
    using System.Net.Http;
    using System.Threading.Tasks;
    using Microsoft.CFS.Approvals.Common.DL.Interface;
    using Microsoft.CFS.Approvals.Contracts;
    using Microsoft.CFS.Approvals.LogManager.Model;
    using Microsoft.CFS.Approvals.LogManager.Provider.Interface;
    using Microsoft.CFS.Approvals.Utilities.Interface;
    using Microsoft.Extensions.Configuration;
    using Microsoft.Graph;
    using Newtonsoft.Json;
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
            string clientId = _config[ConfigurationKey.GraphAPIClientId.ToString()];
            string clientSecret = _config[ConfigurationKey.GraphAPIClientSecret.ToString()];
            string authority = _config[ConfigurationKey.GraphAPIAuthString.ToString()];

            var graphApiResponse = await _httpHelper.SendRequestAsync(
                HttpMethod.Get,
                clientId,
                clientSecret,
                authority,
                "https://graph.microsoft.com",
                string.Format("https://graph.microsoft.com/v1.0/users/{0}", alias + _config[ConfigurationKey.DomainName.ToString()]));

            if (graphApiResponse.IsSuccessStatusCode)
            {
                return JsonConvert.DeserializeObject<User>(await graphApiResponse.Content.ReadAsStringAsync());
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
                    throw new Exception("Empty alias passed in. Not able to resolve from Graph API.");
                }

                if (bool.Parse(_config[ConfigurationKey.ValidateAliasUsingPayloadValidator.ToString()]))
                {
                    using (var summaryRowInsertTracer = _performanceLogger.StartPerformanceLogger("PerfLog", "NameResolution", string.Format(Constants.PerfLogCommon, "Name Resolution By Alias"), logData))
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
            string clientId = _config[ConfigurationKey.GraphAPIClientId.ToString()];
            string clientSecret = _config[ConfigurationKey.GraphAPIClientSecret.ToString()];
            string authority = _config[ConfigurationKey.GraphAPIAuthString.ToString()];

            var graphApiResponse = await _httpHelper.SendRequestAsync(
                HttpMethod.Get,
                clientId,
                clientSecret,
                authority,
                "https://graph.microsoft.com",
                string.Format("https://graph.microsoft.com/v1.0/users/{0}/photo/$value", alias + _config[ConfigurationKey.DomainName.ToString()]));

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
        public async Task<bool> IsValidUser(string alias)
        {
            var user = await GetUser(alias);
            return user != null;
        }

        #endregion Implemented Methods
    }
}