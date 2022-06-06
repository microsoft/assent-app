// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace Microsoft.CFS.Approvals.PrimaryAzFunction
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;
    using Microsoft.Azure.ServiceBus;
    using Microsoft.Azure.WebJobs;
    using Microsoft.CFS.Approvals.Common.BL.Interface;
    using Microsoft.CFS.Approvals.Core.BL.Interface;
    using Microsoft.CFS.Approvals.Model;
    using Microsoft.Extensions.Logging;

    public class PrimaryFunction
    {
        private readonly IApprovalsTopicReceiver _genericReceiver;
        private readonly List<ApprovalTenantInfo> _allTenantInfo;

        public PrimaryFunction(IApprovalTenantInfoHelper approvalTenantInfoHelper, IApprovalsTopicReceiver genericReceiver)
        {
            _allTenantInfo = approvalTenantInfoHelper.GetTenantInfoByHost("Worker");
            _genericReceiver = genericReceiver;
        }

        [FunctionName("PrimaryFunction")]
        public async Task Run([ServiceBusTrigger("%TopicNameMain%", "%SubscriptionName%", Connection = "ServiceBusConnectionString")] Message message, ILogger log)
        {
            var docTypeId = message.UserProperties["ApplicationId"].ToString();
            _genericReceiver.TenantInfo = _allTenantInfo.FirstOrDefault(x => x.DocTypeId.Equals(docTypeId, StringComparison.InvariantCultureIgnoreCase));
            string blobId = System.Text.Encoding.UTF8.GetString(message.Body);
            
            await _genericReceiver.OnMainMessageReceived(blobId, message);
        }
    }
}