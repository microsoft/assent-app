// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace Microsoft.CFS.Approvals.SyntheticTransaction.API.Controllers
{
    using System;
    using System.IO;
    using System.Threading.Tasks;
    using Microsoft.AspNetCore.Mvc;
    using Microsoft.AspNetCore.Mvc.Infrastructure;
    using Microsoft.Azure.Cosmos.Table;
    using Microsoft.CFS.Approvals.Data.Azure.Storage.Interface;
    using Microsoft.CFS.Approvals.SyntheticTransaction.Helpers.Helpers;

    /// <summary>
    /// The Attachment Controller class
    /// </summary>
    [Route("Attachment/{env}")]
    [ApiController]
    [Produces("text/html")]
    public class AttachmentController : ControllerBase
    {
        private readonly IBlobStorageHelper _blobStorageHelper;
        private readonly string _environment;

        /// <summary>
        /// Constructor of AttachmentController
        /// </summary>
        /// <param name="blobStorageHelper"></param>
        /// <param name="configurationSetting"></param>
        /// <param name="actionContextAccessor"></param>
        public AttachmentController(
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
        /// Get attachment
        /// </summary>
        /// <param name="id"></param>
        /// <param name="attachmentName"></param>
        /// <returns></returns>
        [HttpGet]
        [Route("{id}/{attachmentName}")]
        public async Task<IActionResult> Get(int id, string attachmentName)
        {
            if (string.IsNullOrWhiteSpace(attachmentName))
            {
                return BadRequest();
            }

            string blobNameFormat = "{0}|{1}";
            string blobName = string.Format(blobNameFormat, id, attachmentName);
            byte[] contentArray = await _blobStorageHelper.DownloadByteArray("testharnessattachments", blobName);

            switch (attachmentName.Split(".")[1].ToLower())
            {
                case "jpeg":
                case "jpg":
                    return File(contentArray, "image/jpeg");

                case "png":
                    return File(contentArray, "image/png");

                case "gif":
                    return File(contentArray, "image/gif");

                case "pdf":
                    return File(contentArray, "application/pdf");

                default:
                    return null;
            }
        }
    }
}