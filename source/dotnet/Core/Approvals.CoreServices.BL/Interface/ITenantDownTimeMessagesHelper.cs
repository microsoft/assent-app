// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace Microsoft.CFS.Approvals.CoreServices.BL.Interface
{
    using System.Collections.Generic;
    using Microsoft.CFS.Approvals.Model;

    public interface ITenantDownTimeMessagesHelper
    {
        /// <summary>
        /// Returns all down time notifications. Passing isActive=true will return all active notifications
        /// Passing isActive=false will return inactive notifications
        /// </summary>
        /// <returns></returns>
        IEnumerable<TenantDownTimeMessages> GetAllDownTimeNotifications();

        /// <summary>
        ///
        /// </summary>
        /// <param name="isActive"></param>
        /// <param name="messages"></param>
        /// <returns></returns>
        IEnumerable<TenantDownTimeMessages> GetDownTimeNotifications(bool isActive, IEnumerable<TenantDownTimeMessages> messages);

        /// <summary>
        /// Returns all active notifications grouped by bannertype. Mainly consumed by Summary page
        /// </summary>
        IEnumerable<NotificationGroup> GetAllDownTimeNotificationsGroupByBuckets(IEnumerable<TenantDownTimeMessages> tdMessages, string loggedInAlias, string clientDevice);

        IEnumerable<NotificationGroup> GetAllAlerts(string SessionId, string LoggedInAlias, string Alias, string ClientDevice);

        /// <summary>
        /// Inserts into or Updates the Tenant downtime message table
        /// </summary>
        /// <param name="tenantDowntimeMessage"></param>
        /// <returns>boolean value if insert or update completed successfully</returns>
        bool InsertOrUpdateTenantDowntimeMessage(ApprovalTenantInfoRealTime realtimeTenantInfo, string loggedInAlias);

        /// <summary>
        /// Delete TenantDownTimeMessages entry from storage table
        /// </summary>
        /// <param name="rowId">row key for TenantDownTimeMessages object for deletion</param>
        void DeleteAlert(string rowId);

        /// <summary>
        /// Insert or update TenantDownTimeMessages in storage table
        /// </summary>
        /// <param name="DowntimeMessage">TenantDownTimeMessages object for addition or updation</param>
        /// <returns>boolean value if insert or update completed successfully</returns>
        bool InsertOrUpdateAlertNotifications(TenantDownTimeMessages DowntimeMessage);
    }
}