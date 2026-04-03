// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System;
using System.Net.Http;
using Approvals.Webhook.BL.Helpers;
using Approvals.Webhook.BL.Interface;
using Azure.Identity;
using Azure.Storage.Blobs;
using global::Azure.Messaging.ServiceBus;
using Microsoft.CFS.Approvals.Common.BL;
using Microsoft.CFS.Approvals.Common.DL;
using Microsoft.CFS.Approvals.Common.DL.Interface;
using Microsoft.CFS.Approvals.Contracts;
using Microsoft.CFS.Approvals.Data.Azure.Storage.Helpers;
using Microsoft.CFS.Approvals.Data.Azure.Storage.Interface;
using Microsoft.CFS.Approvals.LogManager;
using Microsoft.CFS.Approvals.LogManager.Provider.Interface;
using Microsoft.CFS.Approvals.Messaging.Azure.ServiceBus.Helpers;
using Microsoft.CFS.Approvals.Messaging.Azure.ServiceBus.Interface;
using Microsoft.CFS.Approvals.Model;
using Microsoft.CFS.Approvals.PayloadReceiver.BL;
using Microsoft.CFS.Approvals.PayloadReceiver.BL.Interface;
using Microsoft.CFS.Approvals.Utilities.Helpers;
using Microsoft.CFS.Approvals.Utilities.Interface;
using Microsoft.Azure.Functions.Extensions.DependencyInjection;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Configuration.AzureAppConfiguration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Logging;
using Polly;
using Polly.Extensions.Http;
using WebhookAzFunction;

[assembly: FunctionsStartup(typeof(WebhookStartup))]

namespace WebhookAzFunction;

public class WebhookStartup : FunctionsStartup
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

        // Build the config in order to access the app settings for getting the Azure App Configuration connection settings
        var config = configurationBuilder.Build();
        config = configurationBuilder.AddEnvironmentVariables().Build();
        
        // Replace the existing config with the new one
        builder.Services.Replace(ServiceDescriptor.Singleton(typeof(IConfiguration), config));

        var client = new BlobServiceClient(
                        new Uri($"https://" + config?[Constants.StorageAccountName] + ".blob.core.windows.net/"),
                        azureCredential);
        var serviceBusClient = new ServiceBusClient(
                            config?[Constants.ServiceBusNamespace],
                            azureCredential);
        //Add Services
        builder.Services.AddSingleton<IPerformanceLogger, PerformanceLogger>();
        builder.Services.AddSingleton<ApplicationInsightsTarget, ApplicationInsightsTarget>();
        builder.Services.AddSingleton<ILogProvider, LogProvider>();
        builder.Services.AddScoped<IWebhookHelper, AdobeWebhookHelper>();
        builder.Services.AddSingleton<IBlobStorageHelper, BlobStorageHelper>(x => new BlobStorageHelper(client));
        builder.Services
            .AddHttpClient<IHttpHelper, HttpHelper>()
            .ConfigurePrimaryHttpMessageHandler(() => new HttpClientHandler())
            .SetHandlerLifetime(TimeSpan.FromMinutes(5)) // Set lifetime to five minutes
            .AddPolicyHandler(GetRetryPolicy());

        builder.Services.AddScoped<IApprovalBlobDataProvider, ApprovalBlobDataProvider>();
        builder.Services.AddScoped<IPayloadDelivery<AdobeSignEvent>, PayloadDelivery<AdobeSignEvent>>();
        builder.Services.AddSingleton<IServiceBusHelper, ServiceBusHelper>(x => new ServiceBusHelper(serviceBusClient.CreateSender(config[ConfigurationKey.TopicNameExternalMain.ToString()])));
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
