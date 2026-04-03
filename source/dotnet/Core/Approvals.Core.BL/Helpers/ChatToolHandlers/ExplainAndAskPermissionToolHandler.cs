namespace Microsoft.CFS.Approvals.Core.BL.Helpers.ChatToolHandlers;

using System.Threading.Tasks;
using Microsoft.CFS.Approvals.Contracts;
using Microsoft.CFS.Approvals.Core.BL.Interface;
using Microsoft.CFS.Approvals.Model;
using Microsoft.Extensions.Configuration;
using Microsoft.CFS.Approvals.LogManager.Provider.Interface;
using Microsoft.CFS.Approvals.LogManager.Model;
using OpenAI.Chat;
using System.Collections.Generic;

/// <summary>
/// Handler for the <see cref="ChatToolFunctionNames.ExplainAndAskPermission"/> chat tool.
/// Builds an adaptive card asking the user for consent to mark a request as Out of Sync when applicable.
/// </summary>
public class ExplainAndAskPermissionToolHandler : IChatToolHandler
{
    private readonly IAdaptiveCardResponseHelper _adaptiveCardHelper;
    private readonly IConfiguration _config;
    private readonly ILogProvider _logProvider;

    /// <summary>
    /// Initializes a new instance of the <see cref="ExplainAndAskPermissionToolHandler"/> class.
    /// </summary>
    public ExplainAndAskPermissionToolHandler(IAdaptiveCardResponseHelper adaptiveCardHelper, IConfiguration config, ILogProvider logProvider)
    {
        _adaptiveCardHelper = adaptiveCardHelper;
        _config = config;
        _logProvider = logProvider;
    }

    /// <inheritdoc />
    public string Name => nameof(ChatToolFunctionNames.ExplainAndAskPermission);

    /// <inheritdoc />
    public Task<ToolResult> HandleAsync(ChatToolCall toolCall, ToolContext context)
    {
        var chatRequestContext = context.ChatRequestContext;
        var result = string.Empty;

        var logData = new Dictionary<LogDataKey, object>
        {
            { LogDataKey.ToolName, Name },
            { LogDataKey.UserAlias, context.OnBehalfUser?.MailNickname },
            { LogDataKey.StartDateTime, System.DateTime.UtcNow }
        };

        if (chatRequestContext?.CopilotErrorType == CopilotErrorType.OutOfSync)
        {
            var askReq = context.AskRequest ?? new AskRequest { Input = context.UserPrompt };
            result = _adaptiveCardHelper.CreateTakeActionCard(
                "Would you like us to mark this request as Out of Sync?",
                _config[ConfigurationKey.OutOfSyncExplanation.ToString()],
                askReq,
                chatRequestContext);

            _logProvider.LogInformation(TrackingEvent.ExplainAndAskPermissionToolSuccess, logData);
            return Task.FromResult(ToolResult.Card(result));
        }
        return Task.FromResult(ToolResult.Text(result));
    }
}