// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace NotificationAzFunction
{
    using System.Threading.Tasks;
    using Microsoft.Azure.ServiceBus;
    using Microsoft.Azure.WebJobs;
    using Microsoft.CFS.Approvals.Common.BL.Interface;
    using Microsoft.Extensions.Logging;

    public class NotificationFunction
    {
        private readonly IApprovalsTopicReceiver _notificationReceiver;

        public NotificationFunction(IApprovalsTopicReceiver notificationReceiver)
        {
            _notificationReceiver = notificationReceiver;
        }

        [FunctionName("Notification")]
        public async Task Run([ServiceBusTrigger("%TopicNameNotification%", "%SubscriptionNameNotification%", Connection = "ServiceBusConnectionString")] Message message, ILogger logger)
        {
            logger.LogInformation($"C# ServiceBus topic trigger function processed message: {message}");
            string blobId = System.Text.Encoding.UTF8.GetString(message.Body);
            await _notificationReceiver.OnMainMessageReceived(blobId, message);
        }
    }
}