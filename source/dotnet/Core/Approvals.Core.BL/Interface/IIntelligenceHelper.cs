// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Collections.Generic;
using System.Threading.Tasks;
using OpenAI.Chat;
using Microsoft.CFS.Approvals.Model;

namespace Microsoft.CFS.Approvals.Core.BL.Interface;

/// <summary>
/// Minimal chat helper abstraction for constructing messages and executing completions.
/// Additional concerns (client resolution, schema formatting, JSON utilities, logging truncation) are internal.
/// </summary>
public interface IIntelligenceHelper
{
    /// <summary>
    /// Builds a chat message list combining the system prompt, the user input, and optional conversation history.
    /// </summary>
    /// <param name="systemMessage">The system role message guiding chat behavior.</param>
    /// <param name="userMessage">The user's input message.</param>
    /// <param name="history">Optional prior conversation history.</param>
    /// <returns>A list of <see cref="ChatMessage"/> representing the conversation.</returns>
    List<ChatMessage> BuildMessages(string systemMessage, string userMessage, List<History> history = null);

    /// <summary>
    /// Executes a chat completion against the specified model deployment with provided messages and options.
    /// </summary>
    /// <param name="modelDeploymentName">The model deployment identifier.</param>
    /// <param name="messages">The conversation messages.</param>
    /// <param name="options">Completion options including temperature, tools, etc.</param>
    /// <returns>A <see cref="ChatCompletion"/> result.</returns>
    Task<ChatCompletion> CompleteAsync(string modelDeploymentName, List<ChatMessage> messages, ChatCompletionOptions options);

    /// <summary>
    /// Creates chat completion options from configuration keyed by the provided identifier.
    /// </summary>
    /// <param name="configKey">Configuration key used to resolve options.</param>
    /// <returns><see cref="ChatCompletionOptions"/> populated from configuration.</returns>
    ChatCompletionOptions CreateOptionsFromConfig(string configKey);
}
