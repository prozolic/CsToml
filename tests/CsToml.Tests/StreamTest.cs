
using FluentAssertions;
using Utf8StringInterpolation;

namespace CsToml.Tests;

public class StreamTest
{
    private readonly byte[] tomlText;
    private readonly byte[] expectedtomlText;

    public StreamTest()
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

        using var expectedBuffer = Utf8String.CreateWriter(out var writer);
        writer.AppendLine(@"str = ""value""");
        writer.AppendLine(@"int = 123");
        writer.AppendLine(@"flt = 3.1415");
        writer.AppendLine(@"boolean = true");
        writer.AppendLine(@"odt1 = 1979-05-27T07:32:00Z");
        writer.AppendLine(@"ldt1 = 1979-05-27T07:32:00");
        writer.AppendLine(@"ldt2 = 1979-05-27T00:32:00.999999");
        writer.AppendLine(@"ld1 = 1979-05-27");
        writer.AppendLine(@"lt1 = 07:32:00");
        writer.AppendLine(@"key = ""value""");
        writer.AppendLine(@"first.second.third = ""value""");
        writer.AppendLine(@"number = 123456");
        writer.AppendLine(@"array = [ 123, ""456"", true ]");
        writer.AppendLine(@"inlineTable = { key = 1, key2 = ""value"", key3 = [ 123, 456, 789 ], key4 = { key = ""inlinetable"" } }");
        writer.AppendLine();
        writer.AppendLine(@"[Table.test]");
        writer.AppendLine(@"key = ""value""");
        writer.AppendLine(@"first.second.third = ""value""");
        writer.AppendLine(@"number = 123456");
        writer.AppendLine();
        writer.AppendLine(@"[arrayOfTables]");
        writer.AppendLine();
        writer.AppendLine(@"[[arrayOfTables.test]]");
        writer.AppendLine(@"key = ""value""");
        writer.AppendLine(@"first.second.third = ""value""");
        writer.AppendLine(@"number = 123456");
        writer.AppendLine();
        writer.AppendLine(@"[[arrayOfTables.test]]");
        writer.AppendLine();
        writer.AppendLine(@"[[arrayOfTables.test]]");
        writer.AppendLine(@"key2 = ""value""");
        writer.AppendLine(@"first2.second2.third2 = ""value""");
        writer.AppendLine(@"number2 = 123456");
        writer.Flush();
        expectedtomlText = expectedBuffer.WrittenSpan.ToArray();
    }

    [Fact]
    public void DeserializeMemoryStreamTest()
    {
        var ms = new MemoryStream(tomlText);
        TomlDocument document = CsTomlSerializer.Deserialize<TomlDocument>(ms);

        document!.RootNode["str"u8].GetString().Should().Be("value");
        document!.RootNode["int"u8].GetInt64().Should().Be(123);
        document!.RootNode["flt"u8].GetDouble().Should().Be(3.1415d);
        document!.RootNode["boolean"u8].GetBool().Should().BeTrue();
        document!.RootNode["odt1"u8].GetDateTimeOffset().Should().Be(new DateTimeOffset(1979, 5, 27, 7, 32, 0, TimeSpan.Zero));
        document!.RootNode["ldt1"u8].GetDateTimeOffset().Should().Be(new DateTime(1979, 5, 27, 7, 32, 0));
        document!.RootNode["ldt2"u8].GetDateTimeOffset().Should().Be(new DateTime(1979, 5, 27, 0, 32, 0, 999, 999));
        document!.RootNode["ld1"u8].GetDateOnly().Should().Be(new DateOnly(1979, 5, 27));
        document!.RootNode["lt1"u8].GetTimeOnly().Should().Be(new TimeOnly(7, 32, 0));
        document!.RootNode["key"u8].GetString().Should().Be("value");
        document!.RootNode["first"u8]["second"u8]["third"u8].GetString().Should().Be("value");
        document!.RootNode["number"u8].GetInt64().Should().Be(123456);
        var arrayValue = document!.RootNode["array"u8]!.GetArray();
        arrayValue.Count.Should().Be(3);
        arrayValue[0].GetInt64().Should().Be(123);
        arrayValue[1].GetString().Should().Be("456");
        arrayValue[2].GetBool().Should().BeTrue();
        document!.RootNode["inlineTable"u8]["key"u8].GetInt64().Should().Be(1);
        document!.RootNode["inlineTable"u8]["key2"u8].GetString().Should().Be("value");
        document!.RootNode["inlineTable"u8]["key3"u8][0].GetInt64().Should().Be(123);
        document!.RootNode["inlineTable"u8]["key3"u8][1].GetInt64().Should().Be(456);
        document!.RootNode["inlineTable"u8]["key3"u8][2].GetInt64().Should().Be(789);
        document!.RootNode["inlineTable"u8]["key4"u8]["key"u8].GetString().Should().Be("inlinetable");
        document!.RootNode["Table"u8]["test"u8]["key"u8].GetString().Should().Be("value");
        document!.RootNode["Table"u8]["test"u8]["first"u8]["second"u8]["third"u8].GetString().Should().Be("value");
        document!.RootNode["Table"u8]["test"u8]["number"u8].GetInt64().Should().Be(123456);
        document!.RootNode["arrayOfTables"u8]["test"u8][0]["key"u8].GetString().Should().Be("value");
        document!.RootNode["arrayOfTables"u8]["test"u8][0]["first"u8]["second"u8]["third"u8].GetString().Should().Be("value");
        document!.RootNode["arrayOfTables"u8]["test"u8][0]["number"u8].GetInt64().Should().Be(123456);
        document!.RootNode["arrayOfTables"u8]["test"u8][2]["key2"u8].GetString().Should().Be("value");
        document!.RootNode["arrayOfTables"u8]["test"u8][2]["first2"u8]["second2"u8]["third2"u8].GetString().Should().Be("value");
        document!.RootNode["arrayOfTables"u8]["test"u8][2]["number2"u8].GetInt64().Should().Be(123456);
        // failed
        document!.RootNode["failed"u8].HasValue.Should().BeFalse();
    }

    [Fact]
    public async Task DeserializeAsyncMemoryStreamTest()
    {
        var ms = new MemoryStream(tomlText);
        TomlDocument document = await CsTomlSerializer.DeserializeAsync<TomlDocument>(ms);

        document!.RootNode["str"u8].GetString().Should().Be("value");
        document!.RootNode["int"u8].GetInt64().Should().Be(123);
        document!.RootNode["flt"u8].GetDouble().Should().Be(3.1415d);
        document!.RootNode["boolean"u8].GetBool().Should().BeTrue();
        document!.RootNode["odt1"u8].GetDateTimeOffset().Should().Be(new DateTimeOffset(1979, 5, 27, 7, 32, 0, TimeSpan.Zero));
        document!.RootNode["ldt1"u8].GetDateTimeOffset().Should().Be(new DateTime(1979, 5, 27, 7, 32, 0));
        document!.RootNode["ldt2"u8].GetDateTimeOffset().Should().Be(new DateTime(1979, 5, 27, 0, 32, 0, 999, 999));
        document!.RootNode["ld1"u8].GetDateOnly().Should().Be(new DateOnly(1979, 5, 27));
        document!.RootNode["lt1"u8].GetTimeOnly().Should().Be(new TimeOnly(7, 32, 0));
        document!.RootNode["key"u8].GetString().Should().Be("value");
        document!.RootNode["first"u8]["second"u8]["third"u8].GetString().Should().Be("value");
        document!.RootNode["number"u8].GetInt64().Should().Be(123456);
        var arrayValue = document!.RootNode["array"u8]!.GetArray();
        arrayValue.Count.Should().Be(3);
        arrayValue[0].GetInt64().Should().Be(123);
        arrayValue[1].GetString().Should().Be("456");
        arrayValue[2].GetBool().Should().BeTrue();
        document!.RootNode["inlineTable"u8]["key"u8].GetInt64().Should().Be(1);
        document!.RootNode["inlineTable"u8]["key2"u8].GetString().Should().Be("value");
        document!.RootNode["inlineTable"u8]["key3"u8][0].GetInt64().Should().Be(123);
        document!.RootNode["inlineTable"u8]["key3"u8][1].GetInt64().Should().Be(456);
        document!.RootNode["inlineTable"u8]["key3"u8][2].GetInt64().Should().Be(789);
        document!.RootNode["inlineTable"u8]["key4"u8]["key"u8].GetString().Should().Be("inlinetable");
        document!.RootNode["Table"u8]["test"u8]["key"u8].GetString().Should().Be("value");
        document!.RootNode["Table"u8]["test"u8]["first"u8]["second"u8]["third"u8].GetString().Should().Be("value");
        document!.RootNode["Table"u8]["test"u8]["number"u8].GetInt64().Should().Be(123456);
        document!.RootNode["arrayOfTables"u8]["test"u8][0]["key"u8].GetString().Should().Be("value");
        document!.RootNode["arrayOfTables"u8]["test"u8][0]["first"u8]["second"u8]["third"u8].GetString().Should().Be("value");
        document!.RootNode["arrayOfTables"u8]["test"u8][0]["number"u8].GetInt64().Should().Be(123456);
        document!.RootNode["arrayOfTables"u8]["test"u8][2]["key2"u8].GetString().Should().Be("value");
        document!.RootNode["arrayOfTables"u8]["test"u8][2]["first2"u8]["second2"u8]["third2"u8].GetString().Should().Be("value");
        document!.RootNode["arrayOfTables"u8]["test"u8][2]["number2"u8].GetInt64().Should().Be(123456);
        // failed
        document!.RootNode["failed"u8].HasValue.Should().BeFalse();
    }

    [Fact]
    public void SerializeMemoryStreamTest()
    {
        TomlDocument document = CsTomlSerializer.Deserialize<TomlDocument>(tomlText);

        var ms = new MemoryStream(65536);
        CsTomlSerializer.Serialize(ms, document);

        var buffer = ms.ToArray();

        buffer.Should().Equal(expectedtomlText);
    }

    [Fact]
    public async Task SerializeAsyncMemoryStreamTest()
    {
        TomlDocument document = CsTomlSerializer.Deserialize<TomlDocument>(tomlText);

        var ms = new MemoryStream(65536);
        await CsTomlSerializer.SerializeAsync(ms, document);

        var buffer = ms.ToArray();
        buffer.Should().Equal(expectedtomlText);
    }

}

