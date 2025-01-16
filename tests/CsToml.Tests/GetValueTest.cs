using CsToml.Error;

namespace CsToml.Tests;

public class GetValueTest
{
    private readonly TomlDocument document;

    public GetValueTest()
    {
        var tomlText = @"
str = ""string""
str2 = ""123""
str3 = "" true ""
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

    [Fact]
    public void StringTest()
    {
        var value = document.RootNode["str"u8];
        value!.GetString().ShouldBe("string");

        Assert.Throws<CsTomlException>(() => value!.GetInt64());
        Assert.Throws<CsTomlException>(() => value!.GetDouble());
        Assert.Throws<CsTomlException>(() => value!.GetBool());
        Assert.Throws<CsTomlException>(() => value!.GetDateTime());
        Assert.Throws<CsTomlException>(() => value!.GetDateTimeOffset());
        Assert.Throws<CsTomlException>(() => value!.GetDateOnly());
        Assert.Throws<CsTomlException>(() => value!.GetTimeOnly());
        Assert.Throws<CsTomlException>(() => value!.GetNumber<int>());
    }

    [Fact]
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
        value!.TryGetNumber<int>(out var _).ShouldBeFalse();
        value!.TryGetDateTime(out var _).ShouldBeFalse();
        value!.TryGetDateTimeOffset(out var _).ShouldBeFalse();
        value!.TryGetDateOnly(out var _).ShouldBeFalse();
        value!.TryGetTimeOnly(out var _).ShouldBeFalse();
    }

    [Fact]
    public void StringTest2()
    {
        var value = document.RootNode["str2"u8];
        value!.GetString().ShouldBe("123");
        value!.GetInt64().ShouldBe(123);
        value!.GetDouble().ShouldBe(123d);
        Assert.Throws<CsTomlException>(() => value!.GetBool());
        Assert.Throws<CsTomlException>(() => value!.GetDateTime());
        Assert.Throws<CsTomlException>(() => value!.GetDateTimeOffset());
        Assert.Throws<CsTomlException>(() => value!.GetDateOnly());
        Assert.Throws<CsTomlException>(() => value!.GetTimeOnly());
        value!.GetNumber<int>().ShouldBe(123);
    }

    [Fact]
    public void TryStringTest2()
    {
        var value = document.RootNode["str2"u8];
        {
            value!.TryGetString(out var v).ShouldBeTrue();
            v.ShouldBe("123");
        }
        {
            value!.TryGetInt64(out var v).ShouldBeTrue();
            v.ShouldBe(123);
        }
        {
            value!.TryGetDouble(out var v).ShouldBeTrue();
            v.ShouldBe(123d);
        }

        value!.TryGetBool(out var _).ShouldBeFalse();
        value!.TryGetNumber<int>(out var _).ShouldBeFalse();
        value!.TryGetDateTime(out var _).ShouldBeFalse();
        value!.TryGetDateTimeOffset(out var _).ShouldBeFalse();
        value!.TryGetDateOnly(out var _).ShouldBeFalse();
        value!.TryGetTimeOnly(out var _).ShouldBeFalse();
    }

    [Fact]
    public void StringTest3()
    {
        var value = document.RootNode["str3"u8];
        value!.GetString().ShouldBe(" true ");
        Assert.Throws<CsTomlException>(() => value!.GetInt64());
        Assert.Throws<CsTomlException>(() => value!.GetDouble());
        value!.GetBool().ShouldBeTrue();
        Assert.Throws<CsTomlException>(() => value!.GetDateTime());
        Assert.Throws<CsTomlException>(() => value!.GetDateTimeOffset());
        Assert.Throws<CsTomlException>(() => value!.GetDateOnly());
        Assert.Throws<CsTomlException>(() => value!.GetTimeOnly());
        Assert.Throws<CsTomlException>(() => value!.GetNumber<int>());
    }

    [Fact]
    public void TryStringTest3()
    {
        var value = document.RootNode["str3"u8];
        {
            value!.TryGetString(out var v).ShouldBeTrue();
            v.ShouldBe(" true ");
        }
        {
            value!.TryGetBool(out var v).ShouldBeTrue();
            v.ShouldBeTrue();
        }

        value!.TryGetInt64(out var _).ShouldBeFalse();
        value!.TryGetDouble(out var _).ShouldBeFalse();
        value!.TryGetNumber<int>(out var _).ShouldBeFalse();
        value!.TryGetDateTime(out var _).ShouldBeFalse();
        value!.TryGetDateTimeOffset(out var _).ShouldBeFalse();
        value!.TryGetDateOnly(out var _).ShouldBeFalse();
        value!.TryGetTimeOnly(out var _).ShouldBeFalse();
    }

    [Fact]
    public void StringTest4()
    {
        var value = document.RootNode["str4"u8];
        value!.GetString().ShouldBe("The quick brown fox jumps over the lazy dog.");
        Assert.Throws<CsTomlException>(() => value!.GetInt64());
        Assert.Throws<CsTomlException>(() => value!.GetDouble());
        Assert.Throws<CsTomlException>(() => value!.GetBool());
        Assert.Throws<CsTomlException>(() => value!.GetDateTime());
        Assert.Throws<CsTomlException>(() => value!.GetDateTimeOffset());
        Assert.Throws<CsTomlException>(() => value!.GetDateOnly());
        Assert.Throws<CsTomlException>(() => value!.GetTimeOnly());
        Assert.Throws<CsTomlException>(() => value!.GetNumber<int>());
    }

    [Fact]
    public void StringTest5()
    {
        var value = document.RootNode["str5"u8];
        value!.GetString().ShouldBe("this is str5.");
        Assert.Throws<CsTomlException>(() => value!.GetInt64());
        Assert.Throws<CsTomlException>(() => value!.GetDouble());
        Assert.Throws<CsTomlException>(() => value!.GetBool());
        Assert.Throws<CsTomlException>(() => value!.GetDateTime());
        Assert.Throws<CsTomlException>(() => value!.GetDateTimeOffset());
        Assert.Throws<CsTomlException>(() => value!.GetDateOnly());
        Assert.Throws<CsTomlException>(() => value!.GetTimeOnly());
        Assert.Throws<CsTomlException>(() => value!.GetNumber<int>());
    }

    [Fact]
    public void StringTest6()
    {
        var value = document.RootNode["str6"u8];
        value!.GetString().ShouldBe("t\\h\\i\\s i\\s s\\t\\r\\6");
        Assert.Throws<CsTomlException>(() => value!.GetInt64());
        Assert.Throws<CsTomlException>(() => value!.GetDouble());
        Assert.Throws<CsTomlException>(() => value!.GetBool());
        Assert.Throws<CsTomlException>(() => value!.GetDateTime());
        Assert.Throws<CsTomlException>(() => value!.GetDateTimeOffset());
        Assert.Throws<CsTomlException>(() => value!.GetDateOnly());
        Assert.Throws<CsTomlException>(() => value!.GetTimeOnly());
        Assert.Throws<CsTomlException>(() => value!.GetNumber<int>());
    }

    [Fact]
    public void IntTest()
    {
        var value = document.RootNode["int"u8];
        value!.GetString().ShouldBe("99");
        value!.GetInt64().ShouldBe(99);
        value!.GetDouble().ShouldBe(99d);
        value!.GetBool().ShouldBeTrue();
        Assert.Throws<CsTomlException>(() => value!.GetDateTime());
        Assert.Throws<CsTomlException>(() => value!.GetDateTimeOffset());
        Assert.Throws<CsTomlException>(() => value!.GetDateOnly());
        Assert.Throws<CsTomlException>(() => value!.GetTimeOnly());
        value!.GetNumber<int>().ShouldBe(99);
    }

    [Fact]
    public void TryIntTest()
    {
        var value = document.RootNode["int"u8];
        {
            value!.TryGetString(out var v).ShouldBeTrue();
            v.ShouldBe("99");
        }
        {
            value!.TryGetInt64(out var v).ShouldBeTrue();
            v.ShouldBe(99);
        }
        {
            value!.TryGetDouble(out var v).ShouldBeTrue();
            v.ShouldBe(99d);
        }
        {
            value!.TryGetBool(out var v).ShouldBeTrue();
            v.ShouldBeTrue();
        }
        {
            value!.TryGetNumber<int>(out var v).ShouldBeTrue();
            v.ShouldBe(99);
        }

        value!.TryGetDateTime(out var _).ShouldBeFalse();
        value!.TryGetDateTimeOffset(out var _).ShouldBeFalse();
        value!.TryGetDateOnly(out var _).ShouldBeFalse();
        value!.TryGetTimeOnly(out var _).ShouldBeFalse();
    }

    [Fact]
    public void FloatTest()
    {
        var value = document.RootNode["flt"u8];
        value!.GetString().ShouldBe("1");
        value!.GetInt64().ShouldBe(1);
        value!.GetDouble().ShouldBe(1.0d);
        value!.GetBool().ShouldBeTrue();
        Assert.Throws<CsTomlException>(() => value!.GetDateTime());
        Assert.Throws<CsTomlException>(() => value!.GetDateTimeOffset());
        Assert.Throws<CsTomlException>(() => value!.GetDateOnly());
        Assert.Throws<CsTomlException>(() => value!.GetTimeOnly());
        value!.GetNumber<int>().ShouldBe(1);
    }

    [Fact]
    public void TryFloatTest()
    {
        var value = document.RootNode["flt"u8];
        {
            value!.TryGetString(out var v).ShouldBeTrue();
            v.ShouldBe("1");
        }
        {
            value!.TryGetInt64(out var v).ShouldBeTrue();
            v.ShouldBe(1);
        }
        {
            value!.TryGetDouble(out var v).ShouldBeTrue();
            v.ShouldBe(1.0d);
        }
        {
            value!.TryGetBool(out var v).ShouldBeTrue();
            v.ShouldBeTrue();
        }
        {
            value!.TryGetNumber<int>(out var v).ShouldBeTrue();
            v.ShouldBe(1);
        }

        value!.TryGetDateTime(out var _).ShouldBeFalse();
        value!.TryGetDateTimeOffset(out var _).ShouldBeFalse();
        value!.TryGetDateOnly(out var _).ShouldBeFalse();
        value!.TryGetTimeOnly(out var _).ShouldBeFalse();
    }

    [Fact]
    public void BoolTest()
    {
        var value = document.RootNode["bool"u8];
        value!.GetString().ShouldBe("True");
        value!.GetInt64().ShouldBe(1);
        value!.GetDouble().ShouldBe(1d);
        value!.GetBool().ShouldBeTrue();
        Assert.Throws<CsTomlException>(() => value!.GetDateTime());
        Assert.Throws<CsTomlException>(() => value!.GetDateTimeOffset());
        Assert.Throws<CsTomlException>(() => value!.GetDateOnly());
        Assert.Throws<CsTomlException>(() => value!.GetTimeOnly());
        value!.GetNumber<int>().ShouldBe(1);
    }

    [Fact]
    public void TryBoolTest()
    {
        var value = document.RootNode["bool"u8];
        {
            value!.TryGetString(out var v).ShouldBeTrue();
            v.ShouldBe("True");
        }
        {
            value!.TryGetInt64(out var v).ShouldBeTrue();
            v.ShouldBe(1);
        }
        {
            value!.TryGetDouble(out var v).ShouldBeTrue();
            v.ShouldBe(1.0d);
        }
        {
            value!.TryGetBool(out var v).ShouldBeTrue();
            v.ShouldBeTrue();
        }
        {
            value!.TryGetNumber<int>(out var v).ShouldBeTrue();
            v.ShouldBe(1);
        }

        value!.TryGetDateTime(out var _).ShouldBeFalse();
        value!.TryGetDateTimeOffset(out var _).ShouldBeFalse();
        value!.TryGetDateOnly(out var _).ShouldBeFalse();
        value!.TryGetTimeOnly(out var _).ShouldBeFalse();
    }

    [Fact]
    public void OffsetDateTimeTest()
    {
        var value = document.RootNode["odt"u8];
        value!.GetString().ShouldBe(new DateTimeOffset(1979, 5, 27, 7, 32, 0, TimeSpan.Zero).ToString());
        Assert.Throws<CsTomlException>(() => value!.GetInt64());
        Assert.Throws<CsTomlException>(() => value!.GetDouble());
        Assert.Throws<CsTomlException>(() => value!.GetBool());

        var utcTime1 = DateTime.SpecifyKind(new DateTime(1979, 5, 27, 7, 32, 0), DateTimeKind.Utc);
        DateTimeOffset utcTime2 = utcTime1;

        value!.GetDateTime().ShouldBe(utcTime1);
        value!.GetDateTimeOffset().ShouldBe(utcTime2);
        value!.GetDateOnly().ShouldBe(new DateOnly(1979, 5, 27));
        value!.GetTimeOnly().ShouldBe(new TimeOnly(7, 32, 0));
        Assert.Throws<CsTomlException>(() => value!.GetNumber<int>());
    }

    [Fact]
    public void TryOffsetDateTimeTest()
    {
        var value = document.RootNode["odt"u8];
        {
            value!.TryGetString(out var v).ShouldBeTrue();
            v.ShouldBe(new DateTimeOffset(1979, 5, 27, 7, 32, 0, TimeSpan.Zero).ToString());
        }
        var utcTime1 = DateTime.SpecifyKind(new DateTime(1979, 5, 27, 7, 32, 0), DateTimeKind.Utc);
        DateTimeOffset utcTime2 = utcTime1;
        {
            value!.TryGetDateTime(out var v).ShouldBeTrue();
            v.ShouldBe(utcTime1);
        }
        {
            value!.TryGetDateTimeOffset(out var v).ShouldBeTrue();
            v.ShouldBe(utcTime2);
        }
        {
            value!.TryGetDateOnly(out var v).ShouldBeTrue();
            v.ShouldBe(new DateOnly(1979, 5, 27));
        }
        {
            value!.TryGetTimeOnly(out var v).ShouldBeTrue();
            v.ShouldBe(new TimeOnly(7, 32, 0));
        }

        value!.TryGetInt64(out var _).ShouldBeFalse();
        value!.TryGetDouble(out var _).ShouldBeFalse();
        value!.TryGetBool(out var _).ShouldBeFalse();
        value!.TryGetNumber<int>(out var _).ShouldBeFalse();
    }

    [Fact]
    public void LocalDateTimeTest()
    {
        var value = document.RootNode["ldt"u8];
        value.GetString()!.ShouldBe(new DateTime(1979, 5, 27, 7, 32, 0).ToString());
        Assert.Throws<CsTomlException>(() => value!.GetInt64());
        Assert.Throws<CsTomlException>(() => value!.GetDouble());
        Assert.Throws<CsTomlException>(() => value!.GetBool());

        var utcTime1 = new DateTime(1979, 5, 27, 7, 32, 0);
        DateTimeOffset utcTime2 = utcTime1;
        value!.GetDateTime().ShouldBe(utcTime1);
        value!.GetDateTimeOffset().ShouldBe(utcTime2);
        value!.GetDateOnly().ShouldBe(new DateOnly(1979, 5, 27));
        value!.GetTimeOnly().ShouldBe(new TimeOnly(7, 32, 0));
        Assert.Throws<CsTomlException>(() => value!.GetNumber<int>());
    }

    [Fact]
    public void TryLocalDateTimeTest()
    {
        var value = document.RootNode["ldt"u8];
        {
            value!.TryGetString(out var v).ShouldBeTrue();
            v.ShouldBe(new DateTime(1979, 5, 27, 7, 32, 0).ToString());
        }

        var utcTime1 = new DateTime(1979, 5, 27, 7, 32, 0);
        DateTimeOffset utcTime2 = utcTime1;
        {
            value!.TryGetDateTime(out var v).ShouldBeTrue();
            v.ShouldBe(utcTime1);
        }
        {
            value!.TryGetDateTimeOffset(out var v).ShouldBeTrue();
            v.ShouldBe(utcTime2);
        }
        {
            value!.TryGetDateOnly(out var v).ShouldBeTrue();
            v.ShouldBe(new DateOnly(1979, 5, 27));
        }
        {
            value!.TryGetTimeOnly(out var v).ShouldBeTrue();
            v.ShouldBe(new TimeOnly(7, 32, 0));
        }

        value!.TryGetInt64(out var _).ShouldBeFalse();
        value!.TryGetDouble(out var _).ShouldBeFalse();
        value!.TryGetBool(out var _).ShouldBeFalse();
        value!.TryGetNumber<int>(out var _).ShouldBeFalse();
    }

    [Fact]
    public void LocalDateTest()
    {
        var value = document.RootNode["ld1"u8];
        value!.GetString()!.ShouldBe(new DateOnly(1979, 5, 27).ToString());
        Assert.Throws<CsTomlException>(() => value!.GetInt64());
        Assert.Throws<CsTomlException>(() => value!.GetDouble());
        Assert.Throws<CsTomlException>(() => value!.GetBool());

        var utcTime1 = new DateTime(1979, 5, 27);
        DateTimeOffset utcTime2 = utcTime1;
        value!.GetDateTime().ShouldBe(utcTime1);
        value!.GetDateTimeOffset().ShouldBe(utcTime2);
        value!.GetDateOnly().ShouldBe(new DateOnly(1979, 5, 27));
        Assert.Throws<CsTomlException>(() => value!.GetTimeOnly());
        Assert.Throws<CsTomlException>(() => value!.GetNumber<int>());
    }

    [Fact]
    public void TryLocalDateTest()
    {
        var value = document.RootNode["ld1"u8];
        {
            value!.TryGetString(out var v).ShouldBeTrue();
            v.ShouldBe(new DateOnly(1979, 5, 27).ToString());
        }
        var utcTime1 = new DateTime(1979, 5, 27);
        DateTimeOffset utcTime2 = utcTime1;
        {
            value!.TryGetDateTime(out var v).ShouldBeTrue();
            v.ShouldBe(utcTime1);
        }
        {
            value!.TryGetDateTimeOffset(out var v).ShouldBeTrue();
            v.ShouldBe(utcTime2);
        }
        {
            value!.TryGetDateOnly(out var v).ShouldBeTrue();
            v.ShouldBe(new DateOnly(1979, 5, 27));
        }

        value!.TryGetInt64(out var _).ShouldBeFalse();
        value!.TryGetDouble(out var _).ShouldBeFalse();
        value!.TryGetBool(out var _).ShouldBeFalse();
        value!.TryGetTimeOnly(out var _).ShouldBeFalse();
        value!.TryGetNumber<int>(out var _).ShouldBeFalse();
    }

    [Fact]
    public void LocalTimeTest()
    {
        var value = document.RootNode["lt1"u8];
        value!.GetString()!.ShouldBe(new TimeOnly(7, 32, 30).ToString());
        Assert.Throws<CsTomlException>(() => value!.GetInt64());
        Assert.Throws<CsTomlException>(() => value!.GetDouble());
        Assert.Throws<CsTomlException>(() => value!.GetBool());

        Assert.Throws<CsTomlException>(() => value!.GetDateTime());
        Assert.Throws<CsTomlException>(() => value!.GetDateTimeOffset());
        Assert.Throws<CsTomlException>(() => value!.GetDateOnly());
        value!.GetTimeOnly().ShouldBe(new TimeOnly(7, 32, 30));
        Assert.Throws<CsTomlException>(() => value!.GetNumber<int>());
    }

    [Fact]
    public void TryLocalTimeTest()
    {
        var value = document.RootNode["lt1"u8];
        {
            value!.TryGetString(out var v).ShouldBeTrue();
            v.ShouldBe(new TimeOnly(7, 32, 30).ToString());
        }
        {
            value!.TryGetTimeOnly(out var v).ShouldBeTrue();
            v.ShouldBe(new TimeOnly(7, 32, 30));
        }

        value!.TryGetInt64(out var _).ShouldBeFalse();
        value!.TryGetDouble(out var _).ShouldBeFalse();
        value!.TryGetBool(out var _).ShouldBeFalse();
        value!.TryGetDateTime(out var _).ShouldBeFalse();
        value!.TryGetDateTimeOffset(out var _).ShouldBeFalse();
        value!.TryGetDateOnly(out var _).ShouldBeFalse();
        value!.TryGetNumber<int>(out var _).ShouldBeFalse();
    }

    [Fact]
    public void ArrayTest()
    {
        var value = document.RootNode["array"u8];
        var arrayValue = value!.GetArray();
        arrayValue.Select(i => i.GetInt64()).ShouldBe([1, 2, 3]);
        arrayValue.Count.ShouldBe(3);
        arrayValue[0].GetInt64().ShouldBe(1);
        arrayValue[1].GetInt64().ShouldBe(2);
        arrayValue[2].GetInt64().ShouldBe(3);

        var arrayIndexValue = value!.GetArrayValue(0);
        arrayIndexValue.GetInt64().ShouldBe(1);
        value!.GetString().ShouldBe("[1, 2, 3]");
        Assert.Throws<CsTomlException>(() => value!.GetInt64());
        Assert.Throws<CsTomlException>(() => value!.GetDouble());
        Assert.Throws<CsTomlException>(() => value!.GetBool());
        Assert.Throws<CsTomlException>(() => value!.GetDateTime());
        Assert.Throws<CsTomlException>(() => value!.GetDateTimeOffset());
        Assert.Throws<CsTomlException>(() => value!.GetDateOnly());
        Assert.Throws<CsTomlException>(() => value!.GetTimeOnly());
        Assert.Throws<CsTomlException>(() => value!.GetNumber<int>());
    }

    [Fact]
    public void TryArrayTest()
    {
        var value = document.RootNode["array"u8];

        {
            value!.TryGetArray(out var arrayValue).ShouldBeTrue();
            arrayValue.Select(i => i.GetInt64()).ShouldBe([1, 2, 3]);
            arrayValue.Count.ShouldBe(3);
            arrayValue[0].GetInt64().ShouldBe(1);
            arrayValue[1].GetInt64().ShouldBe(2);
            arrayValue[2].GetInt64().ShouldBe(3);
        }
        {
            value!.TryGetArrayValue(0, out var arrayIndexValue).ShouldBeTrue();
            arrayIndexValue.GetInt64().ShouldBe(1);
        }
        {
            value!.TryGetString(out var v).ShouldBeTrue();
            v!.ShouldBe("[1, 2, 3]");
        }
        value!.TryGetInt64(out var _).ShouldBeFalse();
        value!.TryGetDouble(out var _).ShouldBeFalse();
        value!.TryGetBool(out var _).ShouldBeFalse();
        value!.TryGetDateTime(out var _).ShouldBeFalse();
        value!.TryGetDateTimeOffset(out var _).ShouldBeFalse();
        value!.TryGetDateOnly(out var _).ShouldBeFalse();
        value!.TryGetTimeOnly(out var _).ShouldBeFalse();
        value!.TryGetNumber<int>(out var _).ShouldBeFalse();
    }

    [Fact]
    public void QuotedkeysTest()
    {
        var value = document.RootNode["127.0.0.1"u8];
        value!.GetString().ShouldBe("value");

        Assert.Throws<CsTomlException>(() => value!.GetInt64());
        Assert.Throws<CsTomlException>(() => value!.GetDouble());
        Assert.Throws<CsTomlException>(() => value!.GetBool());
        Assert.Throws<CsTomlException>(() => value!.GetDateTime());
        Assert.Throws<CsTomlException>(() => value!.GetDateTimeOffset());
        Assert.Throws<CsTomlException>(() => value!.GetDateOnly());
        Assert.Throws<CsTomlException>(() => value!.GetTimeOnly());
        Assert.Throws<CsTomlException>(() => value!.GetNumber<int>());
    }


    [Fact]
    public void TableTest()
    {
        var value = document.RootNode["Table"u8]["value"u8];
        value!.GetInt64().ShouldBe(100);
        value!.GetDouble().ShouldBe(100d);
        value!.GetBool().ShouldBeTrue();
        Assert.Throws<CsTomlException>(() => value!.GetDateTime());
        Assert.Throws<CsTomlException>(() => value!.GetDateTimeOffset());
        Assert.Throws<CsTomlException>(() => value!.GetDateOnly());
        Assert.Throws<CsTomlException>(() => value!.GetTimeOnly());
        value!.GetNumber<int>().ShouldBe(100);
    }

    [Fact]
    public void DottedTableTest()
    {
        var value = document.RootNode["Dotted"u8]["Table"u8]["value"u8];
        value!.GetInt64().ShouldBe(200);
        value!.GetDouble().ShouldBe(200d);
        value!.GetBool().ShouldBeTrue();
        Assert.Throws<CsTomlException>(() => value!.GetDateTime());
        Assert.Throws<CsTomlException>(() => value!.GetDateTimeOffset());
        Assert.Throws<CsTomlException>(() => value!.GetDateOnly());
        Assert.Throws<CsTomlException>(() => value!.GetTimeOnly());
        value!.GetNumber<int>().ShouldBe(200);
    }

    [Fact]
    public void ArrayOfTablesTest()
    {
        var arrayOfTables = document.RootNode["ArrayOfTables"u8];
        {
            var value = arrayOfTables[0]["value"u8];
            value!.GetInt64().ShouldBe(1);
            value!.GetDouble().ShouldBe(1d);
            value!.GetBool().ShouldBeTrue();
            Assert.Throws<CsTomlException>(() => value!.GetDateTime());
            Assert.Throws<CsTomlException>(() => value!.GetDateTimeOffset());
            Assert.Throws<CsTomlException>(() => value!.GetDateOnly());
            Assert.Throws<CsTomlException>(() => value!.GetTimeOnly());
            value!.GetNumber<int>().ShouldBe(1);
        }
        {
            var value = arrayOfTables[1]["value2"u8];
            value!.GetInt64().ShouldBe(2);
            value!.GetDouble().ShouldBe(2d);
            value!.GetBool().ShouldBeTrue();
            Assert.Throws<CsTomlException>(() => value!.GetDateTime());
            Assert.Throws<CsTomlException>(() => value!.GetDateTimeOffset());
            Assert.Throws<CsTomlException>(() => value!.GetDateOnly());
            Assert.Throws<CsTomlException>(() => value!.GetTimeOnly());
            value!.GetNumber<int>().ShouldBe(2);
        }
        {
            var value = arrayOfTables[2]["value3"u8];
            value!.GetInt64().ShouldBe(3);
            value!.GetDouble().ShouldBe(3d);
            value!.GetBool().ShouldBeTrue();
            Assert.Throws<CsTomlException>(() => value!.GetDateTime());
            Assert.Throws<CsTomlException>(() => value!.GetDateTimeOffset());
            Assert.Throws<CsTomlException>(() => value!.GetDateOnly());
            Assert.Throws<CsTomlException>(() => value!.GetTimeOnly());
            value!.GetNumber<int>().ShouldBe(3);
        }
    }
}
