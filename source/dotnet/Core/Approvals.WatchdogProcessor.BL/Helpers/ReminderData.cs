// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace Microsoft.CFS.Approvals.WatchdogProcessor.BL.Helpers;

using System;
using System.Collections.Generic;
using Microsoft.CFS.Approvals.Contracts;
using Microsoft.CFS.Approvals.Contracts.DataContracts;
using Microsoft.CFS.Approvals.Data.Azure.Storage.Interface;
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
    /// Constructor of ReminderData
    /// </summary>
    /// <param name="config"></param>
    /// <param name="logger"></param>
    /// <param name="tableHelper"></param>
    public ReminderData(IConfiguration config, ILogProvider logger, ITableHelper tableHelper)
    {
        _config = config;
        _logger = logger;
        _tableHelper = tableHelper;
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
        return approvals;
    }

    /// <summary>
    /// Update Summary Row for updated Next reminder time
    /// </summary>
    /// <param name="approvalTenantInfo"></param>
    /// <param name="summaryToUpdate">summary row to be updated</param>
    public void UpdateReminderInfo(ApprovalTenantInfo approvalTenantInfo, ApprovalSummaryRow summaryToUpdate)
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
        ApprovalDetailsEntity approvalDetails = new ApprovalDetailsEntity() { PartitionKey = summaryJson?.ApprovalIdentifier?.GetDocNumber(approvalTenantInfo), RowKey = Constants.SummaryOperationType, ETag = global::Azure.ETag.All, JSONData = summaryRows.ToJson(), TenantID = approvalTenantInfo.TenantId };

        var summaryTableName = _config[ConfigurationKey.ApprovalSummaryAzureTableName.ToString()];
        try
        {
            _tableHelper.ReplaceRow(summaryTableName, summaryToUpdate);
            _tableHelper.ReplaceRow(Constants.ApprovalDetailsAzureTableName, approvalDetails);
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