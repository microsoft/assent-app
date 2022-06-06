// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace Microsoft.CFS.Approvals.SyntheticTransaction.API.Controllers
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;
    using Microsoft.AspNetCore.Mvc;
    using Microsoft.AspNetCore.Mvc.Infrastructure;
    using Microsoft.CFS.Approvals.Data.Azure.Storage.Interface;
    using Microsoft.CFS.Approvals.Model;
    using Microsoft.CFS.Approvals.SyntheticTransaction.Common.Models;
    using Microsoft.CFS.Approvals.SyntheticTransaction.Helpers.ExtensionMethods;
    using Microsoft.CFS.Approvals.SyntheticTransaction.Helpers.Helpers;
    using Microsoft.CFS.Approvals.SyntheticTransaction.Helpers.Interface;
    using Newtonsoft.Json;
    using Newtonsoft.Json.Linq;

    /// <summary>
    /// The Actions Controller
    /// </summary>
    [Route("api/TestActions")]
    [ApiController]
    public class TestActionsController : ControllerBase
    {
        /// <summary>
        /// The azure storage  helper
        /// </summary>
        private readonly ITableHelper _azureStorageHelper;

        /// <summary>
        /// The bulk delete helper
        /// </summary>
        private readonly IBulkDeleteHelper _bulkDeleteHelper;

        private readonly string _environment;

        /// <summary>
        /// Constructor of TestActionsController
        /// </summary>
        /// <param name="azureStorageHelper"></param>
        /// <param name="bulkDeleteHelper"></param>
        /// <param name="configurationSetting"></param>
        /// <param name="actionContextAccessor"></param>
        public TestActionsController(Func<string, string, ITableHelper> azureStorageHelper,
            IBulkDeleteHelper bulkDeleteHelper,
            ConfigurationSetting configurationSetting,
            IActionContextAccessor actionContextAccessor)
        {
            _environment = actionContextAccessor?.ActionContext?.RouteData?.Values["env"]?.ToString();
            _azureStorageHelper = azureStorageHelper(
              configurationSetting.appSettings[_environment].StorageAccountName,
               configurationSetting.appSettings[_environment].StorageAccountKey);
            _bulkDeleteHelper = bulkDeleteHelper;
        }

        /// <summary>
        /// Generate approval response
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        [HttpPost]
        [Route("{env}")]
        public async Task<IActionResult> Post([FromBody] LOBRequest request)
        {
            var error = new ApprovalResponseErrorInfo();
            ApprovalResponse approvalResponse = null;
            var tenant = _azureStorageHelper.GetTableEntity<ApprovalTenantInfo>("ApprovalTenantInfo").Where(x => x.DocTypeId == request.DocumentTypeID).FirstOrDefault();
            var document = _azureStorageHelper.GetTableEntityListByPartitionKey<TestHarnessDocument>("TestHarnessPayload", request.ActionByAlias).Where(x => x.Status == DocumentStatus.Pending.ToString() && x.RowKey.Split(" | ")[0] == request.DocumentKeys.DisplayDocumentNumber).FirstOrDefault();
            var result = _bulkDeleteHelper.SendDeletePayload(document, request.ActionByAlias, request.ActionDetails.Comment, request.Action);
            var PayloadValidationResults = JsonConvert.DeserializeObject<JObject>(result.Content.ReadAsStringAsync().Result)?.SelectToken("PayloadProcessingResult")?.SelectToken("PayloadValidationResults")?.Value<JArray>();
            if (result.IsSuccessStatusCode && PayloadValidationResults == null)
            {
                document.Status = DocumentStatus.Approved.ToString();
                await _bulkDeleteHelper.UpdateDocumentStatus(document);
                approvalResponse = ApprovalResponseHelper.GenerateApprovalResponse(tenant?.RowKey, request.DocumentTypeID, error, TenantActionMessage.MsgActionSuccess.StringValue<TenantActionMessage>());
            }
            else
            {
                error.ErrorMessages = new List<string> { TenantActionMessage.MsgActionFailure.StringValue<TenantActionMessage>() };
                error.ErrorType = ApprovalResponseErrorType.UnintendedError;
                approvalResponse = ApprovalResponseHelper.GenerateApprovalResponse(tenant?.RowKey, request.DocumentTypeID, error, TenantActionMessage.MsgActionFailure.StringValue<TenantActionMessage>(), false);
            }
            return Ok(approvalResponse);
        }
    }
}