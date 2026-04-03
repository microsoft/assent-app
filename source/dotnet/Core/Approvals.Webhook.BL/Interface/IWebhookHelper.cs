// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace Approvals.Webhook.BL.Interface;

using System.Threading.Tasks;

/// <summary>
/// Interface for Webhook Helper.
/// </summary>
public interface IWebhookHelper
{
    /// <summary>
    /// Validates the request header to ensure it contains the expected handshake code.
    /// </summary>
    /// <param name="receivedHandshakeCode">The received handshake code.</param>
    /// <param name="expectedHandshakeCode">The expected handshake code value.</param>
    /// <returns>True if valid, otherwise false.</returns>
    bool ValidateRequestHeader(string receivedHandshakeCode, string expectedHandshakeCode);

    /// <summary>
    /// Sends the Event payload to Service Bus using IPayloadDelivery.
    /// </summary>
    /// <param name="requestBody">The raw JSON string representing the Event payload.</param>
    /// <param name="applicationId">The application identifier for routing the payload.</param>
    /// <param name="messageId">The unique identifier for the payload message.</param>
    /// <returns>True if the payload was sent successfully; otherwise, false.</returns>
    Task<bool> SendPayloadToServiceBusAsync(string requestBody, string applicationId, string messageId);


}
