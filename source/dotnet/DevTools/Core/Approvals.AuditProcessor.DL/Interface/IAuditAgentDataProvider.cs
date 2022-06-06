// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace Microsoft.CFS.Approvals.AuditProcessor.DL.Interface
{
    using System.Collections.Generic;
    using System.Threading.Tasks;
    using Microsoft.Azure.ServiceBus;
    using Microsoft.CFS.Approvals.Contracts.DataContracts;

    public interface IAuditAgentDataProvider
    {
        void InsertInToDocumentDB(ApprovalRequestExpressionExt approvalRequestExpressionExt, string rawArJson, Message brokeredMessage, string exceptionMessage = "", string stackTrace = "");

        Task DeleteDocument(string document);

        List<dynamic> GetDocuments(string documentNumber, string partitionKey);
    }
}
