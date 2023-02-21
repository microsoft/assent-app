// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace Microsoft.CFS.Approvals.AuditProcessor.BL.Interface;

using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Azure.ServiceBus;
using Microsoft.CFS.Approvals.Contracts.DataContracts;
using Microsoft.CFS.Approvals.LogManager.Model;
using Microsoft.CFS.Approvals.Model;

public interface IAuditAgentLoggingHelper
{
    Task Log(TrackingEvent trackingEvent, Message brokeredMessage, DateTime loggingTime, List<ApprovalRequestExpressionExt> approvalNotificationARXObj = null);

    void Log(TrackingEvent trackingEvent, string brokeredMessageID, string arn, ApprovalTenantInfo tenantInfo, DateTime loggingTime, string exceptionMessage = "");
}