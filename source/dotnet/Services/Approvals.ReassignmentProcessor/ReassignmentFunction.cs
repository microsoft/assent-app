using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Azure.Messaging.ServiceBus;
using Microsoft.Azure.WebJobs;
using Microsoft.CFS.Approvals.Common.BL.Interface;
using Microsoft.CFS.Approvals.Core.BL.Interface;
using Microsoft.CFS.Approvals.Model;
using Microsoft.Extensions.Logging;

namespace Approvals.ReassignmentFunction
{
    /// <summary>
    /// Reassignment Function
    /// </summary>
    public class ReassignmentFunction
    {
        /// <summary>
        /// Reassignment Receiver
        /// </summary>
        private readonly IApprovalsQueueReceiver _reassignmentReceiver;

        /// <summary>
        /// Approval Tenant Info
        /// </summary>
        private readonly List<ApprovalTenantInfo> _allTenantInfo;

        /// <summary>
        /// Reassignment Function Constructor
        /// </summary>
        /// <param name="approvalTenantInfoHelper"></param>
        /// <param name="reassignmentReceiver"></param>
        public ReassignmentFunction(IApprovalTenantInfoHelper approvalTenantInfoHelper, IApprovalsQueueReceiver reassignmentReceiver)
        {
            _reassignmentReceiver = reassignmentReceiver;
            _allTenantInfo = approvalTenantInfoHelper.GetTenantInfoByHost("Worker");
        }

        /// <summary>
        /// Reassignment Function Run
        /// </summary>
        /// <param name="message"></param>
        /// <param name="log"></param>
        /// <returns></returns>
        [FunctionName("ReassignmentFunction")]
        public async Task Run(
            [ServiceBusTrigger("%QueueNameReassignment%", Connection = "ServiceBusNamespace")] ServiceBusReceivedMessage message,
            ILogger log)
        {
            if (message.ApplicationProperties.ContainsKey("ApplicationId"))
            {
                var docTypeId = message.ApplicationProperties["ApplicationId"].ToString();
                _reassignmentReceiver.TenantInfo = _allTenantInfo.FirstOrDefault(x => x.DocTypeId.Equals(docTypeId, StringComparison.InvariantCultureIgnoreCase));
            }

            string blobId = System.Text.Encoding.UTF8.GetString(message.Body.ToArray());
            await _reassignmentReceiver.OnMainMessageReceived(message, blobId);
        }
    }
}


