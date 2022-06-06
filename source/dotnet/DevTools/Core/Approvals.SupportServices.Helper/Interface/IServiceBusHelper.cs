// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace Microsoft.CFS.Approvals.SupportServices.Helper.Interface
{
    using System.Collections;
    using System.Collections.Generic;
    using Microsoft.Azure.ServiceBus;
    public interface IServiceBusHelper
    {
        List<string> GetTopics();
        List<string> GetSubscriptions(string topicName);
        Dictionary<string, ArrayList> PeekServiceBusMessage(string topicName, string subscriptionName, string messageType);
        IList<Message> GetMessagesFromSubscription(string topicName, string subscriptionName, string messageType);
        Dictionary<string, ArrayList> GetMessage(IList<Message> messages);

    }
}
