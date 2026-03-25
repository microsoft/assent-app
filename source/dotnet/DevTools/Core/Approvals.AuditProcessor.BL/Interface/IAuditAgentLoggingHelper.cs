// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace Microsoft.CFS.Approvals.AuditProcessor.BL.Interface;

using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using global::Azure.Messaging.ServiceBus;
using Microsoft.CFS.Approvals.Contracts.DataContracts;
using Microsoft.CFS.Approvals.LogManager.Model;
using Microsoft.CFS.Approvals.Model;

public interface IAuditAgentLoggingHelper
{
    /// <summary>
    /// Takes a BrokeredMessage and a TrackingEvent and fetches the message details so it can be meaningfully logged
    /// </summary>
    /// <param name="trackingEvent"></param>
    /// <param name="brokeredMessage"></param>
    /// <param name="loggingTime"></param>
    /// <param name="approvalNotificationARXObj"></param>
    Task Log(TrackingEvent trackingEvent, ServiceBusReceivedMessage brokeredMessage, DateTime loggingTime, List<ApprovalRequestExpressionExt> approvalNotificationARXObj = null);

    /// <summary>
    /// Logs to either the logging docdb or AI depending on event id configuration
    /// </summary>
    /// <param name="trackingEvent"></param>
    /// <param name="brokeredMessageID"></param>
    /// <param name="arn"></param>
    /// <param name="tenantInfo"></param>
    /// <param name="exceptionMessage"></param>
    void Log(TrackingEvent trackingEvent, string brokeredMessageID, string arn, ApprovalTenantInfo tenantInfo, DateTime loggingTime, string exceptionMessage = "");
}