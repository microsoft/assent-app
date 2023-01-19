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
    /// <param name="loggedInAlias">The logged in alias.</param>
    /// <param name="sessionId">The session identifier.</param>
    /// <returns>returns list</returns>
    [SwaggerOperation(Tags = new[] { "User" })]
    [HttpGet]
    [Route("{loggedInAlias}")]
    public async Task<IEnumerable<dynamic>> Get(string loggedInAlias,string sessionId = "")
    {
        try
        {
            ArgumentGuard.NotNullAndEmpty(loggedInAlias, nameof(loggedInAlias));

            var results = await _delegationHelper.GetInfoOfPeopleDelegatedToMe(loggedInAlias, Alias, Host, sessionId, Xcv, Tcv);
            return results;
        }
        catch
        {
            return null;
        }
    }

    /// <summary>
    /// This method will retrieve delegated users for the loggedInAlias
    /// </summary>
    /// <param name="tenantId">The tenantId</param>
    /// <param name="loggedInAlias">The logged in alias</param>
    /// <param name="sessionId">The session Id</param>
    /// <returns>returns HttpResponse Object</returns>
    /// <remarks>
    /// <para>
    /// e.g.
    /// HTTP GET api/UserDelegationSettings/[tenantId]/loggedInAlias]?SessionId=[userSessionId]
    /// </para>
    /// </remarks>
    [SwaggerOperation(Tags = new[] { "User" })]
    [HttpGet]
    [Route("{tenantId}/{loggedInAlias}")]
    public async Task<IActionResult> Get(int tenantId, string loggedInAlias,string sessionId = "")
    {
        try
        {
            ArgumentGuard.NotNull(tenantId, nameof(tenantId));
            ArgumentGuard.NotNullAndEmpty(loggedInAlias, nameof(loggedInAlias));

            var responseObject = await _delegationHelper.GetUsersDelegatedToAsync(loggedInAlias, Alias, tenantId, Host, sessionId, Xcv, Tcv);
            return Ok(responseObject);
        }
        catch
        {
            return BadRequest(Constants.UserDelegationGetError);
        }
    }
}