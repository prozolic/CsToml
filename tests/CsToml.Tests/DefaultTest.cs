using CsToml.Error;

namespace CsToml.Tests;

public class DefaultTest
{
    private readonly byte[] tomlText;

    public DefaultTest()
    {
        tomlText = @"
key = ""value""
first.second.third = ""value""
number = 123456

[Table.test]
key = ""value""
first.second.third = ""value""
number = 123456

[[arrayOfTables.test]]
key = ""value""
first.second.third = ""value""
number = 123456

[[arrayOfTables.test]]

[[arrayOfTables.test]]
key2 = ""value""
first2.second2.third2 = ""value""
number2 = 123456

"u8.ToArray();

    }

    [Fact]
    public void DeserializeAndSerializeTest()
    {
        var package = CsTomlSerializer.Deserialize<CsTomlPackage>(tomlText);

        var serializeText = CsTomlSerializer.Serialize(package!);
    }

    [Fact]
    public void TryGetValueTest()
    {
        var package = CsTomlSerializer.Deserialize<CsTomlPackage>(tomlText);

        {
            var result = package!.TryGetValue("key"u8, out var value);
            Assert.True(result);
            Assert.Equal("value", value?.GetString());
        }
        {
            var result = package.TryGetValue("first.second.third"u8, out var value, CsTomlPackageOptions.DottedKeys);
            Assert.True(result);
            Assert.Equal("value", value?.GetString());
        }
        {
            var result = package.TryGetValue("key", out var value);
            Assert.True(result);
            Assert.Equal("value", value?.GetString());
        }
        {
            var result = package.TryGetValue("first.second.third", out var value, CsTomlPackageOptions.DottedKeys);
            Assert.True(result);
            Assert.Equal("value", value?.GetString());
        }
        {
            var result = package.TryGetValue(["first"u8,"second"u8,"third"u8], out var value, CsTomlPackageOptions.DottedKeys);
            Assert.True(result);
            Assert.Equal("value", value?.GetString());
        }
        {
            var result = package.TryGetValue("number"u8, out var value);
            Assert.True(result);
            Assert.Equal(123456, value?.GetInt64());
        }
        {
            var result = package.TryGetValue("Table.test"u8, "key"u8, out var value, CsTomlPackageOptions.DottedKeys);
            Assert.True(result);
            Assert.Equal("value", value?.GetString());
        }
        {
            var result = package.TryGetValue("arrayOfTables.test"u8, 0, "key"u8, out var value, CsTomlPackageOptions.DottedKeys);
            Assert.True(result);
            Assert.Equal("value", value?.GetString());
        }
        {
            var result = package.TryGetValue("failed"u8, out var value);
            Assert.False(result);
            Assert.Null(value);
        }

    }

    [Fact]
    public void FindTest()
    {
        var package = CsTomlSerializer.Deserialize<CsTomlPackage>(tomlText)!;

        {
            var value = package.Find("key"u8);
            Assert.Equal("value", value?.GetString());
        }
        {
            var value = package.Find("first.second.third"u8, CsTomlPackageOptions.DottedKeys);
            Assert.Equal("value", value?.GetString());
        }
        {
            var value = package.Find("key");
            Assert.Equal("value", value?.GetString());
        }
        {
            var value = package.Find("first.second.third",CsTomlPackageOptions.DottedKeys);
            Assert.Equal("value", value?.GetString());
        }
        {
            var value = package.Find(["first"u8, "second"u8, "third"u8], CsTomlPackageOptions.DottedKeys);
            Assert.Equal("value", value?.GetString());
        }
        {
            var value = package.Find("number"u8);
            Assert.Equal(123456, value?.GetInt64());
        }
        {
            var value = package.Find("Table.test"u8, "key"u8, CsTomlPackageOptions.DottedKeys);
            Assert.Equal("value", value?.GetString());
        }
        {
            var value = package.Find("arrayOfTables.test"u8, 0, "key"u8, CsTomlPackageOptions.DottedKeys);
            Assert.Equal("value", value?.GetString());
        }
        {
            var value = package.Find("failed"u8);
            Assert.Null(value);
        }

    }
}

