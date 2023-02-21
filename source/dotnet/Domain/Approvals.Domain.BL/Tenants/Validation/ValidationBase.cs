// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace Microsoft.CFS.Approvals.Domain.BL.Tenants.Validations;

using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.CFS.Approvals.Contracts;
using Microsoft.CFS.Approvals.Contracts.DataContracts;
using Microsoft.CFS.Approvals.Domain.BL.Interface;
using Microsoft.CFS.Approvals.Domain.BL.Tenants.Model;
using Microsoft.CFS.Approvals.Model;
using Microsoft.CFS.Approvals.Utilities.Interface;
using Microsoft.Extensions.Configuration;

/// <summary>
/// The Validation Base class
/// </summary>
public class ValidationBase : IValidation
{
    /// <summary>
    /// The configuration
    /// </summary>
    private readonly IConfiguration _config = null;

    /// <summary>
    /// The name resolution helper
    /// </summary>
    private readonly INameResolutionHelper _nameResolutionHelper = null;

    /// <summary>
    /// Constructor of ValidationBase
    /// </summary>
    /// <param name="config"></param>
    /// <param name="nameResolutionHelper"></param>
    public ValidationBase(IConfiguration config, INameResolutionHelper nameResolutionHelper)
    {
        _config = config;
        _nameResolutionHelper = nameResolutionHelper;
    }

    /// <summary>
    /// Validates if the alias is  valid using Graph API
    /// </summary>
    /// <param name="arx">Input payload in ApprovalRequestExpression format</param>
    /// <returns>List of ValidationResults containing validation error in the alias (if any), along with the list of invalid aliases</returns>
    public virtual async Task<List<ValidationResult>> ValidateAlias(ApprovalRequestExpression arx)
    {
        bool validateAlias = Convert.ToBoolean(_config[ConfigurationKey.ValidateAliasUsingPayloadValidator.ToString()]);
        List<ValidationResult> results = new List<ValidationResult>();

        if (validateAlias && arx.Approvers != null && arx.Operation != ApprovalRequestOperation.Delete)
        {
            foreach (Approver approver in arx.Approvers)
            {
                if (await _nameResolutionHelper.GetUserName(approver.Alias) == approver.Alias)
                {
                    results.Add(new ValidationResult("Invalid arx.Approvers.Alias", new List<string> { "ApprovalRequestExpression.Approvers.Alias", approver.Alias.ToString() }));
                }
            }
        }

        return results;
    }

    /// <summary>
    /// Checks if the input alias of the user is from a microsoft domain or not
    /// </summary>
    /// <param name="alias">Alias of the user</param>
    /// <returns>True if the alias is from a non-microsoft domain, else False</returns>
    public virtual bool IsExternalUser(string alias)
    {
        return false;
    }

    /// <summary>
    /// Validation for the file attachment.
    /// </summary>
    /// <param name="file">File uploaded.</param>
    /// <param name="attachments">List of attachments.</param>
    /// <param name="attachmentProperties">Attachment properties for the tenant.</param>
    /// <param name="files">List of files uploaded.</param>
    /// <returns>Returns the validation result object.</returns>
    public virtual ValidationCheckResult ValidateAttachmentUpload(AttachmentUploadInfo file, List<Attachment> attachments, AttachmentProperties attachmentProperties, List<AttachmentUploadInfo> files)
    {
        var validationCheckResult = new ValidationCheckResult();
        bool fileValid = true;
        var errorMessage = new List<string>();

        FileAttachmentOptions fileAttachmentOptions = attachmentProperties?.FileAttachmentOptions;

        if (fileAttachmentOptions != null && !fileAttachmentOptions.AllowFileUpload)
        {
            fileValid = false;
            errorMessage.Add("File upload feature is disabled");
        }

        if (fileAttachmentOptions != null && !fileAttachmentOptions.AllowedFileTypes.Split(',').Contains($".{file.Name.Split('.')[file.Name.Split('.').Length - 1]}"))
        {
            fileValid = false;
            errorMessage.Add("File Type is not permitted.");
        }

        if (fileAttachmentOptions != null && fileAttachmentOptions.MaxAttachments != null && (attachments.Count + files.Count) > fileAttachmentOptions.MaxAttachments)
        {
            fileValid = false;
            errorMessage.Add("Maximum file count exceeded.");
        }

        if (fileAttachmentOptions != null && file.FileSize > fileAttachmentOptions.MaxFileSizeInBytes)
        {
            fileValid = false;
            errorMessage.Add("Maximum file size exceeded.");
        }

        validationCheckResult.ActionResult = fileValid;
        validationCheckResult.ErrorMessages = errorMessage;

        return validationCheckResult;
    }
}