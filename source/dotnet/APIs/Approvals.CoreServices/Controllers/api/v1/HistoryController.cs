// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace Microsoft.CFS.Approvals.CoreServices.Controllers.api.v1;

using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.CFS.Approvals.CoreServices.BL.Interface;
using Microsoft.CFS.Approvals.Utilities.Helpers;
using Swashbuckle.AspNetCore.Annotations;

/// <summary>
/// Class HistoryController.
/// </summary>
/// <seealso cref="BaseApiController" />
public class HistoryController : BaseApiController
{
    /// <summary>
    /// The approval history helper.
    /// </summary>
    private readonly IApprovalHistoryHelper _approvalHistoryHelper = null;

    /// <summary>
    /// Initializes a new instance of the <see cref="HistoryController"/> class.
    /// </summary>
    /// <param name="approvalHistoryHelper">The approval history helper.</param>
    public HistoryController(IApprovalHistoryHelper approvalHistoryHelper)
    {
        _approvalHistoryHelper = approvalHistoryHelper;
    }

    /// <summary>
    /// Gets the history data from mobile service data table for a particular user.
    /// </summary>
    /// <param name="page">Page number for which data needs to be fetched.</param>
    /// <param name="sortColumn">Column on which sorting will be performed.</param>
    /// <param name="sortDirection">Mentions ascending or descending order for sorting.</param>
    /// <param name="searchCriteria">Criteria on which search needs to be done.</param>
    /// <param name="timePeriod">Time period for which records needs to be fetched.</param>
    /// <param name="sessionId">GUID SessionId. Unique for each user session</param>
    /// <param name="tenantId">TenantId. Unique for each Tenant</param>
    /// <returns>
    /// Paginated set of approvals history data filtered by search criteria for the given user, along with an array of all the historical data.
    /// </returns>
    /// <remarks>
    /// <para>
    /// e.g.
    /// HTTP GET api/History?[QueryString]
    /// </para>
    /// </remarks>
    [SwaggerOperation(Tags = new[] { "History" })]
    [HttpGet]
    public async Task<IActionResult> Get(
        int page = 1,
        string sortColumn = "ActionDate",
        string sortDirection = "DESC",
        string searchCriteria = "",
        int timePeriod = 0,
        string sessionId = "",
        string tenantId = "")
    {
        try
        {
            var historyData = await _approvalHistoryHelper.GetHistory(page, sortColumn, sortDirection, searchCriteria, timePeriod, sessionId, LoggedInAlias, Alias, Host, Xcv, Tcv, tenantId);
            return Ok(historyData);
        }
        catch (Exception ex)
        {
            return BadRequest(ex.Message);
        }
    }

    /// <summary>
    /// Creates an Excel Spreadsheet for the historical data.
    /// </summary>
    /// <param name="monthsOfData">time period for which records needs to be fetched.</param>
    /// <param name="searchCriteria"> criteria on which search needs to be done.</param>
    /// <param name="sortField">column on which sorting will be performed.</param>
    /// <param name="sortDirection">mentions ascending or descending order for sorting.</param>
    /// <param name="sessionId">GUID SessionId. Unique for each user session</param>
    /// <param name="tenantId">TenantId. Unique for each Tenant</param>
    /// <returns>
    /// This method returns a spreadsheet file which contains the historical data for the given user based on the specified search criteria and duration (in a CSV format).
    /// </returns>
    /// <remarks>
    /// <para>
    /// e.g.
    /// HTTP GET api/History?[QueryString]
    /// </para>
    /// </remarks>
    [SwaggerOperation(Tags = new[] { "History" })]
    [HttpGet(template: "download")]
    public async Task<IActionResult> GetHistoryInExcel(
        int monthsOfData,
        string searchCriteria,
        string sortField,
        string sortDirection,
        string sessionId = "",
        string tenantId = "")
    {
        try
        {
            ArgumentGuard.NotNull(monthsOfData, nameof(monthsOfData));
            ArgumentGuard.NotNullAndEmpty(sortField, nameof(sortField));
            ArgumentGuard.NotNullAndEmpty(sortDirection, nameof(sortDirection));
            return File(await _approvalHistoryHelper.DownloadHistoryDataInExcel(sortField, sortDirection, searchCriteria, monthsOfData, sessionId, LoggedInAlias, Alias, Host, Xcv, Tcv, tenantId), "application/octet-stream");
        }
        catch (Exception ex)
        {
            return BadRequest(ex);
        }
    }
}