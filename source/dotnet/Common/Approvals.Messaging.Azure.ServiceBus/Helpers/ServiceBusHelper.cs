// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace Microsoft.CFS.Approvals.Messaging.Azure.ServiceBus.Helpers;

using Contracts.DataContracts;
using global::Azure.Messaging.ServiceBus;
using Interface;
using Newtonsoft.Json;

public class ServiceBusHelper : IServiceBusHelper
{
    /// <summary>
    /// Service Bus Sender
    /// </summary>
    private readonly ServiceBusSender _messageSender;

    /// <summary>
    /// Dictionary of Service Bus Senders
    /// </summary>
    private readonly Dictionary<string, ServiceBusSender> _messageSenderV2;

    private static readonly string _notificationTopic = "approvalsnotificationtopic";

    /// <summary>
    /// Constructor of ServiceBusHelper
    /// </summary>
    /// <param name="messageSender"></param>
    public ServiceBusHelper(ServiceBusSender messageSender)
    {
        _messageSender = messageSender;
    }

    /// <summary>
    /// Constructor of ServiceBusHelper
    /// </summary>
    /// <param name="serviceBusSenders"></param>
    public ServiceBusHelper(Dictionary<string, ServiceBusSender> serviceBusSenders)
    {
        _messageSenderV2 = serviceBusSenders;
    }

    /// <summary>
    /// Sends the Approval Request Expression to payload destination provided, after converting it into Brokered Message
    /// </summary>
    /// <param name="approvalRequestExpression"></param>
    /// <param name="messageId"></param>
    /// <param name="approvalRequestVersion"></param>
    /// <param name="topicName"></param>
    /// <param name="blobId"></param>
    /// <returns></returns>
    public async Task<bool> SendMessage(ApprovalRequestExpression approvalRequestExpression, string messageId, string approvalRequestVersion, string? topicName = null, string? blobId = null, int delay = 0)
    {
        if (approvalRequestExpression == null)
            throw new ArgumentNullException("approvalRequestExpression", "Approval Request Expression cannot be null");
        var brokeredMessage = await BuildServiceBusMessage(approvalRequestExpression, approvalRequestExpression.DocumentTypeId.ToString(), messageId, approvalRequestVersion, topicName, blobId, delay);
        await SendMessageToSerivceBus(brokeredMessage, topicName);
        return true;
    }

    /// <summary>
    /// Builds a service bus message using Approval Request Expression and assigns the service bus message id provided
    /// </summary>
    /// <param name="approvalRequestExpression"></param>
    /// <param name="applicationId"></param>
    /// <param name="messageId"></param>
    /// <param name="approvalRequestVersion"></param>
    /// <param name="topicName"></param>
    /// <param name="blobId"></param>
    /// <returns></returns>
    public async Task<ServiceBusMessage> BuildServiceBusMessage(ApprovalRequestExpression approvalRequestExpression, string applicationId, string messageId, string approvalRequestVersion, string? topicName, string? blobId, int delay = 0)
    {
        ServiceBusMessage message;
        if (string.Equals(topicName, _notificationTopic))
        {
            message = new ServiceBusMessage(blobId);
            message.ApplicationProperties["ApprovalNotificationDetails"] = true;
            message.ApplicationProperties["ApprovalNotificationRequestVersion"] = approvalRequestVersion;
            message.CorrelationId = messageId;

            if (delay > 0)
            {
                message.ScheduledEnqueueTime = DateTimeOffset.UtcNow.AddSeconds(delay);
            }
        }
        else
        {
            string messageBody = string.Format("{0}|{1}|{2}|{3}", applicationId, approvalRequestExpression.ApprovalIdentifier.DisplayDocumentNumber, messageId, approvalRequestExpression.Operation.ToString());

            // Create a BrokeredMessage of the customized class,
            // with ApplicationId property set to DocumentTypeId, and the same CorrelationID as the orginal BrokeredMessage
            message = new ServiceBusMessage(messageBody);
            message.MessageId = messageId;
            message.CorrelationId = Guid.NewGuid().ToString();

            // Adding properties to the Message
            message.ApplicationProperties["ApprovalRequestVersion"] = approvalRequestVersion;
            message.ApplicationProperties["ContentType"] = "ApprovalRequestExpression";
            message.ContentType = "application/json";
            message.Body = BinaryData.FromString(messageBody);
        }

        message.ApplicationProperties["ApplicationId"] = applicationId;
        DateTimeOffset now = DateTimeOffset.UtcNow;
        message.ApplicationProperties["CreatedDate"] = now.UtcDateTime;
        message.ApplicationProperties["CreatedEpoch"] = now.ToUnixTimeMilliseconds();

        return message;
    }

    /// <summary>
    /// Add a application property named Filter and then Send Message to Service Bus
    /// TO DO :: Further refactor this method to take all Application Properties as input as key value pair and add them to the message.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="message"></param>
    /// <param name="applicationId"></param>
    /// <param name="messageId"></param>
    /// <param name="correlationId"></param>
    /// <param name="queueOrTopicName"></param>
    /// <param name="filter"></param>
    /// <returns></returns>
    public async Task<bool> AddFilterAndSendMessage<T>(T message, string applicationId, string messageId, string correlationId, string? queueOrTopicName = null, string? filter = null) where T : class
    {
        ServiceBusMessage serviceBusMessage = null;

        if (string.IsNullOrWhiteSpace(messageId))
        {
            // Create a new ServiceBusMessage with the serialized message as the body
            serviceBusMessage = new ServiceBusMessage(JsonConvert.SerializeObject(message));
        }

        else
        {
            serviceBusMessage = new ServiceBusMessage(messageId);

            // Set the MessageId and CorrelationId property
            serviceBusMessage.MessageId = messageId;
            serviceBusMessage.CorrelationId = correlationId;
            serviceBusMessage.SessionId = correlationId;

            // Set the ApplicationId property (note: documentTypeId should be defined in the method or class scope)
            serviceBusMessage.ApplicationProperties["ApplicationId"] = applicationId;
        }
        // If a filter is provided, add it to the ApplicationProperties
        if (!string.IsNullOrEmpty(filter))
        {
            serviceBusMessage.ApplicationProperties["Filter"] = filter;
        }

        // Send the message to Service Bus using the appropriate sender
        await SendMessageToSerivceBus(serviceBusMessage, queueOrTopicName);

        // Return true to indicate the message was sent
        return true;
    }


    /// <summary>
    /// Sends the provided ServiceBusMessage to the specified queue or topic in the Service Bus.
    /// </summary>
    /// <param name="serviceBusMessage">The ServiceBusMessage to send.</param>
    /// <param name="queueOrTopicName">The name of the queue or topic to send the message to. If null, the default message sender will be used.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    public async Task SendMessageToSerivceBus(ServiceBusMessage serviceBusMessage, string? queueOrTopicName = null)
    {
        if (string.IsNullOrEmpty(queueOrTopicName))
            await _messageSender.SendMessageAsync(serviceBusMessage);
        else
        {
            if (_messageSenderV2.ContainsKey(queueOrTopicName))
            {
                await _messageSenderV2[queueOrTopicName].SendMessageAsync(serviceBusMessage);
            }
            else
            {
                throw new KeyNotFoundException($"The key '{queueOrTopicName}' was not found in the message senders.");
            }
        }
    }

    /// <summary>
    /// Sends the provided ServiceBusMessage to the specified queue or topic in the Service Bus at a scheduled time
    /// </summary>
    /// <param name="serviceBusMessage"></param>
    /// <param name="queueOrTopicName"></param>
    /// <returns></returns>
    /// <exception cref="KeyNotFoundException"></exception>
    public async Task<long> SendScheduledMessageToServiceBus(ServiceBusMessage serviceBusMessage, DateTimeOffset scheduledTime, string queueOrTopicName = null)
    {
        if (string.IsNullOrEmpty(queueOrTopicName))
            return await _messageSender.ScheduleMessageAsync(serviceBusMessage, scheduledTime);
        else
        {
            if (_messageSenderV2.ContainsKey(queueOrTopicName))
            {
                var sequenceNumber = await _messageSenderV2[queueOrTopicName].ScheduleMessageAsync(serviceBusMessage, scheduledTime);
                return sequenceNumber;
            }
            else
            {
                throw new KeyNotFoundException($"The key '{queueOrTopicName}' was not found in the message senders.");
            }
        }
    }

    /// <summary>
    /// Cancels a scheduled message in Azure Service Bus using the specified sequence number.
    /// </summary>
    /// <remarks>This method cancels a message that was previously scheduled for delayed delivery in Azure
    /// Service Bus.  If a specific queue or topic name is provided, the cancellation is performed using the
    /// corresponding  message sender. Otherwise, the default message sender is used.</remarks>
    /// <param name="sequenceNumber">The sequence number of the scheduled message to cancel.</param>
    /// <param name="queueOrTopicName">The name of the queue or topic where the message was scheduled. If <see langword="null"/> or empty,  the default
    /// message sender is used.</param>
    /// <returns>A task that represents the asynchronous operation.</returns>
    /// <exception cref="KeyNotFoundException">Thrown if <paramref name="queueOrTopicName"/> is not <see langword="null"/> or empty, but no corresponding 
    /// message sender is found.</exception>
    public async Task CancelScheduledMessageFromServiceBus(long sequenceNumber, string? queueOrTopicName = null)
    {
        if (string.IsNullOrEmpty(queueOrTopicName))
            await _messageSender.CancelScheduledMessageAsync(sequenceNumber);
        else
        {
            if (_messageSenderV2.ContainsKey(queueOrTopicName))
            {
                await _messageSenderV2[queueOrTopicName].CancelScheduledMessageAsync(sequenceNumber);
            }
            else
            {
                throw new KeyNotFoundException($"The key '{queueOrTopicName}' was not found in the message senders.");
            }
        }
    }
}