// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace Microsoft.CFS.Approvals.Domain.BL.Tenants.Core;

using System.Collections.Generic;
using System.Linq;
using AdaptiveCards;
using Microsoft.CFS.Approvals.Common.DL.Interface;
using Microsoft.CFS.Approvals.Contracts;
using Microsoft.CFS.Approvals.Data.Azure.Storage.Interface;
using Microsoft.CFS.Approvals.Extensions;
using Microsoft.CFS.Approvals.LogManager.Model;
using Microsoft.CFS.Approvals.LogManager.Provider.Interface;
using Microsoft.CFS.Approvals.Model;
using Microsoft.CFS.Approvals.Utilities.Helpers;
using Microsoft.CFS.Approvals.Utilities.Interface;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json.Linq;

/// <summary>
/// Class ElevateCapex
/// </summary>
/// <seealso cref="GenericTenant" />
public class ElevateCapex : GenericTenant
{
    #region CONSTRUCTOR

    public ElevateCapex(
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

    public ElevateCapex(
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

    #region Constants

    public const string StandardServerRequest = "standardsrvs";
    public const string StandardServerAction = "StandardServersAction";
    public const string StandardServerApproverComment = "StandardServersApproverComment";
    public const string ManualStandardServerRequest = "manualstdrvs";
    public const string ManualStandardServerAction = "ServerManualAction";
    public const string ManualStandardServerApproverComment = "ServerManualApproverComment";

    public const string NetworkEquipmentRequest = "networkequipt1";
    public const string NetworkEquipmentAction = "NetworkT1EquipmentsAction";
    public const string NetworkEquipmentApproverComment = "NetworkT1EquipmentsApproverComment";
    public const string ManualNetworkRequest = "manualntwrkt1";
    public const string ManualNetworkAction = "NetworkT1ManualAction";
    public const string ManualNetworkComment = "NetworkT1ManualApproverComment";

    public const string NetworkEquipment2Request = "networkequipt2";
    public const string NetworkEquipment2Action = "NetworkT2EquipmentsAction";
    public const string NetworkEquipment2ApproverComment = "NetworkT2EquipmentsApproverComment";
    public const string ManualNetwork2Request = "manualntwrkt2";
    public const string ManualNetwork2Action = "NetworkT2ManualAction";
    public const string ManualNetwork2Comment = "NetworkT2ManualApproverComment";

    public const string CablesStandardRequest = "cablesequipt";
    public const string CablesStandardAction = "CablesStandardAction";
    public const string CablesStandardApproverComment = "CablesStandardApproverComment";
    public const string CablesManualRequest = "manualcables";
    public const string CablesManualAction = "CablesManualAction";
    public const string CablesManualApproverComment = "CablesManualApproverComment";

    public const string RequestLineItemId = "lineitemid";
    public const string ActionBodyLineItemId = "LineItemID";
    public const string AdaptiveCardBody = "body";
    public const string ActionBlock = "actionBlock";

    #endregion Constants

    /// <summary>
    /// Generates adaptive card using template and data
    /// Modifies the adaptive card
    /// </summary>
    /// <param name="template">adaptive card template</param>
    /// <param name="responseJObject">details data Object</param>
    /// <param name="logData">log data</param>
    /// <returns>Adaptive Card</returns>
    public override JObject GenerateAndModifyAdaptiveCard(string template, JObject responseJObject, Dictionary<LogDataKey, object> logData)
    {
        List<string> lineItemIds = new List<string>();
        List<string> actionIds = new List<string>();
        List<string> commentIds = new List<string>();

        AdaptiveCard adaptiveCard = MSAHelper.CreateCard(base.GenerateAndModifyAdaptiveCard(template, responseJObject, logData).ToJson());

        var standardServers = responseJObject[StandardServerRequest]?.ToString()?.FromJson<JArray>();
        if (standardServers != null)
        {
            for (int i = 0; i < standardServers.Count; i++)
            {
                lineItemIds.Add(standardServers[i][RequestLineItemId].ToString());
                actionIds.Add(StandardServerAction + i);
                commentIds.Add(StandardServerApproverComment + i);
            }
        }

        var manualStdServer = responseJObject[ManualStandardServerRequest]?.ToString()?.FromJson<JArray>();
        if (manualStdServer != null)
        {
            for (int i = 0; i < manualStdServer.Count; i++)
            {
                lineItemIds.Add(manualStdServer[i][RequestLineItemId].ToString());
                actionIds.Add(ManualStandardServerAction + i);
                commentIds.Add(ManualStandardServerApproverComment + i);
            }
        }

        var networkEquipments = responseJObject[NetworkEquipmentRequest]?.ToString()?.FromJson<JArray>();
        if (networkEquipments != null)
        {
            for (int i = 0; i < networkEquipments.Count; i++)
            {
                lineItemIds.Add(networkEquipments[i][RequestLineItemId].ToString());
                actionIds.Add(NetworkEquipmentAction + i);
                commentIds.Add(NetworkEquipmentApproverComment + i);
            }
        }

        var manualNetworks = responseJObject[ManualNetworkRequest]?.ToString()?.FromJson<JArray>();
        if (manualNetworks != null)
        {
            for (int i = 0; i < manualNetworks.Count; i++)
            {
                lineItemIds.Add(manualNetworks[i][RequestLineItemId].ToString());
                actionIds.Add(ManualNetworkAction + i);
                commentIds.Add(ManualNetworkComment + i);
            }
        }

        var networkEquipments2 = responseJObject[NetworkEquipment2Request]?.ToString()?.FromJson<JArray>();
        if (networkEquipments2 != null)
        {
            for (int i = 0; i < networkEquipments2.Count; i++)
            {
                lineItemIds.Add(networkEquipments2[i][RequestLineItemId].ToString());
                actionIds.Add(NetworkEquipment2Action + i);
                commentIds.Add(NetworkEquipment2ApproverComment + i);
            }
        }

        var manualNetworks2 = responseJObject[ManualNetwork2Request]?.ToString()?.FromJson<JArray>();
        if (manualNetworks2 != null)
        {
            for (int i = 0; i < manualNetworks2.Count; i++)
            {
                lineItemIds.Add(manualNetworks2[i][RequestLineItemId].ToString());
                actionIds.Add(ManualNetwork2Action + i);
                commentIds.Add(ManualNetwork2Comment + i);
            }
        }

        var cablesStandard = responseJObject[CablesStandardRequest]?.ToString()?.FromJson<JArray>();
        if (cablesStandard != null)
        {
            for (int i = 0; i < cablesStandard.Count; i++)
            {
                lineItemIds.Add(cablesStandard[i][RequestLineItemId].ToString());
                actionIds.Add(CablesStandardAction + i);
                commentIds.Add(CablesStandardApproverComment + i);
            }
        }

        var cablesManual = responseJObject[CablesManualRequest]?.ToString()?.FromJson<JArray>();
        if (cablesManual != null)
        {
            for (int i = 0; i < cablesManual.Count; i++)
            {
                lineItemIds.Add(cablesManual[i][RequestLineItemId].ToString());
                actionIds.Add(CablesManualAction + i);
                commentIds.Add(CablesManualApproverComment + i);
            }
        }

        AdaptiveContainer actionContainer = (AdaptiveContainer)adaptiveCard.Body.Where(e => e.Id == ActionBlock).FirstOrDefault();
        if (actionContainer != null && actionContainer.Items != null && actionContainer.Items.Count > 0)
        {
            string actionBody = ((AdaptiveActionSet)actionContainer.Items[0]).Actions[0].AdditionalProperties[AdaptiveCardBody].ToString();
            JObject actionBodyObj = JObject.Parse(actionBody);

            JArray lineItemsArray = new JArray();
            for (int i = 0; i < lineItemIds.Count; i++)
            {
                JObject lineItemElement = new JObject
                {
                    { ActionBodyLineItemId, lineItemIds[i].ToString() },
                    { Constants.ActionKey, "{{" + actionIds[i] + ".value}}" },
                    { Constants.CommentKey, "{{" + commentIds[i] + ".value}}" }
                };
                lineItemsArray.Add(lineItemElement);
            }
            JObject additionalData = new JObject
            {
                { Constants.LineItems, lineItemsArray.ToJson() }
            };

            actionBodyObj[Constants.ActionBody][Constants.AdditionalData] = additionalData;
            ((AdaptiveActionSet)actionContainer.Items[0]).Actions[0].AdditionalProperties[AdaptiveCardBody] = actionBodyObj.ToJson();
        }
        return JObject.Parse(adaptiveCard.ToJson());
    }
}