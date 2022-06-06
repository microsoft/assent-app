// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace Microsoft.CFS.Approvals.Utilities.Helpers
{
    using System;
    using System.Collections.Generic;
    using System.Globalization;
    using System.IO;
    using System.Linq;

    /// <summary>
    ///  The argument guard.
    /// </summary>
    public static class ArgumentGuard
    {
        #region Static Fields

        /// <summary>
        ///     The invalid file name chars
        /// </summary>
        private static readonly char[] InvalidFileNameChars = Path.GetInvalidFileNameChars();

        #endregion Static Fields

        #region Public Methods and Operators

        /// <summary>
        /// Throws an exception if the argumentValue is less than lowerValue.
        /// </summary>
        /// <typeparam name="T">A type that implements <see cref="T:IComparable" />.</typeparam>
        /// <param name="lowerValue">The lower value accepted as valid.</param>
        /// <param name="argumentValue">The argument value to test.</param>
        /// <param name="argumentName">Name of the argument.</param>
        /// <param name="errorMessage">The error message.</param>
        /// <exception cref="ArgumentOutOfRangeException">Exception of type Argument Out of Range thrown</exception>
        /// <exception cref="T:ArgumentOutOfRangeException">Validation error.</exception>
        public static void GreaterThan<T>(T lowerValue, T argumentValue, string argumentName, string errorMessage)
            where T : struct, IComparable
        {
            NotNullOrEmpty(argumentName, nameof(argumentName));
            NotNullOrEmpty(errorMessage, nameof(errorMessage));

            if (argumentValue.CompareTo(lowerValue) > 0)
            {
                return;
            }

            throw new ArgumentOutOfRangeException(argumentName, argumentValue, errorMessage);
        }

        /// <summary>
        /// Throws an exception if the argumentValue is less than lowerValue.
        /// </summary>
        /// <typeparam name="T">A type that implements <see cref="T:IComparable" />.</typeparam>
        /// <param name="lowerValue">The lower value accepted as valid.</param>
        /// <param name="argumentValue">The argument value to test.</param>
        /// <param name="argumentName">Name of the argument.</param>
        /// <exception cref="ArgumentOutOfRangeException">Exception of type Argument Out of Range thrown</exception>
        /// <exception cref="T:ArgumentOutOfRangeException">Validation error.</exception>
        public static void GreaterThan<T>(T lowerValue, T argumentValue, string argumentName)
            where T : struct, IComparable
        {
            NotNullOrEmpty(argumentName, nameof(argumentName));

            if (argumentValue.CompareTo(lowerValue) > 0)
            {
                return;
            }

            throw new ArgumentOutOfRangeException(argumentName, argumentValue, string.Format(CultureInfo.InvariantCulture, Resource.ArgumentNotGreater, argumentName, lowerValue));
        }

        /// <summary>
        /// Throws an exception if the argumentValue is less than lowerValue.
        /// </summary>
        /// <typeparam name="T">A type that implements <see cref="T:IComparable" />.</typeparam>
        /// <param name="lowerValue">The lower value accepted as valid.</param>
        /// <param name="argumentValue">The argument value to test.</param>
        /// <param name="argumentName">Name of the argument.</param>
        /// <param name="condition">The condition.</param>
        /// <param name="errorMessage">The error message.</param>
        /// <exception cref="ArgumentOutOfRangeException">Exception of type Argument Out of Range thrown</exception>
        /// <exception cref="T:ArgumentOutOfRangeException">Validation error.</exception>
        public static void GreaterThanIf<T>(T lowerValue, T argumentValue, string argumentName, Func<bool> condition, string errorMessage)
            where T : struct, IComparable
        {
            NotNull(condition, nameof(condition));
            NotNullOrEmpty(argumentName, nameof(argumentName));
            NotNullOrEmpty(errorMessage, nameof(errorMessage));

            if (condition())
            {
                GreaterThan<T>(lowerValue, argumentValue, argumentName, errorMessage);
            }
        }

        /// <summary>
        /// Throws an exception if the argumentValue is less than lowerValue.
        /// </summary>
        /// <typeparam name="T">A type that implements <see cref="T:IComparable" />.</typeparam>
        /// <param name="lowerValue">The lower value accepted as valid.</param>
        /// <param name="argumentValue">The argument value to test.</param>
        /// <param name="argumentName">Name of the argument.</param>
        /// <param name="errorMessage">The error message.</param>
        /// <exception cref="ArgumentOutOfRangeException">Exception of type Argument Out of Range thrown</exception>
        /// <exception cref="T:ArgumentOutOfRangeException">Validation error.</exception>
        public static void GreaterThanOrEqual<T>(T lowerValue, T argumentValue, string argumentName, string errorMessage)
            where T : struct, IComparable
        {
            NotNullOrEmpty(argumentName, nameof(argumentName));
            NotNullOrEmpty(errorMessage, nameof(errorMessage));

            if (argumentValue.CompareTo(lowerValue) >= 0)
            {
                return;
            }

            throw new ArgumentOutOfRangeException(
                argumentName,
                argumentValue,
                errorMessage);
        }

        /// <summary>
        /// Throws an exception if the argumentValue is less than lowerValue.
        /// </summary>
        /// <typeparam name="T">A type that implements <see cref="T:IComparable" />.</typeparam>
        /// <param name="lowerValue">The lower value accepted as valid.</param>
        /// <param name="argumentValue">The argument value to test.</param>
        /// <param name="argumentName">Name of the argument.</param>
        /// <exception cref="ArgumentOutOfRangeException">Exception of type Argument Out of Range thrown</exception>
        /// <exception cref="T:ArgumentOutOfRangeException">Validation error.</exception>
        public static void GreaterThanOrEqual<T>(T lowerValue, T argumentValue, string argumentName)
            where T : struct, IComparable
        {
            NotNullOrEmpty(argumentName, nameof(argumentName));

            if (argumentValue.CompareTo(lowerValue) >= 0)
            {
                return;
            }

            throw new ArgumentOutOfRangeException(
                argumentName,
                argumentValue,
                string.Format(CultureInfo.InvariantCulture, Resource.ArgumentNotGreaterOrEqualTo, argumentName, lowerValue));
        }

        /// <summary>
        /// Throws an exception if the argumentValue is less than lowerValue.
        /// </summary>
        /// <typeparam name="T">A type that implements <see cref="T:IComparable" />.</typeparam>
        /// <param name="lowerValue">The lower value accepted as valid.</param>
        /// <param name="argumentValue">The argument value to test.</param>
        /// <param name="argumentName">Name of the argument.</param>
        /// <param name="condition">The condition.</param>
        /// <param name="errorMessage">The error message.</param>
        /// <exception cref="ArgumentOutOfRangeException">Exception of type Argument Out of Range thrown</exception>
        /// <exception cref="T:ArgumentOutOfRangeException">Validation error.</exception>
        public static void GreaterThanOrEqualIf<T>(T lowerValue, T argumentValue, string argumentName, Func<bool> condition, string errorMessage)
            where T : struct, IComparable
        {
            NotNull(condition, nameof(condition));
            NotNullOrEmpty(errorMessage, nameof(errorMessage));

            if (condition())
            {
                GreaterThanOrEqual<T>(lowerValue, argumentValue, argumentName, errorMessage);
            }
        }

        /// <summary>
        /// Throws an exception if the tested TimeSpam argument is not a valid timeout value.
        /// </summary>
        /// <param name="argumentValue">Argument value to check.</param>
        /// <param name="argumentName">Name of argument being checked.</param>
        /// <exception cref="ArgumentOutOfRangeException">Exception of type Argument Out of Range thrown</exception>
        /// <exception cref="T:ArgumentOutOfRangeException">Thrown if the argument is not null and is not a valid timeout value.</exception>
        public static void IsValidTimeout(TimeSpan? argumentValue, string argumentName)
        {
            NotNullOrEmpty(argumentName, nameof(argumentName));

            if (!argumentValue.HasValue)
            {
                return;
            }

            var num = (long)argumentValue.Value.TotalMilliseconds;
            if (num >= -1L && num <= int.MaxValue)
            {
                return;
            }

            throw new ArgumentOutOfRangeException(string.Format(CultureInfo.InvariantCulture, Resource.TimeSpanOutOfRangeError, argumentName));
        }

        /// <summary>
        /// Throws an exception if the tested TimeSpam argument is not a valid timeout value.
        /// </summary>
        /// <param name="argumentValue">Argument value to check.</param>
        /// <param name="argumentName">Name of argument being checked.</param>
        /// <param name="errorMessage">The error message.</param>
        /// <exception cref="ArgumentOutOfRangeException">Exception of type Argument Out of Range thrown</exception>
        /// <exception cref="T:ArgumentOutOfRangeException">Thrown if the argument is not null and is not a valid timeout value.</exception>
        public static void IsValidTimeout(TimeSpan? argumentValue, string argumentName, string errorMessage)
        {
            NotNullOrEmpty(argumentName, nameof(argumentName));
            NotNullOrEmpty(errorMessage, nameof(errorMessage));

            if (!argumentValue.HasValue)
            {
                return;
            }

            var num = (long)argumentValue.Value.TotalMilliseconds;
            if (num >= -1L && num <= int.MaxValue)
            {
                return;
            }

            throw new ArgumentOutOfRangeException(errorMessage);
        }

        /// <summary>
        /// Determines whether [is valid timeout if] [the specified argument value].
        /// </summary>
        /// <param name="argumentValue">The argument value.</param>
        /// <param name="argumentName">Name of the argument.</param>
        /// <param name="condition">The condition.</param>
        /// <param name="errorMessage">The error message.</param>
        public static void IsValidTimeoutIf(TimeSpan? argumentValue, string argumentName, Func<bool> condition, string errorMessage)
        {
            NotNull(condition, nameof(condition));
            NotNullOrEmpty(errorMessage, nameof(errorMessage));

            if (condition())
            {
                IsValidTimeout(argumentValue, argumentName, errorMessage);
            }
        }

        /// <summary>
        /// Throws an exception if the argumentValue is great than higherValue.
        /// </summary>
        /// <typeparam name="T">A type that implements <see cref="T:IComparable" />.</typeparam>
        /// <param name="higherValue">The higher value accepted as valid.</param>
        /// <param name="argumentValue">The argument value to test.</param>
        /// <param name="argumentName">Name of the argument.</param>
        /// <param name="errorMessage">The error message.</param>
        /// <exception cref="ArgumentOutOfRangeException">Exception of type Argument Out of Range thrown</exception>
        /// <exception cref="T:ArgumentOutOfRangeException">Validation error.</exception>
        public static void LowerThan<T>(T higherValue, T argumentValue, string argumentName, string errorMessage)
            where T : struct, IComparable
        {
            NotNullOrEmpty(argumentName, nameof(argumentName));
            NotNullOrEmpty(errorMessage, nameof(errorMessage));

            if (argumentValue.CompareTo(higherValue) < 0)
            {
                return;
            }

            throw new ArgumentOutOfRangeException(argumentName, argumentValue, errorMessage);
        }

        /// <summary>
        /// Throws an exception if the argumentValue is great than higherValue.
        /// </summary>
        /// <typeparam name="T">A type that implements <see cref="T:IComparable" />.</typeparam>
        /// <param name="higherValue">The higher value accepted as valid.</param>
        /// <param name="argumentValue">The argument value to test.</param>
        /// <param name="argumentName">Name of the argument.</param>
        /// <exception cref="ArgumentOutOfRangeException">Exception of type Argument Out of Range thrown</exception>
        /// <exception cref="T:ArgumentOutOfRangeException">Validation error.</exception>
        public static void LowerThan<T>(T higherValue, T argumentValue, string argumentName)
            where T : struct, IComparable
        {
            NotNullOrEmpty(argumentName, nameof(argumentName));

            if (argumentValue.CompareTo(higherValue) < 0)
            {
                return;
            }

            throw new ArgumentOutOfRangeException(argumentName, argumentValue, string.Format(CultureInfo.InvariantCulture, Resource.ArgumentNotLower, argumentName, higherValue));
        }

        /// <summary>
        /// Throws an exception if the argumentValue is great than higherValue.
        /// </summary>
        /// <typeparam name="T">A type that implements <see cref="T:IComparable" />.</typeparam>
        /// <param name="higherValue">The higher value accepted as valid.</param>
        /// <param name="argumentValue">The argument value to test.</param>
        /// <param name="argumentName">Name of the argument.</param>
        /// <param name="condition">The condition.</param>
        /// <param name="errorMessage">The error message.</param>
        /// <exception cref="ArgumentOutOfRangeException">Exception of type Argument Out of Range thrown</exception>
        /// <exception cref="T:ArgumentOutOfRangeException">Validation error.</exception>
        public static void LowerThanIf<T>(T higherValue, T argumentValue, string argumentName, Func<bool> condition, string errorMessage)
            where T : struct, IComparable
        {
            NotNull(condition, nameof(condition));
            NotNullOrEmpty(errorMessage, nameof(errorMessage));

            if (condition())
            {
                LowerThan<T>(higherValue, argumentValue, argumentName, errorMessage);
            }
        }

        /// <summary>
        /// Throws an exception if the argumentValue is great than higherValue.
        /// </summary>
        /// <typeparam name="T">A type that implements <see cref="T:IComparable" />.</typeparam>
        /// <param name="higherValue">The higher value accepted as valid.</param>
        /// <param name="argumentValue">The argument value to test.</param>
        /// <param name="argumentName">Name of the argument.</param>
        /// <exception cref="ArgumentOutOfRangeException">Exception of type Argument Out of Range thrown</exception>
        /// <exception cref="T:ArgumentOutOfRangeException">Validation error.</exception>
        public static void LowerThanOrEqual<T>(T higherValue, T argumentValue, string argumentName)
            where T : struct, IComparable
        {
            NotNullOrEmpty(argumentName, nameof(argumentName));

            if (argumentValue.CompareTo(higherValue) <= 0)
            {
                return;
            }

            throw new ArgumentOutOfRangeException(argumentName, argumentValue, string.Format(CultureInfo.InvariantCulture, Resource.ArgumentNotLowerOrEqualTo, argumentName, higherValue));
        }

        /// <summary>
        /// Throws an exception if the argumentValue is great than higherValue.
        /// </summary>
        /// <typeparam name="T">A type that implements <see cref="T:IComparable" />.</typeparam>
        /// <param name="higherValue">The higher value accepted as valid.</param>
        /// <param name="argumentValue">The argument value to test.</param>
        /// <param name="argumentName">Name of the argument.</param>
        /// <param name="errorMessage">Error message.</param>
        /// <exception cref="ArgumentOutOfRangeException">Exception of type Argument Out of Range thrown</exception>
        /// <exception cref="T:ArgumentOutOfRangeException">Validation error.</exception>
        public static void LowerThanOrEqual<T>(T higherValue, T argumentValue, string argumentName, string errorMessage)
            where T : struct, IComparable
        {
            NotNullOrEmpty(argumentName, nameof(argumentName));
            NotNullOrEmpty(errorMessage, nameof(errorMessage));

            if (argumentValue.CompareTo(higherValue) <= 0)
            {
                return;
            }

            throw new ArgumentOutOfRangeException(argumentName, argumentValue, errorMessage);
        }

        /// <summary>
        /// Throws an exception if the argumentValue is great than higherValue.
        /// </summary>
        /// <typeparam name="T">A type that implements <see cref="T:IComparable" />.</typeparam>
        /// <param name="higherValue">The higher value accepted as valid.</param>
        /// <param name="argumentValue">The argument value to test.</param>
        /// <param name="argumentName">Name of the argument.</param>
        /// <param name="condition">The condition.</param>
        /// <param name="errorMessage">The error message.</param>
        /// <exception cref="ArgumentOutOfRangeException">Exception of type Argument Out of Range thrown</exception>
        /// <exception cref="T:ArgumentOutOfRangeException">Validation error.</exception>
        public static void LowerThanOrEqualIf<T>(T higherValue, T argumentValue, string argumentName, Func<bool> condition, string errorMessage)
            where T : struct, IComparable
        {
            NotNull(condition, nameof(condition));
            NotNullOrEmpty(errorMessage, nameof(errorMessage));

            if (condition())
            {
                LowerThanOrEqual<T>(higherValue, argumentValue, argumentName, errorMessage);
            }
        }

        /// <summary>
        /// Throws <see cref="T:ArgumentNullException" /> if the given argument is null.
        /// </summary>
        /// <param name="argumentValue">Argument value to test.</param>
        /// <param name="argumentName">Name of the argument being tested.</param>
        /// <exception cref="ArgumentNullException">Exception of type Argument Out of Range thrown</exception>
        /// <exception cref="T:ArgumentNullException">If tested value if null.</exception>
        public static void NotNull(object argumentValue, string argumentName)
        {
            NotNullOrEmpty(argumentName, nameof(argumentName));

            if (argumentValue == null)
            {
                throw new ArgumentNullException(argumentName);
            }
        }

        /// <summary>
        /// Throws <see cref="T:ArgumentNullException" /> if the given argument is null.
        /// </summary>
        /// <param name="argumentValue">Argument value to test.</param>
        /// <param name="argumentName">Name of the argument being tested.</param>
        /// <param name="errorMessage">The error message.</param>
        /// <exception cref="ArgumentNullException">Exception of type Argument Out of Range thrown</exception>
        /// <exception cref="T:ArgumentNullException">If tested value if null.</exception>
        public static void NotNull(object argumentValue, string argumentName, string errorMessage = null)
        {
            NotNullOrEmpty(argumentName, nameof(argumentName));

            //// NotNullOrEmpty(errorMessage, nameof(errorMessage));

            if (argumentValue == null)
            {
                throw new ArgumentNullException(argumentName, errorMessage);
            }
        }

        /// <summary>
        /// Throws <see cref="T:ArgumentNullException" /> if the given argument is null. if true.
        /// </summary>
        /// <param name="argumentValue">The argument value.</param>
        /// <param name="argumentName">Name of the argument.</param>
        /// <param name="condition">The condition expression.</param>
        /// <param name="errorMessage">The error message.</param>
        public static void NotNullIf(object argumentValue, string argumentName, Func<bool> condition, string errorMessage)
        {
            NotNull(condition, nameof(condition));
            NotNullOrEmpty(errorMessage, nameof(errorMessage));

            if (condition())
            {
                NotNull(argumentValue, argumentName, errorMessage);
            }
        }

        /// <summary>
        /// Arguments the not null and empty.
        /// </summary>
        /// <typeparam name="T">Type of items in enumerable</typeparam>
        /// <param name="enumerable">The enumerable.</param>
        /// <param name="argumentName">Name of the argument.</param>
        /// <param name="errorMessage">The error message.</param>
        /// <exception cref="ArgumentException">If enumerable is empty</exception>
        public static void NotNullAndEmpty<T>(this IEnumerable<T> enumerable, string argumentName, string errorMessage)
        {
            NotNull(enumerable, argumentName);
            NotNullOrEmpty(argumentName, nameof(argumentName));
            NotNullOrEmpty(errorMessage, nameof(errorMessage));

            if (!enumerable.Any())
            {
                throw new ArgumentException(argumentName, errorMessage);
            }
        }

        /// <summary>
        /// Arguments the not null and empty.
        /// </summary>
        /// <typeparam name="T">Type of items in enumerable</typeparam>
        /// <param name="enumerable">The enumerable.</param>
        /// <param name="argumentName">Name of the argument.</param>
        /// <exception cref="ArgumentException">If enumerable is empty</exception>
        public static void NotNullAndEmpty<T>(this IEnumerable<T> enumerable, string argumentName)
        {
            NotNull(enumerable, argumentName);
            NotNullOrEmpty(argumentName, nameof(argumentName));

            if (!enumerable.Any())
            {
                throw new ArgumentException(argumentName, Resource.ArgumentIsEmptyError);
            }
        }

        /// <summary>
        /// Arguments the not null and empty.
        /// </summary>
        /// <typeparam name="T">Type of items in enumerable</typeparam>
        /// <param name="enumerable">The enumerable.</param>
        /// <param name="argumentName">Name of the argument.</param>
        /// <param name="condition">The condition.</param>
        /// <param name="errorMessage">The error message.</param>
        /// <exception cref="ArgumentException">If enumerable is empty</exception>
        public static void NotNullAndEmptyIf<T>(this IEnumerable<T> enumerable, string argumentName, Func<bool> condition, string errorMessage)
        {
            NotNull(condition, nameof(condition));
            NotNullOrEmpty(errorMessage, nameof(errorMessage));

            if (condition())
            {
                NotNullAndEmpty<T>(enumerable, argumentName, errorMessage);
            }
        }

        /// <summary>
        /// Throws an exception if the tested string argument is null or the empty string.
        /// </summary>
        /// <param name="argumentValue">Argument value to check.</param>
        /// <param name="argumentName">Name of argument being checked.</param>
        /// <exception cref="ArgumentNullException">Exception of type Argument null thrown</exception>
        /// <exception cref="ArgumentException">Exception of type Argument Out of Range thrown</exception>
        /// <exception cref="T:ArgumentNullException">Thrown if string value is null.</exception>
        /// <exception cref="T:ArgumentException">Thrown if the string is empty</exception>
        public static void NotNullOrEmpty(string argumentValue, string argumentName)
        {
            if (argumentValue == null)
            {
                throw new ArgumentNullException(argumentName);
            }

            if (argumentValue.Length == 0)
            {
                throw new ArgumentException(Resource.ArgumentIsEmptyError, argumentName);
            }
        }

        /// <summary>
        /// Throws an exception if the tested string argument is null or the empty string.
        /// </summary>
        /// <param name="argumentValue">Argument value to check.</param>
        /// <param name="argumentName">Name of argument being checked.</param>
        /// <param name="errorMessage">The error message.</param>
        /// <exception cref="ArgumentNullException">Exception of type Argument null thrown</exception>
        /// <exception cref="ArgumentException">Exception of type Argument Out of Range thrown</exception>
        /// <exception cref="T:ArgumentNullException">Thrown if string value is null.</exception>
        /// <exception cref="T:ArgumentException">Thrown if the string is empty</exception>
        public static void NotNullOrEmpty(string argumentValue, string argumentName, string errorMessage)
        {
            if (argumentValue == null)
            {
                throw new ArgumentNullException(argumentName);
            }

            if (argumentValue.Length == 0)
            {
                throw new ArgumentException(errorMessage);
            }
        }

        /// <summary>
        /// Throws an exception if the tested string argument is null or the empty string.
        /// </summary>
        /// <param name="argumentValue">Argument value to check.</param>
        /// <param name="argumentName">Name of argument being checked.</param>
        /// <param name="condition">The condition.</param>
        /// <param name="errorMessage">The error message.</param>
        /// <exception cref="ArgumentNullException">Exception of type Argument null thrown</exception>
        /// <exception cref="ArgumentException">Exception of type Argument Out of Range thrown</exception>
        /// <exception cref="T:ArgumentNullException">Thrown if string value is null.</exception>
        /// <exception cref="T:ArgumentException">Thrown if the string is empty</exception>
        public static void NotNullOrEmptyIf(string argumentValue, string argumentName, Func<bool> condition, string errorMessage)
        {
            NotNull(condition, nameof(condition));
            NotNullOrEmpty(errorMessage, nameof(errorMessage));

            if (condition())
            {
                NotNullOrEmpty(argumentValue, argumentName, errorMessage);
            }
        }

        /// <summary>
        /// Arguments the not read only.
        /// </summary>
        /// <typeparam name="T">
        /// The passed type.
        /// </typeparam>
        /// <param name="destination">
        /// The destination.
        /// </param>
        /// <param name="argumentName">
        /// Name of the argument.
        /// </param>
        /// <exception cref="ArgumentException">
        /// Exception of type Argument Out of Range thrown
        /// </exception>
        public static void NotReadOnly<T>(ICollection<T> destination, string argumentName)
        {
            ArgumentGuard.NotNull(destination, argumentName);
            if (destination.IsReadOnly)
            {
                throw new ArgumentException(string.Format(CultureInfo.InvariantCulture, Resource.CollectionReadOnly, argumentName), argumentName);
            }
        }

        /// <summary>
        /// Validates the time stamp pattern.
        /// </summary>
        /// <param name="pattern">
        /// The time stamp pattern.
        /// </param>
        /// <param name="argumentName">
        /// Name of the argument.
        /// </param>
        /// <exception cref="ArgumentException">
        /// Throws an argument exception.
        /// </exception>
        public static void TimestampPattern(string pattern, string argumentName)
        {
            ArgumentGuard.NotNullOrEmpty(pattern, nameof(pattern));
            ArgumentGuard.NotNullOrEmpty(argumentName, nameof(argumentName));

            if (pattern.Any(ch => InvalidFileNameChars.Contains(ch)))
            {
                throw new ArgumentException(Resource.TimeSpanInvalidChar, argumentName);
            }
        }

        /// <summary>
        /// Validate the date time format.
        /// </summary>
        /// <param name="format">
        ///     The format.
        /// </param>
        /// <param name="argumentName">
        ///     Name of the argument.
        /// </param>
        /// <exception cref="ArgumentException">
        /// Throws an argument exception.
        /// </exception>
        public static void DateTimeFormat(string format, string argumentName)
        {
            ArgumentGuard.NotNullOrEmpty(format, nameof(format));
            ArgumentGuard.NotNullOrEmpty(argumentName, nameof(argumentName));

            try
            {
                DateTime.Now.ToString(format, CultureInfo.InvariantCulture);
            }
            catch (FormatException ex)
            {
                throw new ArgumentException(argumentName, Resource.InvalidDateTimeFormatError, ex);
            }
        }

        #endregion Public Methods and Operators
    }
}