// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace Microsoft.CFS.Approvals.Model
{
    using System.Collections.Generic;

    /// <summary>
    /// Container for holding all downtime notification messages which are grouped by
    /// banner type
    /// </summary>
    public class NotificationGroup
    {
        public int DisplaySequence { get; set; }

        public string Severity { get; set; }

        public List<NotificationMessage> Items { get; set; }
    }
}