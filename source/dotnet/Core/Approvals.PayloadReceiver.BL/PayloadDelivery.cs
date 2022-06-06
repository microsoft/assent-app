// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace Microsoft.CFS.Approvals.PayloadReceiver.BL
{
    using System;
    using System.Threading.Tasks;
    using Microsoft.Azure.ServiceBus;
    using Microsoft.Azure.ServiceBus.Core;
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
            MessageSender messageSender = EstablishServiceBusChannel(payloadDestinationInfo);
            var brokeredMessage = await BuildServiceBusMessage(approvalRequestExpression, brokeredMessageId);
            await messageSender.SendAsync(brokeredMessage);
            return true;
        }

        /// <summary>
        /// Service Bus Channel establishes delivery channel to Service Bus INstance
        /// </summary>
        /// <param name="payloadDestinationInfo"></param>
        /// <returns></returns>
        public MessageSender EstablishServiceBusChannel(Model.PayloadDestinationInfo payloadDestinationInfo)
        {
            var serviceBusConnectionString = "Endpoint=sb://" + payloadDestinationInfo.Namespace + ".servicebus.windows.net/;SharedAccessKeyName=" + payloadDestinationInfo.AcsIdentity + ";SharedAccessKey=" + payloadDestinationInfo.SecretKey;
            MessageSender messageSender = new MessageSender(serviceBusConnectionString, payloadDestinationInfo.Entity);

            return messageSender;
        }

        /// <summary>
        /// Builds a service bus message using Approval Request Expression and assigns the service bus message id provided
        /// </summary>
        /// <param name="approvalRequestExpression"></param>
        /// <param name="messageId"></param>
        /// <returns></returns>
        public async Task<Message> BuildServiceBusMessage(ApprovalRequestExpression approvalRequestExpression, string messageId)
        {
            if (approvalRequestExpression == null)
                throw new ArgumentNullException("approvalRequestExpression", "Approval Request Expression cannot be null");

            string messageBody = string.Format("{0}|{1}|{2}", approvalRequestExpression.DocumentTypeId, approvalRequestExpression.ApprovalIdentifier.DisplayDocumentNumber, approvalRequestExpression.Operation.ToString());

            byte[] messageToUpload = ConvertToByteArray(approvalRequestExpression);
            if (await _blobStorageHelper.DoesExist(Constants.PrimaryMessageContainer, messageBody))
            {
                await _blobStorageHelper.DeleteBlob(Constants.PrimaryMessageContainer, messageBody);
            }
            await _blobStorageHelper.UploadByteArray(messageToUpload, Constants.PrimaryMessageContainer, messageBody);

            if (await _blobStorageHelper.DoesExist(Constants.AuditAgentMessageContainer, messageBody))
            {
                await _blobStorageHelper.DeleteBlob(Constants.AuditAgentMessageContainer, messageBody);
            }
            await _blobStorageHelper.UploadByteArray(messageToUpload, Constants.AuditAgentMessageContainer, messageBody);

            // Create a BrokeredMessage of the customized class,
            // with ApplicationId property set to DocumentTypeId, and the same CorrelationID as the orginal BrokeredMessage
            Message message = new Message();
            message.MessageId = messageId;
            message.CorrelationId = Guid.NewGuid().ToString();

            // Adding properties to the Message
            message.UserProperties["ApplicationId"] = approvalRequestExpression.DocumentTypeId.ToString();
            message.UserProperties["ApprovalRequestVersion"] = _config[ConfigurationKey.ApprovalRequestVersion.ToString()].ToString();
            message.UserProperties["CreatedDate"] = DateTime.UtcNow;
            message.UserProperties["ContentType"] = "ApprovalRequestExpression";
            message.ContentType = "application/json";
            message.Body = System.Text.Encoding.UTF8.GetBytes(messageBody);

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
}