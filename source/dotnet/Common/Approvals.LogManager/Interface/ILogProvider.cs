// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace Microsoft.CFS.Approvals.LogManager.Provider.Interface
{
    using System;
    using System.Collections.Generic;
    using Microsoft.CFS.Approvals.LogManager.Model;

    public interface ILogProvider
    {
        /// <summary>
        /// Log information
        /// </summary>
        /// <param name="trackingEvent"></param>
        /// <param name="dataDictionary"></param>
        /// <param name="dataDictionaryStringObj"></param>
        void LogInformation(TrackingEvent trackingEvent, Dictionary<LogDataKey, object> dataDictionary = null, Dictionary<string, object> dataDictionaryStringObj = null);

        /// <summary>
        /// Log information
        /// </summary>
        /// <param name="trackingEvent"></param>
        /// <param name="dataDictionary"></param>
        /// <param name="dataDictionaryStringObj"></param>
        void LogInformation(int trackingEvent, Dictionary<LogDataKey, object> dataDictionary = null, Dictionary<string, object> dataDictionaryStringObj = null);

        /// <summary>
        /// Log warning
        /// </summary>
        /// <param name="trackingEvent"></param>
        /// <param name="dataDictionary"></param>
        /// <param name="exception"></param>
        /// <param name="dataDictionaryStringObj"></param>
        void LogWarning(TrackingEvent trackingEvent, Dictionary<LogDataKey, object> dataDictionary = null, Exception exception = null, Dictionary<string, object> dataDictionaryStringObj = null);

        /// <summary>
        /// Log warning
        /// </summary>
        /// <param name="trackingEvent"></param>
        /// <param name="dataDictionary"></param>
        /// <param name="exception"></param>
        /// <param name="dataDictionaryStringObj"></param>
        void LogWarning(int trackingEvent, Dictionary<LogDataKey, object> dataDictionary = null, Exception exception = null, Dictionary<string, object> dataDictionaryStringObj = null);

        /// <summary>
        /// Log error
        /// </summary>
        /// <param name="trackingEvent"></param>
        /// <param name="exception"></param>
        /// <param name="dataDictionary"></param>
        /// <param name="dataDictionaryStringObj"></param>
        void LogError(TrackingEvent trackingEvent, Exception exception = null, Dictionary<LogDataKey, object> dataDictionary = null, Dictionary<string, object> dataDictionaryStringObj = null);

        /// <summary>
        /// Log error
        /// </summary>
        /// <param name="trackingEventID"></param>
        /// <param name="exception"></param>
        /// <param name="dataDictionary"></param>
        /// <param name="dataDictionaryStringObj"></param>
        void LogError(int trackingEventID, Exception exception = null, Dictionary<LogDataKey, object> dataDictionary = null, Dictionary<string, object> dataDictionaryStringObj = null);
    }
}