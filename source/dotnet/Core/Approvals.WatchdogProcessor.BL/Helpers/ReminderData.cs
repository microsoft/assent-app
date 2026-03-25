// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace Microsoft.CFS.Approvals.WatchdogProcessor.BL.Helpers;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.CFS.Approvals.Common.DL;
using Microsoft.CFS.Approvals.Common.DL.Interface;
using Microsoft.CFS.Approvals.Contracts;
using Microsoft.CFS.Approvals.Contracts.DataContracts;
using Microsoft.CFS.Approvals.Data.Azure.Storage.Interface;
using Microsoft.CFS.Approvals.Domain.BL.Interface;
using Microsoft.CFS.Approvals.Extensions;
using Microsoft.CFS.Approvals.LogManager.Model;
using Microsoft.CFS.Approvals.LogManager.Provider.Interface;
using Microsoft.CFS.Approvals.Model;
using Microsoft.CFS.Approvals.Utilities.Extension;
using Microsoft.CFS.Approvals.WatchdogProcessor.BL.Interface;
using Microsoft.Extensions.Configuration;

/// <summary>
/// The Reminder Data class
/// </summary>
public class ReminderData : IReminderData
{
    /// <summary>
    /// The configuration
    /// </summary>
    private readonly IConfiguration _config = null;

    /// <summary>
    /// The log provider
    /// </summary>
    private readonly ILogProvider _logger;

    /// <summary>
    /// The table helper
    /// </summary>
    private readonly ITableHelper _tableHelper = null;

    /// <summary>
    /// The approval blob data provider
    /// </summary>
    private readonly IApprovalBlobDataProvider _approvalBlobDataProvider = null;

    /// <summary>
    /// The approval detail provider
    /// </summary>
    private readonly IApprovalDetailProvider _approvalDetailProvider = null;

    /// <summary>
    /// Constructor of ReminderData
    /// </summary>
    /// <param name="config"></param>
    /// <param name="logger"></param>
    /// <param name="tableHelper"></param>
    /// <param name="approvalBlobDataProvider"></param>
    /// <param name="approvalDetailProvider"></param>
    public ReminderData(IConfiguration config, ILogProvider logger, ITableHelper tableHelper, IApprovalBlobDataProvider approvalBlobDataProvider, IApprovalDetailProvider approvalDetailProvider)
    {
        _config = config;
        _logger = logger;
        _tableHelper = tableHelper;
        _approvalBlobDataProvider = approvalBlobDataProvider;
        _approvalDetailProvider = approvalDetailProvider;
    }

    #region Implemented Methods

    /// <summary>
    /// Gets summary rows for which watchdog reminders need to be sent
    /// </summary>
    /// <param name="currentTime">Current UTC Time</param>
    /// <param name="approvalTenantInfo">ApprovalTenantInfo for which Digest Email functionality is enabled</param>
    /// <returns>List of summary rows needing watchdog reminders</returns>
    public IEnumerable<ApprovalSummaryRow> GetApprovalsNeedingReminders(DateTime currentTime, List<ApprovalTenantInfo> approvalTenantInfo)
    {
        int daysForReminderMails = Convert.ToInt32(_config[ConfigurationKey.DaysForReminderMails.ToString()]);

        string query = "NextReminderTime le datetime'" + currentTime.ToString("yyyy-MM-ddTHH:mm:ssZ") + "' and NextReminderTime ge datetime'" + currentTime.Subtract(new TimeSpan(daysForReminderMails, 0, 0, 0)).ToString("yyyy-MM-ddTHH:mm:ssZ") + "'";

        List<ApprovalSummaryRow> approvals = _tableHelper.GetDataCollectionByTableQuerySegmented<ApprovalSummaryRow>(_config[ConfigurationKey.ApprovalSummaryAzureTableName.ToString()], query);
        return (approvals.Select(row => !string.IsNullOrEmpty(row.BlobPointer) ? _approvalBlobDataProvider.GetApprovalSummaryJsonFromBlob(row).Result : row))?.ToList();
    }

    /// <summary>
    /// Update Summary Row for updated Next reminder time
    /// </summary>
    /// <param name="approvalTenantInfo"></param>
    /// <param name="summaryToUpdate">summary row to be updated</param>
    public async Task UpdateReminderInfo(ApprovalTenantInfo approvalTenantInfo, ApprovalSummaryRow summaryToUpdate)
    {
        var logData = new Dictionary<LogDataKey, object>
    {
        { LogDataKey.Xcv, summaryToUpdate.Xcv },
        { LogDataKey.Tcv, summaryToUpdate.Tcv },
        { LogDataKey.ReceivedTcv, summaryToUpdate.Tcv },
        { LogDataKey.DXcv, summaryToUpdate.DocumentNumber }
    };
        if (approvalTenantInfo != null)
            logData.Add(LogDataKey.BusinessProcessName, string.Format(approvalTenantInfo.BusinessProcessName, Constants.BusinessProcessNameSendNotificationToUser, Constants.BusinessProcessNameSendNotificationWatchdogReminder));

        List<ApprovalSummaryRow> summaryRows = new List<ApprovalSummaryRow>() { summaryToUpdate };
        var summaryJson = summaryToUpdate?.SummaryJson?.FromJson<SummaryJson>();

        // Get all approval details data and check if it has row key = SUM or SUM|doctypeid
        var allApprovalDetails = await _approvalDetailProvider.GetAllApprovalDetailsByTenantAndDocumentNumber(approvalTenantInfo.TenantId, summaryJson?.ApprovalIdentifier?.GetDocNumber(approvalTenantInfo));
        var summaryOperationRowFromDetails = allApprovalDetails?.FirstOrDefault(detail =>
            detail.RowKey.Equals(Constants.SummaryOperationType, StringComparison.InvariantCultureIgnoreCase) ||
            detail.RowKey.Equals(string.Format(Constants.SummaryOperationTypeNew, approvalTenantInfo.DocTypeId), StringComparison.InvariantCultureIgnoreCase));

        ApprovalDetailsEntity approvalDetails = new ApprovalDetailsEntity() 
        { 
            PartitionKey = summaryJson?.ApprovalIdentifier?.GetDocNumber(approvalTenantInfo), 
            RowKey = summaryOperationRowFromDetails?.RowKey ?? string.Format(Constants.SummaryOperationTypeNew, approvalTenantInfo.DocTypeId), 
            ETag = global::Azure.ETag.All, 
            JSONData = summaryRows.ToJson(), 
            TenantID = approvalTenantInfo.TenantId 
        };

        var summaryTableName = _config[ConfigurationKey.ApprovalSummaryAzureTableName.ToString()];
        try
        {   
            //setting this to empty to avoid insertion error in case summaryJson exceeds 64kb
            if (!string.IsNullOrWhiteSpace(summaryToUpdate.BlobPointer))
                summaryToUpdate.SummaryJson = string.Empty;
            _tableHelper.ReplaceRow(summaryTableName, summaryToUpdate);

            _approvalDetailProvider.AddTransactionalAndHistoricalDataInApprovalsDetails(approvalDetails, approvalTenantInfo, new ApprovalsTelemetry() { Xcv = summaryToUpdate.Xcv, Tcv = string.Empty, BusinessProcessName = string.Empty }).Wait();

            _logger.LogInformation(TrackingEvent.WatchDogUpdateReminderInfoComplete, logData);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(TrackingEvent.WatchDogReminderUpdateException, logData, ex);
            var changedRow = RetrieveSummaryRow(summaryToUpdate.PartitionKey, summaryToUpdate.RowKey);
            changedRow.NextReminderTime = summaryToUpdate.NextReminderTime;
            _tableHelper.ReplaceRow(summaryTableName, changedRow);
        }
    }

    #endregion Implemented Methods

    #region ReminderData Methods

    /// <summary>
    /// Retrieve summary row
    /// </summary>
    /// <param name="partitionKey"></param>
    /// <param name="rowKey"></param>
    /// <returns></returns>
    private ApprovalSummaryRow RetrieveSummaryRow(string partitionKey, string rowKey)
    {
        var logData = new Dictionary<LogDataKey, object>();
        try
        {
            var approval = _tableHelper.GetTableEntityByPartitionKeyAndRowKey<ApprovalSummaryRow>(_config[ConfigurationKey.ApprovalSummaryAzureTableName.ToString()], partitionKey, rowKey);
            if (!string.IsNullOrEmpty(approval.BlobPointer))
                approval = _approvalBlobDataProvider.GetApprovalSummaryJsonFromBlob(approval).Result;

            _logger.LogInformation(TrackingEvent.WatchDogReminderRetrieveSummaryRowsSuccess, logData);
            return approval;
        }
        catch (Exception ex)
        {
            _logger.LogError<TrackingEvent, LogDataKey>(TrackingEvent.WatchDogReminderRetrieveSummaryRowsFail, ex);
            throw;
        }
    }

    #endregion ReminderData Methods
}