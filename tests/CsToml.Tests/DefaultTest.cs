
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
        var package = CsTomlSerializer.Deserialize<CsTomlPackage>(tomlText);

        var serializeText = CsTomlSerializer.Serialize(package!);
    }

    [Fact]
    public void TryGetValueTest()
    {
        var package = CsTomlSerializer.Deserialize<CsTomlPackage>(tomlText);

        {
            Assert.True(package!.TryFind("key"u8, out var value));
            Assert.Equal("value", value?.GetString());
        }
        {
            Assert.False(package.TryFind("first.second.third"u8, out var _));
            Assert.True(package.TryFind("first.second.third"u8, out var value, true));
            Assert.Equal("value", value?.GetString());

            Assert.False(package.TryFind("first.second"u8, out var _));
            Assert.False(package.TryFind("first.second"u8, out var value2, true));
        }
        {
            var result = package.TryFind("key", out var value);
            Assert.True(result);
            Assert.Equal("value", value?.GetString());
        }
        {
            Assert.False(package.TryFind("first.second.third", out var _));
            Assert.True(package.TryFind("first.second.third", out var value, true));
            Assert.Equal("value", value?.GetString());
        }
        {
            Assert.True(package.TryFind(["first"u8, "second"u8, "third"u8], out var value));
            Assert.Equal("value", value?.GetString());
        }
        {
            Assert.True(package.TryFind("number"u8, out var value));
            Assert.Equal(123456, value?.GetInt64());
        }
        {
            Assert.False(package.TryFind("Table.test"u8, "key"u8, out var _));
            Assert.True(package.TryFind("Table.test"u8, "key"u8, out var value, true));
            Assert.Equal("value", value?.GetString());

            Assert.True(package.TryFind(["Table"u8, "test"u8], "key"u8, out var value2));
            Assert.Equal("value", value2?.GetString());

            Assert.False(package.TryFind("Table.test"u8, ["first"u8, "second"u8, "third"u8], out var __, false));
            Assert.True(package.TryFind("Table.test"u8, ["first"u8, "second"u8, "third"u8], out var value3, true));
            Assert.Equal("value", value3?.GetString());

            Assert.True(package.TryFind(["Table"u8, "test"u8], ["first"u8, "second"u8, "third"u8], out var value4));
            Assert.Equal("value", value4?.GetString());
        }
        {
            Assert.False(package.TryFind("arrayOfTables.test"u8, 0, "key"u8, out var _));
            Assert.True(package.TryFind("arrayOfTables.test"u8, 0, "key"u8, out var value, true));
            Assert.Equal("value", value?.GetString());

            Assert.False(package.TryFind("arrayOfTables.test"u8, 0, "first.second.third"u8, out var __));
            Assert.False(package.TryFind("arrayOfTables.test"u8, 0, "first.second.third"u8, out var ___, isTableHeaderAsDottedKeys:true));
            Assert.False(package.TryFind("arrayOfTables.test"u8, 0, "first.second.third"u8, out var ____, isDottedKeys: true));
            Assert.True(package.TryFind("arrayOfTables.test"u8, 0, "first.second.third"u8, out var value2, true, true));
            Assert.Equal("value", value2?.GetString());
        }
        {
            Assert.False(package.TryFind(["arrayOfTables"u8, "test"u8], 0, "first.second.third"u8, out var _));
            Assert.True(package.TryFind(["arrayOfTables"u8, "test"u8], 0, "first.second.third"u8, out var value, true));
            Assert.Equal("value", value?.GetString());
            Assert.False(package.TryFind("arrayOfTables.test"u8, 0, ["first"u8, "second"u8, "third"u8], out var __));
            Assert.True(package.TryFind("arrayOfTables.test"u8, 0, ["first"u8, "second"u8, "third"u8], out var value2, true));
            Assert.Equal("value", value2?.GetString());
            Assert.True(package.TryFind(["arrayOfTables"u8, "test"u8], 0, ["first"u8, "second"u8, "third"u8], out var value3));
            Assert.Equal("value", value3?.GetString());
        }
        {
            Assert.False(package.TryFind("inlineTable.key"u8, out var _));
            Assert.True(package.TryFind("inlineTable.key"u8, out var value, true));
            Assert.Equal(1, value!.GetInt64());
            Assert.False(package.TryFind("inlineTable.key2"u8, out var _));
            Assert.True(package.TryFind("inlineTable.key2"u8, out var value2, true));
            Assert.Equal("value", value2?.GetString());
            Assert.False(package.TryFind("inlineTable.key4.key"u8, out var _));
            Assert.True(package.TryFind("inlineTable.key4.key"u8, out var value3, true));
            Assert.Equal("inlinetable", value3?.GetString());

            Assert.True(package.TryFind("inlineTable"u8, out var value4));
            Assert.True(value4!.TryFind("key"u8, out var value5));
            Assert.Equal(1, value5!.GetInt64());
        }
        {
            var result = package.TryFind("failed"u8, out var value);
            Assert.False(result);
            Assert.Null(value);
        }

    }

    [Fact]
    public void FindTest()
    {
        var package = CsTomlSerializer.Deserialize<CsTomlPackage>(tomlText)!;

        {
            Assert.Equal("value", package.Find("key"u8)?.GetString());
        }
        {
            Assert.Null(package.Find("first.second.third"u8));
            Assert.Equal("value", package.Find("first.second.third"u8, true)?.GetString());
        }
        {
            Assert.Equal("value", package.Find("key")?.GetString());
        }
        {
            Assert.Null(package.Find("first.second.third"));
            Assert.Equal("value", package.Find("first.second.third", true)?.GetString());
        }
        {
            Assert.Equal("value", package.Find(["first"u8, "second"u8, "third"u8])?.GetString());
        }
        {
            Assert.Equal(123456, package.Find("number"u8)?.GetInt64());
        }
        {
            Assert.Null(package.Find("Table.test"u8, "key"u8));
            Assert.Equal("value", package.Find("Table.test"u8, "key"u8, true)?.GetString());
        }
        {
            Assert.Null(package.Find("arrayOfTables.test"u8, 0, "key"u8));
            Assert.Equal("value", package.Find("arrayOfTables.test"u8, 0, "key"u8, isTableHeaderAsDottedKeys:true)?.GetString());

            Assert.Null(package.Find("arrayOfTables.test"u8, 0, "first.second.third"u8));
            Assert.Null(package.Find("arrayOfTables.test"u8, 0, "first.second.third"u8, isDottedKeys:true));
            Assert.Null(package.Find("arrayOfTables.test"u8, 0, "first.second.third"u8, isTableHeaderAsDottedKeys:true));
            Assert.Equal("value", package.Find("arrayOfTables.test"u8, 0, "first.second.third"u8, true, true)?.GetString());
        }
        {
            Assert.Null(package.Find(["arrayOfTables"u8, "test"u8], 0, "first.second.third"u8));
            Assert.Equal("value", package.Find(["arrayOfTables"u8, "test"u8], 0, "first.second.third"u8, true)?.GetString());
            Assert.Null(package.Find("arrayOfTables.test"u8, 0, ["first"u8, "second"u8, "third"u8]));
            Assert.Equal("value", package.Find("arrayOfTables.test"u8, 0, ["first"u8, "second"u8, "third"u8], true)?.GetString());
            Assert.Equal("value", package.Find(["arrayOfTables"u8, "test"u8], 0, ["first"u8, "second"u8, "third"u8])?.GetString());
        }
        {
            Assert.Null(package.Find("inlineTable.key"u8));
            Assert.Equal(1, (package.Find("inlineTable.key"u8,true)!.GetInt64()));
            Assert.Null(package.Find("inlineTable.key2"u8));
            Assert.Equal("value", package.Find("inlineTable.key2"u8, true)?.GetString());
            Assert.Null(package.Find("inlineTable.key4.key"u8));
            Assert.Equal("inlinetable", package.Find("inlineTable.key4.key"u8, true)?.GetString());

            Assert.Equal(1, package!.Find("inlineTable"u8)!.Find("key"u8)!.GetInt64());
        }
        {
            var value = package.Find("failed"u8);
            Assert.Null(value);
        }

    }

    [Fact]
    public void IndexerTest()
    {
        var package = CsTomlSerializer.Deserialize<CsTomlPackage>(tomlText)!;

        Assert.Equal("value", package!.RootNode["key"u8].GetString());
        Assert.Equal("value", package!.RootNode["first"u8]["second"u8]["third"u8].GetString());
        Assert.Equal(123456, package!.RootNode["number"u8].GetInt64());
        Assert.Equal("value", package!.RootNode["Table"u8]["test"u8]["key"u8].GetString());
        Assert.Equal("value", package!.RootNode["arrayOfTables"u8]["test"u8][0]["key"u8].GetString());
        Assert.Equal("inlinetable", package!.RootNode["inlineTable"u8]["key4"u8]["key"u8].GetString());

        Assert.False(package!.RootNode["failed"u8].HasValue);
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

        var package = CsTomlSerializer.Deserialize<CsTomlPackage>(tomlText);
        {
            var v = package.Find("str"u8)!;
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

