// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace Microsoft.CFS.Approvals.Core.BL.Helpers
{
    using System;
    using System.Collections.Generic;
    using Microsoft.CFS.Approvals.Common.DL.Interface;
    using Microsoft.CFS.Approvals.Contracts;
    using Microsoft.CFS.Approvals.Core.BL.Interface;
    using Microsoft.CFS.Approvals.Extensions;
    using Microsoft.CFS.Approvals.LogManager.Model;
    using Microsoft.CFS.Approvals.LogManager.Provider.Interface;
    using Newtonsoft.Json.Linq;

    /// <summary>
    /// The Read Details Helper class
    /// </summary>
    public class ReadDetailsHelper : IReadDetailsHelper
    {
        /// <summary>
        /// The log provider
        /// </summary>
        private readonly ILogProvider _logProvider;

        /// <summary>
        /// The tenantInfo helper
        /// </summary>
        private readonly IApprovalTenantInfoHelper _tenantInfoHelper;

        /// <summary>
        /// The approval summary provider
        /// </summary>
        private readonly IApprovalSummaryProvider _approvalSummaryProvider;

        /// <summary>
        /// The performance logger
        /// </summary>
        private readonly IPerformanceLogger _performanceLogger;

        /// <summary>
        /// Constructor of ReadDetailsHelper
        /// </summary>
        /// <param name="_approvalSummaryProvider"></param>
        /// <param name="_tenantInfoHelper"></param>
        /// <param name="_logProvider"></param>
        /// <param name="_performanceLogger"></param>
        public ReadDetailsHelper(IApprovalSummaryProvider approvalSummaryProvider, IApprovalTenantInfoHelper tenantInfoHelper, ILogProvider logProvider, IPerformanceLogger performanceLogger)
        {
            _approvalSummaryProvider = approvalSummaryProvider;
            _tenantInfoHelper = tenantInfoHelper;
            _logProvider = logProvider;
            _performanceLogger = performanceLogger;
        }

        /// <summary>
        /// Update detail tabel for IsRead flag
        /// </summary>
        /// <param name="postData"></param>
        /// <param name="tenantId"></param>
        /// <param name="loggedInAlias"></param>
        /// <param name="alias"></param>
        /// <param name="clientDevice"></param>
        /// <param name="sessionId"></param>
        /// <param name="Tcv"></param>
        /// <param name="Xcv"></param>
        /// <returns></returns>
        public bool UpdateIsReadDetails(string postData, int tenantId, string loggedInAlias, string alias, string clientDevice, string sessionId, string Tcv, string Xcv)
        {
            // Create a unique GUID per user transaction
            var activityId = Guid.NewGuid().ToString();

            #region Logging Prep

            if (string.IsNullOrEmpty(sessionId))
            {
                sessionId = Guid.NewGuid().ToString();
            }
            if (string.IsNullOrEmpty(Tcv))
            {
                Tcv = Guid.NewGuid().ToString();
            }
            if (string.IsNullOrEmpty(Xcv))
            {
                Xcv = Tcv;
            }

            // TODO:: Add DXcv, DocumentNumber, FiscalYear to logData
            var logData = new Dictionary<LogDataKey, object>
            {
                {LogDataKey.Tcv, Tcv},
                {LogDataKey.EventType, Constants.FeatureUsageEvent},
                {LogDataKey.SessionId, sessionId},
                {LogDataKey.Xcv, Xcv},
                {LogDataKey.UserRoleName, loggedInAlias},
                {LogDataKey.TenantId, tenantId},
                {LogDataKey.Approver, alias},
                {LogDataKey._ActivityId, activityId},
                {LogDataKey.UserAlias, alias},
                {LogDataKey.IsCriticalEvent, CriticalityLevel.No.ToString()},
                {LogDataKey.StartDateTime, DateTime.UtcNow },
                {LogDataKey.ClientDevice, clientDevice}
            };

            #endregion Logging Prep

            try
            {
                using (_performanceLogger.StartPerformanceLogger("PerfLog", Constants.WebClient, string.Format(Constants.PerfLogAction, "ReadDetailsHelper", "UpdatesIsReadDetails"), logData))
                {
                    #region Get the user action string from request content and validate it

                    // Getting the user action string from request content
                    string userActionsString = postData;
                    // Unknown error
                    if (string.IsNullOrEmpty(userActionsString))
                    {
                        throw new ArgumentNullException("Error no details to be read");
                    }
                    JObject actionObject = (userActionsString).ToJObject();

                    // Set clientdevice = OUTLOOK if action is initiated from outlook
                    if (userActionsString.IsJson())
                    {
                        if (actionObject["ClientType"] != null
                            && !string.IsNullOrEmpty(actionObject["ClientType"].ToString()))
                        {
                            clientDevice = actionObject["ClientType"].ToString();
                        }
                        if (actionObject["ClientDevice"] == null)
                        {
                            actionObject["ClientDevice"] = clientDevice;
                        }
                    }

                    #endregion Get the user action string from request content and validate it

                    #region Get Tenant Info

                    var tenantInfo = _tenantInfoHelper.GetTenantInfo(tenantId);

                    #endregion Get Tenant Info

                    _approvalSummaryProvider.UpdateIsReadSummary(actionObject["DocumentKeys"].ToString(), alias, tenantInfo);
                    logData.Modify(LogDataKey.EndDateTime, DateTime.UtcNow);
                    _logProvider.LogInformation(TrackingEvent.WebApiReadDetailsSuccess, logData);
                }
            }
            catch (Exception exception)
            {
                logData.Modify(LogDataKey.EndDateTime, DateTime.UtcNow);
                _logProvider.LogError(TrackingEvent.WebApiReadDetailsFail, exception, logData);
                throw;
            }
            return true;
        }
    }
}