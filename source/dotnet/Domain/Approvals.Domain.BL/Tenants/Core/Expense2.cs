// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.
namespace Microsoft.CFS.Approvals.Domain.BL.Tenants.Core;

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
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
using Microsoft.CFS.Approvals.Utilities.Helpers;
using Microsoft.CFS.Approvals.Utilities.Interface;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json.Linq;
using ReminderDetail = Contracts.DataContracts.ReminderDetail;

/// <summary>
/// Class Expense2
/// </summary>
/// <seealso cref="GenericTenant" />
public class Expense2 : GenericTenant
{
    public const string ReceiptsRequired = "MS_IT_ReceiptsRequired";
    public const string DocumentDownloadAction = "REC";

    #region CONSTRUCTOR

    public Expense2(
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
        : base(tenantInfo,
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

    public Expense2(ApprovalTenantInfo tenantInfo,
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
        : base(tenantInfo,
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

    #region GET DETAIL

    // GetDetails method is common code executed from TenantBase

    public override JObject ExecutePostAuthSum(JObject authSumObject)
    {
        return (MapApproverNotes(AddMessages(authSumObject.ToJson()), (authSumObject != null && authSumObject["AdditionalData"] != null && authSumObject["AdditionalData"].Any() && authSumObject["AdditionalData"]["MS_IT_Comments"] != null) ? authSumObject["AdditionalData"]["MS_IT_Comments"].ToString() : string.Empty)).ToJObject();
    }

    public override string PostProcessDetails(string jsonDetail, string operation)
    {
        switch (operation)
        {
            case "DT1":
                if (!string.IsNullOrEmpty(jsonDetail))
                {
                    // Iterate over each line item and convert the attchments details
                    JObject lineDetailsObj = jsonDetail.ToJObject();
                    if (lineDetailsObj["LineItems"] != null && !string.IsNullOrEmpty(lineDetailsObj["LineItems"].ToString()))
                    {
                        IEnumerable<dynamic> lineItemObjects = lineDetailsObj["LineItems"].ToString().FromJson<IEnumerable<dynamic>>();

                        var lineItemDetails =
                            (from parent in lineItemObjects
                             join child in lineItemObjects on parent.RecID equals child.ParentRecID into parentGroup
                             where parent.ParentRecID == 0
                             select new
                             {
                                 Attachments = ConvertReceiptsDetailsToAttachments(parent.ReceiptsDetails),
                                 parent.AirlineServiceClass,
                                 parent.AmountCurr,
                                 parent.AmountMST,
                                 parent.Billable,
                                 parent.CostType,
                                 parent.DateFrom,
                                 parent.DateTo,
                                 parent.Deduction,
                                 parent.DeductionBreakfast,
                                 parent.DeductionDinner,
                                 parent.DeductionLunch,
                                 parent.DistributionDetails,
                                 parent.EmployeeJustification,
                                 parent.EntertainmentLocation,
                                 parent.ExchangeCode,
                                 parent.ExchangeRate,
                                 parent.ExpenseDetails,
                                 parent.ExpType,
                                 parent.GuestDetails,
                                 parent.LineType,
                                 parent.Location,
                                 parent.MerchantID,
                                 parent.MilageKMOwnCar,
                                 parent.MilageVehicleType,
                                 parent.MS_IT_CodeValue,
                                 parent.MS_IT_ExchangeCode,
                                 parent.MS_IT_ExpSubCategory,
                                 parent.MS_IT_GLAccount,
                                 parent.ParentRecID,
                                 parent.PayMethod,
                                 parent.PerdiemDetails,
                                 parent.PolicyViolation,
                                 parent.Project,
                                 parent.ReceiptsDetails,
                                 parent.ReceiptsRequired,
                                 parent.RecID,
                                 parent.TransDate,
                                 Children = from child in parentGroup select child
                             }).ToArray();
                        jsonDetail = (new { LineItems = lineItemDetails }).ToJson();
                    }
                }

                break;

            case "REC":
                if ((jsonDetail).ToJObject()["Attachments"] != null && !string.IsNullOrEmpty((jsonDetail).ToJObject()["Attachments"].ToString()))
                {
                    jsonDetail = (new { Attachments = ConvertReceiptsDetailsToAttachments((JArray)(jsonDetail).ToJObject()["Attachments"]) }).ToJson();
                }
                break;
        }
        return jsonDetail;
    }

    protected override async Task<HttpResponseMessage> ExecutePostDetailOperationAsync(HttpResponseMessage lobResponse, string operation, ApprovalIdentifier approvalIdentifier)
    {
        lobResponse = await base.ExecutePostDetailOperationAsync(lobResponse, operation, approvalIdentifier);
        string jsonDetail = await lobResponse.Content.ReadAsStringAsync();
        var jsonDetailsPostProcess = PostProcessDetails(jsonDetail, operation);

        return new HttpResponseMessage() { Content = new StringContent(jsonDetailsPostProcess), StatusCode = lobResponse.StatusCode };
    }

    private object ConvertReceiptsDetailsToAttachments(JArray receiptsDetails)
    {
        List<dynamic> receiptList = new List<dynamic>();
        if (receiptsDetails != null)
        {
            foreach (var receipt in receiptsDetails)
            {
                JToken receiptId, receiptName;
                if (receipt["ID"] == null && receipt["ReceiptRecId"] != null)
                    receiptId = receipt["ReceiptRecId"];
                else
                    receiptId = receipt["ID"];

                if (receipt["Name"] == null && receipt["FileName"] != null && receipt["FileType"] != null)
                    receiptName = receipt["FileName"] + "." + receipt["FileType"];
                else
                    receiptName = receipt["Name"];

                receiptList.Add(new { Name = receiptName, ID = receiptId });
            }
        }
        return receiptList;
    }

    protected override string MapApproverNotes(string response, string approverNotesField)
    {
        if (response.IsJson() && !string.IsNullOrWhiteSpace(approverNotesField))
        {
            JObject jobject = (response).ToJObject();
            // This code assumes ApproverNotes property can be a part of both SummaryJson (as part of ARX) or in any of the tenant calls or sub-property
            // This might undergo changes.
            JToken token = jobject["ApproverNotes"];
            if (token != null && string.IsNullOrEmpty(token.ToString()))
            {
                jobject["ApproverNotes"] = approverNotesField;
            }
            else if (token == null)
            {
                jobject.Add("ApproverNotes", approverNotesField);
            }
            response = jobject.ToJson();
        }

        return response;
    }

    private string AddMessages(string jsonDetail)
    {
        JObject jObject = new JObject();
        if (jsonDetail != "null" && jsonDetail != "")
        {
            jObject = jsonDetail.ToJObject();
            JArray jArray = new JArray();
            JToken value = null;

            if (jObject.TryGetValue(Constants.Messages, out value))
            {
                jArray = (value.ToString()).ToJArray();
            }

            if (jObject.TryGetValue(ReceiptsRequired, out value) && Convert.ToBoolean(value))
            {
                jArray.Add(JObject.FromObject(new { Severity = false, ImageSrc = "/content/images/icons/reciepts16x16blue.png", Text = Config?[ConfigurationKey.Message_ReceiptsRequired.ToString()] }));
            }

            if (jArray.Any())
            {
                jObject[Constants.Messages] = jArray;
            }
        }

        return jObject.ToJson();
    }

    protected override string FutureApproverChainOperationName()
    {
        return "";
    }

    protected override async Task<string> GetDetailURL(string urlFormat, ApprovalIdentifier approvalIdentifier, int page, string xcv = "", string tcv = "", string businessProcessName = "", string docTypeId = "")
    {
        string attachmentId = "0"; // this is done to fetch all the receipts
        return String.Format(urlFormat, approvalIdentifier.DisplayDocumentNumber, "", businessProcessName, "", tcv, xcv, HttpUtility.UrlEncode(attachmentId));
    }

    #endregion GET DETAIL

    #region EXECUTE ACTION

    /// <summary>
    /// Executes the action asynchronously and overriddes the ExexuteActionAsync of TenantBase
    /// </summary>
    /// <param name="approvalRequests">The list of approval request.</param>
    /// <param name="loggedInAlias">The logged in alias.</param>
    /// <param name="sessionId">The session identifier.</param>
    /// <param name="clientDevice">The client device.</param>
    /// <param name="tcv">The Tcv.</param>
    /// <param name="xcv">The Xcv.</param>
    /// <returns>returns task containing HttpResponseMessage</returns>
    public override async Task<HttpResponseMessage> ExecuteActionAsync(List<ApprovalRequest> approvalRequests, string loggedInAlias, string sessionId, string clientDevice, string xcv, string tcv, ApprovalSummaryRow summaryRowParent = null)
    {
        HttpResponseMessage lobResponse = null;
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
            { LogDataKey.TenantId, approvalTenantInfo.TenantId },
            { LogDataKey.UserAlias, this.Alias },
            { LogDataKey.UserRoleName, loggedInAlias }
        };
        try
        {
            SetupGenericErrorMessage(approvalRequest);

            logData.Add(LogDataKey.UserActionsString, approvalRequest.ToJson());

            string expenseComment = string.Empty;
            approvalRequest.ActionDetails.TryGetValue("Comment", out expenseComment);
            if (string.IsNullOrEmpty(expenseComment))
            {
                expenseComment = string.Empty;
            }
            else
            {
                expenseComment = WebUtility.UrlEncode(expenseComment);
            }

            Guid expenseReportGuid = Guid.Empty;
            string expenseReportId = Guid.TryParse(approvalRequest.ApprovalIdentifier.DocumentNumber, out expenseReportGuid) == true ? expenseReportGuid.ToString() : approvalRequest.ApprovalIdentifier.DocumentNumber;
            string expenseDocumentNumber = approvalRequest.ApprovalIdentifier.DisplayDocumentNumber;
            string actionType = approvalRequest.Action;

            logData.Add(LogDataKey.DocumentNumber, expenseDocumentNumber);
            logData.Add(LogDataKey.DXcv, expenseDocumentNumber);
            logData.Add(LogDataKey.BusinessProcessName, string.Format(approvalTenantInfo.BusinessProcessName, Constants.BusinessProcessNameApprovalAction, actionType));

            string endPointUrl = GetTenantActionUrl(null);
            if (String.IsNullOrEmpty(endPointUrl))
                throw new UriFormatException(Config[ConfigurationKey.Message_URLNotDefined.ToString()]);

            endPointUrl = string.Format(endPointUrl, actionType, expenseReportId.ToString(), this.Alias, expenseComment, "", "", "");

            HttpMethod reqHttpMethod = GetHttpMethodForAction();
            HttpRequestMessage reqMessage = new HttpRequestMessage();
            reqMessage = await CreateRequestForDetailsOrAction(reqHttpMethod, endPointUrl);
            var actionContentForSubmissionIntoTenantService = PrepareActionContentForSubmissionIntoTenantService(approvalRequests);

            reqMessage.Content = new StringContent(actionContentForSubmissionIntoTenantService, UTF8Encoding.UTF8, Constants.ContentTypeJson);

            using (var trace1 = PerformanceLogger.StartPerformanceLogger("PerfLog", string.IsNullOrWhiteSpace(clientDevice) ? Constants.WebClient : clientDevice, string.Format(Constants.PerfLogAction, this.approvalTenantInfo.AppName, "Document Action : LOB call")
                , new Dictionary<LogDataKey, object> { { LogDataKey.DocumentNumber, expenseDocumentNumber } }))
            {
                logData[LogDataKey.CustomEventName] = approvalTenantInfo.AppName + "-" + OperationType.ActionInitiated;
                Logger.LogInformation(GetEventId(OperationType.ActionInitiated), logData);

                lobResponse = await SendRequestAsync(reqMessage, logData, clientDevice);

                logData[LogDataKey.CustomEventName] = approvalTenantInfo.AppName + "-" + OperationType.ActionComplete;
                logData[LogDataKey.ResponseStatusCode] = lobResponse.StatusCode;
                Logger.LogInformation(GetEventId(OperationType.ActionComplete), logData);
            }

            return await FormulateActionResponse(lobResponse, approvalRequests, loggedInAlias, sessionId, clientDevice, tcv);
        }
        catch (Exception ex)
        {
            logData[LogDataKey.CustomEventName] = approvalTenantInfo.AppName + "-" + OperationType.Action;
            logData[LogDataKey.ResponseContent] = await lobResponse?.Content?.ReadAsStringAsync();
            Logger.LogError(GetEventId(OperationType.Action), ex, logData);
            throw;
        }
    }

    #endregion EXECUTE ACTION

    #region Document Download

    /// <summary>
    /// Get the attachment operation name
    /// </summary>
    /// <returns>attachment operation name</returns>
    public override string AttachmentOperationName()
    {
        return DocumentDownloadAction;
    }

    #endregion Document Download

    /// <summary>
    /// Processes the ARX objects and modifies them specific to MSExpense 2.0.
    /// </summary>
    /// <param name="requestExpressions">List containing Approval REquest Expression ext.</param>
    /// <returns>returns list containing Approval REquest Expression ext</returns>
    public async override Task<List<ApprovalRequestExpressionExt>> ModifyApprovalRequestExpression(List<ApprovalRequestExpressionExt> requestExpressions)
    {
        // Added 'To' alias in NotificationDetails as the Approver alias. No CC aliases required.
        // ARX is modified in case of CREATE and UPDATE operation only
        List<ApprovalRequestExpressionExt> modifiedRequestExpressions = new List<ApprovalRequestExpressionExt>();
        foreach (var requestExpression in requestExpressions)
        {
            base.AddAdditionalDataToDetailsData(requestExpression, requestExpression.SummaryData, string.Empty);
            if (approvalTenantInfo.NotifyEmailWithApprovalFunctionality && requestExpression.NotificationDetail == null && (requestExpression.Operation == ApprovalRequestOperation.Create || requestExpression.Operation == ApprovalRequestOperation.Update) && requestExpression.Approvers != null)
            {
                requestExpression.NotificationDetail = new NotificationDetail
                {
                    SendNotification = true,
                    To = requestExpression.Approvers.FirstOrDefault().Alias + "@microsoft.com",
                    TemplateKey = "PendingApproval",
                    Reminder = new ReminderDetail
                    {
                        ReminderDates = new List<DateTime>
                        {
                            requestExpression.OperationDateTime.AddDays(10)
                        },
                        Frequency = 0,
                        Expiration = default(DateTime),
                        ReminderTemplate = "MSExpense|Reminder"
                    }
                };
            }
            await base.ModifyApprovers(requestExpression);

            modifiedRequestExpressions.Add(requestExpression);
        }

        return modifiedRequestExpressions;
    }

    #region Adaptive Card Methods

    /// <summary>
    /// Apply format in jobject data
    /// </summary>
    /// <param name="jToken">jToken</param>
    public override void ApplyFormatInJsonData(JToken jToken)
    {
        ////Details Section fields
        base.ApplyFormatInJsonData(jToken);

        ////Additional Information fields
        ApplyFormatting<double>(jToken, "AdditionalData.MS_IT_PersonalAmount", "{0:n2}");

        ////Lineitems fields
        ApplyFormatting<double>(jToken, "AmountMST", "{0:n2}");
        ApplyFormatting<double>(jToken, "AmountCurr", "{0:n2}");
        ApplyFormatting<double>(jToken, "ExchangeRate", "{0:n6}");
        ApplyFormatting<DateTime>(jToken, "TransDate", "{0:MM/dd/yy}");

        ////Per Diem details fields
        ApplyFormatting<double>(jToken, "AmountMST", "{0:n2}");
        ApplyFormatting<double>(jToken, "AmountCurr", "{0:n2}");
        ApplyFormatting<double>(jToken, "Deduction", "{0:n2}");
        ApplyFormatting<DateTime>(jToken, "DateFrom", "{0:MM/dd/yy}");
        ApplyFormatting<DateTime>(jToken, "DateTo", "{0:MM/dd/yy}");
        ApplyFormatting<double>(jToken, "MealDeduction", "{0:n2}");
        ApplyFormatting<DateTime>(jToken, "TransDate", "{0:MM/dd/yy}");

        ////Distributions fields
        ApplyFormatting<double>(jToken, "AllocationFactor", "{0:n2}");
        ApplyFormatting<double>(jToken, "TransactionCurrencyAmount", "{0:n2}");
    }

    #endregion Adaptive Card Methods
}