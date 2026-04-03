// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace Microsoft.CFS.Approvals.Core.BL.Helpers;

using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.CFS.Approvals.Contracts;
using Microsoft.CFS.Approvals.Contracts.DataContracts;
using Microsoft.CFS.Approvals.Core.BL.Interface;
using Microsoft.CFS.Approvals.LogManager.Model;
using Microsoft.CFS.Approvals.LogManager.Provider.Interface;
using Microsoft.CFS.Approvals.Model;
using Microsoft.Extensions.Configuration;
using Constants = Contracts.Constants;

/// <summary>
/// The Error Handler Helper class
/// </summary>
public class ErrorHandlerHelper : IErrorHandlerHelper
{
    #region Private Variables

    /// <summary>
    /// The configuration
    /// </summary>
    protected readonly IConfiguration _config;

    /// <summary>
    /// The log provider
    /// </summary>
    protected readonly ILogProvider _logProvider;

    /// <summary>
    /// The approval tenantInfo helper
    /// </summary>
    protected readonly IApprovalTenantInfoHelper _approvalTenantInfoHelper;

    /// <summary>
    /// The approval summary provider
    /// </summary>
    private readonly ISummaryHelper _summaryHelper;

    /// <summary>
    /// The approval summary provider
    /// </summary>
    private readonly IDelegationHelper _delegationHelper;


    #endregion Private Variables

    #region Constructor

    /// <summary>
    /// Constructor of ErrorHandlerHelper
    /// </summary>
    /// <param name="config"></param>
    /// <param name="logProvider"></param>
    /// <param name="approvalTenantInfoHelper"></param>
    /// <param name="summaryHelper"></param>
    /// <param name="delegationHelper"></param>
    public ErrorHandlerHelper(
        IConfiguration config,
        ILogProvider logProvider,
        IApprovalTenantInfoHelper approvalTenantInfoHelper,
        ISummaryHelper summaryHelper,
        IDelegationHelper delegationHelper)
    {
        _config = config;
        _logProvider = logProvider;
        _approvalTenantInfoHelper = approvalTenantInfoHelper;
        _summaryHelper = summaryHelper;
        _delegationHelper = delegationHelper;
    }

    #endregion Constructor

    #region Event Subscribers

    /// <summary>
    /// The function that is invoked when an event is raised. Directs to the correct scenario
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="chatRequestEventArgs"></param>
    /// <returns></returns>
    public async Task<string> ErrorOrchestrator(object sender, ChatRequestEventArgs chatRequestEventArgs)
    {
        #region Logging

        var logData = new Dictionary<LogDataKey, object>
        {
            { LogDataKey.StartDateTime, DateTime.UtcNow },
            { LogDataKey.TenantId, chatRequestEventArgs.TenantId },
            { LogDataKey.DocumentNumber, chatRequestEventArgs.DocumentNumber },
            { LogDataKey.UserAlias, chatRequestEventArgs.UserAlias },
            { LogDataKey.CopilotErrorType, chatRequestEventArgs.CopilotErrorType.ToString() }
        };

        #endregion Logging

        switch (chatRequestEventArgs.CopilotErrorType)
        {
            case CopilotErrorType.OutOfSync:
                return await ErrorHandler_OutOfSync(sender, chatRequestEventArgs);
            default:
                _logProvider.LogError(TrackingEvent.ErrorHandlerExecutionFailed, new ArgumentException($"Unsupported error type {chatRequestEventArgs.CopilotErrorType}"), logData);
                return "There was an issue handling this error, please create a support ticket.";
        }
    }

    /// <summary>
    /// The function that the assistant calls to update a request as out of sync
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="chatRequestEventArgs"></param>
    /// <returns></returns>
    public async Task<string> ErrorHandler_OutOfSync(object sender, ChatRequestEventArgs chatRequestEventArgs)
    {
        #region Logging

        var logData = new Dictionary<LogDataKey, object>
        {
            { LogDataKey.StartDateTime, DateTime.UtcNow },
            { LogDataKey.TenantId, chatRequestEventArgs.TenantId },
            { LogDataKey.DocumentNumber, chatRequestEventArgs.DocumentNumber },
            { LogDataKey.UserAlias, chatRequestEventArgs.UserAlias }
        };

        #endregion Logging

        try
        {
            if (!chatRequestEventArgs.CopilotErrorType.Equals(CopilotErrorType.OutOfSync))
            {
                throw new ArgumentException("I can't mark the request out of sync if there wasn't an out of sync error.");
            }

            string approverAlias = CheckAuthorizationAndSetAlias(chatRequestEventArgs);

            //Update the summary as out of sync
            ApprovalTenantInfo tenantInfo = _approvalTenantInfoHelper.GetTenantInfo(chatRequestEventArgs.TenantId);

            await _summaryHelper.UpdateSummary(tenantInfo, chatRequestEventArgs.DocumentNumber, approverAlias, "", "", DateTime.Now, Constants.OutOfSyncAction);
            _logProvider.LogInformation(TrackingEvent.ErrorHandlerExecutionSuccess, logData);
            return "Successfully updated the request. Please refresh the page and check.";
        }
        catch (UnauthorizedAccessException ex)
        {
            _logProvider.LogError(TrackingEvent.ErrorHandlerExecutionFailed, ex, logData);
            return "Permission denied: You do not have sufficient permissions for this action.";
        }
        catch (Exception ex)
        {
            _logProvider.LogError(TrackingEvent.ErrorHandlerExecutionFailed, ex, logData);
            return "There was a problem updating this request. Please create a support ticket.";
        }
    }


    #endregion Event Subscribers

    #region Private Methods

    /// <summary>
    /// The function that checks the level of authorization and sets the alias for the approver
    /// </summary>
    /// <param name="chatRequestEventArgs"></param>
    /// <returns></returns>
    private string CheckAuthorizationAndSetAlias(ChatRequestEventArgs chatRequestEventArgs)
    {
        var approverAlias = !string.IsNullOrWhiteSpace(chatRequestEventArgs.OnBehalfUserAlias)
        ? chatRequestEventArgs.OnBehalfUserAlias
        : chatRequestEventArgs.UserAlias;

        // Permission check
        var signedInUser = new User
        {
            MailNickname = chatRequestEventArgs.UserAlias,
            UserPrincipalName = chatRequestEventArgs.UserAlias + "@microsoft.com",
        };
        var onBehalfUser = !string.IsNullOrWhiteSpace(chatRequestEventArgs.OnBehalfUserAlias)
            ? new User
            {
                MailNickname = chatRequestEventArgs.OnBehalfUserAlias,
                UserPrincipalName = chatRequestEventArgs.OnBehalfUserAlias + "@microsoft.com",
            }
            : signedInUser;

        // Check delegation access level if acting on behalf
        if (!string.IsNullOrWhiteSpace(chatRequestEventArgs.OnBehalfUserAlias))
        {
            var accessLevel = _delegationHelper.GetDelegationAccessLevel(onBehalfUser, signedInUser);
            if (accessLevel != DelegationAccessLevel.Admin)
            {
                throw new UnauthorizedAccessException("User does not have sufficient permissions for this action.");
            }
        }

        return approverAlias;
    }

    #endregion Private Methods

}