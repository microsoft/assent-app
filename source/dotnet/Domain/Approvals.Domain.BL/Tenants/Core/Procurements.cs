// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace Microsoft.CFS.Approvals.Domain.BL.Tenants.Core;

using System;
using System.Collections.Generic;
using System.Linq;
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
using Microsoft.CFS.Approvals.Utilities.Helpers;
using Microsoft.CFS.Approvals.Utilities.Interface;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json.Linq;

/// <summary>
/// Class Procurement
/// </summary>
/// <seealso cref="TenantBase" />
public class Procurement : TenantBase
{
    #region CONSTRUCTOR

    public Procurement(
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

    public Procurement(
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

    #region GET SUMMARY

    /// <summary>
    /// This method will be used to return SummaryOperationTypes
    /// </summary>
    /// <returns>List of string</returns>
    public override List<string> GetSummaryOperationTypes()
    {
        return new List<string>() { Constants.SummaryOperationType, Constants.CurrentApprover, Constants.ApprovalChainOperation, Constants.ProcurementDetailAction, Constants.HeaderOperationType, Constants.OcrOperationType };
    }

    #endregion GET SUMMARY

    #region GET DETAIL

    protected override async Task<string> GetDetailURL(string urlFormat, ApprovalIdentifier approvalIdentifier, int page, string xcv = "", string tcv = "", string businessProcessName = "", string docTypeId = "")
    {
        return IsTelemetryMappingNameDefined == true ? String.Format(urlFormat, approvalIdentifier.GetDocNumber(approvalTenantInfo)) : String.Format(urlFormat, approvalIdentifier.GetDocNumber(approvalTenantInfo), xcv, tcv);
    }

    protected override string GetAttachmentDownloadUrl(string urlFormat, string attachmentId, ApprovalIdentifier approvalIdentifier)
    {
        return String.Format(urlFormat, HttpUtility.UrlEncode(attachmentId));
    }

    protected override async Task<HttpResponseMessage> ExecutePostDetailOperationAsync(HttpResponseMessage lobResponse, string operation, ApprovalIdentifier approvalIdentifier)
    {
        lobResponse = await base.ExecutePostDetailOperationAsync(lobResponse, operation, approvalIdentifier);
        string jsonDetail = await lobResponse.Content.ReadAsStringAsync();
        jsonDetail = !operation.Equals(Constants.AdditionalDetails, StringComparison.InvariantCultureIgnoreCase) ? AddMessages(jsonDetail, operation) : jsonDetail;

        return new HttpResponseMessage() { Content = new StringContent(jsonDetail), StatusCode = lobResponse.StatusCode };
    }

    protected virtual string AddMessages(string jsonDetail, string operation)
    {
        JObject jObject = (jsonDetail).ToJObject();
        JArray jArray = new JArray();
        if (operation.Equals("DTL", StringComparison.InvariantCultureIgnoreCase))
        {
            if (jObject["Submitter"] != null && jObject["ProjectOwner"] != null)
            {
                JToken submitterToken = jObject["Submitter"];
                JToken projectOwner = jObject["ProjectOwner"];
                jObject["Submitter"] = projectOwner;
                jObject["ProjectOwner"] = submitterToken;
            }
        }
        else
        {
            if (jObject["Submitter"] != null)
                jObject.Remove("Submitter");
            if (jObject["ProjectOwner"] != null)
                jObject.Remove("ProjectOwner");
        }
        if (jObject.TryGetValue(Constants.Messages, out JToken value))
        {
            jArray = (value.ToString()).ToJArray();
        }
        if (jObject.TryGetValue(Constants.BudgetIndicator, out value))
        {
            if (value.ToString() == Constants.OverBudget)
            {
                jArray.Add(JObject.FromObject(new { Severity = true, Icon = "&#xE1AA;", Text = Config[ConfigurationKey.Message_OverBudget.ToString()] }));
            }
            //Commenting so that the new PSP changes are reflected in Details part.
            //PSP are now sending the BudgetIndicator having Constants.WithinBudget etc. due to which this gets removed without adding it into the Messages part in above if condition.
            //value.Parent.Remove();
        }
        if (jObject.TryGetValue(Constants.IsConfidential, out value))
        {
            if (Convert.ToBoolean(value))
            {
                jArray.Add(JObject.FromObject(new { Severity = true, Icon = "&#xE1F6;", Text = Config[ConfigurationKey.Message_ConfidentialPO.ToString()] }));
            }
            value.Parent.Remove();
        }
        if (jObject.TryGetValue(Constants.StatusAlert, out value))
        {
            if (!String.IsNullOrEmpty(value.ToString()))
            {
                jArray.Add(JObject.FromObject(new { Severity = false, Icon = "&#xE1F6;", Text = value.ToString() }));
            }
            value.Parent.Remove();
        }
        if (jArray.Any())
        {
            jObject[Constants.Messages] = jArray;
        }
        return jObject.ToJson();
    }

    public override JArray ExtractApproverChain(string responseString, string currentApprover, string loggedInUser)
    {
        JObject responseObject = responseString.ToJObject();
        if (responseObject["ApproversChain"] != null)
            return (responseObject["ApproversChain"].ToString()).ToJArray();
        else
            return new JArray();
    }

    protected override string FutureApproverChainOperationName()
    {
        return Constants.HeaderOperationType;
    }

    #endregion GET DETAIL

    #region CONVERTOR changes

    protected override async Task<SummaryJson> ExtractJSONDetails(ApprovalRequestExpression approvalRequest, JObject summaryJsonObject)
    {
        return summaryJsonObject?.ToString().FromJson<SummaryJson>();
    }

    #endregion CONVERTOR changes

    #region DocumentAction Methods

    protected override bool IsTenantPendingUpdateRequired(string actionType, ApprovalRequest approvalRequest)
    {
        bool returnParam = true;
        try
        {
            if (actionType.Equals("AddApprover", StringComparison.InvariantCultureIgnoreCase))
            {
                if (approvalRequest.ActionDetails.ContainsKey("SequenceID") && approvalRequest.ActionDetails["SequenceID"].Equals("Before", StringComparison.InvariantCultureIgnoreCase) == false)
                {
                    returnParam = false;
                }
            }
            else if (!String.IsNullOrEmpty(approvalTenantInfo.TenantActionDetails))
            {
                returnParam = base.IsTenantPendingUpdateRequired(actionType, approvalRequest);
            }
        }
        catch
        {
            //do nothing - Fails only if Parsing Logic Fails
        }
        return returnParam;
    }

    /// <summary>
    /// Updates the action properties.
    /// </summary>
    /// <param name="alias">The alias.</param>
    /// <param name="summary">The summary.</param>
    /// <param name="approvalRequest">The approval request.</param>
    /// <param name="additionalData">The Additional Data from ApprovalDetails table</param>
    protected override void UpdateActionProperties(string alias, ApprovalSummaryRow summary, ApprovalRequest approvalRequest, Dictionary<string, string> additionalData)
    {
        if (approvalRequest.Action.Equals("Reassign", StringComparison.InvariantCultureIgnoreCase))
        {
            if (!string.IsNullOrEmpty(summary.SummaryJson))
            {
                JObject summaryObject = (summary.SummaryJson).ToJObject();
                var approvalHierarchy = MSAHelper.ExtractValueFromJSON(summaryObject, "ApprovalHierarchy").FromJson<List<ApprovalHierarchy>>();
                if (approvalHierarchy != null)
                {
                    var approver = approvalHierarchy.Where(x => x.Approvers != null && x.Approvers.FirstOrDefault(y => y.Alias == alias) != null).FirstOrDefault();
                    if (approver != null)
                    {
                        approvalRequest.ActionDetails["ApproverType"] = approver.ApproverType;
                    }
                }
                else if (additionalData != null && additionalData.ContainsKey("ApproverType"))
                {
                    approvalRequest.ActionDetails["ApproverType"] = additionalData["ApproverType"];
                }
            }
        }
        base.UpdateActionProperties(alias, summary, approvalRequest, additionalData);
    }

    #endregion DocumentAction Methods

    #region Post processing authsum

    /// <summary>
    /// Removes certain fields from Authsum Object
    /// </summary>
    /// <param name="summaryAuthsumObject">The authentication sum object</param>
    /// <returns>authentication sum object after removal of fields</returns>
    public override JObject RemoveFieldsFromResponse(JObject authsumObject)
    {
        if (authsumObject["Submitter"] != null)
        {
            authsumObject.Remove("Submitter");
        }
        if (authsumObject["ProjectOwner"] != null)
        {
            authsumObject.Remove("ProjectOwner");
        }
        return authsumObject;
    }

    #endregion Post processing authsum

    #region Download Document

    public override string AttachmentDetailsOperationName()
    {
        return Constants.HeaderOperationType;
    }

    #endregion Download Document

    #region Construct Dynamic HtmlDetails For OutlookQuickAction Email

    /// <summary>
    /// Generates adaptive card using template and data
    /// Modifies the adaptive card
    /// </summary>
    /// <param name="template">adaptive card template</param>
    /// <param name="responseJObject">details data Object</param>
    /// <param name="logData">log data</param>
    /// <returns>Adaprive Card</returns>
    public override JObject GenerateAndModifyAdaptiveCard(string template, JObject responseJObject, Dictionary<LogDataKey, object> logData)
    {
        JArray relatedPOs = !responseJObject.ContainsKey("RelatedPOs") || string.IsNullOrWhiteSpace(responseJObject["RelatedPOs"].ToString()) ? new JArray() : JArray.Parse(responseJObject["RelatedPOs"].ToString());
        responseJObject["RelatedPOs"] = relatedPOs;

        JArray changeOrderLIs = !responseJObject.ContainsKey("ChangeOrderLineItems") || string.IsNullOrWhiteSpace(responseJObject["ChangeOrderLineItems"].ToString()) ? new JArray() : JArray.Parse(responseJObject["ChangeOrderLineItems"].ToString());
        responseJObject["ChangeOrderLineItems"] = changeOrderLIs;

        return base.GenerateAndModifyAdaptiveCard(template, responseJObject, logData);
    }

    #endregion Construct Dynamic HtmlDetails For OutlookQuickAction Email
}