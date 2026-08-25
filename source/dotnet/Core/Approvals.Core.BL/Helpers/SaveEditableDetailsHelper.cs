// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace Microsoft.CFS.Approvals.Core.BL.Helpers;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Microsoft.Azure.Amqp.Framing;
using Microsoft.CFS.Approvals.Common.DL.Interface;
using Microsoft.CFS.Approvals.Contracts;
using Microsoft.CFS.Approvals.Contracts.DataContracts;
using Microsoft.CFS.Approvals.Core.BL.Interface;
using Microsoft.CFS.Approvals.Data.Azure.Storage.Interface;
using Microsoft.CFS.Approvals.Extensions;
using Microsoft.CFS.Approvals.Model;
using Microsoft.CFS.Approvals.Utilities.Extension;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

/// <summary>
/// The Save Editable Details Helper class
/// </summary>
public class SaveEditableDetailsHelper : ISaveEditableDetailsHelper
{
    /// <summary>
    /// The approval detail provider
    /// </summary>
    private readonly IApprovalDetailProvider _approvalDetailProvider;

    /// <summary>
    /// The approval tenantInfo helper
    /// </summary>
    private readonly IApprovalTenantInfoHelper _approvalTenantInfoHelper;

    /// <summary>
    /// The table helper
    /// </summary>
    private readonly ITableHelper _tableHelper;

    private readonly IDelegationHelper _delegationHelper;

    /// <summary>
    /// Constructor of SaveEditableDetailsHelper
    /// </summary>
    /// <param name="approvalDetailProvider"></param>
    /// <param name="approvalTenantInfoHelper"></param>
    /// <param name="tableHelper"></param>
    public SaveEditableDetailsHelper(
        IApprovalDetailProvider approvalDetailProvider,
        IApprovalTenantInfoHelper approvalTenantInfoHelper,
        ITableHelper tableHelper,
        IDelegationHelper delegationHelper)
    {
        _approvalDetailProvider = approvalDetailProvider;
        _approvalTenantInfoHelper = approvalTenantInfoHelper;
        _tableHelper = tableHelper;
        _delegationHelper = delegationHelper;
    }

    /// <summary>
    /// Method to check whether to enable Edit Details functionality for given user and tenant
    /// </summary>
    /// <param name="signedInUser"></param>
    /// <param name="onBehalfUser"></param>
    /// <param name="oauth2UserToken"></param>
    /// <param name="host"></param>
    /// <param name="Xcv"></param>
    /// <param name="Tcv"></param>
    /// <param name="tenantId"></param>
    /// <param name="documentNumber"></param>
    /// <param name="currentApproverEntity"></param>
    /// <returns></returns>
    public async Task<bool> CheckUserAuthorizationForEdit(User signedInUser, User onBehalfUser, string oauth2UserToken, string host, string Xcv, string Tcv, int tenantId, string documentNumber, ApprovalDetailsEntity currentApproverEntity = null)
    {
        await _delegationHelper.CheckUserAuthorization(signedInUser, onBehalfUser, oauth2UserToken, host, Tcv, Xcv, Tcv);
        ApprovalTenantInfo tenantInfo = _approvalTenantInfoHelper.GetTenantInfo(tenantId);
        if(currentApproverEntity == null)
        {
            currentApproverEntity = _approvalDetailProvider.GetApprovalsDetails(tenantId, documentNumber, Constants.CurrentApprover, tenantInfo?.DocTypeId);
        }
        bool editEnabledForCurrentUser = false;
        if (currentApproverEntity != null)
        {
            var approvers = JsonConvert.DeserializeObject<List<Approver>>(currentApproverEntity.JSONData);
            foreach (var approver in approvers)
            {
                if (approver.Alias == onBehalfUser.MailNickname)
                {
                    if (approver.CanEdit == true)
                    {
                        editEnabledForCurrentUser = true;
                        break;
                    }
                }
            }
        }

        return (tenantInfo.IsDetailsEditable && editEnabledForCurrentUser);
    }

    /// <summary>
    /// Method to save the edited details into ApprovalDetails table
    /// </summary>
    /// <param name="signedInUser">logged in user</param>
    /// <param name="onBehalfUser">on-behalf user</param>
    /// <param name="oauth2UserToken">oauth2 user token</param>
    /// <param name="clientDevice">client device</param>
    /// <param name="detailsString">details string</param>
    /// <param name="tenantId">teanat</param>
    /// <param name="sessionId">session id</param>
    /// <param name="Xcv">cross corelational vector</param>
    /// <param name="Tcv">transactional vector</param>
    /// <returns></returns>
    public async Task<List<string>> SaveEditedDetails(User signedInUser, User onBehalfUser, string oauth2UserToken, string clientDevice, string detailsString, int tenantId, string sessionId, string Xcv, string Tcv)
    {
        var detailsDataObj = detailsString.ToJToken();
        ApprovalTenantInfo tenantInfo = _approvalTenantInfoHelper.GetTenantInfo(tenantId);
        ApprovalIdentifier approvalIdentifier = detailsDataObj["ApprovalIdentifier"].ToString().FromJson<ApprovalIdentifier>();
        // Check to assure usage of Document Number for RowKey
        string documentNumber = approvalIdentifier.GetDocNumber(tenantInfo);
        
        var allApprovalDetails = await _approvalDetailProvider.GetAllApprovalDetailsByTenantAndDocumentNumber(tenantInfo.TenantId, documentNumber);
        var currentApproverEntity = allApprovalDetails != null && allApprovalDetails.Count > 0
                                    ? allApprovalDetails?.FirstOrDefault(detail =>
                                        detail.RowKey.Equals(Constants.CurrentApprover, StringComparison.InvariantCultureIgnoreCase) ||
                                        detail.RowKey.Equals(Constants.CurrentApprover + "|" + tenantInfo.DocTypeId, StringComparison.InvariantCultureIgnoreCase))
                                    : null;

        bool isAuthorized = await CheckUserAuthorizationForEdit(signedInUser, onBehalfUser, oauth2UserToken, clientDevice, Xcv, Tcv, tenantId, documentNumber, currentApproverEntity);
        if (!isAuthorized)
        {
            throw new UnauthorizedAccessException($"User {onBehalfUser.MailNickname} is not authorized to edit details for document {documentNumber}. Either tenant does not allow edits, user is not a current approver, or CanEdit flag is false.");
        }

        #region Get jsonSchema for the document from ApprovalDetails table

        JToken jsonSchemaObj = null;
        foreach (var detail in allApprovalDetails)
        {
            var detailsToken = JToken.Parse(detail.JSONData);
            if (detailsToken is JObject)
            {
                var detailsObj = JObject.Parse(detail.JSONData);
                if (detailsObj["jsonSchema"] != null)
                {
                    jsonSchemaObj = detailsObj["jsonSchema"].ToJToken();
                    break;
                }
                else if(detailsObj["LineItems"] != null)
                {
                    var lineItems = JArray.Parse(detailsObj["LineItems"].ToString());
                    foreach (JObject lineItem in lineItems)
                    {
                        if (lineItem["jsonSchema"] != null)
                        {
                            jsonSchemaObj = lineItem["jsonSchema"].ToJToken();
                            break;
                        }
                    }
                }
            }
            else if(detailsToken is JArray)
            {
                var detailsArray = JArray.Parse(detail.JSONData);
                foreach (JObject detailObj in detailsArray)
                {
                    if (detailObj["jsonSchema"] != null)
                    {
                        jsonSchemaObj = detailObj["jsonSchema"];
                        break;
                    }
                }
            }
        }

        detailsDataObj["jsonSchema"] = detailsDataObj["jsonSchema"] != null && jsonSchemaObj != null ? jsonSchemaObj : detailsDataObj["jsonSchema"];
        if (detailsDataObj["LineItems"] != null)
        {
            foreach (JObject lineItem in detailsDataObj["LineItems"])
            {
                if(lineItem["jsonSchema"] != null && jsonSchemaObj != null)
                {
                    lineItem["jsonSchema"] = jsonSchemaObj;
                }
            }
        }

        #endregion Get jsonSchema for the document from ApprovalDetails table

        #region Validate the edited fields

        bool isValid = false;
        string propertyEditable = "EditableField";
        JArray editableFields = new JArray();
        AddEditableFieldsToRequest(detailsDataObj, propertyEditable, ref editableFields);
        if (detailsDataObj["LineItems"] != null)
        {
            AddEditableFieldsToRequest(detailsDataObj["LineItems"], propertyEditable, ref editableFields);
        }
        List<string> validationFailMessages = ValidateEditableFields(editableFields);
        if (validationFailMessages.Count == 0)
            isValid = true;

        #endregion Validate the edited fields

        if (isValid)
        {
            // Get all approval details data and check if it has row key = EditedDetails|MailNickname or EditedDetails|MailNickname|doctypeid
            var editedDetailsOperationRowFromDetails = allApprovalDetails?.FirstOrDefault(detail =>
                detail.RowKey.Equals(Constants.EditedDetailsOperationType + "|" + onBehalfUser.MailNickname, StringComparison.InvariantCultureIgnoreCase) ||
                detail.RowKey.Equals(Constants.EditedDetailsOperationType + "|" + onBehalfUser.MailNickname + "|" + tenantInfo.DocTypeId, StringComparison.InvariantCultureIgnoreCase));

            ApprovalDetailsEntity approvalDetails = new ApprovalDetailsEntity()
            {
                PartitionKey = documentNumber,
                RowKey = editedDetailsOperationRowFromDetails?.RowKey ?? Constants.EditedDetailsOperationType + '|' + onBehalfUser.MailNickname + '|' + tenantInfo.DocTypeId,
                JSONData = JsonConvert.SerializeObject(detailsDataObj),
                TenantID = tenantId
            };
            ApprovalsTelemetry telemetry = new ApprovalsTelemetry()
            {
                Xcv = Xcv,
                Tcv = Tcv,
                BusinessProcessName = string.Format(tenantInfo.BusinessProcessName, Constants.BusinessProcessNameSaveEditedDetails, Constants.BusinessProcessNameUserTriggered)
            };

            await _approvalDetailProvider.SaveEditedDataInApprovalDetails(approvalDetails, telemetry);

            #region Save Audit trail for editable field

            var editableFieldJSON = GetEditableFieldJSON(editableFields);
            EditableFieldAuditEntity auditEntity = new EditableFieldAuditEntity()
            {
                ClientType = Constants.WebClient,
                EditableFieldJSON = JsonConvert.SerializeObject(editableFieldJSON),
                EditorAlias = onBehalfUser.MailNickname,
                PartitionKey = documentNumber,
                RowKey = Guid.NewGuid().ToString(),
                LoggedInUser = signedInUser.MailNickname
            };

            await _tableHelper.InsertOrReplace(Constants.EditableFieldAuditLogs, auditEntity);

            #endregion Save Audit trail for editable field
        }
        return validationFailMessages;
    }

    /// <summary>
    /// Get editable field JSON
    /// </summary>
    /// <param name="editableFields">JArray for editable fields for forming JSON we need to store</param>
    /// <returns></returns>
    private JArray GetEditableFieldJSON(JArray editableFields)
    {
        JArray editableFieldsArray = new JArray();
        foreach (var editedField in editableFields)
        {
            JObject objEditable = new JObject
            {
                { Constants.Id, editedField[Constants.Id] },
                { Constants.Fields, JObject.Parse(editedField[Constants.Fields].ToString()) }
            };
            editableFieldsArray.Add(objEditable);
        }

        return editableFieldsArray;
    }

    /// <summary>
    /// Validating the edited fields against the jsonSchema
    /// Validation in place for data types: string, boolean, integer, number
    /// Validation yields results for "required" property
    /// </summary>
    /// <param name="editableFields"></param>
    /// <returns></returns>
    private List<string> ValidateEditableFields(JArray editableFields)
    {
        List<string> validationFailMessages = new List<string>();
        // Regex timeout to prevent ReDoS attacks (CWE-1333, CWE-400)
        TimeSpan regexTimeout = TimeSpan.FromMilliseconds(200);

        foreach (var editedField in editableFields)
        {
            var jsonSchema = JObject.Parse(editedField["jsonSchema"].ToString());
            var editableField = JObject.Parse(editedField["Fields"].ToString());
            var keyNames = editableField.Properties().Select(p => p.Name).ToList();
            foreach (var key in keyNames)
            {
                var dynamicProperty = "DynamicSectionEditable" + key;
                var properties = JObject.Parse(jsonSchema[dynamicProperty]["properties"][key].ToString());
                string dataType = properties["type"].ToString();

                var tokenNewValue = editableField[key]["NewValue"][key];
                string newValue = string.Empty;
                if (tokenNewValue == null || string.IsNullOrEmpty(tokenNewValue.ToString()))
                    newValue = string.Empty;
                else
                    newValue = tokenNewValue.ToString();

                var tokenRequired = properties["required"];
                if (tokenRequired != null && !string.IsNullOrEmpty(tokenRequired.ToString()))
                {
                    if (bool.Parse(tokenRequired.ToString()) && string.IsNullOrEmpty(newValue))
                        validationFailMessages.Add(key + " field must have value");
                }

                if (string.IsNullOrEmpty(newValue))
                    break;

                switch (dataType)
                {
                    case "string":
                        var tokenStrMin = properties["minLength"];
                        var tokenStrMax = properties["maxLength"];
                        if (tokenStrMin != null && !string.IsNullOrEmpty(tokenStrMin.ToString()) && tokenStrMax != null && !string.IsNullOrEmpty(tokenStrMax.ToString()))
                        {
                            int minLength = int.Parse(tokenStrMin.ToString());
                            int maxLength = int.Parse(tokenStrMax.ToString());
                            if (newValue.Length < minLength || newValue.Length > maxLength)
                                validationFailMessages.Add("Number of characters in " + key + " field value must be within range " + minLength + " and " + maxLength);
                        }
                        var tokenPattern = properties["pattern"];
                        if (tokenPattern != null && !string.IsNullOrEmpty(tokenPattern.ToString()))
                        {
                            try
                            {
                                Regex regEx = new Regex(tokenPattern.ToString(), RegexOptions.None, regexTimeout);
                            if (!regEx.IsMatch(newValue))
                                validationFailMessages.Add(key + " field value must match pattern: " + tokenPattern.ToString());
                            }
                            catch (RegexMatchTimeoutException)
                            {
                                // Pattern took too long - likely a catastrophic backtracking attack
                                validationFailMessages.Add(key + " field validation pattern is too complex or input is too long.");
                            }
                            catch (ArgumentException ex)
                            {
                                // Invalid regex pattern from client
                                validationFailMessages.Add(key + " field validation pattern is invalid: " + ex.Message);
                            }
                        }
                        break;

                    case "integer":
                        int resultInt;
                        if (!int.TryParse(newValue, out resultInt))
                            validationFailMessages.Add(key + " field value must be an integer.");
                        break;

                    case "number":
                        float resultFloat;
                        if (!float.TryParse(newValue, out resultFloat))
                            validationFailMessages.Add(key + " field value must be an number.");
                        else
                        {
                            var token = properties["multipleOf"];
                            if (token != null && !string.IsNullOrEmpty(token.ToString()))
                            {
                                if (token != null && string.IsNullOrEmpty(token.ToString()))
                                {
                                    var multipleOf = float.Parse(properties["multipleOf"].ToString());
                                    var isMultiple = int.TryParse((resultFloat / multipleOf).ToString(), out int resultDivision);
                                    if (!isMultiple)
                                        validationFailMessages.Add(key + " field value must be a multiple of " + multipleOf);
                                }
                            }
                            var tokenMin = properties["minimum"];
                            var tokenMax = properties["maximum"];
                            if (tokenMin != null && !string.IsNullOrEmpty(tokenMin.ToString()) && tokenMax != null && !string.IsNullOrEmpty(tokenMax.ToString()))
                            {
                                bool isMaxExclusive = false;
                                var tokenExclusiveMax = properties["exclusiveMaximum"];
                                if (tokenExclusiveMax != null && !string.IsNullOrEmpty(tokenExclusiveMax.ToString()))
                                    isMaxExclusive = bool.Parse(tokenExclusiveMax.ToString());
                                if (isMaxExclusive)
                                {
                                    if (resultFloat < float.Parse(tokenMin.ToString()) || resultFloat >= float.Parse(tokenMax.ToString()))
                                        validationFailMessages.Add(key + " field value must be within range " + tokenMin.ToString() + " and " + tokenMax.ToString() + " (exclusive)");
                                }
                                else
                                {
                                    if (resultFloat < float.Parse(tokenMin.ToString()) || resultFloat > float.Parse(tokenMax.ToString()))
                                        validationFailMessages.Add(key + " field value must be within range " + tokenMin.ToString() + " and " + tokenMax.ToString());
                                }
                            }
                        }
                        break;

                    case "object": break;
                    case "array": break;
                    case "boolean":
                        bool resultBool;
                        if (!bool.TryParse(newValue, out resultBool))
                            validationFailMessages.Add(key + "field value must be a bool value.");
                        break;

                    default: break;
                }
            }
        }
        return validationFailMessages;
    }

    /// <summary>
    /// Adding the editable fields and corresponding jsonSchema into a JArray
    /// </summary>
    /// <param name="source"></param>
    /// <param name="property"></param>
    /// <param name="target"></param>
    private void AddEditableFieldsToRequest(JToken source, string property, ref JArray target)
    {
        if (target == null)
            target = new JArray();
        if (source is JObject)
        {
            var requestDetails = JObject.Parse(source.ToString());
            if (requestDetails[property] != null)
            {
                var detailsProperty = JObject.Parse(requestDetails[property].ToString());
                detailsProperty.Add("jsonSchema", requestDetails["jsonSchema"]);
                target.Add(detailsProperty);
            }
        }
        else if (source is JArray)
        {
            var requestDetails = JArray.Parse(source.ToString());
            foreach (JObject detail in requestDetails)
            {
                if (detail[property] != null)
                {
                    var detailsProperty = JObject.Parse(detail[property].ToString());
                    detailsProperty.Add("jsonSchema", detail["jsonSchema"]);
                    target.Add(detailsProperty);
                }
                if (detail["Children"] != null && !string.IsNullOrEmpty(detail["Children"].ToString()))
                {
                    AddEditableFieldsToRequest(detail["Children"], property, ref target);
                }
            }
        }
    }
}