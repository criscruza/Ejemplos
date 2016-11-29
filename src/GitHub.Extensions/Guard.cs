﻿using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.Linq;

namespace GitHub.Extensions
{
    [NullGuard.NullGuard(NullGuard.ValidationFlags.None)]
    public static class Guard
    {
        public static void ArgumentNotNull(object value, string name)
        {
            if (value != null) return;
            string message = String.Format(CultureInfo.InvariantCulture, "Failed Null Check on '{0}'", name);
#if DEBUG
            if (!InUnitTestRunner())
                Debug.Fail(message);
            else
                throw new ArgumentNullException(name, message);
#else
            throw new ArgumentNullException(name, message);
#endif
        }

        public static void ArgumentNonNegative(int value, string name)
        {
            if (value > -1) return;

            var message = String.Format(CultureInfo.InvariantCulture, "The value for '{0}' must be non-negative", name);
#if DEBUG
            if (!InUnitTestRunner())
                Debug.Fail(message);
            else
                throw new ArgumentException(message, name);
#else
            throw new ArgumentException(message, name);
#endif
        }

        /// <summary>
        /// Checks a string argument to ensure it isn't null or empty.
        /// </summary>
        /// <param name = "value">The argument value to check.</param>
        /// <param name = "name">The name of the argument.</param>
        public static void ArgumentNotEmptyString(string value, string name)
        {
            if (value?.Length > 0) return;
            string message = String.Format(CultureInfo.InvariantCulture, "The value for '{0}' must not be empty", name);
#if DEBUG
            if (!InUnitTestRunner())
                Debug.Fail(message);
            else
                throw new ArgumentException(message, name);
#else
            throw new ArgumentException(message, name);
#endif
        }

        public static void ArgumentInRange(int value, int minValue, string name)
        {
            if (value >= minValue) return;
            string message = String.Format(CultureInfo.InvariantCulture,
                "The value '{0}' for '{1}' must be greater than or equal to '{2}'",
                value,
                name,
                minValue);
#if DEBUG
            if (!InUnitTestRunner())
                Debug.Fail(message);
            else
                throw new ArgumentOutOfRangeException(name, message);
#else
            throw new ArgumentOutOfRangeException(name, message);
#endif
        }

        public static void ArgumentInRange(int value, int minValue, int maxValue, string name)
        {
            if (value >= minValue && value <= maxValue) return;
            string message = String.Format(CultureInfo.InvariantCulture,
                "The value '{0}' for '{1}' must be greater than or equal to '{2}' and less than or equal to '{3}'",
                value,
                name,
                minValue,
                maxValue);
#if DEBUG
            if (!InUnitTestRunner())
                Debug.Fail(message);
            else
                throw new ArgumentOutOfRangeException(name, message);
#else
            throw new ArgumentOutOfRangeException(name, message);
#endif
        }

        // Borrowed from Splat.
        public static bool InUnitTestRunner()
        {
            return Splat.ModeDetector.InUnitTestRunner();
        }
    }
}
