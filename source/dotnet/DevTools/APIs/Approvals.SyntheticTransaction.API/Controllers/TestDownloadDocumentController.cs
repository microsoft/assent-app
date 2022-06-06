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
    using Microsoft.CFS.Approvals.SyntheticTransaction.Common.Constant;
    using Microsoft.CFS.Approvals.SyntheticTransaction.Helpers.ExtensionMethods;
    using Microsoft.CFS.Approvals.SyntheticTransaction.Helpers.Helpers;
    using Newtonsoft.Json.Linq;

    /// <summary>
    /// The Download Document Controller
    /// </summary>
    [Route("api/TestDownloadDocument/{env}")]
    [ApiController]
    public class TestDownloadDocumentController : ControllerBase
    {
        private readonly IBlobStorageHelper _blobStorageHelper;
        private readonly string _environment;

        /// <summary>
        /// Constructor of TestDownloadDocumentController
        /// </summary>
        /// <param name="blobStorageHelper"></param>
        /// <param name="configurationSetting"></param>
        /// <param name="actionContextAccessor"></param>
        public TestDownloadDocumentController(
            Func<string, string, IBlobStorageHelper> blobStorageHelper,
            ConfigurationSetting configurationSetting,
            IActionContextAccessor actionContextAccessor)
        {
            _environment = actionContextAccessor?.ActionContext?.RouteData?.Values["env"]?.ToString();
            _blobStorageHelper = blobStorageHelper(
              configurationSetting.appSettings[_environment].StorageAccountName,
              configurationSetting.appSettings[_environment].StorageAccountKey);
        }

        /// <summary>
        /// Download document
        /// </summary>
        /// <param name="documentName"></param>
        /// <returns></returns>
        [HttpGet]
        [Route("{documentName}")]
        public async Task<IActionResult> Get(string documentName)
        {
            List<JObject> attachments = (Constants.AttachmentsJson).FromJson<List<JObject>>();
            var attachment = attachments.Where(s => s.SelectToken("ID")?.ToString() == documentName?.ToString()).FirstOrDefault();
            if (documentName == null && attachments == null && attachments.Count == 0 && attachment == null)
            {
                return BadRequest();
            }

            string blobNameFormat = "{0}|{1}";
            string blobName = string.Format(blobNameFormat, documentName, attachment.SelectToken("Name")?.ToString());

            return File(await _blobStorageHelper.DownloadByteArray("testharnessattachments", blobName), "application/octet-stream");
        }
    }
}