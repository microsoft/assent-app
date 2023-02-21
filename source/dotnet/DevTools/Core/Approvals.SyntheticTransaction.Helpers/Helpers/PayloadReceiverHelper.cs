// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace Microsoft.CFS.Approvals.SyntheticTransaction.Helpers.Helpers;

using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using Microsoft.CFS.Approvals.DevTools.Model.Constant;
using Microsoft.CFS.Approvals.LogManager.Provider.Interface;
using Microsoft.CFS.Approvals.SyntheticTransaction.Helpers.Interface;
using Microsoft.Extensions.Configuration;
using Microsoft.Identity.Client;
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
    private readonly ILogProvider _logProvider;
    private readonly Dictionary<LogDataKey, object> logData = new Dictionary<LogDataKey, object>();

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
        ILogProvider logProvider)
    {
        _configuration = configuration;
        _configurationSetting = configurationSetting;
        _environment = actionContextAccessor?.ActionContext?.RouteData?.Values["env"]?.ToString();
        _logProvider = logProvider;
    }

    /// <summary>
    /// Send payload
    /// </summary>
    /// <param name="payload"></param>
    /// <param name="tcv"></param>
    /// <returns></returns>
    public async Task<HttpResponseMessage> SendPayload(string payload, string tcv)
    {
        logData.Clear();
        logData.Add(LogDataKey.Xcv, tcv);
        logData.Add(LogDataKey.Tcv, tcv);
        logData.Add(LogDataKey.Environment, _environment);
        logData.Add(LogDataKey.Operation, "Send Payload - Helper");

        try
        {
            JObject jObject = JsonConvert.DeserializeObject<JObject>(payload);
            foreach (var jtoken in jObject.SelectToken("DetailsData").Children())
            {
                var key = ((JProperty)jtoken.AsJEnumerable()).Name;
                var jdata = ((JProperty)jtoken).Value;
                jObject["DetailsData"][key] = JsonConvert.SerializeObject(jdata);
            }

            HttpClient _client = new HttpClient();
            HttpRequestMessage requestMessage = new HttpRequestMessage(HttpMethod.Post, string.Format("{0}{1}{2}", _configurationSetting.appSettings[_environment].PayloadReceiverServiceURL, "api/v1/PayloadReceiver?TenantId=", jObject["DocumentTypeId"]));
            requestMessage.Content = new StringContent(jObject.ToString(), UTF8Encoding.UTF8, "application/json");
            _client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", GenerateapplicationToken());
            var result = await _client.SendAsync(requestMessage);

            return result;
        }
        catch (Exception ex)
        {
            logData.Add(LogDataKey.EventName, "SendPayloadFailure");
            _logProvider.LogError(TrackingEvent.SendPayloadFailure, ex, logData);
            return null;
        }
    }

    private string GenerateapplicationToken()
    {
        try
        {
            IConfidentialClientApplication app = ConfidentialClientApplicationBuilder.
                Create(_configurationSetting.appSettings[_environment].PayloadReceiverServiceClientId).
                WithClientSecret(_configurationSetting.appSettings[_environment].PayloadReceiverServiceAppKey).
                WithAuthority(new Uri(string.Format("{0}{1}", _configuration["AADInstance"], _configuration["Tenant"]))).
                Build();
            var scopes = new[] { _configurationSetting.appSettings[_environment].ResourceURL + "/.default" };
            var result = app.AcquireTokenForClient(scopes).ExecuteAsync().Result;

            return result.AccessToken;
        }
        catch (Exception ex)
        {
            logData.Add(LogDataKey.EventName, "TokenGenerationFailure");
            _logProvider.LogError(TrackingEvent.TokenGenerationFailure, ex, logData);
            return null;
        }
    }
}