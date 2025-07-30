// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace Microsoft.CFS.Approvals.SupportServices.Helper.ServiceHelper;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using global::Azure.Data.AppConfiguration;
using global::Azure.Identity;
using global::Azure.Security.KeyVault.Secrets;
using Microsoft.CFS.Approvals.Contracts;
using Microsoft.Extensions.Configuration;

/// <summary>
/// The Application Settings Helper class
/// </summary>
[ExcludeFromCodeCoverage]
public class ApplicationSettingsHelper
{
    /// <summary>
    /// The configuration helper
    /// </summary>
    private readonly IConfiguration _configuration;

    /// <summary>
    /// Constructor of ApplicationSettingsHelper
    /// </summary>
    /// <param name="configuration"></param>
    public ApplicationSettingsHelper(IConfiguration configuration)
    {
        _configuration = configuration;
    }

    /// <summary>
    /// Get settings
    /// </summary>
    /// <returns></returns>
    public Dictionary<string, IConfiguration> GetSettings()
    {
        #region Fetch Azure App Configuration Store
        // Select credential based on environment: DefaultAzureCredential for DEBUG, ManagedIdentityCredential for production.
        global::Azure.Core.TokenCredential credential;
#if DEBUG
        credential = new DefaultAzureCredential();  // CodeQL [SM05137] Suppress CodeQL issue since we only use DefaultAzureCredential in development environments.
#else
        credential = new ManagedIdentityCredential();
#endif

        ConfigurationClient _client = new ConfigurationClient(new Uri(_configuration[Constants.AzureAppConfigurationUrl]), credential);
        var settingsSelector = new SettingSelector() { KeyFilter = "*" };
        var settings = _client.GetConfigurationSettings(settingsSelector);
        var labels = settings.GroupBy(s => s.Label);
        Dictionary<string, IConfiguration> configurations = new Dictionary<string, IConfiguration>();
        foreach (var label in labels)
        {
            var setingsByLabel = settings.Where(s => s.Label == label.Key);
            IConfiguration Config = new ConfigurationBuilder().Build();
            Config = new ConfigurationBuilder().AddEnvironmentVariables().Build();
            foreach (var setting in setingsByLabel)
            {
                if (setting is SecretReferenceConfigurationSetting secretReference)
                {
                    var identifier = new KeyVaultSecretIdentifier(secretReference.SecretId);
                    var secretClient = new SecretClient(identifier.VaultUri, credential);
                    var secret = secretClient.GetSecretAsync(identifier.Name, identifier.Version).Result;
                    Config[setting.Key] = secret.Value.Value;
                }
                else
                {
                    Config[setting.Key] = setting.Value;
                }

            }
            configurations.Add(label.Key, Config);
        }
        return configurations;
        #endregion
    }
}