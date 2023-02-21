// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace Microsoft.CFS.Approvals.Model;

using System;

/// <summary>
/// Store the Real-time Tenant Information for any ongoing issue on Tenant side
/// </summary>

public class ApprovalTenantInfoRealTime : BaseTableEntity
{
    // DocTypeId is the PartitionKey        
    public string DocTypeId
    {
        get
        {
            return PartitionKey;
        }
    }

    // Tenant Downtimestatus (true/false) is the RowKey
    public Boolean IsTenantServicesDown
    {
        get
        {
            bool result;
            Boolean.TryParse(RowKey, out result);
            return result;
        }
    }

    public Int32 TenantId { get; set; }

    public string AppName { get; set; }

    public string CurrentTenantServiceInformation { get; set; }

    public string LastDownTimeInformation { get; set; }
}