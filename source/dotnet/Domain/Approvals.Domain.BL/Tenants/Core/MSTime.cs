// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace Microsoft.CFS.Approvals.Domain.BL.Tenants.Core;

using System;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.CFS.Approvals.Common.DL.Interface;
using Microsoft.CFS.Approvals.Contracts;
using Microsoft.CFS.Approvals.Contracts.DataContracts;
using Microsoft.CFS.Approvals.Data.Azure.Storage.Interface;
using Microsoft.CFS.Approvals.LogManager.Provider.Interface;
using Microsoft.CFS.Approvals.Model;
using Microsoft.CFS.Approvals.Utilities.Interface;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json.Linq;
using static Microsoft.Azure.Amqp.Serialization.SerializableType;

/// <summary>
/// Class MSTime
/// </summary>
/// <seealso cref="TenantBase" />
public class MSTime : TenantBase
{
    #region CONSTRUCTOR

    public MSTime(
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

    public MSTime(
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
    /// This method will provide Future approver chain operation name, here we don't need as of now so it will return empty string.
    /// </summary>
    /// <returns>This will return future approver chain operation name.</returns>
    protected override string FutureApproverChainOperationName()
    {
        return string.Empty;
    }

    /// <summary>
    /// This method will provide Detail URL for MSTime Tenant.
    /// </summary>
    /// <param name="urlFormat">The URL format.</param>
    /// <param name="approvalIdentifier">The approval identifier.</param>
    /// <param name="page">The page.</param>
    /// <param name="xcv">The XCV.</param>
    /// <param name="tcv">The TCV.</param>
    /// <param name="businessProcessName">Name of the business process.</param>
    /// <param name="docTypeId">document type id</param>
    /// <returns></returns>
    protected override async Task<string> GetDetailURL(string urlFormat, ApprovalIdentifier approvalIdentifier, int page, string xcv = "", string tcv = "", string businessProcessName = "", string docTypeId = "")
    {
        return string.Format(urlFormat, approvalIdentifier.DocumentNumber, approvalIdentifier.FiscalYear);
    }

    /// <summary>
    /// This method will return the HTTP method for action. i.e. Patch.
    /// </summary>
    /// <returns>The HttpMethod</returns>
    protected override HttpMethod GetHttpMethodForAction()
    {
        return new HttpMethod(Constants.HTTPMethodPatch);
    }

    /// <summary>
    /// Returns the Title and Description which needs to be stored in database for display purpose
    /// </summary>
    /// <param name="summaryJSON"></param>
    /// <returns></returns>
    public override string GetRequestTitleDescription(JObject summaryJSON)
    {
        return summaryJSON["title"]?.ToString() + ": " + Convert.ToDateTime(summaryJSON["description"]).ToString("MMM dd, yyyy");
    }
}