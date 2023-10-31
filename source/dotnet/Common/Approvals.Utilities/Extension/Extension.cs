// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace Microsoft.CFS.Approvals.Utilities.Extension;

using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.CFS.Approvals.Contracts;
using Microsoft.CFS.Approvals.Contracts.DataContracts;
using Microsoft.CFS.Approvals.Extensions;
using Microsoft.CFS.Approvals.Model;
using Newtonsoft.Json.Linq;

/// <summary>
/// The Extensions class
/// </summary>
public static class Extension
{
    /// <summary>
    /// Check if action is exempt.
    /// </summary>
    /// <param name="actionName"></param>
    /// <returns></returns>
    public static bool IsActionExempt(this string actionName)
    {
        var exemptActions = new List<string> { "SYSTEM CANCEL", "SYSTEM SEND BACK", "TAKEBACK", "CANCEL", "RESUBMITTED" };
        return exemptActions.Contains(actionName.ToUpper());
    }

    /// <summary>
    /// To the device notification information.
    /// </summary>
    /// <param name="approvalRequestExpression">The approval request expression.</param>
    /// <param name="summaryRow">The summary row.</param>
    /// <param name="correlationId">The correlation identifier.</param>
    /// <returns>returns Devise </returns>
    public static DeviceNotificationInfo ToDeviceNotificationInfo(this ApprovalRequestExpression approvalRequestExpression, ApprovalSummaryRow summaryRow, string correlationId)
    {
        string approver;

        if (summaryRow != null && summaryRow.Approver != null)
            approver = summaryRow.Approver;
        else
            approver = approvalRequestExpression.AdditionalData.ContainsKey(Constants.Approver) ? Convert.ToString(approvalRequestExpression.AdditionalData[Constants.Approver]) : String.Empty;
        if (string.IsNullOrEmpty(approver) && approvalRequestExpression.ActionDetail != null)
            approver = approvalRequestExpression.ActionDetail.ActionBy.Alias;

        var devicenotificationinfo = new DeviceNotificationInfo()
        {
            RoutingId = new Guid(Convert.ToString(approvalRequestExpression.AdditionalData[Constants.RoutingIdColumnName])),
            DocumentTypeId = approvalRequestExpression.DocumentTypeId,
            ApprovalIdentifier = approvalRequestExpression.ApprovalIdentifier,
            Approver = approver,
            Application = summaryRow.Application,
            Requestor = approvalRequestExpression.AdditionalData.ContainsKey(Constants.Requestor) ? Convert.ToString(approvalRequestExpression.AdditionalData[Constants.Requestor]) : summaryRow.Requestor,
            Operation = approvalRequestExpression.Operation,
            OriginalApprover = approvalRequestExpression.ActionDetail != null ? approvalRequestExpression.ActionDetail.ActionBy?.Alias : string.Empty,
            NotificationApprover = approvalRequestExpression.ActionDetail != null && approvalRequestExpression.ActionDetail.NewApprover != null ? approvalRequestExpression.ActionDetail.NewApprover.Alias : string.Empty,
            NotificationRequestQueuingByClientUtcDateTime = DateTime.UtcNow,
            NotificationType = Contracts.NotificationType.None,
            ForFirstTimeUser = false,
            SendNotification = approvalRequestExpression.NotificationDetail != null && approvalRequestExpression.NotificationDetail.SendNotification,
            ActionTaken = approvalRequestExpression.ActionDetail != null ? approvalRequestExpression.ActionDetail.Name : string.Empty,
            To = approvalRequestExpression.NotificationDetail != null ? approvalRequestExpression.NotificationDetail.To : string.Empty,
            CC = approvalRequestExpression.NotificationDetail != null ? approvalRequestExpression.NotificationDetail.Cc : string.Empty,
            BCC = approvalRequestExpression.NotificationDetail != null ? approvalRequestExpression.NotificationDetail.Bcc : string.Empty,
            NotificationTemplateKey = approvalRequestExpression.NotificationDetail != null ? approvalRequestExpression.NotificationDetail.TemplateKey : string.Empty,
            ActionDetails = ConvertActionDetailToDocumentKey(approvalRequestExpression.ActionDetail), //TODO::MD1:: ActionDetails need to be populated correctly
            CorrelationId = correlationId
        };

        return devicenotificationinfo;
    }

    /// <summary>
    /// To Approval Request Details
    /// </summary>
    /// <param name="approvalRequestExpression"></param>
    /// <param name="summaryRows"></param>
    /// <param name="devicenotificationinfo"></param>
    /// <param name="tenantInfo"></param>
    /// <returns></returns>
    public static ApprovalRequestDetails ToApprovalRequestDetails(this ApprovalRequestExpression approvalRequestExpression, List<ApprovalSummaryRow> summaryRows, DeviceNotificationInfo devicenotificationinfo, ApprovalTenantInfo tenantInfo)
    {
        var approvalRequestDetails = new ApprovalRequestDetails()
        {
            ApprovalIdentifier = approvalRequestExpression.ApprovalIdentifier,
            CreateDateTime = DateTime.UtcNow,
            DetailsData = approvalRequestExpression.DetailsData,
            SummaryRows = summaryRows,
            DeviceNotificationInfo = devicenotificationinfo,
            Operation = approvalRequestExpression.Operation,
            Xcv = approvalRequestExpression.Telemetry.Xcv,
            Tcv = approvalRequestExpression.Telemetry.Tcv,
            BusinessProcessName = string.IsNullOrEmpty(approvalRequestExpression.Telemetry.BusinessProcessName) ? tenantInfo.BusinessProcessName : approvalRequestExpression.Telemetry.BusinessProcessName,
            TenantTelemetry = approvalRequestExpression.Telemetry.TenantTelemetry,
            RefreshDetails = approvalRequestExpression.RefreshDetails
        };
        return approvalRequestDetails;
    }

    /// <summary>
    /// Convert Action Detail To Document Key
    /// </summary>
    /// <param name="actionDetails"></param>
    /// <returns></returns>
    private static Dictionary<string, string> ConvertActionDetailToDocumentKey(ActionDetail actionDetails)
    {
        Dictionary<string, string> documentKeys = new Dictionary<string, string>();
        if (actionDetails != null)
        {
            if (actionDetails.AdditionalData != null)
            {
                foreach (KeyValuePair<string, string> additionData in actionDetails.AdditionalData)
                {
                    documentKeys.Add(additionData.Key, additionData.Value);
                }
            }
            if (!string.IsNullOrWhiteSpace(actionDetails.Comment))
                documentKeys.Add("Comment", actionDetails.Comment);
            if (!string.IsNullOrWhiteSpace(actionDetails.Placement))
                documentKeys.Add("Placement", actionDetails.Placement);
            if (actionDetails.ActionBy != null && !string.IsNullOrWhiteSpace(actionDetails.ActionBy.Alias))
                documentKeys.Add("ActionBy", actionDetails.ActionBy.Alias);
            if (actionDetails.ActionBy != null && !string.IsNullOrWhiteSpace(actionDetails.ActionBy.Name))
                documentKeys.Add("ActionByName", actionDetails.ActionBy.Name);
            if (actionDetails.Date != default)
                documentKeys.Add("Date", actionDetails.Date.ToString());
            if (!string.IsNullOrWhiteSpace(actionDetails.Name))
                documentKeys.Add("Name", actionDetails.Name);
        }
        return documentKeys;
    }

    /// <summary>
    /// Get Document Number
    /// </summary>
    /// <param name="approvalIdentifier"></param>
    /// <param name="tenantInfo"></param>
    /// <returns></returns>
    public static string GetDocNumber(this ApprovalIdentifier approvalIdentifier, ApprovalTenantInfo tenantInfo)
    {
        if (tenantInfo.UseDocumentNumberForRowKey)
            return approvalIdentifier.DocumentNumber;
        else
            return approvalIdentifier.DisplayDocumentNumber;
    }

    /// <summary>
    /// Converts stringified JSON to a Dictionary
    /// </summary>
    /// <param name="placeHolderDict"></param>
    /// <param name="summaryData"></param>
    /// <param name="dataplaceHolder"></param>
    /// <returns></returns>
    public static Dictionary<string, object> ConvertJsonToDictionary(Dictionary<string, object> placeHolderDict, string summaryData, string dataplaceHolder = "")
    {
        Dictionary<string, object> values = summaryData.FromJson<Dictionary<string, object>>();
        if (values != null)
        {
            foreach (KeyValuePair<string, object> data in values)
            {
                string placeHolder = String.IsNullOrEmpty(dataplaceHolder) ? data.Key : dataplaceHolder + "." + data.Key;
                if (data.Value != null)
                {
                    //Try...catch is implemented to also include JArray objects in our dictionary and also handle special characters like '[', '{' etc
                    //Earlier we were just skipping the JArray objects and were not including it in our dictionary.
                    try
                    {
                        ConvertJsonToDictionary(placeHolderDict, (data.Value).ToJson(), placeHolder);
                        continue;
                    }
                    catch
                    {
                        //If the value is a JArray or a simple string, then this code will add the value into our dictionary with property name as key.
                        //We can use the key to get the JArray and parse it to implement the logic required in our code
                        placeHolderDict.Add(placeHolder, data.Value);
                    }
                }
                else
                {
                    placeHolderDict.Add(placeHolder, null);
                }
            }
        }
        return placeHolderDict;
    }

    /// <summary>
    /// Validate approver alias against invalid special characters and empty string
    /// </summary>
    /// <param name="approver"></param>
    /// <returns></returns>
    public static bool ValidatePartitionKeyCharacters(this string approver)
    {
        List<string> list = new List<string>() { "/", "\\", "#", "?" };
        var flag = true;
        if (approver == string.Empty)
        { return false; }
        list.ForEach(i =>
        {
            if (approver.Contains(i))
            {
                flag = false;
            }
        });
        return flag;
    }

    /// <summary>
    /// Get the endpointURL from tenant configuration table based on the operation type and tenantId.
    /// Also, checks if the client is registered for the calling device/application.
    /// </summary>
    /// <param name="tenantInfo"></param>
    /// <param name="operationType"></param>
    /// <param name="clientInfo"></param>
    /// <returns>endpointURL</returns>
    public static string GetEndPointURL(this ApprovalTenantInfo tenantInfo, string operationType, string clientInfo)
    {
        string endPointUrl = null;
        if (!tenantInfo.IsClientRegistered(clientInfo)) return endPointUrl;
        var tenantOperationDetails = tenantInfo.DetailOperations.DetailOpsList.FirstOrDefault(x => x.operationtype.ToUpper() == operationType.ToUpper());
        endPointUrl = tenantOperationDetails != null ? tenantInfo.TenantBaseUrl + tenantOperationDetails.endpointdata : null;

        return endPointUrl;
    }

    /// <summary>
    /// Check is client registered
    /// </summary>
    /// <param name="tenantInfo"></param>
    /// <param name="clientInfo"></param>
    /// <returns></returns>
    public static bool IsClientRegistered(this ApprovalTenantInfo tenantInfo, string clientInfo)
    {
        return tenantInfo.RegisteredClientsList.Contains(clientInfo.ToUpper());
    }

    /// <summary>
    /// Get Rowkey
    /// </summary>
    /// <param name="tenantInfo"></param>
    /// <param name="documentNumber"></param>
    /// <param name="fiscalYear"></param>
    /// <returns></returns>
    public static string GetRowKey(ApprovalTenantInfo tenantInfo, string documentNumber, string fiscalYear)
    {
        var approvalRequestExpression = new ApprovalRequestExpressionV1()
        {
            DocumentTypeId = new Guid(tenantInfo.DocTypeId),
            ApprovalIdentifier = new ApprovalIdentifier() { DocumentNumber = documentNumber, FiscalYear = fiscalYear },
        };
        return approvalRequestExpression.ApprovalIdentifier.ToAzureTableRowKey(tenantInfo);
    }

    /// <summary>
    /// To Azure Table Rowkey
    /// </summary>
    /// <param name="approvalIdentifier"></param>
    /// <param name="tenantInfo"></param>
    /// <returns></returns>
    public static string ToAzureTableRowKey(this ApprovalIdentifier approvalIdentifier, ApprovalTenantInfo tenantInfo)
    {
        string documentNumber;
        if (tenantInfo.UseDocumentNumberForRowKey)
            documentNumber = approvalIdentifier.DocumentNumber;
        else
            documentNumber = approvalIdentifier.DisplayDocumentNumber;
        string returnValue = string.Format(Constants.AzureTableRowKeyStandardPrefix, tenantInfo.DocTypeId.ToString()) + "DocumentNumber~" + documentNumber;
        if (!string.IsNullOrEmpty(approvalIdentifier.FiscalYear))
            returnValue += "^FiscalYear~" + approvalIdentifier.FiscalYear;
        return returnValue;
    }

    /// <summary>
    /// Extracts the value from json.
    /// </summary>
    /// <param name="jObject">The j object.</param>
    /// <param name="parameterName">Name of the parameter.</param>
    /// <returns>returns string</returns>
    public static string ExtractValueFromJSON(JObject jObject, string parameterName)
    {
        var value = string.Empty;

        var parameters = parameterName.Split('.');

        foreach (var parameter in parameters.Take(parameters.Count() - 1))
        {
            if (jObject[parameter] != null && jObject[parameter].Type != JTokenType.Null && !String.IsNullOrEmpty(jObject[parameter].ToString()))
            {
                jObject = (JObject)jObject[parameter];
            }
            else
                break;
        }
        if (parameters.LastOrDefault() != null && jObject[parameters.LastOrDefault()] != null && jObject[parameters.LastOrDefault()].Type != JTokenType.Null)
        {
            value = jObject[parameters.LastOrDefault()].ToString();
        }
        return value;
    }
}