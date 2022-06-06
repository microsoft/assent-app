// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace Microsoft.CFS.Approvals.LogManager.Model
{
    using System.Collections.Generic;

    /// <summary>
    /// Application Insight Telemetry Data class
    /// </summary>
    internal class AITelemetryData
    {
        public string EventName { get; set; }
        public Dictionary<string, string> CustomProperties { get; set; }
        public Dictionary<string, double> CustomMetrics { get; set; }
        public string EventType { get; set; }
    }
}