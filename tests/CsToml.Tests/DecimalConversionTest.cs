using System.Globalization;
using System.Text;

namespace CsToml.Tests;

public class DecimalConversionTest
{
    private static decimal GetDecimal(string tomlValue)
        => Deserialize(tomlValue).RootNode["v"].GetValue<decimal>();

    private static decimal? GetNullableDecimal(string tomlValue)
        => Deserialize(tomlValue).RootNode["v"].GetValue<decimal?>();

    private static TomlDocument Deserialize(string tomlValue)
        => CsTomlSerializer.Deserialize<TomlDocument>(Encoding.UTF8.GetBytes($"v = {tomlValue}\n"));

    [Theory]
    [InlineData("3.14", "3.14")]
    [InlineData("-123.456", "-123.456")]
    [InlineData("-0.01", "-0.01")]
    [InlineData("-2E-2", "-0.02")]
    [InlineData("+1.0", "1")]
    [InlineData("1.5", "1.5")]
    [InlineData("224_617.445_991_228", "224617.445991228")]
    [InlineData("6.626e-3", "0.006626")]
    [InlineData("1e10", "10000000000")]
    [InlineData("5e+22", "50000000000000000000000")]
    public void Float_PreservesTextDigits(string tomlValue, string expected)
    {
        GetDecimal(tomlValue).ToString(CultureInfo.InvariantCulture).ShouldBe(expected);
    }

    [Theory]
    [InlineData("0.30000000000000004", "0.30000000000000004")]
    [InlineData("9223372036854775808.0", "9223372036854776000")]
    [InlineData("1.2345678901234567e+25", "12345678901234566000000000")]
    public void Float_KeepsShortestRoundTripDigits(string tomlValue, string expected)
    {
        // The shortest round-trippable form of the parsed double is what reaches decimal,
        // so digits beyond the 15 significant digits of the old cast are retained.
        GetDecimal(tomlValue).ToString(CultureInfo.InvariantCulture).ShouldBe(expected);
    }

    [Theory]
    [InlineData("9223372036854775807", "9223372036854775807")]
    [InlineData("-9223372036854775808", "-9223372036854775808")]
    [InlineData("999999", "999999")]
    [InlineData("0", "0")]
    [InlineData("-1", "-1")]
    [InlineData("1_000_000", "1000000")]
    [InlineData("0xDEADBEEF", "3735928559")]
    [InlineData("0o755", "493")]
    [InlineData("0b1101", "13")]
    public void Integer_IsExact(string tomlValue, string expected)
    {
        // long.MaxValue is not representable as double, so a double round-trip would lose precision.
        GetDecimal(tomlValue).ToString(CultureInfo.InvariantCulture).ShouldBe(expected);
    }

    [Theory]
    [InlineData("0.0")]
    [InlineData("-0.0")]
    [InlineData("+0.0")]
    [InlineData("6.626e-34")]
    [InlineData("1e-300")]
    public void Float_ZeroOrUnderflow_IsZero(string tomlValue)
    {
        GetDecimal(tomlValue).ShouldBe(0m);
    }

    [Fact]
    public void Float_SmallestDecimalStep()
    {
        GetDecimal("1e-28").ShouldBe(0.0000000000000000000000000001m);
    }

    [Fact]
    public void Float_NearDecimalMaxValue()
    {
        GetDecimal("7.9e28").ShouldBe(79000000000000000000000000000m);
    }

    [Theory]
    [InlineData("1e+29")]
    [InlineData("1.7976931348623157e+308")]
    public void Float_ExceedsDecimalRange_ThrowsOverflowException(string tomlValue)
    {
        Should.Throw<OverflowException>(() => GetDecimal(tomlValue));
    }

    [Theory]
    [InlineData("inf")]
    [InlineData("+inf")]
    [InlineData("-inf")]
    [InlineData("nan")]
    [InlineData("+nan")]
    [InlineData("-nan")]
    public void Float_SpecialValue_ThrowsOverflowException(string tomlValue)
    {
        Should.Throw<OverflowException>(() => GetDecimal(tomlValue));
    }

    [Theory]
    [InlineData("3.14")]
    [InlineData("-123.456")]
    [InlineData("0.30000000000000004")]
    [InlineData("9223372036854775808.0")]
    [InlineData("9223372036854775807")]
    [InlineData("-9223372036854775808")]
    [InlineData("0")]
    [InlineData("1e-28")]
    public void Nullable_ProducesSameValueAsNonNullable(string tomlValue)
    {
        GetNullableDecimal(tomlValue).ShouldBe(GetDecimal(tomlValue));
    }

    [Theory]
    [InlineData("inf")]
    [InlineData("nan")]
    [InlineData("1e+29")]
    public void Nullable_InvalidValue_ThrowsOverflowException(string tomlValue)
    {
        Should.Throw<OverflowException>(() => GetNullableDecimal(tomlValue));
    }

    [Fact]
    public void Nullable_MissingKey_ReturnsNull()
    {
        var document = CsTomlSerializer.Deserialize<TomlDocument>("v = 1.5"u8);
        document.RootNode["missing"].GetValue<decimal?>().ShouldBeNull();
    }

    [Fact]
    public void Conversion_IsCultureInvariant()
    {
        // A culture using ',' as the decimal separator would break a culture-sensitive
        // format/parse round-trip inside the formatter.
        var original = CultureInfo.CurrentCulture;
        try
        {
            CultureInfo.CurrentCulture = new CultureInfo("de-DE");
            GetDecimal("3.14").ShouldBe(3.14m);
            GetDecimal("-123.456").ShouldBe(-123.456m);
            GetDecimal("0.30000000000000004").ToString(CultureInfo.InvariantCulture).ShouldBe("0.30000000000000004");
        }
        finally
        {
            CultureInfo.CurrentCulture = original;
        }
    }

    [Theory]
    [InlineData("3.14")]
    [InlineData("-99.99")]
    [InlineData("0.0000000000000000000000000001")]
    [InlineData("0.30000000000000004")]
    public void RoundTrip_FloatValue(string literal)
    {
        var value = decimal.Parse(literal, NumberStyles.Float, CultureInfo.InvariantCulture);
        RoundTrip(value).ShouldBe(value);
    }

    [Theory]
    [InlineData(long.MaxValue)]
    [InlineData(long.MinValue)]
    [InlineData(0L)]
    [InlineData(-1L)]
    public void RoundTrip_IntegerValue(long value)
    {
        RoundTrip(value).ShouldBe(value);
    }

    private static decimal RoundTrip(decimal value)
    {
        var dict = new Dictionary<string, decimal>() { ["v"] = value };
        using var serialized = CsTomlSerializer.Serialize(dict);
        var document = CsTomlSerializer.Deserialize<TomlDocument>(serialized.ByteSpan);
        return document.RootNode["v"].GetValue<decimal>();
    }
}
