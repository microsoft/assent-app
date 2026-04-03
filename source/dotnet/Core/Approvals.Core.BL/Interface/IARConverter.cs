// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace Microsoft.CFS.Approvals.Core.BL.Interface;

using System.Collections.Generic;
using global::Azure.Messaging.ServiceBus;
using Microsoft.CFS.Approvals.Contracts.DataContracts;
using Microsoft.CFS.Approvals.Model;

public interface IARConverter
{
    /// <summary>
    /// Get the Approval Request
    /// </summary>
    /// <param name="request"></param>
    /// <param name="message"></param>
    /// <param name="tenantInfo"></param>
    /// <returns>List of Approval Request Expression</returns>
    public List<ApprovalRequestExpressionExt> GetAR(byte[] request, ServiceBusReceivedMessage message, ApprovalTenantInfo tenantInfo);
}