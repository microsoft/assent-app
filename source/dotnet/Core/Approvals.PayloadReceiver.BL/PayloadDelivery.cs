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
public class PayloadDelivery : IPayloadDelivery
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
    /// Sends the Approval Request Expression to Service Bus
    /// </summary>
    /// <param name="approvalRequestExpression"></param>
    /// <param name="payloadId"></param>
    /// <returns></returns>
    public async Task<bool> SendPayload(ApprovalRequestExpression approvalRequestExpression, Guid payloadId)
    {
        var approvalRequestVersion = _config[ConfigurationKey.ApprovalRequestVersion.ToString()].ToString();
        await UploadMessageToBlob(approvalRequestExpression, payloadId.ToString());
        return await _serviceBusHelper.SendMessage(approvalRequestExpression, payloadId.ToString(), approvalRequestVersion);
    }

    /// <summary>
    /// Uploads the Approval Request Expression to Blob, so the background process can pick it up for further processing
    /// </summary>
    /// <param name="approvalRequestExpression"></param>
    /// <param name="messageId"></param>
    /// <returns></returns>
    public async Task UploadMessageToBlob(ApprovalRequestExpression approvalRequestExpression, string messageId)
    {
        if (approvalRequestExpression == null)
            throw new ArgumentNullException("approvalRequestExpression", "Approval Request Expression cannot be null");

        string messageBody = string.Format("{0}|{1}|{2}", approvalRequestExpression.DocumentTypeId, approvalRequestExpression.ApprovalIdentifier.DisplayDocumentNumber, approvalRequestExpression.Operation.ToString());

        byte[] messageToUpload = ConvertToByteArray(approvalRequestExpression);

        await _blobStorageHelper.UploadByteArray(messageToUpload, Constants.PrimaryMessageContainer, messageBody);
        await _blobStorageHelper.UploadByteArray(messageToUpload, Constants.AuditAgentMessageContainer, messageBody);
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