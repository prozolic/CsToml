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

"u8.ToArray();

    }

    [Fact]
    public void DeserializeAndSerializeTest()
    {
        var package = new CsTomlPackage();
        CsTomlSerializer.Deserialize(ref package, tomlText);

        var serializeText = CsTomlSerializer.Serialize(ref package);
    }

    [Fact]
    public void TryGetValueTest()
    {
        var package = new CsTomlPackage();
        CsTomlSerializer.Deserialize(ref package, tomlText);

        {
            var result = package.TryGetValue("key"u8, out var value);
            Assert.True(result);
            Assert.Equal("value", value?.GetString());
        }
        {
            var result = package.TryGetValue("first.second.third"u8, out var value);
            Assert.True(result);
            Assert.Equal("value", value?.GetString());
        }
        {
            var result = package.TryGetValue("number"u8, out var value);
            Assert.True(result);
            Assert.Equal(123456, value?.GetInt64());
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
        var package = new CsTomlPackage();
        CsTomlSerializer.Deserialize(ref package, tomlText);

        {
            var value = package.Find(["key"u8]);
            Assert.Equal("value", value?.GetString());
        }
        {
            var value = package.Find(["first"u8, "second"u8, "third"u8]);
            Assert.Equal("value", value?.GetString());
        }
        {
            var value = package.Find(["number"u8]);
            Assert.Equal(123456, value?.GetInt64());
        }
        {
            var value = package.Find(["failed"u8]);
            Assert.Throws<CsTomlException>(() => { var _ = value.GetString(); });
        }
    }
}

