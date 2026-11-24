# ⚡ SkyWebFramework.Utilities

[![NuGet](https://img.shields.io/nuget/v/SkyWebFramework.Utilities?style=flat-square&color=blue)](https://www.nuget.org/packages/SkyWebFramework.Utilities)
[![Downloads](https://img.shields.io/nuget/dt/SkyWebFramework.Utilities?style=flat-square&color=green)](https://www.nuget.org/packages/SkyWebFramework.Utilities)
[![GitHub Repository](https://img.shields.io/badge/GitHub-SkyWebFramework.Utility-181717?style=flat-square&logo=github)](https://github.com/SkyDevLab/SkyWebFramework.Utility)
[![License: MIT](https://img.shields.io/badge/license-MIT-orange?style=flat-square)](https://github.com/SkyDevLab/SkyWebFramework.Utility/blob/main/LICENCE.txt)
[![.NET](https://img.shields.io/badge/.NET-6.0%20%7C%207.0%20%7C%208.0%20%7C%209.0-512BD4?style=flat-square)](https://dotnet.microsoft.com/)

**A comprehensive, zero-dependency .NET utility library for everyday development**

*String manipulation • Date handling • Security & encryption • Collection extensions • Math & conversions • Object mapping • JSON • Enums • Async resilience • Validation*

---

## 🔗 Repository & Links

- **GitHub Repository**: [SkyDevLab/SkyWebFramework.Utility](https://github.com/SkyDevLab/SkyWebFramework.Utility)
- **NuGet Package**: [SkyWebFramework.Utilities on NuGet.org](https://www.nuget.org/packages/SkyWebFramework.Utilities)

---

## 🎯 Why SkyWebFramework.Utilities?

Stop rewriting the same boilerplate helpers across every solution. **SkyWebFramework.Utilities** delivers **130+ battle-tested static helpers and extension methods** in a single lightweight package with **zero external dependencies**, full nullable reference type annotations, and multi-targeting across **.NET 6.0, 7.0, 8.0, and 9.0**.

```csharp
using SkyWebFramework.Utility;

// Fluent extension methods or static calls
var slug = "Hello World! 123".Slugify();                    // "hello-world-123"
var snake = "UserFirstName".ToSnakeCase();                  // "user_first_name"
var timeAgo = someDate.ToRelativeTime();                    // "5 minutes ago"
var dueDate = DateTime.Today.AddBusinessDays(3);            // Skips weekends
var hash = EasySecurity.HashPassword("S3cur3P@ss!");        // PBKDF2-SHA256 + CSPRNG salt
var cloned = order.DeepClone();                             // Deep clone via System.Text.Json
var json = order.ToJson(indented: true);                    // CamelCase, null-ignoring JSON
```

---

## 📦 Installation

Install via .NET CLI:

```bash
dotnet add package SkyWebFramework.Utilities
```

Or via Visual Studio Package Manager Console:

```powershell
Install-Package SkyWebFramework.Utilities
```

**Supported Frameworks:** `.NET 6.0`, `.NET 7.0`, `.NET 8.0`, `.NET 9.0`

---

## ✨ What's Included

| Class | Focus Area | Highlights |
| :--- | :--- | :--- |
| **`EasyString`** | String Manipulation & Formatting | `Slugify`, `ToSnakeCase`, `ToKebabCase`, `ToCamelCase`, `ToPascalCase`, `Mask`, `MaskEmail`, `StripHtml`, `Truncate`, `RemoveDiacritics`, `SanitizeFileName` |
| **`EasyDate`** | Date, Time & Business Days | `ToRelativeTime`, `AddBusinessDays`, `GetBusinessDays`, `GetQuarter`, `StartOfWeek`/`EndOfWeek`, `ToUnixTimestamp`, `FromUnixTimestamp`, `CalculateAge` |
| **`EasySecurity`** | Cryptography & Tokens | `HashPassword`/`VerifyPassword` (PBKDF2), `EncryptAes`/`DecryptAes` (AES-256), `Sha256Hash`, `Sha512Hash`, `HmacSha256`, `GenerateNumericOtp`, `ConstantTimeEquals` |
| **`EasyCollection`** | LINQ & Collection Extensions | `ToPagedResult` (`PagedResult<T>`), `Batch`, `Paginate`, `ForEachAsync`, `Shuffle`, `Sample`, `SafeAny`, `SafeFirstOrDefault`, `WhereNotNull`, `GetAtOrDefault` |
| **`EasyMath`** | Math, Stats & Unit Conversions | `Clamp`, `IsBetween`, `PercentageOf`, `RoundToNearest`, `Median`, `IsPrime`, `Factorial`, `Gcd`, `Lcm`, `Lerp`, `FormatFileSize`, Temp/Distance/Weight conversions |
| **`EasyObject`** | Object Mapping & Reflection | `DeepClone<T>`, `MapTo<TTarget>`, `CopyPropertiesTo`, `ToDictionary`, `GetPropertyValue<T>` (nested paths), `TryConvert<T>`, `ThrowIfNull` |
| **`EasyJson`** | JSON Helpers (`System.Text.Json`) | `ToJson`, `FromJson<T>`, `TryFromJson<T>`, `IsValidJson`, `PrettifyJson`, `MinifyJson`, `MergeJson` |
| **`EasyEnum`** | Enum Reflection & Flags | `GetDescription`, `GetValues<TEnum>`, `GetNames<TEnum>`, `ToDictionary<TEnum>`, `ParseOrDefault<TEnum>`, `GetFlags<TEnum>` |
| **`EasyAsync`** | Async Resilience & Concurrency | `RetryAsync` (exponential backoff), `WithTimeout`, `SafeFireAndForget`, `WhenAllLimited` |
| **`EasyValidation`** | Validators & Guard Clauses | `IsValidUrl`, `IsValidIPv4`, `IsValidCreditCard` (Luhn), `IsStrongPassword`, `IsValidHexColor`, `IsValidPort`, `RequireNotNullOrWhiteSpace`, `RequireInRange` |

---

## 🚀 Quick Start Examples

### 🔤 Strings & Validation (`EasyString` & `EasyValidation`)

```csharp
using SkyWebFramework.Utility;

var slug = "10 Tips for .NET Developers!".Slugify();       // "10-tips-for-net-developers"
var snake = "UserFirstName".ToSnakeCase();                  // "user_first_name"
var kebab = "UserFirstName".ToKebabCase();                  // "user-first-name"
var clean = "<p>Hello <b>World</b></p>".StripHtml();        // "Hello World"
var maskedEmail = "john.doe@example.com".MaskEmail();       // "j*******@example.com"
var safeFile = "report:2026?.pdf".SanitizeFileName();       // "report_2026_.pdf"

bool isCardValid = EasyValidation.IsValidCreditCard("4532 0151 1283 0366"); // Luhn check
bool isStrongPwd = EasyValidation.IsStrongPassword("Str0ng!Pass");
var apiKey = EasyValidation.RequireNotNullOrWhiteSpace(config["ApiKey"]);
```

### 📅 Dates & Business Days (`EasyDate`)

```csharp
using SkyWebFramework.Utility;

var dueDate = DateTime.UtcNow.AddBusinessDays(5);           // Skips Saturday/Sunday
var quarter = dueDate.GetQuarter();                         // 1..4
var startOfQ = dueDate.StartOfQuarter();
var endOfWeek = dueDate.EndOfWeek();
var unixSeconds = dueDate.ToUnixTimestamp();
var restoredUtc = EasyDate.FromUnixTimestamp(unixSeconds);
var label = DateTime.UtcNow.AddMinutes(-15).ToRelativeTime(); // "15 minutes ago"
```

### 🔐 Security, PBKDF2 & AES-256 (`EasySecurity`)

```csharp
using SkyWebFramework.Utility;

// PBKDF2-SHA256 password hashing with automatic 16-byte random salt
string storedHash = EasySecurity.HashPassword("MySecretPassword!");
bool isValid = EasySecurity.VerifyPassword("MySecretPassword!", storedHash);

// AES-256-CBC encryption with per-message random IV
string encrypted = EasySecurity.EncryptAes("Sensitive data", "encryption-secret");
string decrypted = EasySecurity.DecryptAes(encrypted, "encryption-secret");

// OTP, HMAC & constant-time comparison
string otp = EasySecurity.GenerateNumericOtp(6);
string signature = "webhook-body".HmacSha256("webhook-secret");
bool tokenMatch = EasySecurity.ConstantTimeEquals(expectedToken, providedToken);
```

### 📦 Collections & Pagination (`EasyCollection`)

```csharp
using SkyWebFramework.Utility;

var items = Enumerable.Range(1, 50);

// Rich pagination metadata
PagedResult<int> page2 = items.ToPagedResult(pageNumber: 2, pageSize: 10);
// page2.TotalPages == 5, page2.HasPreviousPage == true, page2.HasNextPage == true

// Random sampling & shuffling
var sample = items.Sample(5);

// Bounded async iteration
await items.ForEachAsync(async item => await ProcessAsync(item), maxDegreeOfParallelism: 4);
```

### 🔢 Math & Unit Conversions (`EasyMath`)

```csharp
using SkyWebFramework.Utility;

var clamped = 150.Clamp(0, 100);                            // 100
var size = EasyMath.FormatFileSize(1_048_576);              // "1 MB"
var median = EasyMath.Median(new double[] { 10, 1, 3, 4 }); // 3.5
var rounded = 13.7.RoundToNearest(5.0);                     // 15.0
var fahrenheit = EasyMath.CelsiusToFahrenheit(100);         // 212.0
```

### 🎯 Objects, JSON, Enums & Async (`EasyObject`, `EasyJson`, `EasyEnum`, `EasyAsync`)

```csharp
using SkyWebFramework.Utility;

// Deep clone, property mapping & nested path lookup
var clone = order.DeepClone();
var dto = order.MapTo<OrderDto>();
var city = order.GetPropertyValue<string>("Customer.Address.City");

// JSON formatting & deep object merge
var json = dto.ToJson(indented: true);
var merged = EasyJson.MergeJson("{\"a\":1,\"cfg\":{\"x\":1}}", "{\"cfg\":{\"y\":2}}");

// Enum descriptions & flags
string desc = OrderStatus.PendingApproval.GetDescription();
var flags = userPermissions.GetFlags();

// Async retry with exponential backoff & timeout
var data = await EasyAsync.RetryAsync(
    () => FetchFromApiAsync().WithTimeout(TimeSpan.FromSeconds(5)),
    maxRetries: 3,
    initialDelay: TimeSpan.FromMilliseconds(200)
);
```

---

## 📚 Complete API Reference

### 🔤 `EasyString`

| Method | Description |
| :--- | :--- |
| `Slugify(string?)` | Converts text to URL-safe slug (`"Hello!"` → `"hello"`) |
| `ToTitleCase(string?)` | Converts text to Title Case |
| `ToCamelCase(string?)` | Converts text/snake/kebab/Pascal into `camelCase` |
| `ToPascalCase(string?)` | Converts text/snake/kebab into `PascalCase` |
| `ToSnakeCase(string?)` | Converts text/camel/Pascal into `snake_case` |
| `ToKebabCase(string?)` | Converts text/camel/Pascal into `kebab-case` |
| `IsNullOrWhiteSpace(string?)` / `IsNullOrEmpty(string?)` | Extension null/whitespace checks |
| `RemoveDiacritics(string?)` | Strips accent marks (`"Café"` → `"Cafe"`) |
| `Truncate(string?, int, bool)` | Truncates string with optional `"..."` ellipsis |
| `ExtractDigits(string?)` | Extracts numeric digits only |
| `ExtractWords(string?)` / `WordCount(string?)` | Extracts whitespace-separated words / count |
| `ExtractLines(string?, bool)` | Splits lines (`\r\n`, `\r`, `\n`) |
| `RemoveWhitespace(string?)` / `CollapseWhitespace(string?)` | Removes or normalizes whitespace |
| `RemoveSpecialCharacters(string?, bool)` | Strips non-alphanumeric characters |
| `StripHtml(string?)` | Strips HTML tags |
| `Mask(string?, int, char)` / `MaskEmail(string?, char)` | Masks sensitive values or email local parts |
| `GetInitials(string?)` | Extracts uppercase initials (`"John Doe"` → `"JD"`) |
| `EqualsIgnoreCase(string?, string?)` / `ContainsIgnoreCase(string?, string?)` | Case-insensitive ordinal comparisons |
| `Reverse(string?)` / `Shuffle(string?)` / `Repeat(string?, int)` | Reverses, shuffles, or repeats strings |
| `EnsureStartsWith` / `EnsureEndsWith` | Guarantees prefix or suffix |
| `NullIfEmpty` / `NullIfWhiteSpace` / `DefaultIfNullOrWhiteSpace` | Null-handling helpers |
| `SanitizeFileName(string?, char)` | Replaces invalid file system characters |
| `IsValidEmail` / `IsValidPhone` / `IsValidUrl` | Format validation helpers |
| `GenerateRandomString(int, string)` | Thread-safe random string generator |

### 📅 `EasyDate`

| Method | Description |
| :--- | :--- |
| `UtcNowRoundedSeconds()` / `Minutes()` / `Hours()` | Rounded UTC timestamps |
| `ToLocal(DateTime)` / `ToUtc(DateTime)` | Timezone conversion helpers |
| `ToRelativeTime(DateTime)` | Past & future human-readable time (`"5 minutes ago"`, `"in 2 hours"`) |
| `ToIso8601(DateTime)` | Formats date as ISO 8601 string |
| `StartOfDay` / `EndOfDay` | Day boundaries |
| `StartOfWeek` / `EndOfWeek` | Week boundaries (configurable start day, default Monday) |
| `StartOfMonth` / `EndOfMonth` | Month boundaries |
| `GetQuarter` / `StartOfQuarter` / `EndOfQuarter` | Calendar quarter helpers |
| `StartOfYear` / `EndOfYear` | Year boundaries |
| `IsToday` / `IsPast` / `IsFuture` | Relative checks |
| `IsWeekend` / `IsBusinessDay` | Weekend vs weekday checks |
| `IsInRange` / `IsBetween` | Range validation (inclusive or exclusive) |
| `GetBusinessDays` / `AddBusinessDays` / `SubtractBusinessDays` | Working day calculations skipping weekends |
| `CalculateAge(DateTime)` | Full-year age calculation |
| `ToUnixTimestamp` / `ToUnixTimestampMilliseconds` | Convert `DateTime` to Unix epoch seconds/ms |
| `FromUnixTimestamp` / `FromUnixTimestampMilliseconds` | Convert Unix epoch seconds/ms to UTC `DateTime` |
| `IsLeapYear` / `DaysInMonth` / `NextDayOfWeek` | Calendar utilities |

### 🔐 `EasySecurity`

| Method | Description |
| :--- | :--- |
| `GenerateSecureRandomString(int, string)` | Unbiased CSPRNG random string |
| `GenerateSecurePassword(int, bool)` | CSPRNG password with guaranteed character class diversity |
| `GenerateNumericOtp(int)` | CSPRNG numeric OTP |
| `Sha256Hash` / `Sha512Hash` / `Md5Hash` | Lowercase hex digest helpers |
| `HmacSha256(string?, string)` | HMAC-SHA256 signature |
| `SimpleHash(string?)` | Deterministic fast non-cryptographic hash |
| `HashPassword(string, int)` / `VerifyPassword(string, string)` | PBKDF2-SHA256 salted password hashing & constant-time verification |
| `EncryptAes(string, string)` / `DecryptAes(string, string)` | AES-256-CBC encryption/decryption with random IV |
| `ToBase64` / `FromBase64` / `ToBase64Url` / `FromBase64Url` | Standard & URL-safe Base64 encoding |
| `NewGuidString(bool)` / `IsValidGuid(string?)` | GUID generation & validation |
| `ConstantTimeEquals(string?, string?)` | Timing-safe string equality |

### 📦 `EasyCollection`

| Method | Description |
| :--- | :--- |
| `GetValueOrDefault` | Safe dictionary lookup with fallback |
| `IsNullOrEmpty` / `HasItems` | Null-safe emptiness checks |
| `SafeAny` / `SafeFirstOrDefault` / `SafeCount` / `OrEmpty` | Null-safe LINQ helpers |
| `Batch(int)` | Splits sequence into chunks |
| `DistinctBy` | Static distinct by key selector |
| `ToDelimitedString(string)` | Joins elements with delimiter |
| `ForEach` / `ForEachAsync` | Synchronous (with index) & bounded-concurrency async iteration |
| `AddRange` / `AddIfNotNull` / `RemoveAll` | `ICollection<T>` mutation helpers |
| `Shuffle` / `Sample(int)` / `RandomItem` | Fisher-Yates randomization & sampling |
| `Paginate(int, int)` / `ToPagedResult(int, int)` | 1-based paging & `PagedResult<T>` metadata |
| `GetAtOrDefault` / `WhereNotNull` / `IndexOf` | Safe indexing, non-null filtering, predicate index lookup |

### 🔢 `EasyMath`

| Method | Description |
| :--- | :--- |
| `Clamp` / `IsInRange` / `IsBetween` | Value clamping and bounds checking |
| `Percentage` / `PercentageOf` / `AddPercentage` / `SubtractPercentage` | Percentage math |
| `FormatFileSize(long)` | Human-readable bytes (`B` through `EB`, supports negative values) |
| `RoundToSignificantDigits` / `RoundToNearest` / `RoundUp` / `RoundDown` | Precision rounding |
| `Min` / `Max` / `Average` / `Median` | Multi-argument statistics |
| `IsEven` / `IsOdd` / `IsPrime` / `Factorial` / `Gcd` / `Lcm` / `Lerp` | Number theory & interpolation |
| `DegreesToRadians` / `RadiansToDegrees` | Angle conversions |
| `CelsiusToFahrenheit` / `FahrenheitToCelsius` | Temperature conversions |
| `KilometersToMiles` / `MilesToKilometers` | Distance conversions |
| `KilogramsToPounds` / `PoundsToKilograms` | Weight conversions |

### 🎯 `EasyObject`, 📄 `EasyJson`, 🏷️ `EasyEnum`, ⚡ `EasyAsync` & ✅ `EasyValidation`

- **`EasyObject`**: `IsNull`, `IsNotNull`, `ThrowIfNull`, `OrDefault`, `IsDefault`, `DeepClone`, `MapTo`, `CopyPropertiesTo`, `ToDictionary`, `GetPropertyValue`, `TryConvert`, `ConvertTo`
- **`EasyJson`**: `ToJson`, `FromJson`, `TryFromJson`, `IsValidJson`, `PrettifyJson`, `MinifyJson`, `MergeJson`
- **`EasyEnum`**: `GetDescription`, `GetValues`, `GetNames`, `ToDictionary`, `ParseOrDefault`, `IsDefined`, `GetFlags`
- **`EasyAsync`**: `RetryAsync`, `WithTimeout`, `SafeFireAndForget`, `WhenAllLimited`
- **`EasyValidation`**: `IsValidUrl`, `IsValidIPv4`, `IsValidIPAddress`, `IsValidCreditCard`, `IsStrongPassword`, `IsAlphanumeric`, `IsNumeric`, `IsValidHexColor`, `IsValidPort`, `IsValidSlug`, `RequireNotNullOrWhiteSpace`, `RequireInRange`

---

## 🧪 Running Tests

The repository includes an xUnit test suite (`SkyWebFramework.Utility.Tests`) targeting both **.NET 8.0** and **.NET 9.0**:

```bash
dotnet test SkyWebFramework.Utility.sln -c Release
```

---

## 🤝 Contributing

Contributions are welcome!

1. Fork the repository
2. Create a feature branch (`git checkout -b feature/my-utility`)
3. Add tests in `SkyWebFramework.Utility.Tests` and run `dotnet test`
4. Open a pull request

---

## 📄 License

Licensed under the [MIT License](https://github.com/SkyDevLab/SkyWebFramework.Utility/blob/main/LICENCE.txt) — free for commercial and personal use.

**Copyright © 2026 Surya Pratap Singh**
