// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace Microsoft.CFS.Approvals.Messaging.Azure.ServiceBus.Helpers;

using global::Azure.Messaging.ServiceBus;
using Interface;
using Contracts.DataContracts;

public class ServiceBusHelper : IServiceBusHelper
{
    /// <summary>
    /// Service Bus Sender
    /// </summary>
    private readonly ServiceBusSender _messageSender;

    /// <summary>
    /// Constructor of ServiceBusHelper
    /// </summary>
    /// <param name="messageSender"></param>
    public ServiceBusHelper(ServiceBusSender messageSender)
    {
        _messageSender = messageSender;
    }

    /// <summary>
    /// Sends the Approval Request Expression to payload destination provided, after converting it into Brokered Message
    /// </summary>
    /// <param name="approvalRequestExpression"></param>
    /// <param name="brokeredMessageId"></param>
    /// <param name="approvalRequestVersion"></param>
    /// <returns></returns>
    public async Task<bool> SendMessage(ApprovalRequestExpression approvalRequestExpression, string brokeredMessageId, string approvalRequestVersion)
    {
        var brokeredMessage = await BuildServiceBusMessage(approvalRequestExpression, brokeredMessageId, approvalRequestVersion);
        await _messageSender.SendMessageAsync(brokeredMessage);
        return true;
    }

    /// <summary>
    /// Builds a service bus message using Approval Request Expression and assigns the service bus message id provided
    /// </summary>
    /// <param name="approvalRequestExpression"></param>
    /// <param name="messageId"></param>
    /// <param name="approvalRequestVersion"></param>
    /// <returns></returns>
    public async Task<ServiceBusMessage> BuildServiceBusMessage(ApprovalRequestExpression approvalRequestExpression, string messageId, string approvalRequestVersion)
    {
        if (approvalRequestExpression == null)
            throw new ArgumentNullException("approvalRequestExpression", "Approval Request Expression cannot be null");

        string messageBody = string.Format("{0}|{1}|{2}", approvalRequestExpression.DocumentTypeId, approvalRequestExpression.ApprovalIdentifier.DisplayDocumentNumber, approvalRequestExpression.Operation.ToString());

        // Create a BrokeredMessage of the customized class,
        // with ApplicationId property set to DocumentTypeId, and the same CorrelationID as the orginal BrokeredMessage
        ServiceBusMessage message = new ServiceBusMessage(messageBody);
        message.MessageId = messageId;
        message.CorrelationId = Guid.NewGuid().ToString();

        // Adding properties to the Message
        message.ApplicationProperties["ApplicationId"] = approvalRequestExpression.DocumentTypeId.ToString();
        message.ApplicationProperties["ApprovalRequestVersion"] = approvalRequestVersion;
        message.ApplicationProperties["CreatedDate"] = DateTime.UtcNow;
        message.ApplicationProperties["ContentType"] = "ApprovalRequestExpression";
        message.ContentType = "application/json";
        message.Body = BinaryData.FromString(messageBody);

        return message;
    }
}