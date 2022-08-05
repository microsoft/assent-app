// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace Microsoft.CFS.Approvals.Core.BL.Helpers
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;
    using AdaptiveCards;
    using global::Azure.Storage.Blobs.Models;
    using Microsoft.CFS.Approvals.Common.DL.Interface;
    using Microsoft.CFS.Approvals.Contracts;
    using Microsoft.CFS.Approvals.Core.BL.Interface;
    using Microsoft.CFS.Approvals.Data.Azure.Storage.Interface;
    using Microsoft.CFS.Approvals.Domain.BL.Interface;
    using Microsoft.CFS.Approvals.Extensions;
    using Microsoft.CFS.Approvals.LogManager.Model;
    using Microsoft.CFS.Approvals.LogManager.Provider.Interface;
    using Microsoft.CFS.Approvals.Model;
    using Microsoft.Extensions.Configuration;
    using Newtonsoft.Json.Linq;

    /// <summary>
    /// AdaptiveDetailsHelper class which implements IAdaptiveDetailsHelper to return adaptive template
    /// </summary>
    public class AdaptiveDetailsHelper : IAdaptiveDetailsHelper
    {
        #region Variables

        /// <summary>
        /// The log provider
        /// </summary>
        private readonly ILogProvider _logProvider;

        /// <summary>
        /// The approval tenant info helper
        /// </summary>
        private readonly IApprovalTenantInfoHelper _approvalTenantInfoHelper;

        /// <summary>
        /// The configuration
        /// </summary>
        private readonly IConfiguration _config;

        /// <summary>
        /// The flighting data provider
        /// </summary>
        private readonly IFlightingDataProvider _flightingDataProvider;

        /// <summary>
        /// The blob storage helper
        /// </summary>
        private readonly IBlobStorageHelper _blobStorageHelper;

        /// <summary>
        /// The tenant factory
        /// </summary>
        private readonly ITenantFactory _tenantFactory;

        /// <summary>
        /// The performance logger
        /// </summary>
        private readonly IPerformanceLogger _performanceLogger = null;

        #endregion Variables

        /// <summary>
        /// Constructor of AdaptiveDetailsHelper
        /// </summary>
        /// <param name="logProvider"></param>
        /// <param name="approvalTenantInfoHelper"></param>
        /// <param name="config"></param>
        /// <param name="flightingDataProvider"></param>
        /// <param name="blobStorageHelper"></param>
        /// <param name="tenantFactory"></param>
        public AdaptiveDetailsHelper(
            ILogProvider logProvider,
            IApprovalTenantInfoHelper approvalTenantInfoHelper,
            IConfiguration config,
            IFlightingDataProvider flightingDataProvider,
            IBlobStorageHelper blobStorageHelper,
            ITenantFactory tenantFactory,
            IPerformanceLogger performanceLogger)
        {
            _logProvider = logProvider;
            _approvalTenantInfoHelper = approvalTenantInfoHelper;
            _config = config;
            _flightingDataProvider = flightingDataProvider;
            _blobStorageHelper = blobStorageHelper;
            _tenantFactory = tenantFactory;
            _performanceLogger = performanceLogger;
        }

        /// <summary>
        /// Get Adaptive tempalates based on the Call Type
        /// </summary>
        /// <param name="tenantId">Tenant ID</param>
        /// <param name="userAlias">User Alias</param>
        /// <param name="loggedInAlias">Logged-in Alias</param>
        /// <param name="clientDevice">Client Device</param>
        /// <param name="aadUserToken">AAD User Token</param>
        /// <param name="sessionId">Session ID</param>
        /// <param name="xcv">X-correlation ID</param>
        /// <param name="tcv">T-Correlation ID</param>
        /// <param name="templateType">Template Type</param>
        /// <returns>Adaptive Card Payload JObject</returns>
        public async Task<Dictionary<string, JObject>> GetAdaptiveTemplate(int tenantId, string userAlias, string loggedInAlias, string clientDevice, string aadUserToken,
                                                                            string sessionId, string xcv, string tcv, int templateType)
        {
            #region Logging Prep

            if (string.IsNullOrEmpty(sessionId))
            {
                sessionId = Guid.NewGuid().ToString();
            }

            if (string.IsNullOrEmpty(tcv))
            {
                tcv = Guid.NewGuid().ToString();
            }

            if (string.IsNullOrEmpty(xcv))
            {
                xcv = Guid.NewGuid().ToString();
            }

            var logData = new Dictionary<LogDataKey, object>
            {
                { LogDataKey.Tcv, tcv },
                { LogDataKey.SessionId, sessionId },
                { LogDataKey.Xcv, xcv },
                { LogDataKey.UserRoleName, loggedInAlias },
                { LogDataKey.TenantId, tenantId },
                { LogDataKey.UserAlias, userAlias },
                { LogDataKey.IsCriticalEvent, CriticalityLevel.No.ToString() },
                { LogDataKey.ClientDevice, clientDevice },
                { LogDataKey.ReceivedTcv, tcv },
                { LogDataKey.Approver, userAlias },
                { LogDataKey.StartDateTime, DateTime.UtcNow },
                { LogDataKey.EndDateTime, DateTime.UtcNow},
                { LogDataKey.TemplateType, Constants.SummaryTemplate}
            };

            #endregion Logging Prep

            try
            {
                #region Getting the Tenant ID

                ApprovalTenantInfo tenantInfo = _approvalTenantInfoHelper.GetTenantInfo(tenantId);
                logData.Add(LogDataKey.BusinessProcessName, string.Format(tenantInfo.BusinessProcessName, Constants.BusinessProcessNameGetDetails, Constants.BusinessProcessNameUserTriggered));
                logData[LogDataKey.TenantName] = tenantInfo.AppName;
                logData[LogDataKey.DocumentTypeId] = tenantInfo.DocTypeId;

                #endregion Getting the Tenant ID
                using (_performanceLogger.StartPerformanceLogger("PerfLog", string.IsNullOrWhiteSpace(clientDevice) ? Constants.WebClient : clientDevice, string.Format(Constants.PerfLogAction, "AdaptiveDetail", "Get Adaptive Templates for the tenant"), logData))
                {
                    var isModernAdaptiveUI = tenantInfo.EnableModernAdaptiveUI switch
                    {
                        (int)EnableModernAdaptiveUI.DisableForAll => false,
                        (int)EnableModernAdaptiveUI.EnableForFlightedUsers => _flightingDataProvider.IsFeatureEnabledForUser(userAlias, (int)FlightingFeatureName.ModernAdaptiveDetailsUI),
                        (int)EnableModernAdaptiveUI.EnableForAll => true,
                        _ => false,
                    };
                    if (isModernAdaptiveUI)
                    {
                        #region Get Tenant Type

                        ITenant tenantAdaptor = null;
                        tenantAdaptor = _tenantFactory.GetTenant(
                                tenantInfo,
                                userAlias,
                                clientDevice,
                                aadUserToken);

                        #endregion Get Tenant Type

                        #region Get Template List

                        var templateList = new Dictionary<string, string>();
                        await GetAllIconsFromBlob(templateList);
                        await GetBlobTemplateByFileName(tenantInfo, clientDevice, templateList, templateType);

                        #endregion Get Template List

                        Dictionary<string, JObject> adaptiveTemplates = new Dictionary<string, JObject>();
                        JArray bodyArray = new JArray();

                        switch (templateType)
                        {
                            case (int)TemplateType.Summary:
                                var summaryTemplate = JObject.Parse(tenantAdaptor.GetSummaryAdaptiveTemplate(templateList));
                                bodyArray = MoveBodyInsideContainer(summaryTemplate, clientDevice);
                                summaryTemplate["body"] = bodyArray;
                                adaptiveTemplates.Add("SUM", summaryTemplate);
                                break;

                            case (int)TemplateType.Details:
                                var detailTemplate = GetDetailTemplate(clientDevice, templateList);
                                bodyArray = MoveBodyInsideContainer(detailTemplate, clientDevice);
                                detailTemplate["body"] = bodyArray;
                                adaptiveTemplates.Add("DTL", detailTemplate);
                                break;

                            case (int)TemplateType.Action:
                                adaptiveTemplates.Add("ACT", GetActionTemplate(clientDevice, templateList));
                                break;

                            case (int)TemplateType.Footer:
                                adaptiveTemplates.Add("FOOTER", GetFooterTemplate(clientDevice, templateList));
                                break;

                            case (int)TemplateType.All:
                                adaptiveTemplates.Add("SUM", JObject.Parse(tenantAdaptor.GetSummaryAdaptiveTemplate(templateList)));
                                adaptiveTemplates.Add("DTL", GetDetailTemplate(clientDevice, templateList));
                                adaptiveTemplates.Add("ACT", GetActionTemplate(clientDevice, templateList));
                                adaptiveTemplates.Add("FOOTER", GetFooterTemplate(clientDevice, templateList));
                                break;

                            case (int)TemplateType.Full:
                                var fullTemplate = tenantAdaptor.GetAdaptiveTemplate(templateList);
                                bodyArray = MoveBodyInsideContainer(fullTemplate, clientDevice);
                                fullTemplate["body"] = bodyArray;
                                adaptiveTemplates.Add("FULL", fullTemplate);
                                break;

                            default:
                                break;
                        }

                        List<string> keys = new List<string>(adaptiveTemplates.Keys);
                        foreach (var key in keys)
                        {
                            var adaptiveTemplateJson = adaptiveTemplates[key].ToJson();
                            adaptiveTemplateJson = adaptiveTemplateJson.Replace("#MSApprovalsCoreServiceURL#", _config[ConfigurationKey.ApprovalsCoreServicesURL.ToString()]);
                            adaptiveTemplateJson = adaptiveTemplateJson.Replace("#MSApprovalsBaseUrl#", _config[ConfigurationKey.ApprovalsBaseUrl.ToString()]);
                            adaptiveTemplateJson = adaptiveTemplateJson.Replace("#ExpenseImg#", await GetIconUrl(templateList, "money-icon.png"));
                            adaptiveTemplateJson = adaptiveTemplateJson.Replace("#approveIcon#", await GetIconUrl(templateList, "greenTick.png"));
                            adaptiveTemplateJson = adaptiveTemplateJson.Replace("#pendingIcon#", await GetIconUrl(templateList, "refresh.png"));
                            adaptiveTemplateJson = adaptiveTemplateJson.Replace("#rejectIcon#", await GetIconUrl(templateList, "error.png"));
                            adaptiveTemplateJson = adaptiveTemplateJson.Replace("#attachmentIcon#", await GetIconUrl(templateList, "attachment.png"));
                            adaptiveTemplateJson = adaptiveTemplateJson.Replace("#ImageIcon#", await GetIconUrl(templateList, "image-icon.png"));
                            adaptiveTemplateJson = adaptiveTemplateJson.Replace("#upIcon#", await GetIconUrl(templateList, "up.png"));
                            adaptiveTemplateJson = adaptiveTemplateJson.Replace("#downIcon#", await GetIconUrl(templateList, "down.png"));
                            adaptiveTemplateJson = adaptiveTemplateJson.Replace("#receiptIcon#", await GetIconUrl(templateList, "receipt.png"));
                            adaptiveTemplateJson = adaptiveTemplateJson.Replace("#policyIcon#", await GetIconUrl(templateList, "policy.png"));
                            adaptiveTemplateJson = adaptiveTemplateJson.Replace("#car-sideIcon#", await GetIconUrl(templateList, "car-side.png"));
                            adaptiveTemplateJson = adaptiveTemplateJson.Replace("#airplaneIcon#", await GetIconUrl(templateList, "airplane.png"));
                            adaptiveTemplateJson = adaptiveTemplateJson.Replace("#downloadIcon#", await GetIconUrl(templateList, "download.png"));
                            adaptiveTemplateJson = adaptiveTemplateJson.Replace("#previewIcon#", await GetIconUrl(templateList, "preview.png"));
                            adaptiveTemplates[key] = JObject.Parse(adaptiveTemplateJson);
                        }
                        logData.Modify(LogDataKey.EndDateTime, DateTime.UtcNow);
                        _logProvider.LogInformation(TrackingEvent.AdaptiveTemplateFetchSuccess, logData);
                        return adaptiveTemplates;
                    }
                    else
                    {
                        throw new InvalidOperationException(Constants.NotFlightedMornUIErrorMessage);
                    }
                }
            }
            catch (Exception ex)
            {
                logData.Modify(LogDataKey.EndDateTime, DateTime.UtcNow);
                _logProvider.LogError(TrackingEvent.AdaptiveTemplateFetchFailure, ex, logData);
                throw;
            }
        }

        /// <summary>
        /// Gets the blob template by file name.
        /// </summary>
        /// <param name="templateList">List of templates</param>
        /// <returns>returns a htmlContent</returns>
        private async Task GetAllIconsFromBlob(IDictionary<string, string> templateList)
        {
            try
            {
                var listBlobs = await _blobStorageHelper.ListBlobsHierarchicalListing(Constants.OutlookActionableEmailIcons, "", null);

                var sasToken = _blobStorageHelper.GetContainerSasToken(Constants.OutlookActionableEmailIcons, DateTimeOffset.UtcNow.AddDays(7));
                foreach (var item in listBlobs)
                {
                    var blobItemName = item.Name;
                    var storageAccountName = Environment.GetEnvironmentVariable("StorageAccountName");
                    templateList[blobItemName] = $"https://{storageAccountName}.blob.core.windows.net/{Constants.OutlookActionableEmailIcons}/{blobItemName}?" + sasToken;
                }
            }
            catch (Exception ex)
            {
                _logProvider.LogError(TrackingEvent.OutlookBlobTemplateByFileName, ex);
                throw ex;
            }
        }

        /// <summary>
        /// Gets the blob template by file name.
        /// </summary>
        /// <param name="tenantInfo">Tenant Info</param>
        /// <param name="clientDevice">Client Device</param>
        /// <param name="templateList">List of templates</param>
        /// <returns>returns a htmlContent</returns>
        private async Task GetBlobTemplateByFileName(ApprovalTenantInfo tenantInfo, string clientDevice, IDictionary<string, string> templateList, int templateType = 4)
        {
            string fileBase64 = string.Empty;
            try
            {
                string directoryName = string.IsNullOrWhiteSpace(tenantInfo.ActionableEmailFolderName) ? tenantInfo.AppName : tenantInfo.ActionableEmailFolderName;
                var listBlobs = await _blobStorageHelper.ListBlobsHierarchicalListing(Constants.OutlookDynamicTemplates, directoryName, null);
                var listCommonBlobs = await _blobStorageHelper.ListBlobsHierarchicalListing(Constants.OutlookDynamicTemplates, Constants.CommonTemplates, null);

                List<BlobItem> adaptiveBlobList = new List<BlobItem>();
                List<BlobItem> adaptiveCommonBlobList = new List<BlobItem>();

                var commonMessageBlob = listCommonBlobs.Where(b => b.Name.Equals(Constants.CommonTemplates + "/" + Constants.COMMONTEMPLATE + clientDevice + ".json")).ToList();
                if (commonMessageBlob == null || commonMessageBlob.Count == 0)
                    commonMessageBlob = listCommonBlobs.Where(b => b.Name.Equals(Constants.CommonTemplates + "/" + Constants.COMMONTEMPLATE + ".json")).ToList();
                adaptiveCommonBlobList.AddRange(commonMessageBlob);

                if (templateType == (int)TemplateType.Summary || templateType == (int)TemplateType.All || templateType == (int)TemplateType.Full)
                {
                    var summaryBlob = listBlobs.Where(b => b.Name.Equals(directoryName + "/" + Constants.SUMMARYBODYTEMPLATE + clientDevice + ".json")).ToList();
                    if (summaryBlob == null || summaryBlob.Count == 0)
                        summaryBlob = listBlobs.Where(b => b.Name.Equals(directoryName + "/" + Constants.SUMMARYBODYTEMPLATE + ".json")).ToList();
                    adaptiveBlobList.AddRange(summaryBlob);

                    var summaryBlobCommon = listCommonBlobs.Where(b => b.Name.Equals(Constants.CommonTemplates + "/" + Constants.SUMMARYBODYTEMPLATE + clientDevice + ".json")).ToList();
                    if (summaryBlobCommon == null || summaryBlobCommon.Count == 0)
                        summaryBlobCommon = listCommonBlobs.Where(b => b.Name.Equals(Constants.CommonTemplates + "/" + Constants.SUMMARYBODYTEMPLATE + ".json")).ToList();
                    adaptiveCommonBlobList.AddRange(summaryBlobCommon);
                }

                if (templateType == (int)TemplateType.Details || templateType == (int)TemplateType.All || templateType == (int)TemplateType.Full || templateType == (int)TemplateType.DetailsAddOn)
                {
                    var detailsBlob = listBlobs.Where(b => b.Name.Equals(directoryName + "/" + Constants.BODYTEMPLATE + clientDevice + ".json")).ToList();
                    if (detailsBlob == null || detailsBlob.Count == 0)
                        detailsBlob = listCommonBlobs.Where(b => b.Name.Equals(Constants.CommonTemplates + "/" + Constants.BODYTEMPLATE + clientDevice + ".json")).ToList();
                    if (detailsBlob == null || detailsBlob.Count == 0)
                        detailsBlob = listBlobs.Where(b => b.Name.Equals(directoryName + "/" + Constants.BODYTEMPLATE + ".json")).ToList();
                    adaptiveBlobList.AddRange(detailsBlob);

                    adaptiveBlobList.AddRange(listBlobs.Where(b => b.Name.Equals(directoryName + "/" + Constants.ATTACHMENTSTEMPLATE + clientDevice + ".json")).ToList());
                    adaptiveCommonBlobList.AddRange(listCommonBlobs.Where(b => b.Name.Equals(Constants.CommonTemplates + "/" + Constants.ATTACHMENTSTEMPLATE + clientDevice + ".json")).ToList());
                }

                if (templateType == (int)TemplateType.Action || templateType == (int)TemplateType.All || templateType == (int)TemplateType.Full || templateType == (int)TemplateType.DetailsAddOn)
                    adaptiveBlobList.AddRange(listBlobs.Where(b => b.Name.Equals(directoryName + "/" + Constants.ACTIONTEMPLATE + clientDevice + ".json")).ToList());

                if (templateType == (int)TemplateType.Footer || templateType == (int)TemplateType.All || templateType == (int)TemplateType.Full || templateType == (int)TemplateType.DetailsAddOn)
                {
                    adaptiveBlobList.AddRange(listBlobs.Where(b => b.Name.Equals(directoryName + "/" + Constants.FOOTERTEMPLATE + clientDevice + ".json")).ToList());
                    adaptiveCommonBlobList.AddRange(listCommonBlobs.Where(b => b.Name.Equals(Constants.CommonTemplates + "/" + Constants.FOOTERTEMPLATE + clientDevice + ".json")).ToList());
                }
                foreach (var item in adaptiveBlobList)
                {
                    var blobItemName = item.Name;
                    var htmlTemplate = await _blobStorageHelper.DownloadText(Constants.OutlookDynamicTemplates, blobItemName);
                    blobItemName = blobItemName.Split('/')[1].ToString();
                    templateList[blobItemName] = htmlTemplate;
                }
                foreach (var commonItem in adaptiveCommonBlobList)
                {
                    var blobItemName = commonItem.Name;
                    var alteredBlobName = blobItemName.Split('/')[1].ToString();
                    if (!templateList.ContainsKey(alteredBlobName))
                    {
                        var htmlTemplate = await _blobStorageHelper.DownloadText(Constants.OutlookDynamicTemplates, blobItemName);
                        templateList[alteredBlobName] = htmlTemplate;
                    }
                }
            }
            catch (Exception ex)
            {
                _logProvider.LogError(TrackingEvent.OutlookBlobTemplateByFileName, ex);
                throw ex;
            }
        }

        /// <summary>
        /// Get Detail Template
        /// </summary>
        /// <param name="clientDevice">Client Device</param>
        /// <param name="templateList">Template list</param>
        /// <returns>details adaptive template object</returns>
        private JObject GetDetailTemplate(string clientDevice, Dictionary<string, string> templateList)
        {
            JArray detailsBodyArr;
            JObject detailsAdaptiveObj, attachmentObj, commonTemplateObj;

            bool isBodyPresent = templateList.TryGetValue(Constants.BODYTEMPLATE + clientDevice + ".json", out string detailsBodyTemplate);
            bool isAttachmentPresent = templateList.TryGetValue(Constants.ATTACHMENTSTEMPLATE + clientDevice + ".json", out string attachmentTemplate);
            bool isCommonMessagePresent = templateList.TryGetValue(Constants.COMMONTEMPLATE + ".json", out string commonTemplate);
            if (!isBodyPresent)
            {
                detailsBodyTemplate = templateList[Constants.BODYTEMPLATE + ".json"];
            }
            detailsAdaptiveObj = detailsBodyTemplate.FromJson<JObject>();
            detailsBodyArr = detailsAdaptiveObj["body"].ToString().FromJson<JArray>();

            if (isAttachmentPresent)
            {
                attachmentObj = attachmentTemplate.FromJson<JObject>();
                detailsBodyArr.AddFirst(attachmentObj);
            }
            if (isCommonMessagePresent)
            {
                commonTemplateObj = commonTemplate.FromJson<JObject>();
                detailsBodyArr.AddFirst(commonTemplateObj);
            }

            detailsAdaptiveObj["body"] = detailsBodyArr;

            return detailsAdaptiveObj;
        }

        /// <summary>
        /// Fetch Icon url by from list name
        /// </summary>
        /// <param name="templateList"></param>
        /// <param name="iconName"></param>
        /// <returns>Returns Icon blob url</returns>
        private async Task<string> GetIconUrl(IDictionary<string, string> templateList, string iconName)
        {
            if (string.IsNullOrEmpty(iconName))
            {
                return string.Empty;
            }

            if (templateList.ContainsKey(iconName))
            {
                var icon = await _blobStorageHelper.DownloadByteArray("outlookactionableemailicons", iconName);
                if (icon != null)
                {
                    return string.Format("{0},{1}", "data:image/jpeg;base64", Convert.ToBase64String(icon, 0, icon.Length));
                }

                return string.Empty;
            }
            else
            {
                return string.Empty;
            }
        }

        /// <summary>
        /// Get Footer Template
        /// </summary>
        /// <param name="clientDevice">Client Device</param>
        /// <param name="templateList">Template list</param>
        /// <returns>footer adaptive template object</returns>
        private JObject GetFooterTemplate(string clientDevice, Dictionary<string, string> templateList)
        {
            AdaptiveCard actionAdaptiveCard = new AdaptiveCard("1.0");
            var adaptiveCardObj = actionAdaptiveCard.ToJson().FromJson<JObject>();
            bool isFooterPresent = templateList.TryGetValue(Constants.FOOTERTEMPLATE + clientDevice + ".json", out string footerTemplate);
            if (isFooterPresent)
            {
                adaptiveCardObj.Add("body", new JArray() { footerTemplate.FromJson<JObject>() });
            }
            return adaptiveCardObj;
        }

        /// <summary>
        /// Get Action Template
        /// </summary>
        /// <param name="clientDevice">Client Device</param>
        /// <param name="templateList">Template list</param>
        /// <returns>action adaptive template object</returns>
        private JObject GetActionTemplate(string clientDevice, Dictionary<string, string> templateList)
        {
            AdaptiveCard actionAdaptiveCard = new AdaptiveCard(Constants.AdaptiveTemplateVersion);
            var adaptiveCardObj = actionAdaptiveCard.ToJson().FromJson<JObject>();
            bool isActionPresent = templateList.TryGetValue(Constants.ACTIONTEMPLATE + clientDevice + ".json", out string actionTemplate);
            if (isActionPresent)
            {
                adaptiveCardObj.Add("body", new JArray() { actionTemplate.FromJson<JObject>() });
            }
            return adaptiveCardObj;
        }

        /// <summary>
        /// Move the entire body items except the header auto-refresh message item into single container
        /// </summary>
        /// <param name="adaptiveJSON">Adaptive Full body template</param>
        /// <param name="clientDevice">Client Device</param>
        private JArray MoveBodyInsideContainer(JObject adaptiveJSON, string clientDevice)
        {
            var bodyArray = adaptiveJSON["body"];
            var array = new JArray();
            if (bodyArray.Count() > 0)
            {
                array.Add(bodyArray[0]);
                bodyArray[0].Remove();
            }

            AdaptiveContainer container = new AdaptiveContainer();
            JObject obj = container.ToJson().FromJson<JObject>();
            (obj["items"] as JArray).Merge(bodyArray);
            switch (clientDevice)
            {
                case Constants.TeamsClient:
                    obj["$when"] = "${and(or(not(exists(Message)), Message == ''), or(not(exists(ActionTakeOnMessage)), ActionTakeOnMessage == ''))}";
                    break;
                default:
                    obj["$when"] = "${or(not(exists(Message)), Message == '')}";
                    break;
            }
            array.Add(obj);
            return array;
        }
    }
}