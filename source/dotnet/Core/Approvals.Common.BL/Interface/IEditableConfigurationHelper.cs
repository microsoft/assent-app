// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace Microsoft.CFS.Approvals.Common.BL.Interface;

using System.Collections.Generic;
using Microsoft.CFS.Approvals.Model;

public interface IEditableConfigurationHelper
{
    /// <summary>
    /// Get editable configuration by tenant
    /// </summary>
    /// <param name="tenantID"></param>
    /// <returns></returns>
    List<EditableConfigurationEntity> GetEditableConfigurationByTenant(int tenantID);
}
