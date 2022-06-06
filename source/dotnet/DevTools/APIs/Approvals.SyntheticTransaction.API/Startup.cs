// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace Microsoft.CFS.Approvals.SyntheticTransaction.API
{
    using System;
    using System.Net.Http;
    using AutoMapper;
    using Common.Helper;
    using Common.Interface;
    using Microsoft.AspNetCore.Builder;
    using Microsoft.AspNetCore.Hosting;
    using Microsoft.AspNetCore.Mvc.Infrastructure;
    using Microsoft.CFS.Approvals.Data.Azure.Storage.Helpers;
    using Microsoft.CFS.Approvals.Data.Azure.Storage.Interface;
    using Microsoft.CFS.Approvals.SyntheticTransaction.Helpers.Helpers;
    using Microsoft.CFS.Approvals.SyntheticTransaction.Helpers.Interface;
    using Microsoft.CFS.Approvals.Utilities.Helpers;
    using Microsoft.CFS.Approvals.Utilities.Interface;
    using Microsoft.Extensions.Configuration;
    using Microsoft.Extensions.DependencyInjection;
    using Microsoft.Extensions.Hosting;
    using Polly;
    using Polly.Extensions.Http;
    using Services;

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
            var appSettings = new KeyVaultHelper(Configuration).GetKeyVault();

            services.AddControllers().AddNewtonsoftJson();
            services.AddRouting(options => options.LowercaseUrls = true);
            services.AddMemoryCache();
            services.AddAutoMapper(AppDomain.CurrentDomain.GetAssemblies());
            services.AddSingleton<ConfigurationSetting>(provider => { return new ConfigurationSetting(appSettings); });
            services.AddScoped<ISyntheticTransactionHelper, SyntheticTransactionHelper>();
            services.AddScoped<IRandomFormDetails, RandomFormDetails>();
            services.AddScoped<ConfigurationHelper>();
            services.AddScoped<IPayloadReceiverHelper, PayloadReceiverHelper>();
            services.AddScoped<IKeyVaultHelper, KeyVaultHelper>();
            services.AddScoped<IBulkDeleteHelper, BulkDeleteHelper>();
            services.AddScoped<ILoadGeneratorHelper, LoadGeneratorHelper>();
            services.AddScoped<Func<string, string, ITableHelper>>((provider) =>
            {
                return new Func<string, string, ITableHelper>((StorageAccountName, StorageAccountKey) => new TableHelper(StorageAccountName, StorageAccountKey));
            });
            services.AddTransient<Func<string, string, IBlobStorageHelper>>((provider) =>
            {
                return new Func<string, string, IBlobStorageHelper>((StorageAccountName, StorageAccountKey) => new BlobStorageHelper(StorageAccountName, StorageAccountKey));
            });
            services.AddSingleton<IActionContextAccessor, ActionContextAccessor>();
            services.AddSingleton<ISchemaGenerator, SchemaGenerator>();
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