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
    public class Approver : User
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
        public string DetailTemplate { get; set; }

        /// <summary>
        /// Provide a list of original approvers
        /// </summary>
        public List<string> OriginalApprovers { get; set; }

        /// <summary>
        /// Provide info whether given approver can edit the details
        /// </summary>
        public Boolean CanEdit { get; set; }

        /// <summary>
        /// Flag to specify if the approver is a backup approver
        /// </summary>
        public bool IsBackupApprover { get; set; }
    }
}