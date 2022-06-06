// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace Microsoft.CFS.Approvals.CoreServices.Controllers.api.v1
{
    using System;
    using System.IO;
    using System.Threading.Tasks;
    using Microsoft.AspNetCore.Mvc;
    using Microsoft.CFS.Approvals.Core.BL.Interface;
    using Microsoft.CFS.Approvals.Utilities.Helpers;
    using Swashbuckle.AspNetCore.Annotations;

    /// <summary>
    /// the ReadDetailsController class
    /// </summary>
    /// <seealso cref="BaseApiController" />
    [Route("api/v1/[controller]/{tenantId}")]
    public class ReadDetailsController : BaseApiController
    {
        /// <summary>
        /// The read details helper
        /// </summary>
        private readonly IReadDetailsHelper _readDetailsHelper = null;

        /// <summary>
        /// Initializes a new instance of the <see cref="ReadDetailsController"/> class.
        /// </summary>
        /// <param name="readDetailsHelper">The read details helper.</param>
        public ReadDetailsController(IReadDetailsHelper readDetailsHelper)
        {
            _readDetailsHelper = readDetailsHelper;
        }

        /// <summary>
        /// Updates row in ReadDetails Table to indicate that details of a particular approval were successfully loaded, and therefore have been read.
        /// Entries from this controller stored in the table include PartitionKey, RowKey (alias), TimeStamp, and JSONData.
        /// JSON Data includes ActionDate, User who took action (alias), and the action (Read Details). JSON Data also stores the document key relative
        /// to the approval as well as which client the details were loaded on.
        /// </summary>
        /// <param name="tenantId">The Unique TenantId (Int32) for which the action needs to be performed for the given request or documentNumber</param>
        /// <param name="sessionId">GUID SessionId. Unique for each user session</param>
        /// <param name="tcv">GUID Transaction Id. Unique for each transaction</param>
        /// <param name="xcv">GUID Cross Correlation Vector. Unique for each request across system boundaries</param>
        /// <returns>
        /// This method returns an OK HttpResponse if the action is successful;
        /// else return a Bad Response along with the Error Message specifying the reason for failure
        /// A tracking GUID is also sent in the reponse content for failure scenarios which can help in failure log analysis
        /// </returns>
        /// <remarks>
        /// <para>
        /// e.g.
        /// HTTP POST api/ReadDetails/[tenantId]
        /// </para>
        /// </remarks>
        [SwaggerOperation(Tags = new[] { "Details" })]
        [HttpPost]
        public async Task<IActionResult> Post(int tenantId, string sessionId = "", string tcv = "", string xcv = "")
        {
            try
            {
                ArgumentGuard.NotNull(tenantId, nameof(tenantId));

                string requestContent;
                using (var reader = new StreamReader(Request.Body))
                {
                    requestContent = await reader.ReadToEndAsync();
                }
                return Ok(_readDetailsHelper.UpdateIsReadDetails(requestContent, tenantId, LoggedInAlias, Alias, Host, sessionId, tcv, xcv));
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }
    }
}