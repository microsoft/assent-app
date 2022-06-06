// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace Microsoft.CFS.Approvals.SyntheticTransaction.Common.Models
{
    using System;
    using System.Collections.Generic;

    /// <summary>
    /// LOB Request class
    /// </summary>
    public class LOBRequest
    {
        public ActionDetails ActionDetails { get; set; }
        public string ActionByAlias { get; set; }
        public string Action { get; set; }
        public DocumentKeys DocumentKeys { get; set; }
        public string ApplicationID { get; set; }
        public string DocumentTypeID { get; set; }
        public string ActionByDelegateInMSApprovals { get; set; }
        public string OriginalApproverInTenantSystem { get; set; }
        public ApprovalsTelemetry Telemetry { get; set; }
        public Dictionary<string, string> AdditionalData { get; set; }
    }

    /// <summary>
    /// The Action Details class
    /// </summary>
    public class ActionDetails
    {
        public string Comment { get; set; }
        public string ReasonCode { get; set; }
        public string ReasonText { get; set; }
        public string ActionDate { get; set; }
    }

    /// <summary>
    /// The Document Keys
    /// </summary>
    public class DocumentKeys
    {
        public string DocumentNumber { get; set; }
        public string DisplayDocumentNumber { get; set; }
    }

    /// <summary>
    /// The Approvals Telemetry class
    /// </summary>
    public class ApprovalsTelemetry
    {
        /// <summary>
        /// Constructor of ApprovalsTelemetry
        /// </summary>
        public ApprovalsTelemetry()
        {
            Xcv = Guid.NewGuid().ToString();
        }
        /// <summary>
        /// This acts like a TransactionID between tenant and Approvals. When used, it is recommended to be of type GUID.
        /// </summary>
        public string Tcv { get; set; }

        /// <summary>
        /// This acts like a CorrelationID between tenant and Approvals. When used, it is recommended to be of type GUID.
        /// </summary>
        public string Xcv { get; set; }

        /// <summary>
        /// This will be used as business process name logged in ApplicationInsight logs.
        /// </summary>
        public string BusinessProcessName { get; set; }
    }
}
