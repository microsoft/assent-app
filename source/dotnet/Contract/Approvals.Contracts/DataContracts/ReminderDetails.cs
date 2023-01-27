// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace Microsoft.CFS.Approvals.Contracts.DataContracts
{
    using System;
    using System.Collections.Generic;

    public class ReminderDetails
    {
        public ReminderDetails()
        { }

        public List<DateTime> ReminderDates { get; set; }
        public int Frequency { get; set; }
        public DateTime Expiration { get; set; }
        public string ReminderTemplate { get; set; }
    }
}