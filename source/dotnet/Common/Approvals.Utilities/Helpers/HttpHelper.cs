// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace Microsoft.CFS.Approvals.Utilities.Helpers
{
    using System;
    using System.Collections.Generic;
    using System.Net.Http;
    using System.Net.Http.Headers;
    using System.Text;
    using System.Threading.Tasks;
    using Microsoft.CFS.Approvals.Utilities.Interface;
    using Microsoft.CFS.Approvals.Contracts;
    using Microsoft.Extensions.Configuration;

    public class HttpHelper : IHttpHelper
    {
        private readonly IAuthenticationHelper _authenticationHelper;
        private readonly HttpClient _client;
        private readonly IConfiguration _config;

        public HttpHelper(HttpClient httpClient, IAuthenticationHelper authenticationHelper, IConfiguration config)
        {
            _config = config;
            _client = httpClient;
            _client.Timeout = TimeSpan.FromMinutes(Convert.ToDouble(_config[ConfigurationKey.TenantAPICallTimeoutValueInMins.ToString()]));
            _authenticationHelper = authenticationHelper;
        }

        /// <summary>
        /// Send Request to target REST endpoint
        /// </summary>
        /// <param name="method"></param>
        /// <param name="clientId"></param>
        /// <param name="clientKey"></param>
        /// <param name="authority"></param>
        /// <param name="resourceUri"></param>
        /// <param name="targetUri"></param>
        /// <param name="content"></param>
        /// <param name="headers"></param>
        /// <returns></returns>
        public async Task<HttpResponseMessage> SendRequestAsync(
            HttpMethod method,
            string clientId,
            string clientKey,
            string authority,
            string resourceUri,
            string targetUri,
            Dictionary<string, string> headers = null,
            string content = "")
        {
            var accessToken = (await _authenticationHelper.AcquireOAuth2TokenAsync(
                              clientId, clientKey, authority, resourceUri, string.Empty)).AccessToken;
            _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue(Constant.Bearer, accessToken);

            // Create HttpRequestMessage
            var httpRequest = new HttpRequestMessage(method, new Uri(targetUri));
            if (!string.IsNullOrWhiteSpace(content))
            {
                httpRequest.Content = new StringContent(content, Encoding.UTF8, Constant.ApplicationJson);
            }

            // Add Http Headers
            if (headers != null)
            {
                foreach (var header in headers)
                {
                    if (httpRequest.Headers.Contains(header.Key))
                    {
                        httpRequest.Headers.Remove(header.Key);
                    }
                    httpRequest.Headers.Add(header.Key, header.Value);
                }
            }
            HttpResponseMessage response = await _client.SendAsync(httpRequest);
            return response;
        }

        /// <summary>
        /// Send Request to REST endpoint
        /// </summary>
        /// <param name="httpRequestMessage"></param>
        /// <returns></returns>
        public async Task<HttpResponseMessage> SendRequestAsync(HttpRequestMessage httpRequestMessage)
        {
            return await _client.SendAsync(httpRequestMessage);
        }
    }
}