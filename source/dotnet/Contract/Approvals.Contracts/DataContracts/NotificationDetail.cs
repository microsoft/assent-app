// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace Microsoft.CFS.Approvals.Contracts.DataContracts
{
    using System.ComponentModel.DataAnnotations;

    /// <summary>
    /// Notification details.
    /// </summary>
    public class NotificationDetail
    {
        /// <summary>
        /// Boolean value which informs Approvals about what should be done with this request i.e. send or not to send user notification.
        /// 0 - No notification should be sent.
        /// 1 - Notification should be sent.
        /// Default value is 0 i.e. no notification is sent by default unless explicitly instructed.
        /// </summary>
        public bool SendNotification
        { get; set; }

        /// <summary>
        /// Represents a template key or type, when supported by Approvals.
        /// The given key or type when supported will be used for this notification to the user.
        /// Ex: PendingApproval|None
        /// </summary>
        [Required(AllowEmptyStrings = false, ErrorMessage = Constants.NotificationDetailTemplateKeyNullMessage)]
        public string TemplateKey
        { get; set; }

        /// <summary>
        /// List of aliases in to whom email should be sent to - TO field.
        /// </summary>
        [Required(AllowEmptyStrings = false, ErrorMessage = Constants.NotificationDetailToMessage)]
        public string To
        { get; set; }

        /// <summary>
        /// List of aliases in to whom email should be copied to - CC field.
        /// </summary>
        public string Cc
        { get; set; }

        /// <summary>
        /// List of aliases in to whom email should be blind copied - BCC field.
        /// </summary>
        public string Bcc
        { get; set; }

        /// <summary>
        /// For watch dog reminder notification - Type of ReminderDetail
        /// </summary>
        public ReminderDetail Reminder { get; set; }

    }
}
