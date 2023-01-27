// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace Microsoft.CFS.Approvals.Core.BL.Interface;

using System.Threading.Tasks;
using Microsoft.CFS.Approvals.Model;

public interface IDocumentApprovalStatusHelper
{
    /// <summary>
    /// Get document status
    /// </summary>
    /// <param name="tenantId"></param>
    /// <param name="documentNumber"></param>
    /// <param name="clientDevice"></param>
    /// <param name="userAlias"></param>
    /// <param name="loggedInUser"></param>
    /// <param name="tcv"></param>
    /// <param name="sessionId"></param>
    /// <param name="xcv"></param>
    /// <returns></returns>
    Task<DocumentStatusResponse> DocumentStatus(int tenantId, string documentNumber, string clientDevice, string userAlias, string loggedInUser, string tcv, string sessionId, string xcv);
}