// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Threading.Tasks;
using Microsoft.CFS.Approvals.Contracts;

namespace Microsoft.CFS.Approvals.Core.BL.Interface;

/// <summary>
/// Dedicated helper for AI analysis using ChatHelper.
/// </summary>
public interface IAIAnalysisHelper
{
    /// <summary>
    /// Generates AI analysis for a request by document number and tenant info.
    /// </summary>
    /// <param name="documentNumber">The request document number.</param>
    /// <param name="tenantInfo">Tenant info for the request.</param>
    /// <param name="messageId">Optional message id used for correlation.</param>
    /// <returns>A <see cref="RequestSummaryData"/> representing the AI-generated analysis.</returns>
    Task<RequestSummaryData> GenerateAIAnalysisAsync(string documentNumber, Microsoft.CFS.Approvals.Model.ApprovalTenantInfo tenantInfo, string messageId = "");
}
