// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace Microsoft.CFS.Approvals.Domain.BL.Tenants.Core;

using System.Collections.Generic;
using Microsoft.CFS.Approvals.Common.DL.Interface;
using Microsoft.CFS.Approvals.Contracts;
using Microsoft.CFS.Approvals.Contracts.DataContracts;
using Microsoft.CFS.Approvals.Data.Azure.Storage.Interface;
using Microsoft.CFS.Approvals.Extensions;
using Microsoft.CFS.Approvals.LogManager.Provider.Interface;
using Microsoft.CFS.Approvals.Model;
using Microsoft.CFS.Approvals.Utilities.Interface;
using Microsoft.Extensions.Configuration;

/// <summary>
/// Class Cosmic
/// </summary>
/// <seealso cref="GenericTenant" />
public class Cosmic : GenericTenant
{
    #region CONSTRUCTOR

    public Cosmic(
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

    public Cosmic(
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
    /// Get Additional Data
    /// </summary>
    /// <param name="additionalData">Additional Data for the request</param>
    /// <param name="displayDocumentNumber">display document number for the request</param>
    /// <param name="tenantId">tenant id</param>
    /// <returns>Returns a dictionary object of additional data string,string</returns>
    public override Dictionary<string, string> GetAdditionalData(Dictionary<string, string> additionalData, string displayDocumentNumber, int tenantId)
    {
        //Get the approvals detail for particular display document number
        ApprovalDetailsEntity additionalDetailsRow = ApprovalDetailProvider.GetApprovalDetailsByOperation(tenantId, displayDocumentNumber, Constants.AdditionalDetails, approvalTenantInfo.DocTypeId).Result;
        Dictionary<string, string> additionalDetailsData = null;
        //if additional details row contains data, retrive the additional data from the same
        if (additionalDetailsRow != null)
        {
            additionalDetailsData = additionalDetailsRow.JSONData.ToJObject()[Constants.AdditionalData]?.ToString().FromJson<Dictionary<string, string>>();
        }

        if (additionalDetailsData != null)
        {
            if (additionalData == null)
            {
                additionalData = new Dictionary<string, string>();
            }
            //check if ApproverDetailsIdKey is present, in that case add it to the additional data to be sent as part of ApproverRequest
            if (additionalDetailsData.ContainsKey(Constants.CosmicApproverDetailsIdKey))
            {
                additionalData.Add(Constants.CosmicApproverDetailsIdKey, additionalDetailsData[Constants.CosmicApproverDetailsIdKey].ToString());
            }
        }

        return additionalData;
    }

    #region Get Approver List for Sending Notifications

    /// <summary>
    /// Gets the email aliases of approvers for whom the notification should be sent.
    /// </summary>
    /// <param name="summaryRow">The summary row</param>
    /// <param name="notificationDetails">Notification details object</param>
    /// <returns>An email alias(es) to whom the notification should be sent</returns>
    public override string GetApproverListForSendingNotifications(ApprovalSummaryRow summaryRow, NotificationDetail notificationDetails)
    {
        return summaryRow.Approver + Config[ConfigurationKey.DomainName.ToString()];
    }

    #endregion Get Approver List for Sending Notifications
}