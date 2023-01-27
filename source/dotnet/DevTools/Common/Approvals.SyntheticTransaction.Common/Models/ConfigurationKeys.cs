// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace Microsoft.CFS.Approvals.SyntheticTransaction.Common.Models;

using Microsoft.CFS.Approvals.Model;

/// <summary>
/// Configuration Keys class
/// </summary>
public class ConfigurationKeys : BaseTableEntity
{
    /// <summary>
    /// Constructor of ConfigurationKeys
    /// </summary>
    public ConfigurationKeys()
    { }

    public string KeyValue { get; set; }
}