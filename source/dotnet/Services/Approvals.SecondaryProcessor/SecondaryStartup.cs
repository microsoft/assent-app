// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System;
using System.Collections.Generic;
using System.Net.Http;
using Approvals.SecondaryProcessor.BL;
using Approvals.SecondaryProcessor.BL.Interface;
using Azure.AI.OpenAI;
using Azure.Identity;
using Azure.Messaging.ServiceBus;
using Azure.Storage.Blobs;
using Microsoft.Azure.Cosmos;
using Microsoft.Azure.Functions.Extensions.DependencyInjection;
using Microsoft.CFS.Approvals.Common.BL;
using Microsoft.CFS.Approvals.Common.BL.Helper;
using Microsoft.CFS.Approvals.Common.BL.Interface;
using Microsoft.CFS.Approvals.Common.DL;
using Microsoft.CFS.Approvals.Common.DL.Interface;
using Microsoft.CFS.Approvals.Contracts;
using Microsoft.CFS.Approvals.Core.BL.Factory;
using Microsoft.CFS.Approvals.Core.BL.Helpers;
using Microsoft.CFS.Approvals.Core.BL.Helpers.ChatToolHandlers;
using Microsoft.CFS.Approvals.Core.BL.Interface;
using Microsoft.CFS.Approvals.Data.Azure.CosmosDb.Helpers;
using Microsoft.CFS.Approvals.Data.Azure.CosmosDb.Interface;
using Microsoft.CFS.Approvals.Data.Azure.Storage.Helpers;
using Microsoft.CFS.Approvals.Data.Azure.Storage.Interface;
using Microsoft.CFS.Approvals.LogManager;
using Microsoft.CFS.Approvals.LogManager.Provider.Interface;
using Microsoft.CFS.Approvals.Messaging.Azure.ServiceBus.Helpers;
using Microsoft.CFS.Approvals.Messaging.Azure.ServiceBus.Interface;
using Microsoft.CFS.Approvals.Utilities.Helpers;
using Microsoft.CFS.Approvals.Utilities.Interface;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Configuration.AzureAppConfiguration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Logging;
using Polly;
using Polly.Extensions.Http;
using SecondaryAzFunction;

[assembly: FunctionsStartup(typeof(SecondaryStartup))]

namespace SecondaryAzFunction;

public class SecondaryStartup : FunctionsStartup
{
    public static IConfigurationRefresher refresher;

    public override void Configure(IFunctionsHostBuilder builder)
    {
        // Create the new ConfigurationBuilder
        var configurationBuilder = new ConfigurationBuilder();

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

        // Build the config in order to access the appsettings for getting the Azure App Configuration connection settings
        var config = configurationBuilder.Build();
        config = configurationBuilder.AddEnvironmentVariables().Build();

        // Replace the existing config with the new one
        builder.Services.Replace(ServiceDescriptor.Singleton(typeof(IConfiguration), config));

        var client = new BlobServiceClient(
                        new Uri($"https://" + config?[Constants.StorageAccountName] + ".blob.core.windows.net/"),
                        azureCredential);
        var cosmosdbClient = new CosmosClient(config?[ConfigurationKey.CosmosDbEndPoint.ToString()],
            azureCredential, new CosmosClientOptions() { AllowBulkExecution = true });
        var serviceBusClient = new ServiceBusClient(
                            config?[Constants.ServiceBusNamespace],
                            azureCredential);
        var serviceBusSenders = new Dictionary<string, ServiceBusSender>
        {
            { config[ConfigurationKey.TopicNameSecondary.ToString()], serviceBusClient.CreateSender(config[ConfigurationKey.TopicNameSecondary.ToString()]) },
            { config[ConfigurationKey.TopicNameAuxiliary.ToString()], serviceBusClient.CreateSender(config[ConfigurationKey.TopicNameAuxiliary.ToString()]) },
            { config[ConfigurationKey.QueueNameReassignment.ToString()], serviceBusClient.CreateSender(config[ConfigurationKey.QueueNameReassignment.ToString()]) }
        };
        var openAIClient = new AzureOpenAIClient(new Uri(config?[ConfigurationKey.AzureOpenAiApiEndpoint.ToString()]), azureCredential);
        var flightingDataProvider = builder.Services.BuildServiceProvider().GetService<IFlightingDataProvider>();
        //Add Services
        builder.Services.AddSingleton<IPerformanceLogger, PerformanceLogger>();
        builder.Services.AddSingleton<ApplicationInsightsTarget, ApplicationInsightsTarget>();
        builder.Services.AddSingleton<ILogProvider, LogProvider>();
        builder.Services.AddSingleton<ICosmosDbHelper, CosmosDbHelper>(x => new CosmosDbHelper(cosmosdbClient));
        builder.Services.AddSingleton<IHistoryStorageFactory, HistoryStorageFactory>();
        builder.Services.AddSingleton<IBlobStorageHelper, BlobStorageHelper>(x => new BlobStorageHelper(client));
        builder.Services.AddSingleton<ITableHelper, TableHelper>((provider) => { return new TableHelper(config[Constants.StorageAccountName], azureCredential); });
        builder.Services
            .AddHttpClient<IHttpHelper, HttpHelper>()
            .ConfigurePrimaryHttpMessageHandler(() => new HttpClientHandler())
            .SetHandlerLifetime(TimeSpan.FromMinutes(5)) // Set lifetime to five minutes
            .AddPolicyHandler(GetRetryPolicy());
        builder.Services.AddScoped<INameResolutionHelper, NameResolutionHelper>();
        builder.Services.AddScoped<IApprovalBlobDataProvider, ApprovalBlobDataProvider>();
        builder.Services.AddScoped<IApprovalTenantInfoProvider, ApprovalTenantInfoProvider>();
        builder.Services.AddScoped<IApprovalTenantInfoHelper, ApprovalTenantInfoHelper>();
        builder.Services.AddScoped<IApprovalSummaryProvider, ApprovalSummaryProvider>();
        builder.Services.AddScoped<IApprovalDetailProvider, ApprovalDetailProvider>();
        builder.Services.AddScoped<IFlightingDataProvider, FlightingDataProvider>();
        builder.Services.AddScoped<IApprovalHistoryProvider, ApprovalHistoryProvider>();
        builder.Services.AddScoped<ITenantFactory, TenantFactory>();
        builder.Services.AddScoped<IApprovalsQueueReceiver, SecondaryReceiver>();
        builder.Services.AddScoped<IAuthenticationHelper, AuthenticationHelper>();
        builder.Services.AddScoped<IEditableConfigurationHelper, EditableConfigurationHelper>();
        builder.Services.AddScoped<IUserDelegationProvider, UserDelegationProvider>();
        builder.Services.AddScoped<ISummaryHelper, SummaryHelper>();
        builder.Services.AddScoped<IDelegationHelper, DelegationHelper>();
        builder.Services.AddScoped<ISummaryHelper, SummaryHelper>();
        builder.Services.AddScoped<IDetailsHelper, DetailsHelper>();
        builder.Services.AddScoped<IImageRetriever, UserImageRetrieval>();
        builder.Services.AddScoped<IAdaptiveCardResponseHelper, AdaptiveCardResponseHelper>();
        builder.Services.AddScoped<ISearchHelper, SearchHelper>();
        builder.Services.AddScoped<IChatToolHandler, ExplainAndAskPermissionToolHandler>();
        builder.Services.AddScoped<IChatToolHandler, OnErrorOccurredToolHandler>();
        builder.Services.AddScoped<IChatToolHandler, RequestDetailsToolHandler>();
        builder.Services.AddScoped<IAIAnalysisHelper, AIAnalysisHelper>();
        builder.Services.AddScoped<IAIAssistedSearchHelper, AIAssistedSearchHelper>();
        builder.Services.AddScoped<IIntelligenceHelper>(provider => new IntelligenceHelper(openAIClient, config, provider.GetRequiredService<ILogProvider>()));
        builder.Services.AddScoped<IApprovalsPluginHelper>(sp => new ApprovalsPluginHelper(
            config,
            sp.GetRequiredService<IIntelligenceHelper>(),
            sp.GetRequiredService<IDelegationHelper>(),
            sp.GetServices<IChatToolHandler>(),
            sp.GetRequiredService<ILogProvider>(),
            sp.GetRequiredService<IPerformanceLogger>()));

        // IntelligenceHelper registered above with minimal constructor

        builder.Services.AddScoped<ISecondaryProcessor, SecondaryProcessor>();
        builder.Services.AddSingleton<IServiceBusHelper, ServiceBusHelper>(x => new ServiceBusHelper(serviceBusSenders));
    }

    private static IAsyncPolicy<HttpResponseMessage> GetRetryPolicy()
    {
        return HttpPolicyExtensions
            .HandleTransientHttpError()
            .OrResult(msg => msg.StatusCode == System.Net.HttpStatusCode.TooManyRequests)
            .WaitAndRetryAsync(6, retryAttempt => TimeSpan.FromSeconds(Math.Pow(2, retryAttempt)));
    }
}