// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Threading.Tasks;
using Microsoft.CFS.Approvals.Contracts;
using Microsoft.CFS.Approvals.Contracts.DataContracts;
using Microsoft.CFS.Approvals.Model;
using OpenAI.Chat;

namespace Microsoft.CFS.Approvals.Core.BL.Interface
{
    /// <summary>
    /// Contract for a chat tool handler. Implementations encapsulate logic for a single tool
    /// (function) invoked by the LLM (e.g., GetRequestDetails). They should be stateless and
    /// registered with DI.
    /// </summary>
    public interface IChatToolHandler
    {
        /// <summary>
        /// Gets the unique tool (function) name exposed to the model.
        /// </summary>
        string Name { get; }

        /// <summary>
        /// Executes the tool logic and returns a structured result.
        /// </summary>
        /// <param name="toolCall">Tool call (includes arguments JSON).</param>
        /// <param name="context">Aggregated execution context (users, auth token, environment).</param>
        /// <returns>ToolResult containing content and optional suggestions.</returns>
        Task<ToolResult> HandleAsync(ChatToolCall toolCall, ToolContext context);
    }

    /// <summary>
    /// Immutable execution context passed to tool handlers.
    /// Augments model arguments with user identities, auth token and environment values.
    /// </summary>
    /// <param name="ChatRequestContext">Context with prior request / details / attachments metadata.</param>
    /// <param name="UserPrompt">Original user prompt text.</param>
    /// <param name="SignedInUser">Currently signed-in user.</param>
    /// <param name="OnBehalfUser">User on whose behalf actions are performed.</param>
    /// <param name="OAuth2UserToken">User OAuth2 token for downstream calls.</param>
    /// <param name="Domain">User domain extracted from UPN.</param>
    /// <param name="Host">Client device / host identifier.</param>
    /// <param name="AskRequest">Original ask request containing chat history.</param>
    public sealed record ToolContext(
        ChatRequestEventArgs ChatRequestContext,
        string UserPrompt,
        User SignedInUser,
        User OnBehalfUser,
        string OAuth2UserToken,
        string Domain,
        string Host,
        AskRequest AskRequest
    );

    /// <summary>
    /// Standardized tool result returned to the orchestration layer.
    /// </summary>
    /// <param name="Content">Primary content (plain text or serialized data).</param>
    /// <param name="MessageType">Message type (Message or AdaptiveCard).</param>
    /// <param name="PromptSuggestions">Optional follow-up prompt suggestions.</param>
    public sealed record ToolResult(string Content, string MessageType, string[] PromptSuggestions = null)
    {
        /// <summary>
        /// Creates a text (Message) result with optional suggestions.
        /// </summary>
        /// <param name="content">Textual content.</param>
        /// <param name="suggestions">Optional prompt suggestions.</param>
        /// <returns>ToolResult instance.</returns>
        public static ToolResult Text(string content, string[] suggestions = null) => new(content, CopilotMessageType.Message, suggestions);

        /// <summary>
        /// Creates an adaptive card (AdaptiveCard) result.
        /// </summary>
        /// <param name="content">Adaptive card JSON payload.</param>
        /// <returns>ToolResult instance.</returns>
        public static ToolResult Card(string content) => new(content, CopilotMessageType.AdaptiveCard, null);
    }
}