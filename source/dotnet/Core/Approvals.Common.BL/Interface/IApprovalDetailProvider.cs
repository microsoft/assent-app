// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace Microsoft.CFS.Approvals.Common.DL.Interface;

using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.CFS.Approvals.Contracts;
using Microsoft.CFS.Approvals.Contracts.DataContracts;
using Microsoft.CFS.Approvals.Model;

public interface IApprovalDetailProvider
{
    /// <summary>
    /// Get Approval Details by Operation
    /// </summary>
    /// <param name="tenantId"></param>
    /// <param name="documentNumber"></param>
    /// <param name="operationName"></param>
    /// <returns></returns>
    Task<ApprovalDetailsEntity> GetApprovalDetailsByOperation(int tenantId, string documentNumber, string operationName);

    /// <summary>
    /// Get Approvals Details
    /// </summary>
    /// <param name="tenantId"></param>
    /// <param name="documentNumber"></param>
    /// <param name="operationName"></param>
    /// <returns></returns>
    ApprovalDetailsEntity GetApprovalsDetails(int tenantId, string documentNumber, string operationName);

    /// <summary>
    /// Get all approvals details
    /// </summary>
    /// <param name="tenantId"></param>
    /// <param name="documentNumber"></param>
    /// <returns></returns>
    List<ApprovalDetailsEntity> GetAllApprovalsDetails(int tenantId, string documentNumber);

    /// <summary>
    /// Get all approval details by tenant and document number
    /// </summary>
    /// <param name="tenantId"></param>
    /// <param name="documentNumber"></param>
    /// <returns></returns>
    Task<List<ApprovalDetailsEntity>> GetAllApprovalDetailsByTenantAndDocumentNumber(int tenantId, string documentNumber);

    /// <summary>
    /// Add Approvals Details
    /// </summary>
    /// <param name="detailsRows"></param>
    /// <param name="tenantInfo"></param>
    /// <param name="loggedInAlias"></param>
    /// <param name="xcv"></param>
    /// <param name="tcv"></param>
    /// <param name="isUserTriggered"></param>
    /// <returns></returns>
    Task<bool> AddApprovalsDetails(List<ApprovalDetailsEntity> detailsRows, ApprovalTenantInfo tenantInfo, string loggedInAlias, string xcv, string tcv, bool isUserTriggered = false);

    /// <summary>
    /// Add Transactional and historical data in approvals details
    /// </summary>
    /// <param name="detailsRow"></param>
    /// <param name="tenantInfo"></param>
    /// <param name="telemetry"></param>
    /// <returns></returns>
    bool AddTransactionalAndHistoricalDataInApprovalsDetails(ApprovalDetailsEntity detailsRow, ApprovalTenantInfo tenantInfo, ApprovalsTelemetry telemetry);

    /// <summary>
    /// Save edited data in approval details
    /// </summary>
    /// <param name="detailsRow"></param>
    /// <param name="telemetry"></param>
    /// <returns></returns>
    bool SaveEditedDataInApprovalDetails(ApprovalDetailsEntity detailsRow, ApprovalsTelemetry telemetry);

    /// <summary>
    /// Remove approvals details
    /// </summary>
    /// <param name="detailsRows"></param>
    /// <returns></returns>
    Task<AzureTableRowDeletionResult> RemoveApprovalsDetails(List<ApprovalDetailsEntity> detailsRows);

    /// <summary>
    /// Remove attachment from blob
    /// </summary>
    /// <param name="attachments"></param>
    /// <param name="approvalIdentifier"></param>
    /// <param name="operationName"></param>
    /// <param name="clientDevice"></param>
    /// <param name="tenantInfo"></param>
    /// <param name="telemetry"></param>
    /// <returns></returns>
    Task RemoveAttachmentFromBlob(List<Attachment> attachments, ApprovalIdentifier approvalIdentifier, string operationName, string clientDevice, ApprovalTenantInfo tenantInfo, ApprovalsTelemetry telemetry);

    /// <summary>
    /// Update details in batch async
    /// </summary>
    /// <param name="detailsEntities"></param>
    /// <param name="xcv"></param>
    /// <param name="sessionId"></param>
    /// <param name="tcv"></param>
    /// <param name="tenantInfo"></param>
    /// <param name="actionName"></param>
    /// <returns></returns>
    Task UpdateDetailsInBatchAsync(List<ApprovalDetailsEntity> detailsEntities, string xcv, string sessionId, string tcv, ApprovalTenantInfo tenantInfo, string actionName);
}