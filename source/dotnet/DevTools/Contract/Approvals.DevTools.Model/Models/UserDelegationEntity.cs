// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace Microsoft.CFS.Approvals.DevTools.Model.Models
{
    using System;
    using Microsoft.Azure.Cosmos.Table;

    /// <summary>
    /// User Delegation Entity class
    /// </summary>
    public class UserDelegationEntity:TableEntity
    {
        public int Id { get; set; }
        public string ManagerAlias { get; set; }
        public string DelegatedToAlias { get; set; }
        public int TenantId { get; set; }
        public DateTime DateFrom { get; set; }
        public DateTime DateTo { get; set; }
        public int AccessType { get; set; }
        public bool IsHidden { get; set; }
    }
}
