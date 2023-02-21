// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace Microsoft.CFS.Approvals.DevTools.Model.Models;
using Microsoft.CFS.Approvals.Model;

/// <summary>
/// Approval Email Notification Templates Entity class
/// </summary>
public class ApprovalEmailNotificationTemplatesEntity : BaseTableEntity
{
    /// <summary>
    /// Constructor of ApprovalEmailNotificationTemplatesEntity
    /// </summary>
    public ApprovalEmailNotificationTemplatesEntity()
    {

    }
    public string TemplateContent { get; set; }
}
