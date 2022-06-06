// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace Microsoft.CFS.Approvals.Common.DL
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Net;
    using System.Threading.Tasks;
    using Microsoft.Azure.Cosmos.Table;
    using Microsoft.CFS.Approvals.Common.DL.Interface;
    using Microsoft.CFS.Approvals.Contracts;
    using Microsoft.CFS.Approvals.Contracts.DataContracts;
    using Microsoft.CFS.Approvals.Data.Azure.Storage.Interface;
    using Microsoft.CFS.Approvals.LogManager.Model;
    using Microsoft.CFS.Approvals.LogManager.Provider.Interface;
    using Microsoft.CFS.Approvals.Model;

    /// <summary>
    /// The Approval Detail Provider class
    /// </summary>
    public class ApprovalDetailProvider : IApprovalDetailProvider
    {
        /// <summary>
        /// The log provider
        /// </summary>
        private readonly ILogProvider _logProvider;

        /// <summary>
        /// The performance logger
        /// </summary>
        private readonly IPerformanceLogger _performanceLogger = null;

        /// <summary>
        /// The approval blob data provider
        /// </summary>
        private readonly IApprovalBlobDataProvider _approvalBlobDataProvider = null;

        /// <summary>
        /// The table helper
        /// </summary>
        private readonly ITableHelper _tableHelper;

        /// <summary>
        /// Initializes a new instance of the <see cref="ApprovalDetailProvider"/> class.
        /// </summary>
        /// <param name="tableHelper">The table helper.</param>
        /// <param name="approvalBlobDataProvider">The approval BLOB data provider.</param>
        /// <param name="logProvider">The log provider.</param>
        /// <param name="performanceLogger">The performance logger.</param>
        public ApprovalDetailProvider(ITableHelper tableHelper, IApprovalBlobDataProvider approvalBlobDataProvider, ILogProvider logProvider, IPerformanceLogger performanceLogger)
        {
            _logProvider = logProvider;
            _performanceLogger = performanceLogger;
            _approvalBlobDataProvider = approvalBlobDataProvider;
            _tableHelper = tableHelper;
        }

        /// <summary>
        /// Adds the request details to the Azure Table Storage and in blob if data is large.
        /// </summary>
        /// <param name="detailsRows"></param>
        /// <param name="isUserTriggered"></param>
        /// <param name="loggedInAlias"></param>
        /// <param name="tcv"></param>
        /// <param name="tenantInfo"></param>
        /// <param name="xcv"></param>
        /// <returns></returns>
        public async Task<bool> AddApprovalsDetails(List<ApprovalDetailsEntity> detailsRows, ApprovalTenantInfo tenantInfo, string loggedInAlias, string xcv, string tcv, bool isUserTriggered = false)
        {
            bool bReturn = true;
            using (var perfTracer = _performanceLogger.StartPerformanceLogger("PerfLog", Constants.WorkerRole, string.Format(Constants.PerfLogActionWithInfo, "Approval Detail Provider", tenantInfo.AppName, "Adds the request details to the Azure Table Storage and in blob if data is large")
                , new Dictionary<LogDataKey, object> { { LogDataKey.DocumentNumber, detailsRows.FirstOrDefault().PartitionKey } }))
            {
                bool blobCheck = false;

                #region Add data in table storage

                foreach (ApprovalDetailsEntity row in detailsRows)
                {
                    var logData = new Dictionary<LogDataKey, object>()
                    {
                        { LogDataKey.Tcv, tcv },
                        { LogDataKey.SessionId, tcv },
                        { LogDataKey.Xcv, xcv },
                        { LogDataKey.UserRoleName, loggedInAlias },
                        { LogDataKey.BusinessProcessName, isUserTriggered ? string.Format(tenantInfo.BusinessProcessName, Constants.BusinessProcessNameAddDetails, Constants.BusinessProcessNameUserTriggered): string.Format(tenantInfo.BusinessProcessName, Constants.BusinessProcessNameAddDetails, Constants.BusinessProcessNameDetailsPrefetched) },
                        { LogDataKey.Operation, row.RowKey },
                        { LogDataKey.DisplayDocumentNumber, row.PartitionKey },
                        { LogDataKey.TenantId, row.TenantID },
                        { LogDataKey.DXcv, row.PartitionKey },
                    };
                    try
                    {
                        #region Insertion in Approval details table

                        try
                        {
                            await _tableHelper.InsertOrReplace<ApprovalDetailsEntity>(Constants.ApprovalDetailsAzureTableName, row, false);
                        }
                        catch (StorageException ex)
                        {
                            // Checks whether exception caused was due to large data.
                            if (ex.RequestInformation.ExtendedErrorInformation.ErrorCode == "PropertyValueTooLarge" || ex.RequestInformation.ExtendedErrorInformation.ErrorCode == "EntityTooLarge" ||
                                ex.RequestInformation.ExtendedErrorInformation.ErrorCode == "RequestBodyTooLarge")
                            {
                                blobCheck = true;
                            }
                            else
                            {
                                throw;
                            }
                        }

                        #endregion Insertion in Approval details table

                        #region Insertion in approvalblobdata

                        if (blobCheck == true)
                        {
                            ApprovalDetailsEntity existingRow = null;
                            existingRow = GetApprovalsDetails(row.TenantID, row.PartitionKey, row.RowKey);
                            if (existingRow == null)
                            {
                                var blobPointer = row.PartitionKey.ToString() + "|" + row.TenantID.ToString() + "|" + row.RowKey.ToString();
                                await _approvalBlobDataProvider.AddApprovalDetails(row, blobPointer);
                                row.BlobPointer = blobPointer;
                                row.JSONData = string.Empty;
                                await _tableHelper.InsertOrReplace<ApprovalDetailsEntity>(Constants.ApprovalDetailsAzureTableName, row, false);
                            }
                        }
                        _logProvider.LogInformation(TrackingEvent.AzureStorageAddRequestDetailsSuccess, logData);

                        #endregion Insertion in approvalblobdata
                    }
                    catch (Exception ex)
                    {
                        if (ex is StorageException storageException && storageException.RequestInformation.HttpStatusCode == (int)HttpStatusCode.Conflict)
                        {
                            continue;
                        }
                        _logProvider.LogError(TrackingEvent.AzureStorageAddRequestDetailsFail, ex, logData);
                        bReturn = false;
                    }
                }

                #endregion Add data in table storage
            }
            return bReturn;
        }

        /// <summary>
        /// Saves a copy of SummaryJson in the ApprovalDetails table
        /// </summary>
        /// <param name="detailsRow"></param>
        /// <param name="tenantInfo"></param>
        /// <param name="telemetry"></param>
        /// <returns></returns>
        public bool AddTransactionalAndHistoricalDataInApprovalsDetails(ApprovalDetailsEntity detailsRow, ApprovalTenantInfo tenantInfo, ApprovalsTelemetry telemetry)
        {
            bool bReturn = true;
            using (var perfTracer = _performanceLogger.StartPerformanceLogger("PerfLog", Constants.WorkerRole, string.Format(Constants.PerfLogActionWithInfo, "Approval Detail Provider", tenantInfo.AppName, "Saves a copy of Summary data to the Azure Table Storage (ApprovalDetails)")
                , new Dictionary<LogDataKey, object> { { LogDataKey.DocumentNumber, detailsRow.PartitionKey } }))
            {
                #region Add data in table storage

                #region Logging

                var logData = new Dictionary<LogDataKey, object>()
                {
                    { LogDataKey.Tcv, telemetry.Tcv },
                    { LogDataKey.SessionId, telemetry.Tcv },
                    { LogDataKey.Xcv, telemetry.Xcv },
                    { LogDataKey.UserRoleName, Environment.UserName },
                    { LogDataKey.BusinessProcessName, telemetry.BusinessProcessName },
                    { LogDataKey.Operation, detailsRow.RowKey },
                    { LogDataKey.DisplayDocumentNumber, detailsRow.PartitionKey },
                    { LogDataKey.TenantId, detailsRow.TenantID },
                    { LogDataKey.DXcv, detailsRow.PartitionKey }
                };

                #endregion Logging

                try
                {
                    _tableHelper.InsertOrReplace<ApprovalDetailsEntity>(Constants.ApprovalDetailsAzureTableName, detailsRow, false);
                }
                catch (Exception ex)
                {
                    _logProvider.LogError(TrackingEvent.AzureStorageAddRequestDetailsFail, ex, logData);
                    bReturn = false;
                }

                #endregion Add data in table storage
            }
            return bReturn;
        }

        /// <summary>
        /// This is a temporary method which will be used by Details Controller
        /// TODO::MD1::
        /// </summary>
        /// <param name="tenantId"></param>
        /// <param name="documentNumber"></param>
        /// <returns></returns>
        public async Task<List<ApprovalDetailsEntity>> GetAllApprovalDetailsByTenantAndDocumentNumber(int tenantId, string documentNumber)
        {
            using (var perfTracer = _performanceLogger.StartPerformanceLogger("PerfLog", Constants.WorkerRole, string.Format(Constants.PerfLogActionWithInfo, "Approval Detail Provider", tenantId.ToString(), "Gets all the request Details from the Azure Storage by tenant id and document number")
                , new Dictionary<LogDataKey, object> { { LogDataKey.DocumentNumber, documentNumber } }))
            {
                var jsonDetails = _tableHelper.GetTableEntityListByPartitionKey<ApprovalDetailsEntity>(
                    Constants.ApprovalDetailsAzureTableName,
                    documentNumber);

                jsonDetails = jsonDetails?.Where(y => y.TenantID == tenantId).ToList();

                List<ApprovalDetailsEntity> tempJsonDetails = new List<ApprovalDetailsEntity>();
                // fetches data from blob if blobPointer exists.
                if (jsonDetails != null)
                {
                    foreach (var filteredSummaryRow in jsonDetails)
                    {
                        if (filteredSummaryRow != null && filteredSummaryRow.BlobPointer != null)
                        {
                            tempJsonDetails.Add(await _approvalBlobDataProvider.GetApprovalDetailsFromBlob(filteredSummaryRow));
                        }
                        else
                        {
                            tempJsonDetails.Add(filteredSummaryRow);
                        }
                    }
                }
                return tempJsonDetails;
            }
        }

        /// <summary>
        /// Gets all the request Details from the Azure Table Storage
        /// </summary>
        /// <param name="tenantId"></param>
        /// <param name="documentNumber"></param>
        /// <returns></returns>
        public List<ApprovalDetailsEntity> GetAllApprovalsDetails(int tenantId, string documentNumber)
        {
            using (var perfTracer = _performanceLogger.StartPerformanceLogger("PerfLog", Constants.WorkerRole, string.Format(Constants.PerfLogActionWithInfo, "Approval Detail Provider", tenantId.ToString(), "Gets all the request Details by document number from the Azure Storage")
                , new Dictionary<LogDataKey, object> { { LogDataKey.DocumentNumber, documentNumber } }))
            {
                return (_tableHelper.GetTableEntityListByPartitionKey<ApprovalDetailsEntity>(
                    Constants.ApprovalDetailsAzureTableName,
                    documentNumber)).Where(d => d.TenantID == tenantId).ToList();
            }
        }

        /// <summary>
        /// Gets all the details from Approval Details table for the given TenantId, DocumentNumber (PartitionKey) and OperationName(RowKey) combination
        /// Gets data from Blob if BlobPointer property has value
        /// </summary>
        /// <param name="tenantId">The int32 tenant id</param>
        /// <param name="documentNumber">Document number of the request</param>
        /// <param name="operationName">Operation Name (DT1 or LINE etc.)</param>
        /// <returns>ApprovalsDetailsEntity object</returns>
        public async Task<ApprovalDetailsEntity> GetApprovalDetailsByOperation(int tenantId, string documentNumber, string operationName)
        {
            var jsonDetails = _tableHelper.GetTableEntityListByPartitionKey<ApprovalDetailsEntity>(
                    Constants.ApprovalDetailsAzureTableName,
                    documentNumber);
            var filteredDetailsRow = jsonDetails?.FirstOrDefault(y => y.RowKey == operationName && y.TenantID == tenantId);

            if (filteredDetailsRow != null && !string.IsNullOrEmpty(filteredDetailsRow.BlobPointer))
            {
                return await _approvalBlobDataProvider.GetApprovalDetailsFromBlob(filteredDetailsRow);
            }
            else
            {
                return filteredDetailsRow;
            }
        }

        /// <summary>
        /// Gets all the details from Approval Details table for the given TenantId, DocumentNumber (PartitionKey) and OperationName(RowKey) combination
        /// </summary>
        /// <param name="tenantId">The int32 tenant id</param>
        /// <param name="documentNumber">Document number of the request</param>
        /// <param name="operationName">Operation Name (DT1 or LINE etc.)</param>
        /// <returns>ApprovalsDetailsEntity object</returns>
        public ApprovalDetailsEntity GetApprovalsDetails(int tenantId, string documentNumber, string operationName)
        {
            using (var perfTracer = _performanceLogger.StartPerformanceLogger("PerfLog", Constants.WorkerRole, string.Format(Constants.PerfLogActionWithInfo, "Approval Detail Provider", tenantId.ToString(), "Gets all the request Details from the Azure Storage by tenant id, document number and operation name")
                , new Dictionary<LogDataKey, object> { { LogDataKey.DocumentNumber, documentNumber } }))
            {
                var jsonDetails = _tableHelper.GetTableEntityListByPartitionKey<ApprovalDetailsEntity>(
                    Constants.ApprovalDetailsAzureTableName,
                    documentNumber);
                var filteredDetailsRows = jsonDetails?.FirstOrDefault(y => y.RowKey == operationName && y.TenantID == tenantId);
                return filteredDetailsRows;
            }
        }

        /// <summary>
        /// Removes the request details from the Azure Table Storage
        /// </summary>
        /// <param name="detailsRows"></param>
        /// <returns></returns>
        public async Task<AzureTableRowDeletionResult> RemoveApprovalsDetails(List<ApprovalDetailsEntity> detailsRows)
        {
            using (var perfTracer = _performanceLogger.StartPerformanceLogger("PerfLog", Constants.WorkerRole, string.Format(Constants.PerfLogAction, "Approval Detail Provider", "Removes the request details from the Azure Table Storage and in blob if data is large")
                , new Dictionary<LogDataKey, object> { { LogDataKey.DocumentNumbers, string.Join(",", detailsRows.Select(d => d.PartitionKey).ToList()) } }))
            {
                if (detailsRows.Count > 0)
                {
                    foreach (ApprovalDetailsEntity row in detailsRows)
                    {
                        await _tableHelper.DeleteRow<ApprovalDetailsEntity>(Constants.ApprovalDetailsAzureTableName, row);

                        // Remove from blob if blobPointer exists.
                        if (row.BlobPointer != null)
                        {
                            await _approvalBlobDataProvider.DeleteBlobData(row.BlobPointer);
                        }
                    }
                    return AzureTableRowDeletionResult.DeletionSuccessful;
                }
                else
                {
                    return AzureTableRowDeletionResult.SkippedDueToNonExistence;
                }
            }
        }

        /// <summary>
        /// Deletes the Blob entries for attachments
        /// </summary>
        /// <param name="attachments"></param>
        /// <param name="approvalIdentifier"></param>
        /// <param name="operationName"></param>
        /// <param name="clientDevice"></param>
        /// <param name="tenantId"></param>
        /// <param name="telemetry"></param>
        public async Task RemoveAttachmentFromBlob(List<Attachment> attachments, ApprovalIdentifier approvalIdentifier, string operationName, string clientDevice, string tenantId, ApprovalsTelemetry telemetry)
        {
            var logData = new Dictionary<LogDataKey, object>();
            try
            {
                logData.Add(LogDataKey.DXcv, approvalIdentifier.DisplayDocumentNumber);
                logData.Add(LogDataKey.Xcv, telemetry?.Xcv);
                logData.Add(LogDataKey.Tcv, telemetry?.Tcv);
                logData.Add(LogDataKey.UserRoleName, Constants.WorkerRole);
                logData.Add(LogDataKey.ClientDevice, clientDevice);
                logData.Add(LogDataKey.TenantId, tenantId);

                foreach (var attachment in attachments)
                {
                    //Form the blob pointer
                    string blobNameFormat = "{0}|{1}|{2}"; //2(tenantId)|572015453(DocumentNumber)|45124525(attachmentId)
                    string blobName = string.Format(blobNameFormat, tenantId, approvalIdentifier.DisplayDocumentNumber, attachment.ID?.ToString() ?? string.Empty);
                    await _approvalBlobDataProvider.DeleteBlobData(blobName, Constants.NotificationAttachmentsBlobName);
                }
            }
            catch (Exception ex)
            {
                // Log Exception
                _logProvider.LogError(TrackingEvent.AttachmentDeleteFailed, ex, logData);
            }
        }

        /// <summary>
        /// Method to Insert/ Replace editable details row into ApprovalDetails table
        /// </summary>
        /// <param name="detailsRow"></param>
        /// <param name="telemetry"></param>
        /// <returns></returns>
        public bool SaveEditedDataInApprovalDetails(ApprovalDetailsEntity detailsRow, ApprovalsTelemetry telemetry)
        {
            bool bReturn = true;

            #region Add data in table storage

            #region Logging

            var logData = new Dictionary<LogDataKey, object>()
            {
                { LogDataKey.Tcv, telemetry.Tcv },
                { LogDataKey.SessionId, telemetry.Tcv },
                { LogDataKey.Xcv, telemetry.Xcv },
                { LogDataKey.UserRoleName, Environment.UserName },
                { LogDataKey.BusinessProcessName, telemetry.BusinessProcessName },
                { LogDataKey.Operation, detailsRow.RowKey },
                { LogDataKey.DisplayDocumentNumber, detailsRow.PartitionKey },
                { LogDataKey.TenantId, detailsRow.TenantID },
                { LogDataKey.DXcv, detailsRow.PartitionKey }
            };

            #endregion Logging

            try
            {
                _tableHelper.InsertOrReplace<ApprovalDetailsEntity>(Constants.ApprovalDetailsAzureTableName, detailsRow, false);
            }
            catch (Exception ex)
            {
                _logProvider.LogError(TrackingEvent.AzureStorageAddRequestDetailsFail, ex, logData);
                bReturn = false;
            }

            #endregion Add data in table storage

            return bReturn;
        }

        /// <summary>
        /// Updates the list of Approval Details in batch.
        /// </summary>
        /// <param name="detailsEntities">List of Approval detail rows.</param>
        /// <param name="xcv">xcv.</param>
        /// <param name="sessionId">sessionId.</param>
        /// <param name="tcv">tcv.</param>
        /// <param name="tenantInfo">Approval Tenant Info</param>
        /// <param name="actionName">Action Name.</param>
        /// <returns>Async method updating details in batch</returns>
        public async Task UpdateDetailsInBatchAsync(List<ApprovalDetailsEntity> detailsEntities, string xcv, string sessionId,
            string tcv, ApprovalTenantInfo tenantInfo,
            string actionName)
        {
            using (var perfTracer = _performanceLogger.StartPerformanceLogger("PerfLog", Constants.WorkerRole,
                string.Format(Constants.PerfLogActionWithInfo, "Approval Detail Provider", tenantInfo.AppName,
                    "Update list of approval details in batch")
                , new Dictionary<LogDataKey, object> { { LogDataKey.DocumentNumber, detailsEntities[0].PartitionKey } }))
            {
                var logData = new Dictionary<LogDataKey, object>()
                {
                    { LogDataKey.StartDateTime, DateTime.UtcNow },
                    { LogDataKey.Xcv, xcv },
                    { LogDataKey.Tcv, tcv },
                    { LogDataKey.SessionId, sessionId },
                    { LogDataKey.ReceivedTcv, tcv },
                    { LogDataKey.BusinessProcessName, string.Format(tenantInfo.BusinessProcessName, Constants.BusinessProcessNameApprovalAction, actionName) }
                };
                try
                {
                    _logProvider.LogInformation(TrackingEvent.BatchUpdateDetailsInitiated, logData);
                    await _tableHelper.InsertOrReplaceRows<ApprovalDetailsEntity>(Constants.ApprovalDetailsAzureTableName, detailsEntities, caseConstraint: false);
                    logData.Add(LogDataKey.EndDateTime, DateTime.UtcNow);
                    _logProvider.LogInformation(TrackingEvent.BatchUpdateDetailsSuccess, logData);
                }
                catch (StorageException exception)
                {
                    if (!logData.ContainsKey(LogDataKey.EndDateTime))
                    {
                        logData.Add(LogDataKey.EndDateTime, DateTime.UtcNow);
                    }
                    if (exception.RequestInformation != null && exception.RequestInformation.ExtendedErrorInformation != null && exception.RequestInformation.ExtendedErrorInformation.ErrorMessage != null)
                    {
                        logData.Add(LogDataKey.ErrorMessage, exception.RequestInformation.ExtendedErrorInformation.ErrorMessage);
                    }
                    _logProvider.LogError(TrackingEvent.BatchUpdateDetailsFailed, exception, logData);
                }
                catch (Exception exception)
                {
                    if (!logData.ContainsKey(LogDataKey.EndDateTime))
                    {
                        logData.Add(LogDataKey.EndDateTime, DateTime.UtcNow);
                    }
                    _logProvider.LogError(TrackingEvent.BatchUpdateDetailsFailed, exception, logData);
                }
            }
        }
    }
}