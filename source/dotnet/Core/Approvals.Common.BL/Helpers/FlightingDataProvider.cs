// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace Microsoft.CFS.Approvals.Common.DL;

using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.CFS.Approvals.Common.DL.Interface;
using Microsoft.CFS.Approvals.Contracts;
using Microsoft.CFS.Approvals.Data.Azure.Storage.Interface;
using Microsoft.CFS.Approvals.Extensions;
using Microsoft.CFS.Approvals.Model.Flighting;
using Microsoft.CFS.Approvals.Utilities.Interface;
using Microsoft.Extensions.Configuration;

/// <summary>
/// The Flight Data Provider class
/// </summary>
public class FlightingDataProvider : IFlightingDataProvider
{
    /// <summary>
    /// The table helper
    /// </summary>
    private readonly ITableHelper _tableHelper;

    /// <summary>
    /// The configuration
    /// </summary>
    private readonly IConfiguration _config;

    /// <summary>
    /// The Name Resolution Helper
    /// </summary>
    private readonly INameResolutionHelper _nameResolutionHelper;

    /// <summary>
    /// Constructor of FlightingDataProvider
    /// </summary>
    /// <param name="tableHelper"></param>
    /// <param name="configuration"></param>
    /// <param name="nameResolutionHelper"></param>
    public FlightingDataProvider(ITableHelper tableHelper, IConfiguration configuration, INameResolutionHelper nameResolutionHelper)
    {
        _tableHelper = tableHelper;
        _config = configuration;
        _nameResolutionHelper = nameResolutionHelper;
    }

    /// <summary>
    /// Check if an feature is enabled for an user
    /// </summary>
    /// <param name="aliasOrUpn"></param>
    /// <param name="featureID"></param>
    /// <param name="domain"></param>
    /// <returns></returns>
    public bool IsFeatureEnabledForUser(string aliasOrUpn, int featureID, string domain = "")
    {
        if (aliasOrUpn.IsUpn())
        {
            domain = aliasOrUpn.GetDomainFromUPN();
        }
        else
        {
            if (string.IsNullOrWhiteSpace(domain))
                domain = _nameResolutionHelper.GetUserPrincipalName(aliasOrUpn).Result.GetDomainFromUPN();
            aliasOrUpn = aliasOrUpn + domain;
        }

        DateTime flightingStartDate = DateTime.UtcNow;
        FlightingFeatureStatus status = GetFeatureStatus(featureID);
        // TODO:: Get the default value from configuration, for enable/disable feature
        // Currently set to 'false' as default
        bool isEnabled;
        switch (status) // TODO:: cache this
        {
            case FlightingFeatureStatus.EnabledForAll: // (feature completely enabled)
                isEnabled = true;
                break;

            case FlightingFeatureStatus.InFlighting:
                string filterString = "PartitionKey eq '" + aliasOrUpn + "' and FeatureID eq " + featureID;
                var allowAlias = _tableHelper.GetDataCollectionByTableQuery<Flighting>(Constants.FlightingAzureTableName, filterString).FirstOrDefault();

                //Backward compatibility
                if (allowAlias == null && _config[Constants.OldWhitelistedDomains].Contains(domain, StringComparison.InvariantCultureIgnoreCase))
                {
                    filterString = "PartitionKey eq '" + aliasOrUpn.GetAliasFromUPN() + "' and FeatureID eq " + featureID;
                    allowAlias = _tableHelper.GetDataCollectionByTableQuery<Flighting>(Constants.FlightingAzureTableName, filterString).FirstOrDefault();
                }
                isEnabled = allowAlias != null;
                break;

            case FlightingFeatureStatus.Disabled:// (feature disabled)
                isEnabled = false;
                break;

            default:
                isEnabled = true;
                break;
        }
        return isEnabled;
    }

    /// <summary>
    /// Check feature enablement status for tenant. If it's flighting then check for flighted user
    /// </summary>
    /// <param name="tenantFeatureFlag"></param>
    /// <param name="userUpn"></param>
    /// <param name="featureId"></param>
    /// <returns></returns>
    public bool IsFeatureEnabledForTenantAndUser(int tenantFeatureFlag, string userUpn, int featureId)
    {
        var isEnabled = tenantFeatureFlag switch
        {
            (int)TenantLevelFlighting.DisableForAll => false,
            (int)TenantLevelFlighting.EnableForFlightedUsers => IsFeatureEnabledForUser(userUpn, featureId),
            (int)TenantLevelFlighting.EnableForAll => true,
            _ => false,
        };
        return isEnabled;
    }

    /// <summary>
    /// Get enabled Flighting features for given alias
    /// </summary>
    /// <param name="userUpn"></param>
    /// <returns></returns>
    public List<FlightingFeature> GetFlightingFeature(string userUpn)
    {
        List<Flighting> flightingList = _tableHelper.GetTableEntityListByPartitionKey<Flighting>(Constants.FlightingAzureTableName, userUpn);

        // Backward compatibility
        if (_config[Constants.OldWhitelistedDomains].Contains(userUpn.GetDomainFromUPN(), StringComparison.InvariantCultureIgnoreCase))
        {
            if (flightingList == null)
                flightingList = _tableHelper.GetTableEntityListByPartitionKey<Flighting>(Constants.FlightingAzureTableName, userUpn.GetAliasFromUPN());
            else
                flightingList.AddRange(_tableHelper.GetTableEntityListByPartitionKey<Flighting>(Constants.FlightingAzureTableName, userUpn.GetAliasFromUPN()));
        }

        List<FlightingFeature> flightingFeatureList = new List<FlightingFeature>();

        if (flightingList != null && flightingList.Count > 0)
        {
            var allFlightingFeatureList = _tableHelper.GetTableEntity<FlightingFeature>(Constants.FlightingFeatureAzureTableName)?.ToList();
            foreach (var flighting in flightingList)
            {
                var flightingFeature = allFlightingFeatureList?.Where(t => t.Id == flighting.FeatureID).ToList();
                flightingFeatureList.AddRange(flightingFeature);
            }
        }
        var flightingFeatureStatus = _tableHelper.GetTableEntity<FlightingFeatureStatusEntity>(Constants.FlightingFeatureStatusAzureTableName).ToList();
        var list = flightingFeatureList.Join(flightingFeatureStatus, feature => feature.FeatureStatusID, status => status.FeatureStatusID,
                                    (first, second) => new
                                    {
                                        first.PartitionKey,
                                        first.RowKey,
                                        first.Timestamp,
                                        first.Id,
                                        first.FeatureName,
                                        first.FeatureStatusID,
                                        first.FeatureStartDate,
                                        first.FeatureDescription,
                                        first.FlightingRing,
                                        first.FeatureLastUpdate,
                                        first.FeatureVersion,
                                        first.Flightings,
                                        FlightingStatus = second.FeatureStatus,
                                        first.QuickTourSlidesJson,
                                        first.TeachingCoach
                                    }).ToList();
        flightingFeatureList = list.ToJson().FromJson<List<FlightingFeature>>();
        return flightingFeatureList;
    }

    /// <summary>
    /// Get all Flighting features
    /// </summary>
    /// <returns></returns>
    public List<FlightingFeature> GetAllFlightingFeature()
    {
        var allFlightingFeatureList = _tableHelper.GetTableEntity<FlightingFeature>(Constants.FlightingFeatureAzureTableName)?.ToList();

        var flightingFeatureStatus = _tableHelper.GetTableEntity<FlightingFeatureStatusEntity>(Constants.FlightingFeatureStatusAzureTableName).ToList();
        var list = allFlightingFeatureList.Join(flightingFeatureStatus, feature => feature.FeatureStatusID, status => status.FeatureStatusID,
                                    (first, second) => new
                                    {
                                        first.PartitionKey,
                                        first.RowKey,
                                        first.Timestamp,
                                        first.Id,
                                        first.FeatureName,
                                        first.FeatureStatusID,
                                        first.FeatureStartDate,
                                        first.FeatureDescription,
                                        first.FlightingRing,
                                        first.FeatureLastUpdate,
                                        first.FeatureVersion,
                                        first.Flightings,
                                        FlightingStatus = second.FeatureStatus,
                                        first.QuickTourSlidesJson,
                                        first.TeachingCoach
                                    }).ToList();
        allFlightingFeatureList = list.ToJson().FromJson<List<FlightingFeature>>();
        return allFlightingFeatureList;
    }

    /// <summary>
    /// Gets the Feature status for the specified Feature
    /// Expected values:
    /// Disabled = 1 --- The feature is disabled
    /// EnabledForAll = 2 --- Not a flighting feature; feature is in production
    /// InFlighting = 3 --- The feature is a flighting feature
    /// </summary>
    /// <param name="featureID"></param>
    /// <returns></returns>
    private FlightingFeatureStatus GetFeatureStatus(int featureID)
    {
        var featureType = _tableHelper.GetTableEntityByRowKey<FlightingFeature>(Constants.FlightingFeatureAzureTableName, featureID.ToString());
        int flightingFeatureStatus = 0; // default state set to 0
        if (featureType != null)
        {
            flightingFeatureStatus = featureType.FeatureStatusID;
        }
        return (FlightingFeatureStatus)Enum.Parse(typeof(FlightingFeatureStatus), flightingFeatureStatus.ToString());
    }
}