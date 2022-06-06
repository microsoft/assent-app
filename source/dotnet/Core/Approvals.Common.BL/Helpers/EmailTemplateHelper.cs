// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace Microsoft.CFS.Approvals.Common.DL
{
    using System.Collections.Generic;
    using Microsoft.CFS.Approvals.Common.DL.Interface;
    using Microsoft.CFS.Approvals.Contracts;
    using Microsoft.CFS.Approvals.Data.Azure.Storage.Interface;
    using Microsoft.CFS.Approvals.Model;
    using Microsoft.Extensions.Configuration;

    /// <summary>
    /// The Email Template Helper
    /// </summary>
    public class EmailTemplateHelper : IEmailTemplateHelper
    {
        /// <summary>
        /// The table helper
        /// </summary>
        private readonly ITableHelper _storageTableHelper;

        /// <summary>
        /// The configuration
        /// </summary>
        private readonly IConfiguration _config;

        /// <summary>
        /// Constructor of EmailTemplateHelper
        /// </summary>
        /// <param name="storageTableHelper"></param>
        /// <param name="config"></param>
        public EmailTemplateHelper(
            ITableHelper storageTableHelper,
            IConfiguration config)
        {
            _storageTableHelper = storageTableHelper;
            _config = config;
        }

        /// <summary>
        /// Get email notification templates
        /// </summary>
        /// <returns></returns>
        public IEnumerable<ApprovalEmailNotificationTemplates> GetTemplates()
        {
            return _storageTableHelper.GetTableEntity<ApprovalEmailNotificationTemplates>(_config[ConfigurationKey.ApprovalEmailNotificationTemplatesAzureTableName.ToString()]);
        }

        /// <summary>
        /// Get email notification templates by partitionKey
        /// </summary>
        /// <param name="partitionKey"></param>
        /// <returns></returns>
        public IEnumerable<ApprovalEmailNotificationTemplates> GetTemplates(string partitionKey)
        {
            return _storageTableHelper.GetTableEntityListByPartitionKey<ApprovalEmailNotificationTemplates>(_config[ConfigurationKey.ApprovalEmailNotificationTemplatesAzureTableName.ToString()], partitionKey);
        }
    }
}