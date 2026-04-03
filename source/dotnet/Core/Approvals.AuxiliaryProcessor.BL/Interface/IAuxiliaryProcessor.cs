namespace Approvals.AuxiliaryProcessor.BL.Interface
{
    using Azure.Messaging.ServiceBus;
    using Microsoft.CFS.Approvals.Model;

    public interface IAuxiliaryProcessor
    {
        ApprovalTenantInfo TenantInfo { get; set; }

        /// <summary>
        /// Process Message
        /// </summary>
        /// <param name="message"></param>
        /// <returns></returns>
        public Task ProcessMessageAsync
            (ServiceBusReceivedMessage message);
    }
}