// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace Microsoft.CFS.Approvals.CoreServices.Controllers.api.v1;

using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.CFS.Approvals.Contracts;
using Microsoft.CFS.Approvals.Core.BL.Interface;
using Microsoft.CFS.Approvals.Extensions;
using Microsoft.CFS.Approvals.Utilities.Helpers;
using Swashbuckle.AspNetCore.Annotations;

/// <summary>
/// the UserDelegationSettingsController class.
/// </summary>
/// <seealso cref="BaseApiController" />
public class UserDelegationSettingsController : BaseApiController
{
    /// <summary>
    /// The delegation helper.
    /// </summary>
    private readonly IDelegationHelper _delegationHelper = null;

    /// <summary>
    /// Initializes a new instance of the <see cref="UserDelegationSettingsController"/> class.
    /// </summary>
    /// <param name="delegationHelper">The delegation helper.</param>
    public UserDelegationSettingsController(IDelegationHelper delegationHelper)
    {
        _delegationHelper = delegationHelper;
    }

    /// <summary>
    /// Gets the specified logged in alias.
    /// </summary>
    /// <param name="sessionId">The session identifier.</param>
    /// <returns>returns list</returns>
    [SwaggerOperation(Tags = new[] { "User" })]
    [HttpGet]
    [Route("/api/v1/me/delegations")]
    public async Task<IEnumerable<dynamic>> Get(string sessionId = "")
    {
        try
        {
            var results = await _delegationHelper.GetMergedDelegationData(SignedInUser, OnBehalfUser, GetTokenOrCookie(), ClientDevice, sessionId, Xcv, MessageId);
            return results;
        }
        catch
        {
            return null;
        }
    }

    /// <summary>
    /// This method retrieves delegated users for the specified userPrincipalName or objectId.
    /// </summary>
    /// <param name="tenantId">tenantId</param>
    /// <param name="sessionId">The session Id</param>
    /// <returns>Returns HttpResponse Object</returns>
    /// <remarks>
    /// <para>
    /// e.g.
    /// HTTP GET api/v1/users/delegations?tenantId=[tenantId]
    /// </para>
    /// </remarks>
    [SwaggerOperation(Tags = new[] { "User" })]
    [HttpGet]
    [Route("/api/v1/users/delegations")]
    public async Task<IActionResult> Get(int tenantId, string sessionId = "")
    {
        try
        {
            ArgumentGuard.NotNull(tenantId, nameof(tenantId));
            var responseObject = await _delegationHelper.GetUsersDelegatedToAsync(SignedInUser, OnBehalfUser, tenantId, ClientDevice, sessionId, Xcv, MessageId);
            return Ok(responseObject);
        }
        catch
        {
            return BadRequest(Constants.UserDelegationGetError);
        }
    }
}
