// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.CFS.Approvals.Common.DL.Interface;
using Microsoft.CFS.Approvals.Contracts;
using Microsoft.CFS.Approvals.Contracts.DataContracts;
using Microsoft.CFS.Approvals.Data.Azure.Storage.Interface;
using Microsoft.CFS.Approvals.Extensions;
using Microsoft.CFS.Approvals.LogManager.Provider.Interface;
using Microsoft.CFS.Approvals.Model;
using Microsoft.CFS.Approvals.Utilities.Extension;
using Microsoft.CFS.Approvals.Utilities.Interface;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;

namespace Microsoft.CFS.Approvals.Domain.BL.Tenants.Core;

/// <summary>
/// Class Invoice
/// </summary>
/// <seealso cref="TenantBase" />
public class ModernInvoice : GenericTenant
{
    public ModernInvoice(ApprovalTenantInfo tenantInfo,
        ILogProvider logger,
        IPerformanceLogger performanceLogger,
        IApprovalSummaryProvider approvalSummaryProvider,
        IConfiguration config,
        INameResolutionHelper nameResolutionHelper,
        IApprovalDetailProvider approvalDetailProvider,
        IFlightingDataProvider flightingDataProvider,
        IApprovalHistoryProvider approvalHistoryProvider,
        IBlobStorageHelper blobStorageHelper,
        IAuthenticationHelper authenticationHelper,
        IHttpHelper httpHelper)
        : base(
              tenantInfo,
              logger,
              performanceLogger,
              approvalSummaryProvider,
              config,
              nameResolutionHelper,
              approvalDetailProvider,
              flightingDataProvider,
              approvalHistoryProvider,
              blobStorageHelper,
              authenticationHelper,
              httpHelper)
    {
    }

    public ModernInvoice(ApprovalTenantInfo tenantInfo,
        string alias,
        string clientDevice,
        string oauth2Token,
        ILogProvider logger,
        IPerformanceLogger performanceLogger,
        IApprovalSummaryProvider approvalSummaryProvider,
        IConfiguration config,
        INameResolutionHelper nameResolutionHelper,
        IApprovalDetailProvider approvalDetailProvider,
        IFlightingDataProvider flightingDataProvider,
        IApprovalHistoryProvider approvalHistoryProvider,
        IBlobStorageHelper blobStorageHelper,
        IAuthenticationHelper authenticationHelper,
        IHttpHelper httpHelper,
        string objectId,
        string domain)
        : base(
              tenantInfo,
              alias,
              clientDevice,
              oauth2Token,
              logger,
              performanceLogger,
              approvalSummaryProvider,
              config,
              nameResolutionHelper,
              approvalDetailProvider,
              flightingDataProvider,
              approvalHistoryProvider,
              blobStorageHelper,
              authenticationHelper,
              httpHelper,
              objectId,
              domain)
    {
    }
    /// <summary>
    /// Check if Attachment flighting feature is enable for user.
    /// This Method will get removed once this feature enable for all users.
    /// </summary>
    /// <param name="userAlias">Alias of the Approver of this request</param>
    /// <param name="uploadAttachmentFeaureId">Attachment flighting feature id.</param>
    /// <returns>return true/false</returns>
    public override bool CheckUserAttachmentFlightingFeature(string userAlias, int uploadAttachmentFeaureId)
    {
        //This check will be remove once upload attachment feature enable for all users.
        return true;
    }

    public override async Task<byte[]> GetAttachmentContentFromLob(ApprovalIdentifier approvalIdentifier, string attachmentId, ApprovalsTelemetry telemetry, string approverUpn = "")
    {
        var placeHolderDict = new Dictionary<string, object>();
        placeHolderDict = Extension.ConvertJsonToDictionary(placeHolderDict, JsonConvert.SerializeObject(approvalIdentifier));
        var placeholderUrlTenantIdList = Config[ConfigurationKey.UrlPlaceholderTenants.ToString()]?.Split(',').ToList() ?? new List<string>();
        placeHolderDict.Add("AttachmentId", attachmentId);
        placeHolderDict.Add("DocumentTypeId", approvalTenantInfo.DocTypeId);

        HttpResponseMessage lobResponse;
        var actionName = AttachmentOperationName();
        byte[] bytArr = null;

        string detailURL = placeholderUrlTenantIdList.Contains(approvalTenantInfo.RowKey)
            ? RetrieveTenantDetailsUrl(actionName, placeHolderDict, ClientDevice) 
            : GetAttachmentDownloadUrl(approvalTenantInfo.GetEndPointURL(actionName, ClientDevice), attachmentId, approvalIdentifier);

        lobResponse = await HttpHelper.SendRequestAsync(await CreateRequestForDetailsOrAction(HttpMethod.Get, detailURL, telemetry.Xcv, telemetry.Tcv, actionName));
        if (lobResponse.IsSuccessStatusCode)
        {
            using (MemoryStream streamData = (MemoryStream)await lobResponse.Content.ReadAsStreamAsync())
            {
                bytArr = streamData.ToArray();
                await SaveAttachmentToBlob(approvalTenantInfo.TenantId, approvalIdentifier, bytArr, attachmentId);
            }
        }
        return bytArr;
    }

}