// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace Microsoft.CFS.Approvals.LogManager;

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

    /// <summary>
    /// Log Information
    /// </summary>
    /// <typeparam name="TEvent"></typeparam>
    /// <typeparam name="TLog"></typeparam>
    /// <param name="trackingEvent"></param>
    /// <param name="dataDictionary"></param>
    /// <param name="dataDictionaryStringObj"></param>
    public void LogInformation<TEvent, TLog>(TEvent trackingEvent, Dictionary<TLog, object> dataDictionary = null, Dictionary<string, object> dataDictionaryStringObj = null) where TEvent : struct where TLog : struct
    {
        Log(LogType.Information, 0, trackingEvent, dataDictionary, null);
    }

    /// <summary>
    /// Log Error
    /// </summary>
    /// <typeparam name="TEvent"></typeparam>
    /// <typeparam name="TLog"></typeparam>
    /// <param name="trackingEvent"></param>
    /// <param name="exception"></param>
    /// <param name="dataDictionary"></param>
    /// <param name="dataDictionaryStringObj"></param>
    public void LogError<TEvent, TLog>(TEvent trackingEvent, Exception exception = null, Dictionary<TLog, object> dataDictionary = null, Dictionary<string, object> dataDictionaryStringObj = null) where TEvent : struct where TLog : struct
    {
        Log(LogType.Error, 0, trackingEvent, dataDictionary, exception);
    }

    #endregion Implemented Methods

    #region Helpers Methods

    /// <summary>
    /// Add action name
    /// </summary>
    /// <typeparam name="TLog"></typeparam>
    /// <param name="sf"></param>
    /// <param name="dataDictionary"></param>
    /// <returns></returns>
    private Dictionary<TLog, object> AddActionName<TLog>(StackFrame sf, Dictionary<TLog, object> dataDictionary) where TLog : struct
    {
        if (dataDictionary == null)
            dataDictionary = new Dictionary<TLog, object>();
        var method = sf.GetMethod();
        if (method != null)
            dataDictionary[Enum.Parse<TLog>("ActionOrComponentUri")] = (method.DeclaringType == null ? "" : method.DeclaringType.FullName) + "." + method.Name;
        return dataDictionary;
    }

    /// <summary>
    /// Log
    /// </summary>
    /// <typeparam name="TEvent"></typeparam>
    /// <typeparam name="TLog"></typeparam>
    /// <param name="logType"></param>
    /// <param name="eventId"></param>
    /// <param name="trackingEvent"></param>
    /// <param name="dataDictionary"></param>
    /// <param name="exception"></param>
    private void Log<TEvent, TLog>(LogType logType, int eventId, TEvent trackingEvent, Dictionary<TLog, object> dataDictionary = null, Exception exception = null) where TEvent : struct where TLog : struct
    {
        try
        {
            if (dataDictionary == null) dataDictionary = new Dictionary<TLog, object>();
            dataDictionary = AddActionName(new StackFrame(2), dataDictionary);
            dataDictionary[Enum.Parse<TLog>("_TrackingEvent")] = trackingEvent.ToString();
            dataDictionary[Enum.Parse<TLog>("EventId")] = eventId.ToString();
            dataDictionary[Enum.Parse<TLog>("EventName")] = trackingEvent.ToString();

            if (!dataDictionary.ContainsKey(Enum.Parse<TLog>("LoggingDateTimeUtc")))
            {
                dataDictionary.Add(Enum.Parse<TLog>("LoggingDateTimeUtc"), DateTime.UtcNow);
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