
using CsToml.Error;
using FluentAssertions;

namespace CsToml.Tests;

public class DefaultTest
{
    private readonly byte[] tomlText;

    public DefaultTest()
    {
        tomlText = @"
str = ""value""
int = 123
flt = 3.1415
boolean = true
odt1 = 1979-05-27T07:32:00Z
ldt1 = 1979-05-27T07:32:00
ldt2 = 1979-05-27T00:32:00.999999
ld1 = 1979-05-27
lt1 = 07:32:00

key = ""value""
first.second.third = ""value""
number = 123456
array = [123 , ""456"", true]
inlineTable = { key = 1 , key2 = ""value"" , key3 = [ 123, 456, 789], key4 = { key = ""inlinetable"" }}

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
        var document = CsTomlSerializer.Deserialize<TomlDocument>(tomlText);
        var serializeText = CsTomlSerializer.Serialize(document!);
    }

    [Fact]
    public void IndexerTest()
    {
        var document = CsTomlSerializer.Deserialize<TomlDocument>(tomlText)!;

        document!.RootNode["key"u8].GetString().Should().Be("value");
        document!.RootNode["first"u8]["second"u8]["third"u8].GetString().Should().Be("value");
        document!.RootNode["number"u8].GetInt64().Should().Be(123456);
        document!.RootNode["Table"u8]["test"u8]["key"u8].GetString().Should().Be("value");
        document!.RootNode["arrayOfTables"u8]["test"u8][0]["key"u8].GetString().Should().Be("value");
        document!.RootNode["inlineTable"u8]["key4"u8]["key"u8].GetString().Should().Be("inlinetable");
        document!.RootNode["failed"u8].HasValue.Should().BeFalse();
    }
}

