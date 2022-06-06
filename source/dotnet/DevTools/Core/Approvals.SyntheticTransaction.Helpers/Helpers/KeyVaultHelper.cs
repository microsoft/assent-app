// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace Microsoft.CFS.Approvals.SyntheticTransaction.Helpers.Helpers
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using Microsoft.Azure.KeyVault;
    using Microsoft.Azure.KeyVault.Models;
    using Microsoft.CFS.Approvals.Contracts;
    using Microsoft.CFS.Approvals.SyntheticTransaction.Common.Models;
    using Microsoft.CFS.Approvals.SyntheticTransaction.Helpers.Interface;
    using Microsoft.Extensions.Configuration;
    using Microsoft.IdentityModel.Clients.ActiveDirectory;
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
            var kvc = new KeyVaultClient(new KeyVaultClient.AuthenticationCallback(
                  async (string authority, string resource, string scope) =>
                  {
                      var authContext = new AuthenticationContext(string.Format("{0}{1}", _configuration["AADInstance"], _configuration["TenantID"]));
                      var credential = new ClientCredential(_configuration["KeyVaultClientId"], _configuration["KeyVaultClientSecret"]);
                      AuthenticationResult result = await authContext.AcquireTokenAsync(resource, credential);
                      if (result == null)
                      {
                          throw new InvalidOperationException("Failed to retrieve JWT token");
                      }
                      return result.AccessToken;
                  }));
            var environments = _configuration["Environmentlist"].Split(',').ToList();
            foreach (var environment in environments)
            {
                SecretBundle secretBundle = new SecretBundle();
                secretBundle = kvc.GetSecretAsync(_configuration["KeyVaultSecretUri"], string.Format("{0}-{1}", environment, "TestHarnessConfiguration")).Result;

                var config = JsonConvert.DeserializeObject<JObject>(secretBundle.Value);
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
}