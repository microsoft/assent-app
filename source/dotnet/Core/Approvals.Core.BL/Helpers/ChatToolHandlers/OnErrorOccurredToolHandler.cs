using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.CFS.Approvals.Contracts;
using Microsoft.CFS.Approvals.Core.BL.Interface;
using Microsoft.CFS.Approvals.LogManager.Model;
using Microsoft.CFS.Approvals.LogManager.Provider.Interface;
using Microsoft.CFS.Approvals.Model;
using Microsoft.Extensions.Configuration;
using OpenAI.Chat;

namespace Microsoft.CFS.Approvals.Core.BL.Helpers.ChatToolHandlers;

/// <summary>
/// Chat tool handler that notifies the orchestration layer when an error occurs.
/// Delegates to the base event implementation and respects user consent.
/// </summary>
public class OnErrorOccurredToolHandler : Events, IChatToolHandler
{
    private readonly IConfiguration _config;
    private readonly ILogProvider _logProvider;

    /// <summary>
    /// Initializes a new instance of the <see cref="OnErrorOccurredToolHandler"/> class.
    /// </summary>
    public OnErrorOccurredToolHandler(IConfiguration config, ILogProvider logProvider)
    {
        _config = config;
        _logProvider = logProvider;
    }

    /// <inheritdoc />
    public string Name => nameof(ChatToolFunctionNames.OnErrorOccurred);

    /// <inheritdoc />
    public async Task<ToolResult> HandleAsync(ChatToolCall toolCall, ToolContext context)
    {
        var chatRequestContext = context.ChatRequestContext;
        string result;

        // Initialize a single logging context and mutate as needed
        var logData = new Dictionary<LogDataKey, object>
        {
            { LogDataKey.ToolName, Name },
            { LogDataKey.UserAlias, context.OnBehalfUser?.MailNickname },
            { LogDataKey.StartDateTime, System.DateTime.UtcNow }
        };
        _logProvider.LogInformation(TrackingEvent.OnErrorOccuredInitiated, logData);

        if (chatRequestContext?.UserConsent == ActionConsentType.Yes)
        {
            result = await OnErrorOccurred(chatRequestContext) ?? "There was an issue handling this error, please create a support ticket.";
        }
        else
        {
            result = _config[ConfigurationKey.NoConsentPrompt.ToString()] ?? string.Empty;
            logData[LogDataKey.ErrorMessage] = "User did not consent to handle error.";
        }

        return ToolResult.Text(result);
    }
}
