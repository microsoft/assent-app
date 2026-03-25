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
    /// <param name="messageId"></param>
    /// <param name="approvalRequestVersion"></param>
    /// <param name="topicName"></param>
    /// <param name="blobId"></param>
    /// <returns></returns>
    Task<bool> SendMessage(ApprovalRequestExpression approvalRequestExpression, string messageId, string approvalRequestVersion, string? topicName = null, string? blobId = null, int delay = 0);

    /// <summary>
    /// Builds a service bus message using Approval Request Expression and assigns the service bus message id provided
    /// </summary>
    /// <param name="approvalRequestExpression"></param>
    /// <param name="messageId"></param>
    /// <param name="approvalRequestVersion"></param>
    /// <param name="topicName"></param>
    /// <param name="blobId"></param>
    /// <returns></returns>
    Task<ServiceBusMessage> BuildServiceBusMessage(ApprovalRequestExpression approvalRequestExpression, string applicationId, string messageId, string approvalRequestVersion, string? topicName, string? blobId, int delay = 0);

    /// <summary>
    /// Send Message
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="applicationId"></param>
    /// <param name="message"></param>
    /// <param name="messageId"></param>
    /// <param name="correlationId"></param>
    /// <param name="queueOrTopicName"></param>
    /// <param name="filter"></param>
    /// <returns></returns>
    Task<bool> AddFilterAndSendMessage<T>(T message, string applicationId, string messageId, string correlationId, string? queueOrTopicName = null, string? filter = null) where T : class;

    /// <summary>
    /// Sends the provided ServiceBusMessage to the specified queue or topic in the Service Bus.
    /// </summary>
    /// <param name="serviceBusMessage">The ServiceBusMessage to send.</param>
    /// <param name="queueOrTopicName">The name of the queue or topic to send the message to. If null, the default message sender will be used.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    Task SendMessageToSerivceBus(ServiceBusMessage serviceBusMessage, string? queueOrTopicName = null);

    /// <summary>
    /// Sends the provided ServiceBusMessage to the queue or topic in the service bus at a scheduled time
    /// </summary>
    /// <param name="serviceBusMessage"></param>
    /// <param name="queueOrTopicName"></param>
    /// <returns></returns>
    Task<long> SendScheduledMessageToServiceBus(ServiceBusMessage serviceBusMessage, DateTimeOffset scheduledTime, string? queueOrTopicName = null);

    /// <summary>
    /// Cancels the scheduled message in the service bus using the sequence number
    /// </summary>
    /// <param name="sequenceNumber">The sequence number of the scheduled message to cancel.</param>
    /// <param name="queueOrTopicName">The name of the queue or topic where the message was scheduled. If null or empty, the default message sender is used.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    Task CancelScheduledMessageFromServiceBus(long sequenceNumber, string? queueOrTopicName = null);
}