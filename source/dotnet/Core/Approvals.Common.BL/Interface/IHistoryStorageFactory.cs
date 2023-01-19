// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace Microsoft.CFS.Approvals.Common.DL.Interface;

using Microsoft.CFS.Approvals.Model;
public interface IHistoryStorageFactory
{
    IHistoryStorageProvider GetStorageProvider(ApprovalTenantInfo tenantInfo);

    IHistoryStorageProvider GetTableStorageProvider();
}
