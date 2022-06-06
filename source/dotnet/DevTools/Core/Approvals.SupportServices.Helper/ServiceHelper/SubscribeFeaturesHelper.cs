// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace Microsoft.CFS.Approvals.SupportServices.Helper.ServiceHelper
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;
    using Microsoft.AspNetCore.Mvc.Infrastructure;
    using Microsoft.CFS.Approvals.Data.Azure.Storage.Interface;
    using Microsoft.CFS.Approvals.DevTools.Model.Constant;
    using Microsoft.CFS.Approvals.DevTools.Model.Models;
    using Microsoft.CFS.Approvals.SupportServices.Helper.Interface;
    using Microsoft.CFS.Approvals.Utilities.Interface;
    using Newtonsoft.Json.Linq;

    /// <summary>
    /// The Subscribe Features Helper class
    /// </summary>
    public class SubscribeFeaturesHelper : ISubscribeFeaturesHelper
    {
        /// <summary>
        /// The name resolution helper
        /// </summary>
        private readonly INameResolutionHelper _nameResolutionHelper;

        /// <summary>
        /// The table storage helper
        /// </summary>
        private readonly ITableHelper _azureTableStorageHelper;

        private readonly string _environment;
        private readonly List<string> inValidAlias = new List<string>();
        private readonly List<string> alreadyFlightedAlias = new List<string>();
        private readonly List<string> subscribedAlias = new List<string>();
        private readonly List<string> failedAlias = new List<string>();
        private readonly List<string> alreadyNonFlightedAlias = new List<string>();
        private readonly List<string> unSubcribedAlias = new List<string>();

        /// <summary>
        /// Constructor of SubscribeFeaturesHelper
        /// </summary>
        /// <param name="nameResolutionHelper"></param>
        /// <param name="azureTableStorageHelper"></param>
        /// <param name="configurationHelper"></param>
        /// <param name="actionContextAccessor"></param>
        public SubscribeFeaturesHelper(
            INameResolutionHelper nameResolutionHelper,
            Func<string, string, ITableHelper> azureTableStorageHelper,
            ConfigurationHelper configurationHelper,
            IActionContextAccessor actionContextAccessor)
        {
            _nameResolutionHelper = nameResolutionHelper;
            _environment = actionContextAccessor?.ActionContext?.RouteData?.Values["env"]?.ToString();
            _azureTableStorageHelper = azureTableStorageHelper(
                configurationHelper.appSettings[_environment].StorageAccountName,
                configurationHelper.appSettings[_environment].StorageAccountKey);
        }

        /// <summary>
        /// Manage feature subcription
        /// </summary>
        /// <param name="featureDetail"></param>
        /// <returns></returns>
        public async Task<List<string>> ManageFeatureSubscription(JToken featureDetail)
        {
            List<string> result = new List<string>();
            var aliases = featureDetail["aliases"]?.ToString().Split(';');
            int featureId = Int32.Parse(featureDetail["featureId"]?.ToString());
            string featureName = featureDetail["featureName"]?.ToString();
            bool isBulkSubscribe = bool.Parse(featureDetail["isBulkSubscribeEnabled"]?.ToString());

            foreach (var alias in aliases)
            {
                if (!(await IsValidAlias(alias)))
                {
                    inValidAlias.Add(alias);
                }
                else
                {
                    if (isBulkSubscribe)
                    {
                        await Subscribe(alias, featureId);
                    }
                    else
                    {
                        await UnSubscribe(alias, featureId);
                    }
                }
            }
            if (subscribedAlias.Count > 0)
                result.Add(string.Format("Feature {0} successfully subscribed for aliases : {1}", featureName, string.Join(",", subscribedAlias)));
            if (unSubcribedAlias.Count > 0)
                result.Add(string.Format("Feature {0} successfully unsubscribed for aliases : {1}", featureName, string.Join(",", unSubcribedAlias)));
            if (inValidAlias.Count > 0)
                result.Add(string.Format("Invalid Aliases : {0} ", string.Join(",", inValidAlias)));
            if (failedAlias.Count > 0)
                result.Add(string.Format("Aliases failed for {0} to feature {1} : {2} ", (isBulkSubscribe ? "subscription" : "unsubscription"), featureName, string.Join(",", failedAlias)));
            if (alreadyFlightedAlias.Count > 0)
                result.Add(string.Format("Aliases already subscribed to the feature {0} : {1}", featureName, string.Join(",", alreadyFlightedAlias)));
            if (alreadyNonFlightedAlias.Count > 0)
                result.Add(string.Format("Aliases already unsubscribed to the feature {0} : {1}", featureName, string.Join(",", alreadyNonFlightedAlias)));

            return result;
        }

        /// <summary>
        /// Subscribe to feature
        /// </summary>
        /// <param name="alias"></param>
        /// <param name="featureID"></param>
        private async Task Subscribe(string alias, int featureID)
        {
            if (IsFeatureEnabledForUser(alias, featureID))
            {
                alreadyFlightedAlias.Add(alias);
            }
            else
            {
                FlightingEntity flighting = new FlightingEntity()
                {
                    PartitionKey = alias.ToString(),
                    RowKey = Guid.NewGuid().ToString(),
                    Alias = alias,
                    FeatureID = featureID,
                    FlightingStartDate = DateTime.UtcNow
                };
                if (await _azureTableStorageHelper.InsertOrReplace<FlightingEntity>("Flighting", flighting))
                {
                    subscribedAlias.Add(alias);
                }
                else
                {
                    failedAlias.Add(alias);
                }
            }
        }

        /// <summary>
        /// Unsubscribe to feature
        /// </summary>
        /// <param name="alias"></param>
        /// <param name="featureID"></param>
        private async Task UnSubscribe(string alias, int featureID)
        {
            if (!IsFeatureEnabledForUser(alias, featureID))
            {
                alreadyNonFlightedAlias.Add(alias);
            }
            else
            {
                var flighting = _azureTableStorageHelper.GetTableEntityListByPartitionKey<FlightingEntity>("Flighting", alias).Where(s => s.FeatureID == featureID).FirstOrDefault();
                if (flighting != null && await _azureTableStorageHelper.DeleteRow<FlightingEntity>("Flighting", flighting))
                {
                    unSubcribedAlias.Add(alias);
                }
                else
                {
                    failedAlias.Add(alias);
                }
            }
        }

        /// <summary>
        /// Check is feature enabled for user
        /// </summary>
        /// <param name="alias"></param>
        /// <param name="featureID"></param>
        /// <returns></returns>
        public bool IsFeatureEnabledForUser(string alias, int featureID)
        {
            bool isEnabled = false;

            DateTime flightingStartDate = DateTime.UtcNow;
            var feature = _azureTableStorageHelper.GetTableEntityByRowKey<FlightingFeatureEntity>("FlightingFeature", featureID.ToString());
            FlightingFeatureStatus status = (FlightingFeatureStatus)Enum.Parse(typeof(FlightingFeatureStatus), feature.FeatureStatusID.ToString());
            switch (status)
            {
                case FlightingFeatureStatus.EnabledForAll: // (feature completely enabled)
                    isEnabled = true;
                    break;

                case FlightingFeatureStatus.InFlighting:
                    var Flighting = _azureTableStorageHelper.GetTableEntity<FlightingEntity>("Flighting").Where(s =>
                       s.PartitionKey == alias.Trim().ToLower() && s.FeatureID == featureID && DateTime.UtcNow > s.FlightingStartDate).FirstOrDefault();
                    isEnabled = Flighting != null;
                    break;

                case FlightingFeatureStatus.Disabled:// (feature disabled)
                default:
                    isEnabled = false;
                    break;
            }
            return isEnabled;
        }

        /// <summary>
        /// Check is valid alias
        /// </summary>
        /// <param name="alias"></param>
        /// <returns></returns>
        public async Task<bool> IsValidAlias(string alias)
        {
            try
            {
                return await _nameResolutionHelper.IsValidUser(alias.Trim().ToLower());
            }
            catch
            {
                return false;
            }
        }
    }
}