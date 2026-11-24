using System;
using System.Collections.Generic;
using System.Linq;

namespace SkyWebFramework.Utility
{
    /// <summary>
    /// Number, math, statistical, and unit conversion utilities.
    /// </summary>
    public static class EasyMath
    {
        /// <summary>
        /// Clamps a value between min and max (inclusive).
        /// </summary>
        public static T Clamp<T>(this T value, T min, T max) where T : IComparable<T>
        {
            if (value.CompareTo(min) < 0) return min;
            if (value.CompareTo(max) > 0) return max;
            return value;
        }

        /// <summary>
        /// Checks if a value is within the specified range (inclusive).
        /// </summary>
        public static bool IsInRange<T>(this T value, T min, T max) where T : IComparable<T>
            => value.CompareTo(min) >= 0 && value.CompareTo(max) <= 0;

        /// <summary>
        /// Checks if a value is between min and max (inclusive by default).
        /// </summary>
        public static bool IsBetween<T>(this T value, T min, T max, bool inclusive = true) where T : IComparable<T>
            => inclusive
                ? value.CompareTo(min) >= 0 && value.CompareTo(max) <= 0
                : value.CompareTo(min) > 0 && value.CompareTo(max) < 0;

        /// <summary>
        /// Calculates what percentage <paramref name="value"/> is of <paramref name="total"/>.
        /// </summary>
        public static double Percentage(double value, double total)
        {
            if (total == 0) return 0;
            return (value / total) * 100;
        }

        /// <summary>
        /// Computes <paramref name="percentage"/>% of <paramref name="total"/>.
        /// </summary>
        public static double PercentageOf(double percentage, double total)
            => (percentage / 100.0) * total;

        /// <summary>
        /// Increases a value by the specified percentage.
        /// </summary>
        public static double AddPercentage(double value, double percentage)
            => value + PercentageOf(percentage, value);

        /// <summary>
        /// Decreases a value by the specified percentage.
        /// </summary>
        public static double SubtractPercentage(double value, double percentage)
            => value - PercentageOf(percentage, value);

        /// <summary>
        /// Formats byte count into human-readable file size (e.g., 1048576 -> "1 MB").
        /// </summary>
        public static string FormatFileSize(long bytes)
        {
            string[] sizes = { "B", "KB", "MB", "GB", "TB", "PB", "EB" };
            if (bytes == 0) return "0 B";

            var sign = bytes < 0 ? "-" : string.Empty;
            double absBytes = bytes == long.MinValue ? (double)long.MaxValue + 1 : Math.Abs((double)bytes);
            var i = Math.Min((int)Math.Floor(Math.Log(absBytes, 1024)), sizes.Length - 1);
            var formatted = Math.Round(absBytes / Math.Pow(1024, i), 2);
            return $"{sign}{formatted} {sizes[i]}";
        }

        /// <summary>
        /// Rounds a number to the specified number of significant digits.
        /// </summary>
        public static double RoundToSignificantDigits(double value, int significantDigits)
        {
            if (value == 0) return 0;
            if (significantDigits <= 0)
                throw new ArgumentOutOfRangeException(nameof(significantDigits), "Significant digits must be positive.");

            var scale = Math.Pow(10, Math.Floor(Math.Log10(Math.Abs(value))) + 1 - significantDigits);
            return Math.Round(value / scale) * scale;
        }

        /// <summary>
        /// Rounds a double to the nearest multiple of <paramref name="step"/>.
        /// </summary>
        public static double RoundToNearest(this double value, double step)
        {
            if (step <= 0) throw new ArgumentOutOfRangeException(nameof(step), "Step must be positive.");
            return Math.Round(value / step, MidpointRounding.AwayFromZero) * step;
        }

        /// <summary>
        /// Rounds a decimal to the nearest multiple of <paramref name="step"/>.
        /// </summary>
        public static decimal RoundToNearest(this decimal value, decimal step)
        {
            if (step <= 0) throw new ArgumentOutOfRangeException(nameof(step), "Step must be positive.");
            return Math.Round(value / step, MidpointRounding.AwayFromZero) * step;
        }

        /// <summary>
        /// Rounds a number up (ceiling) to the specified number of decimal places.
        /// </summary>
        public static double RoundUp(this double value, int decimals = 0)
        {
            var factor = Math.Pow(10, decimals);
            return Math.Ceiling(value * factor) / factor;
        }

        /// <summary>
        /// Rounds a number down (floor) to the specified number of decimal places.
        /// </summary>
        public static double RoundDown(this double value, int decimals = 0)
        {
            var factor = Math.Pow(10, decimals);
            return Math.Floor(value * factor) / factor;
        }

        /// <summary>
        /// Returns the minimum value among the provided integers.
        /// </summary>
        public static int Min(params int[] values)
        {
            if (values is null || values.Length == 0)
                throw new ArgumentException("At least one value is required.", nameof(values));
            return values.Min();
        }

        /// <summary>
        /// Returns the minimum value among the provided doubles.
        /// </summary>
        public static double Min(params double[] values)
        {
            if (values is null || values.Length == 0)
                throw new ArgumentException("At least one value is required.", nameof(values));
            return values.Min();
        }

        /// <summary>
        /// Returns the minimum value among the provided decimals.
        /// </summary>
        public static decimal Min(params decimal[] values)
        {
            if (values is null || values.Length == 0)
                throw new ArgumentException("At least one value is required.", nameof(values));
            return values.Min();
        }

        /// <summary>
        /// Returns the maximum value among the provided integers.
        /// </summary>
        public static int Max(params int[] values)
        {
            if (values is null || values.Length == 0)
                throw new ArgumentException("At least one value is required.", nameof(values));
            return values.Max();
        }

        /// <summary>
        /// Returns the maximum value among the provided doubles.
        /// </summary>
        public static double Max(params double[] values)
        {
            if (values is null || values.Length == 0)
                throw new ArgumentException("At least one value is required.", nameof(values));
            return values.Max();
        }

        /// <summary>
        /// Returns the maximum value among the provided decimals.
        /// </summary>
        public static decimal Max(params decimal[] values)
        {
            if (values is null || values.Length == 0)
                throw new ArgumentException("At least one value is required.", nameof(values));
            return values.Max();
        }

        /// <summary>
        /// Calculates the arithmetic mean of the provided numbers.
        /// </summary>
        public static double Average(params double[] values)
        {
            if (values is null || values.Length == 0)
                throw new ArgumentException("At least one value is required.", nameof(values));
            return values.Average();
        }

        /// <summary>
        /// Calculates the median of a sequence of numbers.
        /// </summary>
        public static double Median(IEnumerable<double> values)
        {
            if (values is null) throw new ArgumentNullException(nameof(values));
            var sorted = values.OrderBy(v => v).ToArray();
            if (sorted.Length == 0)
                throw new ArgumentException("Sequence cannot be empty.", nameof(values));

            int mid = sorted.Length / 2;
            return sorted.Length % 2 == 0
                ? (sorted[mid - 1] + sorted[mid]) / 2.0
                : sorted[mid];
        }

        /// <summary>
        /// Checks whether an integer is even.
        /// </summary>
        public static bool IsEven(this int value) => (value & 1) == 0;

        /// <summary>
        /// Checks whether an integer is odd.
        /// </summary>
        public static bool IsOdd(this int value) => (value & 1) != 0;

        /// <summary>
        /// Checks whether a number is prime.
        /// </summary>
        public static bool IsPrime(this long value)
        {
            if (value <= 1) return false;
            if (value <= 3) return true;
            if (value % 2 == 0 || value % 3 == 0) return false;

            for (long i = 5; i * i <= value; i += 6)
            {
                if (value % i == 0 || value % (i + 2) == 0)
                    return false;
            }
            return true;
        }

        /// <summary>
        /// Computes the factorial of a non-negative integer (0..20).
        /// </summary>
        public static long Factorial(int n)
        {
            if (n < 0 || n > 20)
                throw new ArgumentOutOfRangeException(nameof(n), "Factorial supports values from 0 to 20 without Int64 overflow.");

            long result = 1;
            for (int i = 2; i <= n; i++)
                result *= i;
            return result;
        }

        /// <summary>
        /// Computes the Greatest Common Divisor (GCD) of two integers.
        /// </summary>
        public static long Gcd(long a, long b)
        {
            a = Math.Abs(a);
            b = Math.Abs(b);
            while (b != 0)
            {
                (a, b) = (b, a % b);
            }
            return a;
        }

        /// <summary>
        /// Computes the Least Common Multiple (LCM) of two integers.
        /// </summary>
        public static long Lcm(long a, long b)
        {
            if (a == 0 || b == 0) return 0;
            return Math.Abs(a / Gcd(a, b) * b);
        }

        /// <summary>
        /// Linearly interpolates between <paramref name="start"/> and <paramref name="end"/> by <paramref name="t"/> (0.0 to 1.0).
        /// </summary>
        public static double Lerp(double start, double end, double t)
            => start + (end - start) * Clamp(t, 0.0, 1.0);

        /// <summary>
        /// Converts degrees to radians.
        /// </summary>
        public static double DegreesToRadians(double degrees) => degrees * (Math.PI / 180.0);

        /// <summary>
        /// Converts radians to degrees.
        /// </summary>
        public static double RadiansToDegrees(double radians) => radians * (180.0 / Math.PI);

        /// <summary>
        /// Converts Celsius to Fahrenheit.
        /// </summary>
        public static double CelsiusToFahrenheit(double celsius) => (celsius * 9.0 / 5.0) + 32.0;

        /// <summary>
        /// Converts Fahrenheit to Celsius.
        /// </summary>
        public static double FahrenheitToCelsius(double fahrenheit) => (fahrenheit - 32.0) * 5.0 / 9.0;

        /// <summary>
        /// Converts kilometers to miles.
        /// </summary>
        public static double KilometersToMiles(double kilometers) => kilometers * 0.621371192237334;

        /// <summary>
        /// Converts miles to kilometers.
        /// </summary>
        public static double MilesToKilometers(double miles) => miles / 0.621371192237334;

        /// <summary>
        /// Converts kilograms to pounds.
        /// </summary>
        public static double KilogramsToPounds(double kilograms) => kilograms * 2.20462262185;

        /// <summary>
        /// Converts pounds to kilograms.
        /// </summary>
        public static double PoundsToKilograms(double pounds) => pounds / 2.20462262185;
    }
}
