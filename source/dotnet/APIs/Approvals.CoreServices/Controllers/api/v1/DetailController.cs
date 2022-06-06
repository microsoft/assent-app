// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace Microsoft.CFS.Approvals.CoreServices.Controllers.api.v1
{
    using System;
    using System.Threading.Tasks;
    using Microsoft.AspNetCore.Mvc;
    using Microsoft.CFS.Approvals.Contracts;
    using Microsoft.CFS.Approvals.Core.BL.Interface;
    using Microsoft.CFS.Approvals.Extensions;
    using Microsoft.CFS.Approvals.Utilities.Helpers;
    using Newtonsoft.Json.Linq;
    using Swashbuckle.AspNetCore.Annotations;

    /// <summary>
    /// the Details Controller
    /// </summary>
    /// <seealso cref="BaseApiController" />
    [Route("api/v1/[controller]/{tenantId?}/{documentNumber?}/{operation?}", Name = "GetDetails")]
    public class DetailController : BaseApiController
    {
        /// <summary>
        /// The Details Helper
        /// </summary>
        private readonly IDetailsHelper _detailsHelper;

        /// <summary>
        /// Constructor for Details Helper.
        /// </summary>
        /// <param name="detailsHelper"></param>
        public DetailController(IDetailsHelper detailsHelper)
        {
            _detailsHelper = detailsHelper;
        }

        /// <summary>
        /// Retrieves details of approval item from tenant once a user clicks into the summary tile on the initial page.
        /// </summary>
        /// <param name="tenantId">Indicates the Tenant for which the request's (documentNumber) data needs to be retrieved</param>
        /// <param name="documentNumber">The request number for which data needs to be fetched</param>
        /// <param name="operation">The operation type (authsum/DTL/HDR/LINE) etc. This operation specifies what type of data will be retrieved</param>
        /// <param name="fiscalYear">Fiscal Year of Request number (if any)</param>
        /// <param name="page"></param>
        /// <param name="sessionId">GUID SessionId. Unique for each user session</param>
        /// <param name="tcv">GUID Transaction Id. Unique for each transaction</param>
        /// <param name="xcv">GUID Cross Correlation Vector. Unique for each request across system boundaries</param>
        /// <param name="callType">Data Call Type which can be either Summary or Details OR All</param>
        /// <param name="pageType">Page from where API is called eg. Detail, History</param>
        /// <param name="source">Source for Details call eg. Summary, Notification</param>
        /// <returns>
        /// This method returns the request's details data from storage
        /// if all data (Header/Additional Data/Line Items data) are present in storage along with authsum (Summary) data
        /// else only the summary data along with the list of Callback URLs for missing data.
        /// Per the information regarding a single approval, the details for that approval request are returned.
        /// The details can include attachments and other options depending on the tenant, which means details can vary per certain tenant's requirements.
        /// </returns>
        /// <remarks>
        /// <para>
        /// e.g.
        /// Gets all data for the documentNumber (includes summary/header/line data as well)
        /// HTTP GET api/Detail/[tenantId]/[documentNumber]
        /// </para>
        /// <para>
        /// e.g.
        /// Gets only specific data for the documentNumber (either header/line etc.).
        /// This is called when there are some missing details for that documentNumber and these additional missing details needs to be fetched from the LOB system
        /// HTTP GET api/Detail/[tenantMapId]/[documentNumber]/[operation]
        /// </para>
        /// </remarks>
        [SwaggerOperation(Tags = new[] { "Details" })]
        [HttpGet]
        public async Task<IActionResult> Get(
            int tenantId,
            string documentNumber,
            string operation = "authsum",
            string fiscalYear = "",
            int page = 1,
            string sessionId = "",
            string tcv = "",
            string xcv = "",
            string callType = "All",
            string pageType = "Detail",
            string source = "PendingApproval")
        {
            try
            {
                ArgumentGuard.NotNull(tenantId, nameof(tenantId));
                ArgumentGuard.NotNull(documentNumber, nameof(documentNumber));

                DataCallType dataCallType = (DataCallType)Enum.Parse(typeof(DataCallType), callType, true);

                JObject responseObject = await _detailsHelper.GetDetails(
                    tenantId,
                    documentNumber,
                    operation,
                    fiscalYear,
                    page,
                    sessionId,
                    tcv,
                    xcv,
                    Alias,
                    LoggedInAlias,
                    Host,
                    GetTokenOrCookie(),
                    false,
                    (int)dataCallType,
                    pageType,
                    source);
                return Ok(responseObject);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }
    }
}