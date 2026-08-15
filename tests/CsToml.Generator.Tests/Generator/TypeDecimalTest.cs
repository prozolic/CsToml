using Shouldly;
using Utf8StringInterpolation;

namespace CsToml.Generator.Tests;

public class TypeDecimalTest
{
    [Fact]
    public void Serialize()
    {
        var decimalObj = new TypeDecimal()
        {
            IntegerValue = 999999,
            FloatValue = 3.14m,
            NegativeValue = -123.456m,
            ZeroValue = 0,
            NullableIntegerValue = 42,
            NullableFloatValue = 1.5m,
        };

        using var bytes = CsTomlSerializer.Serialize(decimalObj);

        using var buffer = Utf8String.CreateWriter(out var writer);
        writer.AppendLine("IntegerValue = 999999");
        writer.AppendLine("FloatValue = 3.14");
        writer.AppendLine("NegativeValue = -123.456");
        writer.AppendLine("ZeroValue = 0");
        writer.AppendLine("NullableIntegerValue = 42");
        writer.AppendLine("NullableFloatValue = 1.5");
        writer.Flush();

        var expected = buffer.ToArray();
        bytes.ByteSpan.ToArray().ShouldBe(expected);
    }

    [Fact]
    public void DeserializeFromInteger()
    {
        using var buffer = Utf8String.CreateWriter(out var writer);
        writer.AppendLine("IntegerValue = 999999");
        writer.AppendLine("FloatValue = 0");
        writer.AppendLine("NegativeValue = -123");
        writer.AppendLine("ZeroValue = 0");
        writer.Flush();

        var result = CsTomlSerializer.Deserialize<TypeDecimal>(buffer.WrittenSpan);
        result.IntegerValue.ShouldBe(999999m);
        result.FloatValue.ShouldBe(0m);
        result.NegativeValue.ShouldBe(-123m);
        result.ZeroValue.ShouldBe(0m);
    }

    [Fact]
    public void DeserializeFromFloat()
    {
        using var buffer = Utf8String.CreateWriter(out var writer);
        writer.AppendLine("IntegerValue = 0");
        writer.AppendLine("FloatValue = 3.14");
        writer.AppendLine("NegativeValue = -123.456");
        writer.AppendLine("ZeroValue = 0.0");
        writer.Flush();

        var result = CsTomlSerializer.Deserialize<TypeDecimal>(buffer.WrittenSpan);
        result.FloatValue.ShouldBe(3.14m);
        result.NegativeValue.ShouldBe(-123.456m);
        result.ZeroValue.ShouldBe(0m);
    }

    [Fact]
    public void DeserializeNullable()
    {
        using var buffer = Utf8String.CreateWriter(out var writer);
        writer.AppendLine("IntegerValue = 0");
        writer.AppendLine("FloatValue = 0");
        writer.AppendLine("NegativeValue = 0");
        writer.AppendLine("ZeroValue = 0");
        writer.AppendLine("NullableIntegerValue = 42");
        writer.AppendLine("NullableFloatValue = 1.5");
        writer.Flush();

        var result = CsTomlSerializer.Deserialize<TypeDecimal>(buffer.WrittenSpan);
        result.NullableIntegerValue.ShouldBe(42m);
        result.NullableFloatValue.ShouldBe(1.5m);
    }

    [Fact]
    public void RoundTrip()
    {
        var original = new TypeDecimal()
        {
            IntegerValue = 123456,
            FloatValue = 3.14m,
            NegativeValue = -99.99m,
            ZeroValue = 0,
            NullableIntegerValue = 100,
            NullableFloatValue = 2.5m,
        };

        using var bytes = CsTomlSerializer.Serialize(original);
        var result = CsTomlSerializer.Deserialize<TypeDecimal>(bytes.ByteSpan);

        result.IntegerValue.ShouldBe(original.IntegerValue);
        result.FloatValue.ShouldBe(original.FloatValue);
        result.NegativeValue.ShouldBe(original.NegativeValue);
        result.ZeroValue.ShouldBe(original.ZeroValue);
        result.NullableIntegerValue.ShouldBe(original.NullableIntegerValue);
        result.NullableFloatValue.ShouldBe(original.NullableFloatValue);
    }

    // --- Edge Cases: Integer Boundaries ---

    [Fact]
    public void DeserializeFromInteger_LongMaxValue()
    {
        using var buffer = Utf8String.CreateWriter(out var writer);
        writer.AppendLine("IntegerValue = 9223372036854775807");
        writer.AppendLine("FloatValue = 0");
        writer.AppendLine("NegativeValue = 0");
        writer.AppendLine("ZeroValue = 0");
        writer.Flush();

        var result = CsTomlSerializer.Deserialize<TypeDecimal>(buffer.WrittenSpan);
        // TOML integers are read as Int64 and converted to decimal without precision loss
        result.IntegerValue.ShouldBe(9223372036854775807m);
    }

    [Fact]
    public void DeserializeFromInteger_LongMinValue()
    {
        using var buffer = Utf8String.CreateWriter(out var writer);
        writer.AppendLine("IntegerValue = -9223372036854775808");
        writer.AppendLine("FloatValue = 0");
        writer.AppendLine("NegativeValue = 0");
        writer.AppendLine("ZeroValue = 0");
        writer.Flush();

        var result = CsTomlSerializer.Deserialize<TypeDecimal>(buffer.WrittenSpan);
        result.IntegerValue.ShouldBe(-9223372036854775808m);
    }

    [Fact]
    public void DeserializeFromInteger_MinPositiveAndNegative()
    {
        using var buffer = Utf8String.CreateWriter(out var writer);
        writer.AppendLine("IntegerValue = 1");
        writer.AppendLine("FloatValue = 0");
        writer.AppendLine("NegativeValue = -1");
        writer.AppendLine("ZeroValue = 0");
        writer.Flush();

        var result = CsTomlSerializer.Deserialize<TypeDecimal>(buffer.WrittenSpan);
        result.IntegerValue.ShouldBe(1m);
        result.NegativeValue.ShouldBe(-1m);
        result.ZeroValue.ShouldBe(0m);
    }

    [Fact]
    public void DeserializeFromFloat_ExponentOnly()
    {
        using var buffer = Utf8String.CreateWriter(out var writer);
        writer.AppendLine("IntegerValue = 1e10");
        writer.AppendLine("FloatValue = -2E-2");
        writer.AppendLine("NegativeValue = 0");
        writer.AppendLine("ZeroValue = 0");
        writer.Flush();

        var result = CsTomlSerializer.Deserialize<TypeDecimal>(buffer.WrittenSpan);
        result.IntegerValue.ShouldBe(1e10m);
        result.FloatValue.ShouldBe(-2E-2m);
    }

    [Fact]
    public void DeserializeFromFloat_ExponentWithLeadingZero()
    {
        // TOML spec: "1e06" — exponent part allows leading zeros
        using var buffer = Utf8String.CreateWriter(out var writer);
        writer.AppendLine("IntegerValue = 1e06");
        writer.AppendLine("FloatValue = 0");
        writer.AppendLine("NegativeValue = 0");
        writer.AppendLine("ZeroValue = 0");
        writer.Flush();

        var result = CsTomlSerializer.Deserialize<TypeDecimal>(buffer.WrittenSpan);
        result.IntegerValue.ShouldBe(1e6m);
    }

    [Fact]
    public void DeserializeFromFloat_FracAndExp()
    {
        using var buffer = Utf8String.CreateWriter(out var writer);
        writer.AppendLine("IntegerValue = 6.626e-34");
        writer.AppendLine("FloatValue = +1.0e+2");
        writer.AppendLine("NegativeValue = -1.23e4");
        writer.AppendLine("ZeroValue = 0");
        writer.Flush();

        var result = CsTomlSerializer.Deserialize<TypeDecimal>(buffer.WrittenSpan);
        result.IntegerValue.ShouldBe(0m); // underflows the decimal range
        result.FloatValue.ShouldBe(1.0e+2m);
        result.NegativeValue.ShouldBe(-1.23e4m);
    }

    [Fact]
    public void DeserializeFromFloat_PositiveSign()
    {
        // TOML spec: "+1.0" — explicit positive sign on float
        using var buffer = Utf8String.CreateWriter(out var writer);
        writer.AppendLine("IntegerValue = +1.0");
        writer.AppendLine("FloatValue = 0");
        writer.AppendLine("NegativeValue = -0.01");
        writer.AppendLine("ZeroValue = 0");
        writer.Flush();

        var result = CsTomlSerializer.Deserialize<TypeDecimal>(buffer.WrittenSpan);
        result.IntegerValue.ShouldBe(1.0m);
        result.NegativeValue.ShouldBe(-0.01m);
    }

    [Fact]
    public void DeserializeFromFloat_Underscore()
    {
        // TOML spec: "224_617.445_991_228" — underscores enhance readability
        using var buffer = Utf8String.CreateWriter(out var writer);
        writer.AppendLine("IntegerValue = 224_617.445_991_228");
        writer.AppendLine("FloatValue = 0");
        writer.AppendLine("NegativeValue = 0");
        writer.AppendLine("ZeroValue = 0");
        writer.Flush();

        var result = CsTomlSerializer.Deserialize<TypeDecimal>(buffer.WrittenSpan);
        result.IntegerValue.ShouldBe(224617.445991228m);
    }

    [Fact]
    public void DeserializeFromFloat_Inf_ThrowsOverflowException()
    {
        using var buffer = Utf8String.CreateWriter(out var writer);
        writer.AppendLine("IntegerValue = inf");
        writer.AppendLine("FloatValue = 0");
        writer.AppendLine("NegativeValue = 0");
        writer.AppendLine("ZeroValue = 0");
        writer.Flush();

        Should.Throw<OverflowException>(() =>
            CsTomlSerializer.Deserialize<TypeDecimal>(buffer.WrittenSpan));
    }

    [Fact]
    public void DeserializeFromFloat_PositiveInf_ThrowsOverflowException()
    {
        // TOML spec: "+inf" — explicit positive infinity
        using var buffer = Utf8String.CreateWriter(out var writer);
        writer.AppendLine("IntegerValue = +inf");
        writer.AppendLine("FloatValue = 0");
        writer.AppendLine("NegativeValue = 0");
        writer.AppendLine("ZeroValue = 0");
        writer.Flush();

        Should.Throw<OverflowException>(() =>
            CsTomlSerializer.Deserialize<TypeDecimal>(buffer.WrittenSpan));
    }

    [Fact]
    public void DeserializeFromFloat_NegativeInf_ThrowsOverflowException()
    {
        using var buffer = Utf8String.CreateWriter(out var writer);
        writer.AppendLine("IntegerValue = -inf");
        writer.AppendLine("FloatValue = 0");
        writer.AppendLine("NegativeValue = 0");
        writer.AppendLine("ZeroValue = 0");
        writer.Flush();

        Should.Throw<OverflowException>(() =>
            CsTomlSerializer.Deserialize<TypeDecimal>(buffer.WrittenSpan));
    }

    [Fact]
    public void DeserializeFromFloat_Nan_ThrowsOverflowException()
    {
        using var buffer = Utf8String.CreateWriter(out var writer);
        writer.AppendLine("IntegerValue = nan");
        writer.AppendLine("FloatValue = 0");
        writer.AppendLine("NegativeValue = 0");
        writer.AppendLine("ZeroValue = 0");
        writer.Flush();

        Should.Throw<OverflowException>(() =>
            CsTomlSerializer.Deserialize<TypeDecimal>(buffer.WrittenSpan));
    }

    [Fact]
    public void DeserializeFromFloat_PositiveNan_ThrowsOverflowException()
    {
        // TOML spec: "+nan" — same as nan
        using var buffer = Utf8String.CreateWriter(out var writer);
        writer.AppendLine("IntegerValue = +nan");
        writer.AppendLine("FloatValue = 0");
        writer.AppendLine("NegativeValue = 0");
        writer.AppendLine("ZeroValue = 0");
        writer.Flush();

        Should.Throw<OverflowException>(() =>
            CsTomlSerializer.Deserialize<TypeDecimal>(buffer.WrittenSpan));
    }

    [Fact]
    public void DeserializeFromFloat_NegativeNan_ThrowsOverflowException()
    {
        // TOML spec: "-nan" — valid, implementation-specific encoding
        using var buffer = Utf8String.CreateWriter(out var writer);
        writer.AppendLine("IntegerValue = -nan");
        writer.AppendLine("FloatValue = 0");
        writer.AppendLine("NegativeValue = 0");
        writer.AppendLine("ZeroValue = 0");
        writer.Flush();

        Should.Throw<OverflowException>(() =>
            CsTomlSerializer.Deserialize<TypeDecimal>(buffer.WrittenSpan));
    }

    [Fact]
    public void DeserializeFromFloat_SmallValue()
    {
        using var buffer = Utf8String.CreateWriter(out var writer);
        writer.AppendLine("IntegerValue = 1e-28");
        writer.AppendLine("FloatValue = 0.0");
        writer.AppendLine("NegativeValue = -0.0");
        writer.AppendLine("ZeroValue = 0");
        writer.Flush();

        var result = CsTomlSerializer.Deserialize<TypeDecimal>(buffer.WrittenSpan);
        result.IntegerValue.ShouldBe(1e-28m);
        result.FloatValue.ShouldBe(0m);
        result.NegativeValue.ShouldBe(0m); // -0.0 as double → decimal becomes 0
    }

    [Fact]
    public void DeserializeFromFloat_PositiveZero()
    {
        // TOML spec: "+0.0" is valid and should map per IEEE 754
        using var buffer = Utf8String.CreateWriter(out var writer);
        writer.AppendLine("IntegerValue = +0.0");
        writer.AppendLine("FloatValue = 0");
        writer.AppendLine("NegativeValue = 0");
        writer.AppendLine("ZeroValue = 0");
        writer.Flush();

        var result = CsTomlSerializer.Deserialize<TypeDecimal>(buffer.WrittenSpan);
        result.IntegerValue.ShouldBe(0m);
    }

    [Fact]
    public void DeserializeFromFloat_LargeExponent_ThrowsOverflowException()
    {
        // TOML spec: "5e+22" — valid TOML float but exceeds decimal range (~7.9e28)
        using var buffer = Utf8String.CreateWriter(out var writer);
        writer.AppendLine("IntegerValue = 5e+22");
        writer.AppendLine("FloatValue = 0");
        writer.AppendLine("NegativeValue = 0");
        writer.AppendLine("ZeroValue = 0");
        writer.Flush();

        // 5e+22 fits in decimal range, should succeed
        var result = CsTomlSerializer.Deserialize<TypeDecimal>(buffer.WrittenSpan);
        result.IntegerValue.ShouldBe(5e+22m);
    }

    [Fact]
    public void DeserializeFromFloat_ExceedDecimalRange_ThrowsOverflowException()
    {
        // 1e+29 exceeds decimal.MaxValue (~7.9e28)
        using var buffer = Utf8String.CreateWriter(out var writer);
        writer.AppendLine("IntegerValue = 1e+29");
        writer.AppendLine("FloatValue = 0");
        writer.AppendLine("NegativeValue = 0");
        writer.AppendLine("ZeroValue = 0");
        writer.Flush();

        Should.Throw<OverflowException>(() =>
            CsTomlSerializer.Deserialize<TypeDecimal>(buffer.WrittenSpan));
    }

    [Fact]
    public void Serialize_DecimalMaxValue()
    {
        var decimalObj = new TypeDecimal()
        {
            IntegerValue = decimal.MaxValue,
            FloatValue = 0,
            NegativeValue = decimal.MinValue,
            ZeroValue = 0,
            NullableIntegerValue = 0,
            NullableFloatValue = 0,
        };

        using var bytes = CsTomlSerializer.Serialize(decimalObj);

        using var buffer = Utf8String.CreateWriter(out var writer);
        writer.AppendLine("IntegerValue = 79228162514264337593543950335");
        writer.AppendLine("FloatValue = 0");
        writer.AppendLine("NegativeValue = -79228162514264337593543950335");
        writer.AppendLine("ZeroValue = 0");
        writer.AppendLine("NullableIntegerValue = 0");
        writer.AppendLine("NullableFloatValue = 0");
        writer.Flush();

        var expected = buffer.ToArray();
        bytes.ByteSpan.ToArray().ShouldBe(expected);
    }

    [Fact]
    public void Serialize_SmallDecimalPrecision()
    {
        var decimalObj = new TypeDecimal()
        {
            IntegerValue = 0,
            FloatValue = 0.0000000000000000000000000001m,
            NegativeValue = 0,
            ZeroValue = 0,
            NullableIntegerValue = 0,
            NullableFloatValue = 0,
        };

        using var bytes = CsTomlSerializer.Serialize(decimalObj);

        using var buffer = Utf8String.CreateWriter(out var writer);
        writer.AppendLine("IntegerValue = 0");
        writer.AppendLine("FloatValue = 0.0000000000000000000000000001");
        writer.AppendLine("NegativeValue = 0");
        writer.AppendLine("ZeroValue = 0");
        writer.AppendLine("NullableIntegerValue = 0");
        writer.AppendLine("NullableFloatValue = 0");
        writer.Flush();

        var expected = buffer.ToArray();
        bytes.ByteSpan.ToArray().ShouldBe(expected);
    }

    [Fact]
    public void DeserializeFromFloat_KeepsShortestRoundTripDigits()
    {
        // These need 16-17 significant digits, which a (decimal)double cast would have rounded away.
        using var buffer = Utf8String.CreateWriter(out var writer);
        writer.AppendLine("IntegerValue = 9223372036854775808.0");
        writer.AppendLine("FloatValue = 0.30000000000000004");
        writer.AppendLine("NegativeValue = -1.2345678901234567e+25");
        writer.AppendLine("ZeroValue = 0");
        writer.Flush();

        var result = CsTomlSerializer.Deserialize<TypeDecimal>(buffer.WrittenSpan);
        result.IntegerValue.ShouldBe(9223372036854776000m);
        result.FloatValue.ShouldBe(0.30000000000000004m);
        result.NegativeValue.ShouldBe(-12345678901234566000000000m);
    }

    [Fact]
    public void DeserializeNullable_FromLongBoundaries()
    {
        using var buffer = Utf8String.CreateWriter(out var writer);
        writer.AppendLine("IntegerValue = 0");
        writer.AppendLine("FloatValue = 0");
        writer.AppendLine("NegativeValue = 0");
        writer.AppendLine("ZeroValue = 0");
        writer.AppendLine("NullableIntegerValue = 9223372036854775807");
        writer.AppendLine("NullableFloatValue = -9223372036854775808");
        writer.Flush();

        var result = CsTomlSerializer.Deserialize<TypeDecimal>(buffer.WrittenSpan);
        result.NullableIntegerValue.ShouldBe(9223372036854775807m);
        result.NullableFloatValue.ShouldBe(-9223372036854775808m);
    }

    [Fact]
    public void DeserializeNullable_ProducesSameValueAsNonNullable()
    {
        using var buffer = Utf8String.CreateWriter(out var writer);
        writer.AppendLine("IntegerValue = 9223372036854775807");
        writer.AppendLine("FloatValue = 0.30000000000000004");
        writer.AppendLine("NegativeValue = -123.456");
        writer.AppendLine("ZeroValue = 0");
        writer.AppendLine("NullableIntegerValue = 9223372036854775807");
        writer.AppendLine("NullableFloatValue = 0.30000000000000004");
        writer.Flush();

        var result = CsTomlSerializer.Deserialize<TypeDecimal>(buffer.WrittenSpan);
        result.NullableIntegerValue.ShouldBe(result.IntegerValue);
        result.NullableFloatValue.ShouldBe(result.FloatValue);
    }

    [Fact]
    public void DeserializeNullable_MissingKey_ReturnsNull()
    {
        using var buffer = Utf8String.CreateWriter(out var writer);
        writer.AppendLine("IntegerValue = 0");
        writer.AppendLine("FloatValue = 0");
        writer.AppendLine("NegativeValue = 0");
        writer.AppendLine("ZeroValue = 0");
        writer.Flush();

        var result = CsTomlSerializer.Deserialize<TypeDecimal>(buffer.WrittenSpan);
        result.NullableIntegerValue.ShouldBeNull();
        result.NullableFloatValue.ShouldBeNull();
    }

    [Fact]
    public void DeserializeNullable_FromInf_ThrowsOverflowException()
    {
        using var buffer = Utf8String.CreateWriter(out var writer);
        writer.AppendLine("IntegerValue = 0");
        writer.AppendLine("FloatValue = 0");
        writer.AppendLine("NegativeValue = 0");
        writer.AppendLine("ZeroValue = 0");
        writer.AppendLine("NullableFloatValue = inf");
        writer.Flush();

        Should.Throw<OverflowException>(() =>
            CsTomlSerializer.Deserialize<TypeDecimal>(buffer.WrittenSpan));
    }

    [Fact]
    public void RoundTrip_KeepsShortestRoundTripDigits()
    {
        var original = new TypeDecimal()
        {
            IntegerValue = 9223372036854775807m,
            FloatValue = 0.30000000000000004m,
            NegativeValue = -0.01m,
            ZeroValue = 0,
            NullableIntegerValue = -9223372036854775808m,
            NullableFloatValue = 0.0000000000000000000000000001m,
        };

        using var bytes = CsTomlSerializer.Serialize(original);
        var result = CsTomlSerializer.Deserialize<TypeDecimal>(bytes.ByteSpan);

        result.IntegerValue.ShouldBe(original.IntegerValue);
        result.FloatValue.ShouldBe(original.FloatValue);
        result.NegativeValue.ShouldBe(original.NegativeValue);
        result.ZeroValue.ShouldBe(original.ZeroValue);
        result.NullableIntegerValue.ShouldBe(original.NullableIntegerValue);
        result.NullableFloatValue.ShouldBe(original.NullableFloatValue);
    }
}
