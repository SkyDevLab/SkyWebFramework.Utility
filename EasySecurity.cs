using System;
using System.IO;
using System.Security.Cryptography;
using System.Text;

namespace SkyWebFramework.Utility
{
    /// <summary>
    /// Security, hashing, encryption, and token generation utilities.
    /// </summary>
    public static class EasySecurity
    {
        private const int SaltSize = 16;
        private const int HashSize = 32;

        /// <summary>
        /// Generates a cryptographically secure random string without modulo bias.
        /// </summary>
        public static string GenerateSecureRandomString(int length, string characterSet = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789")
        {
            if (length <= 0) return string.Empty;
            if (string.IsNullOrEmpty(characterSet))
                throw new ArgumentException("Character set cannot be null or empty.", nameof(characterSet));

            var result = new char[length];
            for (int i = 0; i < length; i++)
            {
                result[i] = characterSet[RandomNumberGenerator.GetInt32(characterSet.Length)];
            }

            return new string(result);
        }

        /// <summary>
        /// Generates a cryptographically secure numeric one-time password (OTP).
        /// </summary>
        public static string GenerateNumericOtp(int digits = 6)
        {
            if (digits <= 0)
                throw new ArgumentOutOfRangeException(nameof(digits), "Digits must be positive.");

            return GenerateSecureRandomString(digits, "0123456789");
        }

        /// <summary>
        /// Generates a secure password with guaranteed character class diversity.
        /// </summary>
        public static string GenerateSecurePassword(int length = 12, bool requireSpecialChars = true)
        {
            var minLength = requireSpecialChars ? 4 : 3;
            if (length < minLength)
                throw new ArgumentOutOfRangeException(nameof(length), $"Password length must be at least {minLength}.");

            const string lowercase = "abcdefghijklmnopqrstuvwxyz";
            const string uppercase = "ABCDEFGHIJKLMNOPQRSTUVWXYZ";
            const string digits = "0123456789";
            const string special = "!@#$%^&*()-_=+[]{}|;:,.<>?";

            var characterSet = lowercase + uppercase + digits + (requireSpecialChars ? special : string.Empty);
            var password = new char[length];

            password[0] = lowercase[RandomNumberGenerator.GetInt32(lowercase.Length)];
            password[1] = uppercase[RandomNumberGenerator.GetInt32(uppercase.Length)];
            password[2] = digits[RandomNumberGenerator.GetInt32(digits.Length)];
            if (requireSpecialChars)
                password[3] = special[RandomNumberGenerator.GetInt32(special.Length)];

            for (int i = minLength; i < length; i++)
            {
                password[i] = characterSet[RandomNumberGenerator.GetInt32(characterSet.Length)];
            }

            // Fisher-Yates shuffle with CSPRNG
            for (int i = length - 1; i > 0; i--)
            {
                int j = RandomNumberGenerator.GetInt32(i + 1);
                (password[i], password[j]) = (password[j], password[i]);
            }

            return new string(password);
        }

        /// <summary>
        /// Creates a lowercase hex SHA-256 hash of the input string.
        /// </summary>
        public static string Sha256Hash(this string? input)
        {
            if (string.IsNullOrEmpty(input)) return string.Empty;

            var bytes = Encoding.UTF8.GetBytes(input);
            var hashBytes = SHA256.HashData(bytes);
            return Convert.ToHexString(hashBytes).ToLowerInvariant();
        }

        /// <summary>
        /// Creates a lowercase hex SHA-512 hash of the input string.
        /// </summary>
        public static string Sha512Hash(this string? input)
        {
            if (string.IsNullOrEmpty(input)) return string.Empty;

            var bytes = Encoding.UTF8.GetBytes(input);
            var hashBytes = SHA512.HashData(bytes);
            return Convert.ToHexString(hashBytes).ToLowerInvariant();
        }

        /// <summary>
        /// Creates a lowercase hex MD5 hash of the input string (for checksums/legacy compatibility, not passwords).
        /// </summary>
        public static string Md5Hash(this string? input)
        {
            if (string.IsNullOrEmpty(input)) return string.Empty;

            var bytes = Encoding.UTF8.GetBytes(input);
            var hashBytes = MD5.HashData(bytes);
            return Convert.ToHexString(hashBytes).ToLowerInvariant();
        }

        /// <summary>
        /// Computes a lowercase hex HMAC-SHA256 signature for the input using the specified secret key.
        /// </summary>
        public static string HmacSha256(this string? input, string secretKey)
        {
            if (string.IsNullOrEmpty(secretKey))
                throw new ArgumentException("Secret key cannot be null or empty.", nameof(secretKey));

            var keyBytes = Encoding.UTF8.GetBytes(secretKey);
            var dataBytes = Encoding.UTF8.GetBytes(input ?? string.Empty);
            using var hmac = new HMACSHA256(keyBytes);
            var hashBytes = hmac.ComputeHash(dataBytes);
            return Convert.ToHexString(hashBytes).ToLowerInvariant();
        }

        /// <summary>
        /// Creates a deterministic non-cryptographic hash for cache keys or quick lookups.
        /// </summary>
        public static int SimpleHash(this string? input)
        {
            if (string.IsNullOrEmpty(input)) return 0;

            unchecked
            {
                int hash = 17;
                foreach (char c in input)
                {
                    hash = hash * 31 + c;
                }
                return hash;
            }
        }

        /// <summary>
        /// Hashes a password using PBKDF2 (SHA-256) with a cryptographically random salt.
        /// </summary>
        public static string HashPassword(string password, int iterations = 100_000)
        {
            if (string.IsNullOrEmpty(password))
                throw new ArgumentException("Password cannot be null or empty.", nameof(password));
            if (iterations < 10_000)
                throw new ArgumentOutOfRangeException(nameof(iterations), "Iterations must be at least 10,000.");

            var salt = RandomNumberGenerator.GetBytes(SaltSize);
            using var pbkdf2 = new Rfc2898DeriveBytes(password, salt, iterations, HashAlgorithmName.SHA256);
            var hash = pbkdf2.GetBytes(HashSize);

            return $"{iterations}.{Convert.ToBase64String(salt)}.{Convert.ToBase64String(hash)}";
        }

        /// <summary>
        /// Verifies a password against a PBKDF2 hash produced by <see cref="HashPassword"/>.
        /// </summary>
        public static bool VerifyPassword(string password, string hashedPassword)
        {
            if (string.IsNullOrEmpty(password) || string.IsNullOrWhiteSpace(hashedPassword))
                return false;

            var parts = hashedPassword.Split('.');
            if (parts.Length != 3 || !int.TryParse(parts[0], out var iterations) || iterations <= 0)
                return false;

            try
            {
                var salt = Convert.FromBase64String(parts[1]);
                var expectedHash = Convert.FromBase64String(parts[2]);
                using var pbkdf2 = new Rfc2898DeriveBytes(password, salt, iterations, HashAlgorithmName.SHA256);
                var actualHash = pbkdf2.GetBytes(expectedHash.Length);

                return CryptographicOperations.FixedTimeEquals(actualHash, expectedHash);
            }
            catch (FormatException)
            {
                return false;
            }
        }

        /// <summary>
        /// Encrypts plain text using AES-256-CBC with a random IV and returns a Base64-encoded payload (IV + Ciphertext).
        /// </summary>
        public static string EncryptAes(string plainText, string key)
        {
            if (plainText is null) throw new ArgumentNullException(nameof(plainText));
            if (string.IsNullOrEmpty(key)) throw new ArgumentException("Key cannot be null or empty.", nameof(key));

            var aesKey = SHA256.HashData(Encoding.UTF8.GetBytes(key));

            using var aes = Aes.Create();
            aes.Key = aesKey;
            aes.Mode = CipherMode.CBC;
            aes.Padding = PaddingMode.PKCS7;
            aes.GenerateIV();

            using var ms = new MemoryStream();
            ms.Write(aes.IV, 0, aes.IV.Length);

            using (var encryptor = aes.CreateEncryptor())
            using (var cs = new CryptoStream(ms, encryptor, CryptoStreamMode.Write))
            using (var sw = new StreamWriter(cs, Encoding.UTF8))
            {
                sw.Write(plainText);
            }

            return Convert.ToBase64String(ms.ToArray());
        }

        /// <summary>
        /// Decrypts a Base64-encoded AES-256-CBC payload produced by <see cref="EncryptAes"/>.
        /// </summary>
        public static string DecryptAes(string cipherTextBase64, string key)
        {
            if (string.IsNullOrEmpty(cipherTextBase64)) throw new ArgumentException("Cipher text cannot be null or empty.", nameof(cipherTextBase64));
            if (string.IsNullOrEmpty(key)) throw new ArgumentException("Key cannot be null or empty.", nameof(key));

            var fullCipher = Convert.FromBase64String(cipherTextBase64);
            var aesKey = SHA256.HashData(Encoding.UTF8.GetBytes(key));

            using var aes = Aes.Create();
            var ivLength = aes.BlockSize / 8;
            if (fullCipher.Length < ivLength)
                throw new CryptographicException("Invalid cipher text payload.");

            var iv = new byte[ivLength];
            Buffer.BlockCopy(fullCipher, 0, iv, 0, ivLength);

            aes.Key = aesKey;
            aes.IV = iv;
            aes.Mode = CipherMode.CBC;
            aes.Padding = PaddingMode.PKCS7;

            using var ms = new MemoryStream(fullCipher, ivLength, fullCipher.Length - ivLength);
            using var decryptor = aes.CreateDecryptor();
            using var cs = new CryptoStream(ms, decryptor, CryptoStreamMode.Read);
            using var sr = new StreamReader(cs, Encoding.UTF8);

            return sr.ReadToEnd();
        }

        /// <summary>
        /// Encodes a UTF-8 string to Base64.
        /// </summary>
        public static string ToBase64(this string? input)
        {
            if (string.IsNullOrEmpty(input)) return string.Empty;
            return Convert.ToBase64String(Encoding.UTF8.GetBytes(input));
        }

        /// <summary>
        /// Decodes a Base64 string back to a UTF-8 string.
        /// </summary>
        public static string FromBase64(this string? base64)
        {
            if (string.IsNullOrEmpty(base64)) return string.Empty;
            var bytes = Convert.FromBase64String(base64);
            return Encoding.UTF8.GetString(bytes);
        }

        /// <summary>
        /// Encodes a byte array into a URL-safe Base64 string without padding.
        /// </summary>
        public static string ToBase64Url(this byte[] bytes)
        {
            if (bytes is null || bytes.Length == 0) return string.Empty;
            return Convert.ToBase64String(bytes)
                .Replace('+', '-')
                .Replace('/', '_')
                .TrimEnd('=');
        }

        /// <summary>
        /// Decodes a URL-safe Base64 string back to a byte array.
        /// </summary>
        public static byte[] FromBase64Url(this string? base64Url)
        {
            if (string.IsNullOrEmpty(base64Url)) return Array.Empty<byte>();
            var padded = base64Url.Replace('-', '+').Replace('_', '/');
            switch (padded.Length % 4)
            {
                case 2: padded += "=="; break;
                case 3: padded += "="; break;
            }
            return Convert.FromBase64String(padded);
        }

        /// <summary>
        /// Generates a new GUID formatted string (with or without hyphens).
        /// </summary>
        public static string NewGuidString(bool withoutHyphens = false)
            => Guid.NewGuid().ToString(withoutHyphens ? "N" : "D");

        /// <summary>
        /// Checks whether the input string is a valid GUID.
        /// </summary>
        public static bool IsValidGuid(this string? input)
            => !string.IsNullOrWhiteSpace(input) && Guid.TryParse(input, out _);

        /// <summary>
        /// Compares two strings in constant time to help mitigate timing side-channel attacks.
        /// </summary>
        public static bool ConstantTimeEquals(string? a, string? b)
        {
            if (a is null || b is null)
                return a is null && b is null;

            var bytesA = Encoding.UTF8.GetBytes(a);
            var bytesB = Encoding.UTF8.GetBytes(b);
            return CryptographicOperations.FixedTimeEquals(bytesA, bytesB);
        }
    }
}
