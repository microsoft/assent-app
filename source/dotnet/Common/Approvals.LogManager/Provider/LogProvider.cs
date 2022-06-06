// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace Microsoft.CFS.Approvals.LogManager
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Linq;
    using Microsoft.ApplicationInsights;
    using Microsoft.CFS.Approvals.Contracts;
    using Microsoft.CFS.Approvals.Extensions;
    using Microsoft.CFS.Approvals.LogManager.Model;
    using Microsoft.CFS.Approvals.LogManager.Provider.Interface;
    using NLog;
    using NLog.Config;
    using NLog.Targets;

    public class LogProvider : ILogProvider
    {
        private readonly ApplicationInsightsTarget _applicationInsightsTarget;
        private static readonly Logger _logger = null;

        /// <summary>
        /// Constructor of LogProvider
        /// </summary>
        static LogProvider()
        {
            _logger = LogManager.GetCurrentClassLogger();
        }

        /// <summary>
        /// Constructor of LogProvider
        /// </summary>
        public LogProvider(ApplicationInsightsTarget applicationInsightsTarget)
        {
            _applicationInsightsTarget = applicationInsightsTarget;
            SetupListeners();
        }

        /// <summary>
        /// Setup Listeners
        /// </summary>
        private void SetupListeners()
        {
            LogManager.Configuration = new LoggingConfiguration();

            // Listener to AI
            LogManager.Configuration.AddTarget("applicationInsights", _applicationInsightsTarget);
            var applicationInsightsRule = new LoggingRule("*", LogLevel.Debug, _applicationInsightsTarget);
            LogManager.Configuration.LoggingRules.Add(applicationInsightsRule);

            LogManager.ReconfigExistingLoggers();
        }

        #region Implemented Methods

        /// <summary>
        /// Log information
        /// </summary>
        /// <param name="trackingEvent"></param>
        /// <param name="dataDictionary"></param>
        /// <param name="dataDictionaryStringObj"></param>
        public void LogInformation(TrackingEvent trackingEvent, Dictionary<LogDataKey, object> dataDictionary = null, Dictionary<string, object> dataDictionaryStringObj = null)
        {
            Log(LogType.Information, (int)trackingEvent, trackingEvent, dataDictionary, null);
        }

        /// <summary>
        /// Log warning
        /// </summary>
        /// <param name="trackingEvent"></param>
        /// <param name="dataDictionary"></param>
        /// <param name="exception"></param>
        /// <param name="dataDictionaryStringObj"></param>
        public void LogWarning(TrackingEvent trackingEvent, Dictionary<LogDataKey, object> dataDictionary = null, Exception exception = null, Dictionary<string, object> dataDictionaryStringObj = null)
        {
            Log(LogType.Warning, (int)trackingEvent, trackingEvent, dataDictionary, exception);
        }

        /// <summary>
        /// Log error
        /// </summary>
        /// <param name="trackingEvent"></param>
        /// <param name="exception"></param>
        /// <param name="dataDictionary"></param>
        /// <param name="dataDictionaryStringObj"></param>
        public void LogError(TrackingEvent trackingEvent, Exception exception = null, Dictionary<LogDataKey, object> dataDictionary = null, Dictionary<string, object> dataDictionaryStringObj = null)
        {
            Log(LogType.Error, (int)trackingEvent, trackingEvent, dataDictionary, exception);
        }

        /// <summary>
        /// Log information
        /// </summary>
        /// <param name="trackingEventID"></param>
        /// <param name="dataDictionary"></param>
        /// <param name="dataDictionaryStringObj"></param>
        public void LogInformation(int trackingEventID, Dictionary<LogDataKey, object> dataDictionary = null, Dictionary<string, object> dataDictionaryStringObj = null)
        {
            Log(LogType.Information, trackingEventID, 0, dataDictionary, null);
        }

        /// <summary>
        /// Log warning
        /// </summary>
        /// <param name="trackingEventID"></param>
        /// <param name="dataDictionary"></param>
        /// <param name="exception"></param>
        /// <param name="dataDictionaryStringObj"></param>
        public void LogWarning(int trackingEventID, Dictionary<LogDataKey, object> dataDictionary = null, Exception exception = null, Dictionary<string, object> dataDictionaryStringObj = null)
        {
            Log(LogType.Warning, trackingEventID, 0, dataDictionary, exception);
        }

        /// <summary>
        /// Log error
        /// </summary>
        /// <param name="trackingEventID"></param>
        /// <param name="exception"></param>
        /// <param name="dataDictionary"></param>
        /// <param name="dataDictionaryStringObj"></param>
        public void LogError(int trackingEventID, Exception exception = null, Dictionary<LogDataKey, object> dataDictionary = null, Dictionary<string, object> dataDictionaryStringObj = null)
        {
            Log(LogType.Warning, trackingEventID, 0, dataDictionary, exception);
        }

        #endregion Implemented Methods

        #region Helpers Methods

        /// <summary>
        /// Add action name
        /// </summary>
        /// <param name="sf"></param>
        /// <param name="dataDictionary"></param>
        /// <returns></returns>
        private Dictionary<LogDataKey, object> AddActionName(StackFrame sf, Dictionary<LogDataKey, object> dataDictionary)
        {
            if (dataDictionary == null)
                dataDictionary = new Dictionary<LogDataKey, object>();
            var method = sf.GetMethod();
            if (method != null)
                dataDictionary[LogDataKey.ActionOrComponentUri] = (method.DeclaringType == null ? "" : method.DeclaringType.FullName) + "." + method.Name;
            return dataDictionary;
        }

        /// <summary>
        /// Log
        /// </summary>
        /// <param name="logType"></param>
        /// <param name="eventId"></param>
        /// <param name="trackingEvent"></param>
        /// <param name="dataDictionary"></param>
        /// <param name="exception"></param>
        private void Log(LogType logType, int eventId, TrackingEvent trackingEvent, Dictionary<LogDataKey, object> dataDictionary = null, Exception exception = null)
        {
            try
            {
                if (dataDictionary == null) dataDictionary = new Dictionary<LogDataKey, object>();
                dataDictionary = AddActionName(new StackFrame(2), dataDictionary);
                if (trackingEvent != 0)
                {
                    dataDictionary[LogDataKey._TrackingEvent] = trackingEvent.ToString();
                    dataDictionary[LogDataKey.EventId] = eventId;
                    dataDictionary[LogDataKey.EventName] = trackingEvent.ToString();
                }

                if (!dataDictionary.ContainsKey(LogDataKey.LoggingDateTimeUtc))
                {
                    dataDictionary.Add(LogDataKey.LoggingDateTimeUtc, DateTime.UtcNow);
                }

                LogEventInfo logEventInfo = new LogEventInfo() { Exception = exception, Message = dataDictionary.ToJson(), Level = (logType == LogType.Error ? NLog.LogLevel.Error : (logType == LogType.Warning ? NLog.LogLevel.Warn : NLog.LogLevel.Info)) };

                var isTargetValid = (LogManager.Configuration.AllTargets.Count >= 2) &&
                                    (LogManager.Configuration.AllTargets.FirstOrDefault(t => t.Name != null && t.Name.Equals("applicationInsights")) != null) &&
                                    (LogManager.Configuration.AllTargets.FirstOrDefault(t => t.Name != null && t.Name.Equals("loggerAPI")) != null);
                if (!isTargetValid)
                {
                    SetupListeners();
                }

                _logger.Log(logEventInfo);
            }
            catch (Exception ex)
            {
                new TelemetryClient().TrackException(ex, dataDictionary.ToDictionary(k => k.Key.ToString(), k => k.Value?.ToString() ?? ""));
            }
        }

        #endregion Helpers Methods
    }
}