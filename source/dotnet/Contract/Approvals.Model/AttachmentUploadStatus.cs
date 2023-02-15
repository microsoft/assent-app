// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace Microsoft.CFS.Approvals.Model;

using System.Runtime.Serialization;
using Microsoft.CFS.Approvals.Contracts.DataContracts;

public class AttachmentUploadStatus : AttachmentUploadInfo
{
    /// <summary>
    /// Gets or sets the file upload status.
    /// </summary>
    public bool ActionResult { get; set; }

    /// <summary>
    /// E2E Error Info which carries additional data about each error that occurred.
    /// Applicable for error scenarios only.
    /// </summary>
    [DataMember]
    public ApprovalResponseErrorInfo E2EErrorInformation { get; set; }

    /// <summary>
    /// Contains the Xcv/Tcv/BusinessProcessName which is used for Telemetry and Logging.
    /// </summary>
    [DataMember]
    public ApprovalsTelemetry Telemetry { get; set; }

    /// <summary>
    /// Optionally, provide a display error message when a specific error message needs to be shown to the user OR provide a message for the user even if response is http OK
    /// Ex. If a business rule fails, user needs to understand the reason for failure and hence should see this message
    /// Ex. In case a SQL Connection fails to establish, ErrorMessage can show SQL connection failed, but DisplayMessage should show something like "Your request could not be processed. Please try again later"
    /// </summary>
    [DataMember]
    public string DisplayMessage { get; set; }
}
