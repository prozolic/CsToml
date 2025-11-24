using Shouldly;
using Utf8StringInterpolation;

namespace CsToml.Generator.Tests;

public class TomlPrimitiveTest
{
    [Fact]
    public void Serialize()
    {
        var primitive = new TomlPrimitive
        {
            Str = @"I'm a string.",
            Long = 123,
            Float = 123.456,
            Boolean = true,
            OffsetDateTime = new DateTimeOffset(1979, 5, 27, 7, 32, 0, TimeSpan.Zero),
            LocalDateTime = new DateTime(1979, 5, 27, 7, 32, 0),
            LocalDate = new DateOnly(1979, 5, 27),
            LocalTime = new TimeOnly(7, 32, 30)
        };

        {
            using var bytes = CsTomlSerializer.Serialize(primitive);

            using var buffer = Utf8String.CreateWriter(out var writer);
            writer.AppendLine("Str = \"I'm a string.\"");
            writer.AppendLine("Long = 123");
            writer.AppendLine("Float = 123.456");
            writer.AppendLine("Boolean = true");
            writer.AppendLine("OffsetDateTime = 1979-05-27T07:32:00Z");
            writer.AppendLine("LocalDateTime = 1979-05-27T07:32:00");
            writer.AppendLine("LocalDate = 1979-05-27");
            writer.AppendLine("LocalTime = 07:32:30");
            writer.Flush();

            var expected = buffer.ToArray();
            bytes.ByteSpan.ToArray().ShouldBe(expected);
        }
        {
            using var bytes = CsTomlSerializer.Serialize(primitive, Option.Header);

            using var buffer = Utf8String.CreateWriter(out var writer);
            writer.AppendLine("Str = \"I'm a string.\"");
            writer.AppendLine("Long = 123");
            writer.AppendLine("Float = 123.456");
            writer.AppendLine("Boolean = true");
            writer.AppendLine("OffsetDateTime = 1979-05-27T07:32:00Z");
            writer.AppendLine("LocalDateTime = 1979-05-27T07:32:00");
            writer.AppendLine("LocalDate = 1979-05-27");
            writer.AppendLine("LocalTime = 07:32:30");
            writer.Flush();

            var expected = buffer.ToArray();
            bytes.ByteSpan.ToArray().ShouldBe(expected);
        }
    }

    [Fact]
    public void Deserialize()
    {
        using var buffer = Utf8String.CreateWriter(out var writer);
        writer.AppendLine("Str = \"I'm a string.\"");
        writer.AppendLine("Long = 123");
        writer.AppendLine("Float = 123.456");
        writer.AppendLine("Boolean = true");
        writer.AppendLine("LocalDateTime = 1979-05-27T07:32:00");
        writer.AppendLine("OffsetDateTime = 1979-05-27T07:32:00Z");
        writer.AppendLine("LocalDate = 1979-05-27");
        writer.AppendLine("LocalTime = 07:32:30");
        writer.Flush();

        var primitive = CsTomlSerializer.Deserialize<TomlPrimitive>(buffer.WrittenSpan);
        primitive.Str.ShouldBe("I'm a string.");
        primitive.Long.ShouldBe(123);
        primitive.Float.ShouldBe(123.456);
        primitive.Boolean.ShouldBeTrue();
        primitive.OffsetDateTime.ShouldBe(new DateTimeOffset(1979, 5, 27, 7, 32, 0, TimeSpan.Zero));
        primitive.LocalDateTime.ShouldBe(new DateTime(1979, 5, 27, 7, 32, 0));
        primitive.LocalDate.ShouldBe(new DateOnly(1979, 5, 27));
        primitive.LocalTime.ShouldBe(new TimeOnly(7, 32, 30));
    }
}
