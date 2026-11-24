using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace SkyWebFramework.Utility
{
    /// <summary>
    /// Enhanced string utilities and extension methods for manipulation, formatting, and validation.
    /// </summary>
    public static class EasyString
    {
        private static readonly Regex _nonAlphanumericRegex = new("[^a-z0-9-]", RegexOptions.Compiled);
        private static readonly Regex _whitespaceRegex = new(@"\s+", RegexOptions.Compiled);
        private static readonly Regex _emailRegex = new(@"^[^@\s]+@[^@\s]+\.[^@\s]+$", RegexOptions.Compiled | RegexOptions.IgnoreCase);
        private static readonly Regex _phoneRegex = new(@"^[\+]?[0-9\-\s\(\)]{10,20}$", RegexOptions.Compiled);
        private static readonly Regex _htmlTagRegex = new(@"<[^>]+>", RegexOptions.Compiled);
        private static readonly Regex _specialCharsKeepSpacesRegex = new(@"[^a-zA-Z0-9\s]", RegexOptions.Compiled);
        private static readonly Regex _specialCharsNoSpacesRegex = new(@"[^a-zA-Z0-9]", RegexOptions.Compiled);
        private static readonly Regex _wordSplitRegex = new(@"(?<=[a-z0-9])(?=[A-Z])|(?<=[A-Z])(?=[A-Z][a-z])|[\s_\-]+", RegexOptions.Compiled);

        /// <summary>
        /// Converts a string into a URL-safe slug (e.g., "Hello World!" -> "hello-world").
        /// </summary>
        public static string Slugify(this string? input)
        {
            if (string.IsNullOrWhiteSpace(input))
                return string.Empty;

            var normalized = input.Normalize(NormalizationForm.FormKD);
            var sb = new StringBuilder();

            foreach (var ch in normalized)
            {
                if (char.IsLetterOrDigit(ch))
                    sb.Append(char.ToLowerInvariant(ch));
                else if (char.IsWhiteSpace(ch) || ch == '-' || ch == '_')
                    sb.Append('-');
            }

            var slug = _nonAlphanumericRegex.Replace(sb.ToString(), "-");
            while (slug.Contains("--", StringComparison.Ordinal))
                slug = slug.Replace("--", "-", StringComparison.Ordinal);

            return slug.Trim('-');
        }

        /// <summary>
        /// Converts the string to title case (e.g., "hello world" -> "Hello World").
        /// </summary>
        public static string ToTitleCase(this string? input)
        {
            if (string.IsNullOrWhiteSpace(input))
                return string.Empty;

            return CultureInfo.CurrentCulture.TextInfo.ToTitleCase(input.ToLowerInvariant());
        }

        /// <summary>
        /// Returns true if the string is null, empty, or consists only of whitespace.
        /// </summary>
        public static bool IsNullOrWhiteSpace(this string? input)
            => string.IsNullOrWhiteSpace(input);

        /// <summary>
        /// Returns true if the string is null or empty.
        /// </summary>
        public static bool IsNullOrEmpty(this string? input)
            => string.IsNullOrEmpty(input);

        /// <summary>
        /// Removes diacritic (accent) marks from a string (e.g., "Café" -> "Cafe").
        /// </summary>
        public static string RemoveDiacritics(this string? input)
        {
            if (string.IsNullOrWhiteSpace(input))
                return string.Empty;

            var normalized = input.Normalize(NormalizationForm.FormD);
            var chars = normalized.Where(c => CharUnicodeInfo.GetUnicodeCategory(c) != UnicodeCategory.NonSpacingMark);
            return new string(chars.ToArray()).Normalize(NormalizationForm.FormC);
        }

        /// <summary>
        /// Truncates a string to a maximum length, optionally adding ellipsis ("...").
        /// </summary>
        public static string Truncate(this string? input, int maxLength, bool addEllipsis = false)
        {
            if (string.IsNullOrEmpty(input) || maxLength <= 0)
                return string.Empty;

            if (input.Length <= maxLength)
                return input;

            return addEllipsis && maxLength > 3
                ? input[..(maxLength - 3)] + "..."
                : input[..maxLength];
        }

        /// <summary>
        /// Returns a string containing only digits from the input.
        /// </summary>
        public static string ExtractDigits(this string? input)
        {
            if (string.IsNullOrEmpty(input))
                return string.Empty;

            var sb = new StringBuilder(input.Length);
            foreach (var c in input)
            {
                if (char.IsDigit(c))
                    sb.Append(c);
            }

            return sb.ToString();
        }

        /// <summary>
        /// Checks if two strings are equal ignoring case using ordinal comparison.
        /// </summary>
        public static bool EqualsIgnoreCase(this string? a, string? b)
            => string.Equals(a, b, StringComparison.OrdinalIgnoreCase);

        /// <summary>
        /// Checks whether a string contains the specified substring ignoring case.
        /// </summary>
        public static bool ContainsIgnoreCase(this string? source, string? value)
        {
            if (source is null || value is null) return false;
            return source.Contains(value, StringComparison.OrdinalIgnoreCase);
        }

        /// <summary>
        /// Generates a random string of the specified length from the given character set.
        /// </summary>
        public static string GenerateRandomString(int length, string characterSet = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789")
        {
            if (length <= 0) return string.Empty;
            if (string.IsNullOrEmpty(characterSet))
                throw new ArgumentException("Character set cannot be null or empty.", nameof(characterSet));

            var result = new char[length];
            for (int i = 0; i < length; i++)
            {
                result[i] = characterSet[Random.Shared.Next(characterSet.Length)];
            }

            return new string(result);
        }

        /// <summary>
        /// Validates email format.
        /// </summary>
        public static bool IsValidEmail(this string? email)
            => !string.IsNullOrWhiteSpace(email) && _emailRegex.IsMatch(email);

        /// <summary>
        /// Validates phone number format (international).
        /// </summary>
        public static bool IsValidPhone(this string? phone)
        {
            if (string.IsNullOrWhiteSpace(phone)) return false;
            var digitsOnly = ExtractDigits(phone);
            return digitsOnly.Length is >= 10 and <= 15 && _phoneRegex.IsMatch(phone);
        }

        /// <summary>
        /// Validates whether a string is a valid HTTP or HTTPS URL.
        /// </summary>
        public static bool IsValidUrl(this string? url, bool requireHttps = false)
            => EasyValidation.IsValidUrl(url, requireHttps);

        /// <summary>
        /// Converts string to camelCase (e.g., "Hello World" -> "helloWorld").
        /// </summary>
        public static string ToCamelCase(this string? input)
        {
            var words = SplitIntoWords(input);
            if (words.Count == 0) return string.Empty;

            var sb = new StringBuilder(words[0].ToLowerInvariant());
            for (int i = 1; i < words.Count; i++)
            {
                var w = words[i];
                sb.Append(char.ToUpperInvariant(w[0]));
                if (w.Length > 1)
                    sb.Append(w[1..].ToLowerInvariant());
            }

            return sb.ToString();
        }

        /// <summary>
        /// Converts string to PascalCase (e.g., "hello world" -> "HelloWorld").
        /// </summary>
        public static string ToPascalCase(this string? input)
        {
            var words = SplitIntoWords(input);
            if (words.Count == 0) return string.Empty;

            var sb = new StringBuilder();
            foreach (var w in words)
            {
                sb.Append(char.ToUpperInvariant(w[0]));
                if (w.Length > 1)
                    sb.Append(w[1..].ToLowerInvariant());
            }

            return sb.ToString();
        }

        /// <summary>
        /// Converts string to snake_case (e.g., "HelloWorld" -> "hello_world").
        /// </summary>
        public static string ToSnakeCase(this string? input)
        {
            var words = SplitIntoWords(input);
            if (words.Count == 0) return string.Empty;

            return string.Join("_", words.Select(w => w.ToLowerInvariant()));
        }

        /// <summary>
        /// Converts string to kebab-case (e.g., "HelloWorld" -> "hello-world").
        /// </summary>
        public static string ToKebabCase(this string? input)
        {
            var words = SplitIntoWords(input);
            if (words.Count == 0) return string.Empty;

            return string.Join("-", words.Select(w => w.ToLowerInvariant()));
        }

        /// <summary>
        /// Masks sensitive information leaving the trailing characters visible (e.g., "123456789" -> "*****6789").
        /// </summary>
        public static string Mask(this string? input, int visibleChars = 4, char maskChar = '*')
        {
            if (string.IsNullOrEmpty(input)) return string.Empty;
            if (visibleChars <= 0) return new string(maskChar, input.Length);
            if (input.Length <= visibleChars) return new string(maskChar, input.Length);

            return new string(maskChar, input.Length - visibleChars) + input[^visibleChars..];
        }

        /// <summary>
        /// Masks an email address preserving the first character of the local part and the full domain.
        /// </summary>
        public static string MaskEmail(this string? email, char maskChar = '*')
        {
            if (string.IsNullOrWhiteSpace(email)) return string.Empty;
            var atIndex = email.IndexOf('@');
            if (atIndex <= 0) return Mask(email, 2, maskChar);

            var localPart = email[..atIndex];
            var domainPart = email[atIndex..];

            if (localPart.Length <= 1)
                return maskChar + domainPart;

            return localPart[0] + new string(maskChar, localPart.Length - 1) + domainPart;
        }

        /// <summary>
        /// Extracts initials from a name (e.g., "John Doe" -> "JD").
        /// </summary>
        public static string GetInitials(this string? name)
        {
            if (string.IsNullOrWhiteSpace(name)) return string.Empty;

            var words = _whitespaceRegex.Split(name).Where(w => !string.IsNullOrEmpty(w));
            return string.Concat(words.Select(w => char.ToUpperInvariant(w[0])));
        }

        /// <summary>
        /// Removes all whitespace characters from the input string.
        /// </summary>
        public static string RemoveWhitespace(this string? input)
        {
            if (string.IsNullOrEmpty(input)) return string.Empty;
            return _whitespaceRegex.Replace(input, string.Empty);
        }

        /// <summary>
        /// Collapses consecutive whitespace characters into a single space and trims the result.
        /// </summary>
        public static string CollapseWhitespace(this string? input)
        {
            if (string.IsNullOrWhiteSpace(input)) return string.Empty;
            return _whitespaceRegex.Replace(input.Trim(), " ");
        }

        /// <summary>
        /// Removes special characters (non-alphanumeric) from a string.
        /// </summary>
        public static string RemoveSpecialCharacters(this string? input, bool allowSpaces = true)
        {
            if (string.IsNullOrEmpty(input)) return string.Empty;
            var regex = allowSpaces ? _specialCharsKeepSpacesRegex : _specialCharsNoSpacesRegex;
            return regex.Replace(input, string.Empty);
        }

        /// <summary>
        /// Strips HTML tags from the input string.
        /// </summary>
        public static string StripHtml(this string? input)
        {
            if (string.IsNullOrEmpty(input)) return string.Empty;
            return _htmlTagRegex.Replace(input, string.Empty);
        }

        /// <summary>
        /// Extracts words separated by whitespace from the input string.
        /// </summary>
        public static string[] ExtractWords(this string? input)
        {
            if (string.IsNullOrWhiteSpace(input)) return Array.Empty<string>();
            return _whitespaceRegex.Split(input.Trim()).Where(w => !string.IsNullOrEmpty(w)).ToArray();
        }

        /// <summary>
        /// Extracts lines from a multi-line string.
        /// </summary>
        public static string[] ExtractLines(this string? input, bool removeEmpty = true)
        {
            if (string.IsNullOrEmpty(input)) return Array.Empty<string>();
            var options = removeEmpty ? StringSplitOptions.RemoveEmptyEntries : StringSplitOptions.None;
            return input.Split(new[] { "\r\n", "\r", "\n" }, options);
        }

        /// <summary>
        /// Counts the number of whitespace-separated words in the input string.
        /// </summary>
        public static int WordCount(this string? input)
            => ExtractWords(input).Length;

        /// <summary>
        /// Reverses the characters in a string.
        /// </summary>
        public static string Reverse(this string? input)
        {
            if (string.IsNullOrEmpty(input)) return string.Empty;
            var chars = input.ToCharArray();
            Array.Reverse(chars);
            return new string(chars);
        }

        /// <summary>
        /// Randomly shuffles the characters in a string.
        /// </summary>
        public static string Shuffle(this string? input)
        {
            if (string.IsNullOrEmpty(input)) return string.Empty;
            var chars = input.ToCharArray();
            for (int i = chars.Length - 1; i > 0; i--)
            {
                int j = Random.Shared.Next(i + 1);
                (chars[i], chars[j]) = (chars[j], chars[i]);
            }
            return new string(chars);
        }

        /// <summary>
        /// Repeats a string a specified number of times.
        /// </summary>
        public static string Repeat(this string? input, int count)
        {
            if (string.IsNullOrEmpty(input) || count <= 0) return string.Empty;
            if (count == 1) return input;

            var sb = new StringBuilder(input.Length * count);
            for (int i = 0; i < count; i++)
                sb.Append(input);
            return sb.ToString();
        }

        /// <summary>
        /// Ensures the string starts with the specified prefix.
        /// </summary>
        public static string EnsureStartsWith(this string? input, string prefix, StringComparison comparison = StringComparison.Ordinal)
        {
            if (string.IsNullOrEmpty(prefix)) return input ?? string.Empty;
            if (string.IsNullOrEmpty(input)) return prefix;
            return input.StartsWith(prefix, comparison) ? input : prefix + input;
        }

        /// <summary>
        /// Ensures the string ends with the specified suffix.
        /// </summary>
        public static string EnsureEndsWith(this string? input, string suffix, StringComparison comparison = StringComparison.Ordinal)
        {
            if (string.IsNullOrEmpty(suffix)) return input ?? string.Empty;
            if (string.IsNullOrEmpty(input)) return suffix;
            return input.EndsWith(suffix, comparison) ? input : input + suffix;
        }

        /// <summary>
        /// Returns null if the string is empty; otherwise returns the original string.
        /// </summary>
        public static string? NullIfEmpty(this string? input)
            => string.IsNullOrEmpty(input) ? null : input;

        /// <summary>
        /// Returns null if the string is null, empty, or whitespace; otherwise returns the original string.
        /// </summary>
        public static string? NullIfWhiteSpace(this string? input)
            => string.IsNullOrWhiteSpace(input) ? null : input;

        /// <summary>
        /// Returns the default value if the string is null, empty, or whitespace.
        /// </summary>
        public static string DefaultIfNullOrWhiteSpace(this string? input, string defaultValue)
            => string.IsNullOrWhiteSpace(input) ? defaultValue : input;

        /// <summary>
        /// Sanitizes a file name by replacing invalid file name characters.
        /// </summary>
        public static string SanitizeFileName(this string? fileName, char replacement = '_')
        {
            if (string.IsNullOrWhiteSpace(fileName)) return string.Empty;
            var invalidChars = System.IO.Path.GetInvalidFileNameChars();
            var sb = new StringBuilder(fileName.Length);
            foreach (var ch in fileName)
                sb.Append(Array.IndexOf(invalidChars, ch) >= 0 ? replacement : ch);
            return sb.ToString().Trim();
        }

        private static List<string> SplitIntoWords(string? input)
        {
            if (string.IsNullOrWhiteSpace(input))
                return new List<string>();

            return _wordSplitRegex
                .Split(input.Trim())
                .Where(w => !string.IsNullOrWhiteSpace(w))
                .ToList();
        }
    }
}
