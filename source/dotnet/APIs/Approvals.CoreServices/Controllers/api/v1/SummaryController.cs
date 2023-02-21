// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace Microsoft.CFS.Approvals.CoreServices.Controllers.api.v1;

using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.CFS.Approvals.Core.BL.Interface;
using Swashbuckle.AspNetCore.Annotations;

/// <summary>
/// Class SummaryController.
/// </summary>
/// <seealso cref="BaseApiController" />
public class SummaryController : BaseApiController
{
    /// <summary>
    /// The summary helper/
    /// </summary>
    private readonly ISummaryHelper _summaryHelper = null;

    /// <summary>
    /// Initializes a new instance of the <see cref="SummaryController"/> class.
    /// </summary>
    /// <param name="summaryHelper">The summary helper.</param>
    public SummaryController(ISummaryHelper summaryHelper)
    {
        _summaryHelper = summaryHelper;
    }

    /// <summary>
    /// Get summary for the current user sorted/ filtered by tenant from azure table storage given Document Type ID (optional).
    /// </summary>
    /// <param name="Id">GUID DocumentTypeId of the Tenant</param>
    /// <param name="SessionId">GUID SessionId. Unique for each user session</param>
    /// <returns>
    /// This method returns a JSON for all the Pending approval requests for the given user and Tenant combination.
    /// This contains only summary data (i.e. no details) to be displayed on the home page.
    /// </returns>
    /// <remarks>
    /// <para>
    /// e.g.
    /// HTTP GET api/v1/Summary?Id=[tenantDocTypeID]
    /// </para>
    /// </remarks>
    [SwaggerOperation(Tags = new[] { "Summary" })]
    [HttpGet]
    public async Task<IActionResult> Get(String Id, string SessionId = "")
    {
        try
        {
            return Ok(await _summaryHelper.GetSummary(LoggedInAlias, Alias, Host, SessionId, Id));
        }
        catch (Exception ex)
        {
            return BadRequest(ex.Message);
        }
    }
}