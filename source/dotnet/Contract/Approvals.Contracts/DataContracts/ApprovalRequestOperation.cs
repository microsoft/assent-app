// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace Microsoft.CFS.Approvals.Contracts.DataContracts
{
    using System.Runtime.Serialization;

    /// <summary>
    /// Represents an approval operation.
    /// </summary>
    [DataContract(Name = "ApprovalRequestOperation", Namespace = "http://www.microsoft.com/document/routing/2010/11")]
    public enum ApprovalRequestOperation
    {
        /// <summary>
        /// For Create operation.
        /// </summary>
        [EnumMember]
        Create = 1,

        /// <summary>
        /// For Update operation.
        /// </summary>
        [EnumMember]
        Update = 2,

        /// <summary>
        /// For Delete operation.
        /// </summary>
        [EnumMember]
        Delete = 3,

        /// <summary>
        /// For specific targeted operations.
        /// </summary>
        [EnumMember]
        TargetedAction = 4,

        /// <summary>
        /// For Complete operation.
        /// </summary>
        [EnumMember]
        Complete = 5
    }
}