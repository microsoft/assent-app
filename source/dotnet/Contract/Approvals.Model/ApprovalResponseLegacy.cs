// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace Microsoft.CFS.Approvals.Model
{
    using System;
    using System.Collections.Generic;
    using System.Runtime.Serialization;
    using Microsoft.CFS.Approvals.Contracts;
    using Microsoft.CFS.Approvals.Contracts.DataContracts;

    public class ApprovalResponseLegacy
    {
        /// <summary>
        /// DocumentTypeId is unique for each tenant and is a GUID in form of string.
        /// The GUID is assigned by Approvals.
        /// This field is mandatory and helps identify the tenant.
        /// </summary>
        public string DocumentTypeID { get; set; }

        /// <summary>
        /// Action result - Type of Boolean.
        /// </summary>
        public bool ActionResult { get; set; }

        /// <summary>
        /// List of error messages.
        /// </summary>
        public List<string> ErrorMessages { get; set; }

        /// <summary>
        /// List of E2E Errors. Error Info carries additional data about each error that occurred.
        /// </summary>
        public List<ApprovalResponseErrorInfoLegacy> E2EErrorInformation { get; set; }

        /// <summary>
        /// Document Identifier
        /// Contains the DisplayDocumentNumber (Document Number used for displaying to the user in a user-friendly manner), DocumentNumber, Fiscal Year (Fiscal year in which the document number exists or is applicable) 
        /// Type of ApprovalIdentifier.
        /// </summary>
        public ApprovalIdentifier ApprovalIdentifier { get; set; }

        /// <summary>
        /// Extension data.
        /// </summary>
        [Obsolete]
        public ExtensionDataObject ExtensionData { get; set; }

        /// <summary>
        /// Contains the Xcv/Tcv/BusinessProcessName which is used for Telemetry and Logging 
        /// </summary>
        public ApprovalsTelemetry Telemetry { get; set; }

        /// <summary>
        /// Optionally, provide a display error message when a specific error message needs to be shown to the user OR provide a message for the user even if response is http OK
        /// Ex. If a business rule fails, user needs to understand the reason for failure and hence should see this message
        /// Ex. In case a SQL Connection fails to establish, ErrorMessage can show SQL connection failed, but DisplayMessage should show something like "Your request could not be processed. Please try again later"
        /// </summary>
        public string DisplayMessage { get; set; }

        /// <summary>
        /// Provide a retry interval time in minutes, if a transient error has occurred. This value will be shown to the user on the Approvals UI with a hint to retry after this interval. Please use this only if a transient error occurred and not when a non-transient or fatal error occurs
        /// </summary>
        public int RetryInterval { get; set; }
    }

    public class ApprovalResponseErrorInfoLegacy
    {
        public ApprovalResponseErrorInfoLegacy()
        {
            // Setting default value
            ErrorType = ApprovalResponseErrorTypeLegacy.UnintendedError;
        }

        /// <summary>
        /// Provide the technical error message, either the Exception.Message value or a custom error message explaining technical error that occurred
        /// </summary>
        public string ErrorMessage { get; set; }

        /// <summary>
        /// Indicates the type of error that occurred. Default value is NonTransient always, but this should be overwritten when error type is known
        /// </summary>
        public ApprovalResponseErrorTypeLegacy ErrorType { get; set; }

        /// <summary>
        /// For applications which maintain their own log ids and cannot use correlation id directly, log id property can be used to share this information with Approvals. In either case, correlation id will be needed at Approval Response level
        /// </summary>
        public string LogIdLocal { get; set; }
    }

    public enum ApprovalResponseErrorTypeLegacy
    {
        /// <summary>
        /// Intended Error.
        /// </summary>
        [EnumMember]
        IntendedError = 1,

        /// <summary>
        /// Unintended Error.
        /// </summary>
        [EnumMember]
        UnintendedError = 2,

        /// <summary>
        /// Unintended Transient Error.
        /// </summary>
        [EnumMember]
        UnintendedTransientError = 3,

        /// <summary>
        /// Unintended Non-Transient Error.
        /// </summary>
        [EnumMember]
        UnintendedNonTransientError = 4,
    }
}
