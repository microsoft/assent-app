// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace Microsoft.CFS.Approvals.WatchdogProcessor.BL.Helpers;

using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.CFS.Approvals.Common.DL.Interface;
using Microsoft.CFS.Approvals.Contracts;
using Microsoft.CFS.Approvals.Contracts.DataContracts;
using Microsoft.CFS.Approvals.Core.BL.Interface;
using Microsoft.CFS.Approvals.Domain.BL.Interface;
using Microsoft.CFS.Approvals.Extensions;
using Microsoft.CFS.Approvals.LogManager.Model;
using Microsoft.CFS.Approvals.LogManager.Provider.Interface;
using Microsoft.CFS.Approvals.Model;
using Microsoft.CFS.Approvals.Utilities.Helpers;
using Microsoft.CFS.Approvals.Utilities.Interface;
using Microsoft.CFS.Approvals.WatchdogProcessor.BL.Interface;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json.Linq;
using NotificationType = Model.NotificationType;

/// <summary>
/// The Reminder Processor class
/// </summary>
public class ReminderProcessor : IReminderProcessor
{
    /// <summary>
    /// The data interface
    /// </summary>
    private readonly IReminderData _dataInterface;

    /// <summary>
    /// The notification provider
    /// </summary>
    private readonly INotificationProvider _notificationProvider;

    /// <summary>
    /// The _logger
    /// </summary>
    private readonly ILogProvider _logger;//Already Existed

    /// <summary>
    /// The email template helper
    /// </summary>
    private readonly IEmailTemplateHelper _emailTemplateHelper;//Already Existed

    /// <summary>
    /// The name resolution helper
    /// </summary>
    private readonly INameResolutionHelper _nameResolutionHelper;

    /// <summary>
    /// Performance logger
    /// </summary>
    private readonly IPerformanceLogger _performanceLogger;

    /// <summary>
    /// The configuration
    /// </summary>
    private readonly IConfiguration _config;

    /// <summary>
    /// The approval detail provider
    /// </summary>
    private readonly IApprovalDetailProvider _approvalDetailProvider;

    /// <summary>
    /// The approval history provider
    /// </summary>
    private readonly IApprovalHistoryProvider _approvalHistoryProvider;

    /// <summary>
    /// Tenant Factory
    /// </summary>
    private readonly ITenantFactory _tenantFactory;

    /// <summary>
    /// Email Helper
    /// </summary>
    private readonly IEmailHelper _emailHelper;

    /// <summary>
    /// Initializes a new instance of the <see cref="ReminderProcessor"/> class.
    /// </summary>
    /// <param name="dataInterface"></param>
    /// <param name="notificationProvider"></param>
    /// <param name="logger"></param>
    /// <param name="emailTemplateHelper"></param>
    /// <param name="nameResolutionHelper"></param>
    /// <param name="performanceLogger"></param>
    /// <param name="config"></param>
    /// <param name="approvalDetailProvider"></param>
    /// <param name="approvalHistoryProvider"></param>
    /// <param name="tenantFactory"></param>
    /// <param name="emailHelper"></param>
    public ReminderProcessor(IReminderData dataInterface,
        INotificationProvider notificationProvider,
        ILogProvider logger,
        IEmailTemplateHelper emailTemplateHelper,
        INameResolutionHelper nameResolutionHelper,
        IPerformanceLogger performanceLogger,
        IConfiguration config,
        IApprovalDetailProvider approvalDetailProvider,
        IApprovalHistoryProvider approvalHistoryProvider,
        ITenantFactory tenantFactory,
        IEmailHelper emailHelper)
    {
        _dataInterface = dataInterface;
        _notificationProvider = notificationProvider;
        _logger = logger;
        _emailTemplateHelper = emailTemplateHelper;
        _nameResolutionHelper = nameResolutionHelper;
        _performanceLogger = performanceLogger;
        _config = config;
        _approvalDetailProvider = approvalDetailProvider;
        _approvalHistoryProvider = approvalHistoryProvider;
        _tenantFactory = tenantFactory;
        _emailHelper = emailHelper;
    }

    #region Implemented Methods

    /// <summary>
    /// Sends the reminders.
    /// </summary>
    /// <param name="currentTime">The current time.</param>
    /// <param name="maxFailureCount">The maximum failure count.</param>
    /// <param name="batchSize">Size of the batch.</param>
    /// <param name="approvalTenantInfo">The approval tenant information.</param>
    /// <param name="approvalsBaseUrl">The approvals base URL.</param>
    public async Task SendReminders(DateTime currentTime,
        int maxFailureCount,
        int batchSize,
        List<ApprovalTenantInfo> approvalTenantInfo,
        string approvalsBaseUrl)
    {
        var logData = new Dictionary<LogDataKey, object>
        {
            { LogDataKey.EntryUtcDateTime, currentTime },
            { LogDataKey.MaxFailureCount, maxFailureCount },
            { LogDataKey.BatchSize, batchSize },
            { LogDataKey.ApprovalBaseUrl, approvalsBaseUrl }
        };

        try
        {
            int failureCountWatchdog = 0;
            await SendWatchdogNotifications(currentTime, batchSize, maxFailureCount, approvalTenantInfo, approvalsBaseUrl, logData, failureCountWatchdog);
        }
        catch (Exception ex)
        {
            _logger.LogError(TrackingEvent.WatchDogReminderFailed, ex, logData);
        }
    }

    /// <summary>
    /// This method sends Watchdog notifications to approver
    /// </summary>
    /// <param name="currentTime">current UTC time</param>
    /// <param name="batchSize">batch notification size</param>
    /// <param name="maxFailureCount">The maximum permissible limit for send notification failure</param>
    /// <param name="approvalTenantInfo">Approval tenant info</param>
    /// <param name="approvalsBaseUrl">Base url of MSA website</param>
    /// <param name="genericLogData">log data</param>
    /// <param name="failureCountWatchdog">Number of failed notifications</param>
    private async Task SendWatchdogNotifications(DateTime currentTime, int batchSize, int maxFailureCount, List<ApprovalTenantInfo> approvalTenantInfo, string approvalsBaseUrl, Dictionary<LogDataKey, object> genericLogData, int failureCountWatchdog)
    {
        EmailType emailType = EmailType.NormalEmail;
        int totalWatchdogNotificationsToBeSent = 0;
        int totalActionableEmailsSent = 0;
        int totalNormalEmailsSent = 0;
        int count = 0;
        using (var documentActionTracer = _performanceLogger.StartPerformanceLogger("PerfLog", Constants.WorkerRole, Constants.PerfLogWatchdogEmailProcessing, genericLogData))
        {
            // Filtering Tenant Info list according to IsDigestEmailEnabled flag status
            var tenantsSubscribedForWatchDog = approvalTenantInfo.Where(t => !t.IsDigestEmailEnabled).ToList();
            // Get summary rows for sending watchdog notifications
            List<ApprovalSummaryRow> watchdogNotificationsToSend = _dataInterface.GetApprovalsNeedingReminders(currentTime, tenantsSubscribedForWatchDog).ToList<ApprovalSummaryRow>();

            //// To check for record marked as Out of sync and LOB pending true and offline approver.
            //// In any of these conditions reminder emails will not be sent.
            var approvalFilteredList = watchdogNotificationsToSend.Where(x => x.IsOutOfSyncChallenged == false &&
                            x.LobPending == false &&
                            x.IsOfflineApproval == false);

            var approvalsNeedingRemindersList = approvalFilteredList
                .Where(a => approvalTenantInfo.Select(t => t.AppName.ToUpper()).ToList().Contains(a.Application.ToUpper())).ToList();

            var distinctApproverFilteredList = approvalsNeedingRemindersList.GroupBy(a => new { a?.DocumentNumber, a?.NotificationJson?.FromJson<NotificationDetail>()?.To }).Select(a => a?.First())?.ToList();
            totalWatchdogNotificationsToBeSent = distinctApproverFilteredList != null ? distinctApproverFilteredList.Count : 0;
            foreach (var watchdogNotification in distinctApproverFilteredList)
            {
                var logData = new Dictionary<LogDataKey, object>
                {
                    { LogDataKey.EntryUtcDateTime, currentTime },
                    { LogDataKey.MaxFailureCount, maxFailureCount },
                    { LogDataKey.BatchSize, batchSize },
                    { LogDataKey.ApprovalBaseUrl, approvalsBaseUrl }
                };

                try
                {
                    if (failureCountWatchdog >= maxFailureCount)
                    {
                        break;
                    }

                    ApprovalTenantInfo tenantInfo = approvalTenantInfo.FirstOrDefault((t) => t.AppName == watchdogNotification.Application);
                    ITenant tenant = _tenantFactory.GetTenant(tenantInfo);

                    if (tenantInfo != null)
                    {
                        logData.Add(LogDataKey.BusinessProcessName, string.Format(tenantInfo.BusinessProcessName, Constants.BusinessProcessNameSendNotificationToUser, Constants.BusinessProcessNameSendNotificationWatchdogReminder));
                    }

                    logData.Add(LogDataKey.Xcv, watchdogNotification.Xcv);
                    logData.Add(LogDataKey.Tcv, watchdogNotification.Tcv);
                    logData.Add(LogDataKey.DXcv, watchdogNotification.DocumentNumber);
                    NotificationDetail notificationDetails = ReminderHelper.GetNotificationDetails(watchdogNotification);
                    _logger.LogInformation(TrackingEvent.WatchDogReminderNotificationDetailFetched, logData);

                    var emailData = new NotificationItem();

                    // Add summaryRow into summaryRows list
                    List<ApprovalSummaryRow> summaryRows = new List<ApprovalSummaryRow>
                    {
                        watchdogNotification
                    };

                    // Check NotifyWatchDogEmailWithApprovalFunctionality flag for watchdog to send actionable email.
                    // Validate fligthing feature check for tenant and users
                    if (tenantInfo.NotifyWatchDogEmailWithApprovalFunctionality && tenant.ValidateIfEmailShouldBeSentWithDetails(summaryRows))
                    {
                        emailType = EmailType.ActionableEmail;
                        emailData = await CreateEmailBody(watchdogNotification, notificationDetails, tenantInfo, approvalsBaseUrl, emailType, logData, tenant);
                    }
                    else
                    {
                        emailType = EmailType.NormalEmail;
                        // Send normal reminder mail to users
                        emailData = await CreateEmailBody(watchdogNotification, notificationDetails, tenantInfo, approvalsBaseUrl, emailType, logData, tenant);
                    }

                    // In case of Watchdog Reminder email for multiple approvers at the same level, we were observing that the emails were sent multiple time (which is expected, as there are multiple summary rows)
                    // But in case of COSMIC, they are sending the 'To' list as well (which were the approvers ar the current level)
                    // This was resulting into a multiple emails sent to all the approvers.
                    // To handle this scenario, as a quick work around overwritten the way the 'To' list is fetched and got it from the SummaryRow.approver
                    string destination = tenant.GetApproverListForSendingNotifications(watchdogNotification, notificationDetails);

                    emailData.To = destination;
                    _notificationProvider.Tenant = tenant;
                    await _notificationProvider.SendEmail(emailData);

                    if (emailType == EmailType.ActionableEmail)
                    {
                        totalActionableEmailsSent++;
                    }
                    else if (emailType == EmailType.NormalEmail)
                    {
                        totalNormalEmailsSent++;
                    }

                    var updateNextReminderList = approvalsNeedingRemindersList?.Where(w => w?.DocumentNumber == watchdogNotification?.DocumentNumber);
                    foreach (var updateNextReminder in updateNextReminderList)
                    {
                        updateNextReminder.NextReminderTime = ReminderHelper.NextReminderTime(notificationDetails, currentTime).GetDateTimeWithUtcKind();

                        _dataInterface.UpdateReminderInfo(tenantInfo, updateNextReminder);
                    }
                    if (count++ >= batchSize)
                    {
                        count = 0;
                        Thread.Sleep(new TimeSpan(0, 2, 0));
                    }
                }
                catch (Exception ex)
                {
                    failureCountWatchdog++;
                    _logger.LogError(TrackingEvent.WatchDogReminderSingleReminderFailed, ex, logData);
                }
            }
        }

        genericLogData.Add(LogDataKey.TotalWatchdogNotificationsToBeSent, totalWatchdogNotificationsToBeSent);
        genericLogData.Add(LogDataKey.TotalNormalEmailsSent, totalNormalEmailsSent);
        genericLogData.Add(LogDataKey.TotalActionableEmailsSent, totalActionableEmailsSent);
        genericLogData.Add(LogDataKey.WatchdogFailureCount, failureCountWatchdog);
        //Log if Failure Count reaches Threshold Value
        if (failureCountWatchdog >= maxFailureCount)
        {
            _logger.LogError(TrackingEvent.WatchDogReminderFailureCapReached, new Exception(TrackingEvent.WatchDogReminderFailureCapReached.ToString()), genericLogData);
        }
        else
        {
            _logger.LogInformation(TrackingEvent.WatchDogRemindersSent, genericLogData);
        }
    }

    #endregion Implemented Methods

    #region ReminderProcessor Methods

    /// <summary>
    /// Get Reminder TemplateKey
    /// </summary>
    /// <param name="reminderDetail"></param>
    /// <returns></returns>
    private string GetReminderTemplateKey(Contracts.DataContracts.ReminderDetail reminderDetail)
    {
        if (!String.IsNullOrEmpty(reminderDetail.ReminderTemplate))
        {
            return reminderDetail.ReminderTemplate.Trim();
        }
        else
        {
            return "defaultReminder|Reminder";
        }
    }

    /// <summary>
    /// Creates the email body.
    /// </summary>
    /// <param name="summaryRow">The summary row.</param>
    /// <param name="notificationDetails">The notification details.</param>
    /// <param name="tenantInfo">Approval tenant information.</param>
    /// <param name="approvalsBaseUrl">The approvals base URL.</param>
    /// <returns></returns>
    /// <exception cref="System.Exception">
    /// No reminder details are present
    /// or
    /// Email template not found.
    /// </exception>
    private async Task<NotificationItem> CreateEmailBody(ApprovalSummaryRow summaryRow, NotificationDetail notificationDetails, ApprovalTenantInfo tenantInfo, string approvalsBaseUrl, EmailType emailType, Dictionary<LogDataKey, object> logData, ITenant tenant)
    {
        if (notificationDetails.Reminder == null)
        {
            throw new InvalidDataException("No reminder details are present");
        }

        _emailHelper.PopulateDataInToDictionary<LogDataKey, object>(logData, LogDataKey.TenantName, summaryRow?.Application);
        _emailHelper.PopulateDataInToDictionary<LogDataKey, object>(logData, LogDataKey.Approver, summaryRow?.Approver);
        _emailHelper.PopulateDataInToDictionary<LogDataKey, object>(logData, LogDataKey.IsEmailSentWithDetail, emailType);
        var notificationMessageId = Guid.NewGuid().ToString();
        logData.Add(LogDataKey.NotificationMessageId, notificationMessageId);
        NotificationItem emailData = null;
        JObject responseJObject = null;
        List<NotificationDataAttachment> notificationDataAttachment = new List<NotificationDataAttachment>();
        string templateKey = string.Empty;
        bool isActionableEmailSentFail = false;

        // Get Reminder templatekey from remider details
        string pattern = GetReminderTemplateKey(notificationDetails.Reminder);
        string OriginalNotificationTemplateKey = pattern;

        // this check is for emails with detail templates.
        if (tenantInfo.NotifyWatchDogEmailWithApprovalFunctionality && EmailType.ActionableEmail == emailType)
        {
            // Split notification template key for reminder e.g cosmic|Reminder
            var patternArry = pattern.Split('|');
            templateKey = patternArry[0].ToString();
            var actionName = patternArry[1].ToString();

            // Store original key into notificationTemplateKey e.g cosmic
            string notificationTemplateKey = templateKey;

            // Append key for actionable email e.g cosmicWithAction|Reminder
            templateKey += Constants.EmailNotificationWithActionTemplateKey;
            pattern = string.Format("{0}|{1}", templateKey, actionName);

            SummaryJson summaryJson = summaryRow.SummaryJson.ToJObject().ToObject<SummaryJson>();
            responseJObject = _emailHelper.GetDetailsDataforApprovalRequest(tenantInfo, summaryJson.ApprovalIdentifier, summaryRow, logData);

            tenant = _tenantFactory.GetTenant(tenantInfo);

            if (responseJObject == null || (responseJObject.Property("Message") != null && responseJObject.Property("Message").Value.ToString() != string.Empty))
            {
                // Send generic email with tile details
                // Append key for generic email e.g cosmic|Reminder
                templateKey = notificationTemplateKey;
                pattern = string.Format("{0}|{1}", templateKey, actionName);
                _logger.LogError(TrackingEvent.WatchDogReminderEmailNotificationWithApprovalFunctionalityFail, new Exception("No Actionable Email - Send email with tile details because all details data is not available yet."), logData);
            }
            else if (!_emailHelper.IsNotificationEmailAllowed(responseJObject))
            {
                try
                {
                    // Fetch missing details from LOB system and store it in azuretable
                    _logger.LogInformation(TrackingEvent.ActionableEmailAPIDetailsFetch, logData);
                    _emailHelper.FetchMissingDetailsDataFromLOB(responseJObject, tenantInfo, summaryJson.ApprovalIdentifier, summaryRow, logData);
                }
                catch (Exception ex)
                {
                    // Send generic email with tile details
                    // Append key for generic email e.g cosmic|Reminder
                    templateKey = notificationTemplateKey;
                    pattern = string.Format("{0}|{1}", templateKey, actionName);
                    _logger.LogError(TrackingEvent.WatchDogReminderEmailNotificationWithApprovalFunctionalityFail, new InvalidDataException("No Actionable Email - Send email with tile details because  CallBackURLCollection is null and there is a need to call back further, which fails actionable email model." + ex.Message.ToString() + ex.StackTrace.ToString() + ex.InnerException?.ToString()), logData);
                }
            }

            if (responseJObject != null && responseJObject["Attachments"] != null && responseJObject["Attachments"].Count() > 0)
            {
                // fetch attachment from blob storage/LOB
                _logger.LogInformation(TrackingEvent.ActionableEmailGetAttachments, logData);
                // Flag to determine if all downloadable attachments are downloaded successfully using ID
                bool isAttachmentDownloadSuccess = false;
                notificationDataAttachment = _emailHelper.GetAttachmentsToAttachInEmail(responseJObject, summaryJson.ApprovalIdentifier, tenantInfo, tenant, logData, ref isAttachmentDownloadSuccess);

                if (!isAttachmentDownloadSuccess)
                {
                    // Sent email without actions button with detail
                    // Append key for without attachment email e.g cosmicWithDetails|Reminder
                    templateKey = notificationTemplateKey + Constants.EmailNotificationWithDetailsTemplateKey;
                    pattern = string.Format("{0}|{1}", templateKey, actionName);
                    _logger.LogError(TrackingEvent.WatchDogReminderEmailNotificationFailedToLoadAttachments, new Exception("Actionable Email without Action Buttons - Unable to retrieve attachment(s) from BLOB."), logData);
                }
                else
                {
                    decimal totalAttachmentFileByteSize = 0;
                    foreach (var attachment in notificationDataAttachment)
                    {
                        totalAttachmentFileByteSize += attachment.FileSize;
                    }
                    decimal totalAttachmentFileSizeMB = totalAttachmentFileByteSize / (1024 * 1024);
                    if (totalAttachmentFileSizeMB > int.Parse(_config[ConfigurationKey.AttachmentSizeLimit.ToString()]))
                    {
                        // Set original notificationkey for normal email. e.g. PendingApproval
                        templateKey = notificationTemplateKey;
                        // Call SendEmail function with  EmailType.EmailWithoutDetail to send normal email
                        emailType = EmailType.NormalEmail;
                        notificationDataAttachment.RemoveRange(0, notificationDataAttachment.Count);
                    }
                }
            }
        }

        _emailHelper.PopulateDataInToDictionary<LogDataKey, object>(logData, LogDataKey.NotificationTemplateKey, pattern);

        try
        {
            emailData = await PreparedEmailData(summaryRow, tenantInfo, approvalsBaseUrl, responseJObject, tenant, emailType, logData, templateKey, pattern, notificationDetails, notificationDataAttachment, notificationMessageId);
        }
        catch (FileNotFoundException)
        {
            throw;
        }
        catch (Exception ex)
        {
            // Send Normal Email if Actionable email failed.
            // Set isActionableEmailSentFail to true
            if (emailType == EmailType.ActionableEmail)
            {
                _logger.LogError(TrackingEvent.WatchDogReminderEmailNotificationWithApprovalFunctionalityFail, new InvalidDataException("No Actionable Email - Send regular email with details" + ex.Message.ToString() + ex.StackTrace.ToString() + ex.InnerException?.ToString()), logData);
                // Set isActionableEmailSentFail = true to Send regular email for reminder
                isActionableEmailSentFail = true;
            }
            else
            {
                throw;
            }
        }

        // Send Normal Email based on isActionableEmailSentFail flag

        #region Send Normal Email

        if (isActionableEmailSentFail)
        {
            try
            {
                // Send regular email for reminder
                emailData = await PreparedEmailData(summaryRow, tenantInfo, approvalsBaseUrl, responseJObject, tenant, EmailType.NormalEmail, logData, templateKey, OriginalNotificationTemplateKey, notificationDetails, notificationDataAttachment, notificationMessageId);
            }
            catch (Exception)
            {
                throw;
            }
        }

        #endregion Send Normal Email

        return emailData;
    }

    /// <summary>
    /// Prepared email data
    /// </summary>
    /// <param name="summaryRow"></param>
    /// <param name="tenantInfo"></param>
    /// <param name="approvalsBaseUrl"></param>
    /// <param name="responseJObject"></param>
    /// <param name="tenant"></param>
    /// <param name="emailType"></param>
    /// <param name="logData"></param>
    /// <param name="templateKey"></param>
    /// <param name="pattern"></param>
    /// <param name="notificationDetails"></param>
    /// <param name="notificationDataAttachment"></param>
    /// <returns></returns>
    private async Task<NotificationItem> PreparedEmailData(ApprovalSummaryRow summaryRow, ApprovalTenantInfo tenantInfo, string approvalsBaseUrl, JObject responseJObject, ITenant tenant, EmailType emailType, Dictionary<LogDataKey, object> logData, string templateKey, string pattern, NotificationDetail notificationDetails, List<NotificationDataAttachment> notificationDataAttachment, string notificationMessageId)
    {
        ApprovalEmailNotificationTemplates template = null;
        var emailData = new NotificationItem();

        pattern = "^" + pattern + "$";
        pattern = pattern.Replace("|", "\\|");

        var templates = _emailTemplateHelper.GetTemplates(tenantInfo.TenantId.ToString());

        template = templates.Where(t => Regex.IsMatch(t.RowKey, pattern)).FirstOrDefault();

        if (template != null)
        {
            var emailTemplate = template.TemplateContent;

            if (emailTemplate != null)
            {
                emailData = await MapSummaryData(emailTemplate, summaryRow, tenantInfo, approvalsBaseUrl, responseJObject, tenant, emailType, logData, templateKey, notificationDetails, templates, template.TemplateId);
                emailData.Bcc = notificationDetails.Bcc;
                emailData.Subject = GetSubject(emailData.Body);
                emailData.Cc = notificationDetails.Cc;
                emailData.To = notificationDetails.To;

                emailData.NotificationTypes = new List<NotificationType>();
                emailData.Telemetry = new Telemetry() { Xcv = summaryRow?.Xcv, MessageId = notificationMessageId };
                // Set TemplateId null to avoid NF logic app to refetch the template and send email with placeholders.
                // TODO: Undo this once the place holder replace logic is moved from MSA to NF.
                emailData.TemplateId = template.TemplateId; // string.Format("{0}|{1}", deviceNotificationInfo.NotificationTemplateKey, deviceNotificationInfo.NotificationType.ToString());
                emailData.TenantIdentifier = tenantInfo.RowKey;
                emailData.NotificationTypes.Add(NotificationType.Mail);

                // to bind the attachments.
                emailData.Attachments = notificationDataAttachment?.Select(item => new NotificationDataAttachment
                {
                    FileBase64 = item.FileBase64,
                    FileName = item.FileName,
                    IsInline = item.IsInline,
                    FileUrl = item.FileUrl,
                    FileSize = item.FileSize
                }).ToList();

                _notificationProvider.Tenant = tenant;
            }
        }
        else
        {
            throw new FileNotFoundException(Constants.EmailTemplateNotFound);
        }

        return emailData;
    }

    /// <summary>
    /// Maps the summary data for Watchdog emails.
    /// </summary>
    /// <param name="templateContent">Content of the template.</param>
    /// <param name="summaryRow">The summary row.</param>
    /// <param name="tenantInfo">The tenant information.</param>
    /// <param name="approvalsBaseUrl">The approvals base URL.</param>
    /// <param name="responseJObject">it's contains details information for actionable email placeholder</param>
    /// <param name="tenant">tenant interface object</param>
    /// <param name="emailType">The email type</param>
    /// <param name="logData">Log data for telemetry</param>
    /// <returns>returns a string</returns>
    private async Task<NotificationItem> MapSummaryData(string templateContent, ApprovalSummaryRow summaryRow, ApprovalTenantInfo tenantInfo, string approvalsBaseUrl, JObject responseJObject, ITenant tenant, EmailType emailType, Dictionary<LogDataKey, object> logData, string templateKey, NotificationDetail notificationDetails, IEnumerable<ApprovalEmailNotificationTemplates> templates, string templateId)
    {
        NotificationItem notificationFrameworkItem = new NotificationItem();
        notificationFrameworkItem.TemplateId = templateId;

        // TODO:: Quick Fix for Approver details shown as placeholders in Watch-Dog Emails
        JObject summaryWithApprover = (summaryRow.SummaryJson).ToJObject();
        await AddApprover(summaryRow.Approver, summaryWithApprover, _nameResolutionHelper);
        string summaryData = summaryWithApprover.ToString();

        //
        var additionalDataObj = _approvalDetailProvider.GetApprovalsDetails(tenantInfo.TenantId, summaryRow.DocumentNumber, Constants.AdditionalDetails)?.JSONData?.ToJObject();
        Dictionary<string, string> additionalData = !string.IsNullOrEmpty(additionalDataObj?[Constants.AdditionalData]?.ToJson()) ? (additionalDataObj?[Constants.AdditionalData]?.ToJson()).FromJson<Dictionary<string, string>>() : null;

        SummaryJson summaryJson = summaryRow.SummaryJson.ToJObject().ToObject<SummaryJson>();
        summaryJson.AdditionalData = additionalData;

        Dictionary<string, string> placeHolderDict = new Dictionary<string, string>();
        JSONHelper.ConvertJsonToDictionary(placeHolderDict, summaryData);

        var deviceDetailPage = String.Concat(_config[ConfigurationKey.ApprovalsBaseUrl.ToString()], _config[ConfigurationKey.DeviceDeepLinkUrl.ToString()]);

        _emailHelper.PopulateDataInToDictionary(placeHolderDict, "AdditionalData.RequestorName", (summaryJson.AdditionalData != null && summaryJson.AdditionalData.ContainsKey("RequestorName")) ?
                                                                    summaryJson.AdditionalData["RequestorName"] : string.Empty);

        if (placeHolderDict.TryGetValue("ApprovalIdentifier.DisplayDocumentNumber", out string documentNumber))
        {
            placeHolderDict.TryGetValue("ApprovalIdentifier.FiscalYear", out string fiscalYear);
            _emailHelper.PopulateDataInToDictionary(placeHolderDict, "MSApprovalsDetailPage", String.Format(approvalsBaseUrl + tenantInfo.TenantDetailUrl, tenantInfo.TenantId, documentNumber, fiscalYear, tenantInfo.TemplateName));
            _emailHelper.PopulateDataInToDictionary(placeHolderDict, "MSApprovalsDeviceDetailPage", string.Format(deviceDetailPage, tenantInfo.TenantId, documentNumber, tenantInfo.TemplateName));
        }

        if (!logData.ContainsKey(LogDataKey.DisplayDocumentNumber))
        {
            logData.Add(LogDataKey.DisplayDocumentNumber, summaryJson.ApprovalIdentifier.DisplayDocumentNumber);
        }
        else
        {
            logData[LogDataKey.DisplayDocumentNumber] = summaryJson.ApprovalIdentifier.DisplayDocumentNumber;
        }

        if (!logData.ContainsKey(LogDataKey.DocumentNumber))
        {
            logData.Add(LogDataKey.DocumentNumber, summaryJson.ApprovalIdentifier.DocumentNumber);
        }
        else
        {
            logData[LogDataKey.DocumentNumber] = summaryJson.ApprovalIdentifier.DocumentNumber;
        }

        _emailHelper.PopulateDataInToDictionary(placeHolderDict, "ToolName", tenantInfo.ToolName);
        _emailHelper.PopulateDataInToDictionary(placeHolderDict, "TenantName", tenantInfo.AppName);
        _emailHelper.PopulateDataInToDictionary(placeHolderDict, "TenantId", tenantInfo.RowKey);
        _emailHelper.PopulateDataInToDictionary(placeHolderDict, "AppName", tenantInfo.AppName);
        _emailHelper.PopulateDataInToDictionary(placeHolderDict, "DocumentNumberPrefix", tenantInfo.DocumentNumberPrefix);

        //// Add items to PlaceHolder dictionary for Outlook Quick Action
        _emailHelper.PopulateDataInToDictionary(placeHolderDict, "Xcv", summaryRow.Xcv.ToString());
        var tcv = Guid.NewGuid().ToString();
        _emailHelper.PopulateDataInToDictionary(placeHolderDict, "Tcv", tcv);
        _emailHelper.PopulateDataInToDictionary(placeHolderDict, "SessionId", tcv);
        _emailHelper.PopulateDataInToDictionary(placeHolderDict, "UserMessageForComplianceAndAction", _config[ConfigurationKey.UserMessageForComplianceAndAction.ToString()]);
        _emailHelper.PopulateDataInToDictionary(placeHolderDict, "MSApprovalsCoreServiceURL", _config[ConfigurationKey.ApprovalsCoreServicesURL.ToString()]);

        _emailHelper.PopulateDataInToDictionary(placeHolderDict, "MSApprovalsResourceId", _config[ConfigurationKey.ApprovalsAudienceUrl.ToString()]);
        _emailHelper.PopulateDataInToDictionary(placeHolderDict, "MSApprovalsBaseUrl", _config[ConfigurationKey.ApprovalsBaseUrl.ToString()]);
        _emailHelper.PopulateDataInToDictionary(placeHolderDict, "CurrentApprovers", responseJObject?.SelectToken("CurrentApprovers")?.Value<string>());
        if (placeHolderDict.ContainsKey("UnitValue"))
        {
            _emailHelper.PopulateDataInToDictionary(placeHolderDict, "UnitValue", (Double.TryParse(placeHolderDict["UnitValue"], out double doubleUnitValue) ? doubleUnitValue.ToString("N", CultureInfo.CreateSpecificCulture(Constants.CultureName)) : placeHolderDict["UnitValue"]));
        }

        if (placeHolderDict != null && placeHolderDict.ContainsKey("SubmittedDate"))
        {
            string submittedDate = DateTime.Parse(placeHolderDict["SubmittedDate"], CultureInfo.CreateSpecificCulture(Constants.CultureName)).ToString("MM/dd/yy");
            _emailHelper.PopulateDataInToDictionary(placeHolderDict, "SubmittedDate", submittedDate);
        }

        List<TransactionHistoryExt> historyDataExt = await _approvalHistoryProvider.GetApproverChainHistoryDataAsync(tenantInfo, summaryJson.ApprovalIdentifier.DisplayDocumentNumber, string.Empty, Environment.UserName, summaryRow.Xcv, summaryRow.Tcv, string.Empty);

        var summaryRowApproverChain = summaryData.ToJObject();
        string approverChainString = await _emailHelper.CreateCurrentApproverChain(
                                                                       summaryRowApproverChain,
                                                                       historyDataExt,
                                                                       summaryRow.Approver,
                                                                       tenantInfo);
        var approverChain = (approverChainString)?.ToJArray();
        if (responseJObject != null && approverChain != null)
        {
            if (responseJObject.ContainsKey("Approvers"))
            {
                responseJObject["Approvers"] = approverChain;
            }
            else
            {
                responseJObject.Add("Approvers", approverChain);
            }
        }
        string emailBodySection = string.Empty;
        string currentApproverType = string.Empty;
        JArray approverChainItems = new JArray();
        JArray approverChainColumns = new JArray();
        if (approverChain != null)
        {
            foreach (var item in approverChain)
            {
                string approverType = string.Format("{0}", item["Type"]);
                string formattedApproverType = !string.IsNullOrEmpty(approverType) ? "(" + item["Type"] + ")" : string.Empty;
                string actionDate = string.Format("{0}", item["ActionDate"]);
                string notes = string.Format("{0}", item["Notes"]);
                string delegateUser = string.Format("{0}", item["DelegateUser"]);
                string justification = string.Format("{0}", item["Justification"]);
                if (string.IsNullOrEmpty(item["Action"].ToString()) && !Convert.ToBoolean(item["_future"].ToString()))
                {
                    emailBodySection += string.Format(Constants.ApproverChainCurrent, item["Name"], item["Alias"], formattedApproverType);
                    currentApproverType = approverType;
                    AdaptiveCardHelper.GetItems("TextBlock", string.Format("&#x2794; {0} [{1}] {2}", item["Name"], item["Alias"], formattedApproverType), ref approverChainItems);
                }
                else if (string.IsNullOrEmpty(item["Action"].ToString()) && Convert.ToBoolean(item["_future"].ToString()))
                {
                    emailBodySection += string.Format(Constants.ApproverChainFuture, item["Name"], item["Alias"], formattedApproverType, item["Action"]);
                    AdaptiveCardHelper.GetItems("TextBlock", string.Format("{0} [{1}] {2} {3}", item["Name"], item["Alias"], formattedApproverType, item["Action"]), ref approverChainItems);
                }
                else
                {
                    if (string.IsNullOrEmpty(delegateUser))
                    {
                        emailBodySection += string.Format(Constants.ApproverChain, item["Name"], item["Alias"], formattedApproverType, item["Action"], !string.IsNullOrEmpty(actionDate) ? " on " + DateTime.Parse(actionDate, CultureInfo.CreateSpecificCulture(Constants.CultureName)).ToString("MM/dd/yy hh:mm tt") + "" : string.Empty);
                        AdaptiveCardHelper.GetItems("TextBlock", string.Format("{0} [{1}] {2} - {3} {4}", item["Name"], item["Alias"], formattedApproverType, item["Action"], !string.IsNullOrEmpty(actionDate) ? " on " + DateTime.Parse(actionDate, CultureInfo.CreateSpecificCulture(Constants.CultureName)).ToString("MM/dd/yy hh:mm tt") + "" : string.Empty), ref approverChainItems);
                    }
                    else
                    {
                        emailBodySection += string.Format(Constants.ApproverChain, _nameResolutionHelper.GetUserName(delegateUser), delegateUser, formattedApproverType, item["Action"], !string.IsNullOrEmpty(actionDate) ? "(on behalf of " + item["Name"] + ")  on " + DateTime.Parse(actionDate, CultureInfo.CreateSpecificCulture(Constants.CultureName)).ToString("MM/dd/yy hh:mm tt") + "" : string.Empty);
                        AdaptiveCardHelper.GetItems("TextBlock", string.Format("{0} [{1}] {2} - {3} {4}", _nameResolutionHelper.GetUserName(delegateUser), delegateUser, formattedApproverType, item["Action"], !string.IsNullOrEmpty(actionDate) ? "(on behalf of " + item["Name"] + ")  on " + DateTime.Parse(actionDate, CultureInfo.CreateSpecificCulture(Constants.CultureName)).ToString("MM/dd/yy hh:mm tt") + "" : string.Empty), ref approverChainItems);
                    }

                    if (!string.IsNullOrEmpty(justification))
                    {
                        emailBodySection += string.Format(Constants.ApproverChainNotes, "Justification:", justification);
                        AdaptiveCardHelper.GetItems("TextBlock", string.Format("*{0}* {1}", "Justification:", justification), ref approverChainItems, horizontalAlignment: "left", padding: "small");
                    }

                    if (!string.IsNullOrEmpty(notes))
                    {
                        emailBodySection += string.Format(Constants.ApproverChainNotes, "Notes:", notes);
                        AdaptiveCardHelper.GetItems("TextBlock", string.Format("*{0}* {1}", "Notes:", notes), ref approverChainItems, horizontalAlignment: "left", padding: "small");
                    }
                }
            }
        }

        emailBodySection = string.Format(Constants.ApproverChainHeader, emailBodySection);
        _emailHelper.PopulateDataInToDictionary(placeHolderDict, "ApproverChain", emailBodySection);
        _emailHelper.PopulateDataInToDictionary(placeHolderDict, "ApproverType", currentApproverType);

        AdaptiveCardHelper.GetColumns(approverChainItems, ref approverChainColumns);
        JObject adaptiveApproverChain = AdaptiveCardHelper.GetColumnSets(approverChainColumns);
        placeHolderDict.Add("AdaptiveApproverChain", adaptiveApproverChain?.ToString());

        // To check if theNotifyEmail With approval functionality is set to true.
        if (tenantInfo.NotifyWatchDogEmailWithApprovalFunctionality && EmailType.ActionableEmail == emailType)
        {
            // TODO:: This should be removed once outlook rollout actionable email for mobiles
            // Add MSApprovalsDetailPage key into responseJObject to show deeplink for mobile device users in actionable email
            if (responseJObject != null)
            {
                if (placeHolderDict.ContainsKey("MSApprovalsDetailPage"))
                {
                    if (responseJObject.SelectToken("MSApprovalsDetailPage") == null)
                    {
                        responseJObject.Add("MSApprovalsDetailPage", placeHolderDict["MSApprovalsDetailPage"].ToString());
                    }
                }
                if (placeHolderDict.ContainsKey("MSApprovalsDeviceDetailPage"))
                {
                    if (responseJObject.SelectToken("MSApprovalsDeviceDetailPage") == null)
                    {
                        responseJObject.Add("MSApprovalsDeviceDetailPage", placeHolderDict["MSApprovalsDeviceDetailPage"].ToString());
                    }
                }

                if (responseJObject.SelectToken("ApproverChain") == null)
                {
                    if (!string.IsNullOrEmpty(emailBodySection))
                    {
                        responseJObject.Add("ApproverChain", emailBodySection);
                    }
                    else
                    {
                        responseJObject.Add("ApproverChain", summaryRow.Approver);
                    }
                }

                if (!string.IsNullOrEmpty(templateKey))
                {
                    responseJObject.Add("NotificationTemplateKey", templateKey);
                }

                if (!string.IsNullOrEmpty(adaptiveApproverChain?.ToString()))
                {
                    responseJObject.Add("AdaptiveApproverChain", adaptiveApproverChain?.ToString());
                }

                _emailHelper.PopulateDataInToDictionary(placeHolderDict, "ReceiptAcknowledgmentMessage", _config[ConfigurationKey.ReceiptAcknowledgmentMessage.ToString()]);
                _emailHelper.PopulateDataInToDictionary(placeHolderDict, "AntiCorruptionMessage", _config[ConfigurationKey.AntiCorruptionMessage.ToString()]);
                _emailHelper.PopulateDataInToDictionary(placeHolderDict, "AcceptsTerms", _config[ConfigurationKey.AcceptsTerms.ToString()]);

                string submitterAlias = responseJObject.SelectToken("Submitter.Alias") != null ? responseJObject.SelectToken("Submitter.Alias").Value<string>() : string.Empty;

                // fetch user profile image from blob and show into actionable email
                _emailHelper.PopulateDataInToDictionary(placeHolderDict, "UserImage", await _emailHelper.GetUserImage(submitterAlias, tcv, Constants.ClientDevice, logData));

                // Actionable email only visible, whoever are present in approver list
                _emailHelper.PopulateDataInToDictionary(placeHolderDict, "ApproverList", summaryRow.Approver);

                List<ApprovalSummaryRow> summaryRows = new List<ApprovalSummaryRow>
                {
                    summaryRow
                };

                //// to process complete email template if the NotifyEmail With approval functionality is true.
                string detailTemplate = _emailHelper.ConstructDynamicHtmlDetailsForEmail(responseJObject, tenantInfo, tenant, documentNumber, ref placeHolderDict, summaryRows, ref emailType);
                _emailHelper.PopulateDataInToDictionary(placeHolderDict, "DetailTemplate", detailTemplate.Trim());
            }
        }

        if (emailType.Equals(EmailType.NormalEmail.ToString()))
        {
            // Get Reminder templatekey from remider details
            var pattern = GetReminderTemplateKey(notificationDetails.Reminder);
            var OriginalNotificationTemplateKey = pattern;
            var template = templates.Where(t => Regex.IsMatch(t.RowKey, pattern)).FirstOrDefault();
            templateContent = template.TemplateContent;
            notificationFrameworkItem.TemplateId = template.TemplateId;
        }
        // Replace placeHolder data in template
        // Keep FillTemplateData method at last otherwise placeHolder doesn't replace properly.
        templateContent = FillTemplateData(templateContent, placeHolderDict, false);
        templateContent = FillTemplateData(templateContent, placeHolderDict);

        #region Adding MEO related Template keys

        if (placeHolderDict.TryGetValue("Approver.Name", out string approverName))
            placeHolderDict.Add("ApproverName", approverName);
        else
            placeHolderDict.Add("ApproverName", summaryRow.Approver);
        placeHolderDict.Add("ApprovalIdentifierDisplayDocumentNumber", summaryJson.ApprovalIdentifier.DisplayDocumentNumber);

        #endregion Adding MEO related Template keys

        notificationFrameworkItem.Body = templateContent;
        notificationFrameworkItem.TemplateData = placeHolderDict.ToDictionary(pair => pair.Key, pair => (object)pair.Value);

        return notificationFrameworkItem;
    }

    /// <summary>
    /// Fill template data
    /// </summary>
    /// <param name="templateContent"></param>
    /// <param name="placeHolderDict"></param>
    /// <param name="isPlaceHolderRemoved"></param>
    /// <returns></returns>
    private static string FillTemplateData(string templateContent, Dictionary<string, string> placeHolderDict, bool isPlaceHolderRemoved = true)
    {
        foreach (KeyValuePair<string, string> replaceableData in placeHolderDict)
        {
            templateContent = templateContent.Replace("#" + replaceableData.Key + "#", replaceableData.Value);
        }

        if (isPlaceHolderRemoved)
        {
            //to remove ActionDetails and AdditionalData related placeholders with string.empty if no value is present.
            templateContent = Regex.Replace(templateContent, @"#ActionDetails.[0-9a-zA-Z]*#", string.Empty, RegexOptions.IgnoreCase);
            templateContent = Regex.Replace(templateContent, @"#AdditionalData.[0-9a-zA-Z]*#", string.Empty, RegexOptions.IgnoreCase);
            templateContent = Regex.Replace(templateContent, @"#ApproverNotes#", string.Empty, RegexOptions.IgnoreCase);
        }

        return templateContent;
    }

    /// <summary>
    /// Adds Approver data to the JObject
    /// Used while sending Emails which uses SummaryJSON not having Approver details
    /// </summary>
    /// <param name="approverAlias"></param>
    /// <param name="dataWithApprover"></param>
    /// <param name="_nameResolutionHelper"></param>
    private static async Task AddApprover(string approverAlias, JObject dataWithApprover, INameResolutionHelper _nameResolutionHelper)
    {
        if (dataWithApprover["Approver"] == null)
        {
            Approver approver = new Approver
            {
                Alias = approverAlias,
                Name = await _nameResolutionHelper.GetUserName(approverAlias)
            };
            dataWithApprover.Add("Approver", (approver.ToJson()).ToJObject());
        }
    }

    /// <summary>
    /// Gets the subject.
    /// </summary>
    /// <param name="templateContent">Content of the template.</param>
    /// <returns>returns a string</returns>
    private string GetSubject(string templateContent)
    {
        string result = string.Empty;
        if (templateContent.Contains("<title>"))
        {
            int first = templateContent.IndexOf("<title>") + "<title>".Length;
            int last = templateContent.IndexOf("</title>");

            result = templateContent[first..last];
        }
        return result;
    }

    #endregion ReminderProcessor Methods
}