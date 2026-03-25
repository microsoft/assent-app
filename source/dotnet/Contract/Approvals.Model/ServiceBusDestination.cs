// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace Microsoft.CFS.Approvals.Model;

using Newtonsoft.Json;


/// <summary>
/// Attachment details with additional data
/// </summary>
public class ServiceBusDestination
{
    /// <summary>
    /// queue or topic name
    /// </summary>
    [JsonProperty("destination")]
    public string Destination { get; set; }

    /// <summary>
    /// Topic filters 
    /// </summary>
    [JsonProperty("filter")]
    public string Filter { get; set; }

}