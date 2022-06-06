// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace Microsoft.CFS.Approvals.AuditProcessor.BL.Interface
{
    using System.Threading.Tasks;
    using Microsoft.Azure.ServiceBus;

    public interface IAuditAgentHelper
    {
        /// <summary>
        /// Processes and save ARX into cosmos db
        /// </summary>
        /// <param name="blobId">Blod Id</param>
        /// <param name="message">Service Bus Message</param>
        /// <returns></returns>
        Task ProcessMessage(string blobId, Message message);
    }
}