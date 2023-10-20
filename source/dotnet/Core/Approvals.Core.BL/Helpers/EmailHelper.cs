// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace Microsoft.CFS.Approvals.Core.BL.Helpers;

using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using global::Azure.Storage.Blobs.Models;
using Microsoft.CFS.Approvals.Common.BL.Interface;
using Microsoft.CFS.Approvals.Common.DL.Interface;
using Microsoft.CFS.Approvals.Contracts;
using Microsoft.CFS.Approvals.Contracts.DataContracts;
using Microsoft.CFS.Approvals.Core.BL.Interface;
using Microsoft.CFS.Approvals.Data.Azure.Storage.Interface;
using Microsoft.CFS.Approvals.Domain.BL.Interface;
using Microsoft.CFS.Approvals.Extensions;
using Microsoft.CFS.Approvals.LogManager.Model;
using Microsoft.CFS.Approvals.LogManager.Provider.Interface;
using Microsoft.CFS.Approvals.Model;
using Microsoft.CFS.Approvals.Utilities.Extension;
using Microsoft.CFS.Approvals.Utilities.Helpers;
using Microsoft.CFS.Approvals.Utilities.Interface;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using NotificationType = Model.NotificationType;

/// <summary>
/// The Email Helper class
/// </summary>
public class EmailHelper : IEmailHelper
{
    /// <summary>
    /// The Configuration
    /// </summary>
    private readonly IConfiguration _config;

    /// <summary>
    /// The approval summary provider
    /// </summary>
    private readonly IApprovalSummaryProvider _approvalSummaryProvider;

    /// <summary>
    /// The log provider
    /// </summary>
    private readonly ILogProvider _logProvider;

    /// <summary>
    /// The notification provider
    /// </summary>
    private readonly INotificationProvider _notificationProvider;

    /// <summary>
    /// The name resolution helper
    /// </summary>
    private readonly INameResolutionHelper _nameResolutionHelper;

    /// <summary>
    /// The email template helper
    /// </summary>
    private readonly IEmailTemplateHelper _emailTemplateHelper;

    /// <summary>
    /// The approval history provider
    /// </summary>
    private readonly IApprovalHistoryProvider _approvalHistoryProvider;

    /// <summary>
    /// The blob storage helper
    /// </summary>
    private readonly IBlobStorageHelper _blobStorageHelper;

    /// <summary>
    /// The details helper
    /// </summary>
    private readonly IDetailsHelper _detailsHelper;

    /// <summary>
    /// The image retriever
    /// </summary>
    private readonly IImageRetriever _imageRetriever;

    /// <summary>
    /// Constructor of EmailHelper
    /// </summary>
    /// <param name="config"></param>
    /// <param name="approvalSummaryProvider"></param>
    /// <param name="logProvider"></param>
    /// <param name="nameResolutionHelper"></param>
    /// <param name="emailTemplateHelper"></param>
    /// <param name="approvalHistoryProvider"></param>
    /// <param name="detailsHelper"></param>
    /// <param name="imageRetriever"></param>
    /// <param name="notificationProvider"></param>
    /// <param name="blobStorageHelper"></param>
    public EmailHelper(
        IConfiguration config,
        IApprovalSummaryProvider approvalSummaryProvider,
        ILogProvider logProvider,
        INameResolutionHelper nameResolutionHelper,
        IEmailTemplateHelper emailTemplateHelper,
        IApprovalHistoryProvider approvalHistoryProvider,
        IDetailsHelper detailsHelper,
        IImageRetriever imageRetriever,
        INotificationProvider notificationProvider,
        IBlobStorageHelper blobStorageHelper
        )
    {
        _config = config;
        _approvalSummaryProvider = approvalSummaryProvider;
        _logProvider = logProvider;
        _nameResolutionHelper = nameResolutionHelper;
        _emailTemplateHelper = emailTemplateHelper;
        _approvalHistoryProvider = approvalHistoryProvider;
        _detailsHelper = detailsHelper;
        _imageRetriever = imageRetriever;
        _notificationProvider = notificationProvider;
        _blobStorageHelper = blobStorageHelper;
    }

    /// <summary>
    /// Send Approval Notification Email
    /// </summary>
    /// <param name="approvalNotificationDetails">Approval Notification Detail</param>
    /// <param name="tenant">Tenant adapter object</param>
    /// <param name="emailType">Type of Email (With or without Detail)</param>
    /// <returns>returns a Approval Request result</returns>
    public ApprovalRequestResult SendEmail(ApprovalNotificationDetails approvalNotificationDetails,
        ITenant tenant,
        EmailType emailType)
    {
        ApprovalRequestResult result = null;
        bool isActionableEmailSentFail = false;

        var logData = new Dictionary<LogDataKey, object>();

        var tenantInfo = approvalNotificationDetails.ApprovalTenantInfo;
        var deviceNotificationInfo = approvalNotificationDetails.DeviceNotificationInfo;
        var summaryRows = approvalNotificationDetails.SummaryRows;
        var additionalData = approvalNotificationDetails.AdditionalData;

        logData.Add(LogDataKey.BusinessProcessName, string.Format(tenantInfo.BusinessProcessName, Constants.BusinessProcessNameSendNotificationToUser, Constants.BusinessProcessNameSendNotificationEmail));
        logData.Add(LogDataKey.Xcv, summaryRows.FirstOrDefault().Xcv);
        logData.Add(LogDataKey.Tcv, summaryRows.FirstOrDefault().Tcv);
        logData.Add(LogDataKey.ReceivedTcv, summaryRows.FirstOrDefault().Tcv);
        logData.Add(LogDataKey.DXcv, deviceNotificationInfo.ApprovalIdentifier.DisplayDocumentNumber);
        logData.Add(LogDataKey.DisplayDocumentNumber, deviceNotificationInfo?.ApprovalIdentifier?.DisplayDocumentNumber);
        logData.Add(LogDataKey.DocumentNumber, deviceNotificationInfo?.ApprovalIdentifier?.DocumentNumber);
        logData.Add(LogDataKey.TenantName, deviceNotificationInfo?.Application);
        logData.Add(LogDataKey.Approver, deviceNotificationInfo?.Approver);
        logData.Add(LogDataKey.NotificationTemplateKey, deviceNotificationInfo?.NotificationTemplateKey);

        try
        {
            JObject responseJObject = null;
            List<NotificationDataAttachment> notificationDataAttachment = new List<NotificationDataAttachment>();

            // this check is for emails with detail templates.

            #region Fetch all require details for actionable email

            if (IsNotifyEmailWithApprovalFunctionality(tenantInfo) && EmailType.ActionableEmail == emailType)
            {
                responseJObject = GetDetailsDataforApprovalRequest(tenantInfo, deviceNotificationInfo.ApprovalIdentifier, summaryRows.FirstOrDefault(), logData);

                if (responseJObject == null || (responseJObject.Property("Message") != null && !String.IsNullOrWhiteSpace(responseJObject.Property("Message").Value.ToString())))
                {
                    // Send generic email with tile details
                    deviceNotificationInfo.NotificationTemplateKey = GetOriginalNotificationTemplateKey(deviceNotificationInfo.NotificationTemplateKey);
                    _logProvider.LogError(TrackingEvent.SendEmailNotificationWithApprovalFunctionalityFail, new Exception("No Actionable Email - Send email with tile details because all details data is not available yet."), logData);
                }
                else if (!IsNotificationEmailAllowed(responseJObject))
                {
                    try
                    {
                        // Fetch missing details from LOB system and store it in azuretable
                        _logProvider.LogInformation(TrackingEvent.ActionableEmailAPIDetailsFetch, logData);
                        FetchMissingDetailsDataFromLOB(responseJObject, tenantInfo, deviceNotificationInfo.ApprovalIdentifier, summaryRows.FirstOrDefault(), logData);
                    }
                    catch (Exception ex)
                    {
                        // Send generic email with tile details
                        deviceNotificationInfo.NotificationTemplateKey = GetOriginalNotificationTemplateKey(deviceNotificationInfo.NotificationTemplateKey);
                        _logProvider.LogError(TrackingEvent.SendEmailNotificationWithApprovalFunctionalityFail, new InvalidDataException("No Actionable Email - Send email with tile details because CallBackURLCollection is null and there is a need to call back further, which fails actionable email model." + ex.Message.ToString() + ex.StackTrace.ToString() + ex.InnerException?.ToString()), logData);
                    }
                }

                if (responseJObject != null && responseJObject["Attachments"] != null && responseJObject["Attachments"].Any())
                {
                    // fetch attachment from blob storage/LOB
                    _logProvider.LogInformation(TrackingEvent.ActionableEmailGetAttachments, logData);
                    // Flag to determine if all downloadable attachments are downloaded successfully using ID
                    bool isAttachmentDownloadSuccess = false;
                    notificationDataAttachment = GetAttachmentsToAttachInEmail(responseJObject, deviceNotificationInfo.ApprovalIdentifier, tenantInfo, tenant, logData, ref isAttachmentDownloadSuccess);

                    if (!isAttachmentDownloadSuccess)
                    {
                        // Sent email without actions button/disabled with detail
                        deviceNotificationInfo.NotificationTemplateKey = GetOriginalNotificationTemplateKey(deviceNotificationInfo.NotificationTemplateKey) + Constants.EmailNotificationWithDetailsTemplateKey;
                        _logProvider.LogError(TrackingEvent.SendEmailNotificationFailedToLoadAttachments, new Exception("Actionable Email without Action Buttons - Unable to retrieve attachment(s) from BLOB."), logData);
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
                            approvalNotificationDetails.DeviceNotificationInfo.NotificationTemplateKey = GetOriginalNotificationTemplateKey(approvalNotificationDetails.DeviceNotificationInfo.NotificationTemplateKey);
                            // Call SendEmail function with  EmailType.EmailWithoutDetail to send normal email
                            emailType = EmailType.NormalEmail;
                            notificationDataAttachment.RemoveRange(0, notificationDataAttachment.Count);
                        }
                    }
                }
            }

            #endregion Fetch all require details for actionable email

            if (tenantInfo.NotifyEmail && !deviceNotificationInfo.ForFirstTimeUser)
            {
                int maxRetries = int.Parse(_config?[ConfigurationKey.NotificationFrameworkMaxRetries.ToString()] ?? "1");
                int repeatCounter = 0;
                var task = (Task)null;
                do
                {
                    try
                    {
                        task = ConstructAnEmailAndSendAsync(deviceNotificationInfo, tenantInfo, summaryRows, emailType, tenant, responseJObject, additionalData, notificationDataAttachment);
                        task.Wait();
                    }
                    catch (Exception ex)
                    {
                        repeatCounter++;
                        _logProvider.LogError(TrackingEvent.SendEmailNotificationFail, ex, logData);
                    }
                } while (task.IsFaulted && (repeatCounter < maxRetries));

                // Return an error response if none of the send email calls succeeded.
                if ((task.IsCompleted) && (!task.IsCanceled) && (!task.IsFaulted))
                {
                    result = ApprovalRequestResult.GetApprovalRequestResult(deviceNotificationInfo.ApprovalIdentifier, ApprovalRequestResultType.Success);
                }
                else
                {
                    result = new ApprovalRequestResult()
                    {
                        ApprovalIdentifier = deviceNotificationInfo.ApprovalIdentifier,
                        Result = ApprovalRequestResultType.Error,
                        TimeStamp = DateTime.UtcNow,
                        Exception = task.Exception,
                    };
                }

                _logProvider.LogInformation(TrackingEvent.EmailSent, logData);
                return result;
            }
        }
        catch (Exception ex)
        {
            // Send Normal Email if Actionable email failed.
            // Set isActionableEmailSentFail to true
            if (emailType == EmailType.ActionableEmail)
            {
                _logProvider.LogError(TrackingEvent.ActionableEmailNotificationFail, ex, logData);
                isActionableEmailSentFail = true;
            }
            else
            {
                _logProvider.LogError(TrackingEvent.NormalEmailNotificationFail, ex, logData);
            }
        }

        #region Send Normal Email based on isActionableEmailSentFail flag

        if (isActionableEmailSentFail)
        {
            try
            {
                // Set original notificationkey for normal email. e.g. PendingApproval
                approvalNotificationDetails.DeviceNotificationInfo.NotificationTemplateKey = GetOriginalNotificationTemplateKey(approvalNotificationDetails.DeviceNotificationInfo.NotificationTemplateKey);
                // Call SendEmail function with  EmailType.EmailWithoutDetail to send normal email
                SendEmail(approvalNotificationDetails,
                  tenant,
                  EmailType.NormalEmail);
            }
            catch (Exception ex)
            {
                _logProvider.LogError(TrackingEvent.NormalEmailNotificationFail, ex, logData);
            }
        }

        #endregion Send Normal Email based on isActionableEmailSentFail flag

        return result;
    }

    /// <summary>
    /// sending email
    /// </summary>
    /// <param name="userDelegationNotification">The user delegation notification.</param>
    /// <returns>returns a boolean value</returns>
    public Boolean SendEmail(UserDelegationDeviceNotification userDelegationNotification)
    {
        var logData = new Dictionary<LogDataKey, object>
        {
            { LogDataKey.EventType, Constants.FeatureUsageEvent },
            { LogDataKey.Xcv, userDelegationNotification.Xcv },
            { LogDataKey.Tcv, userDelegationNotification.Tcv },
            { LogDataKey.ReceivedTcv, userDelegationNotification.Tcv },
            { LogDataKey.NotificationTemplateKey, userDelegationNotification?.NotificationTemplateKey }
        };
        Boolean result;

        try
        {
            result = SendEmailRetryOnFail(userDelegationNotification);
            _logProvider.LogInformation(TrackingEvent.EmailSent, logData);

            return true;
        }
        catch (Exception ex)
        {
            _logProvider.LogError(TrackingEvent.SendEmailNotificationFail_UserDelegation, ex, logData);
            return false;
        }
    }

    /// <summary>
    /// Gets details data for approval request.
    /// </summary>
    /// <param name="tenantInfo">Approval tenant info.</param>
    /// <param name="approvalIdentifier">Approval Identifier</param>
    /// <param name="summaryRow">Summary row.</param>
    /// <param name="logData"> Log data information.</param>
    /// <param name="operationName">Operation Name.</param>
    /// <param name="callType">Call type.</param>
    /// <returns>returns a htmlContent</returns>
    public JObject GetDetailsDataforApprovalRequest(ApprovalTenantInfo tenantInfo, ApprovalIdentifier approvalIdentifier, ApprovalSummaryRow summaryRow, Dictionary<LogDataKey, object> logData, string operationName = "authsum", int callType = 2)
    {
        JObject responseJObject = null;
        Guid tcv = Guid.NewGuid();

        // To make and api call to get all the approval details data.
        var detailsInfo = _detailsHelper.GetDetails(tenantInfo.TenantId,
            approvalIdentifier.GetDocNumber(tenantInfo),
            operationName,
            approvalIdentifier.FiscalYear ?? string.Empty,
            1,
            tcv.ToString(), // Pass tcv as session id
            tcv.ToString(),
            summaryRow.Xcv,
            summaryRow.Approver,
            summaryRow.Approver,
            Constants.WorkerRole,
            string.Empty,
            true,
            callType,
            Constants.DetailPageType,
            Constants.SourcePendingApproval);

        Task task = detailsInfo;
        task.Wait();

        // Return an error response if none of the send email calls succeeded.
        if ((task.IsCompleted) && (!task.IsCanceled) && (!task.IsFaulted))
        {
            responseJObject = detailsInfo.Result;
        }

        return responseJObject;
    }

    /// <summary>
    /// Determines whether notification emails will be allowed or not.
    /// 1) if the response object is not null.
    /// 2) It checks if the no of line items are not more are more that allowed actionable email count.
    /// 3) If there are no callback URL collection property. no email will be sent to the user
    /// </summary>
    /// <param name="responseJObject">The response object.</param>
    /// <returns>
    ///   return true if the notification emails are allowed otherwise it returns false.
    /// </returns>
    public bool IsNotificationEmailAllowed(JObject responseJObject)
    {
        string urlPropertyName = "CallBackURLCollection";

        ///If JObject is null, no email will be sent.
        if (responseJObject == null)
        {
            return false;
        }

        //// If there are no callback URL collection property. no email will be sent to the user
        return responseJObject.Property(urlPropertyName)?.Value?.Count() == 0;
    }

    /// <summary>
    /// Fetch missing details data from LOB system.
    /// </summary>
    /// <param name="responseJObject">The response JObject.</param>
    /// <param name="tenantInfo">The Tenant information.</param>
    /// <param name="approvalIdentifier">The approval identifier.</param>
    /// <param name="summaryRow">The summary row.</param>
    /// <param name="logData">The log data information.</param>
    public void FetchMissingDetailsDataFromLOB(JObject responseJObject, ApprovalTenantInfo tenantInfo, ApprovalIdentifier approvalIdentifier, ApprovalSummaryRow summaryRow, Dictionary<LogDataKey, object> logData)
    {
        JObject missingDataResponseJObject = null;
        var urls = new List<JToken>(responseJObject.Property("CallBackURLCollection").Value.ToList());

        if (urls.Any())
        {
            Parallel.ForEach(urls, (url) =>
            {
                // operationName is like HDR,DTL,LINE
                string operationName = url.Value<string>().Split('/').Last();

                // Fetch missing details from LOB system and store it in azuretable
                missingDataResponseJObject = GetDetailsDataforApprovalRequest(tenantInfo, approvalIdentifier, summaryRow, logData, operationName);

                if (missingDataResponseJObject != null && missingDataResponseJObject.Property("Message") != null && !string.IsNullOrWhiteSpace(missingDataResponseJObject.Property("Message").Value.ToString()))
                {
                    throw new Exception(missingDataResponseJObject.Property("Message").Value.ToString());
                }
                if (missingDataResponseJObject != null && missingDataResponseJObject.Count > 0)
                {
                    // Merge missing details call into mail responseJObject to build complete adaptive card with all required details
                    responseJObject.Merge(missingDataResponseJObject);
                }
            });
        }
    }

    /// <summary>
    /// Gets the attachements.
    /// </summary>
    /// <param name="responseJObject">The response j object.</param>
    /// <param name="approvalIdentifier">Approval Idnetifier</param>
    /// <param name="tenantInfo">The tenant information.</param>
    /// <param name="tenant">Tenant object.</param>
    /// <param name="logData">Log Data information</param>
    /// <param name="isAttachmentDownloadSuccess">Flag to identify if all downloadable attachments are downloaded successfully</param>
    /// <returns>returns a list of NotificationDataAttachment</returns>
    public List<NotificationDataAttachment> GetAttachmentsToAttachInEmail(
        JObject responseJObject,
        ApprovalIdentifier approvalIdentifier,
        ApprovalTenantInfo tenantInfo,
        ITenant tenant,
        Dictionary<LogDataKey, object> logData,
        ref bool isAttachmentDownloadSuccess)
    {
        var attachmentList = new List<NotificationDataAttachment>();

        if (responseJObject == null || responseJObject["Attachments"] == null || responseJObject["Attachments"].Count() == 0)
            return attachmentList;

        try
        {
            List<Attachment> attachments = null;

            if (responseJObject["Attachments"] != null)
            {
                attachments = (responseJObject["Attachments"]?.ToString()).FromJson<List<Attachment>>().ToList();
            }

            List<Attachment> downloadableAttachments = attachments.Where(a => string.IsNullOrEmpty(a.Url) && !string.IsNullOrEmpty(a.ID)).ToList();
            int downloadableAttachmentCount = downloadableAttachments.Count();
            int successCount = 0;

            string blobNameFormat = "{0}|{1}|{2}";
            string storageAccountName = _config[ConfigurationKey.StorageAccountName.ToString()];

            foreach (var attachment in downloadableAttachments)
            {
                string blobName = string.Format(blobNameFormat, tenantInfo.TenantId, approvalIdentifier.DisplayDocumentNumber, attachment.ID.ToString());
                var blobExists = _blobStorageHelper.DoesExist(Constants.NotificationAttachmentsBlobName, blobName).Result;
                if (blobExists)
                {
                    byte[] contentArray = _blobStorageHelper.DownloadByteArray(Constants.NotificationAttachmentsBlobName, blobName).Result;
                    var blobUrl = string.Format(@"https://{0}.blob.core.windows.net/{1}/{2}", storageAccountName, Constants.NotificationAttachmentsBlobName, blobName);
                    attachmentList.Add(new NotificationDataAttachment() { FileBase64 = Convert.ToBase64String(contentArray), FileName = attachment.Name.ToString(), IsInline = false, FileSize = contentArray.Length, FileUrl = blobUrl });
                    successCount++;
                }
                else
                {
                    ApprovalsTelemetry telemetry = new ApprovalsTelemetry()
                    {
                        BusinessProcessName = Constants.BusinessProcessNameGetAttachmentContentFromLob,
                        Xcv = logData != null && logData.ContainsKey(LogDataKey.Xcv) ? logData[LogDataKey.Xcv].ToString() : approvalIdentifier.DocumentNumber,
                        Tcv = logData != null && logData.ContainsKey(LogDataKey.Tcv) ? logData[LogDataKey.Tcv].ToString() : Guid.NewGuid().ToString()
                    };

                    // fetch attachment from Lob
                    var lobResponse = tenant.GetAttachmentContentFromLob(approvalIdentifier, attachment.ID.ToString(), telemetry).Result;
                    if (lobResponse != null)
                    {
                        byte[] contentArray = lobResponse;
                        var blobUrl = string.Format(@"https://{0}.blob.core.windows.net/{1}/{2}", storageAccountName, Constants.NotificationAttachmentsBlobName, blobName);
                        attachmentList.Add(new NotificationDataAttachment() { FileBase64 = Convert.ToBase64String(contentArray), FileName = attachment.Name.ToString(), IsInline = false, FileSize = contentArray.Length, FileUrl = blobUrl });
                        successCount++;
                    }
                }
            }
            if (successCount == downloadableAttachmentCount)
            {
                isAttachmentDownloadSuccess = true;
            }
        }
        catch (Exception ex)
        {
            _logProvider.LogError<TrackingEvent, LogDataKey>(TrackingEvent.SendEmailNotificationFailedToLoadAttachments, ex);
            isAttachmentDownloadSuccess = false;
        }

        return attachmentList;
    }

    /// <summary>
    /// Creates the current approver chain.
    /// </summary>
    /// <param name="documentSummary">The document summary.</param>
    /// <param name="historyDataExts">The history data exts.</param>
    /// <param name="alias">The alias.</param>
    /// <param name="tenantInfo">Tenant detail</param>
    /// <returns>returns a string</returns>
    /// <exception cref="Exception">User doesn't have permission to see the report.</exception>
    public async Task<string> CreateCurrentApproverChain(JObject documentSummary, List<TransactionHistoryExt> historyDataExts, string alias, ApprovalTenantInfo tenantInfo)
    {
        var historyData = new List<TransactionHistory>();
        if (documentSummary != null)
        {
            historyDataExts.Add(new TransactionHistoryExt()
            {
                Approver = alias,//documentSummary.Approver,
                JsonData = documentSummary.ToString(),
                ActionTaken = string.Empty
            });

            /*TODO:: IsOldHierarchyEnbled flag is introduced to control Request Activities display with default value as false. When multiple approvers occur at one level, all the current approvers at same level are shown when the flag is false.
                     If any tenant has issue with RequestActivities, Add this flag in TenantInfo table and mark it as 'true' so that Old Hierarchy code will be executed for that tenant.
                     Whenever all the tenants' display appropriate Request Activities, remove this flag.*/
            if (tenantInfo.IsOldHierarchyEnabled == false)
            {
                #region TODO:: Remove OldHierarchyEnabled flag specific Code and enable the code for all tenants
                // Add Future Approver Chain into TransactionHistoryExts object
                if (!string.IsNullOrEmpty(documentSummary.ToString()))
                {
                    SummaryJson summary = documentSummary?.ToString().FromJson<SummaryJson>();
                    if (summary.ApprovalHierarchy != null && summary.ApprovalHierarchy.Any())
                    {
                        int approverCount = 0;
                        foreach (var approver in summary.ApprovalHierarchy)
                        {
                            var isFutureApprover = true;
                            // Skipping the current Approver from the Previous Approver chain
                            if (summary.ApprovalHierarchy.Count >= historyDataExts.Count && historyDataExts.Count > ++approverCount)
                            {
                                continue;
                            }
                            if (approver.Approvers != null && approver.Approvers.FirstOrDefault(x => x.Alias == alias) != null)
                            {
                                // Setting up ApproverType for current approver
                                foreach (var transactionHistoryExt in historyDataExts.Where(transactionHistoryExt => transactionHistoryExt.Approver ==
                                                                                                                     alias))
                                {
                                    if (tenantInfo.IsSameApproverMultipleLevelSupported)
                                    {
                                        if (string.IsNullOrWhiteSpace(transactionHistoryExt.ApproverType)
                                            && string.IsNullOrWhiteSpace(transactionHistoryExt.ActionTaken))
                                        {
                                            transactionHistoryExt.ApproverType = approver.ApproverType;
                                            isFutureApprover = false;
                                        }
                                        else if (transactionHistoryExt.ApproverType == approver.ApproverType)
                                        {
                                            isFutureApprover = false;
                                        }
                                    }
                                    else
                                    {
                                        transactionHistoryExt.ApproverType = approver.ApproverType;
                                        isFutureApprover = false;
                                    }
                                }

                                // Parallel approvers at same level as current approvers gets added to historyDataExts to display under Request Activities
                                if (tenantInfo.IsAllCurrentApproversDisplayInHierarchy && !isFutureApprover && approver.Approvers != null)
                                {
                                    foreach (var currentApprover in approver.Approvers.Where(a => a.Alias != alias))
                                    {
                                        historyDataExts.Add(new TransactionHistoryExt()
                                        {
                                            Approver = currentApprover.Alias,
                                            JsonData = documentSummary.ToString(),
                                            ActionTaken = string.Empty,
                                            ApproverType = approver.ApproverType,
                                            _future = false,
                                            ApproverName = string.IsNullOrEmpty(currentApprover.Name) ? await _nameResolutionHelper.GetUserName(currentApprover.Alias) : currentApprover.Name
                                        });
                                    }
                                }
                            }
                            AddHistoryDataExtsRange(isFutureApprover, approver, true, historyDataExts, documentSummary);
                        }
                    }
                }
                int historyCount = 0;
                var approverChains = (from history in historyDataExts
                                      let histCount = historyCount++
                                      let historyJson = string.IsNullOrEmpty(history.JsonData)
                                         ? ("{}").ToJObject()
                                         : (history.JsonData).ToJObject()
                                      let historyNotes = string.IsNullOrEmpty(history.ApproversNote)
                                         ? ("{}").ToJObject()
                                         : (history.ApproversNote).ToJObject()
                                      let isBasicActions = history.ActionTaken.Equals("System Cancel", StringComparison.InvariantCultureIgnoreCase)
                                             || history.ActionTaken.Equals("System Send Back", StringComparison.InvariantCultureIgnoreCase)
                                             || history.ActionTaken.Equals("Cancel", StringComparison.InvariantCultureIgnoreCase)
                                             || history.ActionTaken.Equals("Resubmitted", StringComparison.InvariantCultureIgnoreCase)
                                      let isBasicActionsWithTakeback = isBasicActions || history.ActionTaken.Equals("Takeback", StringComparison.InvariantCultureIgnoreCase)

                                      let approverAlias = history.Approver
                                      //let approverName = _nameResolutionHelper.GetUserName(history.Approver)
                                      let approverName = string.IsNullOrEmpty(history.ApproverName) ? _nameResolutionHelper.GetUserName(history.Approver).Result : history.ApproverName
                                      // Use ApproverType directly if exists
                                      let approverType = string.IsNullOrEmpty(history.ApproverType) ?
                                                         GetApprovalType(historyJson, approverAlias, histCount)
                                                         : history.ApproverType
                                      select new
                                      {
                                          Alias = (isBasicActionsWithTakeback)
                                                 ? string.Empty
                                                 : history.Approver,
                                          Name = (isBasicActionsWithTakeback)
                                                 ? string.Empty
                                                 : (history.Approver.Equals(approverAlias, StringComparison.InvariantCultureIgnoreCase)
                                                     ? approverName
                                                     : _nameResolutionHelper.GetUserName(history.Approver).Result),
                                          Action = history.ActionTaken,
                                          history.ActionDate,
                                          Type = (isBasicActionsWithTakeback)
                                                 ? null
                                                 : approverType,
                                          Justification = (isBasicActions)
                                                 ? null
                                                 : MSAHelper.ExtractValueFromJSON(historyNotes, "JustificationText"),
                                          Notes = (isBasicActions)
                                                 ? null
                                                 : MSAHelper.ExtractValueFromJSON(historyNotes, "Comment"),
                                          history._future,
                                          history._isPreApprover,
                                          history.DelegateUser
                                      }).ToList<dynamic>();
                CheckUnauthorizedAccess(approverChains, alias);
                string approverChainString = approverChains.ToJson(new JsonSerializerSettings { NullValueHandling = NullValueHandling.Ignore });
                return approverChainString;
                # endregion TODO:: Remove OldHierarchyEnabled flag specific Code and enable the code for all tenants
            }
            else
            {
                #region Hierarchy for remaining all tenants

                // Add Future Approver Chain into TransactionHistoryExts object
                if (!string.IsNullOrEmpty(documentSummary.ToString()))

                {
                    SummaryJson summary = documentSummary?.ToString().FromJson<SummaryJson>();
                    if (summary.ApprovalHierarchy != null && summary.ApprovalHierarchy.Any())
                    {
                        var isFutureApprover = false;
                        foreach (var approver in summary.ApprovalHierarchy)
                        {
                            if (approver.Approvers != null && approver.Approvers.FirstOrDefault(x => x.Alias == alias) != null)
                            {
                                //Add approver type into current Approver
                                foreach (var transactionHistoryExt in historyDataExts.Where(transactionHistoryExt => transactionHistoryExt.Approver ==
                                                                                                                     alias))
                                {
                                    if (tenantInfo.IsSameApproverMultipleLevelSupported)
                                    {
                                        if (string.IsNullOrWhiteSpace(transactionHistoryExt.ApproverType)
                                            && string.IsNullOrWhiteSpace(transactionHistoryExt.ActionTaken))
                                        {
                                            transactionHistoryExt.ApproverType = approver.ApproverType;
                                        }
                                    }
                                    else
                                    {
                                        transactionHistoryExt.ApproverType = approver.ApproverType;
                                    }
                                }
                                isFutureApprover = true;
                            }
                            AddHistoryDataExtsRange(isFutureApprover, approver, approver.Approvers.FirstOrDefault(x => x.Alias == alias) == null,
                                historyDataExts, documentSummary);
                        }
                    }
                }

                #endregion Hierarchy for remaining all tenants

                int historyCount = 0;
                var approverChains = (from history in historyDataExts
                                      let histCount = historyCount++
                                      let historyJson = string.IsNullOrEmpty(history.JsonData)
                                         ? ("{}").ToJObject()
                                         : (history.JsonData).ToJObject()
                                      let historyNotes = string.IsNullOrEmpty(history.ApproversNote)
                                         ? ("{}").ToJObject()
                                         : (history.ApproversNote).ToJObject()
                                      let isBasicActions = history.ActionTaken.Equals("System Cancel", StringComparison.InvariantCultureIgnoreCase)
                                            || history.ActionTaken.Equals("System Send Back", StringComparison.InvariantCultureIgnoreCase)
                                            || history.ActionTaken.Equals("Cancel", StringComparison.InvariantCultureIgnoreCase)
                                            || history.ActionTaken.Equals("Resubmitted", StringComparison.InvariantCultureIgnoreCase)
                                      let isBasicActionsWithTakeback = isBasicActions || history.ActionTaken.Equals("Takeback", StringComparison.InvariantCultureIgnoreCase)

                                      let approverAlias = history.Approver
                                      //let approverName = _nameResolutionHelper.GetUserName(history.Approver)
                                      let approverName = string.IsNullOrEmpty(history.ApproverName) ? _nameResolutionHelper.GetUserName(history.Approver).Result : history.ApproverName
                                      // Use ApproverType directly if exists
                                      let approverType = string.IsNullOrEmpty(history.ApproverType) ?
                                                         GetApprovalType(historyJson, approverAlias)
                                                         : history.ApproverType
                                      select new
                                      {
                                          Alias = (isBasicActionsWithTakeback)
                                                 ? string.Empty
                                                 : history.Approver,
                                          Name = (isBasicActionsWithTakeback)
                                                 ? string.Empty
                                                 : (history.Approver.Equals(approverAlias, StringComparison.InvariantCultureIgnoreCase)
                                                     ? approverName
                                                     : _nameResolutionHelper.GetUserName(history.Approver).Result),
                                          Action = history.ActionTaken,
                                          history.ActionDate,
                                          Type = (isBasicActionsWithTakeback)
                                                 ? null
                                                 : approverType,
                                          Justification = (isBasicActions)
                                                 ? null
                                                 : MSAHelper.ExtractValueFromJSON(historyNotes, "JustificationText"),
                                          Notes = (isBasicActions)
                                                 ? null
                                                 : MSAHelper.ExtractValueFromJSON(historyNotes, "Comment"),
                                          history._future,
                                          history._isPreApprover,
                                          history.DelegateUser
                                      }).ToList<dynamic>();
                CheckUnauthorizedAccess(approverChains, alias);
                string approverChainString = approverChains.ToJson(new JsonSerializerSettings { NullValueHandling = NullValueHandling.Ignore });
                return approverChainString;
            }
        }
        return string.Empty;
    }

    /// <summary>
    /// Add Transaction history data
    /// </summary>
    /// <param name="isFutureApprover"></param>
    /// <param name="approver"></param>
    /// <param name="isAlreadyApproved"></param>
    /// <param name="historyDataExts"></param>
    /// <param name="documentSummary"></param>
    private void AddHistoryDataExtsRange(bool isFutureApprover, ApprovalHierarchy approver, bool isAlreadyApproved,
        List<TransactionHistoryExt> historyDataExts, JObject documentSummary)
    {
        if (isFutureApprover && approver.Approvers != null && isAlreadyApproved)
        {
            historyDataExts.AddRange(
             approver.Approvers.Select(approverAlias => new TransactionHistoryExt()
             {
                 Approver = approverAlias.Alias,
                 JsonData = documentSummary.ToString(),
                 ActionTaken = string.Empty,
                 ApproverType = approver.ApproverType,
                 _future = true,
                 ApproverName = string.IsNullOrEmpty(approverAlias.Name) ? _nameResolutionHelper.GetUserName(approverAlias.Alias).Result : approverAlias.Name
             }));
        }
    }

    /// <summary>
    /// Check if unauthorized access
    /// </summary>
    /// <param name="approverChains"></param>
    /// <param name="alias"></param>
    private void CheckUnauthorizedAccess(List<dynamic> approverChains, string alias)
    {
        if (approverChains.FirstOrDefault(h => h.Alias.Trim().Equals(alias, StringComparison.InvariantCultureIgnoreCase)) == null)
        {
            throw new UnauthorizedAccessException("User doesn't have permission to see the report.");
        }
    }

    /// <summary>
    /// Populate data into dictionary.
    /// </summary>
    /// <typeparam name="T1"></typeparam>
    /// <typeparam name="T2"></typeparam>
    /// <param name="dictionary"></param>
    /// <param name="key"></param>
    /// <param name="value"></param>
    public void PopulateDataInToDictionary<T1, T2>(Dictionary<T1, T2> dictionary, T1 key, T2 value)
    {
        if (dictionary != null)
        {
            if (!dictionary.ContainsKey(key))
            {
                dictionary.Add(key, value);
            }
            else
            {
                dictionary[key] = value;
            }
        }
    }

    /// <summary>
    /// Gets the blob template by file name.
    /// </summary>
    /// <param name="templateList">List of templates</param>
    /// <returns>returns a htmlContent</returns>
    private async Task GetAllIconsFromBlob(IDictionary<string, string> templateList)
    {
        try
        {
            var listBlobs = await _blobStorageHelper.ListBlobsHierarchicalListing(Constants.OutlookActionableEmailIcons, "", null);

            // Generate the shared access signature on the container, setting the constraints directly on the signature.
            var sasToken = _blobStorageHelper.GetContainerSasToken(Constants.OutlookActionableEmailIcons, DateTimeOffset.UtcNow.AddDays(7));
            foreach (var item in listBlobs)
            {
                var blobItemName = item.Name;
                var storageAccountName = _config[ConfigurationKey.StorageAccountName.ToString()];
                templateList[blobItemName] = $"https://{storageAccountName}.blob.core.windows.net/{Constants.OutlookActionableEmailIcons}/{blobItemName}?" + sasToken;
            }
        }
        catch (Exception ex)
        {
            _logProvider.LogError<TrackingEvent, LogDataKey>(TrackingEvent.OutlookBlobTemplateByFileName, ex);
        }
    }

    /// <summary>
    /// Processes the detail email template.
    /// </summary>
    /// <param name="responseJObject"></param>
    /// <param name="tenantInfo"></param>
    /// <param name="tenant"></param>
    /// <param name="displayDocumentNumber"></param>
    /// <param name="placeHolderDict"></param>
    /// <param name="summaryRows"></param>
    /// <returns>returns a complete email template</returns>
    public string ConstructDynamicHtmlDetailsForEmail(
        JObject responseJObject,
        ApprovalTenantInfo tenantInfo,
        ITenant tenant,
        string displayDocumentNumber,
        ref Dictionary<string, string> placeHolderDict,
        List<ApprovalSummaryRow> summaryRows,
        ref EmailType emailType)
    {
        var templateList = new Dictionary<string, string>();

        List<Task> allTasks = new List<Task>();
        Task getBlobIconTask = Task.Run(async ()
                => await GetAllIconsFromBlob(templateList));
        allTasks.Add(getBlobIconTask);

        Task getTemplateListTask = Task.Run(async ()
                => await GetBlobTemplateByFileName(tenantInfo, templateList));
        allTasks.Add(getTemplateListTask);

        Task.WaitAll(allTasks.ToArray());

        return tenant.ConstructDynamicHtmlDetailsForEmail(responseJObject, templateList, displayDocumentNumber, ref placeHolderDict, summaryRows, ref emailType);
    }

    /// <summary>
    /// Gets the blob template by file name.
    /// </summary>
    /// <param name="tenantInfo">The tenant info.</param>
    /// <param name="templateList">List of templates</param>
    /// <returns>returns a htmlContent</returns>
    private async Task GetBlobTemplateByFileName(ApprovalTenantInfo tenantInfo, IDictionary<string, string> templateList)
    {
        try
        {
            var listBlobs = await _blobStorageHelper.ListBlobsHierarchicalListing(
                Constants.OutlookDynamicTemplates,
                (string.IsNullOrWhiteSpace(tenantInfo.ActionableEmailFolderName) ? tenantInfo.AppName : tenantInfo.ActionableEmailFolderName) + "/",
                null);

            var listCommonBlobs = await _blobStorageHelper.ListBlobsHierarchicalListing(
                Constants.OutlookDynamicTemplates,
                Constants.CommonTemplates,
                null);

            foreach (var item in listBlobs)
            {
                var blobItem = (BlobItem)item;
                var blobItemName = blobItem.Name;
                var htmlTemplate = await _blobStorageHelper.DownloadText(Constants.OutlookDynamicTemplates, blobItemName);
                var blobNameParts = blobItemName.Split('/');
                blobItemName = blobNameParts.Length > 1 ? blobNameParts[1].ToString() : blobNameParts[0].ToString();
                templateList[blobItemName] = htmlTemplate;
            }
            foreach (var commonItem in listCommonBlobs)
            {
                var blobItem = (BlobItem)commonItem;
                var blobItemName = blobItem.Name;
                var htmlTemplate = await _blobStorageHelper.DownloadText(Constants.OutlookDynamicTemplates, blobItemName);
                var blobNameParts = blobItemName.Split('/');
                blobItemName = blobNameParts.Length > 1 ? blobNameParts[1].ToString() : blobNameParts[0].ToString();
                if (!templateList.ContainsKey(blobItemName))
                {
                    templateList[blobItemName] = htmlTemplate;
                }
            }
        }
        catch (Exception ex)
        {
            _logProvider.LogError<TrackingEvent, LogDataKey>(TrackingEvent.OutlookBlobTemplateByFileName, ex);
        }
    }

    /// <summary>
    /// Get User Image
    /// </summary>
    /// <param name="alias">User alias.</param>
    /// <param name="SessionId">The User session id.</param>
    /// <param name="clientDevice">The Client device.</param>
    /// <param name="logData">The log data information.</param>
    /// <returns>The base64 string of user image.</returns>
    public async Task<string> GetUserImage(string alias, string SessionId, string clientDevice, Dictionary<LogDataKey, object> logData)
    {
        string base64String = Constants.DefaultUserImageBase64String;
        byte[] photo = null;

        try
        {
            if (!string.IsNullOrEmpty(alias))
            {
                // Bug#4010458 - Submitter Alias is coming as null.
                // Here added telemetry in catch block with more details along with documentnumber.
                // We will deploy this code into production and keep watching this exception.
                // So we will get more AI information with documentNumber.
                // So we can check details data in azure table for that documentNumber to identify rootcause. Why submitter alias coming null for that documentNumber.
                photo = await _imageRetriever.GetUserImageAsync(alias, SessionId, clientDevice);
                if (photo != null)
                {
                    // Convert the Byte Array to Base64 Encoded string.
                    base64String = Convert.ToBase64String(photo, 0, photo.Length);
                }
            }
            else
            {
                _logProvider.LogError(TrackingEvent.WebApiUserImageFail, new InvalidDataException("Alias can not be an empty"), logData);
            }
        }
        catch (Exception ex)
        {
            _logProvider.LogError(TrackingEvent.WebApiUserImageFail, ex, logData);
        }
        finally
        {
            if (photo == null)
            {
                base64String = Constants.DefaultUserImageBase64String;
            }
        }

        return string.Format("{0},{1}", "data:image/jpeg;base64", base64String);
    }

    /// <summary>
    /// to check for the Notify Email With approval functionality is set to true.
    /// </summary>
    /// <param name="tenantInfo">The tenant information.</param>
    /// <returns>
    ///  returns a boolean value.
    /// </returns>
    private bool IsNotifyEmailWithApprovalFunctionality(ApprovalTenantInfo tenantInfo)
    {
        if (tenantInfo == null)
            return false;

        return (tenantInfo.NotifyEmail && tenantInfo.NotifyEmailWithApprovalFunctionality);
    }

    /// <summary>
    /// Get Original NotificationTemplateKey
    /// </summary>
    /// <param name="notificationTemplateKey"></param>
    /// <returns></returns>
    private string GetOriginalNotificationTemplateKey(string notificationTemplateKey)
    {
        string[] stringSeparators = new string[] { Constants.EmailNotificationWithActionTemplateKey };
        string[] originalTemplateKey = notificationTemplateKey.Split(stringSeparators, StringSplitOptions.RemoveEmptyEntries);
        if (originalTemplateKey != null && originalTemplateKey.Count() > 0)
        {
            return originalTemplateKey[0]?.ToString();
        }
        else
        {
            return string.Empty;
        }
    }

    /// <summary>
    /// Used to send emails
    /// </summary>
    /// <param name="deviceNotificationInfo">Device Notification Info</param>
    /// <param name="tenantInfo">Tenant Info</param>
    /// <param name="summaryRows">List of summary rows</param>
    /// <param name="emailType">Type of Email (With or without Detail)</param>
    /// <param name="tenant">Tenant adapter object</param>
    /// <param name="responseJObject">response object</param>
    /// <param name="additionalData">Additional Data</param>
    /// <param name="notificationDataAttachment">Notification Data Attachment</param>
    /// <returns>returns a task result</returns>
    private async Task ConstructAnEmailAndSendAsync(DeviceNotificationInfo deviceNotificationInfo,
        ApprovalTenantInfo tenantInfo,
        List<ApprovalSummaryRow> summaryRows,
        EmailType emailType,
        ITenant tenant,
        JObject responseJObject,
        Dictionary<string, string> additionalData,
        List<NotificationDataAttachment> notificationDataAttachment)
    {
        var notificationMessageId = Guid.NewGuid().ToString();
        var logData = new Dictionary<LogDataKey, object>
        {
            { LogDataKey.BusinessProcessName, string.Format(tenantInfo.BusinessProcessName, Constants.BusinessProcessNameSendNotificationToUser, Constants.BusinessProcessNameSendNotificationEmail) },
            { LogDataKey.Xcv, summaryRows.FirstOrDefault().Xcv },
            { LogDataKey.Tcv, summaryRows.FirstOrDefault().Tcv },
            { LogDataKey.ReceivedTcv, summaryRows.FirstOrDefault().Tcv },
            { LogDataKey.DXcv, deviceNotificationInfo.ApprovalIdentifier.DisplayDocumentNumber },
            { LogDataKey.DisplayDocumentNumber, deviceNotificationInfo?.ApprovalIdentifier?.DisplayDocumentNumber },
            { LogDataKey.DocumentNumber, deviceNotificationInfo?.ApprovalIdentifier?.DocumentNumber },
            { LogDataKey.TenantName, deviceNotificationInfo?.Application },
            { LogDataKey.Approver, deviceNotificationInfo?.Approver },
            { LogDataKey.NotificationTemplateKey, deviceNotificationInfo?.NotificationTemplateKey },
            { LogDataKey.NotificationMessageId, notificationMessageId }
        };

        try
        {
            if (deviceNotificationInfo.SendNotification)
            {
                if (String.IsNullOrEmpty(deviceNotificationInfo.To))
                {
                    throw new InvalidDataException("Email To value is invalid. Email To value must be comma separated.");
                }
                else if (deviceNotificationInfo.SendNotification)
                {
                    string BCCEmailAddress = deviceNotificationInfo.BCC;
                    string pafBCCValue = _config[ConfigurationKey.NotificationBCCEmailAddress.ToString()];
                    if (!string.IsNullOrEmpty(pafBCCValue))
                    {
                        if (string.IsNullOrEmpty(BCCEmailAddress))
                        {
                            BCCEmailAddress = pafBCCValue;
                        }
                        else
                        {
                            BCCEmailAddress += ";" + pafBCCValue;
                        }
                    }

                    // Explicit Check for Failure Email Notification, if yes get the template for partitionKey = 0
                    int tenantId = tenantInfo.TenantId;
                    if (deviceNotificationInfo.NotificationTemplateKey.Equals(Constants.FailureEmailNotificationTemplateKey, StringComparison.InvariantCultureIgnoreCase))
                    {
                        tenantId = 0;
                    }

                    string templatePattern = ApprovalEmailNotificationTemplates.EmailTemplatePartitionKey(deviceNotificationInfo.NotificationTemplateKey, deviceNotificationInfo.ActionTaken);
                    ApprovalEmailNotificationTemplates template = _emailTemplateHelper.GetTemplates().Where(emailtemplate => (emailtemplate.PartitionKey.Equals(tenantId.ToString(), StringComparison.InvariantCultureIgnoreCase)) && Regex.IsMatch(emailtemplate.RowKey, templatePattern)).FirstOrDefault();

                    var templateData = await GetMailContent(deviceNotificationInfo, tenantInfo, summaryRows, summaryRows.FirstOrDefault().Xcv, summaryRows.FirstOrDefault().Tcv, tenant, responseJObject, additionalData, emailType, logData);
                    if (templateData.ContainsKey("EmailType") && templateData["EmailType"].Equals(EmailType.NormalEmail.ToString()))
                    {
                        var templateKey = GetOriginalNotificationTemplateKey(deviceNotificationInfo.NotificationTemplateKey);
                        templatePattern = ApprovalEmailNotificationTemplates.EmailTemplatePartitionKey(templateKey, deviceNotificationInfo.ActionTaken);
                        template = _emailTemplateHelper.GetTemplates().Where(emailtemplate => (emailtemplate.PartitionKey.Equals(tenantId.ToString(), StringComparison.InvariantCultureIgnoreCase)) && Regex.IsMatch(emailtemplate.RowKey, templatePattern)).FirstOrDefault();
                    }

                    var emailData = new NotificationItem
                    {
                        Bcc = BCCEmailAddress,
                        Body = template.TemplateContent,
                        Cc = FormatEmailIfNotValid(deviceNotificationInfo.CC),
                        Subject = GetSubjectTemplate(template.TemplateContent),
                        TemplateData = templateData.ToDictionary(pair => pair.Key, pair => (object)pair.Value),
                        TemplatePartitionKey = new List<string>() { tenantId.ToString() },
                        TemplateRowExp = ApprovalEmailNotificationTemplates.EmailTemplatePartitionKey(deviceNotificationInfo.NotificationTemplateKey, deviceNotificationInfo.ActionTaken),
                        To = FormatEmailIfNotValid(deviceNotificationInfo.To),
                        NotificationTypes = new List<NotificationType>(),
                        Telemetry = new Telemetry() { Xcv = summaryRows.FirstOrDefault().Xcv, MessageId = notificationMessageId },
                        // Set TemplateId null to avoid NF logic app to refetch the template and send email with placeholders.
                        TemplateId = template.TemplateId,
                        TenantIdentifier = tenantInfo.RowKey
                    };
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

                    await _notificationProvider.SendEmail(emailData);

                    if (emailType == EmailType.ActionableEmail)
                    {
                        _logProvider.LogInformation(TrackingEvent.ActionableEmailNotificationSent, logData);
                    }
                    else
                    {
                        _logProvider.LogInformation(TrackingEvent.NormalEmailNotificationSent, logData);
                    }
                }
            }
        }
        catch (Exception exception)
        {
            // Log the error and don't throw it. Otherwise it will not send any other notifications.
            if (emailType == EmailType.ActionableEmail)
            {
                _logProvider.LogError(TrackingEvent.ActionableEmailNotificationFail, exception, logData);
            }
            else
            {
                _logProvider.LogError(TrackingEvent.NormalEmailNotificationFail, exception, logData);
            }

            throw;
        }
    }

    /// <summary>
    /// Formats email id if it is not valid
    /// </summary>
    /// <param name="mailId">Mail ID</param>
    /// <returns>returns a string</returns>
    private string FormatEmailIfNotValid(string mailId)
    {
        StringBuilder finalEmailList = new StringBuilder();
        if (!String.IsNullOrEmpty(mailId))
        {
            String[] emailIds = mailId.Split(',');
            if (emailIds.Any())
            {
                foreach (string emailId in emailIds)
                {
                    finalEmailList.Append(emailId);
                    if (!Regex.IsMatch(emailId, @"\w+([-+.']\w+)*@\w+([-.]\w+)*\.\w+([-.]\w+)*"))
                        finalEmailList.Append("@Microsoft.com");
                    finalEmailList.Append(";");
                }
            }
        }

        if (false == string.IsNullOrEmpty(finalEmailList.ToString()))
        {
            return finalEmailList.ToString().TrimEnd(';');
        }
        else
        {
            return null;
        }
    }

    /// <summary>
    /// Get email subject template
    /// </summary>
    /// <param name="templateContent">Email Template content</param>
    /// <returns>Return Modify Templates</returns>
    private string GetSubjectTemplate(string templateContent)
    {
        string result = string.Empty;
        if (templateContent.Contains("<title>"))
        {
            int first = templateContent.IndexOf("<title>") + "<title>".Length;
            int last = templateContent.IndexOf("</title>");

            result = templateContent.Substring(first, last - first);
        }
        return result;
    }

    /// <summary>
    ///  Creates a dictionary which contains mail content and subject
    /// </summary>
    /// <param name="deviceNotificationInfo">Device Notification Info</param>
    /// <param name="tenantInfo">Approval Tenant Info</param>
    /// <param name="summaryRows">List of summary rows</param>
    /// <param name="Xcv">Xcv</param>
    /// <param name="Tcv">Tcv</param>
    /// <param name="tenant">Tenant adapter object</param>
    /// <param name="responseJObject">response object</param>
    /// <param name="additionalData">Additional Data</param>
    /// <param name="emailType">Email Type</param>
    /// <param name="logData">LogData</param>
    /// <returns>returns a task result</returns>
    private async Task<Dictionary<string, string>> GetMailContent(DeviceNotificationInfo deviceNotificationInfo,
        ApprovalTenantInfo tenantInfo,
        List<ApprovalSummaryRow> summaryRows,
        string Xcv,
        string Tcv,
        ITenant tenant,
        JObject responseJObject,
        Dictionary<string, string> additionalData,
        EmailType emailType,
        Dictionary<LogDataKey, object> logData)
    {
        var placeHolderDictionary = new Dictionary<string, string>();

        string templateData = String.Empty;
        ApprovalSummaryRow summaryData;
        //Check if there were no summary rows. Fetch them, if not.
        if (summaryRows.Count == 0)
            summaryData = _approvalSummaryProvider.GetApprovalSummaryByDocumentNumber(deviceNotificationInfo.DocumentTypeId.ToString(), deviceNotificationInfo.ApprovalIdentifier.DisplayDocumentNumber, deviceNotificationInfo.Approver);
        else
            summaryData = summaryRows.Where(x => deviceNotificationInfo.ApprovalIdentifier.DisplayDocumentNumber.Equals(x.DocumentNumber) && x.Approver != null && deviceNotificationInfo.Approver.Equals(x.Approver)).FirstOrDefault();
        List<TransactionHistoryExt> historyDataExt = await _approvalHistoryProvider.GetApproverChainHistoryDataAsync(tenantInfo, deviceNotificationInfo.ApprovalIdentifier.DisplayDocumentNumber, string.Empty, Environment.UserName, Xcv, Tcv, string.Empty);

        if (summaryData != null)
        {
            //Adding additional data to SummaryRow.SummaryJson to populate the email placeholders
            var summaryJson = summaryData.SummaryJson.FromJson<SummaryJson>();
            summaryJson.AdditionalData = (additionalData != null && additionalData.Count > 0) ? additionalData : null;
            summaryData.SummaryJson = summaryJson.ToJson();

            JObject summaryWithApprover = (summaryData.SummaryJson).ToJObject();
            await AddApprover(summaryData.Approver, summaryWithApprover);

            placeHolderDictionary = await MapSummaryData(summaryWithApprover.ToString(), tenantInfo, deviceNotificationInfo.ActionDetails, summaryData.Approver, historyDataExt, Xcv, tenant, responseJObject, summaryRows, emailType, deviceNotificationInfo, logData);
        }
        else
        {
            List<TransactionHistory> historyData = await _approvalHistoryProvider.GetHistoryDataAsync(tenantInfo, deviceNotificationInfo.ApprovalIdentifier.DisplayDocumentNumber, deviceNotificationInfo.Approver, Xcv, Tcv);
            if (deviceNotificationInfo.ActionTaken.IsActionExempt() && (historyData == null || historyData.Count == 0))
            {
                historyData = await _approvalHistoryProvider.GetHistoryDataAsync(tenantInfo, deviceNotificationInfo.ApprovalIdentifier.DisplayDocumentNumber, string.Empty, Xcv, Tcv);
            }
            if (historyData != null && historyData.Count > 0)
            {
                TransactionHistory historyRow = historyData.OrderByDescending(x => x.ActionDate).FirstOrDefault();

                //Adding additional data to SummaryRow.SummaryJson to populate the email placeholders
                var summaryJsonHistory = historyRow.JsonData.FromJson<SummaryJson>();
                summaryJsonHistory.AdditionalData = (additionalData != null && additionalData.Count > 0) ? additionalData : null;
                historyRow.JsonData = summaryJsonHistory.ToJson();

                JObject historyRowWithApprover = (historyRow.JsonData).ToJObject();
                await AddApprover(historyRow.Approver, historyRowWithApprover);
                placeHolderDictionary = await MapSummaryData(historyRowWithApprover.ToString(), tenantInfo, deviceNotificationInfo.ActionDetails, historyRow.Approver, historyDataExt, Xcv, tenant, responseJObject, summaryRows, emailType, deviceNotificationInfo, logData);
            }
        }
        return placeHolderDictionary;
    }

    /// <summary>
    /// Adds Approver data to the JObject
    /// Used while sending Emails which uses SummaryJSON not having Approver details
    /// </summary>
    /// <param name="approverAlias">Approver Alias</param>
    /// <param name="dataWithApprover">Approver Detail</param>
    private async Task AddApprover(string approverAlias, JObject dataWithApprover)
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
    /// Map template with summary data
    /// </summary>
    /// <param name="summaryData">Serialized Summary Json</param>
    /// <param name="tenantInfo">Approval Tenant Info</param>
    /// <param name="actionDetails">Action Details</param>
    /// <param name="alias">Approver Alias</param>
    /// <param name="transactionHistExt">List of Transaction History</param>
    /// <param name="xcv">Xcv</param>
    /// <param name="tenant">tenant adapter object</param>
    /// <param name="responseJObject">response Object</param>
    /// <param name="summaryRows">List Summary Rows</param>
    /// <param name="emailType">Email Type</param>
    /// <param name="deviceNotificationInfo">Device Notification Detail</param>
    /// <param name="logData">logData</param>
    /// <returns>returns a dictionary object </returns>
    private async Task<Dictionary<string, string>> MapSummaryData(string summaryData,
        ApprovalTenantInfo tenantInfo,
        Dictionary<string, string> actionDetails,
        string alias,
        List<TransactionHistoryExt> transactionHistExt,
        string xcv,
        ITenant tenant,
        JObject responseJObject,
        List<ApprovalSummaryRow> summaryRows,
        EmailType emailType,
        DeviceNotificationInfo deviceNotificationInfo,
        Dictionary<LogDataKey, object> logData)
    {
        string detailsPageURLLink = string.Empty;
        string approvalsBaseUrl = _config[ConfigurationKey.ApprovalsBaseUrl.ToString()];

        Dictionary<string, string> placeHolderDict = new Dictionary<string, string>();
        JSONHelper.ConvertJsonToDictionary(placeHolderDict, summaryData);

        var deviceDetailPage = String.Concat(_config[ConfigurationKey.ApprovalsBaseUrl.ToString()], _config[ConfigurationKey.DeviceDeepLinkUrl.ToString()]);

        if (placeHolderDict.TryGetValue("ApprovalIdentifier.DisplayDocumentNumber", out string displayDocumentNumber))
        {
            string fiscalYear = string.Empty;
            placeHolderDict.TryGetValue("ApprovalIdentifier.FiscalYear", out fiscalYear);

            PopulateDataInToDictionary(placeHolderDict, "MSApprovalsDetailPage", String.Format(approvalsBaseUrl + tenantInfo.TenantDetailUrl, tenantInfo.TenantId, displayDocumentNumber, fiscalYear, tenantInfo.TemplateName));
            PopulateDataInToDictionary(placeHolderDict, "MSApprovalsDeviceDetailPage", string.Format(deviceDetailPage, tenantInfo.TenantId, displayDocumentNumber, tenantInfo.TemplateName));
        }

        PopulateDataInToDictionary(placeHolderDict, "ToolName", tenantInfo.ToolName);
        PopulateDataInToDictionary(placeHolderDict, "TenantName", tenantInfo.AppName);
        PopulateDataInToDictionary(placeHolderDict, "TenantId", tenantInfo.RowKey);
        PopulateDataInToDictionary(placeHolderDict, "DocumentNumberPrefix", tenantInfo.DocumentNumberPrefix);

        placeHolderDict.TryGetValue("DetailPageURL", out detailsPageURLLink);
        PopulateDataInToDictionary(placeHolderDict, "AppName", string.IsNullOrWhiteSpace(detailsPageURLLink) ? tenantInfo.AppName : "[" + tenantInfo.AppName + "](" + detailsPageURLLink + ")");

        //// Add items to PlaceHolder dictionary for Outlook Quick Action
        PopulateDataInToDictionary(placeHolderDict, "Xcv", xcv);
        var tcv = Guid.NewGuid().ToString();
        PopulateDataInToDictionary(placeHolderDict, "Tcv", tcv);
        PopulateDataInToDictionary(placeHolderDict, "SessionId", tcv);
        PopulateDataInToDictionary(placeHolderDict, "UserMessageForComplianceAndAction", _config[ConfigurationKey.UserMessageForComplianceAndAction.ToString()]);

        if (placeHolderDict != null && placeHolderDict.ContainsKey("UnitValue"))
        {
            PopulateDataInToDictionary(placeHolderDict, "UnitValue", (Double.TryParse(placeHolderDict["UnitValue"], out double doubleUnitValue) ? doubleUnitValue.ToString("N", CultureInfo.CreateSpecificCulture(Constants.CultureName)) : placeHolderDict["UnitValue"]));
        }

        if (placeHolderDict != null && placeHolderDict.ContainsKey("SubmittedDate"))
        {
            string submittedDate = DateTime.Parse(placeHolderDict["SubmittedDate"], CultureInfo.CreateSpecificCulture(Constants.CultureName)).ToString("MM/dd/yy");
            PopulateDataInToDictionary(placeHolderDict, "SubmittedDate", submittedDate);
        }

        var summaryRow = summaryData.ToJObject();
        string approverChainString = await CreateCurrentApproverChain(summaryRow,
                                                                transactionHistExt,
                                                                alias,
                                                                tenantInfo
                                                                   );
        var approverChain = (approverChainString).ToJArray();
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
        string approverType = string.Empty;
        string actionDate = string.Empty;
        string notes = string.Empty;
        string justification = string.Empty;
        string delegateUser = string.Empty;
        string formattedApproverType = string.Empty;
        JArray approverChainItems = new JArray();
        JArray approverChainColumns = new JArray();
        foreach (var item in approverChain)
        {
            approverType = string.Format("{0}", item["Type"]);
            formattedApproverType = !string.IsNullOrEmpty(approverType) ? "(" + item["Type"] + ")" : string.Empty;
            actionDate = string.Format("{0}", item["ActionDate"]);
            notes = string.Format("{0}", item["Notes"]);
            delegateUser = string.Format("{0}", item["DelegateUser"]);
            justification = string.Format("{0}", item["Justification"]);
            if (string.IsNullOrEmpty(item["Action"].ToString()) && !Convert.ToBoolean(item["_future"].ToString()))
            {
                emailBodySection += string.Format(Constants.ApproverChainCurrent, item["Name"], item["Alias"], formattedApproverType);
                currentApproverType = approverType;
                AdaptiveCardHelper.GetItems("TextBlock", string.Format("&#x2794; {0} [{1}] {2}", item["Name"], item["Alias"], formattedApproverType), ref approverChainItems);
            }
            else if (string.IsNullOrEmpty(item["Action"].ToString()) && Convert.ToBoolean(item["_future"].ToString()) && !Convert.ToBoolean(item["_isPreApprover"].ToString()))
            {
                emailBodySection += string.Format(Constants.ApproverChainFuture, item["Name"], item["Alias"], formattedApproverType, item["Action"]);
                AdaptiveCardHelper.GetItems("TextBlock", string.Format("{0} [{1}] {2} {3}", item["Name"], item["Alias"], formattedApproverType, item["Action"]), ref approverChainItems);
                if (!string.IsNullOrWhiteSpace(notes))
                {
                    emailBodySection += string.Format(Constants.ApproverChainNotes, "Notes:", notes);
                    AdaptiveCardHelper.GetItems("TextBlock", string.Format("*{0}* {1}", "Notes:", notes), ref approverChainItems, horizontalAlignment: "left", padding: "small");
                }
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

        emailBodySection = string.Format(Constants.ApproverChainHeader, emailBodySection);
        PopulateDataInToDictionary(placeHolderDict, "ApproverChain", emailBodySection);
        PopulateDataInToDictionary(placeHolderDict, "ApproverType", currentApproverType);

        AdaptiveCardHelper.GetColumns(approverChainItems, ref approverChainColumns);
        JObject adaptiveApproverChain = AdaptiveCardHelper.GetColumnSets(approverChainColumns);
        placeHolderDict.Add("AdaptiveApproverChain", adaptiveApproverChain?.ToString());

        //// to check if theNotifyEmail With approval functionality is set to true.
        if (IsNotifyEmailWithApprovalFunctionality(tenantInfo) && EmailType.ActionableEmail == emailType)
        {
            // TODO:: This should be removed once outlook rollout actionable email for mobiles
            // Add MSApprovalsDetailPage key into responseJObject to show deeplink for mobile device users in actionable email
            if (responseJObject != null)
            {
                if (placeHolderDict.ContainsKey("MSApprovalsDetailPage"))
                {
                    if (responseJObject.ContainsKey("MSApprovalsDetailPage"))
                        responseJObject["MSApprovalsDetailPage"] = placeHolderDict["MSApprovalsDetailPage"].ToString();
                    else
                        responseJObject.Add("MSApprovalsDetailPage", placeHolderDict["MSApprovalsDetailPage"].ToString());
                }
                if (placeHolderDict.ContainsKey("MSApprovalsDeviceDetailPage"))
                {
                    if (responseJObject.ContainsKey("MSApprovalsDeviceDetailPage"))
                        responseJObject["MSApprovalsDeviceDetailPage"] = placeHolderDict["MSApprovalsDeviceDetailPage"].ToString();
                    else
                        responseJObject.Add("MSApprovalsDeviceDetailPage", placeHolderDict["MSApprovalsDeviceDetailPage"].ToString());
                }

                if (!string.IsNullOrEmpty(emailBodySection))
                {
                    responseJObject.Add("ApproverChain", emailBodySection);
                }

                if (!string.IsNullOrEmpty(deviceNotificationInfo.NotificationTemplateKey))
                {
                    responseJObject.Add("NotificationTemplateKey", deviceNotificationInfo.NotificationTemplateKey);
                }

                if (!string.IsNullOrEmpty(adaptiveApproverChain?.ToString()))
                {
                    responseJObject.Add("AdaptiveApproverChain", adaptiveApproverChain?.ToString());
                }

                PopulateDataInToDictionary(placeHolderDict, "ReceiptAcknowledgmentMessage", _config[ConfigurationKey.ReceiptAcknowledgmentMessage.ToString()]);
                PopulateDataInToDictionary(placeHolderDict, "AntiCorruptionMessage", _config[ConfigurationKey.AntiCorruptionMessage.ToString()]);
                PopulateDataInToDictionary(placeHolderDict, "AcceptsTerms", _config[ConfigurationKey.AcceptsTerms.ToString()]);

                string submitterAlias = responseJObject.SelectToken("Submitter.Alias") != null ? responseJObject.SelectToken("Submitter.Alias").Value<string>() : string.Empty;

                // fetch user profile image from blob and show into actionable email
                PopulateDataInToDictionary(placeHolderDict, "UserImage", await GetUserImage(submitterAlias, tcv, Constants.ClientDevice, logData));

                // Actionable email only visible, whoever are present in approver list
                PopulateDataInToDictionary(placeHolderDict, "ApproverList", string.Join(",", summaryRows.Select(s => s.Approver.ToLower()).ToList()));

                //// to process complete email template if the NotifyEmail With approval functionality is true.
                string detailTemplate = ConstructDynamicHtmlDetailsForEmail(responseJObject, tenantInfo, tenant, displayDocumentNumber, ref placeHolderDict, summaryRows, ref emailType);
                if (!string.IsNullOrWhiteSpace(detailTemplate))
                    PopulateDataInToDictionary(placeHolderDict, "DetailTemplate", detailTemplate.Trim());
            }
        }

        if (actionDetails != null)
        {
            foreach (var actionDetail in actionDetails)
            {
                if (placeHolderDict.Where(a => a.Key.Equals("ActionDetails." + actionDetail.Key, StringComparison.InvariantCultureIgnoreCase)).Count() == 0)
                {
                    if (!string.IsNullOrEmpty(actionDetail.Value))
                    {
                        PopulateDataInToDictionary(placeHolderDict, "ActionDetails." + actionDetail.Key, actionDetail.Value);
                    }
                    else
                    {
                        PopulateDataInToDictionary(placeHolderDict, "ActionDetails." + actionDetail.Key, string.Empty);
                    }
                }
            }
        }

        PopulateDataInToDictionary(placeHolderDict, "MSApprovalsResourceId", _config[ConfigurationKey.ApprovalsAudienceUrl.ToString()]);
        PopulateDataInToDictionary(placeHolderDict, "MSApprovalsBaseUrl", _config[ConfigurationKey.ApprovalsBaseUrl.ToString()]);
        PopulateDataInToDictionary(placeHolderDict, "MSApprovalsCoreServiceURL", _config[ConfigurationKey.ApprovalsCoreServicesURL.ToString()]);
        PopulateDataInToDictionary(placeHolderDict, "CurrentApprovers", responseJObject?.SelectToken("CurrentApprovers")?.Value<string>());

        return placeHolderDict;
    }

    /// <summary>
    /// Gets the type of the approval.
    /// </summary>
    /// <param name="historyJson">The history json.</param>
    /// <param name="approverAlias">The approver alias.</param>
    /// <param name="historyCount">The history Count.</param>
    /// <returns>returns a string</returns>
    private string GetApprovalType(JObject historyJson, string approverAlias, int historyCount)
    {
        string approverType = string.Empty;
        var approvalHierarchy = MSAHelper.ExtractValueFromJSON(historyJson, "ApprovalHierarchy").FromJson<List<ApprovalHierarchy>>();
        if (approvalHierarchy != null)
        {
            var approverHierarchies = approvalHierarchy?.Where(approver => approver?.Approvers != null && approver?.Approvers?.FirstOrDefault(x => x?.Alias == approverAlias) != null);
            if (approverHierarchies?.Count() > 1)
            {
                var hierarchy = approvalHierarchy?.Count > historyCount ? approvalHierarchy[historyCount] : null;
                var approver = hierarchy?.Approvers?.FirstOrDefault(x => x.Alias == approverAlias);
                if (approver != null)
                    approverType = hierarchy?.ApproverType;
            }
            else
            {
                approverType = approverHierarchies?.FirstOrDefault()?.ApproverType;
            }
        }
        else
        {
            approverType = MSAHelper.ExtractValueFromJSON(historyJson, "AdditionalData.ApproverType");
        }
        return approverType ?? string.Empty;
    }

    /// <summary>
    /// Gets the type of the approval.
    /// </summary>
    /// <param name="historyJson">The history json.</param>
    /// <param name="approverAlias">The approver alias.</param>
    /// <returns>returns a string</returns>
    private string GetApprovalType(JObject historyJson, string approverAlias)
    {
        string approverType = string.Empty;
        var approvalHierarchy = (MSAHelper.ExtractValueFromJSON(historyJson, "ApprovalHierarchy")).FromJson<List<ApprovalHierarchy>>();
        if (approvalHierarchy != null)
        {
            foreach (var approver in approvalHierarchy.Where(approver => approver.Approvers != null && approver.Approvers.FirstOrDefault(x => x.Alias == approverAlias) != null))
            {
                approverType = approver.ApproverType;
                break;
            }
        }
        else
        {
            approverType = MSAHelper.ExtractValueFromJSON(historyJson, "AdditionalData.ApproverType");
        }
        return approverType;
    }

    /// <summary>
    /// Sends the email retry on fail.
    /// </summary>
    /// <param name="usrDelegationNotification">The usr delegation notification.</param>
    /// <returns>returns a boolean value</returns>
    private Boolean SendEmailRetryOnFail(UserDelegationDeviceNotification usrDelegationNotification)
    {
        var logData = new Dictionary<LogDataKey, object>
        {
            { LogDataKey.EventType, Constants.FeatureUsageEvent },
            { LogDataKey.Xcv, usrDelegationNotification.Xcv },
            { LogDataKey.Tcv, usrDelegationNotification.Tcv },
            { LogDataKey.NotificationTemplateKey, usrDelegationNotification.NotificationTemplateKey },
            { LogDataKey.ReceivedTcv, usrDelegationNotification.Tcv }
        };
        int maxRetries = int.Parse(_config[ConfigurationKey.NotificationFrameworkMaxRetries.ToString()]);
        int repeatCounter = 0;

        Task task;
        //// Loop the configured number of times if the send email call fails.
        do
        {
            try
            {
                task = SendEmailAsync(usrDelegationNotification);
                task.Wait();
            }
            catch (Exception ex)
            {
                repeatCounter++;
                _logProvider.LogError(TrackingEvent.SendEmailNotificationFail_UserDelegation, ex, logData);
                return false;
            }
        } while ((true == task.IsFaulted) && (repeatCounter < maxRetries));
        return true;
    }

    /// <summary>
    /// Sends the email asynchronous.
    /// </summary>
    /// <param name="userDelegationNotification">The user delegation notification.</param>
    /// <returns>returns a task result</returns>
    private async Task SendEmailAsync(UserDelegationDeviceNotification userDelegationNotification)
    {
        var logData = new Dictionary<LogDataKey, object>
        {
            { LogDataKey.EventType, Constants.FeatureUsageEvent },
            { LogDataKey.Xcv, userDelegationNotification.Xcv },
            { LogDataKey.Tcv, userDelegationNotification.Tcv },
            { LogDataKey.ReceivedTcv, userDelegationNotification.Tcv },
            { LogDataKey.NotificationTemplateKey, userDelegationNotification.NotificationTemplateKey }
        };

        try
        {
            ApprovalEmailNotificationTemplates template = _emailTemplateHelper.GetTemplates().Where(emailtemplate => Regex.IsMatch(emailtemplate.RowKey, ApprovalEmailNotificationTemplates.EmailTemplatePartitionKey(userDelegationNotification.NotificationTemplateKey, userDelegationNotification.ActionTaken))).FirstOrDefault();
            NotificationItem emailData = new NotificationItem
            {
                Body = template.TemplateContent,
                Cc = FormatEmailIfNotValid(userDelegationNotification.CC),
                Subject = GetSubjectTemplate(template.TemplateContent),
                TemplateData = GetMailContent(userDelegationNotification).ToDictionary(pair => pair.Key, pair => (object)pair.Value),
                TemplatePartitionKey = new List<string>() { "0" },
                TemplateRowExp = ApprovalEmailNotificationTemplates.EmailTemplatePartitionKey(userDelegationNotification.NotificationTemplateKey, userDelegationNotification.ActionTaken),
                To = FormatEmailIfNotValid(userDelegationNotification.To)
            };
            await _notificationProvider.SendEmail(emailData);

            _logProvider.LogInformation(TrackingEvent.SendEmailNotification_UserDelegation, logData);
        }
        catch (Exception exception)
        {
            //// Log the error and don't throw it. Otherwise it will not send any other notifications.
            _logProvider.LogError(TrackingEvent.SendEmailNotificationFail_UserDelegation, exception, logData);

            throw;
        }
    }

    /// <summary>
    /// Gets the content of the mail.
    /// </summary>
    /// <param name="userDelegationNotificationInfo">The user delegation notification information.</param>
    /// <returns>returns a dictionary object</returns>
    private Dictionary<string, string> GetMailContent(UserDelegationDeviceNotification userDelegationNotificationInfo)
    {
        return MapUserDelegationData(userDelegationNotificationInfo);
    }

    /// <summary>
    /// Maps the user delegation data.
    /// </summary>
    /// <param name="userDelegationNotificationInfo">The user delegation notification information.</param>
    /// <returns>returns a dictionay object</returns>
    private Dictionary<string, string> MapUserDelegationData(UserDelegationDeviceNotification userDelegationNotificationInfo)
    {
        Dictionary<string, string> placeHolderDict = new Dictionary<string, string>
        {
            { "LoggedInAlias", userDelegationNotificationInfo.CC },
            { "DelegatedTo", userDelegationNotificationInfo.To },
            { "DateFrom", userDelegationNotificationInfo.DateFrom.ToString() },
            { "DateTo", userDelegationNotificationInfo.DateTo.ToString() }
        };

        return placeHolderDict;
    }
}