// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace Microsoft.CFS.Approvals.Domain.BL.Tenants.Core;

using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.CFS.Approvals.Common.DL.Interface;
using Microsoft.CFS.Approvals.Contracts.DataContracts;
using Microsoft.CFS.Approvals.Data.Azure.Storage.Interface;
using Microsoft.CFS.Approvals.LogManager.Provider.Interface;
using Microsoft.CFS.Approvals.Model;
using Microsoft.CFS.Approvals.Utilities.Interface;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

/// <summary>
/// Class Courier
/// </summary>
/// <seealso cref="GenericTenant" />
public class Courier : GenericTenant
{
    #region CONSTRUCTOR

    public Courier(
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

    public Courier(
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

    /// <summary>
    /// Calling function to add EditableField after reciving response from Courier tenant API
    /// </summary>
    /// <param name="lobResponse"></param>
    /// <param name="operation"></param>
    /// <param name="approvalIdentifier"></param>
    /// <returns></returns>
    protected override async Task<HttpResponseMessage> ExecutePostDetailOperationAsync(HttpResponseMessage lobResponse, string operation, ApprovalIdentifier approvalIdentifier)
    {
        lobResponse = await base.ExecutePostDetailOperationAsync(lobResponse, operation, approvalIdentifier);
        string jsonDetail = await lobResponse.Content.ReadAsStringAsync();
        var jsonDetailsPostProcess = AddEditableFieldsProperties(jsonDetail, operation);

        return new HttpResponseMessage() { Content = new StringContent(jsonDetailsPostProcess), StatusCode = lobResponse.StatusCode };
    }

    /// <summary>
    /// Method to extract the Editable fields from details json, if any,
    /// and adding it into a new property "EditableField" at Lineitem and Sub-Lineitem level
    /// </summary>
    /// <param name="jsonDetail"></param>
    /// <param name="operation"></param>
    /// <returns></returns>
    public override string AddEditableFieldsProperties(string jsonDetail, string operation)
    {
        switch (operation)
        {
            case "LINES":
                if (!string.IsNullOrEmpty(jsonDetail))
                {
                    // Iterate over each line item and convert the attachments details
                    JObject lineDetailsObj = JObject.Parse(jsonDetail);
                    if (lineDetailsObj["LineItems"] != null && !string.IsNullOrEmpty(lineDetailsObj["LineItems"].ToString()))
                    {
                        List<dynamic> lineItemObjects = JsonConvert.DeserializeObject<List<dynamic>>(lineDetailsObj["LineItems"].ToString());
                        for (int i = 0; i < lineItemObjects.Count; i++)
                        {
                            lineItemObjects[i] = ModifyLineItems(lineItemObjects[i]);
                            if (lineItemObjects[i].Children != null)
                            {
                                List<dynamic> childLineItemObjects = JsonConvert.DeserializeObject<List<dynamic>>(JsonConvert.SerializeObject(lineItemObjects[i].Children));
                                for (int j = 0; j < childLineItemObjects.Count; j++)
                                {
                                    childLineItemObjects[j] = ModifyLineItems(childLineItemObjects[j]);
                                }
                                lineItemObjects[i].Children = JArray.Parse(JsonConvert.SerializeObject(childLineItemObjects));
                            }
                        }
                        var lineDetailsObjNew = new JObject
                        {
                            { "LineItems", JToken.Parse(JsonConvert.SerializeObject(lineItemObjects)) }
                        };
                        lineDetailsObj["LineItems"] = lineDetailsObjNew["LineItems"];
                        jsonDetail = JsonConvert.SerializeObject(lineDetailsObj);
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
    /// <returns></returns>
    private static dynamic ModifyLineItems(dynamic lineItemObject)
    {
        if (lineItemObject.jsonForm != null & lineItemObject.jsonSchema != null)
        {
            JArray jsonForm = JArray.Parse(JsonConvert.SerializeObject(lineItemObject.jsonForm));
            List<string> editableParameters = new List<string>();
            foreach (var form in jsonForm)
            {
                JObject obj;
                try { obj = JObject.Parse(form.ToString()); } catch { continue; }
                JToken keyValue;
                if (obj.TryGetValue("key", out keyValue))
                {
                    editableParameters.Add(keyValue.ToString());
                }
            }

            foreach (var editableFeild in editableParameters)
            {
                JObject lineItem = JObject.Parse(JsonConvert.SerializeObject(lineItemObject));
                JObject oldValue = new JObject
                {
                    { editableFeild, bool.Parse(lineItem[editableFeild].ToString()) }
                };
                JObject newValue = new JObject
                {
                    { editableFeild, bool.Parse(lineItem[editableFeild].ToString()) }
                };

                JObject values = new JObject
                {
                    { "OldValue", JToken.Parse(oldValue.ToString()) },
                    { "NewValue", JToken.Parse(newValue.ToString()) }
                };

                JObject billable = new JObject
                {
                    { editableFeild, JToken.Parse(values.ToString()) }
                };

                JObject field = new JObject
                {
                    { "ID", JToken.Parse(lineItemObject.LineItemID.ToString()) },
                    { "Fields", JToken.Parse(billable.ToString()) }
                };

                lineItemObject.EditableField = field;
            }
        }
        return lineItemObject;
    }
}