// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace Microsoft.CFS.Approvals.LogManager.OpenTelemetry;

using Microsoft.CFS.Approvals.Contracts;
using Microsoft.CFS.Approvals.LogManager.Interface;
public class AuditLogger : IAuditLogger
{
    /// <summary>
    /// AuditLogger Constructor
    /// </summary>
    public AuditLogger()
    {
    }

    /// <summary>
    /// This method is used to log data.
    /// </summary>
    /// <param name="operationName"></param>
    /// <param name="operationType"></param>
    /// <param name="user"></param>
    /// <param name="serviceName"></param>
    /// <param name="resourceType"></param>
    /// <param name="resourceName"></param>
    /// <param name="opResult"></param>
    /// <param name="message"></param>
    public virtual void LogAudit(string operationName, AuditOperationType operationType, string user, string serviceName, string resourceType, string resourceName, AuditOperationResult opResult, string message = null)
    {

    }
}
