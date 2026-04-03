// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace Microsoft.CFS.Approvals.Core.BL.Interface;

using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using global::Azure.Messaging.ServiceBus;
using Microsoft.CFS.Approvals.Contracts;
using Microsoft.CFS.Approvals.Contracts.DataContracts;
using Microsoft.CFS.Approvals.Domain.BL.Interface;
using Microsoft.CFS.Approvals.Model;
using Newtonsoft.Json.Linq;

public interface ISummaryHelper
{
    #region CREATE

    /// <summary>
    /// Add approval summary
    /// </summary>
    /// <param name="tenant"></param>
    /// <param name="approvalRequest"></param>
    /// <param name="summaryRows"></param>
    /// <returns></returns>
    Task<bool> AddApprovalSummary(ApprovalTenantInfo tenant, ApprovalRequestExpression approvalRequest, List<ApprovalSummaryRow> summaryRows);

    /// <summary>
    /// Create summary data list
    /// </summary>
    /// <param name="approvalsData"></param>
    /// <param name="tenants"></param>
    /// <param name="checkTenantUserDelegationEnable"></param>
    /// <returns></returns>
    List<ApprovalSummaryData> CreateSummaryDataList(List<ApprovalSummaryData> approvalsData, List<ApprovalTenantInfo> tenants, bool checkTenantUserDelegationEnable = false);

    #endregion CREATE

    #region READ

    /// <summary>
    /// Get summary data
    /// </summary>
    /// <param name="documentNumber"></param>
    /// <param name="fiscalYear"></param>
    /// <param name="tenantInfo"></param>
    /// <param name="alias"></param>
    /// <param name="approverId"></param>
    /// <param name="approverDomain"></param>
    /// <param name="loggedInAlias"></param>
    /// <param name="xcv"></param>
    /// <param name="tcv"></param>
    /// <param name="tenantAdaptor"></param>
    /// <returns></returns>
    ApprovalSummaryRow GetSummaryData(string documentNumber, string fiscalYear, ApprovalTenantInfo tenantInfo, string alias, string approverId, string approverDomain, string loggedInAlias, string xcv, string tcv, ITenant tenantAdaptor);

    /// <summary>
    /// Get summary
    /// </summary>
    /// <param name="signedInUser"></param>
    /// <param name="onBehalfUser"></param>
    /// <param name="host"></param>
    /// <param name="sessionId"></param>
    /// <param name="approverId">Approver Alias's object Id</param>
    /// <param name="domain">Alias's Domain</param>
    /// <param name="oauth2UserToken"></param>
    /// <param name="tenantDocTypeId"></param>
    /// <param name="isSubmittedRequest">flag to get submitted requests</param>
    /// <returns></returns>
    Task<JArray> GetSummary(User signedInUser, User onBehalfUser, string host, string sessionId, string oauth2UserToken, string tenantDocTypeId = "", bool isSubmittedRequest = false);

    /// <summary>
    /// Get other summary requests
    /// </summary>
    /// <param name="signedInUser"></param>
    /// <param name="onBehalfUser"></param>
    /// <param name="host"></param>
    /// <param name="viewType"></param>
    /// <param name="sessionId"></param>
    /// <param name="approverId">Approver Alias's object Id</param>
    /// <param name="domain">Alias's Domain</param>
    /// <param name="oauth2UserToken"></param>
    /// <param name="tenantDocTypeId"></param>
    /// <returns></returns>
    Task<JArray> GetOtherSummaryRequests(User signedInUser, User onBehalfUser, string host, string viewType, string sessionId, string approverId, string domain, string oauth2UserToken, string tenantDocTypeId = "");

    /// <summary>
    /// Get summary count data
    /// </summary>
    /// <param name="signedInUser"></param>
    /// <param name="onBehalfUser"></param>
    /// <param name="tenantDocTypeId"></param>
    /// <param name="sessionId"></param>
    /// <param name="clientDevice"></param>
    /// <param name="domain">Alias's Domain</param>
    /// <param name="oauth2UserToken"></param>
    /// <returns></returns>
    Task<JArray> GetSummaryCountData(User signedInUser, User onBehalfUser, string tenantDocTypeId, string sessionId, string clientDevice, string domain, string oauth2UserToken);

    /// <summary>
    /// Get other summary requests count data
    /// </summary>
    /// <param name="tenantDocTypeId"></param>
    /// <param name="loggedInAlias"></param>
    /// <param name="userAlias"></param>
    /// <param name="viewType"></param>
    /// <param name="sessionId"></param>
    /// <param name="clientDevice"></param>
    /// <param name="approverId">Approver Alias's object Id</param>
    /// <param name="domain">Alias's domain</param>
    /// <returns></returns>
    Task<JArray> GetOtherSummaryRequestsCountData(string tenantDocTypeId, string loggedInAlias, string userAlias, string viewType, string sessionId, string clientDevice, string approverId, string domain);

    /// <summary>
    /// Get Approval SummaryJson Approver for multiple Tenants for given view type
    /// </summary>
    /// <param name="approver">approver alias</param>
    /// <param name="tenants">List<ApprovalTenantInfo> - list of tenants</param>
    /// <param name="approverId">Approver Alias's object Id</param>
    /// <param name="domain">Alias's Domain</param>
    /// <param name="viewType">Default value 'Summary'</param>
    /// <param name="isSubmittedRequest">flag to get submitted requests</param>
    /// <returns>List of approval summary data</returns>
    List<ApprovalSummaryData> GetApprovalSummaryJsonByApproverAndTenants(string approver, List<ApprovalTenantInfo> tenants, string approverId, string domain, string viewType = "Summary", bool isSubmittedRequest = false);

    /// <summary>
    /// Get ApprovalSummaryCountJson By Approver for multiple tenants
    /// </summary>
    /// <param name="approver">approver alias</param>
    /// <param name="tenants">List<ApprovalTenantInfo> - list of tenants</param>
    /// <param name="approverId">Approver Alias's object Id</param>
    /// <param name="domain">Alias's Domain</param>
    /// <param name="viewType">Default value 'Summary'</param>
    /// <returns>JArray- with fields Id, tenantName and count</returns>
    JArray GetApprovalSummaryCountJsonByApproverAndTenants(string approver, List<ApprovalTenantInfo> tenants, string approverId, string domain, string viewType = "Summary");

    /// <summary>
    /// Get Approval Counts by approver
    /// </summary>
    /// <param name="approver"></param>
    /// <returns></returns>
    Task<ApprovalCount[]> GetApprovalCounts(string approver);

    /// <summary>
    /// Get Approval Summary By Document Number and Approver
    /// </summary>
    /// <param name="documentTypeID"></param>
    /// <param name="documentNumber"></param>
    /// <param name="approverAlias"></param>
    /// <param name="approverId"></param>
    /// <param name="approverDomain"></param>
    /// <returns></returns>
    ApprovalSummaryRow GetApprovalSummaryByDocumentNumberAndApprover(string documentTypeID, string documentNumber, string approverAlias, string approverId, string approverDomain);

    #endregion READ

    #region UPDATE

    /// <summary>
    /// Update summary
    /// </summary>
    /// <param name="tenant"></param>
    /// <param name="documentNumber"></param>
    /// <param name="approverAlias"></param>
    /// <param name="approverId"></param>
    /// <param name="approverDomain"></param>
    /// <param name="actionDate"></param>
    /// <param name="actionName"></param>
    /// <returns></returns>
    Task UpdateSummary(ApprovalTenantInfo tenant, string documentNumber, string approverAlias, string approverId, string approverDomain, DateTime? actionDate, string actionName);

    /// <summary>
    /// Update summary
    /// </summary>
    /// <param name="tenant"></param>
    /// <param name="summaryRow"></param>
    /// <param name="actionDate"></param>
    /// <param name="actionName"></param>
    Task UpdateSummary(ApprovalTenantInfo tenant, ApprovalSummaryRow summaryRow, DateTime? actionDate, string actionName);

    /// <summary>
    /// Update summary in batch async
    /// </summary>
    /// <param name="summaryRows"></param>
    /// <param name="xcv"></param>
    /// <param name="sessionId"></param>
    /// <param name="tcv"></param>
    /// <param name="tenantInfo"></param>
    /// <param name="actionName"></param>
    /// <returns></returns>
    Task UpdateSummaryInBatchAsync(List<ApprovalSummaryRow> summaryRows, string xcv, string sessionId, string tcv, ApprovalTenantInfo tenantInfo, string actionName);

    #endregion UPDATE

    #region DELETE

    /// <summary>
    /// Remove approval summary
    /// </summary>
    /// <param name="approvalRequest"></param>
    /// <param name="summaryRows"></param>
    /// <param name="message"></param>
    /// <param name="tenantInfo"></param>
    /// <returns></returns>
    Task<AzureTableRowDeletionResult> RemoveApprovalSummary(ApprovalRequestExpressionExt approvalRequest, List<ApprovalSummaryRow> summaryRows, ServiceBusReceivedMessage message, ApprovalTenantInfo tenantInfo);

    #endregion DELETE
}