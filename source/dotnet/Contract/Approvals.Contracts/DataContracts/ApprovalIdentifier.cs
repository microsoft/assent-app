// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace Microsoft.CFS.Approvals.Contracts.DataContracts
{
    using System.Runtime.Serialization;
    using System.ComponentModel.DataAnnotations;

    /// <summary>
    /// Identifies approval information.
    /// </summary>
    [DataContract]
    public class ApprovalIdentifier
    {
        /// <summary>
        /// Default constructor.
        /// </summary>
        public ApprovalIdentifier()
        {
            DocumentNumber = null;
            DisplayDocumentNumber = null;
            FiscalYear = null;
        }

        /// <summary>
        /// Document Number used for displaying to the user in a user friendly manner.
        /// This can be empty if tenants use a basic document number, in which case Document Number should be used.
        /// </summary>
        [DataMember]
        [Required(AllowEmptyStrings = false, ErrorMessage = Constants.ApprovalIdentifierDispDocNumberMessage)]
        public string DisplayDocumentNumber { get; set; }

        /// <summary>
        /// Primary document number or identifier.
        /// </summary>
        [DataMember]
        [Required(AllowEmptyStrings = false, ErrorMessage = Constants.ApprovalIdentifierDocNumberMessage)]
        public string DocumentNumber { get; set; }

        /// <summary>
        /// Fiscal year in which the document number exists or is applicable.
        /// </summary>
        [DataMember]
        public string FiscalYear { get; set; }
    }
}