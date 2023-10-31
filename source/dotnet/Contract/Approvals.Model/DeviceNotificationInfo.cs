// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace Microsoft.CFS.Approvals.Model;

using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using Microsoft.CFS.Approvals.Contracts;
using Microsoft.CFS.Approvals.Contracts.DataContracts;

[DataContract(Name = "DeviceNotificationInfo", Namespace = "http://www.microsoft.com/document/routing/2010/11")]
public class DeviceNotificationInfo// : IExtensibleDataObject
{
    [DataMember]
    public Guid RoutingId { get; set; }

    [DataMember]
    public Guid ApplicationId { get; set; }

    [DataMember]
    public Guid DocumentTypeId { get; set; }

    [DataMember]
    public bool SendNotification { get; set; }

    [DataMember]
    public String Approver { get; set; }

    [DataMember]
    public String OriginalApprover { get; set; }

    [DataMember]
    public String ApprovalCount { get; set; }

    [DataMember]
    public String Application { get; set; }

    [DataMember]
    public String Requestor { get; set; }

    [DataMember]
    public ApprovalIdentifier ApprovalIdentifier { get; set; }

    [DataMember]
    public DateTime NotificationRequestQueuingByClientUtcDateTime { get; set; }

    [DataMember]
    public DateTime NotificationRequestQueuedByClientUtcDateTime { get; set; }

    [DataMember]
    public ApprovalRequestOperation Operation { get; set; }

    [DataMember]
    public string NotificationApprover { get; set; }

    [DataMember]
    public String ActionTaken { get; set; }

    [DataMember]
    public Dictionary<string, string> ActionDetails { get; set; }

    #region Notification Info

    [DataMember]
    public String To { get; set; }

    [DataMember]
    public String CC { get; set; }

    [DataMember]
    public String BCC { get; set; }

    [DataMember]
    public String NotificationTemplateKey { get; set; }

    #endregion Notification Info

    [DataMember]
    public string NotificationFailResult { get; set; }

    [DataMember]
    public string CorrelationId { get; set; }

    public string DocumentNumber
    {
        get
        {
            return ApprovalIdentifier.DocumentNumber ?? String.Empty;
        }
    }

    public bool IsToastNotificationApplicable
    {
        get
        {
            var result = (Operation == ApprovalRequestOperation.Create || (Operation == ApprovalRequestOperation.Update && !IsDeleteOfUpdate));
            return result;
        }
    }

    [DataMember]
    public bool IsDeleteOfUpdate { get; set; }

    [DataMember]
    public bool ForFirstTimeUser { get; set; }

    [DataMember]
    public Contracts.NotificationType NotificationType { get; set; }

    [DataMember]
    public string BlobID { get; set; }

    [DataMember]
    public List<NotificationDataAttachment> Attachments { get; set; }
}