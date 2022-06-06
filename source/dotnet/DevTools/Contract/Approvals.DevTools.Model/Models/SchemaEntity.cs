// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace Microsoft.CFS.Approvals.DevTools.Model.Models
{
    using Microsoft.Azure.Cosmos.Table;

    /// <summary>
    /// Schema Entity class
    /// </summary>
    public class SchemaEntity : TableEntity
    {
        /// <summary>
        /// Constructor of SchemaEntity
        /// </summary>
        public SchemaEntity()
        {

        }
        public string Schema { get; set; }
    }
}
