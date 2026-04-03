namespace Approvals.ReassignmentProcessor.BL.Interface
{
    using Azure.Messaging.ServiceBus;
    using Microsoft.CFS.Approvals.Contracts.DataContracts;
    using Microsoft.CFS.Approvals.Model;

    /// <summary>
    /// Reassignment Processor Interface
    /// </summary>
    public interface IReassignmentProcessor
    {
        /// <summary>
        /// Process secondary details
        /// </summary>
        /// <param name="approvalRequest"></param>
        /// <param name="blobId"></param>
        /// <param name="tenantInfo"></param>
        /// <param name="message"></param>
        /// <returns></returns>
        Task<ApprovalRequestExpressionExt> ProcessReassignmentDetails
            (ApprovalRequestExpressionExt approvalRequest,
            string blobId,
            ApprovalTenantInfo tenantInfo,
            ServiceBusReceivedMessage message);
    }
}
