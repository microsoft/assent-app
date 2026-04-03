// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace Microsoft.CFS.Approvals.Core.BL.Factory;

using System;
using Microsoft.CFS.Approvals.Common.DL.Interface;
using Microsoft.CFS.Approvals.Contracts;
using Microsoft.CFS.Approvals.Core.BL.Interface;
using Microsoft.CFS.Approvals.Utilities.Interface;
using Microsoft.Extensions.Configuration;

public class ARConverterFactory : IARConverterFactory
{
    /// <summary>
    /// The performance logger
    /// </summary>
    private readonly IPerformanceLogger _performanceLogger = null;

    //THe Configuration
    private readonly IConfiguration _config = null;

    private readonly INameResolutionHelper _nameResolutionHelper = null;

    /// <summary>
    /// ARConverterFactory Constructor
    /// </summary>
    /// <param name="performanceLogger"></param>
    /// <param name="config"></param>
    public ARConverterFactory(IPerformanceLogger performanceLogger, IConfiguration config, INameResolutionHelper nameResolutionHelper)
    {
        _performanceLogger = performanceLogger;
        _config = config;
        _nameResolutionHelper = nameResolutionHelper;
    }

    public IARConverter GetARConverter(ConfigurationKey arConverterClassName = ConfigurationKey.ARConverterClass)
    {
        return (IARConverter)Activator.CreateInstance(Type.GetType(_config[arConverterClassName.ToString()]), _performanceLogger, _config, _nameResolutionHelper);
    }
}
