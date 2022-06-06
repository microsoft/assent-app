// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace Microsoft.CFS.Approvals.Contracts.DataContracts
{
    using System;
    using System.Collections.Generic;
    using System.Runtime.Serialization;

    /// <summary>
    /// Represents details of approval request.
    /// </summary>
    [DataContract]
    public sealed class ApprovalRequest
    {
        /// <summary>
        /// DocumentTypeId is unique for each tenant and is a GUID in form of string.
        /// The GUID is assigned by Approvals.
        /// This field is mandatory and helps identify the tenant.
        /// </summary>
        [DataMember]
        public string DocumentTypeID { get; set; }

        /// <summary>
        /// Action - ex. Primary actions (Approve, Reject), Secondary actions (Reassign, Add Interim Approver, Add Comments and Send Back).
        /// Actions are tenant specific and hence string type allowing new types to be added as per need of a tenant.
        /// </summary>
        [DataMember]
        public string Action { get; set; }

        /// <summary>
        /// Alias of the person who performed the action. This is the approver in Approvals
        /// </summary>
        [DataMember]
        public string ActionByAlias { get; set; }

        /// <summary>
        /// Alias of the person for who the action was performed. This is used in case where delegate approver takes action.
        /// </summary>
        [DataMember]
        public string ActionByDelegateInMSApprovals { get; set; }

        /// <summary>
        /// Alias of the person for who the action was performed. This is used in case where delegate approver takes action.
        /// </summary>
        [DataMember]
        //public string ActualActionByAlias{ get; set; }
        public string OriginalApproverInTenantSystem { get; set; }

        /// <summary>
        /// Indicates the key/value pair for additional action related information (dynamic data depending on the action).
        /// For Update and Delete operations, ActionDetail is mandatory field.
        /// For CREATE operation, ActionDetail must be null.
        /// </summary>
        [DataMember]
        public Dictionary<string, string> ActionDetails { get; set; }

        /// <summary>
        /// Document identfier
        /// Contains the DisplayDocumentNumber (Document Number used for displaying to the user in a user-friendly manner), DocumentNumber and 
        /// Fiscal Year (Fiscal year in which the document number exists or is applicable) 
        /// Type of ApprovalIdentifier.
        /// </summary>
        [DataMember]
        public ApprovalIdentifier ApprovalIdentifier { get; set; }

        /// <summary>
        /// Indicates the key/value pair of Additional data for Approval Request. 
        /// </summary>
        [DataMember]
        public Dictionary<string, string> AdditionalData { get; set; }

        /// <summary>
        /// Extension data.
        /// </summary>
        [Obsolete]
        public ExtensionDataObject ExtensionData { get; set; }        

        /// <summary>
        /// Contains the Xcv/Tcv/BusinessProcessName which is used for Telemetry and Logging 
        /// </summary>
        [DataMember]
        public ApprovalsTelemetry Telemetry { get; set; }
    }
}
