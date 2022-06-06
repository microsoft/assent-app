// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace Microsoft.CFS.Approvals.SupportServices.Helper.ServiceHelper
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics.CodeAnalysis;
    using System.Linq;
    using Microsoft.Azure.KeyVault;
    using Microsoft.Azure.KeyVault.Models;
    using Microsoft.CFS.Approvals.Contracts;
    using Microsoft.CFS.Approvals.DevTools.Model.Models;
    using Microsoft.Extensions.Configuration;
    using Microsoft.IdentityModel.Clients.ActiveDirectory;
    using Newtonsoft.Json;
    using Newtonsoft.Json.Linq;

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
        public Dictionary<string, AppSettings> GetSettings()
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
                secretBundle = kvc.GetSecretAsync(_configuration["KeyVaultSecretUri"], string.Format("{0}-{1}", environment, "SupportPortalConfiguration")).Result;

                var config = JsonConvert.DeserializeObject<JObject>(secretBundle.Value);
                if (!appSettings.ContainsKey(environment))
                {
                    appSettings[environment] = new AppSettings
                    {
                        StorageAccountKey = config?["StorageAccountKey"].ToString(),
                        StorageAccountName = config?["StorageAccountName"].ToString(),
                        ServiceBusConnection = config?["ServiceBusConnection"].ToString(),
                        ServiceBusTopics = config?["ServiceBusTopics"].ToString(),
                        AIBaseURL = config?["AIBaseURL"].ToString(),
                        GraphAPIAuthenticationURL = config["GraphAPIAuthenticationURL"].ToString(),
                        GraphAPIAuthString = config["GraphAPIAuthString"].ToString(),
                        GraphAPIClientId = config["GraphAPIClientId"].ToString(),
                        GraphAPIClientSecret = config["GraphAPIClientSecret"].ToString(),
                        GraphAPIResource = config["GraphAPIResource"].ToString(),
                        AIScope = config["AIScope"].ToString(),
                        FunctionAppConfiguration = config["FunctionAppConfiguration"].ToString(),
                        TestTenantConfiguration = config["TestTenantConfiguration"].ToString(),
                        StorageConnection = config["StorageConnection"].ToString(),
                        TenantIconBlobUrl = config["TenantIconBlobUrl"].ToString(),
                        SamplePayloadBlobContainer = config["SamplePayloadBlobContainer"].ToString(),
                        TenantIconBlob = config["TenantIconBlob"].ToString(),
                        PayloadProcessingFunctionURL = config["PayloadProcessingFunctionURL"].ToString(),
                        ApprovalSummaryTable = _configuration["ApprovalSummaryTable"]
                    };
                }
            }

            return appSettings;
        }
    }
}