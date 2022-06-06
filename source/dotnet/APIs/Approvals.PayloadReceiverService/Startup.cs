// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace Microsoft.CFS.Approvals.PayloadReceiverService
{
    using System;
    using System.IO;
    using System.Net.Http;
    using global::Azure.Identity;
    using global::Azure.Storage.Blobs;
    using Microsoft.AspNetCore.Builder;
    using Microsoft.AspNetCore.Hosting;
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
    using Microsoft.CFS.Approvals.PayloadReceiver.BL;
    using Microsoft.CFS.Approvals.PayloadReceiver.BL.Interface;
    using Microsoft.CFS.Approvals.Utilities.Helpers;
    using Microsoft.CFS.Approvals.Utilities.Interface;
    using Microsoft.Extensions.Configuration;
    using Microsoft.Extensions.DependencyInjection;
    using Microsoft.Extensions.Hosting;
    using Microsoft.Extensions.Logging;
    using Microsoft.OpenApi.Models;
    using PayloadReceiverService.Utils;
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

            services.AddControllers().AddNewtonsoftJson(options =>
                options.SerializerSettings.ReferenceLoopHandling = Newtonsoft.Json.ReferenceLoopHandling.Ignore
            );
            services.AddRouting(options => options.LowercaseUrls = true);

            // Register the Swagger generator, defining one or more Swagger documents
            services.AddSwaggerGen(c =>
            {
                c.SwaggerDoc("v1", new OpenApiInfo
                {
                    Title = "PayloadService",
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
            services.AddSingleton<ILogProvider, LogProvider>();
            services.AddSingleton<IPerformanceLogger, PerformanceLogger>();
            services.AddScoped<IApprovalBlobDataProvider, ApprovalBlobDataProvider>();
            services.AddScoped<IApprovalTenantInfoProvider, ApprovalTenantInfoProvider>();
            services.AddScoped<INameResolutionHelper, NameResolutionHelper>();
            services.AddSingleton<ITableHelper, TableHelper>(x => new TableHelper(config?[Constants.StorageAccountName], config?[ConfigurationKey.StorageAccountKey.ToString()]));
            services.AddSingleton<IBlobStorageHelper, BlobStorageHelper>(x => new BlobStorageHelper(client));
            services.AddScoped<IFlightingDataProvider, FlightingDataProvider>();
            services.AddScoped<IAuthenticationHelper, AuthenticationHelper>();
            services.AddSingleton((Func<IServiceProvider, ICosmosDbHelper>)(c => new CosmosDbHelper(config?[ConfigurationKey.CosmosDbEndPoint.ToString()], config?[ConfigurationKey.CosmosDbAuthKey.ToString()])));
            services.AddSingleton<IHistoryStorageFactory, HistoryStorageFactory>();
            services.AddScoped<IApprovalHistoryProvider, ApprovalHistoryProvider>();
            services.AddScoped<ITenantFactory, TenantFactory>();
            services.AddScoped<IApprovalTenantInfoHelper, ApprovalTenantInfoHelper>();
            services.AddScoped<IValidationFactory, ValidationFactory>();
            services.AddScoped<IPayloadDelivery, PayloadDelivery>();
            services.AddScoped<IPayloadReceiver, PayloadReceiver>();
            services.AddScoped<IPayloadDestination, PayloadDestination>();
            services.AddScoped<IPayloadValidator, PayloadValidator>();
            services.AddScoped<IApprovalRequestExpressionHelper, ApprovalRequestExpressionHelper>();
            services.AddScoped<IPayloadReceiverManager, PayloadReceiverManager>();
            services.AddScoped<IApprovalDetailProvider, ApprovalDetailProvider>();
            services.AddScoped<IApprovalSummaryProvider, ApprovalSummaryProvider>();
            services.AddScoped<IApprovalSummaryHelper, ApprovalSummaryHelper>();
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
                c.SwaggerEndpoint("../swagger/v1/swagger.json", "PayloadService v1");
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