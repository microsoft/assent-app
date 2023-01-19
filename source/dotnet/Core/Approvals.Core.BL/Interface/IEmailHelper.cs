// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace Microsoft.CFS.Approvals.Core.BL.Interface;

using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.CFS.Approvals.Contracts;
using Microsoft.CFS.Approvals.Contracts.DataContracts;
using Microsoft.CFS.Approvals.Domain.BL.Interface;
using Microsoft.CFS.Approvals.LogManager.Model;
using Microsoft.CFS.Approvals.Model;
using Newtonsoft.Json.Linq;

public interface IEmailHelper
{
    /// <summary>
    /// Send email
    /// </summary>
    /// <param name="approvalNotificationDetails"></param>
    /// <param name="tenant"></param>
    /// <param name="emailType"></param>
    /// <returns></returns>
    ApprovalRequestResult SendEmail(ApprovalNotificationDetails approvalNotificationDetails, ITenant tenant, EmailType emailType);

    /// <summary>
    /// Send email
    /// </summary>
    /// <param name="userDelegationNotification"></param>
    /// <returns></returns>
    Boolean SendEmail(UserDelegationDeviceNotification userDelegationNotification);

    /// <summary>
    /// Populate data in dictionary
    /// </summary>
    /// <typeparam name="T1"></typeparam>
    /// <typeparam name="T2"></typeparam>
    /// <param name="dictionary"></param>
    /// <param name="key"></param>
    /// <param name="value"></param>
    void PopulateDataInToDictionary<T1, T2>(Dictionary<T1, T2> dictionary, T1 key, T2 value);

    /// <summary>
    /// Get details data for approval request
    /// </summary>
    /// <param name="tenantInfo"></param>
    /// <param name="approvalIdentifier"></param>
    /// <param name="summaryRow"></param>
    /// <param name="logData"></param>
    /// <param name="operationName"></param>
    /// <param name="callType"></param>
    /// <returns></returns>
    JObject GetDetailsDataforApprovalRequest(ApprovalTenantInfo tenantInfo, ApprovalIdentifier approvalIdentifier, ApprovalSummaryRow summaryRow, Dictionary<LogDataKey, object> logData, string operationName = "authsum", int callType = 2);

    /// <summary>
    /// Check if notification email allowed
    /// </summary>
    /// <param name="responseJObject"></param>
    /// <returns></returns>
    bool IsNotificationEmailAllowed(JObject responseJObject);

    /// <summary>
    /// Fetch missing details data from LOB
    /// </summary>
    /// <param name="responseJObject"></param>
    /// <param name="tenantInfo"></param>
    /// <param name="approvalIdentifier"></param>
    /// <param name="summaryRow"></param>
    /// <param name="logData"></param>
    void FetchMissingDetailsDataFromLOB(JObject responseJObject, ApprovalTenantInfo tenantInfo, ApprovalIdentifier approvalIdentifier, ApprovalSummaryRow summaryRow, Dictionary<LogDataKey, object> logData);

    /// <summary>
    /// Get attachments to attach in email
    /// </summary>
    /// <param name="responseJObject"></param>
    /// <param name="approvalIdentifier"></param>
    /// <param name="tenantInfo"></param>
    /// <param name="tenant"></param>
    /// <param name="logData"></param>
    /// <param name="isAttachmentDownloadSuccess"></param>
    /// <returns></returns>
    List<NotificationDataAttachment> GetAttachmentsToAttachInEmail(JObject responseJObject,
                    ApprovalIdentifier approvalIdentifier,
                    ApprovalTenantInfo tenantInfo,
                    ITenant tenant, Dictionary<LogDataKey, object> logData,
                    ref bool isAttachmentDownloadSuccess);

    /// <summary>
    /// Create current approver chain
    /// </summary>
    /// <param name="documentSummary"></param>
    /// <param name="historyDataExts"></param>
    /// <param name="alias"></param>
    /// <param name="tenantInfo"></param>
    /// <returns></returns>
    Task<string> CreateCurrentApproverChain(JObject documentSummary, List<TransactionHistoryExt> historyDataExts, string alias, ApprovalTenantInfo tenantInfo);

    /// <summary>
    /// Construct dynamic html details for email
    /// </summary>
    /// <param name="responseJObject"></param>
    /// <param name="tenantInfo"></param>
    /// <param name="tenant"></param>
    /// <param name="displayDocumentNumber"></param>
    /// <param name="placeHolderDict"></param>
    /// <param name="summaryRows"></param>
    /// <param name="emailType"></param>
    /// <returns></returns>
    string ConstructDynamicHtmlDetailsForEmail(JObject responseJObject,
        ApprovalTenantInfo tenantInfo,
        ITenant tenant,
        string displayDocumentNumber,
        ref Dictionary<string, string> placeHolderDict,
        List<ApprovalSummaryRow> summaryRows,
        ref EmailType emailType);

    /// <summary>
    /// Get user image
    /// </summary>
    /// <param name="alias"></param>
    /// <param name="SessionId"></param>
    /// <param name="clientDevice"></param>
    /// <param name="logData"></param>
    /// <returns></returns>
    Task<string> GetUserImage(string alias, string SessionId, string clientDevice, Dictionary<LogDataKey, object> logData);
}