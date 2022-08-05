// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace Microsoft.CFS.Approvals.Common.DL.Interface
{
    using System;
    using System.Collections.Generic;
    using System.Threading.Tasks;
    using Microsoft.CFS.Approvals.Contracts.DataContracts;
    using Microsoft.CFS.Approvals.Model;
    public interface IActionAuditLogHelper
    {
        /// <summary>
        /// Gets the action audit logs by using documentnumber and approver
        /// </summary>
        /// <param name="documentNumber">Document number of the requested log</param>
        /// <param name="actualApprover">Original approver</param>
        /// <returns>List of action audit log rows as per the query</returns>
        Task<List<ActionAuditLogInfo>> GetActionAuditLogsByDocumentNumberAndApprover(string documentNumber, string actualApprover);

        /// <summary>
        /// This method will log batch of ActionAuditLog into storage
        /// </summary>
        /// <param name="actionAuditLogs">List of ActionAuditLogInfo</param>
        Task LogActionDetailsAsync(List<ActionAuditLogInfo> actionAuditLogs);

        /// <summary>
        /// This method will log AuctionAuditLog into storage
        /// </summary>
        /// <param name="actionAuditLog">The ActionAuditLogInfo</param>
        Task LogActionDetailsAsync(ActionAuditLogInfo actionAuditLog);
    }
}
