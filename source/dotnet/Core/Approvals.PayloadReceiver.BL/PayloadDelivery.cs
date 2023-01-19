// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace Microsoft.CFS.Approvals.PayloadReceiver.BL;

using System;
using System.Threading.Tasks;
using global::Azure.Identity;
using global::Azure.Messaging.ServiceBus;
using Microsoft.CFS.Approvals.Contracts;
using Microsoft.CFS.Approvals.Contracts.DataContracts;
using Microsoft.CFS.Approvals.Data.Azure.Storage.Interface;
using Microsoft.CFS.Approvals.PayloadReceiver.BL.Interface;
using Microsoft.Extensions.Configuration;

/// <summary>
/// The Payload Delivery class
/// </summary>
public class PayloadDelivery : IPayloadDelivery
{
    /// <summary>
    /// The blob storage helper
    /// </summary>
    private readonly IBlobStorageHelper _blobStorageHelper = null;

    /// <summary>
    /// Configuration
    /// </summary>
    private readonly IConfiguration _config = null;

    /// <summary>
    /// Constructor of PayloadDelivery
    /// </summary>
    /// <param name="blobStorageHelper"></param>
    /// <param name="config"></param>
    public PayloadDelivery(IBlobStorageHelper blobStorageHelper, IConfiguration config)
    {
        _blobStorageHelper = blobStorageHelper;
        _config = config;
    }

    /// <summary>
    /// Sends the Approval Request Expression converting it into Brokered Message
    /// </summary>
    /// <param name="approvalRequestExpression"></param>
    /// <param name="payloadDestinationInfo"></param>
    /// <param name="payloadId"></param>
    /// <returns></returns>
    public async Task<bool> SendPayload(ApprovalRequestExpression approvalRequestExpression, Model.PayloadDestinationInfo payloadDestinationInfo, Guid payloadId)
    {
        string brokeredMessageId = payloadId.ToString();
        ServiceBusSender messageSender = EstablishServiceBusChannel(payloadDestinationInfo);
        var brokeredMessage = await BuildServiceBusMessage(approvalRequestExpression, brokeredMessageId);
        await messageSender.SendMessageAsync(brokeredMessage);
        return true;
    }

    /// <summary>
    /// Service Bus Channel establishes delivery channel to Service Bus INstance
    /// </summary>
    /// <param name="payloadDestinationInfo"></param>
    /// <returns></returns>
    public ServiceBusSender EstablishServiceBusChannel(Model.PayloadDestinationInfo payloadDestinationInfo)
    {
        var endpoint = payloadDestinationInfo.Namespace + ".servicebus.windows.net";
        ServiceBusClient client = new ServiceBusClient(endpoint, new DefaultAzureCredential());
        return client.CreateSender(payloadDestinationInfo.Entity);
    }

    /// <summary>
    /// Builds a service bus message using Approval Request Expression and assigns the service bus message id provided
    /// </summary>
    /// <param name="approvalRequestExpression"></param>
    /// <param name="messageId"></param>
    /// <returns></returns>
    public async Task<ServiceBusMessage> BuildServiceBusMessage(ApprovalRequestExpression approvalRequestExpression, string messageId)
    {
        if (approvalRequestExpression == null)
            throw new ArgumentNullException("approvalRequestExpression", "Approval Request Expression cannot be null");

        string messageBody = string.Format("{0}|{1}|{2}", approvalRequestExpression.DocumentTypeId, approvalRequestExpression.ApprovalIdentifier.DisplayDocumentNumber, approvalRequestExpression.Operation.ToString());

        byte[] messageToUpload = ConvertToByteArray(approvalRequestExpression);

        await _blobStorageHelper.UploadByteArray(messageToUpload, Constants.PrimaryMessageContainer, messageBody);
        await _blobStorageHelper.UploadByteArray(messageToUpload, Constants.AuditAgentMessageContainer, messageBody);

        // Create a BrokeredMessage of the customized class,
        // with ApplicationId property set to DocumentTypeId, and the same CorrelationID as the orginal BrokeredMessage
        ServiceBusMessage message = new ServiceBusMessage(messageBody);
        message.MessageId = messageId;
        message.CorrelationId = Guid.NewGuid().ToString();

        // Adding properties to the Message
        message.ApplicationProperties["ApplicationId"] = approvalRequestExpression.DocumentTypeId.ToString();
        message.ApplicationProperties["ApprovalRequestVersion"] = _config[ConfigurationKey.ApprovalRequestVersion.ToString()].ToString();
        message.ApplicationProperties["CreatedDate"] = DateTime.UtcNow;
        message.ApplicationProperties["ContentType"] = "ApprovalRequestExpression";
        message.ContentType = "application/json";
        message.Body = BinaryData.FromString(messageBody);

        return message;
    }

    /// <summary>
    /// Convert to byte array
    /// </summary>
    /// <param name="ard"></param>
    /// <returns></returns>
    private byte[] ConvertToByteArray(object ard)
    {
        return System.Text.Json.JsonSerializer.SerializeToUtf8Bytes(ard);
    }
}