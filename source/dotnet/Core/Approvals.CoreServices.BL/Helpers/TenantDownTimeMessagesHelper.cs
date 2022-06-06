// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace Microsoft.CFS.Approvals.CoreServices.BL.Helpers
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using Microsoft.CFS.Approvals.Common.DL.Interface;
    using Microsoft.CFS.Approvals.Contracts;
    using Microsoft.CFS.Approvals.CoreServices.BL.Interface;
    using Microsoft.CFS.Approvals.Data.Azure.Storage.Interface;
    using Microsoft.CFS.Approvals.Extensions;
    using Microsoft.CFS.Approvals.LogManager.Model;
    using Microsoft.CFS.Approvals.LogManager.Provider.Interface;
    using Microsoft.CFS.Approvals.Model;

    /// <summary>
    /// Tenant downtime messages helper.
    /// </summary>
    public class TenantDownTimeMessagesHelper : ITenantDownTimeMessagesHelper
    {
        /// <summary>
        /// The tenant downtime messages provider
        /// </summary>
        private readonly ITenantDownTimeMessagesProvider _tenantDownTimeMessagesProvider = null;

        /// <summary>
        /// The log provider
        /// </summary>
        private readonly ILogProvider _logProvider = null;

        /// <summary>
        /// The table helper
        /// </summary>
        private readonly ITableHelper _tableHelper;

        /// <summary>
        /// Constructor of TenantDownTImeMessageHelper
        /// </summary>
        /// <param name="tenantDownTimeMessagesProvider"></param>
        /// <param name="logProvider"></param>
        /// <param name="tableHelper"></param>
        public TenantDownTimeMessagesHelper(ITenantDownTimeMessagesProvider tenantDownTimeMessagesProvider, ILogProvider logProvider, ITableHelper tableHelper)
        {
            _tenantDownTimeMessagesProvider = tenantDownTimeMessagesProvider;
            _logProvider = logProvider;
            _tableHelper = tableHelper;
        }

        #region Implemented Methods

        /// <summary>
        /// Get all down time notifications.
        /// </summary>
        /// <returns></returns>
        public IEnumerable<TenantDownTimeMessages> GetAllDownTimeNotifications()
        {
            return _tenantDownTimeMessagesProvider.GetAllDownTimeNotifications();
        }

        /// <summary>
        /// Get down time notifications.
        /// </summary>
        /// <param name="isActive"></param>
        /// <param name="messages"></param>
        /// <returns></returns>
        public IEnumerable<TenantDownTimeMessages> GetDownTimeNotifications(bool isActive, IEnumerable<TenantDownTimeMessages> messages)
        {
            return _tenantDownTimeMessagesProvider.GetDownTimeNotifications(isActive, messages);
        }

        /// <summary>
        /// Get all down time notifications group by buckets.
        /// </summary>
        /// <param name="tdMessages"></param>
        /// <param name="loggedInAlias"></param>
        /// <param name="clientDevice"></param>
        /// <returns></returns>
        public IEnumerable<NotificationGroup> GetAllDownTimeNotificationsGroupByBuckets(IEnumerable<TenantDownTimeMessages> tdMessages, string loggedInAlias, string clientDevice)
        {
            var nContainer = new List<NotificationGroup>();
            if (tdMessages == null) return nContainer;
            var redMessages = tdMessages.Where(t => t.BannerType.Equals(NotificationBannerType.Danger.ToString().ToLower())).ToList();
            var yelloMessages = tdMessages.Where(t => t.BannerType.Equals(NotificationBannerType.Warning.ToString().ToLower())).ToList();
            var blueMessages = tdMessages.Where(t => t.BannerType.Equals(NotificationBannerType.Info.ToString().ToLower())).ToList();

            var userPreferenceSetting = _tenantDownTimeMessagesProvider.GetUserPreferencesByAlias(loggedInAlias).ToList();

            UserPreference userPreferenceForClient = null;
            if (userPreferenceSetting != null && userPreferenceSetting.Count > 0)
            {
                if (clientDevice.Equals(Constants.WebClient))
                    userPreferenceForClient = userPreferenceSetting.Where(u => string.IsNullOrEmpty(u.ClientDevice) || u.ClientDevice.Equals(clientDevice, StringComparison.InvariantCultureIgnoreCase)).FirstOrDefault();
                else
                    userPreferenceForClient = userPreferenceSetting.Where(u => !string.IsNullOrEmpty(u.ClientDevice) && u.ClientDevice.Equals(clientDevice, StringComparison.InvariantCultureIgnoreCase)).FirstOrDefault();
            }

            List<string> readNotificationList = userPreferenceForClient?.ReadNotificationsList != null ? userPreferenceForClient?.ReadNotificationsList.FromJson<List<string>>() : null;
            if (redMessages.Any())
            {
                var ngRedGroup = new NotificationGroup
                {
                    Severity = NotificationBannerType.Danger.ToString().ToLower(),
                    DisplaySequence = 0,
                    Items = new List<NotificationMessage>()
                };
                foreach (var ynm in redMessages.OrderBy(nmsg => nmsg.EventStartTime).Select(tRedDownTimeMsg => new NotificationMessage
                {
                    TenantId = tRedDownTimeMsg.TenantId,
                    Message = tRedDownTimeMsg.NotificationBody,
                    Title = tRedDownTimeMsg.NotificationTitle,
                    ID = tRedDownTimeMsg.RowKey,
                    IsRead = readNotificationList != null && readNotificationList.Any() && readNotificationList.Contains(tRedDownTimeMsg.RowKey)
                }))
                {
                    ngRedGroup.Items.Add(ynm);
                }

                nContainer.Add(ngRedGroup);
            }

            if (yelloMessages.Any())
            {
                var ngYellowGroup = new NotificationGroup
                {
                    Severity = NotificationBannerType.Warning.ToString().ToLower(),
                    DisplaySequence = 1,
                    Items = new List<NotificationMessage>()
                };
                foreach (var ynm in yelloMessages.OrderBy(nmsg => nmsg.EventStartTime).Select(tYellowDownTimeMsg => new NotificationMessage
                {
                    TenantId = tYellowDownTimeMsg.TenantId,
                    Message = tYellowDownTimeMsg.NotificationBody,
                    Title = tYellowDownTimeMsg.NotificationTitle,
                    ID = tYellowDownTimeMsg.RowKey,
                    IsRead = readNotificationList != null && readNotificationList.Any() && readNotificationList.Contains(tYellowDownTimeMsg.RowKey)
                }))
                {
                    ngYellowGroup.Items.Add(ynm);
                }
                nContainer.Add(ngYellowGroup);
            }

            if (blueMessages.Any())
            {
                var ngBlueGroup = new NotificationGroup
                {
                    Severity = NotificationBannerType.Info.ToString().ToLower(),
                    DisplaySequence = 2,
                    Items = new List<NotificationMessage>()
                };
                foreach (var bnm in blueMessages.OrderBy(nmsg => nmsg.EventStartTime).Select(tBlueDownTimeMsg => new NotificationMessage
                {
                    TenantId = tBlueDownTimeMsg.TenantId,
                    Message = tBlueDownTimeMsg.NotificationBody,
                    Title = tBlueDownTimeMsg.NotificationTitle,
                    ID = tBlueDownTimeMsg.RowKey,
                    IsRead = readNotificationList != null && readNotificationList.Any() && readNotificationList.Contains(tBlueDownTimeMsg.RowKey)
                }))
                {
                    ngBlueGroup.Items.Add(bnm);
                }
                nContainer.Add(ngBlueGroup);
            }
            return nContainer;
        }

        /// <summary>
        /// Insert or Update Tenant Down Time Message.
        /// </summary>
        /// <param name="realtimeTenantInfo"></param>
        /// <param name="loggedInAlias"></param>
        /// <returns></returns>
        public bool InsertOrUpdateTenantDowntimeMessage(ApprovalTenantInfoRealTime realtimeTenantInfo, string loggedInAlias)
        {
            return _tenantDownTimeMessagesProvider.InsertOrUpdateTenantDowntimeMessage(realtimeTenantInfo, loggedInAlias);
        }

        /// <summary>
        /// Get all alerts.
        /// </summary>
        /// <param name="SessionId"></param>
        /// <param name="loggedInAlias"></param>
        /// <param name="Alias"></param>
        /// <param name="ClientDevice"></param>
        /// <returns></returns>
        public IEnumerable<NotificationGroup> GetAllAlerts(string SessionId, string loggedInAlias, string Alias, string ClientDevice)
        {
            #region Logging

            var Tcv = Guid.NewGuid().ToString();

            if (!string.IsNullOrEmpty(SessionId))
            {
                Tcv = SessionId;
            }

            var logData = new Dictionary<LogDataKey, object>
            {
                { LogDataKey.Xcv, Tcv },
                { LogDataKey.Tcv, Tcv },
                { LogDataKey.SessionId, Tcv },
                { LogDataKey.ClientDevice, ClientDevice },
                { LogDataKey.UserRoleName, loggedInAlias },
                { LogDataKey.EventType, Constants.FeatureUsageEvent },
                { LogDataKey.UserAlias, Alias },
                { LogDataKey.StartDateTime, DateTime.UtcNow },
                { LogDataKey.IsCriticalEvent, CriticalityLevel.No.ToString() }
            };

            #endregion Logging

            try
            {
                var downTimeMessages = GetAllDownTimeNotifications();
                var filteredDtMessages = GetDownTimeNotifications(true, downTimeMessages);
                var alerts = GetAllDownTimeNotificationsGroupByBuckets(filteredDtMessages, loggedInAlias, ClientDevice);

                // Log Success
                logData.Modify(LogDataKey.EndDateTime, DateTime.UtcNow);
                _logProvider.LogInformation(TrackingEvent.WebApiGetAllDownTimeMessagesSuccess, logData);

                return alerts;
            }
            catch (Exception ex)
            {
                logData.Modify(LogDataKey.EndDateTime, DateTime.UtcNow);
                _logProvider.LogError(TrackingEvent.WebApiGetAllDownTimeMessagesFail, ex, logData);
                throw;
            }
        }

        /// <summary>
        /// Delete alert or notification based on row key
        /// </summary>
        /// <param name="rowKey">row key</param>
        public void DeleteAlert(string rowKey)
        {
            // Get TenantdownTimeMessage object from storage table based on row key
            TenantDownTimeMessages realtimeTenantInfo = _tableHelper.GetTableEntityByRowKey<TenantDownTimeMessages>(Constants.TableNameTenantDownTimeMessages, rowKey);

            // Generate object to be deleted
            TenantDownTimeMessages tenantDownTimeMessageNew = new TenantDownTimeMessages()
            {
                PartitionKey = realtimeTenantInfo.PartitionKey,
                RowKey = realtimeTenantInfo.RowKey,
                BannerType = realtimeTenantInfo.BannerType,
                CreatedBy = realtimeTenantInfo.CreatedBy,
                CreatedDate = realtimeTenantInfo.CreatedDate,
                EventEndTime = realtimeTenantInfo.EventEndTime,
                EventStartTime = realtimeTenantInfo.EventStartTime,
                IsScheduled = realtimeTenantInfo.IsScheduled,
                NotificationBody = realtimeTenantInfo.NotificationBody,
                NotificationTitle = realtimeTenantInfo.NotificationTitle,
                TenantId = realtimeTenantInfo.TenantId,
                ETag = realtimeTenantInfo.ETag
            };
            // Delete row from table
            _tableHelper.DeleteRow(Constants.TableNameTenantDownTimeMessages, tenantDownTimeMessageNew);
        }

        /// <summary>
        /// Insert or update the notification
        /// </summary>
        /// <param name="downtimeMessage">TenantDownTimeMessages object to be inserted</param>
        /// <returns>boolean value if insert or update completed successfully</returns>
        public bool InsertOrUpdateAlertNotifications(TenantDownTimeMessages downtimeMessage)
        {
            try
            {
                TenantDownTimeMessages tenantDownTimeMessage = null;
                var tenantId = 0;

                // If RowKey has some value then its update scenario
                if (!string.IsNullOrWhiteSpace(downtimeMessage.RowKey))
                {
                    tenantDownTimeMessage = _tableHelper.GetTableEntityByRowKey<TenantDownTimeMessages>(Constants.TableNameTenantDownTimeMessages, downtimeMessage.RowKey);
                    tenantId = tenantDownTimeMessage.TenantId;
                }
                if (tenantDownTimeMessage != null)
                {
                    // In case of update scenario, we need to delete existing row
                    _tableHelper.DeleteRow(Constants.TableNameTenantDownTimeMessages, tenantDownTimeMessage);
                }
                else
                {
                    tenantDownTimeMessage = new TenantDownTimeMessages();
                }
                // Insert a new row with RowKey as new Guid
                tenantDownTimeMessage.RowKey = Guid.NewGuid().ToString();
                tenantDownTimeMessage.PartitionKey = downtimeMessage.PartitionKey;
                tenantDownTimeMessage.BannerType = downtimeMessage.BannerType;
                tenantDownTimeMessage.CreatedBy = downtimeMessage.CreatedBy;
                tenantDownTimeMessage.CreatedDate = DateTime.UtcNow;
                tenantDownTimeMessage.EventEndTime = downtimeMessage.EventEndTime;
                tenantDownTimeMessage.EventStartTime = downtimeMessage.EventStartTime;
                tenantDownTimeMessage.IsScheduled = downtimeMessage.IsScheduled;
                tenantDownTimeMessage.NotificationBody = downtimeMessage.NotificationBody;
                tenantDownTimeMessage.NotificationTitle = downtimeMessage.NotificationTitle;
                tenantDownTimeMessage.TenantId = downtimeMessage.RowKey != string.Empty ? tenantId : downtimeMessage.TenantId;

                _tableHelper.InsertOrReplace(Constants.TableNameTenantDownTimeMessages, tenantDownTimeMessage);
                return true;
            }
            catch (Exception ex)
            {
                return false;
            }
        }

        #endregion Implemented Methods
    }
}