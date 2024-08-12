
using CsToml.Error;

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
    public void TryGetValueTest()
    {
        var document = CsTomlSerializer.Deserialize<TomlDocument>(tomlText);

        {
            Assert.True(document!.TryFind("key"u8, out var value));
            Assert.Equal("value", value?.GetString());
        }
        {
            Assert.False(document.TryFind("first.second.third"u8, out var _));
            Assert.True(document.TryFind("first.second.third"u8, out var value, true));
            Assert.Equal("value", value?.GetString());

            Assert.False(document.TryFind("first.second"u8, out var _));
            Assert.False(document.TryFind("first.second"u8, out var value2, true));
        }
        {
            var result = document.TryFind("key", out var value);
            Assert.True(result);
            Assert.Equal("value", value?.GetString());
        }
        {
            Assert.False(document.TryFind("first.second.third", out var _));
            Assert.True(document.TryFind("first.second.third", out var value, true));
            Assert.Equal("value", value?.GetString());
        }
        {
            Assert.True(document.TryFind(["first"u8, "second"u8, "third"u8], out var value));
            Assert.Equal("value", value?.GetString());
        }
        {
            Assert.True(document.TryFind("number"u8, out var value));
            Assert.Equal(123456, value?.GetInt64());
        }
        {
            Assert.False(document.TryFind("Table.test"u8, "key"u8, out var _));
            Assert.True(document.TryFind("Table.test"u8, "key"u8, out var value, true));
            Assert.Equal("value", value?.GetString());

            Assert.True(document.TryFind(["Table"u8, "test"u8], "key"u8, out var value2));
            Assert.Equal("value", value2?.GetString());

            Assert.False(document.TryFind("Table.test"u8, ["first"u8, "second"u8, "third"u8], out var __, false));
            Assert.True(document.TryFind("Table.test"u8, ["first"u8, "second"u8, "third"u8], out var value3, true));
            Assert.Equal("value", value3?.GetString());

            Assert.True(document.TryFind(["Table"u8, "test"u8], ["first"u8, "second"u8, "third"u8], out var value4));
            Assert.Equal("value", value4?.GetString());
        }
        {
            Assert.False(document.TryFind("arrayOfTables.test"u8, 0, "key"u8, out var _));
            Assert.True(document.TryFind("arrayOfTables.test"u8, 0, "key"u8, out var value, true));
            Assert.Equal("value", value?.GetString());

            Assert.False(document.TryFind("arrayOfTables.test"u8, 0, "first.second.third"u8, out var __));
            Assert.False(document.TryFind("arrayOfTables.test"u8, 0, "first.second.third"u8, out var ___, isTableHeaderAsDottedKeys:true));
            Assert.False(document.TryFind("arrayOfTables.test"u8, 0, "first.second.third"u8, out var ____, isDottedKeys: true));
            Assert.True(document.TryFind("arrayOfTables.test"u8, 0, "first.second.third"u8, out var value2, true, true));
            Assert.Equal("value", value2?.GetString());
        }
        {
            Assert.False(document.TryFind(["arrayOfTables"u8, "test"u8], 0, "first.second.third"u8, out var _));
            Assert.True(document.TryFind(["arrayOfTables"u8, "test"u8], 0, "first.second.third"u8, out var value, true));
            Assert.Equal("value", value?.GetString());
            Assert.False(document.TryFind("arrayOfTables.test"u8, 0, ["first"u8, "second"u8, "third"u8], out var __));
            Assert.True(document.TryFind("arrayOfTables.test"u8, 0, ["first"u8, "second"u8, "third"u8], out var value2, true));
            Assert.Equal("value", value2?.GetString());
            Assert.True(document.TryFind(["arrayOfTables"u8, "test"u8], 0, ["first"u8, "second"u8, "third"u8], out var value3));
            Assert.Equal("value", value3?.GetString());
        }
        {
            Assert.False(document.TryFind("inlineTable.key"u8, out var _));
            Assert.True(document.TryFind("inlineTable.key"u8, out var value, true));
            Assert.Equal(1, value!.GetInt64());
            Assert.False(document.TryFind("inlineTable.key2"u8, out var _));
            Assert.True(document.TryFind("inlineTable.key2"u8, out var value2, true));
            Assert.Equal("value", value2?.GetString());
            Assert.False(document.TryFind("inlineTable.key4.key"u8, out var _));
            Assert.True(document.TryFind("inlineTable.key4.key"u8, out var value3, true));
            Assert.Equal("inlinetable", value3?.GetString());

            Assert.True(document.TryFind("inlineTable"u8, out var value4));
            Assert.True(value4!.TryFind("key"u8, out var value5));
            Assert.Equal(1, value5!.GetInt64());
        }
        {
            var result = document.TryFind("failed"u8, out var value);
            Assert.False(result);
            Assert.Null(value);
        }

    }

    [Fact]
    public void FindTest()
    {
        var document = CsTomlSerializer.Deserialize<TomlDocument>(tomlText)!;

        {
            Assert.Equal("value", document.Find("key"u8)?.GetString());
        }
        {
            Assert.Null(document.Find("first.second.third"u8));
            Assert.Equal("value", document.Find("first.second.third"u8, true)?.GetString());
        }
        {
            Assert.Equal("value", document.Find("key")?.GetString());
        }
        {
            Assert.Null(document.Find("first.second.third"));
            Assert.Equal("value", document.Find("first.second.third", true)?.GetString());
        }
        {
            Assert.Equal("value", document.Find(["first"u8, "second"u8, "third"u8])?.GetString());
        }
        {
            Assert.Equal(123456, document.Find("number"u8)?.GetInt64());
        }
        {
            Assert.Null(document.Find("Table.test"u8, "key"u8));
            Assert.Equal("value", document.Find("Table.test"u8, "key"u8, true)?.GetString());
        }
        {
            Assert.Null(document.Find("arrayOfTables.test"u8, 0, "key"u8));
            Assert.Equal("value", document.Find("arrayOfTables.test"u8, 0, "key"u8, isTableHeaderAsDottedKeys:true)?.GetString());

            Assert.Null(document.Find("arrayOfTables.test"u8, 0, "first.second.third"u8));
            Assert.Null(document.Find("arrayOfTables.test"u8, 0, "first.second.third"u8, isDottedKeys:true));
            Assert.Null(document.Find("arrayOfTables.test"u8, 0, "first.second.third"u8, isTableHeaderAsDottedKeys:true));
            Assert.Equal("value", document.Find("arrayOfTables.test"u8, 0, "first.second.third"u8, true, true)?.GetString());
        }
        {
            Assert.Null(document.Find(["arrayOfTables"u8, "test"u8], 0, "first.second.third"u8));
            Assert.Equal("value", document.Find(["arrayOfTables"u8, "test"u8], 0, "first.second.third"u8, true)?.GetString());
            Assert.Null(document.Find("arrayOfTables.test"u8, 0, ["first"u8, "second"u8, "third"u8]));
            Assert.Equal("value", document.Find("arrayOfTables.test"u8, 0, ["first"u8, "second"u8, "third"u8], true)?.GetString());
            Assert.Equal("value", document.Find(["arrayOfTables"u8, "test"u8], 0, ["first"u8, "second"u8, "third"u8])?.GetString());
        }
        {
            Assert.Null(document.Find("inlineTable.key"u8));
            Assert.Equal(1, (document.Find("inlineTable.key"u8,true)!.GetInt64()));
            Assert.Null(document.Find("inlineTable.key2"u8));
            Assert.Equal("value", document.Find("inlineTable.key2"u8, true)?.GetString());
            Assert.Null(document.Find("inlineTable.key4.key"u8));
            Assert.Equal("inlinetable", document.Find("inlineTable.key4.key"u8, true)?.GetString());

            Assert.Equal(1, document!.Find("inlineTable"u8)!.Find("key"u8)!.GetInt64());
        }
        {
            var value = document.Find("failed"u8);
            Assert.Null(value);
        }

    }

    [Fact]
    public void IndexerTest()
    {
        var document = CsTomlSerializer.Deserialize<TomlDocument>(tomlText)!;

        Assert.Equal("value", document!.RootNode["key"u8].GetString());
        Assert.Equal("value", document!.RootNode["first"u8]["second"u8]["third"u8].GetString());
        Assert.Equal(123456, document!.RootNode["number"u8].GetInt64());
        Assert.Equal("value", document!.RootNode["Table"u8]["test"u8]["key"u8].GetString());
        Assert.Equal("value", document!.RootNode["arrayOfTables"u8]["test"u8][0]["key"u8].GetString());
        Assert.Equal("inlinetable", document!.RootNode["inlineTable"u8]["key4"u8]["key"u8].GetString());

        Assert.False(document!.RootNode["failed"u8].HasValue);
    }

    [Fact]
    public void GetValueTest()
    {
        //str = ""value""
        //int = 123
        //flt = 3.1415
        //boolean = true
        //odt1 = 1979 - 05 - 27T07: 32:00Z
        //ldt1 = 1979 - 05 - 27T07: 32:00
        //ldt2 = 1979 - 05 - 27T00: 32:00.999999
        //ld1 = 1979 - 05 - 27
        //lt1 = 07:32:00

        var document = CsTomlSerializer.Deserialize<TomlDocument>(tomlText);
        {
            var v = document.Find("str"u8)!;
            Assert.Throws<CsTomlException>(() => v.GetArray());
            Assert.Throws<CsTomlException>(() => v.GetArrayValue(0));
            Assert.Equal("value", v.GetString());
            Assert.Throws<CsTomlException>(() => v.GetInt64());
            Assert.Throws<CsTomlException>(() => v.GetDouble());
            Assert.Throws<CsTomlException>(() => v.GetBool());
            Assert.Throws<CsTomlException>(() => v.GetDateTime());
            Assert.Throws<CsTomlException>(() => v.GetDateTimeOffset());
            Assert.Throws<CsTomlException>(() => v.GetDateOnly());
            Assert.Throws<CsTomlException>(() => v.GetTimeOnly());
            Assert.Throws<CsTomlException>(() => v.GetNumber<long>());
            Assert.Throws<CsTomlException>(() => v.GetTimeOnly());
        }
    }

}

