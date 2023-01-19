// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace Microsoft.CFS.Approvals.Common.DL.Helpers;

using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.CFS.Approvals.Common.DL.Interface;
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
    public async Task<List<ActionAuditLogInfo>> GetActionAuditLogsByDocumentNumberAndApprover(string documentNumber, string actualApprover)
    {
        return await _logger.GetActionAuditLogsByDocumentNumberAndApprover(documentNumber, actualApprover);
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
