namespace Approvals.ReassignmentProcessor.BL
{
    using System.Net;
    using System.Net.Http.Headers;
    using System.Text;
    using Approvals.ReassignmentProcessor.BL.Interface;
    using Azure.Messaging.ServiceBus;
    using Microsoft.CFS.Approvals.Common.DL.Interface;
    using Microsoft.CFS.Approvals.Contracts;
    using Microsoft.CFS.Approvals.Contracts.DataContracts;
    using Microsoft.CFS.Approvals.Extensions;
    using Microsoft.CFS.Approvals.LogManager.Model;
    using Microsoft.CFS.Approvals.LogManager.Provider.Interface;
    using Microsoft.CFS.Approvals.Model;
    using Microsoft.CFS.Approvals.Utilities.Interface;
    using Microsoft.Extensions.Configuration;
    using Constants = Microsoft.CFS.Approvals.Contracts.Constants;

    public class ReassignmentProcessor : IReassignmentProcessor
    {
        #region Properties

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
        /// The approval summary provider
        /// </summary>
        private readonly IApprovalSummaryProvider _approvalSummaryProvider;

        /// <summary>
        /// The Authentication Helper
        /// </summary>
        private readonly IAuthenticationHelper _authenticationHelper;

        /// <summary>
        /// The Http Helper
        /// </summary>
        private readonly IHttpHelper _httpHelper;

        #endregion Properties

        #region Constructor

        /// <summary>
        /// ReassignmentProcessor Constructor
        /// </summary>
        /// <param name="performanceLogger"></param>
        /// <param name="configuration"></param>
        /// <param name="logProvider"></param>
        /// <param name="approvalSummaryProvider"></param>
        /// <param name="authenticationHelper"></param>
        /// <param name="httpHelper"></param>
        public ReassignmentProcessor
            (IPerformanceLogger performanceLogger,
            IConfiguration configuration,
            ILogProvider logProvider,
            IApprovalSummaryProvider approvalSummaryProvider,
            IAuthenticationHelper authenticationHelper,
            IHttpHelper httpHelper)
        {
            _performanceLogger = performanceLogger;
            _config = configuration;
            _logProvider = logProvider;
            _approvalSummaryProvider = approvalSummaryProvider;
            _authenticationHelper = authenticationHelper;
            _httpHelper = httpHelper;
        }

        #endregion Constructor

        /// <summary>
        /// Process Reassignment details
        /// </summary>
        /// <param name="approvalRequest"></param>
        /// <param name="blobId"></param>
        /// <param name="tenantInfo"></param>        
        /// <param name="message"></param>
        /// <returns></returns>
        public async Task<ApprovalRequestExpressionExt> ProcessReassignmentDetails(ApprovalRequestExpressionExt approvalRequest, string blobId, ApprovalTenantInfo tenantInfo, ServiceBusReceivedMessage message)
        {
            var logData = new Dictionary<LogDataKey, object> { };
            using (var perfTracer = _performanceLogger.StartPerformanceLogger("PerfLog", "ReassignmentWorker", string.Format(Constants.PerfLogAction, "Reassignment", "Fetch and process auto-reassignment payload"), logData))
            {
                #region Logging
                logData.Add(LogDataKey.Xcv, approvalRequest?.Telemetry?.Xcv);
                logData.Add(LogDataKey.Tcv, approvalRequest?.Telemetry?.Tcv);
                logData.Add(LogDataKey.TenantTelemetryData, approvalRequest?.Telemetry?.TenantTelemetry);
                logData.Add(LogDataKey.EventType, Constants.FeatureUsageEvent);
                logData.Add(LogDataKey.MessageId, blobId);
                logData.Add(LogDataKey.DisplayDocumentNumber, approvalRequest?.ApprovalIdentifier?.DisplayDocumentNumber);
                logData.Add(LogDataKey.DocumentNumber, approvalRequest?.ApprovalIdentifier?.DocumentNumber);
                logData.Add(LogDataKey.TenantId, tenantInfo?.DocTypeId);
                logData.Add(LogDataKey.TenantName, tenantInfo?.AppName);

                #endregion Logging

                // Validate if the request is already actioned by the original approver
                if (approvalRequest?.Approvers != null && approvalRequest.Approvers.Any())
                {
                    foreach (var approver in approvalRequest.Approvers)
                    {
                        string approverAlias = approver.Alias;
                        string approverId = approver.Id.ToString();
                        string domain = approver.UserPrincipalName.GetDomainFromUPN();

                        ApprovalSummaryRow filteredSummaryRow = _approvalSummaryProvider.GetApprovalSummaryByDocumentNumberAndApprover
                            (approvalRequest?.DocumentTypeId.ToString(),
                             approvalRequest?.ApprovalIdentifier?.DocumentNumber,
                             approverAlias,
                             approverId,
                             domain);

                        if (filteredSummaryRow != null && (filteredSummaryRow.LobPending == false || filteredSummaryRow.IsOfflineApproval == true))
                        {
                            // Send the payload to Payload Receiver Service Bus Queue
                            try
                            {
                                var targetUri = string.Format(_config[ConfigurationKey.PayloadReceiverUrl.ToString()], approvalRequest?.DocumentTypeId.ToString());
                                logData[LogDataKey.Uri] = targetUri;

                                HttpRequestMessage requestMessage = new HttpRequestMessage(HttpMethod.Post, targetUri);

                                var accessToken = await _authenticationHelper.GetManagedIdentityToken(_config[ConfigurationKey.ManagedIdentityClientId.ToString()], _config[ConfigurationKey.PayloadReceiverResourceUrl.ToString()]);

                                logData[LogDataKey.IdentityProviderTokenType] = "ManagedIdentityToken";

                                requestMessage.Headers.Authorization = new AuthenticationHeaderValue(Constants.AuthorizationHeaderScheme, accessToken);

                                requestMessage.Content = new StringContent(approvalRequest.ToJson(), Encoding.UTF8);

                                var response = await _httpHelper.SendRequestAsync(requestMessage);
                                var responseData = await response.Content.ReadAsStringAsync();
                                if (!response.IsSuccessStatusCode)
                                {
                                    logData[LogDataKey.ResponseContent] = responseData;
                                    throw new WebException("Status Code: " + response.StatusCode.ToString() + " " + responseData, WebExceptionStatus.ReceiveFailure);
                                }
                                _logProvider.LogInformation(TrackingEvent.ReassignmentProcessingSuccess, logData);
                            }
                            catch (Exception ex)
                            {
                                // Log failure event.
                                logData[LogDataKey.EndDateTime] = DateTime.UtcNow;
                                _logProvider.LogError(TrackingEvent.ReassignmentProcessingFail, ex, logData);
                            }
                        }
                    }
                }
                return null;
            }
        }
    }
}