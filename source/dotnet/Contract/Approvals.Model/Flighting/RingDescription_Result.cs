// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace Microsoft.CFS.Approvals.Model.Flighting
{
    public partial class RingDescription_Result
    {
        public int RingLevel { get; set; }
        public string RingDescription { get; set; }
        public int IsSelected { get; set; }
    }
}
