// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace Microsoft.CFS.Approvals.Model;

using System;

/// <summary>
/// Entity to store tenant down time messages & status
/// </summary>
[Serializable]
public class TenantDownTimeMessages : BaseTableEntity
{
    #region Constructor

    public TenantDownTimeMessages()
    {
    }

    // isActive is PartitionKey
    // For Production Issue notification, RowKey is the DocTypeId of the Tenant
    public TenantDownTimeMessages(bool isActive)
        : base(isActive.ToString(), Guid.NewGuid().ToString()) { }

    #endregion Constructor

    #region Table Columns

    /// <summary>
    /// Stores the integer TenantId if the notification is applicale to a specific tenant
    /// </summary>
    public int TenantId { get; set; }

    /// <summary>
    /// Boolean flag which signifies whether the notification should be displayed or not
    /// Inherits from the PartitionKey
    /// </summary>
    public Boolean IsActive
    {
        get
        {
            bool result;
            Boolean.TryParse(PartitionKey, out result);
            return result;
        }
    }

    /// <summary>
    /// Boolean flag which signifies if the notification is scheduled
    /// </summary>
    public Boolean IsScheduled { get; set; }

    /// <summary>
    /// Start time of the event
    /// </summary>
    public DateTime EventStartTime { get; set; }

    /// <summary>
    /// End time of the event
    /// </summary>
    public DateTime EventEndTime { get; set; }

    /// <summary>
    /// Alias of the person who has added this notification row
    /// </summary>
    public string CreatedBy { get; set; }

    /// <summary>
    /// Date on which the notification row was created
    /// </summary>
    public DateTime CreatedDate { get; set; }

    /// <summary>
    /// Title of the Notification message
    /// </summary>
    public string NotificationTitle { get; set; }

    /// <summary>
    /// Body of the Notification message
    /// </summary>
    public string NotificationBody { get; set; }

    /// <summary>
    /// Type of Notification message (Info/Danger/Warning)
    /// </summary>
    public string BannerType { get; set; }

    #endregion Table Columns
}