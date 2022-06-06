// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace Microsoft.CFS.Approvals.Contracts.DataContracts
{
    using System;

    public class ApprovalRequestExpressionExt : ApprovalRequestExpression
    {
        public Boolean IsDeleteOperationComplete { get; set; }

        public Boolean IsCreateOperationComplete { get; set; }

        public Boolean IsHistoryLogged { get; set; }

        public Boolean IsNotificationSent { get; set; }

        public Boolean IsDetailsLoadSuccess { get; set; }

        public Boolean IsDownloadAttachmentSuccess { get; set; }
    }
}
