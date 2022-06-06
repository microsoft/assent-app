// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace Microsoft.CFS.Approvals.Contracts.DataContracts
{
    using System;
    using System.Collections.Generic;
    using System.Runtime.Serialization;

    [DataContract]
    public class ApprovalsTelemetry
    {
        public ApprovalsTelemetry()
        {
            Xcv = Guid.NewGuid().ToString();
        }

        /// <summary>
        /// This acts like a TransactionID between tenant and Approvals. When used, it is recommended to be of type GUID.
        /// </summary>
        [DataMember]
        public string Tcv { get; set; }

        /// <summary>
        /// This acts like a CorrelationID between tenant and Approvals. When used, it is recommended to be of type GUID.
        /// </summary>
        [DataMember]
        public string Xcv { get; set; }

        /// <summary>
        /// This will be used as business process name logged in ApplicationInsight logs.
        /// </summary>
        [DataMember]
        public string BusinessProcessName { get; set; }

        /// <summary>
        /// A property bag for Tenants to send additional telemetry data which tenants needs Approvals to log additionally
        /// </summary>
        [DataMember]
        public Dictionary<string, string> TenantTelemetry { get; set; }
    }
}