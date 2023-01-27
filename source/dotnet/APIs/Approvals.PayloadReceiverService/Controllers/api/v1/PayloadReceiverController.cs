// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace Microsoft.CFS.Approvals.PayloadReceiverService.Controllers.api.v1;

using System;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.CFS.Approvals.Contracts.DataContracts;
using Microsoft.CFS.Approvals.PayloadReceiver.BL.Interface;
using Swashbuckle.AspNetCore.Annotations;

/// <summary>
/// The PayloadReceiver controller class
/// </summary>
[Route("api/v1/[controller]")]
[ApiController]
public class PayloadReceiverController : ControllerBase
{
    /// <summary>
    /// The payload receiver manager.
    /// </summary>
    private readonly IPayloadReceiverManager _payloadReceiverManager = null;

    /// <summary>
    /// Initializes a new instance of the <see cref="PayloadReceiverController"/> class.
    /// </summary>
    /// <param name="payloadReceiverManager">The payload receiver manager.</param>
    public PayloadReceiverController(IPayloadReceiverManager payloadReceiverManager)
    {
        _payloadReceiverManager = payloadReceiverManager;
    }

    /// <summary>
    /// Main controller method which accepts a post action from tenants to send the ApprovalRequestExpression for processing
    /// This method de serializes and reconstructs the ARX, validates using Business Rules Validator, returns a GUID if successful or errors if validations fail and then sends the payload into destination (Topic, Service or Event Hubs etc.)
    /// TODO:: When server side validations are introduced, especially the alias validation logic which makes http calls to GraphAPI, introduce an async controller manager
    /// </summary>
    /// <param name="tenantId">Unique TenantId (GUID) specifying a particular Tenant for which the Payload is received</param>
    /// <returns>
    /// This method returns a JSON which contains the PayloadId and ValidationResults if any validation rule fails during processing;
    /// In case of success scenarios it returns ValidationResults as null
    /// </returns>
    /// <remarks>
    /// <para>
    /// e.g.
    /// HTTP POST api/v1/PayloadReceiver?TenantId=[DocumentTypeId]
    /// </para>
    /// </remarks>
    [SwaggerOperation(Tags = new[] { "Payload Receiver" })]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(PayloadProcessingResult))]
    [HttpPost]
    public async Task<IActionResult> Post(string tenantId)
    {
        try
        {
            string payload = string.Empty;
            using (StreamReader reader = new StreamReader(Request.Body, Encoding.UTF8))
            {
                payload = await reader.ReadToEndAsync();
            }
            return Ok(await _payloadReceiverManager.ManagePost(tenantId, payload));
        }
        catch (Exception ex)
        {
            return StatusCode(StatusCodes.Status500InternalServerError, ex);
        }
    }
}