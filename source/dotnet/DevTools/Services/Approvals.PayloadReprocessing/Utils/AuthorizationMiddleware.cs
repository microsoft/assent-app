// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace Microsoft.CFS.Approvals.PayloadReprocessing.Utils
{
    using System;
    using System.Linq;
    using System.Security.Claims;

    /// <summary>
    /// Custom Authorization Middleware class which takes care of additional security checks
    /// </summary>
    public class AuthorizationMiddleware : IAuthorizationMiddleware
    {
        /// <summary>
        /// Validates the required claims in the Claims Principal which are added by Azure App Service Authentication (EasyAuth)
        /// </summary>
        /// <param name="claimsPrincipal"></param>
        /// <returns></returns>
        public bool IsValidClaims(ClaimsPrincipal claimsPrincipal)
        {
            #region Check for Valid AppID

            // Get list of AppIds
            var validAppIds = Environment.GetEnvironmentVariable("ValidAppIds");
            var listOfValidAppIds = validAppIds.Split(';');

            if (claimsPrincipal != null)
            {
                var appid = claimsPrincipal.Claims.FirstOrDefault(c => c.Type.Equals("appid")) ?? claimsPrincipal.Claims.FirstOrDefault(c => c.Type.Equals("aud"));

                // if AppId is null or the AppId fetched from claims is different from the Valid AppId list value then return UnAuthorized Response
                if (appid == null || !listOfValidAppIds.Any(id => id.Equals(appid.Value, StringComparison.InvariantCultureIgnoreCase)))
                {
                    return false;
                }
            }

            #endregion Check for Valid AppID

            return true;
        }
    }
}
