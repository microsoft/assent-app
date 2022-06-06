// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace Microsoft.CFS.Approvals.Common.BL
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using Microsoft.ApplicationInsights;
    using Microsoft.ApplicationInsights.Extensibility;
    using Microsoft.CFS.Approvals.Common.DL.Interface;
    using Microsoft.CFS.Approvals.LogManager.Model;
    using Microsoft.Extensions.Configuration;

    /// <summary>
    /// The Performance logger class
    /// </summary>
    public class PerformanceLogger : IPerformanceLogger
    {
        private static TelemetryClient _telemetryClient = null;
        private string _logName = string.Empty;
        private string _category = string.Empty;
        private string _action = string.Empty;
        private IDictionary<string, string> _logDataProperties = new Dictionary<string, string>();

        /// <summary>
        /// Performance Logger constructor.
        /// </summary>
        /// <param name="config"></param>
        public PerformanceLogger(IConfiguration config)
        {
            var appInsightsKey = config["APPINSIGHTS_INSTRUMENTATIONKEY"];
            if (appInsightsKey != null)
            {
                var tConfig = new TelemetryConfiguration { InstrumentationKey = appInsightsKey };
                _telemetryClient = new TelemetryClient(tConfig);
            }
        }

        /// <summary>
        /// Start performance logger.
        /// </summary>
        /// <param name="logName"></param>
        /// <param name="category"></param>
        /// <param name="action"></param>
        /// <param name="arguments"></param>
        /// <param name="enable"></param>
        /// <returns>Custom performance tracer.</returns>
        public IDisposable StartPerformanceLogger(string logName, string category, string action, IDictionary<LogDataKey, object> arguments, bool enable = true)
        {
            _logName = logName;
            _category = category;
            _action = action;
            _logDataProperties = arguments.ToDictionary(k => k.Key.ToString(), k => k.Value == null ? "" : k.Value.ToString());
            return new CustomPerformanceTracer(LogDuration, logName, category, action, _logDataProperties, enable);
        }

        /// <summary>
        /// Start performance logger.
        /// </summary>
        /// <param name="logName"></param>
        /// <param name="category"></param>
        /// <param name="action"></param>
        /// <param name="arguments"></param>
        /// <param name="enable"></param>
        /// <returns>Custom performance tracer.</returns>
        public IDisposable StartPerformanceLogger(string logName, string category, string action, IDictionary<string, string> arguments, bool enable = true)
        {
            return new CustomPerformanceTracer(LogDuration, logName, category, action, arguments, enable);
        }

        public void Dispose()
        {
            // Method intentionally left empty.
        }

        /// <summary>
        /// Log duration.
        /// </summary>
        /// <param name="span"></param>
        private void LogDuration(TimeSpan span)
        {
            string nameFormat = "{0}-{1}-{2}";
            if (_telemetryClient != null)
            {
                _telemetryClient.TrackMetric(new Microsoft.ApplicationInsights.DataContracts.MetricTelemetry()
                {
                    Name = string.Format(nameFormat, _logName, _category, _action),
                    Sum = span.TotalMilliseconds
                });
            }
        }
    }
}