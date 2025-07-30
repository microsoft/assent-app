// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using Azure.Identity;
using Azure.Storage.Blobs;
using Microsoft.AspNetCore.Builder;
using Microsoft.Azure.Cosmos;
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
using Microsoft.CFS.Approvals.Domain.BL.Interface;
using Microsoft.CFS.Approvals.Domain.BL.Tenants.Validations;
using Microsoft.CFS.Approvals.LogManager;
using Microsoft.CFS.Approvals.LogManager.Provider.Interface;
using Microsoft.CFS.Approvals.Utilities.Helpers;
using Microsoft.CFS.Approvals.Utilities.Interface;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Configuration.AzureAppConfiguration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.OpenApi.Models;
using Polly;
using Polly.Extensions.Http;

var builder = WebApplication.CreateBuilder(args);
IConfigurationRefresher refresher;
IConfiguration config = builder.Services.BuildServiceProvider().GetService<IConfiguration>();

#if DEBUG
        var azureCredential = new DefaultAzureCredential(); // CodeQL [SM05137] Suppress CodeQL issue since we only use DefaultAzureCredential in development environments.
#else
        var azureCredential = new ManagedIdentityCredential();
#endif


builder.WebHost.ConfigureAppConfiguration((hostingContext, config) =>
{
    var configuration = config.Build();
    configuration = config.AddEnvironmentVariables().Build(); ;

    config.AddAzureAppConfiguration(options =>
    {
        options.Connect(new Uri(configuration[Constants.AzureAppConfigurationUrl]), azureCredential)
            .Select(KeyFilter.Any, configuration?[Constants.AppConfigurationLabel])
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
    configuration = config.Build();
});

// Add Logging
builder.Services.AddLogging(configure =>
{
    configure.AddApplicationInsights(config?[Constants.AppinsightsInstrumentationkey]);
});

builder.Services.AddControllers().AddNewtonsoftJson(options =>
{
    options.UseCamelCasing(true);
});

builder.Services.AddRouting(options => options.LowercaseUrls = true);

// Register the Swagger generator, defining one or more Swagger documents
builder.Services.AddSwaggerGen(c =>
{
    c.ResolveConflictingActions(apiDescriptions => apiDescriptions.First());
    c.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "CoreServices",
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
builder.Services.AddSingleton<ApplicationInsightsTarget>();
builder.Services.AddScoped<IClientActionHelper, ClientActionHelper>();
builder.Services.AddScoped<IOfficeDocumentCreator, OfficeDocumentCreator>();
builder.Services.AddSingleton<ILogProvider, LogProvider>();
builder.Services.AddSingleton<IPerformanceLogger, PerformanceLogger>();
builder.Services.AddScoped<IApprovalBlobDataProvider, ApprovalBlobDataProvider>();
builder.Services.AddScoped<IApprovalTenantInfoProvider, ApprovalTenantInfoProvider>();
builder.Services.AddScoped<INameResolutionHelper, NameResolutionHelper>();
builder.Services.AddSingleton<ITableHelper, TableHelper>(x => new TableHelper(config?[Constants.StorageAccountName].ToString(), new DefaultAzureCredential()));
builder.Services.AddSingleton<IBlobStorageHelper, BlobStorageHelper>(x => new BlobStorageHelper(client));
builder.Services.AddScoped<IFlightingDataProvider, FlightingDataProvider>();
builder.Services.AddSingleton<ICosmosDbHelper, CosmosDbHelper>(x => new CosmosDbHelper(cosmosdbClient));
builder.Services.AddSingleton<IHistoryStorageFactory, HistoryStorageFactory>();

var tableHelper = builder.Services.BuildServiceProvider().GetService<ITableHelper>();
var logger = builder.Services.BuildServiceProvider().GetService<ILogProvider>();
var performanceLogger = builder.Services.BuildServiceProvider().GetService<IPerformanceLogger>();
var approvalTenantInfoProvider = builder.Services.BuildServiceProvider().GetService<IApprovalTenantInfoProvider>();
var historyStorageFactory = builder.Services.BuildServiceProvider().GetService<IHistoryStorageFactory>();
var cosmosDbHelper = builder.Services.BuildServiceProvider().GetService<ICosmosDbHelper>();

if (bool.Parse(config?[ConfigurationKey.IsAzureSearchEnabled.ToString()]))
    builder.Services.AddScoped<IApprovalHistoryProvider, ApprovalHistoryAzureSearchProvider>(h => new ApprovalHistoryAzureSearchProvider(config, tableHelper, logger, performanceLogger, approvalTenantInfoProvider, historyStorageFactory, cosmosDbHelper));
else
    builder.Services.AddScoped<IApprovalHistoryProvider, ApprovalHistoryProvider>(h => new ApprovalHistoryProvider(config, approvalTenantInfoProvider, logger, performanceLogger, cosmosDbHelper, historyStorageFactory));
builder.Services.AddScoped<ITenantFactory, TenantFactory>();
builder.Services.AddScoped<IApprovalTenantInfoHelper, ApprovalTenantInfoHelper>();
builder.Services.AddScoped<IAboutHelper, AboutHelper>();
builder.Services.AddScoped<IApprovalDetailProvider, ApprovalDetailProvider>();
builder.Services.AddScoped<IApprovalSummaryProvider, ApprovalSummaryProvider>();
builder.Services.AddScoped<IDocumentApprovalStatusHelper, DocumentApprovalStatusHelper>();
builder.Services.AddScoped<IAdaptiveDetailsHelper, AdaptiveDetailsHelper>();
builder.Services.AddScoped<ITenantDownTimeMessagesProvider, TenantDownTimeMessagesProvider>();
builder.Services.AddScoped<ITenantDownTimeMessagesHelper, TenantDownTimeMessagesHelper>();
builder.Services.AddSingleton<ILocalFileCache, LocalFileCache>();
builder.Services.AddScoped<IImageRetriever, UserImageRetrieval>();
builder.Services.AddScoped<IApprovalSummaryHelper, ApprovalSummaryHelper>();
builder.Services.AddScoped<ISummaryHelper, SummaryHelper>();
builder.Services.AddScoped<IEditableConfigurationHelper, EditableConfigurationHelper>();
builder.Services.AddScoped<IActionAuditLogger, ActionAuditLogger>();
builder.Services.AddScoped<IActionAuditLogHelper, ActionAuditLogHelper>();
builder.Services.AddScoped<IUserDelegationProvider, UserDelegationProvider>();
builder.Services.AddScoped<IDelegationHelper, DelegationHelper>();
builder.Services.AddScoped<IDetailsHelper, DetailsHelper>();
builder.Services.AddScoped<IAttachmentHelper, AttachmentHelper>();
builder.Services.AddScoped<IApprovalHistoryHelper, ApprovalHistoryHelper>();
builder.Services.AddScoped<IDocumentActionHelper, DocumentActionHelper>();
builder.Services.AddScoped<IClientActionHelper, ClientActionHelper>();
builder.Services.AddScoped<IFlightingDataProvider, FlightingDataProvider>();
builder.Services.AddScoped<IPullTenantHelper, PullTenantHelper>();
builder.Services.AddScoped<IReadDetailsHelper, ReadDetailsHelper>();
builder.Services.AddScoped<ISaveEditableDetailsHelper, SaveEditableDetailsHelper>();
builder.Services.AddScoped<IUserPreferenceHelper, UserPreferenceHelper>();
builder.Services.AddScoped<IAuthenticationHelper, AuthenticationHelper>();
builder.Services.AddScoped<DocumentActionHelper>();
builder.Services.AddScoped<BulkDocumentActionHelper>();
builder.Services.AddScoped<BulkExternalDocumentActionHelper>();
builder.Services.AddScoped<IValidation, ValidationBase>();

builder.Services.AddScoped<Func<string, IDocumentActionHelper>>(serviceProvider => key =>
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

app.UseHttpsRedirection();

app.UseRouting();

app.UseMiddleware<AuthorizationMiddleware>();

app.UseEndpoints(endpoints =>
{
    endpoints.MapControllers();
});

app.Run();