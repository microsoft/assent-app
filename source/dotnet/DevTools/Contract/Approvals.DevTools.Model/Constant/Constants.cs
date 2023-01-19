// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace Microsoft.CFS.Approvals.DevTools.Model.Constant;

/// <summary>
/// Constants class
/// </summary>
public static class Constants
{
    public const string CurrentDateValidationMessage = "Event Start Time and Event End Time values should be greater than current UTC time.";
    public const string DateTimeRangeValidationMessage = "Event End Time should be greater than or equal to Event Start Time";
    public const string CurrentApprover = "CurrentApprover";
    public const string currentApproverMissingForDocument = "Approver not found , please check inputs i.e. tenant and documentNumber : ";
    public const string summaryNotFoundForDocument = "Summary data not found, please check inputs i.e. tenant, approver and documentNumber : ";
    public const string failedtoMarkOutOfSync = "Failed to mark request out of sync, Document Number : ";
    public const string invalidTenantSelection = "Invalid tenant selected for requests :";
    public const string PendingTenantApproval = "PendingTenantApproval";
    public const string ApprovalTenantInfo = "ApprovalTenantInfo";
    public const string PendingApprovalEmailNotificationTemplates = "PendingApprovalEmailNotificationTemplates";
    public const string ApprovalEmailNotificationTemplates = "ApprovalEmailNotificationTemplates";
    public const string Outlookdynamictemplates = "outlookdynamictemplates";
}
