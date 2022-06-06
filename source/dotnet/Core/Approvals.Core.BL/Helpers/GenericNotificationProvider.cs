// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace Microsoft.CFS.Approvals.Core.BL.Helpers
{
    using System.Collections.Generic;
    using System.Threading.Tasks;
    using Microsoft.CFS.Approvals.Common.DL.Interface;
    using Microsoft.CFS.Approvals.Core.BL.Interface;
    using Microsoft.CFS.Approvals.Domain.BL.Interface;
    using Microsoft.CFS.Approvals.LogManager.Provider.Interface;
    using Microsoft.CFS.Approvals.Model;
    using Microsoft.CFS.Approvals.Utilities.Interface;
    using Microsoft.Extensions.Configuration;

    /// <summary>
    /// Generic Notification Provider class
    /// </summary>
    public class GenericNotificationProvider : INotificationProvider
    {
        /// <summary>
        /// The notification provider
        /// </summary>
        private readonly INotificationProvider _notificationProvider;

        public ITenant Tenant
        {
            get
            {
                return _notificationProvider.Tenant;
            }
            set
            {
                _notificationProvider.Tenant = value;
            }
        }

        /// <summary>
        /// Constructor to create instance of Notification framework
        /// </summary>
        /// <param name="httpHelper">Http helper.</param>
        /// <param name="emailTemplateHelper">Email template helper.</param>
        /// <param name="logProvider">Logger to log telemetry</param>
        /// <param name="config">Configuration helper to read configuration.</param>
        public GenericNotificationProvider(IHttpHelper httpHelper, IEmailTemplateHelper emailTemplateHelper, ILogProvider logProvider, IConfiguration config)
        {
            _notificationProvider = new NotificationFrameworkProvider(httpHelper, emailTemplateHelper, logProvider, config);
        }

        #region INotificationProvider Members

        #region Implemented Methods

        /// <summary>
        /// This is overloaded method to send email based on To, Subject and Body parameters
        /// </summary>
        /// <param name="to">To Parameter</param>
        /// <param name="subject">Subject paramter</param>
        /// <param name="body">Body content of email.</param>
        public async Task SendEmail(string to, string subject, string body)
        {
            await _notificationProvider.SendEmail(to, subject, body);
        }

        /// <summary>
        /// This is overloaded method to send email based on To, Subject, body and priority
        /// </summary>
        /// <param name="to">To parameter</param>
        /// <param name="subject">subject parameter</param>
        /// <param name="body">body parameter</param>
        /// <param name="priority">priority parameter</param>
        public async Task SendEmail(string to, string subject, string body, string priority)
        {
            await _notificationProvider.SendEmail(to, subject, body, priority);
        }

        /// <summary>
        /// This is overloaded method to send email based on template and template data
        /// </summary>
        /// <param name="to">To parameter</param>
        /// <param name="template">template parameter</param>
        /// <param name="templateData">templateData paramter</param>
        public async Task SendEmail(string to, string template, Dictionary<string, string> templateData)
        {
            await _notificationProvider.SendEmail(to, template, templateData);
        }

        /// <summary>
        /// This is overloaded method to send email based on notification data.
        /// </summary>
        /// <param name="data">data parameter</param>
        public async Task SendEmail(NotificationData data)
        {
            await _notificationProvider.SendEmail(data);
        }

        /// <summary>
        /// This method will send email based on list of notification objects
        /// </summary>
        /// <param name="data">List of NotificationData</param>
        public async Task SendEmails(IEnumerable<NotificationData> data)
        {
            await _notificationProvider.SendEmails(data);
        }

        /// <summary>
        /// This method will send email based on To and Message property.
        /// </summary>
        /// <param name="to">To property to send email</param>
        /// <param name="message">Message to send notification</param>
        public async Task SendDeviceNotification(string to, string message)
        {
            await _notificationProvider.SendDeviceNotification(to, message);
        }

        /// <summary>
        /// This method will send email based on data.
        /// </summary>
        /// <param name="data">instance of NotificationData</param>
        public async Task SendDeviceNotification(NotificationData data)
        {
            await _notificationProvider.SendDeviceNotification(data);
        }

        #endregion Implemented Methods

        #endregion INotificationProvider Members
    }
}