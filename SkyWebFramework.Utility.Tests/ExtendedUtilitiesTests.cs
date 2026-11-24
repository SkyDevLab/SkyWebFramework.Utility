using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Threading.Tasks;
using SkyWebFramework.Utility;
using Xunit;

namespace SkyWebFramework.Utility.Tests
{
    public class ExtendedUtilitiesTests
    {
        private class SourceModel
        {
            public string Name { get; set; } = string.Empty;
            public int Age { get; set; }
            public AddressModel? Address { get; set; }
        }

        private class AddressModel
        {
            public string City { get; set; } = string.Empty;
        }

        private class TargetModel
        {
            public string Name { get; set; } = string.Empty;
            public string Age { get; set; } = string.Empty;
        }

        private enum SampleStatus
        {
            [Description("Pending Approval")]
            Pending = 1,
            Approved = 2
        }

        [Flags]
        private enum SamplePermissions
        {
            None = 0,
            Read = 1,
            Write = 2,
            Execute = 4
        }

        [Fact]
        public void EasyObject_CloneMapAndPropertyAccess_WorkProperly()
        {
            var src = new SourceModel
            {
                Name = "Alice",
                Age = 30,
                Address = new AddressModel { City = "Seattle" }
            };

            Assert.True(src.IsNotNull());
            Assert.False(src.IsNull());
            Assert.Same(src, src.ThrowIfNull());
            Assert.Throws<ArgumentNullException>(() => ((SourceModel?)null).ThrowIfNull());
            Assert.True(0.IsDefault());

            var cloned = src.DeepClone();
            Assert.NotNull(cloned);
            Assert.NotSame(src, cloned);
            Assert.Equal("Seattle", cloned!.Address!.City);

            var mapped = src.MapTo<TargetModel>();
            Assert.Equal("Alice", mapped.Name);
            Assert.Equal("30", mapped.Age);

            var dict = src.ToDictionary();
            Assert.Equal("Alice", dict["name"]);

            Assert.Equal("Seattle", src.GetPropertyValue<string>("Address.City"));
            Assert.Equal("Unknown", src.GetPropertyValue("Address.Missing", "Unknown"));
            Assert.Equal(42, "42".ConvertTo<int>());
        }

        [Fact]
        public void EasyJson_SerializeDeserializeAndMerge_WorkProperly()
        {
            var model = new AddressModel { City = "Austin" };
            var json = model.ToJson();
            Assert.True(json.IsValidJson());
            Assert.Contains("\"city\":\"Austin\"", json);

            var parsed = json.FromJson<AddressModel>();
            Assert.Equal("Austin", parsed?.City);

            Assert.True(json.TryFromJson<AddressModel>(out var tryParsed));
            Assert.Equal("Austin", tryParsed?.City);
            Assert.False("{invalid".TryFromJson<AddressModel>(out _));

            var pretty = json.PrettifyJson();
            Assert.Contains("\n", pretty);
            Assert.Equal("{\"city\":\"Austin\"}", pretty.MinifyJson());

            var merged = EasyJson.MergeJson(
                "{\"name\":\"App\",\"nested\":{\"a\":1,\"b\":2}}",
                "{\"nested\":{\"b\":99,\"c\":3}}"
            );
            Assert.Contains("\"a\":1", merged);
            Assert.Contains("\"b\":99", merged);
            Assert.Contains("\"c\":3", merged);
        }

        [Fact]
        public void EasyEnum_Helpers_WorkProperly()
        {
            Assert.Equal("Pending Approval", SampleStatus.Pending.GetDescription());
            Assert.Equal("Approved", SampleStatus.Approved.GetDescription());
            Assert.Equal(2, EasyEnum.GetValues<SampleStatus>().Count);
            Assert.Contains("Pending", EasyEnum.GetNames<SampleStatus>());

            var dict = EasyEnum.ToDictionary<SampleStatus>(useDescription: true);
            Assert.Equal("Pending Approval", dict[1]);
            Assert.Equal(SampleStatus.Approved, EasyEnum.ParseOrDefault("approved", SampleStatus.Pending));
            Assert.Equal(SampleStatus.Pending, EasyEnum.ParseOrDefault("invalid", SampleStatus.Pending));
            Assert.True(SampleStatus.Pending.IsDefined());

            var perms = SamplePermissions.Read | SamplePermissions.Execute;
            Assert.Equal(new[] { SamplePermissions.Read, SamplePermissions.Execute }, perms.GetFlags());
        }

        [Fact]
        public async Task EasyAsync_RetryTimeoutAndLimitedConcurrency_WorkProperly()
        {
            int attempts = 0;
            var result = await EasyAsync.RetryAsync(async () =>
            {
                await Task.Yield();
                attempts++;
                if (attempts < 3)
                    throw new InvalidOperationException("Transient");
                return "ok";
            }, maxRetries: 3, initialDelay: TimeSpan.FromMilliseconds(5));

            Assert.Equal("ok", result);
            Assert.Equal(3, attempts);

            await Assert.ThrowsAsync<TimeoutException>(async () =>
                await Task.Delay(200).WithTimeout(TimeSpan.FromMilliseconds(20)));

            var fast = await Task.FromResult(123).WithTimeout(TimeSpan.FromSeconds(1));
            Assert.Equal(123, fast);

            var factories = new List<Func<Task<int>>>();
            for (int i = 1; i <= 5; i++)
            {
                int captured = i;
                factories.Add(async () =>
                {
                    await Task.Delay(5);
                    return captured * 2;
                });
            }

            var results = await EasyAsync.WhenAllLimited(factories, maxConcurrency: 2);
            Assert.Equal(new[] { 2, 4, 6, 8, 10 }, results);
        }

        [Fact]
        public void EasyValidation_RulesAndGuards_WorkProperly()
        {
            Assert.True(EasyValidation.IsValidUrl("https://example.com/path"));
            Assert.False(EasyValidation.IsValidUrl("http://example.com", requireHttps: true));
            Assert.True(EasyValidation.IsValidIPv4("192.168.1.1"));
            Assert.False(EasyValidation.IsValidIPv4("::1"));
            Assert.True(EasyValidation.IsValidIPAddress("::1"));
            Assert.True(EasyValidation.IsValidCreditCard("4532 0151 1283 0366"));
            Assert.False(EasyValidation.IsValidCreditCard("4532 0151 1283 0367"));
            Assert.True(EasyValidation.IsStrongPassword("Str0ng!Pass"));
            Assert.False(EasyValidation.IsStrongPassword("weak"));
            Assert.True(EasyValidation.IsAlphanumeric("Abc123"));
            Assert.True(EasyValidation.IsNumeric("-42.5"));
            Assert.True(EasyValidation.IsValidHexColor("#FF5733"));
            Assert.True(EasyValidation.IsValidPort(8080));
            Assert.False(EasyValidation.IsValidPort(70000));
            Assert.True(EasyValidation.IsValidSlug("hello-world-2026"));
            Assert.Equal("ok", EasyValidation.RequireNotNullOrWhiteSpace("ok"));
            Assert.Throws<ArgumentException>(() => EasyValidation.RequireNotNullOrWhiteSpace(" "));
            Assert.Equal(5, EasyValidation.RequireInRange(5, 1, 10));
            Assert.Throws<ArgumentOutOfRangeException>(() => EasyValidation.RequireInRange(20, 1, 10));
        }
    }
}
