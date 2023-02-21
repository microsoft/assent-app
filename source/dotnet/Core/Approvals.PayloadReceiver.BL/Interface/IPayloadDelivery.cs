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
    /// <param name="payloadId"></param>
    /// <returns></returns>
    Task<bool> SendPayload(ApprovalRequestExpression approvalRequestExpression, Guid payloadId);

    /// <summary>
    /// Uploads the Approval Request Expression to Blob, so the background process can pick it up for further processing
    /// </summary>
    /// <param name="approvalRequestExpression"></param>
    /// <param name="messageId"></param>
    /// <returns></returns>
    Task UploadMessageToBlob(ApprovalRequestExpression approvalRequestExpression, string messageId);
}