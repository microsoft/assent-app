// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace Microsoft.CFS.Approvals.PrimaryProcessor.BL.Interface;

using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using global::Azure.Messaging.ServiceBus;
using Microsoft.CFS.Approvals.Contracts.DataContracts;
using Microsoft.CFS.Approvals.Domain.BL.Interface;
using Microsoft.CFS.Approvals.Model;

public interface IApprovalPresenter
{
    #region Public Properties

    ApprovalTenantInfo TenantInfo { get; set; }

    #endregion Public Properties

    /// <summary>
    /// Process approval request expressions
    /// </summary>
    /// <param name="approvalRequests"></param>
    /// <param name="message"></param>
    /// <returns>List of approval request expressions</returns>
    Task<List<ApprovalRequestExpressionExt>> ProcessApprovalRequestExpressions(List<ApprovalRequestExpressionExt> approvalRequests, ServiceBusReceivedMessage message);

    /// <summary>
    /// Add approval details to azure table
    /// </summary>
    /// <param name="approvalRequest"></param>
    /// <param name="message"></param>
    /// <returns></returns>
    Task<bool> AddApprovalDetailsToAzureTable(ApprovalRequestDetails approvalRequest, ServiceBusReceivedMessage message);

    /// <summary>
    /// Download and store attachments
    /// </summary>
    /// <param name="approvalRequest"></param>
    /// <param name="activityId"></param>
    /// <param name="tenant"></param>
    /// <returns></returns>
    Task<Tuple<bool, List<Attachment>>> DownloadAndStoreAttachments(ApprovalRequestDetails approvalRequest, string activityId, ITenant tenant);

    /// <summary>
    /// Creates a new Brokered ServiceBusMessage and pushes it to Notification Topic
    /// The messages which will reach this topic will be processed separately
    /// </summary>
    /// <param name="approvalRequest"></param>
    /// <param name="message"
    /// <param name="summaryRows"></param>
    /// <param name="notification"></param>
    /// <param name="detailsLoadSuccess"></param>
    /// <returns></returns>
    Task MoveMessageToNotificationTopic(ApprovalRequestExpressionExt approvalRequest, ServiceBusReceivedMessage message, List<ApprovalSummaryRow> summaryRows, DeviceNotificationInfo notification = null, bool detailsLoadSuccess = false);

    /// <summary>
    /// Creates a new Brokered ServiceBusMessage and pushes it to secondary queue
    /// </summary>
    /// <param name="approvalRequest"></param>
    /// <param name="message"
    /// <returns></returns>
    Task MoveMessageToSecondaryQueue(ApprovalRequestExpressionExt approvalRequest, ServiceBusReceivedMessage message);
}