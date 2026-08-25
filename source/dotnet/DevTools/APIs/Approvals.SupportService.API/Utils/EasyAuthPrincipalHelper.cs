// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace Microsoft.CFS.Approvals.SupportService.API.Utils;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Text.Json;
using Microsoft.AspNetCore.Http;

internal static class EasyAuthPrincipalHelper
{
    internal const string XMsClientPrincipalIdp = "X-MS-CLIENT-PRINCIPAL-IDP";
    internal const string XMsClientPrincipal = "X-MS-CLIENT-PRINCIPAL";

    internal static bool TryBuildPrincipal(IHeaderDictionary headers, string authenticationType, out ClaimsPrincipal principal, out string error)
    {
        principal = null;
        error = null;

        if (!headers.ContainsKey(XMsClientPrincipalIdp) || !headers.ContainsKey(XMsClientPrincipal))
        {
            error = "EasyAuth headers are missing.";
            return false;
        }

        var encodedPrincipal = headers[XMsClientPrincipal].FirstOrDefault();
        if (string.IsNullOrWhiteSpace(encodedPrincipal))
        {
            error = "EasyAuth principal header is empty.";
            return false;
        }

        try
        {
            var claims = new List<Claim>();
            var decoded = Encoding.UTF8.GetString(Convert.FromBase64String(encodedPrincipal));

            using var document = JsonDocument.Parse(decoded);
            if (document.RootElement.TryGetProperty("claims", out var claimsElement) && claimsElement.ValueKind == JsonValueKind.Array)
            {
                foreach (var claimObject in claimsElement.EnumerateArray())
                {
                    if (!claimObject.TryGetProperty("typ", out var typeElement) || !claimObject.TryGetProperty("val", out var valueElement))
                    {
                        continue;
                    }

                    var claimType = typeElement.GetString();
                    var claimValue = valueElement.GetString();
                    if (!string.IsNullOrWhiteSpace(claimType) && !string.IsNullOrWhiteSpace(claimValue))
                    {
                        claims.Add(new Claim(claimType, claimValue));
                    }
                }
            }

            if (!claims.Any())
            {
                error = "No claims found in EasyAuth principal.";
                return false;
            }

            principal = new ClaimsPrincipal(new ClaimsIdentity(claims, authenticationType));
            return true;
        }
        catch
        {
            error = "Invalid EasyAuth principal format.";
            return false;
        }
    }
}
