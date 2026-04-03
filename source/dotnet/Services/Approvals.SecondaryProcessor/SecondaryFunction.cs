namespace SecondaryFunction;

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

public class SecondaryFunction
{
    private readonly IApprovalsQueueReceiver _secondaryReceiver;
    private readonly List<ApprovalTenantInfo> _allTenantInfo;

    public SecondaryFunction(IApprovalTenantInfoHelper approvalTenantInfoHelper, IApprovalsQueueReceiver secondaryReceiver)
    {
        _secondaryReceiver = secondaryReceiver;
        _allTenantInfo = approvalTenantInfoHelper.GetTenantInfoByHost("Worker");
    }

    [FunctionName("SecondaryFunction")]
    public async Task Run(
        [ServiceBusTrigger("%QueueNameSecondary%", Connection = "ServiceBusNamespace")] ServiceBusReceivedMessage message,
        ILogger log)
    {
        if (message.ApplicationProperties.ContainsKey("ApplicationId"))
        {
            var docTypeId = message.ApplicationProperties["ApplicationId"].ToString();
            _secondaryReceiver.TenantInfo = _allTenantInfo.FirstOrDefault(x => x.DocTypeId.Equals(docTypeId, StringComparison.InvariantCultureIgnoreCase));
        }

        string blobId = System.Text.Encoding.UTF8.GetString(message.Body); //not necessairily the blobId in every case
        await _secondaryReceiver.OnMainMessageReceived(message, blobId);
    }
}