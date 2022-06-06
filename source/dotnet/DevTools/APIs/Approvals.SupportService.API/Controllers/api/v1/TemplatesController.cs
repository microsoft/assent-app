// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace Microsoft.CFS.Approvals.SupportService.API.Controllers.api.v1
{
    using System;
    using System.Collections.Generic;
    using System.Threading.Tasks;
    using Microsoft.AspNetCore.Mvc;
    using Microsoft.AspNetCore.Mvc.Infrastructure;
    using Microsoft.CFS.Approvals.Common.DL.Interface;
    using Microsoft.CFS.Approvals.Data.Azure.Storage.Interface;
    using Microsoft.CFS.Approvals.DevTools.Model.Constant;
    using Microsoft.CFS.Approvals.DevTools.Model.Models;
    using Microsoft.CFS.Approvals.SupportServices.Helper.ExtensionMethods;
    using Microsoft.CFS.Approvals.SupportServices.Helper.ServiceHelper;
    using Newtonsoft.Json;
    using Newtonsoft.Json.Linq;

    /// <summary>
    /// The Templates Controller
    /// </summary>
    [Route("api/v1/Templates")]
    [ApiController]
    public class TemplatesController : ControllerBase
    {
        /// <summary>
        /// The blob storage helper
        /// </summary>
        private readonly IBlobStorageHelper _blobStorageHelper;

        /// <summary>
        /// The table helper
        /// </summary>
        private readonly ITableHelper _azureTableStorageHelper;

        private readonly ConfigurationHelper _configurationHelper;
        private readonly string _environment;

        /// <summary>
        /// Constructor of TemplatesController
        /// </summary>
        /// <param name="blobStorageHelper"></param>
        /// <param name="azureTableStorageHelper"></param>
        /// <param name="configurationHelper"></param>
        /// <param name="actionContextAccessor"></param>
        public TemplatesController(Func<string, string, IBlobStorageHelper> blobStorageHelper,
            Func<string, string, ITableHelper> azureTableStorageHelper,
            ConfigurationHelper configurationHelper,
            IActionContextAccessor actionContextAccessor)
        {
            _environment = actionContextAccessor?.ActionContext?.RouteData?.Values["env"]?.ToString();
            _blobStorageHelper = blobStorageHelper(
                configurationHelper.appSettings[_environment].StorageAccountName,
                configurationHelper.appSettings[_environment].StorageAccountKey);
            _configurationHelper = configurationHelper;
            _azureTableStorageHelper = azureTableStorageHelper(
                 configurationHelper.appSettings[_environment].StorageAccountName,
                configurationHelper.appSettings[_environment].StorageAccountKey);
        }

        /// <summary>
        /// Get tenant email templates
        /// </summary>
        /// <param name="operation"></param>
        /// <param name="tenantID"></param>
        /// <returns></returns>
        [HttpGet]
        [Route("{env}")]
        public async Task<IActionResult> Get(string operation, string tenantID)
        {
            try
            {
                JObject templates = new JObject();
                List<ApprovalEmailNotificationTemplatesEntity> emailTemplates = new List<ApprovalEmailNotificationTemplatesEntity>();
                TenantEntity tenantEntity = new TenantEntity();
                switch (operation)
                {
                    case "Approve":
                        tenantEntity = _azureTableStorageHelper.GetTableEntityByRowKey<TenantEntity>(Constants.PendingTenantApproval, tenantID);
                        emailTemplates = _azureTableStorageHelper.GetTableEntityListByPartitionKey<ApprovalEmailNotificationTemplatesEntity>(Constants.PendingApprovalEmailNotificationTemplates, tenantID);
                        break;

                    case "Edit":
                        tenantEntity = _azureTableStorageHelper.GetTableEntityByRowKey<TenantEntity>("ApprovalTenantInfo", tenantID);
                        emailTemplates = _azureTableStorageHelper.GetTableEntityListByPartitionKey<ApprovalEmailNotificationTemplatesEntity>(Constants.ApprovalEmailNotificationTemplates, tenantID);
                        break;
                }
                // Download Adaptive Templates
                var adaptiveTemplates = await _blobStorageHelper.ListBlobsHierarchicalListing(Constants.Outlookdynamictemplates, tenantEntity.ActionableEmailFolderName + "/", null);
                JArray adaptiveTemplatesArray = new JArray();
                foreach (var c in adaptiveTemplates)
                {
                    JObject adaptiveTemplate = new JObject();
                    var jtoken = c.ToJToken();
                    string filename = jtoken["Name"]?.ToString();
                    var prefix = jtoken?.SelectToken("Parent")?.SelectToken("Prefix")?.ToString();
                    if (prefix != null)
                    {
                        filename = filename?.Replace(prefix, string.Empty);
                    }
                    var FileBase64 = await _blobStorageHelper.DownloadByteArray(string.Format("{0}/{1}", tenantEntity.ActionableEmailFolderName, filename), Constants.Outlookdynamictemplates);
                    if (FileBase64 != null)
                    {
                        adaptiveTemplate.Add("FileName", filename);
                        adaptiveTemplate.Add("FileBase64", Convert.ToBase64String(FileBase64));
                        adaptiveTemplatesArray.Add(adaptiveTemplate);
                    }
                }
                //Convert Email Templates content to Base64
                JArray emailTemplatesArray = new JArray();
                {
                    foreach (var template in emailTemplates)
                    {
                        if (!string.IsNullOrWhiteSpace(template.TemplateContent))
                        {
                            JObject emailTemplate = new JObject
                            {
                                { "FileName", template.RowKey },
                                { "FileBase64", Convert.ToBase64String(System.Text.Encoding.UTF8.GetBytes(template.TemplateContent)) }
                            };
                            emailTemplatesArray.Add(emailTemplate);
                        }
                    }
                }
                //Download sample payload
                byte[] samplePayloadContent = null;
                if (await _blobStorageHelper.DoesExist(_configurationHelper.appSettings[_environment].SamplePayloadBlobContainer, string.Format("{0}{1}", tenantID, ".json")))
                {
                    samplePayloadContent = await _blobStorageHelper.DownloadByteArray(_configurationHelper.appSettings[_environment].SamplePayloadBlobContainer, string.Format("{0}{1}", tenantID, ".json"));
                }
                JArray samplePayloadJArray = new JArray();
                if (samplePayloadContent != null)
                {
                    JObject samplePayloadJObject = new JObject
                    {
                        { "FileName", string.Format("{0}{1}", tenantID, ".json") },
                        { "FileBase64", Convert.ToBase64String(samplePayloadContent) }
                    };
                    samplePayloadJArray.Add(samplePayloadJObject);
                }
                //Download Tenant Image
                var tenantImage = JsonConvert.DeserializeObject<JObject>(tenantEntity.TenantImage);
                var tenantIconFileName = tenantImage?.SelectToken("FileName")?.ToString();
                var tenantIconFileType = tenantImage?.SelectToken("FileType")?.ToString();
                JArray tenantIconJArray = new JArray();
                if (!string.IsNullOrWhiteSpace(tenantIconFileName) && !string.IsNullOrWhiteSpace(tenantIconFileType))
                {
                    string iconFileName = string.Format("{0}.{1}", tenantIconFileName, tenantIconFileType);
                    var tenantIconContent = await _blobStorageHelper.DownloadByteArray(iconFileName, _configurationHelper.appSettings[_environment].TenantIconBlob);
                    if (tenantIconContent != null)
                    {
                        JObject tenantIconJObject = new JObject
                        {
                            { "FileName", iconFileName },
                            { "FileBase64", Convert.ToBase64String(tenantIconContent) }
                        };
                        tenantIconJArray.Add(tenantIconJObject);
                    }
                }
                templates.Add("adaptiveTemplates", adaptiveTemplatesArray);
                templates.Add("emailTemplates", emailTemplatesArray);
                templates.Add("samplePayload", samplePayloadJArray);
                templates.Add("tenantIcon", tenantIconJArray);
                return Ok(templates);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }
    }
}