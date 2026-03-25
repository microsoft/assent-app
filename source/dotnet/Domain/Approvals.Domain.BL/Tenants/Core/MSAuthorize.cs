// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace Microsoft.CFS.Approvals.Domain.BL.Tenants.Core;

using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.CFS.Approvals.Common.DL.Interface;
using Microsoft.CFS.Approvals.Contracts;
using Microsoft.CFS.Approvals.Contracts.DataContracts;
using Microsoft.CFS.Approvals.Data.Azure.Storage.Interface;
using Microsoft.CFS.Approvals.Extensions;
using Microsoft.CFS.Approvals.LogManager.Provider.Interface;
using Microsoft.CFS.Approvals.Model;
using Microsoft.CFS.Approvals.Utilities.Interface;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json.Linq;

/// <summary>
/// Class MSAuthorize
/// </summary>
/// <seealso cref="GenericTenant" />
public class MSAuthorize : GenericTenant
{
    #region CONSTRUCTOR

    public MSAuthorize(
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

    public MSAuthorize(
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

    #region GET DETAILS

    /// <summary>
    /// Formulate HTTP Response message to be returned to the client;
    /// that sends XML response which is then converted into JSON and sent back to the clients
    /// </summary>
    /// <param name="lobResponse"></param>
    /// <returns>returns task containing Http Response message</returns>
    protected override async Task<HttpResponseMessage> ExecutePostDetailOperationAsync(HttpResponseMessage lobResponse, string operation, ApprovalIdentifier approvalIdentifier)
    {
        lobResponse = await base.ExecutePostDetailOperationAsync(lobResponse, operation, approvalIdentifier);
        string jsonDetail = await lobResponse.Content.ReadAsStringAsync();
        var jsonDetailsPostProcess = AddEditableFieldsProperties(jsonDetail, operation);

        return new HttpResponseMessage() { Content = new StringContent(jsonDetailsPostProcess), StatusCode = lobResponse.StatusCode };
    }

    /// <summary>
    /// Method to extract the Editable fields from details json, if any,
    /// and adding it into a new property "EditableField" to AdditionalData
    /// </summary>
    /// <param name="jsonDetail">json Detail string</param>
    /// <param name="operation">detail operation</param>
    /// <returns>string</returns>
    public override string AddEditableFieldsProperties(string jsonDetail, string operation)
    {
        switch (operation)
        {
            case "ADDNDTL":
                if (!string.IsNullOrEmpty(jsonDetail))
                {
                    // Iterate over each line item
                    Dictionary<string, string> additionalData = jsonDetail.ToJObject()[Constants.AdditionalData]?.ToString().FromJson<Dictionary<string, string>>();
                    if (additionalData != null)
                    {
                        ModifyDetails(additionalData, out JObject jsonSchema, out JArray jsonForm, out JObject editableField);
                        jsonDetail = (jsonSchema != null && jsonForm != null && editableField != null)
                                        ? (JObject.FromObject(new { AdditionalData = additionalData, jsonSchema, jsonForm, EditableField = editableField })).ToJson()
                                        : (JObject.FromObject(new { AdditionalData = additionalData })).ToJson();
                    }
                }
                break;

            default:
                break;
        }
        return jsonDetail;
    }

    /// <summary>
    /// Method to extract the Editable fields from AdditionalData
    /// </summary>
    /// <param name="additionalData">Dictionary object of Additional Data</param>
    /// <param name="jsonSchema">json Schema object</param>
    /// <param name="jsonForm">json Form array</param>
    /// <param name="EditableField">Editable field object</param>
    private void ModifyDetails(Dictionary<string, string> additionalData, out JObject jsonSchema, out JArray jsonForm, out JObject editableField)
    {
        jsonSchema = null; jsonForm = null; editableField = null;
        if (additionalData.ContainsKey("jsonForm") && additionalData.ContainsKey("jsonSchema"))
        {
            // Parsing string into JObject/ JArray and assigning to out parameters
            jsonSchema = JObject.Parse(additionalData["jsonSchema"].ToString());
            jsonForm = JArray.Parse(additionalData["jsonForm"].ToString());

            //Removing jsonSchema and jsonForm from AdditionalData
            additionalData.Remove("jsonSchema");
            additionalData.Remove("jsonForm");

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

            JObject editableFields = new JObject();
            foreach (var editableFeild in editableParameters)
            {
                JObject oldValue = new JObject
                {
                    { editableFeild, additionalData.ContainsKey(editableFeild) ? additionalData[editableFeild]?.ToString() : string.Empty }
                };
                JObject newValue = new JObject
                {
                    { editableFeild, additionalData.ContainsKey(editableFeild) ? additionalData[editableFeild]?.ToString() : string.Empty }
                };

                JObject values = new JObject
                {
                    { "OldValue", oldValue },
                    { "NewValue", newValue }
                };

                editableFields.Add(editableFeild, (values.ToJson()).ToJToken());
            }

            editableField = new JObject
            {
                { "ID", "1" },
                { "Fields", (editableFields.ToJson()).ToJToken() }
            };
        }
    }

    #endregion GET DETAILS
}