// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace Microsoft.CFS.Approvals.SyntheticTransaction.Common.Helper;
using Microsoft.CFS.Approvals.SyntheticTransaction.Common.Models;
using Microsoft.Extensions.Configuration;

/// <summary>
/// The Configuration Helper class
/// </summary>
public class ConfigurationHelper
{
    /// <summary>
    /// The configuration helper
    /// </summary>
    private readonly IConfiguration config;

    // <summary>
    // Initializes a new instance of ConfigurationHelper class.
    // </summary>
    public ConfigurationHelper(IConfiguration _config)
    {
        config = _config;
    }

    // <summary>
    // Get value for Application configuration
    // </summary>
    // <returns>
    // This method returns the value for configuration
    // </returns>
    public string GetConfigurationValue(ConfigurationKeyEnum configurationKeyEnum)
    {
        return config[configurationKeyEnum.ToString()];
    }
}