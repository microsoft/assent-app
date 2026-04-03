// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace Microsoft.CFS.Approvals.Utilities.Helpers;

using global::Azure.Core;
using global::Azure.Identity;
using Microsoft.CFS.Approvals.Common.DL.Interface;
using Microsoft.CFS.Approvals.LogManager.Model;
using Microsoft.CFS.Approvals.LogManager.Provider.Interface;
using Microsoft.CFS.Approvals.Utilities.Interface;
using Microsoft.Extensions.Configuration;
using Microsoft.Identity.Client;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Globalization;
using System.Net.Http;
using System.Net.Mail;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using System.Web;
using Constants = Contracts.Constants;
using ManagedIdentityId = Identity.Client.AppConfig.ManagedIdentityId;

public class AuthenticationHelper : IAuthenticationHelper
{
    private static readonly string _serviceUnavailable = "temporarily_unavailable";
    private static readonly string _assertionType = "urn:ietf:params:oauth:grant-type:jwt-bearer";

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

    // In-memory cache for tokens, keyed by a composite of clientId, scopes, and authority.
    private static readonly ConcurrentDictionary<string, (string AccessToken, DateTimeOffset ExpiresOn)> _tokenCache = new();

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
    /// <param name="clientSecret">Client Secret</param>
    /// <param name="authority">Authority</param>
    /// <param name="scope">Scope</param>
    /// <returns>Access token</returns>
    public async Task<string> AcquireOAuth2TokenByScopeAsync(string clientId, string clientSecret, string authority, string scope)
    {
        string accessToken = string.Empty;
        var retryCount = 0;
        bool retry;
        do
        {
            retry = false;
            try
            {
                // Compose a cache key based on clientId, authority, and scopes
                string cacheKey = $"{clientId}|{authority}|{scope}";

                // Check if a valid token exists in the cache
                if (_tokenCache.TryGetValue(cacheKey, out var cachedToken))
                {
                    // Add a 1-minute buffer to avoid using a token that's about to expire
                    if (cachedToken.ExpiresOn > DateTimeOffset.UtcNow.AddMinutes(1))
                    {
                        return cachedToken.AccessToken;
                    }
                }

                using var client = new HttpClient();
                var request = new HttpRequestMessage(HttpMethod.Post, authority);
                request.Content = new FormUrlEncodedContent(
                [
                    new KeyValuePair<string, string>("grant_type", "client_credentials"),
                    new KeyValuePair<string, string>("client_id", clientId),
                    new KeyValuePair<string, string>("client_secret", clientSecret),
                    new KeyValuePair<string, string>("scope", scope)
                ]);

                var response = await client.SendAsync(request);
                response.EnsureSuccessStatusCode();
                var tokenJson = await response.Content.ReadAsStringAsync();

                // Parse the access_token and expires_in from the response JSON
                var tokenObj = JsonDocument.Parse(tokenJson);
                accessToken = tokenObj.RootElement.GetProperty("access_token").GetString();
                int expiresInSeconds = tokenObj.RootElement.TryGetProperty("expires_in", out var expiresInProp) ? expiresInProp.GetInt32() : 3600;
                var expiresOn = DateTimeOffset.UtcNow.AddSeconds(expiresInSeconds);

                // Cache the token and its expiry
                _tokenCache[cacheKey] = (accessToken, expiresOn);

            }
            catch (MsalException ex)
            {
                if (ex.ErrorCode == _serviceUnavailable)
                {
                    retry = true;
                    retryCount++;
                    Thread.Sleep(3000);
                }
            }
        } while (retry && (retryCount < 3));

        return accessToken;
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
    /// <param name="miClientId">Managed Identity Client Id</param>
    /// <param name="miAudience">Managed Identity Audience</param>
    /// <returns>OAuth 2.0 User token with changed resource URL</returns>
    public async Task<string> GetOnBehalfUserToken(string userAccessToken, JObject parameterObject, string miClientId, string miAudience)
    {
        var logData = new Dictionary<LogDataKey, object>
        {
            { LogDataKey.EventType, Constants.FeatureUsageEvent },
            { LogDataKey.IdentityProviderTokenType, Constants.OnBehalfUserToken }
        };

        try
        {
            string clientId = parameterObject["ClientID"].ToString();
            string resourceUrl = parameterObject["ResourceURL"].ToString();
            string authority = parameterObject["Authority"].ToString();

            logData.Add(LogDataKey.IdentityProviderClientID, clientId);
            logData.Add(LogDataKey.Uri, resourceUrl);

            string bearerAccessToken = string.Empty;
            UserAssertion userAssertion = new UserAssertion(userAccessToken, _assertionType);

            // Gets a token for the user-assigned Managed Identity.
            async Task<string> miAssertionProvider(AssertionRequestOptions _)
            {
                // MI tokens are always cached in memory
                var miApplication = ManagedIdentityApplicationBuilder
                    .Create(ManagedIdentityId.WithUserAssignedClientId(miClientId))
                    .Build();

                var miResult = await miApplication.AcquireTokenForManagedIdentity(miAudience)
                    .ExecuteAsync()
                    .ConfigureAwait(false);

                return miResult.AccessToken;
            }

            IConfidentialClientApplication app = ConfidentialClientApplicationBuilder.
                    Create(clientId).
                    WithClientAssertion(miAssertionProvider).
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
                    _logger?.LogError(TrackingEvent.OAuth2TokenGenerationError, ex, logData);
                    if (ex.ErrorCode == _serviceUnavailable)
                    {
                        // Transient error, OK to retry.
                        retry = true;
                        retryCount++;
                        Thread.Sleep(1000);
                    }
                }
            } while (retry && retryCount < 2);

            _logger.LogInformation(TrackingEvent.OAuth2TokenGenerationSuccessful, logData);

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
    /// Get OAuth 2.0 token generated using Managed Identity
    /// </summary>
    /// <param name="clientId"></param>
    /// <param name="resourceUri"></param>
    /// <returns>Managed Identity OAuth 2.0 Token</returns>
    public async Task<string> GetManagedIdentityToken(string clientId, string resourceUri)
    {
        var logData = new Dictionary<LogDataKey, object>
        {
            { LogDataKey.EventType, Constants.FeatureUsageEvent },
            { LogDataKey.IdentityProviderTokenType, Constants.ManagedIdentityToken },
            { LogDataKey.Scope, resourceUri },
            { LogDataKey.IdentityProviderClientID, clientId}
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
#if DEBUG
                    var tokenCredential = new DefaultAzureCredential(new DefaultAzureCredentialOptions { ManagedIdentityClientId = clientId }); // CodeQL [SM05137] Suppress CodeQL issue since we only use DefaultAzureCredential in development environments.
#else
                    var tokenCredential = new ManagedIdentityCredential(clientId);
#endif
                    // Always pass CancellationToken explicitly when calling GetTokenAsync.
                    var tokenResponse = await tokenCredential.GetTokenAsync(new TokenRequestContext(new[] { resourceUri + "/.default" }), CancellationToken.None);
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
            _logger.LogInformation(TrackingEvent.ManagedIdentityTokenGenerationSuccess, logData);
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
}
