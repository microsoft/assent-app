// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace Microsoft.CFS.Approvals.Core.BL.Interface;

using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.CFS.Approvals.Common.BL.Interface;
using Microsoft.CFS.Approvals.Contracts.DataContracts;
using Microsoft.CFS.Approvals.Domain.BL.Interface;
using Microsoft.CFS.Approvals.LogManager.Model;
using Microsoft.CFS.Approvals.Model;
using Newtonsoft.Json.Linq;

public interface IDetailsHelper
{
    /// <summary>
    /// Auth Sum operation
    /// </summary>
    /// <param name="tenantAdaptor">The tenant adaptor</param>
    /// <param name="approvalDetails">The approval details</param>
    /// <param name="tenantId">Tenant Id of the tenant (1/2/3..)</param>
    /// <param name="tenantInfo">ApprovalTenantInfo object for the current tenant</param>
    /// <param name="documentSummary">ApprovalSummaryRow object</param>
    /// <param name="transactionHistoryExts">Historical data</param>
    /// <param name="currentApproverInDbJson">Current Approver JSON from Approval Details table</param>
    /// <param name="documentNumber">Document Number of the request</param>
    /// <param name="operation">Operation type (DT1/LINE etc.)</param>
    /// <param name="alias">Alias of the Approver of this request</param>
    /// <param name="loggedInAlias">Logged in User Alias</param>
    /// <param name="sessionId">GUID session id</param>
    /// <param name="fiscalYear">Fiscal year of the request</param>
    /// <param name="page">Page number</param>
    /// <param name="xcv">Cross system correlation vector for telemetry and logging</param>
    /// <param name="tcv">GUID transaction correlation vector for telemetry and logging</param>
    /// <param name="isWorkerTriggered">To understand if Worker role has triggered the details fetch</param>
    /// <param name="sectionType">section type. eg. Summary Details</param>
    /// <param name="clientDevice">Client Device</param>
    /// <param name="aadUserToken">AAD User Token</param>
    /// <returns>Details data as JObject</returns>
    Task<JObject> AuthSum
        (
            ITenant tenantAdaptor,
            List<ApprovalDetailsEntity> approvalDetails,
            int tenantId,
            ApprovalTenantInfo tenantInfo,
            ApprovalSummaryRow documentSummary,
            List<TransactionHistoryExt> transactionHistoryExts,
            ApprovalDetailsEntity currentApproverInDbJson,
            string documentNumber,
            string operation,
            string alias,
            string loggedInAlias,
            string sessionId,
            string fiscalYear,
            int page,
            string xcv,
            string tcv,
            bool isWorkerTriggered,
            int sectionType,
            string clientDevice,
            string aadUserToken
        );

    /// <summary>
    /// Downloads documents
    /// </summary>
    /// <param name="tenantAdaptor">The tenant adaptor</param>
    /// <param name="tenantId">Tenant Id of the tenant (1/2/3..)</param>
    /// <param name="tenantInfo">ApprovalTenantInfo object for the current tenant</param>
    /// <param name="approvalIdentifier">The Approval Identifier object which has the DisplayDocumentNumber, DocumentNumber and FiscalYear</param>
    /// <param name="alias">Alias of the Approver of this request</param>
    /// <param name="attachmentId">Attachment ID of the Document to be downloaded</param>
    /// <param name="sessionId">GUID session id</param>
    /// <param name="loggedInAlias">Logged in User Alias</param>
    /// <param name="xcv">Cross system correlation vector for telemetry and logging</param>
    /// <param name="tcv">GUID transaction correlation vector for telemetry and logging</param>
    /// <returns>HttpResponseMessage with Stream data of the attachment</returns>
    Task<byte[]> DownloadDocumentAsync
        (
            ITenant tenantAdaptor,
            int tenantId,
            ApprovalTenantInfo tenantInfo,
            ApprovalIdentifier approvalIdentifier,
            string alias,
            string attachmentId,
            string sessionId,
            string loggedInAlias,
            string xcv,
            string tcv
        );

    /// <summary>
    /// Gets the details
    /// </summary>
    /// <param name="tenantAdaptor">The tenant adaptor</param>
    /// <param name="tenantId">Tenant Id of the tenant (1/2/3..)</param>
    /// <param name="tenantInfo">ApprovalTenantInfo object for the current tenant</param>
    /// <param name="documentNumber">Document Number of the request</param>
    /// <param name="operation">Operation type (DT1/LINE etc.)</param>
    /// <param name="alias">Alias of the Approver of this request</param>
    /// <param name="loggedInAlias">Logged in User Alias</param>
    /// <param name="sessionId">GUID session id</param>
    /// <param name="fiscalYear">Fiscal year of the request</param>
    /// <param name="page">Page number</param>
    /// <param name="xcv">Cross system correlation vector for telemetry and logging</param>
    /// <param name="tcv">GUID transaction correlation vector for telemetry and logging</param>
    /// <param name="clientDevice">Client Device (Web/WP8..)</param>
    /// <returns>HttpResponseMessage with details data of the request</returns>
    Task<HttpResponseMessage> GetDetailAsync
        (
            ITenant tenantAdaptor,
            int tenantId,
            ApprovalTenantInfo tenantInfo,
            string documentNumber,
            string operation,
            string alias,
            string loggedInAlias,
            string sessionId,
            string fiscalYear,
            int page,
            string xcv,
            string tcv,
            string clientDevice
        );

    /// <summary>
    /// This method gets the details of the request
    /// </summary>
    /// <param name="tenantId">Tenant Id of the tenant (1/2/3..)</param>
    /// <param name="documentNumber">Document Number of the request</param>
    /// <param name="operation">Operation type (DT1/LINE etc.)</param>
    /// <param name="fiscalYear">Fiscal year of the request</param>
    /// <param name="page">Page number</param>
    /// <param name="sessionId">GUID session id</param>
    /// <param name="tcv">GUID transaction correlation vector for telemetry and logging</param>
    /// <param name="xcv">Cross system correlation vector for telemetry and logging</param>
    /// <param name="userAlias">Alias of the Approver of this request</param>
    /// <param name="loggedInAlias">Logged in User Alias</param>
    /// <param name="clientDevice">Client Device (Web/WP8..)</param>
    /// <param name="aadUserToken">The Azure AD user token</param>
    /// <param name="isWorkerTriggered">To understand if Worker role has triggered the details fetch</param>
    /// <param name="sectionType">section type. eg. Summary Details</param>
    /// <returns>Details of the request as a Task of JObject</returns>
    Task<JObject> GetDetails
        (
            int tenantId,
            string documentNumber,
            string operation,
            string fiscalYear,
            int page,
            string sessionId,
            string tcv,
            string xcv,
            string userAlias,
            string loggedInAlias,
            string clientDevice,
            string aadUserToken,
            bool isWorkerTriggered,
            int sectionType,
            string pageType,
            string source
        );

    /// <summary>
    /// This method gets the Attachment content
    /// </summary>
    /// <param name="tenantId">Tenant Id of the tenant (1/2/3..)</param>
    /// <param name="documentNumber">Document Number of the request</param>
    /// <param name="displayDocumentNumber">Display Document Number of the request</param>
    /// <param name="fiscalYear">Fiscal year of the request</param>
    /// <param name="attachmentId">Attachment ID of the Document to be downloaded</param>
    /// <param name="IsPreAttached">Specifies the type of the attachment if the attachment is pre attached from tenant or post attached from ui.</param>
    /// <param name="sessionId">GUID session id</param>
    /// <param name="tcv">GUID transaction correlation vector for telemetry and logging</param>
    /// <param name="xcv">Cross system correlation vector for telemetry and logging</param>
    /// <param name="userAlias">Alias of the Approver of this request</param>
    /// <param name="loggedInAlias">Logged in User Alias</param>
    /// <param name="clientDevice">Client Device (Web/WP8..)</param>
    /// <param name="aadUserToken">The Azure AD user token</param>
    /// <returns>HttpResponseMessage with Stream data of the attachment</returns>
    Task<byte[]> GetDocuments(
            int tenantId,
            string documentNumber,
            string displayDocumentNumber,
            string fiscalYear,
            string attachmentId,
            bool IsPreAttached,
            string sessionId,
            string tcv,
            string xcv,
            string userAlias,
            string loggedInAlias,
            string clientDevice,
            string aadUserToken);

    /// <summary>
    /// This method gets all the Attachment content for a request
    /// </summary>
    /// <param name="tenantId">Tenant Id of the tenant (1/2/3..)</param>
    /// <param name="documentNumber">Document Number of the request</param>
    /// <param name="displayDocumentNumber">Display Document Number of the request</param>
    /// <param name="fiscalYear">Fiscal year of the request</param>
    /// <param name="attachments">Attachment IDs of the Documents to be downloaded</param>
    /// <param name="sessionId">GUID session id</param>
    /// <param name="tcv">GUID transaction correlation vector for telemetry and logging</param>
    /// <param name="xcv">Cross system correlation vector for telemetry and logging</param>
    /// <param name="userAlias">Alias of the Approver of this request</param>
    /// <param name="loggedInAlias">Logged in User Alias</param>
    /// <param name="clientDevice">Client Device (Web/WP8..)</param>
    /// <param name="aadUserToken">The Azure AD user token</param>
    /// <returns>HttpResponseMessage with Stream data of the attachment</returns>
    Task<byte[]> GetAllDocumentsZipped(int tenantId, string documentNumber, string displayDocumentNumber,
       string fiscalYear, IRequestAttachment[] attachments, string sessionId, string tcv, string xcv, string userAlias, string loggedInAlias,
       string clientDevice, string aadUserToken);

    /// <summary>
    /// Bulk Attachment Download : Gets the attachments from the LOB application for the selected requests
    /// </summary>
    /// <param name="tenantId">Tenant Id of the tenant (1/2/3..)</param>
    /// <param name="sessionId">GUID session id</param>
    /// <param name="tcv">GUID transaction correlation vector for telemetry and logging</param>
    /// <param name="requestContent">Request body which is sent to the LoB application as part of the content in the Http call</param>
    /// <param name="userAlias">Alias of the Approver of this request</param>
    /// <param name="loggedInAlias">Logged in User Alias</param>
    /// <param name="clientDevice">Client Device (Web/WP8..)</param>
    /// <param name="authorizationToken">Authorization Token</param>
    /// <returns>HttpResponseMessage with Stream data of all the attachments</returns>
    Task<byte[]> GetAllAttachmentsInBulk(
            int tenantId,
            string sessionId,
            string tcv,
            string requestContent,
            string userAlias,
            string loggedInAlias,
            string clientDevice,
            string authorizationToken);

    /// <summary>
    /// This method gets the Attachment content for preview
    /// </summary>
    /// <param name="tenantId">Tenant Id of the tenant (1/2/3..)</param>
    /// <param name="documentNumber">Document Number of the request</param>
    /// <param name="displayDocumentNumber">Display Document Number of the request</param>
    /// <param name="fiscalYear">Fiscal year of the request</param>
    /// <param name="attachmentId">Attachment ID of the Document to be downloaded</param>
    /// <param name="IsPreAttached">Specifies the type of the attachment if the attachment is pre attached from tenant or post attached from ui.</param>
    /// <param name="sessionId">GUID session id</param>
    /// <param name="tcv">GUID transaction correlation vector for telemetry and logging</param>
    /// <param name="xcv">Cross system correlation vector for telemetry and logging</param>
    /// <param name="userAlias">Alias of the Approver of this request</param>
    /// <param name="loggedInAlias">Logged in User Alias</param>
    /// <param name="clientDevice">Client Device (Web/WP8..)</param>
    /// <param name="aadUserToken">The Azure AD user token</param>
    /// <returns>HttpResponseMessage with Stream data of the attachment</returns>
    Task<byte[]> GetDocumentPreview(int tenantId, string documentNumber, string displayDocumentNumber, string fiscalYear, string attachmentId, bool IsPreAttached, string sessionId, string tcv, string xcv, string alias, string loggedInAlias, string v1, string v2);

    /// <summary>
    /// This method will prepare base64 image of the alias
    /// </summary>
    /// <param name="alias">Input alias</param>
    /// <param name="SessionId">Session ID </param>
    /// <param name="clientDevice">Client Device</param>
    /// <param name="logData">Log Data</param>
    /// <returns>Returns base64 string</returns>
    Task<string> GetUserImage(string alias, string SessionId, string clientDevice, Dictionary<LogDataKey, object> logData);
}