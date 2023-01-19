// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace Microsoft.CFS.Approvals.Model.Flighting;

public class UserPreference: BaseTableEntity
{
    public int Id { get; set; }
    public string Alias { get; set; }
    public int RingPreference { get; set; }
}
