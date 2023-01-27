// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace Microsoft.CFS.Approvals.Common.DL.Interface;

using System.Collections.Generic;
using Microsoft.CFS.Approvals.Model;

public interface IEmailTemplateHelper
{
    /// <summary>
    /// Get email templates
    /// </summary>
    /// <returns></returns>
    IEnumerable<ApprovalEmailNotificationTemplates> GetTemplates();

    /// <summary>
    /// Get email template by key
    /// </summary>
    /// <param name="partitionKey"></param>
    /// <returns></returns>
    IEnumerable<ApprovalEmailNotificationTemplates> GetTemplates(string partitionKey);
}
