using Shouldly;
using Utf8StringInterpolation;

namespace CsToml.Generator.Tests;

public class InitTest
{
    [Fact]
    public void Serialize()
    {
        var primitive = new Init()
        {
            Str = @"I'm a string.",
            IntValue = 123,
            FloatValue = 123.456,
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

        var primitive = CsTomlSerializer.Deserialize<Init>(buffer.WrittenSpan);
        primitive.Str.ShouldBe("I'm a string.");
        primitive.IntValue.ShouldBe(123);
        primitive.FloatValue.ShouldBe(123.456);
        primitive.BooleanValue.ShouldBeTrue();
    }
}

public class Init2Test
{
    [Fact]
    public void Serialize()
    {
        var primitive = new Init2()
        {
            Str = @"I'm a string.",
            IntValue = 123,
            FloatValue = 123.456,
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

        var primitive = CsTomlSerializer.Deserialize<Init2>(buffer.WrittenSpan);
        primitive.Str.ShouldBe("I'm a string.");
        primitive.IntValue.ShouldBe(123);
        primitive.FloatValue.ShouldBe(123.456);
        primitive.BooleanValue.ShouldBeTrue();
    }
}

