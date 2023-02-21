// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace Microsoft.CFS.Approvals.Domain.BL.Interface;

using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Threading.Tasks;
using Microsoft.CFS.Approvals.Contracts.DataContracts;
using Microsoft.CFS.Approvals.Domain.BL.Tenants.Model;
using Microsoft.CFS.Approvals.Model;

public interface IValidation
{
    /// <summary>
    /// Validates if the alias is  valid using Graph API
    /// </summary>
    /// <param name="arx">Input payload in ApprovalRequestExpression format</param>
    /// <returns>List of ValidationResults containing validation error in the alias (if any), along with the list of invalid aliases</returns>
    public Task<List<ValidationResult>> ValidateAlias(ApprovalRequestExpression arx);

    /// <summary>
    /// Checks if the input alias of the user is from a microsoft domain or not
    /// </summary>
    /// <param name="alias">Alias of the user</param>
    /// <returns>True if the alias is from a non-microsoft domain, else False</returns>
    public bool IsExternalUser(string alias);

    /// <summary>
    /// Validation for the file attachment.
    /// </summary>
    /// <param name="file">File uploaded.</param>
    /// <param name="attachments">List of attachments.</param>
    /// <param name="attachmentProperties">Attachment properties for the tenant.</param>
    /// <param name="files">List of files uploaded.</param>
    /// <returns>Returns the validation result object.</returns>
    public ValidationCheckResult ValidateAttachmentUpload(AttachmentUploadInfo file, List<Attachment> attachmentsSummary, AttachmentProperties attachmentProperties, List<AttachmentUploadInfo> files);
}