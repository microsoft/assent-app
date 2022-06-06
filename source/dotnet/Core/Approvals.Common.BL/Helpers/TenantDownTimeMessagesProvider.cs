// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace Microsoft.CFS.Approvals.Common.DL
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using Microsoft.Azure.Cosmos.Table;
    using Microsoft.CFS.Approvals.Common.DL.Interface;
    using Microsoft.CFS.Approvals.Contracts;
    using Microsoft.CFS.Approvals.Data.Azure.Storage.Interface;
    using Microsoft.CFS.Approvals.Model;
    using Microsoft.Extensions.Configuration;

    /// <summary>
    /// Tenant down time messages provider.
    /// </summary>
    public class TenantDownTimeMessagesProvider : ITenantDownTimeMessagesProvider
    {
        /// <summary>
        /// The configuration
        /// </summary>
        private readonly IConfiguration _config = null;

        /// <summary>
        /// The table helper
        /// </summary>
        private readonly ITableHelper _tableHelper = null;

        public TenantDownTimeMessagesProvider(IConfiguration config, ITableHelper tableHelper)
        {
            // This is handled by the unity initialization
            _config = config;
            _tableHelper = tableHelper;
        }

        #region Implemented Methods

        /// <summary>
        /// Get all down time notifications.
        /// </summary>
        /// <returns></returns>
        public IEnumerable<TenantDownTimeMessages> GetAllDownTimeNotifications()
        {
            var query = (new TableQuery<TenantDownTimeMessages>());
            var notificationList = _tableHelper.GetDataCollectionByTableQuery(_config[ConfigurationKey.TenantDownTimeMessagesAzureTableName.ToString()], query);
            return notificationList.OrderBy(nmsg => nmsg.EventStartTime).ToList();
        }

        /// <summary>
        /// Get down time notifications.
        /// </summary>
        /// <param name="isActive"></param>
        /// <param name="messages"></param>
        /// <returns></returns>
        public IEnumerable<TenantDownTimeMessages> GetDownTimeNotifications(bool isActive, IEnumerable<TenantDownTimeMessages> messages)
        {
            return messages.Where(msg => msg.PartitionKey.Equals(isActive.ToString()));
        }

        /// <summary>
        /// Get all down time notifications group by buckets.
        /// </summary>
        /// <param name="tdMessages"></param>
        /// <param name="loggedInAlias"></param>
        /// <param name="clientDevice"></param>
        /// <returns></returns>
        public IEnumerable<UserPreference> GetUserPreferencesByAlias(string loggedInAlias)
        {
            var tableName = _config[ConfigurationKey.UserPreferenceAzureTableName.ToString()];
            return _tableHelper.GetTableEntityListByPartitionKey<UserPreference>(tableName, loggedInAlias.ToLowerInvariant()).ToList();
        }

        /// <summary>
        /// Insert or Update Tenant Down Time Message.
        /// </summary>
        /// <param name="realtimeTenantInfo"></param>
        /// <param name="loggedInAlias"></param>
        /// <returns></returns>
        public bool InsertOrUpdateTenantDowntimeMessage(ApprovalTenantInfoRealTime realtimeTenantInfo, string loggedInAlias)
        {
            try
            {
                //var table = _tableHelper.GetCloudTable(_azureConfigurationHelper[ConfigurationKey.TenantDownTimeMessagesAzureTableName));
                TableQuery<TenantDownTimeMessages> query = (new TableQuery<TenantDownTimeMessages>()
                        .Where(TableQuery.GenerateFilterCondition("RowKey", QueryComparisons.Equal, realtimeTenantInfo.DocTypeId)));
                //var tenantDownTimeMessage = table.ExecuteQuery(query).FirstOrDefault();

                var tenantDownTimeMessage = _tableHelper.GetDataCollectionByTableQuery(_config[ConfigurationKey.TenantDownTimeMessagesAzureTableName.ToString()],
                    query).FirstOrDefault();

                if (tenantDownTimeMessage != null)
                {
                    //delete existing row
                    _tableHelper.DeleteRow(_config[ConfigurationKey.TenantDownTimeMessagesAzureTableName.ToString()], tenantDownTimeMessage);

                    //insert a new row with some changes in existing property values
                    tenantDownTimeMessage.PartitionKey = realtimeTenantInfo.IsTenantServicesDown.ToString();
                    tenantDownTimeMessage.NotificationBody = realtimeTenantInfo.CurrentTenantServiceInformation;
                    tenantDownTimeMessage.CreatedBy = loggedInAlias;
                    _tableHelper.InsertOrReplace(_config[ConfigurationKey.TenantDownTimeMessagesAzureTableName.ToString()],
                        tenantDownTimeMessage);
                }
                else
                {
                    TenantDownTimeMessages tenantDownTimeMessageNew = new TenantDownTimeMessages()
                    {
                        PartitionKey = realtimeTenantInfo.IsTenantServicesDown.ToString(),
                        RowKey = realtimeTenantInfo.DocTypeId,
                        BannerType = "warning",
                        CreatedBy = loggedInAlias,
                        CreatedDate = DateTime.UtcNow,
                        EventEndTime = DateTime.UtcNow,
                        EventStartTime = DateTime.UtcNow,
                        IsScheduled = false,
                        NotificationBody = realtimeTenantInfo.CurrentTenantServiceInformation,
                        NotificationTitle = realtimeTenantInfo.AppName + " tenant is down",
                        TenantId = realtimeTenantInfo.TenantId,
                    };

                    _tableHelper.InsertOrReplace(_config[ConfigurationKey.TenantDownTimeMessagesAzureTableName.ToString()],
                        tenantDownTimeMessage);
                }
                return true;
            }
            catch
            {
                return false;
            }
        }

        #endregion Implemented Methods
    }
}