// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace Microsoft.CFS.Approvals.Model
{
    using Microsoft.Azure.Cosmos.Table;

    public class EditableFieldAuditEntity : TableEntity
    {
        public string ClientType { get; set; }

        public string EditorAlias { get; set; }

        public string EditableFieldJSON { get; set; }

        public string LoggedInUser { get; set; }

        public string DocumentNumber
        {
            get
            {
                return base.PartitionKey;
            }
        }

        public string Id
        {
            get
            {
                return base.RowKey;
            }
        }

        public string EditedDateTime
        {
            get
            {
                return base.Timestamp.ToUniversalTime().ToString();
            }
        }
    }

}
