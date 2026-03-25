// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace Microsoft.CFS.Approvals.SupportServices.Helper.Interface;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using global::Azure.Messaging.ServiceBus;
public interface IServiceBusHelper
{
    /// <summary>
    /// Get topics
    /// </summary>
    /// <returns></returns>
    Task<List<string>> GetTopicsAsync();

    /// <summary>
    /// Get subscriptions
    /// </summary>
    /// <param name="topicName"></param>
    /// <returns></returns>
    Task<List<string>> GetSubscriptionsAsync(string topicName);
    
    /// <summary>
    /// Peek service bus message
    /// </summary>
    /// <param name="topicName"></param>
    /// <param name="subscriptionName"></param>
    /// <param name="messageType"></param>
    /// <returns></returns>
    Dictionary<string, ArrayList> PeekServiceBusMessage(string topicName, string subscriptionName, string messageType);

    /// <summary>
    /// Get all brokered messages for selectd topic and subscription which are either deadletter or active
    /// </summary>
    /// <param name="topicName"></param>
    /// <param name="subscriptionName"></param>
    /// <param name="messageType"></param>
    /// <returns></returns>
    Task<IList<ServiceBusReceivedMessage>> GetMessagesFromSubscriptionAsync(string topicName, string subscriptionName, string messageType);

    /// <summary>
    /// Get message
    /// </summary>
    /// <param name="messages"></param>
    /// <returns></returns>
    Dictionary<string, ArrayList> GetMessage(IList<ServiceBusReceivedMessage> messages);

}
