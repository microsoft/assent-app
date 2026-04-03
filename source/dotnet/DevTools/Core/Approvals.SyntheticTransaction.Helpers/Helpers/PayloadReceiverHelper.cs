// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace Microsoft.CFS.Approvals.SyntheticTransaction.Helpers.Helpers;

using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using Microsoft.CFS.Approvals.DevTools.AppConfiguration;
using Microsoft.CFS.Approvals.DevTools.Model.Constant;
using Microsoft.CFS.Approvals.LogManager.Provider.Interface;
using Microsoft.CFS.Approvals.SyntheticTransaction.Helpers.Interface;
using Microsoft.CFS.Approvals.Utilities.Interface;
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
    private readonly ConfigurationHelper _configurationHelper;
    private readonly ILogProvider _logProvider;
    private readonly Dictionary<LogDataKey, object> logData = new Dictionary<LogDataKey, object>();
    private readonly IAuthenticationHelper _authenticationHelper;
    private readonly IHttpHelper _httpHelper;

    /// <summary>
    /// Constructor of PayloadReceiverHelper
    /// </summary>
    /// <param name="configuration"></param>
    /// <param name="configurationHelper"></param>
    /// <param name="actionContextAccessor"></param>
    /// <param name="logProvider"></param>
    public PayloadReceiverHelper(
        IAuthenticationHelper authenticationHelper,
        IConfiguration configuration,
        ConfigurationHelper configurationHelper,
        IActionContextAccessor actionContextAccessor,
        IHttpHelper httpHelper,
        ILogProvider logProvider)
    {
        _authenticationHelper = authenticationHelper;
        _configuration = configuration;
        _configurationHelper = configurationHelper;
        _environment = actionContextAccessor?.ActionContext?.RouteData?.Values["env"]?.ToString();
        _logProvider = logProvider;
        _httpHelper = httpHelper;
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
        logData.Add(LogDataKey.ComponentName, "API");
        logData.Add(LogDataKey.MSAComponentName, "TestHarness");
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
            var result = await _httpHelper.SendRequestAsync(
                HttpMethod.Post,
                _configurationHelper.appSettings[_environment]["ManagedIdentityClientId"], 
                null, 
                null,
                _configurationHelper.appSettings[_environment]["ResourceURL"],
                string.Format("{0}{1}{2}", _configurationHelper.appSettings[_environment]["PayloadReceiverServiceURL"], "api/v1/PayloadReceiver?TenantId=", jObject["DocumentTypeId"]), 
                null, 
                jObject.ToString(),
                true);

            return result;
        }
        catch (Exception ex)
        {
            _logProvider.LogError(TrackingEvent.SendPayloadFailure, ex, logData);
            return null;
        }
    }
}