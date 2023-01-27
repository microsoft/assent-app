// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace Microsoft.CFS.Approvals.Common.DL.Interface;

using System;
using System.Collections.Generic;
using Microsoft.CFS.Approvals.LogManager.Model;

public interface IPerformanceLogger
{
    /// <summary>
    /// Start performance logger
    /// </summary>
    /// <param name="logName"></param>
    /// <param name="category"></param>
    /// <param name="action"></param>
    /// <param name="arguments"></param>
    /// <param name="enable"></param>
    /// <returns></returns>
    IDisposable StartPerformanceLogger(string logName, string category, string action, IDictionary<LogDataKey, object> arguments, bool enable = true);

    /// <summary>
    /// Start performance logger
    /// </summary>
    /// <param name="logName"></param>
    /// <param name="category"></param>
    /// <param name="action"></param>
    /// <param name="arguments"></param>
    /// <param name="enable"></param>
    /// <returns></returns>
    IDisposable StartPerformanceLogger(string logName, string category, string action, IDictionary<string, string> arguments, bool enable = true);
}