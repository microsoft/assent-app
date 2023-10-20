// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace Microsoft.CFS.Approvals.Core.BL.Interface;

using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.CFS.Approvals.Domain.BL.Interface;
using Microsoft.CFS.Approvals.Model;

public interface INotificationProvider
{
    ITenant Tenant { get; set; }

    /// <summary>
    /// Send email
    /// </summary>
    /// <param name="to"></param>
    /// <param name="subject"></param>
    /// <param name="body"></param>
    Task SendEmail(string to, string subject, string body);

    /// <summary>
    /// Send email
    /// </summary>
    /// <param name="to"></param>
    /// <param name="template"></param>
    /// <param name="templateData"></param>
    Task SendEmail(string to, string template, Dictionary<string, string> templateData);

    /// <summary>
    /// Send email
    /// </summary>
    /// <param name="data"></param>
    Task SendEmail(NotificationItem data);

    /// <summary>
    /// Send emails
    /// </summary>
    /// <param name="data"></param>
    Task SendEmails(IEnumerable<NotificationItem> data);

    /// <summary>
    /// Send device notification
    /// </summary>
    /// <param name="to"></param>
    /// <param name="message"></param>
    Task SendDeviceNotification(string to, string message);

    /// <summary>
    /// Send device notification
    /// </summary>
    /// <param name="data"></param>
    Task SendDeviceNotification(NotificationItem data);
}