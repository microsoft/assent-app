// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace Microsoft.CFS.Approvals.Common.DL.Interface
{
    using System;
    using System.Collections.Generic;
    using System.Threading.Tasks;
    using Microsoft.CFS.Approvals.Contracts.DataContracts;
    using Microsoft.CFS.Approvals.Model;
    public interface IActionAuditLogger
    {
        /// <summary>
        /// Logs the action details asynchronous.
        /// </summary>
        /// <param name="documentNumber">The document number.</param>
        /// <param name="impersonatedUser">The impersonated user.</param>
        /// <param name="actualApprover">The actual approver.</param>
        /// <param name="actionType">Type of the action.</param>
        /// <param name="clientDevice">The client device.</param>
        /// <param name="tenantId">The tenant identifier.</param>
        /// <param name="actionTime">The action time.</param>
        /// <param name="summaryJson">Parameter for ActionAuditLogs in Cosmos DB</param>
        /// <returns>Task.</returns>
        Task LogActionDetailsAsync(string documentNumber, string impersonatedUser, string actualApprover, string actionType, string clientDevice, string tenantId, DateTime actionTime, SummaryJson summaryJson);

        /// <summary>
        /// Gets the action audit logs by document number and approver.
        /// </summary>
        /// <param name="documentNumber">The document number.</param>
        /// <param name="actualApprover">The actual approver.</param>
        /// <returns>List of action audit log table rows as per the queried results</returns>
        List<ActionAuditLogTableRow> GetActionAuditLogsByDocumentNumberAndApprover(string documentNumber, string actualApprover);

        /// <summary>
        /// This method will log ActionAuditLog into storage
        /// </summary>
        /// <param name="actionAuditLogs">List of ActionAuditLogInfo</param>
        Task LogActionDetailsAsync(List<ActionAuditLogInfo> actionAuditLogs);
    }
}
