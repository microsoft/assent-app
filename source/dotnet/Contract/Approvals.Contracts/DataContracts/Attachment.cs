// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace Microsoft.CFS.Approvals.Contracts.DataContracts
{
    using System;

    /// <summary>
    /// Attachment object
    /// </summary>
    public class Attachment
    {
        /// <summary>
        /// User friendly name of the attachment. This name will be visible on Approvals in header section.
        /// </summary>
        private string _name;

        public string Name
        {
            get { return _name.Replace(@"\\\\", @"\\"); }
            set { _name = value; }
        }

        /// <summary>
        /// Unique identification number. This is required to identify the unique attachment.
        /// </summary>
        public string ID { get; set; }

        /// <summary>
        /// URL that can be used to download attachment, if attachment is stored else where.
        /// If not provided, Approvals will build the URL using root URL provided by Tenant.
        /// </summary>
        public string Url { get; set; }

        /// <summary>
        /// Gets or sets wether the attachment is pre attached or post attached (If false, the attachment is uploaded through the UI).
        /// </summary>
        public bool IsPreAttached { get; set; } = true;

        /// <summary>
        /// Gets or sets user who uploaded attachment
        /// </summary>
        public string UploadedBy { get; set; }

        /// <summary>
        /// Gets or sets Attachment uploaded datetime
        /// </summary>
        public Nullable<DateTime> UploadedDate { get; set; }

        /// <summary>
        /// Gets or sets Attachment Category
        /// </summary>
        public string Category { get; set; } = "default";

        /// <summary>
        /// Gets or sets Description
        /// </summary>
        public string Description { get; set; }
    }
}