using Shouldly;
using Utf8StringInterpolation;

namespace CsToml.Generator.Tests;

public class TypeTomlDoubleTest
{
    private TypeTomlDouble typeTomlDouble;

    public TypeTomlDoubleTest()
    {
        typeTomlDouble = new TypeTomlDouble()
        {
            Normal = 123.456,
            Normal2 = -123.456,
            Normal3 = 123d,
            Normal4 = 123.0,
            Normal5 = 0.0000005,
            Inf = double.PositiveInfinity,
            NInf = double.NegativeInfinity,
            Nan = double.NaN,
        };
    }

    [Fact]
    public void Serialize()
    {
        using var bytes = CsTomlSerializer.Serialize(typeTomlDouble);

        using var buffer = Utf8String.CreateWriter(out var writer);
        writer.AppendLine("Normal = 123.456");
        writer.AppendLine("Normal2 = -123.456");
        writer.AppendLine("Normal3 = 123.0");
        writer.AppendLine("Normal4 = 123.0");
        writer.AppendLine("Normal5 = 5E-07");
        writer.AppendLine("Inf = inf");
        writer.AppendLine("NInf = -inf");
        writer.AppendLine("Nan = nan");
        writer.Flush();

        var expected = buffer.ToArray();
        bytes.ByteSpan.ToArray().ShouldBe(expected);
    }

    [Fact]
    public void SerializeWithHeaderOption()
    {
        using var bytes = CsTomlSerializer.Serialize(typeTomlDouble, Option.Header);

        using var buffer = Utf8String.CreateWriter(out var writer);
        writer.AppendLine("Normal = 123.456");
        writer.AppendLine("Normal2 = -123.456");
        writer.AppendLine("Normal3 = 123.0");
        writer.AppendLine("Normal4 = 123.0");
        writer.AppendLine("Normal5 = 5E-07");
        writer.AppendLine("Inf = inf");
        writer.AppendLine("NInf = -inf");
        writer.AppendLine("Nan = nan");
        writer.Flush();

        var expected = buffer.ToArray();
        bytes.ByteSpan.ToArray().ShouldBe(expected);
    }

    [Fact]
    public void SerializeWithArrayHeaderOption()
    {
        using var bytes = CsTomlSerializer.Serialize(typeTomlDouble, Option.ArrayHeader);

        using var buffer = Utf8String.CreateWriter(out var writer);
        writer.AppendLine("Normal = 123.456");
        writer.AppendLine("Normal2 = -123.456");
        writer.AppendLine("Normal3 = 123.0");
        writer.AppendLine("Normal4 = 123.0");
        writer.AppendLine("Normal5 = 5E-07");
        writer.AppendLine("Inf = inf");
        writer.AppendLine("NInf = -inf");
        writer.AppendLine("Nan = nan");
        writer.Flush();

        var expected = buffer.ToArray();
        bytes.ByteSpan.ToArray().ShouldBe(expected);
    }

    [Fact]
    public void SerializeWithHeaderAndArrayHeaderOption()
    {
        using var bytes = CsTomlSerializer.Serialize(typeTomlDouble, Option.HeaderAndArrayHeader);

        using var buffer = Utf8String.CreateWriter(out var writer);
        writer.AppendLine("Normal = 123.456");
        writer.AppendLine("Normal2 = -123.456");
        writer.AppendLine("Normal3 = 123.0");
        writer.AppendLine("Normal4 = 123.0");
        writer.AppendLine("Normal5 = 5E-07");
        writer.AppendLine("Inf = inf");
        writer.AppendLine("NInf = -inf");
        writer.AppendLine("Nan = nan");
        writer.Flush();

        var expected = buffer.ToArray();
        bytes.ByteSpan.ToArray().ShouldBe(expected);
    }

    [Fact]
    public void Deserialize()
    {
        var expected = typeTomlDouble;

        using var buffer = Utf8String.CreateWriter(out var writer);
        writer.AppendLine("Normal = 123.456");
        writer.AppendLine("Normal2 = -123.456");
        writer.AppendLine("Normal3 = 123.0");
        writer.AppendLine("Normal4 = 123.0");
        writer.AppendLine("Normal5 = 5E-07");
        writer.AppendLine("Inf = inf");
        writer.AppendLine("NInf = -inf");
        writer.AppendLine("Nan = nan");
        writer.Flush();

        var value = CsTomlSerializer.Deserialize<TypeTomlDouble>(buffer.WrittenSpan);
        value.Normal.ShouldBe(123.456);
        value.Normal2.ShouldBe(-123.456);
        value.Normal3.ShouldBe(123d);
        value.Normal4.ShouldBe(123.0);
        value.Normal5.ShouldBe(0.0000005);
        value.Inf.ShouldBe(double.PositiveInfinity);
        value.NInf.ShouldBe(double.NegativeInfinity);
        value.Nan.ShouldBe(double.NaN);
    }
}
