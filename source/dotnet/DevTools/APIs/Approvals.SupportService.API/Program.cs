// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System;
using System.Net.Http;
using Azure.Core;
using Azure.Identity;
using Azure.Messaging.ServiceBus.Administration;
using Azure.Storage.Blobs;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using Microsoft.CFS.Approvals.Common.BL;
using Microsoft.CFS.Approvals.Common.DL.Interface;
using Microsoft.CFS.Approvals.Core.BL.Factory;
using Microsoft.CFS.Approvals.Core.BL.Interface;
using Microsoft.CFS.Approvals.Data.Azure.Storage.Helpers;
using Microsoft.CFS.Approvals.Data.Azure.Storage.Interface;
using Microsoft.CFS.Approvals.DevTools.AppConfiguration;
using Microsoft.CFS.Approvals.LogManager;
using Microsoft.CFS.Approvals.LogManager.Provider.Interface;
using Microsoft.CFS.Approvals.SupportService.API.Utils;
using Microsoft.CFS.Approvals.SupportServices.Helper.Interface;
using Microsoft.CFS.Approvals.SupportServices.Helper.ServiceHelper;
using Microsoft.CFS.Approvals.Utilities.Helpers;
using Microsoft.CFS.Approvals.Utilities.Interface;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Polly;
using Polly.Extensions.Http;

string MyAllowSpecificOrigins = "_myAllowSpecificOrigins";
var builder = WebApplication.CreateBuilder(args);
builder.Services.AddCors(options =>
{
    options.AddPolicy(MyAllowSpecificOrigins,
        builder =>
        {
            builder.WithOrigins("https://localhost:9000",
                                "http://10.76.20.69:3000").AllowAnyHeader()
                    .AllowAnyMethod();
        });
});

builder.Services.AddControllers().AddNewtonsoftJson();
builder.Services.AddRouting(options => options.LowercaseUrls = true);
var appSettings = new ApplicationSettingsHelper(builder.Configuration).GetSettings();
builder.Services.AddScoped<IServiceBusHelper, ServiceBusHelper>();
builder.Services.AddSingleton<ConfigurationHelper>(provider =>
{
    return new ConfigurationHelper(appSettings);
});

#if DEBUG
    var azureCredential = new DefaultAzureCredential(); // CodeQL [SM05137] Suppress CodeQL issue since we only use DefaultAzureCredential in development environments.
#else
    var azureCredential = new ManagedIdentityCredential();
#endif

// Secure Azure credential selection for TableHelper
builder.Services.AddTransient<Func<string, ITableHelper>>((provider) =>
{
    return new Func<string, ITableHelper>(
         (StorageAccountName) =>
         {

             return new TableHelper(StorageAccountName, azureCredential);
         }
    );
});

// Secure Azure credential selection for BlobStorageHelper
builder.Services.AddTransient<Func<string, IBlobStorageHelper>>((provider) =>
{
    return new Func<string, IBlobStorageHelper>(
          (StorageAccountName) =>
          {
              return new BlobStorageHelper(new BlobServiceClient(
                     new Uri($"https://" + StorageAccountName + ".blob.core.windows.net/"),
                     azureCredential));
          }
     );
});
builder.Services.AddSingleton<TokenCredential>(azureCredential);
builder.Services.AddScoped<ITelemetryHelper, TelemetryHelper>();
builder.Services.AddScoped<AuthorizationMiddleware, AuthorizationMiddleware>();
builder.Services.AddSingleton<IActionContextAccessor, ActionContextAccessor>();
builder.Services.AddScoped<IUserDelegationHelper, UserDelegationHelper>();
builder.Services.AddSingleton<ApplicationInsightsTarget>();
builder.Services.AddScoped<ILogProvider, LogProvider>();
builder.Services.AddScoped<IPerformanceLogger, PerformanceLogger>();
builder.Services.AddScoped<Func<string, INameResolutionHelper>>((provider) =>
{
    return new Func<string, INameResolutionHelper>(
        (environment) => new NameResolutionHelper(
            provider.GetService<IHttpHelper>(),
            provider.GetService<ConfigurationHelper>().appSettings[environment],
            provider.GetService<ILogProvider>(),
            provider.GetService<IPerformanceLogger>()));
});
builder.Services.AddScoped<IAuthenticationHelper, AuthenticationHelper>();
builder.Services.AddScoped<ISubscribeFeaturesHelper, SubscribeFeaturesHelper>();
builder.Services.AddScoped<IMarkRequestOutOfSyncHelper, MarkRequestOutOfSyncHelper>();
builder.Services.AddScoped<ITenantOnBoardingHelper, TenantOnBoardingHelper>();
// Secure Azure credential selection for ServiceBusAdministrationClient
builder.Services.AddSingleton<Func<string, ServiceBusAdministrationClient>>((provider) =>
{
    return new Func<string, ServiceBusAdministrationClient>(
         (ServiceBusNamespace) =>
         {
             return new ServiceBusAdministrationClient(ServiceBusNamespace, azureCredential);
         }
    );
});
builder.Services.AddScoped<Func<string, IARConverterFactory>>((provider) =>
{
    return new Func<string, IARConverterFactory>(
        (environment) => new ARConverterFactory(
            provider.GetService<IPerformanceLogger>(),
            provider.GetService<ConfigurationHelper>().appSettings[environment],
            provider.GetService<INameResolutionHelper>()));
});

builder.Services
    .AddHttpClient<IHttpHelper, HttpHelper>()
    .ConfigurePrimaryHttpMessageHandler(() => new HttpClientHandler())
    .SetHandlerLifetime(TimeSpan.FromMinutes(5)) // Set lifetime to five minutes
    .AddPolicyHandler(HttpPolicyExtensions
                .HandleTransientHttpError()
                .OrResult(msg => msg.StatusCode == System.Net.HttpStatusCode.TooManyRequests)
                .WaitAndRetryAsync(6, retryAttempt => TimeSpan.FromSeconds(Math.Pow(2,
                    retryAttempt))));

var app = builder.Build();
if(app.Environment.IsDevelopment())
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
app.UseMiddleware<AuthorizationMiddleware>();

app.UseEndpoints(endpoints =>
{
    endpoints.MapControllers();
});
app.Run();
