// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace Microsoft.CFS.Approvals.Domain.BL.Tenants.Core;

using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using Microsoft.CFS.Approvals.Common.DL.Interface;
using Microsoft.CFS.Approvals.Contracts.DataContracts;
using Microsoft.CFS.Approvals.Data.Azure.Storage.Interface;
using Microsoft.CFS.Approvals.Extensions;
using Microsoft.CFS.Approvals.LogManager.Provider.Interface;
using Microsoft.CFS.Approvals.Model;
using Microsoft.CFS.Approvals.Utilities.Extension;
using Microsoft.CFS.Approvals.Utilities.Helpers;
using Microsoft.CFS.Approvals.Utilities.Interface;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json.Linq;

/// <summary>
/// Class Vacation
/// </summary>
/// <seealso cref="TenantBase" />
public class Vacation : TenantBase
{
    #region CONSTRUCTOR

    public Vacation(
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

    public Vacation(
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

    #region Convert JSON Methods

    protected override async Task<SummaryJson> ExtractJSONDetails(ApprovalRequestExpression approvalRequest, JObject summaryJsonObject)
    {
        SummaryJson summaryJson = new SummaryJson();
        summaryJson = summaryJsonObject?.ToString().FromJson<SummaryJson>();
        if (new SummaryJson() == null || new SummaryJson().ApprovalIdentifier == null || new SummaryJson().ApprovalIdentifier.DisplayDocumentNumber == null)
        {
            throw new InvalidDataException("Invalid Summary Data: " + new SummaryJson().ToJson());
        }
        new SummaryJson().Submitter.Name = await GetUserDisplayName(new SummaryJson().Submitter.Alias);
        if (approvalRequest.Approvers != null)
        {
            foreach (var approver in approvalRequest.Approvers)
            {
                approver.Name = await GetUserDisplayName(approver.Alias);
            }
        }

        return new SummaryJson();
    }

    #endregion Convert JSON Methods

    #region GET SUMMARY

    /// <summary>
    /// Formulate Summary Url
    /// </summary>
    /// <param name="tenantInfo"></param>
    /// <param name="documentSummaryUrl"></param>
    /// <param name="approvalRequest"></param>
    /// <param name="docTypeId"></param>
    /// <returns></returns>
    protected override string GetSummaryUrl(ApprovalTenantInfo tenantInfo, string documentSummaryUrl, ApprovalRequestExpression approvalRequest, string docTypeId = "")
    {
        // The summary url uses display document number
        return string.Format(documentSummaryUrl, approvalRequest.ApprovalIdentifier.GetDocNumber(tenantInfo));
    }

    #endregion GET SUMMARY

    #region GET DETAIL

    protected override async Task<string> GetDetailURL(string urlFormat, ApprovalIdentifier approvalIdentifier, int page, string xcv = "", string tcv = "", string businessProcessName = "", string docTypeId = "")
    {
        return string.Format(urlFormat, approvalIdentifier.GetDocNumber(approvalTenantInfo));
    }

    protected override string FutureApproverChainOperationName()
    {
        return string.Empty;
    }

    #endregion GET DETAIL

    #region POST AUTHSUM operation

    public override JObject ExecutePostAuthSum(JObject authSumObject)
    {
        var placeHolderDict = new Dictionary<string, string>();
        if (!authSumObject.TryGetValue("AdditionalData", out JToken additionalDataValue)) return authSumObject;
        JSONHelper.ConvertJsonToDictionary(placeHolderDict, additionalDataValue.ToString());
        if (!placeHolderDict.TryGetValue("ApproverNotes", out string approverNotes)) return authSumObject;
        if (authSumObject.Property("ApproverNotes") == null)
        {
            authSumObject.Add("ApproverNotes", approverNotes);
        }
        else if (string.IsNullOrEmpty(authSumObject.Property("ApproverNotes").Value.ToString()))
        {
            authSumObject.Property("ApproverNotes").Value = approverNotes;
        }
        return authSumObject;
    }

    #endregion POST AUTHSUM operation
}