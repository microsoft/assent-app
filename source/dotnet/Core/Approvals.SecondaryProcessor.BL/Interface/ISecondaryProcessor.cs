// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace Approvals.SecondaryProcessor.BL.Interface
{
    using Azure.Messaging.ServiceBus;
    using Microsoft.CFS.Approvals.Contracts.DataContracts;
    using Microsoft.CFS.Approvals.Model;

    public interface ISecondaryProcessor
    {
        /// <summary>
        /// Process secondary details
        /// </summary>
        /// <param name="approvalRequest"></param>
        /// <param name="blobId"></param>
        /// <param name="tenantInfo"></param>
        /// <param name="message"></param>
        /// <returns></returns>
        public Task<ApprovalRequestExpressionExt> ProcessSecondaryDetailsAsync
            (ApprovalRequestExpressionExt approvalRequest,
            string blobId,
            ApprovalTenantInfo tenantInfo,
            ServiceBusReceivedMessage message,
            bool isReassignmentFlow
            );
    }
}