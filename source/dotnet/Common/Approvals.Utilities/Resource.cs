// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace Microsoft.CFS.Approvals.Utilities
{
    /// <summary>
    /// The Resource class
    /// </summary>
    internal static class Resource
    {
        /// <summary>
        ///     The argument is empty error
        /// </summary>
        public const string ArgumentIsEmptyError = "Argument is empty.";

        /// <summary>
        ///     The argument not greater
        /// </summary>
        public const string ArgumentNotGreater = "The size of '{0}' should be greater to '{1}'.";

        /// <summary>
        ///     The argument not greater or equal to
        /// </summary>
        public const string ArgumentNotGreaterOrEqualTo = "The size of '{0}' should be greater or equal to '{1}'.";

        /// <summary>
        ///     The argument not lower
        /// </summary>
        public const string ArgumentNotLower = "The size of '{0}' should be lower to '{1}'.";

        /// <summary>
        ///     The argument not lower or equal to
        /// </summary>
        public const string ArgumentNotLowerOrEqualTo = "The size of '{0}' should be lower or equal to '{1}'.";

        /// <summary>
        ///     The collection read only
        /// </summary>
        public const string CollectionReadOnly = "Collection '{0}' not editable, it's read-only.";

        /// <summary>
        ///     The invalid date time format error
        /// </summary>
        public const string InvalidDateTimeFormatError = "The date time format is invalid.";

        /// <summary>
        ///     The time span invalid character
        /// </summary>
        public const string TimeSpanInvalidChar = "Timestamp contains invalid characters.";

        /// <summary>
        ///     The time span out of range error
        /// </summary>
        public const string TimeSpanOutOfRangeError = "The valid range for '{0}' is from 0 to 24.20:31:23.647";

        /// <summary>
        ///     The type conversion error
        /// </summary>
        public const string TypeConversionError = "Error on converting value {0} to type {1}.";
    }
}