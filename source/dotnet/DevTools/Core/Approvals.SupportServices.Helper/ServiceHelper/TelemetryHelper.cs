// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace Microsoft.CFS.Approvals.SupportServices.Helper.ServiceHelper;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Web;
using global::Azure.Core;
using global::Azure.Identity;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using Microsoft.CFS.Approvals.DevTools.AppConfiguration;
using Microsoft.CFS.Approvals.SupportServices.Helper.ExtensionMethods;
using Microsoft.CFS.Approvals.SupportServices.Helper.Interface;
using Microsoft.CFS.Approvals.Utilities.Interface;
using Newtonsoft.Json.Linq;

/// <summary>
/// The Telemetry Helper
/// </summary>
public class TelemetryHelper : ITelemetryHelper
{
    private readonly ConfigurationHelper _configurationHelper;
    private readonly IHttpHelper _httpHelper;
    private readonly string _environment;
    private readonly TokenCredential _tokenCredential;

    /// <summary>
    /// Constructor of TelemetryHelper
    /// </summary>
    /// <param name="configurationHelper"></param>
    /// <param name="actionContextAccessor"></param>
    /// <param name="httpHelper"></param>
    public TelemetryHelper(ConfigurationHelper configurationHelper,
        IActionContextAccessor actionContextAccessor,
        IHttpHelper httpHelper,
        TokenCredential tokenCredential = null)
    {
        _environment = actionContextAccessor?.ActionContext?.RouteData?.Values["env"]?.ToString();
        _configurationHelper = configurationHelper;
        _httpHelper = httpHelper;
        _tokenCredential = tokenCredential ?? new DefaultAzureCredential();
    }

    /// <summary>
    /// Get Telemetry data
    /// </summary>
    /// <param name="customQueryParam"></param>
    /// <param name="aiScopes"></param>
    /// <returns></returns>
    public JObject GetAIData(string customQueryParam, List<JToken> aiScopes)
    {
        JObject appInsightsResponse = null;
        var ApplicationInsightsBaseURL = _configurationHelper.appSettings[_environment]["AIBaseURL"];
        try
        {
            var tokenRequestContext = new TokenRequestContext(
                new[] { ApplicationInsightsBaseURL + "/.default" });

            var token = _tokenCredential.GetToken(tokenRequestContext, default);

            foreach (var scope in aiScopes)
            {
                string baseUrl = string.Format(ApplicationInsightsBaseURL + "/v1/apps/{0}", scope?.SelectToken("applicationID")?.ToString());
                string query = "/query?query=" + HttpUtility.UrlEncode(customQueryParam);
                string requestUrl = baseUrl + query;

                var httpRequestMessage = new HttpRequestMessage(HttpMethod.Get, requestUrl);

                httpRequestMessage.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token.Token);

                HttpResponseMessage response = _httpHelper.SendRequestAsync(httpRequestMessage).Result;
                if (response.IsSuccessStatusCode)
                {
                    JObject responseData = response.Content.ReadAsStringAsync().Result.ToJObject();
                    if (CheckIfResponseHasData(responseData))
                    {
                        MergeAIScopeResponse(ref appInsightsResponse, responseData);
                    }
                }
            }
            return appInsightsResponse;
        }
        catch (Exception ex)
        {
            throw ex;
        }
    }

    /// <summary>
    /// check if the response has data
    /// </summary>
    /// <param name="response">Rest api query response</param>
    /// <returns>true or false</returns>
    private bool CheckIfResponseHasData(JObject response)
    {
        if (response != null)
        {
            var rows = response.SelectToken("$..rows");
            if (rows.HasValues)
            {
                return true;
            }
        }
        return false;
    }

    /// <summary>
    /// Merge AI Scope response
    /// </summary>
    /// <param name="response">AI response collection</param>
    /// <param name="responseData">response of specific AI scope</param>
    private void MergeAIScopeResponse(ref JObject response, JObject responseData)
    {
        if (response == null)
        {
            response = responseData;
        }
        else
        {
            var rows = response.SelectToken("$..rows").Value<JArray>();
            foreach (var row in responseData.SelectToken("$..rows").Children())
                rows.Add(row);
        }
    }
}