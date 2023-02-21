// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace Microsoft.CFS.Approvals.SyntheticTransaction.Helpers.Helpers;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using Microsoft.CFS.Approvals.Contracts.DataContracts;
using Microsoft.CFS.Approvals.Data.Azure.Storage.Interface;
using Microsoft.CFS.Approvals.DevTools.Model.Constant;
using Microsoft.CFS.Approvals.LogManager.Provider.Interface;
using Microsoft.CFS.Approvals.SyntheticTransaction.Common.Models;
using Microsoft.CFS.Approvals.SyntheticTransaction.Helpers.ExtensionMethods;
using Microsoft.CFS.Approvals.SyntheticTransaction.Helpers.Interface;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

/// <summary>
/// Bulk Delete Helper class
/// </summary>
public class BulkDeleteHelper : IBulkDeleteHelper
{
    /// <summary>
    /// The azure storage helepr
    /// </summary>
    private readonly ITableHelper _azureStorageHelper;

    /// <summary>
    /// The payload receiver helper
    /// </summary>
    private readonly IPayloadReceiverHelper _payloadReceiverHelper;

    /// <summary>
    /// The log provider
    /// </summary>
    private readonly ILogProvider _logProvider;

    private readonly string _environment;
    private readonly Dictionary<LogDataKey, object> logData = new Dictionary<LogDataKey, object>();

    public BulkDeleteHelper(
        Func<string, string, ITableHelper> azureStorageHelper,
        Func<string, string, IBlobStorageHelper> blobStorageHelper,
        IPayloadReceiverHelper payloadReceiverHelper,
        IActionContextAccessor actionContextAccessor,
        ConfigurationSetting configurationSetting,
        ILogProvider logProvider)
    {
        _environment = actionContextAccessor?.ActionContext?.RouteData?.Values["env"]?.ToString();
        _azureStorageHelper = azureStorageHelper(
           configurationSetting.appSettings[_environment].StorageAccountName,
           configurationSetting.appSettings[_environment].StorageAccountKey);
        _payloadReceiverHelper = payloadReceiverHelper;
        _logProvider = logProvider;
    }

    /// <summary>
    /// Bulk delete
    /// </summary>
    /// <param name="tenant"></param>
    /// <param name="approver"></param>
    /// <param name="days"></param>
    /// <param name="docNumber"></param>
    /// <param name="tcv"></param>
    /// <returns></returns>
    public async Task<object> BulkDelete(string tenant, string approver, string days, string docNumber, string tcv)
    {
        DateTime dateFilter;
        string successDocuments = string.Empty;
        string failureDocuments = string.Empty;
        string invalidDouments = string.Empty;
        int noOfDays = 0, deleteSuccess = 0;

        logData.Add(LogDataKey.Xcv, docNumber);
        logData.Add(LogDataKey.Tcv, tcv);
        logData.Add(LogDataKey.Environment, _environment);
        logData.Add(LogDataKey.TenantName, tenant);
        logData.Add(LogDataKey.UserAlias, approver);
        logData.Add(LogDataKey.DocumentNumber, docNumber);
        logData.Add(LogDataKey.DaysToDelete, days);

        List<TestHarnessDocument> documentList = new List<TestHarnessDocument>();
        try
        {
            documentList = _azureStorageHelper.GetTableEntityListByPartitionKey<TestHarnessDocument>("TestHarnessPayload", approver).Where(x => x.Status == DocumentStatus.Pending.ToString() && x.TenantID == tenant.Trim()).ToList();
            if (!string.IsNullOrEmpty(days) && Int32.TryParse(days, out noOfDays))
            {
                dateFilter = DateTime.UtcNow.AddDays(-noOfDays);
                documentList = documentList.Where(x => x.Timestamp < dateFilter).ToList();
            }
            else if (!string.IsNullOrWhiteSpace(docNumber))
            {
                var documnetnumber = docNumber.Split(",").Select(doc => doc.Trim()).ToArray();
                documentList = documentList.Where(x => documnetnumber.Contains<string>(x.RowKey.Split("|")[0])).ToList();
                invalidDouments = string.Join(',', documnetnumber.Except(documentList.Select(x => x.RowKey.Split("|")[0])));
                logData.Add(LogDataKey.InvalidDouments, invalidDouments);
            }
            else
            {
                dateFilter = DateTime.UtcNow;
                documentList = documentList.Where(x => x.Timestamp < dateFilter).ToList();
            }
            if (documentList.Count > 0)
            {
                foreach (var document in documentList)
                {
                    var result = await SendDeletePayload(document, approver, "Bulk Delete", "Delete", tcv);
                    var PayloadValidationResults = JsonConvert.DeserializeObject<JObject>(result.Content.ReadAsStringAsync().Result)?.SelectToken("PayloadProcessingResult")?.SelectToken("PayloadValidationResults")?.Value<JArray>();
                    if (result.IsSuccessStatusCode && PayloadValidationResults == null)
                    {
                        deleteSuccess--;
                        successDocuments = successDocuments + document.RowKey.Split('|')[0] + ", ";
                        document.Status = DocumentStatus.Approved.ToString();
                        await UpdateDocumentStatus(document);
                    }
                    else
                        failureDocuments = failureDocuments + document.RowKey.Split('|')[0] + ", ";
                }

                logData.Add(LogDataKey.SuccessDocuments, successDocuments);
                logData.Add(LogDataKey.FailureDocuments, failureDocuments);
                _logProvider.LogInformation(TrackingEvent.BulkDeleteCompleted, logData);

                return new { bulkSuccessDocuments = successDocuments, bulkFailureDocuments = failureDocuments };
            }
            else
            {
                logData.Add(LogDataKey.SuccessDocuments, "0");
                logData.Add(LogDataKey.FailureDocuments, "0");
                _logProvider.LogInformation(TrackingEvent.BulkDeleteCompleted, logData);
                return new { bulkSuccessDocuments = "No pending records found for Approver!!" };
            }
        }
        catch (Exception ex)
        {
            logData.Add(LogDataKey.EventName, "ErrorFetchingDeleteRequests");
            logData.Add(LogDataKey.Operation, "Error while fetching Delete requests");
            _logProvider.LogError(TrackingEvent.ErrorFetchingDeleteRequests, ex, logData);
            return new { bulkSuccessDocuments = "Error while bulk deleting requests!" };
        }
    }

    /// <summary>
    /// Send delete payload
    /// </summary>
    /// <param name="documnet"></param>
    /// <param name="approver"></param>
    /// <param name="comment"></param>
    /// <param name="action"></param>
    /// <param name="tcv"></param>
    /// <returns></returns>
    public async Task<HttpResponseMessage> SendDeletePayload(TestHarnessDocument document, string approver, string comment, string action, string tcv)
    {
        JObject documentJObject = JsonConvert.DeserializeObject<JObject>(document.Payload);
        documentJObject["Operation"] = (int)PayLoadOperation.Delete;

        if (!documentJObject.ContainsKey("DeleteFor"))
        {
            documentJObject.Add("DeleteFor");
        }

        documentJObject["DeleteFor"] = new JArray { approver };

        var actionDetail = documentJObject?.SelectToken("ActionDetail")?.Value<JObject>();

        if (actionDetail != null)
        {
            actionDetail["Name"] = action;
            actionDetail["Date"] = DateTime.UtcNow.ToString();
            actionDetail["Comment"] = comment;
            var actionBy = new JObject
            {
                { "Alias", approver }
            };
            actionDetail["ActionBy"] = actionBy;
        }
        else
        {
            ActionDetail actionDetail1 = new ActionDetail
            {
                Name = action,
                Date = DateTime.UtcNow,
                Comment = comment,
                ActionBy = new NameAliasEntity
                {
                    Alias = approver
                }
            };
            documentJObject["ActionDetail"] = JObject.FromObject(actionDetail1);
        }
        try
        {
            _logProvider.LogInformation(TrackingEvent.SendPayloadStarted, logData);
            var result = _payloadReceiverHelper.SendPayload(documentJObject.ToString(), tcv).Result;
            logData.AddUpdate(LogDataKey.PayloadResult, result.ToString());
            _logProvider.LogInformation(TrackingEvent.SendPayloadCompleted, logData);
            return result;
        }
        catch (Exception ex)
        {
            logData.Add(LogDataKey.EventName, "SendDeletePayloadFailure");
            logData.Add(LogDataKey.Operation, "Failed to send Delete payloads");
            _logProvider.LogError(TrackingEvent.SendDeletePayloadFailure, ex, logData);
            return null;
        }
    }

    /// <summary>
    /// Update document  status
    /// </summary>
    /// <param name="document"></param>
    /// <returns></returns>
    public async Task<bool> UpdateDocumentStatus(TestHarnessDocument document)
    {
        try
        {
            var result = await _azureStorageHelper.InsertOrReplace<TestHarnessDocument>("TestHarnessPayload", document);
            _logProvider.LogInformation(TrackingEvent.DocumentStatusUpdateCompleted, logData);
            return result;
        }
        catch (Exception ex)
        {
            logData.Add(LogDataKey.EventName, "DocumentStatusUpdateFailure");
            logData.Add(LogDataKey.Operation, "Failed to update Document status");
            _logProvider.LogError(TrackingEvent.DocumentStatusUpdateFailure, ex, logData);
            return false;
        }
    }
}