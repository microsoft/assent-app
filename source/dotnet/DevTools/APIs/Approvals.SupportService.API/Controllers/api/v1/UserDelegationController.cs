// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace Microsoft.CFS.Approvals.SupportService.API.Controllers;
using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using Microsoft.CFS.Approvals.DevTools.Model.Models;
using Microsoft.CFS.Approvals.SupportServices.Helper.Interface;
using Microsoft.CFS.Approvals.SupportService.API.Filters;
using Microsoft.CFS.Approvals.SupportServices.Helper.Interface;
using Microsoft.CFS.Approvals.Utilities.Interface;
using Newtonsoft.Json.Linq;

/// <summary>
/// The User Delegation Controller
/// </summary>
[Route("api/v1/UserDelegation")]
[ApiController]
[TypeFilter(typeof(AuthorizationFilter))]
public class UserDelegationController : ControllerBase
{
    /// <summary>
    /// The user delegation helper
    /// </summary>
    private readonly IUserDelegationHelper _userDelegationHelper;

    /// <summary>
    /// The name resolution helper
    /// </summary>
    private readonly INameResolutionHelper _nameResolutionHelper;

    /// <summary>
    /// Constructor of UserDelegationController
    /// </summary>
    /// <param name="userDelegationHelper"></param>
    /// <param name="nameResolutionHelper"></param>
    public UserDelegationController(IUserDelegationHelper userDelegationHelper,
        Func<string, INameResolutionHelper> nameResolutionHelper,
        IActionContextAccessor actionContextAccessor)
    {
        _userDelegationHelper = userDelegationHelper;
        _nameResolutionHelper = nameResolutionHelper(actionContextAccessor?.ActionContext?.RouteData?.Values["env"]?.ToString());
    }

    /// <summary>
    /// Get user delegations
    /// </summary>
    /// <returns></returns>
    [HttpGet]
    [Route("{env}")]
    public async Task<IActionResult> Get()
    {
        try
        {
            var result = await _userDelegationHelper.GetUserDelegations();
            return Ok(result);
        }
        catch (Exception ex)
        {
            return BadRequest(ex);
        }
    }

    /// <summary>
    /// Delete user delegations
    /// </summary>
    /// <param name="id"></param>
    /// <returns></returns>
    [HttpDelete]
    [Route("{env}")]
    public IActionResult Delete(string id)
    {
        try
        {
            var delegation = _userDelegationHelper.GetDelegation(id);
            _userDelegationHelper.DeleteUserDelegations(delegation);
            return Ok();
        }
        catch (Exception ex)
        {
            return BadRequest(ex);
        }
    }

    /// <summary>
    /// Add user delegations
    /// </summary>
    /// <param name="receivedData"></param>
    /// <returns></returns>
    [HttpPost]
    [Route("{env}")]
    public async Task<IActionResult> Post(JObject receivedData)
    {
        string message = String.Empty;
        try
        {
            JToken data = receivedData.SelectToken("data");

            if (DateTime.Parse(data["startDate"].ToString()).ToUniversalTime().Date < DateTime.UtcNow.Date.AddDays(-1) || DateTime.Parse(data["endDate"].ToString()).ToUniversalTime().Date < DateTime.UtcNow.Date || DateTime.Parse(data["startDate"].ToString()).ToUniversalTime().Date > DateTime.Parse(data["endDate"].ToString()).ToUniversalTime().Date)
            {
                message = "Dates should be greater than today's Date. End Date should be equal to or greater than Start Date";
            }
            else
            {
                if (!_userDelegationHelper.IsDelegationExist(data["managerAlias"]?.ToString(), data["delegatedTo"]?.ToString(), 0))
                {
                    var delegation = new UserDelegationEntity
                    {
                        ManagerAlias = data["managerAlias"]?.ToString().ToLower(),
                        DelegatedToAlias = data["delegatedTo"]?.ToString().ToLower(),
                        DateFrom = DateTime.Parse(data["startDate"]?.ToString()).ToUniversalTime(),
                        DateTo = DateTime.Parse(data["endDate"]?.ToString()).ToUniversalTime(),
                        AccessType = data["delegationAccess"].ToString() == "ReadOnly" ? 1 : 0,
                        IsHidden = true,
                        TenantId = 0,
                        PartitionKey = data["managerAlias"]?.ToString().ToLower(),
                        RowKey = Guid.NewGuid().ToString()
                    };

                    if (await _nameResolutionHelper.IsValidUser(delegation.ManagerAlias) && await _nameResolutionHelper.IsValidUser(delegation.DelegatedToAlias))
                    {
                        try
                        {
                            await _userDelegationHelper.InsertUserDelegation(delegation);
                        }
                        catch (Exception)
                        {
                                throw new InvalidOperationException("User either exists or the entered field is empty. Please correct the data!");
                        }
                    }
                    else
                    {
                        message = "Input alias not valid.";
                    }
                }
                else
                {
                    message = "User delegation settings already exist for delegate! Please try with different delegate";
                }
            }
            return Ok(message);
        }
        catch (Exception ex)
        {
            return BadRequest(ex.Message);
        }
    }
}