// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace Microsoft.CFS.Approvals.Common.BL.Interface
{
    using System.Threading.Tasks;
    using global::Azure.Messaging.ServiceBus;
    using Microsoft.CFS.Approvals.Model;

    public interface IApprovalsQueueReceiver
    {
        ApprovalTenantInfo TenantInfo { get; set; }

        /// <summary>
        /// Business logic to process a message from the main topic.
        /// </summary>
        /// <param name="message"></param>
        /// <param name="blobId"></param>
        Task OnMainMessageReceived(ServiceBusReceivedMessage message, string blobId = "");
    }
}