// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace Microsoft.CFS.Approvals.Common.BL;

using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using Microsoft.CFS.Approvals.Common.DL.Interface;
using Microsoft.CFS.Approvals.Model;

/// <summary>
/// This will be use to featch Notification Template
/// </summary>
public class NotificationTemplateFactory
{
    /// <summary>
    /// The email template helper
    /// </summary>
    private readonly IEmailTemplateHelper _templateHelper;

    /// <summary>
    /// Constructor of NotificationTemplateFactory
    /// </summary>
    /// <param name="_templateHelper">EmailTemplate Helper to fetch template</param>
    public NotificationTemplateFactory(IEmailTemplateHelper templateHelper)
    {
        _templateHelper = templateHelper;
    }

    /// <summary>
    /// This method will fetch email template based on input data
    /// </summary>
    /// <param name="data">Instance of notification data</param>
    /// <returns>Returns email template</returns>
    public string GetEmailTemplate(NotificationData data)
    {
        string templateBody = string.Empty;
        if (!string.IsNullOrWhiteSpace(data?.TemplateRowExp))
        {
            templateBody = GetSpecificDeviceTemplate(data?.TemplatePartitionKey?.FirstOrDefault(), data?.TemplateRowExp)?.TemplateContent;
        }

        return templateBody;
    }

    /// <summary>
    /// This method will fetch device specific template from storage.
    /// </summary>
    /// <param name="partitionKey">The partitionkey of storage</param>
    /// <param name="rowKey">The rowkey of storage</param>
    /// <returns>Returns ApprovalEmailNotificationTemplates</returns>
    private ApprovalEmailNotificationTemplates GetSpecificDeviceTemplate(string partitionKey, string rowKey)
    {
        IEnumerable<ApprovalEmailNotificationTemplates> possibleTemplates;
        if (!string.IsNullOrWhiteSpace(partitionKey))
        {
            possibleTemplates = _templateHelper?.GetTemplates(partitionKey);
        }
        else
        {
            possibleTemplates = _templateHelper?.GetTemplates();
        }

        return possibleTemplates?.Where(template => Regex.IsMatch(template.TemplateName, rowKey))?.FirstOrDefault();
    }
}
