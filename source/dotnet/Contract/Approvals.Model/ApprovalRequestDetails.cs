// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.


namespace Microsoft.CFS.Approvals.Model
{
    using System;
    using System.Collections.Generic;
    using Microsoft.CFS.Approvals.Contracts.DataContracts;

    public class ApprovalRequestDetails
    {
        public ApprovalIdentifier ApprovalIdentifier { get; set; }

        public DateTimeOffset CreateDateTime { get; set; }

        public Dictionary<string, string> DetailsData { get; set; }

        public ApprovalTenantInfo ApprovalTenantInfo { get; set; }

        public DeviceNotificationInfo DeviceNotificationInfo { get; set; }

        public List<ApprovalSummaryRow> SummaryRows { get; set; }

        public ApprovalRequestOperation Operation { get; set; }

        public string Xcv { get; set; }

        public string Tcv { get; set; }

        public string BusinessProcessName { get; set; }

        public Dictionary<string, string> TenantTelemetry { get; set; }

        public Boolean IsNotificationSent { get; set; }

        public Boolean IsDetailsLoadSuccess { get; set; }

        public Boolean IsDownloadAttachmentSuccess { get; set; }

        public Boolean RefreshDetails { get; set; }
    }
}
