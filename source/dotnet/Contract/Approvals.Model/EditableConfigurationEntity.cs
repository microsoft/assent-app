// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace Microsoft.CFS.Approvals.Model
{
    using Microsoft.Azure.Cosmos.Table;

    public class EditableConfigurationEntity : TableEntity
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
}
