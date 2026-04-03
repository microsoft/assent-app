// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace Microsoft.CFS.Approvals.Model;

using Newtonsoft.Json.Linq;

public class QuickTourFeatureWithStatus
{
    public int Id { get; set; }
    public string Name { get; set; }
    public bool IsViewed { get; set; }
    public bool IsEnabled { get; set; }
    public JArray Slides { get; set; }
    public string Summary { get; set; }
    public string SummaryImage { get; set; }
}
