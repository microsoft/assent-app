// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace Microsoft.CFS.Approvals.NotificationProcessor.BL.Interface
{
    using System.Threading.Tasks;

    using Microsoft.CFS.Approvals.Domain.BL.Interface;
    using Microsoft.CFS.Approvals.Model;

    public interface INotificationProcessor
    {
        Task<bool> SendNotifications(ApprovalNotificationDetails notificationExpression, ITenant tenant);

        ApprovalRequestResult ProcessNotificationInBackground(ApprovalNotificationDetails approvalNotificationDetails, ITenant tenant);

        /// <summary>
        /// This method is to send teams notifications
        /// </summary>
        /// <param name="notificationExpression">ApprovalNotification Details</param>
        /// <param name="tenant">Tenant information for tenant specific configuration</param>
        /// <returns>returns status as true if success else false</returns>
        Task<bool> SendTeamsNotifications(ApprovalNotificationDetails notificationExpression);
    }
}