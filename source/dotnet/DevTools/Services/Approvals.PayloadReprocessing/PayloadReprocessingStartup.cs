// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System;
using Azure.Identity;
using Azure.Storage.Blobs;
using Microsoft.Azure.Cosmos;
using Microsoft.Azure.Functions.Extensions.DependencyInjection;
using Microsoft.CFS.Approvals.Common.BL;
using Microsoft.CFS.Approvals.Common.DL;
using Microsoft.CFS.Approvals.Common.DL.Interface;
using Microsoft.CFS.Approvals.Contracts;
using Microsoft.CFS.Approvals.Data.Azure.CosmosDb.Helpers;
using Microsoft.CFS.Approvals.Data.Azure.CosmosDb.Interface;
using Microsoft.CFS.Approvals.Data.Azure.Storage.Helpers;
using Microsoft.CFS.Approvals.Data.Azure.Storage.Interface;
using Microsoft.CFS.Approvals.LogManager;
using Microsoft.CFS.Approvals.LogManager.Provider.Interface;
using Microsoft.CFS.Approvals.PayloadReprocessing;
using Microsoft.CFS.Approvals.PayloadReprocessing.Utils;
using Microsoft.CFS.Approvals.SupportServices.Helper.Interface;
using Microsoft.CFS.Approvals.SupportServices.Helper.ServiceHelper;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Configuration.AzureAppConfiguration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json.Serialization;

[assembly: FunctionsStartup(typeof(PayloadReprocessingStartup))]

namespace Microsoft.CFS.Approvals.PayloadReprocessing
{
    public class PayloadReprocessingStartup : FunctionsStartup
    {
        public static IConfigurationRefresher refresher;

        public override void Configure(IFunctionsHostBuilder builder)
        {
            var configurationBuilder = new ConfigurationBuilder();
            var config = configurationBuilder.AddEnvironmentVariables().Build();
#if DEBUG
            var azureCredential = new DefaultAzureCredential(); // CodeQL [SM05137] Suppress CodeQL issue since we only use DefaultAzureCredential in development environments.
#else
            var azureCredential = new ManagedIdentityCredential();
#endif
            configurationBuilder.AddAzureAppConfiguration(options =>
            {
                options.Connect(new Uri(Environment.GetEnvironmentVariable(Constants.AzureAppConfigurationUrl)), azureCredential)
                    // Load configuration values with no label
                    .Select(KeyFilter.Any, Environment.GetEnvironmentVariable(Constants.AppConfigurationLabel))
                    .ConfigureKeyVault(kv =>
                    {
                        kv.SetCredential(azureCredential);
                    })
                    .ConfigureRefresh(refreshOptions =>
                    {
                        refreshOptions.Register(Constants.MustUpdateConfig, true)
                        .SetCacheExpiration(TimeSpan.FromMinutes(5));
                        refresher = options.GetRefresher();
                    });
            });

            config = configurationBuilder.Build();

            var client = new BlobServiceClient(
                            new Uri($"https://" + config?[Constants.StorageAccountName] + ".blob.core.windows.net/"),
                            azureCredential);
            var cosmosdbClient = new CosmosClient(config?[ConfigurationKey.CosmosDbEndPoint.ToString()],
                azureCredential, new CosmosClientOptions() { AllowBulkExecution = true });
            builder.Services.Replace(ServiceDescriptor.Singleton(typeof(IConfiguration), config));


            builder.Services.AddLogging(configure =>
            {
                configure.AddApplicationInsights(Environment.GetEnvironmentVariable(Constants.AppinsightsInstrumentationkey));
            });

            //Add Services
            builder.Services.AddMvcCore().AddNewtonsoftJson(o => { o.SerializerSettings.ContractResolver = new DefaultContractResolver(); });
            builder.Services.AddSingleton<ILogProvider, LogProvider>();
            builder.Services.AddSingleton<ApplicationInsightsTarget>();
            builder.Services.AddScoped<IApprovalTenantInfoProvider, ApprovalTenantInfoProvider>();
            builder.Services.AddSingleton<ITableHelper, TableHelper>((provider) => { return new TableHelper(config?[Constants.StorageAccountName], new DefaultAzureCredential()); });
            builder.Services.AddScoped<IDocumentStatusAuditHelper, DocumentStatusAuditHelper>();
            builder.Services.AddSingleton<ICosmosDbHelper, CosmosDbHelper>(x => new CosmosDbHelper(cosmosdbClient));
            builder.Services.AddSingleton<IBlobStorageHelper, BlobStorageHelper>(x => new BlobStorageHelper(client));
            builder.Services.AddScoped<IDocumentRetrieverAudit, DocumentRetrieverAudit>();
            builder.Services.AddScoped<IRePushMessagesHelper, RePushMessagesHelper>();
            builder.Services.AddSingleton<IPerformanceLogger, PerformanceLogger>();
            builder.Services.AddScoped<IAuthorizationMiddleware, AuthorizationMiddleware>();
        }
    }
}
