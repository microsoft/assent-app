// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using Azure.Identity;
using Azure.Messaging.ServiceBus;
using Azure.Storage.Blobs;
using Microsoft.Azure.Cosmos;
using Microsoft.Azure.Functions.Worker;
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
using Microsoft.CFS.Approvals.Messaging.Azure.ServiceBus.Helpers;
using Microsoft.CFS.Approvals.Messaging.Azure.ServiceBus.Interface;
using Microsoft.CFS.Approvals.PrimaryProcessor.BL;
using Microsoft.CFS.Approvals.PrimaryProcessor.BL.Interface;
using Microsoft.CFS.Approvals.Functions.Middleware;
using Microsoft.CFS.Approvals.Utilities.Helpers;
using Microsoft.CFS.Approvals.Utilities.Interface;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Configuration.AzureAppConfiguration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Polly;
using Polly.Extensions.Http;

#if DEBUG
var azureCredential = new DefaultAzureCredential(); // CodeQL [SM05137] Suppress CodeQL issue since we only use DefaultAzureCredential in development environments.
#else
var azureCredential = new ManagedIdentityCredential();
#endif

var host = new HostBuilder()
    .ConfigureFunctionsWebApplication(worker =>
    {
        // Register middleware. Order of registration matters.
        worker.UseMiddleware<TelemetryMiddleware>();
    })
    .ConfigureAppConfiguration((context, config) =>
    {
        config.AddAzureAppConfiguration(options =>
        {
            var appConfigUrl = Environment.GetEnvironmentVariable(Constants.AzureAppConfigurationUrl);
            if (!Uri.TryCreate(appConfigUrl, UriKind.Absolute, out var appConfigUri))
            {
                throw new InvalidOperationException($"The environment variable '{Constants.AzureAppConfigurationUrl}' is missing or not a valid absolute URI.");
            }

            var appConfigurationLabel = Environment.GetEnvironmentVariable(Constants.AppConfigurationLabel) ?? throw new InvalidOperationException("Missing AppConfigurationLabel environment variable.");
            options.Connect(appConfigUri, azureCredential)
                .Select(KeyFilter.Any, appConfigurationLabel)
                .ConfigureKeyVault(kv =>
                {
                    kv.SetCredential(azureCredential);
                })
                .ConfigureRefresh(refreshOptions =>
                {
                    refreshOptions.Register(Constants.MustUpdateConfig, true)
                        .SetCacheExpiration(TimeSpan.FromMinutes(5));
                });
        });

        config.AddEnvironmentVariables();
    })
    .ConfigureServices((context, services) =>
    {
        var config = context.Configuration;

        // Initialize Azure clients
        var storageAccountName = config[Constants.StorageAccountName] ?? throw new InvalidOperationException($"Missing configuration for '{Constants.StorageAccountName}'");
        services.AddSingleton(sp => new BlobServiceClient(
            new Uri($"https://{storageAccountName}.blob.core.windows.net/"),
            azureCredential));

        var cosmosDbEndPoint = config[ConfigurationKey.CosmosDbEndPoint.ToString()] ?? throw new InvalidOperationException($"Missing configuration for '{ConfigurationKey.CosmosDbEndPoint}'");
        services.AddSingleton(sp => new CosmosClient(
            cosmosDbEndPoint,
            azureCredential,
            new CosmosClientOptions() { AllowBulkExecution = true }));

        var serviceBusNamespace = config[Constants.ServiceBusNamespace] ?? throw new InvalidOperationException($"Missing configuration for '{Constants.ServiceBusNamespace}'");
        services.AddSingleton(sp => new ServiceBusClient(
            serviceBusNamespace,
            azureCredential));

        services.AddSingleton(sp =>
        {
            var serviceBusClient = sp.GetRequiredService<ServiceBusClient>();
            var topicNameRetry = config[ConfigurationKey.TopicNameRetry.ToString()] ?? throw new InvalidOperationException($"Missing configuration for '{ConfigurationKey.TopicNameRetry}'");
            var topicNameNotification = config[ConfigurationKey.TopicNameNotification.ToString()] ?? throw new InvalidOperationException($"Missing configuration for '{ConfigurationKey.TopicNameNotification}'");
            var queueNameSecondary = config[ConfigurationKey.QueueNameSecondary.ToString()] ?? throw new InvalidOperationException($"Missing configuration for '{ConfigurationKey.QueueNameSecondary}'");
            var queueNameReassignment = config[ConfigurationKey.QueueNameReassignment.ToString()] ?? throw new InvalidOperationException($"Missing configuration for '{ConfigurationKey.QueueNameReassignment}'");
            var senders = new Dictionary<string, ServiceBusSender>
            {
                { topicNameRetry, serviceBusClient.CreateSender(topicNameRetry) },
                { topicNameNotification, serviceBusClient.CreateSender(topicNameNotification) },
                { queueNameSecondary, serviceBusClient.CreateSender(queueNameSecondary) },
                { queueNameReassignment, serviceBusClient.CreateSender(queueNameReassignment) }
            };
            return new ServiceBusSenderDictionary(senders);
        });

        services.AddSingleton(sp =>
        {
            var wrapper = sp.GetRequiredService<ServiceBusSenderDictionary>();
            return wrapper.Keys.ToDictionary(key => key, key => wrapper[key]);
        });

        // Add singleton services
        services.AddSingleton<IPerformanceLogger, PerformanceLogger>();
        services.AddSingleton<ApplicationInsightsTarget, ApplicationInsightsTarget>();
        services.AddSingleton<ILogProvider, LogProvider>();
        services.AddSingleton<ICosmosDbHelper, CosmosDbHelper>(sp => new CosmosDbHelper(sp.GetRequiredService<CosmosClient>()));
        services.AddSingleton<IHistoryStorageFactory, HistoryStorageFactory>();
        services.AddSingleton<IBlobStorageHelper, BlobStorageHelper>(sp => new BlobStorageHelper(sp.GetRequiredService<BlobServiceClient>()));
        services.AddSingleton<ITableHelper, TableHelper>(sp => new TableHelper(config[Constants.StorageAccountName], azureCredential));
        services.AddSingleton<IServiceBusHelper, ServiceBusHelper>(sp => new ServiceBusHelper(sp.GetRequiredService<Dictionary<string, ServiceBusSender>>()));

        services
            .AddHttpClient<IHttpHelper, HttpHelper>()
            .ConfigurePrimaryHttpMessageHandler(() => new HttpClientHandler())
            .SetHandlerLifetime(TimeSpan.FromMinutes(5)) // Set lifetime to five minutes
            .AddPolicyHandler(GetRetryPolicy());

        // Add scoped services
        services.AddScoped<INameResolutionHelper, NameResolutionHelper>();
        services.AddScoped<IApprovalBlobDataProvider, ApprovalBlobDataProvider>();
        services.AddScoped<IApprovalTenantInfoProvider, ApprovalTenantInfoProvider>();
        services.AddScoped<IApprovalTenantInfoHelper, ApprovalTenantInfoHelper>();
        services.AddScoped<IValidationFactory, ValidationFactory>();
        services.AddScoped<IValidationHelper, ValidationHelper>();
        services.AddScoped<IPayloadValidator, PayloadValidator>();
        services.AddScoped<IApprovalSummaryProvider, ApprovalSummaryProvider>();
        services.AddScoped<IApprovalDetailProvider, ApprovalDetailProvider>();
        services.AddScoped<IFlightingDataProvider, FlightingDataProvider>();
        services.AddScoped<IApprovalHistoryProvider, ApprovalHistoryProvider>();
        services.AddScoped<IARConverterFactory, ARConverterFactory>();
        services.AddScoped<ITenantFactory, TenantFactory>();
        services.AddScoped<IApprovalPresenter, ApprovalPresenter>();
        services.AddScoped<IApprovalsTopicReceiver, GenericReceiver>();
        services.AddScoped<IAuthenticationHelper, AuthenticationHelper>();

        // Add Application Insights for isolated worker
        services.AddApplicationInsightsTelemetryWorkerService();
        services.ConfigureFunctionsApplicationInsights();
    })
    .Build();

host.Run();

static IAsyncPolicy<HttpResponseMessage> GetRetryPolicy()
{
    return HttpPolicyExtensions
        .HandleTransientHttpError()
        .OrResult(msg => msg.StatusCode == System.Net.HttpStatusCode.TooManyRequests)
        .WaitAndRetryAsync(6, retryAttempt => TimeSpan.FromSeconds(Math.Pow(2, retryAttempt)));
}