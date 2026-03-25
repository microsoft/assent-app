// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace Microsoft.CFS.Approvals.Contracts.AttributeValidators
{
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using Microsoft.CFS.Approvals.Contracts.DataContracts;

    /// <summary>
    /// The Notificaton Details validator class.
    /// </summary>
    public class NotificationDetailsValidator : ValidationAttribute
    {
        /// <summary>
        /// The Validation of Notification Detail.
        /// </summary>
        /// <param name="notificationDetails"></param>
        /// <param name="operation"></param>
        /// <returns>List of validation results</returns>
        public List<ValidationResult> Validator(NotificationDetail notificationDetails, ApprovalRequestOperation? operation = null)
        {
            var results = new List<ValidationResult>();

            System.ComponentModel.DataAnnotations.Validator.TryValidateObject(notificationDetails, new ValidationContext(notificationDetails), results);
            if (notificationDetails.Reminder != null && operation != ApprovalRequestOperation.Delete)
                results.AddRange(new ReminderDetailsValidator().Validator(notificationDetails.Reminder));
            return results;
        }
    }
}