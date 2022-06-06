// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace Microsoft.CFS.Approvals.SyntheticTransaction.Common.Models
{
    using System;
    using System.Collections.Generic;
    using System.Net;
    using System.Net.Http;
    using System.Runtime.Serialization;
    using Newtonsoft.Json;

    /// <summary>
    /// The Approval Response class
    /// </summary>
    public class ApprovalResponse
    {
        /// <summary>
        /// Approvals assigned unique identifier for each tenant.
        /// Required field and helps to identify the tenant.
        /// </summary>
        public string DocumentTypeID { get; set; }

        /// <summary>
        /// Action result - Type of Boolean.
        /// Required field and helps to identify the overall result of the transaction.
        /// </summary>
        public bool ActionResult { get; set; }

        /// <summary>
        /// E2E Error Info which carries additional data about each error that occurred.
        /// Applicable for error scenarios only.
        /// </summary>
        public ApprovalResponseErrorInfo E2EErrorInformation { get; set; }

        /// <summary>
        /// Document Identifier.
        /// Contains the DisplayDocumentNumber (Document Number used for displaying to the user in a user-friendly manner), DocumentNumber, Fiscal Year (Fiscal year in which the document number exists or is applicable) 
        /// Type of ApprovalIdentifier.
        /// </summary>
        public ApprovalIdentifier ApprovalIdentifier { get; set; }

        /// <summary>
        /// Contains the Xcv/Tcv/BusinessProcessName which is used for Telemetry and Logging.
        /// </summary>
        public ApprovalsTelemetry Telemetry { get; set; }

        /// <summary>
        /// Optionally, provide a display error message when a specific error message needs to be shown to the user OR provide a message for the user even if response is http OK
        /// Ex. If a business rule fails, user needs to understand the reason for failure and hence should see this message
        /// Ex. In case a SQL Connection fails to establish, ErrorMessage can show SQL connection failed, but DisplayMessage should show something like "Your request could not be processed. Please try again later"
        /// </summary>
        public string DisplayMessage { get; set; }
    }

    /// <summary>
    /// Approval Response Error Info class
    /// </summary>
    public class ApprovalResponseErrorInfo
    {
        /// <summary>
        /// Constructor of ApprovalResponseErrorInfo
        /// </summary>
        public ApprovalResponseErrorInfo()
        {
            // Setting default value
            ErrorType = ApprovalResponseErrorType.UnintendedError;
        }

        /// <summary>
        /// List of technical error messages that occurred on the tenant system for this transaction.
        /// The error list can comprise of all the related errors that happened in the tenant's downstream systems as well for enhanced telemetry.
        /// </summary>
        public List<string> ErrorMessages { get; set; }

        /// <summary>
        /// Indicates the type of error that occurred. 
        /// Default value is NonTransient, but should be overwritten when error type is known by the source system for improved error handling at destination (Approvals).
        /// </summary>
        public ApprovalResponseErrorType ErrorType { get; set; }

        /// <summary>
        /// Transaction retry interval (in mins)
        /// Mandatory parameter if ErrorType is known to be UnintendedTransientError. 
        /// This will be to retry after this interval. Please use this only if a transient error occurred and not when a non-transient or fatal error occurs
        /// </summary>
        public int RetryInterval { get; set; }
    }

    /// <summary>
    /// Approval Response Helper class
    /// </summary>
    public static class ApprovalResponseHelper
    {
        /// <summary>
        /// Generates the ApprovalResponse
        /// </summary>
        /// <param name="applicationID">Tenant ID</param>
        /// <param name="documentTypeID">Document Type ID</param>
        /// <param name="e2EErrorInformation">List of Error Information</param>
        /// <param name="displayMessage">Display Error Message</param>
        /// <param name="isActionSuccess">Flag for Action Success/ Failure</param>
        /// <returns>Approval Response</returns>
        public static ApprovalResponse GenerateApprovalResponse(string applicationID, string documentTypeID, ApprovalResponseErrorInfo e2EErrorInformation, string displayMessage, bool isActionSuccess = true)
        {
            var approvalResponse = new List<ApprovalResponse>();
            approvalResponse.Add(new ApprovalResponse()
            {
                ActionResult = isActionSuccess,
                DocumentTypeID = documentTypeID,
                E2EErrorInformation = e2EErrorInformation,
                DisplayMessage = displayMessage
            });

            return approvalResponse[0];
        }

        /// <summary>
        /// Gets the Http response with Content as serialized ApprovalResponse with proper error messages
        /// </summary>
        /// <param name="exception">Exception</param>
        /// <param name="appName">Tenant Id</param>
        /// <param name="docTypeId">Document Type Id</param>
        /// <param name="errorDisplayMessage">User Friendly Error message to be added in ApprovalResponse</param>
        /// <returns>ApprovalResponse object</returns>
        public static HttpResponseMessage GenerateErrorResponse(InvalidOperationException exception, string appName, string docTypeId, string errorDisplayMessage)
        {
            var error = new ApprovalResponseErrorInfo() { ErrorMessages = new List<string> { exception.Message }, ErrorType = ApprovalResponseErrorType.UnintendedError };
            var approvalResponse = GenerateApprovalResponse(appName, docTypeId, error, errorDisplayMessage, false);

            HttpResponseMessage response = new HttpResponseMessage(HttpStatusCode.BadRequest);
            response.Content = new StringContent(JsonConvert.SerializeObject(approvalResponse));
            return response;
        }
    }
    public enum ApprovalResponseErrorType
    {
        /// <summary>
        /// Intended Error.
        /// </summary>
        [EnumMember]
        IntendedError = 1,

        /// <summary>
        /// Unintended Non-Transient Error.
        /// </summary>
        [EnumMember]
        UnintendedError = 2,

        /// <summary>
        /// Unintended Transient Error.
        /// </summary>
        [EnumMember]
        UnintendedTransientError = 3
    }

    /// <summary>
    /// Approval Identifier class
    /// </summary>
    public class ApprovalIdentifier
    {
        public ApprovalIdentifier()
        {
            DocumentNumber = null;
            DisplayDocumentNumber = null;
            FiscalYear = null;
        }

        /// <summary>
        /// Identifier for the Approval (DocumentNbr, PO #, Invoice Number etc)
        /// </summary>
        public string DisplayDocumentNumber { get; set; }

        /// <summary>
        /// Internal reference ID
        /// </summary>
        public string DocumentNumber
        { get; set; }

        /// <summary>
        /// If not needed, then "" can be used. Some tenants have this as a part of composite key in addition to 
        /// DocumentNumber
        /// </summary>
        public string FiscalYear
        { get; set; }
    }
}
