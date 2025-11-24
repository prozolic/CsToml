using Shouldly;
using Utf8StringInterpolation;

namespace CsToml.Generator.Tests;

public class TypeTomlDoubleTest
{
    [Fact]
    public void Serialize()
    {
        var typeTomlDouble = new TypeTomlDouble()
        {
            Normal = 123.456,
            Inf = double.PositiveInfinity,
            NInf = double.NegativeInfinity,
            Nan = double.NaN,
        };

        {
            using var bytes = CsTomlSerializer.Serialize(typeTomlDouble);

            using var buffer = Utf8String.CreateWriter(out var writer);
            writer.AppendLine("Normal = 123.456");
            writer.AppendLine("Inf = inf");
            writer.AppendLine("NInf = -inf");
            writer.AppendLine("Nan = nan");
            writer.Flush();

            var expected = buffer.ToArray();
            bytes.ByteSpan.ToArray().ShouldBe(expected);
        }
        {
            using var bytes = CsTomlSerializer.Serialize(typeTomlDouble, Option.Header);

            using var buffer = Utf8String.CreateWriter(out var writer);
            writer.AppendLine("Normal = 123.456");
            writer.AppendLine("Inf = inf");
            writer.AppendLine("NInf = -inf");
            writer.AppendLine("Nan = nan");
            writer.Flush();

            var expected = buffer.ToArray();
            bytes.ByteSpan.ToArray().ShouldBe(expected);
        }
    }

    [Fact]
    public void Deserialize()
    {
        using var buffer = Utf8String.CreateWriter(out var writer);
        writer.AppendLine("Normal = 123.456");
        writer.AppendLine("Inf = inf");
        writer.AppendLine("NInf = -inf");
        writer.AppendLine("Nan = nan");
        writer.Flush();

        var typeTomlDouble = CsTomlSerializer.Deserialize<TypeTomlDouble>(buffer.WrittenSpan);
        typeTomlDouble.Normal.ShouldBe(123.456);
        typeTomlDouble.Inf.ShouldBe(double.PositiveInfinity);
        typeTomlDouble.NInf.ShouldBe(double.NegativeInfinity);
        typeTomlDouble.Nan.ShouldBe(double.NaN);
    }
}
