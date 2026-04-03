// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.CFS.Approvals.Common.DL.Interface;
using Microsoft.CFS.Approvals.Contracts;
using Microsoft.CFS.Approvals.Contracts.DataContracts;
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
    /// Orchestrates Approvals plugin chat flow: builds context, executes tool calls iteratively, and returns a structured <see cref="PluginResponse"/>.
    /// Maintains a cached lookup for tool handlers to minimize per-request allocations.
    /// </summary>
    public class ApprovalsPluginHelper : IApprovalsPluginHelper
    {
        #region Variables

        private readonly IIntelligenceHelper _intelligenceHelper;
        private readonly IConfiguration _config;
        private readonly IDelegationHelper _delegationHelper;
        private readonly ILogProvider _logProvider;
        private readonly Dictionary<string, IChatToolHandler> _handlerLookup;
        private readonly IPerformanceLogger _performanceLogger;

        #endregion Variables

        #region Constructor

        /// <summary>
        /// Initializes a new instance of the <see cref="ApprovalsPluginHelper"/> class.
        /// </summary>
        /// <param name="config">Configuration abstraction for system messages and options.</param>
        /// <param name="intelligenceHelper">Helper for building messages and executing chat completions.</param>
        /// <param name="delegationHelper">Delegation/authorization helper.</param>
        /// <param name="toolHandlers">Registered tool handlers exposed to the model.</param>
        /// <param name="logProvider">Centralized logging provider.</param>
        /// <param name="performanceLogger">Performance logger for timing operations.</param>
        public ApprovalsPluginHelper(
            IConfiguration config,
            IIntelligenceHelper intelligenceHelper,
            IDelegationHelper delegationHelper,
            IEnumerable<IChatToolHandler> toolHandlers,
            ILogProvider logProvider,
            IPerformanceLogger performanceLogger)
        {
            _config = config;
            _intelligenceHelper = intelligenceHelper;
            _delegationHelper = delegationHelper;
            _logProvider = logProvider;
            _performanceLogger = performanceLogger;

            // Build a unique-name lookup (assuming service registration prevents duplicates).
            _handlerLookup = toolHandlers.ToDictionary(h => h.Name, h => h, StringComparer.OrdinalIgnoreCase);
        }

        #endregion Constructor

        #region Public Methods

        /// <summary>
        /// Executes the approvals plugin interaction loop for a single user prompt.
        /// Performs authorization, builds contextual system + user messages, executes tool call iterations, and returns the final plugin response.
        /// </summary>
        /// <param name="signedInUser">The signed-in user initiating the request.</param>
        /// <param name="onBehalfUser">Target user if acting on behalf (may differ from signed-in).</param>
        /// <param name="oauth2UserToken">OAuth token for downstream service access.</param>
        /// <param name="clientDevice">Client device identifier for telemetry.</param>
        /// <param name="askRequest">Incoming chat request payload (user input + history + additional details).</param>
        /// <param name="tcv">Transaction correlation vector.</param>
        /// <param name="xcv">Extended correlation vector.</param>
        /// <returns>Structured <see cref="PluginResponse"/> (message, type, follow-up prompts or adaptive card).</returns>
        public async Task<PluginResponse> GetApprovalsPluginCompletionAsync(User signedInUser, User onBehalfUser, string oauth2UserToken, string clientDevice, AskRequest askRequest, string tcv, string xcv)
        {
            #region Logging initiation event

            var LogData = new Dictionary<LogDataKey, object>
            {
                { LogDataKey.ClientDevice, clientDevice },
                { LogDataKey.UserAlias, signedInUser?.Alias ?? string.Empty },
                { LogDataKey.AskRequest, askRequest }
            };

            _logProvider.LogInformation(TrackingEvent.ApprovalsPluginInitiated, LogData);

            #endregion Logging initiation event

            using (_performanceLogger.StartPerformanceLogger("PerfLog", Constants.WebClient, string.Format(Constants.PerfLogAction, nameof(ApprovalsPluginHelper), nameof(GetApprovalsPluginCompletionAsync)), LogData))
            {
                try
                {
                    await _delegationHelper.CheckUserAuthorization(signedInUser, onBehalfUser, oauth2UserToken, clientDevice, "", xcv, tcv);

                    var contextArgs = ExtractContext(askRequest.AdditionalDetails, signedInUser.MailNickname);

                    // Null contextArgs indicate the user has started a new chat
                    if (contextArgs == null)
                    {
                        var response = NewChatPromptSuggestions();

                        #region Logging Success event

                        LogData[LogDataKey.CopilotResponse] = response;
                        _logProvider.LogInformation(TrackingEvent.ApprovalsPluginSuccess, LogData);

                        #endregion Logging Success event

                        return response;
                    }

                    var (approvalsContext, messages) = BuildContextAndMessages(contextArgs, askRequest);
                    PluginResponse finalResponse = await ExecutePluginChatFlowAsync(signedInUser, onBehalfUser, oauth2UserToken, clientDevice, contextArgs, approvalsContext, messages);

                    #region Logging Success event

                    LogData[LogDataKey.CopilotResponse] = finalResponse;
                    LogData[LogDataKey.UserContext] = contextArgs.UserContext;
                    _logProvider.LogInformation(TrackingEvent.ApprovalsPluginSuccess, LogData);

                    #endregion Logging Success event

                    return finalResponse;
                }
                catch (Exception ex)
                {
                    LogData[LogDataKey.ErrorMessage] = ex.Message;
                    _logProvider.LogError(TrackingEvent.ApprovalsPluginFailed, ex, LogData);
                    throw;
                }
            }
        }

        #endregion Public Methods

        #region Private Methods

        /// <summary>
        /// Runs the chat loop and executes any tools called by the model until completion.
        /// </summary>
        private async Task<PluginResponse> ExecutePluginChatFlowAsync(User signedInUser, User onBehalfUser, string oauth2UserToken, string clientDevice, ChatRequestEventArgs contextArgs, ChatContextParameters approvalsContext, List<ChatMessage> messages)
        {
            var completion = await _intelligenceHelper.CompleteAsync(approvalsContext.ModelDeploymentName, messages, approvalsContext.ChatCompletionOptions);
            string adaptiveCardContent = null;
            if (completion.FinishReason == ChatFinishReason.ToolCalls)
            {
                var toolLoopResult = await HandleToolCallsAsync(approvalsContext, contextArgs, signedInUser, onBehalfUser, oauth2UserToken, clientDevice, approvalsContext.ChatCompletionOptions, messages, completion);
                completion = toolLoopResult.completion;
                adaptiveCardContent = toolLoopResult.adaptiveCardContent;
            }

            PluginResponse finalResponse;
            if (!string.IsNullOrWhiteSpace(adaptiveCardContent))
            {
                finalResponse = SetPluginResponse(adaptiveCardContent, CopilotMessageType.AdaptiveCard);
            }
            else
            {
                finalResponse = ParsePluginResponse(completion);
            }

            return finalResponse;
        }

        #region Context building

        /// <summary>
        /// Extracts and normalizes chat context arguments from additional details dictionary.
        /// </summary>
        private ChatRequestEventArgs ExtractContext(Dictionary<string, string> additionalDetails, string userAlias)
        {
            var chatRequestContextArgs = additionalDetails?.ToJson().FromJson<ChatRequestEventArgs>();
            if (chatRequestContextArgs != null)
            {
                chatRequestContextArgs.UserAlias = userAlias;
                chatRequestContextArgs.UserContext = string.IsNullOrEmpty(chatRequestContextArgs.UserContext) ? nameof(CopilotUserContextType.EXTERNAL) : chatRequestContextArgs.UserContext.ToUpperInvariant();
            }
            return chatRequestContextArgs;
        }

        /// <summary>
        /// Builds context parameters and initial message list from request + history.
        /// </summary>
        private (ChatContextParameters ctx, List<ChatMessage> messages) BuildContextAndMessages(ChatRequestEventArgs contextArgs, AskRequest askRequest)
        {
            var approvalsContext = BuildApprovalsContext(contextArgs, askRequest.Input);
            var messages = _intelligenceHelper.BuildMessages(approvalsContext.SystemMessage, approvalsContext.UserMessage, askRequest.History);
            return (approvalsContext, messages);
        }

        /// <summary>
        /// Creates base context parameters (deployment name + initial user message + options from config) and applies response format.
        /// </summary>
        private ChatContextParameters CreateBaseContextParameters(string userPrompt)
        {
            var options = _intelligenceHelper.CreateOptionsFromConfig(ConfigurationKey.AzureOpenAICompletionsOptions.ToString());
            options.ResponseFormat = ChatResponseFormat.CreateJsonSchemaFormat("pluginResponse", BinaryData.FromString(_config[ConfigurationKey.ApprovalsPluginResponseSchema.ToString()]));
            return new ChatContextParameters
            {
                ChatCompletionOptions = options,
                ModelDeploymentName = _config[ConfigurationKey.AzureOpenAIModelName.ToString()],
                UserMessage = userPrompt
            };
        }

        /// <summary>
        /// Constructs the contextual system message, user message, response schema, and tool set based on user context and error state.
        /// </summary>
        private ChatContextParameters BuildApprovalsContext(ChatRequestEventArgs chatRequestContext, string userPrompt)
        {
            ChatContextParameters chatContextParameters = CreateBaseContextParameters(userPrompt);
            string userContext = ResolveUserContextType(chatRequestContext);

            string baseSystemMessage = BuildBaseApprovalsPluginSystemMessage();
            chatContextParameters.SystemMessage = $"{baseSystemMessage} \r\n\r\n Context: {userContext}";

            if (chatRequestContext.CopilotErrorType == CopilotErrorType.OutOfSync)
            {
                chatContextParameters.SystemMessage += $" \n\n OutOfSync detected for document {chatRequestContext.DocumentNumber}.";
                chatContextParameters.ChatCompletionOptions.Tools.Clear();
                if (chatRequestContext.UserConsent == ActionConsentType.Yes)
                {
                    chatContextParameters.UserMessage = "Mark the request out of sync.";
                    chatContextParameters.ChatCompletionOptions.Tools.Add(ChatTools.ErrorHandlerTool);
                }
                else
                {
                    chatContextParameters.UserMessage = "Help me troubleshoot this out of sync request";
                    chatContextParameters.ChatCompletionOptions.Tools.Add(ChatTools.ExplainAndAskPermissionTool);
                }
                return chatContextParameters;
            }

            switch (userContext)
            {
                case nameof(CopilotUserContextType.APPROVALS):
                    if (!string.IsNullOrWhiteSpace(chatRequestContext.DocumentNumber))
                    {
                        chatContextParameters.SystemMessage += $" \n\n The user currently has the request with this document number open: {chatRequestContext.DocumentNumber}";
                    }
                    if (!string.IsNullOrWhiteSpace(chatRequestContext.DetailsData))
                    {
                        chatContextParameters.SystemMessage += $" \n\n These are the Request details: {chatRequestContext.DetailsData}";
                    }
                    if (!string.IsNullOrWhiteSpace(chatRequestContext.AttachmentData))
                    {
                        chatContextParameters.SystemMessage += $" \n\n These are the Attachment details: {chatRequestContext.AttachmentData}";
                    }
                    AddStandardTools(chatContextParameters.ChatCompletionOptions);
                    chatContextParameters.ChatCompletionOptions.Tools.Add(ChatTools.ExplainAndAskPermissionTool);
                    break;

                case nameof(CopilotUserContextType.ERROR):
                    chatContextParameters.UserMessage = "Handle the error";
                    chatContextParameters.ChatCompletionOptions.Tools.Add(ChatTools.ErrorHandlerTool);
                    break;

                default:
                    chatContextParameters.SystemMessage += " \r\n\r\n Context source: External";
                    AddStandardTools(chatContextParameters.ChatCompletionOptions);
                    break;
            }
            return chatContextParameters;
        }

        private static string ResolveUserContextType(ChatRequestEventArgs chatRequestContext)
        {
            var userContext = chatRequestContext.UserContext;
            if (userContext == nameof(CopilotUserContextType.DETAILS) ||
                userContext == nameof(CopilotUserContextType.ATTACHMENT) ||
                userContext == nameof(CopilotUserContextType.SUMMARY))
            {
                userContext = nameof(CopilotUserContextType.APPROVALS);
            }

            return userContext;
        }

        /// <summary>
        /// Build the Approvals plugin system message from configuration.
        /// </summary>
        private string BuildBaseApprovalsPluginSystemMessage()
        {
            string systemMessage = _config[ConfigurationKey.ApprovalsPluginSystemMessageScope.ToString()] + "\r\n\r\n" 
                + _config[ConfigurationKey.ApprovalsPluginSystemMessageTools.ToString()] + "\r\n\r\n"
                + _config[ConfigurationKey.ApprovalsPluginSystemMessageFormatting.ToString()] + "\r\n\r\n"
                + _config[ConfigurationKey.ApprovalsPluginSystemMessageResponseBehavior.ToString()] + "\r\n\r\n";
            return systemMessage;
        }

        /// <summary>
        /// Adds standard search/request detail tools to the options collection.
        /// </summary>
        private void AddStandardTools(ChatCompletionOptions options)
        {
            options.Tools.Add(ChatTools.SearchTool);
            options.Tools.Add(ChatTools.GetRequestDetailsTool);
        }

        #endregion Context building

        #region Tool Execution Loop

        /// <summary>
        /// Iteratively handles model tool calls until a non-tool completion is produced, an adaptive card is generated, or max iterations reached.
        /// </summary>
        private async Task<(ChatCompletion completion, string adaptiveCardContent)> HandleToolCallsAsync(ChatContextParameters chatContextParameters, ChatRequestEventArgs chatRequestContextArgs, User signedInUser, User onBehalfUser, string oauth2UserToken, string clientDevice, ChatCompletionOptions options, List<ChatMessage> conversationMessages, ChatCompletion chatCompletionResult)
        {
            string domain = onBehalfUser.UserPrincipalName.GetDomainFromUPN();
            string host = clientDevice;
            int iteration = 0;
            const int maxIterations = 8; // Consider externalizing to configuration for tuning.
            string adaptiveCardContent = null;
            conversationMessages.Add(new AssistantChatMessage(chatCompletionResult));
            bool breakLoop = false;
            while (chatCompletionResult.FinishReason == ChatFinishReason.ToolCalls && iteration < maxIterations && !breakLoop)
            {
                iteration++;
                foreach (var toolCall in chatCompletionResult.ToolCalls)
                {
                    var toolPerfData = new Dictionary<LogDataKey, object>
                    {
                        { LogDataKey.EventName, toolCall.FunctionName },
                        { LogDataKey.ClientDevice, host }
                    };
                    ToolResult toolResult;
                    using (_performanceLogger.StartPerformanceLogger("PerfLog", Constants.WebClient, string.Format(Constants.PerfLogAction, "ToolExecution", toolCall.FunctionName), toolPerfData))
                    {
                        toolResult = await ExecuteSingleToolCall(toolCall, _handlerLookup, chatRequestContextArgs, chatContextParameters.UserMessage, signedInUser, onBehalfUser, oauth2UserToken, domain, host, null);
                    }

                    if (toolResult.MessageType == CopilotMessageType.AdaptiveCard && string.IsNullOrWhiteSpace(adaptiveCardContent))
                    {
                        adaptiveCardContent = toolResult.Content; // Capture first adaptive card response.
                        breakLoop = true;
                        conversationMessages.Add(new ToolChatMessage(toolCall.Id, toolResult.Content));
                        break;
                    }
                    conversationMessages.Add(new ToolChatMessage(toolCall.Id, toolResult.Content));
                }
                if (breakLoop) break;
                chatCompletionResult = await _intelligenceHelper.CompleteAsync(chatContextParameters.ModelDeploymentName, conversationMessages, options);
                if (chatCompletionResult.FinishReason == ChatFinishReason.ToolCalls)
                {
                    conversationMessages.Add(new AssistantChatMessage(chatCompletionResult));
                }
            }
            if (!breakLoop && chatCompletionResult.FinishReason == ChatFinishReason.ToolCalls && iteration >= maxIterations)
            {
                conversationMessages.Add(new AssistantChatMessage("Tool execution truncated (max iterations reached)."));
                chatCompletionResult = await _intelligenceHelper.CompleteAsync(chatContextParameters.ModelDeploymentName, conversationMessages, options);
            }
            return (chatCompletionResult, adaptiveCardContent);
        }

        /// <summary>
        /// Executes a single tool call, delegating to registered handler or executing built-in search logic.
        /// Returns a tool result (text or adaptive card) and logs failures.
        /// </summary>
        private async Task<ToolResult> ExecuteSingleToolCall(ChatToolCall toolCall, Dictionary<string, IChatToolHandler> handlerLookup, ChatRequestEventArgs ctxArgs, string userPrompt, User signedInUser, User onBehalfUser, string token, string domain, string host, AskRequest askRequest)
        {
            if (handlerLookup.TryGetValue(toolCall.FunctionName, out var handler))
            {
                var toolCtx = new ToolContext(ctxArgs, userPrompt, signedInUser, onBehalfUser, token, domain, host, askRequest);
                var invokeLog = new Dictionary<LogDataKey, object>
                {
                    { LogDataKey.EventName, toolCall.FunctionName },
                    { LogDataKey.ToolName, toolCall.FunctionName },
                    { LogDataKey.UserAlias, signedInUser?.Alias },
                    { LogDataKey.Domain, domain }
                };
                _logProvider.LogInformation(TrackingEvent.CopilotToolInvoked, invokeLog);
                try
                {
                    var result = await handler.HandleAsync(toolCall, toolCtx);
                    _logProvider.LogInformation(TrackingEvent.CopilotToolExecutionSuccess, invokeLog);
                    return result;
                }
                catch (Exception ex)
                {
                    invokeLog[LogDataKey.ErrorMessage] = ex.Message;
                    _logProvider.LogError(TrackingEvent.CopilotToolExecutionFailed, ex, invokeLog);
                    return ToolResult.Text(JsonSerializer.Serialize(new { tool = toolCall.FunctionName, error = "There was an error with this tool" }));
                }
            }
            return ToolResult.Text(JsonSerializer.Serialize(new { tool = toolCall.FunctionName, error = "Handler not found" }));
        }

        #endregion Tool Execution Loop

        #region Plugin Response Helpers

        private PluginResponse NewChatPromptSuggestions()
        {
            var copilotDefaultPrompts = _config[ConfigurationKey.CopilotDefaultPrompts.ToString()];
            string[] promptList = copilotDefaultPrompts.Split(',', StringSplitOptions.RemoveEmptyEntries)
                                                       .Select(p => p.Trim())
                                                       .Where(p => p.Length > 0)
                                                       .ToArray();
            return SetPluginResponse("Get Started with one of the suggested prompts below", CopilotMessageType.Message, promptList);
        }

        private PluginResponse ParsePluginResponse(ChatCompletion completion)
        {
            var rawModelText = completion?.Content?.Count > 0 ? completion.Content[0].Text : string.Empty;
            if (string.IsNullOrWhiteSpace(rawModelText))
            {
                return SetPluginResponse(string.Empty, CopilotMessageType.Message);
            }
            try
            {
                var response = JsonSerializer.Deserialize<PluginResponse>(rawModelText, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
                return response ?? SetPluginResponse(rawModelText, CopilotMessageType.Message);
            }
            catch
            {
                return SetPluginResponse(rawModelText, CopilotMessageType.Message);
            }
        }

        private PluginResponse SetPluginResponse(string message, string messageType, string[] promptQuestions = null)
        {
            return new PluginResponse
            {
                Message = message,
                MessageType = messageType,
                PromptQuestions = promptQuestions ?? Array.Empty<string>()
            };
        }

        #endregion Plugin Response Helpers

        #endregion Private Methods
    }
}