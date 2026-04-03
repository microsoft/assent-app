// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace Microsoft.CFS.Approvals.CoreServices.Controllers.api.v1;

using Microsoft.AspNetCore.Mvc;
using Microsoft.CFS.Approvals.Common.DL.Interface;
using Microsoft.CFS.Approvals.LogManager.Model;
using Microsoft.CFS.Approvals.LogManager.Provider.Interface;
using Microsoft.CFS.Approvals.Model;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

/// <summary>
/// Class FeedbackController.
/// </summary>
/// <seealso cref="BaseApiController" />
public class FeedbackController : BaseApiController
{
    /// <summary>
    /// The feedback helper
    /// </summary>
    private readonly IFeedbackHelper _feedbackHelper;

    /// <summary>
    /// The log provider
    /// </summary>
    private readonly ILogProvider _logProvider = null;

    /// <summary>
    /// Initializes a new instance of the <see cref="FeedbackController"/> class.
    /// </summary>
    /// <param name="feedbackHelper">The feedback helper.</param>
    /// <param name="logProvider"></param>
    public FeedbackController(IFeedbackHelper feedbackHelper, ILogProvider logProvider)
    {
        _feedbackHelper = feedbackHelper;
        _logProvider = logProvider;
    }

    /// <summary>
    /// Submits user feedback to be stored in the system
    /// </summary>
    /// <param name="feedback">The feedback data</param>
    /// <returns>
    /// This method returns an OK HttpResponse if the action is successful;
    /// else returns a Bad Response along with the Error Message specifying the reason for failure
    /// </returns>
    /// <remarks>
    /// <para>
    /// e.g.
    /// HTTP POST api/Feedback
    /// </para>
    /// </remarks>
    [HttpPost]
    public async Task<IActionResult> Post([FromBody] UserFeedback feedback)
    {
        var logData = new Dictionary<LogDataKey, object>
        {
            { LogDataKey.StartDateTime, DateTime.UtcNow },
            { LogDataKey.Xcv, Xcv },
        };
        try
        {
            if (feedback == null || feedback.Inputs == null || feedback.Inputs.Count == 0)
            {
                return BadRequest("Feedback data cannot be empty");
            }

            // Set delegation information - true when onBehalfUser?.MailNickname is not null or empty
            feedback.IsDelegatedUser = !string.IsNullOrEmpty(OnBehalfUser?.MailNickname);
            feedback.ClientDevice = ClientDevice;
            feedback.Xcv = Xcv;
            
            // Ensure ApprovalIdentifier is set if not provided by the client
            if (string.IsNullOrEmpty(feedback.ApprovalIdentifier))
            {
                // If DocumentNumber and FiscalYear are provided, concatenate them
                if (!string.IsNullOrEmpty(feedback.DocumentNumber) && !string.IsNullOrEmpty(feedback.FiscalYear))
                {
                    feedback.ApprovalIdentifier = $"{feedback.DocumentNumber}-{feedback.FiscalYear}";
                }
                else
                {
                    feedback.ApprovalIdentifier = "NA";
                }
            }
            
            await _feedbackHelper.AddFeedbackAsync(feedback);
            return Ok();
        }
        catch (Exception ex)
        {
            _logProvider.LogError(TrackingEvent.FeedbackRequestFailed, ex, logData);
            return BadRequest("An error occurred while processing your feedback, please try again at a later time.");
        }
    }
}