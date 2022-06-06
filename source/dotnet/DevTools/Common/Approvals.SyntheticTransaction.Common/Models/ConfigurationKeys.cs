// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace Microsoft.CFS.Approvals.SyntheticTransaction.Common.Models
{
    using Microsoft.Azure.Cosmos.Table;

    /// <summary>
    /// Configuration Keys class
    /// </summary>
    public class ConfigurationKeys : TableEntity
    {
        /// <summary>
        /// Constructor of ConfigurationKeys
        /// </summary>
        public ConfigurationKeys()
        { }

        public string KeyValue { get; set; }
    }
}