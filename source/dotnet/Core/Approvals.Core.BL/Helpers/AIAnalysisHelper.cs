// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using Azure; // BinaryData
using Microsoft.CFS.Approvals.Common.DL.Interface;
using Microsoft.CFS.Approvals.Contracts;
using Microsoft.CFS.Approvals.Core.BL.Interface;
using Microsoft.CFS.Approvals.Extensions;
using Microsoft.CFS.Approvals.LogManager.Model;
using Microsoft.CFS.Approvals.LogManager.Provider.Interface;
using Microsoft.CFS.Approvals.Model;
using Microsoft.Extensions.Configuration;
using OpenAI.Chat;

namespace Microsoft.CFS.Approvals.Core.BL.Helpers
{
    /// <summary>
    /// Helper responsible for generating AI analysis (request summary + attachment insights) for an approval request.
    /// Workflow:
    /// 1. Fetch request details and OCR data.
    /// 2. Build chat messages and model options (JSON schema enforced).
    /// 3. Execute chat completion via <see cref="IChatHelper"/>.
    /// 4. Persist the analysis as an approval details entity.
    /// </summary>
    public class AIAnalysisHelper : IAIAnalysisHelper
    {
        private readonly IApprovalDetailProvider _approvalDetailProvider;
        private readonly IIntelligenceHelper _intelligenceHelper;
        private readonly IConfiguration _config;
        private readonly ILogProvider _logProvider;
        private readonly IPerformanceLogger _performanceLogger;
        private readonly IApprovalTenantInfoHelper _approvalTenantInfoHelper;

        /// <summary>
        /// Initializes a new instance of the <see cref="AIAnalysisHelper"/> class.
        /// </summary>
        public AIAnalysisHelper(
            ILogProvider logProvider,
            IPerformanceLogger performanceLogger,
            IConfiguration configuration,
            IApprovalDetailProvider approvalDetailProvider,
            IIntelligenceHelper intelligenceHelper,
            IApprovalTenantInfoHelper approvalTenantInfoHelper)
        {
            _logProvider = logProvider;
            _performanceLogger = performanceLogger;
            _config = configuration;
            _approvalDetailProvider = approvalDetailProvider;
            _intelligenceHelper = intelligenceHelper;
            _approvalTenantInfoHelper = approvalTenantInfoHelper;
        }

        /// <summary>
        /// Generates AI-driven summary and attachment insights for a request and stores the result.
        /// </summary>
        /// <param name="documentNumber">Approval document number.</param>
        /// <param name="tenantInfo">Tenant information owning the request.</param>
        /// <param name="messageId">Optional correlation/message identifier.</param>
        /// <returns>Populated <see cref="RequestSummaryData"/> describing summary and insights.</returns>
        /// <exception cref="Exception">Propagates underlying exceptions after logging; caller may wrap.</exception>
        public async Task<RequestSummaryData> GenerateAIAnalysisAsync(string documentNumber, ApprovalTenantInfo tenantInfo, string messageId = "")
        {
            var logData = new Dictionary<LogDataKey, object>
            {
                { LogDataKey.StartDateTime, DateTime.UtcNow },
                { LogDataKey.DocumentNumber, documentNumber },
                { LogDataKey.MessageId, messageId }
            };
            _logProvider.LogInformation(TrackingEvent.GetAIAnalysisInitiated, logData);

            try
            {
                using (_performanceLogger.StartPerformanceLogger("PerfLog", Constants.WebClient, string.Format(Constants.PerfLogAction, nameof(AIAnalysisHelper), nameof(GenerateAIAnalysisAsync)), logData))
                {
                    var (detailsJson, ocrJson) = await FetchDetailsAndOcrAsync(tenantInfo.TenantId, documentNumber);
                    var (messages, options, modelName) = BuildMessagesAndOptions(detailsJson, ocrJson);
                    var requestSummaryData = await ExecuteAnalysisAsync(messages, options, modelName);
                    await UploadAIAnalysisAsync(documentNumber, requestSummaryData, tenantInfo);
                    _logProvider.LogInformation(TrackingEvent.GetAIAnalysisSuccess, logData);
                    return requestSummaryData;
                }
            }
            catch (Exception ex)
            {
                logData.Modify(LogDataKey.EndDateTime, DateTime.UtcNow);
                _logProvider.LogError(TrackingEvent.GetAIAnalysisFailed, ex, logData);
                throw;
            }
        }

        /// <summary>
        /// Deserializes model JSON response into <see cref="RequestSummaryData"/> (case-insensitive property names).
        /// </summary>
        private static RequestSummaryData ConvertToRequestSummaryData(string chatCompletionJson)
        {
            var result = JsonSerializer.Deserialize<RequestSummaryData>(chatCompletionJson, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

            // Defense-in-depth: filter out attachment insights with null/empty Id to prevent UI card crashes
            if (result?.AttachmentInsights != null)
            {
                result.AttachmentInsights = result.AttachmentInsights
                    .Where(a => a != null && !string.IsNullOrWhiteSpace(a.Id))
                    .ToList();
            }

            return result;
        }

        /// <summary>
        /// Builds system + user messages and completion options (including JSON schema response format).
        /// </summary>
        private (List<ChatMessage> messages, ChatCompletionOptions options, string modelName) BuildMessagesAndOptions(string detailsJson, string ocrJson)
        {
            string systemMessage = _config[ConfigurationKey.AIAnalysisSystemMessage.ToString()];
            string outputSchema = _config[ConfigurationKey.AISummarizationAndInsightsSchema.ToString()];

            var options = _intelligenceHelper.CreateOptionsFromConfig(ConfigurationKey.AIAnalysisCompletionOptions.ToString());
            options.ResponseFormat = ChatResponseFormat.CreateJsonSchemaFormat("AIAnalysis", BinaryData.FromString(outputSchema));

            var messages = _intelligenceHelper.BuildMessages(
                systemMessage + $"\n\n This is the list of OCR-processed attachments in JSON format: {ocrJson}\n This is the JSON object representing the Approval request: {detailsJson}\n\n",
                "Generate a list of Attachment Insights and a Request Summary with the provided information. Attachment insights should be concise,2-3 well structured sentences max with only the most important information. The request summary should also focus on summarizing key information and be as concise as possible.");

            string modelName = _config[ConfigurationKey.AIAnalysisModelName.ToString()];
            return (messages, options, modelName);
        }

        /// <summary>
        /// Executes the AI model completion and converts structured response JSON to domain object.
        /// </summary>
        private async Task<RequestSummaryData> ExecuteAnalysisAsync(List<ChatMessage> messages, ChatCompletionOptions options, string modelName)
        {
            var completion = await _intelligenceHelper.CompleteAsync(modelName, messages, options);
            var chatCompletionJson = completion?.Content?.Count > 0 ? completion.Content[0].Text : null;
            return ConvertToRequestSummaryData(chatCompletionJson);
        }

        /// <summary>
        /// Fetches raw approval details and OCR attachment JSON for the specified request.
        /// </summary>
        private async Task<(string detailsJson, string ocrJson)> FetchDetailsAndOcrAsync(int tenantId, string documentNumber)
        {
            string details = string.Empty;
            string ocrJSONData = string.Empty;

            var requestDetails = await _approvalDetailProvider.GetAllApprovalDetailsByTenantAndDocumentNumber(tenantId, documentNumber);
            string docTypeId = _approvalTenantInfoHelper.GetTenantInfo(tenantId)?.DocTypeId;
            var ocrOperationTypeNew = string.Format(Constants.OcrOperationTypeNew, docTypeId);
            var aiAnalysisOperationTypeNew = string.Format(Constants.AIAnalysisOperationTypeNew, docTypeId);
            if (requestDetails != null)
            {
                foreach (var detailsRow in requestDetails)
                {
                    var isOcrRow = detailsRow.RowKey.Equals(Constants.OcrOperationType, StringComparison.InvariantCultureIgnoreCase)
                        || detailsRow.RowKey.Equals(ocrOperationTypeNew, StringComparison.InvariantCultureIgnoreCase);
                    var isAIRow = detailsRow.RowKey.Equals(Constants.AIAnalysisOperationType, StringComparison.InvariantCultureIgnoreCase)
                        || detailsRow.RowKey.Equals(aiAnalysisOperationTypeNew, StringComparison.InvariantCultureIgnoreCase);

                    if (!isOcrRow && !isAIRow)
                    {
                        var row = JsonSerializer.Deserialize<dynamic>(detailsRow.JSONData);
                        details += row;
                    }
                    else if (isOcrRow && !string.IsNullOrWhiteSpace(detailsRow.JSONData))
                    {
                        ocrJSONData = detailsRow.JSONData;
                    }
                }
            }
            return (details, ocrJSONData);
        }

        /// <summary>
        /// Persists AI analysis results as an approval details entity for later retrieval.
        /// </summary>
        private async Task UploadAIAnalysisAsync(string documentNumber, RequestSummaryData data, ApprovalTenantInfo tenantInfo)
        {
            // Get all approval details data and check if it has row key = AI or AI|doctypeid
            var allApprovalDetails = await _approvalDetailProvider.GetAllApprovalDetailsByTenantAndDocumentNumber(tenantInfo.TenantId, documentNumber);
            var AIAnalysisOperationRowFromDetails = allApprovalDetails?.FirstOrDefault(detail =>
                detail.RowKey.Equals(Constants.AIAnalysisOperationType, StringComparison.InvariantCultureIgnoreCase) ||
                detail.RowKey.Equals(string.Format(Constants.AIAnalysisOperationTypeNew, tenantInfo.DocTypeId), StringComparison.InvariantCultureIgnoreCase));

            var json = JsonSerializer.Serialize(data);
            var approvalDetails = new ApprovalDetailsEntity
            {
                PartitionKey = documentNumber,
                RowKey = AIAnalysisOperationRowFromDetails?.RowKey ?? string.Format(Constants.AIAnalysisOperationTypeNew, tenantInfo.DocTypeId),
                ETag = ETag.All,
                JSONData = json,
                TenantID = tenantInfo.TenantId
            };
            await _approvalDetailProvider.AddApprovalsDetails(new List<ApprovalDetailsEntity> { approvalDetails }, tenantInfo, string.Empty, string.Empty, string.Empty);
        }
    }
}