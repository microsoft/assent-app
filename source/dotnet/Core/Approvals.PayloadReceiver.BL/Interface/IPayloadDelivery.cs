// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace Microsoft.CFS.Approvals.PayloadReceiver.BL.Interface;

using System;
using System.Threading.Tasks;
using global::Azure.Messaging.ServiceBus;
using Microsoft.CFS.Approvals.Contracts.DataContracts;
using Microsoft.CFS.Approvals.Model;

public interface IPayloadDelivery
{
    /// <summary>
    /// Sends payload to payload destination provided
    /// </summary>
    /// <param name="approvalRequestExpression"></param>
    /// <param name="payloadDestinationInfo"></param>
    /// <param name="payloadId"></param>
    /// <returns></returns>
    Task<bool> SendPayload(ApprovalRequestExpression approvalRequestExpression, PayloadDestinationInfo payloadDestinationInfo, Guid payloadId);

    /// <summary>
    /// Establishes a service bus channel using payload destination info per tenant
    /// </summary>
    /// <param name="payloadDestinationInfo"></param>
    /// <returns></returns>
    ServiceBusSender EstablishServiceBusChannel(Model.PayloadDestinationInfo payloadDestinationInfo);

    /// <summary>
    /// Builds a service bus message using Approval Request Expression and assigns the service bus message id provided
    /// </summary>
    /// <param name="approvalRequestExpression"></param>
    /// <param name="messageId"></param>
    /// <returns></returns>
    Task<ServiceBusMessage> BuildServiceBusMessage(ApprovalRequestExpression approvalRequestExpression, string messageId);
}