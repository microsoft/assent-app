// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace Microsoft.CFS.Approvals.LogManager.OpenTelemetry;

using System;
using Microsoft.CFS.Approvals.Contracts;
using Microsoft.CFS.Approvals.LogManager.Interface;
using Microsoft.Extensions.Configuration;

public class AuditFactory : IAuditFactory
{    
    /// <summary>
    /// The IConfiguration
    /// </summary>
    private readonly IConfiguration _config = null;
    /// <summary>
    /// AuditFactory Constructor
    /// </summary>
    /// <param name="config"></param>
    public AuditFactory(IConfiguration config)
    {        
        _config = config;
    }

    /// <summary>
    /// This method will create the instance of AuditLogger
    /// </summary>
    public IAuditLogger GetAuditLogger()
    {
        return (IAuditLogger)Activator.CreateInstance(Type.GetType(_config[ConfigurationKey.AuditLoggerClass.ToString()]));
    }
}
