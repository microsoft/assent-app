// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.


namespace Microsoft.CFS.Approvals.SyntheticTransaction.Common.Models;
using System.ComponentModel;

public enum ConfigurationKeyEnum
{
    BlobContainerForSchema,
    BlobContainerForDataUpload,
    BlobForSchema,
    PrefixForBlobName,
    EnvironmentList
}
public enum DocumentStatus
{
    Pending,
    Approved
}
public enum PayLoadOperation
{
    Create = 1,
    Update = 2,
    Delete = 3
}

public enum TenantActionMessage
{
    [Description("Action is successful from test tenant !!!")]
    MsgActionSuccess,
    [Description("Failed to process the request due to internal system issue")]
    MsgActionFailure
}
public enum TestPullTenant
{
    TestMSTime = 1030
}