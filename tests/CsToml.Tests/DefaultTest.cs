
using CsToml.Error;
using System.Text;
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
        document!.RootNode["str"u8].GetString().ShouldBe("value");
        document!.RootNode["int"u8].GetInt64().ShouldBe(123);
        document!.RootNode["flt"u8].GetDouble().ShouldBe(3.1415d);
        document!.RootNode["boolean"u8].GetBool().ShouldBeTrue();
        document!.RootNode["odt1"u8].GetDateTimeOffset().ShouldBe(new DateTimeOffset(1979, 5, 27, 7, 32, 0, TimeSpan.Zero));
        document!.RootNode["ldt1"u8].GetDateTimeOffset().ShouldBe(new DateTime(1979, 5, 27, 7, 32, 0));
        document!.RootNode["ldt2"u8].GetDateTimeOffset().ShouldBe(new DateTime(1979, 5, 27, 0, 32, 0, 999, 999));
        document!.RootNode["ld1"u8].GetDateOnly().ShouldBe(new DateOnly(1979, 5, 27));
        document!.RootNode["lt1"u8].GetTimeOnly().ShouldBe(new TimeOnly(7, 32, 0));
        document!.RootNode["key"u8].GetString().ShouldBe("value");
        document!.RootNode["first"u8]["second"u8]["third"u8].GetString().ShouldBe("value");
        document!.RootNode["number"u8].GetInt64().ShouldBe(123456);
        var arrayValue = document!.RootNode["array"u8]!.GetArray();
        arrayValue.Count.ShouldBe(3);
        arrayValue[0].GetInt64().ShouldBe(123);
        arrayValue[1].GetString().ShouldBe("456");
        arrayValue[2].GetBool().ShouldBeTrue();
        document!.RootNode["inlineTable"u8]["key"u8].GetInt64().ShouldBe(1);
        document!.RootNode["inlineTable"u8]["key2"u8].GetString().ShouldBe("value");
        document!.RootNode["inlineTable"u8]["key3"u8][0].GetInt64().ShouldBe(123);
        document!.RootNode["inlineTable"u8]["key3"u8][1].GetInt64().ShouldBe(456);
        document!.RootNode["inlineTable"u8]["key3"u8][2].GetInt64().ShouldBe(789);
        document!.RootNode["inlineTable"u8]["key4"u8]["key"u8].GetString().ShouldBe("inlinetable");
        document!.RootNode["Table"u8]["test"u8]["key"u8].GetString().ShouldBe("value");
        document!.RootNode["Table"u8]["test"u8]["first"u8]["second"u8]["third"u8].GetString().ShouldBe("value");
        document!.RootNode["Table"u8]["test"u8]["number"u8].GetInt64().ShouldBe(123456);
        document!.RootNode["arrayOfTables"u8]["test"u8][0]["key"u8].GetString().ShouldBe("value");
        document!.RootNode["arrayOfTables"u8]["test"u8][0]["first"u8]["second"u8]["third"u8].GetString().ShouldBe("value");
        document!.RootNode["arrayOfTables"u8]["test"u8][0]["number"u8].GetInt64().ShouldBe(123456);
        document!.RootNode["arrayOfTables"u8]["test"u8][2]["key2"u8].GetString().ShouldBe("value");
        document!.RootNode["arrayOfTables"u8]["test"u8][2]["first2"u8]["second2"u8]["third2"u8].GetString().ShouldBe("value");
        document!.RootNode["arrayOfTables"u8]["test"u8][2]["number2"u8].GetInt64().ShouldBe(123456);
        // failed
        document!.RootNode["failed"u8].HasValue.ShouldBeFalse();

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

        buffer.ToArray().ShouldBe(serializeText.ByteSpan.ToArray());

    }

    [Fact]
    public void Utf16IndexerTest()
    {
        //TomlDocumentNode[ReadOnlySpan<char> key]
        var document = CsTomlSerializer.Deserialize<TomlDocument>(tomlText);
        document!.RootNode["str"].GetString().ShouldBe("value");
        document!.RootNode["int"].GetInt64().ShouldBe(123);
        document!.RootNode["flt"].GetDouble().ShouldBe(3.1415d);
        document!.RootNode["boolean"].GetBool().ShouldBeTrue();
        document!.RootNode["odt1"].GetDateTimeOffset().ShouldBe(new DateTimeOffset(1979, 5, 27, 7, 32, 0, TimeSpan.Zero));
        document!.RootNode["ldt1"].GetDateTimeOffset().ShouldBe(new DateTime(1979, 5, 27, 7, 32, 0));
        document!.RootNode["ldt2"].GetDateTimeOffset().ShouldBe(new DateTime(1979, 5, 27, 0, 32, 0, 999, 999));
        document!.RootNode["ld1"].GetDateOnly().ShouldBe(new DateOnly(1979, 5, 27));
        document!.RootNode["lt1"].GetTimeOnly().ShouldBe(new TimeOnly(7, 32, 0));
        document!.RootNode["key"].GetString().ShouldBe("value");
        document!.RootNode["first"]["second"]["third"].GetString().ShouldBe("value");
        document!.RootNode["number"].GetInt64().ShouldBe(123456);
        var arrayValue = document!.RootNode["array"]!.GetArray();
        arrayValue.Count.ShouldBe(3);
        arrayValue[0].GetInt64().ShouldBe(123);
        arrayValue[1].GetString().ShouldBe("456");
        arrayValue[2].GetBool().ShouldBeTrue();
        document!.RootNode["inlineTable"]["key"].GetInt64().ShouldBe(1);
        document!.RootNode["inlineTable"]["key2"].GetString().ShouldBe("value");
        document!.RootNode["inlineTable"]["key3"][0].GetInt64().ShouldBe(123);
        document!.RootNode["inlineTable"]["key3"][1].GetInt64().ShouldBe(456);
        document!.RootNode["inlineTable"]["key3"][2].GetInt64().ShouldBe(789);
        document!.RootNode["inlineTable"]["key4"]["key"].GetString().ShouldBe("inlinetable");
        document!.RootNode["Table"]["test"]["key"].GetString().ShouldBe("value");
        document!.RootNode["Table"]["test"]["first"]["second"]["third"].GetString().ShouldBe("value");
        document!.RootNode["Table"]["test"]["number"].GetInt64().ShouldBe(123456);
        document!.RootNode["arrayOfTables"]["test"][0]["key"].GetString().ShouldBe("value");
        document!.RootNode["arrayOfTables"]["test"][0]["first"]["second"]["third"].GetString().ShouldBe("value");
        document!.RootNode["arrayOfTables"]["test"][0]["number"].GetInt64().ShouldBe(123456);
        document!.RootNode["arrayOfTables"]["test"][2]["key2"].GetString().ShouldBe("value");
        document!.RootNode["arrayOfTables"]["test"][2]["first2"]["second2"]["third2"].GetString().ShouldBe("value");
        document!.RootNode["arrayOfTables"]["test"][2]["number2"].GetInt64().ShouldBe(123456);
        // failed
        document!.RootNode["failed"].HasValue.ShouldBeFalse();
    }

    [Fact]
    public void ThrowTest()
    {
        Should.Throw<CsTomlSerializeException>(() =>
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
            catch (CsTomlSerializeException ctse)
            {
                ctse.Exceptions!.Count.ShouldBe(1);
                throw;
            }
        });
    }
}

public class TomlStringTest
{
    [Fact]
    public void DeserializeAndSerialize()
    {
        var toml = @"
key = ""value""
1234 = ""value""
""127.0.0.1"" = ""value""
'key2' = ""value""
"""" = ""blank"" 
str = ""I'm a string. \""You can quote me\"". Name\tJos\u00E9\nLocation\tSF.""
str1 = """"""
Roses are red
Violets are blue""""""
regex    = '<\i\c*\s*>'
lines  = '''
The first newline is
trimmed in raw strings.
   All other whitespace
   is preserved.
'''
"u8;
        var document = CsTomlSerializer.Deserialize<TomlDocument>(toml);
        using var serializeText = CsTomlSerializer.Serialize(document!);

        using var buffer = Utf8String.CreateWriter(out var writer);
        writer.AppendLine(@"key = ""value""");
        writer.AppendLine(@"1234 = ""value""");
        writer.AppendLine(@"""127.0.0.1"" = ""value""");
        writer.AppendLine(@"'key2' = ""value""");
        writer.AppendLine(@""""" = ""blank""");
        writer.AppendLine(@"str = ""I'm a string. \""You can quote me\"". Name\tJosé\nLocation\tSF.""");
        writer.AppendLine(@"str1 = """"""Roses are red\r\nViolets are blue""""""");
        writer.AppendLine(@"regex = '<\i\c*\s*>'");
        writer.AppendLine(@"lines = '''The first newline is");
        writer.AppendLine(@"trimmed in raw strings.");
        writer.AppendLine(@"   All other whitespace");
        writer.AppendLine(@"   is preserved.");
        writer.AppendLine(@"'''");
        writer.Flush();

        var expected = Encoding.UTF8.GetString(buffer.ToArray()).Replace("\r\n", "\n");
        var actual = Encoding.UTF8.GetString(serializeText.ByteSpan).Replace("\r\n", "\n");

        expected.ShouldBe(actual);

        //buffer.ToArray().ShouldBe(serializeText.ByteSpan.ToArray());
    }
}

public class TomlIntegerTest
{
    [Fact]
    public void DeserializeAndSerialize()
    {
        var toml = @"
int1 = +99
int2 = 42
int3 = 0
int4 = -17
int5 = 1_000
int6 = 5_349_221
int7 = 53_49_221
int8 = 1_2_3_4_5
int9 = 9223372036854775807
int10 = -9223372036854775808
hex1 = 0xDEADBEEF
hex2 = 0xdeadbeef
hex3 = 0xdead_beef
oct1 = 0o01234567
oct2 = 0o755
bin1 = 0b11010110
"u8;

        var document = CsTomlSerializer.Deserialize<TomlDocument>(toml);
        using var serializeText = CsTomlSerializer.Serialize(document!);

        using var buffer = Utf8String.CreateWriter(out var writer);
        writer.AppendLine("int1 = 99");
        writer.AppendLine("int2 = 42");
        writer.AppendLine("int3 = 0");
        writer.AppendLine("int4 = -17");
        writer.AppendLine("int5 = 1000");
        writer.AppendLine("int6 = 5349221");
        writer.AppendLine("int7 = 5349221");
        writer.AppendLine("int8 = 12345");
        writer.AppendLine("int9 = 9223372036854775807");
        writer.AppendLine("int10 = -9223372036854775808");
        writer.AppendLine("hex1 = 3735928559");
        writer.AppendLine("hex2 = 3735928559");
        writer.AppendLine("hex3 = 3735928559");
        writer.AppendLine("oct1 = 342391");
        writer.AppendLine("oct2 = 493");
        writer.AppendLine("bin1 = 214");
        writer.Flush();

        buffer.ToArray().ShouldBe(serializeText.ByteSpan.ToArray());
    }
}

public class TomlFloatTest
{
    [Fact]
    public void DeserializeAndSerialize()
    {
        var toml = @"
flt1 = +1.0
flt2 = 3.1415
flt3 = -0.01
flt4 = 5e+22
flt5 = 1e06
flt6 = -2E-2
flt7 = 6.626e-34
flt8 = 224_617.445_991_228
sf1 = inf
sf2 = +inf
sf3 = -inf
sf4 = nan
sf5 = +nan
sf6 = -nan
"u8;

        var document = CsTomlSerializer.Deserialize<TomlDocument>(toml);
        using var serializeText = CsTomlSerializer.Serialize(document!);

        using var buffer = Utf8String.CreateWriter(out var writer);
        writer.AppendLine("flt1 = 1.0");
        writer.AppendLine("flt2 = 3.1415");
        writer.AppendLine("flt3 = -0.01");
        writer.AppendLine("flt4 = 5E+22");
        writer.AppendLine("flt5 = 1000000.0");
        writer.AppendLine("flt6 = -0.02");
        writer.AppendLine("flt7 = 6.626E-34");
        writer.AppendLine("flt8 = 224617.445991228");
        writer.AppendLine("sf1 = inf");
        writer.AppendLine("sf2 = inf");
        writer.AppendLine("sf3 = -inf");
        writer.AppendLine("sf4 = nan");
        writer.AppendLine("sf5 = nan");
        writer.AppendLine("sf6 = nan");
        writer.Flush();

        buffer.ToArray().ShouldBe(serializeText.ByteSpan.ToArray());
    }
}

public class TomlBooleanTest
{
    [Fact]
    public void DeserializeAndSerialize()
    {
        var toml = @"
bool1 = true
bool2 = false
"u8;

        var document = CsTomlSerializer.Deserialize<TomlDocument>(toml);
        using var serializeText = CsTomlSerializer.Serialize(document!);

        using var buffer = Utf8String.CreateWriter(out var writer);
        writer.AppendLine("bool1 = true");
        writer.AppendLine("bool2 = false");
        writer.Flush();

        buffer.ToArray().ShouldBe(serializeText.ByteSpan.ToArray());
    }
}

public class TomlOffsetDateTimeTest
{
    [Fact]
    public void DeserializeAndSerialize()
    {
        var toml = @"
odt1 = 1979-05-27T07:32:00Z
odt2 = 1979-05-27T00:32:00-07:00
odt3 = 1979-05-27T00:32:00.999999-07:00
odt4 = 1979-05-27T00:32:00.1Z
odt5 = 1979-05-27T00:32:00.01Z
odt6 = 1979-05-27T00:32:00.11Z
odt7 = 1979-05-27T00:32:00.001Z
odt8 = 1979-05-27T00:32:00.011Z
odt9 = 1979-05-27T00:32:00.111Z
odt10 = 1979-05-27T00:32:00.0001Z
odt11 = 1979-05-27T00:32:00.0011Z
odt12 = 1979-05-27T00:32:00.0111Z
odt13 = 1979-05-27T00:32:00.1111Z
odt14 = 1979-05-27T00:32:00.00001Z
odt15 = 1979-05-27T00:32:00.00011Z
odt16 = 1979-05-27T00:32:00.00111Z
odt17 = 1979-05-27T00:32:00.01111Z
odt18 = 1979-05-27T00:32:00.11111Z
odt19 = 1979-05-27T00:32:00.000001Z
odt20 = 1979-05-27T00:32:00.000011Z
odt21 = 1979-05-27T00:32:00.000111Z
odt22 = 1979-05-27T00:32:00.001111Z
odt23 = 1979-05-27T00:32:00.011111Z
odt24 = 1979-05-27T00:32:00.111111Z
odt25 = 1979-05-27T00:32:00.1-07:00
odt26 = 1979-05-27T00:32:00.01-07:00
odt27 = 1979-05-27T00:32:00.11-07:00
odt28 = 1979-05-27T00:32:00.001-07:00
odt29 = 1979-05-27T00:32:00.011-07:00
odt30 = 1979-05-27T00:32:00.111-07:00
odt31 = 1979-05-27T00:32:00.0001-07:00
odt32 = 1979-05-27T00:32:00.0011-07:00
odt33 = 1979-05-27T00:32:00.0111-07:00
odt34 = 1979-05-27T00:32:00.1111-07:00
odt35 = 1979-05-27T00:32:00.00001-07:00
odt36 = 1979-05-27T00:32:00.00011-07:00
odt37 = 1979-05-27T00:32:00.00111-07:00
odt38 = 1979-05-27T00:32:00.01111-07:00
odt39 = 1979-05-27T00:32:00.11111-07:00
odt40 = 1979-05-27T00:32:00.000001-07:00
odt41 = 1979-05-27T00:32:00.000011-07:00
odt42 = 1979-05-27T00:32:00.000111-07:00
odt43 = 1979-05-27T00:32:00.001111-07:00
odt44 = 1979-05-27T00:32:00.011111-07:00
odt45 = 1979-05-27T00:32:00.111111-07:00
"u8;

        var document = CsTomlSerializer.Deserialize<TomlDocument>(toml);
        using var serializeText = CsTomlSerializer.Serialize(document!);

        using var buffer = Utf8String.CreateWriter(out var writer);
        writer.AppendLine("odt1 = 1979-05-27T07:32:00Z");
        writer.AppendLine("odt2 = 1979-05-27T00:32:00-07:00");
        writer.AppendLine("odt3 = 1979-05-27T00:32:00.999999-07:00");
        writer.AppendLine("odt4 = 1979-05-27T00:32:00.1Z");
        writer.AppendLine("odt5 = 1979-05-27T00:32:00.01Z");
        writer.AppendLine("odt6 = 1979-05-27T00:32:00.11Z");
        writer.AppendLine("odt7 = 1979-05-27T00:32:00.001Z");
        writer.AppendLine("odt8 = 1979-05-27T00:32:00.011Z");
        writer.AppendLine("odt9 = 1979-05-27T00:32:00.111Z");
        writer.AppendLine("odt10 = 1979-05-27T00:32:00.0001Z");
        writer.AppendLine("odt11 = 1979-05-27T00:32:00.0011Z");
        writer.AppendLine("odt12 = 1979-05-27T00:32:00.0111Z");
        writer.AppendLine("odt13 = 1979-05-27T00:32:00.1111Z");
        writer.AppendLine("odt14 = 1979-05-27T00:32:00.00001Z");
        writer.AppendLine("odt15 = 1979-05-27T00:32:00.00011Z");
        writer.AppendLine("odt16 = 1979-05-27T00:32:00.00111Z");
        writer.AppendLine("odt17 = 1979-05-27T00:32:00.01111Z");
        writer.AppendLine("odt18 = 1979-05-27T00:32:00.11111Z");
        writer.AppendLine("odt19 = 1979-05-27T00:32:00.000001Z");
        writer.AppendLine("odt20 = 1979-05-27T00:32:00.000011Z");
        writer.AppendLine("odt21 = 1979-05-27T00:32:00.000111Z");
        writer.AppendLine("odt22 = 1979-05-27T00:32:00.001111Z");
        writer.AppendLine("odt23 = 1979-05-27T00:32:00.011111Z");
        writer.AppendLine("odt24 = 1979-05-27T00:32:00.111111Z");
        writer.AppendLine("odt25 = 1979-05-27T00:32:00.1-07:00");
        writer.AppendLine("odt26 = 1979-05-27T00:32:00.01-07:00");
        writer.AppendLine("odt27 = 1979-05-27T00:32:00.11-07:00");
        writer.AppendLine("odt28 = 1979-05-27T00:32:00.001-07:00");
        writer.AppendLine("odt29 = 1979-05-27T00:32:00.011-07:00");
        writer.AppendLine("odt30 = 1979-05-27T00:32:00.111-07:00");
        writer.AppendLine("odt31 = 1979-05-27T00:32:00.0001-07:00");
        writer.AppendLine("odt32 = 1979-05-27T00:32:00.0011-07:00");
        writer.AppendLine("odt33 = 1979-05-27T00:32:00.0111-07:00");
        writer.AppendLine("odt34 = 1979-05-27T00:32:00.1111-07:00");
        writer.AppendLine("odt35 = 1979-05-27T00:32:00.00001-07:00");
        writer.AppendLine("odt36 = 1979-05-27T00:32:00.00011-07:00");
        writer.AppendLine("odt37 = 1979-05-27T00:32:00.00111-07:00");
        writer.AppendLine("odt38 = 1979-05-27T00:32:00.01111-07:00");
        writer.AppendLine("odt39 = 1979-05-27T00:32:00.11111-07:00");
        writer.AppendLine("odt40 = 1979-05-27T00:32:00.000001-07:00");
        writer.AppendLine("odt41 = 1979-05-27T00:32:00.000011-07:00");
        writer.AppendLine("odt42 = 1979-05-27T00:32:00.000111-07:00");
        writer.AppendLine("odt43 = 1979-05-27T00:32:00.001111-07:00");
        writer.AppendLine("odt44 = 1979-05-27T00:32:00.011111-07:00");
        writer.AppendLine("odt45 = 1979-05-27T00:32:00.111111-07:00");
        writer.Flush();

        buffer.ToArray().ShouldBe(serializeText.ByteSpan.ToArray());
    }
}

public class TomlLocalDateTimeTest
{
    [Fact]
    public void DeserializeAndSerialize()
    {
        var toml = @"
ldt1 = 1979-05-27T07:32:00
ldt2 = 1979-05-27T00:32:00.123
ldt3 = 1979-05-27T00:32:00.999999
ldt4 = 1979-05-27T00:32:00.1
ldt5 = 1979-05-27T00:32:00.01
ldt6 = 1979-05-27T00:32:00.11
ldt7 = 1979-05-27T00:32:00.001
ldt8 = 1979-05-27T00:32:00.011
ldt9 = 1979-05-27T00:32:00.111
ldt10 = 1979-05-27T00:32:00.0001
ldt11 = 1979-05-27T00:32:00.0011
ldt12 = 1979-05-27T00:32:00.0111
ldt13 = 1979-05-27T00:32:00.1111
ldt14 = 1979-05-27T00:32:00.00001
ldt15 = 1979-05-27T00:32:00.00011
ldt16 = 1979-05-27T00:32:00.00111
ldt17 = 1979-05-27T00:32:00.01111
ldt18 = 1979-05-27T00:32:00.11111
ldt19 = 1979-05-27T00:32:00.000001
ldt20 = 1979-05-27T00:32:00.000011
ldt21 = 1979-05-27T00:32:00.000111
ldt22 = 1979-05-27T00:32:00.001111
ldt23 = 1979-05-27T00:32:00.011111
ldt24 = 1979-05-27T00:32:00.111111
"u8;

        var document = CsTomlSerializer.Deserialize<TomlDocument>(toml);
        using var serializeText = CsTomlSerializer.Serialize(document!);

        using var buffer = Utf8String.CreateWriter(out var writer);
        writer.AppendLine("ldt1 = 1979-05-27T07:32:00");
        writer.AppendLine("ldt2 = 1979-05-27T00:32:00.123");
        writer.AppendLine("ldt3 = 1979-05-27T00:32:00.999999");
        writer.AppendLine("ldt4 = 1979-05-27T00:32:00.1");
        writer.AppendLine("ldt5 = 1979-05-27T00:32:00.01");
        writer.AppendLine("ldt6 = 1979-05-27T00:32:00.11");
        writer.AppendLine("ldt7 = 1979-05-27T00:32:00.001");
        writer.AppendLine("ldt8 = 1979-05-27T00:32:00.011");
        writer.AppendLine("ldt9 = 1979-05-27T00:32:00.111");
        writer.AppendLine("ldt10 = 1979-05-27T00:32:00.0001");
        writer.AppendLine("ldt11 = 1979-05-27T00:32:00.0011");
        writer.AppendLine("ldt12 = 1979-05-27T00:32:00.0111");
        writer.AppendLine("ldt13 = 1979-05-27T00:32:00.1111");
        writer.AppendLine("ldt14 = 1979-05-27T00:32:00.00001");
        writer.AppendLine("ldt15 = 1979-05-27T00:32:00.00011");
        writer.AppendLine("ldt16 = 1979-05-27T00:32:00.00111");
        writer.AppendLine("ldt17 = 1979-05-27T00:32:00.01111");
        writer.AppendLine("ldt18 = 1979-05-27T00:32:00.11111");
        writer.AppendLine("ldt19 = 1979-05-27T00:32:00.000001");
        writer.AppendLine("ldt20 = 1979-05-27T00:32:00.000011");
        writer.AppendLine("ldt21 = 1979-05-27T00:32:00.000111");
        writer.AppendLine("ldt22 = 1979-05-27T00:32:00.001111");
        writer.AppendLine("ldt23 = 1979-05-27T00:32:00.011111");
        writer.AppendLine("ldt24 = 1979-05-27T00:32:00.111111");
        writer.Flush();

        buffer.ToArray().ShouldBe(serializeText.ByteSpan.ToArray());
    }
}

public class TomlLocalDateTest
{
    [Fact]
    public void DeserializeAndSerialize()
    {
        var toml = @"
ld1 = 1979-05-27
ld2 = 1979-01-01
ld3 = 1979-12-31
"u8;

        var document = CsTomlSerializer.Deserialize<TomlDocument>(toml);
        using var serializeText = CsTomlSerializer.Serialize(document!);

        using var buffer = Utf8String.CreateWriter(out var writer);
        writer.AppendLine("ld1 = 1979-05-27");
        writer.AppendLine("ld2 = 1979-01-01");
        writer.AppendLine("ld3 = 1979-12-31");
        writer.Flush();

        buffer.ToArray().ShouldBe(serializeText.ByteSpan.ToArray());
    }
}

public class TomlLocalTimeTest
{
    [Fact]
    public void DeserializeAndSerialize()
    {
        var toml = @"
lt1 = 07:32:00
lt2 = 00:32:00.123
lt3 = 00:32:00.999999
lt4 = 00:32:00.1
lt5 = 00:32:00.01
lt6 = 00:32:00.11
lt7 = 00:32:00.001
lt8 = 00:32:00.011
lt9 = 00:32:00.111
lt10 = 00:32:00.0001
lt11 = 00:32:00.0011
lt12 = 00:32:00.0111
lt13 = 00:32:00.1111
lt14 = 00:32:00.00001
lt15 = 00:32:00.00011
lt16 = 00:32:00.00111
lt17 = 00:32:00.01111
lt18 = 00:32:00.11111
lt19 = 00:32:00.000001
lt20 = 00:32:00.000011
lt21 = 00:32:00.000111
lt22 = 00:32:00.001111
lt23 = 00:32:00.011111
lt24 = 00:32:00.111111
"u8;

        var document = CsTomlSerializer.Deserialize<TomlDocument>(toml);
        using var serializeText = CsTomlSerializer.Serialize(document!);

        using var buffer = Utf8String.CreateWriter(out var writer);
        writer.AppendLine("lt1 = 07:32:00");
        writer.AppendLine("lt2 = 00:32:00.123");
        writer.AppendLine("lt3 = 00:32:00.999999");
        writer.AppendLine("lt4 = 00:32:00.1");
        writer.AppendLine("lt5 = 00:32:00.01");
        writer.AppendLine("lt6 = 00:32:00.11");
        writer.AppendLine("lt7 = 00:32:00.001");
        writer.AppendLine("lt8 = 00:32:00.011");
        writer.AppendLine("lt9 = 00:32:00.111");
        writer.AppendLine("lt10 = 00:32:00.0001");
        writer.AppendLine("lt11 = 00:32:00.0011");
        writer.AppendLine("lt12 = 00:32:00.0111");
        writer.AppendLine("lt13 = 00:32:00.1111");
        writer.AppendLine("lt14 = 00:32:00.00001");
        writer.AppendLine("lt15 = 00:32:00.00011");
        writer.AppendLine("lt16 = 00:32:00.00111");
        writer.AppendLine("lt17 = 00:32:00.01111");
        writer.AppendLine("lt18 = 00:32:00.11111");
        writer.AppendLine("lt19 = 00:32:00.000001");
        writer.AppendLine("lt20 = 00:32:00.000011");
        writer.AppendLine("lt21 = 00:32:00.000111");
        writer.AppendLine("lt22 = 00:32:00.001111");
        writer.AppendLine("lt23 = 00:32:00.011111");
        writer.AppendLine("lt24 = 00:32:00.111111");
        writer.Flush();

        buffer.ToArray().ShouldBe(serializeText.ByteSpan.ToArray());
    }
}

public class TomlArrayTest
{
    [Fact]
    public void DeserializeAndSerialize()
    {
        var toml = @"
empty = []
integers = [ 1, 2, 3 ]
colors = [ ""red"", ""yellow"", ""green""]
nested_arrays_of_ints = [[1, 2], [3, 4, 5]]
nested_mixed_array = [[1, 2], [""a"", ""b"", ""c""]]
string_array = [""all"", 'strings', """"""are the same"""""", '''type''']
"u8;

        var document = CsTomlSerializer.Deserialize<TomlDocument>(toml);
        using var serializeText = CsTomlSerializer.Serialize(document!);

        using var buffer = Utf8String.CreateWriter(out var writer);
        writer.AppendLine("empty = [ ]");
        writer.AppendLine("integers = [ 1, 2, 3 ]");
        writer.AppendLine(@"colors = [ ""red"", ""yellow"", ""green"" ]");
        writer.AppendLine("nested_arrays_of_ints = [ [ 1, 2 ], [ 3, 4, 5 ] ]");
        writer.AppendLine(@"nested_mixed_array = [ [ 1, 2 ], [ ""a"", ""b"", ""c"" ] ]");
        writer.AppendLine(@"string_array = [ ""all"", 'strings', """"""are the same"""""", '''type''' ]");
        writer.Flush();

        buffer.ToArray().ShouldBe(serializeText.ByteSpan.ToArray());
    }
}

public class TomlTableTest
{
    [Fact]
    public void DeserializeAndSerialize()
    {
        var toml = @"
[table-1]
key1 = ""some string""
key2 = 123

[table-2]
key1 = ""another string""
key2 = 456

[fruit]
apple.color = ""red""
apple.taste.sweet = true

[fruit.apple.texture]
smooth = true
"u8;

        var document = CsTomlSerializer.Deserialize<TomlDocument>(toml);
        using var serializeText = CsTomlSerializer.Serialize(document!);

        using var buffer = Utf8String.CreateWriter(out var writer);
        writer.AppendLine("[table-1]");
        writer.AppendLine(@"key1 = ""some string""");
        writer.AppendLine(@"key2 = 123");
        writer.AppendLine();
        writer.AppendLine("[table-2]");
        writer.AppendLine(@"key1 = ""another string""");
        writer.AppendLine(@"key2 = 456");
        writer.AppendLine();
        writer.AppendLine("[fruit]");
        writer.AppendLine(@"apple.color = ""red""");
        writer.AppendLine(@"apple.taste.sweet = true");
        writer.AppendLine();
        writer.AppendLine("[fruit.apple.texture]");
        writer.AppendLine("smooth = true");
        writer.Flush();

        buffer.ToArray().ShouldBe(serializeText.ByteSpan.ToArray());
    }
}

public class TomlInlineTableTest
{
    [Fact]
    public void DeserializeAndSerialize()
    {
        var toml = @"
name = { first = ""CsToml"", last = ""prozolic"" }
point = { x = 1, y = 2 }
animal = { type.name = ""pug"" }
"u8;

        var document = CsTomlSerializer.Deserialize<TomlDocument>(toml);
        using var serializeText = CsTomlSerializer.Serialize(document!);

        using var buffer = Utf8String.CreateWriter(out var writer);
        writer.AppendLine(@"name = { first = ""CsToml"", last = ""prozolic"" }");
        writer.AppendLine(@"point = { x = 1, y = 2 }");
        writer.AppendLine(@"animal = { type.name = ""pug"" }");
        writer.Flush();

        buffer.ToArray().ShouldBe(serializeText.ByteSpan.ToArray());
    }
}

public class DeserializeValueTypeTest
{
    [Fact]
    public void Test()
    {
        var tomlIntValue = CsTomlSerializer.DeserializeValueType<long>("1234"u8);
        tomlIntValue.ShouldBe(1234);

        var tomlStringValue = CsTomlSerializer.DeserializeValueType<string>("\"\\U00000061\\U00000062\\U00000063\""u8);
        tomlStringValue.ShouldBe("abc");

        var tomlDateTimeValue = CsTomlSerializer.DeserializeValueType<DateTime>("2024-10-20T15:16:00"u8);
        tomlDateTimeValue.ShouldBe(new DateTime(2024, 10, 20, 15, 16, 0, DateTimeKind.Local));

        var tomlArrayValue = CsTomlSerializer.DeserializeValueType<string[]>("[ \"red\", \"yellow\", \"green\" ]"u8);
        tomlArrayValue.ShouldBe(["red", "yellow", "green"]);

        var tomlinlineTableValue = CsTomlSerializer.DeserializeValueType<IDictionary<string, object>>("{ x = 1, y = 2, z = \"3\" }"u8);
        tomlinlineTableValue.Count.ShouldBe(3);
        tomlinlineTableValue["x"].ShouldBe((object)1);
        tomlinlineTableValue["y"].ShouldBe((object)2);
        tomlinlineTableValue["z"].ShouldBe((object)"3");

        var tomlTupleValue = CsTomlSerializer.DeserializeValueType<Tuple<string, string, string>>("[ \"red\", \"yellow\", \"green\" ]"u8);
        tomlTupleValue.ShouldBe(new Tuple<string, string, string>("red", "yellow", "green"));
    }
}

public class SerializeValueTypeTest
{
    [Fact]
    public void Test()
    {
        using var serializedTomlValue1 = CsTomlSerializer.SerializeValueType(123);
        serializedTomlValue1.ByteSpan.ToArray().ShouldBe("123"u8.ToArray());

        using var serializedTomlValue2 = CsTomlSerializer.SerializeValueType("abc");
        serializedTomlValue2.ByteSpan.ToArray().ShouldBe("\"abc\""u8.ToArray());

        using var serializedTomlValue3 = CsTomlSerializer.SerializeValueType(new DateTime(2024, 10, 20, 15, 16, 0, DateTimeKind.Local));
        serializedTomlValue3.ByteSpan.ToArray().ShouldBe("2024-10-20T15:16:00"u8.ToArray());

        using var serializedTomlValue4 = CsTomlSerializer.SerializeValueType<string[]>(["red", "yellow", "green"]);
        serializedTomlValue4.ByteSpan.ToArray().ShouldBe("[ \"red\", \"yellow\", \"green\" ]"u8.ToArray());

        var dict = new Dictionary<string, object>();
        dict["x"] = 1;
        dict["y"] = 2;
        dict["z"] = "3";
        using var serializedTomlValue5 = CsTomlSerializer.SerializeValueType(dict);
        serializedTomlValue5.ByteSpan.ToArray().ShouldBe("{x = 1, y = 2, z = \"3\"}"u8.ToArray());

        using var serializedTomlValue6 = CsTomlSerializer.SerializeValueType(new Tuple<string, string, string>("red", "yellow", "green"));
        serializedTomlValue6.ByteSpan.ToArray().ShouldBe("[ \"red\", \"yellow\", \"green\" ]"u8.ToArray());
    }
}

public class MulitipleThreadTest
{
    [Fact]
    public async Task ExecuteAsync()
    {
        var tomlText = @"
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
        var numbers = Enumerable.Range(1, 10000).ToArray();
        var document = CsTomlSerializer.Deserialize<TomlDocument>(tomlText);
        var expected = document!.ToJsonObject();

        await Parallel.ForEachAsync(numbers, (number, token) =>
        {
            var document2 = CsTomlSerializer.Deserialize<TomlDocument>(tomlText);
            var actual = document2!.ToJsonObject();
            JsonNodeExtensions.DeepEqualsForTomlFormat(actual, expected).ShouldBeTrue();
            return ValueTask.CompletedTask;
        });
    }

}

public class TomlDocumentTest
{
    [Fact]
    public void Execute()
    {
        var tomlText = @"
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

"u8;

        var document = CsTomlSerializer.Deserialize<TomlDocument>(tomlText).ToDictionary<object, object>();
        document["str"].ShouldBe("value");
        document["int"].ShouldBe(123);
        document["flt"].ShouldBe(3.1415d);
        ((bool)document["boolean"]).ShouldBeTrue();
        document["odt1"].ShouldBe(new DateTimeOffset(1979, 5, 27, 7, 32, 0, TimeSpan.Zero));
        document["ldt1"].ShouldBe(new DateTime(1979, 5, 27, 7, 32, 0));
        document["ldt2"].ShouldBe(new DateTime(1979, 5, 27, 0, 32, 0, 999, 999));
        document["ld1"].ShouldBe(new DateOnly(1979, 5, 27));
        document["lt1"].ShouldBe(new TimeOnly(7, 32, 0));
        document["key"].ShouldBe("value");
        ((IDictionary<object, object>)((IDictionary<object, object>)document["first"])["second"])["third"].ShouldBe("value");
        document["number"].ShouldBe(123456);
        ((object[])document["array"])[0].ShouldBe(123);
        ((object[])document["array"])[1].ShouldBe("456");
        ((bool)((object[])document["array"])[2]).ShouldBeTrue();

        var inlineTable = (IDictionary<object, object>)document["inlineTable"];
        inlineTable["key"].ShouldBe(1);
        inlineTable["key2"].ShouldBe("value");
        ((object[])inlineTable["key3"])[0].ShouldBe(123);
        ((object[])inlineTable["key3"])[1].ShouldBe(456);
        ((object[])inlineTable["key3"])[2].ShouldBe(789);
        ((IDictionary<object, object>)inlineTable["key4"])["key"].ShouldBe("inlinetable");

        var table = (IDictionary<object, object>)document["Table"];
        var test = (IDictionary<object, object>)table["test"];
        test["key"].ShouldBe("value");
        ((IDictionary<object, object>)((IDictionary<object, object>)test["first"])["second"])["third"].ShouldBe("value");
        test["number"].ShouldBe(123456);
        ((object[])inlineTable["key3"])[0].ShouldBe(123);
        ((object[])inlineTable["key3"])[1].ShouldBe(456);
        ((object[])inlineTable["key3"])[2].ShouldBe(789);
        ((IDictionary<object, object>)inlineTable["key4"])["key"].ShouldBe("inlinetable");

        var arrayOfTables = (IDictionary<object, object>)document["arrayOfTables"];
        var testArray = (object[])arrayOfTables["test"];

        ((IDictionary<object, object>)testArray[0])["key"].ShouldBe("value");
        ((IDictionary<object, object>)((IDictionary<object, object>)((IDictionary<object, object>)testArray[0])["first"])["second"])["third"].ShouldBe("value");
        ((IDictionary<object, object>)testArray[0])["number"].ShouldBe(123456);
        ((IDictionary<object, object>)testArray[2])["key2"].ShouldBe("value");
        ((IDictionary<object, object>)((IDictionary<object, object>)((IDictionary<object, object>)testArray[2])["first2"])["second2"])["third2"].ShouldBe("value");
        ((IDictionary<object, object>)testArray[2])["number2"].ShouldBe(123456);
    }
}

public class DynamicTest
{
    [Fact]
    public void Execute()
    {
        var tomlText = @"
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

"u8;

        var document = (IDictionary<object, object>)CsTomlSerializer.Deserialize<dynamic>(tomlText);
        document["str"].ShouldBe("value");
        document["int"].ShouldBe(123);
        document["flt"].ShouldBe(3.1415d);
        ((bool)document["boolean"]).ShouldBeTrue();
        document["odt1"].ShouldBe(new DateTimeOffset(1979, 5, 27, 7, 32, 0, TimeSpan.Zero));
        document["ldt1"].ShouldBe(new DateTime(1979, 5, 27, 7, 32, 0));
        document["ldt2"].ShouldBe(new DateTime(1979, 5, 27, 0, 32, 0, 999, 999));
        document["ld1"].ShouldBe(new DateOnly(1979, 5, 27));
        document["lt1"].ShouldBe(new TimeOnly(7, 32, 0));
        document["key"].ShouldBe("value");
        ((IDictionary<object, object>)((IDictionary<object, object>)document["first"])["second"])["third"].ShouldBe("value");
        document["number"].ShouldBe(123456);
        ((object[])document["array"])[0].ShouldBe(123);
        ((object[])document["array"])[1].ShouldBe("456");
        ((bool)((object[])document["array"])[2]).ShouldBeTrue();

        var inlineTable = (IDictionary<object, object>)document["inlineTable"];
        inlineTable["key"].ShouldBe(1);
        inlineTable["key2"].ShouldBe("value");
        ((object[])inlineTable["key3"])[0].ShouldBe(123);
        ((object[])inlineTable["key3"])[1].ShouldBe(456);
        ((object[])inlineTable["key3"])[2].ShouldBe(789);
        ((IDictionary<object, object>)inlineTable["key4"])["key"].ShouldBe("inlinetable");

        var table = (IDictionary<object, object>)document["Table"];
        var test = (IDictionary<object, object>)table["test"];
        test["key"].ShouldBe("value");
        ((IDictionary<object, object>)((IDictionary<object, object>)test["first"])["second"])["third"].ShouldBe("value");
        test["number"].ShouldBe(123456);
        ((object[])inlineTable["key3"])[0].ShouldBe(123);
        ((object[])inlineTable["key3"])[1].ShouldBe(456);
        ((object[])inlineTable["key3"])[2].ShouldBe(789);
        ((IDictionary<object, object>)inlineTable["key4"])["key"].ShouldBe("inlinetable");

        var arrayOfTables = (IDictionary<object, object>)document["arrayOfTables"];
        var testArray = (object[])arrayOfTables["test"];

        ((IDictionary<object, object>)testArray[0])["key"].ShouldBe("value");
        ((IDictionary<object, object>)((IDictionary<object, object>)((IDictionary<object, object>)testArray[0])["first"])["second"])["third"].ShouldBe("value");
        ((IDictionary<object, object>)testArray[0])["number"].ShouldBe(123456);
        ((IDictionary<object, object>)testArray[2])["key2"].ShouldBe("value");
        ((IDictionary<object, object>)((IDictionary<object, object>)((IDictionary<object, object>)testArray[2])["first2"])["second2"])["third2"].ShouldBe("value");
        ((IDictionary<object, object>)testArray[2])["number2"].ShouldBe(123456);
    }
}