// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace Microsoft.CFS.Approvals.CoreServices.Controllers.api.v1
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Text;
    using System.Threading.Tasks;
    using Microsoft.AspNetCore.Mvc;
    using Microsoft.CFS.Approvals.Common.DL.Interface;
    using Microsoft.CFS.Approvals.Contracts;
    using Microsoft.CFS.Approvals.Contracts.DataContracts;
    using Microsoft.CFS.Approvals.Core.BL.Interface;
    using Microsoft.CFS.Approvals.Extensions;
    using Microsoft.CFS.Approvals.LogManager.Model;
    using Microsoft.CFS.Approvals.Utilities.Helpers;
    using Newtonsoft.Json.Linq;
    using Swashbuckle.AspNetCore.Annotations;

    /// <summary>
    /// Class DocumentActionController.
    /// </summary>
    /// <seealso cref="BaseApiController" />
    [Route("api/v1/[controller]/{tenantId}")]
    public class DocumentActionController : BaseApiController
    {
        /// <summary>
        /// The Approval Tenant Info Helper
        /// </summary>
        private readonly IApprovalTenantInfoHelper _approvalTenantInfoHelper;

        /// <summary>
        /// The Client Action Helper
        /// </summary>
        private readonly IClientActionHelper _clientActionHelper;

        /// <summary>
        /// The Document Action Helper Delegate
        /// </summary>
        private readonly Func<string, IDocumentActionHelper> _documentActionHelperDelegate;

        /// <summary>
        /// The performance logger
        /// </summary>
        private readonly IPerformanceLogger _performanceLogger = null;

        /// <summary>
        /// Constructor of DocumentActionController
        /// </summary>
        /// <param name="approvalTenantInfoHelper"></param>
        /// <param name="clientActionHelper"></param>
        /// <param name="documentActionHelperDelegate"></param>
        /// <param name="performanceLogger"></param>
        public DocumentActionController(
            IApprovalTenantInfoHelper approvalTenantInfoHelper,
            IClientActionHelper clientActionHelper,
            Func<string, IDocumentActionHelper> documentActionHelperDelegate,
            IPerformanceLogger performanceLogger)
        {
            _approvalTenantInfoHelper = approvalTenantInfoHelper;
            _clientActionHelper = clientActionHelper;
            _documentActionHelperDelegate = documentActionHelperDelegate;
            _performanceLogger = performanceLogger;
        }

        /// <summary>
        /// Sends action of a given approval request whether it's an approval, rejection, etc...
        /// The user is subsequently notified once the action is processed and completed through the notification center.
        /// The ActionString is a part of the HttpRequest's Content, which contains details regarding the type of action taken;
        /// the request or DocumentNumber on which action needs to be taken; the comments added by the approver; and additional information (if any)
        /// </summary>
        /// <param name="approvalRequest">The Approval Request object</param>
        /// <param name="tenantId">The Unique TenantId (Int32) for which the action needs to be performed for the given request or documentNumber</param>
        /// <param name="sessionId">GUID SessionId. Unique for each user session</param>
        /// <param name="clientDevice">Client Device</param>
        /// <returns>
        /// This method returns an OK HttpResponse if the action is successful;
        /// else return a Bad Response along with the Error Message specifying the reason for failure
        /// A tracking GUID is also sent in the response content for failure scenarios which can help in failure log analysis
        /// </returns>
        /// <remarks>
        /// <para>
        /// e.g.
        /// HTTP POST api/DocumentAction/[tenantId]
        /// </para>
        /// </remarks>
        [SwaggerOperation(Tags = new[] { "Action" })]
        [HttpPost]
        public async Task<IActionResult> Post(int tenantId, string sessionId = "", string clientDevice = "")
        {
            #region Logging

            var logData = new Dictionary<LogDataKey, object>
            {
                { LogDataKey.StartDateTime, DateTime.UtcNow },
                { LogDataKey.ReceivedTcv, Tcv },
                { LogDataKey.SessionId, sessionId },
                { LogDataKey.UserRoleName, LoggedInAlias },
                { LogDataKey.TenantId, tenantId },
                { LogDataKey.UserAlias, Alias },
                { LogDataKey.ClientDevice, clientDevice },
                { LogDataKey.DisplayDocumentNumber, string.Empty },
                { LogDataKey.DocumentNumber, string.Empty }
            };

            #endregion Logging

            try
            {
                ArgumentGuard.NotNull(tenantId, nameof(tenantId));

                var tenantInfo = _approvalTenantInfoHelper.GetTenantInfo(tenantId);
                var submissionType = (ActionSubmissionType)tenantInfo.ActionSubmissionType;
                var documentActionHelper = _documentActionHelperDelegate(submissionType.ToString());

                if (string.IsNullOrWhiteSpace(clientDevice))
                {
                    clientDevice = Host;
                }

                if (!string.IsNullOrEmpty(clientDevice) && (clientDevice.Equals(Constants.OutlookClient) || clientDevice.Equals(Constants.TeamsClient)))
                {
                    using (_performanceLogger.StartPerformanceLogger("PerfLog", clientDevice, string.Format(Constants.PerfLogAction, "DocumentActionController", "Post Action From Non WebClient"), logData))
                    {
                        var result = await _clientActionHelper.TakeActionFromNonWebClient(tenantId, Request, clientDevice, Alias, LoggedInAlias, GetTokenOrCookie(), submissionType, Xcv, Tcv, sessionId);
                        logData.Modify(LogDataKey.EndDateTime, DateTime.UtcNow);
                        return result;
                    }
                }
                else
                {
                    using (_performanceLogger.StartPerformanceLogger("PerfLog", clientDevice, string.Format(Constants.PerfLogAction, "DocumentActionController", "Post Action From Non WebClient"), logData))
                    {
                        string content;
                        using (var reader = new StreamReader(Request.Body, Encoding.UTF8, true, 1024, true))
                        {
                            content = await reader.ReadToEndAsync();
                        }
                        var result = Ok(await documentActionHelper.TakeAction(tenantId, content, clientDevice, Alias, LoggedInAlias, GetTokenOrCookie(), Xcv, Tcv, sessionId));
                        logData.Modify(LogDataKey.EndDateTime, DateTime.UtcNow);
                        return result;
                    }
                }
            }
            catch (Exception exception)
            {
                return BadRequest(exception.InnerException != null ? exception.InnerException.Message : exception.Message);
            }
        }

        /// <summary>
        /// Sends action of a given approval request whether it's an approval, rejection, etc...
        /// The user is subsequently notified once the action is processed and completed through the notification center.
        /// The ActionString is a part of the HttpRequest's Content, which contains details regarding the type of action taken;
        /// the request or DocumentNumber on which action needs to be taken; the comments added by the approver; and additional information (if any)
        /// </summary>
        /// <param name="tenantId">The Unique TenantId (Int32) for which the action needs to be performed for the given request or documentNumber</param>
        /// <param name="actionString">ActionString is Json Content, which contains details regarding the type of action</param>
        /// <param name="sessionId">GUID SessionId. Unique for each user session</param>
        /// <param name="clientDevice">Client Device</param>
        /// <returns>
        /// This method returns an OK HttpResponse if the action is successful;
        /// else return a Bad Response along with the Error Message specifying the reason for failure
        /// A tracking GUID is also sent in the response content for failure scenarios which can help in failure log analysis
        /// </returns>
        /// <remarks>
        /// <para>
        /// e.g.
        /// HTTP POST api/DocumentAction/[tenantId]
        /// </para>
        /// </remarks>
        [SwaggerOperation(Tags = new[] { "Action" })]
        [HttpPost(template: "WithActionString")]
        public async Task<IActionResult> Post(int tenantId, [FromBody] JObject actionString, string sessionId = "", string clientDevice = "")
        {
            try
            {
                Request.Body = new MemoryStream(Encoding.UTF8.GetBytes(actionString?.ToString()));
                ArgumentGuard.NotNull(tenantId, nameof(tenantId));
                var tenantInfo = _approvalTenantInfoHelper.GetTenantInfo(tenantId);
                var submissionType = (ActionSubmissionType)tenantInfo.ActionSubmissionType;
                var documentActionHelper = _documentActionHelperDelegate(submissionType.ToString());

                if (string.IsNullOrWhiteSpace(clientDevice))
                {
                    clientDevice = Host;
                }
                if (!string.IsNullOrEmpty(clientDevice) && clientDevice.Equals(Constants.OutlookClient) || clientDevice.Equals(Constants.TeamsClient))
                {
                    return await _clientActionHelper.TakeActionFromNonWebClient(tenantId, Request, clientDevice, Alias, LoggedInAlias, GetTokenOrCookie(), submissionType, Xcv, Tcv, sessionId);
                }
                else
                {
                    return Ok(await documentActionHelper.TakeAction(tenantId, actionString?.ToString(), clientDevice, Alias, LoggedInAlias, GetTokenOrCookie(), Xcv, Tcv, sessionId));
                }
            }
            catch (Exception exception)
            {
                return BadRequest(exception.InnerException != null ? exception.InnerException.Message : exception.Message);
            }
        }
    }
}