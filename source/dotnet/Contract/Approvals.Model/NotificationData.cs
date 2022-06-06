// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace Microsoft.CFS.Approvals.Model
{
    using System.Collections.Generic;

    /// <summary>
    /// NotificationData class used to send notification
    /// </summary>
    public class NotificationData
    {
        /// <summary>
        /// Gets or sets To property for notification
        /// </summary>
        public string To;

        /// <summary>
        /// Gets or sets Cc property for notification
        /// </summary>
        public string Cc;

        /// <summary>
        /// Gets or sets Bcc property for notification
        /// </summary>
        public string Bcc;

        /// <summary>
        /// Gets or sets Subject property for notification
        /// </summary>
        public string Subject;

        /// <summary>
        /// Gets or sets Body property for notification
        /// </summary>
        public string Body;

        /// <summary>
        /// Gets or sets From property for notification
        /// </summary>
        public string From;

        /// <summary>
        /// Gets or sets PartitionKey for Template
        /// </summary>
        public List<string> TemplatePartitionKey;

        /// <summary>
        /// Gets or sets Template expressions
        /// </summary>
        public string TemplateRowExp;

        /// <summary>
        /// Gets or sets File Url
        /// </summary>
        public string FileUrl { get; set; }

        /// <summary>
        /// Gets or sets template data
        /// </summary>
        public Dictionary<string, string> TemplateData;

        /// <summary>
        /// Gets or sets PendingCount per tenants
        /// </summary>
        public List<ApprovalCount> PendingCountPerTenants;

        /// <summary>
        /// Gets or sets Attachments
        /// </summary>
        public List<NotificationDataAttachment> Attachments;
    }
}
