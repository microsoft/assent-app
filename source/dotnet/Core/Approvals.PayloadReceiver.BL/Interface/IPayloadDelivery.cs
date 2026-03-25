// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace Microsoft.CFS.Approvals.PayloadReceiver.BL.Interface;

using System;
using System.Threading.Tasks;
using global::Azure.Messaging.ServiceBus;
using Microsoft.CFS.Approvals.Contracts.DataContracts;
using Microsoft.CFS.Approvals.Model;

public interface IPayloadDelivery<T> where T : class
{
    /// <summary>
    /// Sends payload to payload destination provided
    /// </summary>
    /// <param name="payload"></param>
    /// <param name="applicationId"></param>
    /// <param name="messageId"></param>
    /// <param name="correlationId"></param>
    /// <returns></returns>
    Task<bool> SendPayload(T payload, string applicationId, string messageId, string correlationId);

    /// <summary>
    /// Uploads the payload to Blob, so the background process can pick it up for further processing
    /// </summary>
    /// <param name="payload"></param>
    /// <param name="messageId"></param>
    /// <param name="correlationId"></param>
    /// <returns></returns>
    Task UploadMessageToBlob(T payload, string messageId, string correlationId);

    /// <summary>
    /// Uploads the metadata to Blob
    /// </summary>
    /// <param name="payload"></param>
    /// <param name="blobName"></param>
    /// <param name="containerName"></param>
    /// <returns></returns>
    Task UploadMetaDataToBlob(string payload, string blobName, string containerName);
}