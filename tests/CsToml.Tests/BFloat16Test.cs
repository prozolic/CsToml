using CsToml.Error;
using CsToml.Formatter;
using CsToml.Formatter.Resolver;
using System.Numerics;
using System.Text;

namespace CsToml.Tests;

#if NET11_0_OR_GREATER

public class BFloat16Test
{
    private static TomlDocument Deserialize(string tomlValue)
        => CsTomlSerializer.Deserialize<TomlDocument>(Encoding.UTF8.GetBytes($"v = {tomlValue}\n"));

    private static BFloat16 GetBFloat16(string tomlValue)
        => Deserialize(tomlValue).RootNode["v"].GetValue<BFloat16>();

    private static BFloat16? GetNullableBFloat16(string tomlValue)
        => Deserialize(tomlValue).RootNode["v"].GetValue<BFloat16?>();

    private static string Serialize(BFloat16 value)
    {
        using var serialized = CsTomlSerializer.SerializeValueType(value);
        return Encoding.UTF8.GetString(serialized.ByteSpan);
    }

    [Test]
    [Arguments("1.5", 1.5)]
    [Arguments("-2.0", -2.0)]
    [Arguments("0.25", 0.25)]
    [Arguments("0.0", 0.0)]
    [Arguments("3", 3.0)]
    [Arguments("1e10", 1e10)]
    [Arguments("3.14", 3.14)]
    public void Deserialize_Float(string tomlValue, double expected)
    {
        GetBFloat16(tomlValue).ShouldBe((BFloat16)expected);
    }

    [Test]
    public void Deserialize_SpecialValue()
    {
        BFloat16.IsPositiveInfinity(GetBFloat16("inf")).ShouldBeTrue();
        BFloat16.IsNegativeInfinity(GetBFloat16("-inf")).ShouldBeTrue();
        BFloat16.IsNaN(GetBFloat16("nan")).ShouldBeTrue();
    }

    [Test]
    public void Deserialize_OutOfRange_IsInfinity()
    {
        // BFloat16.MaxValue is about 3.39E+38.
        BFloat16.IsPositiveInfinity(GetBFloat16("1e300")).ShouldBeTrue();
        BFloat16.IsNegativeInfinity(GetBFloat16("-1e300")).ShouldBeTrue();
    }

    [Test]
    public void Deserialize_InvalidType_Throws()
    {
        Should.Throw<CsTomlException>(() => GetBFloat16("[ 1.5 ]"));
        Should.Throw<CsTomlException>(() => GetBFloat16("\"abc\""));
    }

    [Test]
    [Arguments(1.5, "1.5")]
    [Arguments(-2.0, "-2.0")]
    [Arguments(0.25, "0.25")]
    [Arguments(0.0, "0.0")]
    [Arguments(100.0, "100.0")]
    public void Serialize_ExactlyRepresentableValue(double value, string expected)
    {
        Serialize((BFloat16)value).ShouldBe(expected);
    }

    [Test]
    public void Serialize_WritesWidenedDoubleValue()
    {
        // Like float and Half, the value is widened to double: BFloat16 has 8 significant bits,
        // so 3.14 is stored as 3.140625.
        Serialize((BFloat16)3.14).ShouldBe("3.140625");
    }

    [Test]
    public void Serialize_SpecialValue()
    {
        Serialize(BFloat16.PositiveInfinity).ShouldBe("inf");
        Serialize(BFloat16.NegativeInfinity).ShouldBe("-inf");
        Serialize(BFloat16.NaN).ShouldBe("nan");
    }

    [Test]
    [Arguments(3.14)]
    [Arguments(0.1)]
    [Arguments(-123456.789)]
    [Arguments(1e-10)]
    [Arguments(3.0e38)]
    public void RoundTrip(double source)
    {
        var value = (BFloat16)source;
        // The dictionary only carries the value; registering its formatter lets this also run without dynamic code (NativeAOT).
        TomlValueFormatterResolver.Register<Dictionary<string, BFloat16>, DictionaryFormatter<string, BFloat16>>();
        var dict = new Dictionary<string, BFloat16>() { ["v"] = value };
        using var serialized = CsTomlSerializer.Serialize(dict);
        var document = CsTomlSerializer.Deserialize<TomlDocument>(serialized.ByteSpan);

        document.RootNode["v"].GetValue<BFloat16>().ShouldBe(value);
    }

    [Test]
    public void RoundTrip_MinMaxEpsilon()
    {
        foreach (var value in new[] { BFloat16.MaxValue, BFloat16.MinValue, BFloat16.Epsilon })
        {
            using var serialized = CsTomlSerializer.SerializeValueType(value);
            CsTomlSerializer.DeserializeValueType<BFloat16>(serialized.ByteSpan).ShouldBe(value);
        }
    }

    [Test]
    [Arguments("1.5")]
    [Arguments("3")]
    [Arguments("3.14")]
    public void Nullable_ProducesSameValueAsNonNullable(string tomlValue)
    {
        GetNullableBFloat16(tomlValue).ShouldBe(GetBFloat16(tomlValue));
    }

    [Test]
    public void Nullable_MissingKey_ReturnsNull()
    {
        Deserialize("1.5").RootNode["missing"].GetValue<BFloat16?>().ShouldBeNull();
    }

    [Test]
    public void Nullable_Serialize()
    {
        using var serialized = CsTomlSerializer.SerializeValueType<BFloat16?>((BFloat16)1.5);
        Encoding.UTF8.GetString(serialized.ByteSpan).ShouldBe("1.5");

        Should.Throw<CsTomlException>(() => CsTomlSerializer.SerializeValueType<BFloat16?>(null));
    }

    [Test]
    [SkipOnNativeAot("BFloat16[] and List<BFloat16> have no pre-registered formatter.")]
    public void Collection()
    {
        BFloat16[] values = [(BFloat16)1.5, (BFloat16)(-0.25), (BFloat16)8.0];

        using var serialized = CsTomlSerializer.SerializeValueType(values);
        Encoding.UTF8.GetString(serialized.ByteSpan).ShouldBe("[ 1.5, -0.25, 8.0 ]");

        CsTomlSerializer.DeserializeValueType<BFloat16[]>(serialized.ByteSpan).ShouldBe(values);
        CsTomlSerializer.DeserializeValueType<List<BFloat16>>(serialized.ByteSpan).ShouldBe(values);
    }
}

#endif