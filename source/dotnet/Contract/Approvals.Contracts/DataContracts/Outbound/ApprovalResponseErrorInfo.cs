// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace Microsoft.CFS.Approvals.Contracts.DataContracts
{
    using System.Collections.Generic;
    using System.Runtime.Serialization;

    [DataContract]
    public class ApprovalResponseErrorInfo
    {
        public ApprovalResponseErrorInfo()
        {
            // Setting default value
            ErrorType = ApprovalResponseErrorType.UnintendedError;
        }

        /// <summary>
        /// List of technical error messages that occurred on the tenant system for this transaction.
        /// The error list can comprise of all the related errors that happened in the tenant's downstream systems as well for enhanced telemetry.
        /// </summary>
        [DataMember]
        public List<string> ErrorMessages { get; set; }

        /// <summary>
        /// Indicates the type of error that occurred. 
        /// Default value is NonTransient, but should be overwritten when error type is known by the source system for improved error handling at destination (Approvals).
        /// </summary>
        [DataMember]
        public ApprovalResponseErrorType ErrorType { get; set; }


        /// <summary>
        /// Transaction retry interval (in mins)
        /// Mandatory parameter if ErrorType is known to be UnintendedTransientError. 
        /// This will be to retry after this interval. Please use this only if a transient error occurred and not when a non-transient or fatal error occurs
        /// </summary>
        [DataMember]
        public int RetryInterval { get; set; }
    }
}