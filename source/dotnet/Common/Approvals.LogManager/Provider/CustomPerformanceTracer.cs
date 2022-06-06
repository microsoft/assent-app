// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace Microsoft.CFS.Approvals.Common.BL
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;

    /// <summary>
    /// The Customer Performance Tracer class
    /// </summary>
    public class CustomPerformanceTracer : IDisposable
    {
        private readonly Stopwatch _stopwatch = new Stopwatch();
        private readonly string _logName = string.Empty;
        private readonly string _category = string.Empty;
        private readonly string _action = string.Empty;
        private readonly IDictionary<string, string> _arguments = new Dictionary<string, string>();
        private readonly bool _enable = true;
        private readonly Action<TimeSpan> _callback;
        private readonly CustomPerformanceTracer _tracer;

        /// <summary>
        /// CustomPerformanceTracer constructor.
        /// </summary>
        /// <param name="logName"></param>
        /// <param name="category"></param>
        /// <param name="action"></param>
        /// <param name="arguments"></param>
        /// <param name="enable"></param>
        public CustomPerformanceTracer(string logName, string category, string action, IDictionary<string, string> arguments, bool enable = true)
        {
            _stopwatch.Start();
            _logName = logName;
            _category = category;
            _action = action;
            _arguments = arguments;
            _enable = enable;
        }

        /// <summary>
        /// CustomPerformanceTracer constructor.
        /// </summary>
        /// <param name="callback"></param>
        /// <param name="logName"></param>
        /// <param name="category"></param>
        /// <param name="action"></param>
        /// <param name="arguments"></param>
        /// <param name="enable"></param>
        public CustomPerformanceTracer(Action<TimeSpan> callback, string logName, string category, string action, IDictionary<string, string> arguments, bool enable = true)
            : this(logName, category, action, arguments, enable)
        {
            _callback = callback;
            _tracer = new CustomPerformanceTracer(logName, category, action, arguments, enable);
        }

        /// <summary>
        /// Dispose implmentation.
        /// </summary>
        public void Dispose()
        {
            _stopwatch.Stop();
            if (_tracer != null)
                _tracer.Dispose();
            _callback?.Invoke(Result);
        }

        /// <summary>
        /// Time elapsed result.
        /// </summary>
        public TimeSpan Result
        {
            get { return _stopwatch.Elapsed; }
        }
    }
}