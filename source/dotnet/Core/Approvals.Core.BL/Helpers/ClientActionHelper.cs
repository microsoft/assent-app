// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace Microsoft.CFS.Approvals.Core.BL.Helpers
{
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
    using Microsoft.CFS.Approvals.Core.BL.Interface;
    using Microsoft.CFS.Approvals.Data.Azure.Storage.Interface;
    using Microsoft.CFS.Approvals.Extensions;
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
        public ClientActionHelper(
            IConfiguration config,
            ILogProvider logger,
            IPerformanceLogger performanceLogger,
            Func<string, IDocumentActionHelper> documentActionHelperDel,
            IDocumentApprovalStatusHelper documentApprovalStatusHelper,
            IDetailsHelper detailsHelper,
            IBlobStorageHelper blobStorageHelper)
        {
            _config = config;
            _logger = logger;
            _performanceLogger = performanceLogger;
            _documentActionHelperDel = documentActionHelperDel;
            _documentApprovalStatusHelper = documentApprovalStatusHelper;
            _detailsHelper = detailsHelper;
            _blobStorageHelper = blobStorageHelper;
        }

        #endregion Constructor

        #region Implemented Methods

        /// <summary>
        /// Processes the User String and formulates a proper Response Card OR Error response after taking action on request from Outlook
        /// </summary>
        /// <param name="tenantId">Tenant ID</param>
        /// <param name="request">Http request</param>
        /// <param name="clientDevice">Client Device</param>
        /// <param name="userAlias">User Alias</param>
        /// <param name="loggedInUser">Logged-in user Alias</param>
        /// <param name="aadUserToken">AAD Token</param>
        /// <param name="submissionType">Action submission type</param>
        /// <param name="xcv">X-Correlation ID</param>
        /// <param name="tcv">T-Correlation ID</param>
        /// <param name="sessionId">Session ID</param>
        /// <returns>Http Response</returns>
        public async Task<IActionResult> TakeActionFromNonWebClient(
                int tenantId,
                HttpRequest request,
                string clientDevice,
                string userAlias,
                string loggedInUser,
                string aadUserToken,
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
                { LogDataKey.UserRoleName, loggedInUser },
                { LogDataKey.TenantId, tenantId },
                { LogDataKey.UserAlias, userAlias },
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

                #endregion Get templates from Blob

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

                    if (string.IsNullOrEmpty(loggedInUser))
                    {
                        _logger.LogError(TrackingEvent.WebApiOutlookAADActionFail, new UnauthorizedAccessException(Constants.InValidSenderClaim), logData);
                        response.StatusCode = HttpStatusCode.Unauthorized;
                        response.Headers.Add(Constants.CardActionStatus, _config[ConfigurationKey.UnAuthorizedException.ToString()]);
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
                        _logger.LogError(TrackingEvent.WebApiOutlookAADActionFail, new InvalidDataException(Constants.UserActionsStringIsNull), logData);
                        response.Headers.Add(Constants.CardActionStatus, Constants.OutlookGenericErrorMessage);
                        return new HttpResponseMessageResult(response);
                    }

                    // For time being the logic can use the content being posted by the user instead of querying the data from Tenant Info Table in Approvals
                    JObject userActionObj = JObject.Parse(userActionsString);

                    // Modify User Action String to add ActionByAlias
                    userActionObj = ModifyUserAction(userActionObj, loggedInUser, clientDevice, xcv, tcv);

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

                    userActionObj[Constants.UserImage] = userImage;

                    if (actionRequestObj[Constants.DocumentKeys] != null && actionRequestObj[Constants.DocumentKeys].HasValues)
                    {
                        logData[LogDataKey.DisplayDocumentNumber] = actionRequestObj[Constants.DocumentKeys].First()[Constants.DisplayDocumentNumber] != null ? actionRequestObj[Constants.DocumentKeys].First()[Constants.DisplayDocumentNumber].ToString() : string.Empty;
                        logData[LogDataKey.DocumentNumber] = actionRequestObj[Constants.DocumentKeys].First()[Constants.DocumentNumber] != null ? actionRequestObj[Constants.DocumentKeys].First()[Constants.DocumentNumber].ToString() : string.Empty;
                    }

                    response.StatusCode = HttpStatusCode.OK;
                    try
                    {
                        var DocumentActionHelper = _documentActionHelperDel(submissionType.ToString());
                        var responseObject = await DocumentActionHelper.TakeAction(tenantId, actionRequestObj.ToString(), clientDevice, userAlias, loggedInUser, aadUserToken, xcv, tcv, sessionId);

                        string responseContent = string.Empty;
                        response.Headers.Add(Constants.CardActionStatus, Constants.ActionSuccessfulMessage);
                        response.Headers.Add(Constants.CardUpdateInBody, "true");

                        templateList = await task;
                        responseContent = MSAHelper.CreateCard(templateList[Constants.ACTIONRESPONSETEMPLATE + clientDevice + ".json"], userActionObj.ToJson());
                        response.Content = new StringContent(responseContent);

                        logData[LogDataKey.ResponseContent] = responseContent;
                        _logger.LogInformation(TrackingEvent.WebApiOutlookAADActionSuccess, logData);

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
                        response.Headers.Add(Constants.CardUpdateInBody, "true");

                        JObject messageObject = new JObject(new JProperty(Constants.MessageTitle, message));
                        templateList = await task;
                        string responseContent = MSAHelper.CreateCard(templateList[Constants.ACTIONERRORRESPONSETEMPLATE + clientDevice + ".json"], messageObject.ToJson());
                        response.Content = new StringContent(responseContent);

                        logData[LogDataKey.ResponseContent] = responseContent;
                        logData[LogDataKey.ResponseStatusCode] = response.StatusCode;
                        _logger.LogError(TrackingEvent.WebApiOutlookAADActionFail, new Exception(message), logData);

                        return new HttpResponseMessageResult(response);
                    }
                }
                catch (Exception exception)
                {
                    response.StatusCode = HttpStatusCode.BadRequest;
                    string errorMessage = JSONHelper.ExtractMessageFromJSON(exception.Message);
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
                    _logger.LogError(TrackingEvent.WebApiOutlookAADActionFail, exception, logData);

                    response.Headers.Add(Constants.CardActionStatus, string.IsNullOrEmpty(errorMessage) ? Constants.OutlookGenericErrorMessage : errorMessage);
                    return new HttpResponseMessageResult(response);
                }
            }
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
        /// <param name="tenantId">Tenant ID</param>
        /// <param name="request">Http request</param>
        /// <param name="clientDevice">Client Device</param>
        /// <param name="userAlias">User Alias</param>
        /// <param name="loggedInUser">Logged-in user Alias</param>
        /// <param name="tcv">T-correlation ID</param>
        /// <param name="sessionId">Session ID</param>
        /// <param name="xcv">X-Correlation ID</param>
        /// <returns>Https Response</returns>
        public async Task<IActionResult> ClientAutoRefresh(int tenantId, HttpRequest request, string clientDevice, string userAlias, string loggedInUser, string tcv, string sessionId, string xcv)
        {
            #region Logging Prep

            var logData = new Dictionary<LogDataKey, object>
            {
                { LogDataKey.SessionId, sessionId },
                { LogDataKey.UserRoleName, loggedInUser },
                { LogDataKey.TenantId, tenantId },
                { LogDataKey.UserAlias, userAlias },
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
                    if (string.IsNullOrEmpty(loggedInUser))
                    {
                        _logger.LogError(TrackingEvent.WebApiOutlookAADAutoRefreshFail, new UnauthorizedAccessException(Constants.InValidSenderClaim), logData);
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
                        _logger.LogError(TrackingEvent.WebApiOutlookAADAutoRefreshFail, new InvalidDataException(Constants.UserActionsStringIsNull), logData);
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

                var autoRefreshRequestObj = new { ApproverAlias = loggedInUser, DocumentNumber = documentNumber, RequestVersion = requestVersion };

                logData[LogDataKey.DisplayDocumentNumber] = documentNumber;
                logData.Modify(LogDataKey.StartDateTime, DateTime.UtcNow);

                #region Submit action to Approvals API and return the response

                using (var performanceTracer = _performanceLogger.StartPerformanceLogger("PerfLog", Constants.OutlookClient, "AutoRefresh-Card", logData))
                {
                    try
                    {
                        DocumentStatusResponse documentStatusResponse = await _documentApprovalStatusHelper.DocumentStatus(tenantId, autoRefreshRequestObj.ToJson(), clientDevice, userAlias, loggedInUser, tcv, sessionId, xcv);

                        if (documentStatusResponse != null)
                        {
                            logData.Add(LogDataKey.AppAction, documentStatusResponse.CurrentStatus);

                            response.StatusCode = HttpStatusCode.OK;

                            if (string.IsNullOrEmpty(documentStatusResponse.CurrentStatus))
                            {
                                response.StatusCode = HttpStatusCode.BadRequest;
                                _logger.LogError(TrackingEvent.WebApiOutlookAADAutoRefreshFail, new Exception(Constants.DocumentStatusAPIResponseIsNull), logData);
                            }
                            else
                            {
                                placeHolderDict = GetPlaceHolderData(documentStatusResponse);
                                placeHolderDict.Add(Constants.ApprovalIdentifierDisplayDocNumber, autoRefreshRequest.DocumentNumber);

                                switch (documentStatusResponse.CurrentStatus)
                                {
                                    case Constants.Pending:
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
                                _logger.LogInformation(TrackingEvent.WebApiOutlookAADAutoRefreshSuccess, logData);
                            }
                        }
                        else
                        {
                            logData.Modify(LogDataKey.EndDateTime, DateTime.UtcNow);
                            _logger.LogError(TrackingEvent.WebApiOutlookAADAutoRefreshFail, new Exception(Constants.DocumentStatusAPIResponseIsNull), logData);
                        }
                    }
                    catch
                    {
                        _logger.LogError(TrackingEvent.WebApiOutlookAADAutoRefreshFail, new Exception(string.Format(Constants.DocumentStatusAPIFailedWithStatusCode, HttpStatusCode.BadRequest)), logData);
                    }
                }

                #endregion Submit action to Approvals API and return the response

                return new HttpResponseMessageResult(response);
            }
            catch (Exception exception)
            {
                logData.Modify(LogDataKey.EndDateTime, DateTime.UtcNow);
                _logger.LogError(TrackingEvent.WebApiOutlookAADAutoRefreshFail, exception, logData);

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
        private JObject ModifyUserAction(JObject userActionObj, string loggedInUser, string clientDevice, string xcv, string tcv)
        {
            // Extract ActionBody from the request content
            var actionRequestObj = userActionObj[Constants.ActionBody];

            actionRequestObj[Constants.ActionByAlias] = loggedInUser;

            // to get the action details.
            JObject actionRequestDetailsObj = actionRequestObj[Constants.ActionDetailsKey].ToJson().ToJObject();

            // to replace the ActionDate Field.
            string actionDate = actionRequestDetailsObj[Constants.ActionDateKey].ToString();
            actionRequestDetailsObj[Constants.ActionDateKey] = DateTime.TryParse(actionDate, out DateTime utcNow) ? utcNow.ToString("o") : DateTime.UtcNow.ToString("o");

            var justification = actionRequestDetailsObj[Constants.JustificationKey];
            var comment = actionRequestDetailsObj[Constants.CommentKey];
            if (clientDevice.Equals(Constants.TeamsClient))
            {
                List<string> actionValues = new List<string>();
                List<string> commentValues = new List<string>();
                List<string> lineItemIdValues = new List<string>();
                foreach (var actionToken in actionRequestDetailsObj)
                {
                    if (actionToken.Value is JArray)
                    {
                        Dictionary<int, string> tempAction = new Dictionary<int, string>();
                        Dictionary<int, string> tempComment = new Dictionary<int, string>();
                        int i = 0;
                        if (actionToken.Key.Contains("LineItem"))
                        {
                            lineItemIdValues.AddRange(actionToken.Value.ToJson().FromJson<List<string>>());
                        }
                        foreach (var multipleActionToken in actionToken.Value.ToJson().ToJArray())
                        {
                            var itemId = (multipleActionToken.ToString().Split(new string[] { "{{" }, StringSplitOptions.RemoveEmptyEntries)?[0])?.Split('.')?[0];
                            if (itemId.Contains("Action"))
                                actionValues.Add(userActionObj[itemId].ToString());
                            if (itemId.Contains("Comment"))
                                commentValues.Add(userActionObj[itemId].ToString());
                            i++;
                        }
                    }
                    else if (actionToken.Value.ToString().Contains("{{") && actionToken.Value.ToString().Contains("}}"))
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

                JArray lineItemsArray = new JArray();
                for (int i = 0; i < lineItemIdValues.Count; i++)
                {
                    JObject lineItemElement = new JObject
                    {
                        { Constants.ActionBodyLineItemId, lineItemIdValues[i].ToString() },
                        { Constants.ActionKey, actionValues[i] },
                        { Constants.CommentKey, commentValues[i] }
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
}