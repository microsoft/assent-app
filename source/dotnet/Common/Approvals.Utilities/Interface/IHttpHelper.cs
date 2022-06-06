// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace Microsoft.CFS.Approvals.Utilities.Interface
{
    using System.Collections.Generic;
    using System.Net.Http;
    using System.Threading.Tasks;

    public interface IHttpHelper
    {
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
        Task<HttpResponseMessage> SendRequestAsync(
            HttpMethod method,
            string clientId,
            string clientKey,
            string authority,
            string resourceUri,
            string targetUri,
            Dictionary<string, string> headers = null,
            string content = "");

        /// <summary>
        /// Send Request to REST endpoint
        /// </summary>
        /// <param name="httpRequestMessage"></param>
        /// <returns></returns>
        Task<HttpResponseMessage> SendRequestAsync(HttpRequestMessage httpRequestMessage);
    }
}