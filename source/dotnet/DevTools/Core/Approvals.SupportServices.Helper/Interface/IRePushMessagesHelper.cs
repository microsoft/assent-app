// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace Microsoft.CFS.Approvals.SupportServices.Helper.Interface
{
    using System.Collections.Generic;
    using System.Net.Http;
    using System.Threading.Tasks;

    public interface IRePushMessagesHelper
    {
        bool SendNotification { get; set; }
        Task<HttpResponseMessage> CheckAndRePushIfPayloadExists(string jsonData);

        Task<List<string>> RepushDocumentAsync(List<dynamic> resultsAudit);

        Task<bool> BuildAndSendBrokeredMessageAsync(dynamic resultAudit);
    }
}
