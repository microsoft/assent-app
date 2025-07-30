// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System;
using System.Net.Http;
using Azure.Identity;
using Azure.Storage.Blobs;
using Microsoft.Azure.Cosmos;
using Microsoft.Azure.Functions.Extensions.DependencyInjection;
using Microsoft.CFS.Approvals.Common.BL;
using Microsoft.CFS.Approvals.Common.BL.Helper;
using Microsoft.CFS.Approvals.Common.BL.Interface;
using Microsoft.CFS.Approvals.Common.DL;
using Microsoft.CFS.Approvals.Common.DL.Helpers;
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
using Microsoft.CFS.Approvals.Utilities.Helpers;
using Microsoft.CFS.Approvals.Utilities.Interface;
using Microsoft.CFS.Approvals.WatchdogAzFunction;
using Microsoft.CFS.Approvals.WatchdogProcessor.BL.Helpers;
using Microsoft.CFS.Approvals.WatchdogProcessor.BL.Interface;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Configuration.AzureAppConfiguration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Hosting.Internal;
using Microsoft.Extensions.Logging;
using Polly;
using Polly.Extensions.Http;

[assembly: FunctionsStartup(typeof(WatchdogStartup))]

namespace Microsoft.CFS.Approvals.WatchdogAzFunction;

public class WatchdogStartup : FunctionsStartup
{
    public static IConfigurationRefresher refresher;

    public override void Configure(IFunctionsHostBuilder builder)
    {
        // Create the new ConfigurationBuilder
        var configurationBuilder = new ConfigurationBuilder();

        // Select credential based on environment: DefaultAzureCredential for DEBUG, ManagedIdentityCredential for production
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

        builder.Services.AddLogging(configure =>
        {
            configure.AddApplicationInsights(Environment.GetEnvironmentVariable(Constants.AppinsightsInstrumentationkey));
        });

        var client = new BlobServiceClient(
                        new Uri($"https://" + config?[Constants.StorageAccountName] + ".blob.core.windows.net/"),
                        azureCredential);
        var cosmosdbClient = new CosmosClient(config?[ConfigurationKey.CosmosDbEndPoint.ToString()],
            azureCredential, new CosmosClientOptions() { AllowBulkExecution = true });

        // Add services
        builder.Services.AddSingleton<ApplicationInsightsTarget>();
        builder.Services.AddScoped<IHostingEnvironment, HostingEnvironment>();
        builder.Services.AddScoped<IUserDelegationProvider, UserDelegationProvider>();
        builder.Services.AddSingleton<ITableHelper, TableHelper>(x => new TableHelper(config?[Constants.StorageAccountName], azureCredential));
        builder.Services.AddScoped<IEmailTemplateHelper, EmailTemplateHelper>();
        builder.Services.AddSingleton<ILogProvider, LogProvider>();
        builder.Services.AddScoped<INotificationProvider, GenericNotificationProvider>();
        builder.Services.AddSingleton<IPerformanceLogger, PerformanceLogger>();
        builder.Services.AddScoped<INameResolutionHelper, NameResolutionHelper>();
        builder.Services.AddSingleton<IBlobStorageHelper, BlobStorageHelper>(x => new BlobStorageHelper(client));
        builder.Services.AddScoped<IApprovalBlobDataProvider, ApprovalBlobDataProvider>();
        builder.Services.AddScoped<IApprovalDetailProvider, ApprovalDetailProvider>();
        builder.Services.AddScoped<IApprovalTenantInfoProvider, ApprovalTenantInfoProvider>();
        builder.Services.AddScoped<IApprovalSummaryProvider, ApprovalSummaryProvider>();
        builder.Services.AddScoped<IFlightingDataProvider, FlightingDataProvider>();
        builder.Services.AddSingleton<ICosmosDbHelper, CosmosDbHelper>(x => new CosmosDbHelper(cosmosdbClient));
        builder.Services.AddSingleton<IHistoryStorageFactory, HistoryStorageFactory>();
        builder.Services.AddScoped<IApprovalHistoryProvider, ApprovalHistoryProvider>();
        builder.Services.AddScoped<ITenantFactory, TenantFactory>();
        builder.Services.AddScoped<IDelegationHelper, DelegationHelper>();
        builder.Services.AddScoped<IActionAuditLogger, ActionAuditLogger>();
        builder.Services.AddScoped<IActionAuditLogHelper, ActionAuditLogHelper>();
        builder.Services.AddScoped<IEditableConfigurationHelper, EditableConfigurationHelper>();
        builder.Services.AddScoped<IApprovalSummaryHelper, ApprovalSummaryHelper>();
        builder.Services.AddScoped<ISummaryHelper, SummaryHelper>();
        builder.Services.AddScoped<IDetailsHelper, DetailsHelper>();
        builder.Services.AddScoped<IImageRetriever, UserImageRetrieval>();
        builder.Services.AddScoped<ILocalFileCache, LocalFileCache>();
        builder.Services.AddScoped<IReminderData, ReminderData>();
        builder.Services.AddScoped<IReminderProcessor, ReminderProcessor>();
        builder.Services.AddScoped<IApprovalTenantInfoHelper, ApprovalTenantInfoHelper>();
        builder.Services.AddScoped<IEmailHelper, EmailHelper>();
        builder.Services.AddScoped<IAuthenticationHelper, AuthenticationHelper>();
        builder.Services.AddSingleton<HttpClientHandler>();
        builder.Services.AddHttpClient<IHttpHelper, HttpHelper>()
            .SetHandlerLifetime(TimeSpan.FromMinutes(5)) // Set lifetime to five minutes
            .AddPolicyHandler(GetRetryPolicy());
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