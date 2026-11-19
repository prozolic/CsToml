using CsToml.Error;
using CsToml.Values;
using System.Collections.Immutable;

namespace CsToml.Tests;

public class GetValueTest
{
    private readonly TomlDocument document;

    public GetValueTest()
    {
        var tomlText = @"
str = ""string""
str2 = ""123""
str4 = """"""\
       The quick brown \
       fox jumps over \
       the lazy dog.\
       """"""
str5 = 'this is str5.'
str6 = '''t\h\i\s i\s s\t\r\6'''
int = 99
flt = 1.0
bool = true
odt = 1979-05-27T07:32:00Z
ldt = 1979-05-27T07:32:00
ld1 = 1979-05-27
lt1 = 07:32:30
array = [ 1, 2, 3]

""127.0.0.1"" = ""value""

[Table]
value = 100

[Dotted.Table]
value = 200

[[ArrayOfTables]]
value = 1

[[ArrayOfTables]]
value2 = 2

[[ArrayOfTables]]
value3 = 3

"u8;
        document = CsTomlSerializer.Deserialize<TomlDocument>(tomlText);
    }

    [Test]
    public void StringTest()
    {
        var value = document.RootNode["str"u8];
        value!.GetString().ShouldBe("string");
        value[0].HasValue.ShouldBeFalse();

        Assert.ThrowsExactly<CsTomlException>(() => value!.GetArray());
        Assert.ThrowsExactly<CsTomlException>(() => value!.GetArraySpan());
        Assert.ThrowsExactly<CsTomlException>(() => value!.GetInt64());
        Assert.ThrowsExactly<CsTomlException>(() => value!.GetDouble());
        Assert.ThrowsExactly<CsTomlException>(() => value!.GetBool());
        Assert.ThrowsExactly<CsTomlException>(() => value!.GetDateTime());
        Assert.ThrowsExactly<CsTomlException>(() => value!.GetDateTimeOffset());
        Assert.ThrowsExactly<CsTomlException>(() => value!.GetDateOnly());
        Assert.ThrowsExactly<CsTomlException>(() => value!.GetTimeOnly());
    }

    [Test]
    public void TryStringTest()
    {
        var value = document.RootNode["str"u8];
        {
            value!.TryGetString(out var v).ShouldBeTrue();
            v.ShouldBe("string");
        }

        value!.TryGetInt64(out var _).ShouldBeFalse();
        value!.TryGetDouble(out var _).ShouldBeFalse();
        value!.TryGetBool(out var _).ShouldBeFalse();
        value!.TryGetDateTime(out var _).ShouldBeFalse();
        value!.TryGetDateTimeOffset(out var _).ShouldBeFalse();
        value!.TryGetDateOnly(out var _).ShouldBeFalse();
        value!.TryGetTimeOnly(out var _).ShouldBeFalse();
        value!.TryGetArray(out var array).ShouldBeFalse();
        array.IsEmpty.ShouldBeTrue();
    }

    [Test]
    public void StringTest2()
    {
        var value = document.RootNode["str2"u8];
        value!.GetString().ShouldBe("123");
        Assert.ThrowsExactly<CsTomlException>(() => value!.GetInt64());
        value!.TryGetInt64(out var _).ShouldBeFalse();
    }

    [Test]
    public void StringTest4()
    {
        var value = document.RootNode["str4"u8];
        value!.GetString().ShouldBe("The quick brown fox jumps over the lazy dog.");
    }

    [Test]
    public void StringTest5()
    {
        var value = document.RootNode["str5"u8];
        value!.GetString().ShouldBe("this is str5.");
    }

    [Test]
    public void StringTest6()
    {
        var value = document.RootNode["str6"u8];
        value!.GetString().ShouldBe("t\\h\\i\\s i\\s s\\t\\r\\6");
    }

    [Test]
    public void IntTest()
    {
        var value = document.RootNode["int"u8];
        value!.GetInt64().ShouldBe(99);
        value!.GetDouble().ShouldBe(99d);
        Assert.ThrowsExactly<CsTomlException>(() => value!.GetString());
        Assert.ThrowsExactly<CsTomlException>(() => value!.GetBool());
        Assert.ThrowsExactly<CsTomlException>(() => value!.GetDateTime());
        Assert.ThrowsExactly<CsTomlException>(() => value!.GetDateTimeOffset());
        Assert.ThrowsExactly<CsTomlException>(() => value!.GetDateOnly());
        Assert.ThrowsExactly<CsTomlException>(() => value!.GetTimeOnly());
    }

    [Test]
    public void TryIntTest()
    {
        var value = document.RootNode["int"u8];
        {
            value!.TryGetInt64(out var v).ShouldBeTrue();
            v.ShouldBe(99);
        }
        {
            value!.TryGetDouble(out var v).ShouldBeTrue();
            v.ShouldBe(99d);
        }

        value!.TryGetString(out var _).ShouldBeFalse();
        value!.TryGetBool(out var _).ShouldBeFalse();
        value!.TryGetDateTime(out var _).ShouldBeFalse();
        value!.TryGetDateTimeOffset(out var _).ShouldBeFalse();
        value!.TryGetDateOnly(out var _).ShouldBeFalse();
        value!.TryGetTimeOnly(out var _).ShouldBeFalse();
    }

    [Test]
    public void FloatTest()
    {
        var value = document.RootNode["flt"u8];
        value!.GetDouble().ShouldBe(1.0d);
        Assert.ThrowsExactly<CsTomlException>(() => value!.GetInt64());
        Assert.ThrowsExactly<CsTomlException>(() => value!.GetString());
        Assert.ThrowsExactly<CsTomlException>(() => value!.GetBool());
        Assert.ThrowsExactly<CsTomlException>(() => value!.GetDateTime());
        Assert.ThrowsExactly<CsTomlException>(() => value!.GetDateTimeOffset());
        Assert.ThrowsExactly<CsTomlException>(() => value!.GetDateOnly());
        Assert.ThrowsExactly<CsTomlException>(() => value!.GetTimeOnly());
    }

    [Test]
    public void TryFloatTest()
    {
        var value = document.RootNode["flt"u8];
        {
            value!.TryGetDouble(out var v).ShouldBeTrue();
            v.ShouldBe(1.0d);
        }

        value!.TryGetInt64(out var _).ShouldBeFalse();
        value!.TryGetString(out var _).ShouldBeFalse();
        value!.TryGetBool(out var _).ShouldBeFalse();
        value!.TryGetDateTime(out var _).ShouldBeFalse();
        value!.TryGetDateTimeOffset(out var _).ShouldBeFalse();
        value!.TryGetDateOnly(out var _).ShouldBeFalse();
        value!.TryGetTimeOnly(out var _).ShouldBeFalse();
    }

    [Test]
    public void BoolTest()
    {
        var value = document.RootNode["bool"u8];
        value!.GetBool().ShouldBeTrue();
        Assert.ThrowsExactly<CsTomlException>(() => value!.GetString());
        Assert.ThrowsExactly<CsTomlException>(() => value!.GetInt64());
        Assert.ThrowsExactly<CsTomlException>(() => value!.GetDouble());
        Assert.ThrowsExactly<CsTomlException>(() => value!.GetDateTime());
        Assert.ThrowsExactly<CsTomlException>(() => value!.GetDateTimeOffset());
        Assert.ThrowsExactly<CsTomlException>(() => value!.GetDateOnly());
        Assert.ThrowsExactly<CsTomlException>(() => value!.GetTimeOnly());
    }

    [Test]
    public void TryBoolTest()
    {
        var value = document.RootNode["bool"u8];
        {
            value!.TryGetBool(out var v).ShouldBeTrue();
            v.ShouldBeTrue();
        }

        value!.TryGetString(out var _).ShouldBeFalse();
        value!.TryGetInt64(out var _).ShouldBeFalse();
        value!.TryGetDouble(out var _).ShouldBeFalse();
        value!.TryGetDateTime(out var _).ShouldBeFalse();
        value!.TryGetDateTimeOffset(out var _).ShouldBeFalse();
        value!.TryGetDateOnly(out var _).ShouldBeFalse();
        value!.TryGetTimeOnly(out var _).ShouldBeFalse();
    }

    [Test]
    public void OffsetDateTimeTest()
    {
        var value = document.RootNode["odt"u8];
        value!.GetDateTimeOffset().ShouldBe(new DateTimeOffset(1979, 5, 27, 7, 32, 0, TimeSpan.Zero));
        Assert.ThrowsExactly<CsTomlException>(() => value!.GetString());
        Assert.ThrowsExactly<CsTomlException>(() => value!.GetInt64());
        Assert.ThrowsExactly<CsTomlException>(() => value!.GetDouble());
        Assert.ThrowsExactly<CsTomlException>(() => value!.GetBool());
        Assert.ThrowsExactly<CsTomlException>(() => value!.GetDateTime());
        Assert.ThrowsExactly<CsTomlException>(() => value!.GetDateOnly());
        Assert.ThrowsExactly<CsTomlException>(() => value!.GetTimeOnly());
    }

    [Test]
    public void TryOffsetDateTimeTest()
    {
        var value = document.RootNode["odt"u8];
        {
            value!.TryGetDateTimeOffset(out var v).ShouldBeTrue();
            v.ShouldBe(new DateTimeOffset(1979, 5, 27, 7, 32, 0, TimeSpan.Zero));
        }

        value!.TryGetString(out var _).ShouldBeFalse();
        value!.TryGetInt64(out var _).ShouldBeFalse();
        value!.TryGetDouble(out var _).ShouldBeFalse();
        value!.TryGetBool(out var _).ShouldBeFalse();
        value!.TryGetDateTime(out var _).ShouldBeFalse();
        value!.TryGetDateOnly(out var _).ShouldBeFalse();
        value!.TryGetTimeOnly(out var _).ShouldBeFalse();
    }

    [Test]
    public void LocalDateTimeTest()
    {
        var value = document.RootNode["ldt"u8];
        value!.GetDateTime().ShouldBe(new DateTime(1979, 5, 27, 7, 32, 0));
        Assert.ThrowsExactly<CsTomlException>(() => value!.GetString());
        Assert.ThrowsExactly<CsTomlException>(() => value!.GetInt64());
        Assert.ThrowsExactly<CsTomlException>(() => value!.GetDouble());
        Assert.ThrowsExactly<CsTomlException>(() => value!.GetBool());
        Assert.ThrowsExactly<CsTomlException>(() => value!.GetDateTimeOffset());
        Assert.ThrowsExactly<CsTomlException>(() => value!.GetDateOnly());
        Assert.ThrowsExactly<CsTomlException>(() => value!.GetTimeOnly());
    }

    [Test]
    public void TryLocalDateTimeTest()
    {
        var value = document.RootNode["ldt"u8];
        {
            value!.TryGetDateTime(out var v).ShouldBeTrue();
            v.ShouldBe(new DateTime(1979, 5, 27, 7, 32, 0));
        }

        value!.TryGetString(out var _).ShouldBeFalse();
        value!.TryGetInt64(out var _).ShouldBeFalse();
        value!.TryGetDouble(out var _).ShouldBeFalse();
        value!.TryGetBool(out var _).ShouldBeFalse();
        value!.TryGetDateTimeOffset(out var _).ShouldBeFalse();
        value!.TryGetDateOnly(out var _).ShouldBeFalse();
        value!.TryGetTimeOnly(out var _).ShouldBeFalse();
    }

    [Test]
    public void LocalDateTest()
    {
        var value = document.RootNode["ld1"u8];
        value!.GetDateOnly().ShouldBe(new DateOnly(1979, 5, 27));
        Assert.ThrowsExactly<CsTomlException>(() => value!.GetString());
        Assert.ThrowsExactly<CsTomlException>(() => value!.GetInt64());
        Assert.ThrowsExactly<CsTomlException>(() => value!.GetDouble());
        Assert.ThrowsExactly<CsTomlException>(() => value!.GetBool());
        Assert.ThrowsExactly<CsTomlException>(() => value!.GetDateTime());
        Assert.ThrowsExactly<CsTomlException>(() => value!.GetDateTimeOffset());
        Assert.ThrowsExactly<CsTomlException>(() => value!.GetTimeOnly());
    }

    [Test]
    public void TryLocalDateTest()
    {
        var value = document.RootNode["ld1"u8];
        {
            value!.TryGetDateOnly(out var v).ShouldBeTrue();
            v.ShouldBe(new DateOnly(1979, 5, 27));
        }

        value!.TryGetString(out var _).ShouldBeFalse();
        value!.TryGetInt64(out var _).ShouldBeFalse();
        value!.TryGetDouble(out var _).ShouldBeFalse();
        value!.TryGetBool(out var _).ShouldBeFalse();
        value!.TryGetDateTime(out var _).ShouldBeFalse();
        value!.TryGetDateTimeOffset(out var _).ShouldBeFalse();
        value!.TryGetTimeOnly(out var _).ShouldBeFalse();
    }

    [Test]
    public void LocalTimeTest()
    {
        var value = document.RootNode["lt1"u8];
        value!.GetTimeOnly().ShouldBe(new TimeOnly(7, 32, 30));
        Assert.ThrowsExactly<CsTomlException>(() => value!.GetString());
        Assert.ThrowsExactly<CsTomlException>(() => value!.GetInt64());
        Assert.ThrowsExactly<CsTomlException>(() => value!.GetDouble());
        Assert.ThrowsExactly<CsTomlException>(() => value!.GetBool());
        Assert.ThrowsExactly<CsTomlException>(() => value!.GetDateTime());
        Assert.ThrowsExactly<CsTomlException>(() => value!.GetDateTimeOffset());
        Assert.ThrowsExactly<CsTomlException>(() => value!.GetDateOnly());
    }

    [Test]
    public void TryLocalTimeTest()
    {
        var value = document.RootNode["lt1"u8];
        {
            value!.TryGetTimeOnly(out var v).ShouldBeTrue();
            v.ShouldBe(new TimeOnly(7, 32, 30));
        }

        value!.TryGetString(out var _).ShouldBeFalse();
        value!.TryGetInt64(out var _).ShouldBeFalse();
        value!.TryGetDouble(out var _).ShouldBeFalse();
        value!.TryGetBool(out var _).ShouldBeFalse();
        value!.TryGetDateTime(out var _).ShouldBeFalse();
        value!.TryGetDateTimeOffset(out var _).ShouldBeFalse();
        value!.TryGetDateOnly(out var _).ShouldBeFalse();
    }

    [Test]
    public void ArrayTest()
    {
        var value = document.RootNode["array"u8];
        var arrayValue = value!.GetArray();
        arrayValue.Select(i => i.GetInt64()).ShouldBe([1, 2, 3]);

        value[0].GetInt64().ShouldBe(1);
        Assert.ThrowsExactly<CsTomlException>(() => value!.GetString());
        Assert.ThrowsExactly<CsTomlException>(() => value!.GetInt64());
        Assert.ThrowsExactly<CsTomlException>(() => value!.GetDouble());
        Assert.ThrowsExactly<CsTomlException>(() => value!.GetBool());
        Assert.ThrowsExactly<CsTomlException>(() => value!.GetDateTime());
        Assert.ThrowsExactly<CsTomlException>(() => value!.GetDateTimeOffset());
        Assert.ThrowsExactly<CsTomlException>(() => value!.GetDateOnly());
        Assert.ThrowsExactly<CsTomlException>(() => value!.GetTimeOnly());
    }

    [Test]
    public void TryArrayTest()
    {
        var value = document.RootNode["array"u8];

        value!.TryGetArray(out var arrayValue).ShouldBeTrue();
        arrayValue.Select(i => i.GetInt64()).ShouldBe([1, 2, 3]);
        value!.TryGetArraySpan(out var arraySpan).ShouldBeTrue();
        arraySpan.Length.ShouldBe(3);

        value!.TryGetString(out var _).ShouldBeFalse();
        value!.TryGetInt64(out var _).ShouldBeFalse();
        value!.TryGetDouble(out var _).ShouldBeFalse();
        value!.TryGetBool(out var _).ShouldBeFalse();
        value!.TryGetDateTime(out var _).ShouldBeFalse();
        value!.TryGetDateTimeOffset(out var _).ShouldBeFalse();
        value!.TryGetDateOnly(out var _).ShouldBeFalse();
        value!.TryGetTimeOnly(out var _).ShouldBeFalse();
    }

    [Test]
    public void DeserializeNodeTest()
    {
        document.RootNode["int"u8].GetValue<int>().ShouldBe(99);
        document.RootNode["missing"u8].GetValue<int?>().ShouldBeNull();
        Should.Throw<CsTomlSerializeException>(() => document.RootNode["str"u8].GetValue<int?>()).InnerException!.Message.ShouldBe("Cannot deserialize a TOML String value to System.Int32.");
    }

    [Test]
    public void TryGetValueNodeTest()
    {
        document.RootNode["int"u8].TryGetValue<int>(out var value).ShouldBeTrue();
        value.ShouldBe(99);
        document.RootNode["str"u8].TryGetValue<int>(out var _).ShouldBeFalse();
        document.RootNode["missing"u8].TryGetValue<int?>(out var _).ShouldBeFalse();
    }

    [Test]
    public void EmptyTableNodeTest()
    {
        var rootNode = CsTomlSerializer.Deserialize<TomlDocument>("[E]"u8).RootNode;
        rootNode["E"u8].HasValue.ShouldBeTrue();
        Should.Throw<CsTomlSerializeException>(() => rootNode["E"u8].GetValue<int>()).InnerException!.Message.ShouldBe("Cannot deserialize a TOML Table value to System.Int32.");

        var enumerator = rootNode.GetEnumerator();
        enumerator.MoveNext().ShouldBeTrue();
        enumerator.Current.Value.HasValue.ShouldBeTrue();
    }

    [Test]
    public void TypeMismatchMessageTest()
    {
        Assert.ThrowsExactly<CsTomlException>(() => document.RootNode["int"u8].GetString()).Message.ShouldBe("A TOML Integer value cannot be retrieved as string.");
        Assert.ThrowsExactly<CsTomlException>(() => document.RootNode["Table"u8].GetString()).Message.ShouldBe("A TOML Table value cannot be retrieved as string.");
        Assert.ThrowsExactly<CsTomlException>(() => document.RootNode["missing"u8].GetString()).Message.ShouldBe("There is no value to get as string.");
    }

    [Test]
    public void QuotedkeysTest()
    {
        var value = document.RootNode["127.0.0.1"u8];
        value!.GetString().ShouldBe("value");
    }

    [Test]
    public void TableTest()
    {
        var value = document.RootNode["Table"u8]["value"u8];
        value!.GetInt64().ShouldBe(100);

        ((IDictionary<object, object>)document.RootNode["Table"u8].GetObject())["value"].ShouldBe(100L);
        document.RootNode["Table"u8].TryGetObject(out var table).ShouldBeTrue();
        ((IDictionary<object, object>)table!)["value"].ShouldBe(100L);
    }

    [Test]
    public void DottedTableTest()
    {
        var value = document.RootNode["Dotted"u8]["Table"u8]["value"u8];
        value!.GetInt64().ShouldBe(200);
    }

    [Test]
    public void ArrayOfTablesTest()
    {
        var arrayOfTables = document.RootNode["ArrayOfTables"u8];
        arrayOfTables.GetArraySpan().Length.ShouldBe(3);
        arrayOfTables.GetArray().Length.ShouldBe(3);
        arrayOfTables[3].HasValue.ShouldBeFalse();
        arrayOfTables[0]["value"u8].GetInt64().ShouldBe(1);
        arrayOfTables[1]["value2"u8].GetInt64().ShouldBe(2);
        arrayOfTables[2]["value3"u8].GetInt64().ShouldBe(3);
    }
}
