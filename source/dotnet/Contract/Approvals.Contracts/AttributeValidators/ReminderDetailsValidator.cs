// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace Microsoft.CFS.Approvals.Contracts.AttributeValidators
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using Microsoft.CFS.Approvals.Contracts.DataContracts;

    /// <summary>
    /// The Reminder Details Validator class.
    /// </summary>
    public class ReminderDetailsValidator : ValidationAttribute
    {
        /// <summary>
        /// The Validation of Reminder Detail.
        /// </summary>
        /// <param name="reminderDetails"></param>
        /// <returns>List of validation results</returns>
        public List<ValidationResult> Validator(ReminderDetail reminderDetails)
        {
            var results = new List<ValidationResult>();

            System.ComponentModel.DataAnnotations.Validator.TryValidateObject(reminderDetails, new ValidationContext(reminderDetails), results);

            if (reminderDetails.ReminderDates == null || reminderDetails.ReminderDates.Count == 0)
            {
                if (reminderDetails.Frequency <= 0 || reminderDetails.Expiration <= DateTime.Now)
                    results.Add(new ValidationResult(Constants.ReminderDetailFrequencyMessage, new List<string> { "ApprovalRequestExpression.NotificationDetail.ReminderDetail.Frequency", "ApprovalRequestExpression.NotificationDetail.ReminderDetail.Expiration" }));
            }

            return results;
        }
    }
}