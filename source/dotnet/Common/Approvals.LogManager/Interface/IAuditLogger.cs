// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace Microsoft.CFS.Approvals.LogManager.Interface;

using Microsoft.CFS.Approvals.Contracts;
public interface IAuditLogger
{
    /// <summary>
    /// OpenTelemetry audit log
    /// </summary>
    /// <param name="operationName"></param>
    /// <param name="operationType"></param>
    /// <param name="user"></param>
    /// <param name="serviceName"></param>
    /// <param name="resourceType"></param>
    /// <param name="resourceName"></param>
    /// <param name="opResult"></param>
    /// <param name="message"></param>
    void LogAudit(string operationName, AuditOperationType operationType, string user, string serviceName, string resourceType, string resourceName, AuditOperationResult opResult, string message = null);
}
