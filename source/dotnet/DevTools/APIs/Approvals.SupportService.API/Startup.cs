// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace Microsoft.CFS.Approvals.SupportService.API
{
    using System;
    using System.Diagnostics.CodeAnalysis;
    using System.Net.Http;
    using Microsoft.AspNetCore.Builder;
    using Microsoft.AspNetCore.Hosting;
    using Microsoft.AspNetCore.Mvc.Infrastructure;
    using Microsoft.Azure.ServiceBus.Management;
    using Microsoft.CFS.Approvals.Common.BL.Interface;
    using Microsoft.CFS.Approvals.Common.DL.Interface;
    using Microsoft.CFS.Approvals.LogManager;
    using Microsoft.CFS.Approvals.LogManager.Provider.Interface;
    using Microsoft.CFS.Approvals.SupportServices.Helper.Interface;
    using Microsoft.CFS.Approvals.SupportServices.Helper.ServiceHelper;
    using Microsoft.Extensions.Configuration;
    using Microsoft.Extensions.DependencyInjection;
    using Microsoft.Extensions.Hosting;
    using Microsoft.CFS.Approvals.Common.BL;
    using Microsoft.CFS.Approvals.Data.Azure.Storage.Interface;
    using Microsoft.CFS.Approvals.Data.Azure.Storage.Helpers;
    using Microsoft.CFS.Approvals.Utilities.Interface;
    using Microsoft.CFS.Approvals.Utilities.Helpers;
    using Polly;
    using Polly.Extensions.Http;

    [ExcludeFromCodeCoverage]
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }
        private readonly string MyAllowSpecificOrigins = "_myAllowSpecificOrigins";

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            IConfiguration config = services.BuildServiceProvider().GetService<IConfiguration>();
            services.AddCors(options =>
            {
                options.AddPolicy(MyAllowSpecificOrigins,
                    builder =>
                    {
                        builder.WithOrigins("https://localhost:9000",
                                            "http://10.76.20.69:3000").AllowAnyHeader()
                                .AllowAnyMethod();
                    });
            });

            services.AddControllers().AddNewtonsoftJson();
            services.AddRouting(options => options.LowercaseUrls = true);

            var appSettings = new ApplicationSettingsHelper(Configuration).GetSettings();
            services.AddSingleton<IAIHelper, AIHelper>();
            services.AddSingleton<IServiceBusHelper, ServiceBusHelper>();
            services.AddSingleton<ConfigurationHelper>(provider =>
            {
                foreach (var key in appSettings.Keys)
                {
                    config[key + "_StorageAccountName"] = appSettings[key].StorageAccountName;
                    config[key + "_StorageAccountKey"] = appSettings[key].StorageAccountKey;
                }
                return new ConfigurationHelper(appSettings);
            });
            services.AddTransient<Func<string, string, ITableHelper>>((provider) =>
            {
                return new Func<string, string, ITableHelper>(
                     (StorageAccountName, StorageAccountKey) => new TableHelper(StorageAccountName, StorageAccountKey)
                );
            });
            services.AddTransient<Func<string, string, IBlobStorageHelper>>((provider) =>
            {
                return new Func<string, string, IBlobStorageHelper>(
                     (StorageAccountName, StorageAccountKey) => new BlobStorageHelper(StorageAccountName, StorageAccountKey)
                );
            });
            services.AddSingleton<IActionContextAccessor, ActionContextAccessor>();
            services.AddSingleton<IUserDelegationHelper, UserDelegationHelper>();
            services.AddSingleton<ApplicationInsightsTarget>();
            services.AddSingleton<ILogProvider, LogProvider>();
            services.AddSingleton<IPerformanceLogger, PerformanceLogger>();
            services.AddSingleton<INameResolutionHelper, NameResolutionHelper>();
            services.AddSingleton<ISubscribeFeaturesHelper, SubscribeFeaturesHelper>();
            services.AddSingleton<IMarkRequestOutOfSyncHelper, MarkRequestOutOfSyncHelper>();
            services.AddSingleton<ITenantOnBoardingHelper, TenantOnBoardingHelper>();
            services.AddSingleton<Func<string, ManagementClient>>((provider) =>
            {
                return new Func<string, ManagementClient>(
                     (ServiceBusConnection) => new ManagementClient(ServiceBusConnection)
                );
            });
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

            app.UseHttpsRedirection();
            app.UseRouting();
            app.UseCors(MyAllowSpecificOrigins);
            app.UseAuthorization();

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