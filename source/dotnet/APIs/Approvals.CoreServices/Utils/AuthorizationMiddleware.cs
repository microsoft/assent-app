// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace Microsoft.CFS.Approvals.CoreServices.Utils
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Security.Claims;
    using System.Text;
    using System.Threading.Tasks;
    using Microsoft.AspNetCore.Http;
    using Newtonsoft.Json;
    using Newtonsoft.Json.Linq;

    /// <summary>
    /// Custom Authorization Middleware class which takes care of additional security checks
    /// </summary>
    public class AuthorizationMiddleware : IMiddleware
    {
        private static readonly string XMsClientPrincipalIdp = "X-MS-CLIENT-PRINCIPAL-IDP";
        private static readonly string XMsClientPrincipal = "X-MS-CLIENT-PRINCIPAL";

        /// <summary>
        /// Create Claims Principal from Request Headers which are added by Azure App Service Authentication (EasyAuth) and validate the required claims as applicable
        /// </summary>
        /// <param name="context"></param>
        /// <param name="next"></param>
        /// <returns></returns>
        public async Task InvokeAsync(HttpContext context, RequestDelegate next)
        {
            var claims = new List<Claim>();

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
            }

            await next(context);
        }
    }
}