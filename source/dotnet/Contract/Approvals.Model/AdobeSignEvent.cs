namespace Microsoft.CFS.Approvals.Model;

using System;
using System.Collections.Generic;
using Newtonsoft.Json;

public class AdobeSignEvent
{
    [JsonProperty("webhookId")]
    public string WebhookId { get; set; }

    [JsonProperty("webhookName")]
    public string WebhookName { get; set; }

    [JsonProperty("webhookNotificationId")]
    public string WebhookNotificationId { get; set; }

    [JsonProperty("webhookUrlInfo")]
    public WebhookUrlInfo WebhookUrlInfo { get; set; }

    [JsonProperty("webhookScope")]
    public string WebhookScope { get; set; }

    [JsonProperty("event")]
    public string Event { get; set; }

    [JsonProperty("eventDate")]
    public DateTime EventDate { get; set; }

    [JsonProperty("eventDateTimezoneOffset")]
    public string EventDateTimezoneOffset { get; set; }

    [JsonProperty("eventResourceType")]
    public string EventResourceType { get; set; }

    [JsonProperty("participantRole")]
    public string ParticipantRole { get; set; }

    [JsonProperty("actionType")]
    public string ActionType { get; set; }

    [JsonProperty("participantUserId")]
    public string ParticipantUserId { get; set; }

    [JsonProperty("participantUserEmail")]
    public string ParticipantUserEmail { get; set; }

    [JsonProperty("actingUserId")]
    public string ActingUserId { get; set; }

    [JsonProperty("actingUserEmail")]
    public string ActingUserEmail { get; set; }

    [JsonProperty("actingUserIpAddress")]
    public string ActingUserIpAddress { get; set; }

    [JsonProperty("initiatingUserId")]
    public string InitiatingUserId { get; set; }

    [JsonProperty("initiatingUserEmail")]
    public string InitiatingUserEmail { get; set; }

    [JsonProperty("agreement")]
    public Agreement Agreement { get; set; }

    [JsonProperty("agreementCancellationInfo")]
    public AgreementCancellationInfo AgreementCancellationInfo { get; set; }
}

public class Agreement
{
    [JsonProperty("id")]
    public string Id { get; set; }

    [JsonProperty("name")]
    public string Name { get; set; }

    [JsonProperty("signatureType")]
    public string SignatureType { get; set; }

    [JsonProperty("status")]
    public string Status { get; set; }

    [JsonProperty("documentVisibilityEnabled")]
    public bool DocumentVisibilityEnabled { get; set; }

    [JsonProperty("createdDate")]
    public DateTime CreatedDate { get; set; }

    [JsonProperty("expirationTime")]
    public DateTime ExpirationTime { get; set; }

    [JsonProperty("firstReminderDelay")]
    public int FirstReminderDelay { get; set; }

    [JsonProperty("locale")]
    public string Locale { get; set; }

    [JsonProperty("message")]
    public string Message { get; set; }

    [JsonProperty("reminderFrequency")]
    public string ReminderFrequency { get; set; }

    [JsonProperty("senderEmail")]
    public string SenderEmail { get; set; }

    [JsonProperty("participantSetsInfo")]
    public ParticipantSetsInfo ParticipantSetsInfo { get; set; }

    [JsonProperty("documentsInfo")]
    public DocumentsInfo DocumentsInfo { get; set; }

    [JsonProperty("createdGroupId")]
    public string CreatedGroupId { get; set; }
}

public class Document
{
    [JsonProperty("id")]
    public string Id { get; set; }

    [JsonProperty("numPages")]
    public int NumPages { get; set; }

    [JsonProperty("mimeType")]
    public string MimeType { get; set; }

    [JsonProperty("name")]
    public string Name { get; set; }
}

public class DocumentsInfo
{
    [JsonProperty("documents")]
    public List<Document> Documents { get; set; }
}

public class MemberInfo
{
    [JsonProperty("id")]
    public string Id { get; set; }

    [JsonProperty("email")]
    public string Email { get; set; }

    [JsonProperty("company")]
    public string Company { get; set; }

    [JsonProperty("name")]
    public string Name { get; set; }

    [JsonProperty("status")]
    public string Status { get; set; }

    [JsonProperty("userId")]
    public string UserId { get; set; }

    [JsonProperty("securityOption")]
    public SecurityOption SecurityOption { get; set; }
}

public class ParticipantSet
{
    [JsonProperty("memberInfos")]
    public List<MemberInfo> MemberInfos { get; set; }

    [JsonProperty("order")]
    public int Order { get; set; }

    [JsonProperty("role")]
    public string Role { get; set; }

    [JsonProperty("status")]
    public string Status { get; set; }

    [JsonProperty("id")]
    public string Id { get; set; }
}

public class ParticipantSetsInfo
{
    [JsonProperty("participantSets")]
    public List<ParticipantSet> ParticipantSets { get; set; }
}

public class SecurityOption
{
    [JsonProperty("authenticationMethod")]
    public string AuthenticationMethod { get; set; }
}

public class WebhookUrlInfo
{
    [JsonProperty("url")]
    public string Url { get; set; }
}

public class AgreementCancellationInfo
{
    [JsonProperty("comment")]
    public string Comment { get; set; }
    [JsonProperty("notifyOthers")]
    public string NotifyOthers { get; set; }
}
