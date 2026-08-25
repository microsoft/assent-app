// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace Microsoft.CFS.Approvals.CoreServices.Utils;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Mail;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.CFS.Approvals.Contracts;
using Microsoft.CFS.Approvals.Core.BL.Interface;
using Microsoft.CFS.Approvals.Extensions;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;


/// <summary>
/// Custom Authorization Middleware class which takes care of additional security checks
/// </summary>
public class AuthorizationMiddleware : IMiddleware
{
    private readonly IApprovalTenantInfoHelper _approvalTenantInfoHelper;
    private readonly IConfiguration _configuration;

    /// <summary>
    /// Constructor
    /// </summary>
    public AuthorizationMiddleware(IApprovalTenantInfoHelper approvalTenantInfoHelper, IConfiguration configuration)
    {
        _approvalTenantInfoHelper = approvalTenantInfoHelper;
        _configuration = configuration;
    }

    /// <summary>
    /// Create Claims Principal from Request Headers which are added by Azure App Service Authentication (EasyAuth) and validate the required claims as applicable
    /// </summary>
    /// <param name="context"></param>
    /// <param name="next"></param>
    /// <returns></returns>
    public async Task InvokeAsync(HttpContext context, RequestDelegate next)
    {
        // Always strip reserved headers injected by clients.
        if (context.Request.Headers.ContainsKey(Constants.LoggedInUserAlias))
        {
            context.Request.Headers.Remove(Constants.LoggedInUserAlias);
        }

        // SECURITY: Reject requests that bypass EasyAuth (App Service Authentication)
        if (!EasyAuthPrincipalHelper.TryBuildPrincipal(context.Request.Headers, nameof(AuthorizationMiddleware), out var principal, out _))
        {
            context.Response.StatusCode = StatusCodes.Status401Unauthorized;
            await context.Response.WriteAsync("Unauthorized request");
            return;
        }

        context.User = principal;

        if (context.User != null)
        {

            #region Check for Valid AppID

            // Get list of AppIds
            var validAppIds = Environment.GetEnvironmentVariable("ValidAppIds");
            var listOfValidAppIds = validAppIds.Split(';');


            // Check azp (user-delegated tokens) first, then appid (app-only tokens)
            // SECURITY: Do NOT fallback to aud claim as it represents the API's own client ID
            var clientAppId = GetClaimValue(context.User, "azp", "appid");

            // if clientAppId is null or not in the Valid AppId list then return UnAuthorized Response
            if (string.IsNullOrWhiteSpace(clientAppId) || !listOfValidAppIds.Any(id => id.Equals(clientAppId, StringComparison.InvariantCultureIgnoreCase)))
            {
                context.Response.StatusCode = StatusCodes.Status401Unauthorized;
                await context.Response.WriteAsync("Unauthorized request - invalid client application");
                return;
            }
            else
            {
                context.Request.Headers.Add(Constants.AppClientId, clientAppId);
            }

            #endregion Check for Valid AppID

            var userAlias = string.Empty;
            var currentDomain = string.Empty;
            var userPrincipalName = GetClaimValue(context.User, ClaimTypes.Upn, "upn", "preferred_username");
            var whitelistedDomains = _configuration[Constants.WhitelistedDomains]?.Split(";").ToList();
            whitelistedDomains.ForEach(domain =>
            {
                if (userPrincipalName.EndsWith(domain, StringComparison.InvariantCultureIgnoreCase))
                {
                    userAlias = new MailAddress(userPrincipalName).User;
                    currentDomain = domain;
                }
            });

            context.Request.Headers[Constants.LoggedInUserAlias] = userAlias;
            context.Request.Headers[Constants.LoggedInUserUpn] = userPrincipalName;

            #region Check for Delegation Headers

            if (!string.IsNullOrWhiteSpace(currentDomain))
            {
                context.Request.Headers[Constants.Domain] = currentDomain;
            }
            if (!string.IsNullOrWhiteSpace(userAlias))
            {
                // Check for External Delegation
                if (context.Request.Headers.ContainsKey(Constants.DelegatedUserAlias) && context.Request.Headers.ContainsKey(Constants.UserAlias))
                {
                    string externalDelegatedUser = context.Request.Headers.FirstOrDefault(x => x.Key.ToLower().Equals(Constants.DelegatedUserAlias.ToLower())).Value.FirstOrDefault();
                    string internalDelegatedUser = context.Request.Headers.FirstOrDefault(x => x.Key.ToLower().Equals(Constants.UserAlias.ToLower())).Value.FirstOrDefault();
                    //This check to be removed once UI changes are done to pass either of one header - UserAlias or DelegatedUserAlias
                    if (!externalDelegatedUser.Equals(internalDelegatedUser, StringComparison.InvariantCultureIgnoreCase))
                    {
                        context.Response.StatusCode = StatusCodes.Status400BadRequest;
                        await context.Response.WriteAsync("You have passed both UserAlias (to be passed for internal delegation) and DelegatedUserAlias (to be passed for external delegation) in httpHeader. Only one of them can be passed at any given point of time. This is an invalid request and will not be processed.");
                        return;
                    }
                }

                if (context.Request.Headers.ContainsKey(Constants.DelegatedUserAlias))
                {
                    string delegatedUser = context.Request.Headers.FirstOrDefault(x => x.Key.ToLower().Equals(Constants.DelegatedUserAlias.ToLower())).Value.FirstOrDefault();

                    if (!context.Request.Headers.ContainsKey(Constants.TenantId) ||
                        !(_approvalTenantInfoHelper.GetTenantInfo(int.Parse(context.Request.Headers.FirstOrDefault(x => x.Key.ToLower().Equals(Constants.TenantId.ToLower())).Value.FirstOrDefault()))?.EnableExternalUserDelegation).GetValueOrDefault())
                    {
                        // Throwing back a bad request so that this message is not processed
                        context.Response.StatusCode = StatusCodes.Status400BadRequest;
                        await context.Response.WriteAsync("External delegation feature is not enabled for the selected application. Application can be specified by TenantId passed in httpHeader.");
                        return;
                    }
                    context.Request.Headers[Constants.UserAlias] = delegatedUser;
                }
                else if (!context.Request.Headers.ContainsKey(Constants.UserAlias))
                {
                    context.Request.Headers[Constants.UserAlias] = userAlias;
                }
                else
                {
                    context.Request.Headers[Constants.Domain] = context.Request.Headers[Constants.OnBehalfUserUpn].ToString().GetDomainFromUPN();
                }
            }

            #endregion Check for Delegation Headers
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