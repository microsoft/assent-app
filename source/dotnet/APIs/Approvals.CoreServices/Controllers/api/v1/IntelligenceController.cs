// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace Microsoft.CFS.Approvals.CoreServices.Controllers.api.v1;

using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs.Extensions.OpenApi.Core.Attributes;
using Microsoft.CFS.Approvals.Contracts;
using Microsoft.CFS.Approvals.Contracts.DataContracts;
using Microsoft.CFS.Approvals.Core.BL.Interface;
using Microsoft.CFS.Approvals.Extensions;
using Microsoft.CFS.Approvals.LogManager.Model;
using Microsoft.CFS.Approvals.LogManager.Provider.Interface;
using Microsoft.CFS.Approvals.Model;
using Microsoft.CFS.Approvals.Utilities.Extension;
using Microsoft.Extensions.Configuration;
using Microsoft.OpenApi.Models;

/// <summary>
/// Class IntelligenceController.
/// </summary>
/// <seealso cref="BaseApiController" />
[Route("")]
public class IntelligenceController : BaseApiController
{
    private readonly IApprovalsPluginHelper _approvalsPluginHelper;
    private readonly IAIAssistedSearchHelper _aiAssistedSearchHelper;

    /// <summary>
    /// The log provider
    /// </summary>
    private readonly ILogProvider _logProvider = null;

    /// <summary>
    /// Initializes a new instance of the <see cref="IntelligenceController"/> class.
    /// </summary>
    /// <param name="config">The configuration</param>
    /// <param name="aiAssistedSearchHelper">The AI-assisted search helper.</param>
    /// <param name="approvalsPlugin"></param>
    /// <param name="logProvider"></param>
    public IntelligenceController(IConfiguration config, IAIAssistedSearchHelper aiAssistedSearchHelper, IApprovalsPluginHelper approvalsPlugin, ILogProvider logProvider)
    {
        _config = config;
        _aiAssistedSearchHelper = aiAssistedSearchHelper;
        _approvalsPluginHelper = approvalsPlugin;
        _logProvider = logProvider;
    }

    /// <summary>
    /// Gets response from Azure Open AI for user question.
    /// </summary>
    /// <param name="chatRequest">Question from user.</param>
    /// <returns>Answer from GPT models through Azure AI.</returns>
    [HttpPost]
    [Route("/chatrequest")]
    [OpenApiOperation(operationId: "chatrequest", tags: ["approvalsrequest"], Description = "Answers questions related to Approvals.")]
    [OpenApiRequestBody("application/json", typeof(AskRequest))]
    [OpenApiResponseWithBody(statusCode: HttpStatusCode.OK, contentType: "text/plain", bodyType: typeof(string), Description = "Returns the answer to the question asked.")]
    [OpenApiResponseWithBody(statusCode: HttpStatusCode.BadRequest, contentType: "application/json", bodyType: typeof(string), Description = "Returns the error of the input.")]
    public async Task<IActionResult> GetChatCompletionAsync([FromBody] PluginChatRequest chatRequest)
    {
        var logData = new Dictionary<LogDataKey, object>
        {
            { LogDataKey.StartDateTime, DateTime.UtcNow },
        };
        try
        {
            // Extract AskRequest or throw so we centralize error logging in catch block
            AskRequest requestDetails = chatRequest?.AskRequest ?? chatRequest?.AdaptiveCardSubmissionData?.AskRequest
                ?? throw new ArgumentException("Invalid request: AskRequest is null.");

            // Add request details early (even if Input invalid) for troubleshooting
            logData.Add(LogDataKey.AskRequest, requestDetails);

            if (string.IsNullOrWhiteSpace(requestDetails.Input))
            {
                throw new ArgumentException("Invalid request: Input is empty.");
            }

            // As requests to this endpoint go through Finance Assistant, we can be sure that the input is sanitized already.

            User OnBehalfUserFromRequest = OnBehalfUser;

            // Set OnBehalfUser properties from AskRequest.AdditionalDetails if present
            if (requestDetails.AdditionalDetails != null)
            {
                if (requestDetails.AdditionalDetails.TryGetValue("onBehalfUserAlias", out var alias) && !string.IsNullOrWhiteSpace(alias) && requestDetails.AdditionalDetails.TryGetValue("onBehalfUserId", out var id) && !string.IsNullOrWhiteSpace(id))
                {
                    // Sanitize alias and id before using them
                    string sanitizedAlias = Extension.SanitizeInput(alias);
                    string sanitizedId = Extension.SanitizeInput(id);

                    if (!string.IsNullOrWhiteSpace(sanitizedAlias) && !string.IsNullOrWhiteSpace(sanitizedId))
                    {
                        OnBehalfUserFromRequest = new User
                        {
                            MailNickname = sanitizedAlias,
                            UserPrincipalName = sanitizedAlias + "@microsoft.com",
                            Id = sanitizedId
                        };
                    }
                }
            }

            logData.Add(LogDataKey.UserAlias, OnBehalfUserFromRequest.MailNickname);
            _logProvider.LogInformation(TrackingEvent.ChatRequestInitiated, logData);

            var chatCompletion = await _approvalsPluginHelper.GetApprovalsPluginCompletionAsync(SignedInUser, OnBehalfUserFromRequest, GetTokenOrCookie(), ClientDevice, requestDetails, MessageId, Xcv);
            var formattedResult = new { Data = chatCompletion };

            logData.Add(LogDataKey.CopilotResponse, chatCompletion);
            _logProvider.LogInformation(TrackingEvent.ChatRequestSuccess, logData);

            return Ok(formattedResult);
        }
        catch (Exception ex)
        {
            // Add error message detail if not already present
            if (!logData.ContainsKey(LogDataKey.ErrorMessage))
            {
                logData.Add(LogDataKey.ErrorMessage, ex.Message);
            }
            _logProvider.LogError(TrackingEvent.ChatRequestFailed, ex, logData);
            return BadRequest(ex.Message);
        }
    }

    /// <summary>
    /// Gets AI-assisted search results using the new method in IntelligenceHelper.
    /// </summary>
    /// <param name="query">The user's search query.</param>
    /// <returns>List of search results or error message.</returns>
    [HttpGet]
    [Route("/api/v1/search")]
    public async Task<IActionResult> GetAIAssistedSearchResults([FromQuery] string query)
    {
        var logData = new Dictionary<LogDataKey, object>
        {
            { LogDataKey.StartDateTime, DateTime.UtcNow },
            { LogDataKey.InputPrompt, query },
            { LogDataKey.UserAlias, SignedInUser?.MailNickname }
        };

        try
        {
            if (string.IsNullOrWhiteSpace(query))
            {
                return BadRequest("Query parameter cannot be empty.");
            }
            string sanitizedInput = Extension.SanitizeInput(query);
            if (sanitizedInput == null)
            {
                return BadRequest("Invalid request: query contains disallowed characters.");
            }

            // Call the AI-assisted search method
            var results = await _aiAssistedSearchHelper.GetAIAssistedSearchResults<string>(
                SignedInUser,
                OnBehalfUser,
                sanitizedInput,
                SearchResultReturnType.DocumentNumbers,
                OnBehalfUser.UserPrincipalName.GetDomainFromUPN(),
                ClientDevice,
                GetTokenOrCookie()
            );

            logData.Add(LogDataKey.CopilotResponse, results);
            _logProvider.LogInformation(TrackingEvent.DeepSearchSuccess, logData);

            return Ok(results);
        }
        catch (Exception ex)
        {
            _logProvider.LogError(TrackingEvent.DeepSearchRequestFailed, ex, logData);
            return BadRequest("An error occurred while processing your search request. Please try again later.");
        }
    }

    /// <summary>
    /// Gets the ai-plugin information for OpenAPI specifications.
    /// </summary>
    /// <returns>aiplugin.json.</returns>
    [HttpGet]
    [Route(".well-known/ai-plugin.json/")]
    [ProducesResponseType(typeof(AIPlugin), 200)]
    [OpenApiOperation(operationId: "SearchKbArticles", tags: ["ExecuteFunction"], Description = "Useful for answering questions related to Approvals.")]
    [OpenApiParameter(name: "question", Description = "The question asked by user", Required = true, In = ParameterLocation.Query)]
    [OpenApiResponseWithBody(statusCode: HttpStatusCode.OK, contentType: "text/plain", bodyType: typeof(string), Description = "Returns the answer to the question asked.")]
    [OpenApiResponseWithBody(statusCode: HttpStatusCode.BadRequest, contentType: "application/json", bodyType: typeof(string), Description = "Returns the error of the input.")]
    public IActionResult AIPlugin()
    {
        var aiPlugin = _config["AIPlugin"];

        if (String.IsNullOrEmpty(aiPlugin))
        {
            return BadRequest(Constants.AIPluginNotFoundMessage);
        }

        return Ok(aiPlugin);
    }

    /// <summary>
    /// Gets openapi.yaml file.
    /// </summary>
    /// <returns>Returns contents of openapi.yaml file.</returns>
    [HttpGet]
    [Route("/openapi.yaml")]
    public IActionResult GetOpenApiYaml()
    {
        var openApiYaml = _config["OpenApiYaml"];

        if (String.IsNullOrEmpty(openApiYaml))
        {
            _logProvider.LogError<TrackingEvent, LogDataKey>(TrackingEvent.WebApiOpenAPIYamlFail, new FileNotFoundException("File <openapi.yaml> doesn't exist!"));
            return BadRequest(Constants.OpenApiYamlNotFoundMessage);
        }

        MemoryStream ms = new(Encoding.UTF8.GetBytes(openApiYaml));
        return new FileStreamResult(ms, "text/plain");
    }
}