// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace Microsoft.CFS.Approvals.Model;

public class EditableConfigurationEntity : BaseTableEntity
{
    #region Constructor
    public EditableConfigurationEntity()
    {

    }
    #endregion

    public string RegularExpression
    {
        get;
        set;
    }


    //PartitionKey - TenantID 

    //RowKey - ColumnName
}