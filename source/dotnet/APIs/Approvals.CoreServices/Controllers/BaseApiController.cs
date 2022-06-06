// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace Microsoft.CFS.Approvals.CoreServices.Controllers
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Net.Mail;
    using Microsoft.AspNetCore.Mvc;
    using Microsoft.CFS.Approvals.Contracts;
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
        /// User alias.
        /// </summary>
        public string Alias => GetAlias();

        /// <summary>
        /// Logged-in alias.
        /// </summary>
        public string LoggedInAlias => GetLoggedInAlias();

        /// <summary>
        /// Client device.
        /// </summary>
        public string Host => GetClientDevice();

        /// <summary>
        /// Xcv.
        /// </summary>
        public string Xcv => GetXcv();

        /// <summary>
        /// Tcv.
        /// </summary>
        public string Tcv => GetTcv();

        /// <summary>
        /// The Get Alias
        /// </summary>
        /// <returns>user alias</returns>
        private string GetAlias()
        {
            var alias = string.Empty;
            if (Request.Headers.Keys.Contains(Constants.UserAlias) || Request.Headers.Keys.Contains("userAlias", StringComparer.InvariantCultureIgnoreCase))
            {
                alias = Request.Headers.FirstOrDefault(x => x.Key.ToLower().Equals(Constants.UserAlias.ToLower())).Value.FirstOrDefault();
            }
            else
            {
                if (Request.Headers.Keys.Contains("X-MS-CLIENT-PRINCIPAL-NAME"))
                {
                    alias = new MailAddress(Request.Headers["X-MS-CLIENT-PRINCIPAL-NAME"].ToString()).User;
                }
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
            else
            {
                if (Request.Headers.Keys.Contains("X-MS-CLIENT-PRINCIPAL-NAME"))
                {
                    alias = new MailAddress(Request.Headers["X-MS-CLIENT-PRINCIPAL-NAME"].ToString()).User;
                }
            }
            return alias;
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
            var result = string.Empty;
            if (!Request.Headers.Keys.Contains(Constants.AuthorizationHeader) && Request.Headers.Keys.Contains(Constants.CookieHeader))
            {
                result = Request.Headers.First(x => x.Key.Equals(Constants.CookieHeader)).Value.FirstOrDefault();
            }
            else if (Request.Headers.Keys.Contains(Constants.AuthorizationHeader) && !Request.Headers.Keys.Contains(Constants.CookieHeader))
            {
                result = Request.Headers.FirstOrDefault(x => x.Key.Equals(Constants.AuthorizationHeader)).Value.FirstOrDefault();
            }
            return result;
        }

        /// <summary>
        /// Get Filter Parameters
        /// </summary>
        /// <returns>Dictionary of filter parameters</returns>
        protected Dictionary<string, object> GetFilterParameters()
        {
            string header = string.Empty;
            Dictionary<string, object> dictObj = new Dictionary<string, object>();

            if (Request.Headers.Keys.Contains(Constants.FilterParameters))
            {
                header = Request.Headers.FirstOrDefault(x => x.Key.Equals(Constants.FilterParameters)).Value.FirstOrDefault();
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
        private string GetTcv()
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
    }
}