// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace Microsoft.CFS.Approvals.AuditAgentAzFunction
{
    using System.Threading.Tasks;
    using Microsoft.Azure.ServiceBus;
    using Microsoft.Azure.WebJobs;
    using Microsoft.CFS.Approvals.AuditProcessor.BL.Interface;
    using Microsoft.Extensions.Logging;

    public class AuditAgentFunction
    {
        private readonly IAuditAgentHelper _auditAgentHelper = null;

        public AuditAgentFunction(IAuditAgentHelper auditAgentHelper)
        {
            _auditAgentHelper = auditAgentHelper;
        }

        [FunctionName("AuditAgentFunction")]
        public async Task Run([ServiceBusTrigger("%TopicNameMain%", "%SubscriptionNameAuditAgent%", Connection = "ServiceBusConnectionString")] Message message, ILogger log)
        {
            string blobId = System.Text.Encoding.UTF8.GetString(message.Body);
            await _auditAgentHelper.ProcessMessage(blobId, message);
        }
    }
}