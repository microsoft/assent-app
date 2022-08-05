// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace Microsoft.CFS.Approvals.CoreServices
{
    using System;
    using System.IO;
    using System.Linq;
    using System.Net.Http;
    using global::Azure.Identity;
    using global::Azure.Storage.Blobs;
    using Microsoft.AspNetCore.Builder;
    using Microsoft.AspNetCore.Hosting;
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
    using Microsoft.CFS.Approvals.CoreServices.BL.Helpers;
    using Microsoft.CFS.Approvals.CoreServices.BL.Interface;
    using Microsoft.CFS.Approvals.CoreServices.Utils;
    using Microsoft.CFS.Approvals.Data.Azure.CosmosDb.Helpers;
    using Microsoft.CFS.Approvals.Data.Azure.CosmosDb.Interface;
    using Microsoft.CFS.Approvals.Data.Azure.Storage.Helpers;
    using Microsoft.CFS.Approvals.Data.Azure.Storage.Interface;
    using Microsoft.CFS.Approvals.LogManager;
    using Microsoft.CFS.Approvals.LogManager.Provider.Interface;
    using Microsoft.CFS.Approvals.Utilities.Helpers;
    using Microsoft.CFS.Approvals.Utilities.Interface;
    using Microsoft.Extensions.Configuration;
    using Microsoft.Extensions.DependencyInjection;
    using Microsoft.Extensions.Hosting;
    using Microsoft.Extensions.Logging;
    using Microsoft.OpenApi.Models;
    using Newtonsoft.Json.Serialization;
    using Polly;
    using Polly.Extensions.Http;

    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            IConfiguration config = services.BuildServiceProvider().GetService<IConfiguration>();

            // Add Logging
            services.AddLogging(configure =>
            {
                configure.AddApplicationInsights(config?[Constants.AppinsightsInstrumentationkey]);
            });

            services.AddControllers()
                .AddNewtonsoftJson(options => options.UseCamelCasing(true));
            services.AddRouting(options => options.LowercaseUrls = true);

            // Register the Swagger generator, defining one or more Swagger documents
            services.AddSwaggerGen(c =>
            {
                c.ResolveConflictingActions(apiDescriptions => apiDescriptions.First());
                c.SwaggerDoc("v1", new OpenApiInfo
                {
                    Title = "CoreServices",
                    Version = "v1"
                });

                var xmlPath = Path.ChangeExtension(typeof(Startup).Assembly.Location, ".xml");
                c.IncludeXmlComments(xmlPath);
                c.OperationFilter<AddRequiredHeaderParameter>();
                c.EnableAnnotations();
            });

            var client = new BlobServiceClient(
                            new Uri($"https://" + config?[Constants.StorageAccountName] + ".blob.core.windows.net/"),
                            new DefaultAzureCredential());
            services.AddSingleton<ApplicationInsightsTarget>();
            services.AddScoped<IClientActionHelper, ClientActionHelper>();
            services.AddScoped<IOfficeDocumentCreator, OfficeDocumentCreator>();
            services.AddSingleton<ILogProvider, LogProvider>();
            services.AddSingleton<IPerformanceLogger, PerformanceLogger>();
            services.AddScoped<IApprovalBlobDataProvider, ApprovalBlobDataProvider>();
            services.AddScoped<IApprovalTenantInfoProvider, ApprovalTenantInfoProvider>();
            services.AddScoped<INameResolutionHelper, NameResolutionHelper>();
            services.AddSingleton<ITableHelper, TableHelper>(x => new TableHelper(config?[Constants.StorageAccountName], config?[ConfigurationKey.StorageAccountKey.ToString()]));
            services.AddSingleton<IBlobStorageHelper, BlobStorageHelper>(x => new BlobStorageHelper(client));
            services.AddScoped<IFlightingDataProvider, FlightingDataProvider>();
            services.AddSingleton((Func<IServiceProvider, ICosmosDbHelper>)(c => new CosmosDbHelper(config?[ConfigurationKey.CosmosDbEndPoint.ToString()], config?[ConfigurationKey.CosmosDbAuthKey.ToString()])));
            services.AddSingleton<IHistoryStorageFactory, HistoryStorageFactory>();

            var tableHelper = services.BuildServiceProvider().GetService<ITableHelper>();
            var logger = services.BuildServiceProvider().GetService<ILogProvider>();
            var performanceLogger = services.BuildServiceProvider().GetService<IPerformanceLogger>();
            var approvalTenantInfoProvider = services.BuildServiceProvider().GetService<IApprovalTenantInfoProvider>();
            var historyStorageFactory = services.BuildServiceProvider().GetService<IHistoryStorageFactory>();
            var cosmosDbHelper = services.BuildServiceProvider().GetService<ICosmosDbHelper>();

            if (bool.Parse(config?[ConfigurationKey.IsAzureSearchEnabled.ToString()]))
                services.AddScoped<IApprovalHistoryProvider, ApprovalHistoryAzureSearchProvider>(h => new ApprovalHistoryAzureSearchProvider(config, tableHelper, logger, performanceLogger, approvalTenantInfoProvider, historyStorageFactory, cosmosDbHelper));
            else
                services.AddScoped<IApprovalHistoryProvider, ApprovalHistoryProvider>(h => new ApprovalHistoryProvider(config, approvalTenantInfoProvider, logger, performanceLogger, cosmosDbHelper, historyStorageFactory));
            services.AddScoped<ITenantFactory, TenantFactory>();
            services.AddScoped<IApprovalTenantInfoHelper, ApprovalTenantInfoHelper>();
            services.AddScoped<IAboutHelper, AboutHelper>();
            services.AddScoped<IApprovalDetailProvider, ApprovalDetailProvider>();
            services.AddScoped<IApprovalSummaryProvider, ApprovalSummaryProvider>();
            services.AddScoped<IDocumentApprovalStatusHelper, DocumentApprovalStatusHelper>();
            services.AddScoped<IAdaptiveDetailsHelper, AdaptiveDetailsHelper>();
            services.AddScoped<ITenantDownTimeMessagesProvider, TenantDownTimeMessagesProvider>();
            services.AddScoped<ITenantDownTimeMessagesHelper, TenantDownTimeMessagesHelper>();
            services.AddSingleton<ILocalFileCache, LocalFileCache>();
            services.AddScoped<IImageRetriever, UserImageRetrieval>();
            services.AddScoped<IApprovalSummaryHelper, ApprovalSummaryHelper>();
            services.AddScoped<ISummaryHelper, SummaryHelper>();
            services.AddScoped<IEditableConfigurationHelper, EditableConfigurationHelper>();
            services.AddScoped<IActionAuditLogger, ActionAuditLogger>();
            services.AddScoped<IActionAuditLogHelper, ActionAuditLogHelper>();
            services.AddScoped<IUserDelegationProvider, UserDelegationProvider>();
            services.AddScoped<IDelegationHelper, DelegationHelper>();
            services.AddScoped<IDetailsHelper, DetailsHelper>();
            services.AddScoped<IApprovalHistoryHelper, ApprovalHistoryHelper>();
            services.AddScoped<IDocumentActionHelper, DocumentActionHelper>();
            services.AddScoped<IClientActionHelper, ClientActionHelper>();
            services.AddScoped<IFlightingDataProvider, FlightingDataProvider>();
            services.AddScoped<IPullTenantHelper, PullTenantHelper>();
            services.AddScoped<IReadDetailsHelper, ReadDetailsHelper>();
            services.AddScoped<ISaveEditableDetailsHelper, SaveEditableDetailsHelper>();
            services.AddScoped<IUserPreferenceHelper, UserPreferenceHelper>();
            services.AddScoped<IAuthenticationHelper, AuthenticationHelper>();
            services.AddScoped<DocumentActionHelper>();
            services.AddScoped<BulkDocumentActionHelper>();
            services.AddScoped<BulkExternalDocumentActionHelper>();

            services.AddScoped<Func<string, IDocumentActionHelper>>(serviceProvider => key =>
            {
                return key switch
                {
                    "Single" => serviceProvider.GetService<DocumentActionHelper>(),
                    "PseudoBulk" => serviceProvider.GetService<DocumentActionHelper>(),
                    "Bulk" => serviceProvider.GetService<BulkDocumentActionHelper>(),
                    "BulkExternal" => serviceProvider.GetService<BulkExternalDocumentActionHelper>(),
                    _ => serviceProvider.GetService<DocumentActionHelper>(),
                };
            });
            services.AddScoped<AuthorizationMiddleware, AuthorizationMiddleware>();
            services.AddSingleton<HttpClientHandler>();
            services.AddHttpClient<IHttpHelper, HttpHelper>()
                .SetHandlerLifetime(TimeSpan.FromMinutes(5)) // Set lifetime to five minutes
                .AddPolicyHandler(GetRetryPolicy());
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            else
            {
                // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
                app.UseHsts();
            }

            app.UseSwagger();
            app.UseSwaggerUI(c =>
            {
                c.RoutePrefix = "";
                c.SwaggerEndpoint("../swagger/v1/swagger.json", "CoreServices v1");
                c.DefaultModelsExpandDepth(-1);
            });

            app.UseHttpsRedirection();

            app.UseRouting();

            app.UseMiddleware<AuthorizationMiddleware>();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
            });
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