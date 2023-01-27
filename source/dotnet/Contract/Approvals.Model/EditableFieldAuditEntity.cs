// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System;

namespace Microsoft.CFS.Approvals.Model;

public class EditableFieldAuditEntity : BaseTableEntity
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
            return DateTime.Parse(base.Timestamp.ToString()).ToUniversalTime().ToString();
        }
    }
}