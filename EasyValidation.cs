using System;
using System.Net;
using System.Net.Sockets;
using System.Runtime.CompilerServices;
using System.Text.RegularExpressions;

namespace SkyWebFramework.Utility
{
    /// <summary>
    /// Validation and guard helpers for URLs, IPs, credit cards (Luhn), passwords, and parameter checks.
    /// </summary>
    public static class EasyValidation
    {
        private static readonly Regex _alphanumericRegex = new("^[a-zA-Z0-9]+$", RegexOptions.Compiled);
        private static readonly Regex _numericRegex = new(@"^-?\d+(\.\d+)?$", RegexOptions.Compiled);
        private static readonly Regex _hexColorRegex = new("^#(?:[0-9a-fA-F]{3}|[0-9a-fA-F]{6}|[0-9a-fA-F]{8})$", RegexOptions.Compiled);
        private static readonly Regex _slugRegex = new("^[a-z0-9]+(?:-[a-z0-9]+)*$", RegexOptions.Compiled);

        /// <summary>
        /// Checks whether a string is a well-formed HTTP or HTTPS URL.
        /// </summary>
        public static bool IsValidUrl(string? url, bool requireHttps = false)
        {
            if (string.IsNullOrWhiteSpace(url))
                return false;

            if (!Uri.TryCreate(url, UriKind.Absolute, out var uri))
                return false;

            return requireHttps
                ? uri.Scheme == Uri.UriSchemeHttps
                : uri.Scheme == Uri.UriSchemeHttp || uri.Scheme == Uri.UriSchemeHttps;
        }

        /// <summary>
        /// Checks whether a string is a valid IPv4 address.
        /// </summary>
        public static bool IsValidIPv4(string? ip)
            => !string.IsNullOrWhiteSpace(ip)
               && IPAddress.TryParse(ip, out var parsed)
               && parsed.AddressFamily == AddressFamily.InterNetwork;

        /// <summary>
        /// Checks whether a string is a valid IPv4 or IPv6 address.
        /// </summary>
        public static bool IsValidIPAddress(string? ip)
            => !string.IsNullOrWhiteSpace(ip) && IPAddress.TryParse(ip, out _);

        /// <summary>
        /// Validates a credit card number using the Luhn checksum algorithm (ignores spaces and hyphens).
        /// </summary>
        public static bool IsValidCreditCard(string? cardNumber)
        {
            if (string.IsNullOrWhiteSpace(cardNumber))
                return false;

            var digits = EasyString.ExtractDigits(cardNumber);
            if (digits.Length < 13 || digits.Length > 19)
                return false;

            int sum = 0;
            bool alternate = false;

            for (int i = digits.Length - 1; i >= 0; i--)
            {
                int n = digits[i] - '0';
                if (alternate)
                {
                    n *= 2;
                    if (n > 9) n -= 9;
                }
                sum += n;
                alternate = !alternate;
            }

            return sum % 10 == 0;
        }

        /// <summary>
        /// Checks whether a password meets configurable strength criteria.
        /// </summary>
        public static bool IsStrongPassword(
            string? password,
            int minLength = 8,
            bool requireUpper = true,
            bool requireLower = true,
            bool requireDigit = true,
            bool requireSpecial = true)
        {
            if (string.IsNullOrEmpty(password) || password.Length < minLength)
                return false;

            bool hasUpper = false, hasLower = false, hasDigit = false, hasSpecial = false;

            foreach (var c in password)
            {
                if (char.IsUpper(c)) hasUpper = true;
                else if (char.IsLower(c)) hasLower = true;
                else if (char.IsDigit(c)) hasDigit = true;
                else hasSpecial = true;
            }

            return (!requireUpper || hasUpper)
                && (!requireLower || hasLower)
                && (!requireDigit || hasDigit)
                && (!requireSpecial || hasSpecial);
        }

        /// <summary>
        /// Checks whether a string contains only ASCII alphanumeric characters.
        /// </summary>
        public static bool IsAlphanumeric(string? input)
            => !string.IsNullOrEmpty(input) && _alphanumericRegex.IsMatch(input);

        /// <summary>
        /// Checks whether a string represents a valid integer or decimal number.
        /// </summary>
        public static bool IsNumeric(string? input)
            => !string.IsNullOrWhiteSpace(input) && _numericRegex.IsMatch(input.Trim());

        /// <summary>
        /// Checks whether a string is a valid CSS hex color (#RGB, #RRGGBB, or #RRGGBBAA).
        /// </summary>
        public static bool IsValidHexColor(string? color)
            => !string.IsNullOrWhiteSpace(color) && _hexColorRegex.IsMatch(color.Trim());

        /// <summary>
        /// Checks whether an integer is a valid TCP/UDP port number (1-65535).
        /// </summary>
        public static bool IsValidPort(int port)
            => port is >= 1 and <= 65535;

        /// <summary>
        /// Checks whether a string is a valid lowercase URL slug.
        /// </summary>
        public static bool IsValidSlug(string? slug)
            => !string.IsNullOrWhiteSpace(slug) && _slugRegex.IsMatch(slug);

        /// <summary>
        /// Guard clause ensuring a string parameter is not null, empty, or whitespace.
        /// </summary>
        public static string RequireNotNullOrWhiteSpace(string? value, [CallerArgumentExpression(nameof(value))] string? paramName = null)
        {
            if (string.IsNullOrWhiteSpace(value))
                throw new ArgumentException("Value cannot be null, empty, or whitespace.", paramName);
            return value;
        }

        /// <summary>
        /// Guard clause ensuring a value falls within the inclusive range [<paramref name="min"/>, <paramref name="max"/>].
        /// </summary>
        public static T RequireInRange<T>(T value, T min, T max, [CallerArgumentExpression(nameof(value))] string? paramName = null)
            where T : IComparable<T>
        {
            if (value.CompareTo(min) < 0 || value.CompareTo(max) > 0)
                throw new ArgumentOutOfRangeException(paramName, value, $"Value must be between {min} and {max}.");
            return value;
        }
    }
}
