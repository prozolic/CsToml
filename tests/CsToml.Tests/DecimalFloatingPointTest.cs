using CsToml.Error;
using CsToml.Formatter;
using CsToml.Formatter.Resolver;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Numerics;
using System.Text;

namespace CsToml.Tests;

#if NET11_0_OR_GREATER

public class DecimalFloatingPointTest
{
    private static TomlDocument Deserialize(string tomlValue)
        => CsTomlSerializer.Deserialize<TomlDocument>(Encoding.UTF8.GetBytes($"v = {tomlValue}\n"));

    // Returns the invariant text of the deserialized value, which also exposes its quantum (trailing zeros).
    private static string GetText<T>(string tomlValue)
        where T : struct, IDecimalFloatingPointIeee754<T>
        => Deserialize(tomlValue).RootNode["v"].GetValue<T>().ToString(null, CultureInfo.InvariantCulture);

    private static T Get<T>(string tomlValue)
        where T : struct, IDecimalFloatingPointIeee754<T>
        => Deserialize(tomlValue).RootNode["v"].GetValue<T>();

    private static T Parse<T>(string text)
        where T : struct, IDecimalFloatingPointIeee754<T>
        => T.Parse(text, NumberStyles.Float, CultureInfo.InvariantCulture);

    private static string Serialize<[DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.All)] T>(T value)
    {
        using var serialized = CsTomlSerializer.SerializeValueType(value);
        return Encoding.UTF8.GetString(serialized.ByteSpan);
    }

    private static T RoundTrip<T>(T value)
        where T : struct, IDecimalFloatingPointIeee754<T>
    {
        // The dictionary only carries the value; registering its formatter lets this also run without dynamic code (NativeAOT).
        TomlValueFormatterResolver.Register<Dictionary<string, T>, DictionaryFormatter<string, T>>();
        var dict = new Dictionary<string, T>() { ["v"] = value };
        using var serialized = CsTomlSerializer.Serialize(dict);
        var document = CsTomlSerializer.Deserialize<TomlDocument>(serialized.ByteSpan);
        return document.RootNode["v"].GetValue<T>();
    }

    [Test]
    [Arguments("3.14", "3.14")]
    [Arguments("-123.456", "-123.456")]
    [Arguments("-0.01", "-0.01")]
    [Arguments("+1.0", "1")]
    [Arguments("1.5", "1.5")]
    [Arguments("6.626e-3", "0.006626")]
    public void Float_PreservesTextDigits(string tomlValue, string expected)
    {
        // A cast from double would give 3.140000 / 3.140000000000000 / 3.140000000000000124344978758017533.
        GetText<Decimal32>(tomlValue).ShouldBe(expected);
        GetText<Decimal64>(tomlValue).ShouldBe(expected);
        GetText<Decimal128>(tomlValue).ShouldBe(expected);
    }

    [Test]
    public void Float_RoundsToPrecisionOfTargetType()
    {
        // The shortest round-trippable form of the parsed double has 17 digits.
        GetText<Decimal32>("0.30000000000000004").ShouldBe("0.3000000");
        GetText<Decimal64>("0.30000000000000004").ShouldBe("0.3000000000000000");
        GetText<Decimal128>("0.30000000000000004").ShouldBe("0.30000000000000004");
    }

    [Test]
    [Arguments("0")]
    [Arguments("42")]
    [Arguments("-1")]
    [Arguments("999999")]
    [Arguments("1_000_000")]
    [Arguments("0xFF")]
    public void Integer_IsExact(string tomlValue)
    {
        var expected = Deserialize(tomlValue).RootNode["v"].GetValue<long>();

        Get<Decimal32>(tomlValue).ShouldBe((Decimal32)expected);
        Get<Decimal64>(tomlValue).ShouldBe((Decimal64)expected);
        Get<Decimal128>(tomlValue).ShouldBe((Decimal128)expected);
        GetText<Decimal128>(tomlValue).ShouldBe(expected.ToString(CultureInfo.InvariantCulture));
    }

    [Test]
    public void Integer_Int64Boundary()
    {
        // long.MaxValue is not representable as double, so going through double would give ...808.
        GetText<Decimal128>("9223372036854775807").ShouldBe("9223372036854775807");
        GetText<Decimal128>("-9223372036854775808").ShouldBe("-9223372036854775808");

        // Decimal64 / Decimal32 round once to 16 / 7 significant digits.
        GetText<Decimal64>("9223372036854775807").ShouldBe("9.223372036854776E+18");
        GetText<Decimal32>("9223372036854775807").ShouldBe("9.223372E+18");
    }

    [Test]
    public void Float_SpecialValue()
    {
        Decimal32.IsPositiveInfinity(Get<Decimal32>("inf")).ShouldBeTrue();
        Decimal32.IsNegativeInfinity(Get<Decimal32>("-inf")).ShouldBeTrue();
        Decimal32.IsNaN(Get<Decimal32>("nan")).ShouldBeTrue();

        Decimal64.IsPositiveInfinity(Get<Decimal64>("+inf")).ShouldBeTrue();
        Decimal64.IsNegativeInfinity(Get<Decimal64>("-inf")).ShouldBeTrue();
        Decimal64.IsNaN(Get<Decimal64>("-nan")).ShouldBeTrue();

        Decimal128.IsPositiveInfinity(Get<Decimal128>("inf")).ShouldBeTrue();
        Decimal128.IsNegativeInfinity(Get<Decimal128>("-inf")).ShouldBeTrue();
        Decimal128.IsNaN(Get<Decimal128>("+nan")).ShouldBeTrue();
    }

    [Test]
    public void Float_OutOfRangeOfDecimal32()
    {
        // Decimal32 covers 1E-101 .. 9.999999E+96, narrower than double.
        Decimal32.IsPositiveInfinity(Get<Decimal32>("1.7976931348623157e+308")).ShouldBeTrue();
        Decimal32.IsNegativeInfinity(Get<Decimal32>("-1e200")).ShouldBeTrue();
        Get<Decimal32>("1e-300").ShouldBe(Decimal32.Zero);

        GetText<Decimal64>("1.7976931348623157e+308").ShouldBe("1.797693134862316E+308");
        GetText<Decimal128>("1.7976931348623157e+308").ShouldBe("1.7976931348623157E+308");
        GetText<Decimal128>("1e-300").ShouldBe("1E-300");
    }

    [Test]
    public void InvalidType_Throws()
    {
        Should.Throw<CsTomlException>(() => Get<Decimal32>("[ 1.5 ]"));
        Should.Throw<CsTomlException>(() => Get<Decimal64>("[ 1.5 ]"));
        Should.Throw<CsTomlException>(() => Get<Decimal128>("[ 1.5 ]"));
    }

    [Test]
    [Arguments("3.14")]
    [Arguments("1.50")]
    [Arguments("-0.5")]
    [Arguments("100")]
    [Arguments("0")]
    [Arguments("1E+20")]
    [Arguments("1E-06")]
    public void Serialize_WritesInvariantText(string text)
    {
        // The quantum is kept as is (1.50 stays 1.50) and the exponent form is a valid TOML float.
        Serialize(Parse<Decimal32>(text)).ShouldBe(text);
        Serialize(Parse<Decimal64>(text)).ShouldBe(text);
        Serialize(Parse<Decimal128>(text)).ShouldBe(text);
    }

    [Test]
    public void Serialize_SpecialValue()
    {
        Serialize(Decimal32.PositiveInfinity).ShouldBe("inf");
        Serialize(Decimal32.NegativeInfinity).ShouldBe("-inf");
        Serialize(Decimal32.NaN).ShouldBe("nan");

        Serialize(Decimal64.PositiveInfinity).ShouldBe("inf");
        Serialize(Decimal64.NegativeInfinity).ShouldBe("-inf");
        Serialize(Decimal64.NaN).ShouldBe("nan");
        Serialize(-Decimal64.NaN).ShouldBe("nan");

        Serialize(Decimal128.PositiveInfinity).ShouldBe("inf");
        Serialize(Decimal128.NegativeInfinity).ShouldBe("-inf");
        Serialize(Decimal128.NaN).ShouldBe("nan");
    }

    [Test]
    public void Serialize_IsCultureInvariant()
    {
        var original = CultureInfo.CurrentCulture;
        try
        {
            CultureInfo.CurrentCulture = new CultureInfo("de-DE");
            Serialize(Parse<Decimal64>("3.14")).ShouldBe("3.14");
            GetText<Decimal64>("3.14").ShouldBe("3.14");
            GetText<Decimal128>("-123.456").ShouldBe("-123.456");
        }
        finally
        {
            CultureInfo.CurrentCulture = original;
        }
    }

    [Test]
    [Arguments("9223372036854775807", "9223372036854775807")]
    [Arguments("-9223372036854775808", "-9223372036854775808")]
    [Arguments("9223372036854775808", "9223372036854775808.0")]
    [Arguments("-9223372036854775809", "-9223372036854775809.0")]
    [Arguments("1234567890123456789012345678901234", "1234567890123456789012345678901234.0")]
    [Arguments("-1234567890123456789012345678901234", "-1234567890123456789012345678901234.0")]
    [Arguments("123456789012345678", "123456789012345678")]
    public void Serialize_Decimal128IntegerBeyondInt64_IsWrittenAsFloat(string text, string expected)
    {
        // A TOML integer must fit in Int64; anything larger has to be a float to stay parsable.
        Serialize(Parse<Decimal128>(text)).ShouldBe(expected);
    }

    [Test]
    public void RoundTrip_Decimal128IntegerBeyondInt64()
    {
        var value = Parse<Decimal128>("1234567890123456789012345678901234");

        // Read back through double, so only the leading 17 digits survive.
        RoundTrip(value).ShouldBe(Parse<Decimal128>("1.2345678901234568E+33"));
        RoundTrip((Decimal128)long.MaxValue).ShouldBe((Decimal128)long.MaxValue);
        RoundTrip((Decimal128)long.MinValue).ShouldBe((Decimal128)long.MinValue);
    }

    [Test]
    [Arguments("3.14")]
    [Arguments("-99.99")]
    [Arguments("0.000001")]
    [Arguments("1E-20")]
    [Arguments("1E+20")]
    [Arguments("1234567")]
    [Arguments("-0.5")]
    public void RoundTrip_Value(string text)
    {
        RoundTrip(Parse<Decimal32>(text)).ShouldBe(Parse<Decimal32>(text));
        RoundTrip(Parse<Decimal64>(text)).ShouldBe(Parse<Decimal64>(text));
        RoundTrip(Parse<Decimal128>(text)).ShouldBe(Parse<Decimal128>(text));
    }

    [Test]
    public void RoundTrip_ValueRoundedOnConstruction()
    {
        // 12345678 does not fit in the 7 digits of Decimal32 and is formatted as 1.234568E+07.
        var value = (Decimal32)12345678L;
        Serialize(value).ShouldBe("1.234568E+07");
        RoundTrip(value).ShouldBe(value);
    }

    [Test]
    public void RoundTrip_DefaultValue()
    {
        // default has the smallest exponent of each type (0E-101 / 0E-398 / 0E-6176).
        RoundTrip(default(Decimal32)).ShouldBe(Decimal32.Zero);
        RoundTrip(default(Decimal64)).ShouldBe(Decimal64.Zero);
        RoundTrip(default(Decimal128)).ShouldBe(Decimal128.Zero);
    }

    [Test]
    public void RoundTrip_SpecialValue()
    {
        Decimal64.IsPositiveInfinity(RoundTrip(Decimal64.PositiveInfinity)).ShouldBeTrue();
        Decimal64.IsNegativeInfinity(RoundTrip(Decimal64.NegativeInfinity)).ShouldBeTrue();
        Decimal64.IsNaN(RoundTrip(Decimal64.NaN)).ShouldBeTrue();
    }

    [Test]
    [Arguments("3.14")]
    [Arguments("0.30000000000000004")]
    [Arguments("9223372036854775807")]
    [Arguments("0")]
    [Arguments("inf")]
    public void Nullable_ProducesSameValueAsNonNullable(string tomlValue)
    {
        var node = Deserialize(tomlValue).RootNode["v"];

        node.GetValue<Decimal32?>().ShouldBe(node.GetValue<Decimal32>());
        node.GetValue<Decimal64?>().ShouldBe(node.GetValue<Decimal64>());
        node.GetValue<Decimal128?>().ShouldBe(node.GetValue<Decimal128>());
    }

    [Test]
    public void Nullable_MissingKey_ReturnsNull()
    {
        var node = Deserialize("1.5").RootNode["missing"];

        node.GetValue<Decimal32?>().ShouldBeNull();
        node.GetValue<Decimal64?>().ShouldBeNull();
        node.GetValue<Decimal128?>().ShouldBeNull();
    }

    [Test]
    public void Nullable_Serialize()
    {
        Serialize<Decimal32?>(Parse<Decimal32>("3.14")).ShouldBe("3.14");
        Serialize<Decimal64?>(Parse<Decimal64>("3.14")).ShouldBe("3.14");
        Serialize<Decimal128?>(Parse<Decimal128>("3.14")).ShouldBe("3.14");

        Should.Throw<CsTomlException>(() => Serialize<Decimal32?>(null));
        Should.Throw<CsTomlException>(() => Serialize<Decimal64?>(null));
        Should.Throw<CsTomlException>(() => Serialize<Decimal128?>(null));
    }

    [Test]
    [SkipOnNativeAot("Decimal64[] and List<Decimal64> have no pre-registered formatter.")]
    public void Collection()
    {
        Decimal64[] values = [Parse<Decimal64>("3.14"), Parse<Decimal64>("-0.5"), (Decimal64)100];

        using var serialized = CsTomlSerializer.SerializeValueType(values);
        Encoding.UTF8.GetString(serialized.ByteSpan).ShouldBe("[ 3.14, -0.5, 100 ]");

        CsTomlSerializer.DeserializeValueType<Decimal64[]>(serialized.ByteSpan).ShouldBe(values);
        CsTomlSerializer.DeserializeValueType<List<Decimal64>>(serialized.ByteSpan).ShouldBe(values);
    }
}

#endif