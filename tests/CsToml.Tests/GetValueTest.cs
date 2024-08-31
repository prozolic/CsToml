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
int = 99
flt = 1.0
bool = true
odt = 1979-05-27T07:32:00Z
ldt = 1979-05-27T07:32:00
ld1 = 1979-05-27
lt1 = 07:32:30
array = [ 1, 2, 3]

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
        Assert.Equal("string", value!.GetString());
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
    public void StringTest2()
    {
        var value = document.RootNode["str2"u8];
        Assert.Equal("123", value!.GetString());
        Assert.Equal(123, value!.GetInt64());
        Assert.Equal(123d, value!.GetDouble());
        Assert.Throws<CsTomlException>(() => value!.GetBool());
        Assert.Throws<CsTomlException>(() => value!.GetDateTime());
        Assert.Throws<CsTomlException>(() => value!.GetDateTimeOffset());
        Assert.Throws<CsTomlException>(() => value!.GetDateOnly());
        Assert.Throws<CsTomlException>(() => value!.GetTimeOnly());
        Assert.Equal(123d, value!.GetNumber<int>());
    }

    [Fact]
    public void StringTest3()
    {
        var value = document.RootNode["str3"u8];
        Assert.Equal(" true ", value!.GetString());
        Assert.Throws<CsTomlException>(() => value!.GetInt64());
        Assert.Throws<CsTomlException>(() => value!.GetDouble());
        Assert.True(value!.GetBool());
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
        Assert.Equal("99", value!.GetString());
        Assert.Equal(99, value!.GetInt64());
        Assert.Equal(99d, value!.GetDouble());
        Assert.True(value!.GetBool());
        Assert.Throws<CsTomlException>(() => value!.GetDateTime());
        Assert.Throws<CsTomlException>(() => value!.GetDateTimeOffset());
        Assert.Throws<CsTomlException>(() => value!.GetDateOnly());
        Assert.Throws<CsTomlException>(() => value!.GetTimeOnly());
        Assert.Equal(99, value!.GetNumber<int>());
    }

    [Fact]
    public void TryIntTest()
    {
        var value = document.RootNode["int"u8];
        {
            var result = value!.TryGetString(out var v);
            Assert.Equal("99", v);
        }
        {
            var result = value!.TryGetInt64(out var v);
            Assert.Equal(99, v);
        }
        {
            var result = value!.TryGetDouble(out var v);
            Assert.Equal(99d, v);
        }
        {
            var result = value!.TryGetBool(out var v);
            Assert.True(v);
        }
        {
            var result = value!.TryGetDateTime(out var v);
            Assert.False(result);
        }
        {
            var result = value!.TryGetDateTimeOffset(out var v);
            Assert.False(result);
        }
        {
            var result = value!.TryGetDateOnly(out var v);
            Assert.False(result);
        }
        {
            var result = value!.TryGetTimeOnly(out var v);
            Assert.False(result);
        }
        {
            var result = value!.TryGetNumber<int>(out var v);
            Assert.Equal(99, v);
        }
    }

    [Fact]
    public void FloatTest()
    {
        var value = document.RootNode["flt"u8];
        Assert.Equal("1", value!.GetString());
        Assert.Equal(1, value!.GetInt64());
        Assert.Equal(1.0d, value!.GetDouble());
        Assert.True(value!.GetBool());
        Assert.Throws<CsTomlException>(() => value!.GetDateTime());
        Assert.Throws<CsTomlException>(() => value!.GetDateTimeOffset());
        Assert.Throws<CsTomlException>(() => value!.GetDateOnly());
        Assert.Throws<CsTomlException>(() => value!.GetTimeOnly());
        Assert.Equal(1, value!.GetNumber<int>());
    }

    [Fact]
    public void TryFloatTest()
    {
        var value = document.RootNode["flt"u8];
        {
            var result = value!.TryGetString(out var v);
            Assert.Equal("1", v);
        }
        {
            var result = value!.TryGetInt64(out var v);
            Assert.Equal(1, v);
        }
        {
            var result = value!.TryGetDouble(out var v);
            Assert.Equal(1.0d, v);
        }
        {
            var result = value!.TryGetBool(out var v);
            Assert.True(v);
        }
        {
            var result = value!.TryGetDateTime(out var v);
            Assert.False(result);
        }
        {
            var result = value!.TryGetDateTimeOffset(out var v);
            Assert.False(result);
        }
        {
            var result = value!.TryGetDateOnly(out var v);
            Assert.False(result);
        }
        {
            var result = value!.TryGetTimeOnly(out var v);
            Assert.False(result);
        }
        {
            var result = value!.TryGetNumber<int>(out var v);
            Assert.Equal(1, v);
        }
    }

    [Fact]
    public void BoolTest()
    {
        var value = document.RootNode["bool"u8];
        Assert.Equal("True", value!.GetString());
        Assert.Equal(1, value!.GetInt64());
        Assert.Equal(1d, value!.GetDouble());
        Assert.True(value!.GetBool());
        Assert.Throws<CsTomlException>(() => value!.GetDateTime());
        Assert.Throws<CsTomlException>(() => value!.GetDateTimeOffset());
        Assert.Throws<CsTomlException>(() => value!.GetDateOnly());
        Assert.Throws<CsTomlException>(() => value!.GetTimeOnly());
        Assert.Equal(1, value!.GetNumber<int>());
    }

    [Fact]
    public void TryBoolTest()
    {
        var value = document.RootNode["bool"u8];
        {
            var result = value!.TryGetString(out var v);
            Assert.Equal("True", v);
        }
        {
            var result = value!.TryGetInt64(out var v);
            Assert.Equal(1, v);
        }
        {
            var result = value!.TryGetDouble(out var v);
            Assert.Equal(1.0d, v);
        }
        {
            var result = value!.TryGetBool(out var v);
            Assert.True(v);
        }
        {
            var result = value!.TryGetDateTime(out var v);
            Assert.False(result);
        }
        {
            var result = value!.TryGetDateTimeOffset(out var v);
            Assert.False(result);
        }
        {
            var result = value!.TryGetDateOnly(out var v);
            Assert.False(result);
        }
        {
            var result = value!.TryGetTimeOnly(out var v);
            Assert.False(result);
        }
        {
            var result = value!.TryGetNumber<int>(out var v);
            Assert.Equal(1, v);
        }
    }

    [Fact]
    public void OffsetDateTimeTest()
    {
        var value = document.RootNode["odt"u8];
        Assert.Equal(new DateTimeOffset(1979, 5, 27, 7, 32, 0, TimeSpan.Zero).ToString(), value!.GetString());
        Assert.Throws<CsTomlException>(() => value!.GetInt64());
        Assert.Throws<CsTomlException>(() => value!.GetDouble());
        Assert.Throws<CsTomlException>(() => value!.GetBool());

        var utcTime1 = DateTime.SpecifyKind(new DateTime(1979, 5, 27, 7, 32, 0), DateTimeKind.Utc);
        DateTimeOffset utcTime2 = utcTime1;
        Assert.Equal(utcTime1, value!.GetDateTime());
        Assert.Equal(utcTime2, value!.GetDateTimeOffset());
        Assert.Equal(new DateOnly(1979, 5, 27), value!.GetDateOnly());
        Assert.Equal(new TimeOnly(7, 32, 0), value!.GetTimeOnly());
        Assert.Throws<CsTomlException>(() => value!.GetNumber<int>());
    }

    [Fact]
    public void TryOffsetDateTimeTest()
    {
        var value = document.RootNode["odt"u8];
        {
            var result = value!.TryGetString(out var v);
            Assert.Equal(new DateTimeOffset(1979, 5, 27, 7, 32, 0, TimeSpan.Zero).ToString(), v);
        }
        {
            var result = value!.TryGetInt64(out var v);
            Assert.False(result);
        }
        {
            var result = value!.TryGetDouble(out var v);
            Assert.False(result);
        }
        {
            var result = value!.TryGetBool(out var v);
            Assert.False(result);
        }

        var utcTime1 = DateTime.SpecifyKind(new DateTime(1979, 5, 27, 7, 32, 0), DateTimeKind.Utc);
        DateTimeOffset utcTime2 = utcTime1;
        {
            var result = value!.TryGetDateTime(out var v);
            Assert.Equal(utcTime1, v);
        }
        {
            var result = value!.TryGetDateTimeOffset(out var v);
            Assert.Equal(utcTime2, v);
        }
        {
            var result = value!.TryGetDateOnly(out var v);
            Assert.Equal(new DateOnly(1979, 5, 27), value!.GetDateOnly());
        }
        {
            var result = value!.TryGetTimeOnly(out var v);
            Assert.Equal(new TimeOnly(7, 32, 0), value!.GetTimeOnly());
        }
        {
            var result = value!.TryGetNumber<int>(out var v);
            Assert.False(result);
        }
    }

    [Fact]
    public void LocalDateTimeTest()
    {
        var value = document.RootNode["ldt"u8];
        Assert.Equal(new DateTime(1979, 5, 27, 7, 32, 0).ToString(), value!.GetString());
        Assert.Throws<CsTomlException>(() => value!.GetInt64());
        Assert.Throws<CsTomlException>(() => value!.GetDouble());
        Assert.Throws<CsTomlException>(() => value!.GetBool());

        var utcTime1 = new DateTime(1979, 5, 27, 7, 32, 0);
        DateTimeOffset utcTime2 = utcTime1;
        Assert.Equal(utcTime1, value!.GetDateTime());
        Assert.Equal(utcTime2, value!.GetDateTimeOffset());
        Assert.Equal(new DateOnly(1979, 5, 27), value!.GetDateOnly());
        Assert.Equal(new TimeOnly(7, 32, 0), value!.GetTimeOnly());
        Assert.Throws<CsTomlException>(() => value!.GetNumber<int>());
    }

    [Fact]
    public void TryLocalDateTimeTest()
    {
        var value = document.RootNode["ldt"u8];
        {
            var result = value!.TryGetString(out var v);
            Assert.Equal(new DateTime(1979, 5, 27, 7, 32, 0).ToString(), v);
        }
        {
            var result = value!.TryGetInt64(out var v);
            Assert.False(result);
        }
        {
            var result = value!.TryGetDouble(out var v);
            Assert.False(result);
        }
        {
            var result = value!.TryGetBool(out var v);
            Assert.False(result);
        }

        var utcTime1 = new DateTime(1979, 5, 27, 7, 32, 0);
        DateTimeOffset utcTime2 = utcTime1;
        {
            var result = value!.TryGetDateTime(out var v);
            Assert.Equal(utcTime1, v);
        }
        {
            var result = value!.TryGetDateTimeOffset(out var v);
            Assert.Equal(utcTime2, v);
        }
        {
            var result = value!.TryGetDateOnly(out var v);
            Assert.Equal(new DateOnly(1979, 5, 27), value!.GetDateOnly());
        }
        {
            var result = value!.TryGetTimeOnly(out var v);
            Assert.Equal(new TimeOnly(7, 32, 0), value!.GetTimeOnly());
        }
        {
            var result = value!.TryGetNumber<int>(out var v);
            Assert.False(result);
        }
    }

    [Fact]
    public void LocalDateTest()
    {
        var value = document.RootNode["ld1"u8];
        Assert.Equal(new DateOnly(1979, 5, 27).ToString(), value!.GetString());
        Assert.Throws<CsTomlException>(() => value!.GetInt64());
        Assert.Throws<CsTomlException>(() => value!.GetDouble());
        Assert.Throws<CsTomlException>(() => value!.GetBool());

        var utcTime1 = new DateTime(1979, 5, 27);
        DateTimeOffset utcTime2 = utcTime1;
        Assert.Equal(utcTime1, value!.GetDateTime());
        Assert.Equal(utcTime2, value!.GetDateTimeOffset());
        Assert.Equal(new DateOnly(1979, 5, 27), value!.GetDateOnly());
        Assert.Throws<CsTomlException>(() => value!.GetTimeOnly());
        Assert.Throws<CsTomlException>(() => value!.GetNumber<int>());
    }

    [Fact]
    public void TryLocalDateTest()
    {
        var value = document.RootNode["ld1"u8];
        {
            var result = value!.TryGetString(out var v);
            Assert.Equal(new DateOnly(1979, 5, 27).ToString(), v);
        }
        {
            var result = value!.TryGetInt64(out var v);
            Assert.False(result);
        }
        {
            var result = value!.TryGetDouble(out var v);
            Assert.False(result);
        }
        {
            var result = value!.TryGetBool(out var v);
            Assert.False(result);
        }

        var utcTime1 = new DateTime(1979, 5, 27);
        DateTimeOffset utcTime2 = utcTime1;
        {
            var result = value!.TryGetDateTime(out var v);
            Assert.Equal(utcTime1, v);
        }
        {
            var result = value!.TryGetDateTimeOffset(out var v);
            Assert.Equal(utcTime2, v);
        }
        {
            var result = value!.TryGetDateOnly(out var v);
            Assert.Equal(new DateOnly(1979, 5, 27), value!.GetDateOnly());
        }
        {
            var result = value!.TryGetTimeOnly(out var v);
            Assert.False(result);
        }
        {
            var result = value!.TryGetNumber<int>(out var v);
            Assert.False(result);
        }
    }

    [Fact]
    public void LocalTimeTest()
    {
        var value = document.RootNode["lt1"u8];
        Assert.Equal(new TimeOnly(7,32,30).ToString(), value!.GetString());
        Assert.Throws<CsTomlException>(() => value!.GetInt64());
        Assert.Throws<CsTomlException>(() => value!.GetDouble());
        Assert.Throws<CsTomlException>(() => value!.GetBool());

        Assert.Throws<CsTomlException>(() => value!.GetDateTime());
        Assert.Throws<CsTomlException>(() => value!.GetDateTimeOffset());
        Assert.Throws<CsTomlException>(() => value!.GetDateOnly());
        Assert.Equal(new TimeOnly(7,32,30), value!.GetTimeOnly());
        Assert.Throws<CsTomlException>(() => value!.GetNumber<int>());
    }

    [Fact]
    public void TryLocalTimeTest()
    {
        var value = document.RootNode["lt1"u8];
        {
            var result = value!.TryGetString(out var v);
            Assert.Equal(new TimeOnly(7,32,30).ToString(), v);
        }
        {
            var result = value!.TryGetInt64(out var v);
            Assert.False(result);
        }
        {
            var result = value!.TryGetDouble(out var v);
            Assert.False(result);
        }
        {
            var result = value!.TryGetBool(out var v);
            Assert.False(result);
        }
        {
            var result = value!.TryGetDateTime(out var v);
            Assert.False(result);
        }
        {
            var result = value!.TryGetDateTimeOffset(out var v);
            Assert.False(result);
        }
        {
            var result = value!.TryGetDateOnly(out var v);
            Assert.False(result);
        }
        {
            var result = value!.TryGetTimeOnly(out var v);
            Assert.Equal(new TimeOnly(7, 32, 30), value!.GetTimeOnly());
        }
        {
            var result = value!.TryGetNumber<int>(out var v);
            Assert.False(result);
        }
    }

    [Fact]
    public void ArrayTest()
    {
        var value = document.RootNode["array"u8];
        var arrayValue = value!.GetArray();
        Assert.Equal(3, arrayValue.Count);
        Assert.Equal(1, arrayValue[0].GetInt64());
        Assert.Equal(2, arrayValue[1].GetInt64());
        Assert.Equal(3, arrayValue[2].GetInt64());
        var arrayIndexValue = value!.GetArrayValue(0);
        Assert.Equal(1, arrayIndexValue.GetInt64());

        Assert.Equal("[1, 2, 3]", value!.GetString());
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
            var result = value!.TryGetArray(out var arrayValue);
            Assert.Equal(3, arrayValue.Count);
            Assert.Equal(1, arrayValue[0].GetInt64());
            Assert.Equal(2, arrayValue[1].GetInt64());
            Assert.Equal(3, arrayValue[2].GetInt64());
        }
        {
            var result = value!.TryGetArrayValue(0, out var arrayIndexValue);
            Assert.True(result);
            Assert.Equal(1, arrayIndexValue.GetInt64());
        }
        {
            var result = value!.TryGetString(out var v);
            Assert.True(result);
            Assert.Equal("[1, 2, 3]", v);
        }
        {
            var result = value!.TryGetInt64(out var v);
            Assert.False(result);
        }
        {
            var result = value!.TryGetDouble(out var v);
            Assert.False(result);
        }
        {
            var result = value!.TryGetBool(out var v);
            Assert.False(result);
        }
        {
            var result = value!.TryGetDateTime(out var v);
            Assert.False(result);
        }
        {
            var result = value!.TryGetDateTimeOffset(out var v);
            Assert.False(result);
        }
        {
            var result = value!.TryGetDateOnly(out var v);
            Assert.False(result);
        }
        {
            var result = value!.TryGetTimeOnly(out var v);
            Assert.False(result);
        }
        {
            var result = value!.TryGetNumber<int>(out var v);
            Assert.False(result);
        }
    }
}

