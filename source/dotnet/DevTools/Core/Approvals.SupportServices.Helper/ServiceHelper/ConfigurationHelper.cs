// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace Microsoft.CFS.Approvals.SupportServices.Helper.ServiceHelper
{
    using System.Collections.Generic;
    using Microsoft.CFS.Approvals.DevTools.Model.Models;

    /// <summary>
    /// The Configuration Helper class
    /// </summary>
    public class ConfigurationHelper
    {
        public readonly Dictionary<string, AppSettings> appSettings;

        /// <summary>
        /// Constructor of ConfigurationHelper
        /// </summary>
        /// <param name="keyValuePairs"></param>
        public ConfigurationHelper(Dictionary<string, AppSettings> keyValuePairs)
        {
            appSettings = keyValuePairs;
        }
    }
}
