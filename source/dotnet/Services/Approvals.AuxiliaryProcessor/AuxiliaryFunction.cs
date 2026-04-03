using System.Threading.Tasks;
using Approvals.AuxiliaryProcessor.BL.Interface;
using Azure.Messaging.ServiceBus;
using Microsoft.Azure.WebJobs;
using Microsoft.CFS.Approvals.Core.BL.Interface;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;

namespace Approvals.AuxiliaryProcessor
{
    public class AuxiliaryFunction
    {
        private readonly IAuxiliaryProcessor _auxiliaryProcessor;
        private readonly IApprovalTenantInfoHelper _approvalTenantInfoHelper;

        public AuxiliaryFunction(IApprovalTenantInfoHelper approvalTenantInfoHelper, IAuxiliaryProcessor auxiliaryProcessor)
        {
            _approvalTenantInfoHelper = approvalTenantInfoHelper;
            _auxiliaryProcessor = auxiliaryProcessor;
        }

        [FunctionName("GetAttachmentInsightsAsync")]
        public async Task ProcessMessageAsync(
            [ServiceBusTrigger("%TopicNameAuxiliary%", "%TopicSubscriptionDiscrepancies%",
            Connection = "ServiceBusNamespace")] ServiceBusReceivedMessage message,
            ILogger log)
        {
            if (message.Body != null)
            {
                dynamic messageJSON = JsonConvert.DeserializeObject(System.Text.Encoding.UTF8.GetString(message.Body));
                int tenantId = (int)messageJSON.tenantId;

                _auxiliaryProcessor.TenantInfo = _approvalTenantInfoHelper.GetTenantInfo(tenantId);
                await _auxiliaryProcessor.ProcessMessageAsync(message);
            }
        }
    }
}