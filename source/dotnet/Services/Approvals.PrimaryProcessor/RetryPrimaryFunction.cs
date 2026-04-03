// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace Microsoft.CFS.Approvals.PrimaryAzFunction;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using global::Azure.Messaging.ServiceBus;
using Microsoft.Azure.Functions.Worker;
using Microsoft.CFS.Approvals.Common.BL.Interface;
using Microsoft.CFS.Approvals.Core.BL.Interface;
using Microsoft.CFS.Approvals.LogManager.Model;
using Microsoft.CFS.Approvals.LogManager.Provider.Interface;
using Microsoft.CFS.Approvals.Model;

public class RetryPrimaryFunction
{
    private readonly IApprovalsTopicReceiver _genericReceiver;
    private readonly List<ApprovalTenantInfo> _allTenantInfo;
    private readonly ILogProvider _logger;

    public RetryPrimaryFunction(IApprovalTenantInfoHelper approvalTenantInfoHelper, IApprovalsTopicReceiver genericReceiver, ILogProvider logger)
    {
        _allTenantInfo = approvalTenantInfoHelper.GetTenantInfoByHost("Worker");
        _genericReceiver = genericReceiver;
        _logger = logger;
    }

    [Function("RetryPrimaryFunction")]
    public async Task Run(
        [ServiceBusTrigger("%TopicNameRetry%", "%SubscriptionName%", Connection = "ServiceBusEndpoint")] ServiceBusReceivedMessage message,
        ServiceBusMessageActions messageActions)
    {
        var logData = new Dictionary<LogDataKey, object>();

        try
        {
            var messageAge = DateTimeOffset.UtcNow - message.EnqueuedTime;
            var docTypeId = message.ApplicationProperties["ApplicationId"].ToString();
            _genericReceiver.TenantInfo = _allTenantInfo.FirstOrDefault(x => x.DocTypeId.Equals(docTypeId, StringComparison.InvariantCultureIgnoreCase));
            string blobId = System.Text.Encoding.UTF8.GetString(message.Body.ToArray());

            logData.Add(LogDataKey.MessageId, message.MessageId);
            logData.Add(LogDataKey.TenantId, _genericReceiver.TenantInfo?.TenantId);
            logData.Add(LogDataKey.TenantName, _genericReceiver.TenantInfo?.AppName);
            logData.Add(LogDataKey.DocumentTypeId, _genericReceiver.TenantInfo?.DocTypeId);

            _logger.LogInformation(TrackingEvent.MessagePickedUp, logData);

            await _genericReceiver.OnRetryMessageRecieved(blobId, message);
            await messageActions.CompleteMessageAsync(message);
        }
        catch (Exception ex)
        {
            _logger.LogError(TrackingEvent.MessageProcessingException, ex, logData);
            if (ex is ServiceBusException sbEx && sbEx.IsTransient)
            {
                // For transient exceptions, let the host retry policy handle it
                throw;
            }
            else
            {
                // For non-transient exceptions, dead-letter the message
                string truncatedDescription = ex.Message.Length > 4096 ? ex.Message.Substring(0, 4096) : ex.Message;
                await messageActions.DeadLetterMessageAsync(message, deadLetterReason: "ProcessingRetryError", deadLetterErrorDescription: truncatedDescription);
            }
        }
    }
}