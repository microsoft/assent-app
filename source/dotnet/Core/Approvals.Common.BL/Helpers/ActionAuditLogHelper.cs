// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace Microsoft.CFS.Approvals.Common.DL.Helpers
{
    using System;
    using System.Collections.Generic;
    using System.Threading.Tasks;
    using Microsoft.CFS.Approvals.Common.DL.Interface;
    using Microsoft.CFS.Approvals.Contracts.DataContracts;
    using Microsoft.CFS.Approvals.Model;

    /// <summary>
    /// The Action Audit Log Helper
    /// </summary>
    public class ActionAuditLogHelper : IActionAuditLogHelper
    {
        /// <summary>
        /// The action audit _logger
        /// </summary>
        private readonly IActionAuditLogger _logger;

        /// <summary>
        /// Constructor of ActionAuditLogHelper
        /// </summary>
        /// <param name="_logger"></param>
        public ActionAuditLogHelper(IActionAuditLogger logger)
        {
            _logger = logger;
        }
        #region Implemented Methods

        /// <summary>
        /// Get action audit logs by document number and approver
        /// </summary>
        /// <param name="documentNumber"></param>
        /// <param name="actualApprover"></param>
        /// <returns></returns>
        public List<ActionAuditLogTableRow> GetActionAuditLogsByDocumentNumberAndApprover(string documentNumber, string actualApprover)
        {
            return _logger.GetActionAuditLogsByDocumentNumberAndApprover(documentNumber, actualApprover);
        }
        
        /// <summary>
        /// Method to log action audit details.
        /// </summary>
        /// <param name="documentNumber"></param>
        /// <param name="impersonatedUser"></param>
        /// <param name="actualApprover">original approver</param>
        /// <param name="actionType"> type of action i.e. approve, reject, complete etc.</param>
        /// <param name="clientDevice">device from which action is taken</param>
        /// <param name="tenantId"></param>
        /// <param name="actionDate"> action time & date of action</param>
        /// <param name="summaryJson">additional parameter for audit action logging details</param>
        /// <returns></returns>
        public Task LogActionDetails(string documentNumber, string impersonatedUser, string actualApprover, string actionType, string clientDevice, string tenantId, DateTime actionDate, SummaryJson summaryJson)
        {
            return _logger.LogActionDetailsAsync(documentNumber, impersonatedUser, actualApprover, actionType, clientDevice, tenantId, actionDate, summaryJson);
        }

        /// <summary>
        /// This method will log list of ActionAuditLog into storage
        /// </summary>
        /// <param name="actionAuditLogs">List of ActionAuditLogInfo</param>
        public async Task LogActionDetailsAsync(List<ActionAuditLogInfo> actionAuditLogs)
        {
            await _logger.LogActionDetailsAsync(actionAuditLogs);
        }

        /// <summary>
        /// This method will log ActionAuditLog into storage
        /// </summary>
        /// <param name="actionAuditLog">The ActionAuditLogInfo</param>
        public async Task LogActionDetailsAsync(ActionAuditLogInfo actionAuditLog)
        {
            var actionAuditLogs = new List<ActionAuditLogInfo> { actionAuditLog };
            await _logger.LogActionDetailsAsync(actionAuditLogs);
        }

        #endregion Implemented Methods
    }
}
