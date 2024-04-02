

using Newtonsoft.Json.Linq;
using System.Text;

namespace CsToml.Generator.Tests;

public class DefaultTest
{
    [Fact]
    public void SerializeTest()
    {
        var part = new TestPackagePart();
        using var tomlBytes = CsTomlSerializer.Serialize(ref part, CsTomlSerializerOptions.CreateOptions(newLine: NewLineOption.Default));

        Assert.Equal("""
IntValue = 123
StringValue = "TestPackagePart"

""", Encoding.UTF8.GetString(tomlBytes.ByteSpan));

        var package = CsTomlSerializer.Deserialize<CsTomlPackage>(tomlBytes.ByteSpan);
        Assert.Equal(part.IntValue, package!.Find("IntValue"u8)!.GetInt64());
        Assert.Equal(part.StringValue, package!.Find("StringValue"u8)!.GetString());
    }

    [Fact]
    public void SerializeTest2()
    {
        var part = new TestPackagePart2();
        using var tomlBytes = CsTomlSerializer.Serialize(ref part, CsTomlSerializerOptions.CreateOptions(newLine: NewLineOption.CrLf));

        var package = CsTomlSerializer.Deserialize<CsTomlPackage>(tomlBytes.ByteSpan);
        Assert.Equal(part.IntValue, package!.Find("IntValue"u8)!.GetNumber<uint>());
        Assert.Equal(part.LongValue, package!.Find("LongValue"u8)!.GetInt64());
        Assert.Equal(part.boolValue, package!.Find("boolValue"u8)!.GetBool());
        Assert.Equal(part.DoubleValue, package!.Find("DoubleValue"u8)!.GetDouble());
        Assert.Equal(part.StringValue, package!.Find("StringValue"u8)!.GetString());
    }
}
