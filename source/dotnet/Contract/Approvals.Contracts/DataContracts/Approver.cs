// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace Microsoft.CFS.Approvals.Contracts.DataContracts
{
    using System;
    using System.Collections.Generic;
    using System.Runtime.Serialization;

    /// <summary>
    /// Represents the current approver.
    /// </summary>
    [DataContract]
    public class Approver : NameAliasEntity
    {
        /// <summary>
        /// Instantiates an Approver object setting the CanEdit flag to false by default
        /// </summary>
        public Approver()
        {
            CanEdit = false;
        }

        /// <summary>
        /// A details template, if associated with given approver.
        /// </summary>
        [DataMember]
        public string DetailTemplate { get; set; }

        /// <summary>
        /// Provide the alias of Original Approver(s)
        /// This should be used when delegation feature is supported by Approvals.
        /// </summary>
        [Obsolete]
        [DataMember]
        public List<string> Delegation { get; set; }

        /// <summary>
        /// Provide a list of original approvers
        /// </summary>
        [DataMember]
        public List<string> OriginalApprovers { get; set; }

        /// <summary>
        /// Provide info whether given approver can edit the details
        /// </summary>
        [DataMember]
        public Boolean CanEdit { get; set; }
    }
}