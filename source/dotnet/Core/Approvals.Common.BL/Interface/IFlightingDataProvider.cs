// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace Microsoft.CFS.Approvals.Common.DL.Interface;

using System.Collections.Generic;
using Microsoft.CFS.Approvals.Model.Flighting;

public interface IFlightingDataProvider
{
    /// <summary>
    /// Check if feature is enabled for user
    /// </summary>
    /// <param name="aliasOrUpn"></param>
    /// <param name="featureID"></param>
    /// <param name="domain"></param>
    /// <returns></returns>
    bool IsFeatureEnabledForUser(string aliasOrUpn, int featureID, string domain = "");

    /// <summary>
    /// Check feature enablement status for tenant. If it's flighting then check for flighted user
    /// </summary>
    /// <param name="tenantFeatureFlag"></param>
    /// <param name="userUpn"></param>
    /// <param name="featureId"></param>
    /// <param name="domain"></param>
    /// <returns></returns>
    bool IsFeatureEnabledForTenantAndUser(int tenantFeatureFlag, string userUpn, int featureId);

    /// <summary>
    /// Get enabled Flighting features for given alias
    /// </summary>
    /// <param name="userUpn"></param>
    /// <param name="domain"></param>
    /// <returns></returns>
    List<FlightingFeature> GetFlightingFeature(string userUpn);

    /// <summary>
    /// Get all Flighting features
    /// </summary>
    /// <returns></returns>
    List<FlightingFeature> GetAllFlightingFeature();
}
