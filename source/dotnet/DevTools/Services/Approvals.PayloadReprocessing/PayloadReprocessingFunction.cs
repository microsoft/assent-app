// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace Microsoft.CFS.Approvals.PayloadReprocessing
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Security.Claims;
    using System.Threading.Tasks;
    using Microsoft.AspNetCore.Http;
    using Microsoft.AspNetCore.Mvc;
    using Microsoft.Azure.WebJobs;
    using Microsoft.Azure.WebJobs.Extensions.Http;
    using Microsoft.CFS.Approvals.Common.DL.Interface;
    using Microsoft.CFS.Approvals.Contracts.DataContracts;
    using Microsoft.CFS.Approvals.LogManager.Model;
    using Microsoft.CFS.Approvals.LogManager.Provider.Interface;
    using Microsoft.CFS.Approvals.PayloadReprocessing.Utils;
    using Microsoft.CFS.Approvals.SupportServices.Helper.ExtensionMethods;
    using Microsoft.CFS.Approvals.SupportServices.Helper.Interface;
    using Microsoft.Extensions.Configuration;
    using Microsoft.Extensions.Logging;
    using Newtonsoft.Json;
    using Newtonsoft.Json.Linq;

    public class PayloadReprocessingFunction
    {
        private readonly IConfiguration _config;
        private readonly IApprovalTenantInfoProvider _approvalTenantInfoProvider;
        private readonly IDocumentStatusAuditHelper _documentStatusAuditHelper;
        private readonly IRePushMessagesHelper _rePushMessagesHelper;
        private readonly IAuthorizationMiddleware _authService;
        private readonly ILogProvider _logProvider;

        public PayloadReprocessingFunction(IConfiguration config,
            ILogProvider logProvider,
            IApprovalTenantInfoProvider approvalTenantInfoProvider,
            IDocumentStatusAuditHelper documentStatusAuditHelper,
            IRePushMessagesHelper rePushMessagesHelper,
            IAuthorizationMiddleware authService)
        {
            _config = config;
            _logProvider = logProvider;
            _approvalTenantInfoProvider = approvalTenantInfoProvider;
            _documentStatusAuditHelper = documentStatusAuditHelper;
            _rePushMessagesHelper = rePushMessagesHelper;
            _authService = authService;
        }

        [FunctionName("PayloadReprocessing")]
        public async Task<IActionResult> Run(
            [HttpTrigger(AuthorizationLevel.Anonymous, "get", "post", Route = null)] HttpRequest req, ILogger logger,
             ClaimsPrincipal claimsPrincipal)
        {

            var logData = new Dictionary<LogDataKey, object>
            {
                { LogDataKey.RequestMethod, req.Method },
                {LogDataKey.ComponentName, "PayloadReprocessingFunction" }
            };
            try
            {
                // Authorization using claims
                if (!_authService.IsValidClaims(claimsPrincipal))
                {
                    return new UnauthorizedResult();
                }
                logger.LogInformation("C# HTTP trigger function processed a request.");

                var jtokenData = await GetRequestBodyAsync(req);
                if (jtokenData == null)
                {
                    return new BadRequestObjectResult("Please enter valid parameter ");
                }
                var collectionName = jtokenData["collection"]?.ToString();
                var DocTypeId = _approvalTenantInfoProvider.GetTenantInfo(Convert.ToInt32(jtokenData?["tenantID"])).DocTypeId;
                switch (req.Method)
                {
                    case "GET":
                        var payloadHistory = new List<dynamic>();
                        string documentCollection = jtokenData["documentCollection"]?.ToString();
                        string tenantID = jtokenData["tenantID"]?.ToString();
                        string fromDate = ConvertToUtcFormat(jtokenData["fromDate"]?.ToString());
                        string toDate = ConvertToUtcFormat(jtokenData["toDate"]?.ToString());
                        List<dynamic> result;
                        if (string.IsNullOrEmpty(documentCollection))
                        {
                            result = _documentStatusAuditHelper.GetReceivedRequests(DocTypeId, fromDate, toDate, collectionName);
                        }
                        else
                        {
                            result = _documentStatusAuditHelper.GetReceivedRequestsByDocumentNumbers(DocTypeId, documentCollection, collectionName);
                        }
                        if (result != null || result?.Count > 0)
                        {
                            foreach (dynamic doc in result)
                            {
                                payloadHistory.Add(new
                                {
                                    IsEnabled = true,
                                    DocumentNumber = doc.ApprovalRequest.ApprovalIdentifier.DocumentNumber,
                                    DisplayDocumentNumber = doc.ApprovalRequest.ApprovalIdentifier.DisplayDocumentNumber,
                                    BrokeredMessageID = doc.BrokeredMsgId,
                                    FiscalYear = doc.ApprovalRequest.ApprovalIdentifier.FiscalYear,
                                    OperationType = Enum.Parse(typeof(ApprovalRequestOperation), doc.ApprovalRequest.Operation.ToString()).ToString(),
                                    ApproverAlias = doc.ApprovalRequest.Approvers != null ? doc.ApprovalRequest.Approvers[0].Alias : string.Empty,
                                    TimeStamp = Convert.ToDateTime(doc.EnqueuedTimeUtc).ToString("G"),
                                    Source = "Audit Agent"
                                });
                            }
                        }
                        _logProvider.LogInformation(TrackingEvent.PayloadReprocessingSuccess, logData);
                        return new OkObjectResult(payloadHistory);
                    case "POST":
                        List<string> failedRecords = new List<string>();
                        var requestNumber = jtokenData["requestNumber"]?.ToString();
                        var brokeredMsgId = jtokenData["brokeredMessageID"]?.ToString();
                        var documentHistory = _documentStatusAuditHelper.GetDocumentHistory(DocTypeId, string.Empty, requestNumber, collectionName, brokeredMsgId, string.Empty);
                        if (documentHistory != null && documentHistory.Count > 0)
                        {
                            List<dynamic> documents = new List<dynamic>();
                            foreach (var document in documentHistory)
                            {
                                documents.Add(JsonConvert.DeserializeObject(document.ToString()));
                            }
                            _rePushMessagesHelper.SendNotification = true;
                            if (jtokenData?["SendNotification"] != null)
                            {
                                _rePushMessagesHelper.SendNotification = Convert.ToBoolean(jtokenData?["SendNotification"]);
                            }
                            failedRecords = await _rePushMessagesHelper.RepushDocumentAsync(documents);
                            if (failedRecords != null && failedRecords.Any())
                            {
                                return new BadRequestObjectResult("List of Failed Messages are : " + string.Join<string>(",", failedRecords));
                            }
                            _logProvider.LogInformation(TrackingEvent.PayloadReprocessingSuccess, logData);
                            return new OkObjectResult("Records re-pushed successfully");
                        }
                        else
                        {
                            return new NotFoundObjectResult($"No record(s) found for request number {requestNumber}  and message id ${brokeredMsgId}");
                        }
                    default:
                        _logProvider.LogInformation(TrackingEvent.PayloadReprocessingSuccess, logData);
                        return new BadRequestObjectResult($"Method {req.Method} not supported")
                        {
                            StatusCode = StatusCodes.Status405MethodNotAllowed
                        };
                }
            }
            catch (Exception ex)
            {
                _logProvider.LogError(TrackingEvent.PayloadReprocessingFailed, ex, logData);
                return new BadRequestObjectResult($"Error occured");
            }
        }

        private async Task<JToken> GetRequestBodyAsync(HttpRequest request)
        {
            var jsonData = await new StreamReader(request.Body).ReadToEndAsync();
            return jsonData.FromJson<JToken>();
        }

        private string ConvertToUtcFormat(string dateTimeValue)
        {
            if (string.IsNullOrWhiteSpace(dateTimeValue)) return string.Empty;

            var date = Convert.ToDateTime(dateTimeValue);
            var utcFormat = Convert.ToString(date.ToString("o"));
            return utcFormat;
        }
    }
}
