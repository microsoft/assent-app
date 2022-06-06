using System;
using Microsoft.CFS.Approvals.Common.DL.Interface;
using Microsoft.CFS.Approvals.Contracts;
using Microsoft.CFS.Approvals.Core.BL.Interface;
using Microsoft.Extensions.Configuration;

namespace Microsoft.CFS.Approvals.Core.BL.Factory
{
    public class ARConverterFactory : IARConverterFactory
    {
        /// <summary>
        /// The performance logger
        /// </summary>
        private readonly IPerformanceLogger _performanceLogger = null;

        //THe Configuration
        private readonly IConfiguration _config = null;

        /// <summary>
        /// ARConverterFactory Constructor
        /// </summary>
        /// <param name="performanceLogger"></param>
        /// <param name="config"></param>
        public ARConverterFactory(IPerformanceLogger performanceLogger, IConfiguration config)
        {
            _performanceLogger = performanceLogger;
            _config = config;
        }

        public IARConverter GetARConverter()
        {
            return (IARConverter)Activator.CreateInstance(Type.GetType(_config[ConfigurationKey.ARConverterClass.ToString()]), _performanceLogger, _config);
        }
    }
}
