// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace Microsoft.CFS.Approvals.AuditProcessor.DL.Interface;

using Microsoft.Azure.ServiceBus;
using Microsoft.CFS.Approvals.Contracts.DataContracts;

public interface IAuditAgentDataProvider
{
    void InsertInToDocumentDB(ApprovalRequestExpressionExt approvalRequestExpressionExt, string rawArJson, Message brokeredMessage, string exceptionMessage = "", string stackTrace = "");
}
