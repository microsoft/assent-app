// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace Microsoft.CFS.Approvals.Utilities.Interface;

using System.Security.Claims;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;

public interface IAuthenticationHelper
{
    /// <summary>
    /// Get the OAuth 2.0 Access Token
    /// </summary>
    /// <param name="clientId">Client Id</param>
    /// <param name="appKey">Client Secret</param>
    /// <param name="authority">Authority</param>
    /// <param name="resource">Resource Uri</param>
    /// <param name="resource">Scope</param>
    /// <returns>AuthenticationResult token</returns>
    Task<Identity.Client.AuthenticationResult> AcquireOAuth2TokenByScopeAsync(string clientId, string appKey, string authority, string resource, string scope);

    /// <summary>
    /// Generate SAS token based on Shared Access Policy Name and value
    /// As this is the client this should have a Send Access policy key
    /// The resource URI should be the complete url which needs to be accessed
    /// </summary>
    /// <param name="parameterObject"></param>
    /// <param name="resourceUri"></param>
    /// <returns></returns>
    public string GetSASToken(JObject parameterObject, string resourceUri);

    /// <summary>
    /// Generate OAuth 2.0 User token from ClientId and Client Secret for the specified resource using the ClaimsIdentity
    /// </summary>
    /// <param name="identity"></param>
    /// <param name="parameterObject"></param>
    /// <returns></returns>
    public Task<string> GetOnBehalfBearerToken(ClaimsIdentity identity, JObject parameterObject);

    /// <summary>
    /// Get the On Behalf OAuth 2.0 User token
    /// </summary>
    /// <param name="userAccessToken">current OAuth 2.0 user token</param>
    /// <param name="parameterObject">OAuth 2.0 token generation parameters</param>
    /// <returns>OAuth 2.0 User token with changed resource URL</returns>
    public Task<string> GetOnBehalfUserToken(string userAccessToken, JObject parameterObject);

    /// <summary>
    /// Get Acs simple web token from shared secret.
    /// </summary>
    /// <param name="Hmac"></param>
    /// <returns></returns>
    public string GetAcsSimpleWebTokenFromSharedSecret(bool Hmac);

    /// <summary>
    /// Get OAuth 2.0 token generated using Managed Identity
    /// </summary>
    /// <param name="scope"></param>
    /// <returns>Managed Identity Token</returns>
    public Task<string> GetManagedIdentityToken(string scope);
}