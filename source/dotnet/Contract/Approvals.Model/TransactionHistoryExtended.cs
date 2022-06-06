// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace Microsoft.CFS.Approvals.Model
{
    using System;
    using System.Collections.Generic;
    using System.Globalization;
    using System.Text;
    using Microsoft.CFS.Approvals.Contracts;
    using Microsoft.CFS.Approvals.Contracts.DataContracts;
    using Microsoft.CFS.Approvals.Extensions;
    using Newtonsoft.Json;

    public class TransactionHistoryExtended : TransactionHistory
    {
        public string CondensedAppName { get; set; }
        public string TemplateName { get; set; }
        public bool IsHistoryClickable { get; set; }

        //public string Xcv { get; set; }
        public string BusinessProcessName { get; set; }

        private static void ConvertJsonToDictionary(Dictionary<string, string> placeHolderDict, string summaryData, string dataplaceHolder = "")
        {
            Dictionary<string, object> values = summaryData.FromJson<Dictionary<string, object>>();
            if (values != null)
            {
                foreach (KeyValuePair<string, object> data in values)
                {
                    string placeHolder;
                    placeHolder = String.IsNullOrEmpty(dataplaceHolder) ? data.Key : dataplaceHolder + "." + data.Key;

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
                            placeHolderDict.Add(placeHolder, Convert.ToString(data.Value));
                        }
                    }
                    else
                    {
                        placeHolderDict.Add(placeHolder, null);
                    }
                }
            }
        }

        public static TransactionHistory CreateHistoryData(ApprovalSummaryRow approvalSummary,
           ApprovalRequestExpression approvalRequestExpression, ApprovalTenantInfo tenantInfo, string messageId)
        {
            TransactionHistory historyData = null;
            if (approvalRequestExpression.ActionDetail != null)
            {
                var requestDocTypeId = approvalRequestExpression.DocumentTypeId.ToString();

                var placeHolderDict = new Dictionary<string, string>();

                //TODO::This assembly refers to NewtonSoft Nuget just for this method. Check if this method is correctly placed or if this conversion can be delegated elsewhere
                ConvertJsonToDictionary(placeHolderDict, approvalSummary.SummaryJson);
                var approvalsNote = "{}";
                if (approvalRequestExpression.ActionDetail != null && approvalRequestExpression.ActionDetail.AdditionalData != null)
                {
                    approvalsNote = (approvalRequestExpression.ActionDetail.AdditionalData).ToJson();
                }
                var approvalNoteObject = (approvalsNote).ToJObject();
                if (approvalNoteObject["Comment"] == null && approvalRequestExpression.ActionDetail != null)
                    approvalNoteObject["Comment"] = approvalRequestExpression.ActionDetail.Comment;
                if (approvalNoteObject["Placement"] == null && approvalRequestExpression.ActionDetail != null)
                    approvalNoteObject["Placement"] = approvalRequestExpression.ActionDetail.Placement;

                approvalsNote = approvalNoteObject.ToString();

                var actionByAlias = (IsActionExempt(approvalRequestExpression.ActionDetail.Name)
                                     || string.IsNullOrEmpty(approvalRequestExpression.ActionDetail.ActionBy?.Alias))
                    ? string.Empty
                    : approvalRequestExpression.ActionDetail.ActionBy?.Alias;

                string actionByDelegateInMSA = string.Empty;
                if (approvalRequestExpression.ActionDetail != null && approvalRequestExpression.ActionDetail.AdditionalData != null)
                {
                    approvalRequestExpression.ActionDetail.AdditionalData.TryGetValue("ActionByDelegateInMSApprovals", out actionByDelegateInMSA);
                }

                actionByDelegateInMSA = (string.IsNullOrWhiteSpace(actionByDelegateInMSA) || actionByAlias.Trim().ToLower() == actionByDelegateInMSA.Trim().ToLower())
                    ? string.Empty
                    : actionByDelegateInMSA.Trim().ToLower();

                var documentNumber = (tenantInfo.UseDocumentNumberForRowKey) ? "ApprovalIdentifier.DocumentNumber" : "ApprovalIdentifier.DisplayDocumentNumber";

                var summary = approvalSummary.SummaryJson.FromJson<SummaryJson>(
                    new JsonSerializerSettings
                    {
                        NullValueHandling = NullValueHandling.Ignore
                    });
                summary.AdditionalData = null;

                historyData = new TransactionHistory()
                {
                    PartitionKey = GetDocNumber(approvalRequestExpression.ApprovalIdentifier, tenantInfo),
                    RowKey = Guid.NewGuid().ToString(),
                    Title = GetValueFromSummaryDictionary(placeHolderDict, "Title"),
                    Approver = actionByAlias,
                    UnitValue = placeHolderDict.TryGetValue("UnitValue", out string value) ? value : "0",
                    AmountUnits = GetValueFromSummaryDictionary(placeHolderDict, "UnitOfMeasure"),
                    SubmittedDate = placeHolderDict.TryGetValue("SubmittedDate", out value)
                        ? (DateTime.TryParse(value, out DateTime dateValue) ? dateValue : (DateTime?)null) : (DateTime?)null,
                    SubmitterName = GetValueFromSummaryDictionary(placeHolderDict, "Submitter.Name"),
                    SubmittedAlias = GetValueFromSummaryDictionary(placeHolderDict, "Submitter.Alias"),
                    FiscalYear = GetValueFromSummaryDictionary(placeHolderDict, "ApprovalIdentifier.FiscalYear"),
                    TenantUrl = GetValueFromSummaryDictionary(placeHolderDict, "DetailPageURL"),
                    ActionDate = (!approvalRequestExpression.ActionDetail.Date.Equals(DateTime.MinValue))
                        ? approvalRequestExpression.ActionDetail.Date : DateTime.UtcNow,
                    ActionTaken = !string.IsNullOrWhiteSpace(approvalRequestExpression.ActionDetail.Name)
                        ? approvalRequestExpression.ActionDetail.Name : "None",
                    DocumentNumber = GetValueFromSummaryDictionary(placeHolderDict, documentNumber),
                    JsonData = summary.ToJson(),
                    TenantId = tenantInfo.TenantId.ToString(CultureInfo.InvariantCulture),
                    DocumentTypeID = requestDocTypeId,
                    ApproversNote = approvalsNote,
                    VendorInvoiceNumber = "",
                    CompanyCode = GetValueFromSummaryDictionary(placeHolderDict, "CompanyCode"),
                    CustomAttribute = GetValueFromSummaryDictionary(placeHolderDict, "CustomAttribute.CustomAttributeValue"),
                    AppName = approvalSummary.Application,
                    ActionTakenOnClient = approvalSummary.ActionTakenOnClient ?? "None",
                    MessageId = messageId,
                    DelegateUser = actionByDelegateInMSA,
                    Xcv = approvalSummary.Xcv
                };

                if (approvalRequestExpression.DocumentTypeId.ToString().ToLowerInvariant() == Constants.InvoiceDocumentTypeId.ToString().ToLowerInvariant())
                {
                    historyData.VendorInvoiceNumber = GetValueFromSummaryDictionary(placeHolderDict, "CustomAttribute.CustomAttributeValue");
                }
            }
            return historyData;
        }

        private static string GetValueFromSummaryDictionary(Dictionary<string, string> placeholderDictionary, string key)
        {
            if (!placeholderDictionary.TryGetValue(key, out string value)) return "";
            return !string.IsNullOrEmpty(value) ? value : "";
        }

        private static string GetDocNumber(ApprovalIdentifier approvalIdentifier, ApprovalTenantInfo tenantInfo)
        {
            if (tenantInfo.UseDocumentNumberForRowKey)
            {
                return approvalIdentifier.DocumentNumber;
            }
            else
            {
                return approvalIdentifier.DisplayDocumentNumber;
            }
        }

        /// <summary>
        /// Check if action is exempt.
        /// </summary>
        /// <param name="actionName"></param>
        /// <returns></returns>
        public static bool IsActionExempt(string actionName)
        {
            var exemptActions = new List<string> { "SYSTEM CANCEL", "SYSTEM SEND BACK", "TAKEBACK", "CANCEL", "RESUBMITTED" };
            return exemptActions.Contains(actionName.ToUpper());
        }
    }
}