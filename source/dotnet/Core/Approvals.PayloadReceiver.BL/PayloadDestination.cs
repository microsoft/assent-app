// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace Microsoft.CFS.Approvals.PayloadReceiver.BL;

using Microsoft.CFS.Approvals.Contracts;
using Microsoft.CFS.Approvals.Model;
using Microsoft.CFS.Approvals.PayloadReceiver.BL.Interface;
using Microsoft.Extensions.Configuration;

/// <summary>
/// The Payload Destination class
/// </summary>
public class PayloadDestination : IPayloadDestination
{
    /// <summary>
    /// The configuration
    /// </summary>
    private readonly IConfiguration _config;

    /// <summary>
    /// Constructor of PayloadDestination
    /// </summary>
    /// <param name="config"></param>
    public PayloadDestination(IConfiguration config)
    {
        _config = config;
    }

    /// <summary>
    /// Gets the final destination for payload for each given tenant
    /// TODO:: Move this logic to get data from DAL (DAL class currently missing) as this acts as a BL
    /// </summary>
    /// <param name="tenantId"></param>
    /// <returns></returns>
    public PayloadDestinationInfo GetPayloadDestinationAndConfigInfo(string tenantId)
    {
        // Based on tenant id, find out from TenantInfoTable what the

        // Hard coded for now
        PayloadDestinationInfo payloadDestinationInfo = new PayloadDestinationInfo
        {
            DestinationType = PayloadDestinationType.AzureServiceBusTopic,

            Namespace = _config[ConfigurationKey.ServiceBusNamespace.ToString()],

            Entity = _config[ConfigurationKey.TopicNameMain.ToString()],

            SecretKey = _config[ConfigurationKey.ServiceBusIssuerSecret.ToString()],

            AcsIdentity = _config[ConfigurationKey.ServiceBusIssuerName.ToString()],

            UsefulInfoAvailable = true
        };

        return payloadDestinationInfo;
    }
}