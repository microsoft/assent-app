// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace Microsoft.CFS.Approvals.Model;

public class ApprovalEmailNotificationTemplates : BaseTableEntity
{
    public string TemplateContent { get; set; }

    public static string EmailTemplatePartitionKey(string notificationTemplateKey, string actionTaken)
    {
        string pattern;
        if (!string.IsNullOrEmpty(notificationTemplateKey))
        {
            pattern = "^" + notificationTemplateKey + "\\|";
        }
        else
        {
            pattern = "\\|" + actionTaken + "$";
        }
        return pattern;
    }

    public string TemplateName
    {
        get
        {
            return RowKey;
        }
    }

    public string TemplateId { get; set; }
}