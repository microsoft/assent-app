// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace Microsoft.CFS.Approvals.Utilities.Helpers;

using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Net.Mail;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Web;
using global::Azure.Core;
using global::Azure.Identity;
using Microsoft.CFS.Approvals.Common.DL.Interface;
using Microsoft.CFS.Approvals.Contracts;
using Microsoft.CFS.Approvals.LogManager.Model;
using Microsoft.CFS.Approvals.LogManager.Provider.Interface;
using Microsoft.CFS.Approvals.Utilities.Interface;
using Microsoft.Extensions.Configuration;
using Microsoft.Identity.Client;
using Newtonsoft.Json.Linq;

public class AuthenticationHelper : IAuthenticationHelper
{
    private static readonly string _serviceUnavailable = "temporarily_unavailable";
    private static readonly string _assertionType = "urn:ietf:params:oauth:grant-type:jwt-bearer";
    private static string _hmacToken = string.Empty;
    private static string _nonHmacToken = string.Empty;

    /// <summary>
    /// The log provider
    /// </summary>
    private readonly ILogProvider _logger;

    /// <summary>
    /// The configuration
    /// </summary>
    private readonly IConfiguration _config;

    /// <summary>
    /// The performance _logger
    /// </summary>
    private readonly IPerformanceLogger _performanceLogger;

    /// <summary>
    /// Constructor of AuthenticationHelper
    /// </summary>
    /// <param name="logger"></param>
    /// <param name="config"></param>
    /// <param name="performanceLogger"></param>
    public AuthenticationHelper(
        ILogProvider logger,
        IConfiguration config,
        IPerformanceLogger performanceLogger)
    {
        _logger = logger;
        _config = config;
        _performanceLogger = performanceLogger;
    }

    /// <summary>
    /// Get the OAuth 2.0 Access Token
    /// </summary>
    /// <param name="clientId">Client Id</param>
    /// <param name="appKey">Client Secret</param>
    /// <param name="authority">Authority</param>
    /// <param name="resource">Resource Uri</param>
    /// <param name="scope">Scope</param>
    /// <returns>AuthenticationResult token</returns>
    public async Task<Identity.Client.AuthenticationResult> AcquireOAuth2TokenByScopeAsync(string clientId, string appKey, string authority, string resource, string scope)
    {
        Identity.Client.AuthenticationResult result = null;
        var retryCount = 0;
        bool retry;
        do
        {
            retry = false;
            try
            {
                Identity.Client.IConfidentialClientApplication app = Identity.Client.ConfidentialClientApplicationBuilder.
                    Create(clientId).
                    WithClientSecret(appKey).
                    WithAuthority(new Uri(authority)).
                    Build();
                var scopes = new[] { resource + scope };
                result = await app.AcquireTokenForClient(scopes).ExecuteAsync();
            }
            catch (MsalException ex)
            {
                if (ex.ErrorCode == "temporarily_unavailable")
                {
                    retry = true;
                    retryCount++;
                    Thread.Sleep(3000);
                }

                Console.WriteLine($"An error occurred while acquiring a token\nTime: {DateTime.UtcNow.ToString(CultureInfo.InvariantCulture)}\nError: {ex}\nRetry: {retry}\n");
            }
        } while (retry && (retryCount < 3));

        return result;
    }

    /// <summary>
    /// Generate OAuth 2.0 User token from ClientId and Client Secret for the specified resource using the ClaimsIdentity
    /// </summary>
    /// <param name="identity"></param>
    /// <param name="parameterObject"></param>
    /// <returns></returns>
    public async Task<string> GetOnBehalfBearerToken(ClaimsIdentity identity, JObject parameterObject)
    {
        var logData = new Dictionary<LogDataKey, object>
        {
            { LogDataKey.EventType, Constants.FeatureUsageEvent }
        };

        try
        {
            string clientId = parameterObject["ClientID"].ToString();
            string appKey = parameterObject["AuthKey"].ToString();
            string resourceUrl = parameterObject["ResourceURL"].ToString();
            string authority = parameterObject["Authority"].ToString();

            logData.Add(LogDataKey.IdentityProviderClientID, clientId);
            logData.Add(LogDataKey.Uri, resourceUrl);

            if (identity?.BootstrapContext == null)
            {
                throw new InvalidOperationException("IdentityProvider_Flow:: Not able to find BootstrapContext");
            }

            string bearerAccessToken = string.Empty;

            string userAccessToken = identity.BootstrapContext as string;
            var userName = identity.FindFirst(ClaimTypes.Upn) != null ? identity.FindFirst(ClaimTypes.Upn).Value : identity.FindFirst(ClaimTypes.Email).Value;

            logData.Add(LogDataKey.UserAlias, !string.IsNullOrWhiteSpace(userName) ? new MailAddress(userName).User : string.Empty);
            logData.Add(LogDataKey.UserEmail, userName);

            UserAssertion userAssertion = new UserAssertion(userAccessToken, _assertionType);
            IConfidentialClientApplication app = ConfidentialClientApplicationBuilder.
                    Create(clientId).
                    WithClientSecret(appKey).
                    WithAuthority(new Uri(authority)).
                    Build();
            var scopes = new[] { resourceUrl + "/.default" };

            bool retry;
            int retryCount = 0;

            do
            {
                retry = false;
                try
                {
                    var result = await app.AcquireTokenOnBehalfOf(scopes, userAssertion).ExecuteAsync();
                    bearerAccessToken = result.AccessToken;
                }
                catch (MsalException ex)
                {
                    if (ex.ErrorCode == _serviceUnavailable)
                    {
                        // Transient error, OK to retry.
                        retry = true;
                        retryCount++;
                        Thread.Sleep(1000);
                    }
                }
            } while (retry && retryCount < 2);

            return bearerAccessToken;
        }
        catch (Exception ex)
        {
            _logger?.LogError(TrackingEvent.OAuth2TokenGenerationError, ex, logData);
            throw;
        }
    }

    /// <summary>
    /// Get the On Behalf OAuth 2.0 User token
    /// </summary>
    /// <param name="userAccessToken">current OAuth 2.0 user token</param>
    /// <param name="parameterObject">OAuth 2.0 token generation parameters</param>
    /// <returns>OAuth 2.0 User token with changed resource URL</returns>
    public async Task<string> GetOnBehalfUserToken(string userAccessToken, JObject parameterObject)
    {
        var logData = new Dictionary<LogDataKey, object>
        {
            { LogDataKey.EventType, Constants.FeatureUsageEvent }
        };

        try
        {
            string clientId = parameterObject["ClientID"].ToString();
            string appKey = parameterObject["AuthKey"].ToString();
            string resourceUrl = parameterObject["ResourceURL"].ToString();
            string authority = parameterObject["Authority"].ToString();

            logData.Add(LogDataKey.IdentityProviderClientID, clientId);
            logData.Add(LogDataKey.Uri, resourceUrl);

            string bearerAccessToken = string.Empty;
            UserAssertion userAssertion = new UserAssertion(userAccessToken, _assertionType);

            IConfidentialClientApplication app = ConfidentialClientApplicationBuilder.
                    Create(clientId).
                    WithClientSecret(appKey).
                    WithAuthority(new Uri(authority)).
                    Build();
            var scopes = new[] { resourceUrl + "/.default" };

            bool retry;
            int retryCount = 0;

            do
            {
                retry = false;
                try
                {
                    var result = await app.AcquireTokenOnBehalfOf(scopes, userAssertion).ExecuteAsync();
                    bearerAccessToken = result.AccessToken;
                }
                catch (MsalException ex)
                {
                    if (ex.ErrorCode == _serviceUnavailable)
                    {
                        // Transient error, OK to retry.
                        retry = true;
                        retryCount++;
                        Thread.Sleep(1000);
                    }
                }
            } while (retry && retryCount < 2);

            return bearerAccessToken;
        }
        catch (Exception ex)
        {
            _logger?.LogError(TrackingEvent.OAuth2TokenGenerationError, ex, logData);
            throw;
        }
    }

    /// <summary>
    /// Generate SAS token based on Shared Access Policy Name and value
    /// As this is the client this should have a Send Access policy key
    /// The resource URI should be the complete url which needs to be accessed
    /// </summary>
    /// <param name="parameterObject"></param>
    /// <param name="resourceUri"></param>
    /// <returns></returns>
    public string GetSASToken(JObject parameterObject, string resourceUri)
    {
        #region Logging

        var logData = new Dictionary<LogDataKey, object>
        {
            { LogDataKey.EventType, Constants.FeatureUsageEvent }
        };

        #endregion Logging

        try
        {
            string sharedAccessPolicyName = parameterObject["SASPolicyName"].ToString();
            string sharedAccessKey = parameterObject["SASKey"].ToString();

            var expiry = GetExpiry();
            string stringToSign = HttpUtility.UrlEncode(resourceUri) + "\n" + expiry;
            HMACSHA256 hmac = new HMACSHA256(Encoding.UTF8.GetBytes(sharedAccessKey));

            var signature = Convert.ToBase64String(hmac.ComputeHash(Encoding.UTF8.GetBytes(stringToSign)));
            return String.Format(CultureInfo.InvariantCulture, "SharedAccessSignature sr={0}&sig={1}&se={2}&skn={3}",
                HttpUtility.UrlEncode(resourceUri), HttpUtility.UrlEncode(signature), expiry, sharedAccessPolicyName);
        }
        catch (Exception ex)
        {
            _logger?.LogError(TrackingEvent.SASTokenGenerationError, ex, logData);
            throw;
        }
    }

    /// <summary>
    /// Get Acs simple web token from shared secret.
    /// </summary>
    /// <param name="Hmac"></param>
    public string GetAcsSimpleWebTokenFromSharedSecret(bool Hmac)
    {
        #region Logging

        var logData = new Dictionary<LogDataKey, object>
        {
            { LogDataKey.EventType, Constants.FeatureUsageEvent }
        };

        #endregion Logging

        try
        {
            var serviceBusNamespace = _config[ConfigurationKey.ServiceBusNamespace.ToString()];
            var baseaddress = "http://" + serviceBusNamespace + ".servicebus.windows.net";
            string AcsUri = @"https://" + serviceBusNamespace + "-sb.accesscontrol.windows.net/WRAPv0.9/";
            string HmacUri = @"https://" + serviceBusNamespace + ".accesscontrol.windows.net/WRAPv0.9/";

            logData.Add(LogDataKey.BaseAddress, baseaddress);

            using (var wc = new System.Net.WebClient())
            {
                var data = String.Format(CultureInfo.InvariantCulture, Constants.DataString,
                                         HttpUtility.UrlEncode(_config[ConfigurationKey.ServiceBusIssuerName.ToString()]),
                                         HttpUtility.UrlEncode(_config[ConfigurationKey.ServiceBusIssuerSecret.ToString()]),
                                         HttpUtility.UrlEncode(baseaddress));

                wc.Headers[Constants.ContentTypeKeyName] = Constants.ContentTypeKeyValue;
                String result = String.Empty;

                if (Hmac)
                {
                    if (string.IsNullOrEmpty(_hmacToken) || GetExpiryTime(_hmacToken).AddMinutes(-5) < DateTime.UtcNow)
                    {
                        logData.Add(LogDataKey.Uri, HmacUri);
                        logData.Add(LogDataKey.UriType, Constants.HmacUri);
                        using (_performanceLogger.StartPerformanceLogger("PerfLog", "SecurityHelper", string.Format(Constants.PerfLogAction, "Security Helper", "Get ACS WebToken"), logData))
                        {
                            _hmacToken = wc.UploadString(new Uri(HmacUri), Constants.PostString, data);
                        }
                    }
                    result = _hmacToken;
                }
                else
                {
                    if (string.IsNullOrEmpty(_nonHmacToken) || GetExpiryTime(_nonHmacToken).AddMinutes(-5) < DateTime.UtcNow)
                    {
                        logData.Add(LogDataKey.Uri, AcsUri);
                        logData.Add(LogDataKey.UriType, Constants.AcsUri);
                        using (_performanceLogger.StartPerformanceLogger("PerfLog", "SecurityHelper", string.Format(Constants.PerfLogAction, "Security Helper", "Get ACS WebToken"), logData))
                        {
                            _nonHmacToken = wc.UploadString(new Uri(AcsUri), Constants.PostString, data);
                        }
                    }
                    result = _nonHmacToken;
                }

                var token = result.Split(Constants.SplitToken).Single(x => x.StartsWith(Constants.Wrap_Access_TokenKeyName,
                                     StringComparison.OrdinalIgnoreCase)).Split(Constants.SplitToken2)[1];
                var decodedToken = HttpUtility.UrlDecode(token);

                var authorizationKeyValue = String.Format(CultureInfo.InvariantCulture, Constants.AuthorizationKeyString, decodedToken);
                return authorizationKeyValue;
            }
        }
        catch (Exception ex)
        {
            if (_logger != null)
            {
                _logger.LogError(TrackingEvent.AcsSimpleWebToken, ex, logData);
            }
            throw;
        }
    }

    /// <summary>
    /// Get OAuth 2.0 token generated using Managed Identity
    /// </summary>
    /// <param name="scope"></param>
    /// <returns>Managed Identity OAuth 2.0 Token</returns>
    public async Task<string> GetManagedIdentityToken(string scope)
    {
        var logData = new Dictionary<LogDataKey, object>
        {
            { LogDataKey.EventType, Constants.FeatureUsageEvent },
            { LogDataKey.Scope, scope }
        };
        try
        {
            string accessToken = string.Empty;
            var retryCount = 0;
            bool retry;
            do
            {
                retry = false;
                try
                {
                    // ADAL includes an in memory cache, so this call will only send a message to the server if the cached token is expired.
#if DEBUG
                    var tokenCredential = new DefaultAzureCredential(); // CodeQL [SM05137] Suppress CodeQL issue since we only use DefaultAzureCredential in development environments.
#else               
                    var tokenCredential = new ManagedIdentityCredential();
#endif
                    var tokenResponse = await tokenCredential.GetTokenAsync(new TokenRequestContext(new[] { string.Format(scope, ".default") }));
                    accessToken = tokenResponse.Token;
                }
                catch (MsalException ex)
                {
                    if (ex.ErrorCode == _serviceUnavailable)
                    {
                        retry = true;
                        retryCount++;
                        Thread.Sleep(3000);
                    }

                    Console.WriteLine($"An error occurred while acquiring a token\nTime: {DateTime.UtcNow.ToString(CultureInfo.InvariantCulture)}\nError: {ex}\nRetry: {retry}\n");
                }
            } while (retry && (retryCount < 3));
            return accessToken;
        }
        catch (Exception ex)
        {
            _logger?.LogError(TrackingEvent.ManagedIdentityTokenGenerationError, ex, logData);
            throw;
        }
    }

    /// <summary>
    /// Get expiry
    /// </summary>
    /// <returns></returns>
    private string GetExpiry()
    {
        TimeSpan sinceEpoch = DateTime.UtcNow - new DateTime(1970, 1, 1);

        // TODO:: Do we need the expiry time to be configurable ?
        // Setting the expiry time as 10 mins
        return Convert.ToString((int)sinceEpoch.TotalSeconds + 600);
    }

    /// <summary>
    /// Get expiry time
    /// </summary>
    /// <param name="token"></param>
    /// <returns></returns>
    private DateTime GetExpiryTime(string token)
    {
        var swt = token.Substring("wrap_access_token=\"".Length, token.Length - ("wrap_access_token=\"".Length + 1));
        var tokenValue = Uri.UnescapeDataString(swt);
        var properties = (from prop in tokenValue.Split('&')
                          let pair = prop.Split(new[] { '=' }, 2)
                          select new { Name = pair[0], Value = pair[1] })
                         .ToDictionary(p => p.Name, p => p.Value);

        var expiresOnUnixTicks = int.Parse(properties["ExpiresOn"]);
        var epochStart = new DateTime(1970, 01, 01, 0, 0, 0, 0, DateTimeKind.Utc);

        return epochStart.AddSeconds(expiresOnUnixTicks);
    }
}