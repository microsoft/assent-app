// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace Microsoft.CFS.Approvals.WatchdogProcessor.BL.Interface;

using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.CFS.Approvals.Model;

public interface IReminderProcessor
{
    /// <summary>
    /// Sends the reminders.
    /// </summary>
    /// <param name="currentTime">The current time.</param>
    /// <param name="maxFailureCount">The maximum failure count.</param>
    /// <param name="batchSize">Size of the batch.</param>
    /// <param name="approvalTenantInfo">The approval tenant information.</param>
    /// <param name="approvalsBaseUrl">The approvals base URL.</param>
    Task SendReminders(DateTime currentTime, int maxFailureCount, int batchSize, List<ApprovalTenantInfo> approvalTenantInfo, string approvalsBaseUrl);
}