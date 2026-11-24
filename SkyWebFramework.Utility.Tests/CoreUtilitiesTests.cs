using System;
using System.Collections.Generic;
using System.Linq;
using SkyWebFramework.Utility;
using Xunit;

namespace SkyWebFramework.Utility.Tests
{
    public class CoreUtilitiesTests
    {
        [Fact]
        public void EasyString_TransformationsAndExtraction_WorkAsExtensionAndStatic()
        {
            Assert.Equal("hello-world-123", EasyString.Slugify("Hello World! 123"));
            Assert.Equal("hello-world-123", "Hello World! 123".Slugify());
            Assert.Equal("Cafe", "Café".RemoveDiacritics());
            Assert.Equal("helloWorld", "Hello World".ToCamelCase());
            Assert.Equal("HelloWorld", "hello_world".ToPascalCase());
            Assert.Equal("hello_world", "HelloWorld".ToSnakeCase());
            Assert.Equal("hello-world", "HelloWorld".ToKebabCase());
            Assert.Equal("Hello World", "hello world".ToTitleCase());
            Assert.Equal("Long te...", "Long text here".Truncate(10, addEllipsis: true));
            Assert.Equal("12345", "abc123def45".ExtractDigits());
            Assert.Equal("******7890", "1234567890".Mask(4));
            Assert.Equal("j***@example.com", "john@example.com".MaskEmail());
            Assert.Equal("JMD", "John Michael Doe".GetInitials());
            Assert.Equal("helloworld", "hello   world\t\n".RemoveWhitespace());
            Assert.Equal("hello world", "  hello   world  ".CollapseWhitespace());
            Assert.Equal("Hello 123", "Hello! @123#".RemoveSpecialCharacters(allowSpaces: true));
            Assert.Equal("Hello123", "Hello! @123#".RemoveSpecialCharacters(allowSpaces: false));
            Assert.Equal("Hello World", "<p>Hello <b>World</b></p>".StripHtml());
            Assert.Equal(new[] { "one", "two", "three" }, "one  two\nthree".ExtractWords());
            Assert.Equal(3, "one two three".WordCount());
            Assert.Equal(new[] { "line1", "line2" }, "line1\r\n\r\nline2".ExtractLines());
            Assert.Equal("cba", "abc".Reverse());
            Assert.Equal("ababab", "ab".Repeat(3));
            Assert.Equal("/api/users", "api/users".EnsureStartsWith("/"));
            Assert.Equal("file.json", "file".EnsureEndsWith(".json"));
            Assert.Null("".NullIfEmpty());
            Assert.Null("   ".NullIfWhiteSpace());
            Assert.Equal("fallback", "  ".DefaultIfNullOrWhiteSpace("fallback"));
            Assert.True("Hello".ContainsIgnoreCase("ELL"));
            Assert.True("Hello".EqualsIgnoreCase("hello"));
            Assert.True("user@example.com".IsValidEmail());
            Assert.True("+1 (555) 123-4567".IsValidPhone());
            Assert.True("https://example.com".IsValidUrl());
            Assert.Equal("report_2026_.pdf", "report:2026?.pdf".SanitizeFileName());
            Assert.Equal(10, EasyString.GenerateRandomString(10).Length);
            Assert.Equal(5, "hello".Shuffle().Length);
        }

        [Fact]
        public void EasyDate_CalculationsAndConversions_WorkProperly()
        {
            var friday = new DateTime(2026, 3, 6, 14, 30, 0, DateTimeKind.Utc); // Friday
            Assert.True(friday.IsBusinessDay());
            Assert.False(friday.IsWeekend());

            var nextMonday = friday.AddBusinessDays(1);
            Assert.Equal(new DateTime(2026, 3, 9, 14, 30, 0, DateTimeKind.Utc), nextMonday);
            Assert.Equal(friday, nextMonday.SubtractBusinessDays(1));
            Assert.Equal(2, EasyDate.GetBusinessDays(friday, nextMonday));

            Assert.Equal(1, friday.GetQuarter());
            Assert.Equal(new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc), friday.StartOfQuarter());
            Assert.Equal(new DateTime(2026, 3, 31, 23, 59, 59, 999, DateTimeKind.Utc).Date, friday.EndOfQuarter().Date);
            Assert.Equal(new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc), friday.StartOfYear());
            Assert.Equal(new DateTime(2026, 12, 31), friday.EndOfYear().Date);

            Assert.Equal(new DateTime(2026, 3, 2), friday.StartOfWeek());
            Assert.Equal(new DateTime(2026, 3, 8), friday.EndOfWeek().Date);
            Assert.True(friday.IsInRange(new DateTime(2026, 3, 1, 0, 0, 0, DateTimeKind.Utc), new DateTime(2026, 3, 31, 0, 0, 0, DateTimeKind.Utc)));
            Assert.True(friday.IsBetween(friday.AddDays(-1), friday.AddDays(1), inclusive: false));

            var epoch = new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc);
            var ts = epoch.ToUnixTimestamp();
            Assert.Equal(epoch, EasyDate.FromUnixTimestamp(ts));
            var tsMs = epoch.ToUnixTimestampMilliseconds();
            Assert.Equal(epoch, EasyDate.FromUnixTimestampMilliseconds(tsMs));

            Assert.Contains("ago", DateTime.UtcNow.AddMinutes(-5).ToRelativeTime());
            Assert.Contains("in ", DateTime.UtcNow.AddMinutes(5).ToRelativeTime());
            Assert.False(new DateTime(2026, 1, 1).IsLeapYear());
            Assert.True(new DateTime(2024, 1, 1).IsLeapYear());
            Assert.Equal(29, new DateTime(2024, 2, 1).DaysInMonth());
            Assert.Equal(new DateTime(2026, 3, 9), friday.NextDayOfWeek(DayOfWeek.Monday));
        }

        [Fact]
        public void EasySecurity_HashingEncryptionAndTokens_WorkProperly()
        {
            Assert.Equal(64, "hello".Sha256Hash().Length);
            Assert.Equal(128, "hello".Sha512Hash().Length);
            Assert.Equal(32, "hello".Md5Hash().Length);
            Assert.Equal(64, "payload".HmacSha256("secret").Length);
            Assert.Equal("abc".SimpleHash(), EasySecurity.SimpleHash("abc"));

            var passwordHash = EasySecurity.HashPassword("S3cur3P@ss!", iterations: 10_000);
            Assert.True(EasySecurity.VerifyPassword("S3cur3P@ss!", passwordHash));
            Assert.False(EasySecurity.VerifyPassword("WrongPass", passwordHash));

            var encrypted = EasySecurity.EncryptAes("Sensitive Payload 123", "my-super-secret-key");
            Assert.NotEqual("Sensitive Payload 123", encrypted);
            Assert.Equal("Sensitive Payload 123", EasySecurity.DecryptAes(encrypted, "my-super-secret-key"));

            var b64 = "Hello World!".ToBase64();
            Assert.Equal("Hello World!", b64.FromBase64());

            var rawBytes = new byte[] { 1, 2, 254, 255 };
            Assert.Equal(rawBytes, rawBytes.ToBase64Url().FromBase64Url());

            Assert.True(EasySecurity.NewGuidString().IsValidGuid());
            Assert.Equal(32, EasySecurity.NewGuidString(withoutHyphens: true).Length);
            Assert.True(EasySecurity.ConstantTimeEquals("token-abc", "token-abc"));
            Assert.False(EasySecurity.ConstantTimeEquals("token-abc", "token-abd"));

            var otp = EasySecurity.GenerateNumericOtp(6);
            Assert.Equal(6, otp.Length);
            Assert.All(otp, c => Assert.True(char.IsDigit(c)));

            var genPwd = EasySecurity.GenerateSecurePassword(16, requireSpecialChars: true);
            Assert.Equal(16, genPwd.Length);
            Assert.True(EasyValidation.IsStrongPassword(genPwd, 16));
        }

        [Fact]
        public void EasyCollection_ExtensionsAndPagination_WorkProperly()
        {
            IDictionary<string, int> dict = new Dictionary<string, int> { ["a"] = 1 };
            var concreteDict = new Dictionary<string, int> { ["a"] = 1 };
            Assert.Equal(1, dict.GetValueOrDefault("a", 99));
            Assert.Equal(99, dict.GetValueOrDefault("missing", 99));
            Assert.Equal(1, EasyCollection.GetValueOrDefault(concreteDict, "a", 99));
            Assert.Equal(new[] { 1, 2 }, EasyCollection.DistinctBy(new[] { 1, 2, 1 }, x => x));

            IEnumerable<int>? nullSeq = null;
            Assert.True(nullSeq.IsNullOrEmpty());
            Assert.False(nullSeq.HasItems());
            Assert.False(nullSeq.SafeAny());
            Assert.Equal(42, nullSeq.SafeFirstOrDefault(defaultValue: 42));
            Assert.Equal(0, nullSeq.SafeCount());
            Assert.Empty(nullSeq.OrEmpty());

            var numbers = Enumerable.Range(1, 10).ToList();
            Assert.Equal(4, numbers.Batch(3).Count());
            Assert.Equal("1, 2, 3", numbers.Take(3).ToDelimitedString(", "));

            var paged = numbers.ToPagedResult(pageNumber: 2, pageSize: 3);
            Assert.Equal(new[] { 4, 5, 6 }, paged.Items);
            Assert.Equal(4, paged.TotalPages);
            Assert.True(paged.HasPreviousPage);
            Assert.True(paged.HasNextPage);

            Assert.Equal(2, numbers.GetAtOrDefault(1));
            Assert.Equal(-1, numbers.GetAtOrDefault(99, -1));
            Assert.Equal(4, numbers.IndexOf(x => x == 5));

            var list = new List<string> { "a", "b" };
            list.AddRange(new[] { "c", "d" });
            list.AddIfNotNull(null);
            Assert.Equal(4, list.Count);
            Assert.Equal(2, list.RemoveAll(x => x is "b" or "c"));
            Assert.Equal(new[] { "a", "d" }, list);

            var nullableItems = new string?[] { "x", null, "y" };
            Assert.Equal(new[] { "x", "y" }, nullableItems.WhereNotNull());

            Assert.Equal(10, numbers.Shuffle().Count);
            Assert.Equal(3, numbers.Sample(3).Count);
            Assert.Contains(numbers.RandomItem(), numbers);
        }

        [Fact]
        public void EasyMath_MathAndConversions_WorkProperly()
        {
            Assert.Equal(100, 150.Clamp(0, 100));
            Assert.True(50.IsInRange(0, 100));
            Assert.True(50.IsBetween(0, 100, inclusive: false));
            Assert.Equal(12.5, EasyMath.Percentage(25, 200));
            Assert.Equal(30, EasyMath.PercentageOf(15, 200));
            Assert.Equal(115, EasyMath.AddPercentage(100, 15));
            Assert.Equal(85, EasyMath.SubtractPercentage(100, 15));
            Assert.Equal("1 MB", EasyMath.FormatFileSize(1048576));
            Assert.Equal("-1 KB", EasyMath.FormatFileSize(-1024));
            Assert.Equal(12300, EasyMath.RoundToSignificantDigits(12345.6789, 3));
            Assert.Equal(15.0, 13.7.RoundToNearest(5.0));
            Assert.Equal(1.24, 1.231.RoundUp(2));
            Assert.Equal(1.23, 1.239.RoundDown(2));
            Assert.Equal(1, EasyMath.Min(5, 1, 9));
            Assert.Equal(9, EasyMath.Max(5, 1, 9));
            Assert.Equal(5.0, EasyMath.Average(2, 5, 8));
            Assert.Equal(3.5, EasyMath.Median(new double[] { 10, 1, 3, 4 }));
            Assert.True(4.IsEven());
            Assert.True(5.IsOdd());
            Assert.True(29L.IsPrime());
            Assert.False(28L.IsPrime());
            Assert.Equal(120, EasyMath.Factorial(5));
            Assert.Equal(6, EasyMath.Gcd(54, 24));
            Assert.Equal(12, EasyMath.Lcm(4, 6));
            Assert.Equal(50.0, EasyMath.Lerp(0, 100, 0.5));
            Assert.Equal(212.0, EasyMath.CelsiusToFahrenheit(100), 6);
            Assert.Equal(100.0, EasyMath.FahrenheitToCelsius(212), 6);
        }
    }
}
