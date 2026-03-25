// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Azure.Messaging.ServiceBus;

/// <summary>
/// Wrapper for ServiceBus senders that ensures proper disposal of all senders on shutdown.
/// </summary>
public class ServiceBusSenderDictionary : IAsyncDisposable
{
    private readonly Dictionary<string, ServiceBusSender> _serviceBusSenders;

    public ServiceBusSenderDictionary(Dictionary<string, ServiceBusSender> senders)
    {
        _serviceBusSenders = senders;
    }

    /// <summary>
    /// Gets the ServiceBus sender for the specified key.
    /// </summary>
    /// <param name="key">The sender key (topic/queue name).</param>
    /// <returns>The ServiceBusSender instance.</returns>
    /// <exception cref="KeyNotFoundException">If the key doesn't exist.</exception>
    public ServiceBusSender this[string key] => _serviceBusSenders.TryGetValue(key, out var sender) ?
        sender :
        throw new KeyNotFoundException($"The key '{key}' was not found in the sender dictionary.");

    public IEnumerable<string> Keys => _serviceBusSenders.Keys;

    public bool TryGetValue(string key, out ServiceBusSender sender) => _serviceBusSenders.TryGetValue(key, out sender);

    public async ValueTask DisposeAsync()
    {
        List<Exception> exceptions = [];
        foreach (var sender in _serviceBusSenders.Values)
        {
            try
            {
                await sender.DisposeAsync();
            }
            catch (Exception ex)
            {
                exceptions.Add(ex);
            }
        }
        if (exceptions.Count > 0)
        {
            throw new AggregateException("One or more senders failed to dispose.", exceptions);
        }
    }
}