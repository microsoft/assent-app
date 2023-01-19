// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace Microsoft.CFS.Approvals.Contracts.DataContracts
{
    using System;
    using System.Collections.Generic;
    using System.Runtime.Serialization;

    /// <summary>
    /// Response object type to be used by tenants when an approval action is submitted by Approvals.
    /// </summary>
    [DataContract]
    public sealed class ApprovalResponse
    {
        /// <summary>
        /// Approvals assigned unique identifier for each tenant.
        /// Required field and helps to identify the tenant.
        /// </summary>
        [DataMember]
        public string DocumentTypeID { get; set; }

        /// <summary>
        /// Action result - Type of Boolean.
        /// Required field and helps to identify the overall result of the transaction.
        /// </summary>
        [DataMember]
        public bool ActionResult { get; set; }

        /// <summary>
        /// List of error messages.
        /// </summary>
        [DataMember]
        [Obsolete]
        public List<string> ErrorMessages { get; set; }

        /// <summary>
        /// E2E Error Info which carries additional data about each error that occurred.
        /// Applicable for error scenarios only.
        /// </summary>
        [DataMember]
        public ApprovalResponseErrorInfo E2EErrorInformation { get; set; }

        /// <summary>
        /// Document Identifier.
        /// Contains the DisplayDocumentNumber (Document Number used for displaying to the user in a user-friendly manner), DocumentNumber, Fiscal Year (Fiscal year in which the document number exists or is applicable) 
        /// Type of ApprovalIdentifier.
        /// </summary>
        [DataMember]
        public ApprovalIdentifier ApprovalIdentifier { get; set; }

        /// <summary>
        /// Extension data.
        /// </summary>
        [Obsolete]
        public ExtensionDataObject ExtensionData { get; set; }

        /// <summary>
        /// Contains the Xcv/Tcv/BusinessProcessName which is used for Telemetry and Logging.
        /// </summary>
        [DataMember]
        public ApprovalsTelemetry Telemetry { get; set; }

        /// <summary>
        /// Optionally, provide a display error message when a specific error message needs to be shown to the user OR provide a message for the user even if response is http OK
        /// Ex. If a business rule fails, user needs to understand the reason for failure and hence should see this message
        /// Ex. In case a SQL Connection fails to establish, ErrorMessage can show SQL connection failed, but DisplayMessage should show something like "Your request could not be processed. Please try again later"
        /// </summary>
        [DataMember]
        public string DisplayMessage { get; set; }
    }
}