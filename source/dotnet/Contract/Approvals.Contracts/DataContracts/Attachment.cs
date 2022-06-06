// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace Microsoft.CFS.Approvals.Contracts.DataContracts
{
    /// <summary>
    /// Attachment object
    /// </summary>
    public class Attachment
    {

        /// <summary>
        /// User friendly name of the attachment. This name will be visible on Approvals in header section.
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Unique identification number. This is required to identify the unique attachment.
        /// </summary>
        public string ID { get; set; }

        /// <summary>
        /// URL that can be used to download attachment, if attachment is stored else where. 
        /// If not provided, Approvals will build the URL using root URL provided by Tenant.
        /// </summary>
        public string Url { get; set; }

    }
}
