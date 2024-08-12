
using System.Text;

namespace CsToml.Generator.Tests;

public class DefaultTest
{
    [Fact]
    public void SerializeTest()
    {
        var part = new TestPackagePart();
        using var tomlBytes = CsTomlSerializer.Serialize(part);

        var text = """
IntValue = 123
StringValue = "TestPackagePart"

""";
        if (OperatingSystem.IsWindows())
        {
            Assert.Equal(text, Encoding.UTF8.GetString(tomlBytes.ByteSpan));
        }
        else
        {
            Assert.Equal(text.Replace("\r\n", "\n"), Encoding.UTF8.GetString(tomlBytes.ByteSpan).Replace("\r\n", "\n"));
        }
        var document = CsTomlSerializer.Deserialize<TomlDocument>(tomlBytes.ByteSpan);
        Assert.Equal(part.IntValue, document!.Find("IntValue"u8)!.GetInt64());
        Assert.Equal(part.StringValue, document!.Find("StringValue"u8)!.GetString());
    }

    [Fact]
    public void SerializeTest2()
    {
        var part = new TestPackagePart2();
        using var tomlBytes = CsTomlSerializer.Serialize(part);

        var document = CsTomlSerializer.Deserialize<TomlDocument>(tomlBytes.ByteSpan);
        Assert.Equal(part.IntValue, document!.Find("IntValue"u8)!.GetNumber<uint>());
        Assert.Equal(part.LongValue, document!.Find("LongValue"u8)!.GetInt64());
        Assert.Equal(part.boolValue, document!.Find("boolValue"u8)!.GetBool());
        Assert.Equal(part.DoubleValue, document!.Find("DoubleValue"u8)!.GetDouble());
        Assert.Equal(part.StringValue, document!.Find("StringValue"u8)!.GetString());
    }
}
