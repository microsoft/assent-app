// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace Microsoft.CFS.Approvals.PayloadReceiverService
{
    using System;
    using global::Azure.Identity;
    using Microsoft.AspNetCore.Hosting;
    using Microsoft.CFS.Approvals.Contracts;
    using Microsoft.Extensions.Configuration;
    using Microsoft.Extensions.Configuration.AzureAppConfiguration;
    using Microsoft.Extensions.Hosting;

    public class Program
    {
        public static IConfigurationRefresher refresher;

        public static void Main(string[] args)
        {
            CreateHostBuilder(args).Build().Run();
        }

        public static IHostBuilder CreateHostBuilder(string[] args) =>
            Host.CreateDefaultBuilder(args)
                .ConfigureWebHostDefaults(webBuilder =>
                webBuilder.ConfigureAppConfiguration((hostingContext, config) =>
                {
                    var configuration = config.Build();
                    configuration = config.AddEnvironmentVariables().Build(); ;

                    config.AddAzureAppConfiguration(options =>
                    {
                        options.Connect(configuration?[Constants.AzureAppConfiguration])
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
                })
                .UseStartup<Startup>());
    }
}
