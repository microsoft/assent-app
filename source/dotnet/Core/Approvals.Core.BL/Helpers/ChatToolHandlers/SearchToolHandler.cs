using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.CFS.Approvals.Contracts;
using Microsoft.CFS.Approvals.Core.BL.Interface;
using Microsoft.CFS.Approvals.Extensions;
using Microsoft.CFS.Approvals.LogManager.Model;
using Microsoft.CFS.Approvals.LogManager.Provider.Interface;
using Microsoft.CFS.Approvals.Model;
using OpenAI.Chat;
namespace Microsoft.CFS.Approvals.Core.BL.Helpers.ChatToolHandlers;


/// <summary>
/// Tool handler for GetAIAssistedSearchResults. Minimal implementation: invokes AI-assisted search
/// and returns raw count + JSON serialized results (same format as original inline logic).
/// </summary>
/// <summary>
/// Handler for <see cref="ChatToolFunctionNames.GetAIAssistedSearchResults"/>.
/// Executes AI-assisted search and returns a summary count with serialized results.
/// </summary>
public class SearchToolHandler : IChatToolHandler
{
    private readonly IAIAssistedSearchHelper _aiSearchHelper;
    private readonly ILogProvider _logProvider;

    public SearchToolHandler(IAIAssistedSearchHelper aiSearchHelper, ILogProvider logProvider)
    {
        _aiSearchHelper = aiSearchHelper;
        _logProvider = logProvider;
    }

    public string Name => nameof(ChatToolFunctionNames.GetAIAssistedSearchResults);

    public async Task<ToolResult> HandleAsync(ChatToolCall toolCall, ToolContext context)
    {
        var logCtx = new Dictionary<LogDataKey, object>
        {
            { LogDataKey.ToolName, Name },
            { LogDataKey.SearchText, context.UserPrompt },
            { LogDataKey.UserAlias, context.SignedInUser?.Alias }
        };
        _logProvider.LogInformation(TrackingEvent.SearchToolInitiated, logCtx);
        try
        {
            var results = await _aiSearchHelper.GetAIAssistedSearchResults<ApprovalSummaryData>(
                context.SignedInUser,
                context.OnBehalfUser,
                context.UserPrompt,
                SearchResultReturnType.FullDetails,
                context.Domain,
                context.Host,
                context.OAuth2UserToken);

            logCtx[LogDataKey.SummaryCount] = results?.Count ?? 0;
            _logProvider.LogInformation(TrackingEvent.SearchToolSuccess, logCtx);

            if (results == null)
            {
                return ToolResult.Text("Total number of results: 0 Results: []");
            }

            return ToolResult.Text($"Total number of results: {results.Count} Results: {results.ToJson()}");
        }
        catch (Exception ex)
        {
            _logProvider.LogError(TrackingEvent.SearchToolFailed, ex, logCtx);
            return ToolResult.Text($"There was an issue fetching the requests.");
        }
    }
}
