// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace Microsoft.CFS.Approvals.Common.DL;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using global::Azure.Messaging.ServiceBus;
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
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

/// <summary>
/// The Approval Summary Provider class
/// </summary>
public class ApprovalSummaryProvider : IApprovalSummaryProvider
{
    /// <summary>
    /// The configuration
    /// </summary>
    private readonly IConfiguration _config;

    /// <summary>
    /// The log provider
    /// </summary>
    private readonly ILogProvider _logProvider;

    /// <summary>
    /// The table helper
    /// </summary>
    private readonly ITableHelper _tableHelper;

    /// <summary>
    /// The performance logger
    /// </summary>
    private readonly IPerformanceLogger _performanceLogger;

    /// <summary>
    /// The approval detail provider
    /// </summary>
    private readonly IApprovalDetailProvider _approvalDetailProvider;

    /// <summary>
    /// The approval tenant provider
    /// </summary>
    private readonly IApprovalTenantInfoProvider _approvalTenantInfoProvider;

    /// <summary>
    /// The name resolution helper
    /// </summary>
    private readonly INameResolutionHelper _nameResolutionHelper;

    /// <summary>
    /// The approval blob data provider
    /// </summary>
    private readonly IApprovalBlobDataProvider _approvalBlobDataProvider = null;

    /// <summary>
    /// Constructor of ApprovalSummaryProvider
    /// </summary>
    /// <param name="config"></param>
    /// <param name="logProvider"></param>
    /// <param name="tableHelper"></param>
    /// <param name="performanceLogger"></param>
    /// <param name="approvalDetailProvider"></param>
    /// <param name="approvalTenantInfoProvider"></param>
    /// <param name="nameResolutionHelper"></param>
    /// <param name="approvalBlobDataProvider"></param>
    public ApprovalSummaryProvider(
        IConfiguration config,
        ILogProvider logProvider,
        ITableHelper tableHelper,
        IPerformanceLogger performanceLogger,
        IApprovalDetailProvider approvalDetailProvider,
        IApprovalTenantInfoProvider approvalTenantInfoProvider,
        INameResolutionHelper nameResolutionHelper,
        IApprovalBlobDataProvider approvalBlobDataProvider
        )
    {
        _config = config;
        _logProvider = logProvider;
        _tableHelper = tableHelper;
        _performanceLogger = performanceLogger;
        _approvalDetailProvider = approvalDetailProvider;
        _approvalTenantInfoProvider = approvalTenantInfoProvider;
        _nameResolutionHelper = nameResolutionHelper;
        _approvalBlobDataProvider = approvalBlobDataProvider;
    }

    /// <summary>
    /// Add approval summary
    /// </summary>
    /// <param name="tenant"></param>
    /// <param name="approvalRequest"></param>
    /// <param name="summaryRows"></param>
    /// <returns></returns>
    public async Task<bool> AddApprovalSummary(ApprovalTenantInfo tenant, ApprovalRequestExpression approvalRequest, List<ApprovalSummaryRow> summaryRows)
    {
        bool bReturn = true;
        if (summaryRows == null || summaryRows.Count == 0)
        {
            return bReturn;
        }
        var savechangesoptions = tenant.IsRaceConditionHandled ? SaveOptions.ReplaceOnUpdate : (bool.Parse(_config[ConfigurationKey.SaveChangesOptionsContinueOnError.ToString()]) ? SaveOptions.ContinueOnError : SaveOptions.ReplaceOnUpdate);

        foreach (ApprovalSummaryRow row in summaryRows)
        {
            var domain = string.IsNullOrWhiteSpace(row.ApproverDomain) ? (await _nameResolutionHelper.GetUserPrincipalName(row.Approver)).GetDomainFromUPN() : row.ApproverDomain;
            bool blobCheck = false;
            ApplyCaseConstraints(row);
            SetNextReminderTime(row, DateTime.UtcNow);
            ApprovalSummaryRow existingRow = GetApprovalSummaryByDocumentNumber(approvalRequest.DocumentTypeId.ToString(), row.DocumentNumber, row.Approver, row.PartitionKey, domain);

            var logData = new Dictionary<LogDataKey, object>()
            {
                { LogDataKey.BusinessProcessName, string.Format(tenant.BusinessProcessName, Constants.BusinessProcessNameSendPayload, approvalRequest.Operation) },
                { LogDataKey.Tcv, approvalRequest.Telemetry.Tcv },
                { LogDataKey.Xcv, approvalRequest.Telemetry.Xcv },
                { LogDataKey.DXcv, approvalRequest.ApprovalIdentifier.DisplayDocumentNumber },
                { LogDataKey.DocumentNumber, row.DocumentNumber },
                { LogDataKey.UserAlias, row.PartitionKey },
                { LogDataKey.ReceivedTcv, row.Tcv },
                { LogDataKey.TenantTelemetryData, approvalRequest.Telemetry.TenantTelemetry },
                { LogDataKey.TableOperation, savechangesoptions.ToString() }
            };

            #region Get the previous Transactional Details and push the new messages into the list

            // Get all details from ApprovalDetails table and filter to get only the row which has TransactionalDetails (LastfailedException message etc.)
            List<ApprovalDetailsEntity> transactionalDetails = await _approvalDetailProvider.GetAllApprovalDetailsByTenantAndDocumentNumber(tenant.TenantId, approvalRequest.ApprovalIdentifier.GetDocNumber(tenant));
            JObject previousExceptionsMessages = new JObject();
            ApprovalDetailsEntity previousExceptionsMessagesRow = null;
            if (transactionalDetails != null && transactionalDetails.Any())
            {
                // Filter to get only the row which has TransactionalDetails (LastfailedException message etc.)
                previousExceptionsMessagesRow = transactionalDetails
                                                    .FirstOrDefault(x => x.RowKey.Equals(string.Format(Constants.TransactionDetailsOperationTypeNew + '|' + row.Approver, tenant.DocTypeId), StringComparison.InvariantCultureIgnoreCase) ||
                                                                         x.RowKey.Equals(Constants.TransactionDetailsOperationType + '|' + row.Approver, StringComparison.InvariantCultureIgnoreCase));


                //Backward compatibility
                if (previousExceptionsMessagesRow == null && _config[Constants.OldWhitelistedDomains].Contains(domain, StringComparison.InvariantCultureIgnoreCase))
                {
                    var previousExceptionsMessagesRows = transactionalDetails
                                                        .Where(x => x.RowKey.Equals(string.Format(Constants.TransactionDetailsOperationTypeNew + '|' + row.Approver, tenant.DocTypeId), StringComparison.InvariantCultureIgnoreCase) ||
                                                                    x.RowKey.Equals(Constants.TransactionDetailsOperationType + '|' + row.Approver, StringComparison.InvariantCultureIgnoreCase));
                    previousExceptionsMessagesRow = previousExceptionsMessagesRows.FirstOrDefault(x => string.IsNullOrEmpty(x.ApproverDomain));
                }
                if (previousExceptionsMessagesRow != null)
                {
                    previousExceptionsMessages = previousExceptionsMessagesRow.JSONData.ToJObject();
                }
            }

            // if last failed exception message is present, then push the new message into the existing list, else create a new array with only one item
            if (approvalRequest.ActionDetail != null && !string.IsNullOrWhiteSpace(approvalRequest.ActionDetail.UserActionFailureReason))
            {
                if (previousExceptionsMessages["LastFailedExceptionMessage"] != null)
                {
                    JArray items = previousExceptionsMessages["LastFailedExceptionMessage"].ToString().ToJArray();
                    items.Add(approvalRequest.ActionDetail.UserActionFailureReason);
                    previousExceptionsMessages["LastFailedExceptionMessage"] = items;
                }
                else
                {
                    previousExceptionsMessages.Add("LastFailedExceptionMessage", new JArray() { approvalRequest.ActionDetail.UserActionFailureReason });
                }
            }
            if (previousExceptionsMessages.Count > 0)
            {
                ApprovalDetailsEntity approvalDetails = new ApprovalDetailsEntity()
                {
                    PartitionKey = approvalRequest?.ApprovalIdentifier?.GetDocNumber(tenant),
                    RowKey = previousExceptionsMessagesRow?.RowKey ?? string.Format(Constants.TransactionDetailsOperationTypeNew + '|' + row.PartitionKey, tenant.DocTypeId),
                    ETag = global::Azure.ETag.All,
                    JSONData = previousExceptionsMessages.ToString(),
                    TenantID = tenant.TenantId
                };
                // Insert the details into the ApprovalDetails table
                await _approvalDetailProvider.AddApprovalsDetails(new List<ApprovalDetailsEntity>() { approvalDetails }, tenant, Environment.UserName, approvalRequest.Telemetry.Xcv, approvalRequest.Telemetry.Tcv);
            }

            #endregion Get the previous Transactional Details and push the new messages into the list

            if (existingRow == null || DateTime.Compare(row.OperationDateTime, existingRow.OperationDateTime) > 0)
            {
                if (approvalRequest.ActionDetail != null && !string.IsNullOrEmpty(approvalRequest.ActionDetail.UserActionFailureReason))
                {
                    row.LastFailed = true;
                }
                if (existingRow != null && !string.IsNullOrEmpty(existingRow.NotificationJson) && !existingRow.NotificationJson.Equals("null", StringComparison.InvariantCultureIgnoreCase))
                {
                    var existingRowNotification = existingRow.NotificationJson.FromJson<NotificationDetail>(); ;
                    if (approvalRequest.NotificationDetail != null)
                    {
                        approvalRequest.NotificationDetail.To = existingRowNotification.To;
                        row.NotificationJson = (approvalRequest.NotificationDetail).ToJson();
                    }
                }
                using (var summaryRowInsertTracer = _performanceLogger.StartPerformanceLogger("PerfLog", "SummaryProvider", string.Format(Constants.PerfLogAction, "Summary Provider", "Add approval summary")
                        , new Dictionary<LogDataKey, object> { { LogDataKey.RowKey, row.RowKey }, { LogDataKey.PartitionKey, row.PartitionKey }, { LogDataKey.DocumentNumber, row.DocumentNumber } }))
                {
                    try
                    {
                        switch (savechangesoptions)
                        {
                            case SaveOptions.ContinueOnError:
                                await _tableHelper.Insert<ApprovalSummaryRow>(_config[ConfigurationKey.ApprovalSummaryAzureTableName.ToString()], row);
                                break;

                            case SaveOptions.ReplaceOnUpdate:
                                await _tableHelper.InsertOrReplace(_config[ConfigurationKey.ApprovalSummaryAzureTableName.ToString()], row);
                                break;
                        }
                    }
                    catch (global::Azure.RequestFailedException ex)
                    {
                        // Checks whether exception caused was due to large data.
                        if (ex.ErrorCode == "PropertyValueTooLarge" || ex.ErrorCode == "EntityTooLarge" ||
                            ex.ErrorCode == "RequestBodyTooLarge")
                        {
                            blobCheck = true;
                        }
                        else
                        {
                            throw;
                        }
                    }

                    #region Insertion in approvalsummaryblobdata

                    if (blobCheck == true)
                    {
                        var blobPointer = row.PartitionKey.ToString() + "|" + row.RowKey.ToString();
                        await _approvalBlobDataProvider.AddApprovalSummaryJson(row, blobPointer);
                        row.BlobPointer = blobPointer;
                        row.SummaryJson = string.Empty;

                        switch (savechangesoptions)
                        {
                            case SaveOptions.ContinueOnError:
                                await _tableHelper.Insert<ApprovalSummaryRow>(_config[ConfigurationKey.ApprovalSummaryAzureTableName.ToString()], row);
                                break;

                            case SaveOptions.ReplaceOnUpdate:
                                await _tableHelper.InsertOrReplace(_config[ConfigurationKey.ApprovalSummaryAzureTableName.ToString()], row);
                                break;
                        }
                    }

                    #endregion Insertion in approvalsummaryblobdata

                    _logProvider.LogInformation(TrackingEvent.SummaryInsertedOrReplaced, logData);
                }
            }
            else
            {
                if (approvalRequest.ActionDetail == null || string.IsNullOrEmpty(approvalRequest.ActionDetail.UserActionFailureReason))
                    continue;
                existingRow.LastFailed = true;
                await _tableHelper.InsertOrReplace<ApprovalSummaryRow>(_config[ConfigurationKey.ApprovalSummaryAzureTableName.ToString()], existingRow);
                _logProvider.LogInformation(TrackingEvent.SummaryInsertedOrReplaced, logData);
            }
        }

        ApprovalDetailsEntity detailCurrentApprover = new ApprovalDetailsEntity() { PartitionKey = approvalRequest?.ApprovalIdentifier?.GetDocNumber(tenant), RowKey = Constants.CurrentApprover + "|" + tenant.DocTypeId, JSONData = (approvalRequest.Approvers).ToJson(new JsonSerializerSettings { NullValueHandling = NullValueHandling.Ignore }), TenantID = tenant.TenantId };
        List<ApprovalDetailsEntity> detailRows = new List<ApprovalDetailsEntity>() { detailCurrentApprover };

        // Get all approval details data and check if it has row key = ADDNDTL or ADDNDTL|doctypeid
        var allApprovalDetails = await _approvalDetailProvider.GetAllApprovalDetailsByTenantAndDocumentNumber(tenant.TenantId, approvalRequest?.ApprovalIdentifier?.GetDocNumber(tenant));
        var additionalDataOperationRowFromDetails = allApprovalDetails?.FirstOrDefault(detail =>
            detail.RowKey.Equals(Constants.AdditionalDetails, StringComparison.InvariantCultureIgnoreCase) ||
            detail.RowKey.Equals(string.Format(Constants.AdditionalDetailsNew, tenant.DocTypeId), StringComparison.InvariantCultureIgnoreCase));


        // Adding Additional Details into table
        if (approvalRequest.DetailsData != null && approvalRequest.DetailsData.ContainsKey(Constants.AdditionalDetails))
        {
            ApprovalDetailsEntity additionalDetails = new ApprovalDetailsEntity()
            {
                PartitionKey = approvalRequest?.ApprovalIdentifier?.GetDocNumber(tenant),
                RowKey = additionalDataOperationRowFromDetails?.RowKey ?? string.Format(Constants.AdditionalDetailsNew, tenant.DocTypeId),
                JSONData = approvalRequest.DetailsData[Constants.AdditionalDetails],
                TenantID = tenant.TenantId
            };
            detailRows.Add(additionalDetails);
        }
        await _approvalDetailProvider.AddApprovalsDetails(detailRows, tenant, Environment.UserName, approvalRequest.Telemetry.Xcv, approvalRequest.Telemetry.Tcv);
        return bReturn;
    }

    /// <summary>
    /// Apply case constraints
    /// </summary>
    /// <param name="row"></param>
    public void ApplyCaseConstraints(ApprovalSummaryRow row)
    {
        row.PartitionKey = row.PartitionKey.ToLowerInvariant();
    }

    /// <summary>
    /// Get approval counts
    /// </summary>
    /// <param name="approver"></param>
    /// <returns></returns>
    public async Task<ApprovalCount[]> GetApprovalCounts(string approver)
    {
        using (var summaryRowInsertTracer = _performanceLogger.StartPerformanceLogger("PerfLog", "SummaryProvider", string.Format(Constants.PerfLogAction, "Summary Provider", "Get Approval Count")
                            , new Dictionary<LogDataKey, object> { { LogDataKey.UserAlias, approver } }))
        {
            string approverKey = approver.ToLowerInvariant();
            List<ApprovalTenantInfo> tenants = (await _approvalTenantInfoProvider.GetAllTenantInfo()).ToList();

            var jsonSummary = _tableHelper.GetDataCollectionByTableQuery<ApprovalSummaryRow>(_config[ConfigurationKey.ApprovalSummaryAzureTableName.ToString()],
                $"PartitionKey eq '{approverKey}' and LobPending eq {false}").Select(x => new ApprovalSummaryRow() { PartitionKey = x.PartitionKey, Approver = x.Approver, RowKey = x.RowKey });

            var allList = from r in jsonSummary
                          select new { DocumentTypeId = r.RowKey.Split('|')[0], r.Approver };

            var approvalCounts = allList.GroupBy(r => r.DocumentTypeId).Select(r => new ApprovalCount()
            {
                DocumentTypeId = r.Key,
                Count = r.Count(),
                AppName = tenants.FirstOrDefault(t => t.DocTypeId.Equals(r.Key, StringComparison.InvariantCultureIgnoreCase)).AppName
            }).ToArray();

            return approvalCounts;
        }
    }

    /// <summary>
    /// Get approval summary by document number
    /// </summary>
    /// <param name="documentTypeID"></param>
    /// <param name="documentNumber"></param>
    /// <param name="approverAlias"></param>
    /// <param name="approverId"></param>
    /// <param name="domain"></param>
    /// <returns></returns>
    public ApprovalSummaryRow GetApprovalSummaryByDocumentNumber(string documentTypeID, string documentNumber, string approverAlias, string approverId, string domain)
    {
        using (var perfTracer = _performanceLogger.StartPerformanceLogger("PerfLog", "Worker", string.Format(Constants.PerfLogAction, documentTypeID, "Get approval summary by documentNumber from azure storage")
            , new Dictionary<LogDataKey, object> { { LogDataKey.DocumentNumber, documentNumber } }))
        {
            ApprovalSummaryRow filteredSummaryRow = GetApprovalSummaryByDocumentNumberAndApprover(documentTypeID, documentNumber, approverAlias, approverId, domain);

            if (filteredSummaryRow != null && (filteredSummaryRow.LobPending == false || filteredSummaryRow.IsOfflineApproval == true))
                return filteredSummaryRow;
            else
                return null;
        }
    }

    /// <summary>
    /// Get approval summary by document number and approver
    /// </summary>
    /// <param name="documentTypeID"></param>
    /// <param name="documentNumber"></param>
    /// <param name="approverAlias"></param>
    /// <param name="approverId">In case external users allowed: ObjectId else Alias</param>
    /// <param name="approverDomain"></param>
    /// <returns></returns>
    public ApprovalSummaryRow GetApprovalSummaryByDocumentNumberAndApprover(string documentTypeID, string documentNumber, string approverAlias, string approverId, string approverDomain)
    {
        using (var summaryRowInsertTracer = _performanceLogger.StartPerformanceLogger("PerfLog", "SummaryProvider", string.Format(Constants.PerfLogAction, "Summary Provider", "Get Summary by DocumentNumber and Approver")
                , new Dictionary<LogDataKey, object> { { LogDataKey.Approver, approverAlias }, { LogDataKey.DocumentNumber, documentNumber }, { LogDataKey.DocumentTypeId, documentTypeID } }))
        {
            ApprovalSummaryRow filteredSummaryRow = null;
            string filterString = string.Empty;
            //External Domain
            if (!String.IsNullOrEmpty(approverId) && !_config[Constants.OldWhitelistedDomains].Contains(approverDomain, StringComparison.InvariantCultureIgnoreCase))
                filterString = "PartitionKey eq '" + approverId + "' and RowKey gt '" + (documentTypeID + Constants.FieldsSeparator) + "' and DocumentNumber eq '" + documentNumber + "'";
            else
                filterString = "PartitionKey eq '" + approverAlias.ToLowerInvariant() + "' and RowKey gt '" + (documentTypeID + Constants.FieldsSeparator) + "' and DocumentNumber eq '" + documentNumber + "'";

            filteredSummaryRow = _tableHelper.GetDataCollectionByTableQuery<ApprovalSummaryRow>(
                    _config[ConfigurationKey.ApprovalSummaryAzureTableName.ToString()], filterString).FirstOrDefault();

            if (filteredSummaryRow != null && !string.IsNullOrWhiteSpace(filteredSummaryRow.BlobPointer))
            {
                filteredSummaryRow = _approvalBlobDataProvider.GetApprovalSummaryJsonFromBlob(filteredSummaryRow).Result;
            }
            return filteredSummaryRow;
        }
    }

    /// <summary>
    /// Finds a summary row by approver and document number without requiring a documentTypeID.
    /// Queries ApprovalSummary with PartitionKey scoped to the approver and a DocumentNumber column filter.
    /// </summary>
    /// <param name="documentNumber">The document number to search for.</param>
    /// <param name="approverAlias">The approver alias.</param>
    /// <param name="approverId">The approver's object ID (for external users).</param>
    /// <param name="approverDomain">The approver's domain.</param>
    /// <returns>The first matching ApprovalSummaryRow, or null if not found.</returns>
    public async Task<ApprovalSummaryRow> FindSummaryByApproverAndDocumentNumberAsync(string documentNumber, string approverAlias, string approverId, string approverDomain)
    {
        using (var perfTracer = _performanceLogger.StartPerformanceLogger("PerfLog", "SummaryProvider", string.Format(Constants.PerfLogAction, "Summary Provider", "Find Summary by Approver and DocumentNumber")
                , new Dictionary<LogDataKey, object> { { LogDataKey.Approver, approverAlias }, { LogDataKey.DocumentNumber, documentNumber } }))
        {
            // approverAlias and approverId are derived from the signed-in user's auth context
            // (onBehalfUser.MailNickname / onBehalfUser.Id) — not from user input.
            string partitionKey;
            // External users use objectId as partition key; internal users use alias
            if (!string.IsNullOrEmpty(approverId) && !_config[Constants.OldWhitelistedDomains].Contains(approverDomain, StringComparison.InvariantCultureIgnoreCase))
            {
                partitionKey = approverId;
            }
            else
            {
                partitionKey = approverAlias.ToLowerInvariant();
            }

            // Escape single quotes to prevent OData filter injection (documentNumber is user-supplied via Copilot chat)
            var safePartitionKey = partitionKey.Replace("'", "''");
            var safeDocumentNumber = documentNumber.Replace("'", "''");
            string filterString = "PartitionKey eq '" + safePartitionKey + "' and DocumentNumber eq '" + safeDocumentNumber + "'";

            var filteredSummaryRow = _tableHelper.GetDataCollectionByTableQuery<ApprovalSummaryRow>(
                    _config[ConfigurationKey.ApprovalSummaryAzureTableName.ToString()], filterString).FirstOrDefault();

            if (filteredSummaryRow != null && !string.IsNullOrWhiteSpace(filteredSummaryRow.BlobPointer))
            {
                filteredSummaryRow = await _approvalBlobDataProvider.GetApprovalSummaryJsonFromBlob(filteredSummaryRow);
            }
            return filteredSummaryRow;
        }
    }

    /// <summary>
    /// Get approval summary by document number including soft delete data
    /// </summary>
    /// <param name="documentTypeID"></param>
    /// <param name="documentNumber"></param>
    /// <param name="approverAlias"></param>
    /// <param name="approverId"></param>
    /// <returns></returns>
    public ApprovalSummaryRow GetApprovalSummaryByDocumentNumberIncludingSoftDeleteData(string documentTypeID, string documentNumber, string approverAlias, string approverId, string approverDomain)
    {
        using (var perfTracer = _performanceLogger.StartPerformanceLogger("PerfLog", "Worker", string.Format(Constants.PerfLogAction, documentTypeID, "Get approval summary by documentNumber from azure storage")
            , new Dictionary<LogDataKey, object> { { LogDataKey.DocumentNumber, documentNumber } }))
        {
            return GetApprovalSummaryByDocumentNumberAndApprover(documentTypeID, documentNumber, approverAlias, approverId, approverDomain);
        }
    }

    /// <summary>
    /// Get approval summary by RowKey and Approver
    /// </summary>
    /// <param name="documentTypeID"></param>
    /// <param name="rowKey"></param>
    /// <param name="approverAlias"></param>
    /// <param name="approverId"></param>
    /// <param name="approverDomain"></param>
    /// <returns></returns>
    public ApprovalSummaryRow GetApprovalSummaryByRowKeyAndApprover(string documentTypeID, string rowKey, string approverAlias, string approverId, string approverDomain)
    {
        using (var summaryRowInsertTracer = _performanceLogger.StartPerformanceLogger("PerfLog", "SummaryProvider", string.Format(Constants.PerfLogAction, "Summary Provider", "Get Summary by rowkey and Approver")
                , new Dictionary<LogDataKey, object> { { LogDataKey.Approver, approverAlias }, { LogDataKey.RowKey, rowKey }, { LogDataKey.DocumentTypeId, documentTypeID } }))
        {
            List<ApprovalSummaryRow> jsonSummary;

            if (_config[Constants.OldWhitelistedDomains].Contains(approverDomain, StringComparison.InvariantCultureIgnoreCase))
                jsonSummary = _tableHelper.GetTableEntityListByPartitionKeyAndRowKey<ApprovalSummaryRow>(
                _config[ConfigurationKey.ApprovalSummaryAzureTableName.ToString()], approverAlias.ToLowerInvariant(), rowKey);

            //External Domain
            else
                jsonSummary = _tableHelper.GetTableEntityListByPartitionKeyAndRowKey<ApprovalSummaryRow>(
                _config[ConfigurationKey.ApprovalSummaryAzureTableName.ToString()], approverId, rowKey);

            var filteredSummaryRow = jsonSummary?.FirstOrDefault(y => (!y.LobPending || y.IsOfflineApproval));
            if (filteredSummaryRow != null && !string.IsNullOrWhiteSpace(filteredSummaryRow.BlobPointer))
            {
                filteredSummaryRow = _approvalBlobDataProvider.GetApprovalSummaryJsonFromBlob(filteredSummaryRow).Result;
            }
            return filteredSummaryRow;
        }
    }

    /// <summary>
    /// Get approval summary count json by approver and tenants
    /// </summary>
    /// <param name="approver"></param>
    /// <param name="tenants"></param>
    /// <param name="approverId">Approver Alias's object Id</param>
    /// <param name="approverDomain">Approver Alias's domain</param>
    /// <returns></returns>
    public List<ApprovalSummaryRow> GetApprovalSummaryCountJsonByApproverAndTenants(string approver, List<ApprovalTenantInfo> tenants, string approverId, string approverDomain)
    {
        using (var summaryRowInsertTracer = _performanceLogger.StartPerformanceLogger("PerfLog", "SummaryProvider", string.Format(Constants.PerfLogAction, "Summary Provider", "Get Summary Count by Approver and Tenant")
                    , new Dictionary<LogDataKey, object> { { LogDataKey.UserAlias, approver }, { LogDataKey.Tenants, string.Join(",", tenants.Select(t => t.AppName)) } }))
        {
            List<ApprovalSummaryRow> summaryRows;
            if (_config[Constants.OldWhitelistedDomains].Contains(approverDomain, StringComparison.InvariantCultureIgnoreCase))
                summaryRows = _tableHelper.GetTableEntityListByPartitionKey<ApprovalSummaryRow>(_config[ConfigurationKey.ApprovalSummaryAzureTableName.ToString()], approver.ToLowerInvariant());
            //External Domain
            else
                summaryRows = _tableHelper.GetTableEntityListByPartitionKey<ApprovalSummaryRow>(_config[ConfigurationKey.ApprovalSummaryAzureTableName.ToString()], approverId);

            return summaryRows;
        }
    }

    /// <summary>
    /// Get approval summary json by approver and tenants
    /// </summary>
    /// <param name="approver"></param>
    /// <param name="tenants"></param>
    /// <param name="approverId">Approver Alias's object Id</param>
    /// <param name="approverDomain">Approver Alias's domain</param>
    /// <param name="isSubmittedRequest">Flag to indicate if the request is for submitted approvals</param>
    /// <returns></returns>
    public List<ApprovalSummaryData> GetApprovalSummaryJsonByApproverAndTenants(string approver, List<ApprovalTenantInfo> tenants, string approverId, string approverDomain, bool isSubmittedRequest = false)
    {
        using (var summaryRowInsertTracer = _performanceLogger.StartPerformanceLogger("PerfLog", "SummaryProvider", string.Format(Constants.PerfLogAction, "Summary Provider", "Get Summary by Approver and Tenant")
                , new Dictionary<LogDataKey, object> { { LogDataKey.UserAlias, approver }, { LogDataKey.Tenants, string.Join(",", tenants.Select(t => t.AppName)) } }))
        {
            List<ApprovalSummaryData> filteredRowKeys = new List<ApprovalSummaryData>();
            IEnumerable<ApprovalSummaryRow> jsonSummary;

            if (isSubmittedRequest)
            {
                try
                {
                    var bySubmitterRows = _tableHelper.GetTableEntityListByPartitionKey<SubmissionSummaryRow>(_config[ConfigurationKey.SubmissionSummaryAzureTableName.ToString()], approver);
                    if (bySubmitterRows == null || !bySubmitterRows.Any())
                    {
                        jsonSummary = new List<ApprovalSummaryRow>();
                    }
                    else
                    {
                        var tasks = bySubmitterRows.Select(async item =>
                        {
                            try
                            {
                                return await _tableHelper.GetTableEntityByPartitionKeyAndRowKeyAsync<ApprovalSummaryRow>(
                                     _config[ConfigurationKey.ApprovalSummaryAzureTableName.ToString()],
                                     item.ApprovalSummaryPartitionKey,
                                     item.ApprovalSummaryRowKey);
                            }
                            catch (global::Azure.RequestFailedException ex) when (ex.Status == (int)HttpStatusCode.NotFound)
                            {
                                // The referenced ApprovalSummaryRow no longer exists; skip it.
                                return null;
                            }
                        });
                        var results = Task.WhenAll(tasks).GetAwaiter().GetResult();
                        jsonSummary = results.Where(r => r != null).ToList();
                    }
                }
                catch (global::Azure.RequestFailedException ex) when (ex.Status == (int)HttpStatusCode.NotFound)
                {
                    _logProvider.LogWarning(TrackingEvent.SummaryNotFoundInTenantSystem, new Dictionary<LogDataKey, object>
                    {
                        { LogDataKey.UserAlias, approver },
                        { LogDataKey.ErrorMessage, "SubmissionSummary table or resource not found" }
                    }, ex);
                    jsonSummary = new List<ApprovalSummaryRow>();
                }
            }
            else
            {
                if (_config[Constants.OldWhitelistedDomains].Contains(approverDomain, StringComparison.InvariantCultureIgnoreCase))
                    jsonSummary = _tableHelper.GetTableEntityListByPartitionKey<ApprovalSummaryRow>(_config[ConfigurationKey.ApprovalSummaryAzureTableName.ToString()], approver.ToLowerInvariant());
                //External Domain
                else
                    jsonSummary = _tableHelper.GetTableEntityListByPartitionKey<ApprovalSummaryRow>(_config[ConfigurationKey.ApprovalSummaryAzureTableName.ToString()], approverId);
            }
            filteredRowKeys = jsonSummary.Select(j => new ApprovalSummaryData()
            {
                SummaryJson = string.IsNullOrWhiteSpace(j.BlobPointer) ? j.SummaryJson : _approvalBlobDataProvider.GetApprovalSummaryJsonFromBlob(j).Result.SummaryJson,
                DocumentTypeId = j.RowKey.Split('|')[0],
                Approver = j.PartitionKey,
                LastFailed = j.LastFailed,
                LastFailedExceptionMessage = j.LastFailedExceptionMessage,
                Xcv = j.Xcv,
                IsRead = j.IsRead,
                IsOutOfSyncChallenged = j.IsOutOfSyncChallenged,
                IsOfflineApproval = j.IsOfflineApproval,
                LobPending = j.LobPending,
                DocumentNumber = j.DocumentNumber
            }).ToList();

            return filteredRowKeys;
        }
    }

    /// <summary>
    /// Get document summary by RowKey
    /// </summary>
    /// <param name="rowKey"></param>
    /// <returns></returns>
    public List<ApprovalSummaryRow> GetDocumentSummaryByRowKey(string rowKey)
    {
        using (var summaryRowInsertTracer = _performanceLogger.StartPerformanceLogger("PerfLog", "SummaryProvider", string.Format(Constants.PerfLogAction, "Summary Provider", "Get Summary by rowkey")
                    , new Dictionary<LogDataKey, object> { { LogDataKey.RowKey, rowKey } }))
        {
            var approvalSummaryRows = _tableHelper.GetTableEntityListByRowKey<ApprovalSummaryRow>(_config[ConfigurationKey.ApprovalSummaryAzureTableName.ToString()], rowKey).ToList();
            return (approvalSummaryRows.Select(row => !string.IsNullOrEmpty(row.BlobPointer) ? _approvalBlobDataProvider.GetApprovalSummaryJsonFromBlob(row).Result : row))?.ToList();
        }
    }

    /// <summary>
    /// Remove approval summary
    /// </summary>
    /// <param name="approvalRequest"></param>
    /// <param name="summaryRows"></param>
    /// <param name="message"></param>
    /// <param name="tenantInfo"></param>
    /// <returns></returns>
    public async Task<AzureTableRowDeletionResult> RemoveApprovalSummary(ApprovalRequestExpressionExt approvalRequest, List<ApprovalSummaryRow> summaryRows, ServiceBusReceivedMessage message, ApprovalTenantInfo tenantInfo)
    {
        var logData = new Dictionary<LogDataKey, object>
        {
            { LogDataKey._CorrelationId, message.GetCorrelationId() },
            { LogDataKey.BusinessProcessName, string.Format(tenantInfo.BusinessProcessName, Constants.BusinessProcessNameSendPayload, approvalRequest.Operation) },
            { LogDataKey.Tcv, approvalRequest.Telemetry.Tcv },
            { LogDataKey.Xcv, approvalRequest.Telemetry.Xcv },
            { LogDataKey.DXcv, approvalRequest.ApprovalIdentifier.DisplayDocumentNumber },
            { LogDataKey.DocumentNumber, approvalRequest.ApprovalIdentifier.DisplayDocumentNumber }
        };

        try
        {
            if (summaryRows != null && summaryRows.Any())
            {
                // TODO : Need to check how to handle multiple summary rows for received tcv.
                logData[LogDataKey.ReceivedTcv] = summaryRows.FirstOrDefault().Tcv;
                logData[LogDataKey.TenantTelemetryData] = approvalRequest.Telemetry.TenantTelemetry;

                if (tenantInfo.IsRaceConditionHandled && summaryRows.Any(summaryRow => DateTime.Compare(approvalRequest.OperationDateTime, summaryRow.OperationDateTime) < 0))
                {
                    approvalRequest.IsDeleteOperationComplete = true;
                    _logProvider.LogInformation(TrackingEvent.ApprovalSummaryRemovalSkipped, logData);
                    return AzureTableRowDeletionResult.SkippedDueToRaceCondition;
                }

                if (approvalRequest.ActionDetail != null && !string.IsNullOrEmpty(approvalRequest.ActionDetail.Name) && approvalRequest.ActionDetail.Name.Equals("TacitApprove", StringComparison.InvariantCultureIgnoreCase))
                {
                    Thread.Sleep(1000 * 20);
                    throw new InvalidOperationException("Summary is exists while processing tacit approve message");
                }

                using (var summaryRowInsertTracer = _performanceLogger.StartPerformanceLogger("PerfLog", "SummaryProvider", string.Format(Constants.PerfLogAction, "Summary Provider", "Delete approval summary")
                        , new Dictionary<LogDataKey, object> { { LogDataKey.DocumentNumber, summaryRows.FirstOrDefault().DocumentNumber }, { LogDataKey.Approvers, String.Join(",", summaryRows.Select(s => s.PartitionKey).ToList()) } }))
                {
                    foreach (var summaryRow in summaryRows)
                    {
                        await _tableHelper.DeleteRow<ApprovalSummaryRow>(_config[ConfigurationKey.ApprovalSummaryAzureTableName.ToString()], summaryRow);

                        // Remove from blob if blobPointer exists.
                        if (summaryRow.BlobPointer != null)
                        {
                            await _approvalBlobDataProvider.DeleteBlobData(summaryRow.BlobPointer, Constants.ApprovalSummaryBlobContainerName);
                        }
                    }
                }

                for (int i = 0; i < 3; i++)
                {
                    try
                    {
                        if (tenantInfo.IgnoreCurrentApproverCheck)
                        {
                            var summaryOperationRow = _approvalDetailProvider.GetApprovalsDetails(tenantInfo.TenantId, summaryRows?.FirstOrDefault()?.DocumentNumber, Constants.SummaryOperationType, tenantInfo.DocTypeId);
                            await _approvalDetailProvider.RemoveApprovalsDetails(
                                new List<ApprovalDetailsEntity>()
                                {
                                    new ApprovalDetailsEntity() { PartitionKey = approvalRequest?.ApprovalIdentifier?.GetDocNumber(tenantInfo), RowKey = Constants.CurrentApprover + "|" + tenantInfo.DocTypeId, TenantID = tenantInfo.TenantId },
                                    new ApprovalDetailsEntity() { PartitionKey = approvalRequest?.ApprovalIdentifier?.GetDocNumber(tenantInfo), RowKey = Constants.SummaryOperationType + "|" + tenantInfo.DocTypeId, TenantID = tenantInfo.TenantId, BlobPointer = summaryOperationRow.BlobPointer },
                                    new ApprovalDetailsEntity() { PartitionKey = approvalRequest?.ApprovalIdentifier?.GetDocNumber(tenantInfo), RowKey = Constants.CurrentApprover, TenantID = tenantInfo.TenantId },
                                    new ApprovalDetailsEntity() { PartitionKey = approvalRequest?.ApprovalIdentifier?.GetDocNumber(tenantInfo), RowKey = Constants.SummaryOperationType, TenantID = tenantInfo.TenantId, BlobPointer = summaryOperationRow.BlobPointer },
                                });
                            if (!string.IsNullOrWhiteSpace(summaryOperationRow.BlobPointer))
                                await _approvalBlobDataProvider.DeleteBlobData(summaryOperationRow.BlobPointer);
                        }
                        else
                        {
                            //TODO:: Debug carefully as earlier ApprovalDetailsEntity was used instead of ApprovalSummaryRow with Summary table
                            var rows = _tableHelper.GetTableEntityListByRowKey<ApprovalSummaryRow>(_config[ConfigurationKey.ApprovalSummaryAzureTableName.ToString()], summaryRows?.FirstOrDefault()?.RowKey); 
                            if (rows.Count() == 0 || approvalRequest.Operation != ApprovalRequestOperation.Delete)
                            {
                                var summaryOperationRow = _approvalDetailProvider.GetApprovalsDetails(tenantInfo.TenantId, summaryRows?.FirstOrDefault()?.DocumentNumber, Constants.SummaryOperationType, tenantInfo.DocTypeId);
                                await _approvalDetailProvider.RemoveApprovalsDetails(new List<ApprovalDetailsEntity>()
                                {
                                    new ApprovalDetailsEntity() { PartitionKey = approvalRequest?.ApprovalIdentifier?.GetDocNumber(tenantInfo), RowKey = Constants.CurrentApprover + "|" + tenantInfo.DocTypeId, TenantID = tenantInfo.TenantId },
                                    new ApprovalDetailsEntity() { PartitionKey = approvalRequest?.ApprovalIdentifier?.GetDocNumber(tenantInfo), RowKey = Constants.SummaryOperationType + "|" + tenantInfo.DocTypeId, TenantID = tenantInfo.TenantId, BlobPointer = summaryOperationRow.BlobPointer },
                                    new ApprovalDetailsEntity() { PartitionKey = approvalRequest?.ApprovalIdentifier?.GetDocNumber(tenantInfo), RowKey = Constants.CurrentApprover, TenantID = tenantInfo.TenantId },
                                    new ApprovalDetailsEntity() { PartitionKey = approvalRequest?.ApprovalIdentifier?.GetDocNumber(tenantInfo), RowKey = Constants.SummaryOperationType, TenantID = tenantInfo.TenantId, BlobPointer = summaryOperationRow.BlobPointer },
                                });
                                if (!string.IsNullOrWhiteSpace(summaryOperationRow.BlobPointer))
                                    await _approvalBlobDataProvider.DeleteBlobData(summaryOperationRow.BlobPointer);
                            }
                            else
                            {
                                var currentApproverRow = _approvalDetailProvider.GetApprovalsDetails(tenantInfo.TenantId, summaryRows?.FirstOrDefault()?.DocumentNumber, Constants.CurrentApprover, tenantInfo.DocTypeId);
                                var approvers = JsonConvert.DeserializeObject<List<Approver>>(currentApproverRow?.JSONData);
                                List<Approver> currentApprovers = new List<Approver>();
                                foreach (var row in rows)
                                {
                                    var approver = approvers?.FirstOrDefault(a => a.Alias == row.PartitionKey);
                                    if (approver != null)
                                    {
                                        currentApprovers.Add(approver);
                                    }
                                }
                                ApprovalDetailsEntity detailCurrentApprover = new ApprovalDetailsEntity() { PartitionKey = approvalRequest?.ApprovalIdentifier?.GetDocNumber(tenantInfo), RowKey = Constants.CurrentApprover + "|" + tenantInfo.DocTypeId, JSONData = (currentApprovers)?.ToJson(new JsonSerializerSettings { NullValueHandling = NullValueHandling.Ignore }), TenantID = tenantInfo.TenantId };
                                List<ApprovalDetailsEntity> detailRows = new List<ApprovalDetailsEntity>() { detailCurrentApprover };
                                await _approvalDetailProvider.AddApprovalsDetails(detailRows, tenantInfo, Environment.UserName, approvalRequest?.Telemetry?.Xcv, approvalRequest?.Telemetry?.Tcv);
                            }
                        }

                        break;
                    }
                    catch (Exception ex)
                    {
                        logData.Add(LogDataKey.Operation, Constants.CurrentApprover);
                        _logProvider.LogWarning(TrackingEvent.ApprovalDetailRemoveFail, logData, ex);
                    }
                }
            }
            approvalRequest.IsDeleteOperationComplete = true;
            return AzureTableRowDeletionResult.DeletionSuccessful;
        }
        catch (Exception exception)
        {
            if (exception is global::Azure.RequestFailedException dataServiceClientException && dataServiceClientException.Status == (int)HttpStatusCode.NotFound)
            {
                _logProvider.LogWarning(TrackingEvent.TopicMessageProcessDelete, logData, exception);
                return AzureTableRowDeletionResult.SkippedDueToNonExistence;
            }
            throw;
        }
    }

    /// <summary>
    /// Set next reminder time
    /// </summary>
    /// <param name="row"></param>
    /// <param name="currentTime"></param>
    public void SetNextReminderTime(ApprovalSummaryRow row, DateTime currentTime)
    {
        if (!string.IsNullOrEmpty(row.NotificationJson))
        {
            NotificationDetail notificationDetails = ReminderHelper.GetNotificationDetails(row);
            row.NextReminderTime = ReminderHelper.NextReminderTime(notificationDetails, currentTime);
        }
        else
        {
            row.NextReminderTime = DateTime.MaxValue;
        }
        row.NextReminderTime = DateTime.SpecifyKind(row.NextReminderTime, DateTimeKind.Utc);
    }

    /// <summary>
    /// Update summary
    /// </summary>
    /// <param name="tenant"></param>
    /// <param name="documentNumber"></param>
    /// <param name="approverAlias"></param>
    /// <param name="approverId"></param>
    /// <param name="approverDomain"></param>
    /// <param name="actionDate"></param>
    /// <param name="actionName"></param>
    /// <returns></returns>
    public async Task UpdateSummary(ApprovalTenantInfo tenant, string documentNumber, string approverAlias, string approverId, string approverDomain, DateTime? actionDate, string actionName)
    {
        var logData = new Dictionary<LogDataKey, object>
        {
            { LogDataKey.DisplayDocumentNumber, documentNumber },
            { LogDataKey.Approver, approverAlias },
            { LogDataKey.DocumentTypeId, tenant?.DocTypeId}
        };
        try
        {
            ApprovalSummaryRow summaryRow = GetApprovalSummaryByDocumentNumberAndApprover(tenant?.DocTypeId, documentNumber, approverAlias, approverId, approverDomain) ?? throw new InvalidOperationException("summaryRow could not be found.");
            await UpdateSummary(tenant, summaryRow, actionDate, actionName);
        }
        catch (Exception ex)
        {
            _logProvider.LogError(TrackingEvent.UpdateSummaryFail, ex, logData);
        }
    }

    /// <summary>
    /// Update summary
    /// </summary>
    /// <param name="tenant"></param>
    /// <param name="summaryRow"></param>
    /// <param name="actionDate"></param>
    /// <param name="actionName"></param>
    public async Task UpdateSummary(ApprovalTenantInfo tenant, ApprovalSummaryRow summaryRow, DateTime? actionDate, string actionName)
    {
        using (var summaryRowInsertTracer = _performanceLogger.StartPerformanceLogger("PerfLog", "SummaryProvider", string.Format(Constants.PerfLogAction, "Summary Provider", "Update approval summary")
               , new Dictionary<LogDataKey, object> { { LogDataKey.RowKey, summaryRow.RowKey }, { LogDataKey.PartitionKey, summaryRow.PartitionKey }, { LogDataKey.DocumentNumber, summaryRow.DocumentNumber } }))
        {
            var logData = new Dictionary<LogDataKey, object>();
            try
            {
                #region Logging

                logData.Add(LogDataKey.PartitionKey, summaryRow.PartitionKey);
                logData.Add(LogDataKey.RowKey, summaryRow.RowKey);
                logData.Add(LogDataKey.UserRoleName, summaryRow.PartitionKey);
                logData.Add(LogDataKey.BusinessProcessName, string.Format(tenant.BusinessProcessName, Constants.BusinessProcessNameApprovalAction, actionName));
                logData.Add(LogDataKey.Tcv, summaryRow.Tcv);
                logData.Add(LogDataKey.Xcv, summaryRow.Xcv);
                logData.Add(LogDataKey.DXcv, summaryRow.DocumentNumber);
                logData.Add(LogDataKey.DocumentNumber, summaryRow.DocumentNumber);
                logData.Add(LogDataKey.UserAlias, summaryRow.PartitionKey);
                logData.Add(LogDataKey.ReceivedTcv, summaryRow.Tcv);

                #endregion Logging

                ApplyCaseConstraints(summaryRow);
                SetNextReminderTime(summaryRow, DateTime.UtcNow);

                summaryRow.IsOutOfSyncChallenged = actionName == Constants.OutOfSyncAction;
                summaryRow.IsOfflineApproval = actionName == Constants.OfflineApproval;
                summaryRow.IsRead = actionName == Constants.IsRead;
                if (summaryRow.IsOutOfSyncChallenged || summaryRow.IsOfflineApproval)
                {
                    summaryRow.LobPending = false;
                }

                var summaryJson = summaryRow?.SummaryJson?.FromJson<SummaryJson>(new JsonSerializerSettings { NullValueHandling = NullValueHandling.Ignore });

                //setting this to empty to avoid insertion error in case summaryJson exceeds 64kb
                if (!string.IsNullOrWhiteSpace(summaryRow.BlobPointer))
                    summaryRow.SummaryJson = string.Empty;
                await _tableHelper.InsertOrReplace<ApprovalSummaryRow>(_config[ConfigurationKey.ApprovalSummaryAzureTableName.ToString()], summaryRow);

                // Get all approval details data and check if it has row key = SUM or SUM|doctypeid
                var allApprovalDetails = await _approvalDetailProvider.GetAllApprovalDetailsByTenantAndDocumentNumber(tenant.TenantId, summaryRow?.DocumentNumber);
                var summaryOperationRowFromDetails = allApprovalDetails?.FirstOrDefault(detail =>
                    detail.RowKey.Equals(Constants.SummaryOperationType, StringComparison.InvariantCultureIgnoreCase) ||
                    detail.RowKey.Equals(string.Format(Constants.SummaryOperationTypeNew, tenant.DocTypeId), StringComparison.InvariantCultureIgnoreCase));

                List<ApprovalSummaryRow> summaryRows = [summaryRow];
                ApprovalDetailsEntity approvalDetails = new() { PartitionKey = summaryJson?.ApprovalIdentifier?.GetDocNumber(tenant), RowKey = summaryOperationRowFromDetails?.RowKey ?? Constants.SummaryOperationType + "|" + tenant.DocTypeId, ETag = global::Azure.ETag.All, JSONData = summaryRows.ToJson(), TenantID = tenant.TenantId };
                await _approvalDetailProvider.AddTransactionalAndHistoricalDataInApprovalsDetails(approvalDetails, tenant, new ApprovalsTelemetry() { Xcv = summaryRow.Xcv, Tcv = string.Empty, BusinessProcessName = string.Empty });

                _logProvider.LogInformation(TrackingEvent.UpdateSummarySuccess, logData);
            }
            catch (global::Azure.RequestFailedException exception)
            {
                if (exception != null
                        && exception.Message != null)
                {
                    logData.Add(LogDataKey.ErrorMessage, exception.Message);
                }
                _logProvider.LogError(TrackingEvent.UpdateSummaryFail, exception, logData);
            }
            catch (Exception exception)
            {
                // Ignore Exception as Row Might Have been already Deleted
                _logProvider.LogError(TrackingEvent.UpdateSummaryFail, exception, logData);
            }
        }
    }

    /// <summary>
    /// Update summary in batch async
    /// </summary>
    /// <param name="summaryRows"></param>
    /// <param name="xcv"></param>
    /// <param name="sessionId"></param>
    /// <param name="tcv"></param>
    /// <param name="tenantInfo"></param>
    /// <param name="actionName"></param>
    /// <returns></returns>
    public async Task UpdateSummaryInBatchAsync(List<ApprovalSummaryRow> summaryRows, string xcv, string sessionId, string tcv, ApprovalTenantInfo tenantInfo, string actionName)
    {
        using (var summaryRowInsertTracer = _performanceLogger.StartPerformanceLogger("PerfLog", "SummaryProvider", string.Format(Constants.PerfLogAction, "Summary Provider", "Mark SoftDelete Summary In Batch")
            , new Dictionary<LogDataKey, object> { { LogDataKey.UserActionsString, summaryRows } }))
        {
            var logData = new Dictionary<LogDataKey, object>
            {
                { LogDataKey.StartDateTime, DateTime.UtcNow },
                { LogDataKey.BusinessProcessName, string.Format(tenantInfo.BusinessProcessName, Constants.BusinessProcessNameApprovalAction, actionName) },
                { LogDataKey.Xcv, xcv },
                { LogDataKey.SessionId, sessionId },
                { LogDataKey.Tcv, tcv },
                { LogDataKey.ReceivedTcv, tcv }
            };

            try
            {
                _logProvider.LogInformation(TrackingEvent.BatchUpdateSummaryInitiated, logData);
                await _tableHelper.InsertOrReplaceRows<ApprovalSummaryRow>(_config[ConfigurationKey.ApprovalSummaryAzureTableName.ToString()], summaryRows);
                logData.Add(LogDataKey.EndDateTime, DateTime.UtcNow);
                _logProvider.LogInformation(TrackingEvent.BatchUpdateSummarySuccess, logData);
            }
            catch (global::Azure.RequestFailedException exception)
            {
                logData.Modify(LogDataKey.EndDateTime, DateTime.UtcNow);
                if (exception != null
                    && exception.Message != null)
                {
                    logData.Add(LogDataKey.ErrorMessage, exception.Message);
                }
                _logProvider.LogError(TrackingEvent.BatchUpdateSummaryFailed, exception, logData);
            }
            catch (Exception exception)
            {
                logData.Modify(LogDataKey.EndDateTime, DateTime.UtcNow);
                _logProvider.LogError(TrackingEvent.BatchUpdateSummaryFailed, exception, logData);
            }
        }
    }
}