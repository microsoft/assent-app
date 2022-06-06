// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System;
using System.Net.Http;
using Azure.Identity;
using Azure.Storage.Blobs;
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
using Microsoft.CFS.Approvals.NotificationProcessor.BL;
using Microsoft.CFS.Approvals.NotificationProcessor.BL.Interface;
using Microsoft.CFS.Approvals.Utilities.Helpers;
using Microsoft.CFS.Approvals.Utilities.Interface;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Configuration.AzureAppConfiguration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Hosting.Internal;
using Microsoft.Extensions.Logging;
using NotificationAzFunction;
using Polly;
using Polly.Extensions.Http;

[assembly: FunctionsStartup(typeof(NotificationStartup))]

namespace NotificationAzFunction
{
    public class NotificationStartup : FunctionsStartup
    {
        public static IConfigurationRefresher refresher;

        public override void Configure(IFunctionsHostBuilder builder)
        {
            // Create the new ConfigurationBuilder
            var configurationBuilder = new ConfigurationBuilder();

            configurationBuilder.AddAzureAppConfiguration(options =>
            {
                options.Connect(Environment.GetEnvironmentVariable(Constants.AzureAppConfiguration))
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
            IConfiguration configuration = builder.Services.BuildServiceProvider().GetService<IConfiguration>();

            builder.Services.AddLogging(configure =>
            {
                configure.AddApplicationInsights(Environment.GetEnvironmentVariable(Constants.AppinsightsInstrumentationkey));
            });

            var client = new BlobServiceClient(
                            new Uri($"https://" + config?[Constants.StorageAccountName] + ".blob.core.windows.net/"),
                            new DefaultAzureCredential());

            //Add Services
            builder.Services.AddSingleton<IPerformanceLogger, PerformanceLogger>();
            builder.Services.AddScoped<IApprovalsTopicReceiver, NotificationReceiver>();
            builder.Services.AddSingleton<ILogProvider, LogProvider>();
            builder.Services.AddSingleton<ApplicationInsightsTarget>();
            builder.Services.AddScoped<IEmailHelper, EmailHelper>();
            builder.Services.AddScoped<ITenantFactory, TenantFactory>();
            builder.Services.AddScoped<INotificationProvider, NotificationFrameworkProvider>();
            builder.Services.AddScoped<INotificationProcessor, NotificationProcessor>();
            builder.Services.AddScoped<IApprovalSummaryProvider, ApprovalSummaryProvider>();
            builder.Services.AddScoped<IEmailHelper, EmailHelper>();
            builder.Services.AddSingleton<ITableHelper, TableHelper>((provider) => { return new TableHelper(config?[Constants.StorageAccountName], config?[ConfigurationKey.StorageAccountKey.ToString()]); });
            builder.Services.AddScoped<IApprovalDetailProvider, ApprovalDetailProvider>();
            builder.Services.AddScoped<IApprovalTenantInfoProvider, ApprovalTenantInfoProvider>();
            builder.Services.AddScoped<IApprovalBlobDataProvider, ApprovalBlobDataProvider>();
            builder.Services.AddScoped<INameResolutionHelper, NameResolutionHelper>();
            builder.Services.AddScoped<IEmailTemplateHelper, EmailTemplateHelper>();
            builder.Services.AddScoped<IApprovalHistoryProvider, ApprovalHistoryProvider>();
            builder.Services.AddSingleton((Func<IServiceProvider, ICosmosDbHelper>)(c => new CosmosDbHelper(config?[ConfigurationKey.CosmosDbEndPoint.ToString()], config?[ConfigurationKey.CosmosDbAuthKey.ToString()])));
            builder.Services.AddSingleton<IHistoryStorageFactory, HistoryStorageFactory>();
            builder.Services.AddScoped<IDetailsHelper, DetailsHelper>();
            builder.Services.AddSingleton<IBlobStorageHelper, BlobStorageHelper>(x => new BlobStorageHelper(client));
            builder.Services.AddScoped<IImageRetriever, UserImageRetrieval>();
            builder.Services.AddScoped<IDelegationHelper, DelegationHelper>();
            builder.Services.AddScoped<IActionAuditLogHelper, ActionAuditLogHelper>();
            builder.Services.AddScoped<IApprovalTenantInfoHelper, ApprovalTenantInfoHelper>();
            builder.Services.AddScoped<IFlightingDataProvider, FlightingDataProvider>();
            builder.Services.AddScoped<IEditableConfigurationHelper, EditableConfigurationHelper>();
            builder.Services.AddScoped<ISummaryHelper, SummaryHelper>();
            builder.Services.AddScoped<IUserDelegationProvider, UserDelegationProvider>();
            builder.Services.AddScoped<IActionAuditLogger, ActionAuditLogger>();
            builder.Services.AddScoped<IApprovalSummaryHelper, ApprovalSummaryHelper>();
            builder.Services.AddScoped<ILocalFileCache, LocalFileCache>();
            builder.Services.AddScoped<IHostingEnvironment, HostingEnvironment>();
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
}