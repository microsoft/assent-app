// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace Microsoft.CFS.Approvals.SupportService.API.Utils;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Mail;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.CFS.Approvals.Contracts;
using Microsoft.CFS.Approvals.DevTools.AppConfiguration;
using Microsoft.CFS.Approvals.SupportServices.Helper.ServiceHelper;

/// <summary>
/// Custom Authorization Middleware class which takes care of additional security checks
/// </summary>
public class AuthorizationMiddleware : IMiddleware
{
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

            if (!EasyAuthPrincipalHelper.TryBuildPrincipal(context.Request.Headers, Constants.EasyAuthScheme, out var principal, out _))
            {
                context.Response.StatusCode = StatusCodes.Status401Unauthorized;
                await context.Response.WriteAsync("Unauthorized request");
                return;
            }

            context.User = principal;

            #region Check for Valid AppID

            if (context.User != null)
            {
                var clientAppId = GetClaimValue(context.User, "azp", "appid");
                if (string.IsNullOrWhiteSpace(clientAppId))
                {
                    context.Response.StatusCode = StatusCodes.Status401Unauthorized;
                    await context.Response.WriteAsync("Unauthorized request");
                    return;
                }

                var validAppIds = Environment.GetEnvironmentVariable("ValidAppIds");
                var listOfValidAppIds = validAppIds?.Split(';', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries) ?? Array.Empty<string>();

                if (!listOfValidAppIds.Any(id => id.Equals(clientAppId, StringComparison.InvariantCultureIgnoreCase)))
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

        await next(context);
    }

    /// <summary>
    /// Gets the claim value from the claims principal based on the provided claim types.
    /// </summary>
    /// <param name="principal">The claims principal.</param>
    /// <param name="claimTypes">The claim types to search for.</param>
    /// <returns>The claim value if found; otherwise, an empty string.</returns>
    private static string GetClaimValue(ClaimsPrincipal principal, params string[] claimTypes)
    {
        foreach (var claimType in claimTypes)
        {
            var value = principal.Claims.FirstOrDefault(c => c.Type.Equals(claimType, StringComparison.InvariantCultureIgnoreCase))?.Value;
            if (!string.IsNullOrWhiteSpace(value))
            {
                return value;
            }
        }

        return string.Empty;
    }
}