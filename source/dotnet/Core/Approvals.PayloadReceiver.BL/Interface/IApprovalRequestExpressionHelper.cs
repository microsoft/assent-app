// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace Microsoft.CFS.Approvals.PayloadReceiver.BL.Interface
{
    using System;

    public interface IApprovalRequestExpressionHelper
    {
        Type GetCurrrentApprovalRequestExpressionType(string tenantId);
    }
}
