// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace Approvals.AuxiliaryProcessor.BL
{
    using Approvals.AuxiliaryProcessor.BL.Interface;
    using Azure.Messaging.ServiceBus;
    using Microsoft.CFS.Approvals.Common.DL.Interface;
    using Microsoft.CFS.Approvals.Contracts;
    using Microsoft.CFS.Approvals.Core.BL.Interface;
    using Microsoft.CFS.Approvals.LogManager.Model;
    using Microsoft.CFS.Approvals.LogManager.Provider.Interface;
    using Microsoft.CFS.Approvals.Messaging.Azure.ServiceBus.Interface;
    using Microsoft.CFS.Approvals.Model;
    using Microsoft.Extensions.Configuration;
    using Newtonsoft.Json;

    public class AuxiliaryProcessor : IAuxiliaryProcessor
    {
        #region Variables & Properties

        public ApprovalTenantInfo TenantInfo { get; set; }

        /// <summary>
        /// The performance logger
        /// </summary>
        private readonly IPerformanceLogger _performanceLogger;

        /// <summary>
        /// The configuration
        /// </summary>
        private readonly IConfiguration _config;

        /// <summary>
        /// The log provider
        /// </summary>
        private readonly ILogProvider _logProvider;

        /// <summary>
        /// The secondary processor
        /// </summary>
        private readonly ITenantFactory _tenantFactory;

        /// <summary>
        /// The service bus helper
        /// </summary>
        private readonly IServiceBusHelper _serviceBusHelper;

        /// <summary>
        /// The approval summary provider
        /// </summary>
        private readonly IApprovalSummaryProvider _approvalSummaryProvider;

        /// <summary>
        /// The approval Tenant info helper
        /// </summary>
        private readonly IApprovalTenantInfoHelper _approvalTenantInfoHelper;

        /// <summary>
        /// The details helper
        /// </summary>
        private readonly IDetailsHelper _detailsHelper;

        /// <summary>
        /// The approval detail provider
        /// </summary>
        private readonly IApprovalDetailProvider _approvalDetailProvider;

        /// <summary>
        /// The AI analysis helper
        /// </summary>
        private readonly IAIAnalysisHelper _aiAnalysisHelper;

        #endregion Variables & Properties

        #region Constructor

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="performanceLogger"></param>
        /// <param name="configuration"></param>
        /// <param name="logProvider"></param>
        /// <param name="tenantFactory"></param>
        /// <param name="serviceBusHelper"></param>
        /// <param name="approvalSummaryProvider"></param>
        /// <param name="detailsHelper"></param>
        /// <param name="approvalDetailProvider"></param>
        /// <param name="aiAnalysisHelper"></param>
        public AuxiliaryProcessor
            (IPerformanceLogger performanceLogger,
            IConfiguration configuration,
            ILogProvider logProvider,
            ITenantFactory tenantFactory,
            IServiceBusHelper serviceBusHelper,
            IApprovalSummaryProvider approvalSummaryProvider,
            IApprovalTenantInfoHelper approvalTenantInfoHelper,
            IDetailsHelper detailsHelper,
            IApprovalDetailProvider approvalDetailProvider,
            IAIAnalysisHelper aiAnalysisHelper)
        {
            _performanceLogger = performanceLogger;
            _config = configuration;
            _logProvider = logProvider;
            _tenantFactory = tenantFactory;
            _serviceBusHelper = serviceBusHelper;
            _approvalSummaryProvider = approvalSummaryProvider;
            _approvalTenantInfoHelper = approvalTenantInfoHelper;
            _detailsHelper = detailsHelper;
            _approvalDetailProvider = approvalDetailProvider;
            _aiAnalysisHelper = aiAnalysisHelper;
        }

        #endregion Constructor

        #region Public methods

        /// <summary>
        /// Process Message
        /// </summary>
        /// <param name="message"></param>
        /// <returns></returns>
        public async Task ProcessMessageAsync(ServiceBusReceivedMessage message)
        {
            #region Logging

            var logData = new Dictionary<LogDataKey, object>
            {
                { LogDataKey.MessageId, message.MessageId},
                { LogDataKey.LocalTime, DateTime.UtcNow },
                { LogDataKey.TenantId, TenantInfo.TenantId },
            };

            #endregion Logging

            using (var perfTracer = _performanceLogger.StartPerformanceLogger("PerfLog", "AuxiliaryWorker", string.Format(Constants.PerfLogAction, TenantInfo.AppName, "Processes received Brokered Message"), logData))
            {
                try
                {
                    dynamic messageJSON = JsonConvert.DeserializeObject(System.Text.Encoding.UTF8.GetString(message.Body));
                    string? documentNumber = messageJSON?.documentNumber;

                    #region Logging

                    logData.Add(LogDataKey.DocumentNumber, documentNumber);

                    #endregion Logging

                    await ProcessMessageAsync(documentNumber, TenantInfo, message);

                    #region Logging

                    _logProvider.LogInformation(TrackingEvent.AuxiliaryProcessingSuccess, logData);

                    #endregion Logging
                }
                catch (Exception ex)
                {
                    _logProvider.LogError(TrackingEvent.AuxiliaryProcessingFailed, ex, logData);
                }
            }
        }

        #endregion Public methods

        #region Private methods

        /// <summary>
        /// Process Message
        /// </summary>
        /// <param name="documentNumber"></param>
        /// <param name="tenantInfo"></param>
        /// <param name="message"></param>
        /// <returns></returns>
        private async Task ProcessMessageAsync(string documentNumber, ApprovalTenantInfo tenantInfo, ServiceBusReceivedMessage message)
        {
            #region Logging

            var logData = new Dictionary<LogDataKey, object>
            {
            { LogDataKey.DocumentNumber, documentNumber },
            { LogDataKey.TenantId, tenantInfo.DocTypeId },
            { LogDataKey.TenantName, tenantInfo.AppName },
            { LogDataKey.MessageId, message.MessageId }
        };

            #endregion Logging

            using (var perfTracer = _performanceLogger.StartPerformanceLogger("PerfLog", "AuxiliaryWorker", string.Format(Constants.PerfLogAction, tenantInfo.AppName, "Generate discrepancy data for attachements"), logData))
            {
                try
                {
                    // Generate AI analysis of the request and persist results
                    RequestSummaryData openAIResponse = await _aiAnalysisHelper.GenerateAIAnalysisAsync(documentNumber, tenantInfo, message.MessageId);

                    _logProvider.LogInformation(TrackingEvent.AIAnalysisProcessingSuccess, logData);
                }


                catch (Exception ex)
                {
                    logData.Add(LogDataKey.DocumentNumber, documentNumber);
                    _logProvider.LogWarning(TrackingEvent.AIAnalysisProcessingFail, logData, ex);
                }
            }
        }

        #endregion Private methods
    }
}