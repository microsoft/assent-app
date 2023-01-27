// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.


namespace Microsoft.CFS.Approvals.SyntheticTransaction.Helpers.Helpers;
using System.Collections.Generic;
using Microsoft.CFS.Approvals.SyntheticTransaction.Common.Models;

/// <summary>
/// Configuration Setting class
/// </summary>
public class ConfigurationSetting
{
    public readonly Dictionary<string, AppSettings> appSettings;

    /// <summary>
    /// Constructor of ConfigurationSetting
    /// </summary>
    /// <param name="keyValuePairs"></param>
    public ConfigurationSetting(Dictionary<string, AppSettings> keyValuePairs)
    {
        appSettings = keyValuePairs;
    }
}
