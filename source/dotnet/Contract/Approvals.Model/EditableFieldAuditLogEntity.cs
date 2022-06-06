// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace Microsoft.CFS.Approvals.Model
{
    public class EditableFieldAuditLogEntity
    {
        public string Id { get; set; }

        public string DocumentNumber { get; set; }

        public string ClientType { get; set; }

        public string EditorAlias { get; set; }

        public string EditableFieldJSON { get; set; }

        public string LoggedInUser { get; set; }

        public string EditedDateTime { get; set; }
    }
}
