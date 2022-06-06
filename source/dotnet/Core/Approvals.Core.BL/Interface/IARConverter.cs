// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace Microsoft.CFS.Approvals.Core.BL.Interface
{
    using System.Collections.Generic;
    using Microsoft.Azure.ServiceBus;
    using Microsoft.CFS.Approvals.Contracts.DataContracts;
    using Microsoft.CFS.Approvals.Model;

    public interface IARConverter
    {
        public List<ApprovalRequestExpressionExt> GetAR(byte[] request, Message message, ApprovalTenantInfo tenantInfo);
    }
}