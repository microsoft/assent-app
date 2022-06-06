// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace Microsoft.CFS.Approvals.Model
{
    using Microsoft.CFS.Approvals.Contracts.DataContracts;
    using System;
    using System.Collections.Generic;

    /// <summary>
    /// This class is used to return the data from Summary Controller.
    /// </summary>
    public class ApprovalSummaryData
    {
        public int TenantId { get; set; }

        public string DocumentTypeId { get; set; }

        public string Approver { get; set; }

        public String SummaryJson { get; set; }

        public string AppName { get; set; }

        public string CondensedAppName { get; set; }

        public Approver Submitter { get; set; }

        public string Title { get; set; }

        public string Amount { get; set; }

        public string CurrencyCode { get; set; }

        public DateTime? SubmittedDate { get; set; }

        public string DocumentNumber { get; set; }

        public TenantCustomAttribute CustomAttribute { get; set; }

        public List<DetailOperation> DetailOperations { get; set; }

        public string TemplateName { get; set; }

        public string CompanyCode { get; set; }

        public bool IsOfflineApprovalSupported { get; set; }

        public bool ReadDetails { get; set; }

        #region New WebPortal Properties

        /// <summary>
        /// Data to identify document
        /// </summary>
        public DocumentIdentifier ApprovalIdentifier { get; set; }

        /// <summary>
        /// Total Amount
        /// </summary>
        public string UnitValue { get; set; }

        /// <summary>
        /// Currency Code
        /// </summary>
        public string UnitOfMeasure { get; set; }

        public Dictionary<String, String> AdditionalData { get; set; }

        #endregion New WebPortal Properties

        public bool LastFailed { get; set; }
        public string LastFailedExceptionMessage { get; set; }

        public string Xcv { get; set; }

        public string BusinessProcessName { get; set; }

        public bool IsRead { get; set; }

        public bool IsControlsAndComplianceRequired { get; set; }

        public bool IsBackgroundApprovalSupportedUpfront { get; set; }

        public bool IsOutOfSyncChallenged { get; set; }

        public bool IsOfflineApproval { get; set; }

        public bool LobPending { get; set; }
    }

    public class TenantCustomAttribute
    {
        public string CustomAttributeName { get; set; }
        public string CustomAttributeValue { get; set; }
    }

    public class DetailOperation
    {
        public bool SupportsPagination { get; set; }

        public string EndpointData { get; set; }

        public string OperationType { get; set; }
    }

    public class DocumentIdentifier
    {
        public string DisplayDocumentNumber { get; set; }

        public string DocumentNumber { get; set; }

        public string FiscalYear { get; set; }

        public string DocumentNumberPrefix { get; set; }
    }
}
