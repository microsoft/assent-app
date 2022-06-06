// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace Microsoft.CFS.Approvals.DevTools.Model.Constant
{
    public enum ServiceBusMessageType
    {
        Active,
        DeadLetter
    }
    public enum FlightingFeatureStatus : int
    {
        Disabled = 1, // The feature is disabled
        EnabledForAll = 2, // Not a flighting feature; feature in production
        InFlighting = 3 // The feature is a flighting feature
    }
}
