using System;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Text.Json.Serialization;

namespace SkyWebFramework.Utility
{
    /// <summary>
    /// Zero-dependency JSON serialization, validation, formatting, and merging helpers built on System.Text.Json.
    /// </summary>
    public static class EasyJson
    {
        /// <summary>
        /// Serializes an object to a JSON string.
        /// </summary>
        public static string ToJson<T>(this T value, bool indented = false, bool camelCase = true, bool ignoreNulls = true)
        {
            var options = new JsonSerializerOptions
            {
                WriteIndented = indented,
                PropertyNamingPolicy = camelCase ? JsonNamingPolicy.CamelCase : null,
                DefaultIgnoreCondition = ignoreNulls ? JsonIgnoreCondition.WhenWritingNull : JsonIgnoreCondition.Never
            };

            return JsonSerializer.Serialize(value, options);
        }

        /// <summary>
        /// Deserializes a JSON string to <typeparamref name="T"/> (returns default if input is null or whitespace).
        /// </summary>
        public static T? FromJson<T>(this string? json, bool caseInsensitive = true)
        {
            if (string.IsNullOrWhiteSpace(json))
                return default;

            var options = new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = caseInsensitive
            };

            return JsonSerializer.Deserialize<T>(json, options);
        }

        /// <summary>
        /// Attempts to deserialize a JSON string without throwing on malformed JSON.
        /// </summary>
        public static bool TryFromJson<T>(this string? json, out T? result, bool caseInsensitive = true)
        {
            result = default;
            if (string.IsNullOrWhiteSpace(json))
                return false;

            try
            {
                result = FromJson<T>(json, caseInsensitive);
                return true;
            }
            catch (JsonException)
            {
                return false;
            }
        }

        /// <summary>
        /// Checks whether a string is syntactically valid JSON.
        /// </summary>
        public static bool IsValidJson(this string? json)
        {
            if (string.IsNullOrWhiteSpace(json))
                return false;

            try
            {
                using var doc = JsonDocument.Parse(json);
                return doc is not null;
            }
            catch (JsonException)
            {
                return false;
            }
        }

        /// <summary>
        /// Formats a valid JSON string with indentation.
        /// </summary>
        public static string PrettifyJson(this string? json)
        {
            if (string.IsNullOrWhiteSpace(json))
                return string.Empty;

            using var doc = JsonDocument.Parse(json);
            return JsonSerializer.Serialize(doc.RootElement, new JsonSerializerOptions { WriteIndented = true });
        }

        /// <summary>
        /// Minifies a valid JSON string by stripping unnecessary whitespace.
        /// </summary>
        public static string MinifyJson(this string? json)
        {
            if (string.IsNullOrWhiteSpace(json))
                return string.Empty;

            using var doc = JsonDocument.Parse(json);
            return JsonSerializer.Serialize(doc.RootElement, new JsonSerializerOptions { WriteIndented = false });
        }

        /// <summary>
        /// Deep-merges two JSON object strings, where properties in <paramref name="overrideJson"/> override those in <paramref name="baseJson"/>.
        /// </summary>
        public static string MergeJson(string baseJson, string overrideJson, bool indented = false)
        {
            if (string.IsNullOrWhiteSpace(baseJson)) throw new ArgumentException("Base JSON cannot be empty.", nameof(baseJson));
            if (string.IsNullOrWhiteSpace(overrideJson)) throw new ArgumentException("Override JSON cannot be empty.", nameof(overrideJson));

            var baseNode = JsonNode.Parse(baseJson) as JsonObject
                ?? throw new ArgumentException("Base JSON must be a JSON object.", nameof(baseJson));
            var overrideNode = JsonNode.Parse(overrideJson) as JsonObject
                ?? throw new ArgumentException("Override JSON must be a JSON object.", nameof(overrideJson));

            MergeObjects(baseNode, overrideNode);
            return baseNode.ToJsonString(new JsonSerializerOptions { WriteIndented = indented });
        }

        private static void MergeObjects(JsonObject target, JsonObject source)
        {
            foreach (var kvp in source)
            {
                if (kvp.Value is JsonObject sourceObj && target[kvp.Key] is JsonObject targetObj)
                {
                    MergeObjects(targetObj, sourceObj);
                }
                else
                {
                    target[kvp.Key] = kvp.Value?.DeepClone();
                }
            }
        }
    }
}
