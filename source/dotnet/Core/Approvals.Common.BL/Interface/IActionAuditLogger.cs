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
        /// Gets the action audit logs by document number and approver.
        /// </summary>
        /// <param name="documentNumber">The document number.</param>
        /// <param name="actualApprover">The actual approver.</param>
        /// <returns>List of action audit log table rows as per the queried results</returns>
        Task<List<ActionAuditLogInfo>> GetActionAuditLogsByDocumentNumberAndApprover(string documentNumber, string actualApprover);

        /// <summary>
        /// This method will log ActionAuditLog into storage
        /// </summary>
        /// <param name="actionAuditLogs">List of ActionAuditLogInfo</param>
        Task LogActionDetailsAsync(List<ActionAuditLogInfo> actionAuditLogs);
    }
}
