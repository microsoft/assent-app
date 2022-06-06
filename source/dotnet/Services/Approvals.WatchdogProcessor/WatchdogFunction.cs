// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace Microsoft.CFS.Approvals.WatchdogAzFunction
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Threading.Tasks;
    using Microsoft.Azure.WebJobs;
    using Microsoft.CFS.Approvals.Contracts;
    using Microsoft.CFS.Approvals.Core.BL.Interface;
    using Microsoft.CFS.Approvals.LogManager.Model;
    using Microsoft.CFS.Approvals.LogManager.Provider.Interface;
    using Microsoft.CFS.Approvals.WatchdogProcessor.BL.Interface;
    using Microsoft.Extensions.Configuration;
    using Microsoft.Extensions.Logging;

    /// <summary>
    /// Watchdog Azure Function
    /// </summary>
    public class WatchdogFunction
    {
        private readonly IConfiguration _config;
        private readonly IApprovalTenantInfoHelper _approvalTenantInfoHelper;
        private readonly IReminderProcessor _reminderProcessor;
        private readonly ILogProvider _logProvider;

        public WatchdogFunction(IConfiguration config,
            IReminderProcessor reminderProcessor,
            IApprovalTenantInfoHelper approvalTenantInfoHelper,
            ILogProvider logProvider)
        {
            _config = config;
            _reminderProcessor = reminderProcessor;
            _logProvider = logProvider;
            _approvalTenantInfoHelper = approvalTenantInfoHelper;
        }

        [FunctionName("Watchdog")]
        public async Task Run([TimerTrigger("%Schedule%")] TimerInfo myTimer, ILogger logger)
        {
            logger.LogInformation($"C# Timer trigger function executed at: {DateTime.Now}");

            DateTime startTime = DateTime.UtcNow;
            int batchSize = int.Parse(_config[ConfigurationKey.WatchDogBatchSize.ToString()]);
            int maxFailureCount = int.Parse(_config[ConfigurationKey.WatchDogMaxFailureCount.ToString()]);
            string baseUrl = _config[ConfigurationKey.ApprovalsBaseUrl.ToString()];

            var logData = new Dictionary<LogDataKey, object>
            {
                { LogDataKey.EventType, Constants.FeatureUsageEvent },
                { LogDataKey.MachineName, Environment.MachineName },
                { LogDataKey.EntryUtcDateTime, startTime },
                { LogDataKey.BatchSize, batchSize },
                { LogDataKey.MaxFailureCount, maxFailureCount },
                { LogDataKey.ApprovalBaseUrl, baseUrl }
            };

            try
            {
                _logProvider.LogInformation(TrackingEvent.WatchDogSendRemindersCallInitiated, logData);

                string tenantId = Environment.GetEnvironmentVariable("TenantIds");
                List<Model.ApprovalTenantInfo> approvalTenantInfo;
                if (tenantId?.Length > 0)
                {
                    List<string> tenantIds = tenantId?.Split(',')?.ToList();
                    approvalTenantInfo = (await _approvalTenantInfoHelper?.GetTenants(false))?.Where(x => tenantIds.Contains(x.TenantId.ToString()))?.ToList();
                }
                else
                {
                    approvalTenantInfo = (await _approvalTenantInfoHelper?.GetTenants(false))?.Where(x => x.TenantEnabled == true & x.NotifyEmail == true)?.ToList();
                }

                if (approvalTenantInfo == null)
                {
                    throw new InvalidDataException("TenantIds configuration is not setup properly in the function app settings.");
                }

                await _reminderProcessor.SendReminders(startTime, maxFailureCount, batchSize, approvalTenantInfo, baseUrl);
            }
            catch (Exception ex)
            {
                _logProvider.LogError(TrackingEvent.WorkerRoleWatchDogError, ex, logData);
            }
        }
    }
}