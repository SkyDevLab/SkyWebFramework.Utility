using System;
using System.Collections.Generic;
using System.Globalization;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Text.Json;

namespace SkyWebFramework.Utility
{
    /// <summary>
    /// Object manipulation, cloning, mapping, conversion, and null-safety utilities.
    /// </summary>
    public static class EasyObject
    {
        private static readonly JsonSerializerOptions _cloneOptions = new()
        {
            PropertyNameCaseInsensitive = true
        };

        /// <summary>
        /// Checks whether the object reference is null.
        /// </summary>
        public static bool IsNull(this object? obj) => obj is null;

        /// <summary>
        /// Checks whether the object reference is not null.
        /// </summary>
        public static bool IsNotNull(this object? obj) => obj is not null;

        /// <summary>
        /// Throws an <see cref="ArgumentNullException"/> if <paramref name="obj"/> is null; otherwise returns the non-null instance.
        /// </summary>
        public static T ThrowIfNull<T>(this T? obj, [CallerArgumentExpression(nameof(obj))] string? paramName = null)
            where T : class
        {
            if (obj is null)
                throw new ArgumentNullException(paramName);
            return obj;
        }

        /// <summary>
        /// Returns <paramref name="defaultValue"/> if <paramref name="obj"/> is null; otherwise returns <paramref name="obj"/>.
        /// </summary>
        public static T OrDefault<T>(this T? obj, T defaultValue) where T : class
            => obj ?? defaultValue;

        /// <summary>
        /// Checks whether a value equals the default value for its type.
        /// </summary>
        public static bool IsDefault<T>(this T value)
            => EqualityComparer<T>.Default.Equals(value, default!);

        /// <summary>
        /// Creates a deep clone of an object using JSON serialization.
        /// </summary>
        public static T? DeepClone<T>(this T? source)
        {
            if (source is null) return default;
            var json = JsonSerializer.Serialize(source, _cloneOptions);
            return JsonSerializer.Deserialize<T>(json, _cloneOptions);
        }

        /// <summary>
        /// Maps matching public readable/writable properties from <paramref name="source"/> onto a new instance of <typeparamref name="TTarget"/>.
        /// </summary>
        public static TTarget MapTo<TTarget>(this object source, bool ignoreNulls = false) where TTarget : new()
        {
            if (source is null) throw new ArgumentNullException(nameof(source));
            var target = new TTarget();
            CopyPropertiesTo(source, target, ignoreNulls);
            return target;
        }

        /// <summary>
        /// Copies matching public properties from <paramref name="source"/> to <paramref name="target"/>.
        /// </summary>
        public static void CopyPropertiesTo<TTarget>(this object source, TTarget target, bool ignoreNulls = false)
        {
            if (source is null) throw new ArgumentNullException(nameof(source));
            if (target is null) throw new ArgumentNullException(nameof(target));

            var sourceProps = source.GetType().GetProperties(BindingFlags.Public | BindingFlags.Instance);
            var targetProps = typeof(TTarget).GetProperties(BindingFlags.Public | BindingFlags.Instance);

            var targetMap = new Dictionary<string, PropertyInfo>(StringComparer.OrdinalIgnoreCase);
            foreach (var tp in targetProps)
            {
                if (tp.CanWrite)
                    targetMap[tp.Name] = tp;
            }

            foreach (var sp in sourceProps)
            {
                if (!sp.CanRead || sp.GetIndexParameters().Length > 0) continue;
                if (!targetMap.TryGetValue(sp.Name, out var tp)) continue;

                var value = sp.GetValue(source);
                if (ignoreNulls && value is null) continue;

                if (value is null)
                {
                    if (!tp.PropertyType.IsValueType || Nullable.GetUnderlyingType(tp.PropertyType) is not null)
                        tp.SetValue(target, null);
                    continue;
                }

                var underlyingTarget = Nullable.GetUnderlyingType(tp.PropertyType) ?? tp.PropertyType;
                if (underlyingTarget.IsAssignableFrom(value.GetType()))
                {
                    tp.SetValue(target, value);
                }
                else if (TryConvertInternal(value, tp.PropertyType, out var converted))
                {
                    tp.SetValue(target, converted);
                }
            }
        }

        /// <summary>
        /// Converts an object's public readable properties into a case-insensitive dictionary.
        /// </summary>
        public static Dictionary<string, object?> ToDictionary(this object? obj)
        {
            var dict = new Dictionary<string, object?>(StringComparer.OrdinalIgnoreCase);
            if (obj is null) return dict;

            foreach (var prop in obj.GetType().GetProperties(BindingFlags.Public | BindingFlags.Instance))
            {
                if (prop.CanRead && prop.GetIndexParameters().Length == 0)
                    dict[prop.Name] = prop.GetValue(obj);
            }

            return dict;
        }

        /// <summary>
        /// Safely retrieves a property value (supporting nested dot notation such as "Address.City") from an object.
        /// </summary>
        public static T? GetPropertyValue<T>(this object? obj, string propertyPath, T? defaultValue = default)
        {
            if (obj is null || string.IsNullOrWhiteSpace(propertyPath))
                return defaultValue;

            object? current = obj;
            foreach (var segment in propertyPath.Split('.', StringSplitOptions.RemoveEmptyEntries))
            {
                if (current is null) return defaultValue;

                var prop = current.GetType().GetProperty(segment, BindingFlags.Public | BindingFlags.Instance | BindingFlags.IgnoreCase);
                if (prop is null || !prop.CanRead)
                    return defaultValue;

                current = prop.GetValue(current);
            }

            if (current is null) return defaultValue;
            return TryConvert(current, out T? converted) ? converted : defaultValue;
        }

        /// <summary>
        /// Attempts to convert a value to the target type <typeparamref name="T"/>.
        /// </summary>
        public static bool TryConvert<T>(this object? value, out T? result)
        {
            if (TryConvertInternal(value, typeof(T), out var raw) && raw is T typed)
            {
                result = typed;
                return true;
            }

            if (value is null && (!typeof(T).IsValueType || Nullable.GetUnderlyingType(typeof(T)) is not null))
            {
                result = default;
                return true;
            }

            result = default;
            return false;
        }

        /// <summary>
        /// Converts a value to the target type <typeparamref name="T"/>, or returns <paramref name="defaultValue"/> on failure.
        /// </summary>
        public static T? ConvertTo<T>(this object? value, T? defaultValue = default)
            => TryConvert<T>(value, out var result) ? result : defaultValue;

        private static bool TryConvertInternal(object? value, Type targetType, out object? result)
        {
            if (value is null)
            {
                result = null;
                return !targetType.IsValueType || Nullable.GetUnderlyingType(targetType) is not null;
            }

            var nonNullableTarget = Nullable.GetUnderlyingType(targetType) ?? targetType;
            if (nonNullableTarget.IsAssignableFrom(value.GetType()))
            {
                result = value;
                return true;
            }

            try
            {
                if (nonNullableTarget.IsEnum)
                {
                    if (value is string s)
                    {
                        if (Enum.TryParse(nonNullableTarget, s, ignoreCase: true, out var enumVal))
                        {
                            result = enumVal;
                            return true;
                        }
                        result = null;
                        return false;
                    }

                    result = Enum.ToObject(nonNullableTarget, value);
                    return true;
                }

                if (nonNullableTarget == typeof(Guid) && value is string guidStr)
                {
                    if (Guid.TryParse(guidStr, out var guid))
                    {
                        result = guid;
                        return true;
                    }
                    result = null;
                    return false;
                }

                result = Convert.ChangeType(value, nonNullableTarget, CultureInfo.InvariantCulture);
                return true;
            }
            catch
            {
                result = null;
                return false;
            }
        }
    }
}
