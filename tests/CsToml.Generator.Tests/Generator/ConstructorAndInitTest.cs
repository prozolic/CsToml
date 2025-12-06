using Shouldly;
using Utf8StringInterpolation;

namespace CsToml.Generator.Tests.Seirialization;

public class ConstructorAndInitTest
{
    [Fact]
    public void Serialize()
    {
        var primitive = new ConstructorAndInit(123, 123.456)
        {
            Str = @"I'm a string.",
            BooleanValue = true,
        };
        using var bytes = CsTomlSerializer.Serialize(primitive);

        using var buffer = Utf8String.CreateWriter(out var writer);
        writer.AppendLine("Str = \"I'm a string.\"");
        writer.AppendLine("IntValue = 123");
        writer.AppendLine("FloatValue = 123.456");
        writer.AppendLine("BooleanValue = true");
        writer.Flush();

        var expected = buffer.ToArray();
        bytes.ByteSpan.ToArray().ShouldBe(expected);
    }

    [Fact]
    public void Deserialize()
    {
        using var buffer = Utf8String.CreateWriter(out var writer);
        writer.AppendLine("Str = \"I'm a string.\"");
        writer.AppendLine("IntValue = 123");
        writer.AppendLine("FloatValue = 123.456");
        writer.AppendLine("BooleanValue = true");
        writer.Flush();

        var primitive = CsTomlSerializer.Deserialize<ConstructorAndInit>(buffer.WrittenSpan);
        primitive.Str.ShouldBe("I'm a string.");
        primitive.IntValue.ShouldBe(123);
        primitive.FloatValue.ShouldBe(123.456);
        primitive.BooleanValue.ShouldBeTrue();
    }
}
