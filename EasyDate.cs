using System;

namespace SkyWebFramework.Utility
{
    /// <summary>
    /// Enhanced date and time utilities and extension methods.
    /// </summary>
    public static class EasyDate
    {
        /// <summary>
        /// Returns the current UTC time rounded down to the whole second.
        /// </summary>
        public static DateTime UtcNowRoundedSeconds()
        {
            var now = DateTime.UtcNow;
            return new DateTime(now.Year, now.Month, now.Day, now.Hour, now.Minute, now.Second, DateTimeKind.Utc);
        }

        /// <summary>
        /// Returns the current UTC time rounded down to the whole minute.
        /// </summary>
        public static DateTime UtcNowRoundedMinutes()
        {
            var now = DateTime.UtcNow;
            return new DateTime(now.Year, now.Month, now.Day, now.Hour, now.Minute, 0, DateTimeKind.Utc);
        }

        /// <summary>
        /// Returns the current UTC time rounded down to the whole hour.
        /// </summary>
        public static DateTime UtcNowRoundedHours()
        {
            var now = DateTime.UtcNow;
            return new DateTime(now.Year, now.Month, now.Day, now.Hour, 0, 0, DateTimeKind.Utc);
        }

        /// <summary>
        /// Converts a UTC time to the local system time.
        /// </summary>
        public static DateTime ToLocal(this DateTime utcDateTime)
            => utcDateTime.ToLocalTime();

        /// <summary>
        /// Converts a local system time to UTC.
        /// </summary>
        public static DateTime ToUtc(this DateTime localDateTime)
            => localDateTime.ToUniversalTime();

        /// <summary>
        /// Returns a human-readable relative time (e.g., "5 minutes ago" or "in 5 minutes").
        /// </summary>
        public static string ToRelativeTime(this DateTime inputUtc)
        {
            var utcTime = inputUtc.Kind == DateTimeKind.Local ? inputUtc.ToUniversalTime() : inputUtc;
            var delta = DateTime.UtcNow - utcTime;
            var isFuture = delta.TotalSeconds < 0;
            var absDelta = isFuture ? delta.Negate() : delta;

            if (absDelta.TotalSeconds < 60)
                return "just now";

            string unit;
            int value;

            if (absDelta.TotalMinutes < 60)
            {
                value = (int)absDelta.TotalMinutes;
                unit = "minute";
            }
            else if (absDelta.TotalHours < 24)
            {
                value = (int)absDelta.TotalHours;
                unit = "hour";
            }
            else if (absDelta.TotalDays < 30)
            {
                value = (int)absDelta.TotalDays;
                unit = "day";
            }
            else if (absDelta.TotalDays < 365)
            {
                value = Math.Max(1, (int)(absDelta.TotalDays / 30));
                unit = "month";
            }
            else
            {
                value = Math.Max(1, (int)(absDelta.TotalDays / 365));
                unit = "year";
            }

            var plural = value > 1 ? "s" : "";
            return isFuture ? $"in {value} {unit}{plural}" : $"{value} {unit}{plural} ago";
        }

        /// <summary>
        /// Gets the start of the day (00:00:00) for a given date.
        /// </summary>
        public static DateTime StartOfDay(this DateTime date)
            => date.Date;

        /// <summary>
        /// Gets the end of the day (23:59:59.9999999) for a given date.
        /// </summary>
        public static DateTime EndOfDay(this DateTime date)
            => date.Date.AddDays(1).AddTicks(-1);

        /// <summary>
        /// Gets the start of the week for a given date (defaults to Monday).
        /// </summary>
        public static DateTime StartOfWeek(this DateTime date, DayOfWeek startOfWeek = DayOfWeek.Monday)
        {
            var diff = (7 + (date.DayOfWeek - startOfWeek)) % 7;
            return date.AddDays(-diff).Date;
        }

        /// <summary>
        /// Gets the end of the week for a given date (defaults to Sunday 23:59:59.9999999).
        /// </summary>
        public static DateTime EndOfWeek(this DateTime date, DayOfWeek startOfWeek = DayOfWeek.Monday)
            => StartOfWeek(date, startOfWeek).AddDays(7).AddTicks(-1);

        /// <summary>
        /// Gets the start of the month for a given date.
        /// </summary>
        public static DateTime StartOfMonth(this DateTime date)
            => new DateTime(date.Year, date.Month, 1, 0, 0, 0, date.Kind);

        /// <summary>
        /// Gets the end of the month for a given date.
        /// </summary>
        public static DateTime EndOfMonth(this DateTime date)
            => new DateTime(date.Year, date.Month, DateTime.DaysInMonth(date.Year, date.Month), 23, 59, 59, 999, date.Kind);

        /// <summary>
        /// Gets the calendar quarter (1-4) for a given date.
        /// </summary>
        public static int GetQuarter(this DateTime date)
            => (date.Month - 1) / 3 + 1;

        /// <summary>
        /// Gets the start of the calendar quarter for a given date.
        /// </summary>
        public static DateTime StartOfQuarter(this DateTime date)
        {
            var firstMonth = (GetQuarter(date) - 1) * 3 + 1;
            return new DateTime(date.Year, firstMonth, 1, 0, 0, 0, date.Kind);
        }

        /// <summary>
        /// Gets the end of the calendar quarter for a given date.
        /// </summary>
        public static DateTime EndOfQuarter(this DateTime date)
            => StartOfQuarter(date).AddMonths(3).AddTicks(-1);

        /// <summary>
        /// Gets the start of the year for a given date.
        /// </summary>
        public static DateTime StartOfYear(this DateTime date)
            => new DateTime(date.Year, 1, 1, 0, 0, 0, date.Kind);

        /// <summary>
        /// Gets the end of the year for a given date.
        /// </summary>
        public static DateTime EndOfYear(this DateTime date)
            => new DateTime(date.Year, 12, 31, 23, 59, 59, 999, date.Kind);

        /// <summary>
        /// Checks if a date is today (local time).
        /// </summary>
        public static bool IsToday(this DateTime date)
            => date.Date == DateTime.Today;

        /// <summary>
        /// Checks if a date is in the past.
        /// </summary>
        public static bool IsPast(this DateTime date)
            => date.Kind == DateTimeKind.Utc ? date < DateTime.UtcNow : date < DateTime.Now;

        /// <summary>
        /// Checks if a date is in the future.
        /// </summary>
        public static bool IsFuture(this DateTime date)
            => date.Kind == DateTimeKind.Utc ? date > DateTime.UtcNow : date > DateTime.Now;

        /// <summary>
        /// Checks if a date falls on a Saturday or Sunday.
        /// </summary>
        public static bool IsWeekend(this DateTime date)
            => date.DayOfWeek == DayOfWeek.Saturday || date.DayOfWeek == DayOfWeek.Sunday;

        /// <summary>
        /// Checks if a date falls on a weekday (Monday through Friday).
        /// </summary>
        public static bool IsBusinessDay(this DateTime date)
            => !IsWeekend(date);

        /// <summary>
        /// Checks if a date is within the specified range (inclusive).
        /// </summary>
        public static bool IsInRange(this DateTime date, DateTime start, DateTime end)
            => date >= start && date <= end;

        /// <summary>
        /// Checks if a date is between two dates (inclusive by default).
        /// </summary>
        public static bool IsBetween(this DateTime date, DateTime start, DateTime end, bool inclusive = true)
            => inclusive ? (date >= start && date <= end) : (date > start && date < end);

        /// <summary>
        /// Calculates age in full years from a birth date.
        /// </summary>
        public static int CalculateAge(this DateTime birthDate)
        {
            var today = DateTime.Today;
            var age = today.Year - birthDate.Year;
            if (birthDate.Date > today.AddYears(-age)) age--;
            return age;
        }

        /// <summary>
        /// Formats date in ISO 8601 format.
        /// </summary>
        public static string ToIso8601(this DateTime date)
            => date.ToString("yyyy-MM-ddTHH:mm:ss.fffZ");

        /// <summary>
        /// Gets the count of business days between two dates (inclusive, excludes weekends).
        /// </summary>
        public static int GetBusinessDays(DateTime start, DateTime end)
        {
            if (end.Date < start.Date) return 0;

            var totalDays = 0;
            for (var date = start.Date; date <= end.Date; date = date.AddDays(1))
            {
                if (IsBusinessDay(date))
                    totalDays++;
            }
            return totalDays;
        }

        /// <summary>
        /// Adds (or subtracts when negative) the specified number of business days, skipping weekends.
        /// </summary>
        public static DateTime AddBusinessDays(this DateTime date, int businessDays)
        {
            if (businessDays == 0) return date;

            var direction = businessDays > 0 ? 1 : -1;
            var remaining = Math.Abs(businessDays);
            var current = date;

            while (remaining > 0)
            {
                current = current.AddDays(direction);
                if (IsBusinessDay(current))
                    remaining--;
            }

            return current;
        }

        /// <summary>
        /// Subtracts the specified number of business days, skipping weekends.
        /// </summary>
        public static DateTime SubtractBusinessDays(this DateTime date, int businessDays)
            => AddBusinessDays(date, -businessDays);

        /// <summary>
        /// Converts a DateTime to Unix timestamp in seconds.
        /// </summary>
        public static long ToUnixTimestamp(this DateTime date)
        {
            var dto = date.Kind == DateTimeKind.Unspecified
                ? new DateTimeOffset(DateTime.SpecifyKind(date, DateTimeKind.Utc))
                : new DateTimeOffset(date);
            return dto.ToUnixTimeSeconds();
        }

        /// <summary>
        /// Converts a DateTime to Unix timestamp in milliseconds.
        /// </summary>
        public static long ToUnixTimestampMilliseconds(this DateTime date)
        {
            var dto = date.Kind == DateTimeKind.Unspecified
                ? new DateTimeOffset(DateTime.SpecifyKind(date, DateTimeKind.Utc))
                : new DateTimeOffset(date);
            return dto.ToUnixTimeMilliseconds();
        }

        /// <summary>
        /// Converts a Unix timestamp in seconds to a UTC DateTime.
        /// </summary>
        public static DateTime FromUnixTimestamp(long unixSeconds)
            => DateTimeOffset.FromUnixTimeSeconds(unixSeconds).UtcDateTime;

        /// <summary>
        /// Converts a Unix timestamp in milliseconds to a UTC DateTime.
        /// </summary>
        public static DateTime FromUnixTimestampMilliseconds(long unixMilliseconds)
            => DateTimeOffset.FromUnixTimeMilliseconds(unixMilliseconds).UtcDateTime;

        /// <summary>
        /// Checks whether the year of the given date is a leap year.
        /// </summary>
        public static bool IsLeapYear(this DateTime date)
            => DateTime.IsLeapYear(date.Year);

        /// <summary>
        /// Gets the total number of days in the month of the given date.
        /// </summary>
        public static int DaysInMonth(this DateTime date)
            => DateTime.DaysInMonth(date.Year, date.Month);

        /// <summary>
        /// Gets the next occurrence of the specified day of the week.
        /// </summary>
        public static DateTime NextDayOfWeek(this DateTime date, DayOfWeek targetDay, bool includeToday = false)
        {
            var start = includeToday ? date.Date : date.Date.AddDays(1);
            var daysUntil = ((int)targetDay - (int)start.DayOfWeek + 7) % 7;
            return start.AddDays(daysUntil);
        }
    }
}
