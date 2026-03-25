// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace Microsoft.CFS.Approvals.Common.DL.Interface;

using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using global::Azure.Messaging.ServiceBus;
using Microsoft.CFS.Approvals.Contracts;
using Microsoft.CFS.Approvals.Contracts.DataContracts;
using Microsoft.CFS.Approvals.Model;

public interface IApprovalSummaryProvider
{
    /// <summary>
    /// Get approval counts by approver
    /// </summary>
    /// <param name="approver"></param>
    /// <returns></returns>
    Task<ApprovalCount[]> GetApprovalCounts(string approver);

    /// <summary>
    /// Get approvals summary json by approver and tenants
    /// </summary>
    /// <param name="approver"></param>
    /// <param name="tenants"></param>
    /// <param name="approverId">Approver Alias's object Id</param>
    /// <param name="approverDomain">Approver Alias's domain</param>
    /// <param name="isSubmittedRequest">Flag to indicate if the request is for submitted approvals</param>
    /// <returns></returns>
    List<ApprovalSummaryData> GetApprovalSummaryJsonByApproverAndTenants(string approver, List<ApprovalTenantInfo> tenants, string approverId, string approverDomain, bool isSubmittedRequest = false);

    /// <summary>
    /// Get approval summary count json by approver and tenants
    /// </summary>
    /// <param name="approver"></param>
    /// <param name="tenants"></param>
    /// <param name="approverId">Approver Alias's object Id</param>
    /// <param name="approverDomain">Approver Alias's domain</param>
    /// <returns></returns>
    List<ApprovalSummaryRow> GetApprovalSummaryCountJsonByApproverAndTenants(string approver, List<ApprovalTenantInfo> tenants, string approverId, string approverDomain);

    /// <summary>
    /// Get approval summary by document number
    /// </summary>
    /// <param name="documentTypeID"></param>
    /// <param name="documentNumber"></param>
    /// <param name="approverAlias"></param>
    /// <param name="approverId"></param>
    /// <param name="domain"></param>
    /// <returns></returns>
    ApprovalSummaryRow GetApprovalSummaryByDocumentNumber(string documentTypeID, string documentNumber, string approverAlias, string approverId, string domain);

    /// <summary>
    /// Get approvals summary by document number including soft delete data
    /// </summary>
    /// <param name="documentTypeID"></param>
    /// <param name="documentNumber"></param>
    /// <param name="approverAlias"></param>
    /// <param name="approverId"></param>
    /// <param name="approverDomain"></param>
    /// <returns></returns>
    ApprovalSummaryRow GetApprovalSummaryByDocumentNumberIncludingSoftDeleteData(string documentTypeID, string documentNumber, string approverAlias, string approverId, string approverDomain);

    /// <summary>
    /// Get approval summary by document number and approver
    /// </summary>
    /// <param name="documentTypeID"></param>
    /// <param name="documentNumber"></param>
    /// <param name="approverAlias"></param>
    /// <param name="approverId">In case external users allowed: ObjectId else Alias</param>
    /// <param name="approverDomain"></param>
    /// <returns></returns>
    ApprovalSummaryRow GetApprovalSummaryByDocumentNumberAndApprover(string documentTypeID, string documentNumber, string approverAlias, string approverId, string approverDomain);

    /// <summary>
    /// Get approval summary by RowKey and approver
    /// </summary>
    /// <param name="documentTypeID"></param>
    /// <param name="rowKey"></param>
    /// <param name="approverAlias"></param>
    /// <param name="approverId"></param>
    /// <param name="approverDomain"></param>
    /// <returns></returns>
    ApprovalSummaryRow GetApprovalSummaryByRowKeyAndApprover(string documentTypeID, string rowKey, string approverAlias, string approverId, string approverDomain);

    /// <summary>
    /// Get document summary by RowKey
    /// </summary>
    /// <param name="rowKey"></param>
    /// <returns></returns>
    List<ApprovalSummaryRow> GetDocumentSummaryByRowKey(string rowKey);

    /// <summary>
    /// Add approval summary
    /// </summary>
    /// <param name="tenant"></param>
    /// <param name="approvalRequest"></param>
    /// <param name="summaryRows"></param>
    /// <returns></returns>
    Task<bool> AddApprovalSummary(ApprovalTenantInfo tenant, ApprovalRequestExpression approvalRequest, List<ApprovalSummaryRow> summaryRows);

    /// <summary>
    /// Remove approval summary
    /// </summary>
    /// <param name="approvalRequest"></param>
    /// <param name="summaryRows"></param>
    /// <param name="message"></param>
    /// <param name="tenantInfo"></param>
    /// <returns></returns>
    Task<AzureTableRowDeletionResult> RemoveApprovalSummary(ApprovalRequestExpressionExt approvalRequest, List<ApprovalSummaryRow> summaryRows, ServiceBusReceivedMessage message, ApprovalTenantInfo tenantInfo);

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

    /// <summary>
    /// Set next reminder time
    /// </summary>
    /// <param name="row"></param>
    /// <param name="currentTime"></param>
    void SetNextReminderTime(ApprovalSummaryRow row, DateTime currentTime);

    /// <summary>
    /// Apply case constraints.
    /// </summary>
    /// <param name="row"></param>
    void ApplyCaseConstraints(ApprovalSummaryRow row);

    /// <summary>
    /// Finds a summary row by approver and document number without requiring a documentTypeID.
    /// Queries ApprovalSummary with PartitionKey scoped to the approver and a DocumentNumber column filter.
    /// </summary>
    /// <param name="documentNumber">The document number to search for.</param>
    /// <param name="approverAlias">The approver alias.</param>
    /// <param name="approverId">The approver's object ID (for external users).</param>
    /// <param name="approverDomain">The approver's domain.</param>
    /// <returns>The first matching ApprovalSummaryRow, or null if not found.</returns>
    Task<ApprovalSummaryRow> FindSummaryByApproverAndDocumentNumberAsync(string documentNumber, string approverAlias, string approverId, string approverDomain);
}