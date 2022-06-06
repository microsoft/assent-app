// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace Microsoft.CFS.Approvals.PayloadReceiver.BL
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics.CodeAnalysis;
    using Microsoft.CFS.Approvals.Contracts;
    using Microsoft.CFS.Approvals.PayloadReceiver.BL.Interface;
    using Microsoft.Extensions.Configuration;

    /// <summary>
    /// Approval request expression helper
    /// </summary>
    [ExcludeFromCodeCoverage]
    public class ApprovalRequestExpressionHelper : IApprovalRequestExpressionHelper
    {
        /// <summary>
        /// The configuration
        /// </summary>
        private readonly IConfiguration _config = null;

        /// <summary>
        /// Contructor of ApprovalRequestExpressionHelper
        /// </summary>
        /// <param name="config"></param>
        public ApprovalRequestExpressionHelper(IConfiguration config)
        {
            _config = config;
        }

        /// <summary>
        /// Get current approval request expression type
        /// </summary>
        /// <param name="tenantId"></param>
        /// <returns></returns>
        public Type GetCurrrentApprovalRequestExpressionType(string tenantId)
        {
            Type type = null;
            try
            {
                var keyValue = string.Empty;
                var configKey = _config[ConfigurationKey.ApprovalRequestExpressionClass.ToString()];
                if (configKey != null)
                {
                    keyValue = configKey;
                }
                else
                {
                    throw new KeyNotFoundException(ConfigurationKey.ApprovalRequestExpressionClass.ToString() + " not found!!");
                }

                // If version number is provided, find its type, else get the latest default version from configuration
                if (!string.IsNullOrEmpty(tenantId))
                {
                    // Temp Code
                    // TODO:: Replace with logic to find the type from Tenant Info Table
                    type = Activator.CreateInstance(Type.GetType(keyValue))
                            .GetType();
                }
                else
                {
                    type = Activator.CreateInstance(Type.GetType(keyValue))
                            .GetType();
                }
            }
            catch
            {
                // Catch and log and return Null for Payload Type
            }

            return type;
        }
    }
}