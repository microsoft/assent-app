// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace Microsoft.CFS.Approvals.Utilities.Helpers
{
    using System;
    using Microsoft.CFS.Approvals.Contracts.DataContracts;
    using Microsoft.CFS.Approvals.Extensions;
    using Microsoft.CFS.Approvals.Model;

    /// <summary>
    /// Reminder Helper class
    /// </summary>
    public static class ReminderHelper
    {
        /// <summary>
        /// Get Notification Details.
        /// </summary>
        /// <param name="summaryRow"></param>
        /// <returns>NotificationDetail</returns>
        public static NotificationDetail GetNotificationDetails(ApprovalSummaryRow summaryRow)
        {
            if (summaryRow != null)
            {
                if (summaryRow.NotificationJson != null)
                {
                    return summaryRow.NotificationJson.FromJson<NotificationDetail>();
                }
            }
            return new NotificationDetail();
        }

        /// <summary>
        /// Get Next reminder time.
        /// </summary>
        /// <param name="notificationDetail"></param>
        /// <param name="currentTime"></param>
        /// <returns>Next reminder time.</returns>
        public static DateTime NextReminderTime(NotificationDetail notificationDetail, DateTime currentTime)
        {
            try
            {
                if (notificationDetail.Reminder != null)
                {
                    if (notificationDetail.Reminder.Expiration != default)
                    {
                        if (notificationDetail.Reminder.Expiration.Date >= currentTime.Add(new TimeSpan(notificationDetail.Reminder.Frequency, 0, 0, 0)).Date && notificationDetail.Reminder.Frequency > 0)
                        {
                            return currentTime.Add(new TimeSpan(notificationDetail.Reminder.Frequency, 0, 0, 0));
                        }
                    }
                    else if (notificationDetail.Reminder.Frequency > 0)
                    {
                        return currentTime.Add(new TimeSpan(notificationDetail.Reminder.Frequency, 0, 0, 0));
                    }
                    else if (notificationDetail.Reminder?.ReminderDates?.Count > 0)
                    {
                        notificationDetail.Reminder.ReminderDates.Sort();
                        foreach (DateTime day in notificationDetail.Reminder.ReminderDates)
                        {
                            if (day > currentTime)
                                return day;
                        }
                    }
                }
                return DateTime.MaxValue;
            }
            catch
            {
                return DateTime.MaxValue;
            }
        }
    }
}