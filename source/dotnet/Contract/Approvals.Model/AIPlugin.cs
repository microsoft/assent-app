// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace Microsoft.CFS.Approvals.Model;

/// <summary>
/// AIPlugin config to return for OpenAI Plugins.
/// </summary>
public class AIPlugin
{
    public string SchemaVersion { get; set; }
    public string NameForModel { get; set; }
    public string NameForHuman { get; set; }
    public string DescriptionForModel { get; set; }
    public string DescriptionForHuman { get; set; }
    public AuthConfig Auth { get; set; }
    public ApiConfig Api { get; set; }
    public string LogoUrl { get; set; }
    public string ContactEmail { get; set; }
    public string LegalInfoUrl { get; set; }
}

/// <summary>
/// Represents the authentication configuration for the AIPlugin.
/// </summary>
public class AuthConfig
{
    public string Type { get; set; }
}

/// <summary>
/// Represents the configuration for the API.
/// </summary>
public class ApiConfig
{
    public string Type { get; set; }
    public string Url { get; set; }
}