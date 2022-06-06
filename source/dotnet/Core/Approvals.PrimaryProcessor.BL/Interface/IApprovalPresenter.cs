// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace Microsoft.CFS.Approvals.PrimaryProcessor.BL.Interface
{
    using System.Collections.Generic;
    using System.Threading.Tasks;
    using Microsoft.Azure.ServiceBus;
    using Microsoft.CFS.Approvals.Contracts.DataContracts;
    using Microsoft.CFS.Approvals.Domain.BL.Interface;
    using Microsoft.CFS.Approvals.Model;

    public interface IApprovalPresenter
    {
        #region Public Properties

        ApprovalTenantInfo TenantInfo { get; set; }

        #endregion Public Properties

        Task<List<ApprovalRequestExpressionExt>> ProcessApprovalRequestExpressions(List<ApprovalRequestExpressionExt> approvalRequests, Message message);

        Task<bool> AddApprovalDetailsToAzureTable(ApprovalRequestDetails approvalRequest, Message message);

        Task<bool> DownloadAndStoreAttachments(ApprovalRequestDetails approvalRequest, string activityId, ITenant tenant);

        Task MoveMessageToNotificationTopic(ApprovalRequestExpressionExt approvalRequest, Message message, List<ApprovalSummaryRow> summaryRows, DeviceNotificationInfo notification = null, bool detailsLoadSuccess = false);
    }
}