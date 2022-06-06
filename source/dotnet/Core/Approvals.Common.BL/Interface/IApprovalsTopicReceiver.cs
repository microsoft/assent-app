// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace Microsoft.CFS.Approvals.Common.BL.Interface
{
    using System;
    using System.Threading.Tasks;
    using Microsoft.Azure.ServiceBus;
    using Microsoft.CFS.Approvals.Model;

    public interface IApprovalsTopicReceiver
    {
        ApprovalTenantInfo TenantInfo { get; set; }

        int FailedCount { get; set; }

        DateTime LastMessageProcessingTime { get; set; }

        /// <summary>
        /// Business logic to process a message from the main topic.
        /// </summary>
        /// <param name="blobId"></param>
        /// <param name="message"></param>
        Task OnMainMessageReceived(string blobId, Message message);

        /// <summary>
        /// Business logic to retry process a message from the retry topic.
        /// </summary>
        /// <param name="blobId"></param>
        /// <param name="message"></param>
        /// <returns></returns>
        Task OnRetryMessageRecieved(string blobId, Message message);
    }
}