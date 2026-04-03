// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace Microsoft.CFS.Approvals.CoreServices.Controllers;

using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNetCore.Mvc;
using Microsoft.CFS.Approvals.Contracts;
using Microsoft.CFS.Approvals.Contracts.DataContracts;
using Microsoft.CFS.Approvals.Model;
using Microsoft.CFS.Approvals.Utilities.Helpers;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json.Linq;

/// <summary>
/// The baseapicontroller class.
/// </summary>
[Route("api/v1/[controller]")]
[ApiController]
public class BaseApiController : ControllerBase
{
    /// <summary>
    /// The configuration.
    /// </summary>
    protected IConfiguration _config;

    /// <summary>
    /// Initializes a new instance of the <see cref="BaseApiController"/> class.
    /// </summary>
    public BaseApiController()
    { }

    /// <summary>
    /// Alias's Domain
    /// </summary>
    public string DomainName => GetDomain();

    /// <summary>
    /// Client device.
    /// </summary>
    public string ClientDevice => GetClientDevice();

    /// <summary>
    /// Xcv.
    /// </summary>
    public string Xcv => GetXcv();

    /// <summary>
    /// Tcv.
    /// </summary>
    public string MessageId => GetMessageId();

    /// <summary>
    /// on behalf user
    /// </summary>
    public User OnBehalfUser => new User
    {
        MailNickname = GetAlias(),
        UserPrincipalName = GetAlias() + DomainName,
        Id = GetUserObjectId()
    };

    /// <summary>
    /// on signed-in user
    /// </summary>
    public User SignedInUser => new User
    {
        MailNickname = GetLoggedInAlias(),
        UserPrincipalName = GetLoggedInUpn(),
        Id = GetSignedInUserId()
    };

    /// <summary>
    /// Get alias
    /// </summary>
    /// <returns>user alias</returns>
    private string GetAlias()
    {
        var alias = string.Empty;
        if (Request.Headers.Keys.Contains(Constants.UserAlias) || Request.Headers.Keys.Contains("userAlias", StringComparer.InvariantCultureIgnoreCase))
        {
            alias = Request.Headers.FirstOrDefault(x => x.Key.ToLower().Equals(Constants.UserAlias.ToLower())).Value.FirstOrDefault();
        }

        return alias;
    }

    /// <summary>
    /// Get logged in alias.
    /// </summary>
    /// <returns>logged in alias</returns>
    private string GetLoggedInAlias()
    {
        var alias = string.Empty;
        if (Request.Headers.Keys.Contains(Constants.LoggedInUserAlias) || Request.Headers.Keys.Contains("loggedInUserAlias", StringComparer.InvariantCultureIgnoreCase))
        {
            alias = Request.Headers.FirstOrDefault(x => x.Key.ToLower().Equals(Constants.LoggedInUserAlias.ToLower())).Value.FirstOrDefault();
        }
        return alias;
    }

    /// <summary>
    /// Get logged in upn.
    /// </summary>
    /// <returns>logged in user UPN</returns>
    private string GetLoggedInUpn()
    {
        var upn = string.Empty;
        if (Request.Headers.Keys.Contains(Constants.LoggedInUserUpn) || Request.Headers.Keys.Contains("loggedInUserUpn", StringComparer.InvariantCultureIgnoreCase))
        {
            upn = Request.Headers.FirstOrDefault(x => x.Key.ToLower().Equals(Constants.LoggedInUserUpn.ToLower())).Value.FirstOrDefault();
        }
        return upn;
    }

    private string GetSignedInUserId()
    {
        var objectId = string.Empty;
        if (Request.Headers.Keys.Contains(Constants.XMSClientPrincipalId))
        {
            objectId = Request.Headers.FirstOrDefault(x => x.Key.Equals(Constants.XMSClientPrincipalId)).Value.FirstOrDefault();
        }

        return objectId;
    }

    private string GetUserObjectId()
    {
        var objectId = Request.Headers.FirstOrDefault(x => x.Key.ToLower().Equals(Constants.OnBehalfUserId.ToLower())).Value.FirstOrDefault();
        if (string.IsNullOrWhiteSpace(objectId))
        {
            objectId = Request.Headers.FirstOrDefault(x => x.Key.Equals(Constants.XMSClientPrincipalId)).Value.FirstOrDefault();
        }

        return objectId;
    }

    private string GetDomain()
    {
        var domain = string.Empty;
        if (Request.Headers.Keys.Contains(Constants.Domain) || Request.Headers.Keys.Contains("domain", StringComparer.InvariantCultureIgnoreCase))
        {
            domain = Request.Headers.FirstOrDefault(x => x.Key.ToLower().Equals(Constants.Domain.ToLower())).Value.FirstOrDefault();
        }

        return domain;
    }

    /// <summary>
    /// Get Client Device
    /// </summary>
    /// <returns>client device string</returns>
    protected string GetClientDevice()
    {
        var header = Request.Headers.Where(x => x.Key.ToLower().Equals(Constants.ClientDeviceHeader.ToLower())).ToList();

        if (!header.Any())
        {
            return Constants.WebClient;
        }
        var headerEntry = header.FirstOrDefault();
        return headerEntry.Value.FirstOrDefault();
    }

    /// <summary>
    /// Get Token or Cookie
    /// </summary>
    /// <returns>token or cookie string</returns>
    protected string GetTokenOrCookie()
    {
        if (Request.Headers.ContainsKey(Constants.AuthorizationHeader))
        {
            return Request.Headers.FirstOrDefault(x => x.Key.Equals(Constants.AuthorizationHeader)).Value.FirstOrDefault();
        }
        else if (Request.Headers.ContainsKey(Constants.CookieHeader))
        {
            return Request.Headers.First(x => x.Key.Equals(Constants.CookieHeader)).Value.FirstOrDefault();
        }
        else
        {
            return string.Empty;
        }
    }

    /// <summary>
    /// Get Filter Parameters
    /// </summary>
    /// <returns>Dictionary of filter parameters</returns>
    protected Dictionary<string, object> GetFilterParameters()
    {
        string header = string.Empty;
        Dictionary<string, object> dictObj = new Dictionary<string, object>();

        if (Request.Headers.Keys.Contains(Constants.FilterParameters, StringComparer.OrdinalIgnoreCase))
        {
            header = Request.Headers.FirstOrDefault(x => x.Key.Equals(Constants.FilterParameters, StringComparison.OrdinalIgnoreCase)).Value.FirstOrDefault();
            header = string.IsNullOrWhiteSpace(header) ? "" : Uri.UnescapeDataString(header);
            JSONHelper.ConvertObjectToJSON(header);
            JObject jsonObj = JObject.Parse(header);
            dictObj = jsonObj.ToObject<Dictionary<string, object>>();
        }

        return dictObj;
    }

    /// <summary>
    /// The Get Xcv
    /// </summary>
    /// <returns>Xcv string</returns>
    private string GetXcv()
    {
        if (Request.Headers.Keys.Contains(Constants.Xcv) || Request.Headers.Keys.Contains("xcv"))
        {
            return Request.Headers.FirstOrDefault(x => x.Key.ToLower().Equals(Constants.Xcv.ToLower())).Value.FirstOrDefault();
        }
        else
        {
            return Guid.NewGuid().ToString();
        }
    }

    /// <summary>
    /// Get Tcv
    /// </summary>
    /// <returns>Tcv string</returns>
    private string GetMessageId()
    {
        if (Request.Headers.Keys.Contains(Constants.Tcv) || Request.Headers.Keys.Contains("tcv"))
        {
            return Request.Headers.FirstOrDefault(x => x.Key.ToLower().Equals(Constants.Tcv.ToLower())).Value.FirstOrDefault();
        }
        else if (Request.Headers.Keys.Contains(Constants.MessageId) || Request.Headers.Keys.Contains("messageId"))
        {
            return Request.Headers.FirstOrDefault(x => x.Key.ToLower().Equals(Constants.MessageId.ToLower())).Value.FirstOrDefault();
        }
        else
        {
            return Guid.NewGuid().ToString();
        }
    }

    /// <summary>
    /// Get Tenants.
    /// </summary>
    /// <returns>Tenants</returns>
    private string GetTenants()
    {
        var alias = string.Empty;
        if (Request.Headers.Keys.Contains(Constants.Tenants) || Request.Headers.Keys.Contains("Tenants", StringComparer.InvariantCultureIgnoreCase))
        {
            alias = Request.Headers.FirstOrDefault(x => x.Key.ToLower().Equals(Constants.Tenants.ToLower())).Value.FirstOrDefault();
        }
        return alias;
    }
}
