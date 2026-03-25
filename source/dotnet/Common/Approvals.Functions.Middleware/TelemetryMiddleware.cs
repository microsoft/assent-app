// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace Microsoft.CFS.Approvals.Functions.Middleware;

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.Threading.Tasks;
using Microsoft.ApplicationInsights;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Middleware;

public class TelemetryMiddleware(TelemetryClient telemetryClient) : IFunctionsWorkerMiddleware
{
    public async Task Invoke(FunctionContext context, FunctionExecutionDelegate next)
    {
        var startTime = DateTimeOffset.UtcNow;
        var stopwatch = Stopwatch.StartNew();
        var functionName = context.FunctionDefinition.Name;

        var telemetryProperties = new Dictionary<string, string>
        {
            { "FunctionName", functionName },
            { "InvocationId", context.InvocationId }
        };

        TrackMessagePickupDelay(functionName, context, startTime, telemetryProperties);

        try
        {
            await next(context);
        }
        finally
        {
            stopwatch.Stop();
            telemetryClient.TrackMetric($"{functionName} ExecutionTime", stopwatch.ElapsedMilliseconds, telemetryProperties);
        }
    }

    private void TrackMessagePickupDelay(string functionName, FunctionContext context, DateTimeOffset pickupTime, Dictionary<string, string> telemetryProperties)
    {
        if (!context.BindingContext.BindingData.TryGetValue("EnqueuedTimeUtc", out var messageEnqueuedTimeRaw)
            || messageEnqueuedTimeRaw is not string enqueuedTimeStr)
            return;

        // The host serializes DateTime trigger metadata as JSON via Newtonsoft.Json, producing a
        // raw JSON string literal including surrounding quotes (e.g. "\"2024-01-15T10:30:00Z\"").
        // The isolated worker returns typedData.Json verbatim, so the quotes must be stripped
        // before parsing. Trim is a no-op when the host uses TypedData.String instead.
        var normalizedDateStr = enqueuedTimeStr.Trim('"');
        if (!DateTimeOffset.TryParse(normalizedDateStr, null, DateTimeStyles.RoundtripKind, out var enqueuedTime))
            return;

        var pickupDelayMs = (pickupTime - enqueuedTime).TotalMilliseconds;
        telemetryClient.TrackMetric($"{functionName} MessagePickupDelayMs", pickupDelayMs, telemetryProperties);
    }
}
