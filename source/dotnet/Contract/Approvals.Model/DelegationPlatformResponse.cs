// Copyright (c) Microsoft Corporation. 
// Licensed under the MIT License.

namespace Microsoft.CFS.Approvals.Model;

using Microsoft.Graph.Models;

public class DelegationPlatformResponse
{
	public string AppId { get; set; } 
	public string AppName { get; set; } 
	public User Delegator { get; set; }
	public bool IsDelegationPlatform { get; set; }
}
