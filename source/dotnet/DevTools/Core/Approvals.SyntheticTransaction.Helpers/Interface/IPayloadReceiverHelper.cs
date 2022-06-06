// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace Microsoft.CFS.Approvals.SyntheticTransaction.Helpers.Interface
{
    using System.Net.Http;
    using System.Threading.Tasks;

    public interface IPayloadReceiverHelper
    {
        Task<HttpResponseMessage> SendPayload(string Payload);
    }
}
