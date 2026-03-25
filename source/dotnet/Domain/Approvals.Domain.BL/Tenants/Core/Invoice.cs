// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace Microsoft.CFS.Approvals.Domain.BL.Tenants.Core;

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.CFS.Approvals.Common.DL.Interface;
using Microsoft.CFS.Approvals.Contracts;
using Microsoft.CFS.Approvals.Contracts.DataContracts;
using Microsoft.CFS.Approvals.Data.Azure.Storage.Interface;
using Microsoft.CFS.Approvals.Extensions;
using Microsoft.CFS.Approvals.LogManager.Provider.Interface;
using Microsoft.CFS.Approvals.Model;
using Microsoft.CFS.Approvals.Utilities.Extension;
using Microsoft.CFS.Approvals.Utilities.Helpers;
using Microsoft.CFS.Approvals.Utilities.Interface;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json.Linq;

/// <summary>
/// Class Invoice
/// </summary>
/// <seealso cref="TenantBase" />
public class Invoice : TenantBase
{
    #region CONSTRUCTOR

    public Invoice(
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

    public Invoice(
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
    /// Form an URL to access MSInvoice services
    /// </summary>
    /// <param name="documentSummaryUrl"></param>
    /// <param name="approvalRequest"></param>
    /// <returns></returns>
    protected override string GetSummaryUrl(ApprovalTenantInfo tenantInfo, string documentSummaryUrl, ApprovalRequestExpression approvalRequest, string docTypeId = "")
    {
        return string.Format(documentSummaryUrl, approvalRequest.ApprovalIdentifier.GetDocNumber(tenantInfo), approvalRequest.ApprovalIdentifier.FiscalYear);
    }

    /// <summary>
    /// Get Approval Summary By RowKey And Approver
    /// </summary>
    /// <param name="rowKey"></param>
    /// <param name="approverAlias"></param>
    /// <param name="approverId"></param>
    /// <param name="approverDomain"></param>
    /// <param name="fiscalYear"></param>
    /// <param name="tenantInfo"></param>
    /// <returns></returns>
    public override ApprovalSummaryRow GetApprovalSummaryByRowKeyAndApprover(string rowKey, string approverAlias, string approverId, string approverDomain, string fiscalYear, ApprovalTenantInfo tenantInfo)
    {
        var rowkey = Extension.GetRowKey(tenantInfo, rowKey, fiscalYear);
        return ApprovalSummaryProvider.GetApprovalSummaryByRowKeyAndApprover(tenantInfo.DocTypeId, rowkey, approverAlias, approverId, approverDomain);
    }

    #endregion GET SUMMARY

    #region Convert JSON Methods

    /// <summary>
    /// Extracts the json details.
    /// </summary>
    /// <param name="approvalRequest">The approval request.</param>
    /// <param name="summaryJsonObject">The summary json object.</param>
    /// <returns>
    /// returns summary Json
    /// </returns>
    /// <exception cref="Exception">Invalid Summary Data: " + summaryJson.ToJson()</exception>
    protected override async Task<SummaryJson> ExtractJSONDetails(ApprovalRequestExpression approvalRequest, JObject summaryJsonObject)
    {
        var tenantOperationDetails = approvalTenantInfo.DetailOperations.DetailOpsList.FirstOrDefault(x => x.operationtype.ToUpper() == Constants.SummaryOperationType);
        var serializationType = tenantOperationDetails != null ? tenantOperationDetails.SerializerType : 0;
        SummaryJson summaryJson = new SummaryJson();

        if (summaryJsonObject.TryGetValue("VendorName", out JToken value))
        {
            string submitterName = value.ToString();
            summaryJson.Submitter =
                new User()
                {
                    Name = submitterName,
                    Alias = null
                };

            value.Parent.Remove();
        }

        DateTime.TryParse(ReadAndRemoveToken(summaryJsonObject, "SubmittedDate"), out DateTime submittedDate);
        summaryJson = serializationType switch
        {
            (int)(SerializerType.DataContractSerializer) => JSONHelper.ConvertJSONToObject<SummaryJson>(summaryJsonObject.ToString()),
            _ => summaryJsonObject?.ToString().FromJson<SummaryJson>(),
        };
        if (summaryJson == null || summaryJson.ApprovalIdentifier == null || summaryJson.ApprovalIdentifier.DisplayDocumentNumber == null)
        {
            throw new InvalidDataException("Invalid Summary Data: " + summaryJson.ToJson());
        }

        summaryJson.SubmittedDate = submittedDate;
        return summaryJson;
    }

    #endregion Convert JSON Methods

    #region COMMOM Methods

    /// <summary>
    /// Check if HttpStatusCode.NotFound should be treated as error or not for a specific Tenant
    /// </summary>
    /// <returns>returns false if HttpStatus.NotFound is not supposed to be treated as error</returns>
    public override bool TreatNotFoundAsError()
    {
        return true;
    }

    #endregion COMMOM Methods

    #region GET DETAIL

    /// <summary>
    /// Gets the detail URL.
    /// </summary>
    /// <param name="urlFormat">The URL format.</param>
    /// <param name="approvalIdentifier">The approval identifier.</param>
    /// <param name="page">The page.</param>
    /// <param name="xcv">The XCV.</param>
    /// <param name="tcv">The TCV.</param>
    /// <param name="businessProcessName">Name of the business process.</param>
    /// <param name="docTypeId">Document type id</param>
    /// <returns></returns>
    protected override async Task<string> GetDetailURL(string urlFormat, ApprovalIdentifier approvalIdentifier, int page, string xcv = "", string tcv = "", string businessProcessName = "", string docTypeId = "")
    {
        return String.Format(urlFormat, approvalIdentifier.DocumentNumber, approvalIdentifier.FiscalYear);
    }

    /// <summary>
    /// Futures the name of the approver chain operation.
    /// </summary>
    /// <returns></returns>
    protected override string FutureApproverChainOperationName()
    {
        return Constants.InvoiceDetailsAction;
    }

    /// <summary>
    /// Formulate Http Response in case of Invoice for JSON since Invoice returns the data in XML format
    /// </summary>
    /// <param name="lobResponse"></param>
    /// <returns>HttpResponseMessage</returns>
    protected override async Task<HttpResponseMessage> ExecutePostDetailOperationAsync(HttpResponseMessage lobResponse, string operation, ApprovalIdentifier approvalIdentifier)
    {
        lobResponse = await base.ExecutePostDetailOperationAsync(lobResponse, operation, approvalIdentifier);
        string jsonDetail = await lobResponse.Content.ReadAsStringAsync();
        var jsonDetailsPostProcess = PostProcessDetails(jsonDetail, operation);
        jsonDetailsPostProcess = AddEditableFieldsProperties(jsonDetailsPostProcess, operation);
        return new HttpResponseMessage() { Content = new StringContent(jsonDetailsPostProcess), StatusCode = lobResponse.StatusCode };
    }

    /// <summary>
    /// Checks if Proof of Execution is required
    /// </summary>
    /// <param name="jsonDetail">jsonDetail</param>
    /// <param name="operation">operation</param>
    /// <returns>string</returns>
    public override string PostProcessDetails(string jsonDetail, string operation)
    {
        if (operation.Equals(Constants.InvoiceDetailsAction, StringComparison.InvariantCultureIgnoreCase))
        {
            JObject jobject = (jsonDetail).ToJObject();
            if (jobject["HasPOE"] != null && jobject["HasPOE"].Type != JTokenType.Null && jobject["HasPOE"].ToString().Equals("true", StringComparison.InvariantCultureIgnoreCase) && jobject["Messages"] == null)
            {
                JArray jArray = new JArray
                {
                    JObject.FromObject(new { Severity = true, Icon = "&#xE1AA;", Text = "Proof of Execution (POE) approval and attestation is required for this invoice. To manage the approval for this invoice, access <a href='http://msinvoice' target='_blank'>http://msinvoice</a> as it cannot be managed within the Approvals application." })
                };
                jobject["Messages"] = jArray;
            }

            jsonDetail = jobject.ToJson();
        }
        return jsonDetail;
    }

    /// <summary>
    /// Method to extract the Editable fields from details json, if any,
    /// and adding it into a new property "EditableField" at Lineitem and Sub-Lineitem level
    /// </summary>
    /// <param name="jsonDetail"></param>
    /// <param name="operation"></param>
    /// <returns>string</returns>
    public override string AddEditableFieldsProperties(string jsonDetail, string operation)
    {
        switch (operation)
        {
            case "LINE":
                if (!string.IsNullOrEmpty(jsonDetail))
                {
                    // Iterate over each line item
                    JObject lineDetailsObj = jsonDetail.ToJObject();
                    if (lineDetailsObj["LineItems"] != null && !string.IsNullOrWhiteSpace(lineDetailsObj["LineItems"].ToString()))
                    {
                        List<dynamic> lineItemObjects = lineDetailsObj["LineItems"].ToString().FromJson<List<dynamic>>();
                        List<dynamic> modifiedLineItemObjects = new List<dynamic>();
                        foreach (var lineItemObject in lineItemObjects)
                        {
                            var modifiedlineItemObject = ModifyLineItems(lineItemObject);
                            modifiedLineItemObjects.Add(modifiedlineItemObject);
                        }
                        var lineDetailsObjNew = new JObject
                        {
                            { "LineItems", modifiedLineItemObjects.ToJson().ToJToken() }
                        };
                        lineDetailsObj["LineItems"] = lineDetailsObjNew["LineItems"];
                        jsonDetail = lineDetailsObj.ToJson();
                    }
                }
                break;

            default:
                break;
        }
        return jsonDetail;
    }

    /// <summary>
    /// Method to extract the Editable fields from Line items/ Sub-line items, if any
    /// </summary>
    /// <param name="lineItemObject"></param>
    /// <returns>dynamic</returns>
    private dynamic ModifyLineItems(dynamic lineItemObject)
    {
        if (lineItemObject.jsonForm != null & lineItemObject.jsonSchema != null)
        {
            JArray jsonForm = ((string)lineItemObject.jsonForm.ToString()).ToJArray();
            List<string> editableParameters = new List<string>();
            foreach (var form in jsonForm)
            {
                JObject obj;
                try { obj = form.ToString().ToJObject(); } catch { continue; }
                if (obj.TryGetValue("key", out JToken keyValue))
                {
                    editableParameters.Add(keyValue.ToString());
                }
            }

            foreach (var editableFeild in editableParameters)
            {
                JObject lineItem = ((string)lineItemObject.ToString()).ToJObject();
                JObject oldValue = new JObject
                {
                    { editableFeild, lineItem[editableFeild].ToString() }
                };
                JObject newValue = new JObject
                {
                    { editableFeild, lineItem[editableFeild].ToString() }
                };

                JObject values = new JObject
                {
                    { "OldValue", oldValue },
                    { "NewValue", newValue }
                };

                JObject billable = new JObject
                {
                    { editableFeild, values.ToString().ToJToken() }
                };

                JObject field = new JObject
                {
                    // Invoice sends value like 001, 002...008 in the AssociatedLineItemID
                    // When this is considered as a string, Newtonsoft's JToken.Parse method consider this as interger
                    // But as this has leading zeros, it considers this as Octal Number.
                    // There is an exception thrown when 008 is tried to parse as 8 is not a valid Octal Number
                    // To handle this as well as retain the leading zeros, we considered this as an object and used the JToken.FromObject method instead of JToken.Parse
                    { "ID", ((object)lineItemObject.AssociatedLineItemID).ToJToken() },
                    { "Fields", billable.ToString().ToJToken() }
                };

                lineItemObject.EditableField = field;
            }
        }
        return lineItemObject;
    }

    #endregion GET DETAIL

    #region DocumentAction Methods

    /// <summary>
    /// Updates the action properties.
    /// </summary>
    /// <param name="alias">The alias.</param>
    /// <param name="summary">The summary.</param>
    /// <param name="approvalRequest">The approval request.</param>
    /// <param name="additionalData">The Additional Data from ApprovalDetails table</param>
    protected override void UpdateActionProperties(string alias, ApprovalSummaryRow summary, ApprovalRequest approvalRequest, Dictionary<string, string> additionalData)
    {
        base.UpdateActionProperties(alias, summary, approvalRequest, additionalData);
        string commentString = string.Empty;
        if (approvalRequest.ActionDetails != null && approvalRequest.ActionDetails.ContainsKey("Comment"))
        {
            commentString = approvalRequest.ActionDetails["Comment"];
        }

        if (approvalRequest.Action.Equals("Reassign", StringComparison.InvariantCultureIgnoreCase))
        {
            approvalRequest.ActionDetails.Add("CommentsToNextApprover", commentString);
        }
        else
        {
            approvalRequest.ActionDetails.Add("CommentsToVendor", commentString);
        }
        if (approvalRequest.Action.Equals("Reassign", StringComparison.InvariantCultureIgnoreCase))
        {
            string newApproverAlias = string.Empty;
            if (approvalRequest.ActionDetails != null && approvalRequest.ActionDetails.ContainsKey("NewApproverAlias"))
            {
                newApproverAlias = approvalRequest.ActionDetails["NewApproverAlias"];
            }

            approvalRequest.ActionDetails.Add("ReassignAlias", newApproverAlias);
        }
        else
        {
            approvalRequest.ActionDetails.Add("POESelected", false.ToString());
        }
    }

    /// <summary>
    /// Modify ARX to Update Submitter name with vendor name
    /// </summary>
    /// <param name="requestExpressions"></param>
    /// <returns>List<ApprovalRequestExpressionExt></returns>
    public async override Task<List<ApprovalRequestExpressionExt>> ModifyApprovalRequestExpression(List<ApprovalRequestExpressionExt> requestExpressions)
    {
        List<ApprovalRequestExpressionExt> modifiedRequestExpressions = new List<ApprovalRequestExpressionExt>();
        foreach (var requestExpression in requestExpressions)
        {
            base.AddAdditionalDataToDetailsData(requestExpression, requestExpression.SummaryData, string.Empty);
            Dictionary<string, string> additionalDetails = null;
            if (requestExpression.DetailsData != null && requestExpression.DetailsData.Any() && requestExpression.DetailsData.ContainsKey(Constants.AdditionalDetails))
            {
                additionalDetails = requestExpression.DetailsData[Constants.AdditionalDetails]?.ToJObject()[Constants.AdditionalData]?.ToString().FromJson<Dictionary<string, string>>();
            }
            if (requestExpression.SummaryData != null && requestExpression.DetailsData != null && additionalDetails != null
                && additionalDetails.ContainsKey(Constants.MSInvoiceVendorNameKey))
            {
                requestExpression.SummaryData.Submitter = new User()
                {
                    Name = additionalDetails[Constants.MSInvoiceVendorNameKey],
                    Alias = null
                };
            }
            await base.ModifyApprovers(requestExpression);
            modifiedRequestExpressions.Add(requestExpression);
        }
        return modifiedRequestExpressions;
    }

    #endregion DocumentAction Methods
}