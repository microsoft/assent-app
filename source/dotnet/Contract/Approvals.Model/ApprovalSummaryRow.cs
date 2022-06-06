// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace Microsoft.CFS.Approvals.Model
{
    using System;
    using Microsoft.Azure.Cosmos.Table;
    public class ApprovalSummaryRow : TableEntity
    {
        private DateTime operationDateTime = new DateTime(2001, 01, 01);

        public string Application { get; set; }

        public string RoutingId { get; set; }

        public string Approver { get; set; }

        public string PreviousApprover { get; set; }

        public string Requestor { get; set; }

        public string DocumentNumber { get; set; }

        public string SummaryJson { get; set; }

        public bool LobPending { get; set; }

        public bool WaitForLOBResponse { get; set; }

        public string ActionTakenOnClient { get; set; }

        public DateTime NextReminderTime { get; set; }

        public string NotificationJson { get; set; }

        public bool LastFailed { get; set; }

        [Obsolete("This property should not be used.")]
        public string LastFailedExceptionMessage { get; set; }

        public string OriginalApprovers { get; set; }

        public DateTime OperationDateTime
        {
            get
            {
                return operationDateTime;
            }
            set
            {
                if (value < new DateTime(2001, 01, 01))
                    operationDateTime = new DateTime(2001, 01, 01);
                else
                    operationDateTime = value;
            }
        }

        public string Xcv { get; set; }

        public string Tcv { get; set; }

        public bool IsRead { get; set; }

        public bool IsOfflineApproval { get; set; }

        public bool IsOutOfSyncChallenged { get; set; }

        [Obsolete("This property should not be used.")]
        public string LastFailedOutOfSyncMessage { get; set; }

        public Guid RequestVersion { get; set; }
    }
}