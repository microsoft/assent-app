// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Azure.AI.OpenAI;
using Microsoft.CFS.Approvals.Model;
using Microsoft.CFS.Approvals.Core.BL.Interface;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json.Linq;
using OpenAI.Chat;
using Microsoft.CFS.Approvals.LogManager.Provider.Interface;
using Microsoft.CFS.Approvals.LogManager.Model;

namespace Microsoft.CFS.Approvals.Core.BL.Helpers
{
    /// <summary>
    /// Helper for constructing chat message sequences and executing Azure OpenAI chat completions.
    /// Encapsulates configuration-driven option creation, client resolution, and defensive validation.
    /// </summary>
    public class IntelligenceHelper : IIntelligenceHelper
    {
        #region Variables
        private readonly AzureOpenAIClient _openAIClient;
        private readonly IConfiguration _config;
        private readonly ILogProvider _logProvider;
        #endregion Variables

        #region Constructor
        /// <summary>
        /// Initializes a new instance of the <see cref="IntelligenceHelper"/> class.
        /// </summary>
        /// <param name="openAIClient">Azure OpenAI client used to obtain chat clients.</param>
        /// <param name="configuration">Configuration source for option construction.</param>
        /// <param name="logProvider">Optional log provider instance</param>
        /// <exception cref="ArgumentNullException">Thrown when any dependency is null.</exception>
        public IntelligenceHelper(AzureOpenAIClient openAIClient, IConfiguration configuration, ILogProvider logProvider = null)
        {
            _openAIClient = openAIClient;
            _config = configuration;
            _logProvider = logProvider;
        }
        #endregion Constructor

        #region Public Methods
        /// <summary>
        /// Builds a list of chat messages combining the system message, optional history, and current user message.
        /// </summary>
        /// <param name="systemMessage">System-level instruction (required).</param>
        /// <param name="userMessage">Current user input (optional).</param>
        /// <param name="history">Prior conversation turns (optional).</param>
        /// <returns>Ordered list of <see cref="ChatMessage"/> instances suitable for completion.</returns>
        /// <exception cref="ArgumentException">Thrown when <paramref name="systemMessage"/> is missing.</exception>
        public List<ChatMessage> BuildMessages(string systemMessage, string userMessage, List<History> history = null)
        {
            if (string.IsNullOrWhiteSpace(systemMessage))
            {
                throw new ArgumentException("System message must be provided", nameof(systemMessage));
            }

            var messages = new List<ChatMessage> { new SystemChatMessage(systemMessage) };

            if (history != null && history.Count > 0)
            {
                foreach (var h in history)
                {
                    if (!string.IsNullOrWhiteSpace(h?.User))
                    {
                        messages.Add(new UserChatMessage(h.User));
                    }
                    if (!string.IsNullOrWhiteSpace(h?.Bot))
                    {
                        var botContent = h.Bot.Contains("adaptivecards.io/schemas/adaptive-card.json", StringComparison.OrdinalIgnoreCase)
                            ? "[An adaptive card was displayed to the user]"
                            : h.Bot;
                        messages.Add(new AssistantChatMessage(botContent));
                    }
                }
            }

            if (!string.IsNullOrWhiteSpace(userMessage))
            {
                messages.Add(new UserChatMessage(userMessage));
            }

            return messages;
        }

        /// <summary>
        /// Creates <see cref="ChatCompletionOptions"/> by parsing a JSON configuration value for the specified key.
        /// </summary>
        /// <param name="configKey">Configuration key containing JSON defining model options.</param>
        /// <returns>Populated <see cref="ChatCompletionOptions"/> instance.</returns>
        /// <exception cref="ArgumentException">Thrown when key is missing.</exception>
        /// <exception cref="InvalidOperationException">Thrown when configuration value cannot be found.</exception>
        public ChatCompletionOptions CreateOptionsFromConfig(string configKey)
        {
            if (string.IsNullOrWhiteSpace(configKey))
            {
                throw new ArgumentException("Config key must be provided", nameof(configKey));
            }

            var configValues = _config[configKey];
            if (string.IsNullOrWhiteSpace(configValues))
            {
                throw new InvalidOperationException($"Configuration value not found for key '{configKey}'");
            }

            var chatCompletionOptionsJObject = JObject.Parse(configValues);
            return CreateOptions(chatCompletionOptionsJObject);
        }

        /// <summary>
        /// Executes a chat completion request against the specified model deployment.
        /// </summary>
        /// <param name="modelDeploymentName">Azure OpenAI deployment name.</param>
        /// <param name="messages">Conversation messages (must contain at least system + one user or history entry).</param>
        /// <param name="options">Completion options (temperature, penalties, etc.).</param>
        /// <returns>The resulting <see cref="ChatCompletion"/>.</returns>
        /// <exception cref="ArgumentException">Thrown when <paramref name="messages"/> is null or empty.</exception>
        public async Task<ChatCompletion> CompleteAsync(string modelDeploymentName, List<ChatMessage> messages, ChatCompletionOptions options)
        {
            if (messages == null || messages.Count == 0)
            {
                throw new ArgumentException("At least one message must be provided", nameof(messages));
            }

            var client = GetChatClient(modelDeploymentName);

            var logData = new Dictionary<LogDataKey, object>
            {
                { LogDataKey.ModelName, modelDeploymentName }
            };
            _logProvider?.LogInformation(TrackingEvent.ChatCompletionInitiated, logData);
            try
            {
                var completion = await client.CompleteChatAsync(messages, options).ConfigureAwait(false);

                // Log finish reason for better error analysis
                if (completion is not null)
                {
                    logData[LogDataKey.ChatFinishReason] = completion.Value.FinishReason.ToString();
                }
                _logProvider?.LogInformation(TrackingEvent.ChatCompletionSuccess, logData);
                return completion;
            }
            catch (Exception ex)
            {
                logData[LogDataKey.ErrorMessage] = ex.Message;
                _logProvider?.LogError(TrackingEvent.ChatCompletionFailed, ex, logData);
                throw;
            }
        }
        #endregion Public Methods

        #region Private Methods
        /// <summary>
        /// Resolves a chat client for the given deployment name.
        /// </summary>
        private ChatClient GetChatClient(string modelDeploymentName)
        {
            if (string.IsNullOrWhiteSpace(modelDeploymentName))
            {
                throw new ArgumentException("Model deployment name must be provided", nameof(modelDeploymentName));
            }
            return _openAIClient.GetChatClient(modelDeploymentName);
        }

        /// <summary>
        /// Creates completion options from a parsed JSON object.
        /// Expected keys: MaxTokens, Temperature, NucleusSamplingFactor (TopP), FrequencyPenalty, PresencePenalty, StoredOutputEnabled.
        /// </summary>
        private static ChatCompletionOptions CreateOptions(JObject j)
        {
            //TODO: See if we can serialize the chat options from Configuration and throw an error if it's not valid
            if (j == null) throw new ArgumentNullException(nameof(j));

            bool storedOutputEnabled = j.Value<bool?>("StoredOutputEnabled") ?? false;
            var options = new ChatCompletionOptions
            {
                MaxOutputTokenCount = (int)j["MaxTokens"],
                Temperature = (float)j["Temperature"],
                TopP = (float)j["NucleusSamplingFactor"],
                FrequencyPenalty = (float)j["FrequencyPenalty"],
                PresencePenalty = (float)j["PresencePenalty"],
                StoredOutputEnabled = storedOutputEnabled
            };
            return options;
        }
        #endregion Private Methods
    }
}