// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace Microsoft.CFS.Approvals.Model;

using System;
public class ApproverChainEntity
{
    public string Alias { get; set; }
    public string Name { get; set; }
    public DateTimeOffset? ActionDate { get; set; }
    public string Action { get; set; }
    public string Type { get; set; }
    public string Justification { get; set; }
    public string Notes { get; set; }
    public bool _future { get; set; }
    public string DelegateUser { get; set; }
}
