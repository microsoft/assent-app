// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace Microsoft.CFS.Approvals.CoreServices.BL.Helpers
{
    using System;
    using System.Collections.Generic;
    using Microsoft.CFS.Approvals.Common.DL.Interface;
    using Microsoft.CFS.Approvals.Contracts;
    using Microsoft.CFS.Approvals.CoreServices.BL.Interface;
    using Microsoft.CFS.Approvals.Extensions;
    using Microsoft.CFS.Approvals.LogManager.Model;
    using Microsoft.CFS.Approvals.LogManager.Provider.Interface;
    using Microsoft.Extensions.Configuration;
    using Newtonsoft.Json.Linq;

    /// <summary>
    /// The About Helper class
    /// </summary>
    public class AboutHelper : IAboutHelper
    {
        private readonly ILogProvider _logProvider = null;
        private readonly IPerformanceLogger _performanceLogger = null;
        private readonly IConfiguration _config;

        public AboutHelper(ILogProvider logProvider, IPerformanceLogger performanceLogger, IConfiguration config)
        {
            _logProvider = logProvider;
            _config = config;
            _performanceLogger = performanceLogger;
        }

        /// <summary>
        /// Get about
        /// </summary>
        /// <param name="host"></param>
        /// <param name="sessionId"></param>
        /// <param name="loggedInAlias"></param>
        /// <param name="clientDevice"></param>
        /// <param name="alias"></param>
        /// <returns></returns>
        public dynamic GetAbout(string host, string sessionId, string loggedInAlias, string clientDevice, string alias)
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
                logData.Add(LogDataKey.UserRoleName, loggedInAlias);
                logData.Add(LogDataKey.ClientDevice, clientDevice);
                logData.Add(LogDataKey.EventType, Constants.FeatureUsageEvent);
                logData.Add(LogDataKey.UserAlias, alias);
                logData.Add(LogDataKey.IsCriticalEvent, CriticalityLevel.No.ToString());

                #endregion Logging

                using (_performanceLogger.StartPerformanceLogger("PerfLog", string.IsNullOrWhiteSpace(clientDevice) ? Constants.WebClient : clientDevice, string.Format(Constants.PerfLogCommon, "About"), logData))
                {
                    // Check if Host is null or empty and throw back an exception which will get returned as bad request to caller
                    if (string.IsNullOrEmpty(host))
                    {
                        throw new Exception(_config[ConfigurationKey.Message_NoTenantForDevice.ToString()]);
                    }

                    // Get support email id from configuration
                    var supportEmailId = _config[ConfigurationKey.SupportEmailId.ToString()];

                    // Log Success
                    _logProvider.LogInformation(TrackingEvent.WebApiAboutSuccess, logData);

                    // Serialize and send the URL
                    return (new { supportEmailId });
                }
            }
            catch (Exception ex)
            {
                _logProvider.LogError(TrackingEvent.WebApiAboutFail, ex, logData);
                throw;
            }
        }

        /// <summary>
        /// Get help data
        /// </summary>
        /// <param name="host"></param>
        /// <param name="sessionId"></param>
        /// <param name="loggedInAlias"></param>
        /// <param name="clientDevice"></param>
        /// <param name="alias"></param>
        /// <returns></returns>
        public JObject GetHelpData(string host, string sessionId, string loggedInAlias, string clientDevice, string alias)
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
                logData.Add(LogDataKey.UserRoleName, loggedInAlias);
                logData.Add(LogDataKey.ClientDevice, clientDevice);
                logData.Add(LogDataKey.ActionOrComponentUri, "Help/Get");
                logData.Add(LogDataKey.EventType, Constants.FeatureUsageEvent);
                logData.Add(LogDataKey.UserAlias, alias);
                logData.Add(LogDataKey.StartDateTime, DateTime.UtcNow);
                logData.Add(LogDataKey.IsCriticalEvent, CriticalityLevel.No.ToString());

                #endregion Logging

                using (_performanceLogger.StartPerformanceLogger("PerfLog", string.IsNullOrWhiteSpace(clientDevice) ? Constants.WebClient : clientDevice, string.Format(Constants.PerfLogCommon, "Help"), logData))
                {
                    // Check if Host is null or empty and throw back an exception which will get returned as bad request to caller
                    if (string.IsNullOrEmpty(host))
                    {
                        throw new Exception(_config[ConfigurationKey.Message_NoTenantForDevice.ToString()]);
                    }

                    // Get support email id from configuration
                    var supportEmailId = _config[ConfigurationKey.SupportEmailId.ToString()];

                    // Log Success
                    logData.Modify(LogDataKey.EndDateTime, DateTime.UtcNow);
                    _logProvider.LogInformation(TrackingEvent.WebApiHelpSuccess, logData);

                    // Serialize and send the URL
                    return (JObject.FromObject(new { supportEmailId }));
                }
            }
            catch (Exception ex)
            {
                logData.Modify(LogDataKey.EndDateTime, DateTime.UtcNow);
                _logProvider.LogError(TrackingEvent.WebApiHelpFail, ex, logData);
                throw;
            }
        }
    }
}