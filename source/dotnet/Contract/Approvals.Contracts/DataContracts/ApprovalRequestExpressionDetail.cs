// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace Microsoft.CFS.Approvals.Contracts.DataContracts
{
    using System;

    /// <summary>
    /// This represents a 'details object' for any given tenant.
    /// This is a tenant specific object with metadata about details and details JSON itself.
    /// This object can be embedded inside an ARX and sent to Approvals.
    /// However, this should only be used if Approvals handles details for this specific tenant "By Design", else will be ignored.
    /// </summary>
    public sealed class ApprovalRequestExpressionDetail
    {
        #region Properties

        /// <summary>
        /// DocumentTypeId is unique for each tenant and is a GUID in form of string.
        /// The GUID is assigned by Approvals.
        /// This field is mandatory and helps identify the tenant.
        /// </summary>
        public Guid DocumentTypeId
        { get; set; }

        /// <summary>
        /// Contains the DisplayDocumentNumber, DocumentNumber, Fiscal Year - Type of ApprovalIdentifier.
        /// </summary>
        public ApprovalIdentifier ApprovalIdentifier
        { get; set; }

        /// <summary>
        /// Operation name for detail like DT1, LINE, EXT etc. 
        /// Each supported operation needs to be configured in Approvals based on agreed design and details breakdown.
        /// Only configured operation will be supported on Approvals side.
        /// </summary>
        public string Operation
        { get; set; }

        /// <summary>
        /// Details in JSON format - represent all details for the given ApprovalIdentifier.
        /// </summary>
        public string Detail { get; set; }

        #endregion
    }
}
