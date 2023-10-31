// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace Microsoft.CFS.Approvals.Domain.BL.Tenants.Core;

using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Security.Claims;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Microsoft.CFS.Approvals.Common.DL.Interface;
using Microsoft.CFS.Approvals.Contracts;
using Microsoft.CFS.Approvals.Contracts.DataContracts;
using Microsoft.CFS.Approvals.Data.Azure.Storage.Interface;
using Microsoft.CFS.Approvals.Domain.BL.Interface;
using Microsoft.CFS.Approvals.Extensions;
using Microsoft.CFS.Approvals.LogManager.Model;
using Microsoft.CFS.Approvals.LogManager.Provider.Interface;
using Microsoft.CFS.Approvals.Model;
using Microsoft.CFS.Approvals.Utilities.Extension;
using Microsoft.CFS.Approvals.Utilities.Helpers;
using Microsoft.CFS.Approvals.Utilities.Interface;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

/// <summary>
/// The TenantBase class
/// </summary>
public abstract class TenantBase : ITenant
{
    #region VARIABLES

    /// <summary>
    /// The approval tenant info
    /// </summary>
    protected ApprovalTenantInfo approvalTenantInfo;

    #endregion VARIABLES

    #region CONSTRUCTOR

    protected TenantBase(
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
        IHttpHelper httpHelper)
    {
        approvalTenantInfo = tenantInfo;
        Alias = alias;
        ClientDevice = clientDevice;
        UserToken = oauth2Token;
        PerformanceLogger = performanceLogger;
        Logger = logger;
        ApprovalSummaryProvider = approvalSummaryProvider;
        Config = config;
        NameResolutionHelper = nameResolutionHelper;
        ApprovalDetailProvider = approvalDetailProvider;
        FlightingDataProvider = flightingDataProvider;
        ApprovalHistoryProvider = approvalHistoryProvider;
        BlobStorageHelper = blobStorageHelper;
        AuthenticationHelper = authenticationHelper;
        HttpHelper = httpHelper;
    }

    protected TenantBase(
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
    {
        approvalTenantInfo = tenantInfo;
        PerformanceLogger = performanceLogger;
        Logger = logger;
        ApprovalSummaryProvider = approvalSummaryProvider;
        Config = config;
        NameResolutionHelper = nameResolutionHelper;
        ApprovalDetailProvider = approvalDetailProvider;
        FlightingDataProvider = flightingDataProvider;
        ApprovalHistoryProvider = approvalHistoryProvider;
        BlobStorageHelper = blobStorageHelper;
        AuthenticationHelper = authenticationHelper;
        HttpHelper = httpHelper;

        // Assigned Default ClientDevice as Worker
        ClientDevice = Constants.WorkerRole;
    }

    protected TenantBase(
        ApprovalTenantInfo tenantInfo,
        string alias,
        string clientDevice,
        string aadToken,
        ILogProvider logger,
        IPerformanceLogger performanceLogger,
        IApprovalSummaryProvider approvalSummaryProvider,
        IConfiguration config,
        INameResolutionHelper nameResolutionHelper,
        IApprovalDetailProvider approvalDetailProvider,
        IFlightingDataProvider flightingDataProvider,
        IApprovalHistoryProvider approvalHistoryProvider,
        IAuthenticationHelper authenticationHelper)
    {
        approvalTenantInfo = tenantInfo;
        Alias = alias;
        ClientDevice = clientDevice;
        UserToken = aadToken;
        Logger = logger;
        PerformanceLogger = performanceLogger;
        ApprovalSummaryProvider = approvalSummaryProvider;
        Config = config;
        NameResolutionHelper = nameResolutionHelper;
        ApprovalDetailProvider = approvalDetailProvider;
        FlightingDataProvider = flightingDataProvider;
        ApprovalHistoryProvider = approvalHistoryProvider;
        AuthenticationHelper = authenticationHelper;
    }

    #endregion CONSTRUCTOR

    #region PROPERTIES

    /// <summary>
    /// Gets or sets the Generic Error message
    /// To handle error message from tenant. This is not a permanent fix and we have review this flow to correct the contract of error message we receive from tenants.
    /// </summary>
    public string GenericErrorMessage { get; set; }

    /// <summary>
    /// Gets or sets the user token.
    /// </summary>
    /// <value>
    /// The user token.
    /// </value>
    protected string UserToken { get; set; }

    /// <summary>
    /// Gets or sets the alias.
    /// </summary>
    /// <value>
    /// The alias.
    /// </value>
    protected string Alias { get; set; }

    /// <summary>
    /// Gets or sets the client device.
    /// </summary>
    /// <value>
    /// The client device.
    /// </value>
    protected string ClientDevice { get; set; }

    /// <summary>
    /// Gets or sets the performance logger.
    /// </summary>
    /// <value>
    /// The performance logger.
    /// </value>
    protected IPerformanceLogger PerformanceLogger { get; set; }

    /// <summary>
    /// Gets or sets the logger.
    /// </summary>
    /// <value>
    /// The logger.
    /// </value>
    protected ILogProvider Logger { get; set; }

    /// <summary>
    /// Gets or sets the approval summary provider.
    /// </summary>
    /// <value>
    /// The approval summary provider.
    /// </value>
    protected IApprovalSummaryProvider ApprovalSummaryProvider { get; set; }

    /// <summary>
    /// Gets or sets the configuration.
    /// </summary>
    /// <value>
    /// The configuration.
    /// </value>
    protected IConfiguration Config { get; set; }

    /// <summary>
    /// Gets or sets the name resolution helper.
    /// </summary>
    /// <value>
    /// The name resolution helper.
    /// </value>
    protected INameResolutionHelper NameResolutionHelper { get; set; }

    /// <summary>
    /// Gets or sets the approval detail provider.
    /// </summary>
    /// <value>
    /// The approval detail provider.
    /// </value>
    protected IApprovalDetailProvider ApprovalDetailProvider { get; set; }

    /// <summary>
    /// Gets the value if tenant has defined mapping or not for Xcv, Tcv in service paramter of approval tenant info table
    /// </summary>
    /// <value>
    /// true/false.
    /// </value>
    protected bool IsTelemetryMappingNameDefined
    {
        get
        {
            return !string.IsNullOrWhiteSpace(GetXcvOrTcvMappingKeyOfTenant(Constants.XcvMappingKey)) && !string.IsNullOrWhiteSpace(GetXcvOrTcvMappingKeyOfTenant(Constants.TcvMappingKey));
        }
    }

    /// <summary>
    /// Gets or sets the flighting data provider.
    /// </summary>
    /// <value>
    /// The flighting data provider.
    /// </value>
    protected IFlightingDataProvider FlightingDataProvider { get; set; }

    /// <summary>
    /// Gets or sets the approval history provider.
    /// </summary>
    /// <value>
    /// The approval history provider.
    /// </value>
    protected IApprovalHistoryProvider ApprovalHistoryProvider { get; set; }

    /// <summary>
    /// Gets or sets the blob helper.
    /// </summary>
    /// <value>
    /// The blob storage helper.
    /// </value>
    protected IBlobStorageHelper BlobStorageHelper { get; set; }

    /// <summary>
    /// Gets or Sets the Authentication Helper.
    /// </summary>
    protected IAuthenticationHelper AuthenticationHelper { get; set; }

    /// <summary>
    /// Gets or Sets the Http Helper.
    /// </summary>
    protected IHttpHelper HttpHelper { get; set; }

    #endregion PROPERTIES

    #region GET SUMMARY

    #region Implemented Methods

    /// <summary>
    /// Gets the summary from tenant asynchronous.
    /// </summary>
    /// <param name="approvalRequest">The approval request.</param>
    /// <param name="loggedInAlias">The logged in alias.</param>
    /// <param name="telemetry">The telemetry.</param>
    /// <returns>returns approval summary rows</returns>
    public virtual async Task<List<ApprovalSummaryRow>> GetSummaryFromTenantAsync(ApprovalRequestExpression approvalRequest, string loggedInAlias, ApprovalsTelemetry telemetry)
    {
        JObject summaryObject = null;

        #region Logging

        var logData = new Dictionary<LogDataKey, object>
        {
            { LogDataKey.BusinessProcessName, string.Format(approvalTenantInfo.BusinessProcessName, Constants.BusinessProcessNameGetSummary, Constants.BusinessProcessNameSumamryFromBackChannel) },
            { LogDataKey.Tcv, approvalRequest.Telemetry.Tcv },
            { LogDataKey.ReceivedTcv, approvalRequest.Telemetry.Tcv },
            { LogDataKey.TenantTelemetryData, approvalRequest.Telemetry.TenantTelemetry },
            { LogDataKey.Xcv, approvalRequest.Telemetry.Xcv },
            { LogDataKey.UserRoleName, loggedInAlias },
            { LogDataKey.TenantId, approvalTenantInfo.TenantId },
            { LogDataKey.DocumentNumber, approvalRequest.ApprovalIdentifier.DisplayDocumentNumber },
            { LogDataKey.DXcv, approvalRequest.ApprovalIdentifier.DisplayDocumentNumber },
            { LogDataKey.FiscalYear, approvalRequest.ApprovalIdentifier.FiscalYear },
            { LogDataKey.UserAlias, Alias }
        };

        #endregion Logging

        using (PerformanceLogger.StartPerformanceLogger("PerfLog", Constants.WorkerRole, string.Format(Constants.PerfLogAction, approvalTenantInfo.AppName, "Gets summary from Tenants through TenantBase"), logData))
        {
            HttpResponseMessage response = null;
            try
            {
                var placeHolderDict = new Dictionary<string, object>();
                placeHolderDict = Extension.ConvertJsonToDictionary(placeHolderDict, JsonConvert.SerializeObject(approvalRequest?.ApprovalIdentifier));
                var placeholderUrlTenantIdList = Config[ConfigurationKey.UrlPlaceholderTenants.ToString()].Split(',').ToList();
                var summaryUrl = (placeholderUrlTenantIdList.Contains(approvalTenantInfo.RowKey)) ? RetrieveTenantSummaryUrl(placeHolderDict) : GetSummaryUrl(approvalTenantInfo, approvalTenantInfo.TenantBaseUrl + approvalTenantInfo.SummaryURL, approvalRequest, approvalTenantInfo.DocTypeId);
                var serviceRoot = new Uri(summaryUrl);
                HttpRequestMessage requestMessage = await CreateRequestForTenantSummary(serviceRoot, approvalRequest.Telemetry.Xcv, approvalRequest.Telemetry.Tcv);

                logData.Modify(LogDataKey.EventId, GetEventId(OperationType.SummaryFetchInitiated));
                logData.Modify(LogDataKey.EventName, approvalTenantInfo.AppName + "-" + OperationType.SummaryFetchInitiated);
                Logger.LogInformation(GetEventId(OperationType.SummaryFetchInitiated), logData);
                response = await SendRequestAsync(requestMessage, logData, Constants.WorkerRole);

                logData.Modify(LogDataKey.EventId, GetEventId(OperationType.SummaryFetchComplete));
                logData.Modify(LogDataKey.EventName, approvalTenantInfo.AppName + "-" + OperationType.SummaryFetchComplete);
                logData.Add(LogDataKey.ResponseStatusCode, response.StatusCode);

                #region logging Xcv, Tcv from response headers

                string xcvMappingKeyInRequestHeader = GetXcvOrTcvMappingKeyOfTenant(Constants.XcvMappingKey);
                string tcvMappingKeyInRequestHeader = GetXcvOrTcvMappingKeyOfTenant(Constants.TcvMappingKey);
                if (!string.IsNullOrWhiteSpace(xcvMappingKeyInRequestHeader) && response.Headers.Contains(xcvMappingKeyInRequestHeader))
                {
                    if (logData.ContainsKey(LogDataKey.Xcv))
                    {
                        logData.Remove(LogDataKey.Xcv);
                    }

                    logData.Add(LogDataKey.Xcv, response.Headers.GetValues(xcvMappingKeyInRequestHeader).FirstOrDefault());
                }
                if (!string.IsNullOrWhiteSpace(tcvMappingKeyInRequestHeader) && response.Headers.Contains(tcvMappingKeyInRequestHeader))
                {
                    if (logData.ContainsKey(LogDataKey.Tcv))
                    {
                        logData.Remove(LogDataKey.Tcv);
                    }

                    logData.Add(LogDataKey.Tcv, response.Headers.GetValues(tcvMappingKeyInRequestHeader).FirstOrDefault());
                }

                #endregion logging Xcv, Tcv from response headers

                summaryObject = await JSONHelper.ValidateJsonResponse(Config, response, approvalTenantInfo.AppName);
                Logger.LogInformation(GetEventId(OperationType.SummaryFetchComplete), logData);
            }
            catch (Exception jsonValidationException)
            {
                logData.Modify(LogDataKey.EventId, GetEventId(OperationType.Summary));
                logData.Modify(LogDataKey.EventName, approvalTenantInfo.AppName + "-" + OperationType.Summary);
                logData.Add(LogDataKey.ResponseContent, await response?.Content?.ReadAsStringAsync());
                Logger.LogError(GetEventId(OperationType.Summary), new Exception("Failed to retrive summary from " + approvalTenantInfo.AppName, jsonValidationException), logData);
                if (jsonValidationException != null && jsonValidationException.InnerException != null)
                {
                    throw jsonValidationException.InnerException;
                }
                else
                {
                    throw;
                }
            }
        }
        // ApprovalRequestExpression will contain the partition key, approver and documentTypeID. These values should not be taken from tenant side, but populated from here instead.
        return await ConvertJSON(approvalRequest, summaryObject);
    }

    #endregion Implemented Methods

    #region TenantBase Methods

    /// <summary>
    /// Method to formulate URL.
    /// </summary>
    /// <param name="tenantInfo"></param>
    /// <param name="documentSummaryUrl"></param>
    /// <param name="approvalRequest"></param>
    /// <param name="docTypeId"></param>
    /// <returns>returns string</returns>
    protected virtual string GetSummaryUrl(ApprovalTenantInfo tenantInfo, string documentSummaryUrl, ApprovalRequestExpression approvalRequest, string docTypeId = "")
    {
        return string.Format(documentSummaryUrl, approvalRequest.ApprovalIdentifier.GetDocNumber(tenantInfo));
    }

    /// <summary>
    /// Method to create HttpRequestMessage. This will be overridden in each LOB that would return us an object of HTTPRequestMessage
    /// that will be formulated using appropriate Token in request headers
    /// </summary>
    /// <param name="serviceRoot"></param>
    /// <param name="Xcv">Xcv</param>
    /// <param name="Tcv">Tcv</param>
    /// <returns>returns Http Request message</returns>
    protected virtual async Task<HttpRequestMessage> CreateRequestForTenantSummary(Uri serviceRoot, string Xcv, string Tcv)
    {
        HttpRequestMessage requestMessage = new HttpRequestMessage(HttpMethod.Get, serviceRoot);
        JObject serviceParameterObject = GetServiceParameter(Constants.OperationTypeSummary);
        await GetAndAttachTokenBasedOnAuthType(requestMessage, serviceRoot.ToString(), serviceParameterObject);
        AddXcvTcvToRequestHeaders(ref requestMessage, Xcv, Tcv);
        return requestMessage;
    }

    /// <summary>
    /// This method will be used to return SummaryOperationTypes
    /// </summary>
    /// <returns>List of string</returns>
    public virtual List<string> GetSummaryOperationTypes()
    {
        return new List<string>() { Constants.SummaryOperationType, Constants.CurrentApprover, Constants.ApprovalChainOperation };
    }

    /// <summary>
    /// Get Approval Summary By RowKey And Approver
    /// </summary>
    /// <param name="rowKey"></param>
    /// <param name="approver"></param>
    /// <param name="fiscalYear"></param>
    /// <param name="tenantInfo"></param>
    /// <returns></returns>
    public virtual ApprovalSummaryRow GetApprovalSummaryByRowKeyAndApprover(string rowKey, string approver, string fiscalYear, ApprovalTenantInfo tenantInfo)
    {
        return ApprovalSummaryProvider.GetApprovalSummaryByDocumentNumberIncludingSoftDeleteData(tenantInfo.DocTypeId, rowKey, approver);
    }

    #endregion TenantBase Methods

    #endregion GET SUMMARY

    #region Convert JSON Methods

    #region Implemented Methods

    /// <summary>
    /// Gets summary from within the brokered message/ approval request expresssion object
    /// </summary>
    /// <param name="approvalRequest"></param>
    /// <param name="summaryJson"></param>
    /// <returns>returns list containing approval summary row</returns>
    public async Task<List<ApprovalSummaryRow>> GetSummaryFromARX(ApprovalRequestExpression approvalRequest, SummaryJson summaryJson)
    {
        #region Logging

        var logData = new Dictionary<LogDataKey, object>
            {
                { LogDataKey.Tcv, approvalRequest.Telemetry.Tcv },
                { LogDataKey.ReceivedTcv, approvalRequest.Telemetry.Tcv },
                { LogDataKey.TenantTelemetryData, approvalRequest.Telemetry.TenantTelemetry },
                { LogDataKey.Xcv, approvalRequest.Telemetry.Xcv },
                { LogDataKey.BusinessProcessName, string.Format(approvalTenantInfo.BusinessProcessName, Constants.BusinessProcessNameGetSummary, Constants.BusinessProcessNameSumamryFromARX) },
                { LogDataKey.TenantId, approvalTenantInfo.TenantId },
                { LogDataKey.TenantName, approvalTenantInfo.AppName },
                { LogDataKey.DocumentNumber, approvalRequest.ApprovalIdentifier.DisplayDocumentNumber },
                { LogDataKey.DXcv, approvalRequest.ApprovalIdentifier.DisplayDocumentNumber },
                { LogDataKey.FiscalYear, approvalRequest.ApprovalIdentifier.FiscalYear }
            };

        #endregion Logging

        using (PerformanceLogger.StartPerformanceLogger("PerfLog", Constants.WorkerRole, string.Format(Constants.PerfLogAction, approvalTenantInfo.AppName, "Extracts summary row data from ARX and summaryJson"), logData))
        {
            if (summaryJson == null || summaryJson.ApprovalIdentifier == null || summaryJson.ApprovalIdentifier.DisplayDocumentNumber == null)
            {
                throw new InvalidDataException("Invalid or null Summary Data: ");
            }

            List<ApprovalSummaryRow> summaryRows = new List<ApprovalSummaryRow>();
            var isApproverAliasValid = true;

            #region Validate approver alias

            // validate if approver is compatible for partition key.
            if (approvalRequest.Approvers != null && approvalRequest.Approvers.Count > 0)
            {
                foreach (var approver in approvalRequest.Approvers)
                {
                    if (!approver.Alias.ValidatePartitionKeyCharacters())
                    {
                        isApproverAliasValid = false;
                        if (logData.ContainsKey(LogDataKey.UserAlias))
                        {
                            logData[LogDataKey.UserAlias] = approver.Alias;
                        }
                        else
                        {
                            logData.Add(LogDataKey.UserAlias, approver.Alias);
                        }

                        Logger.LogError(TrackingEvent.InvalidApprover, new Exception("Approver alias (" + approver.Alias + ") has some invalid characters or is empty!"), logData);
                    }
                }
            }

            #endregion Validate approver alias

            if (isApproverAliasValid)
            {
                AddAdditionalDataToDetailsData((ApprovalRequestExpressionExt)approvalRequest, summaryJson, string.Empty);

                summaryJson.DocumentTypeId = approvalRequest.DocumentTypeId.ToString();
                if (!string.IsNullOrWhiteSpace(summaryJson.Submitter.Alias))
                {
                    var submitterName = await NameResolutionHelper.GetUserName(summaryJson.Submitter.Alias);
                    if (submitterName != summaryJson.Submitter.Alias || string.IsNullOrWhiteSpace(summaryJson.Submitter.Name))
                    {
                        summaryJson.Submitter.Name = submitterName;
                    }
                }

                ApprovalSummaryRow summaryRow =
                    new ApprovalSummaryRow()
                    {
                        DocumentNumber = approvalRequest.ApprovalIdentifier.GetDocNumber(approvalTenantInfo),
                        Application = approvalTenantInfo.AppName,
                        LobPending = false,
                        ActionTakenOnClient = "None",
                        Requestor = summaryJson.Submitter.Alias,
                        RowKey = approvalRequest.ApprovalIdentifier.ToAzureTableRowKey(approvalTenantInfo),
                        RoutingId = approvalRequest.AdditionalData[Constants.RoutingIdColumnName].ToString(),
                        WaitForLOBResponse = false,
                        LastFailed = false,
                        PreviousApprover = null,
                        SummaryJson = summaryJson.ToJson(),
                        NotificationJson = (approvalRequest.NotificationDetail).ToJson(),
                        OperationDateTime = approvalRequest.OperationDateTime,
                        Xcv = approvalRequest.Telemetry.Xcv,
                        Tcv = approvalRequest.Telemetry.Tcv,
                        RequestVersion = summaryJson.RequestVersion
                    };

                // Hack - Temporary, add Approver field based on 0th Approver.

                // Update Approvers with updated Names
                var summaryRowJsonString = summaryRow.ToJson();
                if (approvalRequest.Approvers != null && approvalRequest.Approvers.Count > 0)
                {
                    foreach (var approver in approvalRequest.Approvers)
                    {
                        approver.Name = await NameResolutionHelper.GetUserName(approver.Alias);
                        var copySummaryRow = summaryRowJsonString.FromJson<ApprovalSummaryRow>();

                        copySummaryRow.PartitionKey = approver.Alias;
                        copySummaryRow.Approver = approver.Alias;
                        copySummaryRow.OriginalApprovers = approver.OriginalApprovers != null && approver.OriginalApprovers.Any() ? (approver.OriginalApprovers).ToJson() : string.Empty;
                        if (!approvalRequest.AdditionalData.ContainsKey(Constants.Approver))
                        {
                            approvalRequest.AdditionalData.Add(Constants.Approver, copySummaryRow.Approver);
                        }
                        summaryRows.Add(copySummaryRow);
                    }
                }
                else
                {
                    approvalRequest.Approvers = new List<Approver>
                    {
                        approvalRequest.AdditionalData.ContainsKey("Approver")
                        ? new Approver()
                        {
                            Name = await GetUserDisplayName(approvalRequest.AdditionalData["Approver"]),
                            Alias = approvalRequest.AdditionalData["Approver"]
                        }
                        : new Approver() { Name = string.Empty, Alias = string.Empty }
                    };

                    if (approvalRequest.AdditionalData.ContainsKey(Constants.Approver))
                    {
                        summaryRow.PartitionKey = approvalRequest.AdditionalData[Constants.Approver];
                        summaryRow.Approver = approvalRequest.AdditionalData[Constants.Approver];
                    }
                    summaryRows.Add(summaryRow);
                }
            }
            return summaryRows;
        }
    }

    #endregion Implemented Methods

    #region TenantBase Methods

    /// <summary>
    /// Converts the json.
    /// </summary>
    /// <param name="approvalRequest">The approval request.</param>
    /// <param name="summaryJsonObject">The summary json object.</param>
    /// <returns>returns list containing approval summary row</returns>
    private async Task<List<ApprovalSummaryRow>> ConvertJSON(ApprovalRequestExpression approvalRequest, JObject summaryJsonObject)
    {
        #region Logging

        var logData = new Dictionary<LogDataKey, object>
            {
                { LogDataKey.Tcv, approvalRequest.Telemetry.Tcv },
                { LogDataKey.ReceivedTcv, approvalRequest.Telemetry.Tcv },
                { LogDataKey.TenantTelemetryData, approvalRequest.Telemetry.TenantTelemetry },
                { LogDataKey.Xcv, approvalRequest.Telemetry.Xcv },
                { LogDataKey.BusinessProcessName, string.Format(approvalTenantInfo.BusinessProcessName, Constants.BusinessProcessNameGetSummary, Constants.BusinessProcessNameSumamryFromARX) },
                { LogDataKey.TenantId, approvalTenantInfo.TenantId },
                { LogDataKey.TenantName, approvalTenantInfo.AppName },
                { LogDataKey.DocumentNumber, approvalRequest.ApprovalIdentifier.DisplayDocumentNumber },
                { LogDataKey.DXcv, approvalRequest.ApprovalIdentifier.DisplayDocumentNumber },
                { LogDataKey.FiscalYear, approvalRequest.ApprovalIdentifier.FiscalYear }
            };

        #endregion Logging

        using (PerformanceLogger.StartPerformanceLogger("PerfLog", Constants.WorkerRole, string.Format(Constants.PerfLogAction, approvalTenantInfo.AppName, "Converts summary JSON to list of approval summary rows"), logData))
        {
            SummaryJson summaryJson = await ExtractJSONDetails(approvalRequest, summaryJsonObject);
            return await GetSummaryFromARX(approvalRequest, summaryJson);
        }
    }

    /// <summary>
    /// Extracts the json details.
    /// </summary>
    /// <param name="approvalRequest">The approval request.</param>
    /// <param name="summaryJsonObject">The summary json object.</param>
    /// <returns>returns summary Json</returns>
    /// <exception cref="System.Exception">Invalid Summary Data: " + JsonConvert.SerializeObject(summaryJson)</exception>
    protected virtual async Task<SummaryJson> ExtractJSONDetails(ApprovalRequestExpression approvalRequest, JObject summaryJsonObject)
    {
        #region Logging

        var logData = new Dictionary<LogDataKey, object>
        {
            { LogDataKey.Tcv, approvalRequest.Telemetry.Tcv },
            { LogDataKey.ReceivedTcv, approvalRequest.Telemetry.Tcv },
            { LogDataKey.TenantTelemetryData, approvalRequest.Telemetry.TenantTelemetry },
            { LogDataKey.Xcv, approvalRequest.Telemetry.Xcv },
            { LogDataKey.BusinessProcessName, string.Format(approvalTenantInfo.BusinessProcessName, Constants.BusinessProcessNameGetSummary, Constants.BusinessProcessNameSumamryFromARX) },
            { LogDataKey.TenantId, approvalTenantInfo.TenantId },
            { LogDataKey.TenantName, approvalTenantInfo.AppName },
            { LogDataKey.DocumentNumber, approvalRequest.ApprovalIdentifier.DisplayDocumentNumber },
            { LogDataKey.DXcv, approvalRequest.ApprovalIdentifier.DisplayDocumentNumber },
            { LogDataKey.FiscalYear, approvalRequest.ApprovalIdentifier.FiscalYear }
        };

        #endregion Logging

        using (PerformanceLogger.StartPerformanceLogger("PerfLog", Constants.WorkerRole, string.Format(Constants.PerfLogAction, approvalTenantInfo.AppName, "Extracts summary JSON"), logData))
        {
            var summaryJson = summaryJsonObject.ToString().FromJson<SummaryJson>();
            if (summaryJson != null)
            {
                if (summaryJson.ApprovalIdentifier == null || summaryJson.ApprovalIdentifier.DisplayDocumentNumber == null)
                {
                    throw new InvalidDataException("Invalid Summary Data: " + summaryJson.ToJson());
                }

                using (PerformanceLogger.StartPerformanceLogger("PerfLog", Constants.WorkerRole, string.Format(Constants.PerfLogAction, approvalTenantInfo.AppName, "Extract Submitter Name from summaryJson"), logData))
                {
                    summaryJson.Submitter.Name = await GetUserDisplayName(summaryJson.Submitter.Alias);
                }
                using (PerformanceLogger.StartPerformanceLogger("PerfLog", Constants.WorkerRole, string.Format(Constants.PerfLogAction, approvalTenantInfo.AppName, "Extract Approver Name from approval request"), logData))
                {
                    if (approvalRequest.Approvers != null)
                    {
                        foreach (var approver in approvalRequest.Approvers)
                        {
                            approver.Name = await GetUserDisplayName(approver.Alias);
                        }
                    }
                }
            }
            return summaryJson;
        }
    }

    /// <summary>
    /// Gets the current approvers.
    /// </summary>
    /// <param name="summaryObject">The summary object.</param>
    /// <param name="approverField">The approver field.</param>
    /// <param name="approvalRequest">The approval request.</param>
    /// <returns>returns JArray</returns>
    protected async Task<JArray> GetCurrentApprovers(JObject summaryObject, string approverField, ApprovalRequestExpression approvalRequest)
    {
        JArray currentApprovers = new JArray();
        if (summaryObject[approverField] != null && summaryObject[approverField].Type != JTokenType.Null)
        {
            if ((summaryObject[approverField].ToString()).IsJsonArray())
            {
                currentApprovers = (JArray)summaryObject[approverField];
            }
            else
            {
                currentApprovers.Add(summaryObject[approverField]);
            }
        }
        if (currentApprovers.Count == 0)
        {
            currentApprovers.Add(JToken.FromObject(new Approver() { Alias = approvalRequest.AdditionalData["Approver"], Name = await GetUserDisplayName(approvalRequest.AdditionalData["Approver"]) }));
        }

        return currentApprovers;
    }

    /// <summary>
    /// Gets the display name of the user.
    /// </summary>
    /// <param name="nameAliasToken">The name alias token.</param>
    /// <returns>returns string</returns>
    protected async Task<string> GetUserDisplayName(JToken nameAliasToken)
    {
        string alias = nameAliasToken["Alias"].ToString();
        string name = nameAliasToken["Name"].ToString();
        string nameFromTenant = await NameResolutionHelper.GetUserName(alias);
        string nameForDisplay = (!String.IsNullOrWhiteSpace(nameFromTenant)) ? nameFromTenant
                                 : (!String.IsNullOrWhiteSpace(name)) ? name : alias;
        return nameForDisplay;
    }

    /// <summary>
    /// Gets the display name of the user.
    /// </summary>
    /// <param name="alias">The alias.</param>
    /// <returns>returns string</returns>
    protected async Task<string> GetUserDisplayName(string alias)
    {
        string nameFromTenant = await NameResolutionHelper.GetUserName(alias);
        string nameForDisplay = (!String.IsNullOrWhiteSpace(nameFromTenant)) ? nameFromTenant
                                 : alias;
        return nameForDisplay;
    }

    /// <summary>
    /// Reads the and remove token.
    /// </summary>
    /// <param name="jObject">The j object.</param>
    /// <param name="propertyName">Name of the property.</param>
    /// <returns>returns string</returns>
    protected string ReadAndRemoveToken(JObject jObject, string propertyName)
    {
        string propertyValue = string.Empty;

        if (jObject.TryGetValue(propertyName, out JToken value))
        {
            propertyValue = value.ToString();
            value.Parent.Remove();
        }
        return propertyValue;
    }

    #endregion TenantBase Methods

    #endregion Convert JSON Methods

    #region COMMON METHODS

    #region TenantBase Methods

    /// <summary>
    /// Method to make HTTP call and return the response back to GetSummaryData() method
    /// </summary>
    /// <param name="requestMessage"></param>
    /// <param name="componentName"></param>
    /// <param name="logData"></param>
    /// <returns>returns task containing Http Response message</returns>
    protected async Task<HttpResponseMessage> SendRequestAsync(HttpRequestMessage requestMessage, Dictionary<LogDataKey, object> logData, string componentName = Constants.WebClient)
    {
        using (PerformanceLogger.StartPerformanceLogger("PerfLog", componentName, string.Format(Constants.PerfLogAction, approvalTenantInfo.AppName, "Send request asynchronously through TenantBase"), logData))
        {
            var tenantAPILogData = new Dictionary<LogDataKey, object>
            {
                { LogDataKey.Uri, requestMessage.RequestUri.ToString() },
                { LogDataKey.StartDateTime, DateTime.UtcNow },
                { LogDataKey.BusinessProcessName, logData.ContainsKey(LogDataKey.BusinessProcessName) ? logData[LogDataKey.BusinessProcessName] : "" },
                { LogDataKey.Tcv, logData.ContainsKey(LogDataKey.Tcv) ? logData[LogDataKey.Tcv] : "" },
                { LogDataKey.Xcv, logData.ContainsKey(LogDataKey.Xcv) ? logData[LogDataKey.Xcv] : "" },
                { LogDataKey.UserRoleName, logData.ContainsKey(LogDataKey.UserRoleName) ? logData[LogDataKey.UserRoleName] : "" },
                { LogDataKey.TenantId, approvalTenantInfo?.TenantId },
                { LogDataKey.TenantName, approvalTenantInfo?.AppName },
                { LogDataKey.DocumentNumber, logData.ContainsKey(LogDataKey.DocumentNumber) ? logData[LogDataKey.DocumentNumber] : "" },
                { LogDataKey.UserAlias, Alias },
            };

            var result = await HttpHelper.SendRequestAsync(requestMessage);
            tenantAPILogData.Add(LogDataKey.EndDateTime, DateTime.UtcNow);
            Logger.LogInformation(TrackingEvent.TenantApiComplete, tenantAPILogData);

            return result;
        }
    }

    /// <summary>
    /// Creates the request for details or action.
    /// </summary>
    /// <param name="method">The method.</param>
    /// <param name="uri">The URI.</param>
    /// <param name="Xcv">Xcv</param>
    /// <param name="Tcv">Tcv</param>
    /// <param name="operationType">The Type of operation.</param>
    /// <returns>returns Http Request Message</returns>
    protected virtual async Task<HttpRequestMessage> CreateRequestForDetailsOrAction(HttpMethod method, string uri, string Xcv = "", string Tcv = "", string operationType = "")
    {
        HttpRequestMessage reqMessage = new HttpRequestMessage(method, uri);

        JObject serviceParameterObject = GetServiceParameter(operationType);

        await GetAndAttachTokenBasedOnAuthType(reqMessage, uri, serviceParameterObject);
        AddXcvTcvToRequestHeaders(ref reqMessage, Xcv, Tcv);
        reqMessage.Headers.Add(Constants.ActionByAlias, Alias);
        return reqMessage;
    }

    /// <summary>
    /// Gets the event identifier.
    /// </summary>
    /// <param name="operationType">Type of the operation.</param>
    /// <returns>returns integer</returns>
    protected int GetEventId(OperationType operationType)
    {
        try
        {
            return operationType switch
            {
                OperationType.Summary => 10000 + approvalTenantInfo.TenantId,
                OperationType.Detail => 11000 + approvalTenantInfo.TenantId,
                OperationType.Action => 12000 + approvalTenantInfo.TenantId,
                OperationType.SummaryFetchInitiated => 13000 + approvalTenantInfo.TenantId,
                OperationType.DetailFetchInitiated => 14000 + approvalTenantInfo.TenantId,
                OperationType.ActionInitiated => 15000 + approvalTenantInfo.TenantId,
                OperationType.SummaryFetchComplete => 16000 + approvalTenantInfo.TenantId,
                OperationType.ActionComplete => 17000 + approvalTenantInfo.TenantId,
                OperationType.DetailFetchComplete => 18000 + approvalTenantInfo.TenantId,
                _ => 10000,
            };
        }
        catch
        {
            return 10000;
        }
    }

    /// <summary>
    /// Check if HttpStatusCode.NotFound should be treated as error or not for a specific Tenant
    /// </summary>
    /// <returns>returns false if HttpStatus.NotFound is not supposed to be treated as error</returns>
    public virtual bool TreatNotFoundAsError()
    {
        return false;
    }

    /// <summary>
    /// Gets the service parameter for a given operation type.
    /// </summary>
    /// <param name="operationType">The Type of operation.</param>
    /// <returns>returns the service parameter</returns>
    private JObject GetServiceParameter(string operationType)
    {
        JObject serviceParameterObject = null;
        if (!string.IsNullOrWhiteSpace(operationType))
        {
            serviceParameterObject = JObject.Parse(approvalTenantInfo?.TenantOperationDetails)?[Constants.DetailOpsList]?.FirstOrDefault(x => x.Value<string>(Constants.OperationType) == operationType)?[Constants.ServiceParameter] as JObject;

            if (serviceParameterObject != null)
            {
                SetServiceParameter(serviceParameterObject);
            }
        }
        return serviceParameterObject;
    }

    /// <summary>
    /// Set Service Parameter
    /// </summary>
    /// <param name="serviceParameterObject"></param>
    private void SetServiceParameter(JObject serviceParameterObject)
    {
        if (serviceParameterObject != null)
        {
            if (!serviceParameterObject.ContainsKey(Constants.Authority))
            {
                serviceParameterObject[Constants.Authority] = Config[ConfigurationKey.Authority.ToString()].ToString();
            }
            if (serviceParameterObject.ContainsKey(Constants.KeyVaultUri))
            {
                serviceParameterObject[Constants.AuthKey] = Config[ConfigurationKey.ServiceParameterAuthKey.ToString() + "-" + serviceParameterObject[Constants.KeyVaultUri].ToString()].ToString();
                serviceParameterObject[Constants.ClientID] = Config[ConfigurationKey.ServiceParameterClientID.ToString() + "-" + serviceParameterObject[Constants.KeyVaultUri].ToString()].ToString();
            }
            else
            {
                serviceParameterObject[Constants.AuthKey] = Config[ConfigurationKey.ServiceParameterAuthKey.ToString()].ToString();
                serviceParameterObject[Constants.ClientID] = Config[ConfigurationKey.ServiceParameterClientID.ToString()].ToString();
            }
        }
    }

    #endregion TenantBase Methods

    #endregion COMMON METHODS

    #region Get Details Methods

    #region Implemented Methods

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
    /// <param name="componentName">Name of the component.</param>
    /// <returns>returns Http Response Message</returns>
    public virtual async Task<HttpResponseMessage> GetDetailAsync(ApprovalIdentifier approvalIdentifier, string operation, int page, string loggedInAlias = "", string xcv = "", string tcv = "", string businessProcessName = "", bool isUserTriggered = false, string componentName = Constants.WorkerRole)
    {
        HttpResponseMessage lobResponse = null;
        var logData = new Dictionary<LogDataKey, object>
            {
                { LogDataKey.EventId, GetEventId(OperationType.Detail) },
                { LogDataKey.EventName, approvalTenantInfo.AppName + "-" + OperationType.Detail + "-" + operation },
                { LogDataKey.Tcv, tcv },
                { LogDataKey.ReceivedTcv, tcv },
                { LogDataKey.SessionId, tcv },
                { LogDataKey.Xcv, xcv },
                { LogDataKey.UserRoleName, loggedInAlias },
                { LogDataKey.TenantId, approvalTenantInfo.TenantId },
                { LogDataKey.DXcv, approvalIdentifier.DisplayDocumentNumber },
                { LogDataKey.DocumentNumber, approvalIdentifier.DisplayDocumentNumber },
                { LogDataKey.FiscalYear, approvalIdentifier.FiscalYear },
                { LogDataKey.Operation, operation },
                { LogDataKey.UserAlias, Alias }
            };
        try
        {
            logData.Add(LogDataKey.BusinessProcessName,
                isUserTriggered
                    ? string.Format(approvalTenantInfo.BusinessProcessName,
                        Constants.BusinessProcessNameGetDetailsFromTenant,
                        Constants.BusinessProcessNameUserTriggered)
                    : string.Format(approvalTenantInfo.BusinessProcessName,
                        Constants.BusinessProcessNameGetDetailsFromTenant,
                        Constants.BusinessProcessNameDetailsPrefetched));
            logData.Add(LogDataKey.ClientDevice, componentName);

            using (PerformanceLogger.StartPerformanceLogger("PerfLog", componentName, string.Format(Constants.PerfLogActionWithInfo, approvalTenantInfo.AppName, operation, "Internal"),
                new Dictionary<LogDataKey, object> { { LogDataKey.DocumentNumber, approvalIdentifier.DocumentNumber } }))
            {
                if (logData.ContainsKey(LogDataKey.EventId))
                {
                    logData.Remove(LogDataKey.EventId);
                }

                if (logData.ContainsKey(LogDataKey.EventName))
                {
                    logData.Remove(LogDataKey.EventName);
                }

                Logger.LogInformation(TrackingEvent.DetailFetchInitiated, logData);

                lobResponse = await GetLobResponseAsync(approvalIdentifier, operation, page, xcv, tcv, businessProcessName, approvalTenantInfo.DocTypeId);

                if (logData.ContainsKey(LogDataKey.EventId))
                {
                    logData.Remove(LogDataKey.EventId);
                }

                if (logData.ContainsKey(LogDataKey.EventName))
                {
                    logData.Remove(LogDataKey.EventName);
                }

                logData.Add(LogDataKey.ResponseStatusCode, lobResponse.StatusCode);

                #region logging Xcv, Tcv from response headers

                string xcvMappingKeyInRequestHeader = GetXcvOrTcvMappingKeyOfTenant(Constants.XcvMappingKey);
                string tcvMappingKeyInRequestHeader = GetXcvOrTcvMappingKeyOfTenant(Constants.TcvMappingKey);
                if (!string.IsNullOrEmpty(xcvMappingKeyInRequestHeader) && lobResponse.Headers.Contains(xcvMappingKeyInRequestHeader))
                {
                    if (logData.ContainsKey(LogDataKey.Xcv))
                    {
                        logData.Remove(LogDataKey.Xcv);
                    }

                    logData.Add(LogDataKey.Xcv, lobResponse.Headers.GetValues(xcvMappingKeyInRequestHeader).FirstOrDefault());
                }
                if (!string.IsNullOrEmpty(tcvMappingKeyInRequestHeader) && lobResponse.Headers.Contains(tcvMappingKeyInRequestHeader))
                {
                    if (logData.ContainsKey(LogDataKey.Tcv))
                    {
                        logData.Remove(LogDataKey.Tcv);
                    }

                    logData.Add(LogDataKey.Tcv, lobResponse.Headers.GetValues(tcvMappingKeyInRequestHeader).FirstOrDefault());
                }

                #endregion logging Xcv, Tcv from response headers
            }
            await ValidateDetailResponseAsync(lobResponse);
            lobResponse = await ExecutePostDetailOperationAsync(lobResponse, operation, approvalIdentifier);
            Logger.LogInformation(TrackingEvent.DetailFetchSuccess, logData);
            return lobResponse;
        }
        catch (Exception ex)
        {
            if (logData.ContainsKey(LogDataKey.EventId))
            {
                logData.Remove(LogDataKey.EventId);
            }

            if (logData.ContainsKey(LogDataKey.EventName))
            {
                logData.Remove(LogDataKey.EventName);
            }

            logData.Add(LogDataKey.ResponseContent, await lobResponse?.Content?.ReadAsStringAsync());
            Logger.LogError(TrackingEvent.DetailFetchFailure, ex, logData);
            throw;
        }
    }

    /// <summary>
    /// Gets the lob response asynchronous.
    /// </summary>
    /// <param name="approvalIdentifier">The approval identifier.</param>
    /// <param name="operation">The operation.</param>
    /// <param name="page">The page.</param>
    /// <param name="xcv">The XCV.</param>
    /// <param name="tcv">The TCV.</param>
    /// <param name="businessProcessName">Name of the business process.</param>
    /// <param name="docTypeId">Document Type ID</param>
    public async Task<HttpResponseMessage> GetLobResponseAsync(ApprovalIdentifier approvalIdentifier, string operation, int page, string xcv, string tcv, string businessProcessName, string docTypeId)
    {
        var placeHolderDict = new Dictionary<string, object>();
        var actualApprovalIdentifier = await GetApprovalIdentifier(approvalIdentifier.DocumentNumber, xcv, tcv) ?? approvalIdentifier;
        placeHolderDict = Extension.ConvertJsonToDictionary(placeHolderDict, JsonConvert.SerializeObject(actualApprovalIdentifier));
        placeHolderDict.Add("DocumentTypeId", docTypeId);
        var placeholderUrlTenantIdList = Config[ConfigurationKey.UrlPlaceholderTenants.ToString()].Split(',').ToList();
        string detailURL = (placeholderUrlTenantIdList.Contains(approvalTenantInfo.RowKey)) ? RetrieveTenantDetailsUrl(operation, placeHolderDict, ClientDevice) : await GetDetailURL(approvalTenantInfo.GetEndPointURL(operation, ClientDevice), approvalIdentifier, page, xcv, tcv, businessProcessName, docTypeId);

        HttpResponseMessage lobResponse;
        lobResponse = await HttpHelper.SendRequestAsync(await CreateRequestForDetailsOrAction(HttpMethod.Get, detailURL, xcv, tcv, operation));
        return lobResponse;
    }

    /// <summary>
    /// Extracts the approver chain.
    /// </summary>
    /// <param name="responseString">The response string.</param>
    /// <param name="currentApprover">The current approver.</param>
    /// <param name="loggedInUser">The logged in user.</param>
    public virtual JArray ExtractApproverChain(string responseString, string currentApprover, string loggedInUser)
    {
        JArray approverChainArray = new JArray();
        if (!string.IsNullOrEmpty(loggedInUser) && !currentApprover.Equals(loggedInUser, StringComparison.InvariantCultureIgnoreCase))
        {
            approverChainArray.Add(JObject.FromObject(new { Name = NameResolutionHelper.GetUserName(currentApprover), Type = string.Empty, Alias = currentApprover }));
        }
        return approverChainArray;
    }

    /// <summary>
    /// Creates the future approver chain.
    /// </summary>
    /// <param name="approverChainFromTenant">The approver chain from tenant.</param>
    /// <param name="documentSummary">The document summary.</param>
    /// <param name="alias">The alias.</param>
    /// <returns>returns JArray</returns>
    public JArray CreateFutureApproverChain(JArray approverChainFromTenant, ApprovalSummaryRow documentSummary, string alias)
    {
        JArray approverChainArray = new JArray();

        bool futureApprover = false;

        foreach (JToken token in approverChainFromTenant)
        {
            if (futureApprover == false)
            {
                if (token["Alias"].ToString().Trim().Equals(documentSummary.Approver, StringComparison.InvariantCultureIgnoreCase))
                {
                    if (!token["Alias"].ToString().Trim().Equals(alias, StringComparison.InvariantCultureIgnoreCase))
                    {
                        approverChainArray.Add(JObject.FromObject(new { Name = token["Name"], Type = token["Type"], Alias = token["Alias"], _future = false, ActionTaken = string.Empty }));
                    }

                    futureApprover = true;
                }
            }
            else
            {
                approverChainArray.Add(JObject.FromObject(new { Name = token["Name"], Type = token["Type"], Alias = token["Alias"], _future = true }));
            }
        }

        return approverChainArray;
    }

    #endregion Implemented Methods

    #region TenantBase Methods

    /// <summary>
    /// Gets the detail URL.
    /// </summary>
    /// <param name="urlFormat">The URL format.</param>
    /// <param name="approvalIdentifier">The approval identifier.</param>
    /// <param name="page">The page.</param>
    /// <param name="xcv">The XCV.</param>
    /// <param name="tcv">The TCV.</param>
    /// <param name="businessProcessName">Name of the business process.</param>
    /// <param name="docTypeId">document type id</param>
    protected abstract Task<string> GetDetailURL(string urlFormat, ApprovalIdentifier approvalIdentifier, int page, string xcv = "", string tcv = "", string businessProcessName = "", string docTypeId = "");

    /// <summary>
    /// Gets the detail URL using attachment identifier.
    /// </summary>
    /// <param name="urlFormat">The URL format.</param>
    /// <param name="attachmentId">The attachment identifier.</param>
    /// <param name="page">The page.</param>
    protected virtual string GetDetailURLUsingAttachmentId(string urlFormat, string attachmentId)
    {
        throw new NotImplementedException();
    }

    /// <summary>
    /// Validates the detail response asynchronous.
    /// </summary>
    /// <param name="lobResponse">The lob response.</param>
    protected virtual async Task ValidateDetailResponseAsync(HttpResponseMessage lobResponse)
    {
        await JSONHelper.ValidateJsonResponse(Config, lobResponse, approvalTenantInfo.AppName);
    }

    /// <summary>
    /// Fetch Details from Tenant for a particular operation
    /// </summary>
    /// <param name="approvalIdentifier"></param>
    /// <param name="operation"></param>
    /// <param name="page"></param>
    /// <param name="receiptName"></param>
    /// <returns>returns string</returns>
    protected virtual string MapApproverNotes(string response, string approverNotesField)
    {
        if (response.IsJson())
        {
            JObject jobject = response.ToJObject();
            jobject.Add("ApproverNotes", jobject.TryGetValue(approverNotesField, out JToken value) ? value.ToString() : string.Empty);
            response = jobject.ToJson();
        }
        return response;
    }

    /// <summary>
    /// Futures the name of the approver chain operation.
    /// </summary>
    protected abstract string FutureApproverChainOperationName();

    /// <summary>
    /// Attachments the name of the operation.
    /// </summary>
    public virtual string AttachmentOperationName()
    {
        return Constants.DocumentDownloadAction;
    }

    /// <summary>
    /// Attachments the name of the details operation.
    /// </summary>
    /// <returns>Attachment Name as string</returns>
    public virtual string AttachmentDetailsOperationName()
    {
        return AttachmentOperationName();
    }

    /// <summary>
    /// Formulate HTTP Response message to be returned to the client
    /// that sends XML response which is then converted into JSON and sent back to the clients
    /// </summary>
    /// <param name="lobResponse"></param>
    /// <returns>returns task containing Http Response message</returns>
    protected virtual async Task<HttpResponseMessage> ExecutePostDetailOperationAsync(HttpResponseMessage lobResponse, string operation, ApprovalIdentifier approvalIdentifier)
    {
        if (operation.Equals(FutureApproverChainOperationName(), StringComparison.InvariantCultureIgnoreCase))
        {
            string currentApprover = string.Empty;

            #region GET DOCUMENT SUMMARY FOR CURRENT APPROVER

            ApprovalSummaryRow documentSummary = null;
            string currentUser = string.Empty;
            if (!string.IsNullOrEmpty(Alias))
            {
                currentUser = Alias;
            }

            using (IDisposable trace1 = PerformanceLogger.StartPerformanceLogger("PerfLog", Constants.WebClient, string.Format(Constants.PerfLogActionWithInfo, approvalTenantInfo.AppName, operation, "Approver Chain : Getting current document"), new Dictionary<LogDataKey, object> { { LogDataKey.DocumentNumber, approvalIdentifier.DocumentNumber }, { LogDataKey.UserAlias, currentUser } }))
            {
                var approverEntity = ApprovalDetailProvider.GetApprovalsDetails(approvalTenantInfo.TenantId, approvalIdentifier.DisplayDocumentNumber, Constants.CurrentApprover);
                if (approverEntity != null)
                {
                    var approvers = approverEntity.JSONData.FromJson<List<Approver>>();
                    if (approvers.Count > 0)
                    {
                        documentSummary = ApprovalSummaryProvider.GetApprovalSummaryByDocumentNumberAndApprover(approvalTenantInfo.DocTypeId, approvalIdentifier.DisplayDocumentNumber, approvers.FirstOrDefault().Alias);
                    }
                }
                if (documentSummary == null)
                {
                    ApprovalRequestExpression tempApprovalRequestExpression = new ApprovalRequestExpressionV1()
                    {
                        DocumentTypeId = new Guid(approvalTenantInfo.DocTypeId),
                        ApprovalIdentifier = approvalIdentifier
                    };
                    documentSummary = ApprovalSummaryProvider.GetDocumentSummaryByRowKey(tempApprovalRequestExpression.ApprovalIdentifier.ToAzureTableRowKey(approvalTenantInfo)).FirstOrDefault();
                }
            }

            if (documentSummary != null)
            {
                currentApprover = documentSummary.PartitionKey;
            }

            #endregion GET DOCUMENT SUMMARY FOR CURRENT APPROVER

            string responseString = await lobResponse.Content.ReadAsStringAsync();
            JArray approverChainFromTenant = ExtractApproverChain(responseString, currentApprover, currentUser);

            if (approverChainFromTenant != null && approverChainFromTenant.Any())
            {
                #region CREATE FUTURE APPROVER CHAIN

                if (documentSummary != null &&
                        documentSummary.SummaryJson != null &&
                        (documentSummary.SummaryJson).IsJson() && (
                            (documentSummary.SummaryJson).ToJObject().Property("ApprovalHierarchy") == null
                            ||
                            (documentSummary.SummaryJson).ToJObject().Property("ApprovalHierarchy").Value.Type == JTokenType.Null))
                {
                    JArray approverChainArray = CreateFutureApproverChain(approverChainFromTenant, documentSummary, currentUser);
                    JObject responseObject = responseString.ToJObject();
                    responseObject["Approvers"] = approverChainArray;
                    lobResponse = new HttpResponseMessage { Content = new StringContent(responseObject.ToString()), StatusCode = lobResponse.StatusCode };
                }

                #endregion CREATE FUTURE APPROVER CHAIN
            }
        }
        else if (operation.Equals(Constants.AdditionalDetails, StringComparison.InvariantCultureIgnoreCase))
        {
            var responseString = await lobResponse.Content.ReadAsStringAsync();
            if (!string.IsNullOrWhiteSpace(responseString) && responseString.IsJson())
            {
                var summaryData = responseString.FromJson<SummaryJson>();
                if (summaryData != null && summaryData.AdditionalData != null && summaryData.AdditionalData.Any())
                {
                    var responseObject = JObject.FromObject(new { AdditionalData = summaryData.AdditionalData.ToJToken() });
                    var modifiedResponseObject = ExecutePostAuthSum(responseObject);
                    lobResponse = new HttpResponseMessage { Content = new StringContent(modifiedResponseObject.ToJson()), StatusCode = lobResponse.StatusCode };
                }
            }
        }
        return lobResponse;
    }

    /// <summary>
    /// Post process details
    /// </summary>
    /// <param name="jsonDetail"></param>
    /// <param name="operation"></param>
    /// <returns></returns>
    public virtual string PostProcessDetails(string jsonDetail, string operation)
    {
        return jsonDetail;
    }

    /// <summary>
    /// Method to extract the Editable fields from details json, if any
    /// This method should be overridden appropriately for respective tenants
    /// </summary>
    /// <param name="jsonDetail"></param>
    /// <param name="operation"></param>
    /// <returns></returns>
    public virtual string AddEditableFieldsProperties(string jsonDetail, string operation)
    {
        return jsonDetail;
    }

    #endregion TenantBase Methods

    #endregion Get Details Methods

    #region Load Details Methods (Fetches Request details either from Azure Storage or Tenant)

    #region Implemented Methods

    /// <summary>
    /// Fetches Request details either from Azure Storage or Tenant
    /// </summary>
    /// <param name="tenantInfo">Approval Tenant Info</param>
    /// <param name="approvalIdentifier">Approval Identifier</param>
    /// <param name="operation">Operation</param>
    /// <param name="page">Page number</param>
    /// <param name="loggedInAlias"></param>
    /// <param name="xcv">X-correlated ID</param>
    /// <param name="tcv">T-correlated ID</param>
    /// <param name="clientDevice">Client Device</param>
    /// <returns>returns task containing http response message</returns>
    public async Task<HttpResponseMessage> LoadDetailAsync(ApprovalTenantInfo tenantInfo, ApprovalIdentifier approvalIdentifier, string operation, int page, string loggedInAlias, string xcv, string tcv, string clientDevice)
    {
        #region Logging

        var logData = new Dictionary<LogDataKey, object>
        {
            { LogDataKey.Tcv, tcv },
            { LogDataKey.ReceivedTcv, tcv },
            { LogDataKey.SessionId, tcv },
            { LogDataKey.Xcv, xcv },
            { LogDataKey.UserRoleName, loggedInAlias },
            { LogDataKey.BusinessProcessName, string.Format(approvalTenantInfo.BusinessProcessName, Constants.BusinessProcessNameGetDetailsFromTenant, Constants.BusinessProcessNameUserTriggered) },
            { LogDataKey.TenantId, tenantInfo.TenantId },
            { LogDataKey.DocumentNumber, approvalIdentifier.DocumentNumber },
            { LogDataKey.FiscalYear, approvalIdentifier.FiscalYear },
            { LogDataKey.Operation, operation },
            { LogDataKey.UserAlias, Alias },
            { LogDataKey.ClientDevice, clientDevice }
        };

        #endregion Logging

        // Checks whether the current operation is cached or not
        var operationCached = tenantInfo.DetailOperations.DetailOpsList.Find(item => item.operationtype == operation);
        var isOperationCached = false;
        var isDetailsPresentInStorage = false;
        if (operationCached != null)
        {
            isOperationCached = operationCached.IsCached;
        }
        HttpResponseMessage responseAdaptor = null;
        HttpContent responseContent = null;
        HttpStatusCode statusCode;

        #region Fetch Request Details from Azure Storage

        if (isOperationCached)
        {
            try
            {
                using (PerformanceLogger.StartPerformanceLogger("PerfLog", string.IsNullOrWhiteSpace(clientDevice) ? Constants.WebClient : clientDevice, string.Format(Constants.PerfLogActionWithInfo, tenantInfo.AppName, operation, "from storage"),
                    new Dictionary<LogDataKey, object> { { LogDataKey.DocumentNumber, approvalIdentifier.DocumentNumber } }))
                {
                    //fetch cached details here
                    ApprovalDetailsEntity content = (ApprovalDetailProvider.GetApprovalsDetails(tenantInfo.TenantId, approvalIdentifier.DocumentNumber, operation));
                    if (content != null)
                    {
                        responseAdaptor = new HttpResponseMessage
                        {
                            Content = new StringContent(content.JSONData),
                            StatusCode = HttpStatusCode.OK
                        };
                        isDetailsPresentInStorage = true;
                    }
                }
            }
            catch (Exception storageDetailException)
            {
                if (logData[LogDataKey.FiscalYear] == null)
                {
                    logData[LogDataKey.FiscalYear] = "";
                }
                Logger.LogError(TrackingEvent.AzureStorageGetRequestDetailsFail, storageDetailException, logData);
                responseAdaptor = null;
                isDetailsPresentInStorage = false;
            }
        }

        #endregion Fetch Request Details from Azure Storage

        #region Fetch Request Details from Tenant

        if (responseAdaptor == null)
        {
            try
            {
                string businessProcessName = string.Format(approvalTenantInfo.BusinessProcessName, Constants.BusinessProcessNameGetDetailsFromTenant, Constants.BusinessProcessNameUserTriggered);
                using (PerformanceLogger.StartPerformanceLogger("PerfLog", string.IsNullOrWhiteSpace(clientDevice) ? Constants.WebClient : clientDevice, string.Format(Constants.PerfLogActionWithInfo, tenantInfo.AppName, operation, "from tenant"),
                    new Dictionary<LogDataKey, object> { { LogDataKey.DocumentNumber, approvalIdentifier.DocumentNumber } }))
                {
                    responseAdaptor = await GetDetailAsync(approvalIdentifier, operation, page, loggedInAlias, xcv, tcv, businessProcessName, true, clientDevice);
                }
            }
            catch (Exception tenantDetailException)
            {
                Logger.LogError(TrackingEvent.TenantApiFail, tenantDetailException, logData);
                responseAdaptor = new HttpResponseMessage
                {
                    StatusCode = HttpStatusCode.OK,
                    Content = new StringContent(JObject.FromObject(
                                new { Message = string.Format(Constants.TenantDetailFetchFailureMessage, Config[ConfigurationKey.SupportEmailId.ToString()]) }).ToJson())
                };
                return responseAdaptor;
            }
        }

        #endregion Fetch Request Details from Tenant

        responseContent = responseAdaptor.Content;
        statusCode = responseAdaptor.StatusCode;

        #region Add Request Details to Storage if not present

        try
        {
            if (isOperationCached && !isDetailsPresentInStorage)
            {
                if (ApprovalDetailProvider.GetApprovalsDetails(tenantInfo.TenantId, approvalIdentifier.DisplayDocumentNumber, Constants.CurrentApprover) != null)
                {
                    List<ApprovalDetailsEntity> detailsRows = new List<ApprovalDetailsEntity>();
                    ApprovalDetailsEntity detailsRow = new ApprovalDetailsEntity();
                    if (statusCode.Equals(HttpStatusCode.OK))
                    {
                        detailsRow.JSONData = await responseContent.ReadAsStringAsync();
                        detailsRow.TenantID = tenantInfo.TenantId;
                        detailsRow.PartitionKey = approvalIdentifier.GetDocNumber(tenantInfo);
                        detailsRow.RowKey = operation;
                    }
                    detailsRows.Add(detailsRow);
                    await ApprovalDetailProvider.AddApprovalsDetails(detailsRows, tenantInfo, loggedInAlias, xcv, tcv, true);
                }
            }
        }
        catch (Exception failedToAddDetailsInAzureTable)
        {
            Logger.LogError(TrackingEvent.AzureStorageAddRequestDetailsFail, failedToAddDetailsInAzureTable, logData);
        }

        return responseAdaptor;

        #endregion Add Request Details to Storage if not present
    }

    #endregion Implemented Methods

    #endregion Load Details Methods (Fetches Request details either from Azure Storage or Tenant)

    #region DocumentAction Methods

    #region Implemented Methods

    /// <summary>
    /// Executes the action asynchronously.
    /// </summary>
    /// <param name="approvalRequests">The list of approval request.</param>
    /// <param name="loggedInAlias">The logged in alias.</param>
    /// <param name="sessionId">The session identifier.</param>
    /// <param name="clientDevice">The client device.</param>
    /// <param name="xcv">The xcv.</param>
    /// <param name="tcv">The tcv.</param>
    /// <param name="summaryRowParent">The summary row parent.</param>
    /// <returns>returns task containing HttpResponseMessage</returns>
    public virtual async Task<HttpResponseMessage> ExecuteActionAsync(List<ApprovalRequest> approvalRequests, string loggedInAlias, string sessionId, string clientDevice, string xcv, string tcv, ApprovalSummaryRow summaryRowParent = null)
    {
        HttpResponseMessage lobResponse = null;
        var tenantId = approvalTenantInfo.TenantId;
        var approvalRequest = approvalRequests.FirstOrDefault();
        if (approvalRequest == null)
        {
            throw new InvalidDataException("Invalid ApprovalRequest object");
        }
        var logData = new Dictionary<LogDataKey, object>
        {
            { LogDataKey.Tcv, tcv },
            { LogDataKey.TenantTelemetryData, approvalRequest.Telemetry.TenantTelemetry },
            { LogDataKey.Xcv, xcv },
            { LogDataKey.SessionId, sessionId },
            { LogDataKey.ClientDevice, clientDevice },
            { LogDataKey.TenantId, tenantId },
            { LogDataKey.UserAlias, Alias },
            { LogDataKey.UserRoleName, loggedInAlias }
        };

        try
        {
            SetupGenericErrorMessage(approvalRequest);

            string documentNumber = approvalRequest.ApprovalIdentifier.GetDocNumber(approvalTenantInfo);
            // TODO:: Revisit this as just for logging we are making one summary call here which we can pass via parameter as calling method must have summary information already
            ApprovalSummaryRow summary = summaryRowParent ?? ApprovalSummaryProvider.GetApprovalSummaryByDocumentNumberAndApprover(approvalTenantInfo.DocTypeId, documentNumber, Alias);
            string endPointUrl = GetTenantActionUrl(null, approvalRequest.Action);

            logData.Add(LogDataKey.DocumentNumber, documentNumber);
            logData.Add(LogDataKey.DXcv, documentNumber);
            logData.Add(LogDataKey.BusinessProcessName, string.Format(approvalTenantInfo.BusinessProcessName, Constants.BusinessProcessNameApprovalAction, approvalRequest.Action));
            logData.Add(LogDataKey.ReceivedTcv, summary?.Tcv);

            if (String.IsNullOrEmpty(endPointUrl))
            {
                throw new UriFormatException(Config[ConfigurationKey.Message_URLNotDefined.ToString()]);
            }

            HttpMethod reqHttpMethod = GetHttpMethodForAction();

            HttpRequestMessage reqMessage = await CreateRequestForDetailsOrAction(reqHttpMethod, endPointUrl, xcv, tcv, Constants.OperationTypeAction);

            var actionContentForSubmissionIntoTenantService = PrepareActionContentForSubmissionIntoTenantService(approvalRequests, summary);
            logData.Add(LogDataKey.UserActionsString, actionContentForSubmissionIntoTenantService);
            reqMessage.Content = new StringContent(actionContentForSubmissionIntoTenantService, UTF8Encoding.UTF8, Constants.ContentTypeJson);

            using (PerformanceLogger.StartPerformanceLogger("PerfLog", string.IsNullOrWhiteSpace(clientDevice) ? Constants.WebClient : clientDevice, string.Format(Constants.PerfLogAction, approvalTenantInfo.AppName, "Document Action : LOB call"),
                new Dictionary<LogDataKey, object> { { LogDataKey.UserActionsString, approvalRequests.ToJson() } }))
            {
                logData.Add(LogDataKey.EventId, GetEventId(OperationType.ActionInitiated));
                logData.Add(LogDataKey.EventName, approvalTenantInfo.AppName + "-" + OperationType.ActionInitiated);
                Logger.LogInformation(GetEventId(OperationType.ActionInitiated), logData);

                lobResponse = await SendRequestAsync(reqMessage, logData, clientDevice);

                logData.Modify(LogDataKey.EventId, GetEventId(OperationType.ActionComplete));
                logData.Modify(LogDataKey.EventName, approvalTenantInfo.AppName + "-" + OperationType.ActionComplete);
                logData.Add(LogDataKey.ResponseStatusCode, lobResponse.StatusCode);
                Logger.LogInformation(GetEventId(OperationType.ActionComplete), logData);
            }

            return await FormulateActionResponse(lobResponse, approvalRequests, loggedInAlias, sessionId, clientDevice, tcv);
        }
        catch (Exception ex)
        {
            logData.Modify(LogDataKey.EventId, GetEventId(OperationType.Action));
            logData.Modify(LogDataKey.EventName, approvalTenantInfo.AppName + "-" + OperationType.Action);

            var responseContent = lobResponse?.Content != null ? await lobResponse?.Content?.ReadAsStringAsync() : string.Empty;
            logData.Add(LogDataKey.ResponseContent, responseContent);
            Logger.LogError(GetEventId(OperationType.Action), ex, logData);
            throw;
        }
    }

    /// <summary>
    /// Updates the summary row and approval details entity.
    /// </summary>
    /// <param name="approvalRequest">The approval request.</param>
    /// <param name="isSoftDelete">if set to <c>true</c> [is soft delete].</param>
    /// <param name="exceptionMessage">The exception message.</param>
    /// <param name="loggedInAlias">The logged in alias.</param>
    /// <param name="sessionId">The session identifier.</param>
    /// <param name="summaryRowParent">The summary row parent.</param>
    /// <returns>returns tuple of ApprovalSummaryRow and List of ApprovalDetailsEntity</returns>
    public Tuple<ApprovalSummaryRow, List<ApprovalDetailsEntity>> UpdateTransactionalDetails(ApprovalRequest approvalRequest, bool isSoftDelete, string exceptionMessage, string loggedInAlias, string sessionId, ApprovalSummaryRow summaryRowParent = null)
    {
        var logData = new Dictionary<LogDataKey, object>
        {
            { LogDataKey.SessionId, sessionId },
            { LogDataKey.UserRoleName, loggedInAlias },
            { LogDataKey.TenantId, approvalTenantInfo.TenantId },
            { LogDataKey.UserAlias, Alias }
        };
        try
        {
            logData.Add(LogDataKey.UserActionsString, approvalRequest.ToJson());
            logData.Add(LogDataKey.Xcv, approvalRequest.Telemetry.Xcv);
            logData.Add(LogDataKey.Tcv, approvalRequest.Telemetry.Tcv);
            logData.Add(LogDataKey.ReceivedTcv, approvalRequest.Telemetry.Tcv);
            logData.Add(LogDataKey.TenantTelemetryData, approvalRequest.Telemetry.TenantTelemetry);
            ApprovalSummaryRow approvalSummaryRow = summaryRowParent ?? GetSummaryObject(approvalRequest.ApprovalIdentifier.GetDocNumber(approvalTenantInfo));

            // Get all details from ApprovalDetails table and filter to get only the row which has TransactionalDetails (LastfailedException message etc.)
            List<ApprovalDetailsEntity> transactionalDetails = ApprovalDetailProvider.GetAllApprovalsDetails(approvalTenantInfo.TenantId, approvalRequest.ApprovalIdentifier.GetDocNumber(approvalTenantInfo));
            JObject previousExceptionsMessages = new JObject();
            if (transactionalDetails != null && transactionalDetails.Any())
            {
                var previousExceptionsMessagesRow = transactionalDetails.FirstOrDefault(x => x.RowKey.Equals(Constants.TransactionDetailsOperationType + '|' + Alias, StringComparison.InvariantCultureIgnoreCase));
                if (previousExceptionsMessagesRow != null)
                {
                    previousExceptionsMessages = previousExceptionsMessagesRow.JSONData.ToJObject();
                }
            }

            logData.Add(LogDataKey.BusinessProcessName, string.Format(approvalTenantInfo.BusinessProcessName, Constants.BusinessProcessNameApprovalAction, approvalRequest.Action));
            if (approvalSummaryRow != null)
            {
                approvalSummaryRow.WaitForLOBResponse = isSoftDelete;
                approvalSummaryRow.ActionTakenOnClient = String.IsNullOrWhiteSpace(ClientDevice) ? "None" : ClientDevice;
                if (IsTenantPendingUpdateRequired(approvalRequest.Action, approvalRequest))
                {
                    approvalSummaryRow.LobPending = isSoftDelete;
                }
                DateTime? actionDate = DateTime.Now.AddMinutes(-1);
                if (approvalTenantInfo.HistoryLogging)
                {
                    if (approvalRequest.ActionDetails != null && approvalRequest.ActionDetails.ContainsKey(Constants.ActionDateKey) && !string.IsNullOrEmpty(approvalRequest.ActionDetails[Constants.ActionDateKey]))
                    {
                        actionDate = DateTime.Parse(approvalRequest.ActionDetails[Constants.ActionDateKey]);
                    }
                }
                else
                {
                    actionDate = null;
                }

                if (approvalRequest.Action.Equals(Constants.OutOfSyncAction))
                {
                    approvalSummaryRow.IsOutOfSyncChallenged = false;
                    if (!string.IsNullOrWhiteSpace(exceptionMessage))
                    {
                        if (previousExceptionsMessages["LastFailedOutOfSyncMessage"] != null)
                        {
                            JArray items = previousExceptionsMessages["LastFailedOutOfSyncMessage"].ToString().ToJArray();
                            items.Add(exceptionMessage);
                            previousExceptionsMessages["LastFailedOutOfSyncMessage"] = items;
                        }
                        else
                        {
                            previousExceptionsMessages.Add("LastFailedOutOfSyncMessage", new JArray() { exceptionMessage });
                        }
                    }
                }
                else
                {
                    approvalSummaryRow.LastFailed = !isSoftDelete;
                    if (!string.IsNullOrWhiteSpace(exceptionMessage))
                    {
                        if (previousExceptionsMessages["LastFailedExceptionMessage"] != null)
                        {
                            JArray items = previousExceptionsMessages["LastFailedExceptionMessage"].ToString().ToJArray();
                            items.Add(exceptionMessage);
                            previousExceptionsMessages["LastFailedExceptionMessage"] = items;
                        }
                        else
                        {
                            previousExceptionsMessages.Add("LastFailedExceptionMessage", new JArray() { exceptionMessage });
                        }
                    }
                }

                ApprovalSummaryProvider.ApplyCaseConstraints(approvalSummaryRow);
                ApprovalSummaryProvider.SetNextReminderTime(approvalSummaryRow, DateTime.UtcNow);

                List<ApprovalSummaryRow> summaryRowsToUpdate = new List<ApprovalSummaryRow>() { approvalSummaryRow };
                List<ApprovalDetailsEntity> detailsEntitiesToUpdate = new List<ApprovalDetailsEntity>();
                ApprovalDetailsEntity approvalDetailsEntitySum = new ApprovalDetailsEntity()
                {
                    PartitionKey = approvalRequest?.ApprovalIdentifier?.GetDocNumber(approvalTenantInfo),
                    RowKey = Constants.SummaryOperationType,
                    ETag = global::Azure.ETag.All,
                    JSONData = summaryRowsToUpdate.ToJson(),
                    TenantID = approvalTenantInfo.TenantId
                };
                detailsEntitiesToUpdate.Add(approvalDetailsEntitySum);

                if (previousExceptionsMessages.Count > 0)
                {
                    ApprovalDetailsEntity approvalDetailsEntityTransactionDetails = new ApprovalDetailsEntity()
                    {
                        PartitionKey = approvalRequest?.ApprovalIdentifier?.GetDocNumber(approvalTenantInfo),
                        RowKey = Constants.TransactionDetailsOperationType + '|' + Alias,
                        ETag = global::Azure.ETag.All,
                        JSONData = previousExceptionsMessages.ToString(),
                        TenantID = approvalTenantInfo.TenantId
                    };
                    detailsEntitiesToUpdate.Add(approvalDetailsEntityTransactionDetails);
                }

                return new Tuple<ApprovalSummaryRow, List<ApprovalDetailsEntity>>(approvalSummaryRow,
                    detailsEntitiesToUpdate);
            }

            return null;
        }
        catch (Exception ex)
        {
            // TODO:: IMP:: In case of this error, the action proceeds and the tile will be either visible after successful action
            // or invisible after failure action. This needs to be inspected.
            Logger.LogError(TrackingEvent.SummaryRowUpdateFailed, ex, logData);
            return null;
        }
    }

    /// <summary>
    /// Sets the Generic Error Message with support link
    /// </summary>
    /// <param name="approvalRequest">Approval Request object</param>
    public virtual void SetupGenericErrorMessage(ApprovalRequest approvalRequest)
    {
        var supportEmailId = Config[ConfigurationKey.SupportEmailId.ToString()];
        GenericErrorMessage = string.Format(Constants.GenericErrorMessage, supportEmailId, "Tracking ID:" + approvalRequest.Telemetry.Tcv);
    }

    #endregion Implemented Methods

    #region TenantBase Methods

    /// <summary>
    /// Gets the summary object.
    /// </summary>
    /// <param name="documentNumber">The document number.</param>
    /// <returns>returns approval summary row</returns>
    private ApprovalSummaryRow GetSummaryObject(string documentNumber)
    {
        var summary = ApprovalSummaryProvider.GetApprovalSummaryByDocumentNumberAndApprover(approvalTenantInfo.DocTypeId, documentNumber, Alias);

        if (summary == null)
        {
            throw new InvalidDataException(Config[ConfigurationKey.Message_UnAuthorizedUser.ToString()]);
        }

        return summary;
    }

    /// <summary>
    /// Get the Additional Data from either SummaryJson or ApprovalDetails table
    /// </summary>
    /// <param name="summaryRow">Approval Summary Row</param>
    /// <returns>Dictionary of AdditionalData</returns>
    private Dictionary<string, string> GetAdditionalData(ApprovalSummaryRow summaryRow)
    {
        var additionalDataJson = ApprovalDetailProvider.GetApprovalsDetails(approvalTenantInfo.TenantId, summaryRow.DocumentNumber, Constants.AdditionalDetails)?.JSONData;
        SummaryModel summaryData = summaryRow.SummaryJson.FromJson<SummaryModel>(
                                                            new JsonSerializerSettings
                                                            {
                                                                NullValueHandling = NullValueHandling.Ignore
                                                            });
        return ((summaryData.AdditionalData == null || summaryData.AdditionalData.Count == 0) ?
            additionalDataJson?.ToJObject()[Constants.AdditionalData]?.ToJson()?.FromJson<Dictionary<string, string>>() : summaryData.AdditionalData);
    }

    /// <summary>
    /// Get Additional Data method
    /// </summary>
    /// <param name="additionalData">additional data for the particular request</param>
    /// <param name="displayDocumentNumber">display document number</param>
    /// <param name="tenantId">tenant id for the request</param>
    /// <returns>Returns a dictionary object of type string-string which is additional data</string></returns>
    public virtual Dictionary<string, string> GetAdditionalData(Dictionary<string, string> additionalData, string displayDocumentNumber, int tenantId)
    {
        return additionalData;
    }

    /// <summary>
    /// Gets the action object.
    /// </summary>
    /// <param name="approvalRequests">List of approval requests.</param>
    /// <param name="summaryRowParent">Summary Row</param>
    /// <returns>returns string</returns>
    protected virtual string PrepareActionContentForSubmissionIntoTenantService(List<ApprovalRequest> approvalRequests, ApprovalSummaryRow summaryRowParent = null)
    {
        var actionObjectArray = new JArray();
        var actionObject = new JObject();
        var submissionType = (ActionSubmissionType)approvalTenantInfo.ActionSubmissionType;

        // This is a http request type fork:

        // Case 1: ActionSubmissionType.Single :
        // A single http request is made to the tenant
        // The loop is iterated only once
        // For type List<ApprovalRequest> and actionObject contains the serialized 'ApprovalRequest' object

        // Case 2: ActionSubmissionType.PseudoBulk :
        // Multiple http requests are made to the tenant
        // The loop is iterated only once
        // For type List<ApprovalRequest> and actionObject contains the serialized 'ApprovalRequest' object

        // Case 3: ActionSubmissionType.Bulk :
        // A single http request is made to the tenant
        // The loop iterated multiple times
        // For type List<ApprovalRequest> and actionObjectArray contains the serialized 'List<ApprovalRequest>' object

        // Case 4: ActionSubmissionType.BulkExternal :
        // A single http request is made to the tenant with bulk Approval requests
        // For type List<ApprovalRequest> and actionObjectArray contains the serialized 'List<ApprovalRequest>' object

        foreach (var approvalRequest in approvalRequests)
        {
            ApprovalSummaryRow summaryRow = null;
            Dictionary<string, string> additionalData = null;
            // TODO:: Need to revisit this logic again
            if (approvalRequest.Action != Constants.SingleDownloadAction && submissionType != ActionSubmissionType.BulkExternal)
            {
                summaryRow = summaryRowParent ?? GetSummaryObject(approvalRequest.ApprovalIdentifier.DisplayDocumentNumber);
                additionalData = GetAdditionalData(summaryRow);
            }
            UpdateActionProperties(Alias, summaryRow, approvalRequest, additionalData);

            var tenantOperationDetails = approvalTenantInfo.DetailOperations.DetailOpsList.FirstOrDefault(x => x.operationtype.ToUpper() == Constants.OperationTypeAction);
            var serializationType = tenantOperationDetails != null ? tenantOperationDetails.SerializerType : 0;
            actionObject = serializationType switch
            {
                (int)(SerializerType.DataContractSerializer) => (JSONHelper.ConvertObjectToJSON<ApprovalRequest>(approvalRequest)).ToJObject(),
                _ => (approvalRequest.ToJson()).ToJObject(),
            };

            // This is done to support old tenants
            // Add DocumentKeys in user action for some old tenant
            actionObject.Add(Constants.DocumentKeys, (approvalRequest.ApprovalIdentifier.ToJson()).ToJObject());

            // Addition of TelemetryContract (appInsightsContract) is currently used by MSExpense only
            actionObject.Add(Constants.TelemetryContractName, JObject.FromObject(new { parmBusinessProcessName = approvalRequest.Telemetry.BusinessProcessName, parmDetails = "", parmTCV = approvalRequest.Telemetry.Tcv, parmXCV = approvalRequest.Telemetry.Xcv, Telemetry = new { parmTelemetryKey = "", parmTelemetryValue = "" } }));

            // Assign current approver here in approvalRequest
            if (actionObject[Constants.CurrentApprover] == null)
            {
                actionObject[Constants.CurrentApprover] = approvalRequest.ActionByAlias;
            }

            actionObjectArray.Add((actionObject.ToString()).ToJToken());
        }

        switch (submissionType)
        {
            case ActionSubmissionType.Bulk:
            case ActionSubmissionType.BulkExternal:
                return actionObjectArray.ToString();

            case ActionSubmissionType.Single:
            case ActionSubmissionType.PseudoBulk:
            default:
                return actionObject.ToString();
        }
    }

    /// <summary>
    /// Formulates the action response.
    /// </summary>
    /// <param name="httpResponseMessageFromTenant">The http response received from the tenant.</param>
    /// <param name="approvalRequests">List of approval request.</param>
    /// <param name="loggedInAlias">The logged in alias.</param>
    /// <param name="sessionId">The session identifier.</param>
    /// <param name="clientDevice">The client device.</param>
    /// <param name="tcv">The Tcv.</param>
    /// <returns>returns task containing HttpResponseMessage</returns>
    protected async Task<HttpResponseMessage> FormulateActionResponse(HttpResponseMessage httpResponseMessageFromTenant, List<ApprovalRequest> approvalRequests, string loggedInAlias, string sessionId, string clientDevice, string tcv)
    {
        var approvalRequest = approvalRequests.FirstOrDefault();

        #region Prepare Log Data

        var logData = new Dictionary<LogDataKey, object>
            {
                { LogDataKey.Tcv, tcv },
                { LogDataKey.SessionId, sessionId },
                { LogDataKey.UserRoleName, loggedInAlias },
                { LogDataKey.TenantId, approvalTenantInfo.TenantId },
                { LogDataKey.StartDateTime, DateTime.UtcNow },
                { LogDataKey.Operation, approvalRequest.Action },
                { LogDataKey.UserAlias, approvalRequest.ActionByAlias },
                { LogDataKey.IsCriticalEvent, CriticalityLevel.No.ToString() },
                { LogDataKey.ClientDevice, clientDevice },
                { LogDataKey.ReceivedTcv, tcv },
                { LogDataKey._CorrelationId, tcv },
                { LogDataKey.Approver, approvalRequest.ActionByAlias },
                { LogDataKey.TenantName, approvalTenantInfo.AppName },
                { LogDataKey.BusinessProcessName, string.Format(approvalTenantInfo.BusinessProcessName, Constants.BusinessProcessNameGetDetails, Constants.BusinessProcessNameUserTriggered) },
                { LogDataKey.DocumentTypeId, approvalTenantInfo.DocTypeId }
    };

        #endregion Prepare Log Data

        try
        {
            HttpResponseMessage httpResponseMessageForMSApprovals = new HttpResponseMessage(HttpStatusCode.BadRequest);
            List<ApprovalResponse> approvalResponses = new List<ApprovalResponse>();

            #region ApprovalResponse handler

            // This is done for Proxy based clients which doesn't return any HTTPResponseMessage and hence httpResponseMessageFromTenant is always null and hence instantiating it
            // or this may also happen if there is some exception in Approvals code
            if (httpResponseMessageFromTenant == null)
            {
                foreach (var approvalRequestObj in approvalRequests)
                {
                    approvalResponses.Add(new ApprovalResponse()
                    {
                        ActionResult = true,
                        DisplayMessage = GenericErrorMessage,
                        ApprovalIdentifier = approvalRequestObj.ApprovalIdentifier,
                        DocumentTypeID = approvalRequestObj.DocumentTypeID
                    });
                }
                httpResponseMessageFromTenant = new HttpResponseMessage()
                {
                    Content = new StringContent(approvalResponses.ToJson(), new UTF8Encoding(), Constants.ContentTypeJson),
                    StatusCode = HttpStatusCode.OK
                };
            }

            // Beyond this point httpResponseMessageFromTenant will always exist
            // Handling the httpResponseMessageFromTenant
            string responseString = await httpResponseMessageFromTenant.Content.ReadAsStringAsync();
            if (!responseString.IsJson())
            {
                responseString = responseString.Replace("\\\"", "\"").TrimStart('"').TrimEnd('"');
            }

            if (responseString.IsJson())
            {
                if (responseString.IsJsonArray())
                {
                    // Assumption: If the response is being sent from the tenant in List<ApprovalResponse> then all the properties are filled in properly
                    approvalResponses = ParseResponseString<List<ApprovalResponse>>(responseString);
                    foreach (var approvalResponse in approvalResponses)
                    {
                        FormulateErrorMessages(approvalResponse);
                    }
                }
                else
                {
                    // Here when the response is in either ApprovalResponse format, we need to handle cases where the tenant doesn't send data in correct format
                    // e.g. DocumentNumber or DocumentTypeId is missing.
                    // So, we need to add that here.
                    var approvalResponse = ParseResponseString<ApprovalResponse>(responseString);

                    // Add default items to approvalResponse.
                    if (approvalResponse != null)
                    {
                        FormulateErrorMessages(approvalResponse);

                        // Add generic error message to ApprovalResponse if the DisplayMessage property is null or empty
                        if (string.IsNullOrWhiteSpace(approvalResponse.DisplayMessage))
                        {
                            approvalResponse.DisplayMessage = GenericErrorMessage;
                        }
                        // Add ApprovalIdentifier and DocumentTypeID
                        approvalResponse.ApprovalIdentifier = approvalRequests.FirstOrDefault()?.ApprovalIdentifier;
                        approvalResponse.DocumentTypeID = approvalRequests.FirstOrDefault()?.DocumentTypeID;

                        // Add the ApprovalRespone to the list of ApprovalResponse
                        approvalResponses.Add(approvalResponse);
                    }
                }
            }
            else
            {
                foreach (var approvalRequestObj in approvalRequests)
                {
                    approvalResponses.Add(new ApprovalResponse()
                    {
                        ActionResult = false,
                        ApprovalIdentifier = approvalRequestObj.ApprovalIdentifier,
                        DocumentTypeID = approvalRequestObj.DocumentTypeID
                    });
                }
            }

            #endregion ApprovalResponse handler

            #region Handling based on HttpStatusCode of tenant and creating HttpResponse for Approvals client to consume

            if (httpResponseMessageFromTenant.IsSuccessStatusCode)
            {
                TenantAction action = approvalTenantInfo?.ActionDetails?.Primary?.FirstOrDefault(a => a.Code.Equals(approvalRequest.Action.ToString(), StringComparison.InvariantCultureIgnoreCase));
                if (action == null)
                {
                    action = approvalTenantInfo?.ActionDetails?.Secondary?.FirstOrDefault(a => a.Code.Equals(approvalRequest.Action.ToString(), StringComparison.InvariantCultureIgnoreCase));
                }

                if (approvalResponses.Any(res => res.E2EErrorInformation?.ErrorMessages != null) && approvalResponses.Any(res => res.E2EErrorInformation?.ErrorMessages.Count > 0))
                {
                    httpResponseMessageForMSApprovals = new HttpResponseMessage() { Content = new StringContent(approvalResponses.ToJson(), new UTF8Encoding(), Constants.ContentTypeJson), StatusCode = HttpStatusCode.BadRequest };
                }
                else
                {
                    approvalResponses.ForEach(res => res.ActionResult = true);
                    httpResponseMessageForMSApprovals = new HttpResponseMessage() { Content = new StringContent(approvalResponses.ToJson()), StatusCode = HttpStatusCode.OK };
                }
            }
            else // if (!httpResponseMessageFromTenant.IsSuccessStatusCode)
            {
                // Assumption: If the tenant is not sending anything in the ErrorMessage property in case of failure scenarios,
                // then add the response content in the ErrorMessages property
                // In case the tenant is sending the response in the correct format during the failure scenarios, the ErrorMessages will have the data in it.
                foreach (var approvalResponse in approvalResponses)
                {
                    if (approvalResponse.E2EErrorInformation == null)
                    {
                        approvalResponse.E2EErrorInformation = new ApprovalResponseErrorInfo { ErrorMessages = new List<string>() { responseString } };
                    }
                    else if (approvalResponse.E2EErrorInformation.ErrorMessages == null)
                    {
                        approvalResponse.E2EErrorInformation.ErrorMessages = new List<string>() { responseString };
                    }
                    else
                    {
                        approvalResponse.E2EErrorInformation.ErrorMessages.Add(responseString);
                    }
                }

                httpResponseMessageForMSApprovals = new HttpResponseMessage()
                {
                    StatusCode = HttpStatusCode.BadRequest
                };
            }

            #endregion Handling based on HttpStatusCode of tenant and creating HttpResponse for Approvals client to consume

            if (!httpResponseMessageForMSApprovals.IsSuccessStatusCode)
            {
                for (int i = 0; i < approvalResponses.Count; i++)
                {
                    if (!approvalResponses[i].ActionResult && string.IsNullOrEmpty(approvalResponses[i].DisplayMessage))
                    {
                        approvalResponses[i].DisplayMessage = GenericErrorMessage;
                    }
                }

                httpResponseMessageForMSApprovals.Content = new StringContent(approvalResponses.ToJson(), new UTF8Encoding(), Constants.ContentTypeJson);
            }
            return httpResponseMessageForMSApprovals;
        }
        catch (Exception ex)
        {
            // Log failure event.
            logData.Add(LogDataKey.EndDateTime, DateTime.UtcNow);
            Logger.LogError(TrackingEvent.ApprovalActionFailedOnTenantSide, ex, logData);
            throw;
        }
    }

    /// <summary>
    /// This method will parse response and prepares list of ApprovalResponse
    /// </summary>
    /// <param name="responseString">The responseString.</param>
    /// <returns>List of ApprovalResponse</returns>
    public T ParseResponseString<T>(string responseString)
    {
        var tenantOperationDetails = approvalTenantInfo.DetailOperations.DetailOpsList.FirstOrDefault(x => x.operationtype.ToUpper() == Constants.OperationTypeAction);
        var isLegacyResponse = tenantOperationDetails != null && tenantOperationDetails.IsLegacyResponse;
        if (!isLegacyResponse)
        {
            return responseString.FromJson<T>();
        }
        else
        {
            if (responseString.IsJsonArray())
            {
                var approvalResponseLegacyList = responseString.FromJson<List<ApprovalResponseLegacy>>();
                dynamic approvalResponses = new List<ApprovalResponse>();
                foreach (var approvalResponseLegacy in approvalResponseLegacyList)
                {
                    approvalResponses.Add(ConvertLegacyToLatest(approvalResponseLegacy));
                }

                return approvalResponses;
            }
            else
            {
                var approvalResponseLegacy = responseString.FromJson<ApprovalResponseLegacy>();
                return ConvertLegacyToLatest(approvalResponseLegacy);
            }
        }
    }

    /// <summary>
    /// This method will convert Legacy object to latest object
    /// </summary>
    /// <param name="approvalResponseLegacy">The ApprovalResponseLegacy</param>
    /// <returns>returns converted latest object</returns>
    private dynamic ConvertLegacyToLatest(ApprovalResponseLegacy approvalResponseLegacy)
    {
        var approvalResponse = new ApprovalResponse
        {
            ApprovalIdentifier = approvalResponseLegacy.ApprovalIdentifier,
            Telemetry = approvalResponseLegacy.Telemetry,
            DocumentTypeID = approvalResponseLegacy.DocumentTypeID,
            ActionResult = approvalResponseLegacy.ActionResult,
            DisplayMessage = approvalResponseLegacy.DisplayMessage,
            ErrorMessages = approvalResponseLegacy.ErrorMessages,
            ExtensionData = approvalResponseLegacy.ExtensionData
        };
        if (approvalResponseLegacy.E2EErrorInformation != null)
        {
            approvalResponse.E2EErrorInformation = new ApprovalResponseErrorInfo
            {
                ErrorMessages = new List<string>
                                { approvalResponseLegacy.E2EErrorInformation?.FirstOrDefault()?.ErrorMessage },
                ErrorType = (ApprovalResponseErrorType)Enum.Parse(
                    typeof(ApprovalResponseErrorType),
                    approvalResponseLegacy.E2EErrorInformation?.FirstOrDefault()?.ErrorType.ToString(),
                    true),
                RetryInterval = approvalResponseLegacy.RetryInterval
            };
        }

        return approvalResponse;
    }

    /// <summary>
    /// This Method will prepare ErrorMessages to make sure to convert backword compatible response to new version
    /// </summary>
    /// <param name="approvalResponse"></param>
    private static void FormulateErrorMessages(ApprovalResponse approvalResponse)
    {
        if (approvalResponse.ErrorMessages != null)
        {
            if (approvalResponse.E2EErrorInformation == null)
            {
                approvalResponse.E2EErrorInformation = new ApprovalResponseErrorInfo { ErrorMessages = approvalResponse.ErrorMessages };
            }
            else if (approvalResponse.E2EErrorInformation.ErrorMessages == null)
            {
                approvalResponse.E2EErrorInformation.ErrorMessages = approvalResponse.ErrorMessages;
            }
            else
            {
                approvalResponse.E2EErrorInformation.ErrorMessages.AddRange(approvalResponse.ErrorMessages);
                approvalResponse.E2EErrorInformation.ErrorMessages = approvalResponse.E2EErrorInformation.ErrorMessages.Distinct().ToList();
            }
        }
    }

    /// <summary>
    /// Updates the action properties.
    /// </summary>
    /// <param name="alias">The alias.</param>
    /// <param name="summary">The summary.</param>
    /// <param name="approvalRequest">The approval request.</param>
    /// <param name="additionalData">The Additional Data from ApprovalDetails table</param>
    protected virtual void UpdateActionProperties(string alias, ApprovalSummaryRow summary, ApprovalRequest approvalRequest, Dictionary<string, string> additionalData)
    {
        if (approvalRequest.ActionDetails == null)
        {
            var actionDetails = new Dictionary<string, string>
            {
                { Constants.ActionDateKey, DateTime.UtcNow.ToString() }
            };
            approvalRequest.ActionDetails = actionDetails;
        }
        else
        {
            if (string.IsNullOrWhiteSpace(approvalRequest.ActionDetails[approvalRequest.ActionDetails.Keys.First(k => k.ToLowerInvariant() == Constants.ActionDateKey.ToLowerInvariant())]))
            {
                approvalRequest.ActionDetails[Constants.ActionDateKey] = DateTime.UtcNow.ToString();
            }
        }
        approvalRequest.ActionByAlias = alias;
        if (string.IsNullOrWhiteSpace(approvalRequest.DocumentTypeID))
        {
            approvalRequest.DocumentTypeID = approvalTenantInfo.DocTypeId.ToUpperInvariant();
        }
    }

    /// <summary>
    /// Gets the tenant action URL.
    /// </summary>
    /// <param name="digitalSignature">The digital signature.</param>
    /// <param name="action">The action.</param>
    /// <returns>returns string</returns>
    protected virtual string GetTenantActionUrl(string digitalSignature, string action = "")
    {
        if (action.Equals(Constants.OutOfSyncAction) || action.Equals(Constants.UndoOutOfSyncAction))
        {
            return approvalTenantInfo.GetEndPointURL(Constants.OperationTypeOutOfSync, ClientDevice);
        }
        else
        {
            return approvalTenantInfo.GetEndPointURL(Constants.OperationTypeAction, ClientDevice);
        }
    }

    /// <summary>
    /// Gets the HTTP method for action.
    /// </summary>
    /// <returns>returns http method</returns>
    protected virtual HttpMethod GetHttpMethodForAction()
    {
        return HttpMethod.Post;
    }

    /// <summary>
    /// Determines whether [is tenant pending update required] [the specified action type].
    /// </summary>
    /// <param name="actionType">Type of the action.</param>
    /// <param name="approvalRequest">The approval request.</param>
    /// <returns>
    ///   <c>true</c> if [is tenant pending update required] [the specified action type]; otherwise, <c>false</c>.
    /// </returns>
    protected virtual bool IsTenantPendingUpdateRequired(string actionType, ApprovalRequest approvalRequest)
    {
        bool returnParam = true;
        try
        {
            if (!String.IsNullOrEmpty(approvalTenantInfo.TenantActionDetails))
            {
                TenantAction action = approvalTenantInfo.ActionDetails.Primary.FirstOrDefault(a => a.Code.Equals(actionType, StringComparison.InvariantCultureIgnoreCase));
                if (action == null)
                {
                    action = approvalTenantInfo.ActionDetails.Secondary.FirstOrDefault(a => a.Code.Equals(actionType, StringComparison.InvariantCultureIgnoreCase));
                }
                return action.IsInterimStateRequired;
            }
        }
        catch
        {
            //do nothing - Fails only if Parsing Logic Fails
        }
        return returnParam;
    }

    #endregion TenantBase Methods

    #endregion DocumentAction Methods

    #region POST AUTHSUM operation

    #region Implemented Methods

    /// <summary>
    /// Executes the post authentication sum.
    /// </summary>
    /// <param name="authSumObject">The authentication sum object with available details data</param>
    /// <returns>returns JObject</returns>
    public virtual JObject ExecutePostAuthSum(JObject authSumObject)
    {
        return authSumObject;
    }

    /// <summary>
    /// Removes certain fields from Authsum Object
    /// </summary>
    /// <param name="summaryAuthsumObject">The authentication sum object</param>
    /// <returns>authentication sum object after removal of fields</returns>
    public virtual JObject RemoveFieldsFromResponse(JObject authsumObject)
    {
        return authsumObject;
    }

    #endregion Implemented Methods

    #endregion POST AUTHSUM operation

    #region Download document

    #region Implemented Methods

    /// <summary>
    /// Downloads the attachments in bulk for the given set of requests
    /// </summary>
    /// <param name="approvalRequests">List of ApprovalRequest which is sent to the LoB application as part of the content in the Http call</param>
    /// <param name="loggedInAlias">Alias of the logged in User</param>
    /// <param name="sessionId">GUID session id</param>
    /// <param name="clientDevice">Client Device (Web/WP8..)</param>
    /// <returns>HttpResponseMessage with Stream data of all the attachments</returns>
    public async Task<byte[]> BulkDownloadDocumentAsync(List<ApprovalRequest> approvalRequests, string loggedInAlias, string sessionId, string clientDevice)
    {
        // We are not storing the Attachments in the Blob when there is a Bulk Download option available. Future scope.
        HttpResponseMessage lobResponse;
        byte[] bytArr = null;
        string bulkDownloadLOBEndpointURL = approvalTenantInfo.GetEndPointURL(Constants.BulkDocumentDownloadAction, ClientDevice);

        HttpRequestMessage requestMessage = await CreateRequestForDetailsOrAction(HttpMethod.Post, bulkDownloadLOBEndpointURL, "", "", Constants.BulkDocumentDownloadAction);
        var actionString = PrepareActionContentForSubmissionIntoTenantService(approvalRequests);
        requestMessage.Content = new StringContent(actionString, UTF8Encoding.UTF8, Constants.ContentTypeJson);

        lobResponse = await HttpHelper.SendRequestAsync(requestMessage);

        if (lobResponse.IsSuccessStatusCode)
        {
            using (MemoryStream streamData = (MemoryStream)await lobResponse.Content.ReadAsStreamAsync())
            {
                bytArr = streamData.ToArray();
            }
        }

        return bytArr;
    }

    /// <summary>
    /// Downloads the document using attachment identifier asynchronous.
    /// </summary>
    /// <param name="approvalIdentifier">The approval identifier.</param>
    /// <param name="attachmentId">The attachment identifier.</param>
    /// <param name="telemetry">The telemetry.</param>
    /// <returns></returns>
    public async Task<byte[]> DownloadDocumentUsingAttachmentIdAsync(ApprovalIdentifier approvalIdentifier, string attachmentId, ApprovalsTelemetry telemetry)
    {
        string blobNameFormat = "{0}|{1}|{2}"; //2(tenantId)|572015453(DocumentNumber)|45124525(attachmentId)
        string blobName = string.Format(blobNameFormat, approvalTenantInfo.TenantId, approvalIdentifier.DisplayDocumentNumber, attachmentId);

        var blobExists = await BlobStorageHelper.DoesExist(Constants.NotificationAttachmentsBlobName, blobName);
        if (blobExists) // Check if Attachment is stored in BLOB
        {
            var bytes = await BlobStorageHelper.DownloadByteArray(Constants.NotificationAttachmentsBlobName, blobName);

            return bytes;
        }
        else // if attachment is not found in BLOB, get it from LoB application and also save the attachment in BLOB
        {
            return await GetAttachmentContentFromLob(approvalIdentifier, attachmentId, telemetry);
        }
    }

    /// <summary>
    /// Downloads the document for preview using attachment identifier asynchronous.
    /// </summary>
    /// <param name="approvalIdentifier">The approval identifier.</param>
    /// <param name="attachmentId">The attachment identifier.</param>
    /// <param name="telemetry">The telemetry.</param>
    /// <returns></returns>
    public async Task<byte[]> PreviewDocumentUsingAttachmentIdAsync(ApprovalIdentifier approvalIdentifier, string attachmentId, ApprovalsTelemetry telemetry)
    {
        string blobNameFormat = "{0}|{1}|{2}"; //2(tenantId)|572015453(DocumentNumber)|45124525(attachmentId)
        string blobName = string.Format(blobNameFormat, approvalTenantInfo.TenantId, approvalIdentifier.DisplayDocumentNumber, attachmentId);

        var blobExists = await BlobStorageHelper.DoesExist(Constants.NotificationAttachmentsBlobName, blobName);
        if (blobExists) // Check if Attachment is stored in BLOB
        {
            byte[] target = await BlobStorageHelper.DownloadByteArray(Constants.NotificationAttachmentsBlobName, blobName);
            return target;
        }
        else // if attachment is not found in BLOB, get it from LoB application and also save the attachment in BLOB
        {
            return await GetAttachmentContentFromLob(approvalIdentifier, attachmentId, telemetry);
        }
    }

    /// <summary>
    /// Gets the Attachment content from Line Of Business application and stores that in the blob.
    /// </summary>
    /// <param name="approvalIdentifier">The approval identifier.</param>
    /// <param name="attachmentId">The attachment identifier.</param>
    /// <param name="telemetry">The telemetry.</param>
    /// <returns></returns>
    public virtual async Task<byte[]> GetAttachmentContentFromLob(ApprovalIdentifier approvalIdentifier, string attachmentId, ApprovalsTelemetry telemetry)
    {
        HttpResponseMessage lobResponse;
        var actionName = AttachmentOperationName();
        byte[] bytArr = null;
        string detailURL = GetDetailURLUsingAttachmentId(approvalTenantInfo.GetEndPointURL(actionName, ClientDevice), attachmentId);
        lobResponse = await HttpHelper.SendRequestAsync(await CreateRequestForDetailsOrAction(HttpMethod.Get, detailURL, telemetry.Xcv, telemetry.Tcv, actionName));
        if (lobResponse.IsSuccessStatusCode)
        {
            using (MemoryStream streamData = (MemoryStream)await lobResponse.Content.ReadAsStreamAsync())
            {
                bytArr = streamData.ToArray();
                await SaveAttachmentToBlob(approvalTenantInfo.TenantId, approvalIdentifier, bytArr, attachmentId);
            }
        }
        return bytArr;
    }

    /// <summary>
    /// Gets the attachment details.
    /// </summary>
    /// <param name="summaryRows">The summary rows.</param>
    /// <param name="approvalIdentifier">The approval identifier.</param>
    /// <param name="telemetry">Telemetry object containing Xcv and Tcv.</param>
    /// <returns>returns a task of List of Attachments</returns>
    public async Task<List<Attachment>> GetAttachmentDetails(List<ApprovalSummaryRow> summaryRows, ApprovalIdentifier approvalIdentifier, ApprovalsTelemetry telemetry)
    {
        // Check if Attachment Operation is supported for Tenant or not.
        // If not attachment operation is configured, exit the method.
        var isAttachmentsAvailableForTenant = approvalTenantInfo.DetailOperations.DetailOpsList.FirstOrDefault(x => x.operationtype.Equals(AttachmentDetailsOperationName(), StringComparison.InvariantCultureIgnoreCase)) != null;
        if (!isAttachmentsAvailableForTenant)
        {
            return new List<Attachment>();
        }
        List<Attachment> attachments = summaryRows.FirstOrDefault()?.SummaryJson.FromJson<SummaryJson>().Attachments;
        // If attachments is not found in SummaryJson.Attachments, then query the Details table to get the Attachment details
        if (attachments == null || attachments.Count == 0)
        {
            // Get Attachment details from Approval Details table
            var detailRow = ApprovalDetailProvider.GetApprovalsDetails(approvalTenantInfo.TenantId, approvalIdentifier.DisplayDocumentNumber, AttachmentDetailsOperationName());
            if (detailRow == null)
            {
                // If attachment details are not found in table, get it at real-time from tenant
                var lobResponse = await GetDetailAsync(approvalIdentifier, AttachmentDetailsOperationName(), 0, Environment.UserName, telemetry.Xcv, telemetry.Tcv, telemetry.BusinessProcessName, false, Constants.WorkerRole);
                if (lobResponse.IsSuccessStatusCode)
                {
                    detailRow = new ApprovalDetailsEntity
                    {
                        JSONData = await lobResponse.Content.ReadAsStringAsync(),
                        TenantID = approvalTenantInfo.TenantId,
                        PartitionKey = approvalIdentifier?.GetDocNumber(approvalTenantInfo),
                        RowKey = AttachmentDetailsOperationName()
                    };
                }
            }
            if (detailRow != null)
            {
                var attachmentsObject = detailRow.JSONData.FromJson<JObject>();
                if (attachmentsObject["Attachments"] != null && !string.IsNullOrEmpty(attachmentsObject["Attachments"].ToString()))
                {
                    attachments = attachmentsObject["Attachments"]?.ToString().FromJson<List<Attachment>>().ToList();
                }
                else
                {
                    attachments = new List<Attachment>();
                }
            }
            else
            {
                attachments = new List<Attachment>();
            }
        }

        // Combine the list of attachments uploaded from the ui for the approval request.
        // Get the approval transaction details for the given document id.
        List<Attachment> attachmentsSummary = new List<Attachment>();
        var approvalDetails = ApprovalDetailProvider.GetAllApprovalsDetails(approvalTenantInfo.TenantId, approvalIdentifier.DisplayDocumentNumber);

        if (approvalDetails != null && approvalDetails.Any())
        {
            // Filter to get only the row which has TransactionalDetails
            var existingAttachmentsRecord = approvalDetails.FirstOrDefault(x => x.RowKey.Equals(Constants.AttachmentsOperationType, StringComparison.InvariantCultureIgnoreCase));

            if (existingAttachmentsRecord != null)
            {
                attachmentsSummary = JsonConvert.DeserializeObject<List<Attachment>>(existingAttachmentsRecord?.JSONData);

                if (attachmentsSummary.Any())
                {
                    foreach (var attachment in attachmentsSummary)
                    {
                        attachments.Add(new Attachment()
                        {
                            ID = attachment.ID,
                            IsPreAttached = attachment.IsPreAttached,
                            Name = attachment.Name,
                            Url = attachment.Url
                        });
                    }
                }
            }
        }

        return attachments;
    }

    /// <summary>
    /// Saves the attachment to BLOB only if this is an active request. For Historical requests, let the call be realtime always
    /// </summary>
    /// <param name="tenantId">The tenant identifier.</param>
    /// <param name="approvalIdentifier">The approval identifier.</param>
    /// <param name="bytes">The bytes.</param>
    /// <param name="attachmentId">The attachment identifier.</param>
    /// <returns>returns string</returns>
    protected virtual async Task<string> SaveAttachmentToBlob(int tenantId, ApprovalIdentifier approvalIdentifier, byte[] bytes, string attachmentId)
    {
        string blobId = null;
        try
        {
            // iff this is an active request, store the attachment in BLOB
            if (ApprovalDetailProvider.GetApprovalsDetails(approvalTenantInfo.TenantId, approvalIdentifier.DisplayDocumentNumber, Constants.CurrentApprover) != null)
            {
                string blobNameFormat = "{0}|{1}|{2}"; //2(tenantId)|572015453(DocumentNumber)|45124525(attachmentId)
                string blobName = string.Format(blobNameFormat, tenantId, approvalIdentifier.DisplayDocumentNumber, attachmentId);

                await BlobStorageHelper.UploadByteArray(bytes, Constants.NotificationAttachmentsBlobName, blobName);
                var storageAccountName = Config[ConfigurationKey.StorageAccountName.ToString()];
                blobId = string.Format(@"https://{0}.blob.core.windows.net/{1}/{2}", storageAccountName, Constants.NotificationAttachmentsBlobName, blobName);
            }

            return blobId;
        }
        catch
        {
            return null;
        }
    }

    /// <summary>
    /// Checks whether Email With Approval functionality is configured for the specific Tenant or not.
    /// Return true if enabled, else returns false
    /// </summary>
    /// <param name="documentSummaries"></param>
    /// <returns>returns bool</returns>
    public virtual bool ValidateIfEmailShouldBeSentWithDetails(List<ApprovalSummaryRow> documentSummaries)
    {
        return approvalTenantInfo.NotifyEmailWithApprovalFunctionality;
    }

    /// <summary>
    /// Send regular email if user is not added into ActionableEmail flighting feature
    /// </summary>
    /// <param name="isActionableEmailSent"></param>
    /// <returns></returns>
    public virtual bool ShouldSendRegularEmail(bool isActionableEmailSent)
    {
        //// Send normal email to user if he is not a part of flighting feature(Actionable email)
        return approvalTenantInfo.NotifyEmail && !isActionableEmailSent;
    }

    /// <summary>
    /// Checks whether Email With Approval functionality is configured for the specific Tenant or not.
    /// Return true if enabled, else returns false
    /// </summary>
    /// <param name="documentSummaries"></param>
    /// <param name="featureName"></param>
    /// <returns>returns bool</returns>
    protected bool IsActionableEmailAllowed(List<ApprovalSummaryRow> documentSummaries, FlightingFeatureName featureName)
    {
        var isFlightingEnabled = false;
        var logData = new Dictionary<LogDataKey, object>
        {
            { LogDataKey.EventType, Constants.FeatureUsageEvent },
            { LogDataKey.TenantId, approvalTenantInfo.TenantId },
            { LogDataKey.TenantName, approvalTenantInfo.AppName }
        };
        try
        {
            if (documentSummaries == null || documentSummaries.Count == 0)
            {
                return false;
            }

            logData.Add(LogDataKey.Tcv, documentSummaries.FirstOrDefault()?.Tcv);
            logData.Add(LogDataKey.ReceivedTcv, documentSummaries.FirstOrDefault()?.Tcv);
            logData.Add(LogDataKey.Xcv, documentSummaries.FirstOrDefault()?.Xcv);
            logData.Add(LogDataKey.DXcv, documentSummaries.FirstOrDefault()?.DocumentNumber);
            logData.Add(LogDataKey.FeatureId, (int)featureName);

            isFlightingEnabled = FlightingDataProvider.IsFeatureEnabledForUser(documentSummaries.FirstOrDefault()?.Approver, (int)featureName);

            return approvalTenantInfo.NotifyEmailWithApprovalFunctionality && isFlightingEnabled;
        }
        catch (Exception ex)
        {
            Logger.LogError(TrackingEvent.ValidateIfEamilSentWithDetailsFail, ex, logData);
            return isFlightingEnabled;
        }
    }

    /// <summary>
    /// Processes the ARX objects and modifies them specific to tenant.
    /// Default implementation doesn't modify the ARX object and return the object as is.
    /// </summary>
    /// <param name="requestExpressions"></param>
    /// <returns>returns list containing Approval REquest Expression ext</returns>
    public virtual List<ApprovalRequestExpressionExt> ModifyApprovalRequestExpression(List<ApprovalRequestExpressionExt> requestExpressions)
    {
        // Added 'To' alias in NotificationDetails as the Approver alias. No CC aliases required.
        // ARX is modified in case of CREATE and UPDATE operation only
        List<ApprovalRequestExpressionExt> modifiedRequestExpressions = new List<ApprovalRequestExpressionExt>();
        foreach (var requestExpression in requestExpressions)
        {
            AddAdditionalDataToDetailsData(requestExpression, requestExpression.SummaryData, string.Empty);
            if (approvalTenantInfo.NotifyEmailWithApprovalFunctionality && requestExpression.NotificationDetail == null && (requestExpression.Operation == ApprovalRequestOperation.Create || requestExpression.Operation == ApprovalRequestOperation.Update) && requestExpression.Approvers != null)
            {
                requestExpression.NotificationDetail = new NotificationDetail()
                {
                    // TODO:: Get the TemplateKey from Configuration
                    SendNotification = true,
                    To = requestExpression.Approvers.FirstOrDefault().Alias + Config[ConfigurationKey.DomainName.ToString()],
                    TemplateKey = "PendingApproval"
                };
            }

            modifiedRequestExpressions.Add(requestExpression);
        }
        return modifiedRequestExpressions;
    }

    /// <summary>
    /// Method to extract AdditionalData from SummaryData and add it into DetailsData
    /// </summary>
    /// <param name="arxExtended">Approval Request Expression</param>
    /// <param name="summaryJson">Summary Data</param>
    /// <param name="additionalDetails">Additional Data</param>
    public void AddAdditionalDataToDetailsData(ApprovalRequestExpressionExt arxExtended, Contracts.DataContracts.SummaryJson summaryJson, string additionalDetails)
    {
        if (arxExtended.DetailsData == null)
        {
            arxExtended.DetailsData = new Dictionary<string, string>();
        }
        if (!arxExtended.DetailsData.ContainsKey(Constants.AdditionalDetails))
        {
            string additionalDetailsJson;
            if (summaryJson != null && summaryJson.AdditionalData != null)
            {
                additionalDetailsJson = JObject.FromObject(new { AdditionalData = summaryJson.AdditionalData.ToJToken() }).ToJson();
                arxExtended.DetailsData.Add(Constants.AdditionalDetails, additionalDetailsJson);
            }
            if (!string.IsNullOrEmpty(additionalDetails))
            {
                arxExtended.DetailsData.Add(Constants.AdditionalDetails, additionalDetails);
            }
        }
    }

    /// <summary>
    /// Returns the Title and Description which needs to be stored in database for display purpose
    /// </summary>
    /// <param name="summaryJSON"></param>
    /// <returns></returns>
    public virtual string GetRequestTitleDescription(JObject summaryJSON)
    {
        return summaryJSON["description"]?.ToString();
    }

    #endregion Implemented Methods

    #endregion Download document

    #region PAYLOAD PROCESSING

    /// <summary>
    /// Payloads the processing response.
    /// </summary>
    /// <param name="payloadProcessingResult">The payload processing result.</param>
    /// <returns>returns JObject</returns>
    public virtual JObject PayloadProcessingResponse(PayloadProcessingResult payloadProcessingResult)
    {
        // Add the payload validation result and add a default hard coded time of 3 min
        // TODO:: This logic of returning the reconciliation await time needs to change such that it is aligned to queue length of payloads
        payloadProcessingResult.ReconciliationAwaitTime = int.Parse(Config[ConfigurationKey.ReconciliationAwaitTime.ToString()]);
        return JObject.FromObject(new { PayloadProcessingResult = payloadProcessingResult });
    }

    /// <summary>
    /// Constructs the dynamic HTML details for email.
    /// </summary>
    /// <param name="responseJObject">The response j object.</param>
    /// <param name="templateList"></param>
    /// <returns>returns a string</returns>
    public virtual string ConstructDynamicHtmlDetailsForEmail(JObject responseJObject,
        IDictionary<string, string> templateList,
        string displayDocumentNumber,
        ref Dictionary<string, string> placeHolderDict,
         List<ApprovalSummaryRow> summaryRows,
         ref EmailType emailType)
    {
        var logData = new Dictionary<LogDataKey, object>
        {
            { LogDataKey.TenantId, approvalTenantInfo.TenantId },
            { LogDataKey.TenantName, approvalTenantInfo.AppName },
            { LogDataKey.DisplayDocumentNumber, displayDocumentNumber },
            { LogDataKey.Xcv, summaryRows?.FirstOrDefault().Xcv},
            { LogDataKey.Tcv, summaryRows?.FirstOrDefault().Tcv},
            { LogDataKey.StartDateTime, DateTime.UtcNow },
            { LogDataKey.EndDateTime, DateTime.UtcNow},
            { LogDataKey.TemplateType, Constants.SummaryTemplate}
        };
        var isMobileFriendlyAdaptiveCard = approvalTenantInfo.NotifyEmailWithMobileFriendlyActionCard switch
        {
            (int)NotifyEmailWithMobileFriendlyActionCard.DisableForAll => false,
            (int)NotifyEmailWithMobileFriendlyActionCard.EnableForFlightedUsers => IsActionableEmailAllowed(summaryRows, FlightingFeatureName.ActionableEmailWithMobileFriendlyAdaptiveCard),
            (int)NotifyEmailWithMobileFriendlyActionCard.EnableForAll => true,
            _ => false,
        };
        bool mobileFriendlyMailGenerationFailure = false;

        if (isMobileFriendlyAdaptiveCard)
        {
            try
            {
                JObject adaptiveCardFinal = GenerateAndMergeSummaryAndDetailAdaptiveCard(
                    responseJObject,
                    displayDocumentNumber,
                    summaryRows?.FirstOrDefault().Approver,
                    summaryRows?.FirstOrDefault().Xcv,
                    summaryRows?.FirstOrDefault().Tcv,
                    templateList,
                    logData).Result;

                PopulatePlaceHolderDict(placeHolderDict, "AdaptiveJSON", adaptiveCardFinal.ToString());

                logData.Modify(LogDataKey.EndDateTime, DateTime.UtcNow);
                Logger.LogInformation(TrackingEvent.MobileFriendlyTemplateGenerationSuccessful, logData);

                return BindDetailTemplateToActionableEmailBody(templateList, responseJObject);
            }
            catch (Exception ex)
            {
                logData.Modify(LogDataKey.EndDateTime, DateTime.UtcNow);
                Logger.LogError(TrackingEvent.MobileFriendlyTemplateGenerationFailure, ex, logData);
                mobileFriendlyMailGenerationFailure = true;
            }
        }

        if (!isMobileFriendlyAdaptiveCard || mobileFriendlyMailGenerationFailure)
        {
            emailType = EmailType.NormalEmail;
        }
        return string.Empty;
    }

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
    public async Task<JObject> GenerateAndMergeSummaryAndDetailAdaptiveCard(
        JObject responseJObject,
        string documentNumber,
        string approver,
        string xcv,
        string tcv,
        IDictionary<string, string> templateList,
        Dictionary<LogDataKey, object> logData)
    {
        #region Generate Summary Adaptive Card

        List<Task<string>> allTasks = new List<Task<string>>();
        Task<string> generateSummaryAdaptiveCard = GenerateSummaryAdaptiveCard(responseJObject, templateList, logData);
        allTasks.Add(generateSummaryAdaptiveCard);

        #endregion Generate Summary Adaptive Card

        if (!CheckIfExistsAndGetCachedAdaptiveCard(documentNumber, out JObject detailsAdaptiveCardObj))
        {
            Task<string> generateDetailsAdaptiveCard = Task.Run(() => GenerateAndAddDetailsAdaptiveCard(responseJObject, templateList, documentNumber, approver, xcv, tcv, logData));
            allTasks.Add(generateDetailsAdaptiveCard);
        }

        await Task.WhenAll(allTasks.ToArray());
        string summaryAdaptiveCardJson = await allTasks[0];
        detailsAdaptiveCardObj = allTasks.Count > 1 ? JObject.Parse(await allTasks[1]) : detailsAdaptiveCardObj;

        #region Merge Summary and Detail Adaptive Card

        JObject adaptiveCardFinal = JObject.Parse(summaryAdaptiveCardJson);
        JArray bodyArr = JArray.Parse(adaptiveCardFinal["body"].ToString());
        JArray detailsBodyArr = JArray.Parse(detailsAdaptiveCardObj["body"].ToString());
        bodyArr.Merge(detailsBodyArr);

        adaptiveCardFinal["body"] = bodyArr;

        if (detailsAdaptiveCardObj.ContainsKey("originator"))
            adaptiveCardFinal["originator"] = detailsAdaptiveCardObj["originator"];

        #endregion Merge Summary and Detail Adaptive Card

        return adaptiveCardFinal;
    }

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
    public string GenerateAndAddDetailsAdaptiveCard(JObject responseJObject, IDictionary<string, string> templateList, string documentNumber, string approver, string xcv, string tcv, Dictionary<LogDataKey, object> logData)
    {
        #region Generate Details Adaptive Card

        logData.Modify(LogDataKey.TemplateType, Constants.DetailsTemplate);

        var template = GetDetailAdaptiveTemplate(templateList);
        JObject adaptiveCard = GenerateAndModifyAdaptiveCard(template, responseJObject, logData);
        var jsonSerializerSetting = new JsonSerializerSettings
        {
            Formatting = Formatting.None,
            NullValueHandling = NullValueHandling.Ignore,
            DefaultValueHandling = DefaultValueHandling.Ignore
        };
        var adaptiveJSON = JsonConvert.SerializeObject(adaptiveCard, jsonSerializerSetting);
        adaptiveJSON = JSONHelper.ReplaceInJsonString(adaptiveJSON, "text", "\"\"", "\" \"");
        adaptiveJSON = JSONHelper.ReplaceInJsonString(adaptiveJSON, "text", "null", "\" \"");

        adaptiveJSON = adaptiveJSON.Replace("#MSApprovalsCoreServiceURL#", Config[ConfigurationKey.ApprovalsCoreServicesURL.ToString()]);
        adaptiveJSON = adaptiveJSON.Replace("#attachmentIcon#", GetIconUrl(templateList, "attachment.png"));
        adaptiveJSON = adaptiveJSON.Replace("#ImageIcon#", GetIconUrl(templateList, "image-icon.png"));
        adaptiveJSON = adaptiveJSON.Replace("#upIcon#", GetIconUrl(templateList, "up.png"));
        adaptiveJSON = adaptiveJSON.Replace("#downIcon#", GetIconUrl(templateList, "down.png"));
        adaptiveJSON = adaptiveJSON.Replace("#receiptIcon#", GetIconUrl(templateList, "receipt.png"));
        adaptiveJSON = adaptiveJSON.Replace("#policyIcon#", GetIconUrl(templateList, "policy.png"));
        adaptiveJSON = adaptiveJSON.Replace("#car-sideIcon#", GetIconUrl(templateList, "car-side.png"));
        adaptiveJSON = adaptiveJSON.Replace("#airplaneIcon#", GetIconUrl(templateList, "airplane.png"));

        #endregion Generate Details Adaptive Card

        return adaptiveJSON;
    }

    /// <summary>
    /// Creates Summary Adaptive Card
    /// </summary>
    /// <param name="responseJObject">details JObject</param>
    /// <param name="templateList">list of templates</param>
    /// <param name="logData">log Data object</param>
    /// <returns>returns the summary adaptive card</returns>
    public async Task<string> GenerateSummaryAdaptiveCard(JObject responseJObject, IDictionary<string, string> templateList, Dictionary<LogDataKey, object> logData)
    {
        var summaryTemplate = GetSummaryAdaptiveTemplate(templateList);

        JArray attachments = !responseJObject.ContainsKey("Attachments") || string.IsNullOrWhiteSpace(responseJObject["Attachments"].ToString()) ? new JArray() : JArray.Parse(responseJObject["Attachments"].ToString());
        responseJObject["Attachments"] = attachments;
        if (responseJObject.ContainsKey("TenantId"))
            responseJObject["TenantId"] = approvalTenantInfo.RowKey;
        else
            responseJObject.Add("TenantId", approvalTenantInfo.RowKey);

        JObject summaryAdaptiveCard = GenerateAndModifyAdaptiveCard(summaryTemplate, responseJObject, logData);
        var jsonSerializerSetting = new JsonSerializerSettings
        {
            Formatting = Formatting.None,
            NullValueHandling = NullValueHandling.Ignore,
            DefaultValueHandling = DefaultValueHandling.Ignore
        };
        string summaryAdaptiveCardJson = JsonConvert.SerializeObject(summaryAdaptiveCard, jsonSerializerSetting);
        summaryAdaptiveCardJson = JSONHelper.ReplaceInJsonString(summaryAdaptiveCardJson, "text", "\"\"", "\" \"");
        summaryAdaptiveCardJson = JSONHelper.ReplaceInJsonString(summaryAdaptiveCardJson, "text", "null", "\" \"");
        summaryAdaptiveCardJson = summaryAdaptiveCardJson.Replace("#ExpenseImg#", GetIconUrl(templateList, "money-icon.png"));
        summaryAdaptiveCardJson = summaryAdaptiveCardJson.Replace("#approveIcon#", GetIconUrl(templateList, "greenTick.png"));
        summaryAdaptiveCardJson = summaryAdaptiveCardJson.Replace("#pendingIcon#", GetIconUrl(templateList, "refresh.png"));
        summaryAdaptiveCardJson = summaryAdaptiveCardJson.Replace("#rejectIcon#", GetIconUrl(templateList, "error.png"));

        return summaryAdaptiveCardJson;
    }

    /// <summary>
    /// Gets the complete adaptive template by Client device
    /// </summary>
    /// <param name="templateList">List of templates</param>
    /// <returns>template</returns>
    public virtual JObject GetAdaptiveTemplate(Dictionary<string, string> templateList)
    {
        JObject adaptiveTemplateFinal = JObject.Parse(GetSummaryAdaptiveTemplate(templateList));
        JObject detailAdaptiveTemplateObj = JObject.Parse(GetDetailAdaptiveTemplate(templateList));

        JArray bodyArr = JArray.Parse(adaptiveTemplateFinal["body"].ToString());
        JArray detailsBodyArr = JArray.Parse(detailAdaptiveTemplateObj["body"].ToString());
        bodyArr.Merge(detailsBodyArr);

        adaptiveTemplateFinal["body"] = bodyArr;
        if (detailAdaptiveTemplateObj.ContainsKey("actions"))
        {
            adaptiveTemplateFinal["actions"] = detailAdaptiveTemplateObj["actions"];
        }

        return adaptiveTemplateFinal;
    }

    /// <summary>
    /// Gets the adaptive summary template by Client device
    /// </summary>
    /// <param name="templateList">List of templates</param>
    /// <returns>template</returns>
    public virtual string GetSummaryAdaptiveTemplate(IDictionary<string, string> templateList)
    {
        bool isSummaryBodyPresent = templateList.TryGetValue(Constants.SUMMARYBODYTEMPLATE + ClientDevice + ".json", out string summaryBodyTemplate);
        if (!isSummaryBodyPresent)
        {
            summaryBodyTemplate = templateList[Constants.SUMMARYBODYTEMPLATE + ".json"];
        }
        return summaryBodyTemplate;
    }

    /// <summary>
    /// Gets the adaptive body and action template by Client device and formulates into a single template
    /// </summary>
    /// <param name="templateList">List of templates</param>
    /// <returns>template</returns>
    public virtual string GetDetailAdaptiveTemplate(IDictionary<string, string> templateList)
    {
        JArray detailsBodyArr;
        JObject detailsAdaptiveObj, attachmentObj, actionObj, footerObj;

        bool isBodyPresent = templateList.TryGetValue(Constants.BODYTEMPLATE + ClientDevice + ".json", out string detailsBodyTemplate);
        bool isAttachmentPresent = templateList.TryGetValue(Constants.ATTACHMENTSTEMPLATE + ClientDevice + ".json", out string attachmentTemplate);
        bool isActionPresent = templateList.TryGetValue(Constants.ACTIONTEMPLATE + ClientDevice + ".json", out string actionTemplate);
        bool isFooterPresent = templateList.TryGetValue(Constants.FOOTERTEMPLATE + ClientDevice + ".json", out string footerTemplate);

        if (!isBodyPresent)
        {
            detailsBodyTemplate = templateList[Constants.BODYTEMPLATE + ".json"];
        }
        detailsAdaptiveObj = JObject.Parse(detailsBodyTemplate);
        detailsBodyArr = JArray.Parse(detailsAdaptiveObj["body"].ToString());

        if (isAttachmentPresent)
        {
            attachmentObj = JObject.Parse(attachmentTemplate);
            detailsBodyArr.AddFirst(attachmentObj);
        }
        if (isActionPresent)
        {
            actionObj = JObject.Parse(actionTemplate);
            if (actionObj.ContainsKey("actions"))
            {
                detailsAdaptiveObj.Add("actions", actionObj["actions"]);
            }
            else
            {
                detailsBodyArr.Add(actionObj);
            }
        }
        if (isFooterPresent)
        {
            footerObj = JObject.Parse(footerTemplate);
            detailsBodyArr.Add(footerObj);
        }

        detailsAdaptiveObj["body"] = detailsBodyArr;
        detailsBodyTemplate = detailsAdaptiveObj.ToString();

        return detailsBodyTemplate;
    }

    /// <summary>
    /// Gets the adaptive card json from the ApprovalDetails table
    /// </summary>
    /// <param name="documentNumber">Document Number</param>
    /// <param name="adaptiveCard">out parameter which has adaptive card value</param>
    /// <returns></returns>
    public bool CheckIfExistsAndGetCachedAdaptiveCard(string documentNumber, out JObject adaptiveCard)
    {
        adaptiveCard = new JObject();
        ApprovalDetailsEntity detailsData = ApprovalDetailProvider.GetApprovalDetailsByOperation(approvalTenantInfo.TenantId, documentNumber, Constants.AdaptiveDTL + ClientDevice).Result;
        if (detailsData != null)
        {
            if (!string.IsNullOrEmpty(detailsData.Version) && detailsData.Version.Equals(approvalTenantInfo.AdaptiveCardVersion))
            {
                adaptiveCard = JObject.Parse(detailsData.JSONData);
                return true;
            }
            else
            {
                ApprovalDetailProvider.RemoveApprovalsDetails(new List<ApprovalDetailsEntity>() { detailsData });
                return false;
            }
        }
        return false;
    }

    /// <summary>
    /// Generates adaptive card using template and data
    /// Modifies the adaptive card
    /// </summary>
    /// <param name="template">adaptive card template</param>
    /// <param name="responseJObject">details data Object</param>
    /// <param name="logData">log data</param>
    /// <returns>Adaprive Card</returns>
    public virtual JObject GenerateAndModifyAdaptiveCard(string template, JObject responseJObject, Dictionary<LogDataKey, object> logData)
    {
        var logDataNew = new Dictionary<LogDataKey, object>();
        foreach (var keyValuePair in logData)
        {
            logDataNew.Add(keyValuePair.Key, keyValuePair.Value);
        }
        logDataNew.Modify(LogDataKey.StartDateTime, DateTime.UtcNow);

        string adaptiveCardString = MSAHelper.CreateCard(template, responseJObject.ToString());
        JObject adaptiveCard = adaptiveCardString.FromJson<JObject>();

        logDataNew.Modify(LogDataKey.EndDateTime, DateTime.UtcNow);
        Logger.LogInformation(TrackingEvent.AdaptiveCardTemplatingSuccessful, logDataNew);
        return adaptiveCard;
    }

    /// <summary>
    /// Binds the simple data.
    /// </summary>
    /// <param name="template">The template.</param>
    /// <param name="reportData">The report data.</param>
    /// <returns>returns email template</returns>
    public virtual string BindSimpleData(string template,
        JToken reportData,
        bool isPlaceHolderRemoved = false)
    {
        string pattern = @"{{(\S+)}}";
        Regex r = new Regex(pattern);
        MatchCollection m = r.Matches(template);

        foreach (Match match in m)
        {
            string search = match.Groups[0].Value;
            string property = match.Groups[1].Value;
            string value = "";

            var selectToken = reportData.SelectToken(property);
            if (selectToken == null)
            {
                if (isPlaceHolderRemoved)
                {
                    template = template.Replace(search, "");
                }
                continue;
            }

            if (selectToken.Type == JTokenType.Array || selectToken.Type == JTokenType.Object)
            {
                continue;
            }
            else if (selectToken.Type == JTokenType.Boolean)
            {
                value = selectToken.Value<bool>() ? "Yes" : "No";
            }
            else
            {
                value = selectToken.Value<string>();
            }

            template = template.Replace(search, value);
        }

        return template;
    }

    /// <summary>
    /// Bind the messages
    /// </summary>
    /// <param name="template"></param>
    /// <param name="reportData"></param>
    /// <returns></returns>
    public virtual string BindMessages(string template,
       JToken reportData)
    {
        List<string> messagesString = new List<string>();

        JArray messageItems = new JArray();
        JArray messagColumns = new JArray();
        JObject adaptiveMessages = new JObject();

        var messages = reportData.SelectToken("Messages");
        if (messages != null)
        {
            foreach (JObject message in messages as JArray)
            {
                messagesString.Add(string.Format("<span>{0}</span>", message["Text"]));
                AdaptiveCardHelper.GetItems("TextBlock", string.Format("{0}", message["Text"]), ref messageItems);
            }

            AdaptiveCardHelper.GetColumns(messageItems, ref messagColumns, width: "stretch");
            adaptiveMessages = AdaptiveCardHelper.GetColumnSets(messagColumns);
        }

        template = template.Replace("{{Messages}}", string.Join("<br>", messagesString));
        return template.Replace("#Messages#", adaptiveMessages?.ToString());
    }

    /// <summary>
    /// Binds the approver.
    /// </summary>
    /// <param name="template">The template.</param>
    /// <param name="reportData">The report data.</param>
    /// <returns>returns email template</returns>
    public virtual string BindApprover(string template,
        JToken reportData)
    {
        List<string> approversString = new List<string>();
        var approvers = reportData.SelectToken("Approvers") as JArray;
        foreach (JObject approver in approvers)
        {
            approversString.Add(string.Format("<span>{0}[{1}][{2}]</span>", approver["Name"], approver["Alias"], approver["Type"]));
        }

        string approverChain = string.Empty;

        var adaptiveApproverChain = reportData.SelectToken("AdaptiveApproverChain");

        if (adaptiveApproverChain != null)
        {
            approverChain = adaptiveApproverChain.Value<string>();
        }

        template = template.Replace("#AdaptiveApproverChain#", approverChain);

        return template.Replace("{{Approvers}}", string.Join("<br>", approversString));
    }

    /// <summary>
    /// Binds the attachments.
    /// </summary>
    /// <param name="template">The template.</param>
    /// <param name="reportData">The report data.</param>
    /// <param name="templateList">List of templates</param>
    /// <returns>returns email template</returns>
    public virtual string BindAttachments(string template,
        JToken reportData,
        IDictionary<string, string> templateList)
    {
        List<string> attachmentList = new List<string>();

        JArray attachmentItems = new JArray();
        JArray attachmentColumns = new JArray();
        JObject adaptiveMessages = new JObject();

        if ((reportData.SelectToken("Attachments") as JArray) != null && (reportData.SelectToken("Attachments") as JArray).Count > 0 && (reportData.SelectToken("Attachments") as JArray)[0]["Url"] != null && !string.IsNullOrEmpty((reportData.SelectToken("Attachments") as JArray)[0]["Url"].ToString()))
        {
            if (template.Contains("{{AttachmentInfo}}"))
            {
                string attachmentTemplate = templateList[Constants.ATTACHMENTTEMPLATE];
                attachmentTemplate = BindSimpleData(attachmentTemplate, reportData, true);
                template = template.Replace("{{AttachmentInfo}}", string.Join("\r\n", attachmentTemplate));
            }

            foreach (JObject attachment in reportData.SelectToken("Attachments") as JArray)
            {
                //TODO :: Add check if Url is not NullOrEmpty
                attachmentList.Add(string.Format("<a href='{0}'>{1}</a>", attachment["Url"], attachment["Name"]));
                AdaptiveCardHelper.GetItems("TextBlock", string.Format("[{0}]({1})", attachment["Name"], attachment["Url"]), ref attachmentItems);
            }

            AdaptiveCardHelper.GetColumns(attachmentItems, ref attachmentColumns, width: "stretch");
            adaptiveMessages = AdaptiveCardHelper.GetColumnSets(attachmentColumns);
        }
        else
        {
            template = template.Replace("Attachments:", string.Empty);
            template = template.Replace("{{AttachmentInfo}}", string.Empty);
        }

        template = template.Replace("{{Attachments}}", string.Join("<br>", attachmentList));
        return template.Replace("#Attachments#", adaptiveMessages?.ToString());
    }

    /// <summary>
    /// Bind Notes
    /// </summary>
    /// <param name="template">The template.</param>
    /// <param name="reportData">The report data.</param>
    /// <returns>Returns Email Template</returns>
    public virtual string BindNotes(string template, JToken reportData)
    {
        return template;
    }

    /// <summary>
    /// Bind the custom attribute
    /// </summary>
    /// <param name="template">The html template</param>
    /// <param name="reportData">Details Response Data</param>
    /// <returns>returns email template</returns>
    public virtual string BindCustomAttribute(string template,
       JToken reportData)
    {
        string customAttributeValue = string.Empty;

        var customAttribute = reportData.SelectToken("CustomAttribute");
        if (customAttribute != null && customAttribute.HasValues)
        {
            customAttributeValue = customAttribute["CustomAttributeValue"]?.ToString();
        }

        return template.Replace("{{CustomAttribute.CustomAttributeValue}}", customAttributeValue);
    }

    /// <summary>
    /// Bind Line items
    /// </summary>
    /// <param name="template">email template</param>
    /// <param name="lineItems">line items oject</param>
    /// <param name="templateWidth">width of email template</param>
    /// <param name="templateList">template list</param>
    /// <returns>email template with line items incorporated in it</returns>
    public virtual string BindLineItems(string template,
        JToken lineItems,
        int templateWidth,
        IDictionary<string, string> templateList)
    {
        return template;
    }

    /// <summary>
    /// Bind Detail template to Email HTMl Body
    /// </summary>
    /// <param name="templateList">email templates</param>
    /// <param name="reportData">report Data</param>
    /// <returns>string</returns>
    public virtual string BindDetailTemplateToActionableEmailBody(IDictionary<string, string> templateList, JObject reportData)
    {
        return string.Empty;
    }

    #endregion PAYLOAD PROCESSING

    #region Access Token Helper Methods

    /// <summary>
    /// Gets Approval Identifier object either from ApprovalSummary table or TransactionHistory table.
    /// </summary>
    /// <param name="documentNumber">Document Number.</param>
    /// <param name="xcv">Xcv.</param>
    /// <param name="tcv">Tcv.</param>
    /// <returns><see cref="ApprovalIdentifier"/> object.</returns>
    private async Task<ApprovalIdentifier> GetApprovalIdentifier(string documentNumber, string xcv = "", string tcv = "")
    {
        ApprovalIdentifier approvalIdentifier = null;

        // If ApprovalSummary record is available, get the ApprovalIdentifier from SummaryJson.
        // Else if TransactionHistory record is available, get the ApprovalIdentifier from JsonData.
        var summary = ApprovalSummaryProvider.GetApprovalSummaryByDocumentNumberAndApprover(approvalTenantInfo.DocTypeId, documentNumber, Alias);

        if (summary != null)
        {
            var approvalSummary = summary?.SummaryJson?.FromJson<SummaryJson>();
            approvalIdentifier = approvalSummary?.ApprovalIdentifier;
        }
        else
        {
            var historyData = await ApprovalHistoryProvider.GetHistoryDataAsync(approvalTenantInfo, documentNumber, Alias, xcv, tcv);

            if (historyData != null && historyData.Count > 0)
            {
                var historyRow = historyData.OrderByDescending(x => x.ActionDate).FirstOrDefault();
                var summaryFromHistory = historyRow?.JsonData?.FromJson<SummaryJson>();
                approvalIdentifier = summaryFromHistory?.ApprovalIdentifier;
            }
        }

        return approvalIdentifier;
    }

    /// <summary>
    /// Generates and adds the Access token to the HttpRequest Authorization Header based on the Authentication model selected for the tenant
    /// Also adds any additional items in the HttpRequest Header if that is specified in the ServiceParameter of ApprovalTenantInfo
    /// </summary>
    /// <param name="requestMessage"></param>
    /// <param name="serviceRoot"></param>
    /// <param name="serviceParameterObject">Service Parameter</param>
    private async Task GetAndAttachTokenBasedOnAuthType(HttpRequestMessage requestMessage, string serviceRoot, JObject serviceParameterObject = null)
    {
        string accessToken;
        if (serviceParameterObject == null)
        {
            serviceParameterObject = approvalTenantInfo.ServiceParameter.ToJObject();
            SetServiceParameter(serviceParameterObject);
        }

        AuthenticationModelType authType = (AuthenticationModelType)Enum.Parse(typeof(AuthenticationModelType), Convert.ToString(serviceParameterObject[Constants.AuthenticationType]));

        var logData = new Dictionary<LogDataKey, object>
        {
            { LogDataKey.BusinessProcessName, string.Format(approvalTenantInfo.BusinessProcessName, Constants.BusinessProcessNameGetSummary, Constants.BusinessProcessNameSumamryFromBackChannel) },
            { LogDataKey.ClientDevice, ClientDevice },
            { LogDataKey.TenantId, approvalTenantInfo.TenantId },
            { LogDataKey.UserAlias, Alias },
            { LogDataKey.Uri, requestMessage.RequestUri}
        };

        switch (authType)
        {
            case AuthenticationModelType.SAS:
                // There is no need for an ACS token to be passed like in past, this is because this part of security where the tenant team intercepted this ACS based SWT token is being replaced with a policy on tenant side,
                // which in turn in enforced by Approvals on tenants, whereby a binding setting "RelayAuthenticationType" is set to "AccessToken".
                // If "RelayAuthenticationType" is set to NONE, it would result in a P1 bug on Tenant Team.
                accessToken = AuthenticationHelper.GetSASToken(serviceParameterObject, serviceRoot);
                requestMessage.Headers.Add(Constants.AuthorizationHeader, accessToken);
                break;

            case AuthenticationModelType.OAuth2:
                // accessToken variable contains the App token generated using the ClientId and Client Secret which is passed in the AuthorizationHeader

                // There is no need for a user token be passed, like the ACS token in past, because AAD based authentication is only based on app tokens for now.
                // If user tokens are supported in future, they should be added here.
                accessToken = (await AuthenticationHelper.AcquireOAuth2TokenByScopeAsync(
                                                serviceParameterObject[Constants.ClientID].ToString(),
                                                serviceParameterObject[Constants.AuthKey].ToString(),
                                                serviceParameterObject[Constants.Authority].ToString(),
                                                serviceParameterObject[Constants.ResourceURL].ToString(),
                                                "/.default")).AccessToken;
                requestMessage.Headers.Authorization = new AuthenticationHeaderValue(Constants.AuthorizationHeaderScheme, accessToken);
                break;

            case AuthenticationModelType.CorpSTS:
                accessToken = AuthenticationHelper.GetAcsSimpleWebTokenFromSharedSecret(false);
                string accessToken2;
                try
                {
                    accessToken2 = AuthenticationHelper.GetAcsSimpleWebTokenFromSharedSecret(true);
                }
                catch
                {
                    accessToken2 = accessToken;
                }

                requestMessage.Headers.Add(Constants.AuthorizationHeader, accessToken);
                requestMessage.Headers.Add("AuthorizationToken", accessToken2);
                break;

            case AuthenticationModelType.OAuth2OnBehalf:
                // get the On behalf AAD Token containing the User Claims and specific scope
                accessToken = await AuthenticationHelper.GetOnBehalfBearerToken((ClaimsIdentity)ClaimsPrincipal.Current?.Identity, serviceParameterObject);
                requestMessage.Headers.Authorization = new AuthenticationHeaderValue(Constants.AuthorizationHeaderScheme, accessToken);
                break;

            case AuthenticationModelType.UserOnBehalf:
                try
                {
                    accessToken = await AuthenticationHelper.GetOnBehalfUserToken(UserToken.Replace("Bearer ", string.Empty).Replace("bearer ", string.Empty),
                        serviceParameterObject);
                    logData.Add(LogDataKey.IdentityProviderTokenType, "OnBehalfUserToken");
                }
                catch (Exception ex)
                {
                    Logger.LogError(TrackingEvent.OAuth2TokenGenerationError, ex, logData);
                    accessToken = (await AuthenticationHelper.AcquireOAuth2TokenByScopeAsync(
                                    serviceParameterObject[Constants.ClientID].ToString(),
                                    serviceParameterObject[Constants.AuthKey].ToString(),
                                    serviceParameterObject[Constants.Authority].ToString(),
                                    serviceParameterObject[Constants.ResourceURL].ToString(),
                                    "/.default")).AccessToken;
                    logData.Add(LogDataKey.IdentityProviderTokenType, "AADAppTokenFallback");
                }

                requestMessage.Headers.Authorization = new AuthenticationHeaderValue(Constants.AuthorizationHeaderScheme, accessToken);
                break;

            case AuthenticationModelType.ManagedIdentityToken:
                accessToken = await AuthenticationHelper.GetManagedIdentityToken(serviceParameterObject[Constants.Scope].ToString());
                requestMessage.Headers.Authorization = new AuthenticationHeaderValue(Constants.AuthorizationHeaderScheme, accessToken);
                break;
        }

        Logger.LogInformation(TrackingEvent.OAuth2TokenGenerationSuccessful, logData);
        // Add additional items to Request Headers
        if (serviceParameterObject[Constants.AdditionalData] != null)
        {
            var dictAdditionalItems = Convert.ToString(serviceParameterObject[Constants.AdditionalData]).FromJson<Dictionary<string, string>>();
            foreach (var additionalItem in dictAdditionalItems)
            {
                requestMessage.Headers.Add(additionalItem.Key, additionalItem.Value);
            }
        }
    }

    #endregion Access Token Helper Methods

    #region Request headers Xcv, Tcv Mappings

    /// <summary>
    /// add Xcv, Tcv to request headers
    /// </summary>
    /// <param name="requestMessage">httprequest message</param>
    /// <param name="Xcv">Xcv</param>
    /// <param name="Tcv">Tcv</param>
    protected void AddXcvTcvToRequestHeaders(ref HttpRequestMessage requestMessage, string Xcv, string Tcv)
    {
        string xcvMappingKey = GetXcvOrTcvMappingKeyOfTenant(Constants.XcvMappingKey);
        string tcvMappingKey = GetXcvOrTcvMappingKeyOfTenant(Constants.TcvMappingKey);
        if (!string.IsNullOrEmpty(xcvMappingKey) && !string.IsNullOrEmpty(tcvMappingKey)
            && !string.IsNullOrEmpty(Xcv) && !string.IsNullOrEmpty(Tcv))
        {
            requestMessage.Headers.Add(xcvMappingKey, Xcv);
            requestMessage.Headers.Add(tcvMappingKey, Tcv);
        }
        else
        {
            //sending Xcv, Tcv values in headers by default for other tenants who has no mapping in serviceparameter of approvaltenantinfo.
            requestMessage.Headers.Add(Constants.Xcv, Xcv);
            requestMessage.Headers.Add(Constants.Tcv, Tcv);
        }
    }

    /// <summary>
    /// Get Mapping key of Xcv/Tcv for a tenant
    /// </summary>
    /// <param name="key"></param>
    /// <returns>Xcv/Tcv mapping key of tenant</returns>
    private string GetXcvOrTcvMappingKeyOfTenant(string key)
    {
        JObject serviceParameterObject = approvalTenantInfo.ServiceParameter.ToJObject();
        if (serviceParameterObject != null && serviceParameterObject[key] != null)
        {
            return serviceParameterObject[key].ToString();
        }
        return string.Empty;
    }

    #endregion Request headers Xcv, Tcv Mappings

    #region Get Approver List for Sending Notifications

    /// <summary>
    /// Gets the email aliases of approvers for whom the notification should be sent.
    /// </summary>
    /// <param name="summaryRow">The summary row</param>
    /// <param name="notificationDetails">Notification details object</param>
    /// <returns>An email alias(es) to whom the notification should be sent</returns>
    public virtual string GetApproverListForSendingNotifications(ApprovalSummaryRow summaryRow, NotificationDetail notificationDetails)
    {
        return string.IsNullOrEmpty(notificationDetails.To) ? summaryRow.Approver + Config[ConfigurationKey.DomainName.ToString()] : notificationDetails.To;
    }

    #endregion Get Approver List for Sending Notifications

    #region Adaptive Card Methods

    /// <summary>
    /// Reformat value of Jtoken based on datatype and format
    /// </summary>
    /// <typeparam name="T">DataType e.g DateTime,double etc</typeparam>
    /// <param name="jToken">jToken</param>
    /// <param name="tokenName">tokenName</param>
    /// <param name="format">format which support string format</param>
    public virtual void ApplyFormatting<T>(JToken jToken, string tokenName, string format)
    {
        var token = jToken.SelectToken(tokenName);

        if (token != null)
        {
            try
            {
                jToken[tokenName] = String.Format(CultureInfo.CreateSpecificCulture(Constants.CultureName), format, token.Value<T>());
            }
            catch (Exception)
            {
                // We can not throw an exception if parse error occure while formatting to block actionable email notification. because it affect only UI
            }
        }
    }

    /// <summary>
    /// Apply format in jToken data
    /// </summary>
    /// <param name="jToken">jToken data</param>
    public virtual void ApplyFormatInJsonData(JToken jToken)
    {
        ApplyFormatting<double>(jToken, "UnitValue", "{0:n2}");
        ApplyFormatting<DateTime>(jToken, "SubmittedDate", "{0:MM/dd/yy}");
    }

    /// <summary>
    /// Fetch Icon url by from list name
    /// </summary>
    /// <param name="templateList"></param>
    /// <param name="iconName"></param>
    /// <returns></returns>
    public string GetIconUrl(IDictionary<string, string> templateList, string iconName)
    {
        if (string.IsNullOrEmpty(iconName))
        {
            return string.Empty;
        }

        if (templateList.ContainsKey(iconName))
        {
            return templateList[iconName];
        }
        else
        {
            return string.Empty;
        }
    }

    public static void PopulatePlaceHolderDict(Dictionary<string, string> placeHolderDict, string key, string value)
    {
        if (placeHolderDict != null)
        {
            if (!placeHolderDict.ContainsKey(key))
            {
                placeHolderDict.Add(key, value);
            }
            else
            {
                placeHolderDict[key] = value;
            }
        }
    }

    #endregion Adaptive Card Methods

    #region PullTenant Methods

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
    public async Task<HttpResponseMessage> GetUsersDelegatedToAsync(string alias, Dictionary<string, object> parameters, string clientDevice, string xcv, string tcv, string sessionId)
    {
        // Prepare Log data.
        var logData = new Dictionary<LogDataKey, object>
        {
            { LogDataKey.Xcv, xcv },
            { LogDataKey.Tcv, tcv },
            { LogDataKey.SessionId, sessionId },
            { LogDataKey.StartDateTime, DateTime.UtcNow },
            { LogDataKey.IsCriticalEvent, CriticalityLevel.Yes.ToString() },
            { LogDataKey.UserRoleName, alias },
            { LogDataKey.TenantId, approvalTenantInfo.TenantId },
            { LogDataKey.TenantName, approvalTenantInfo.AppName },
            { LogDataKey.DocumentTypeId, approvalTenantInfo.DocTypeId }
        };

        try
        {
            var url = RetrieveTenantOperationUrl(Constants.DelegateOperationType, parameters, clientDevice);
            var response = await SendRequestAsync(await CreateRequestForDetailsOrAction(HttpMethod.Get, url, xcv, tcv, Constants.DelegateOperationType), logData);

            // Log success event.
            logData.Add(LogDataKey.EndDateTime, DateTime.UtcNow);
            Logger.LogInformation(TrackingEvent.ImpersonationGetSuccess, logData);

            return response;
        }
        catch (Exception ex)
        {
            // Log failure event.
            logData.Add(LogDataKey.EndDateTime, DateTime.UtcNow);
            Logger.LogError(TrackingEvent.ImpersonationGetFailed, ex, logData);
            throw;
        }
    }

    /// <summary>
    /// Gets the tenant action details asynchronously.
    /// </summary>
    /// <param name="alias">The alias.</param>
    /// <param name="loggedInAlias">The loggedInAlias.</param>
    /// <param name="clientDevice">The clientDevice.</param>
    /// <param name="sessionId">The sessionId.</param>
    /// <param name="xcv">The Xcv.</param>
    /// <param name="tcv">The Tcv.</param>
    /// <returns>Returns tenant action details</returns>
    public async Task<HttpResponseMessage> GetTenantActionDetails(string alias, string loggedInAlias, string clientDevice, string sessionId, string xcv, string tcv)
    {
        // Prepare Log data.
        var logData = new Dictionary<LogDataKey, object>
        {
            { LogDataKey.StartDateTime, DateTime.UtcNow },
            { LogDataKey.Xcv, xcv },
            { LogDataKey.Tcv, tcv },
            { LogDataKey.SessionId, sessionId },
            { LogDataKey.IsCriticalEvent, CriticalityLevel.Yes.ToString() },
            { LogDataKey.UserAlias, alias },
            { LogDataKey.UserRoleName, loggedInAlias },
            { LogDataKey.Operation, OperationType.TenantActionDetail },
            { LogDataKey.TenantId, approvalTenantInfo.TenantId },
            { LogDataKey.TenantName, approvalTenantInfo.AppName },
            { LogDataKey.AppAction, Constants.BusinessProcessNameGetActionDetailsFromTenant },
            { LogDataKey.EventType, Constants.BusinessProcessEvent },
            { LogDataKey.BusinessProcessName, string.Format(approvalTenantInfo.BusinessProcessName, Constants.BusinessProcessNameGetActionDetailsFromTenant, Constants.BusinessProcessNameUserTriggered) },
            { LogDataKey.DocumentTypeId, approvalTenantInfo.DocTypeId }
        };

        try
        {
            Dictionary<string, object> parameters = new Dictionary<string, object>
            {
                { "alias", loggedInAlias }
            };
            var url = RetrieveTenantOperationUrl(Constants.TenantActionDetails, parameters, clientDevice);
            var response = await SendRequestAsync(await CreateRequestForDetailsOrAction(HttpMethod.Get, url, xcv, tcv, Constants.TenantActionDetails), logData);

            // Log success event.
            logData.Add(LogDataKey.EndDateTime, DateTime.UtcNow);
            Logger.LogInformation(TrackingEvent.GetTenantActionDetailsSuccess, logData);

            return response;
        }
        catch (Exception ex)
        {
            // Log failure event.
            logData.Add(LogDataKey.EndDateTime, DateTime.UtcNow);
            Logger.LogError(TrackingEvent.GetTenantActionDetailsFailed, ex, logData);
            throw;
        }
    }

    /// <summary>
    /// Retrieve Summary Endpoint URL for tenant.
    /// </summary>
    /// <param name="parameters">Key-value pair for filtering parameters.</param>
    /// <returns>Summary Endpoint URL.</returns>
    private string RetrieveTenantSummaryUrl(Dictionary<string, object> parameters)
    {
        string tenantSummaryUrl = approvalTenantInfo.TenantBaseUrl + approvalTenantInfo.SummaryURL;
        return JSONHelper.ReplacePlaceholder(tenantSummaryUrl, parameters, Constants.PullModelURLPlaceHolderStart, Constants.PullModelURLPlaceHolderEnd);
    }

    /// <summary>
    /// Retrieve Details Endpoint URL for tenant.
    /// </summary>
    /// <param name="operationType">Type of operation which needs to be executed from tenant info configuration.</param>
    /// <param name="parameters">Key-value pair for filtering parameters.</param>
    /// <param name="clientDevice">The clientDevice.</param>
    /// <returns>Details Endpoint URL.</returns>
    public string RetrieveTenantDetailsUrl(string operationType, Dictionary<string, object> parameters, string clientDevice)
    {
        string tenantDetailsUrl = Extension.GetEndPointURL(approvalTenantInfo, operationType, clientDevice);
        return string.IsNullOrWhiteSpace(tenantDetailsUrl) ? null : JSONHelper.ReplacePlaceholder(tenantDetailsUrl, parameters, Constants.PullModelURLPlaceHolderStart, Constants.PullModelURLPlaceHolderEnd);
    }

    /// <summary>
    /// Retrieve Operation Endpoint URL for tenant.
    /// </summary>
    /// <param name="operationType">The operationType.</param>
    /// <param name="parameters">The parameters.</param>
    /// <returns>Operation endpoint URL</returns>
    private string RetrieveTenantOperationUrl(string operationType, IDictionary<string, object> parameters, string clientDevice)
    {
        string tenantOperationUrl = Extension.GetEndPointURL(approvalTenantInfo, operationType, clientDevice);
        if (string.IsNullOrWhiteSpace(tenantOperationUrl))
        {
            throw new InvalidOperationException($"OperationType {operationType} is not configured for Tenant {approvalTenantInfo.AppName}.");
        }

        return JSONHelper.ReplacePlaceholder(tenantOperationUrl, parameters, Constants.PullModelURLPlaceHolderStart, Constants.PullModelURLPlaceHolderEnd);
    }

    /// <summary>
    /// Creates the request for notification.
    /// </summary>
    /// <param name="method">The method.</param>
    /// <param name="uri">The URI.</param>
    /// <param name="Xcv">Xcv</param>
    /// <param name="Tcv">Tcv</param>
    /// <returns>returns Http Request Message</returns>
    public async Task<HttpRequestMessage> CreateRequestForNotification(HttpMethod method, string uri, string Xcv = "", string Tcv = "")
    {
        HttpRequestMessage reqMessage = new HttpRequestMessage(method, uri);
        JObject serviceParameter = new JObject
        {
            { "Authority", Config[ConfigurationKey.Authority.ToString()] },
            { "ClientID", Config[ConfigurationKey.NotificationFrameworkClientId.ToString()] },
            { "AuthKey", Config[ConfigurationKey.NotificationFrameworkAuthKey.ToString()] }
        };
        var resourceUrl = Config[ConfigurationKey.NotificationFrameworkResourceUrl.ToString()];
        serviceParameter.Add("ResourceURL", resourceUrl);
        serviceParameter.Add("AuthenticationType", 1);
        await GetAndAttachTokenBasedOnAuthType(reqMessage, uri, serviceParameter);
        AddXcvTcvToRequestHeaders(ref reqMessage, Xcv, Tcv);
        reqMessage.Headers.Add(Constants.ActionByAlias, Alias);
        return reqMessage;
    }

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
    public async Task<HttpResponseMessage> GetTenantSummaryAsync(Dictionary<string, Object> parameters, string approverAlias, string loggedInAlias, string xcv, string tcv, string sessionId)
    {
        // Prepare log data.
        var logData = new Dictionary<LogDataKey, object>
        {
            { LogDataKey.Xcv, xcv },
            { LogDataKey.Tcv, tcv },
            { LogDataKey.SessionId, sessionId },
            { LogDataKey.IsCriticalEvent, CriticalityLevel.Yes.ToString() },
            { LogDataKey.UserAlias, approverAlias },
            { LogDataKey.UserRoleName, loggedInAlias },
            { LogDataKey.Operation, OperationType.Summary },
            { LogDataKey.AppAction, Constants.BusinessProcessNameGetSummaryFromTenant },
            { LogDataKey.TenantId, approvalTenantInfo.TenantId },
            { LogDataKey.TenantName, approvalTenantInfo.AppName },
            { LogDataKey.EventType, Constants.BusinessProcessEvent },
            { LogDataKey.BusinessProcessName, string.Format(approvalTenantInfo.BusinessProcessName, Constants.BusinessProcessNameGetSummaryFromTenant, Constants.BusinessProcessNameUserTriggered) },
            { LogDataKey.DocumentTypeId, approvalTenantInfo.DocTypeId }
        };

        try
        {
            logData.Add(LogDataKey.StartDateTime, DateTime.UtcNow);
            // Get the Summary Endpoint URL.
            var url = RetrieveTenantSummaryUrl(parameters);

            // Fetch the Summary from tenant system.
            var response = await SendRequestAsync(await CreateRequestForDetailsOrAction(HttpMethod.Get, url, xcv, tcv, Constants.SummaryOperationType), logData);

            // Log success event.
            logData.Add(LogDataKey.EndDateTime, DateTime.UtcNow);
            Logger.LogInformation(TrackingEvent.PullTenantGetSummarySuccess, logData);

            return response;
        }
        catch (Exception ex)
        {
            // Log failure event.
            logData.Add(LogDataKey.EndDateTime, DateTime.UtcNow);
            Logger.LogError(TrackingEvent.PullTenantGetSummaryFailed, ex, logData);
            throw;
        }
    }

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
    public async Task<HttpResponseMessage> GetTenantDetailsAsync(string operationType, Dictionary<string, object> parameters, string approverAlias, string loggedInAlias, string clientDevice, string xcv, string tcv, string sessionId)
    {
        // Prepare Log data.
        var logData = new Dictionary<LogDataKey, object>
        {
            { LogDataKey.StartDateTime, DateTime.UtcNow },
            { LogDataKey.Xcv, xcv },
            { LogDataKey.Tcv, tcv },
            { LogDataKey.SessionId, sessionId },
            { LogDataKey.IsCriticalEvent, CriticalityLevel.Yes.ToString() },
            { LogDataKey.UserAlias, approverAlias },
            { LogDataKey.UserRoleName, loggedInAlias },
            { LogDataKey.Operation, operationType },
            { LogDataKey.TenantId, approvalTenantInfo.TenantId },
            { LogDataKey.TenantName, approvalTenantInfo.AppName },
            { LogDataKey.AppAction, Constants.BusinessProcessNameGetDetailsFromTenant },
            { LogDataKey.EventType, Constants.BusinessProcessEvent },
            { LogDataKey.BusinessProcessName, string.Format(approvalTenantInfo.BusinessProcessName, Constants.BusinessProcessNameGetDetailsFromTenant, Constants.BusinessProcessNameUserTriggered) },
            { LogDataKey.DocumentTypeId, approvalTenantInfo.DocTypeId }
        };

        try
        {
            var url = RetrieveTenantDetailsUrl(operationType, parameters, clientDevice);
            if (string.IsNullOrWhiteSpace(url))
            {
                return null;
            }

            var response = await SendRequestAsync(await CreateRequestForDetailsOrAction(HttpMethod.Get, url, xcv, tcv, operationType), logData);

            // Log success event.
            logData.Add(LogDataKey.EndDateTime, DateTime.UtcNow);
            Logger.LogInformation(TrackingEvent.PullTenantGetDetailsSuccess, logData);

            return response;
        }
        catch (Exception ex)
        {
            // Log failure event.
            logData.Add(LogDataKey.EndDateTime, DateTime.UtcNow);
            Logger.LogError(TrackingEvent.PullTenantGetDetailsFailed, ex, logData);
            throw;
        }
    }

    #endregion PullTenant Methods
}