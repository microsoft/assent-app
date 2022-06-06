// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace Microsoft.CFS.Approvals.LogManager
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using Microsoft.ApplicationInsights;
    using Microsoft.CFS.Approvals.Contracts;
    using Microsoft.CFS.Approvals.Extensions;
    using Microsoft.CFS.Approvals.LogManager.Model;
    using Microsoft.Extensions.Configuration;
    using NLog;
    using NLog.Targets;

    /// <summary>
    /// The Applicaton Insights Target class
    /// </summary>
    public class ApplicationInsightsTarget : Target
    {
        /// <summary>
        /// The configuration.
        /// </summary>
        /// <value>
        /// The configuration.
        /// </value>
        private readonly IConfiguration _config;

        /// <summary>
        /// The component name
        /// </summary>
        private string _componentName = string.Empty;

        /// <summary>
        /// Environment Name
        /// </summary>
        private string _environmentName = string.Empty;

        /// <summary>
        /// Service Offering
        /// </summary>
        private string _serviceOffering = string.Empty;

        /// <summary>
        /// Service Line
        /// </summary>
        private string _serviceLine = string.Empty;

        /// <summary>
        /// Service
        /// </summary>
        private string _service = string.Empty;

        /// <summary>
        /// Component Id
        /// </summary>
        private string _componentId = string.Empty;

        /// <summary>
        /// The data dictionary
        /// </summary>
        private Dictionary<string, object> _dataDictionary = new Dictionary<string, object>();

        /// <summary>
        /// Initializes a new instance of the <see cref="ApplicationInsightsTarget"/> class.
        /// </summary>
        /// <param name="config">The configuration.</param>
        public ApplicationInsightsTarget(IConfiguration config)
        {
            _config = config;
            Name = "applicationInsights";
        }

        /// <summary>
        /// Writes logging event to the log target.
        /// classes.
        /// </summary>
        /// <param name="logEvent">Logging event to be written out.</param>
        protected override void Write(LogEventInfo logEvent)
        {
            var logData = AILogDataMapper(logEvent);
            if (logData.CustomProperties != null)
            {
                logData.CustomProperties["_OperationDateTime"] = DateTime.UtcNow.ToString("yyyy-MM-ddThh:mm:ss.fff");
            }
            switch (logEvent.Level.Name)
            {
                case "Error":
                case "Warn":
                    new TelemetryClient().TrackException(logEvent.Exception, logData.CustomProperties, logData.CustomMetrics);
                    break;

                default:
                    new TelemetryClient().TrackEvent(logData.EventName, logData.CustomProperties, logData.CustomMetrics);
                    break;
            }
        }

        /// <summary>
        /// Initializes the target. Can be used by inheriting classes
        /// to initialize logging.
        /// </summary>
        protected override void InitializeTarget()
        {
            base.InitializeTarget();
            _environmentName = _config[ConfigurationKey.EnvironmentName.ToString()];
            _serviceOffering = _config[ConfigurationKey.ServiceOfferingName.ToString()];
            _serviceLine = _config[ConfigurationKey.ServiceLineName.ToString()];
            _service = _config[ConfigurationKey.ServiceName.ToString()];
            _componentName = _config?[ConfigurationKey.ComponentName.ToString()];
            _componentId = _config[ConfigurationKey.ServiceComponentId.ToString()];
        }

        #region AI Helpers

        /// <summary>
        /// Mapper method which maps the properties of the MSIT extension from the custom dictionary
        /// </summary>
        /// <param name="logEvent"></param>
        /// <returns></returns>
        private AITelemetryData AILogDataMapper(LogEventInfo logEvent)
        {
            string dataJSON = logEvent.Message;

            if (!string.IsNullOrWhiteSpace(dataJSON))
                _dataDictionary = dataJSON.FromJson<Dictionary<string, object>>();

            if (!_dataDictionary.ContainsKey(LogDataKey.UserRoleName.ToString()))
            {
                _dataDictionary[LogDataKey.UserRoleName.ToString()] = Environment.UserName;
            }

            if (!_dataDictionary.ContainsKey(LogDataKey.ReceivedTcv.ToString()) && _dataDictionary.ContainsKey(LogDataKey.Tcv.ToString()))
            {
                _dataDictionary.Add(LogDataKey.ReceivedTcv.ToString(), _dataDictionary[LogDataKey.Tcv.ToString()]);
            }

            #region adding AI EnvironmentInitializer properties

            if (!string.IsNullOrEmpty(_environmentName))
            {
                _dataDictionary.Add(LogDataKey.EnvironmentName.ToString(), _environmentName);
            }

            if (!string.IsNullOrEmpty(_serviceOffering))
            {
                _dataDictionary.Add(LogDataKey.ServiceOffering.ToString(), _serviceOffering);
            }

            if (!string.IsNullOrEmpty(_serviceLine))
            {
                _dataDictionary.Add(LogDataKey.ServiceLine.ToString(), _serviceLine);
            }

            if (!string.IsNullOrEmpty(_service))
            {
                _dataDictionary.Add(LogDataKey.Service.ToString(), _service);
            }

            if (!string.IsNullOrEmpty(_componentId))
            {
                _dataDictionary.Add(LogDataKey.ComponentId.ToString(), _componentId);
            }

            if (!string.IsNullOrEmpty(_componentName))
            {
                _dataDictionary.Modify(LogDataKey.ComponentName.ToString(), _componentName);
            }

            #endregion adding AI EnvironmentInitializer properties

            if (!_dataDictionary.ContainsKey(LogDataKey.EventType.ToString()))
            {
                _dataDictionary.Add(LogDataKey.EventType.ToString(), Constants.BusinessProcessEvent);
            }
            AITelemetryData telemetryData = new AITelemetryData();

            switch (Convert.ToString(_dataDictionary[LogDataKey.EventType.ToString()]))
            {
                case Constants.FeatureUsageEvent:
                    _dataDictionary.Remove(LogDataKey.EventType.ToString());
                    telemetryData = CreateFeatureUsageEventData(_dataDictionary);
                    telemetryData.EventType = Constants.FeatureUsageEvent;
                    break;

                case Constants.BusinessProcessEvent:
                    _dataDictionary.Remove(LogDataKey.EventType.ToString());
                    telemetryData = CreateBusinessProcessEventData(_dataDictionary);
                    telemetryData.EventType = Constants.BusinessProcessEvent;
                    break;
            }

            return telemetryData;
        }

        /// <summary>
        /// Create the FeatureUsageEvent data
        /// </summary>
        /// <param name="logdata"></param>
        /// <returns></returns>
        private AITelemetryData CreateFeatureUsageEventData(Dictionary<string, object> logdata)
        {
            AITelemetryData telemetryData = new AITelemetryData
            {
                EventName = string.Format("{0}-{1}", _componentName, logdata.ContainsKey(LogDataKey.EventName.ToString()) ? logdata[LogDataKey.EventName.ToString()].ToString() : "FeatureUsageEvent")
            };

            if (logdata.ContainsKey(LogDataKey.Duration.ToString()) && double.TryParse(Convert.ToString(logdata[LogDataKey.Duration.ToString()]), out double duration))
            {
                if (duration > 0.0)
                {
                    telemetryData.CustomMetrics = new Dictionary<string, double>
                    {
                        { "Duration", duration }
                    };
                }
            }

            var complexObjectKeys = logdata.Where(k => k.Value != null && k.Value.GetType().IsSerializable == false).Select(k => k.Key).ToList();
            foreach (var complexObjectKey in complexObjectKeys)
            {
                logdata[complexObjectKey] = (logdata[complexObjectKey]).ToJson();
            }
            telemetryData.CustomProperties = logdata.ToDictionary(k => k.Key.ToString(), k => k.Value == null ? "" : k.Value.ToString());
            return telemetryData;
        }

        /// <summary>
        /// Create the BusinessProcessEvent data
        /// </summary>
        /// <param name="logdata"></param>
        /// <returns></returns>
        private AITelemetryData CreateBusinessProcessEventData(Dictionary<string, object> logdata)
        {
            AITelemetryData telemetryData = new AITelemetryData
            {
                EventName = logdata.ContainsKey(LogDataKey.EventName.ToString()) ? logdata[LogDataKey.EventName.ToString()].ToString() : (logdata.ContainsKey(LogDataKey.BusinessProcessName.ToString()) ? logdata[LogDataKey.BusinessProcessName.ToString()].ToString() : "BusinessProcessEvent")
            };

            if (logdata.ContainsKey(LogDataKey.StartDateTime.ToString()) && logdata.ContainsKey(LogDataKey.EndDateTime.ToString()))
            {
                var startDateTime = Convert.ToDateTime(logdata[LogDataKey.StartDateTime.ToString()]);
                var endDateTime = Convert.ToDateTime(logdata[LogDataKey.EndDateTime.ToString()]);
                double duration = (endDateTime - startDateTime).TotalSeconds;
                if (duration >= 0.0)
                {
                    telemetryData.CustomMetrics = new Dictionary<string, double>
                    {
                        { "Duration", duration }
                    };
                }
            }
            var complexObjectKeys = logdata.Where(k => k.Value != null && k.Value.GetType().IsSerializable == false).Select(k => k.Key).ToList();
            foreach (var complexObjectKey in complexObjectKeys)
            {
                logdata[complexObjectKey] = (logdata[complexObjectKey]).ToJson();
            }
            telemetryData.CustomProperties = logdata.ToDictionary(k => k.Key.ToString(), k => k.Value == null ? "" : k.Value.ToString());
            return telemetryData;
        }

        #endregion AI Helpers
    }
}