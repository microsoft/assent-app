namespace Microsoft.CFS.Approvals.Model;

using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using NCrontab;
using Newtonsoft.Json;

/// <summary>
/// Notification Item
/// </summary>
public class NotificationItem
{
    /// <summary>
    /// Initializes a new instance of the <see cref="NotificationItem" /> class.
    /// </summary>
    public NotificationItem()
    {
    }

    /// <summary>
    /// Gets or sets the type of the notification.
    /// </summary>
    [JsonProperty("notificationTypes")]
    public List<NotificationType> NotificationTypes { get; set; }

    /// <summary>
    /// Gets or sets the name of the application.
    /// </summary>
    [JsonProperty("applicationName")]
    public string ApplicationName { get; set; }

    /// <summary>
    /// Gets or sets the tag for the notification.
    /// </summary>
    [JsonProperty("webPushnotificationTag")]
    public string WebPushNotificationTag { get; set; }

    /// <summary>
    /// Gets or sets the deeplink url for the notification.
    /// </summary>
    [JsonProperty("deeplinkUrl")]
    public string DeeplinkUrl { get; set; }

    /// <summary>
    /// Gets or sets the send on UTC date.
    /// </summary>
    [JsonProperty("sendOnUtcDate")]
    public DateTime? SendOnUtcDate { get; set; }

    /// <summary>
    /// Gets or sets from.
    /// </summary>
    [JsonProperty("from")]
    public string From { get; set; }

    /// <summary>
    /// Gets or sets to.
    /// </summary>
    [JsonProperty("to")]
    public string To { get; set; }

    /// <summary>
    /// Gets or sets the cc.
    /// </summary>
    [JsonProperty("cc")]
    public string Cc { get; set; }

    /// <summary>
    /// Gets or sets the BCC.
    /// </summary>
    [JsonProperty("bcc")]
    public string Bcc { get; set; }

    /// <summary>
    /// Gets or sets the subject.
    /// </summary>
    [JsonProperty("subject")]
    public string Subject { get; set; }

    /// <summary>
    /// Gets or sets the body.
    /// </summary>
    [JsonProperty("body")]
    public string Body { get; set; }

    /// <summary>
    /// Gets or sets the attachments.
    /// </summary>
    [JsonProperty("attachments")]
    public List<NotificationDataAttachment> Attachments { get; set; }

    /// <summary>
    /// Gets or sets the tenantIdentifier.
    /// </summary>
    [JsonProperty("tenantIdentifier")]
    [Required]
    public string TenantIdentifier { get; set; }

    /// <summary>
    /// Gets or sets the TemplateId.
    /// </summary>
    [JsonProperty("templateId")]
    public string TemplateId { get; set; }

    /// <summary>
    /// Gets or sets the Template Content Arguments.
    /// </summary>
    [JsonProperty("templateData")]
    public Dictionary<string, object> TemplateData { get; set; }

    /// <summary>
    /// Gets or sets the telemetry object.
    /// </summary>
    [JsonProperty("telemetry")]
    [Required]
    public Telemetry Telemetry { get; set; }

    /// <summary>
    /// Gets or sets the Email account number to be attempted to send email
    /// </summary>
    [JsonProperty("emailAccountNumberToUse")]
    public string EmailAccountNumberToUse { get; set; }

    /// <summary>
    /// Gets or sets the attachment blob name.
    /// </summary>
    [JsonProperty("attachmentBlobName")]
    public string AttachmentBlobName { get; set; }

    /// <summary>
    /// For watch dog reminder notification - Type of ReminderDetail
    /// </summary>
    [JsonProperty("reminder")]
    public ReminderDetail Reminder { get; set; }

    /// <summary>
    /// Gets or sets Id.
    /// </summary>
    [JsonProperty("id")]
    public string Id { get; set; }

    /// <summary>
    /// Gets or sets Sequence Number.
    /// </summary>
    [JsonProperty("sequenceNumber")]
    public long SequenceNumber { get; set; }

    [JsonProperty("culture")]
    public string Culture { get; set; }

    /// <summary>
    /// Gets or sets PartitionKey for Template
    /// </summary>
    public List<string> TemplatePartitionKey;

    /// <summary>
    /// Gets or sets Template expressions
    /// </summary>
    public string TemplateRowExp;
}

/// <summary>
/// Attachment
/// </summary>
public class NotificationDataAttachment
{
    /// <summary>
    /// Gets or sets the name of the file.
    /// </summary>
    /// <value>
    /// The name of the file.
    /// </value>
    [JsonProperty("fileName")]
    public string FileName { get; set; }

    /// <summary>
    /// Gets or sets the file base64.
    /// </summary>
    [JsonProperty("fileBase64")]
    public string FileBase64 { get; set; }

    [JsonProperty("fileType")]
    public string FileType { get; set; }

    /// <summary>
    /// Gets or sets the file url.
    /// </summary>
    [JsonProperty("fileUrl")]
    public string FileUrl { get; set; }

    [JsonProperty("cid")]
    public string EmbeddedContentId { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether this instance is inline.
    /// </summary>
    public bool IsInline { get; set; }

    /// <summary>
    /// Gets or sets File size in bytes
    /// </summary>
    public long FileSize { get; set; }
}

/// <summary>
/// Telemetry
/// </summary>
public class Telemetry
{
    /// <summary>
    /// Gets or sets the file Xcv.
    /// </summary>
    [JsonProperty("xcv")]
    [Required]
    public string Xcv { get; set; }

    /// <summary>
    /// Gets or sets the file MessageId.
    /// </summary>
    [JsonProperty("messageId")]
    [Required]
    public string MessageId { get; set; }
}

/// <summary>
/// Reminder detail
/// </summary>
public class ReminderDetail
{
    /// <summary>
    /// Gets or sets the type of the notification.
    /// </summary>
    [JsonProperty("notificationTypes")]
    public List<NotificationType> NotificationTypes { get; set; }

    /// <summary>
    /// CRON Expression of the reminder notification.
    /// </summary>
    [JsonProperty("expression")]
    public string Expression { get; set; }

    /// <summary>
    /// Frequency of the reminder notification (in hours).
    /// </summary>
    [JsonProperty("frequency")]
    public double Frequency { get; set; }

    /// <summary>
    /// Reminder Expiration Date.
    /// </summary>
    [JsonProperty("expirationDate")]
    public DateTime ExpirationDate { get; set; }

    /// <summary>
    /// Next Reminder Date.
    /// </summary>
    public DateTime NextReminderDate
    {
        get
        {
            // Get next occurence time as per the reminder expression/frequency
            if (Frequency > 0)
            {
                return DateTime.UtcNow.AddHours(Frequency);
            }
            else
            {
                var date = CrontabSchedule.TryParse(Expression)?.GetNextOccurrence(DateTime.UtcNow);
                return (date != null && date <= ExpirationDate) ? (DateTime)date : DateTime.MinValue;
            }
        }
    }
}

/// <summary>
/// NotificationType
/// </summary>
public enum NotificationType
{
    Mail,
    ActionableEmail,
    Tile,
    Toast,
    Badge,
    Raw,
    WebPush,
    Text,
    Cancel
}