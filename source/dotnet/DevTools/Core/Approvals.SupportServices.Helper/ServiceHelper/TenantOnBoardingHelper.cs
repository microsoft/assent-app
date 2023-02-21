// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace Microsoft.CFS.Approvals.SupportServices.Helper.ServiceHelper;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using Microsoft.CFS.Approvals.Data.Azure.Storage.Interface;
using Microsoft.CFS.Approvals.DevTools.Model.Constant;
using Microsoft.CFS.Approvals.DevTools.Model.Models;
using Microsoft.CFS.Approvals.SupportServices.Helper.Interface;
using Microsoft.CFS.Approvals.Utilities.Interface;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

/// <summary>
/// Tenant On Boarding Helper class
/// </summary>
public class TenantOnBoardingHelper : ITenantOnBoardingHelper
{
    private readonly ITableHelper _azureTableStorageHelper;
    private readonly IBlobStorageHelper _blobStorageHelper;
    private readonly ConfigurationHelper _configurationHelper;
    private readonly IHttpHelper _httpHelper;
    private readonly string _environment;

    /// <summary>
    /// Constructor of TenantOnBoardingHelper
    /// </summary>
    /// <param name="azureTableStorageHelper"></param>
    /// <param name="configurationHelper"></param>
    /// <param name="actionContextAccessor"></param>
    /// <param name="tokenGenerator"></param>
    /// <param name="blobStorageHelper"></param>
    /// <param name="httpHelper"></param>
    public TenantOnBoardingHelper(Func<string, string, ITableHelper> azureTableStorageHelper,
        ConfigurationHelper configurationHelper,
        IActionContextAccessor actionContextAccessor,
        Func<string, IBlobStorageHelper> blobStorageHelper,
        IHttpHelper httpHelper)
    {
        _environment = actionContextAccessor?.ActionContext?.RouteData?.Values["env"]?.ToString();

        _configurationHelper = configurationHelper;
        _azureTableStorageHelper = azureTableStorageHelper(
             configurationHelper.appSettings[_environment]["StorageAccountName"],
            configurationHelper.appSettings[_environment]["StorageAccountKey"]);
        _blobStorageHelper = blobStorageHelper(configurationHelper.appSettings[_environment]["StorageAccountName"]);
        _httpHelper = httpHelper;
    }

    /// <summary>
    /// Get tenant list
    /// </summary>
    /// <param name="tenantType"></param>
    /// <returns></returns>
    public async Task<string> GetTenantList(string tenantType)
    {
        JObject functionAppConfiguration = JsonConvert.DeserializeObject<JObject>(_configurationHelper.appSettings[_environment]["FunctionAppConfiguration"]);
        string url = functionAppConfiguration?["tenantInfoFunctionAppUrl"]?.ToString();

        var response = await _httpHelper.SendRequestAsync(
            HttpMethod.Get,
            functionAppConfiguration["clientID"].ToString(),
            functionAppConfiguration["clientSecret"].ToString(),
            functionAppConfiguration["audiance"].ToString(),
            functionAppConfiguration["resource"].ToString(),
            url,
            new Dictionary<string, string>() { { "tenantType", tenantType } });

        return await response.Content.ReadAsStringAsync();
    }

    /// <summary>
    /// Tenant on boarding
    /// </summary>
    /// <param name="applicationDetail"></param>
    /// <param name="files"></param>
    /// <returns></returns>
    public async Task<bool> TenantOnBoarding(JObject applicationDetail, IFormFileCollection files)
    {
        try
        {
            var operation = applicationDetail?.SelectToken("RequestType")?.ToString();
            switch (operation.ToLower())
            {
                case "submit":
                    await SubmitRequest(applicationDetail, files);
                    break;

                case "approve":
                    await ApproveRequest(applicationDetail, files);
                    break;

                case "edit":
                    await EditTenant(applicationDetail, files);
                    break;

                default:
                    throw new InvalidOperationException("Invalid request type.");
            }
            return true;
        }
        catch (Exception ex)
        {
            throw ex;
        }
    }

    /// <summary>
    /// Submit request
    /// </summary>
    /// <param name="applicationDetail"></param>
    /// <param name="files"></param>
    /// <returns></returns>
    private async Task SubmitRequest(JObject applicationDetail, IFormFileCollection files)
    {
        var tenantID = Guid.NewGuid();
        var appid = Guid.NewGuid();
        applicationDetail["RowKey"] = tenantID;
        applicationDetail["PartitionKey"] = appid;
        applicationDetail["ActionableEmailFolderName"] = applicationDetail["AppName"];
        applicationDetail["ActionableNotificationTemplateKeys"] = "[\"PendingApproval\"]";
        applicationDetail["TenantType"] = "Prod";

        // Update ServiceParameter,Attachment URL & TenantImage
        applicationDetail = ReplaceURL(applicationDetail, files);

        JObject requestBody = new JObject
            {
                { "operation", "Create" },
                { "content", applicationDetail }
            };

        var response = SendAsync(requestBody).Result;
        if (response.IsSuccessStatusCode)
        {
            await UploadAdaptiveTemplates(applicationDetail, files);
            await UploadEmaiTemplates(applicationDetail, files, Constants.PendingApprovalEmailNotificationTemplates);
            await UploadTenantIcon(files);
            await CreateTestApplication(applicationDetail, files);
        }
        else
            throw new InvalidOperationException("Error to create the application.");
    }

    /// <summary>
    /// Approve request
    /// </summary>
    /// <param name="applicationDetail"></param>
    /// <param name="files"></param>
    /// <returns></returns>
    private async Task ApproveRequest(JObject applicationDetail, IFormFileCollection files)
    {
        var rowKey = applicationDetail?["RowKey"];
        var partitionKey = applicationDetail["PartitionKey"];
        var tenants = _azureTableStorageHelper.GetTableEntity<TenantEntity>("ApprovalTenantInfo").ToList();
        var tenantID = Convert.ToInt32(tenants?.Select(t => Convert.ToInt32(t.RowKey)).Max()) + 1;
        var DocTypeId = GenerateGuid(tenants, "DocTypeId");
        var appid = GenerateGuid(tenants, "PartitionKey");
        applicationDetail["RowKey"] = tenantID;
        applicationDetail["PartitionKey"] = appid;
        applicationDetail["DocTypeId"] = DocTypeId;
        applicationDetail["SubscriptionFilter"] = string.Format("ApplicationId like '{0}'", DocTypeId);

        // Update ServiceParameter,Attachment URL & TenantImage
        applicationDetail = ReplaceURL(applicationDetail, files);

        JObject requestBody = new JObject
            {
                { "operation", "Approve" },
                { "content", applicationDetail.ToString() }
            };
        var response = SendAsync(requestBody).Result;
        if (response.IsSuccessStatusCode)
        {
            await Task.Run(() => UploadAdaptiveTemplates(applicationDetail, files));
            await Task.Run(() => UploadTenantIcon(files));
            List<ApprovalEmailNotificationTemplatesEntity> EmailNotificationTemplates = _azureTableStorageHelper.GetTableEntityListByPartitionKey<ApprovalEmailNotificationTemplatesEntity>(Constants.PendingApprovalEmailNotificationTemplates, rowKey?.ToString());
            foreach (var emailTemplate in EmailNotificationTemplates)
            {
                emailTemplate.PartitionKey = Convert.ToString(tenantID);
                await _azureTableStorageHelper.InsertOrReplace<ApprovalEmailNotificationTemplatesEntity>(Constants.ApprovalEmailNotificationTemplates, emailTemplate);

                // Delete Email Templates From PendingApprovalEmailNotificationTemplates
                emailTemplate.PartitionKey = rowKey?.ToString();
                emailTemplate.ETag = global::Azure.ETag.All;
                await _azureTableStorageHelper.DeleteRow<ApprovalEmailNotificationTemplatesEntity>(Constants.PendingApprovalEmailNotificationTemplates, emailTemplate);
            }
            response = SendAsync(applicationDetail, true).Result;

            // Delete pending request from PendingTenantApproval table once it will get Approved.
            if (response.IsSuccessStatusCode)
            {
                var pendingRequest = _azureTableStorageHelper.GetTableEntityByRowKey<PendingTenantApprovalEntity>(Constants.PendingTenantApproval, rowKey?.ToString());
                await _azureTableStorageHelper.DeleteRow<PendingTenantApprovalEntity>(Constants.PendingTenantApproval, pendingRequest);
            }
        }
        else
            throw new InvalidOperationException("Error to approve the application.");
    }

    /// <summary>
    /// Edit tenant
    /// </summary>
    /// <param name="applicationDetail"></param>
    /// <param name="files"></param>
    /// <returns></returns>
    private async Task EditTenant(JObject applicationDetail, IFormFileCollection files)
    {
        // Update ServiceParameter,Attachment URL & TenantImage
        applicationDetail = ReplaceURL(applicationDetail, files);

        JObject requestBody = new JObject
            {
                { "operation", "Approve" },
                { "content", applicationDetail.ToString() }
            };
        var response = SendAsync(requestBody).Result;
        if (response.IsSuccessStatusCode)
        {
            await UploadAdaptiveTemplates(applicationDetail, files);
            await UploadEmaiTemplates(applicationDetail, files, Constants.ApprovalEmailNotificationTemplates);
            await UploadTenantIcon(files);
            await UploadSamplePayload(applicationDetail, files);
        }
    }

    /// <summary>
    /// Create test application
    /// </summary>
    /// <param name="applicationDetail"></param>
    /// <param name="files"></param>
    private async Task CreateTestApplication(JObject applicationDetail, IFormFileCollection files)
    {
        var tenants = _azureTableStorageHelper.GetTableEntity<TenantEntity>(Constants.ApprovalTenantInfo).ToList();
        var tenantID = Convert.ToInt32(tenants?.Select(t => Convert.ToInt32(t.RowKey)).Max()) + 1;
        var DocTypeId = GenerateGuid(tenants, "DocTypeId");
        var appid = GenerateGuid(tenants, "PartitionKey");

        var testTenantConfigurationSettings = JsonConvert.DeserializeObject<JObject>(_configurationHelper.appSettings[_environment]["TestTenantConfiguration"]);

        #region DetailOperation

        var TenantOperationDetails = JsonConvert.DeserializeObject<JObject>(applicationDetail?.SelectToken("TenantOperationDetails")?.ToString());
        JArray joperationArray = new JArray();
        if (TenantOperationDetails != null)
        {
            foreach (var operarion in TenantOperationDetails.SelectToken("DetailOpsList").Children())
            {
                JObject joperation = new JObject
                    {
                        { "operationtype", "" },
                        { "endpointdata", "" },
                        { "SupportsPagination", "" },
                        { "_client", "" },
                        { "IsCached", "" },
                        { "SerializerType", "" },
                        { "IsLegacyResponse", "" }
                    };
                switch (operarion["operationtype"].ToString())
                {
                    case "SUM":
                        joperation["operationtype"] = operarion?["operationtype"]?.ToString();
                        joperation["endpointdata"] = string.Format("{0}/{1}/{2}", "TestSummary", _environment, "{0}");
                        joperation["SupportsPagination"] = Convert.ToBoolean(operarion?["SupportsPagination"]?.ToString());
                        joperation["_client"] = false;
                        joperation["IsCached"] = false;
                        joperation["SerializerType"] = 0;
                        joperation["IsLegacyResponse"] = false;
                        joperationArray.Add(joperation);
                        break;

                    case "DOC1":
                        joperation["operationtype"] = operarion?["operationtype"]?.ToString();
                        joperation["endpointdata"] = string.Format("{0}/{1}/{2}", "TestDownloadDocument", _environment, "{0}");
                        joperation["SupportsPagination"] = Convert.ToBoolean(operarion?["SupportsPagination"]?.ToString());
                        joperation["_client"] = false;
                        joperation["IsCached"] = false;
                        joperation["SerializerType"] = 0;
                        joperation["IsLegacyResponse"] = false;
                        joperationArray.Add(joperation);
                        break;

                    case "ACT":
                        joperation["operationtype"] = operarion?["OperationType"]?.ToString();
                        joperation["endpointdata"] = string.Format("{0}/{1}", "TestActions", _environment);
                        joperation["SupportsPagination"] = Convert.ToBoolean(operarion?["SupportsPagination"]?.ToString());
                        joperation["_client"] = false;
                        joperation["IsCached"] = false;
                        joperation["SerializerType"] = 0;
                        joperation["IsLegacyResponse"] = false;
                        joperationArray.Add(joperation);
                        break;

                    default:
                        joperation["operationtype"] = operarion?["operationtype"]?.ToString();
                        operarion["endpointdata"] = string.Format("{0}/{1}/{2}", "TestDetails", _environment, "{0}");
                        joperation["SupportsPagination"] = Convert.ToBoolean(operarion?["SupportsPagination"]?.ToString());
                        joperation["_client"] = true;
                        joperation["IsCached"] = true;
                        joperation["SerializerType"] = 0;
                        joperation["IsLegacyResponse"] = false;
                        joperationArray.Add(joperation);
                        break;
                }
            }
            JObject operation = new JObject
                {
                    { "DetailOpsList", joperationArray }
                };
            applicationDetail["TenantOperationDetails"] = JsonConvert.SerializeObject(operation);
        }

        #endregion DetailOperation

        applicationDetail["RowKey"] = tenantID;
        applicationDetail["PartitionKey"] = appid;
        applicationDetail["DocTypeId"] = DocTypeId;
        applicationDetail["ActionableNotificationTemplateKeys"] = "[\"PendingApproval\"]";
        applicationDetail["TenantBaseUrl"] = testTenantConfigurationSettings?.SelectToken("TenantBaseUrl")?.ToString();
        applicationDetail["SummaryURL"] = testTenantConfigurationSettings?.SelectToken("SummaryURL")?.ToString();
        applicationDetail["RegisteredClients"] = testTenantConfigurationSettings?.SelectToken("RegisteredClients")?.ToString();
        applicationDetail["ActionableEmailFolderName"] = applicationDetail?["AppName"]?.ToString();
        applicationDetail["AppName"] = string.Format("Test {0}", applicationDetail?["AppName"]);
        applicationDetail["TemplateName"] = applicationDetail?["AppName"]?.ToString();
        applicationDetail["ToolName"] = applicationDetail?["ToolName"]?.ToString();
        applicationDetail["TenantDetailUrl"] = testTenantConfigurationSettings?.SelectToken("TenantDetailUrl")?.ToString();
        applicationDetail["Subscription"] = testTenantConfigurationSettings?.SelectToken("Subscription")?.ToString();
        applicationDetail["SubscriptionFilter"] = string.Format("ApplicationId like '{0}'", DocTypeId);
        applicationDetail["TenantEnabled"] = true;
        applicationDetail["ProcessSecondaryAction"] = 0;
        applicationDetail["ClassName"] = testTenantConfigurationSettings?.SelectToken("ClassName")?.ToString();
        applicationDetail["UseDocumentNumberForRowKey"] = true;
        applicationDetail["TenantMessageProcessingEnabled"] = true;
        applicationDetail["ServiceParameter"] = testTenantConfigurationSettings?.SelectToken("ServiceParameter")?.ToString();
        applicationDetail["ValidationClassName"] = testTenantConfigurationSettings?.SelectToken("ValidationClassName")?.ToString();
        applicationDetail["TenantDetailRetryCount"] = 3;
        applicationDetail["EnableUserDelegation"] = true;
        applicationDetail["TenantType"] = "Test";
        applicationDetail["BulkActionConcurrentCall"] = 10;
        applicationDetail["ActionSubmissionType"] = 1;
        applicationDetail["BackgroundApprovalRetryInterval"] = 3;
        applicationDetail["EnableModernAdaptiveUI"] = 2;
        applicationDetail["AdaptiveCardVersion"] = "v1";
        applicationDetail["IsRaceConditionHandled"] = false;
        applicationDetail["EnableUserDelegation"] = false;
        applicationDetail["ProcessAttachedSummary"] = true;

        JObject testApplication = new JObject
            {
                { "operation", "Approve" },
                { "content", applicationDetail.ToString() }
            };
        var response = SendAsync(testApplication).Result;
        if (response.IsSuccessStatusCode)
        {
            await UploadSamplePayload(applicationDetail, files);
            await UploadEmaiTemplates(applicationDetail, files, Constants.ApprovalEmailNotificationTemplates);
            response = SendAsync(applicationDetail, true).Result;
        }
    }

    /// <summary>
    /// Send async
    /// </summary>
    /// <param name="requestBody"></param>
    /// <param name="updateAzureResource"></param>
    /// <returns></returns>
    private async Task<HttpResponseMessage> SendAsync(JObject requestBody, bool updateAzureResource = false)
    {
        JObject functionAppConfiguration = JsonConvert.DeserializeObject<JObject>(_configurationHelper.appSettings[_environment]["FunctionAppConfiguration"]);
        string url = updateAzureResource ? functionAppConfiguration["manageAzureResourcefunctionAppUrl"].ToString() : functionAppConfiguration["tenantInfoFunctionAppUrl"].ToString();

        return await _httpHelper.SendRequestAsync(
                            HttpMethod.Post,
                            functionAppConfiguration["clientID"].ToString(),
                            functionAppConfiguration["clientSecret"].ToString(),
                            functionAppConfiguration["audiance"].ToString(),
                            functionAppConfiguration["resource"].ToString(),
                            url,
                            null,
                            JsonConvert.SerializeObject(requestBody));
    }

    /// <summary>
    /// Upload adaptive templates
    /// </summary>
    /// <param name="tenantInfoDetail"></param>
    /// <param name="files"></param>
    private async Task UploadAdaptiveTemplates(JObject tenantInfoDetail, IFormFileCollection files)
    {
        var adaptiveTemplates = files.GetFiles("adaptiveTemplate");
        if (adaptiveTemplates?.Count > 0)
        {
            await _blobStorageHelper.UploadFileToBlob(adaptiveTemplates.ToList(), tenantInfoDetail["ActionableEmailFolderName"].ToString(), Constants.Outlookdynamictemplates, string.Empty);
        }
    }

    /// <summary>
    /// Upload email templates
    /// </summary>
    /// <param name="tenantInfoDetail"></param>
    /// <param name="files"></param>
    /// <param name="storageTable"></param>
    private async Task UploadEmaiTemplates(JObject tenantInfoDetail, IFormFileCollection files, string storageTable)
    {
        var emailTemplates = files.GetFiles("EmailTemplate");
        if (emailTemplates?.Count > 0)
        {
            foreach (var file in emailTemplates)
            {
                try
                {
                    var templateContent = string.Empty;
                    using (var reader = new StreamReader(file.OpenReadStream()))
                    {
                        templateContent = reader.ReadToEnd();
                    }
                    ApprovalEmailNotificationTemplatesEntity approvalEmailNotificationTemplatesEntity = new ApprovalEmailNotificationTemplatesEntity
                    {
                        PartitionKey = tenantInfoDetail["RowKey"].ToString(),
                        RowKey = file.FileName,
                        TemplateContent = templateContent
                    };
                    await _azureTableStorageHelper.InsertOrReplace<ApprovalEmailNotificationTemplatesEntity>(storageTable, approvalEmailNotificationTemplatesEntity);
                }
                catch (Exception)
                {
                    //Telemetry log
                }
            }
        }
    }

    /// <summary>
    /// Upload sample payload
    /// </summary>
    /// <param name="tenantInfoDetail"></param>
    /// <param name="files"></param>
    private async Task UploadSamplePayload(JObject tenantInfoDetail, IFormFileCollection files)
    {
        try
        {
            var samplePayload = files.GetFiles("samplePayload").FirstOrDefault();
            if (samplePayload != null)
            {
                string fileName = string.Format("{0}{1}", tenantInfoDetail["RowKey"].ToString(), ".json");
                var content = string.Empty;

                using (var reader = new StreamReader(samplePayload.OpenReadStream()))
                {
                    content = reader.ReadToEnd();
                }
                await _blobStorageHelper.UploadFileToBlobAsync(fileName, content, samplePayload.ContentType, string.Empty, _configurationHelper.appSettings[_environment]["SamplePayloadBlobContainer"]);
            }
        }
        catch (Exception)
        {
            //Telemetry Log
        }
    }

    /// <summary>
    /// Upload tenant icon
    /// </summary>
    /// <param name="files"></param>
    private async Task UploadTenantIcon(IFormFileCollection files)
    {
        try
        {
            var tenanticon = files.GetFiles("TenantImage").FirstOrDefault();
            if (tenanticon != null)
            {
                string fileName = tenanticon.FileName;
                await _blobStorageHelper.UploadImageFileToBlobAsync(fileName, tenanticon, tenanticon.ContentType, "tenanticons");
            }
        }
        catch (Exception)
        {
            //Telemetry Log
        }
    }

    /// <summary>
    /// Generate guid
    /// </summary>
    /// <param name="tenants"></param>
    /// <param name="field"></param>
    /// <returns></returns>
    private Guid GenerateGuid(List<TenantEntity> tenants, string field)
    {
        Guid guid = Guid.NewGuid();
        bool isExist = false;
        switch (field)
        {
            case "DocTypeId":
                isExist = tenants.FirstOrDefault(t => t.DocTypeId.Equals(guid.ToString(), StringComparison.InvariantCultureIgnoreCase)) != null;
                break;

            case "PartitionKey":
                isExist = tenants.FirstOrDefault(t => t.PartitionKey.Equals(guid.ToString(), StringComparison.InvariantCultureIgnoreCase)) != null;
                break;
        }
        if (isExist)
        {
            GenerateGuid(tenants, field);
        }
        return guid;
    }

    /// <summary>
    /// Generate service parameter
    /// </summary>
    /// <param name="applicationDetail"></param>
    /// <returns></returns>
    private string GenerateServiceParameter(JObject applicationDetail)
    {
        string resourceURL = string.Empty;
        if (applicationDetail?.SelectToken("ResourceUrl") != null)
        {
            resourceURL = applicationDetail?.SelectToken("ResourceUrl").ToString();
        }

        var TenantConfigurationSettings = JsonConvert.DeserializeObject<JObject>(_configurationHelper.appSettings[_environment]["TestTenantConfiguration"]);
        var jServiceParameter = TenantConfigurationSettings?.SelectToken("ServiceParameter")?.Value<JObject>();
        jServiceParameter["ResourceURL"] = resourceURL;
        return JsonConvert.SerializeObject(jServiceParameter);
    }

    /// <summary>
    /// Replace URL
    /// </summary>
    /// <param name="applicationDetail"></param>
    /// <param name="files"></param>
    /// <returns></returns>
    private JObject ReplaceURL(JObject applicationDetail, IFormFileCollection files)
    {
        //Replace resource url to service parameter
        applicationDetail["ServiceParameter"] = GenerateServiceParameter(applicationDetail);

        //Add Attachment Operation in Operations collection
        if (!string.IsNullOrWhiteSpace(applicationDetail?.SelectToken("TenantOperationDetails")?.ToString()))
        {
            var TenantOperationDetails = JsonConvert.DeserializeObject<JObject>(applicationDetail?.SelectToken("TenantOperationDetails")?.ToString());
            if (!string.IsNullOrWhiteSpace(applicationDetail?["AttachmentUrl"]?.ToString()))
            {
                var attachmentOperation = TenantOperationDetails?.SelectToken("DetailOpsList").Children().Where(x => x["operationtype"]?.ToString() == "DOC1").FirstOrDefault();
                if (attachmentOperation == null)
                {
                    var operations = TenantOperationDetails.SelectToken("DetailOpsList")?.Value<JArray>();
                    JObject joperation = new JObject
                        {
                            { "operationtype", "DOC1" },
                            { "endpointdata", applicationDetail?["AttachmentUrl"] },
                            { "SupportsPagination", false },
                            { "_client", false },
                            { "IsCached", false },
                            { "SerializerType", 0 },
                            { "IsLegacyResponse", false }
                        };
                    operations.Add(joperation);
                    JObject detailsOperation = new JObject
                        {
                            { "DetailOpsList", operations }
                        };
                    applicationDetail["TenantOperationDetails"] = JsonConvert.SerializeObject(detailsOperation);
                }
                else
                {
                    attachmentOperation["endpointdata"] = applicationDetail?["AttachmentUrl"];
                }
            }
        }

        //Update TenantImage Detail
        if (files.GetFiles("TenantImage").FirstOrDefault() != null)
        {
            JObject tenantImage = JsonConvert.DeserializeObject<JObject>(applicationDetail["TenantImage"]?.ToString());
            tenantImage["FileURL"] = string.Format("{0}{1}.{2}", _configurationHelper.appSettings[_environment]["TenantIconBlobUrl"], tenantImage["FileName"]?.ToString(), tenantImage["FileType"]?.ToString());
            applicationDetail["TenantImage"] = JsonConvert.SerializeObject(tenantImage);
        }

        return applicationDetail;
    }
}