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
    using Microsoft.CFS.Approvals.Contracts.DataContracts;
    using Microsoft.CFS.Approvals.Core.BL.Interface;
    using Microsoft.CFS.Approvals.Extensions;
    using Microsoft.CFS.Approvals.LogManager.Model;
    using Microsoft.CFS.Approvals.LogManager.Provider.Interface;
    using Microsoft.CFS.Approvals.Model.Flighting;

    public class FlightingHelper : IFlightingHelper
    {
        private IFlightingDataProvider flightingDataProvider;
        private ILogProvider logProvider;
        private IPerformanceLogger performanceLogger;
        private IDelegationHelper _delegationHelper;

        /// <summary>
        /// FlightingHelper controller
        /// </summary>
        /// <param name="flightingDataProvider"></param>
        /// <param name="logProvider"></param>
        /// <param name="performanceLogger"></param>
        public FlightingHelper(IFlightingDataProvider flightingDataProvider, ILogProvider logProvider, IPerformanceLogger performanceLogger, IDelegationHelper delegationHelper)
        {
            this.flightingDataProvider = flightingDataProvider;
            this.logProvider = logProvider;
            this.performanceLogger = performanceLogger;
            _delegationHelper = delegationHelper;
        }

        /// <summary>
        /// Checks if the give feature is enabled/ flighted for given user
        /// </summary>
        /// <param name="upn">User UPN</param>
        /// <param name="featureID">Feature Id</param>
        /// <returns></returns>
        private bool IsFeatureEnabledForUser(string upn, int featureID)
        {
            return flightingDataProvider.IsFeatureEnabledForUser(upn, featureID);
        }

        /// <summary>
        /// Get all flighted features for given alias
        /// </summary>
        /// <param name="signedInUser">signed-in user</param>
        /// <param name="onBehalfUser">On-behalf user</param>
        /// <param name="oauth2UserToken">OAuth2 User Token</param>
        /// <param name="clientDevice">Client Device</param>
        /// <param name="sessionId">Session ID</param>
        /// <param name="tcv">TCV</param>
        /// <param name="xcv">XCV</param>
        /// <returns>list of flighting features</returns>
        public List<FlightingFeature> GetFlightingFeatures(User signedInUser, User onBehalfUser, string oauth2UserToken, string clientDevice, string sessionId, string tcv, string xcv)
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
                { LogDataKey.ClientDevice, clientDevice },
                { LogDataKey.UserRoleName, signedInUser.UserPrincipalName },
                { LogDataKey.UserAlias, signedInUser.UserPrincipalName },
                { LogDataKey.StartDateTime, DateTime.UtcNow },
                { LogDataKey.IsCriticalEvent, CriticalityLevel.No.ToString() }
            };

            #endregion Logging

            try
            {
                return flightingDataProvider.GetFlightingFeature(signedInUser.UserPrincipalName);
            }
            catch (Exception ex)
            {
                logData.Modify(LogDataKey.EndDateTime, DateTime.UtcNow);
                logProvider.LogError(TrackingEvent.GetFlightingFeaturesByAliasFailure, ex, logData);
                return null;
            }
        }

        /// <summary>
        /// Get all flighting features
        /// </summary>
        /// <param name="signedInUser">Signed-in user</param>
        /// <param name="onBehalfUser">On-behalf user</param>
        /// <param name="oauth2UserToken">OAuth2 User Token</param>
        /// <param name="clientDevice">Client Device</param>
        /// <param name="sessionId">Session ID</param>
        /// <param name="tcv">TCV</param>
        /// <param name="xcv">XCV</param>
        /// <returns>list of flighting feature</returns>
        public async Task<List<FlightingFeature>> GetAllFlightingFeatures(User signedInUser, User onBehalfUser, string oauth2UserToken, string clientDevice, string sessionId, string tcv, string xcv)
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
                { LogDataKey.ClientDevice, clientDevice },
                { LogDataKey.UserRoleName, signedInUser.MailNickname },
                { LogDataKey.UserAlias, signedInUser.MailNickname },
                { LogDataKey.StartDateTime, DateTime.UtcNow },
                { LogDataKey.IsCriticalEvent, CriticalityLevel.No.ToString() }
            };

            #endregion Logging

            try
            {
                await _delegationHelper.CheckUserAuthorization(signedInUser, onBehalfUser, oauth2UserToken, clientDevice, sessionId, xcv, tcv);
                return flightingDataProvider.GetAllFlightingFeature();
            }
            catch (Exception ex)
            {
                logData.Modify(LogDataKey.EndDateTime, DateTime.UtcNow);
                logProvider.LogError(TrackingEvent.GetAllFlightingFeaturesFailure, ex, logData);
                return null;
            }
        }

        /// <summary>
        /// Get list of features
        /// </summary>
        /// <param name="featureIDs">feature IDs</param>
        /// <param name="loggedInUpn">Logged-in UPN</param>
        /// <param name="sessionId">Session ID</param>
        /// <param name="clientDevice">Client Device</param>
        /// <returns>List of features with Status</returns>
        public List<dynamic> GetFeatures(string featureIDs, string loggedInUpn, string sessionId, string clientDevice)
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
                { LogDataKey.UserRoleName, loggedInUpn },
                { LogDataKey.ComponentName, clientDevice },
                { LogDataKey.EventType, Constants.FeatureUsageEvent },
                { LogDataKey.UserAlias, loggedInUpn },
                { LogDataKey.IsCriticalEvent, CriticalityLevel.No.ToString() }
            };

            #endregion Logging

            try
            {
                using (var documentStatustracer = performanceLogger.StartPerformanceLogger("PerfLog", "Flighting", string.Format(Constants.PerfLogCommon, "Flighting Get"), new Dictionary<LogDataKey, object>()))
                {
                    string[] IDs = featureIDs.Split(',');
                    List<int> iFeatureIDs = new List<int>();
                    foreach (string sid in IDs)
                    {
                        int id;
                        if (int.TryParse(sid, out id))
                            iFeatureIDs.Add(id);
                    }

                    bool result;
                    var results = new List<dynamic>();
                    foreach (int id in iFeatureIDs)
                    {
                        result = IsFeatureEnabledForUser(loggedInUpn, id);
                        results.Add(new { featureId = id, enabled = result });
                    }
                    if (results != null && results.Count > 0)
                        logProvider.LogInformation(TrackingEvent.WebApiFlightingReadSuccess, logData);
                    else
                        logProvider.LogError(TrackingEvent.WebApiFlightingReadFail, new Exception("Not able to get user feature status"), logData);
                    return results;
                }
            }
            catch (Exception ex)
            {
                logProvider.LogError(TrackingEvent.WebApiFlightingReadFail, ex, logData);
                return null;
            }
        }
    }
}