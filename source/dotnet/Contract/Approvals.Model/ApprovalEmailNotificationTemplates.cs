// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace Microsoft.CFS.Approvals.Model
{
    using Microsoft.Azure.Cosmos.Table;

    public class ApprovalEmailNotificationTemplates : TableEntity
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
    }
}