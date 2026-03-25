// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace Microsoft.CFS.Approvals.AuditProcessor.DL.Interface;

using global::Azure.Messaging.ServiceBus;
using Microsoft.CFS.Approvals.Contracts.DataContracts;

public interface IAuditAgentDataProvider
{
    /// <summary>
    /// Inserts the in to document database.
    /// </summary>
    /// <param name="approvalRequestExpressionExt">The approval request expression ext.</param>
    /// <param name="rawArJson">The raw ar json.</param>
    /// <param name="brokeredMessage">The brokered message.</param>
    /// <param name="exceptionMessage">The exception message.</param>
    /// <param name="stackTrace">The stack trace.</param>
    /// <returns></returns>
    void InsertInToDocumentDB(ApprovalRequestExpressionExt approvalRequestExpressionExt, string rawArJson, ServiceBusReceivedMessage brokeredMessage, string exceptionMessage = "", string stackTrace = "");
}
