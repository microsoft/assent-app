// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace Microsoft.CFS.Approvals.Utilities.Helpers
{
    using System.Net.Http;
    using System.Net.Http.Headers;
    using System.Threading.Tasks;
    using Microsoft.Graph;
    using Microsoft.Identity.Client;

    /// <summary>
    /// The MSAL Authentication provider class
    /// </summary>
    public class AuthenticationProvider : IAuthenticationProvider
    {
        /// <summary>
        /// The confidential client application
        /// </summary>
        private readonly IConfidentialClientApplication _clientApplication;

        private readonly string[] _scopes;

        /// <summary>
        /// Constructor of MsalAuthenticationProvider
        /// </summary>
        /// <param name="_clientApplication"></param>
        /// <param name="_scopes"></param>
        public AuthenticationProvider(IConfidentialClientApplication clientApplication, string[] scopes)
        {
            _clientApplication = clientApplication;
            _scopes = scopes;
        }

        /// <summary>
        /// Authenticate request
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        public async Task AuthenticateRequestAsync(HttpRequestMessage request)
        {
            var token = await GetTokenAsync();
            request.Headers.Authorization = new AuthenticationHeaderValue("bearer", token);
        }

        /// <summary>
        /// Get token asynchronously
        /// </summary>
        /// <returns></returns>
        public async Task<string> GetTokenAsync()
        {
            AuthenticationResult authResult;
            authResult = await _clientApplication.AcquireTokenForClient(_scopes).ExecuteAsync();

            return authResult.AccessToken;
        }
    }
}