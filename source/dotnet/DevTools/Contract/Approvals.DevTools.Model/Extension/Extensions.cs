// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace Microsoft.CFS.Approvals.DevTools.Model.Extension
{
    using System;
    public static class Extensions
    {
        /// <summary>
        /// Get Date Time With Utc Kind 
        /// </summary>
        /// <param name="dateTime"></param>
        /// <returns></returns>
        public static DateTime GetDateTimeWithUtcKind(this DateTime dateTime)
        {
            switch (dateTime.Kind)
            {
                case DateTimeKind.Unspecified:
                    return DateTime.SpecifyKind(dateTime, DateTimeKind.Utc);
                case DateTimeKind.Local:
                    return dateTime.ToUniversalTime();
                default:
                    return DateTime.SpecifyKind(dateTime, DateTimeKind.Utc);
            }
        }
    }
}
