// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace Microsoft.CFS.Approvals.Domain.BL.Interface;

using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.CFS.Approvals.Contracts;
using Microsoft.CFS.Approvals.Contracts.DataContracts;
using Microsoft.CFS.Approvals.LogManager.Model;
using Microsoft.CFS.Approvals.Model;
using Newtonsoft.Json.Linq;

public interface ITenant
{
    /// <summary>
    /// To handle error message from tenant. This is not a permanent fix and we have review this flow to correct the contract of error message we receive from tenants.
    /// </summary>
    string GenericErrorMessage { get; set; }

    /// <summary>
    /// Gets the summary from tenant asynchronous.
    /// </summary>
    /// <param name="approvalRequest">The approval request.</param>
    /// <param name="loggedInAlias">The logged in alias.</param>
    /// <param name="telemetry">The telemetry.</param>
    /// <returns>returns a list containing approval summary row</returns>
    Task<List<ApprovalSummaryRow>> GetSummaryFromTenantAsync(ApprovalRequestExpression approvalRequest, string loggedInAlias, ApprovalsTelemetry telemetry);

    /// <summary>
    /// Gets the delegated users asynchronously.
    /// </summary>
    /// <param name="alias">User alias.</param>
    /// <param name="parameters">Key-value pair for filtering parameters.</param>
    /// <param name="clientDevice">The clientDevice.</param>
    /// <param name="xcv">The XCV.</param>
    /// <param name="tcv">The TCV.</param>
    /// <param name="sessionId">Current user session Id.</param>
    /// <returns>returns delegated users.</returns>
    Task<HttpResponseMessage> GetUsersDelegatedToAsync(string alias, Dictionary<string, object> parameters, string clientDevice, string xcv, string tcv, string sessionId);

    /// <summary>
    /// Gets the tenant action details asynchronously.
    /// </summary>
    /// <param name="alias">The alias.</param>
    /// <param name="loggedInAlias">The loggedInAlias.</param>
    /// <param name="clientDevice">The clientDevice.</param>
    /// <param name="sessionId">The sessionId.</param>
    /// <param name="xcv">The xcv.</param>
    /// <param name="tcv">The tcv.</param>
    /// <returns></returns>
    Task<HttpResponseMessage> GetTenantActionDetails(string alias, string loggedInAlias, string clientDevice, string sessionId, string xcv, string tcv);

    /// <summary>
    /// Gets the summary from arx.
    /// </summary>
    /// <param name="approvalRequest">The approval request.</param>
    /// <param name="summaryJson">The summary json.</param>
    /// <returns>returns a list containing approval summary row</returns>
    Task<List<ApprovalSummaryRow>> GetSummaryFromARX(ApprovalRequestExpression approvalRequest, SummaryJson summaryJson);

    /// <summary>
    /// Gets the detail asynchronous.
    /// </summary>
    /// <param name="approvalIdentifier">The approval identifier.</param>
    /// <param name="operation">The operation.</param>
    /// <param name="page">The page.</param>
    /// <param name="loggedInAlias">The logged in alias.</param>
    /// <param name="xcv">The XCV.</param>
    /// <param name="tcv">The TCV.</param>
    /// <param name="businessProcessName">Name of the business process.</param>
    /// <param name="isUserTriggered">if set to <c>true</c> [is user triggered].</param>
    /// <param name="clientDevice">The client device.</param>
    /// <returns>returns a Http response message</returns>
    Task<HttpResponseMessage> GetDetailAsync(ApprovalIdentifier approvalIdentifier, string operation, int page, string loggedInAlias = "", string xcv = "", string tcv = "", string businessProcessName = "", bool isUserTriggered = false, string clientDevice = Constants.WorkerRole);

    /// <summary>
    /// Loads the detail asynchronous.
    /// </summary>
    /// <param name="tenantInfo">The tenant information.</param>
    /// <param name="approvalIdentifier">The approval identifier.</param>
    /// <param name="operation">The operation.</param>
    /// <param name="page">The page.</param>
    /// <param name="loggedInAlias">The logged in alias.</param>
    /// <param name="xcv">The XCV.</param>
    /// <param name="tcv">The TCV.</param>
    /// <param name="clientDevice">The client device.</param>
    /// <returns>returns a http response message</returns>
    Task<HttpResponseMessage> LoadDetailAsync(ApprovalTenantInfo tenantInfo, ApprovalIdentifier approvalIdentifier, string operation, int page, string loggedInAlias, string xcv, string tcv, string clientDevice);

    /// <summary>
    /// Executes the action asynchronous.
    /// </summary>
    /// <param name="approvalRequests">The list of approval requests.</param>
    /// <param name="loggedInAlias">The logged in alias.</param>
    /// <param name="sessionId">The session identifier.</param>
    /// <param name="clientDevice">The client device.</param>
    /// <param name="xcv">The xcv.</param>
    /// <param name="tcv">The tcv.</param>
    /// <param name="summaryRowParent">The summary entity.</param>
    /// <returns>returns a HttpResponseMessage</returns>
    Task<HttpResponseMessage> ExecuteActionAsync(List<ApprovalRequest> approvalRequests, string loggedInAlias, string sessionId, string clientDevice, string xcv, string tcv, ApprovalSummaryRow summaryRowParent);

    /// <summary>
    /// Executes the post authentication sum.
    /// </summary>
    /// <param name="authSumObject">The authentication sum object with available details data</param>
    /// <returns>returns a JObject</returns>
    JObject ExecutePostAuthSum(JObject authSumObject);

    /// <summary>
    /// Removes certain fields from Authsum Object
    /// </summary>
    /// <param name="summaryAuthsumObject">The authentication sum object</param>
    /// <returns>authentication sum object after removal of fields</returns>
    JObject RemoveFieldsFromResponse(JObject authsumObject);

    /// <summary>
    /// Posts the process details.
    /// </summary>
    /// <param name="jsonDetail">The json detail.</param>
    /// <param name="operation">The operation.</param>
    /// <returns>returns a string</returns>
    string PostProcessDetails(string jsonDetail, string operation);

    /// <summary>
    /// Adds the editable fields properties.
    /// </summary>
    /// <param name="jsonDetail">The json detail.</param>
    /// <param name="operation">The operation.</param>
    /// <returns>returns a string</returns>
    string AddEditableFieldsProperties(string jsonDetail, string operation);

    /// <summary>
    /// Creates the future approver chain.
    /// </summary>
    /// <param name="approverChainFromTenant">The approver chain from tenant.</param>
    /// <param name="documentSummary">The document summary.</param>
    /// <param name="alias">The alias.</param>
    /// <returns>returns a JArray object</returns>
    JArray CreateFutureApproverChain(JArray approverChainFromTenant, ApprovalSummaryRow documentSummary, string alias);

    /// <summary>
    /// Extracts the approver chain.
    /// </summary>
    /// <param name="responseString">The response string.</param>
    /// <param name="currentApprover">The current approver.</param>
    /// <param name="loggedInUser">The logged in user.</param>
    /// <returns>returns a JArray object</returns>
    JArray ExtractApproverChain(string responseString, string currentApprover, string loggedInUser);

    /// <summary>
    /// Gets the lob response asynchronous.
    /// </summary>
    /// <param name="approvalIdentifier">The approval identifier.</param>
    /// <param name="operation">The operation.</param>
    /// <param name="page">The page.</param>
    /// <param name="xcv">The XCV.</param>
    /// <param name="tcv">The TCV.</param>
    /// <param name="businessProcessName">Name of the business process.</param>
    /// <param name="documentTypeId">The document type identifier.</param>
    /// <returns>returns a Http response message</returns>
    Task<HttpResponseMessage> GetLobResponseAsync(ApprovalIdentifier approvalIdentifier, string operation, int page, string xcv, string tcv, string businessProcessName, string documentTypeId);

    /// <summary>
    /// Downloads the document using attachment identifier asynchronous.
    /// </summary>
    /// <param name="approvalIdentifier">The approval identifier.</param>
    /// <param name="attachmentId">The attachment identifier.</param>
    /// <param name="telemetry">The telemetry.</param>
    /// <returns>returns a Http response message</returns>
    Task<byte[]> DownloadDocumentUsingAttachmentIdAsync(ApprovalIdentifier approvalIdentifier, string attachmentId, ApprovalsTelemetry telemetry);

    /// <summary>
    /// Previews the document using attachment identifier asynchronous.
    /// </summary>
    /// <param name="approvalIdentifier">The approval identifier.</param>
    /// <param name="attachmentId">The attachment identifier.</param>
    /// <param name="telemetry">The telemetry.</param>
    /// <returns>returns a Http response message</returns>
    Task<byte[]> PreviewDocumentUsingAttachmentIdAsync(ApprovalIdentifier approvalIdentifier, string attachmentId, ApprovalsTelemetry telemetry);

    /// <summary>
    /// Downloads the attachments in bulk for the given set of requests
    /// </summary>
    /// <param name="approvalRequests">List of ApprovalRequest which is sent to the LoB application as part of the content in the Http call</param>
    /// <param name="loggedInAlias">Alias of the logged in User</param>
    /// <param name="sessionId">GUID session id</param>
    /// <param name="clientDevice">Client Device (Web/WP8..)</param>
    /// <returns>HttpResponseMessage with Stream data of all the attachments</returns>
    Task<byte[]> BulkDownloadDocumentAsync(List<ApprovalRequest> approvalRequests, string loggedInAlias, string sessionId, string clientDevice);

    /// <summary>
    /// Gets the attachment content from lob.
    /// </summary>
    /// <param name="approvalIdentifier">The approval identifier.</param>
    /// <param name="attachmentId">The attachment identifier.</param>
    /// <param name="telemetry">The telemetry.</param>
    /// <returns>returns a Http response message</returns>
    Task<byte[]> GetAttachmentContentFromLob(ApprovalIdentifier approvalIdentifier, string attachmentId, ApprovalsTelemetry telemetry);

    /// <summary>
    /// Updates the summary row and Approval details entity.
    /// </summary>
    /// <param name="approvalRequest">The approval request.</param>
    /// <param name="isSoftDelete">if set to <c>true</c> [is soft delete].</param>
    /// <param name="exceptionMessage">The exception message.</param>
    /// <param name="loggedInAlias">The logged in alias.</param>
    /// <param name="sessionId">The session identifier.</param>
    /// <param name="summaryRowParent">The summary row parent.</param>
    /// <returns>returns tuple of ApprovalSummaryRow and List of ApprovalDetailsEntity</returns>
    Tuple<ApprovalSummaryRow, List<ApprovalDetailsEntity>> UpdateTransactionalDetails(ApprovalRequest approvalRequest, bool isSoftDelete, string exceptionMessage, string loggedInAlias, string sessionId, ApprovalSummaryRow summaryRowParent = null);

    /// <summary>
    /// Check if HttpStatusCode.NotFound should be treated as error or not for a specific Tenant
    /// </summary>
    /// <returns>returns false if HttpStatus.NotFound is not supposed to be treated as error</returns>
    bool TreatNotFoundAsError();

    /// <summary>
    /// Validates if email should be sent with details.
    /// </summary>
    /// <param name="documentSummaries">The document summaries.</param>
    /// <returns>returns a boolean value</returns>
    bool ValidateIfEmailShouldBeSentWithDetails(List<ApprovalSummaryRow> documentSummaries);

    /// <summary>
    /// Send regular email if user is not added into ActionableEmail flighting feature
    /// </summary>
    /// <param name="isActionableEmailSent"></param>
    /// <returns></returns>
    bool ShouldSendRegularEmail(bool isActionableEmailSent);

    /// <summary>
    /// Payloads the processing response.
    /// </summary>
    /// <param name="payloadProcessingResult">The payload processing result.</param>
    /// <returns>returns a JObject</returns>
    JObject PayloadProcessingResponse(PayloadProcessingResult payloadProcessingResult);

    /// <summary>
    /// Attachments the name of the operation.
    /// </summary>
    /// <returns>returns a string</returns>
    string AttachmentOperationName();

    /// <summary>
    /// Attachments the name of the details operation.
    /// </summary>
    /// <returns>returns a string</returns>
    string AttachmentDetailsOperationName();

    /// <summary>
    /// Sets the Generic Error Message with support link
    /// </summary>
    /// <param name="approvalRequest">Approval Request object</param>
    void SetupGenericErrorMessage(ApprovalRequest approvalRequest);

    /// <summary>
    /// Modifies the approval request expression.
    /// </summary>
    /// <param name="requestExpressions">The request expressions.</param>
    /// <returns>returns a list containing Approval request expression ext.</returns>
    List<ApprovalRequestExpressionExt> ModifyApprovalRequestExpression(List<ApprovalRequestExpressionExt> requestExpressions);

    /// <summary>
    /// Method to extract AdditionalData from SummaryData and add it into DetailsData
    /// </summary>
    /// <param name="arxExtended">Approval Request Expression</param>
    /// <param name="summaryJson">Summary Data</param>
    /// <param name="additionalDetails">Additional Data</param>
    void AddAdditionalDataToDetailsData(ApprovalRequestExpressionExt arxExtended, SummaryJson summaryJson, string additionalDetails);

    /// <summary>
    /// Gets the attachment details.
    /// </summary>
    /// <param name="summaryRows">The summary rows.</param>
    /// <param name="approvalIdentifier">The approval identifier.</param>
    /// <param name="telemetry">The telemetry.</param>
    /// <returns>returns a list of attachments</returns>
    Task<List<Attachment>> GetAttachmentDetails(List<ApprovalSummaryRow> summaryRows, ApprovalIdentifier approvalIdentifier, ApprovalsTelemetry telemetry);

    /// <summary>
    /// Constructs the dynamic HTML details for email.
    /// </summary>
    /// <param name="responseJObject">The response j object.</param>
    /// <param name="templateList">The template list.</param>
    /// <param name="displayDocumentNumber">The display document number.</param>
    /// <param name="placeHolderDict">The place holders.</param>
    /// <param name="summaryRows">The summary row.</param>
    /// <returns>returns a string</returns>
    string ConstructDynamicHtmlDetailsForEmail(JObject responseJObject, IDictionary<string, string> templateList, string displayDocumentNumber, ref Dictionary<string, string> placeHolderDict, List<ApprovalSummaryRow> summaryRows, ref EmailType emailType);

    /// <summary>
    /// Generates summary and details adaptive card and merge into single card
    /// </summary>
    /// <param name="responseJObject">details data object</param>
    /// <param name="documentNumber">Document Number</param>
    /// <param name="approver">Approver alias</param>
    /// <param name="xcv">Xcv</param>
    /// <param name="tcv">Tcv</param>
    /// <param name="templateList">Template List</param>
    /// <param name="logData">Log Data</param>
    /// <returns>Adaptive Card JObject</returns>
    Task<JObject> GenerateAndMergeSummaryAndDetailAdaptiveCard(JObject responseJObject, string documentNumber, string approver, string xcv, string tcv,
        IDictionary<string, string> templateList, Dictionary<LogDataKey, object> logData);

    /// <summary>
    /// Creates Summary Adaptive Card
    /// </summary>
    /// <param name="responseJObject">details JObject</param>
    /// <param name="templateList">list of templates</param>
    /// <param name="logData">log Data object</param>
    /// <returns>returns the summary adaptive card</returns>
    Task<string> GenerateSummaryAdaptiveCard(JObject responseJObject, IDictionary<string, string> templateList, Dictionary<LogDataKey, object> logData);

    /// <summary>
    /// Creates Details Adaptive Card
    /// </summary>
    /// <param name="responseJObject">details JObject</param>
    /// <param name="templateList">list of templates</param>
    /// <param name="documentNumber">Document Number</param>
    /// <param name="approver">Approver</param>
    /// <param name="xcv">Xcv</param>
    /// <param name="tcv">Tcv</param>
    /// <param name="logData">log Data object</param>
    /// <returns>returns the details adaptive card</returns>
    string GenerateAndAddDetailsAdaptiveCard(JObject responseJObject, IDictionary<string, string> templateList, string documentNumber, string approver, string xcv, string tcv, Dictionary<LogDataKey, object> logData);

    /// <summary>
    /// Gets the complete adaptive template by Client device
    /// </summary>
    /// <param name="templateList">List of templates</param>
    /// <returns>template</returns>
    JObject GetAdaptiveTemplate(Dictionary<string, string> templateList);

    /// <summary>
    /// Gets the adaptive summary template by Client device
    /// </summary>
    /// <param name="templateList">List of templates</param>
    /// <returns>template</returns>
    string GetSummaryAdaptiveTemplate(IDictionary<string, string> templateList);

    /// <summary>
    /// Gets the adaptive body and action template by Client device and formulates into a single template
    /// </summary>
    /// <param name="templateList">List of templates</param>
    /// <returns>template</returns>
    string GetDetailAdaptiveTemplate(IDictionary<string, string> templateList);

    /// <summary>
    /// Generates using template and data
    /// Modifies the adaptive card
    /// </summary>
    /// <param name="template">adaptive card template</param>
    /// <param name="responseJObject">details data Object</param>
    /// <param name="logData">log data</param>
    /// <returns>Adaprive Card</returns>
    JObject GenerateAndModifyAdaptiveCard(string template, JObject responseJObject, Dictionary<LogDataKey, object> logData);

    /// <summary>
    /// Gets the adaptive card json from the ApprovalDetails table
    /// </summary>
    /// <param name="documentNumber">Document Number</param>
    /// <param name="adaptiveCard">out parameter which has adaptive card value</param>
    /// <returns></returns>
    bool CheckIfExistsAndGetCachedAdaptiveCard(string documentNumber, out JObject adaptiveCard);

    /// <summary>
    /// Method to get dictionary object containing additional data
    /// </summary>
    /// <param name="additionalData"> additional data in form of dictinary string</param>
    /// <param name="displayDocumentNumber">display document number</param>
    /// <param name="tenantId">tenant id</param>
    /// <returns>returns a dictionary oject containing additional data</returns>
    Dictionary<string, string> GetAdditionalData(Dictionary<string, string> additionalData, string displayDocumentNumber, int tenantId);

    /// <summary>
    /// Gets the email aliases of approvers for whom the notification should be sent.
    /// </summary>
    /// <param name="summaryRow">The summary row</param>
    /// <param name="notificationDetails">Notification details object</param>
    /// <returns>An email alias(es) to whom the notification should be sent</returns>
    string GetApproverListForSendingNotifications(ApprovalSummaryRow summaryRow, NotificationDetail notificationDetails);

    /// <summary>
    /// Reformat value of Jtoken based on datatype and format
    /// </summary>
    /// <typeparam name="T">DataType e.g DateTime,double etc</typeparam>
    /// <param name="jToken">jToken</param>
    /// <param name="tokenName">tokenName</param>
    /// <param name="format">format which support string format</param>
    void ApplyFormatting<T>(JToken jToken, string tokenName, string format);

    /// <summary>
    /// Apply format in jToken data
    /// </summary>
    /// <param name="jToken">jToken data</param>
    void ApplyFormatInJsonData(JToken jToken);

    /// <summary>
    /// Returns the Title and Description which needs to be stored in database for display purpose
    /// </summary>
    /// <param name="jObject"></param>
    /// <returns></returns>
    string GetRequestTitleDescription(JObject jObject);

    /// <summary>
    /// This method will parse response and prepares list of ApprovalResponse
    /// </summary>
    /// <param name="responseString">The responseString.</param>
    /// <returns>List of ApprovalResponse</returns>
    T ParseResponseString<T>(string responseString);

    /// <summary>
    /// This method will be used to return SummaryOperationTypes
    /// </summary>
    /// <returns>List of string</returns>
    List<string> GetSummaryOperationTypes();

    /// <summary>
    /// Creates the request for Notification.
    /// </summary>
    /// <param name="method">The method.</param>
    /// <param name="uri">The URI.</param>
    /// <param name="Xcv">Xcv</param>
    /// <param name="Tcv">Tcv</param>
    /// <returns>returns Http Request Message</returns>
    Task<HttpRequestMessage> CreateRequestForNotification(HttpMethod method, string uri, string Xcv = "", string Tcv = "");

    /// <summary>
    /// Gets the summary asynchronously for pull-model tenants.
    /// </summary>
    /// <param name="parameters">Key-value pair for filtering parameters.</param>
    /// <param name="approverAlias">Approver alias.</param>
    /// <param name="loggedInAlias">Logged-in user alias.</param>
    /// <param name="xcv">The XCV.</param>
    /// <param name="tcv">The TCV.</param>
    /// <param name="sessionId">Current user session Id.</param>
    /// <returns>returns summary records.</returns>
    Task<HttpResponseMessage> GetTenantSummaryAsync(Dictionary<string, Object> parameters, string approverAlias, string loggedInAlias, string xcv, string tcv, string sessionId);

    /// <summary>
    /// Gets the request details asynchronously for pull-model tenant.
    /// </summary>
    /// <param name="operationType">Type of operation which needs to be executed from tenant info configuration.</param>
    /// <param name="parameters">Key-value pair for filtering parameters.</param>
    /// <param name="approverAlias">Approver alias.</param>
    /// <param name="loggedInAlias">Logged-in user alias.</param>
    /// <param name="clientDevice">The clientDevice.</param>
    /// <param name="xcv">The XCV.</param>
    /// <param name="tcv">The TCV.</param>
    /// <param name="sessionId">Current user session Id.</param>
    /// <returns>returns request details.</returns>
    Task<HttpResponseMessage> GetTenantDetailsAsync(string operationType, Dictionary<string, object> parameters, string approverAlias, string loggedInAlias, string clientDevice, string xcv, string tcv, string sessionId);

    /// <summary>
    /// Get Approval Summary By RowKey And Approver
    /// </summary>
    /// <param name="rowKey"></param>
    /// <param name="approver"></param>
    /// <param name="fiscalYear"></param>
    /// <param name="tenantInfo"></param>
    /// <returns></returns>
    ApprovalSummaryRow GetApprovalSummaryByRowKeyAndApprover(string rowKey, string approver, string fiscalYear, ApprovalTenantInfo tenantInfo);
}