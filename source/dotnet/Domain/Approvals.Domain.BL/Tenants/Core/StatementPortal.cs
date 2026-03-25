// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace Microsoft.CFS.Approvals.Domain.BL.Tenants.Core;

using System.Collections.Generic;
using System.IO;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Microsoft.CFS.Approvals.Common.DL.Interface;
using Microsoft.CFS.Approvals.Contracts;
using Microsoft.CFS.Approvals.Contracts.DataContracts;
using Microsoft.CFS.Approvals.Data.Azure.Storage.Interface;
using Microsoft.CFS.Approvals.LogManager.Provider.Interface;
using Microsoft.CFS.Approvals.Model;
using Microsoft.CFS.Approvals.Utilities.Extension;
using Microsoft.CFS.Approvals.Utilities.Interface;
using Microsoft.Extensions.Configuration;

/// <summary>
/// Class StatementPortal
/// </summary>
/// <seealso cref="GenericTenant" />
public class StatementPortal : GenericTenant
{
    #region CONSTRUCTOR

    public StatementPortal(
        ApprovalTenantInfo tenantInfo,
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

    public StatementPortal(
        ApprovalTenantInfo tenantInfo,
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

    #endregion CONSTRUCTOR

    #region Implemented Methods

    #region Download document

    /// <summary>
    /// Gets the Attachment content from Line Of Business application and stores that in the blob.
    /// </summary>
    /// <param name="approvalIdentifier">The approval identifier.</param>
    /// <param name="attachmentId">The attachment identifier.</param>
    /// <param name="telemetry">The telemetry.</param>
    /// <returns></returns>
    public override async Task<byte[]> GetAttachmentContentFromLob(ApprovalIdentifier approvalIdentifier, string attachmentId, ApprovalsTelemetry telemetry, string approverUpn = "")
    {
        HttpResponseMessage lobResponse;
        string detailURL = GetAttachmentDownloadUrl(approvalTenantInfo.GetEndPointURL(AttachmentOperationName(), ClientDevice), attachmentId, approvalIdentifier);

        // This uses a POST method to get the data from tenant.
        // The tenant Statement Portal uses POST api for downloading attachment.
        // We pass the content (document number) in the request body
        // This is different from other tenants where the document number/attchmentid is passed in the query string and a the http verb used is Get
        HttpRequestMessage requestMessage = await CreateRequestForDetailsOrAction(HttpMethod.Post, detailURL);
        List<ApprovalRequest> approvalRequests = new List<ApprovalRequest>()
        {
            new ApprovalRequest() {
            ApprovalIdentifier = approvalIdentifier,
            Telemetry = telemetry,
            Action = Constants.SingleDownloadAction}
        };

        byte[] bytArr = null;
        var actionString = PrepareActionContentForSubmissionIntoTenantService(approvalRequests);
        requestMessage.Content = new StringContent(actionString, UTF8Encoding.UTF8, Constants.ContentTypeJson);

        lobResponse = await HttpHelper.SendRequestAsync(requestMessage);
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

    #endregion Download document

    #endregion Implemented Methods
}