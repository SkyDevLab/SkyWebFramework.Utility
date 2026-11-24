using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Reflection;

namespace SkyWebFramework.Utility
{
    /// <summary>
    /// Enumeration helper and extension methods.
    /// </summary>
    public static class EasyEnum
    {
        /// <summary>
        /// Returns the <see cref="DescriptionAttribute"/> text for an enum value, or its string name if none is defined.
        /// </summary>
        public static string GetDescription(this Enum value)
        {
            if (value is null) throw new ArgumentNullException(nameof(value));

            var name = value.ToString();
            var field = value.GetType().GetField(name);
            if (field is null) return name;

            var attr = field.GetCustomAttribute<DescriptionAttribute>();
            return attr?.Description ?? name;
        }

        /// <summary>
        /// Returns all values of the enum type <typeparamref name="TEnum"/>.
        /// </summary>
        public static IReadOnlyList<TEnum> GetValues<TEnum>() where TEnum : struct, Enum
            => Enum.GetValues<TEnum>();

        /// <summary>
        /// Returns all names of the enum type <typeparamref name="TEnum"/>.
        /// </summary>
        public static IReadOnlyList<string> GetNames<TEnum>() where TEnum : struct, Enum
            => Enum.GetNames<TEnum>();

        /// <summary>
        /// Converts an enum type into a dictionary mapping integer values to names (or descriptions when <paramref name="useDescription"/> is true).
        /// </summary>
        public static Dictionary<int, string> ToDictionary<TEnum>(bool useDescription = false) where TEnum : struct, Enum
        {
            var result = new Dictionary<int, string>();
            foreach (var value in Enum.GetValues<TEnum>())
            {
                var intVal = Convert.ToInt32(value);
                result[intVal] = useDescription ? value.GetDescription() : value.ToString();
            }
            return result;
        }

        /// <summary>
        /// Parses an enum value from a string, returning <paramref name="defaultValue"/> if parsing fails.
        /// </summary>
        public static TEnum ParseOrDefault<TEnum>(string? value, TEnum defaultValue = default, bool ignoreCase = true)
            where TEnum : struct, Enum
        {
            if (string.IsNullOrWhiteSpace(value))
                return defaultValue;

            return Enum.TryParse<TEnum>(value.Trim(), ignoreCase, out var parsed)
                ? parsed
                : defaultValue;
        }

        /// <summary>
        /// Checks whether an enum value is formally defined on <typeparamref name="TEnum"/>.
        /// </summary>
        public static bool IsDefined<TEnum>(this TEnum value) where TEnum : struct, Enum
            => Enum.IsDefined(value);

        /// <summary>
        /// Extracts individual active flags from a <see cref="FlagsAttribute"/> enum value (excludes zero-valued flags).
        /// </summary>
        public static IEnumerable<TEnum> GetFlags<TEnum>(this TEnum value) where TEnum : struct, Enum
        {
            foreach (var flag in Enum.GetValues<TEnum>())
            {
                if (Convert.ToInt64(flag) != 0 && value.HasFlag(flag))
                    yield return flag;
            }
        }
    }
}
