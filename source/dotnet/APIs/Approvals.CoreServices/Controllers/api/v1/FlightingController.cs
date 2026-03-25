// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace Microsoft.CFS.Approvals.CoreServices.Controllers.api.v1;

using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.CFS.Approvals.Contracts;
using Microsoft.CFS.Approvals.Core.BL.Interface;
using Swashbuckle.AspNetCore.Annotations;

/// <summary>
/// FlightingController
/// </summary>
/// <seealso cref="BaseApiController" />
public class FlightingController : BaseApiController
{
    /// <summary>
    /// The approval tenant info helper
    /// </summary>
    private readonly IFlightingHelper _flightingHelper = null;

    /// <summary>
    /// Initializes a new instance of the <see cref="FlightingController"/> class.
    /// </summary>
    /// <param name="flightingHelper"></param>
    public FlightingController(IFlightingHelper flightingHelper)
    {
        _flightingHelper = flightingHelper;
    }

    /// <summary>
    /// HTTP GET api/v1/user/flightingFeatures
    /// </summary>
    /// <returns>Http action result</returns>
    [Route("/api/v1/user/flightingFeatures/")]
    [SwaggerOperation(Tags = new[] { "User" })]
    [HttpGet]
    public async Task<IActionResult> GetFlightingFeatures(string sessionId = "")
    {
        try
        {
            var results = _flightingHelper.GetFlightingFeatures(SignedInUser, OnBehalfUser, GetTokenOrCookie(), ClientDevice, sessionId, MessageId, Xcv);
            if (results != null)
                return Ok(results);
            else
                return BadRequest(Constants.FlightingAPIErrorMessage);
        }
        catch
        {
            return BadRequest(Constants.FlightingAPIErrorMessage);
        }
    }

    /// <summary>
    /// HTTP GET api/v1/metadata/flightingFeatures
    /// </summary>
    /// <returns>Http action result</returns>
    [Route("/api/v1/metadata/flightingFeatures/")]
    [SwaggerOperation(Tags = new[] { "Metadata" })]
    [HttpGet]
    public async Task<IActionResult> GetAllFlightingFeatures(string sessionId = "")
    {
        try
        {
            var results = await _flightingHelper.GetAllFlightingFeatures(SignedInUser, OnBehalfUser, GetTokenOrCookie(), ClientDevice, sessionId, MessageId, Xcv);
            if (results != null)
                return Ok(results);
            else
                return BadRequest(Constants.FlightingAPIErrorMessage);
        }
        catch
        {
            return BadRequest(Constants.FlightingAPIErrorMessage);
        }
    }
}