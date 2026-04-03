// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace Microsoft.CFS.Approvals.Core.BL.Interface;

using System.Threading.Tasks;
using Microsoft.CFS.Approvals.Contracts.DataContracts;
using Microsoft.CFS.Approvals.Model;

public interface IDocumentApprovalStatusHelper
{
    /// <summary>
    /// Get the document status.
    /// </summary>
    /// <param name="onBehalfUser"></param>
    /// <param name="signedInUser"></param>
    /// <param name="oauth2UserToken"></param>
    /// <param name="tenantId"></param>
    /// <param name="requestData"></param>
    /// <param name="clientDevice"></param>
    /// <param name="tcv"></param>
    /// <param name="sessionId"></param>
    /// <param name="xcv"></param>
    /// <returns>Document status response object.</returns>
    Task<DocumentStatusResponse> DocumentStatus(User signedInUser, User onBehalfUser, string oauth2UserToken, int tenantId, string requestData, string clientDevice, string tcv, string sessionId, string xcv);
}