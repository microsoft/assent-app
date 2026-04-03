// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace Microsoft.CFS.Approvals.Core.BL.Helpers;

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Reflection;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.CFS.Approvals.Common.DL.Interface;
using Microsoft.CFS.Approvals.Contracts;
using Microsoft.CFS.Approvals.Contracts.DataContracts;
using Microsoft.CFS.Approvals.Core.BL.Interface;
using Microsoft.CFS.Approvals.Data.Azure.Storage.Interface;
using Microsoft.CFS.Approvals.Extensions;
using Microsoft.CFS.Approvals.LogManager.Interface;
using Microsoft.CFS.Approvals.LogManager.Model;
using Microsoft.CFS.Approvals.LogManager.Provider.Interface;
using Microsoft.CFS.Approvals.Model;
using Microsoft.CFS.Approvals.Utilities.Helpers;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json.Linq;

/// <summary>
/// The Client Action Helper class
/// </summary>
public class ClientActionHelper : IClientActionHelper
{
    #region Variables

    /// <summary>
    /// The configuration
    /// </summary>
    protected readonly IConfiguration _config;

    /// <summary>
    /// The logger
    /// </summary>
    protected readonly ILogProvider _logger = null;

    /// <summary>
    /// The performance logger
    /// </summary>
    protected readonly IPerformanceLogger _performanceLogger = null;

    /// <summary>
    /// The Document Action Helper Delegate
    /// </summary>
    private readonly Func<string, IDocumentActionHelper> _documentActionHelperDel;

    /// <summary>
    /// The document Approval Status Helper
    /// </summary>
    protected readonly IDocumentApprovalStatusHelper _documentApprovalStatusHelper = null;

    /// <summary>
    /// The details helper
    /// </summary>
    protected readonly IDetailsHelper _detailsHelper = null;

    /// <summary>
    /// The blob storage helper
    /// </summary>
    protected readonly IBlobStorageHelper _blobStorageHelper = null;

    /// <summary>
    /// OpenTelemetry audit logger
    /// </summary>
    private readonly IAuditLogger _auditLogger;

    /// <summary>
    /// The adaptive card response helper
    /// </summary>
    private readonly IAdaptiveCardResponseHelper _adaptiveCardResponseHelper;

    /// <summary>
    /// The read details helper
    /// </summary>
    private readonly IReadDetailsHelper _readDetailsHelper;

    #endregion Variables

    #region Constructor

    /// <summary>
    /// Initializes a new instance of the <see cref="DocumentActionHelper"/> class.
    /// </summary>
    /// <param name="config"></param>
    /// <param name="logger"></param>
    /// <param name="performanceLogger"></param>
    /// <param name="documentActionHelperDel"></param>
    /// <param name="documentApprovalStatusHelper"></param>
    /// <param name="detailsHelper"></param>
    /// <param name="blobStorageHelper"></param>
    /// <param name="auditLogger"></param>
    /// <param name="adaptiveCardResponseHelper"></param>
    public ClientActionHelper(
        IConfiguration config,
        ILogProvider logger,
        IPerformanceLogger performanceLogger,
        Func<string, IDocumentActionHelper> documentActionHelperDel,
        IDocumentApprovalStatusHelper documentApprovalStatusHelper,
        IDetailsHelper detailsHelper,
        IBlobStorageHelper blobStorageHelper,
        IAuditLogger auditLogger,
        IAdaptiveCardResponseHelper adaptiveCardResponseHelper,
        IReadDetailsHelper readDetailsHelper)
    {
        _config = config;
        _logger = logger;
        _performanceLogger = performanceLogger;
        _documentActionHelperDel = documentActionHelperDel;
        _documentApprovalStatusHelper = documentApprovalStatusHelper;
        _detailsHelper = detailsHelper;
        _blobStorageHelper = blobStorageHelper;
        _auditLogger = auditLogger;
        _adaptiveCardResponseHelper = adaptiveCardResponseHelper;
        _readDetailsHelper = readDetailsHelper;
    }

    #endregion Constructor

    #region Implemented Methods

    /// <summary>
    /// Processes the User String and formulates a proper Response Card OR Error response after taking action on request from Outlook
    /// </summary>
    /// <param name="tenantId">Tenant ID</param>
    /// <param name="request">Http request</param>
    /// <param name="clientDevice">Client Device</param>
    /// <param name="onBehalfUser">on-behalf user entity</param>
    /// <param name="signedInUser">signed-in user entity</param>
    /// <param name="oauth2UserToken">OAuth 2.0 Token</param>
    /// <param name="submissionType">Action submission type</param>
    /// <param name="xcv">X-Correlation ID</param>
    /// <param name="tcv">T-Correlation ID</param>
    /// <param name="sessionId">Session ID</param>
    /// <param name="auditLogger"></param>
    /// <returns>Http Response</returns>
    public async Task<IActionResult> TakeActionFromNonWebClient(
            int tenantId,
            HttpRequest request,
            string clientDevice,
            User onBehalfUser,
            User signedInUser,
            string oauth2UserToken,
            ActionSubmissionType submissionType,
            string xcv = "",
            string tcv = "",
            string sessionId = "")
    {
        #region Logging Prep

        var logData = new Dictionary<LogDataKey, object>
        {
            { LogDataKey.StartDateTime, DateTime.UtcNow },
            { LogDataKey.ReceivedTcv, tcv },
            { LogDataKey.SessionId, sessionId },
            { LogDataKey.UserRoleName, signedInUser.UserPrincipalName },
            { LogDataKey.TenantId, tenantId },
            { LogDataKey.UserAlias, onBehalfUser.MailNickname },
            { LogDataKey.ClientDevice, clientDevice },
            { LogDataKey.DisplayDocumentNumber, string.Empty},
            { LogDataKey.DocumentNumber, string.Empty},
            { LogDataKey.ResponseContent, string.Empty },
            { LogDataKey.ResponseStatusCode, HttpStatusCode.OK}
        };

        #endregion Logging Prep

        HttpResponseMessage response = new HttpResponseMessage(HttpStatusCode.BadRequest);
        using (var documentActionTracer = _performanceLogger.StartPerformanceLogger("PerfLog", string.IsNullOrWhiteSpace(clientDevice) ? Constants.WebClient : clientDevice, string.Format(Constants.PerfLogCommon, clientDevice + " Document Action"), logData))
        {
            #region Get templates from Blob

            Dictionary<string, string> templateList = new Dictionary<string, string>();
            Task<Dictionary<string, string>> task = GetBlobTemplates();
            _auditLogger.LogAudit("TakeActionFromNonWebClient", AuditOperationType.Read, signedInUser.MailNickname, "CoreServices", "BlobStorage", "NA", AuditOperationResult.Success, $"ClientActionHelper.cs - TakeActionFromNonWebClient - GetBlobTemplates, tenantId:{tenantId}, sessionId:{sessionId}, tcv:{tcv}, xcv:{xcv}");

            #endregion Get templates from Blob

            bool isFinanceAssistant = false;

            try
            {
                string cardCorrelationId = string.Empty;
                string actionRequestId = string.Empty;

                cardCorrelationId = request.Headers.FirstOrDefault(x => x.Key.Equals(Constants.CardCorrelationId)).Value.FirstOrDefault();

                if (!string.IsNullOrEmpty(cardCorrelationId))
                {
                    xcv = cardCorrelationId;
                }

                actionRequestId = request.Headers.FirstOrDefault(x => x.Key.Equals(Constants.ActionRequestId)).Value.FirstOrDefault();

                if (!string.IsNullOrEmpty(actionRequestId))
                {
                    tcv = actionRequestId;
                }
                logData.Add(LogDataKey.Xcv, xcv);
                logData.Add(LogDataKey.Tcv, tcv);

                if (string.IsNullOrEmpty(signedInUser.MailNickname))
                {
                    _logger.LogError(TrackingEvent.WebApiOutlookIdentityActionFail, new UnauthorizedAccessException(Constants.InValidSenderClaim), logData);
                    response.StatusCode = HttpStatusCode.Unauthorized;
                    response.Headers.Add(Constants.CardActionStatus, _config[ConfigurationKey.UnAuthorizedException.ToString()]);
                    _auditLogger.LogAudit("TakeActionFromNonWebClient", AuditOperationType.Read, signedInUser.MailNickname, "CoreServices", "NA", "NA", AuditOperationResult.Failure, $"ClientActionHelper.cs - TakeActionFromNonWebClient, tenantId:{tenantId}, sessionId:{sessionId}, tcv:{tcv}, xcv:{xcv}, errorMessage: {Constants.InValidSenderClaim}, logData: {logData}");
                    return new HttpResponseMessageResult(response);
                }

                // Getting the user action string from request content
                string userActionsString;
                using (var reader = new StreamReader(request.Body))
                {
                    userActionsString = await reader.ReadToEndAsync();
                }
                if (string.IsNullOrEmpty(userActionsString))
                {
                    _logger.LogError(TrackingEvent.WebApiOutlookIdentityActionFail, new InvalidDataException(Constants.UserActionsStringIsNull), logData);
                    response.Headers.Add(Constants.CardActionStatus, Constants.OutlookGenericErrorMessage);
                    _auditLogger.LogAudit("TakeActionFromNonWebClient", AuditOperationType.Read, signedInUser.MailNickname, "CoreServices", "NA", "NA", AuditOperationResult.Failure, $"ClientActionHelper.cs - TakeActionFromNonWebClient, tenantId:{tenantId}, sessionId:{sessionId}, tcv:{tcv}, xcv:{xcv}, errorMessage: {Constants.UserActionsStringIsNull}, logData: {logData}");
                    return new HttpResponseMessageResult(response);
                }

                // For time being the logic can use the content being posted by the user instead of querying the data from Tenant Info Table in Approvals
                JObject userActionObj = JObject.Parse(userActionsString);

                #region Unwrap Data Property (M365 Copilot/Finance Assistant Compatibility)

                // M365 Copilot wraps Action.Submit data inside a "data" property, while Teams flattens it at root level.
                // Unwrap the "data" property if ActionBody is not at root level but exists inside "data".
                // Log client device as "Finance Assistant" for telemetry but use Teams logic for processing.
                if (userActionObj[Constants.ActionBody] == null && userActionObj["data"] != null)
                {
                    JToken dataToken = userActionObj["data"];
                    if (dataToken is JObject dataObject && dataObject[Constants.ActionBody] != null)
                    {
                        // Merge the contents of "data" into the root object
                        foreach (JProperty property in dataObject.Properties().ToList())
                        {
                            userActionObj[property.Name] = property.Value;
                        }
                        isFinanceAssistant = true;
                        logData[LogDataKey.ClientDevice] = Constants.FinanceAssistantClient;
                        logData[LogDataKey.EventName] = "ActionPayloadUnwrapped";
                        _logger.LogInformation(TrackingEvent.WebApiOutlookIdentityActionSuccess, logData);
                    }
                }

                #endregion Unwrap Data Property (M365 Copilot/Finance Assistant Compatibility)

                // Modify User Action String to add ActionByAlias
                userActionObj = ModifyUserAction(userActionObj, signedInUser.MailNickname, clientDevice);

                // Extract the required details from ActionBody
                var actionRequestObj = userActionObj[Constants.ActionBody];

                // Perform action name
                string actionName = actionRequestObj[Constants.ActionName]?.ToString();

                string color = actionRequestObj[Constants.Color]?.ToString() ?? Constants.StatusGood;

                userActionObj[Constants.DisplayDocumentNumber] = actionRequestObj[Constants.DocumentKeys].First()[Constants.DisplayDocumentNumber];
                userActionObj[Constants.ActionName] = actionName;
                userActionObj[Constants.Color] = color;

                string userImageTemp = string.Empty;
                var submitterAlias = userActionObj["SubmitterAlias"].ToString();
                string userImage = await _detailsHelper.GetUserImage(submitterAlias, sessionId, clientDevice, logData);
                _auditLogger.LogAudit("TakeActionFromNonWebClient", AuditOperationType.Read, signedInUser.MailNickname, "CoreServices", "BlobStorage", "NA", AuditOperationResult.Success, $"ClientActionHelper.cs - TakeActionFromNonWebClient - GetUserImage, tenantId:{tenantId}, sessionId:{sessionId}, tcv:{tcv}, xcv:{xcv}");

                userActionObj[Constants.UserImage] = userImage;

                if (actionRequestObj[Constants.DocumentKeys] != null && actionRequestObj[Constants.DocumentKeys].HasValues)
                {
                    logData[LogDataKey.DisplayDocumentNumber] = actionRequestObj[Constants.DocumentKeys].First()[Constants.DisplayDocumentNumber] != null ? actionRequestObj[Constants.DocumentKeys].First()[Constants.DisplayDocumentNumber].ToString() : string.Empty;
                    logData[LogDataKey.DocumentNumber] = actionRequestObj[Constants.DocumentKeys].First()[Constants.DocumentNumber] != null ? actionRequestObj[Constants.DocumentKeys].First()[Constants.DocumentNumber].ToString() : string.Empty;
                }

                response.StatusCode = HttpStatusCode.OK;
                string displayDocNumber = userActionObj[Constants.DisplayDocumentNumber]?.ToString() ?? string.Empty;

                try
                {
                    var DocumentActionHelper = _documentActionHelperDel(submissionType.ToString());
                    var responseObject = await DocumentActionHelper.TakeAction(tenantId, actionRequestObj.ToString(), clientDevice, onBehalfUser, signedInUser, oauth2UserToken, xcv, tcv, sessionId);

                    #region Finance Assistant PluginResponse

                    if (isFinanceAssistant)
                    {
                        return CreateFinanceAssistantResponse(
                            $"Successfully completed '{actionName}' on request {displayDocNumber}.",
                            isError: false, logData, null, signedInUser, tenantId, sessionId, tcv, xcv);
                    }

                    #endregion Finance Assistant PluginResponse

                    string responseContent = string.Empty;
                    response.Headers.Add(Constants.CardActionStatus, Constants.ActionSuccessfulMessage);
                    response.Headers.Add(Constants.CardUpdateInBody, "true");

                    templateList = await task;
                    responseContent = MSAHelper.CreateCard(templateList[Constants.ACTIONRESPONSETEMPLATE + clientDevice + ".json"], userActionObj.ToJson());
                    response.Content = new StringContent(responseContent);

                    logData[LogDataKey.ResponseContent] = responseContent;
                    _logger.LogInformation(TrackingEvent.WebApiOutlookIdentityActionSuccess, logData);

                    return new HttpResponseMessageResult(response);
                }
                catch (Exception ex)
                {
                    // Assigning the string message into CARD-ACTION-STATUS as this doesn't work with JSON string.
                    string message = FormateMessage(ex.InnerException != null ? ex.InnerException.Message : ex.Message);
                    if (message.Contains("ApprovalResponseDetails"))
                    {
                        string[] str = new string[] { "\"ApprovalResponseDetails\"" };
                        var messages = message.Split(str, StringSplitOptions.None);
                        message = JObject.Parse("{ \"ApprovalResponseDetails\"" + messages[1])["ApprovalResponseDetails"]["False"]?[0]?["Value"]?.ToString();
                    }

                    #region Finance Assistant Error PluginResponse

                    if (isFinanceAssistant)
                    {
                        return CreateFinanceAssistantResponse(
                            $"We were unable to complete '{actionName}' on request {displayDocNumber}. Please try again or open the request in MSApprovals.",
                            isError: true, logData, new Exception(message), signedInUser, tenantId, sessionId, tcv, xcv);
                    }

                    #endregion Finance Assistant Error PluginResponse

                    response.Headers.Add(Constants.CardUpdateInBody, "true");

                    JObject messageObject = new JObject(new JProperty(Constants.MessageTitle, message));
                    templateList = await task;
                    string responseContent = MSAHelper.CreateCard(templateList[Constants.ACTIONERRORRESPONSETEMPLATE + clientDevice + ".json"], messageObject.ToJson());
                    response.Content = new StringContent(responseContent);

                    logData[LogDataKey.ResponseContent] = responseContent;
                    logData[LogDataKey.ResponseStatusCode] = response.StatusCode;
                    _logger.LogError(TrackingEvent.WebApiOutlookIdentityActionFail, new Exception(message), logData);
                    _auditLogger.LogAudit("TakeActionFromNonWebClient", AuditOperationType.Read, signedInUser.MailNickname, "CoreServices", "NA", "NA", AuditOperationResult.Failure, $"ClientActionHelper.cs - TakeActionFromNonWebClient, tenantId:{tenantId}, sessionId:{sessionId}, tcv:{tcv}, xcv:{xcv}, errorMessage: {message}, logData: {logData}");

                    return new HttpResponseMessageResult(response);
                }
            }
            catch (Exception exception)
            {
                response.StatusCode = HttpStatusCode.BadRequest;
                string errorMessage = JSONHelper.ExtractMessageFromJSON(exception.Message);

                #region Finance Assistant Outer Error PluginResponse

                if (isFinanceAssistant)
                {
                    errorMessage = string.IsNullOrEmpty(errorMessage) ? Constants.OutlookGenericErrorMessage : errorMessage;
                    return CreateFinanceAssistantResponse(
                        "An error occurred while processing your request. Please try again or open the request in MSApprovals.",
                        isError: true, logData, exception, signedInUser, tenantId, sessionId, tcv, xcv);
                }

                #endregion Finance Assistant Outer Error PluginResponse

                switch (clientDevice)
                {
                    case Constants.OutlookClient:
                        response.Headers.Add(Constants.CardActionStatus, string.IsNullOrEmpty(errorMessage) ? Constants.OutlookGenericErrorMessage : errorMessage);
                        break;

                    case Constants.TeamsClient:
                    default:
                        errorMessage = string.IsNullOrEmpty(errorMessage) ? Constants.OutlookGenericErrorMessage : errorMessage;
                        JObject messageObject = new JObject(new JProperty(Constants.MessageTitle, errorMessage));
                        templateList = await task;
                        string responseContent = MSAHelper.CreateCard(templateList[Constants.ACTIONERRORRESPONSETEMPLATE + clientDevice + ".json"], messageObject.ToJson());
                        response.Content = new StringContent(responseContent);
                        logData[LogDataKey.ResponseContent] = responseContent;
                        break;
                }
                logData[LogDataKey.ResponseStatusCode] = response.StatusCode;
                _logger.LogError(TrackingEvent.WebApiOutlookIdentityActionFail, exception, logData);
                _auditLogger.LogAudit("TakeActionFromNonWebClient", AuditOperationType.Read, signedInUser.MailNickname, "CoreServices", "NA", "NA", AuditOperationResult.Failure, $"ClientActionHelper.cs - TakeActionFromNonWebClient, tenantId:{tenantId}, sessionId:{sessionId}, tcv:{tcv}, xcv:{xcv}, errorMessage: {errorMessage}");

                response.Headers.Add(Constants.CardActionStatus, string.IsNullOrEmpty(errorMessage) ? Constants.OutlookGenericErrorMessage : errorMessage);
                return new HttpResponseMessageResult(response);
            }
        }
    }

    /// <summary>
    /// Creates a Finance Assistant (M365 Copilot) response wrapped in a "data" property with logging and audit.
    /// </summary>
    private IActionResult CreateFinanceAssistantResponse(
        string message,
        bool isError,
        Dictionary<LogDataKey, object> logData,
        Exception exception,
        User signedInUser,
        int tenantId,
        string sessionId,
        string tcv,
        string xcv)
    {
        var pluginResponse = new PluginResponse
        {
            // Note: FA currently can only render adaptive cards as a response to an adaptive card action
            Message = _adaptiveCardResponseHelper.CreateTextCard(message),
            MessageType = CopilotMessageType.AdaptiveCard
        };

        var wrappedResponse = new { data = pluginResponse };
        logData[LogDataKey.ResponseContent] = wrappedResponse.ToJson();

        if (isError)
        {
            logData[LogDataKey.ResponseStatusCode] = HttpStatusCode.BadRequest;
            _logger.LogError(TrackingEvent.WebApiOutlookIdentityActionFail, exception, logData);
            _auditLogger.LogAudit("TakeActionFromNonWebClient", AuditOperationType.Read, signedInUser.MailNickname, "CoreServices", "NA", "NA", AuditOperationResult.Failure, $"ClientActionHelper.cs - TakeActionFromNonWebClient, tenantId:{tenantId}, sessionId:{sessionId}, tcv:{tcv}, xcv:{xcv}, errorMessage: {message}");
            return new BadRequestObjectResult(wrappedResponse);
        }

        _logger.LogInformation(TrackingEvent.WebApiOutlookIdentityActionSuccess, logData);
        return new OkObjectResult(wrappedResponse);
    }

    private async Task<Dictionary<string, string>> GetBlobTemplates()
    {
        var blobList = await _blobStorageHelper.ListBlobsHierarchicalListing(Constants.OutlookDynamicTemplates, "ActionResponse", null);

        Dictionary<string, string> templateList = new Dictionary<string, string>();
        foreach (var item in blobList)
        {
            var blobItemName = item.Name;
            var htmlTemplate = await _blobStorageHelper.DownloadText(Constants.OutlookDynamicTemplates, blobItemName);
            blobItemName = blobItemName.Split('/')[1].ToString();
            templateList[blobItemName] = htmlTemplate;
        }

        return templateList;
    }

    /// <summary>
    /// Formulates a proper response card OR error response after checking the Status of a particular request
    /// </summary>
    /// <param name="onBehalfUser">on-Behalf User</param>
    /// <param name="signedInUser">Logged-in user </param>
    /// <param name="oauth2UserToken"></param>
    /// <param name="tenantId">Tenant ID</param>
    /// <param name="request">Http request</param>
    /// <param name="clientDevice">Client Device</param>
    /// <param name="tcv">T-correlation ID</param>
    /// <param name="sessionId">Session ID</param>
    /// <param name="xcv">X-Correlation ID</param>
    /// <param name="domainName"></param>
    /// <returns>Https Response</returns>
    public async Task<IActionResult> ClientAutoRefresh(User signedInUser, User onBehalfUser, string oauth2UserToken, int tenantId, HttpRequest request, string clientDevice, string tcv, string sessionId, string xcv, string domainName)
    {
        #region Logging Prep

        var logData = new Dictionary<LogDataKey, object>
        {
            { LogDataKey.SessionId, sessionId },
            { LogDataKey.UserRoleName, signedInUser.MailNickname },
            { LogDataKey.TenantId, tenantId },
            { LogDataKey.UserAlias, onBehalfUser.MailNickname },
            { LogDataKey.ClientDevice, clientDevice },
            { LogDataKey.ActionOrComponentUri, "CoreService/AutoRefresh"},
            { LogDataKey.DisplayDocumentNumber, string.Empty}
        };

        #endregion Logging Prep

        HttpResponseMessage response = new HttpResponseMessage(HttpStatusCode.BadRequest);

        try
        {
            string actionPerformer = string.Empty;
            DocumentStatusRequest autoRefreshRequest = new DocumentStatusRequest();
            string cardCorrelationId = string.Empty;
            string actionRequestId = string.Empty;

            cardCorrelationId = request.Headers.FirstOrDefault(x => x.Key.Equals(Constants.CardCorrelationId)).Value.FirstOrDefault();

            if (!string.IsNullOrEmpty(cardCorrelationId))
            {
                xcv = cardCorrelationId;
            }

            actionRequestId = request.Headers.FirstOrDefault(x => x.Key.Equals(Constants.ActionRequestId)).Value.FirstOrDefault();

            if (!string.IsNullOrEmpty(actionRequestId))
            {
                tcv = actionRequestId;
            }
            logData.Add(LogDataKey.Xcv, xcv);
            logData.Add(LogDataKey.Tcv, tcv);
            logData.Add(LogDataKey.StartDateTime, DateTime.UtcNow);

            using (var performanceTracer = _performanceLogger.StartPerformanceLogger("PerfLog", Constants.OutlookClient, "AutoRefresh-Submitter", logData))
            {
                if (string.IsNullOrEmpty(signedInUser.MailNickname))
                {
                    _logger.LogError(TrackingEvent.WebApiOutlookIdentityAutoRefreshFail, new UnauthorizedAccessException(Constants.InValidSenderClaim), logData);
                    response.StatusCode = HttpStatusCode.Unauthorized;
                    response.Headers.Add(Constants.CardActionStatus, _config[ConfigurationKey.UnAuthorizedException.ToString()]);
                    return new HttpResponseMessageResult(response);
                }

                #region Get the user action string from request content and validate it

                // Getting the user action string from request content
                string userActionsString;
                using (var reader = new StreamReader(request.Body))
                {
                    userActionsString = await reader.ReadToEndAsync();
                }
                if (string.IsNullOrEmpty(userActionsString))
                {
                    _logger.LogError(TrackingEvent.WebApiOutlookIdentityAutoRefreshFail, new InvalidDataException(Constants.UserActionsStringIsNull), logData);
                    response.Headers.Add(Constants.CardActionStatus, Constants.OutlookGenericErrorMessage);
                    return new HttpResponseMessageResult(response);
                }

                #endregion Get the user action string from request content and validate it

                logData.Add(LogDataKey.UserActionsString, userActionsString);

                autoRefreshRequest = userActionsString.FromJson<DocumentStatusRequest>();
                logData.Modify(LogDataKey.EndDateTime, DateTime.UtcNow);
            }

            response.Headers.Add(Constants.CardActionStatus, Constants.OutlookAutoRefreshGenericMessage);

            // Return a non successful status code so that the Adaptive Card is rendered properly in OWA.
            // Current OWA version has an issue where in the Adaptive Card doesn't load when a Success status code (2xx) is sent in the response.
            // Earlier this was sent as a BadRequest (400) which was creating a lot of noise in telemetry logs.
            // Rather than sending an error status code (4xx) it is better to send the status code 302.
            // This logic of Auto-refresh is going to change completely once we re-write this code and then we can remove this patch (workaround).
            response.StatusCode = HttpStatusCode.Found;

            Dictionary<string, string> placeHolderDict = new Dictionary<string, string>();

            var documentNumber = autoRefreshRequest != null ? autoRefreshRequest.DocumentNumber : string.Empty;
            var requestVersion = autoRefreshRequest != null ? autoRefreshRequest.RequestVersion.ToString() : string.Empty;

            /* Call ApprovalRequest status api to identify whether request is approved or not
            and return updated card if request is approved */

            var autoRefreshRequestObj = new { ApproverAlias = signedInUser.MailNickname, DocumentNumber = documentNumber, RequestVersion = requestVersion };

            logData[LogDataKey.DisplayDocumentNumber] = documentNumber;
            logData.Modify(LogDataKey.StartDateTime, DateTime.UtcNow);

            #region Submit action to Approvals API and return the response

            using (var performanceTracer = _performanceLogger.StartPerformanceLogger("PerfLog", Constants.OutlookClient, "AutoRefresh-Card", logData))
            {
                try
                {
                    DocumentStatusResponse documentStatusResponse = await _documentApprovalStatusHelper.DocumentStatus(signedInUser, onBehalfUser, oauth2UserToken, tenantId, autoRefreshRequestObj.ToJson(), clientDevice, tcv, sessionId, xcv);

                    if (documentStatusResponse != null)
                    {
                        logData.Add(LogDataKey.AppAction, documentStatusResponse.CurrentStatus);

                        response.StatusCode = HttpStatusCode.OK;

                        if (string.IsNullOrEmpty(documentStatusResponse.CurrentStatus))
                        {
                            response.StatusCode = HttpStatusCode.BadRequest;
                            _logger.LogError(TrackingEvent.WebApiOutlookIdentityAutoRefreshFail, new Exception(Constants.DocumentStatusAPIResponseIsNull), logData);
                        }
                        else
                        {
                            placeHolderDict = GetPlaceHolderData(documentStatusResponse);
                            placeHolderDict.Add(Constants.ApprovalIdentifierDisplayDocNumber, autoRefreshRequest.DocumentNumber);

                            switch (documentStatusResponse.CurrentStatus)
                            {
                                case Constants.Pending:
                                    if (!documentStatusResponse.IsRead)
                                    {
                                        JObject isReadInputObject = new JObject();
                                        var actionDetails = new JObject
                                        {
                                            { "Action", "Read Details" }
                                        };
                                        isReadInputObject.Add("DocumentKeys", autoRefreshRequest.DocumentNumber);
                                        isReadInputObject.Add("ActionDetails", actionDetails);
                                        await _readDetailsHelper.UpdateIsReadDetails(signedInUser, onBehalfUser, oauth2UserToken, isReadInputObject.ToJson(), tenantId, clientDevice, sessionId, tcv, xcv, domainName);
                                    }
                                    break;

                                case Constants.OldRequest:
                                    placeHolderDict.Add(Constants.MessageTitle, Constants.OldRequestMessage);
                                    UpdateHttpResponseMessage(ref response, Constants.OutlookAutoRefreshActionResponseMessage, placeHolderDict);
                                    break;

                                case Constants.LobPending:
                                    placeHolderDict.Add(Constants.MessageTitle, Constants.LobPendingMessage);
                                    UpdateHttpResponseMessage(ref response, Constants.OutlookAutoRefreshActionResponseMessage, placeHolderDict);
                                    break;

                                case Constants.SubmittedForBackgroundApproval:
                                    placeHolderDict.Add(Constants.MessageTitle, Constants.SubmittedForBackgroundMessage);
                                    UpdateHttpResponseMessage(ref response, Constants.OutlookAutoRefreshActionResponseMessage, placeHolderDict);
                                    break;

                                case Constants.OutOfSyncRecord:
                                    placeHolderDict.Add(Constants.MessageTitle, Constants.OutOfSyncMessage);
                                    UpdateHttpResponseMessage(ref response, Constants.OutlookAutoRefreshActionResponseMessage, placeHolderDict);
                                    break;

                                default:
                                    placeHolderDict.Add(Constants.MessageTitle, Constants.ActionTakenMessage);
                                    UpdateHttpResponseMessage(ref response, Constants.OutlookAutoRefreshActionResponseMessage, placeHolderDict);
                                    break;
                                    //TODO :: User Input String updation for AutoRefresh API and corresponding Action Card design implementation
                                    //Thus commenting this code temporarily
                                    //string actionName = documentStatusResponse.CurrentStatus;
                                    //placeHolderDict[Constants.CurrentStatus] = actionName;
                                    //UpdateHttpResponseMessage(ref response, Constants.OutlookAutoRefreshActionResponse, placeHolderDict);
                            }
                            logData.Modify(LogDataKey.EndDateTime, DateTime.UtcNow);
                            _logger.LogInformation(TrackingEvent.WebApiOutlookIdentityAutoRefreshSuccess, logData);
                        }
                    }
                    else
                    {
                        logData.Modify(LogDataKey.EndDateTime, DateTime.UtcNow);
                        _logger.LogError(TrackingEvent.WebApiOutlookIdentityAutoRefreshFail, new Exception(Constants.DocumentStatusAPIResponseIsNull), logData);
                    }
                }
                catch
                {
                    _logger.LogError(TrackingEvent.WebApiOutlookIdentityAutoRefreshFail, new Exception(string.Format(Constants.DocumentStatusAPIFailedWithStatusCode, HttpStatusCode.BadRequest)), logData);
                }
            }

            #endregion Submit action to Approvals API and return the response

            return new HttpResponseMessageResult(response);
        }
        catch (Exception exception)
        {
            logData.Modify(LogDataKey.EndDateTime, DateTime.UtcNow);
            _logger.LogError(TrackingEvent.WebApiOutlookIdentityAutoRefreshFail, exception, logData);

            response.StatusCode = HttpStatusCode.BadRequest;
            string errorMessage = JSONHelper.ExtractMessageFromJSON(exception.Message);
            response.Headers.Add(Constants.CardActionStatus, string.IsNullOrEmpty(errorMessage) ? Constants.OutlookGenericErrorMessage : errorMessage);
            return new HttpResponseMessageResult(response);
        }
    }

    #endregion Implemented Methods

    #region Helper Methods

    /// <summary>
    /// Modify UserAction Json object
    /// </summary>
    /// <param name="userActionObj"></param>
    /// <param name="loggedInUser"></param>
    /// <returns></returns>
    private JObject ModifyUserAction(JObject userActionObj, string loggedInUser, string clientDevice)
    {
        // Extract ActionBody from the request content
        var actionRequestObj = userActionObj[Constants.ActionBody];

        actionRequestObj[Constants.ActionByAlias] = loggedInUser;

        // to get the action details.
        JObject actionRequestDetailsObj = actionRequestObj[Constants.ActionDetailsKey].ToJson().FromJson<JObject>();

        // to replace the ActionDate Field.
        string actionDate = actionRequestDetailsObj[Constants.ActionDateKey].ToString();
        actionRequestDetailsObj[Constants.ActionDateKey] = DateTime.TryParse(actionDate, out DateTime utcNow) ? utcNow.ToString("o") : DateTime.UtcNow.ToString("o");

        var justification = actionRequestDetailsObj[Constants.JustificationKey];
        var comment = actionRequestDetailsObj[Constants.CommentKey];
        if (clientDevice.Equals(Constants.TeamsClient))
        {
            foreach (var actionToken in actionRequestDetailsObj)
            {
                if (actionToken.Value.ToString().Contains("{{") && actionToken.Value.ToString().Contains("}}"))
                {
                    var blockId = (actionToken.Value.ToString().Split(new string[] { "{{" }, StringSplitOptions.RemoveEmptyEntries)?[0])?.Split('.')?[0];
                    actionRequestDetailsObj[actionToken.Key] = userActionObj[blockId];
                }
            }
            foreach (var actionToken in actionRequestDetailsObj)
            {
                if (actionToken.Value is JArray)
                {
                    actionRequestDetailsObj[actionToken.Key] = string.Empty;
                }
            }

            var lineItemAction = new Dictionary<string, string>();
            var lineItemComment = new Dictionary<string, string>();
            List<string> lineItemProperties = (userActionObj["LineItemProperties"].ToJson().FromJson<List<string>>()) != null ? userActionObj["LineItemProperties"].ToJson().FromJson<List<string>>() : new List<string>();
            foreach (var lineItemProperty in lineItemProperties)
            {
                foreach (var userActionToken in userActionObj)
                {
                    if (userActionToken.Key.Contains(lineItemProperty + "Action"))
                    {
                        lineItemAction.Add(userActionToken.Key.Replace(lineItemProperty + "Action-", ""), userActionToken.Value.ToString());
                    }
                    if (userActionToken.Key.Contains(lineItemProperty + "Comment"))
                    {
                        lineItemComment.Add(userActionToken.Key.Replace(lineItemProperty + "Comment-", ""), userActionToken.Value.ToString());
                    }
                }
            }

            JArray lineItemsArray = new JArray();
            foreach (var lineItemActionObj in lineItemAction)
            {
                JObject lineItemElement = new JObject
                {
                    { Constants.ActionBodyLineItemId, lineItemActionObj.Key },
                    { Constants.ActionKey, lineItemActionObj.Value },
                    { Constants.CommentKey, lineItemComment.ContainsKey(lineItemActionObj.Key) ? lineItemComment[lineItemActionObj.Key] : string.Empty }
                };
                lineItemsArray.Add(lineItemElement);
            }
            if (lineItemsArray != null && lineItemsArray.Count > 0)
            {
                JObject additionalData = new JObject
                {
                    { Constants.LineItems, lineItemsArray.ToJson() }
                };
                // Extract ActionBody from the request content
                var actionBodyObj = userActionObj[Constants.ActionBody];
                actionRequestObj[Constants.AdditionalData] = additionalData;
            }
        }
        if (justification != null)
        {
            var justificationArray = justification?.ToString()?.Split(',');
            if (justificationArray != null && justificationArray.Count() > 1)
            {
                actionRequestDetailsObj[Constants.ReasonCodeKey] = justificationArray[0] != null && !string.IsNullOrWhiteSpace(justificationArray[0].ToString()) ? justificationArray[0].ToString() : string.Empty;
                actionRequestDetailsObj[Constants.ReasonTextKey] = justificationArray[1] != null && !string.IsNullOrWhiteSpace(justificationArray[1].ToString()) ? justificationArray[1].ToString() : string.Empty;
            }
        }

        actionRequestObj[Constants.ActionDetailsKey] = actionRequestDetailsObj;
        userActionObj[Constants.ActionBody] = actionRequestObj;
        return userActionObj;
    }

    /// <summary>
    /// Formates the message.
    /// </summary>
    /// <param name="message">The message.</param>
    /// <returns>returns formated string</returns>
    private string FormateMessage(string message)
    {
        if (message.Length > 2 && Regex.IsMatch(message, Constants.RegexSquareBracket))
        {
            message = message.Substring(1, message.Length - 2);
        }

        int position = message.IndexOf(':');
        message = message.Substring(position + 1);

        return message;
    }

    /// <summary>
    /// Populate placeholder data from class object
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="classObject"></param>
    /// <returns></returns>
    private Dictionary<string, string> GetPlaceHolderData<T>(T classObject)
    {
        return classObject.GetType().GetProperties(BindingFlags.Instance | BindingFlags.Public).ToDictionary(prop => prop.Name, prop => prop.GetValue(classObject).ToString());
    }

    /// <summary>
    /// Update HttpResponseMessage
    /// </summary>
    /// <param name="response"></param>
    /// <param name="responseFromat"></param>
    /// <param name="placeHolderDict"></param>
    private void UpdateHttpResponseMessage(ref HttpResponseMessage response, string responseFromat, Dictionary<string, string> placeHolderDict)
    {
        response.Headers.Add(Constants.CardActionStatus, Constants.OutlookSuccessMessage);
        response.Headers.Add(Constants.CardUpdateInBody, "true");
        var responseContent = JSONHelper.StringFormat(responseFromat, placeHolderDict);
        response.Content = new StringContent(responseContent);
    }

    #endregion Helper Methods
}