using CsToml.Error;
using FluentAssertions;

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
        value!.GetString().Should().Be("string");

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
            value!.TryGetString(out var v).Should().BeTrue();
            v.Should().Be("string");
        }

        value!.TryGetInt64(out var _).Should().BeFalse();
        value!.TryGetDouble(out var _).Should().BeFalse();
        value!.TryGetBool(out var _).Should().BeFalse();
        value!.TryGetNumber<int>(out var _).Should().BeFalse();
        value!.TryGetDateTime(out var _).Should().BeFalse();
        value!.TryGetDateTimeOffset(out var _).Should().BeFalse();
        value!.TryGetDateOnly(out var _).Should().BeFalse();
        value!.TryGetTimeOnly(out var _).Should().BeFalse();
    }

    [Fact]
    public void StringTest2()
    {
        var value = document.RootNode["str2"u8];
        value!.GetString().Should().Be("123");
        value!.GetInt64().Should().Be(123);
        value!.GetDouble().Should().Be(123d);
        Assert.Throws<CsTomlException>(() => value!.GetBool());
        Assert.Throws<CsTomlException>(() => value!.GetDateTime());
        Assert.Throws<CsTomlException>(() => value!.GetDateTimeOffset());
        Assert.Throws<CsTomlException>(() => value!.GetDateOnly());
        Assert.Throws<CsTomlException>(() => value!.GetTimeOnly());
        value!.GetNumber<int>().Should().Be(123);
    }

    [Fact]
    public void TryStringTest2()
    {
        var value = document.RootNode["str2"u8];
        {
            value!.TryGetString(out var v).Should().BeTrue();
            v.Should().Be("123");
        }
        {
            value!.TryGetInt64(out var v).Should().BeTrue();
            v.Should().Be(123);
        }
        {
            value!.TryGetDouble(out var v).Should().BeTrue();
            v.Should().Be(123d);
        }

        value!.TryGetBool(out var _).Should().BeFalse();
        value!.TryGetNumber<int>(out var _).Should().BeFalse();
        value!.TryGetDateTime(out var _).Should().BeFalse();
        value!.TryGetDateTimeOffset(out var _).Should().BeFalse();
        value!.TryGetDateOnly(out var _).Should().BeFalse();
        value!.TryGetTimeOnly(out var _).Should().BeFalse();
    }

    [Fact]
    public void StringTest3()
    {
        var value = document.RootNode["str3"u8];
        value!.GetString().Should().Be(" true ");
        Assert.Throws<CsTomlException>(() => value!.GetInt64());
        Assert.Throws<CsTomlException>(() => value!.GetDouble());
        value!.GetBool().Should().BeTrue();
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
            value!.TryGetString(out var v).Should().BeTrue();
            v.Should().Be(" true ");
        }
        {
            value!.TryGetBool(out var v).Should().BeTrue();
            v.Should().BeTrue();
        }

        value!.TryGetInt64(out var _).Should().BeFalse();
        value!.TryGetDouble(out var _).Should().BeFalse();
        value!.TryGetNumber<int>(out var _).Should().BeFalse();
        value!.TryGetDateTime(out var _).Should().BeFalse();
        value!.TryGetDateTimeOffset(out var _).Should().BeFalse();
        value!.TryGetDateOnly(out var _).Should().BeFalse();
        value!.TryGetTimeOnly(out var _).Should().BeFalse();
    }

    [Fact]
    public void StringTest4()
    {
        var value = document.RootNode["str4"u8];
        value!.GetString().Should().Be("The quick brown fox jumps over the lazy dog.");
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
        value!.GetString().Should().Be("this is str5.");
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
        value!.GetString().Should().Be("t\\h\\i\\s i\\s s\\t\\r\\6");
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
        value!.GetString().Should().Be("99");
        value!.GetInt64().Should().Be(99);
        value!.GetDouble().Should().Be(99d);
        value!.GetBool().Should().BeTrue();
        Assert.Throws<CsTomlException>(() => value!.GetDateTime());
        Assert.Throws<CsTomlException>(() => value!.GetDateTimeOffset());
        Assert.Throws<CsTomlException>(() => value!.GetDateOnly());
        Assert.Throws<CsTomlException>(() => value!.GetTimeOnly());
        value!.GetNumber<int>().Should().Be(99);
    }

    [Fact]
    public void TryIntTest()
    {
        var value = document.RootNode["int"u8];
        {
            value!.TryGetString(out var v).Should().BeTrue();
            v.Should().Be("99");
        }
        {
            value!.TryGetInt64(out var v).Should().BeTrue();
            v.Should().Be(99);
        }
        {
            value!.TryGetDouble(out var v).Should().BeTrue();
            v.Should().Be(99d);
        }
        {
            value!.TryGetBool(out var v).Should().BeTrue();
            v.Should().BeTrue();
        }
        {
            value!.TryGetNumber<int>(out var v).Should().BeTrue();
            v.Should().Be(99);
        }

        value!.TryGetDateTime(out var _).Should().BeFalse();
        value!.TryGetDateTimeOffset(out var _).Should().BeFalse();
        value!.TryGetDateOnly(out var _).Should().BeFalse();
        value!.TryGetTimeOnly(out var _).Should().BeFalse();
    }

    [Fact]
    public void FloatTest()
    {
        var value = document.RootNode["flt"u8];
        value!.GetString().Should().Be("1");
        value!.GetInt64().Should().Be(1);
        value!.GetDouble().Should().Be(1.0d);
        value!.GetBool().Should().BeTrue();
        Assert.Throws<CsTomlException>(() => value!.GetDateTime());
        Assert.Throws<CsTomlException>(() => value!.GetDateTimeOffset());
        Assert.Throws<CsTomlException>(() => value!.GetDateOnly());
        Assert.Throws<CsTomlException>(() => value!.GetTimeOnly());
        value!.GetNumber<int>().Should().Be(1);
    }

    [Fact]
    public void TryFloatTest()
    {
        var value = document.RootNode["flt"u8];
        {
            value!.TryGetString(out var v).Should().BeTrue();
            v.Should().Be("1");
        }
        {
            value!.TryGetInt64(out var v).Should().BeTrue();
            v.Should().Be(1);
        }
        {
            value!.TryGetDouble(out var v).Should().BeTrue();
            v.Should().Be(1.0d);
        }
        {
            value!.TryGetBool(out var v).Should().BeTrue();
            v.Should().BeTrue();
        }
        {
            value!.TryGetNumber<int>(out var v).Should().BeTrue();
            v.Should().Be(1);
        }

        value!.TryGetDateTime(out var _).Should().BeFalse();
        value!.TryGetDateTimeOffset(out var _).Should().BeFalse();
        value!.TryGetDateOnly(out var _).Should().BeFalse();
        value!.TryGetTimeOnly(out var _).Should().BeFalse();
    }

    [Fact]
    public void BoolTest()
    {
        var value = document.RootNode["bool"u8];
        value!.GetString().Should().Be("True");
        value!.GetInt64().Should().Be(1);
        value!.GetDouble().Should().Be(1d);
        value!.GetBool().Should().BeTrue();
        Assert.Throws<CsTomlException>(() => value!.GetDateTime());
        Assert.Throws<CsTomlException>(() => value!.GetDateTimeOffset());
        Assert.Throws<CsTomlException>(() => value!.GetDateOnly());
        Assert.Throws<CsTomlException>(() => value!.GetTimeOnly());
        value!.GetNumber<int>().Should().Be(1);
    }

    [Fact]
    public void TryBoolTest()
    {
        var value = document.RootNode["bool"u8];
        {
            value!.TryGetString(out var v).Should().BeTrue();
            v.Should().Be("True");
        }
        {
            value!.TryGetInt64(out var v).Should().BeTrue();
            v.Should().Be(1);
        }
        {
            value!.TryGetDouble(out var v).Should().BeTrue();
            v.Should().Be(1.0d);
        }
        {
            value!.TryGetBool(out var v).Should().BeTrue();
            v.Should().BeTrue();
        }
        {
            value!.TryGetNumber<int>(out var v).Should().BeTrue();
            v.Should().Be(1);
        }

        value!.TryGetDateTime(out var _).Should().BeFalse();
        value!.TryGetDateTimeOffset(out var _).Should().BeFalse();
        value!.TryGetDateOnly(out var _).Should().BeFalse();
        value!.TryGetTimeOnly(out var _).Should().BeFalse();
    }

    [Fact]
    public void OffsetDateTimeTest()
    {
        var value = document.RootNode["odt"u8];
        value!.GetString().Should().Be(new DateTimeOffset(1979, 5, 27, 7, 32, 0, TimeSpan.Zero).ToString());
        Assert.Throws<CsTomlException>(() => value!.GetInt64());
        Assert.Throws<CsTomlException>(() => value!.GetDouble());
        Assert.Throws<CsTomlException>(() => value!.GetBool());

        var utcTime1 = DateTime.SpecifyKind(new DateTime(1979, 5, 27, 7, 32, 0), DateTimeKind.Utc);
        DateTimeOffset utcTime2 = utcTime1;

        value!.GetDateTime().Should().Be(utcTime1);
        value!.GetDateTimeOffset().Should().Be(utcTime2);
        value!.GetDateOnly().Should().Be(new DateOnly(1979, 5, 27));
        value!.GetTimeOnly().Should().Be(new TimeOnly(7, 32, 0));
        Assert.Throws<CsTomlException>(() => value!.GetNumber<int>());
    }

    [Fact]
    public void TryOffsetDateTimeTest()
    {
        var value = document.RootNode["odt"u8];
        {
            value!.TryGetString(out var v).Should().BeTrue();
            v.Should().Be(new DateTimeOffset(1979, 5, 27, 7, 32, 0, TimeSpan.Zero).ToString());
        }
        var utcTime1 = DateTime.SpecifyKind(new DateTime(1979, 5, 27, 7, 32, 0), DateTimeKind.Utc);
        DateTimeOffset utcTime2 = utcTime1;
        {
            value!.TryGetDateTime(out var v).Should().BeTrue();
            v.Should().Be(utcTime1);
        }
        {
            value!.TryGetDateTimeOffset(out var v).Should().BeTrue();
            v.Should().Be(utcTime2);
        }
        {
            value!.TryGetDateOnly(out var v).Should().BeTrue();
            v.Should().Be(new DateOnly(1979, 5, 27));
        }
        {
            value!.TryGetTimeOnly(out var v).Should().BeTrue();
            v.Should().Be(new TimeOnly(7, 32, 0));
        }

        value!.TryGetInt64(out var _).Should().BeFalse();
        value!.TryGetDouble(out var _).Should().BeFalse();
        value!.TryGetBool(out var _).Should().BeFalse();
        value!.TryGetNumber<int>(out var _).Should().BeFalse();
    }

    [Fact]
    public void LocalDateTimeTest()
    {
        var value = document.RootNode["ldt"u8];
        value.GetString()!.Should().Be(new DateTime(1979, 5, 27, 7, 32, 0).ToString());
        Assert.Throws<CsTomlException>(() => value!.GetInt64());
        Assert.Throws<CsTomlException>(() => value!.GetDouble());
        Assert.Throws<CsTomlException>(() => value!.GetBool());

        var utcTime1 = new DateTime(1979, 5, 27, 7, 32, 0);
        DateTimeOffset utcTime2 = utcTime1;
        value!.GetDateTime().Should().Be(utcTime1);
        value!.GetDateTimeOffset().Should().Be(utcTime2);
        value!.GetDateOnly().Should().Be(new DateOnly(1979, 5, 27));
        value!.GetTimeOnly().Should().Be(new TimeOnly(7, 32, 0));
        Assert.Throws<CsTomlException>(() => value!.GetNumber<int>());
    }

    [Fact]
    public void TryLocalDateTimeTest()
    {
        var value = document.RootNode["ldt"u8];
        {
            value!.TryGetString(out var v).Should().BeTrue();
            v.Should().Be(new DateTime(1979, 5, 27, 7, 32, 0).ToString());
        }

        var utcTime1 = new DateTime(1979, 5, 27, 7, 32, 0);
        DateTimeOffset utcTime2 = utcTime1;
        {
            value!.TryGetDateTime(out var v).Should().BeTrue();
            v.Should().Be(utcTime1);
        }
        {
            value!.TryGetDateTimeOffset(out var v).Should().BeTrue();
            v.Should().Be(utcTime2);
        }
        {
            value!.TryGetDateOnly(out var v).Should().BeTrue();
            v.Should().Be(new DateOnly(1979, 5, 27));
        }
        {
            value!.TryGetTimeOnly(out var v).Should().BeTrue();
            v.Should().Be(new TimeOnly(7, 32, 0));
        }

        value!.TryGetInt64(out var _).Should().BeFalse();
        value!.TryGetDouble(out var _).Should().BeFalse();
        value!.TryGetBool(out var _).Should().BeFalse();
        value!.TryGetNumber<int>(out var _).Should().BeFalse();
    }

    [Fact]
    public void LocalDateTest()
    {
        var value = document.RootNode["ld1"u8];
        value!.GetString()!.Should().Be(new DateOnly(1979, 5, 27).ToString());
        Assert.Throws<CsTomlException>(() => value!.GetInt64());
        Assert.Throws<CsTomlException>(() => value!.GetDouble());
        Assert.Throws<CsTomlException>(() => value!.GetBool());

        var utcTime1 = new DateTime(1979, 5, 27);
        DateTimeOffset utcTime2 = utcTime1;
        value!.GetDateTime().Should().Be(utcTime1);
        value!.GetDateTimeOffset().Should().Be(utcTime2);
        value!.GetDateOnly().Should().Be(new DateOnly(1979, 5, 27));
        Assert.Throws<CsTomlException>(() => value!.GetTimeOnly());
        Assert.Throws<CsTomlException>(() => value!.GetNumber<int>());
    }

    [Fact]
    public void TryLocalDateTest()
    {
        var value = document.RootNode["ld1"u8];
        {
            value!.TryGetString(out var v).Should().BeTrue();
            v.Should().Be(new DateOnly(1979, 5, 27).ToString());
        }
        var utcTime1 = new DateTime(1979, 5, 27);
        DateTimeOffset utcTime2 = utcTime1;
        {
            value!.TryGetDateTime(out var v).Should().BeTrue();
            v.Should().Be(utcTime1);
        }
        {
            value!.TryGetDateTimeOffset(out var v).Should().BeTrue();
            v.Should().Be(utcTime2);
        }
        {
            value!.TryGetDateOnly(out var v).Should().BeTrue();
            v.Should().Be(new DateOnly(1979, 5, 27));
        }

        value!.TryGetInt64(out var _).Should().BeFalse();
        value!.TryGetDouble(out var _).Should().BeFalse();
        value!.TryGetBool(out var _).Should().BeFalse();
        value!.TryGetTimeOnly(out var _).Should().BeFalse();
        value!.TryGetNumber<int>(out var _).Should().BeFalse();
    }

    [Fact]
    public void LocalTimeTest()
    {
        var value = document.RootNode["lt1"u8];
        value!.GetString()!.Should().Be(new TimeOnly(7, 32, 30).ToString());
        Assert.Throws<CsTomlException>(() => value!.GetInt64());
        Assert.Throws<CsTomlException>(() => value!.GetDouble());
        Assert.Throws<CsTomlException>(() => value!.GetBool());

        Assert.Throws<CsTomlException>(() => value!.GetDateTime());
        Assert.Throws<CsTomlException>(() => value!.GetDateTimeOffset());
        Assert.Throws<CsTomlException>(() => value!.GetDateOnly());
        value!.GetTimeOnly().Should().Be(new TimeOnly(7, 32, 30));
        Assert.Throws<CsTomlException>(() => value!.GetNumber<int>());
    }

    [Fact]
    public void TryLocalTimeTest()
    {
        var value = document.RootNode["lt1"u8];
        {
            value!.TryGetString(out var v).Should().BeTrue();
            v.Should().Be(new TimeOnly(7, 32, 30).ToString());
        }
        {
            value!.TryGetTimeOnly(out var v).Should().BeTrue();
            v.Should().Be(new TimeOnly(7, 32, 30));
        }

        value!.TryGetInt64(out var _).Should().BeFalse();
        value!.TryGetDouble(out var _).Should().BeFalse();
        value!.TryGetBool(out var _).Should().BeFalse();
        value!.TryGetDateTime(out var _).Should().BeFalse();
        value!.TryGetDateTimeOffset(out var _).Should().BeFalse();
        value!.TryGetDateOnly(out var _).Should().BeFalse();
        value!.TryGetNumber<int>(out var _).Should().BeFalse();
    }

    [Fact]
    public void ArrayTest()
    {
        var value = document.RootNode["array"u8];
        var arrayValue = value!.GetArray();
        arrayValue.Select(i => i.GetInt64()).Should().Equal([1, 2, 3]);
        arrayValue.Count.Should().Be(3);
        arrayValue[0].GetInt64().Should().Be(1);
        arrayValue[1].GetInt64().Should().Be(2);
        arrayValue[2].GetInt64().Should().Be(3);

        var arrayIndexValue = value!.GetArrayValue(0);
        arrayIndexValue.GetInt64().Should().Be(1);
        value!.GetString().Should().Be("[1, 2, 3]");
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
            value!.TryGetArray(out var arrayValue).Should().BeTrue();
            arrayValue.Select(i => i.GetInt64()).Should().Equal([1, 2, 3]);
            arrayValue.Count.Should().Be(3);
            arrayValue[0].GetInt64().Should().Be(1);
            arrayValue[1].GetInt64().Should().Be(2);
            arrayValue[2].GetInt64().Should().Be(3);
        }
        {
            value!.TryGetArrayValue(0, out var arrayIndexValue).Should().BeTrue();
            arrayIndexValue.GetInt64().Should().Be(1);
        }
        {
            value!.TryGetString(out var v).Should().BeTrue();
            v!.Should().Be("[1, 2, 3]");
        }
        value!.TryGetInt64(out var _).Should().BeFalse();
        value!.TryGetDouble(out var _).Should().BeFalse();
        value!.TryGetBool(out var _).Should().BeFalse();
        value!.TryGetDateTime(out var _).Should().BeFalse();
        value!.TryGetDateTimeOffset(out var _).Should().BeFalse();
        value!.TryGetDateOnly(out var _).Should().BeFalse();
        value!.TryGetTimeOnly(out var _).Should().BeFalse();
        value!.TryGetNumber<int>(out var _).Should().BeFalse();
    }

    [Fact]
    public void QuotedkeysTest()
    {
        var value = document.RootNode["127.0.0.1"u8];
        value!.GetString().Should().Be("value");

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
        value!.GetInt64().Should().Be(100);
        value!.GetDouble().Should().Be(100d);
        value!.GetBool().Should().BeTrue();
        Assert.Throws<CsTomlException>(() => value!.GetDateTime());
        Assert.Throws<CsTomlException>(() => value!.GetDateTimeOffset());
        Assert.Throws<CsTomlException>(() => value!.GetDateOnly());
        Assert.Throws<CsTomlException>(() => value!.GetTimeOnly());
        value!.GetNumber<int>().Should().Be(100);
    }

    [Fact]
    public void DottedTableTest()
    {
        var value = document.RootNode["Dotted"u8]["Table"u8]["value"u8];
        value!.GetInt64().Should().Be(200);
        value!.GetDouble().Should().Be(200d);
        value!.GetBool().Should().BeTrue();
        Assert.Throws<CsTomlException>(() => value!.GetDateTime());
        Assert.Throws<CsTomlException>(() => value!.GetDateTimeOffset());
        Assert.Throws<CsTomlException>(() => value!.GetDateOnly());
        Assert.Throws<CsTomlException>(() => value!.GetTimeOnly());
        value!.GetNumber<int>().Should().Be(200);
    }

    [Fact]
    public void ArrayOfTablesTest()
    {
        var arrayOfTables = document.RootNode["ArrayOfTables"u8];
        {
            var value = arrayOfTables[0]["value"u8];
            value!.GetInt64().Should().Be(1);
            value!.GetDouble().Should().Be(1d);
            value!.GetBool().Should().BeTrue();
            Assert.Throws<CsTomlException>(() => value!.GetDateTime());
            Assert.Throws<CsTomlException>(() => value!.GetDateTimeOffset());
            Assert.Throws<CsTomlException>(() => value!.GetDateOnly());
            Assert.Throws<CsTomlException>(() => value!.GetTimeOnly());
            value!.GetNumber<int>().Should().Be(1);
        }
        {
            var value = arrayOfTables[1]["value2"u8];
            value!.GetInt64().Should().Be(2);
            value!.GetDouble().Should().Be(2d);
            value!.GetBool().Should().BeTrue();
            Assert.Throws<CsTomlException>(() => value!.GetDateTime());
            Assert.Throws<CsTomlException>(() => value!.GetDateTimeOffset());
            Assert.Throws<CsTomlException>(() => value!.GetDateOnly());
            Assert.Throws<CsTomlException>(() => value!.GetTimeOnly());
            value!.GetNumber<int>().Should().Be(2);
        }
        {
            var value = arrayOfTables[2]["value3"u8];
            value!.GetInt64().Should().Be(3);
            value!.GetDouble().Should().Be(3d);
            value!.GetBool().Should().BeTrue();
            Assert.Throws<CsTomlException>(() => value!.GetDateTime());
            Assert.Throws<CsTomlException>(() => value!.GetDateTimeOffset());
            Assert.Throws<CsTomlException>(() => value!.GetDateOnly());
            Assert.Throws<CsTomlException>(() => value!.GetTimeOnly());
            value!.GetNumber<int>().Should().Be(3);
        }
    }
}
