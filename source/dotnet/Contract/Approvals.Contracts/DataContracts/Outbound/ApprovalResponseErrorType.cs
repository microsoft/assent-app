// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace Microsoft.CFS.Approvals.Contracts.DataContracts
{
    using System.Runtime.Serialization;

    /// <summary>
    /// Classification of Error Type in Approval Response object.
    /// </summary>
    public enum ApprovalResponseErrorType
    {
        /// <summary>
        /// Intended Error.
        /// </summary>
        [EnumMember]
        IntendedError = 1,

        /// <summary>
        /// Unintended Non-Transient Error.
        /// </summary>
        [EnumMember]
        UnintendedError = 2,

        /// <summary>
        /// Unintended Transient Error.
        /// </summary>
        [EnumMember]
        UnintendedTransientError = 3
    }
}