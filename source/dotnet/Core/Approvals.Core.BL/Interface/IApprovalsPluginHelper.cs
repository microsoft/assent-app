// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Threading.Tasks;
using Microsoft.CFS.Approvals.Contracts.DataContracts;
using Microsoft.CFS.Approvals.Model;

namespace Microsoft.CFS.Approvals.Core.BL.Interface;

/// <summary>
/// Abstraction for interacting with the Approvals plugin to obtain completions based on user prompts and context.
/// </summary>
public interface IApprovalsPluginHelper
{
    /// <summary>
    /// Requests a completion from the Approvals plugin.
    /// </summary>
    /// <param name="signedInUser">The signed-in user.</param>
    /// <param name="onBehalfUser">The user on whose behalf the action is performed.</param>
    /// <param name="oauth2UserToken">OAuth2 token for downstream calls.</param>
    /// <param name="clientDevice">Client device identifier.</param>
    /// <param name="askRequest">Prompt request payload sent to the plugin.</param>
    /// <param name="tcv">Trace correlation vector.</param>
    /// <param name="xcv">Transaction correlation vector.</param>
    /// <returns>A <see cref="PluginResponse"/> from the plugin.</returns>
    Task<PluginResponse> GetApprovalsPluginCompletionAsync(User signedInUser, User onBehalfUser, string oauth2UserToken, string clientDevice, AskRequest askRequest, string tcv, string xcv);
}
