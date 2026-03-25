namespace Microsoft.CFS.Approvals.Core.BL.Helpers.ChatToolHandlers;

using System;
using System.Collections.Generic;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.CFS.Approvals.Common.DL.Interface;
using Microsoft.CFS.Approvals.Contracts;
using Microsoft.CFS.Approvals.Core.BL.Interface;
using Microsoft.CFS.Approvals.Extensions;
using Microsoft.CFS.Approvals.LogManager.Model;
using Microsoft.CFS.Approvals.LogManager.Provider.Interface;
using Microsoft.CFS.Approvals.Model;
using Newtonsoft.Json.Linq;
using OpenAI.Chat;

/// <summary>
/// Chat tool handler that returns request details (authsum + attachments) for a specific document number.
/// If <c>tenantId</c> is not provided it attempts to resolve it via the search helper.
/// </summary>
public class RequestDetailsToolHandler : IChatToolHandler
{
    private readonly IDetailsHelper _detailsHelper;
    private readonly ILogProvider _logProvider;
    private readonly ISearchHelper _searchHelper;
    private readonly IAdaptiveCardResponseHelper _adaptiveCardResponseHelper;
    private readonly IAdaptiveDetailsHelper _adaptiveDetailsHelper;
    private readonly IFlightingDataProvider _flightingDataProvider;

    /// <summary>
    /// Initializes a new instance of the <see cref="RequestDetailsToolHandler"/> class.
    /// </summary>
    public RequestDetailsToolHandler(
        IDetailsHelper detailsHelper,
        ILogProvider logProvider,
        ISearchHelper searchHelper,
        IAdaptiveCardResponseHelper adaptiveCardResponseHelper,
        IAdaptiveDetailsHelper adaptiveDetailsHelper,
        IFlightingDataProvider flightingDataProvider)
    {
        _detailsHelper = detailsHelper;
        _logProvider = logProvider;
        _searchHelper = searchHelper;
        _adaptiveCardResponseHelper = adaptiveCardResponseHelper;
        _adaptiveDetailsHelper = adaptiveDetailsHelper;
        _flightingDataProvider = flightingDataProvider;
    }

    /// <summary>
    /// Gets the tool name (function identifier) for this handler.
    /// </summary>
    public string Name => nameof(ChatToolFunctionNames.GetRequestDetails);

    /// <summary>
    /// Handles the request details tool call, performing tenant resolution, details fetch and attachment analysis.
    /// </summary>
    public async Task<ToolResult> HandleAsync(ChatToolCall toolCall, ToolContext toolContext)
    {
        var logData = new Dictionary<LogDataKey, object>
        {
            { LogDataKey.ToolName, Name },
            { LogDataKey.UserAlias, toolContext.OnBehalfUser?.MailNickname },
            { LogDataKey.StartDateTime, DateTime.UtcNow }
        };

        try
        {
            _logProvider.LogInformation(TrackingEvent.GetRequestDetailsToolInitiated, logData);

            var (documentNumber, tenantId, shouldReturnAdaptiveCard) = ParseToolArguments(toolCall, toolContext);

            // Gate adaptive card response behind flighting
            if (shouldReturnAdaptiveCard
                && !_flightingDataProvider.IsFeatureEnabledForUser(toolContext.SignedInUser?.UserPrincipalName, (int)FlightingFeatureName.AdaptiveCardCopilot))
            {
                shouldReturnAdaptiveCard = false;
            }

            var validationResult = await ValidateAndResolveParametersAsync(documentNumber, tenantId, toolContext);
            if (!validationResult.IsValid)
            {
                return validationResult.Error;
            }

            documentNumber = validationResult.DocumentNumber;
            tenantId = validationResult.TenantId;

            var result = shouldReturnAdaptiveCard
                ? await BuildAdaptiveCardResultAsync(documentNumber, tenantId, toolContext)
                : await BuildTextResultAsync(documentNumber, tenantId, toolContext);

            logData.Add(LogDataKey.DocumentNumber, documentNumber);
            logData.Add(LogDataKey.TenantId, tenantId);
            _logProvider.LogInformation(TrackingEvent.GetRequestDetailsToolSuccess, logData);

            return result;
        }
        catch (Exception ex)
        {
            _logProvider.LogError(TrackingEvent.GetRequestDetailsToolFailed, ex, logData);
            return ToolResult.Text("There was an issue using this tool");
        }
    }

    /// <summary>
    /// Builds an adaptive card result by fetching details and template in parallel.
    /// </summary>
    private async Task<ToolResult> BuildAdaptiveCardResultAsync(string documentNumber, int tenantId, ToolContext toolContext)
    {
        var detailsTask = FetchDetailsAsync(documentNumber, tenantId, toolContext);
        var templateTask = FetchTemplateAsync(tenantId, toolContext);

        await Task.WhenAll(detailsTask, templateTask);

        var details = await detailsTask;
        var templateDict = await templateTask;

        if (!templateDict.TryGetValue("FULL", out var template) || template == null)
        {
            return ToolResult.Text(JsonSerializer.Serialize(new { error = "Adaptive card template not available for this tenant." }));
        }

        var adaptiveCard = GenerateAdaptiveCard(details, template, documentNumber, tenantId, toolContext);
        return ToolResult.Card(adaptiveCard.ToString());
    }

    /// <summary>
    /// Builds a text/JSON result with details and attachment info.
    /// </summary>
    private async Task<ToolResult> BuildTextResultAsync(string documentNumber, int tenantId, ToolContext toolContext)
    {
        var details = await FetchDetailsAsync(documentNumber, tenantId, toolContext);
        var (requestDetailsAvailable, attachmentInfo) = ExtractAttachmentInfo(details);
        var payload = BuildSuccessPayload(documentNumber, tenantId, details, requestDetailsAvailable, attachmentInfo);
        return ToolResult.Text(JsonSerializer.Serialize(payload));
    }

    /// <summary>
    /// Fetches request details (authsum operation) for the specified tenant and document.
    /// </summary>
    private async Task<JObject> FetchDetailsAsync(string documentNumber, int tenantId, ToolContext toolContext)
    {
        return await _detailsHelper.GetDetails(
            tenantId,
            documentNumber,
            operation: "authsum",
            fiscalYear: string.Empty,
            page: 1,
            sessionId: string.Empty,
            tcv: string.Empty,
            xcv: string.Empty,
            userAlias: toolContext.OnBehalfUser.MailNickname,
            loggedInUpn: toolContext.SignedInUser.UserPrincipalName,
            clientDevice: toolContext.Host,
            oauth2UserToken: toolContext.OAuth2UserToken,
            isWorkerTriggered: false,
            sectionType: (int)DataCallType.All,
            pageType: string.Empty,
            source: "Copilot",
            objectId: toolContext.OnBehalfUser.Id,
            domain: toolContext.OnBehalfUser.UserPrincipalName.GetDomainFromUPN());
    }

    /// <summary>
    /// Fetches the adaptive card template for the specified tenant.
    /// </summary>
    private Task<Dictionary<string, JObject>> FetchTemplateAsync(int tenantId, ToolContext toolContext)
    {
        return _adaptiveDetailsHelper.GetAdaptiveTemplate(
            tenantId,
            toolContext.OnBehalfUser.MailNickname,
            toolContext.SignedInUser.UserPrincipalName,
            Constants.FinanceAssistantClient,
            toolContext.OAuth2UserToken,
            sessionId: string.Empty,
            xcv: string.Empty,
            tcv: string.Empty,
            templateType: (int)TemplateType.Full,
            objectId: toolContext.OnBehalfUser.Id,
            domain: toolContext.Domain);
    }

    /// <summary>
    /// Generates a populated adaptive card using a pre-fetched template.
    /// </summary>
    private JObject GenerateAdaptiveCard(JObject details, JObject template, string documentNumber, int tenantId, ToolContext toolContext)
    {
        var approverDisplayName = toolContext.SignedInUser?.Name ?? toolContext.SignedInUser?.UserPrincipalName ?? string.Empty;
        var tcv = Guid.NewGuid().ToString();

        return _adaptiveCardResponseHelper.CreateApprovalAssistantRequestCard(
            tenantId,
            documentNumber,
            details,
            template,
            toolContext.OnBehalfUser.MailNickname,
            toolContext.SignedInUser.UserPrincipalName,
            toolContext.OAuth2UserToken,
            toolContext.OnBehalfUser.Id,
            toolContext.Domain,
            approverDisplayName,
            tcv);
    }

    /// <summary>
    /// Validates required parameters and resolves tenantId if needed.
    /// </summary>
    private async Task<ValidationResult> ValidateAndResolveParametersAsync(string documentNumber, int tenantId, ToolContext toolContext)
    {
        if (string.IsNullOrWhiteSpace(documentNumber))
        {
            return ValidationResult.Failed(
                ToolResult.Text(JsonSerializer.Serialize(new { error = "Invalid or missing documentNumber." })));
        }

        tenantId = await EnsureTenantIdAsync(tenantId, documentNumber, toolContext);
        if (tenantId <= 0)
        {
            return ValidationResult.Failed(
                ToolResult.Text(JsonSerializer.Serialize(new { error = "There was an issue using this tool" })));
        }

        return ValidationResult.Success(documentNumber, tenantId);
    }

    /// <summary>
    /// Ensures tenant id is available, performing resolution if necessary.
    /// </summary>
    private async Task<int> EnsureTenantIdAsync(int tenantId, string documentNumber, ToolContext context)
    {
        if (tenantId > 0)
        {
            if (context.ChatRequestContext != null && context.ChatRequestContext.TenantId <= 0)
            {
                context.ChatRequestContext.TenantId = tenantId;
            }
            return tenantId;
        }

        if (context.ChatRequestContext != null && string.IsNullOrWhiteSpace(context.ChatRequestContext.DocumentNumber))
        {
            context.ChatRequestContext.DocumentNumber = documentNumber;
        }

        var resolved = await ResolveTenantAsync(context);
        if (!resolved)
        {
            return tenantId;
        }

        return context.ChatRequestContext?.TenantId ?? tenantId;
    }

    /// <summary>
    /// Attempts tenant id resolution via search helper if not supplied.
    /// </summary>
    private async Task<bool> ResolveTenantAsync(ToolContext toolContext)
    {
        var chatRequestContext = toolContext.ChatRequestContext;
        var tenantId = await _searchHelper.FindTenantIdByDocumentNumberAsync(
            chatRequestContext.DocumentNumber,
            toolContext.SignedInUser,
            toolContext.OnBehalfUser,
            toolContext.Host,
            toolContext.Domain,
            toolContext.OAuth2UserToken);

        if (tenantId.HasValue && tenantId.Value > 0)
        {
            chatRequestContext.TenantId = tenantId.Value;
            return true;
        }
        return false;
    }

    /// <summary>
    /// Parses tool call function arguments, returning effective documentNumber, tenantId, and shouldReturnAdaptiveCard.
    /// Falls back to existing context values on parse errors.
    /// </summary>
    private (string documentNumber, int tenantId, bool shouldReturnAdaptiveCard) ParseToolArguments(ChatToolCall toolCall, ToolContext context)
    {
        string documentNumber = context.ChatRequestContext?.DocumentNumber;
        int tenantId = context.ChatRequestContext?.TenantId ?? 0;
        bool shouldReturnAdaptiveCard = false;

        var rawArgs = toolCall.FunctionArguments?.ToString();
        if (string.IsNullOrWhiteSpace(rawArgs) || !rawArgs.TrimStart().StartsWith("{"))
        {
            return (documentNumber, tenantId, shouldReturnAdaptiveCard);
        }

        var args = JsonSerializer.Deserialize<RequestDetailsArguments>(rawArgs, new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        });

        if (args is null)
        {
            return (documentNumber, tenantId, shouldReturnAdaptiveCard);
        }

        if (!string.IsNullOrWhiteSpace(args.DocumentNumber))
        {
            documentNumber = args.DocumentNumber.Trim();
        }

        if (tenantId <= 0 && args.TenantId.HasValue && args.TenantId.Value > 0)
        {
            tenantId = args.TenantId.Value;
        }

        if (args.ShouldReturnAdaptiveCard.HasValue)
        {
            shouldReturnAdaptiveCard = args.ShouldReturnAdaptiveCard.Value;
        }

        return (documentNumber, tenantId, shouldReturnAdaptiveCard);
    }

    private (bool requestDetailsAvailable, AttachmentInfo attachmentInfo) ExtractAttachmentInfo(JObject details)
    {
        bool requestDetailsAvailable = details != null && details.HasValues;
        var attachmentInfo = AnalyzeAttachments(details?["Attachments"]);
        return (requestDetailsAvailable, attachmentInfo);
    }

    private static AttachmentInfo AnalyzeAttachments(JToken attachmentsToken)
    {
        if (attachmentsToken == null)
        {
            return new AttachmentInfo("None", "No attachments were provided for this request.", null);
        }
        if (!attachmentsToken.HasValues)
        {
            return new AttachmentInfo("Empty", "No readable attachment information was found (attachments list is empty).", null);
        }
        return new AttachmentInfo("Loaded", null, attachmentsToken.ToString());
    }

    private static object BuildSuccessPayload(string documentNumber, int tenantId, JObject details, bool requestDetailsAvailable, AttachmentInfo attachmentInfo) => new
    {
        documentNumber,
        tenantId,
        detailsData = requestDetailsAvailable ? details.ToString() : null,
        requestDetailsStatus = requestDetailsAvailable ? "Loaded" : "Unavailable",
        requestDetailsMessage = requestDetailsAvailable ? null : "Request details could not be loaded.",
        attachmentData = attachmentInfo.Data,
        attachmentStatus = attachmentInfo.Status,
        attachmentMessage = attachmentInfo.Message
    };

    private readonly record struct AttachmentInfo(string Status, string Message, string Data);

    private readonly record struct ValidationResult(bool IsValid, string DocumentNumber, int TenantId, ToolResult Error)
    {
        public static ValidationResult Success(string documentNumber, int tenantId) =>
            new(true, documentNumber, tenantId, null);

        public static ValidationResult Failed(ToolResult error) =>
            new(false, null, 0, error);
    }

    private class RequestDetailsArguments
    {
        public string DocumentNumber { get; set; }
        public int? TenantId { get; set; }
        public bool? ShouldReturnAdaptiveCard { get; set; }
    }
}