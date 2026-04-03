using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Azure.Messaging.ServiceBus;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.ServiceBus;
using Microsoft.CFS.Approvals.Common.BL.Interface;
using Microsoft.CFS.Approvals.Core.BL.Interface;
using Microsoft.CFS.Approvals.Model;
using Microsoft.Extensions.Logging;

namespace Approvals.External.PrimaryProcessor
{
    public class ExternalPrimaryFunction
    {
        // Receiver that processes messages from Service Bus
        private readonly IApprovalsTopicReceiver _externalGenericReceiver;
        // List of all tenant information loaded for the host "Worker"
        private readonly List<ApprovalTenantInfo> _allTenantInfo;

        /// <summary>
        /// Constructor injects tenant info helper and topic receiver.
        /// Loads all tenant info for the host "Worker".
        /// </summary>
        public ExternalPrimaryFunction(IApprovalTenantInfoHelper approvalTenantInfoHelper, IApprovalsTopicReceiver externalGenericReceiver)
        {
            // Fetch all tenant info for the host "Worker"
            _allTenantInfo = approvalTenantInfoHelper.GetTenantInfoByHost("Worker");
            _externalGenericReceiver = externalGenericReceiver;
        }

        /// <summary>
        /// Azure Function entry point for Service Bus trigger.
        /// Processes messages from the main topic and delegates to the receiver.
        /// </summary>
        /// <param name="message">The received Service Bus message.</param>
        /// <param name="log">Logger instance for diagnostics.</param>
        /// todo :: remove hard coding of topic and subscription names
        [FunctionName("ExternalPrimaryFunction")]
        public async Task Run(
            [ServiceBusTrigger("%TopicNameExternalMain%", "%SubscriptionNameExternal%", Connection = "ServiceBusNamespace", IsSessionsEnabled = true)]
            ServiceBusReceivedMessage message,
            ServiceBusSessionMessageActions sessionActions,
            ILogger log)
        {
            // Extract the document type ID from message properties
            var docTypeId = message.ApplicationProperties["ApplicationId"].ToString();

            // Set the receiver's tenant info based on the document type ID
            _externalGenericReceiver.TenantInfo = _allTenantInfo.FirstOrDefault(x => x.DocTypeId.Equals(docTypeId, StringComparison.InvariantCultureIgnoreCase));

            // Get the blob ID from the message body (assumes body is a UTF8-encoded string)
            string blobId = Encoding.UTF8.GetString(message.Body);

            // Delegate message processing to the receiver
            await _externalGenericReceiver.OnMainMessageReceived(blobId, message);

            await sessionActions.CompleteMessageAsync(message);
        }
    }
}
