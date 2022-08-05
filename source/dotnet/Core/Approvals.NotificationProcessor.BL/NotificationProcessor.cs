// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace Microsoft.CFS.Approvals.NotificationProcessor.BL
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Net.Http;
    using System.Threading.Tasks;
    using Microsoft.CFS.Approvals.Common.DL.Interface;
    using Microsoft.CFS.Approvals.Contracts;
    using Microsoft.CFS.Approvals.Contracts.DataContracts;
    using Microsoft.CFS.Approvals.Core.BL.Interface;
    using Microsoft.CFS.Approvals.Domain.BL.Interface;
    using Microsoft.CFS.Approvals.LogManager.Model;
    using Microsoft.CFS.Approvals.LogManager.Provider.Interface;
    using Microsoft.CFS.Approvals.Model;
    using Microsoft.CFS.Approvals.NotificationProcessor.BL.Interface;
    using Microsoft.CFS.Approvals.Utilities.Extension;
    using Microsoft.CFS.Approvals.Utilities.Interface;
    using Microsoft.Extensions.Configuration;
    using Newtonsoft.Json;

    /// <summary>
    /// The Notification Processor class
    /// </summary>
    public class NotificationProcessor : INotificationProcessor
    {
        /// <summary>
        /// The Log Provider
        /// </summary>
        private readonly ILogProvider _logProvider;

        /// <summary>
        /// The Approval Summary Provider
        /// </summary>
        private readonly IApprovalSummaryHelper _approvalSummaryHelper;

        /// <summary>
        /// The Email Helper
        /// </summary>
        private readonly IEmailHelper _emailHelper;

        /// <summary>
        /// The Flighting Data Provider
        /// </summary>
        private readonly IFlightingDataProvider _flightingDataProvider;

        /// <summary>
        /// The configuration
        /// </summary>
        private readonly IConfiguration _config;

        /// <summary>
        /// The Name Resolution Helper
        /// </summary>
        private readonly INameResolutionHelper _nameResolutionHelper;

        /// <summary>
        /// Gets or Sets the Authentication Helper.
        /// </summary>
        private IAuthenticationHelper _authenticationHelper { get; set; }

        /// <summary>
        /// Gets or Sets the Http Helper.
        /// </summary>
        protected IHttpHelper _httpHelper { get; set; }

        /// <summary>
        /// Constructor of NotificationProcessor
        /// </summary>
        /// <param name="logProvider"></param>
        /// <param name="emailHelper"></param>
        /// <param name="notificationProvider"></param>
        public NotificationProcessor(
            IConfiguration config,
            ILogProvider logProvider,
            IApprovalSummaryHelper approvalSummaryHelper,
            IEmailHelper emailHelper,
            IFlightingDataProvider flightingDataProvider,
            INameResolutionHelper nameResolutionHelper,
            IAuthenticationHelper authenticationHelper,
            IHttpHelper httpHelper
            )
        {
            _config = config;
            _logProvider = logProvider;
            _approvalSummaryHelper = approvalSummaryHelper;
            _emailHelper = emailHelper;
            _flightingDataProvider = flightingDataProvider;
            _nameResolutionHelper = nameResolutionHelper;
            _authenticationHelper = authenticationHelper;
            _httpHelper = httpHelper;
        }

        #region Notification Methods

        /// <summary>
        /// Sends the notifications.
        /// </summary>
        /// <param name="requestExpressions">The request expressions.</param>
        /// <param name="tenant">The tenant.</param>
        /// <returns>returns a boolean value</returns>
        /// <exception cref="Exception">DeviceNotificationInfo is null</exception>
        public async Task<bool> SendNotifications(ApprovalNotificationDetails requestExpressions, ITenant tenant)
        {
            var logData = new Dictionary<LogDataKey, object>();
            ApprovalRequestResult result = null;
            try
            {
                logData.Add(LogDataKey.Xcv, requestExpressions.Xcv);
                logData.Add(LogDataKey.Tcv, requestExpressions.Tcv);
                logData.Add(LogDataKey.ReceivedTcv, requestExpressions.Tcv);
                logData.Add(LogDataKey.TenantTelemetryData, requestExpressions.TenantTelemetry);
                logData.Add(LogDataKey.DXcv, requestExpressions.ApprovalIdentifier.DisplayDocumentNumber);
                logData.Add(LogDataKey.DisplayDocumentNumber, requestExpressions?.ApprovalIdentifier?.DisplayDocumentNumber);
                logData.Add(LogDataKey.DocumentNumber, requestExpressions?.ApprovalIdentifier?.DocumentNumber);
                logData.Add(LogDataKey.TenantId, requestExpressions?.ApprovalTenantInfo?.DocTypeId);
                logData.Add(LogDataKey.TenantName, requestExpressions?.ApprovalTenantInfo?.AppName);
                logData.Add(LogDataKey.DetailStatus, requestExpressions?.DetailsLoadSuccess);
                logData.Add(LogDataKey.BusinessProcessName, string.Format(requestExpressions.ApprovalTenantInfo.BusinessProcessName, Constants.BusinessProcessNameSendNotificationToUser, Constants.BusinessProcessNameSendNotificationAll));
                if (requestExpressions.DeviceNotificationInfo != null)
                {
                    bool isActionableEmailSent = false;
                    //Checking if tenant is subscribe for normal notify email
                    //Checking if tenant is subscribe outlook actionable email feature
                    //Checking if tenant is subscribe for Pending Approval action only
                    if (requestExpressions.ApprovalTenantInfo.NotifyEmail && requestExpressions.ApprovalTenantInfo.NotifyEmailWithApprovalFunctionality && IsActionEnableForNotifyEmailWithApprovalFunctionality(requestExpressions.DeviceNotificationInfo, requestExpressions.ApprovalTenantInfo))
                    {
                        isActionableEmailSent = tenant.ValidateIfEmailShouldBeSentWithDetails(requestExpressions.SummaryRows);
                        if (isActionableEmailSent)
                        {
                            requestExpressions.DeviceNotificationInfo.NotificationTemplateKey += Constants.EmailNotificationWithActionTemplateKey;

                            //Add the NotificationTemplateKey in logData
                            logData.Add(LogDataKey.NotificationTemplateKey, requestExpressions?.DeviceNotificationInfo?.NotificationTemplateKey);

                            // The worker processes the notification, captures the screen shot, saves into blob and send a message to notification topic
                            result = ProcessNotificationInBackground(requestExpressions, tenant);
                        }
                    }

                    if (tenant.ShouldSendRegularEmail(isActionableEmailSent))
                    {
                        // * Send an Email Notification
                        result = _emailHelper.SendEmail(requestExpressions, tenant, EmailType.NormalEmail);
                    }

                    if ((null != result) && (null != result.Exception))
                    {
                        requestExpressions.DeviceNotificationInfo.NotificationFailResult = result.Exception.Message;
                        //PlaceFailedNotificationOnQueue(requestExpressions);
                    }
                    _logProvider.LogInformation(TrackingEvent.ProcessNotificationComplete, logData);
                    return true;
                }
                else
                {
                    throw new Exception("DeviceNotificationInfo is null");
                }
            }
            catch (Exception exception)
            {
                _logProvider.LogError(TrackingEvent.ProcessNotificationFail, exception, logData);
                return false;
            }
        }

        /// <summary>
        /// Processes the notification in background.
        /// </summary>
        /// <param name="approvalNotificationDetails">The approval notification details.</param>
        /// <param name="tenant">The tenant.</param>
        /// <returns>returns Approval Request Result</returns>
        public ApprovalRequestResult ProcessNotificationInBackground(ApprovalNotificationDetails approvalNotificationDetails, ITenant tenant)
        {
            return _emailHelper.SendEmail(approvalNotificationDetails,
                tenant,
                EmailType.ActionableEmail
                );
        }

        /// <summary>
        /// Sends teams notifications.
        /// </summary>
        /// <param name="requestExpressions">The request expressions.</param>
        /// <param name="tenant">The tenant.</param>
        /// <returns>returns a boolean value</returns>
        /// <exception cref="System.Exception"></exception>
        public async Task<bool> SendTeamsNotifications(ApprovalNotificationDetails requestExpressions)
        {
            bool enableMSTeamsNotification = false;
            List<string> approvers = new List<string>();
            switch (requestExpressions.ApprovalTenantInfo.NotifyTeams)
            {
                case (int)EnableMSTeamsNotification.DisableForAll:
                    enableMSTeamsNotification = false;
                    break;
                case (int)EnableMSTeamsNotification.EnableForFlightedUsers:
                    foreach (var approver in requestExpressions.SummaryRows.Select(s => s.Approver).ToList())
                    {
                        if (_flightingDataProvider.IsFeatureEnabledForUser(approver, (int)FlightingFeatureName.MSTeamNotification))
                        {
                            approvers.Add(approver);
                            enableMSTeamsNotification = true;
                        }
                    }
                    break;
                case (int)EnableMSTeamsNotification.EnableForAll:
                    enableMSTeamsNotification = true;
                    break;
                default:
                    enableMSTeamsNotification = false;
                    break;
            }

            if (enableMSTeamsNotification && requestExpressions.DeviceNotificationInfo.Operation != ApprovalRequestOperation.Delete)
            {
                string postUrl = _config[ConfigurationKey.TeamsEndpointUrl.ToString()];
                string AdaptiveTemplateUrl = _config[ConfigurationKey.ApprovalsCoreServicesURL.ToString()] + "/api/v1/AdaptiveDetail/{0}";
                string DataUrl = _config[ConfigurationKey.ApprovalsCoreServicesURL.ToString()] + "/api/v1/Detail/{0}/{1}?Source=Notification";
                var teamsNotificationCorrelationId = Guid.NewGuid().ToString();

                var logData = new Dictionary<LogDataKey, object>
                {
                    { LogDataKey.TenantTelemetryData, requestExpressions.TenantTelemetry },
                    { LogDataKey.Tcv, requestExpressions.Tcv },
                    { LogDataKey.Xcv, requestExpressions.Xcv },
                    { LogDataKey.ComponentName, Constants.WorkerRole },
                    { LogDataKey.TenantId, requestExpressions.ApprovalTenantInfo.TenantId },
                    { LogDataKey.DocumentNumber, requestExpressions.ApprovalIdentifier.DisplayDocumentNumber },
                    { LogDataKey.DXcv, requestExpressions.ApprovalIdentifier.DisplayDocumentNumber },
                    { LogDataKey.BusinessProcessName, string.Format(requestExpressions.ApprovalTenantInfo.BusinessProcessName, Constants.BusinessProcessNameApprovalAction, "TeamsAPI") },
                    { LogDataKey.TeamsNotificationCorrelationId, teamsNotificationCorrelationId }
                };

                try
                {
                    if (String.IsNullOrEmpty(postUrl))
                    {
                        throw new UriFormatException(_config[ConfigurationKey.Message_URLNotDefined.ToString()]);
                    }

                    var summaryJson = JsonConvert.DeserializeObject<SummaryJson>(requestExpressions?.SummaryRows.FirstOrDefault()?.SummaryJson);

                    if (approvers == null || approvers.Count <= 0)
                        approvers = requestExpressions.SummaryRows.Select(s => s.Approver).ToList();

                    MSApprovalsTeamsNotificationControllerInput teamsNotificationInput = new MSApprovalsTeamsNotificationControllerInput()
                    {
                        NotificationReceiverAadId = new string[approvers.Count],
                        NotificationSender = string.Empty,
                        Provider = ApprovalProviderType.Extensibility,
                        EventType = ApprovalEventType.Creation,
                        UserRole = ApprovalUserRole.Approver,
                        DetailsUri = new Uri(string.Format(DataUrl, requestExpressions.ApprovalTenantInfo.TenantId, requestExpressions.ApprovalIdentifier.GetDocNumber(requestExpressions.ApprovalTenantInfo))),
                        TemplateUri = new Uri(string.Format(AdaptiveTemplateUrl, requestExpressions.ApprovalTenantInfo.TenantId))
                    };

                    string senderAadId = string.Empty;
                    if (summaryJson != null)
                    {
                        try
                        {
                            senderAadId = _nameResolutionHelper?.GetUser(summaryJson.Submitter.Alias)?.Result?.Id;
                        }
                        catch
                        {
                            senderAadId = string.IsNullOrWhiteSpace(summaryJson.Submitter?.Alias) ? summaryJson.Submitter?.Name : summaryJson.Submitter?.Alias;
                        }
                    }

                    teamsNotificationInput.NotificationSender = senderAadId;

                    for (int i = 0; i < approvers.Count; i++)
                    {
                        teamsNotificationInput.NotificationReceiverAadId[i] = _nameResolutionHelper?.GetUser(approvers[i])?.Result?.Id;
                    }

                    teamsNotificationInput.Title = "MSApprovals - " + requestExpressions?.ApprovalIdentifier?.DisplayDocumentNumber + ": " + (string.IsNullOrWhiteSpace(summaryJson?.Title) ? string.Empty : summaryJson.Title);

                    logData.Add(LogDataKey.TeamsNotificationJson, JsonConvert.SerializeObject(teamsNotificationInput));

                    var lobResponse = await _httpHelper.SendRequestAsync(HttpMethod.Post,
                        _config[ConfigurationKey.TeamsClientId.ToString()],
                        _config[ConfigurationKey.TeamsAppKey.ToString()],
                        String.Format(_config[ConfigurationKey.AADInstance.ToString()], _config[ConfigurationKey.AADTenantId.ToString()]),
                        _config[ConfigurationKey.TeamsResourceUrl.ToString()],
                        postUrl,
                        new Dictionary<string, string>() { { Constants.TeamsNotificationCorrelationHeader, teamsNotificationCorrelationId } },
                        JsonConvert.SerializeObject(teamsNotificationInput)
                        );

                    if (lobResponse.IsSuccessStatusCode)
                    {
                        _logProvider.LogInformation(TrackingEvent.TeamsNotificationAPISuccess, logData);
                        return true;
                    }
                    else
                    {
                        Exception ex = new Exception(await lobResponse.Content.ReadAsStringAsync());
                        _logProvider.LogError(TrackingEvent.TeamsNotificationAPIFail, ex, logData);
                        return false;
                    }
                }
                catch (Exception ex)
                {
                    _logProvider.LogError(TrackingEvent.TeamsNotificationAPIFail, ex, logData);
                    return false;
                }
            }
            return true;
        }
        #endregion Notification Methods

        #region Helper Methods

        /// <summary>
        /// Enable action for notify to user with inline action functionality in email for approve/reject
        /// </summary>
        /// <param name="deviceNotificationInfo"></param>
        /// <param name="approvalTenantInfo"></param>
        /// <returns></returns>
        public bool IsActionEnableForNotifyEmailWithApprovalFunctionality(DeviceNotificationInfo deviceNotificationInfo, ApprovalTenantInfo approvalTenantInfo)
        {
            var notificationTemplateKey = approvalTenantInfo.ActionableNotificationTemplateKeysList.Where(s => s == deviceNotificationInfo.NotificationTemplateKey).FirstOrDefault();
            return (notificationTemplateKey != null);
        }

        #endregion Helper Methods
    }
}