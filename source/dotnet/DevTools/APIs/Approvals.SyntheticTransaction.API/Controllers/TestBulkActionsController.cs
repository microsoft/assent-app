// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace Microsoft.CFS.Approvals.SyntheticTransaction.API.Controllers
{
    using System;
    using System.Linq;
    using System.Threading.Tasks;
    using Microsoft.AspNetCore.Mvc;
    using Microsoft.AspNetCore.Mvc.Infrastructure;
    using Microsoft.Azure.Cosmos.Table;
    using Microsoft.CFS.Approvals.Data.Azure.Storage.Interface;
    using Microsoft.CFS.Approvals.Model;
    using Microsoft.CFS.Approvals.SyntheticTransaction.Common.Models;
    using Microsoft.CFS.Approvals.SyntheticTransaction.Helpers.ExtensionMethods;
    using Microsoft.CFS.Approvals.SyntheticTransaction.Helpers.Helpers;

    /// <summary>
    /// The Bulk Action Controller
    /// </summary>
    [Route("{env}/api/TestBulkActions")]
    [ApiController]
    public class TestBulkActionsController : ControllerBase
    {
        /// <summary>
        /// The azure storage helper
        /// </summary>
        private readonly ITableHelper _azureStorageHelper;

        private readonly string _environment;

        /// <summary>
        /// Constructor of TestBulkActionsController
        /// </summary>
        /// <param name="azureStorageHelper"></param>
        /// <param name="actionContextAccessor"></param>
        /// <param name="configurationSetting"></param>
        public TestBulkActionsController(Func<string, string, ITableHelper> azureStorageHelper,
            IActionContextAccessor actionContextAccessor,
            ConfigurationSetting configurationSetting)
        {
            _environment = actionContextAccessor?.ActionContext?.RouteData?.Values["env"]?.ToString();
            _azureStorageHelper = azureStorageHelper(
                configurationSetting.appSettings[_environment].StorageAccountName,
                configurationSetting.appSettings[_environment].StorageAccountKey);
        }

        /// <summary>
        /// Generate approval response
        /// </summary>
        /// <param name="requests"></param>
        /// <returns></returns>
        [HttpPost]
        public async Task<IActionResult> Post([FromBody] LOBRequest[] requests)
        {
            var errors = new ApprovalResponseErrorInfo();
            var tenantEntity = (_azureStorageHelper.GetTableEntity<ApprovalTenantInfo>("ApprovalTenantInfo")).Where(x => x.DocTypeId == requests[0].DocumentTypeID).FirstOrDefault();
            var approvalResponse = ApprovalResponseHelper.GenerateApprovalResponse(tenantEntity?.AppName, requests[0].DocumentTypeID, errors, TenantActionMessage.MsgActionSuccess.StringValue<TenantActionMessage>());
            return Ok(approvalResponse);
        }
    }
}