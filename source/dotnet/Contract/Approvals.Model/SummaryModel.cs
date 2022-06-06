// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace Microsoft.CFS.Approvals.Model
{
    using System;
    using System.Collections.Generic;
    using Microsoft.CFS.Approvals.Contracts.DataContracts;

    public class SummaryModel
    {
        public ApprovalIdentifier ApprovalIdentifier { get; set; }

        public string Title { get; set; }

        //public Submitter Submitter { get; set; }

        public Approver Approver { get; set; }

        public string UnitOfMeasure { get; set; }

        public string UnitValue { get; set; }

        public DateTime SubmittedDate { get; set; }

        public CustomAttribute CustomAttribute { get; set; }

        public string DetailPageURL { get; set; }

        public string DocumentTypeId { get; set; }

        public Dictionary<String, String> AdditionalData { get; set; }

        public List<ApprovalHierarchy> ApprovalHierarchy { get; set; }
    }
}
