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
        /// To log action details into azure storage and document db
        /// </summary>
        /// <param name="documentNumber">Document number of the action audit log</param>
        /// <param name="impersonatedUser">Delegated user</param>
        /// <param name="actualApprover">Original user</param>
        /// <param name="actionType">Type of action i.e. Approve/Reject etc</param>
        /// <param name="clientDevice">device from which the action is taken</param>
        /// <param name="tenantId"></param>
        /// <param name="actionDate">Date time of the action taken</param>
        /// <param name="summaryJson">SummaryJson to acquire further logging details as "UnitValue" and "UnitOfMeasure"</param>
        /// <returns></returns>
        Task LogActionDetails(string documentNumber, string impersonatedUser, string actualApprover, string actionType, string clientDevice, string tenantId, DateTime actionDate, SummaryJson summaryJson);

        /// <summary>
        /// Gets the action audit logs by using documentnumber and approver
        /// </summary>
        /// <param name="documentNumber">Document number of the requested log</param>
        /// <param name="actualApprover">Original approver</param>
        /// <returns>List of action audit log rows as per the query</returns>
        List<ActionAuditLogTableRow> GetActionAuditLogsByDocumentNumberAndApprover(string documentNumber, string actualApprover);

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
