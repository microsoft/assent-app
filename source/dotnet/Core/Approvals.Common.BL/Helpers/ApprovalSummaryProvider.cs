// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace Microsoft.CFS.Approvals.Common.DL
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Net;
    using System.Threading;
    using System.Threading.Tasks;
    using Microsoft.Azure.Cosmos.Table;
    using Microsoft.Azure.ServiceBus;
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
        /// The approval tenantInfo provider
        /// </summary>
        private readonly IApprovalTenantInfoProvider _approvalTenantInfoProvider;

        /// <summary>
        /// Constructor of ApprovalSummaryProvider
        /// </summary>
        /// <param name="config"></param>
        /// <param name="logProvider"></param>
        /// <param name="tableHelper"></param>
        /// <param name="performanceLogger"></param>
        /// <param name="approvalDetailProvider"></param>
        /// <param name="approvalTenantInfoProvider"></param>
        public ApprovalSummaryProvider(
            IConfiguration config,
            ILogProvider logProvider,
            ITableHelper tableHelper,
            IPerformanceLogger performanceLogger,
            IApprovalDetailProvider approvalDetailProvider,
            IApprovalTenantInfoProvider approvalTenantInfoProvider
            )
        {
            _config = config;
            _logProvider = logProvider;
            _tableHelper = tableHelper;
            _performanceLogger = performanceLogger;
            _approvalDetailProvider = approvalDetailProvider;
            _approvalTenantInfoProvider = approvalTenantInfoProvider;
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
                ApplyCaseConstraints(row);
                SetNextReminderTime(row, DateTime.UtcNow);
                ApprovalSummaryRow existingRow = GetApprovalSummaryByDocumentNumber(approvalRequest.DocumentTypeId.ToString(), row.DocumentNumber, row.PartitionKey);

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
                List<ApprovalDetailsEntity> transactionalDetails = _approvalDetailProvider.GetAllApprovalsDetails(tenant.TenantId, approvalRequest.ApprovalIdentifier.GetDocNumber(tenant));
                JObject previousExceptionsMessages = new JObject();

                if (transactionalDetails != null && transactionalDetails.Any())
                {
                    // Filter to get only the row which has TransactionalDetails (LastfailedException message etc.)
                    var previousExceptionsMessagesRow = transactionalDetails.FirstOrDefault(x => x.RowKey.Equals(Constants.TransactionDetailsOperationType + '|' + row.PartitionKey, StringComparison.InvariantCultureIgnoreCase));
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
                        RowKey = Constants.TransactionDetailsOperationType + '|' + row.PartitionKey,
                        ETag = "*",
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
                        switch (savechangesoptions)
                        {
                            case SaveOptions.ContinueOnError:
                                await _tableHelper.Insert<ApprovalSummaryRow>(_config[ConfigurationKey.ApprovalSummaryAzureTableName.ToString()], row);
                                break;

                            case SaveOptions.ReplaceOnUpdate:
                                await _tableHelper.InsertOrReplace(_config[ConfigurationKey.ApprovalSummaryAzureTableName.ToString()], row);
                                break;
                        }

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

            ApprovalDetailsEntity detailCurrentApprover = new ApprovalDetailsEntity() { PartitionKey = approvalRequest?.ApprovalIdentifier?.GetDocNumber(tenant), RowKey = Constants.CurrentApprover, JSONData = (approvalRequest.Approvers).ToJson(), TenantID = tenant.TenantId };
            List<ApprovalDetailsEntity> detailRows = new List<ApprovalDetailsEntity>() { detailCurrentApprover };

            // Adding Additional Details into table
            if (approvalRequest.DetailsData != null && approvalRequest.DetailsData.ContainsKey(Constants.AdditionalDetails))
            {
                ApprovalDetailsEntity additionalDetails = new ApprovalDetailsEntity()
                {
                    PartitionKey = approvalRequest?.ApprovalIdentifier?.GetDocNumber(tenant),
                    RowKey = Constants.AdditionalDetails,
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
                TableQuery<ApprovalSummaryRow> query = (new TableQuery<ApprovalSummaryRow>()
                    .Where(TableQuery.CombineFilters(TableQuery.GenerateFilterCondition("PartitionKey", QueryComparisons.Equal, approverKey), TableOperators.And, TableQuery.GenerateFilterConditionForBool("LobPending", QueryComparisons.Equal, false)))
                    .Select(new string[] { "PartitionKey", "Approver", "RowKey" }));

                var jsonSummary = _tableHelper.GetDataCollectionByTableQuery(_config[ConfigurationKey.ApprovalSummaryAzureTableName.ToString()], query);

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
        /// <param name="approver"></param>
        /// <returns></returns>
        public ApprovalSummaryRow GetApprovalSummaryByDocumentNumber(string documentTypeID, string documentNumber, string approver)
        {
            using (var perfTracer = _performanceLogger.StartPerformanceLogger("PerfLog", "Worker", string.Format(Constants.PerfLogAction, documentTypeID, "Get approval summary by documentNumber from azure storage")
                , new Dictionary<LogDataKey, object> { { LogDataKey.DocumentNumber, documentNumber } }))
            {
                ApprovalSummaryRow filteredSummaryRow = GetApprovalSummaryByDocumentNumberAndApprover(documentTypeID, documentNumber, approver);

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
        /// <param name="approver"></param>
        /// <returns></returns>
        public ApprovalSummaryRow GetApprovalSummaryByDocumentNumberAndApprover(string documentTypeID, string documentNumber, string approver)
        {
            using (var summaryRowInsertTracer = _performanceLogger.StartPerformanceLogger("PerfLog", "SummaryProvider", string.Format(Constants.PerfLogAction, "Summary Provider", "Get Summary by DocumentNumber and Approver")
                    , new Dictionary<LogDataKey, object> { { LogDataKey.Approver, approver }, { LogDataKey.DocumentNumber, documentNumber }, { LogDataKey.DocumentTypeId, documentTypeID } }))
            {
                ApprovalSummaryRow filteredSummaryRow = null;

                if (!String.IsNullOrEmpty(approver))
                {
                    string filterString = "PartitionKey eq '" + approver.ToLowerInvariant() + "' and RowKey gt '" + (documentTypeID + Constants.FieldsSeparator) + "' and DocumentNumber eq '" + documentNumber + "'";
                    filteredSummaryRow = _tableHelper.GetDataCollectionByTableQuery<ApprovalSummaryRow>(
                        _config[ConfigurationKey.ApprovalSummaryAzureTableName.ToString()],
                        new TableQuery<ApprovalSummaryRow>(){
                            FilterString = filterString
                        }).FirstOrDefault();
                }
                return filteredSummaryRow;
            }
        }

        /// <summary>
        /// Get approval summary by document number including soft delete data
        /// </summary>
        /// <param name="documentTypeID"></param>
        /// <param name="documentNumber"></param>
        /// <param name="approver"></param>
        /// <returns></returns>
        public ApprovalSummaryRow GetApprovalSummaryByDocumentNumberIncludingSoftDeleteData(string documentTypeID, string documentNumber, string approver)
        {
            using (var perfTracer = _performanceLogger.StartPerformanceLogger("PerfLog", "Worker", string.Format(Constants.PerfLogAction, documentTypeID, "Get approval summary by documentNumber from azure storage")
                , new Dictionary<LogDataKey, object> { { LogDataKey.DocumentNumber, documentNumber } }))
            {
                return GetApprovalSummaryByDocumentNumberAndApprover(documentTypeID, documentNumber, approver);
            }
        }

        /// <summary>
        /// Get approval summary by RowKey and Approver
        /// </summary>
        /// <param name="documentTypeID"></param>
        /// <param name="rowKey"></param>
        /// <param name="approver"></param>
        /// <returns></returns>
        public ApprovalSummaryRow GetApprovalSummaryByRowKeyAndApprover(string documentTypeID, string rowKey, string approver)
        {
            using (var summaryRowInsertTracer = _performanceLogger.StartPerformanceLogger("PerfLog", "SummaryProvider", string.Format(Constants.PerfLogAction, "Summary Provider", "Get Summary by rowkey and Approver")
                    , new Dictionary<LogDataKey, object> { { LogDataKey.Approver, approver }, { LogDataKey.RowKey, rowKey }, { LogDataKey.DocumentTypeId, documentTypeID } }))
            {
                var jsonSummary = _tableHelper.GetTableEntityListByPartitionKeyAndRowKey<ApprovalSummaryRow>(
                    _config[ConfigurationKey.ApprovalSummaryAzureTableName.ToString()], approver.ToLowerInvariant(), rowKey);

                var filteredSummaryRows = jsonSummary?.FirstOrDefault(y => (!y.LobPending || y.IsOfflineApproval));
                return filteredSummaryRows;
            }
        }

        /// <summary>
        /// Get approval summary count json by approver and tenants
        /// </summary>
        /// <param name="approver"></param>
        /// <param name="tenants"></param>
        /// <returns></returns>
        public List<ApprovalSummaryRow> GetApprovalSummaryCountJsonByApproverAndTenants(string approver, List<ApprovalTenantInfo> tenants)
        {
            using (var summaryRowInsertTracer = _performanceLogger.StartPerformanceLogger("PerfLog", "SummaryProvider", string.Format(Constants.PerfLogAction, "Summary Provider", "Get Summary Count by Approver and Tenant")
                        , new Dictionary<LogDataKey, object> { { LogDataKey.UserAlias, approver }, { LogDataKey.Tenants, string.Join(",", tenants.Select(t => t.AppName)) } }))
            {
                return _tableHelper.GetTableEntityListByPartitionKey<ApprovalSummaryRow>(_config[ConfigurationKey.ApprovalSummaryAzureTableName.ToString()], approver.ToLowerInvariant());
            }
        }

        /// <summary>
        /// Get approval summary json by approver and tenants
        /// </summary>
        /// <param name="approver"></param>
        /// <param name="tenants"></param>
        /// <returns></returns>
        public List<ApprovalSummaryData> GetApprovalSummaryJsonByApproverAndTenants(string approver, List<ApprovalTenantInfo> tenants)
        {
            using (var summaryRowInsertTracer = _performanceLogger.StartPerformanceLogger("PerfLog", "SummaryProvider", string.Format(Constants.PerfLogAction, "Summary Provider", "Get Summary by Approver and Tenant")
                    , new Dictionary<LogDataKey, object> { { LogDataKey.UserAlias, approver }, { LogDataKey.Tenants, string.Join(",", tenants.Select(t => t.AppName)) } }))
            {
                List<ApprovalSummaryData> filteredRowKeys = new List<ApprovalSummaryData>();
                IEnumerable<ApprovalSummaryRow> jsonSummary = _tableHelper.GetTableEntityListByPartitionKey<ApprovalSummaryRow>(_config[ConfigurationKey.ApprovalSummaryAzureTableName.ToString()], approver.ToLowerInvariant());
                filteredRowKeys = jsonSummary.Select(j => new ApprovalSummaryData()
                {
                    SummaryJson = j.SummaryJson,
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
                return _tableHelper.GetTableEntityListByRowKey<ApprovalSummaryRow>(_config[ConfigurationKey.ApprovalSummaryAzureTableName.ToString()], rowKey).ToList();
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
        public async Task<AzureTableRowDeletionResult> RemoveApprovalSummary(ApprovalRequestExpressionExt approvalRequest, List<ApprovalSummaryRow> summaryRows, Message message, ApprovalTenantInfo tenantInfo)
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
                        throw new Exception("Summary is exists while processing tacit approve message");
                    }

                    using (var summaryRowInsertTracer = _performanceLogger.StartPerformanceLogger("PerfLog", "SummaryProvider", string.Format(Constants.PerfLogAction, "Summary Provider", "Delete approval summary")
                            , new Dictionary<LogDataKey, object> { { LogDataKey.DocumentNumber, summaryRows.FirstOrDefault().DocumentNumber }, { LogDataKey.Approvers, String.Join(",", summaryRows.Select(s => s.PartitionKey).ToList()) } }))
                    {
                        foreach (var summaryRow in summaryRows)
                        {
                            await _tableHelper.DeleteRow<ApprovalSummaryRow>(_config[ConfigurationKey.ApprovalSummaryAzureTableName.ToString()], summaryRow);
                        }
                    }

                    for (int i = 0; i < 3; i++)
                    {
                        try
                        {
                            if (tenantInfo.IgnoreCurrentApproverCheck)
                            {
                                await _approvalDetailProvider.RemoveApprovalsDetails(new List<ApprovalDetailsEntity>() { new ApprovalDetailsEntity() { PartitionKey = approvalRequest?.ApprovalIdentifier?.GetDocNumber(tenantInfo), RowKey = Constants.CurrentApprover, TenantID = tenantInfo.TenantId }, new ApprovalDetailsEntity() { PartitionKey = approvalRequest?.ApprovalIdentifier?.GetDocNumber(tenantInfo), RowKey = Constants.SummaryOperationType, TenantID = tenantInfo.TenantId } });
                            }
                            else
                            {
                                //TODO:: Debug carefully as earlier ApprovalDetailsEntity was used instead of ApprovalSummaryRow with Summary table
                                var rows = _tableHelper.GetTableEntityListByRowKey<ApprovalSummaryRow>(_config[ConfigurationKey.ApprovalSummaryAzureTableName.ToString()], summaryRows?.FirstOrDefault()?.RowKey);
                                if (rows.Count() == 0 || approvalRequest.Operation != ApprovalRequestOperation.Delete)
                                {
                                    await _approvalDetailProvider.RemoveApprovalsDetails(new List<ApprovalDetailsEntity>() { new ApprovalDetailsEntity() { PartitionKey = approvalRequest?.ApprovalIdentifier?.GetDocNumber(tenantInfo), RowKey = Constants.CurrentApprover, TenantID = tenantInfo.TenantId }, new ApprovalDetailsEntity() { PartitionKey = approvalRequest?.ApprovalIdentifier?.GetDocNumber(tenantInfo), RowKey = Constants.SummaryOperationType, TenantID = tenantInfo.TenantId } });
                                }
                                else
                                {
                                    var currentApproverRow = _approvalDetailProvider.GetApprovalsDetails(tenantInfo.TenantId, summaryRows?.FirstOrDefault()?.DocumentNumber, Constants.CurrentApprover);
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
                                    ApprovalDetailsEntity detailCurrentApprover = new ApprovalDetailsEntity() { PartitionKey = approvalRequest?.ApprovalIdentifier?.GetDocNumber(tenantInfo), RowKey = Constants.CurrentApprover, JSONData = (currentApprovers)?.ToJson(), TenantID = tenantInfo.TenantId };
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
                if (exception is StorageException dataServiceClientException && dataServiceClientException.RequestInformation.HttpStatusCode == (int)HttpStatusCode.NotFound)
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
        }

        /// <summary>
        /// Update isRead flag for summary row
        /// </summary>
        /// <param name="documentNumber"></param>
        /// <param name="approver"></param>
        /// <param name="tenantInfo"></param>
        /// <returns></returns>
        public bool UpdateIsReadSummary(string documentNumber, string approver, ApprovalTenantInfo tenantInfo)
        {
            var logData = new Dictionary<LogDataKey, object>
            {
                {LogDataKey.DisplayDocumentNumber, documentNumber},
                {LogDataKey.Approver, approver},
                {LogDataKey.DocumentTypeId, tenantInfo?.DocTypeId}
            };
            try
            {
                ApprovalSummaryRow summaryRow = GetApprovalSummaryByDocumentNumberAndApprover(tenantInfo?.DocTypeId, documentNumber, approver);
                if (summaryRow == null)
                    throw new Exception("SummaryRow could not be found.");
                summaryRow.IsRead = true;
                _tableHelper.InsertOrReplace<ApprovalSummaryRow>(_config[ConfigurationKey.ApprovalSummaryAzureTableName.ToString()], summaryRow);

                List<ApprovalSummaryRow> summaryRows = new List<ApprovalSummaryRow>() { summaryRow };
                var summaryJson = summaryRow?.SummaryJson?.FromJson<SummaryJson>();
                ApprovalDetailsEntity approvalDetails = new ApprovalDetailsEntity() { PartitionKey = summaryJson?.ApprovalIdentifier?.GetDocNumber(tenantInfo), RowKey = Constants.SummaryOperationType, ETag = "*", JSONData = summaryRows.ToJson(), TenantID = tenantInfo.TenantId };

                _tableHelper.InsertOrReplace<ApprovalDetailsEntity>(Constants.ApprovalDetailsAzureTableName, approvalDetails, false);

                return true;
            }
            catch (Exception ex)
            {
                _logProvider.LogError(TrackingEvent.SummaryRowUpdateFailed, ex, logData);
                return false;
            }
        }

        /// <summary>
        /// Update summary for offline approval
        /// </summary>
        /// <param name="tenant"></param>
        /// <param name="SummaryRow"></param>
        /// <param name="actionName"></param>
        public void UpdateSummaryForOfflineApproval(ApprovalTenantInfo tenant, ApprovalSummaryRow SummaryRow, string actionName)
        {
            using (var summaryRowInsertTracer = _performanceLogger.StartPerformanceLogger("PerfLog", "SummaryProvider", string.Format(Constants.PerfLogAction, "Summary Provider", "Update approval summary for IsOutOfSyncChallenged")
                   , new Dictionary<LogDataKey, object> { { LogDataKey.RowKey, SummaryRow.RowKey }, { LogDataKey.PartitionKey, SummaryRow.PartitionKey }, { LogDataKey.DocumentNumber, SummaryRow.DocumentNumber } }))
            {
                var logData = new Dictionary<LogDataKey, object>();
                try
                {
                    #region Logging

                    logData.Add(LogDataKey.PartitionKey, SummaryRow.PartitionKey);
                    logData.Add(LogDataKey.RowKey, SummaryRow.RowKey);
                    logData.Add(LogDataKey.UserRoleName, SummaryRow.PartitionKey);
                    logData.Add(LogDataKey.BusinessProcessName, string.Format(tenant.BusinessProcessName, Constants.BusinessProcessNameApprovalAction, actionName));
                    logData.Add(LogDataKey.Tcv, SummaryRow.Tcv);
                    logData.Add(LogDataKey.Xcv, SummaryRow.Xcv);
                    logData.Add(LogDataKey.DXcv, SummaryRow.DocumentNumber);
                    logData.Add(LogDataKey.DocumentNumber, SummaryRow.DocumentNumber);
                    logData.Add(LogDataKey.UserAlias, SummaryRow.PartitionKey);
                    logData.Add(LogDataKey.ReceivedTcv, SummaryRow.Tcv);

                    #endregion Logging

                    ApplyCaseConstraints(SummaryRow);
                    SetNextReminderTime(SummaryRow, DateTime.UtcNow);
                    SummaryRow.LobPending = false;
                    SummaryRow.IsOfflineApproval = true;
                    _tableHelper.InsertOrReplace(_config[ConfigurationKey.ApprovalSummaryAzureTableName.ToString()], SummaryRow);

                    List<ApprovalSummaryRow> summaryRows = new List<ApprovalSummaryRow>() { SummaryRow };
                    var summaryJson = SummaryRow?.SummaryJson?.FromJson<SummaryJson>();
                    ApprovalDetailsEntity approvalDetails = new ApprovalDetailsEntity() { PartitionKey = summaryJson?.ApprovalIdentifier?.GetDocNumber(tenant), RowKey = Constants.SummaryOperationType, ETag = "*", JSONData = summaryRows.ToJson(), TenantID = tenant.TenantId };
                    //TODO :: Refactor to call BL methods instead of directly calling DAL methods
                    _tableHelper.InsertOrReplace(Constants.ApprovalDetailsAzureTableName, approvalDetails, false);
                    _logProvider.LogInformation(TrackingEvent.UpdateSummaryForOfflineApproval, logData);
                }
                catch (StorageException exception)
                {
                    if (exception.RequestInformation != null
                            && exception.RequestInformation.ExtendedErrorInformation != null
                            && exception.RequestInformation.ExtendedErrorInformation.ErrorMessage != null)
                    {
                        logData.Add(LogDataKey.ErrorMessage, exception.RequestInformation.ExtendedErrorInformation.ErrorMessage);
                    }
                    _logProvider.LogError(TrackingEvent.UpdateSummaryForOfflineApproval, exception, logData);
                }
                catch (Exception exception)
                {
                    // Ignore Exception as Row Might Have been already Deleted
                    _logProvider.LogError(TrackingEvent.UpdateSummaryForOfflineApproval, exception, logData);
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
                catch (StorageException exception)
                {
                    logData.Modify(LogDataKey.EndDateTime, DateTime.UtcNow);
                    if (exception.RequestInformation != null
                        && exception.RequestInformation.ExtendedErrorInformation != null
                        && exception.RequestInformation.ExtendedErrorInformation.ErrorMessage != null)
                    {
                        logData.Add(LogDataKey.ErrorMessage, exception.RequestInformation.ExtendedErrorInformation.ErrorMessage);
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

        /// <summary>
        /// Update summary using post action
        /// </summary>
        /// <param name="tenant"></param>
        /// <param name="SummaryRow"></param>
        /// <param name="actionDate"></param>
        /// <param name="actionName"></param>
        public void UpdateSummaryIsOutOfSyncChallenged(ApprovalTenantInfo tenant, ApprovalSummaryRow SummaryRow, DateTime? actionDate, string actionName)
        {
            using (var summaryRowInsertTracer = _performanceLogger.StartPerformanceLogger("PerfLog", "SummaryProvider", string.Format(Constants.PerfLogAction, "Summary Provider", "Update approval summary for IsOutOfSyncChallenged")
                   , new Dictionary<LogDataKey, object> { { LogDataKey.RowKey, SummaryRow.RowKey }, { LogDataKey.PartitionKey, SummaryRow.PartitionKey }, { LogDataKey.DocumentNumber, SummaryRow.DocumentNumber } }))
            {
                var logData = new Dictionary<LogDataKey, object>();
                try
                {
                    #region Logging

                    logData.Add(LogDataKey.PartitionKey, SummaryRow.PartitionKey);
                    logData.Add(LogDataKey.RowKey, SummaryRow.RowKey);
                    logData.Add(LogDataKey.UserRoleName, SummaryRow.PartitionKey);
                    logData.Add(LogDataKey.BusinessProcessName, string.Format(tenant.BusinessProcessName, Constants.BusinessProcessNameApprovalAction, actionName));
                    logData.Add(LogDataKey.Tcv, SummaryRow.Tcv);
                    logData.Add(LogDataKey.Xcv, SummaryRow.Xcv);
                    logData.Add(LogDataKey.DXcv, SummaryRow.DocumentNumber);
                    logData.Add(LogDataKey.DocumentNumber, SummaryRow.DocumentNumber);
                    logData.Add(LogDataKey.UserAlias, SummaryRow.PartitionKey);
                    logData.Add(LogDataKey.ReceivedTcv, SummaryRow.Tcv);

                    #endregion Logging

                    ApplyCaseConstraints(SummaryRow);
                    SetNextReminderTime(SummaryRow, DateTime.UtcNow);
                    SummaryRow.IsOutOfSyncChallenged = actionName == Constants.OutOfSyncAction;

                    _tableHelper.InsertOrReplace<ApprovalSummaryRow>(_config[ConfigurationKey.ApprovalSummaryAzureTableName.ToString()], SummaryRow);

                    List<ApprovalSummaryRow> summaryRows = new List<ApprovalSummaryRow>() { SummaryRow };
                    var summaryJson = SummaryRow?.SummaryJson?.FromJson<SummaryJson>();
                    ApprovalDetailsEntity approvalDetails = new ApprovalDetailsEntity() { PartitionKey = summaryJson?.ApprovalIdentifier?.GetDocNumber(tenant), RowKey = Constants.SummaryOperationType, ETag = "*", JSONData = summaryRows.ToJson(), TenantID = tenant.TenantId };
                    //TODO :: Refactor to call BL methods instead of directly calling DAL methods
                    _tableHelper.InsertOrReplace<ApprovalDetailsEntity>(Constants.ApprovalDetailsAzureTableName, approvalDetails, false);

                    _logProvider.LogInformation(TrackingEvent.UpdateSummaryIsOutOfSyncChallenged, logData);
                }
                catch (StorageException exception)
                {
                    if (exception.RequestInformation != null
                            && exception.RequestInformation.ExtendedErrorInformation != null
                            && exception.RequestInformation.ExtendedErrorInformation.ErrorMessage != null)
                    {
                        logData.Add(LogDataKey.ErrorMessage, exception.RequestInformation.ExtendedErrorInformation.ErrorMessage);
                    }
                    _logProvider.LogError(TrackingEvent.UpdateSummaryIsOutOfSyncChallenged, exception, logData);
                }
                catch (Exception exception)
                {
                    // Ignore Exception as Row Might Have been already Deleted
                    _logProvider.LogError(TrackingEvent.UpdateSummaryIsOutOfSyncChallenged, exception, logData);
                }
            }
        }

        /// <summary>
        /// Update summary post action
        /// </summary>
        /// <param name="tenant"></param>
        /// <param name="SummaryRow"></param>
        /// <param name="actionDate"></param>
        /// <param name="actionName"></param>
        public void UpdateSummaryPostAction(ApprovalTenantInfo tenant, ApprovalSummaryRow SummaryRow, DateTime? actionDate, string actionName)
        {
            using (var summaryRowInsertTracer = _performanceLogger.StartPerformanceLogger("PerfLog", "SummaryProvider", string.Format(Constants.PerfLogAction, "Summary Provider", "Update approval summary")
                    , new Dictionary<LogDataKey, object> { { LogDataKey.RowKey, SummaryRow.RowKey }, { LogDataKey.PartitionKey, SummaryRow.PartitionKey }, { LogDataKey.DocumentNumber, SummaryRow.DocumentNumber } }))
            {
                var logData = new Dictionary<LogDataKey, object>();
                try
                {
                    #region Logging

                    logData.Add(LogDataKey.PartitionKey, SummaryRow.PartitionKey);
                    logData.Add(LogDataKey.RowKey, SummaryRow.RowKey);
                    logData.Add(LogDataKey.UserRoleName, SummaryRow.PartitionKey);
                    logData.Add(LogDataKey.BusinessProcessName, string.Format(tenant.BusinessProcessName, Constants.BusinessProcessNameApprovalAction, actionName));
                    logData.Add(LogDataKey.Tcv, SummaryRow.Tcv);
                    logData.Add(LogDataKey.Xcv, SummaryRow.Xcv);
                    logData.Add(LogDataKey.DXcv, SummaryRow.DocumentNumber);
                    logData.Add(LogDataKey.DocumentNumber, SummaryRow.DocumentNumber);
                    logData.Add(LogDataKey.UserAlias, SummaryRow.PartitionKey);
                    logData.Add(LogDataKey.ReceivedTcv, SummaryRow.Tcv);

                    #endregion Logging

                    ApplyCaseConstraints(SummaryRow);
                    SetNextReminderTime(SummaryRow, DateTime.UtcNow);
                    _tableHelper.Insert<ApprovalSummaryRow>(_config[ConfigurationKey.ApprovalSummaryAzureTableName.ToString()], SummaryRow);

                    List<ApprovalSummaryRow> summaryRows = new List<ApprovalSummaryRow>() { SummaryRow };
                    var summaryJson = SummaryRow?.SummaryJson?.FromJson<SummaryJson>();
                    ApprovalDetailsEntity approvalDetails = new ApprovalDetailsEntity() { PartitionKey = summaryJson?.ApprovalIdentifier?.GetDocNumber(tenant), RowKey = Constants.SummaryOperationType, ETag = "*", JSONData = summaryRows.ToJson(), TenantID = tenant.TenantId };
                    _tableHelper.InsertOrReplace<ApprovalDetailsEntity>(Constants.ApprovalDetailsAzureTableName, approvalDetails, false);
                    _logProvider.LogInformation(TrackingEvent.UpdateSummarySuccess, logData);
                }
                catch (StorageException exception)
                {
                    if (exception.RequestInformation != null
                            && exception.RequestInformation.ExtendedErrorInformation != null
                            && exception.RequestInformation.ExtendedErrorInformation.ErrorMessage != null)
                    {
                        logData.Add(LogDataKey.ErrorMessage, exception.RequestInformation.ExtendedErrorInformation.ErrorMessage);
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
    }
}