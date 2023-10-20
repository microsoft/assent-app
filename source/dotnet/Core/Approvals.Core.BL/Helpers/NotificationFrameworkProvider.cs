// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace Microsoft.CFS.Approvals.Core.BL.Helpers;

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Microsoft.CFS.Approvals.Common.BL;
using Microsoft.CFS.Approvals.Common.DL.Interface;
using Microsoft.CFS.Approvals.Contracts;
using Microsoft.CFS.Approvals.Core.BL.Interface;
using Microsoft.CFS.Approvals.Domain.BL.Interface;
using Microsoft.CFS.Approvals.Extensions;
using Microsoft.CFS.Approvals.LogManager.Model;
using Microsoft.CFS.Approvals.LogManager.Provider.Interface;
using Microsoft.CFS.Approvals.Model;
using Microsoft.CFS.Approvals.Utilities.Interface;
using Microsoft.Extensions.Configuration;
using WebMarkupMin.Core;
using NotificationType = Model.NotificationType;

public class NotificationFrameworkProvider : INotificationProvider
{
    /// <summary>
    /// The http helper
    /// </summary>
    private readonly IHttpHelper _httpHelper;

    /// <summary>
    /// The notification template factory
    /// </summary>
    private readonly NotificationTemplateFactory _templateFactory;

    /// <summary>
    /// The log provider
    /// </summary>
    private readonly ILogProvider _logProvider = null;

    /// <summary>
    /// The configuration
    /// </summary>
    private readonly IConfiguration _config;

    /// <summary>
    /// The notification broadcast uri
    /// </summary>
    private readonly string _notificationBroadcastUri = null;

    /// <summary>
    /// Tenant specific configuration.
    /// </summary>
    public ITenant Tenant { get; set; }

    /// <summary>
    /// Constructor to create instance of Notification Provider
    /// </summary>
    /// <param name="httpHelper">The http helper</param>
    /// <param name="templateHelper">The email template helper</param>
    /// <param name="logProvider">Logger to log error</param>
    /// <param name="config">Configuration Helper to read configuration</param>
    public NotificationFrameworkProvider(IHttpHelper httpHelper, IEmailTemplateHelper templateHelper, ILogProvider logProvider, IConfiguration config)
    {
        _httpHelper = httpHelper;
        _templateFactory = new NotificationTemplateFactory(templateHelper);
        _logProvider = logProvider;
        _config = config;
        _notificationBroadcastUri = config[ConfigurationKey.NotificationBroadcastUri.ToString()];
    }

    /// <summary>
    /// This method will send email based on To and Message property.
    /// </summary>
    /// <param name="to">To property to send email</param>
    /// <param name="message">Message to send notification</param>
    public async Task SendDeviceNotification(string to, string message)
    {
        await SendToNotificationFramework(new NotificationItem { To = to, Body = message });
    }

    /// <summary>
    /// This method will send email based on data.
    /// </summary>
    /// <param name="data">instance of NotificationData</param>
    public async Task SendDeviceNotification(NotificationItem data)
    {
        await SendToNotificationFramework(data);
    }

    /// <summary>
    /// This is overloaded method to send email based on To, Subject and Body parameters
    /// </summary>
    /// <param name="to">To Parameter</param>
    /// <param name="subject">Subject paramter</param>
    /// <param name="body">Body content of email.</param>
    public async Task SendEmail(string to, string subject, string body)
    {
        await SendToNotificationFramework(new NotificationItem() { To = to, Subject = subject, Body = body });
    }

    /// <summary>
    /// This is overloaded method to send email based on template and template data
    /// </summary>
    /// <param name="to">To parameter</param>
    /// <param name="template">template parameter</param>
    /// <param name="templateData">templateData paramter</param>
    public async Task SendEmail(string to, string template, Dictionary<string, string> templateData)
    {
        await SendToNotificationFramework(new NotificationItem() { To = to, TemplateData = templateData.ToDictionary(pair => pair.Key, pair => (object)pair.Value) });
    }

    /// <summary>
    /// This is overloaded method to send email based on notification data.
    /// </summary>
    /// <param name="data">data parameter</param>
    public async Task SendEmail(NotificationItem data)
    {
        await SendToNotificationFramework(data);
    }

    /// <summary>
    /// This method will send email based on list of notification objects
    /// </summary>
    /// <param name="data">List of NotificationData</param>
    public async Task SendEmails(IEnumerable<NotificationItem> data)
    {
        foreach (var notificationData in data)
        {
            await SendEmail(notificationData);
        }
    }

    /// <summary>
    /// This method will convert NotificationData object to Notification Framework and then send email through Notification framework
    /// </summary>
    /// <param name="data">Notification data object.</param>
    private async Task SendToNotificationFramework(NotificationItem notificationData)
    {
        if (notificationData?.NotificationTypes == null)
        {
            notificationData.NotificationTypes = new List<NotificationType>
            {
                NotificationType.Mail
            };
        }
        if (notificationData?.Attachments == null)
        {
            notificationData.Attachments = new List<NotificationDataAttachment>();
        }
        foreach (NotificationDataAttachment attachment in notificationData.Attachments)
        {
            attachment.FileType = "application/octet-stream";
        }
        string uri = _notificationBroadcastUri;
        HttpRequestMessage reqMessage = await Tenant.CreateRequestForNotification(HttpMethod.Post, uri, notificationData.TemplateId);
        var notificationItem = NotificationDataToNotificationItem(notificationData);

        var logData = new Dictionary<LogDataKey, object>
        {
            { LogDataKey.ClientDevice, Constants.NotificationFrameworkProvider },
            { LogDataKey.EventType, Constants.BusinessProcessEvent },
            { LogDataKey.IsCriticalEvent, CriticalityLevel.Yes.ToString() },
            { LogDataKey.TenantId, notificationData.TenantIdentifier },
            { LogDataKey.Subject, notificationData.Subject }
        };

        reqMessage.Content = new StringContent(notificationItem.ToJson(), Encoding.UTF8);
        HttpResponseMessage response = await _httpHelper.SendRequestAsync(reqMessage);
        if (response.StatusCode != System.Net.HttpStatusCode.OK)
        {
            var ex = new Exception($"Error Sending Email:{Constants.NotificationFrameworkProvider} | ResponseStatusCode:{response.StatusCode} | Reason:{response.ReasonPhrase}");
            _logProvider.LogError(TrackingEvent.SendEmailNotificationFail, ex, logData);
            throw ex;
        }

        _logProvider.LogInformation(TrackingEvent.NotificationFrameworkSendEmailCompleted, logData);
    }

    /// <summary>
    /// This is mapper to prepare NotificationFrameworkItem which requires by notification framework.
    /// </summary>
    /// <param name="notification"></param>
    /// <returns>Returns final NotificationFrameworkItem with replaced values</returns>
    private NotificationItem NotificationDataToNotificationItem(NotificationItem notification)
    {
        notification.ApplicationName = "Approvals";
        if (!string.IsNullOrWhiteSpace(notification.TemplateRowExp))
        {
            var htmlMinifier = new HtmlMinifier();

            var templateBody = _templateFactory.GetEmailTemplate(notification);
            if (!string.IsNullOrWhiteSpace(templateBody))
            {
                var result = htmlMinifier.Minify(FillTemplate(templateBody, notification, false), generateStatistics: true);

                if (result.Errors.Count == 0)
                {
                    templateBody = result.MinifiedContent;

                    templateBody = FillTemplate(templateBody, notification, false);
                    //// Placeholder itself contains placeholder so we need to call fillTemplate again to get it replace all palceholders properly.
                    templateBody = FillTemplate(templateBody, notification);
                }
                else
                {
                    templateBody = FillTemplate(templateBody, notification, false);
                    //// Placeholder itself contains placeholder so we need to call fillTemplate again to get it replace all palceholders properly.
                    templateBody = FillTemplate(templateBody, notification);
                }

                notification.Body = templateBody;
                if (notification.TemplateData != null && notification.TemplateData.ContainsKey("ActionableMessage"))
                {
                    notification.TemplateData["ActionableMessage"] = FillTemplate(notification.TemplateData["ActionableMessage"].ToString(), notification, false);
                }

                var templateSubject = GetSubject(templateBody);
                if (!string.IsNullOrWhiteSpace(templateSubject))
                {
                    notification.Subject = FillTemplate(templateSubject, notification);
                }
            }
            else
            {
                throw new InvalidDataException("Email template not found!");
            }
        }
        return notification;
    }

    /// <summary>
    /// This method will replace placeholder with actual data.
    /// </summary>
    /// <param name="template">Email template</param>
    /// <param name="data">Data which will be replaced for placeholder</param>
    /// <param name="isPlaceHolderRemoved">Flag to remove placeholder if data is not present.</param>
    /// <returns>Returns template with data replaced to placeholders.</returns>
    private string FillTemplate(string template, NotificationItem data, bool isPlaceHolderRemoved = true)
    {
        string output = template;
        if (data.TemplateData != null)
        {
            foreach (KeyValuePair<string, object> replacableData in data.TemplateData)
            {
                output = output.Replace("#" + replacableData.Key + "#", (string)replacableData.Value);
            }
        }

        if (isPlaceHolderRemoved)
        {
            //to remove ActionDetails and AdditionalData related placeholders with string.empty if no value is present.
            output = Regex.Replace(output, @"#ActionDetails.[0-9a-zA-Z]*#", string.Empty, RegexOptions.IgnoreCase);
            output = Regex.Replace(output, @"#AdditionalData.[0-9a-zA-Z]*#", string.Empty, RegexOptions.IgnoreCase);
            output = Regex.Replace(output, @"#ApproverNotes#", string.Empty, RegexOptions.IgnoreCase);
        }

        return output;
    }

    /// <summary>
    /// This method will prepare Subject from template.
    /// </summary>
    /// <param name="templateContent">Template content</param>
    /// <returns>Returns subject of email.</returns>
    private string GetSubject(string templateContent)
    {
        string result = string.Empty;
        if (templateContent.Contains("<title>"))
        {
            int first = templateContent.IndexOf("<title>") + "<title>".Length;
            int last = templateContent.IndexOf("</title>");

            result = templateContent.Substring(first, last - first);
        }
        return result;
    }
}