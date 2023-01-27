// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace Microsoft.CFS.Approvals.CoreServices.Controllers.api.v1;

using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.CFS.Approvals.Contracts;
using Microsoft.CFS.Approvals.Core.BL.Interface;
using Microsoft.CFS.Approvals.Utilities.Helpers;
using Swashbuckle.AspNetCore.Annotations;

/// <summary>
/// Class AutoRefreshController.
/// </summary>
/// <seealso cref="BaseApiController" />
[Route("api/v1/[controller]/{tenantId?}")]
public class AutoRefreshController : BaseApiController
{
    /// <summary>
    /// The Approval Tenant Info Helper.
    /// </summary>
    private readonly IApprovalTenantInfoHelper _approvalTenantInfoHelper;

    /// <summary>
    /// The Client Action Helper.
    /// </summary>
    private readonly IClientActionHelper _clientActionHelper;

    /// <summary>
    /// Constructor of AutoRefreshController
    /// </summary>
    /// <param name="approvalTenantInfoHelper"></param>
    /// <param name="clientActionHelper"></param>
    public AutoRefreshController(IApprovalTenantInfoHelper approvalTenantInfoHelper,
        IClientActionHelper clientActionHelper)
    {
        _approvalTenantInfoHelper = approvalTenantInfoHelper;
        _clientActionHelper = clientActionHelper;
    }

    /// <summary>
    /// This method checks the status of the Current Document
    /// </summary>
    /// <param name="tenantId">Tenant ID</param>
    /// <param name="sessionId">Session ID</param>
    /// <returns>Response that may contain the actionable card if the record is no longer pending with Logged in User</returns>
    [SwaggerOperation(Tags = new[] { "Details" })]
    [HttpPost]
    public async Task<IActionResult> Post(int tenantId, string sessionId = "")
    {
        try
        {
            ArgumentGuard.NotNull(tenantId, nameof(tenantId));

            var tenantInfo = _approvalTenantInfoHelper.GetTenantInfo(tenantId);
            var submissionType = (ActionSubmissionType)tenantInfo.ActionSubmissionType;

            return await _clientActionHelper.ClientAutoRefresh(tenantId, Request, Constants.OutlookClient, Alias, LoggedInAlias, Tcv, sessionId, Xcv);
        }
        catch (Exception exception)
        {
            return BadRequest(exception.InnerException != null ? exception.InnerException.Message : exception.Message);
        }
    }
}