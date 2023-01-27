// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace Microsoft.CFS.Approvals.Common.DL.Interface;

public interface IFlightingDataProvider
{
    /// <summary>
    /// Check if feature is enabled for user
    /// </summary>
    /// <param name="alias"></param>
    /// <param name="featureID"></param>
    /// <returns></returns>
    bool IsFeatureEnabledForUser(string alias, int featureID);
}
