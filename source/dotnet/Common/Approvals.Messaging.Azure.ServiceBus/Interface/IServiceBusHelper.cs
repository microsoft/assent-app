// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace Microsoft.CFS.Approvals.Messaging.Azure.ServiceBus.Interface;

using global::Azure.Messaging.ServiceBus;
using Microsoft.CFS.Approvals.Contracts.DataContracts;

public interface IServiceBusHelper
{
    /// <summary>
    /// Sends the Approval Request Expression to payload destination provided after converting it into Brokered Message
    /// </summary>
    /// <param name="approvalRequestExpression"></param>
    /// <param name="brokeredMessageId"></param>
    /// <param name="approvalRequestVersion"></param>
    /// <returns></returns>
    Task<bool> SendMessage(ApprovalRequestExpression approvalRequestExpression, string messageId, string approvalRequestVersion);

    /// <summary>
    /// Builds a service bus message using Approval Request Expression and assigns the service bus message id provided
    /// </summary>
    /// <param name="approvalRequestExpression"></param>
    /// <param name="messageId"></param>
    /// <param name="approvalRequestVersion"></param>
    /// <returns></returns>
    Task<ServiceBusMessage> BuildServiceBusMessage(ApprovalRequestExpression approvalRequestExpression, string messageId, string approvalRequestVersion);
}