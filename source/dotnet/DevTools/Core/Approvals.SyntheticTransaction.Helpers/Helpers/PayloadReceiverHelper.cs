// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace Microsoft.CFS.Approvals.SyntheticTransaction.Helpers.Helpers
{
    using System.Net.Http;
    using System.Text;
    using System.Threading.Tasks;
    using Microsoft.AspNetCore.Mvc.Infrastructure;
    using Microsoft.CFS.Approvals.SyntheticTransaction.Helpers.Interface;
    using Microsoft.CFS.Approvals.Utilities.Interface;
    using Microsoft.Extensions.Configuration;
    using Newtonsoft.Json;
    using Newtonsoft.Json.Linq;

    /// <summary>
    /// Payload Receiver Helper class
    /// </summary>
    public class PayloadReceiverHelper : IPayloadReceiverHelper
    {
        private readonly IConfiguration _configuration;
        private readonly string _environment;
        private readonly ConfigurationSetting _configurationSetting;
        private readonly IHttpHelper _httpHelper;
        private readonly IAuthenticationHelper _authenticationHelper;

        /// <summary>
        /// Constructor of PayloadReceiverHelper
        /// </summary>
        /// <param name="configuration"></param>
        /// <param name="configurationSetting"></param>
        /// <param name="actionContextAccessor"></param>
        /// <param name="httpHelper"></param>
        public PayloadReceiverHelper(
            IConfiguration configuration,
            ConfigurationSetting configurationSetting,
            IActionContextAccessor actionContextAccessor,
            IHttpHelper httpHelper,
            IAuthenticationHelper authenticationHelper)
        {
            _configuration = configuration;
            _configurationSetting = configurationSetting;
            _httpHelper = httpHelper;
            _authenticationHelper = authenticationHelper;
            _environment = actionContextAccessor?.ActionContext?.RouteData?.Values["env"]?.ToString();
        }

        /// <summary>
        /// Send payload
        /// </summary>
        /// <param name="Payload"></param>
        /// <returns></returns>
        public async Task<HttpResponseMessage> SendPayload(string Payload)
        {
            JObject jObject = JsonConvert.DeserializeObject<JObject>(Payload);
            foreach (var jtoken in jObject.SelectToken("DetailsData").Children())
            {
                var key = ((JProperty)jtoken.AsJEnumerable()).Name;
                var jdata = ((JProperty)jtoken).Value;
                jObject["DetailsData"][key] = JsonConvert.SerializeObject(jdata);
            }

            HttpRequestMessage requestMessage = new HttpRequestMessage(HttpMethod.Post, string.Format("{0}{1}{2}", _configurationSetting.appSettings[_environment].PayloadReceiverServiceURL, "api/v1/PayloadReceiver?TenantId=", jObject["DocumentTypeId"]));
            requestMessage.Content = new StringContent(jObject.ToString(), UTF8Encoding.UTF8, "application/json");
            requestMessage.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer",
                (await _authenticationHelper.AcquireOAuth2TokenByScopeAsync(
                    _configurationSetting.appSettings[_environment].PayloadReceiverServiceClientId,
                    _configurationSetting.appSettings[_environment].PayloadReceiverServiceAppKey,
                    string.Format("{0}{1}", _configuration["AADInstance"], _configuration["Tenant"]),
                    _configurationSetting.appSettings[_environment].ResourceURL,
                    "/.default")).AccessToken);
            var result = await _httpHelper.SendRequestAsync(requestMessage);

            return result;
        }
    }
}