// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace Microsoft.CFS.Approvals.Common.BL.Helper
{
    using System.Collections.Generic;
    using System.Globalization;
    using System.Linq;
    using Microsoft.CFS.Approvals.Common.BL.Interface;
    using Microsoft.CFS.Approvals.Contracts;
    using Microsoft.CFS.Approvals.Data.Azure.Storage.Interface;
    using Microsoft.CFS.Approvals.Model;
    using Microsoft.Extensions.Configuration;

    /// <summary>
    /// The Editable Configuration Helper class
    /// </summary>
    public class EditableConfigurationHelper : IEditableConfigurationHelper
    {
        /// <summary>
        /// The configuration
        /// </summary>
        private readonly IConfiguration _config;

        /// <summary>
        /// The table helper
        /// </summary>
        private readonly ITableHelper _tableHelper;

        /// <summary>
        /// Constructor of EditableConfigurationHelper
        /// </summary>
        /// <param name="config"></param>
        /// <param name="tableHelper"></param>
        public EditableConfigurationHelper(IConfiguration config, ITableHelper tableHelper)
        {
            _config = config;
            _tableHelper = tableHelper;
        }

        /// <summary>
        /// Get editable configuration by tenant
        /// </summary>
        /// <param name="tenantID"></param>
        /// <returns></returns>
        public List<EditableConfigurationEntity> GetEditableConfigurationByTenant(int tenantID)
        {
            return _tableHelper.GetTableEntityListByPartitionKey<EditableConfigurationEntity>(_config[ConfigurationKey.EditableConfigurationTableName.ToString()], tenantID.ToString(CultureInfo.InvariantCulture)).ToList();
        }
    }
}