// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace Microsoft.CFS.Approvals.CoreServices.Controllers.api.v1
{
    using System;
    using System.Linq;
    using System.Threading.Tasks;
    using Microsoft.AspNetCore.Mvc;
    using Microsoft.CFS.Approvals.Core.BL.Interface;
    using Microsoft.CFS.Approvals.Extensions;
    using Microsoft.CFS.Approvals.Utilities.Helpers;
    using Newtonsoft.Json.Linq;
    using Swashbuckle.AspNetCore.Annotations;

    /// <summary>
    /// Class SummaryCountController.
    /// </summary>
    /// <seealso cref="BaseApiController" />
    public class SummaryCountController : BaseApiController
    {
        /// <summary>
        /// The summary helper
        /// </summary>
        private readonly ISummaryHelper _summaryHelper = null;

        /// <summary>
        /// The approval tenant information helper/
        /// </summary>
        private readonly IApprovalTenantInfoHelper _approvalTenantInfoHelper = null;

        /// <summary>
        /// Initializes a new instance of the <see cref="SummaryCountController"/> class.
        /// </summary>
        /// <param name="summaryHelper">The summary helper.</param>
        /// <param name="approvalTenantInfoHelper">The approval tenant information helper.</param>
        public SummaryCountController(ISummaryHelper summaryHelper, IApprovalTenantInfoHelper approvalTenantInfoHelper)
        {
            _summaryHelper = summaryHelper;
            _approvalTenantInfoHelper = approvalTenantInfoHelper;
        }

        /// <summary>
        /// Get summary count for the current user from azure table storage.
        /// </summary>
        /// <param name="sessionId">GUID SessionId. Unique for each user session</param>
        /// <returns>
        /// This method returns a JSON specifying the total number of request pending approval grouped by tenant.
        /// </returns>
        /// <remarks>
        /// <para>
        /// e.g.
        /// HTTP GET api/SummaryCount
        /// </para>
        /// </remarks>
        [SwaggerOperation(Tags = new[] { "Summary" })]
        [HttpGet]
        public async Task<IActionResult> Get(string sessionId = "")
        {
            try
            {
                var approvalsData = await _summaryHelper.GetSummaryCountData(string.Empty, LoggedInAlias, Alias, sessionId, Host);
                return Ok(approvalsData);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        /// <summary>
        /// Get summary count for the current user and specified tenant from azure table storage given Document Type ID.
        /// </summary>
        /// <param name="tenantDocTypeID">GUID DocumentTypeId of the Tenant</param>
        /// <param name="sessionId">GUID SessionId</param>
        /// <returns>
        /// This method returns a JSON specifying the total number of request pending approval for the given user and tenant combination.
        /// </returns>
        /// <remarks>
        /// <para>
        /// e.g.
        /// HTTP GET api/SummaryCount?tenantDocTypeId=[tenantDocTypeID]
        /// </para>
        /// </remarks>
        [SwaggerOperation(Tags = new[] { "Summary" })]
        [HttpGet(template: "GetSummaryById")]
        public async Task<IActionResult> Get(string tenantDocTypeID, string sessionId = "")
        {
            try
            {
                ArgumentGuard.NotNullAndEmpty(tenantDocTypeID, nameof(tenantDocTypeID));

                // Get Tenant Info by DocTypeID
                var tenants = (await _approvalTenantInfoHelper.GetTenants(false)).Where(x => x.DocTypeId.Equals(Guid.Parse(tenantDocTypeID).ToString())).ToList();

                var approvalsCount = await _summaryHelper.GetSummaryCountData(tenantDocTypeID, LoggedInAlias, Alias, sessionId, Host);
                var count = "0";
                if (approvalsCount != null && approvalsCount.Any())
                {
                    count = approvalsCount.FirstOrDefault()["Count"].ToString();
                }

                var pendingRequestObject = JObject.FromObject(new { ApproverAlias = Alias, Count = count, ID = tenantDocTypeID, TenantName = tenants.FirstOrDefault().AppName });
                return Ok(pendingRequestObject);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }
    }
}