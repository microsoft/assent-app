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
using Microsoft.CFS.Approvals.Contracts.DataContracts;
using Microsoft.CFS.Approvals.Core.BL.Interface;
using Microsoft.CFS.Approvals.Extensions;
using Microsoft.CFS.Approvals.Model;
using Microsoft.CFS.Approvals.Utilities.Interface;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;


/// <summary>
/// Custom Authorization Middleware class which takes care of additional security checks
/// </summary>
public class AuthorizationMiddleware : IMiddleware
{
    private readonly IDelegationHelper _delegationHelper;
    private readonly IApprovalTenantInfoHelper _approvalTenantInfoHelper;
    private readonly INameResolutionHelper _nameResolutionHelper;
    private readonly IConfiguration _configuration;
    private static readonly string XMsClientPrincipalIdp = "X-MS-CLIENT-PRINCIPAL-IDP";
    private static readonly string XMsClientPrincipalId = "X-MS-CLIENT-PRINCIPAL-ID";
    private static readonly string XMsClientPrincipal = "X-MS-CLIENT-PRINCIPAL";

    /// <summary>
    /// Constructor
    /// </summary>
    public AuthorizationMiddleware(IApprovalTenantInfoHelper approvalTenantInfoHelper, IDelegationHelper delegationHelper, INameResolutionHelper nameResolutionHelper, IConfiguration configuration)
    {
        _delegationHelper = delegationHelper;
        _approvalTenantInfoHelper = approvalTenantInfoHelper;
        _nameResolutionHelper = nameResolutionHelper;
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
        var claims = new List<Claim>();
        var userAlias = string.Empty;
        var userPrincipalName = string.Empty;
        string currentDomain = string.Empty;
        List<UserDelegationSetting> currentUserDelegation = null;

        // Get UserPrincipalName /alias from Header
        if (context.Request.Headers.Keys.Contains("X-MS-CLIENT-PRINCIPAL-NAME"))
        {
            userPrincipalName = context.Request.Headers["X-MS-CLIENT-PRINCIPAL-NAME"].ToString();
            var whitelistedDomains = _configuration[Constants.WhitelistedDomains]?.Split(";").ToList();
            whitelistedDomains.ForEach(domain =>
            {
                if (userPrincipalName.EndsWith(domain, StringComparison.InvariantCultureIgnoreCase))
                {
                    userAlias = new MailAddress(userPrincipalName).User;
                    currentDomain = domain;
                }
            });

            context.Request.Headers.Add(Constants.LoggedInUserUpn, userPrincipalName);
        }

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
                if (context.Request.Headers.ContainsKey(Constants.LoggedInUserUpn))
                    context.Request.Headers[Constants.LoggedInUserUpn] = userPrincipalName;
                else
                    context.Request.Headers.Add(Constants.LoggedInUserUpn, userPrincipalName);
            }

            #endregion Check for Reserved Headers

            #region Check for Delegation Headers

            if (!string.IsNullOrWhiteSpace(currentDomain))
            {
                if (context.Request.Headers.ContainsKey(Constants.Domain))
                {
                    context.Request.Headers[Constants.Domain] = currentDomain;
                }
                else
                {
                    context.Request.Headers.Add(Constants.Domain, currentDomain);
                }
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
                    context.Request.Headers.Add(Constants.UserAlias, userAlias);
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
}