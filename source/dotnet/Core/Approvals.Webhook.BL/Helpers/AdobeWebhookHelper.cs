// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace Approvals.Webhook.BL.Helpers;

using Approvals.Webhook.BL.Interface;
using Microsoft.CFS.Approvals.Extensions;
using Microsoft.CFS.Approvals.LogManager.Model;
using Microsoft.CFS.Approvals.LogManager.Provider.Interface;
using Microsoft.CFS.Approvals.Model;
using Microsoft.CFS.Approvals.PayloadReceiver.BL.Interface;
using Newtonsoft.Json;

public class AdobeWebhookHelper : IWebhookHelper
{
    /// <summary>
    /// Dependency for sending payloads to Service Bus 
    /// </summary>
    private readonly IPayloadDelivery<AdobeSignEvent> _payloadDelivery;

    /// <summary>
    /// Dependency for logging
    /// </summary>
    private readonly ILogProvider _logProvider;

    /// <summary>
    /// Constructor injects dependencies
    /// </summary>
    /// <param name="logProvider"></param>
    /// <param name="payloadDelivery"></param>
    public AdobeWebhookHelper(
        ILogProvider logProvider,
        IPayloadDelivery<AdobeSignEvent> payloadDelivery)
    {
        _logProvider = logProvider;
        _payloadDelivery = payloadDelivery;
    }

    /// <summary>
    /// Validates the request header to ensure it contains the expected handshake code.
    /// </summary>
    /// <param name="receivedHandshakeCode">The handshake code received from the request header.</param>
    /// <param name="expectedHandshakeCode">The handshake code expected/configured.</param>
    /// <returns>True if the handshake code is valid, otherwise false.</returns>
    public bool ValidateRequestHeader(string receivedHandshakeCode, string expectedHandshakeCode)
    {
        // Prepare log data for diagnostics
        var logData = new Dictionary<string, object>
        {
            { "ReceivedHandshakeCode", receivedHandshakeCode },
            { "ExpectedHandshakeCode", expectedHandshakeCode }
        };

        // Check for null/empty or mismatch
        if (string.IsNullOrEmpty(receivedHandshakeCode) || string.IsNullOrEmpty(expectedHandshakeCode) || receivedHandshakeCode != expectedHandshakeCode)
        {
            logData.Add(LogDataKey.PayloadValidationResult.ToString(), $"Unauthorized: Missing Required Header or Code ({receivedHandshakeCode}) invalid");
            // Log error for invalid handshake code
            _logProvider.LogError<TrackingEvent, LogDataKey>(TrackingEvent.PayloadValidationFailure, new Exception(TrackingEvent.PayloadValidationFailure.ToString()), null, logData);
            return false;
        }
        return true;
    }

    /// <summary>
    /// Sends the Event payload to Service Bus using IPayloadDelivery.
    /// </summary>
    /// <param name="requestBody">The raw JSON string representing the Event payload.</param>
    /// <param name="applicationId">The application identifier for routing the payload.</param>
    /// <param name="messageId">The unique identifier for the payload message.</param>
    /// <returns>True if the payload was sent successfully; otherwise, false.</returns>
    public async Task<bool> SendPayloadToServiceBusAsync(string requestBody, string applicationId, string messageId)
    {
        // Prepare log data for diagnostics
        var logData = new Dictionary<LogDataKey, object>
        {
            { LogDataKey.DocumentTypeId, applicationId },
            { LogDataKey.Xcv, messageId },
            { LogDataKey.MessageId, messageId }
        };

        string adobeMetaDataContainerName = "adobemetadata";
        await _payloadDelivery.UploadMetaDataToBlob(requestBody, messageId, adobeMetaDataContainerName);
        AdobeSignEvent? adobeEventPayload;

        try
        {
            // Validate request body is not empty
            if (string.IsNullOrWhiteSpace(requestBody)) throw new InvalidDataException("An empty payload was sent");

            // Deserialize the JSON payload to AdobeSignEvent
            adobeEventPayload = JsonConvert.DeserializeObject<AdobeSignEvent>(requestBody);
            if (adobeEventPayload == null)
            {
                throw new InvalidOperationException("Failed to deserialize payload.");
            }
        }
        catch (Exception ex)
        {
            // Log deserialization failure
            logData.Add(LogDataKey.ErrorMessage, ex.Message);
            _logProvider.LogError<TrackingEvent, LogDataKey>(TrackingEvent.PayloadValidationFailure, ex, logData, null);
            return false;
        }
        try
        {
            // Update log data with payload details
            logData.Modify(LogDataKey.Xcv, adobeEventPayload.Agreement.Id);

            // Send payload to Service Bus using IPayloadDelivery
            bool sent = await _payloadDelivery.SendPayload(adobeEventPayload, applicationId, messageId, adobeEventPayload.Agreement.Id);
            if (!sent)
            {
                // Log if sending failed
                throw new InvalidOperationException("Unable to send payload to Service Bus.");
            }
            // Return result of sending
            return sent;
        }
        catch (Exception ex)
        {
            // Log any error during Service Bus send
            logData.Add(LogDataKey.ErrorMessage, ex.Message);
            _logProvider.LogError<TrackingEvent, LogDataKey>(TrackingEvent.PayloadProcessingFailure, ex, logData, null);
            return false;
        }
    }
}
