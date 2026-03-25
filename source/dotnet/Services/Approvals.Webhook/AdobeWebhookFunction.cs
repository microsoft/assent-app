namespace Microsoft.Approvals.WebhookAzFunction;

using global::Approvals.Webhook.BL.Interface;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System;
using System.IO;
using System.Text;
using System.Threading.Tasks;

public class AdobeWebhookFunction
{
    // Configuration for handshake code and other settings
    private readonly IConfiguration _config;
    // Helper for validating requests and sending payloads
    private readonly IWebhookHelper _adobeWebhookHelper;

    /// <summary>
    /// Constructor for dependency injection of configuration and webhook helper.
    /// </summary>
    public AdobeWebhookFunction(IConfiguration config, IWebhookHelper adobeWebhookHelper)
    {
        _config = config;
        _adobeWebhookHelper = adobeWebhookHelper;
    }

    /// <summary>
    /// Azure Function entry point for Adobe webhook.
    /// Handles GET and POST requests.
    /// </summary>
    [FunctionName("AdobeWebhookFunction")]
    public async Task<IActionResult> Run(
        [HttpTrigger(AuthorizationLevel.Function, "get", "post", Route = null)] HttpRequest req)
    {
        // Generate a unique tracking ID for this request
        string messageId = Guid.NewGuid().ToString();

        // Retrieve handshake code name, handshake code value and other configurations
        string expectedHandshakeCodeName = _config["AdobeHandshakeCodeName"];
        string expectedHandshakeCode = _config["AdobeHandshakeCodeValue"];
        string applicationId = _config["AdobeApplicationId"];

        // Extract handshake code from request header
        string receivedHandshakeCode = req.Headers[expectedHandshakeCodeName];

        // Validate the handshake code using the helper
        if (!_adobeWebhookHelper.ValidateRequestHeader(receivedHandshakeCode, expectedHandshakeCode))
        {
            // Return 401 Unauthorized if validation fails
            return new UnauthorizedResult();
        }
        else
        {
            // Read the request body (payload) as a string
            string requestBody = string.Empty;
            using (var reader = new StreamReader(req.Body, Encoding.UTF8, true, 1024, true))
            {
                requestBody = await reader.ReadToEndAsync();
            }
            if (req.Method.Equals("get", StringComparison.InvariantCultureIgnoreCase))
            {
                req.HttpContext.Response.Headers.Append(expectedHandshakeCodeName, receivedHandshakeCode);
                return new OkObjectResult(new { Message = "OK", PayloadId = messageId });
            }
            // Send the payload to Service Bus using the helper
            if (await _adobeWebhookHelper.SendPayloadToServiceBusAsync(requestBody, applicationId, messageId))
            {
                // Add handshake code to response header for tracking
                req.HttpContext.Response.Headers.Append(expectedHandshakeCodeName, receivedHandshakeCode);
                // Return 200 OK with tracking information
                return new OkObjectResult(new { Message = "Payload processed. Please use the tracking ID for further inquiries. Tracking Id: " + messageId, PayloadId = messageId });
            }
            else
            {
                // Return 400 Bad Request if payload processing fails
                return new BadRequestObjectResult("Payload processing failed. Please contact Approvals Engineering Team. Tracking Id: " + messageId.ToString());
            }
        }
    }
}