// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace Microsoft.CFS.Approvals.PayloadReceiver.BL.Interface
{
    using System.Threading.Tasks;
    using Newtonsoft.Json.Linq;

    public interface IPayloadReceiverManager
    {
        /// <summary>
        /// Orchestration: POST method which receives the payload from tenant services and sends that to Approvals ServiceBus after all validation checks are successful
        /// </summary>
        /// <param name="documentTypeId">Unique TenantId (GUID) specifying a particular Tenant for which the Payload is received</param>
        /// <param name="payload">Data payload</param>
        /// <returns>Http Response Message</returns>
        Task<JObject> ManagePost(string documentTypeId, string payload);
    }
}
