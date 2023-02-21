// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace Microsoft.CFS.Approvals.Contracts.DataContracts
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.Runtime.Serialization;

    /// <summary>
    /// Type which represents an approval request - base type
    /// </summary>
    [KnownType(typeof(ApprovalRequestExpressionV1))]
    public abstract class ApprovalRequestExpression
    {
        /// <summary>
        /// Operation date time
        /// </summary>
        private DateTime operationDateTime = DateTime.UtcNow;

        /// <summary>
        /// Instantiates an ApprovalRequestExpression object setting the RefreshDetails flag to true by default
        /// </summary>
        public ApprovalRequestExpression()
        {
            RefreshDetails = true;
            OperationDateTime = DateTime.UtcNow;
        }

        #region Properties

        /// <summary>
        /// DocumentTypeId is unique for each tenant and is a GUID in form of string.
        /// The GUID is assigned by Approvals.
        /// This field is mandatory and helps identify the tenant.
        /// </summary>
        [Required(AllowEmptyStrings = false, ErrorMessage = Constants.ARXDocTypeIdNullorEmptyMessage)]
        public Guid DocumentTypeId
        { get; set; }

        /// <summary>
        /// This is nothing but a value sent by the tenants to indicate the type of operation while sending a request.
        /// Enumeration. Ex -
        /// Create = 1: For CREATE operation
        /// Update = 2: For UPDATE operation
        /// Delete = 3: For DELETE operation
        /// TargetedAction = 4: For specific targeted operations
        /// </summary>
        public ApprovalRequestOperation Operation
        { get; set; }

        /// <summary>
        /// Document identfier
        /// Contains the DisplayDocumentNumber (Document Number used for displaying to the user in a user-friendly manner), DocumentNumber and
        /// Fiscal Year (Fiscal year in which the document number exists or is applicable)
        /// Type of ApprovalIdentifier.
        /// </summary>
        [Required(ErrorMessage = Constants.ApprovalIdentifierNullMessage)]
        public ApprovalIdentifier ApprovalIdentifier
        { get; set; }

        /// <summary>
        /// List of Approver for the request.
        /// In case of create, this is list of approvers
        /// In case of update, this is list of new approvers
        /// In case of delete, this is list of approvers for whom this request should be removed from pending approval queue
        /// </summary>
        public List<Approver> Approvers { get; set; }

        /// <summary>
        /// List of approvers for whom summary data should be deleted
        /// Ex. In case of any action Approve, Reject, Reassign, Sendback or Takeback actions on request, this property contains the approver alias for whom request is still pending for approval in Approvals.
        /// </summary>
        public List<string> DeleteFor { get; set; }

        /// <summary>
        /// Indicates the key/value pair for additional action related information (dynamic data depending on the action).
        /// For Update and Delete operations, ActionDetail is mandatory field.
        /// For CREATE operation, ActionDetail must be null.
        /// </summary>
        public ActionDetail ActionDetail { get; set; }

        /// <summary>
        /// This field should be populated only when the notifications need to be sent out to users (ex. in CREATE, DELETE, COMPLETE scenarios) - Type of NotificationDetail.
        /// </summary>
        public NotificationDetail NotificationDetail { get; set; }

        /// <summary>
        /// Indicates the key/value pair of Additional data. Ex: ApproverType, RequestorName, AmountUSD, ApproverList
        /// </summary>
        public Dictionary<string, string> AdditionalData { get; set; }

        /// <summary>
        /// Summary JSON Object - Type of SummaryJson.
        /// This property should be used to send Summary JSON along with ARX to Approvals if such integration is by design.
        /// By sending Summary JSON upfront, Approvals is expected to process the Summary object without making a call back for Summary.
        /// </summary>
        //[DocTypeIdComparisonToTenantId(DocumentTypeId, ErrorMessage = Constants.SummaryJsonDocTypeIdNullorEmptyMessage)]
        public SummaryJson SummaryData { get; set; }

        /// <summary>
        /// This represents an instruction to Approvals on how details should be refreshed - default value is true.
        /// When true, Approvals will delete any existing details in Approvals system and fetch details again upon receiving this ARX
        /// However, if data does not change between multiple approvals for a given document, then send this as false.
        /// When sent as false, Approvals does not delete existing details data assuming that the data has not changed and hence retains and reuses existing details.
        /// </summary>
        public Boolean RefreshDetails { get; set; }

        /// <summary>
        /// Details in JSON format - represent all details for the given ApprovalIdentifier.
        /// </summary>
        public Dictionary<string, string> DetailsData { get; set; }

        /// <summary>
        /// This date time property represents the moment when the operation is performed. Operation includes the Create, Update or Delete.
        /// </summary>
        public DateTime OperationDateTime
        {
            get
            {
                return operationDateTime;
            }
            set
            {
                operationDateTime = value.GetDateTimeWithUtcKind();
            }
        }

        /// <summary>
        /// Contains the Xcv/Tcv/BusinessProcessName which is used for Telemetry and Logging
        /// </summary>
        public ApprovalsTelemetry Telemetry { get; set; }

        #endregion Properties
    }
}