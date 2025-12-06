using Shouldly;
using Utf8StringInterpolation;

namespace CsToml.Generator.Tests;

public class TypeIntegerTest
{
    [Fact]
    public void Serialize()
    {
        var integer = new TypeInteger()
        {
            Byte = 255,
            SByte = -128,
            Short = -12345,
            UShort = 12345,
            Int = -123456,
            Uint = 123456,
            Long = -1234567,
            ULong = 1234567,
            Decimal = 999999,
            BigInteger = 99999999,
            Int128 = -99999999,
            UInt128 = 99999999
        };

        using var bytes = CsTomlSerializer.Serialize(integer);

        using var buffer = Utf8String.CreateWriter(out var writer);
        writer.AppendLine("Byte = 255");
        writer.AppendLine("SByte = -128");
        writer.AppendLine("Short = -12345");
        writer.AppendLine("UShort = 12345");
        writer.AppendLine("Int = -123456");
        writer.AppendLine("Uint = 123456");
        writer.AppendLine("Long = -1234567");
        writer.AppendLine("ULong = 1234567");
        writer.AppendLine("Decimal = 999999");
        writer.AppendLine("BigInteger = 99999999");
        writer.AppendLine("Int128 = -99999999");
        writer.AppendLine("UInt128 = 99999999");
        writer.Flush();

        var expected = buffer.ToArray();
        bytes.ByteSpan.ToArray().ShouldBe(expected);
    }

    [Fact]
    public void Deserialize()
    {
        using var buffer = Utf8String.CreateWriter(out var writer);
        writer.AppendLine("Byte = 255");
        writer.AppendLine("SByte = -128");
        writer.AppendLine("Short = -12345");
        writer.AppendLine("UShort = 12345");
        writer.AppendLine("Int = -123456");
        writer.AppendLine("Uint = 123456");
        writer.AppendLine("Long = -1234567");
        writer.AppendLine("ULong = 1234567");
        writer.AppendLine("Decimal = 999999");
        writer.AppendLine("BigInteger = 99999999");
        writer.AppendLine("Int128 = -99999999");
        writer.AppendLine("UInt128 = 99999999");
        writer.Flush();

        var typeInteger = CsTomlSerializer.Deserialize<TypeInteger>(buffer.WrittenSpan);
        typeInteger.Byte.ShouldBe((byte)255);
        typeInteger.SByte.ShouldBe((sbyte)-128);
        typeInteger.Short.ShouldBe((short)-12345);
        typeInteger.UShort.ShouldBe((ushort)12345);
        typeInteger.Int.ShouldBe(-123456);
        typeInteger.Uint.ShouldBe((uint)123456);
        typeInteger.Long.ShouldBe(-1234567);
        typeInteger.ULong.ShouldBe((ulong)1234567);
        typeInteger.Decimal.ShouldBe(999999);
        typeInteger.BigInteger.ShouldBe(99999999);
        typeInteger.Int128.ShouldBe(-99999999);
        typeInteger.UInt128.ShouldBe((UInt128)99999999);
    }
}
