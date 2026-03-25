// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace Microsoft.CFS.Approvals.SupportService.API.Utils;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Mail;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.CFS.Approvals.Contracts;
using Microsoft.CFS.Approvals.DevTools.AppConfiguration;
using Microsoft.CFS.Approvals.SupportServices.Helper.ServiceHelper;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

/// <summary>
/// Custom Authorization Middleware class which takes care of additional security checks
/// </summary>
public class AuthorizationMiddleware : IMiddleware
{
    private static readonly string XMsClientPrincipalIdp = "X-MS-CLIENT-PRINCIPAL-IDP";
    private static readonly string XMsClientPrincipal = "X-MS-CLIENT-PRINCIPAL";

    private readonly ConfigurationHelper _configurationHelper;

    /// <summary>
    /// Constructor
    /// </summary>
    public AuthorizationMiddleware(ConfigurationHelper configurationHelper)
    {
        _configurationHelper = configurationHelper;
    }

    /// <summary>
    /// Create Claims Principal from Request Headers which are added by Azure App Service Authentication (EasyAuth) and validate the required claims as applicable
    /// </summary>
    /// <param name="context"></param>
    /// <param name="next"></param>
    /// <returns></returns>
    public async Task InvokeAsync(HttpContext context, RequestDelegate next)
    {
        var claims = new List<Claim>();
        var userAlias = string.Empty;
        string environment = context?.Request?.RouteValues["env"]?.ToString();
        if (string.IsNullOrWhiteSpace(environment))
        {
            var environmentnames = Environment.GetEnvironmentVariable("Environmentlist");
            if (environmentnames != null)
            {
                environment = environmentnames.Split(',').ToList()?.FirstOrDefault()?.Trim();
            }
            else
            {
                context.Response.StatusCode = StatusCodes.Status500InternalServerError;
                await context.Response.WriteAsync("Configuration data is invalid");
                return;
            }
        }

        // Get UserPrincipalName /alias from Header
        if (context.Request.Headers.ContainsKey("X-MS-CLIENT-PRINCIPAL-NAME"))
        {
            userAlias = context.Request.Headers["X-MS-CLIENT-PRINCIPAL-NAME"].ToString();
            var whitelistedDomains = _configurationHelper.appSettings[environment][Constants.WhitelistedDomains]?.Split(";").ToList();
            whitelistedDomains.ForEach(domain =>
            {
                if (userAlias.EndsWith(domain, StringComparison.InvariantCultureIgnoreCase))
                {
                    userAlias = new MailAddress(userAlias).User;
                }
            });

            // Authorize after the user has been authenticated by App Service Authentication Service
            if (context.Request.Headers.ContainsKey(XMsClientPrincipalIdp))
            {
                if (context.Request.Headers.ContainsKey(XMsClientPrincipal))
                {
                    var clientPrincipal = JsonConvert.DeserializeObject<JObject>(
                        Encoding.UTF8.GetString(Convert.FromBase64String(context.Request.Headers[XMsClientPrincipal].FirstOrDefault())));
                    foreach (var claimObj in clientPrincipal["claims"]?.ToObject<JObject[]>())
                    {
                        claims.Add(new Claim(claimObj["typ"]?.ToString(), claimObj["val"]?.ToString()));
                    }
                }
                context.User = new ClaimsPrincipal(new ClaimsIdentity(claims));

                #region Check for Valid AppID

                // Get list of AppIds
                var validAppIds = Environment.GetEnvironmentVariable("ValidAppIds");
                var listOfValidAppIds = validAppIds.Split(';');

                if (context.User != null)
                {
                    var appid = context.User.Claims.FirstOrDefault(c => c.Type.Equals("appid")) ?? context.User.Claims.FirstOrDefault(c => c.Type.Equals("aud"));

                    // if AppId is null or the AppId fetched from claims is different from the Valid AppId list value then return UnAuthorized Response
                    if (appid == null || !listOfValidAppIds.Any(id => id.Equals(appid.Value, StringComparison.InvariantCultureIgnoreCase)))
                    {
                        context.Response.StatusCode = StatusCodes.Status401Unauthorized;
                        await context.Response.WriteAsync("Unauthorized request");
                        return;
                    }
                }

                #endregion Check for Valid AppID

                #region Check for Reserved Headers

                if (context.Request.Headers.ContainsKey(Constants.LoggedInUserAlias))
                {
                    // Logging the details of LoggedInUserAliasHeader header and actual logged in alias details
                    var loggedInUserAliasHeaderValue = context.Request.Headers.FirstOrDefault(x => x.Key.ToLower().Equals(Constants.LoggedInUserAlias.ToLower())).Value.FirstOrDefault();

                    // Throwing back a bad request so that this message is not processed
                    context.Response.StatusCode = StatusCodes.Status403Forbidden;
                    await context.Response.WriteAsync("Forbidden: You are not allowed to send a reserved httpHeader value under LoggedInUserAliasHeader. This is an invalid request and will not be processed.");
                    return;
                }
                else
                {
                    context.Request.Headers.Add(Constants.LoggedInUserAlias, userAlias);
                }

                #endregion Check for Reserved Headers
            }
        }

        await next(context);
    }
}