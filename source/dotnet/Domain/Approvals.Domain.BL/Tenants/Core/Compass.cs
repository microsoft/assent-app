// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace Microsoft.CFS.Approvals.Domain.BL.Tenants.Core;

using System;
using System.Collections.Generic;
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
using Newtonsoft.Json.Linq;

/// <summary>
/// Class Compass
/// </summary>
/// <seealso cref="Procurement" />
public class Compass : Procurement
{
    #region CONSTRUCTOR

    public Compass(
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

    public Compass(
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

    /// <summary>
    /// Add Messages
    /// </summary>
    /// <param name="jsonDetail"></param>
    /// <param name="operation"></param>
    /// <returns></returns>
    protected override string AddMessages(string jsonDetail, string operation)
    {
        JObject jObject = (jsonDetail).ToJObject();
        JArray jArray = new JArray();
        if (!operation.Equals("DTL", StringComparison.InvariantCultureIgnoreCase))
        {
            if (jObject["Submitter"] != null)
                jObject.Remove("Submitter");
            if (jObject["ProjectOwner"] != null)
                jObject.Remove("ProjectOwner");
        }
        return jObject.ToJson();
    }

    /// <summary>
    /// Gets the lob response asynchronous.
    /// TO DO: This is a temporary change to handle the scenario wherein the correct value of approval identifier is not available in the request coming from UI. 
    /// This will be removed once the approval identifier is available in the request.
    /// Created a work item to get the approval identifier in the request.
    /// 12763484 :: Approval Identifier is not being passed from UI to API
    /// </summary>
    /// <param name="approvalIdentifier">The approval identifier.</param>
    /// <param name="operation">The operation.</param>
    /// <param name="page">The page.</param>
    /// <param name="xcv">The XCV.</param>
    /// <param name="tcv">The TCV.</param>
    /// <param name="businessProcessName">Name of the business process.</param>
    /// <param name="docTypeId">Document Type ID</param>
    public override async Task<HttpResponseMessage> GetLobResponseAsync(ApprovalIdentifier approvalIdentifier, string operation, int page, string xcv, string tcv, string businessProcessName, string docTypeId)
    {
        var placeHolderDict = new Dictionary<string, object>();
        var actualApprovalIdentifier = await GetApprovalIdentifier(approvalIdentifier.GetDocNumber(approvalTenantInfo), xcv, tcv) ?? approvalIdentifier;
        placeHolderDict = Extension.ConvertJsonToDictionary(placeHolderDict, JsonConvert.SerializeObject(actualApprovalIdentifier));
        placeHolderDict.Add("DocumentTypeId", docTypeId);
        var placeholderUrlTenantIdList = Config[ConfigurationKey.UrlPlaceholderTenants.ToString()].Split(',').ToList();
        string detailURL = (placeholderUrlTenantIdList.Contains(approvalTenantInfo.RowKey)) ? RetrieveTenantDetailsUrl(operation, placeHolderDict, ClientDevice) : await GetDetailURL(approvalTenantInfo.GetEndPointURL(operation, ClientDevice), approvalIdentifier, page, xcv, tcv, businessProcessName, docTypeId);

        HttpResponseMessage lobResponse;
        lobResponse = await HttpHelper.SendRequestAsync(await CreateRequestForDetailsOrAction(HttpMethod.Get, detailURL, xcv, tcv, operation));
        return lobResponse;
    }
}