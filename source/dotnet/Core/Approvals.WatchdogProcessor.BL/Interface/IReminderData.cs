// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace Microsoft.CFS.Approvals.WatchdogProcessor.BL.Interface
{
    using System;
    using System.Collections.Generic;
    using Microsoft.CFS.Approvals.Model;

    public interface IReminderData
    {
        /// <summary>
        /// Gets summary rows for which watchdog reminders need to be sent
        /// </summary>
        /// <param name="currentTime">Current UTC Time</param>
        /// <param name="approvalTenantInfo">ApprovalTenantInfo for which Digest Email functionality is enabled</param>
        /// <returns>List of summary rows needing watchdog reminders</returns>
        IEnumerable<ApprovalSummaryRow> GetApprovalsNeedingReminders(DateTime currentTime, List<ApprovalTenantInfo> approvalTenantInfo);

        /// <summary>
        /// Update Summary Row for updated Next reminder time
        /// </summary>
        /// <param name="approvalTenantInfo"
        /// <param name="summaryToUpdate">summary row to be updated</param>
        void UpdateReminderInfo(ApprovalTenantInfo approvalTenantInfo, ApprovalSummaryRow summaryToUpdate);
    }
}