// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace Microsoft.CFS.Approvals.PayloadReceiver.BL;

using System;
using System.Threading.Tasks;
using Microsoft.CFS.Approvals.Messaging.Azure.ServiceBus.Interface;
using Microsoft.CFS.Approvals.Contracts;
using Microsoft.CFS.Approvals.Contracts.DataContracts;
using Microsoft.CFS.Approvals.Data.Azure.Storage.Interface;
using Microsoft.CFS.Approvals.PayloadReceiver.BL.Interface;
using Microsoft.Extensions.Configuration;

/// <summary>
/// The Payload Delivery class
/// </summary>
public class PayloadDelivery<T> : IPayloadDelivery<T> where T : class
{
    /// <summary>
    /// The blob storage helper
    /// </summary>
    private readonly IBlobStorageHelper _blobStorageHelper = null;

    /// <summary>
    /// The service bus helper
    /// </summary>
    private readonly IServiceBusHelper _serviceBusHelper = null;

    /// <summary>
    /// Configuration
    /// </summary>
    private readonly IConfiguration _config = null;

    /// <summary>
    /// Constructor of PayloadDelivery
    /// </summary>
    /// <param name="blobStorageHelper"></param>
    /// <param name="serviceBusHelper"></param>
    /// <param name="config"></param>
    public PayloadDelivery(IBlobStorageHelper blobStorageHelper, IServiceBusHelper serviceBusHelper, IConfiguration config)
    {
        _blobStorageHelper = blobStorageHelper;
        _serviceBusHelper = serviceBusHelper;
        _config = config;
    }

    /// <summary>
    /// Sends the payload to Service Bus
    /// </summary>
    /// <param name="payload"></param>
    /// <param name="applicationId"></param>
    /// <param name="messageId"></param>
    /// <param name="correlationId"></param>
    /// <returns></returns>
    public async Task<bool> SendPayload(T payload, string applicationId, string messageId, string correlationId)
    {
        await UploadMessageToBlob(payload, messageId, correlationId);
        // Use generic method if available, otherwise cast to ApprovalRequestExpression
        if (payload is ApprovalRequestExpression arx)
        {
            var approvalRequestVersion = _config["ApprovalRequestVersion"];
            return await _serviceBusHelper.SendMessage(arx, messageId, approvalRequestVersion);
        }
        else
        {
            return await _serviceBusHelper.AddFilterAndSendMessage(payload, applicationId, messageId, correlationId, null, null);
        }
    }

    /// <summary>
    /// Uploads the payload to Blob, so the background process can pick it up for further processing
    /// </summary>
    /// <param name="payload"></param>
    /// <param name="messageId"></param>
    /// <param name="correlationId"></param>
    /// <returns></returns>
    public async Task UploadMessageToBlob(T payload, string messageId, string correlationId)
    {
        if (payload == null)
            throw new ArgumentNullException(nameof(payload), "Payload cannot be null");

        string messageBody = messageId;
        // If T is ApprovalRequestExpression, use its properties for messageBody
        if (payload is ApprovalRequestExpression arx)
        {
            messageBody = string.Format("{0}|{1}|{2}|{3}",
                arx.DocumentTypeId,
                arx.ApprovalIdentifier?.DisplayDocumentNumber,
                messageId,
                arx.Operation.ToString());
        }

        byte[] messageToUpload = ConvertToByteArray(payload);

        await _blobStorageHelper.UploadByteArray(messageToUpload, Constants.PrimaryMessageContainer, messageBody);
        await _blobStorageHelper.UploadByteArray(messageToUpload, Constants.AuditAgentMessageContainer, messageBody);
    }

    /// <summary>
    /// Uploads the metadata to Blob
    /// </summary>
    /// <param name="payload"></param>
    /// <param name="blobName"></param>
    /// <param name="containerName"></param>
    /// <returns></returns>
    /// <exception cref="ArgumentNullException"></exception>
    public async Task UploadMetaDataToBlob(string payload, string blobName, string containerName)
    {
        if (payload == null)
            throw new ArgumentNullException(nameof(payload), "Payload cannot be null");

        await _blobStorageHelper.UploadText(payload, containerName, blobName);
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