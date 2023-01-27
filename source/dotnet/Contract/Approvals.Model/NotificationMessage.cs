// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace Microsoft.CFS.Approvals.Model;

public class NotificationMessage
{
    public string Title { get; set; }
    public string Message { get; set; }
    public int TenantId { get; set; }
    public string ID { get; set; }
    public bool IsRead { get; set; }
}
