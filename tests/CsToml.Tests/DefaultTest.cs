
using CsToml.Error;
using FluentAssertions;
using FluentAssertions.Execution;
using Utf8StringInterpolation;

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
    public void BasicTest()
    {
        var document = CsTomlSerializer.Deserialize<TomlDocument>(tomlText);
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

        using var serializeText = CsTomlSerializer.Serialize(document!);

        using var buffer = Utf8String.CreateWriter(out var writer);
        writer.AppendLine("str = \"value\"");
        writer.AppendLine("int = 123");
        writer.AppendLine("flt = 3.1415");
        writer.AppendLine("boolean = true");
        writer.AppendLine("odt1 = 1979-05-27T07:32:00Z");
        writer.AppendLine("ldt1 = 1979-05-27T07:32:00");
        writer.AppendLine("ldt2 = 1979-05-27T00:32:00.999999");
        writer.AppendLine("ld1 = 1979-05-27");
        writer.AppendLine("lt1 = 07:32:00");
        writer.AppendLine("key = \"value\"");
        writer.AppendLine("first.second.third = \"value\"");
        writer.AppendLine("number = 123456");
        writer.AppendLine("array = [ 123, \"456\", true ]");
        writer.AppendLine("inlineTable = { key = 1, key2 = \"value\", key3 = [ 123, 456, 789 ], key4 = { key = \"inlinetable\" } }");
        writer.AppendLine("");
        writer.AppendLine("[Table.test]");
        writer.AppendLine("key = \"value\"");
        writer.AppendLine("first.second.third = \"value\"");
        writer.AppendLine("number = 123456");
        writer.AppendLine("");
        writer.AppendLine("[arrayOfTables]");
        writer.AppendLine("");
        writer.AppendLine("[[arrayOfTables.test]]");
        writer.AppendLine("key = \"value\"");
        writer.AppendLine("first.second.third = \"value\"");
        writer.AppendLine("number = 123456");
        writer.AppendLine("");
        writer.AppendLine("[[arrayOfTables.test]]");
        writer.AppendLine("");
        writer.AppendLine("[[arrayOfTables.test]]");
        writer.AppendLine("key2 = \"value\"");
        writer.AppendLine("first2.second2.third2 = \"value\"");
        writer.AppendLine("number2 = 123456");
        writer.Flush();

        buffer.ToArray().Should().Equal(serializeText.ByteSpan.ToArray());

    }

    [Fact]
    public void Utf16IndexerTest()
    {
        //TomlDocumentNode[ReadOnlySpan<char> key]
        var document = CsTomlSerializer.Deserialize<TomlDocument>(tomlText);
        document!.RootNode["str"].GetString().Should().Be("value");
        document!.RootNode["int"].GetInt64().Should().Be(123);
        document!.RootNode["flt"].GetDouble().Should().Be(3.1415d);
        document!.RootNode["boolean"].GetBool().Should().BeTrue();
        document!.RootNode["odt1"].GetDateTimeOffset().Should().Be(new DateTimeOffset(1979, 5, 27, 7, 32, 0, TimeSpan.Zero));
        document!.RootNode["ldt1"].GetDateTimeOffset().Should().Be(new DateTime(1979, 5, 27, 7, 32, 0));
        document!.RootNode["ldt2"].GetDateTimeOffset().Should().Be(new DateTime(1979, 5, 27, 0, 32, 0, 999, 999));
        document!.RootNode["ld1"].GetDateOnly().Should().Be(new DateOnly(1979, 5, 27));
        document!.RootNode["lt1"].GetTimeOnly().Should().Be(new TimeOnly(7, 32, 0));
        document!.RootNode["key"].GetString().Should().Be("value");
        document!.RootNode["first"]["second"]["third"].GetString().Should().Be("value");
        document!.RootNode["number"].GetInt64().Should().Be(123456);
        var arrayValue = document!.RootNode["array"]!.GetArray();
        arrayValue.Count.Should().Be(3);
        arrayValue[0].GetInt64().Should().Be(123);
        arrayValue[1].GetString().Should().Be("456");
        arrayValue[2].GetBool().Should().BeTrue();
        document!.RootNode["inlineTable"]["key"].GetInt64().Should().Be(1);
        document!.RootNode["inlineTable"]["key2"].GetString().Should().Be("value");
        document!.RootNode["inlineTable"]["key3"][0].GetInt64().Should().Be(123);
        document!.RootNode["inlineTable"]["key3"][1].GetInt64().Should().Be(456);
        document!.RootNode["inlineTable"]["key3"][2].GetInt64().Should().Be(789);
        document!.RootNode["inlineTable"]["key4"]["key"].GetString().Should().Be("inlinetable");
        document!.RootNode["Table"]["test"]["key"].GetString().Should().Be("value");
        document!.RootNode["Table"]["test"]["first"]["second"]["third"].GetString().Should().Be("value");
        document!.RootNode["Table"]["test"]["number"].GetInt64().Should().Be(123456);
        document!.RootNode["arrayOfTables"]["test"][0]["key"].GetString().Should().Be("value");
        document!.RootNode["arrayOfTables"]["test"][0]["first"]["second"]["third"].GetString().Should().Be("value");
        document!.RootNode["arrayOfTables"]["test"][0]["number"].GetInt64().Should().Be(123456);
        document!.RootNode["arrayOfTables"]["test"][2]["key2"].GetString().Should().Be("value");
        document!.RootNode["arrayOfTables"]["test"][2]["first2"]["second2"]["third2"].GetString().Should().Be("value");
        document!.RootNode["arrayOfTables"]["test"][2]["number2"].GetInt64().Should().Be(123456);
        // failed
        document!.RootNode["failed"].HasValue.Should().BeFalse();
    }

    [Fact]
    public void ThrowTest()
    {
        var toml = @"
str = ""value""
intError 
flt = 3.1415
"u8;

        try
        {
            var document = CsTomlSerializer.Deserialize<TomlDocument>(toml);
        }
        catch(CsTomlSerializeException ctse)
        {
            ctse.Exceptions!.Count.Should().Be(1);
            return;
        }
        Execute.Assertion.FailWith("No CsTomlSerializeException is thrown.");
    }
}

public class DeserializeValueTypeTest
{
    [Fact]
    public void Test()
    {
        var tomlIntValue = CsTomlSerializer.DeserializeValueType<long>("1234"u8);
        tomlIntValue.Should().Be(1234);

        var tomlStringValue = CsTomlSerializer.DeserializeValueType<string>("\"\\U00000061\\U00000062\\U00000063\""u8);
        tomlStringValue.Should().Be("abc");

        var tomlDateTimeValue = CsTomlSerializer.DeserializeValueType<DateTime>("2024-10-20T15:16:00"u8);
        tomlDateTimeValue.Should().Be(new DateTime(2024, 10, 20, 15, 16, 0, DateTimeKind.Local));

        var tomlArrayValue = CsTomlSerializer.DeserializeValueType<string[]>("[ \"red\", \"yellow\", \"green\" ]"u8);
        tomlArrayValue.Should().Equal(["red", "yellow", "green"]);

        var tomlinlineTableValue = CsTomlSerializer.DeserializeValueType<IDictionary<string, object>>("{ x = 1, y = 2, z = \"3\" }"u8);
        tomlinlineTableValue.Count.Should().Be(3);
        tomlinlineTableValue["x"].Should().Be((object)1);
        tomlinlineTableValue["y"].Should().Be((object)2);
        tomlinlineTableValue["z"].Should().Be((object)"3");

        var tomlTupleValue = CsTomlSerializer.DeserializeValueType<Tuple<string, string, string>>("[ \"red\", \"yellow\", \"green\" ]"u8);
        tomlTupleValue.Should().Be(new Tuple<string, string, string>("red", "yellow", "green"));
    }
}

public class SerializeValueTypeTest
{
    [Fact]
    public void Test()
    {
        using var serializedTomlValue1 = CsTomlSerializer.SerializeValueType(123);
        serializedTomlValue1.ByteSpan.ToArray().Should().Equal("123"u8.ToArray());

        using var serializedTomlValue2 = CsTomlSerializer.SerializeValueType("abc");
        serializedTomlValue2.ByteSpan.ToArray().Should().Equal("\"abc\""u8.ToArray());

        using var serializedTomlValue3 = CsTomlSerializer.SerializeValueType(new DateTime(2024, 10, 20, 15, 16, 0, DateTimeKind.Local));
        serializedTomlValue3.ByteSpan.ToArray().Should().Equal("2024-10-20T15:16:00"u8.ToArray());

        using var serializedTomlValue4 = CsTomlSerializer.SerializeValueType<string[]>(["red", "yellow", "green"]);
        serializedTomlValue4.ByteSpan.ToArray().Should().Equal("[ \"red\", \"yellow\", \"green\" ]"u8.ToArray());

        var dict = new Dictionary<string, object>();
        dict["x"] = 1;
        dict["y"] = 2;
        dict["z"] = "3";
        using var serializedTomlValue5 = CsTomlSerializer.SerializeValueType(dict);
        serializedTomlValue5.ByteSpan.ToArray().Should().Equal("{x = 1, y = 2, z = \"3\"}"u8.ToArray());

        using var serializedTomlValue6 = CsTomlSerializer.SerializeValueType(new Tuple<string, string, string>("red", "yellow", "green"));
        serializedTomlValue6.ByteSpan.ToArray().Should().Equal("[ \"red\", \"yellow\", \"green\" ]"u8.ToArray());

    }
}