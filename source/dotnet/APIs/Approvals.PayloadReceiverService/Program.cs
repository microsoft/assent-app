// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Http;
using Azure.Storage.Blobs;
using global::Azure.Identity;
using global::Azure.Messaging.ServiceBus;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Azure.Cosmos;
using Microsoft.CFS.Approvals.Messaging.Azure.ServiceBus.Helpers;
using Microsoft.CFS.Approvals.Messaging.Azure.ServiceBus.Interface;
using Microsoft.CFS.Approvals.Common.BL.Interface;
using Microsoft.CFS.Approvals.Common.BL;
using Microsoft.CFS.Approvals.Common.DL.Interface;
using Microsoft.CFS.Approvals.Common.DL;
using Microsoft.CFS.Approvals.Contracts;
using Microsoft.CFS.Approvals.Core.BL.Factory;
using Microsoft.CFS.Approvals.Core.BL.Helpers;
using Microsoft.CFS.Approvals.Core.BL.Interface;
using Microsoft.CFS.Approvals.Data.Azure.CosmosDb.Helpers;
using Microsoft.CFS.Approvals.Data.Azure.CosmosDb.Interface;
using Microsoft.CFS.Approvals.Data.Azure.Storage.Helpers;
using Microsoft.CFS.Approvals.Data.Azure.Storage.Interface;
using Microsoft.CFS.Approvals.LogManager.Provider.Interface;
using Microsoft.CFS.Approvals.LogManager;
using Microsoft.CFS.Approvals.PayloadReceiver.BL.Interface;
using Microsoft.CFS.Approvals.PayloadReceiver.BL;
using Microsoft.CFS.Approvals.PayloadReceiverService.Utils;
using Microsoft.CFS.Approvals.Utilities.Helpers;
using Microsoft.CFS.Approvals.Utilities.Interface;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Configuration.AzureAppConfiguration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.OpenApi.Models;
using Polly.Extensions.Http;
using Polly;

var builder = WebApplication.CreateBuilder(args);
IConfigurationRefresher refresher;
IConfiguration config = builder.Services.BuildServiceProvider().GetService<IConfiguration>();
builder.WebHost.ConfigureAppConfiguration((hostingContext, config) =>
{
    var configuration = config.Build();
    configuration = config.AddEnvironmentVariables().Build(); ;

    config.AddAzureAppConfiguration(options =>
    {
        options.Connect(new Uri(configuration?[Constants.AzureAppConfigurationUrl]), new DefaultAzureCredential())
                .Select(KeyFilter.Any, configuration?[Constants.AppConfigurationLabel])
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
    configuration = config.Build();
});

builder.Services.AddControllers().AddNewtonsoftJson(options =>
    options.SerializerSettings.ReferenceLoopHandling = Newtonsoft.Json.ReferenceLoopHandling.Ignore
);
// Add Logging
builder.Services.AddLogging(configure =>
{
    configure.AddApplicationInsights(config?[Constants.AppinsightsInstrumentationkey]);
});
builder.Services.AddRouting(options => options.LowercaseUrls = true);
// Register the Swagger generator, defining one or more Swagger documents
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "PayloadService",
        Version = "v1"
    });

    var xmlPath = Path.ChangeExtension(typeof(AddRequiredHeaderParameter).Assembly.Location, ".xml");
    c.IncludeXmlComments(xmlPath);
    c.OperationFilter<AddRequiredHeaderParameter>();
    c.EnableAnnotations();
});

var client = new BlobServiceClient(
                            new Uri($"https://" + config?[Constants.StorageAccountName] + ".blob.core.windows.net/"),
                            new DefaultAzureCredential());
var cosmosdbClient = new CosmosClient(config?[ConfigurationKey.CosmosDbEndPoint.ToString()],
                new DefaultAzureCredential(), new CosmosClientOptions() { AllowBulkExecution = true });
var serviceBusClient = new ServiceBusClient(
                            config?[ConfigurationKey.ServiceBusNamespace.ToString()] + ".servicebus.windows.net",
                            new DefaultAzureCredential());
builder.Services.AddSingleton<ApplicationInsightsTarget>();
builder.Services.AddSingleton<ILogProvider, LogProvider>();
builder.Services.AddSingleton<IPerformanceLogger, PerformanceLogger>();
builder.Services.AddScoped<IApprovalBlobDataProvider, ApprovalBlobDataProvider>();
builder.Services.AddScoped<IApprovalTenantInfoProvider, ApprovalTenantInfoProvider>();
builder.Services.AddScoped<INameResolutionHelper, NameResolutionHelper>();
builder.Services.AddSingleton<ITableHelper, TableHelper>(x => new TableHelper(config?[Constants.StorageAccountName].ToString(), new DefaultAzureCredential()));
builder.Services.AddSingleton<IBlobStorageHelper, BlobStorageHelper>(x => new BlobStorageHelper(client));
builder.Services.AddScoped<IFlightingDataProvider, FlightingDataProvider>();
builder.Services.AddScoped<IAuthenticationHelper, AuthenticationHelper>();
builder.Services.AddSingleton<ICosmosDbHelper, CosmosDbHelper>(x => new CosmosDbHelper(cosmosdbClient));
builder.Services.AddSingleton<IServiceBusHelper, ServiceBusHelper>(x => new ServiceBusHelper(serviceBusClient.CreateSender(config[ConfigurationKey.TopicNameMain.ToString()])));
builder.Services.AddSingleton<IHistoryStorageFactory, HistoryStorageFactory>();
builder.Services.AddScoped<IApprovalHistoryProvider, ApprovalHistoryProvider>();
builder.Services.AddScoped<ITenantFactory, TenantFactory>();
builder.Services.AddScoped<IApprovalTenantInfoHelper, ApprovalTenantInfoHelper>();
builder.Services.AddScoped<IValidationFactory, ValidationFactory>();
builder.Services.AddScoped<IPayloadDelivery, PayloadDelivery>();
builder.Services.AddScoped<IPayloadReceiver, PayloadReceiver>();
builder.Services.AddScoped<IPayloadValidator, PayloadValidator>();
builder.Services.AddScoped<IApprovalRequestExpressionHelper, ApprovalRequestExpressionHelper>();
builder.Services.AddScoped<IPayloadReceiverManager, PayloadReceiverManager>();
builder.Services.AddScoped<IApprovalDetailProvider, ApprovalDetailProvider>();
builder.Services.AddScoped<IApprovalSummaryProvider, ApprovalSummaryProvider>();
builder.Services.AddScoped<IApprovalSummaryHelper, ApprovalSummaryHelper>();
builder.Services.AddScoped<AuthorizationMiddleware, AuthorizationMiddleware>();
builder.Services.AddSingleton<HttpClientHandler>();
builder.Services.AddHttpClient<IHttpHelper, HttpHelper>()
    .SetHandlerLifetime(TimeSpan.FromMinutes(5)) // Set lifetime to five minutes
    .AddPolicyHandler(HttpPolicyExtensions
        .HandleTransientHttpError()
        .OrResult(msg => msg.StatusCode == System.Net.HttpStatusCode.TooManyRequests)
        .WaitAndRetryAsync(6, retryAttempt => TimeSpan.FromSeconds(Math.Pow(2,
            retryAttempt))));

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

app.UseSwagger(c =>
{
    c.PreSerializeFilters.Add((swagger, httpReq) =>
    {
        swagger.Servers = new List<OpenApiServer> { new OpenApiServer { Url = $"{httpReq.Scheme}://{httpReq.Host.Value}" } };
    });
});
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

app.Run();