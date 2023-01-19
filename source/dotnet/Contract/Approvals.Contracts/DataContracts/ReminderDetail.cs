// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace Microsoft.CFS.Approvals.Contracts.DataContracts
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;

    /// <summary>
    /// Reminder detail.
    /// </summary>
    public class ReminderDetail
    {
        /// <summary>
        /// List of dates on which reminders should be sent - List of type dates.
        /// </summary>
        public List<DateTime> ReminderDates { get; set; }

        /// <summary>
        /// Frequency at which reminders should be sent.
        /// </summary>
        [Range(0, int.MaxValue, ErrorMessage = Constants.ReminderDetailFrequencyMessage)]
        public int Frequency { get; set; }

        /// <summary>
        /// Expiration date of reminder notification.
        /// </summary>
        public DateTime Expiration { get; set; }

        /// <summary>
        /// Reminder notification template that should be used for reminders that should be sent.
        /// </summary>
        [Required(AllowEmptyStrings = false, ErrorMessage = Constants.ReminderDetailReminderTemplateMessage)]
        public string ReminderTemplate { get; set; }

    }
}