// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace Microsoft.CFS.Approvals.Domain.BL.Tenants.Core;

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web;
using Microsoft.CFS.Approvals.Common.DL.Interface;
using Microsoft.CFS.Approvals.Contracts;
using Microsoft.CFS.Approvals.Contracts.DataContracts;
using Microsoft.CFS.Approvals.Data.Azure.Storage.Interface;
using Microsoft.CFS.Approvals.Extensions;
using Microsoft.CFS.Approvals.LogManager.Model;
using Microsoft.CFS.Approvals.LogManager.Provider.Interface;
using Microsoft.CFS.Approvals.Model;
using Microsoft.CFS.Approvals.Utilities.Extension;
using Microsoft.CFS.Approvals.Utilities.Interface;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

/// <summary>
/// Class WRT
/// </summary>
/// <seealso cref="TenantBase" />
public class WRT : TenantBase
{
    #region CONSTRUCTOR

    public WRT(
        ApprovalTenantInfo tenantInfo,
        ILogProvider logger,
        IPerformanceLogger performanceLogger,
        IApprovalSummaryProvider approvalSummaryProvider,
        IConfiguration config,
        INameResolutionHelper nameResolutionHelper,
        IApprovalDetailProvider approvalDetailProvider,
        IFlightingDataProvider flightingDataProvider,
        IApprovalHistoryProvider approvalHistoryProvider,
        IBlobStorageHelper blobStorageHelper,
        IAuthenticationHelper authenticationHelper,
        IHttpHelper httpHelper)
        : base(
              tenantInfo,
              logger,
              performanceLogger,
              approvalSummaryProvider,
              config,
              nameResolutionHelper,
              approvalDetailProvider,
              flightingDataProvider,
              approvalHistoryProvider,
              blobStorageHelper,
              authenticationHelper,
              httpHelper)
    {
    }

    public WRT(
        ApprovalTenantInfo tenantInfo,
        string alias,
        string clientDevice,
        string oauth2Token,
        ILogProvider logger,
        IPerformanceLogger performanceLogger,
        IApprovalSummaryProvider approvalSummaryProvider,
        IConfiguration config,
        INameResolutionHelper nameResolutionHelper,
        IApprovalDetailProvider approvalDetailProvider,
        IFlightingDataProvider flightingDataProvider,
        IApprovalHistoryProvider approvalHistoryProvider,
        IBlobStorageHelper blobStorageHelper,
        IAuthenticationHelper authenticationHelper,
        IHttpHelper httpHelper,
        string objectId,
        string domain)
        : base(
              tenantInfo,
              alias,
              clientDevice,
              oauth2Token,
              logger,
              performanceLogger,
              approvalSummaryProvider,
              config,
              nameResolutionHelper,
              approvalDetailProvider,
              flightingDataProvider,
              approvalHistoryProvider,
              blobStorageHelper,
              authenticationHelper,
              httpHelper, 
              objectId,
              domain)
    {
    }

    #endregion CONSTRUCTOR

    #region Convert JSON Methods

    protected override async Task<SummaryJson> ExtractJSONDetails(ApprovalRequestExpression approvalRequest, JObject summaryJsonObject)
    {
        SummaryJson summaryJson = new SummaryJson
        {
            AdditionalData = new Dictionary<string, string>()
        };
        if (summaryJsonObject.TryGetValue(Constants.WireRequestID, out JToken value))
        {
            string documentNumber = value.ToString();
            summaryJson.ApprovalIdentifier =
                    new ApprovalIdentifier()
                    {
                        DisplayDocumentNumber = documentNumber,
                        DocumentNumber = documentNumber,
                        FiscalYear = null
                    };

            value.Parent.Remove();
        }

        if (summaryJsonObject.TryGetValue("ReqTypeName", out value))
        {
            summaryJson.CustomAttribute =
                new CustomAttribute()
                {
                    CustomAttributeName = "ReqTypeName",
                    CustomAttributeValue = value.ToString()
                };

            value.Parent.Remove();
        }

        if (summaryJsonObject.TryGetValue("RequestorAlias", out value))
        {
            string submitterAlias = value.ToString();
            string submitterName = await GetUserDisplayName(submitterAlias);
            summaryJson.Submitter =
                new User()
                {
                    Name = submitterName,
                    Alias = submitterAlias
                };

            value.Parent.Remove();
        }

        JArray currentApprovers = await GetCurrentApprovers(summaryJsonObject, "CurrentApprover", approvalRequest);
        approvalRequest.Approvers = new List<Approver>();

        foreach (JToken approver in currentApprovers)
        {
            string approverAlias = null;
            string approverName = null;

            if (approver.Type != JTokenType.Null)
            {
                approverAlias = approver.ToString();
                approverName = await GetUserDisplayName(approverAlias);
            }

            approvalRequest.Approvers.Add(
                    new Approver()
                    {
                        Name = approverName,
                        Alias = approverAlias
                    });
        }
        summaryJsonObject.Remove("CurrentApprover");

        DateTime.TryParse(ReadAndRemoveToken(summaryJsonObject, "CreatedDate"), out DateTime submittedDate);
        summaryJson.SubmittedDate = submittedDate;

        summaryJson.Title = ReadAndRemoveToken(summaryJsonObject, "WireText");

        summaryJson.UnitOfMeasure = ReadAndRemoveToken(summaryJsonObject, "Currency");

        summaryJson.UnitValue = ReadAndRemoveToken(summaryJsonObject, "Amount");
        summaryJson.CompanyCode = ReadAndRemoveToken(summaryJsonObject, "DebitEntity");

        return summaryJson;
    }

    #endregion Convert JSON Methods

    #region GET DETAIL

    protected override async Task<string> GetDetailURL(string urlFormat, ApprovalIdentifier approvalIdentifier, int page, string xcv = "", string tcv = "", string businessProcessName = "", string docTypeId = "")
    {
        return String.Format(urlFormat, HttpUtility.UrlEncode(approvalIdentifier.DocumentNumber), Alias);
    }

    protected override string FutureApproverChainOperationName()
    {
        return Constants.WRTDetailsAction;
    }

    protected override async Task<HttpResponseMessage> ExecutePostDetailOperationAsync(HttpResponseMessage lobResponse, string operation, ApprovalIdentifier approvalIdentifier)
    {
        string responseString = await lobResponse.Content.ReadAsStringAsync();
        if (!String.IsNullOrWhiteSpace(responseString))
        {
            // The WRT Json Attachments collection has AttachmentId and ActualFileName fields,
            // which we want to map to the standard ID and Name fields.
            JObject responseObject = responseString.ToJObject();
            JArray attachments = responseObject["Attachments"] as JArray;
            foreach (JToken attachment in attachments)
            {
                string attachmentId = (null == attachment["AttachmentName"]) ? String.Empty : attachment["AttachmentName"].ToString();
                string attachmentName = (null == attachment["ActualFileName"]) ? String.Empty : attachment["ActualFileName"].ToString();

                if (!String.IsNullOrWhiteSpace(attachmentId))
                    attachment["ID"] = attachmentId;

                if (!String.IsNullOrWhiteSpace(attachmentName))
                    attachment["Name"] = attachmentName;
            }
            responseObject["Attachments"] = attachments;
            lobResponse.Content = new StringContent(responseObject.ToString());
        }

        lobResponse = await base.ExecutePostDetailOperationAsync(lobResponse, operation, approvalIdentifier);
        var response = await lobResponse.Content.ReadAsStringAsync();

        // The WRT details Json  has VendorComments field,
        // which we want to map to the ApproverNotes field as per the UI bindings.
        response = MapApproverNotes(response, "Comments");
        return new HttpResponseMessage() { Content = new StringContent(response), StatusCode = lobResponse.StatusCode };
    }

    #endregion GET DETAIL

    #region GET SUMMARY

    protected override string GetSummaryUrl(ApprovalTenantInfo tenantInfo, string documentSummaryUrl, ApprovalRequestExpression approvalRequest, string docTypeId = "")
    {
        approvalRequest.AdditionalData.TryGetValue(Constants.Approver, out string approver);
        return string.Format(documentSummaryUrl, approvalRequest.ApprovalIdentifier.GetDocNumber(tenantInfo), approver);
    }

    #endregion GET SUMMARY

    #region DocumentAction Methods

    /// <summary>
    /// Executes the action asynchronously and overriddes the ExexuteActionAsync of TenantBase
    /// </summary>
    /// <param name="approvalRequests">The list of approval request.</param>
    /// <param name="loggedInAlias">The logged in alias.</param>
    /// <param name="sessionId">The session identifier.</param>
    /// <param name="clientDevice">The client device.</param>
    /// <param name="tcv">The Tcv.</param>
    /// <param name="xcv">The Xcv.</param>
    /// <param name="summaryRowParent">The summary row parent.</param>
    /// <returns>returns task containing HttpResponseMessage</returns>
    public override async Task<HttpResponseMessage> ExecuteActionAsync(List<ApprovalRequest> approvalRequests, string loggedInAlias, string sessionId, string clientDevice, string xcv, string tcv, ApprovalSummaryRow summaryRowParent = null)
    {
        var tenantId = approvalTenantInfo.TenantId;
        var approvalRequest = approvalRequests.FirstOrDefault();
        if (approvalRequest == null)
        {
            throw new InvalidDataException("Invalid ApprovalRequest object");
        }
        var logData = new Dictionary<LogDataKey, object>
        {
            { LogDataKey.Tcv, tcv },
            { LogDataKey.SessionId, sessionId },
            { LogDataKey.Xcv, xcv },
            { LogDataKey.ClientDevice, clientDevice },
            { LogDataKey.TenantId, tenantId },
            { LogDataKey.UserAlias, Alias },
            { LogDataKey.UserRoleName, loggedInAlias }
        };
        ApprovalSummaryRow summary = null;
        string actionContentForSubmissionIntoTenantService = string.Empty;
        HttpResponseMessage response = null;
        try
        {
            if (approvalRequest != null)
            {
                SetupGenericErrorMessage(approvalRequest);

                logData.Add(LogDataKey.UserActionsString, approvalRequest.ToJson());

                string documentNumber = approvalRequest.ApprovalIdentifier.GetDocNumber(approvalTenantInfo);

                logData.Add(LogDataKey.DocumentNumber, documentNumber);
                logData.Add(LogDataKey.DXcv, documentNumber);
                logData.Add(LogDataKey.BusinessProcessName, string.Format(approvalTenantInfo.BusinessProcessName, Constants.BusinessProcessNameApprovalAction, approvalRequest.Action));

                summary = ApprovalSummaryProvider.GetApprovalSummaryByDocumentNumberAndApprover(approvalTenantInfo.DocTypeId, documentNumber, Alias, ObjectId, Domain);
                if (summary == null)
                {
                    throw new UnauthorizedAccessException(JObject.FromObject(new { Message = Config[ConfigurationKey.Message_UnAuthorizedUser.ToString()] }).ToString(Formatting.None));
                }
                HttpResponseMessage wrtMessage = null;
                try
                {
                    wrtMessage = await GetDetailAsync(new ApprovalIdentifier() { DocumentNumber = documentNumber, DisplayDocumentNumber = documentNumber }, "DT1", 0, loggedInAlias, approvalRequest.Telemetry.Xcv, approvalRequest.Telemetry.Tcv, approvalRequest.Telemetry.BusinessProcessName, false, clientDevice);
                }
                catch
                {
                    throw new InvalidOperationException(JObject.FromObject(new { Message = approvalTenantInfo.AppName + " service is down" }).ToString(Formatting.None));
                }
                actionContentForSubmissionIntoTenantService = await wrtMessage.Content.ReadAsStringAsync();
                JObject wrtObject = (actionContentForSubmissionIntoTenantService).ToJObject();
                wrtObject["Comments"] = approvalRequest.ActionDetails.ContainsKey("Comment") ? approvalRequest.ActionDetails["Comment"] : string.Empty;
                actionContentForSubmissionIntoTenantService = wrtObject.ToJson();
            }

            string digitalSignature = approvalRequest.ActionDetails.ContainsKey("DigitalSignature") ? approvalRequest.ActionDetails["DigitalSignature"] : string.Empty;

            HttpMethod reqHttpMethod = GetHttpMethodForAction();
            string endPointUrl = GetTenantActionUrl(digitalSignature);
            if (String.IsNullOrEmpty(endPointUrl))
                throw new UriFormatException(Config[ConfigurationKey.Message_URLNotDefined.ToString()]);

            HttpRequestMessage reqMessage = await CreateRequestForDetailsOrAction(reqHttpMethod, endPointUrl);
            if (!string.IsNullOrEmpty(digitalSignature))
            {
                reqMessage.Headers.Add("DigitalSignature", digitalSignature);
            }
            if (!String.IsNullOrEmpty(actionContentForSubmissionIntoTenantService))
            {
                reqMessage.Content = new StringContent(actionContentForSubmissionIntoTenantService, System.Text.UTF8Encoding.UTF8, Constants.ContentTypeJson);
            }

            using (var trace1 = PerformanceLogger.StartPerformanceLogger("PerfLog", string.IsNullOrWhiteSpace(clientDevice) ? Constants.WebClient : clientDevice, string.Format(Constants.PerfLogActionWithInfo, approvalTenantInfo.AppName, approvalRequest.Action, "Document Action"), logData))
            {
                logData[LogDataKey.ReceivedTcv] = summary.Tcv;
                logData[LogDataKey.CustomEventName] = approvalTenantInfo.AppName + "-" + OperationType.ActionInitiated;
                Logger.LogInformation(GetEventId(OperationType.ActionInitiated), logData);
                response = await SendRequestAsync(reqMessage, logData, clientDevice);

                logData[LogDataKey.CustomEventName] = approvalTenantInfo.AppName + "-" + OperationType.ActionComplete;
                logData[LogDataKey.ResponseStatusCode] = response.StatusCode;
                Logger.LogInformation(GetEventId(OperationType.ActionComplete), logData);
            }

            //Sending Response
            return await FormulateActionResponse(response, approvalRequests, loggedInAlias, sessionId, clientDevice, tcv);
        }
        catch (Exception ex)
        {
            logData[LogDataKey.CustomEventName] = approvalTenantInfo.AppName + "-" + OperationType.Action;
            logData[LogDataKey.ResponseContent] = await response?.Content?.ReadAsStringAsync();
            Logger.LogError(GetEventId(OperationType.Action), ex, logData);
            throw;
        }
    }

    protected override string GetTenantActionUrl(string digitalSignature, string action = "")
    {
        string endPointUrl = string.Empty;
        if (action.Equals(Constants.OutOfSyncAction))
            approvalTenantInfo.GetEndPointURL(Constants.OperationTypeOutOfSync, Constants.WebClient);
        else
            endPointUrl = approvalTenantInfo.GetEndPointURL(Constants.OperationTypeAction, ClientDevice);
        if (!string.IsNullOrEmpty(digitalSignature))
        {
            //Approve case
            endPointUrl = string.Format(endPointUrl, digitalSignature, Alias);
        }
        else
        {
            //Reject case
            endPointUrl = string.Format(endPointUrl, String.Empty, Alias);
            endPointUrl = endPointUrl.Replace(Constants.digitalSignatureReplace, String.Empty);
        }
        return endPointUrl;
    }

    protected override HttpMethod GetHttpMethodForAction()
    {
        return HttpMethod.Put;
    }

    #endregion DocumentAction Methods

    #region Download Document

    public override string AttachmentDetailsOperationName()
    {
        return Constants.WRTDetailsAction;
    }

    /// <summary>
    /// Gets the detail URL using attachment identifier.
    /// </summary>
    /// <param name="urlFormat">The URL format.</param>
    /// <param name="attachmentId">The attachment identifier.</param>
    /// <param name="approvalIdentifier"></param>
    protected override string GetAttachmentDownloadUrl(string urlFormat, string attachmentId, ApprovalIdentifier approvalIdentifier)
    {
        return String.Format(urlFormat, HttpUtility.UrlEncode(attachmentId), approvalIdentifier.GetDocNumber(approvalTenantInfo));
    }
    
    #endregion Download Document
}