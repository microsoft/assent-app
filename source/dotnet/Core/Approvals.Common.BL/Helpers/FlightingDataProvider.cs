// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace Microsoft.CFS.Approvals.Common.DL
{
    using System;
    using System.Linq;
    using Microsoft.Azure.Cosmos.Table;
    using Microsoft.CFS.Approvals.Common.DL.Interface;
    using Microsoft.CFS.Approvals.Contracts;
    using Microsoft.CFS.Approvals.Data.Azure.Storage.Interface;
    using Microsoft.CFS.Approvals.Model.Flighting;

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
        /// Constructor of FlightingDataProvider
        /// </summary>
        /// <param name="tableHelper"></param>
        public FlightingDataProvider(ITableHelper tableHelper)
        {
            _tableHelper = tableHelper;
        }

        /// <summary>
        /// Check if an feature is enabled for an user
        /// </summary>
        /// <param name="alias"></param>
        /// <param name="featureID"></param>
        /// <returns></returns>
        public bool IsFeatureEnabledForUser(string alias, int featureID)
        {
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
                    string partitionKeyFilter = TableQuery.GenerateFilterCondition("PartitionKey", QueryComparisons.Equal, alias);
                    string featureIDFilter = TableQuery.GenerateFilterConditionForInt("FeatureID", QueryComparisons.Equal, featureID);
                    string flightingStartDateFilter = TableQuery.GenerateFilterConditionForDate("FlightingStartDate", QueryComparisons.GreaterThan, flightingStartDate);
                    TableQuery<Flighting> query = new TableQuery<Flighting>().Where(TableQuery.CombineFilters(TableQuery.CombineFilters(partitionKeyFilter, TableOperators.And, featureIDFilter), TableOperators.And, flightingStartDateFilter));
                    var allowAlias = _tableHelper.GetDataCollectionByTableQuery(Constants.FlightingAzureTableName, query).FirstOrDefault();
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
}