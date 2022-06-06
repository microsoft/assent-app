// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace Microsoft.CFS.Approvals.Common.DL.Interface
{
    using System.Collections.Generic;
    using Microsoft.CFS.Approvals.Model;

    public interface ITenantDownTimeMessagesProvider
    {
        /// <summary>
        /// Returns all down time notifications. Passing isActive=true will return all active notifications
        /// Passing isActive=false will return inactive notifications
        /// </summary>
        /// <returns>IEnumerable<TenantDownTimeMessages> All down time notifications</returns>
        IEnumerable<TenantDownTimeMessages> GetAllDownTimeNotifications();

        /// <summary>
        /// filter the list provided with IsActive=isActive value and return the messages after filtering
        /// </summary>
        /// <param name="isActive">boolean value for active (true) or inactive(false)</param>
        /// <param name="messages">IEnumerable<TenantDownTimeMessages>List of messages to be filtered</param>
        /// <returns>IEnumerable<TenantDownTimeMessages> list of downtime messages</returns>
        IEnumerable<TenantDownTimeMessages> GetDownTimeNotifications(bool isActive, IEnumerable<TenantDownTimeMessages> messages);

        /// <summary>
        /// Returns all active notifications grouped by bannertype. Mainly consumed by Summary page
        /// </summary>
        IEnumerable<UserPreference> GetUserPreferencesByAlias(string loggedInAlias);

        /// <summary>
        /// Inserts into or Updates the Tenant downtime message table
        /// </summary>
        /// <param name="loggedInAlias"></param>
        /// <param name="realtimeTenantInfo"></param>
        /// <returns>boolean value if insert or update completed successfully</returns>
        bool InsertOrUpdateTenantDowntimeMessage(ApprovalTenantInfoRealTime realtimeTenantInfo, string loggedInAlias);
    }
}
