// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace Microsoft.CFS.Approvals.SyntheticTransaction.Helpers.Helpers;

using System;
using System.Collections.Generic;
using System.Linq;
using global::Azure.Identity;
using global::Azure.Security.KeyVault.Secrets;
using Microsoft.Azure.KeyVault;
using Microsoft.CFS.Approvals.SyntheticTransaction.Common.Models;
using Microsoft.CFS.Approvals.SyntheticTransaction.Helpers.Interface;
using Microsoft.Extensions.Configuration;
using Microsoft.Identity.Client;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

/// <summary>
/// KeyVault Helper class
/// </summary>
public class KeyVaultHelper : IKeyVaultHelper
{
    /// <summary>
    /// The coniguration helper
    /// </summary>
    private readonly IConfiguration _configuration;

    /// <summary>
    /// Constructor of KeyVault helper
    /// </summary>
    /// <param name="configuration"></param>
    public KeyVaultHelper(IConfiguration configuration)
    {
        _configuration = configuration;
    }

    /// <summary>
    /// Get keyvault
    /// </summary>
    /// <returns></returns>
    public Dictionary<string, AppSettings> GetKeyVault()
    {
        Dictionary<string, AppSettings> appSettings = new Dictionary<string, AppSettings>();
#if DEBUG
        var azureCredential = new DefaultAzureCredential(); // CodeQL [SM05137] Suppress CodeQL issue since we only use DefaultAzureCredential in development environments.
#else
        var azureCredential = new ManagedIdentityCredential();
#endif
        var environments = _configuration["Environmentlist"].Split(',').ToList();
        foreach (var environment in environments)
        {
            SecretClient secretClient = new SecretClient(new Uri(_configuration["KeyVaultSecretUri"]), azureCredential);
            var secretResponse = secretClient.GetSecret(string.Format("{0}-{1}", environment, "TestHarnessConfiguration"));
            var config = JsonConvert.DeserializeObject<JObject>(secretResponse.Value.Value);
            if (!appSettings.ContainsKey(environment))
            {
                appSettings[environment] = new AppSettings
                {
                    StorageAccountKey = config?["StorageAccountKey"].ToString(),
                    StorageAccountName = config?["StorageAccountName"].ToString(),
                    PayloadReceiverServiceAppKey = config?["PayloadReceiverServiceAppKey"].ToString(),
                    PayloadReceiverServiceClientId = config?["PayloadReceiverServiceClientId"].ToString(),
                    PayloadReceiverServiceURL = config?["PayloadReceiverServiceURL"].ToString(),
                    ResourceURL = config?["ResourceURL"].ToString()
                };
            }
        }

        return appSettings;
    }
}