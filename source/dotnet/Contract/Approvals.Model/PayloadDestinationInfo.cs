// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.


namespace Microsoft.CFS.Approvals.Model
{
    using System;
    using System.Collections.Generic;
    using System.Text;
    public class PayloadDestinationInfo
    {
        public bool UsefulInfoAvailable { get; set; }

        public PayloadDestinationType DestinationType { get; set; }

        public string Namespace { get; set; }

        public string Entity { get; set; }

        public string AcsIdentity { get; set; }

        public string SecretKey { get; set; }

    }

    public enum PayloadDestinationType
    {
        AzureServiceBusTopic,
        AzureServiceBusQueue,
        AzureQueue
    }
}
