// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace Microsoft.CFS.Approvals.Contracts.DataContracts
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.Runtime.Serialization;

    /// <summary>
    /// This is a standard summary object defined by Approvals.
    /// </summary>
    [DataContract]
    public class SummaryJson
    {
        private Guid requestVersion;

        /// <summary>
        /// Default constructor.
        /// </summary>
        public SummaryJson()
        {
            DocumentTypeId = null;
            UnitValue = null;
            UnitOfMeasure = null;
            DetailPageURL = null;
            RequestVersion = Guid.NewGuid();
        }

        /// <summary>
        /// Method which gets invoked when de-serialization occurs
        /// </summary>
        /// <param name="context"></param>
        [OnDeserializing]
        private void OnDeserializing(StreamingContext context)
        {
            SetDefaults();
        }

        /// <summary>
        /// Sets the default values for the properties
        /// </summary>
        private void SetDefaults()
        {
            RequestVersion = Guid.NewGuid();
        }

        /// <summary>
        /// DocumentTypeId is unique for each tenant and is a GUID in form of string.
        /// The GUID is assigned by Approvals.
        /// This field is mandatory and helps identify the tenant.
        /// This value needs to match the DocumentTypeId in ARX when SummaryJson is sent with ARX.
        /// </summary>
        [DataMember]
        [Required(AllowEmptyStrings = false, ErrorMessage = Constants.SummaryJsonDocTypeIdNullorEmptyMessage)]
        public String DocumentTypeId { get; set; }

        /// <summary>
        /// Title of the request.
        /// </summary>
        [DataMember]
        public String Title { get; set; }

        /// <summary>
        /// Unit value of the request.
        /// For ex: for some tenants this value would be some currency amount, whereas for other tenant this value is the number of days.
        /// This value should be a non-empty and non-null.
        /// </summary>
        [DataMember]
        [Required(AllowEmptyStrings = false, ErrorMessage = Constants.SummaryJsonUnitValueNullorEmptyMessage)]
        public String UnitValue { get; set; }

        /// <summary>
        /// Represents the unit of measure, depending on the type of request.
        /// Ex. for some tenant it can be Currency and for some it can be Days or any other value. This value should be a non-empty and non-null
        /// </summary>
        [DataMember]
        [Required(AllowEmptyStrings = false, ErrorMessage = Constants.SummaryJsonUnitOfMeasureNullorEmptyMessage)]
        public String UnitOfMeasure { get; set; }

        /// <summary>
        /// Submitted Date is helpful in identifying the date when the request was submitted.
        /// This should be a not null and non-empty field.
        /// </summary>
        [DataMember]
        [Required(AllowEmptyStrings = false, ErrorMessage = Constants.SummaryJsonSubmittedDateNullorEmptyMessage)]
        public DateTime SubmittedDate { get; set; }

        /// <summary>
        /// Tenant's details page URL.
        /// </summary>
        [DataMember]
        public String DetailPageURL { get; set; }

        /// <summary>
        /// Company code for this request.
        /// </summary>
        [DataMember]
        public String CompanyCode { get; set; }

        /// <summary>
        /// Document identfier
        /// Contains the DisplayDocumentNumber (Document Number used for displaying to the user in a user-friendly manner), DocumentNumber, Fiscal Year (Fiscal year in which the document number exists or is applicable)
        /// Type of ApprovalIdentifier.
        /// </summary>
        [DataMember]
        [Required(ErrorMessage = Constants.SummaryJsonApprovalIdentifierMessage)]
        public ApprovalIdentifier ApprovalIdentifier { get; set; }

        /// <summary>
        /// Submitter is one who has submitted the request from tenant side.
        /// The user can on the Approvals landing page see and sort the requests using submitter values.
        /// This hence, should be a not-null value.
        /// </summary>
        [DataMember]
        public NameAliasEntity Submitter { get; set; }

        /// <summary>
        /// Custom attribute is key value pair that can be sent by the tenant as a completely different entity that needs to be shown in Approvals.
        /// We need to have this value as a non-null value to be displayed
        /// </summary>
        [DataMember]
        public CustomAttribute CustomAttribute { get; set; }

        /// <summary>
        /// The list of approvers should have at least one element in the list making it not null and greater than 0.
        /// We need this information to display on the approver chain on the details page.
        /// </summary>
        [DataMember]
        public List<ApprovalHierarchy> ApprovalHierarchy { get; set; }

        /// <summary>
        /// (Optional) The list of Approval Action (names) that should be displayed to the user. When used, in most cases, this will allow tenant teams to filter and reduce the number of approval actions shown to user. This filters actions from the standard list of actions pre-configured at design time
        /// </summary>
        [DataMember]
        public List<string> ApprovalActionsApplicable { get; set; }

        /// <summary>
        /// Indicates the key/value pair of additional data for request.
        /// </summary>
        [DataMember]
        public Dictionary<String, String> AdditionalData { get; set; }

        /// <summary>
        /// List of attachments which contains attachment id, name and URL - List of Type of Attachment.
        /// </summary>
        [DataMember]
        public List<Attachment> Attachments { get; set; }

        /// <summary>
        /// Notes to be displayed on Approvals UI for approver.
        /// Send this information only if Approvals can pre-fetch this information.
        /// </summary>
        [DataMember]
        public string ApproverNotes { get; set; }

        /// <summary>
        /// Read-only property which acts as a version for the request
        /// This is used to handle stale requests in Approvals.
        /// Needn't be used by any tenant
        /// </summary>
        [DataMember]
        public Guid RequestVersion
        {
            get
            {
                return requestVersion;
            }
            private set
            {
                requestVersion = value;
            }
        }
    }
}