using CsToml.Error;
using System.Globalization;
using System.Numerics;
using System.Text;

namespace CsToml.Tests;

#if NET11_0_OR_GREATER

public class ComplexOfTTest
{
    private static TomlDocument Deserialize(string tomlValue)
        => CsTomlSerializer.Deserialize<TomlDocument>(Encoding.UTF8.GetBytes($"v = {tomlValue}\n"));

    private static string Serialize<T>(T value)
    {
        using var serialized = CsTomlSerializer.SerializeValueType(value);
        return Encoding.UTF8.GetString(serialized.ByteSpan);
    }

    [Fact]
    public void Serialize_BinaryFloatingPoint()
    {
        Serialize(new Complex<float>(1.5f, -2f)).ShouldBe("[ 1.5, -2.0 ]");
        Serialize(new Complex<double>(12, 6)).ShouldBe("[ 12.0, 6.0 ]");
        Serialize(new Complex<Half>((Half)1.5, (Half)0.25)).ShouldBe("[ 1.5, 0.25 ]");
        Serialize(new Complex<BFloat16>((BFloat16)1.5, (BFloat16)0.25)).ShouldBe("[ 1.5, 0.25 ]");
    }

    [Fact]
    public void Serialize_DecimalFloatingPoint()
    {
        Serialize(new Complex<Decimal32>(Decimal32.Parse("3.14", CultureInfo.InvariantCulture), (Decimal32)100)).ShouldBe("[ 3.14, 100 ]");
        Serialize(new Complex<Decimal64>(Decimal64.Parse("3.14", CultureInfo.InvariantCulture), 100)).ShouldBe("[ 3.14, 100 ]");
        Serialize(new Complex<Decimal128>(Decimal128.Parse("3.14", CultureInfo.InvariantCulture), 100)).ShouldBe("[ 3.14, 100 ]");
    }

    [Fact]
    public void Serialize_SpecialValue()
    {
        Serialize(new Complex<double>(double.NaN, double.NegativeInfinity)).ShouldBe("[ nan, -inf ]");
        Serialize(new Complex<Decimal64>(Decimal64.PositiveInfinity, Decimal64.NaN)).ShouldBe("[ inf, nan ]");
    }

    [Fact]
    public void Deserialize_BinaryFloatingPoint()
    {
        var node = Deserialize("[ 1.5, -2.0 ]").RootNode["v"];

        node.GetValue<Complex<float>>().ShouldBe(new Complex<float>(1.5f, -2f));
        node.GetValue<Complex<double>>().ShouldBe(new Complex<double>(1.5, -2));
        node.GetValue<Complex<Half>>().ShouldBe(new Complex<Half>((Half)1.5, (Half)(-2)));
        node.GetValue<Complex<BFloat16>>().ShouldBe(new Complex<BFloat16>((BFloat16)1.5, (BFloat16)(-2)));
    }

    [Fact]
    public void Deserialize_DecimalFloatingPoint()
    {
        var node = Deserialize("[ 3.14, 100 ]").RootNode["v"];

        var decimal32 = node.GetValue<Complex<Decimal32>>();
        decimal32.Real.ToString(null, CultureInfo.InvariantCulture).ShouldBe("3.14");
        decimal32.Imaginary.ShouldBe((Decimal32)100);

        var decimal64 = node.GetValue<Complex<Decimal64>>();
        decimal64.Real.ToString(null, CultureInfo.InvariantCulture).ShouldBe("3.14");
        decimal64.Imaginary.ShouldBe((Decimal64)100);

        var decimal128 = node.GetValue<Complex<Decimal128>>();
        decimal128.Real.ToString(null, CultureInfo.InvariantCulture).ShouldBe("3.14");
        decimal128.Imaginary.ShouldBe((Decimal128)100);
    }

    [Fact]
    public void Deserialize_IntegerElements()
    {
        Deserialize("[ 12, 6 ]").RootNode["v"].GetValue<Complex<double>>().ShouldBe(new Complex<double>(12, 6));
    }

    [Theory]
    [InlineData("[ 1.0 ]")]
    [InlineData("[ 1.0, 2.0, 3.0 ]")]
    [InlineData("[]")]
    [InlineData("1.0")]
    [InlineData("\"1.0\"")]
    public void Deserialize_InvalidShape_Throws(string tomlValue)
    {
        Should.Throw<CsTomlException>(() => Deserialize(tomlValue).RootNode["v"].GetValue<Complex<double>>());
    }

    [Fact]
    public void RoundTrip()
    {
        var value = new Complex<float>(3.14f, -0.1f);
        var dict = new Dictionary<string, Complex<float>>() { ["v"] = value };
        using var serialized = CsTomlSerializer.Serialize(dict);
        var document = CsTomlSerializer.Deserialize<TomlDocument>(serialized.ByteSpan);

        document.RootNode["v"].GetValue<Complex<float>>().ShouldBe(value);
    }

    [Fact]
    public void Nullable()
    {
        var node = Deserialize("[ 1.5, -2.0 ]").RootNode["v"];
        node.GetValue<Complex<float>?>().ShouldBe(new Complex<float>(1.5f, -2f));

        Deserialize("[ 1.5, -2.0 ]").RootNode["missing"].GetValue<Complex<float>?>().ShouldBeNull();

        Serialize<Complex<float>?>(new Complex<float>(1.5f, -2f)).ShouldBe("[ 1.5, -2.0 ]");
        Should.Throw<CsTomlException>(() => Serialize<Complex<float>?>(null));
    }

    [Fact]
    public void Collection()
    {
        Complex<double>[] values = [new(1, 2), new(-3.5, 0.25)];

        using var serialized = CsTomlSerializer.SerializeValueType(values);
        Encoding.UTF8.GetString(serialized.ByteSpan).ShouldBe("[ [ 1.0, 2.0 ], [ -3.5, 0.25 ] ]");

        CsTomlSerializer.DeserializeValueType<Complex<double>[]>(serialized.ByteSpan).ShouldBe(values);
        CsTomlSerializer.DeserializeValueType<List<Complex<double>>>(serialized.ByteSpan).ShouldBe(values);
    }
}

#endif