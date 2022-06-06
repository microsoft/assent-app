// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace Microsoft.CFS.Approvals.Contracts.DataContracts
{
    using System;
    using System.Runtime.Serialization;

    /// <summary>
    /// Fault contract for the ApprovalService.
    /// </summary>
    [DataContract]
    public sealed class ApprovalServiceFault
    {
        /// <summary>
        /// ID - Type of Guid
        /// </summary>
        [DataMember]
        public Guid id { get; set; }

        /// <summary>
        /// Error/exception message text.
        /// </summary>
        [DataMember]
        public string MessageText { get; set; }

        /// <summary>
        /// Stack trace for the error/exception.
        /// </summary>
        [DataMember]
        public string StackTrace { get; set; }


    }
}
