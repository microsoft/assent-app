// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System;
using System.Net.Http;
using Azure.Identity;
using Azure.Storage.Blobs;
using Microsoft.Azure.Cosmos;
using Microsoft.Azure.Functions.Extensions.DependencyInjection;
using Microsoft.CFS.Approvals.Common.BL;
using Microsoft.CFS.Approvals.Common.BL.Interface;
using Microsoft.CFS.Approvals.Common.DL;
using Microsoft.CFS.Approvals.Common.DL.Interface;
using Microsoft.CFS.Approvals.Contracts;
using Microsoft.CFS.Approvals.Core.BL.Factory;
using Microsoft.CFS.Approvals.Core.BL.Helpers;
using Microsoft.CFS.Approvals.Core.BL.Interface;
using Microsoft.CFS.Approvals.Data.Azure.CosmosDb.Helpers;
using Microsoft.CFS.Approvals.Data.Azure.CosmosDb.Interface;
using Microsoft.CFS.Approvals.Data.Azure.Storage.Helpers;
using Microsoft.CFS.Approvals.Data.Azure.Storage.Interface;
using Microsoft.CFS.Approvals.LogManager;
using Microsoft.CFS.Approvals.LogManager.Provider.Interface;
using Microsoft.CFS.Approvals.PrimaryProcessor.BL;
using Microsoft.CFS.Approvals.PrimaryProcessor.BL.Interface;
using Microsoft.CFS.Approvals.Utilities.Helpers;
using Microsoft.CFS.Approvals.Utilities.Interface;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Configuration.AzureAppConfiguration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Logging;
using Polly;
using Polly.Extensions.Http;
using PrimaryAzFunction;

[assembly: FunctionsStartup(typeof(PrimaryStartup))]

namespace PrimaryAzFunction;

public class PrimaryStartup : FunctionsStartup
{
    public static IConfigurationRefresher refresher;

    public override void Configure(IFunctionsHostBuilder builder)
    {
        // Create the new ConfigurationBuilder
        var configurationBuilder = new ConfigurationBuilder();

        configurationBuilder.AddAzureAppConfiguration(options =>
        {
            options.Connect(new Uri(Environment.GetEnvironmentVariable(Constants.AzureAppConfigurationUrl)), new DefaultAzureCredential())
                // Load configuration values with no label
                .Select(KeyFilter.Any, Environment.GetEnvironmentVariable(Constants.AppConfigurationLabel))
                .ConfigureKeyVault(kv =>
                {
                    kv.SetCredential(new DefaultAzureCredential());
                })
                .ConfigureRefresh(refreshOptions =>
                {
                    refreshOptions.Register(Constants.MustUpdateConfig, true)
                    .SetCacheExpiration(TimeSpan.FromMinutes(5));
                    refresher = options.GetRefresher();
                });
        });

        // Build the config in order to access the appsettings for getting the Azure App Configuration connection settings
        var config = configurationBuilder.Build();
        config = configurationBuilder.AddEnvironmentVariables().Build();

        // Replace the existing config with the new one
        builder.Services.Replace(ServiceDescriptor.Singleton(typeof(IConfiguration), config));

        builder.Services.AddLogging(configure =>
        {
            configure.AddApplicationInsights(Environment.GetEnvironmentVariable(Constants.AppinsightsInstrumentationkey));
        });

        var client = new BlobServiceClient(
                        new Uri($"https://" + config?[Constants.StorageAccountName] + ".blob.core.windows.net/"),
                        new DefaultAzureCredential());
        var cosmosdbClient = new CosmosClient(config?[ConfigurationKey.CosmosDbEndPoint.ToString()],
            new DefaultAzureCredential(), new CosmosClientOptions() { AllowBulkExecution = true });

        //Add Services
        builder.Services.AddSingleton<IPerformanceLogger, PerformanceLogger>();
        builder.Services.AddSingleton<ApplicationInsightsTarget, ApplicationInsightsTarget>();
        builder.Services.AddSingleton<ILogProvider, LogProvider>();
        builder.Services.AddSingleton<ICosmosDbHelper, CosmosDbHelper>(x => new CosmosDbHelper(cosmosdbClient));
        builder.Services.AddSingleton<IHistoryStorageFactory, HistoryStorageFactory>();
        builder.Services.AddSingleton<IBlobStorageHelper, BlobStorageHelper>(x => new BlobStorageHelper(client));
        builder.Services.AddSingleton<ITableHelper, TableHelper>((provider) => { return new TableHelper(config[Constants.StorageAccountName], new DefaultAzureCredential()); });
        builder.Services.AddSingleton<HttpClientHandler>();
        builder.Services.AddHttpClient<IHttpHelper, HttpHelper>()
            .SetHandlerLifetime(TimeSpan.FromMinutes(5)) // Set lifetime to five minutes
            .AddPolicyHandler(GetRetryPolicy());
        builder.Services.AddScoped<INameResolutionHelper, NameResolutionHelper>();
        builder.Services.AddScoped<IApprovalBlobDataProvider, ApprovalBlobDataProvider>();
        builder.Services.AddScoped<IApprovalTenantInfoProvider, ApprovalTenantInfoProvider>();
        builder.Services.AddScoped<IApprovalTenantInfoHelper, ApprovalTenantInfoHelper>();
        builder.Services.AddScoped<IValidationFactory, ValidationFactory>();
        builder.Services.AddScoped<IValidationHelper, ValidationHelper>();
        builder.Services.AddScoped<IPayloadValidator, PayloadValidator>();
        builder.Services.AddScoped<IApprovalSummaryProvider, ApprovalSummaryProvider>();
        builder.Services.AddScoped<IApprovalDetailProvider, ApprovalDetailProvider>();
        builder.Services.AddScoped<IFlightingDataProvider, FlightingDataProvider>();
        builder.Services.AddScoped<IApprovalHistoryProvider, ApprovalHistoryProvider>();
        builder.Services.AddScoped<IARConverterFactory, ARConverterFactory>();
        builder.Services.AddScoped<ITenantFactory, TenantFactory>();
        builder.Services.AddScoped<IApprovalPresenter, ApprovalPresenter>();
        builder.Services.AddScoped<IApprovalsTopicReceiver, GenericReceiver>();
        builder.Services.AddScoped<IAuthenticationHelper, AuthenticationHelper>();
    }

    private static IAsyncPolicy<HttpResponseMessage> GetRetryPolicy()
    {
        return HttpPolicyExtensions
            .HandleTransientHttpError()
            .OrResult(msg => msg.StatusCode == System.Net.HttpStatusCode.TooManyRequests)
            .WaitAndRetryAsync(6, retryAttempt => TimeSpan.FromSeconds(Math.Pow(2,
                retryAttempt)));
    }
}