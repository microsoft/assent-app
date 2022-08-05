// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace Microsoft.CFS.Approvals.Utilities.Interface
{
    using System.Security.Claims;
    using System.Threading.Tasks;
    using Microsoft.IdentityModel.Clients.ActiveDirectory;
    using Newtonsoft.Json.Linq;

    public interface IAuthenticationHelper
    {
        /// <summary>
        /// Get the Azure AD Access Token
        /// </summary>
        /// <param name="clientId">Client Id</param>
        /// <param name="appKey">Client Secret</param>
        /// <param name="authority">AADInstanceName</param>
        /// <param name="resource">Resource Uri</param>
        /// <returns>AuthenticationResult token</returns>
        Task<AuthenticationResult> AcquireOAuth2TokenAsync(string clientId, string appKey, string aadInstanceName, string resource, string tenant);

        /// <summary>
        /// Get the Azure AD Access Token
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
        /// Generate AAD User token from ClientId and Client Secret for the specified resource using the ClaimsIdentity
        /// </summary>
        /// <param name="identity"></param>
        /// <param name="parameterObject"></param>
        /// <returns></returns>
        public Task<string> GetOnBehalfBearerToken(ClaimsIdentity identity, JObject parameterObject);

        /// <summary>
        /// Get the On Behalf AAD User token
        /// </summary>
        /// <param name="userAccessToken">current AAD user token</param>
        /// <param name="parameterObject">AAD token generation parameters</param>
        /// <returns>AAD User token with changed resource URL</returns>
        public Task<string> GetOnBehalfUserToken(string userAccessToken, JObject parameterObject);

        /// <summary>
        /// Get Acs simple web token from shared secret.
        /// </summary>
        /// <param name="Hmac"></param>
        /// <returns></returns>
        public string GetAcsSimpleWebTokenFromSharedSecret(bool Hmac);
    }
}