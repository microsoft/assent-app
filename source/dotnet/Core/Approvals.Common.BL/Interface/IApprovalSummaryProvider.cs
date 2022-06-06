// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace Microsoft.CFS.Approvals.Common.DL.Interface
{
    using System;
    using System.Collections.Generic;
    using System.Threading.Tasks;
    using Microsoft.Azure.ServiceBus;
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
        /// <returns></returns>
        List<ApprovalSummaryData> GetApprovalSummaryJsonByApproverAndTenants(string approver, List<ApprovalTenantInfo> tenants);

        /// <summary>
        /// Get approval summary count json by approver and tenants
        /// </summary>
        /// <param name="approver"></param>
        /// <param name="tenants"></param>
        /// <returns></returns>
        List<ApprovalSummaryRow> GetApprovalSummaryCountJsonByApproverAndTenants(string approver, List<ApprovalTenantInfo> tenants);

        /// <summary>
        /// Get approval summary by document number
        /// </summary>
        /// <param name="documentTypeID"></param>
        /// <param name="documentNumber"></param>
        /// <param name="approver"></param>
        /// <returns></returns>
        ApprovalSummaryRow GetApprovalSummaryByDocumentNumber(string documentTypeID, string documentNumber, string approver);

        /// <summary>
        /// Get approvals summary by document number including soft delete data
        /// </summary>
        /// <param name="documentTypeID"></param>
        /// <param name="documentNumber"></param>
        /// <param name="approver"></param>
        /// <returns></returns>
        ApprovalSummaryRow GetApprovalSummaryByDocumentNumberIncludingSoftDeleteData(string documentTypeID, string documentNumber, string approver);

        /// <summary>
        /// Get approval summary by document number and approver
        /// </summary>
        /// <param name="documentTypeID"></param>
        /// <param name="documentNumber"></param>
        /// <param name="approver"></param>
        /// <returns></returns>
        ApprovalSummaryRow GetApprovalSummaryByDocumentNumberAndApprover(string documentTypeID, string documentNumber, string approver);

        /// <summary>
        /// Get approval summary by RowKey and approver
        /// </summary>
        /// <param name="documentTypeID"></param>
        /// <param name="rowKey"></param>
        /// <param name="approver"></param>
        /// <returns></returns>
        ApprovalSummaryRow GetApprovalSummaryByRowKeyAndApprover(string documentTypeID, string rowKey, string approver);

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
        /// Update summary post action
        /// </summary>
        /// <param name="tenant"></param>
        /// <param name="SummaryRow"></param>
        /// <param name="actionDate"></param>
        /// <param name="actionName"></param>
        void UpdateSummaryPostAction(ApprovalTenantInfo tenant, ApprovalSummaryRow SummaryRow, DateTime? actionDate, string actionName);

        /// <summary>
        /// Remove approval summary
        /// </summary>
        /// <param name="approvalRequest"></param>
        /// <param name="summaryRows"></param>
        /// <param name="message"></param>
        /// <param name="tenantInfo"></param>
        /// <returns></returns>
        Task<AzureTableRowDeletionResult> RemoveApprovalSummary(ApprovalRequestExpressionExt approvalRequest, List<ApprovalSummaryRow> summaryRows, Message message, ApprovalTenantInfo tenantInfo);

        /// <summary>
        /// Update is read summary
        /// </summary>
        /// <param name="documentNumber"></param>
        /// <param name="approver"></param>
        /// <param name="tenantInfo"></param>
        /// <returns></returns>
        bool UpdateIsReadSummary(string documentNumber, string approver, ApprovalTenantInfo tenantInfo);

        /// <summary>
        /// Update summary is out of sync challenged
        /// </summary>
        /// <param name="tenant"></param>
        /// <param name="SummaryRow"></param>
        /// <param name="actionDate"></param>
        /// <param name="actionName"></param>
        void UpdateSummaryIsOutOfSyncChallenged(ApprovalTenantInfo tenant, ApprovalSummaryRow SummaryRow, DateTime? actionDate, string actionName);

        /// <summary>
        /// Update summary for offline approval
        /// </summary>
        /// <param name="tenant"></param>
        /// <param name="SummaryRow"></param>
        /// <param name="actionName"></param>
        void UpdateSummaryForOfflineApproval(ApprovalTenantInfo tenant, ApprovalSummaryRow SummaryRow, string actionName);

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
    }
}