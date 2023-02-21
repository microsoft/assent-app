// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.CFS.Approvals.SupportServices.Helper.ServiceHelper;

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
            if (!configurationHelper.appSettings[context?.HttpContext?.Request?.RouteValues["env"]?.ToString()]["AdminUserList"].Contains(context?.HttpContext?.Request?.Headers["useralias"]))
                context.Result = new UnauthorizedResult();
        }
    }
}
