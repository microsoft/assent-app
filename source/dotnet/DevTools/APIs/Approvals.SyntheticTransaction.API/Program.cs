// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System;
using System.Net.Http;
using AutoMapper;
using Azure.Identity;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using Microsoft.CFS.Approvals.Common.BL;
using Microsoft.CFS.Approvals.Common.DL.Interface;
using Microsoft.CFS.Approvals.Contracts;
using Microsoft.CFS.Approvals.Data.Azure.Storage.Helpers;
using Microsoft.CFS.Approvals.Data.Azure.Storage.Interface;
using Microsoft.CFS.Approvals.LogManager;
using Microsoft.CFS.Approvals.LogManager.Provider.Interface;
using Microsoft.CFS.Approvals.SyntheticTransaction.API.Services;
using Microsoft.CFS.Approvals.SyntheticTransaction.Common.Helper;
using Microsoft.CFS.Approvals.SyntheticTransaction.Common.Interface;
using Microsoft.CFS.Approvals.SyntheticTransaction.Helpers.Helpers;
using Microsoft.CFS.Approvals.SyntheticTransaction.Helpers.Interface;
using Microsoft.CFS.Approvals.Utilities.Helpers;
using Microsoft.CFS.Approvals.Utilities.Interface;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Polly;
using Polly.Extensions.Http;

string MyAllowSpecificOrigins = "_myAllowSpecificOrigins";
var builder = WebApplication.CreateBuilder(args);
IConfiguration config = builder.Services.BuildServiceProvider().GetService<IConfiguration>();
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
var appSettings = new KeyVaultHelper(builder.Configuration).GetKeyVault();

builder.Services.AddControllers().AddNewtonsoftJson();
builder.Services.AddRouting(options => options.LowercaseUrls = true);
builder.Services.AddMemoryCache();
builder.Services.AddAutoMapper(AppDomain.CurrentDomain.GetAssemblies());
builder.Services.AddSingleton<ConfigurationSetting>(provider => { return new ConfigurationSetting(appSettings); });
builder.Services.AddScoped<ISyntheticTransactionHelper, SyntheticTransactionHelper>();
builder.Services.AddScoped<IRandomFormDetails, RandomFormDetails>();
builder.Services.AddScoped<ConfigurationHelper>();
builder.Services.AddSingleton<IPerformanceLogger, PerformanceLogger>();
builder.Services.AddSingleton<ApplicationInsightsTarget>();
builder.Services.AddSingleton<ILogProvider, LogProvider>();
builder.Services.AddScoped<IAuthenticationHelper, AuthenticationHelper>();
builder.Services.AddScoped<IPayloadReceiverHelper, PayloadReceiverHelper>();
builder.Services.AddScoped<IKeyVaultHelper, KeyVaultHelper>();
builder.Services.AddScoped<IBulkDeleteHelper, BulkDeleteHelper>();
builder.Services.AddScoped<ILoadGeneratorHelper, LoadGeneratorHelper>();
// Secure credential selection for Azure resources
#if DEBUG
var azureCredential = new DefaultAzureCredential();  // CodeQL [SM05137] Suppress CodeQL issue since we only use DefaultAzureCredential in development environments.
#else
    var azureCredential = new ManagedIdentityCredential(); // For production
#endif
builder.Services.AddScoped<Func<string, string, ITableHelper>>((provider) =>
{
    return new Func<string, string, ITableHelper>((StorageAccountName, TokenCredential) => new TableHelper(StorageAccountName, azureCredential));
});
builder.Services.AddTransient<Func<string, string, IBlobStorageHelper>>((provider) =>
{
    return new Func<string, string, IBlobStorageHelper>((StorageAccountName, TokenCredential) => new BlobStorageHelper(StorageAccountName, azureCredential));
});
builder.Services.AddSingleton<IActionContextAccessor, ActionContextAccessor>();
builder.Services.AddSingleton<ISchemaGenerator, SchemaGenerator>();
builder.Services.AddSingleton<HttpClientHandler>();
builder.Services.AddHttpClient<IHttpHelper, HttpHelper>()
    .SetHandlerLifetime(TimeSpan.FromMinutes(5)) // Set lifetime to five minutes
    .AddPolicyHandler(HttpPolicyExtensions
                .HandleTransientHttpError()
                .OrResult(msg => msg.StatusCode == System.Net.HttpStatusCode.TooManyRequests)
                .WaitAndRetryAsync(6, retryAttempt => TimeSpan.FromSeconds(Math.Pow(2,
                    retryAttempt))));
// Add Logging
builder.Services.AddLogging(configure =>
{
    configure.AddApplicationInsights(config?[Constants.AppinsightsInstrumentationkey]);
});
var app = builder.Build();
if (app.Environment.IsDevelopment())
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

app.Run();