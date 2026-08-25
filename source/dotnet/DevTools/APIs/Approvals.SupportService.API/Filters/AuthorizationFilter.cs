// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.CFS.Approvals.Contracts;
using Microsoft.CFS.Approvals.DevTools.AppConfiguration;

namespace Microsoft.CFS.Approvals.SupportService.API.Filters
{
    [AttributeUsage(AttributeTargets.All, AllowMultiple = true)]
    public class AuthorizationFilter : Attribute, IAuthorizationFilter
    {
        /// <summary>
        /// The Configuration Helper
        /// </summary>
        private ConfigurationHelper configurationHelper { get; }
        public AuthorizationFilter(ConfigurationHelper configurationHelper)
        {
            this.configurationHelper = configurationHelper;
        }
        public void OnAuthorization(AuthorizationFilterContext context)
        {
            // Derive caller identity from server-set header (populated by AuthorizationMiddleware
            // from validated X-MS-CLIENT-PRINCIPAL-NAME), never from client-controlled header
            var loggedInAlias = context?.HttpContext?.Request?.Headers[Constants.LoggedInUserAlias].FirstOrDefault();
            if (string.IsNullOrWhiteSpace(loggedInAlias))
            {
                context.Result = new UnauthorizedResult();
                return;
            }

            // Resolve environment from route; fall back to first configured environment
            // (same pattern as AuthorizationMiddleware) for routes without {env}
            var env = context?.HttpContext?.Request?.RouteValues["env"]?.ToString();
            if (string.IsNullOrWhiteSpace(env))
            {
                var environmentNames = Environment.GetEnvironmentVariable("Environmentlist");
                env = environmentNames?.Split(',').FirstOrDefault()?.Trim();
            }

            if (string.IsNullOrWhiteSpace(env) || !configurationHelper.appSettings.ContainsKey(env))
            {
                context.Result = new UnauthorizedResult();
                return;
            }

            // Parse admin list into exact-match set (semicolon-delimited) with case-insensitive comparison
            var adminListValue = configurationHelper.appSettings[env]["AdminUserList"] ?? string.Empty;
            var adminSet = new HashSet<string>(
                adminListValue.Split(';', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries),
                StringComparer.OrdinalIgnoreCase);

            if (!adminSet.Contains(loggedInAlias))
            {
                context.Result = new UnauthorizedResult();
            }
        }
    }
}
