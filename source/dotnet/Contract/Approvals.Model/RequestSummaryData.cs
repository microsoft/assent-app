using System.Collections.Generic;
using Newtonsoft.Json;

public class RequestSummaryData
{
    /// <summary>
    /// Concise, context-aware summary of the request including inferred approval type, total amount,
    /// key dates, approver hierarchy, and synthesized attachment insights.
    /// </summary>
    [JsonProperty("requestSummary")]
    public string RequestSummary { get; set; }

    /// <summary>
    /// Optional list of per-attachment synthesized insights.
    /// Each item has a fixed shape: { "name": "...", "id": "...", "insight": "..." }.
    /// Omit this property or set it to null when there are no attachments.
    /// </summary>
    [JsonProperty("attachmentInsights")]
    public List<AttachmentInsight> AttachmentInsights { get; set; }
}

public class AttachmentInsight
{
    /// <summary>
    /// Attachment label or filename (e.g., "attachment_1" or "invoice.pdf").
    /// </summary>
    [JsonProperty("name")]
    public string Name { get; set; }

    /// <summary>
    /// Unique identifier for the attachment.
    /// </summary>
    [JsonProperty("id")]
    public string Id { get; set; }

    /// <summary>
    /// Summary of key data and any relevant observations for the attachment.
    /// </summary>
    [JsonProperty("insight")]
    public string Insight { get; set; }
}
