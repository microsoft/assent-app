// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace Microsoft.CFS.Approvals.PayloadReprocessing.Utils
{

    using System.Security.Claims;

    public interface IAuthorizationMiddleware
    {
        bool IsValidClaims(ClaimsPrincipal claimsPrincipal);
    }
}
