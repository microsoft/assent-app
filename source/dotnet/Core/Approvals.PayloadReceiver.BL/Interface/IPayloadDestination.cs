// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace Microsoft.CFS.Approvals.PayloadReceiver.BL.Interface
{
    using Microsoft.CFS.Approvals.Model;

    public interface IPayloadDestination
    {
        PayloadDestinationInfo GetPayloadDestinationAndConfigInfo(string tenantId);
    }
}
